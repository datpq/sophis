using System;
using System.Collections.Generic;
using System.Reflection;
using MEDIO.CORE.Tools;
using Oracle.DataAccess.Client;
using sophis.FastPnl;
using sophis.instrument;
using sophis.modelPortfolio;
using sophis.oms;
using Sophis.OMS.Util;
using sophis.OrderGeneration.DOB.Builders;
using sophis.OrderGeneration.PortfolioColumn;
using sophis.portfolio;
using sophis.utils;
using sophis.value;
using Sophis.DataAccess.NH;
using Sophis.OrderBookCompliance;

namespace MEDIO.OrderAutomation.NET.Source.OrderCreationValidator
{
    public class CSxOrderCreationShortSellValidator : IOrderCreationValidator
    {
        public ValidationResult Validate(IOrder order, bool creating)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                SingleOrder sOrder = (SingleOrder)order;
                if (sOrder != null)
                {
                    if (sOrder.AllocationRulesSet == null || sOrder.AllocationRulesSet.Allocations.IsNullOrEmpty())
                    {
                        string msg = String.Format("Order {0} does not have any allocation! Please amend!", sOrder.ID);
                        return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() {msg}};
                    }

                    if (sOrder.Side != ESide.Sell)
                    {
                        return new ValidationResult() { IsValid = true };
                    }
                    CSMInstrument instrument = CSMInstrument.GetInstance(sOrder.Target.SecurityID);
                    if (instrument != null)
                    {
                        if (instrument.GetInstrumentType() != 'A' && instrument.GetInstrumentType() != 'Z' &&
                            instrument.GetInstrumentType() != 'O')
                        {
                            return new ValidationResult() { IsValid = true };
                        }
                    }
                    bool failed = false;
                    List<string> msgList = new List<string>();
                    foreach (var alloc in sOrder.AllocationRulesSet.Allocations)
                    {
                        if (alloc == null) continue;
                        string fundName = net.Source.Tools.CSxUtils.GetFundName(alloc.PortfolioID);
                        CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(alloc.PortfolioID);
                        if (portfolio == null)
                        {
                            LOG.Write(CSMLog.eMVerbosity.M_debug, "Cannot retrieve portfolio " + alloc.PortfolioID);
                            continue;
                        }
                        double provQty = 0;
                        for (int i = 0; i < portfolio.GetFlatViewPositionCount(); i++)
                        {
                            var pos = portfolio.GetNthFlatViewPosition(i);
                            if (pos!= null && pos.GetInstrumentCode() == sOrder.Target.SecurityID)
                            {
                                double qty = pos.GetQuantityToBeProvisioned();
                                provQty += qty;
                                LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Pos = #{0} qty = {1}; all qty = {2}", pos.GetIdentifier(), qty, provQty));
                            }
                        }

                        // Substract simulated orders 
                        if (DynamicOrderBuilder.Instance.IsSessionActive)
                        {
                            foreach (var simulatedOrder in DynamicOrderBuilder.Instance.GetOrdersToSend())
                            {
                                if (simulatedOrder.FolioCode == alloc.PortfolioID &&
                                    simulatedOrder.InstrumentCode == sOrder.Target.SecurityID)
                                {
                                    provQty -= simulatedOrder.Quantity.value; // -1000
                                }
                            }
                        }
                        var allocQty = alloc.Quantity;
                        #region funds 
                        CSMAmExternalFundBase extFund = instrument;
                        if (extFund != null)
                        {
                            int digits = GetDigitsCount(extFund.GetTradingUnits());
                            if (order.QuantityData.OrderedType == EQuantityType.Unit)
                            {
                                allocQty *= extFund.GetTradingUnits();
                            }
                            else if (order.QuantityData.OrderedType == EQuantityType.Amount)
                            {
                                var last = CSMInstrument.GetLast(extFund.GetCode());
                                allocQty = Math.Abs(last) < 0.000000001 ? allocQty : allocQty / last;
                            }
                            allocQty = Math.Round(allocQty, digits);
                            provQty = Math.Round(provQty, digits); 
                        }
                        #endregion
                        #region bonds 
                        CSMBond bond = instrument;
                        if (bond != null)
                        {
                            var minSize = bond.GetMinTradingSize();
                            var contractSize = bond.GetNotionalInProduct();
                            if (order.QuantityData.OrderedType == EQuantityType.Nominal)
                            {
                                allocQty = Math.Abs(contractSize) < 0.000000001 ? allocQty : allocQty / contractSize;
                            }
                            double remainingQty = provQty - Math.Abs(allocQty);
                            minSize = Math.Abs(contractSize) < 0.000000001 ? minSize : minSize / contractSize;
                            remainingQty = Math.Round(remainingQty, 4);
                            if (remainingQty > 0.000000001 && remainingQty < minSize)
                            {
                                failed = true;
                                string msg = String.Format("Minimum Trading : Remaing nominal in fund [{0}] = {1}, which is less than the minimum trading size {2}!", fundName, remainingQty * contractSize, minSize * contractSize);
                                msgList.Add(msg);
                                LOG.Write(CSMLog.eMVerbosity.M_debug, msg);
                            }
                            else if (remainingQty < 0)
                            {
                                failed = true;
                                string msg = String.Format("Short Sell : Total nominal in fund [{0}] = {1}, which is less than the allocated order nominal {2}!", fundName, provQty * contractSize, Math.Abs(allocQty) * contractSize);
                                msgList.Add(msg);
                                LOG.Write(CSMLog.eMVerbosity.M_debug, msg);
                            }
                            else
                                LOG.Write(CSMLog.eMVerbosity.M_debug,"Passed check! All positions qty = " + provQty + ", order qty = " + Math.Abs(allocQty));
                        }
                        #endregion
                        else if (provQty < Math.Abs(allocQty))
                        {
                            failed = true;
                            string msg = String.Format("Short Sell : Total number of securities in fund [{0}] = {1}, which is less than the allocated order quantity {2}!", fundName, provQty, Math.Abs(allocQty));
                            msgList.Add(msg);
                            LOG.Write(CSMLog.eMVerbosity.M_debug, msg);
                        }
                        else
                            LOG.Write(CSMLog.eMVerbosity.M_debug, "Passed check! All positions qty = " + provQty + ", order qty = " + Math.Abs(allocQty));
                    }
                    if (failed)
                    {
                        msgList.Insert(0, "Order Validation Breaches: ");
                        return new ValidationResult() { IsValid = false, ValidationMessages = msgList };
                    }
                }
            }
            return new ValidationResult() { IsValid = true };
        }


        private static int GetDigitsCount(double value)
        {
            return (int)Math.Round(Math.Abs(Math.Log(value) / Math.Log(10)), 0);
        }

    }
}

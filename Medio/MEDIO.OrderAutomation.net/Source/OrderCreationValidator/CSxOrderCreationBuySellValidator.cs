using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DevExpress.Internal;
using sophis.instrument;
using sophis.oms;
using Sophis.OMS.Util;
using sophis.portfolio;
using sophis.utils;
using Sophis.DataAccess.NH;
using Sophis.OrderBookCompliance;

namespace MEDIO.OrderAutomation.NET.Source.OrderCreationValidator
{
    public class CSxOrderCreationBuySellValidator : IOrderCreationValidator
    {
        public ValidationResult Validate(IOrder order, bool creating)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                SingleOrder sOrder = (SingleOrder) order;
                if (sOrder != null)
                {
                    if (sOrder.AllocationRulesSet == null || sOrder.AllocationRulesSet.Allocations.IsNullOrEmpty())
                    {
                        string msg = String.Format("Order {0} does not have any allocation! Please amend!", sOrder.ID);
                        return new ValidationResult() {IsValid = false, ValidationMessages = new List<string>() {msg}};
                    }

                    CSMInstrument instrument = CSMInstrument.GetInstance(sOrder.Target.SecurityID);
                    if (instrument != null)
                    {
                        if (instrument.GetInstrumentType() != 'A' && instrument.GetInstrumentType() != 'Z' &&
                            instrument.GetInstrumentType() != 'O')
                        {
                            return new ValidationResult() {IsValid = true};
                        }
                    }

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
                        for (int i = 0; i < portfolio.GetFlatViewPositionCount(); i++)
                        {
                            var pos = portfolio.GetNthFlatViewPosition(i);
                            if (pos != null && pos.GetInstrumentCode() == sOrder.Target.SecurityID)
                            {
                                if (pos.GetPositionType() != eMPositionType.M_pStandard)
                                {
                                    foreach (IOrder pendingOrder in GetOrdersFromPositions(pos))
                                    {
                                        if (sOrder.Side == ESide.Sell && pendingOrder.Side == ESide.Buy)
                                        {
                                            return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { String.Format("You have a pending buy order #{0}, please execute it before placing a sell order", pendingOrder.ID) } };
                                        }
                                        if (sOrder.Side == ESide.Buy && pendingOrder.Side == ESide.Sell)
                                        {
                                            return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { String.Format("You have a pending sell order #{0}, please execute it before placing a buy order", pendingOrder.ID) } };
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new ValidationResult() { IsValid = true };
        }


        public static OrderList GetOrdersFromPositions(CSMPosition pos)
        {
            OrderCriteria criteria = new OrderCriteria();
            criteria.OnlyAlive = true;
            criteria.OrderKind = new Sophis.DataAccess.NH.DisjunctionCriterion<int>(new List<int> { OrderKind.SingleOrder });
            criteria.Status = new DisjunctionCriterion<string>(OrderReportingStatuses.GetInstance().GetAuthorizedStatuses());
            criteria.AllocatedPortfolio = new DisjunctionCriterion<int>(new List<int>() { pos.GetPortfolioCode() });
            criteria.SecurityID = new DisjunctionCriterion<int>(new List<int>() { pos.GetInstrumentCode() });
            OrderCriteriaDisjunction criterias = new OrderCriteriaDisjunction(criteria);
            criterias.LoadWorkflowStateInfo = true;
            //getting list of orders
            var res = OrderManagerConnector.Instance.GetOrderManager().GetOrdersByCriteria(criterias);
            return res;
        }
    }
}

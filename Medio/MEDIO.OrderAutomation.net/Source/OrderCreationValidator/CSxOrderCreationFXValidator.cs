using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using sophis.instrument;
using sophis.oms;
using sophis.OrderGeneration.PortfolioColumn;
using sophis.static_data;
using sophis.utils;
using Sophis.OrderBookCompliance;
using SophisAMDotNetTools;
using sophisTools;
namespace MEDIO.OrderAutomation.NET.Source.OrderCreationValidator
{
    public class CSxOrderCreationFXValidator : IOrderCreationValidator
    {
        private List<int> _NDFCurrencyList;
        public List<int> NDFCurrencyList
        {
            get
            {
                if (_NDFCurrencyList == null)
                {
                    InitNDFCurrencies(ref _NDFCurrencyList);
                }
                return _NDFCurrencyList;
            }
        }

        public ValidationResult Validate(IOrder order, bool creating)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                SingleOrder sOrder = (SingleOrder)order;
                if (sOrder != null)
                {
                    if (sOrder.Target is ForexTarget)
                    {
                        LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Order #{0} is an FX order", order.ID));

                        

                        if (!ValidateFXNDF(sOrder))
                        {
                            //If forex et un des 2 NDF: error
                            return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { "Can not create a spot forex order with a non deliverable currency" } };
                        }

                        if (sOrder.Target.SecurityType==ESecurityType.ForexNDF)
                        {
                            CSMForexFuture fxFrwd = CSMInstrument.GetInstance(sOrder.Target.SecurityID);
                            // GET FX CCY1/CCY2
                            // FX SPOT -> GetSpotDate() <= sOrderGetSettlementDate...
                            
                            if (fxFrwd != null)
                            {

                                int ccy1 = fxFrwd.GetExpiryCurrency();
                                int ccy2 = fxFrwd.GetSettlementCurrency();

                                SSMFxPair pair = new SSMFxPair(ccy1, ccy2);
                                CSMForexSpot fxSpot = new CSMForexSpot(pair);
                                

                                DateTime now = DateTime.Now;
                                CSMDay today = new CSMDay(now.Day,now.Month,now.Year);

                                int fxSettleDate = fxSpot.GetSettlementDate(today.toLong());
                                
                                DateTime orderSettle = sOrder.SettlementDate;
                                CSMDay orderSettleDate = new CSMDay(orderSettle.Day, orderSettle.Month, orderSettle.Year);

                                if (fxSettleDate >= orderSettleDate.toLong())
                                {

                                    return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { "Can not create a spot forex order with a non deliverable currency" } };
                                }
                            }
                        }

                        //SetCurrencyMarketWay(sOrder);
                        SetToNDF(sOrder);
                    }
                }
            }
            return new ValidationResult() { IsValid = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sOrder"></param>
        private bool ValidateFXNDF(SingleOrder sOrder)
        {
            CSMForexSpot fx = CSMInstrument.GetInstance(sOrder.Target.SecurityID);


            if (fx != null)
            {
                int ccy1 = fx.GetForex1();
                int ccy2 = fx.GetForex2();
                if (NDFCurrencyList.Contains(ccy1) || NDFCurrencyList.Contains(ccy2))
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// The Order has to be an FX order 
        /// </summary>
        /// <param name="sOrder"></param>
        private void SetCurrencyMarketWay(SingleOrder sOrder)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                CSMForexSpot spot = CSMInstrument.GetInstance(sOrder.Target.SecurityID);
                if (spot != null)
                {
                    var isMarketWay = spot.GetMarketWay() != -1;
                    var marketCcy1 = isMarketWay ? spot.GetForex1() : spot.GetForex2();
                    if (!isMarketWay)
                    {
                        sOrder.Target.Currency = marketCcy1;
                        if(sOrder.Side == ESide.Buy )
                        {
                            sOrder.Side = ESide.Sell;
                        }
                        else
                        {
                            sOrder.Side = ESide.Buy;
                        }
    
                        var invSpot = CSMForexSpot.GetCSRForexSpot(spot.GetForex2(), spot.GetForex1());
                        if (invSpot != null)
                        {
                            sOrder.Target.SecurityID = invSpot.GetCode();
                            LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Order spot {0} created with mod FX Spot {1} ", sOrder.ID, invSpot.GetCode()));
                        }
                        else
                        {
                            LOG.Write(CSMLog.eMVerbosity.M_error, String.Format("Cannot get inversed FX Spot for order {0} ", sOrder.ID));
                        }
                    }
                }

                // FX fwd & NDF
                CSMForexFuture fxfwd = CSMInstrument.GetInstance(sOrder.Target.SecurityID);
                if (fxfwd != null)
                {
                    int ccy1 = fxfwd.GetExpiryCurrency();
                    int ccy2 = fxfwd.GetSettlementCurrency();
                    spot = CSMForexSpot.GetCSRForexSpot(ccy1, ccy2);
                    if (spot != null)
                    {
                        var isMarketWay = spot.GetMarketWay() != -1;
                        var marketCcy1 = isMarketWay ? spot.GetForex1() : spot.GetForex2();
                        if (sOrder.Target.Currency != marketCcy1)
                        {
                            sOrder.Target.Currency = marketCcy1;

                            //TODO test and check
                            //var newSicovam = CSAMInstrumentDotNetTools.GetSettlementReportingInstrumentId(ccy2,
                            //            ccy1, sOrder.SettlementDate,
                            //            EForexNDFCreationBehaviour.CreateAsForward, true);
                            var newSicovam = CSAMInstrumentDotNetTools.GetForexInstrument(ccy2,ccy1,sophis.amCommon.DateUtils.ToInt(sOrder.SettlementDate));

                            var forward = CSMForexFuture.GetInstance(newSicovam);
                            if (forward != null)
                            {
                                sOrder.Target.SecurityID = newSicovam;
                                LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Order Forward {0} created with mod FX Forward {1} ", sOrder.ID, newSicovam));
                            }
                            else
                            {
                                LOG.Write(CSMLog.eMVerbosity.M_error, String.Format("Cannot create inversed FX Forward for order {0} ", sOrder.ID));
                            }
                        }
                    }
                }
                LOG.End();
            }
        }

       /// <summary>
       ///The Order has to be an FX order  
       /// </summary>
       /// <param name="sOrder"></param>
        private void SetToNDF(SingleOrder sOrder)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                var fxTarget = (ForexTarget)sOrder.Target;
                if (fxTarget != null)
                {
                    if (!fxTarget.NonDeliverableForward)
                    {
                        int ccy1 = 0, ccy2 = 0;
                        CSMForexFuture fxfwd = CSMInstrument.GetInstance(sOrder.Target.SecurityID);
                        if (fxfwd != null)
                        {
                            ccy1 = fxfwd.GetExpiryCurrency();
                            ccy2 = fxfwd.GetSettlementCurrency();
                        }
                        if (NDFCurrencyList.Contains(ccy1) || NDFCurrencyList.Contains(ccy2))
                        {
                            fxTarget.NonDeliverableForward = true;
                            var newSicovam = CSAMInstrumentDotNetTools.GetForexInstrument(ccy1,ccy2, sophis.amCommon.DateUtils.ToInt(sOrder.SettlementDate));

                            var ndf = CSMNonDeliverableForexForward.GetInstance(newSicovam);
                            if (ndf != null)
                            {
                                sOrder.Target.SecurityID = newSicovam;
                                sOrder.Target.SecurityType = ESecurityType.ForexNDF;
                                sOrder.Target.Allotment = ndf.GetAllotment();
                                LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("{0} or {1} is in the NDF currency list", ccy1, ccy2));
                            }
                        }
                        else
                        {
                            LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Both {0} and {1} are not in the NDF currency list", ccy1, ccy2));
                        }
                    }
                }
                LOG.End();
            }
        }

        private bool IsValidCurrency(SingleOrder sOrder, List<string> msgList)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                int ccy1 = 0, ccy2 = 0;
                CSMForexSpot spot = CSMInstrument.GetInstance(sOrder.Target.SecurityID);
                if (spot != null)
                {
                    ccy1 = spot.GetForex1();
                    ccy2 = spot.GetForex2();
                }
                CSMForexFuture fxfwd = CSMInstrument.GetInstance(sOrder.Target.SecurityID);
                if (fxfwd != null)
                {
                    ccy1 = fxfwd.GetExpiryCurrency();
                    ccy2 = fxfwd.GetSettlementCurrency();
                }
                CMString ccyName = "";
                var currency = CSMCurrency.GetCSRCurrency(ccy1);
                if (currency != null)
                {
                    ccyName = currency.GetName();
                    string ccyNameUpper = ccyName.StringValue.ToUpper();
                    if (!ccyName.StringValue.Equals(ccyNameUpper))
                    {
                        msgList.Add(String.Format("{0} is not allowed! Please use {1}", ccyName, ccyNameUpper));
                        return false;
                    }
                }
                currency = CSMCurrency.GetCSRCurrency(ccy2);
                if (currency != null)
                {
                    ccyName = currency.GetName();
                    string ccyNameUpper = ccyName.StringValue.ToUpper();
                    if (!ccyName.StringValue.Equals(ccyNameUpper))
                    {
                        msgList.Add(String.Format("{0} is not allowed! Please use {1}", ccyName, ccyNameUpper));
                        return false;
                    }
                }
                LOG.End();
                return true;
            }
        }

        public static void InitNDFCurrencies(ref List<int> NDFCurrencyList)
        {
            string sql = "select STR_TO_DEVISE(currency) from MEDIO_NDF_CURRENCY";
            NDFCurrencyList = CSxDBHelper.GetMultiRecords(sql).ConvertAll(x => Convert.ToInt32(x.ToString()));
        }

    }
}

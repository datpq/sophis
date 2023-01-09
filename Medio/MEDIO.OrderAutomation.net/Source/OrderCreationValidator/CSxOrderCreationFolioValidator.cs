using System;
using System.Collections.Generic;
using System.Reflection;
using MEDIO.CORE.Tools;
using sophis.oms;
using Sophis.OMS.Util;
using sophis.portfolio;
using sophis.utils;
using Sophis.OMS;
using MEDIO.MEDIO_CUSTOM_PARAM;

namespace MEDIO.OrderAutomation.NET.Source.OrderCreationValidator
{
    public class CSxOrderCreationFolioValidator : IOrderCreationValidator
    {
        private static string _portfolioName = CSxToolkitCustomParameter.Instance.OMS_WF_PORTFOLIONAME;
        private static readonly string _GenericAccount = "GENERIC ACCOUNT";
        private readonly string _FXALL = "FXALL";

        private static string _defaultTradingAcc;
        public static string _DefaultTradingAccount 
        {
            get
            {
                if (_defaultTradingAcc.IsNullOrEmpty())
                    _defaultTradingAcc = GetDefaultTradingAccount();
                return _defaultTradingAcc;
            } 
        }

        public ValidationResult Validate(IOrder order, bool creating)
        {
            using (var LOG = new CSMLog())
            {
                bool isValid = true;
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                SingleOrder singleOrder = (SingleOrder)order;
                if (singleOrder != null)
                {
                    isValid = ContainsFolioName(singleOrder, _portfolioName);
                }
                else
                {
                    IOrder groupOrder = null;
                    IGroupable groupable = order as IGroupable;
                    if (groupable != null)
                    {
                        if (groupable.GroupID.HasValue && groupable.GroupID.Value > 0)
                        {
                            try
                            {
                                groupOrder = OrderManager.Instance.GetOrderById(groupable.GroupID.Value);
                            }
                            catch (NotFoundOrderException ex)
                            {
                                LOG.Write(CSMLog.eMVerbosity.M_error, ex.Message);
                            }
                        }
                    }
                    if (groupOrder != null)
                    {
                        OrderGroup gOrder = groupOrder as OrderGroup;
                        if (gOrder != null)
                        {
                            foreach (var sOrder in gOrder.Orders)
                            {
                                isValid = ContainsFolioName(sOrder, _portfolioName);
                            }
                        }
                    }
                }

                if (!isValid)
                    return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { "Only " + _portfolioName + " allocations are allowed! Please amend the order!"} };

                return new ValidationResult() { IsValid = true };
            }
        }

        private bool ContainsFolioName(SingleOrder singleOrder, string folioName)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                if (!singleOrder.EffectiveTime.HasValue)
                {
                    singleOrder.EffectiveTime = DateTime.Now;
                }

                if (!singleOrder.ThirdParties.IsNullOrEmpty())
                {
                    singleOrder.ThirdParties.Clear();
                }

                bool containsName = true;
                bool isFXAll = false;
                if (!singleOrder.ExternalSystem.IsNullOrEmpty())
                {
                    isFXAll = singleOrder.ExternalSystem.Equals(_FXALL);
                }
                
                if (singleOrder.AllocationRulesSet == null || singleOrder.AllocationRulesSet.Allocations.IsNullOrEmpty())
                {
                    LOG.Write(CSMLog.eMVerbosity.M_error, String.Format("Order {0} does not have any allocation!", singleOrder.ID));
                    return false;
                }

                foreach (var allocation in singleOrder.AllocationRulesSet.Allocations)
                {
                    if (allocation == null) continue;
                    if (isFXAll && allocation.TradingAccount.IsNullOrEmpty()) SetDefaultTradingAccount(allocation);
                    
                    var folioId = allocation.PortfolioID;
                    CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioId);
                    if (folio != null)
                    {
                        CMString fullName = new CMString();
                        folio.GetName(fullName);
                        containsName = fullName.StringValue.Contains(folioName);
                        LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Order {0} allocation contains folio name? {1}. Folio = {2}", singleOrder.ID, containsName, fullName));
                        if (!containsName) return containsName;
                    }
                }
                return containsName;
            }
        }

        private void SetDefaultTradingAccount(AllocationRule allocation)
        {
            allocation.TradingAccount = _DefaultTradingAccount;
        }

        private static string GetDefaultTradingAccount()
        {
            string sql = "select TRADING_ACCOUNT from ORDER_DEFPARAM_SELECTOR where EXTERNAL_SYSTEM='FXALL' and ENTITY is NULL";
            string res = CSxDBHelper.GetOneRecord<String>(sql);
            res = res.IsNullOrEmpty() ? _GenericAccount : res;
            return res;
        }
    }

}

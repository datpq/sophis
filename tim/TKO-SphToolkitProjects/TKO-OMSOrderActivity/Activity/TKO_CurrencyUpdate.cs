using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Activities;
using Sophis.Logging;
using sophis.oms;
using Sophis.WF.Core;
using Sophis.WF.Runtime;
using Sophis.DataAccess.NH;
using sophis.portfolio;
using sophis.value;
using sophis.utils;
using TkoPortfolioColumn.DbRequester;
using sophis.instrument;
using Sophis.OMS.Activities;

namespace SampleOrderActivity
{
    public class TKO_CurrencyUpdate : CodeActivity<ActivityInstanceState>
	{
        #region Properties

        [Description("Identifies the order")]
        [Category("Order")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public InArgument<int> OrderID { get; set; }

        #endregion Properties

        protected override ActivityInstanceState Execute(CodeActivityContext context)
        {
            Exception outerException = null;
            ActivityInstanceState result = ActivityInstanceState.Canceled;
            // All the process in the lamba will be done in the API thread
            sophis.SophisWcfConfig.SynchronizationContext.Send(_ =>
            {
                try
                {
                    result = ExecuteSynchro(context);
                }
                catch (Exception ex)
                {
                    // Capture the exception
                    outerException = ex;
                }
            }, null);
            return result;
        }

        private ActivityInstanceState ExecuteSynchro(CodeActivityContext context)
        {
            //@DPH
            using (ISophisLogger logger = LogManager.GetLogger(LogManager.RegisterLogger("Execute")))
            //using (SophisLogger logger = LogManager.GetDefaultLogger(this, "Execute"))
            {
                try
                {
                    //DPH Do not use using here, that will rollback everything at disposition of the session
                    //using (var session = DataAccess.Instance.OpenSession(true))
                    //logger.Debug("Start DataAccess.Instance.OpenSession");
                    //var session = DataAccess.Instance.OpenSession(true);
                    logger.Debug("******************************************************************************************************");
                    logger.Debug(">>> TKO_CurrencyUpdate : Begin");

                    IOrder order = context.GetOrder();
                    //int orderId = context.GetValue(this.OrderID);
                    //try
                    //{
                    //    order = OrderManager.Instance.GetOrderById(orderId);
                    //}
                    //catch (System.Exception ex1)
                    //{
                    //    logger.Error(String.Format("Unable to retrieve Order {0} from Order Manager", orderId), ex1);
                    //    return ActivityInstanceState.Closed;
                    //}

                    //logger.Debug("Start GetLastUserInfo");
                    //UserInfo user = GetLastUserInfo(context.WorkflowInstanceId);
                    logger.Debug("Use default userinfo");
                    UserInfo user = new UserInfo();
                    //logger.Debug("Start OrderManager.Instance.UpdateOrder");
                    //OrderManager.Instance.UpdateOrder(order, user);

                    System.Collections.Generic.List<IOrderTarget> orderTarget = (from x in order.GetOrderTargetOwners() select x.Target).ToList();

                    //Get Instrument.
                    int securityID = 0;
                    int securityType;
                    securityID = orderTarget.First().SecurityID;
                    logger.Debug("- Instrument Id =>" + securityID);
                    securityType = orderTarget.First().SecurityType;

                    var instrument = CSMInstrument.GetInstance(securityID);
                    if (instrument == null)
                    {
                        logger.Debug("Failed to instanciate instrument instrumentid [ " + securityID + " ]");
                    }
                    eMInstrumentType apitype = ((eMInstrumentType)instrument.GetType_API());
                    logger.Debug("- API TYPE => " + apitype);
                    if (securityType == ESecurityType.Option || securityType == ESecurityType.Equity )
                    {
                        logger.Debug("Order has  " + order.Properties.Count() + " Properties");
                        for (int i = 0; i < order.Properties.Count(); i++)
                        {
                            OrderProperty p = order.Properties[i];
                            logger.Debug("Property " + i + " is " + p.Definition.Name);
                            //Set the Custom Property in the order
                            if (p.Definition.Name == "Currency")
                            {
                                string comment = "";

                                var market = instrument.GetCSRMarket();
                                sophis.static_data.eMQuotationUnitType marketquotationType = market.GetQuotationUnitType();
                                if (marketquotationType == sophis.static_data.eMQuotationUnitType.M_quHundredth)
                                {
                                    logger.Debug("Quotation Type in percent");
                                    logger.Debug("Quotation Type[ " + market + " ]");
                                   
                                    var code = instrument.GetCurrency();
                                    CMString str = new CMString("");
                                    sophis.static_data.CSMCurrency.CurrencyToString(code, str);

                                    var currency = str.StringValue;
                                    var length = currency.Length;
                                    var prefix = currency.Substring(0, length - 1);
                                    var sufix = currency.Substring(length - 1, 1);
                                    var newcurrency = prefix + sufix.ToLowerInvariant();
                                    comment = newcurrency;
                                }
                                else
                                {
                                    logger.Debug("Others");
                                    logger.Debug("Quotation Type[ " + market + " ]");

                                    var code = instrument.GetCurrency();

                                    CMString str = new CMString("");
                                    sophis.static_data.CSMCurrency.CurrencyToString(code, str);

                                    var newcurrency = str.StringValue ;
                                    comment = newcurrency ;
                                }

                                logger.Debug("Currency is : " + comment);
                                //order.SetOrderPropertyValue(p.Definition, comment, user);
                                p.SetValue(comment);
                            }
                        }
                    }

                    //logger.Debug("Start session.Commit");
                    //session.Commit();

                    logger.Debug("<<< TKO_CurrencyUpdate : End");
                    logger.Debug("******************************************************************************************************");
                }
                catch (Exception ex)
                {
                    logger.Error("exception occured --> Canceled: " + ex.ToString());
                    return ActivityInstanceState.Canceled;
                } 
                return ActivityInstanceState.Closed;
            }
        }

        public static UserInfo GetLastUserInfo(Guid workflowInstanceId)
        {
            UserInfo user = new UserInfo();
            //@DPH
            using (ISophisLogger logger = LogManager.GetLogger(LogManager.RegisterLogger("GetLastUserInfo")))
            //using (SophisLogger logger = LogManager.GetDefaultLogger("ActivityUtils", "GetLastUserInfo"))
            {
                try
                {
                    //@DPH
                    //EventRaisingInfo info = Sophis.AaaS.ServicesProvider.Instance.GetService<IEventRaisingInfoProvider>().GetLastEventRaisingInfo();
                    logger.Debug("GetService");
                    var workflowManager = Sophis.AaaS.ServicesProvider.Instance.GetService<IWorkflowManager>();
                    logger.Debug(string.Format("GetWorkflowStateInfo workflowInstanceId = {0}", workflowInstanceId));
                    var workflowStateInfo = workflowManager.GetWorkflowStateInfo(workflowInstanceId);
                    logger.Debug("get LastEventRaisen");
                    var info = workflowStateInfo.LastEventRaisen;
                    //EventRaisingInfo info = WorkflowStatusTrackingService.Instance.GetLastEvent(workflowInstanceId).RaisingInfo;
                    user.UserID = info.UserID;
                    user.WindowsIdentity = info.WindowsIdentity;
                    user.WorkStation = info.WorkStation;
                }
                catch (Exception e)
                {
                    logger.Error("error when GetLastUserInfo: " + e.ToString());
                }
            }
            return user;
        }
	}
}

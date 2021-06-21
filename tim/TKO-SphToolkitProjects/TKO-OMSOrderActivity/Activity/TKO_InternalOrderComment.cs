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
using sophis.instrument;
using TkoPortfolioColumn.DbRequester;
using Sophis.OMS.Activities;

namespace SampleOrderActivity
{
    public class TKO_InternalOrderComment : CodeActivity<ActivityInstanceState>
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
                    logger.Debug("***********************************************************************************");
                    logger.Debug(">>> Internal Order Comment : Begin");

                    IOrder order = context.GetOrder();
                    int orderId = context.GetValue(this.OrderID);
                    //var comment = "NONE";
                    //try
                    //{
                    //    logger.Debug("=> ORDER MANAGER");

                    //    order = OrderManager.Instance.GetOrderById(orderId);

                    //    logger.Debug("<= ORDER MANAGER");
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

                    List<IOrderTarget> orderTarget = (from x in order.GetOrderTargetOwners() select x.Target).ToList();

                    System.Collections.Generic.List<AllocationRule> allocations = (from rulleSet in
                                                                                       (from x in order.GetOrderTargetOwners() select x.AllocationRulesSet)
                                                                                   select rulleSet.Allocations).SelectMany(x => x).ToList();
                    logger.Debug("****RetrieveTradeFromSophisOrderID");
                    
                    var listofpositions = DbrTikehau_Config.RetrieveTradeFromSophisOrderID(orderId);

                    logger.Debug("****size - order id [" + listofpositions.Count + "-" + orderId + "]");
                    if (listofpositions.Any())
                    {
                        for (int i = 0; i < order.Properties.Count(); i++)
                        {
                            OrderProperty p = order.Properties[i];
                            if (p.Definition.Name == "Internal Order Comment")
                            {
                                logger.Debug("=> OrderComments");
                                var commentinfo = OrderManager.Instance.GetComments(orderId);
                                foreach (var elt in commentinfo)
                                {
                                    logger.Debug("OrderComments [" + elt.Value  + " ]");
                                }
                                logger.Debug("<= OrderComments");

                                string usercomment = p.GetValueAsString();
                                logger.Debug("UserComment [" + usercomment + " ]");
                                foreach (AllocationRule rule in allocations)
                                {
                                    int portfoliocode = rule.PortfolioID;
                                    CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(rule.PortfolioID);
                                    var folioname = folio.GetName().StringValue;
                                    logger.Debug("Folio [" + folioname + " ]");
                                    if (folio != null)
                                    {
                                        foreach(var pos in listofpositions)
                                        {
                                            var currentposition = folio.GetTreeViewPosition(pos.MVTIDENT);
                                            CSMTransactionVector transactionvector = new CSMTransactionVector();
                                            currentposition.GetTransactions(transactionvector);
                                            foreach (CSMTransaction deal in transactionvector)
                                            {
                                                logger.Debug("Tansaction id [" + deal.GetPositionID() + " ]");
                                                CMString str = new CMString(usercomment);
                                                deal.SetComment(str);
                                                deal.Store(str);
                                            }
                                            currentposition.Save();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    logger.Debug("***********************************************************************************");
                    //logger.Debug("Start session.Commit");
                    //session.Commit();

                    logger.Debug("<<< Internal Order Comment   : End");
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
                try {
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

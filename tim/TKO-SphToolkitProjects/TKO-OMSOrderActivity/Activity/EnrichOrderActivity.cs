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
using System.Configuration;
using Sophis.OMS.Activities;

namespace SampleOrderActivity
{
    public class EnrichOrderActivity : CodeActivity<ActivityInstanceState>
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
                    logger.Debug(">>> TKO_SetOrderInstruction : Begin");

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

                    System.Collections.Generic.List<AllocationRule> allocations = (from rulleSet in
                                                                                       (from x in order.GetOrderTargetOwners() select x.AllocationRulesSet)
                                                                                   select rulleSet.Allocations).SelectMany(x => x).ToList();

                    System.Collections.Generic.List<IOrderTarget> orderTarget = (from x in order.GetOrderTargetOwners() select x.Target).ToList();
                    int allotment = 0;
                    if (orderTarget.Any())
                    {
                        allotment = orderTarget.First().Allotment;
                        logger.Debug("Allotment " + allotment);
                    }

                    var config = DbrTikehau_Config.GetTikehauConfigFromID(allotment);
                    var confrounding = DbrTikehau_Config.GetTikehauConfigFromID(-1);

                    logger.Debug("Config " + config.Count + " TikehauConfig");
                    logger.Debug("Found " + allocations.Count() + " allocation rules");
                    logger.Debug("Order has  " + order.Properties.Count() + " Properties");
                    for (int i = 0; i < order.Properties.Count(); i++)
                    {
                        OrderProperty p = order.Properties[i];
                        logger.Debug("Property " + i + " is " + p.Definition.Name);
                        //Set the Custom Property in the order
                        if (p.Definition.Name == "Broker Instruction")
                        {
                            string comment = "";
                            if (config.Any())
                                comment += config.First().VALUE + " ";

                            int round = 4;
                            if (confrounding.Any())
                            {
                                if (!int.TryParse(confrounding.First().VALUE, out round))
                                {
                                    round = 4;
                                    logger.Debug("Rounding = 4");
                                }
                                else
                                {
                                    logger.Debug("Rounding [" + round + "]");
                                }
                            }

                            bool first = true;
                            double quantitytotale = 0;
                            foreach (AllocationRule rule in allocations)
                            {
                                quantitytotale += rule.Quantity;
                            }
                            foreach (AllocationRule rule in allocations)
                            {
                               int portfoliocode = rule.PortfolioID;
                               CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(rule.PortfolioID);
                               CSMAmFund fund = CSMAmFund.GetFundFromFolio(folio);
                               var name = fund.GetExternalReference().ToString();
                               logger.Debug("External ref  " + name);
                            
                                var quantityinpercent = (rule.Quantity / quantitytotale) * 100;

                                logger.Debug("without Rounding [" + quantityinpercent + "]");

                                quantityinpercent = Math.Round(quantityinpercent, round);

                                logger.Debug("with Rounding [" + quantityinpercent + "]");

                                if (first)
                                    comment += quantityinpercent + "% " + name;
                                else
                                    comment += ", " + quantityinpercent + "% " + name;
                                first = false;
                            }

                            logger.Debug("Order allocation is : " + comment);
                            logger.Debug("Setting value of OrderProperty");
                            //order.SetOrderPropertyValue(p.Definition, comment, user);
                            p.SetValue(comment);
                        }
                    }

                    //logger.Debug("Start session.Commit");
                    //session.Commit();

                    logger.Debug("<<< TKO_SetOrderInstruction : End");
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

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
    public class AllocationInfo
    {
        private string _positionEffect;
        public string PositionEffect
        {
            get { return _positionEffect; }
            set { _positionEffect = value; }
        }

        private int _instrumentCode;
        public int InstrumentCode
        {
            get { return _instrumentCode; }
            set { _instrumentCode = value; }
        }

        private double _nbsecurities;
        public double NumberOfSecurities
        {
            get { return _nbsecurities; }
            set { _nbsecurities = value; }
        }

        private double _orderquantity;
        public double OrderQuantity
        {
            get { return _orderquantity; }
            set { _orderquantity = value; }
        }
    }


    public class TKO_OpenClose : CodeActivity<ActivityInstanceState>
    {

        public static readonly Dictionary<int, AllocationInfo> DicOfFolioAllocationInfo = new Dictionary<int, AllocationInfo>();

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
                    DicOfFolioAllocationInfo.Clear();

                    //DPH Do not use using here, that will rollback everything at disposition of the session
                    //using (var session = DataAccess.Instance.OpenSession(true))
                    //logger.Debug("Start DataAccess.Instance.OpenSession");
                    //var session = DataAccess.Instance.OpenSession(true);
                    logger.Debug(">>> CheckPositionEffectOrderActivity : Begin");

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

                    List<AllocationRule> allocations = (from rulleSet in
                                                            (from x in order.GetOrderTargetOwners() select x.AllocationRulesSet)
                                                        select rulleSet.Allocations).SelectMany(x => x).ToList();

                    List<IOrderTarget> orderTarget = (from x in order.GetOrderTargetOwners() select x.Target).ToList();

                    //Get Allotment.
                    int allotment = 0;
                    if (allocations.Any())
                    {
                        allotment = orderTarget.First().Allotment;
                        logger.Debug("*** Allotment =>" + allotment);
                    }

                    //Get Instrument.
                    int securityID = 0;
                    int securityType;
                    securityID = orderTarget.First().SecurityID;
                    logger.Debug("*** Instrument Id =>" + securityID);
                    securityType = orderTarget.First().SecurityType;

                    var instrument = CSMInstrument.GetInstance(securityID);
                    if (instrument == null)
                    {
                        logger.Debug("Failed to instanciate instrument instrumentid [ " + securityID + " ]");
                    }
                    eMInstrumentType apitype = ((eMInstrumentType)instrument.GetType_API());

                    logger.Debug("*** API TYPE => " + apitype);
                    if (securityType == ESecurityType.Option)
                    {
                        //check Quantity in Folio by allocation.
                        logger.Debug("*** AllocationRule => " + apitype);
                        foreach (AllocationRule rule in allocations)
                        {
                            int portfoliocode = rule.PortfolioID;
                            using (CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(rule.PortfolioID))
                            {
                                CMString folioname = folio.GetName();
                                logger.Debug("===========================================");

                                logger.Debug("Folio [ " + rule.PortfolioID + "-" + folioname.StringValue + " ]");
                                logger.Debug("Quantity [ " + rule.Quantity + " ]");

                                //double nbsecurities  = GetFolioNbSecuritiesFromSecurityId(securityID, folio);
                                double nbsecurities = 0;
                                var listtrades = DbrTikehau_Config.GetPositionInfoFromFolioidSicovam(rule.PortfolioID, securityID);
                                if (listtrades.Any())
                                {
                                    logger.Debug("QUERY EXECUTION OK");
                                    nbsecurities = listtrades.First().NBSECURITIES;
                                }

                                logger.Debug("nbsecurities [ " + nbsecurities + " ]");

                                string positionstatus = RetrivePositionEffect(rule.Quantity, nbsecurities);
                                logger.Debug("postionstatus [ " + positionstatus + " ]");

                                AllocationInfo current = new AllocationInfo();
                                if (DicOfFolioAllocationInfo.TryGetValue(rule.PortfolioID, out current))
                                {
                                    logger.Debug("Duplicate allocation please check you allocation config");
                                    DicOfFolioAllocationInfo.Clear();
                                    break;
                                }
                                else
                                {
                                    logger.Debug("Add AllocationInfo");

                                    DicOfFolioAllocationInfo.Add(rule.PortfolioID, new AllocationInfo() { InstrumentCode = securityID, OrderQuantity = rule.Quantity, PositionEffect = positionstatus, NumberOfSecurities = nbsecurities });

                                    foreach (var value in DicOfFolioAllocationInfo.Values.SkipWhile(pp => pp == null))
                                    {
                                        logger.Debug("value [ " + value.NumberOfSecurities + " ]");

                                    }
                                    logger.Debug("positionstatus [ " + positionstatus + " ]");
                                    logger.Debug("nb securities [ " + nbsecurities + " ]");
                                }
                            }
                            logger.Debug("===========================================");
                        }

                    logger.Debug("Found " + allocations.Count() + " allocation rules");
                    logger.Debug("Order has  " + order.Properties.Count() + " Properties");

                    string comment = "";
                    for (int i = 0; i < order.Properties.Count(); i++)
                    {
                        OrderProperty p = order.Properties[i];
                        logger.Debug("Property " + i + " is " + p.Definition.Name);
                        //Set the Custom Property in the order
                        if (p.Definition.Name == "Position Effect")
                        {
                            logger.Debug("===================================================================");
                            if (DicOfFolioAllocationInfo == null || DicOfFolioAllocationInfo.Any() == false)
                            {
                                logger.Debug("Null Dictionnary exception");
                                comment = "PleaseSelectValue";

                                //order.SetOrderPropertyValue(p.Definition, comment, user);
                                p.SetValue(comment);
                            }
                            else
                            {
                                logger.Debug("****Check position effect status****");
                                int nbFolio = DicOfFolioAllocationInfo.Keys.Count;
                                logger.Debug("nb Folio [ " + nbFolio + " ]");

                                int nbsecuritiesPositif = 0;
                                int nbsecuritiesNegatif = 0;

                                bool isundefinedValue = false;
                                bool isclosedPositionEffectValue = false;
                                bool isopenPositionEffectValue = false;


                                logger.Debug("count[ " + DicOfFolioAllocationInfo.Values.Count + " ]");
                                foreach (var value in DicOfFolioAllocationInfo.Values.SkipWhile(pp => pp == null))
                                {

                                    logger.Debug("nbsecurities[ " + value.NumberOfSecurities + " ]");
                                    logger.Debug("positionstatus[ " + value.PositionEffect + " ]");
                                    if (value.NumberOfSecurities >= 0)
                                        nbsecuritiesPositif++;

                                    if (value.NumberOfSecurities < 0)
                                        nbsecuritiesNegatif++;

                                    if (value.PositionEffect.CompareTo("PleaseSelectValue") == 0)
                                    {
                                        logger.Debug("PleaseSelectValue");
                                        isundefinedValue = true;
                                    }
                                    if (value.PositionEffect.CompareTo("C") == 0)
                                    {
                                        logger.Debug("C");
                                        isclosedPositionEffectValue = true;
                                    }

                                    if (value.PositionEffect.CompareTo("O") == 0)
                                    {
                                        logger.Debug("O");
                                        isopenPositionEffectValue = true;
                                    }


                                    logger.Debug("nbsecurities[ " + nbsecuritiesPositif + "-" + nbsecuritiesNegatif + " ]");

                                }
                                logger.Debug("nbsecurities[ " + nbsecuritiesPositif + "-" + nbsecuritiesNegatif + " ]");

                                if ((nbsecuritiesPositif == nbFolio) || (nbsecuritiesNegatif == nbFolio))
                                {
                                    if (isundefinedValue == true)
                                    {
                                        return ActivityInstanceState.Faulted;
                                        logger.Debug("Undefined");
                                        comment = "PleaseSelectValue";
                                        //order.SetOrderPropertyValue(p.Definition, comment, user);
                                        p.SetValue(comment);
                                    }
                                    else if (isclosedPositionEffectValue == true && isopenPositionEffectValue == false)
                                    {
                                        logger.Debug("C");
                                        comment = "C";
                                        //order.SetOrderPropertyValue(p.Definition, comment, user);
                                        p.SetValue(comment);
                                    }
                                    else if (isclosedPositionEffectValue == false && isopenPositionEffectValue == true)
                                    {
                                        logger.Debug("O");
                                        comment = "O";
                                        //order.SetOrderPropertyValue(p.Definition, comment, user);
                                        p.SetValue(comment);
                                    }
                                    else
                                    {
                                        return ActivityInstanceState.Faulted;
                                        logger.Debug("U");
                                        comment = "PleaseSelectValue";
                                        //order.SetOrderPropertyValue(p.Definition, comment, user);
                                        p.SetValue(comment);
                                    }
                                }
                                else
                                {
                                    return ActivityInstanceState.Faulted;
                                    logger.Debug("Mixed position effects");
                                    comment = "PleaseSelectValue";
                                    //order.SetOrderPropertyValue(p.Definition, comment, user);
                                    p.SetValue(comment);
                                }
                            }
                        }
                    }
                    }
                    //logger.Debug("Start session.Commit");
                    //session.Commit();

                    logger.Debug("<<< CheckPositionEffectOrderActivity : End");
                }
                catch (Exception ex)
                {
                    logger.Error("exception occured --> Canceled: " + ex.ToString());
                    return ActivityInstanceState.Canceled;
                }
                return ActivityInstanceState.Closed;
            }
        }

        private static double GetFolioNbSecuritiesFromSecurityId(int securityID, CSMPortfolio folio)
        {

            double nbsecurities = 0;
            //CSMPosition position  = folio.GetFlatViewPosition(securityID, eMPositionType.);
            //if (position != null)
            //{
            //    nbsecurities = position.GetInstrumentCount();
            //}
            for (int positionIndex = 0; positionIndex < folio.GetTreeViewPositionCount(); ++positionIndex)
            {
                using (CSMPosition currentposition = folio.GetNthTreeViewPosition(positionIndex))
                {
                    if (currentposition.GetInstrumentCode() == securityID)
                    {

                        nbsecurities += currentposition.GetInstrumentCount();
                    }
                }
            }
            return nbsecurities;
        }


        public static string RetrivePositionEffect(double quantity, double quantityinfolio)
        {
            if (quantityinfolio < 0)
            {
                if (quantity < 0) return "O";

                if (quantity > 0)
                {
                    var absquantityinfolio = Math.Abs(quantityinfolio);
                    if (absquantityinfolio >= quantity)
                    {
                        return "C";
                    }
                    else
                    {
                        return "PleaseSelectValue";
                    }
                }
            }
            else
            {
                if (quantity > 0) return "O";

                if (quantity < 0)
                {
                    var absquantity = Math.Abs(quantityinfolio);
                    if (quantityinfolio >= absquantity)
                    {
                        return "C";
                    }
                    else
                    {
                        return "PleaseSelectValue";
                    }
                }
            }
            return "PleaseSelectValue";
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

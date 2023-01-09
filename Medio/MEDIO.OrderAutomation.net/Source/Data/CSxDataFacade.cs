using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using sophis.oms;
using sophis.utils;
using MEDIO.OrderAutomation.net.Source.DataModel;
using sophis.instrument;
using sophis.oms.execution;
using sophis.portfolio;
using MEDIO.CORE.Tools;
using MEDIO.OrderAutomation.net.Source.Criteria;
using MEDIO.OrderAutomation.NET.Source.DataModel;
using MEDIO.OrderAutomation.NET.Source.GUI;
using Oracle.DataAccess.Client;
using sophis;
using sophis.oms.entry;
//using sophis.oms.entry.gui;
//using sophis.oms.execution.entity;
//using sophis.OrderGeneration;
//using sophis.rma.execution;
//using sophisOrderBlotters;
//using sophisTools;
//using Sophis.AaaS;
using Sophis.OMS.Util;
using Sophis.Windows.OMS;
using System.IO;
using Sophis.OMS.Executions;
using System.Runtime.Serialization;

namespace MEDIO.OrderAutomation.net.Source.Data
{
    public static class CSxDataFacade
    {
        public static IOrderManagerService IOrderManager = OrderManagerConnector.Instance.GetOrderManager();
        private static DateTime _defautForwardDate = DateTime.MinValue;

        /************************
        * LoadSingleOrderData function
        * 
        * Load all the orders of type SingleOrder that have status "status"
        ***********************/
       
        /// <summary>
        /// Returns true if the order should be filtered (i.e. not displayed in the GUI)
        /// </summary>
        /// <param name="singleOrder"></param>
        /// <returns></returns>
        public static bool ShouldBeFiltered( SingleOrder singleOrder)
        {
            if (CSxHedgingFundingCriterium.IsFXDeal(singleOrder.Target.SecurityID))
            {
                return true;
            }

            //JIRA-543 remove types F&D
            CSMInstrument inst = CSMInstrument.GetInstance(singleOrder.Target.SecurityID);
            if (inst != null)
            {
                if (inst.GetType_API() == 'F' || inst.GetType_API() == 'D')
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Load all executions from db and break down into allocations
        /// </summary>
        /// <returns></returns>
        public static CSxCustomAllocationData GetAllAllocatedExecutions()
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxDataFacade", MethodBase.GetCurrentMethod().Name);
                CSxCustomAllocationData res = new CSxCustomAllocationData();

                try
                {
                    string filterStr = "[Creation Date] <> #1904-01-01#"; // work around for an API issue
                                                                          //Performance: the request below is not very long (<10s)
                                                                          //The code here is the same than the code executed by Execution Blotter
                                                                          //Looks like filterStr is used in the SQL request. If needed investigate with debug hook activated.
                                                                          //this code filters on orders: string filterStr = "[Creation Date] > #2018-08-08#"; 


                    //TODO Cleanup
                    /*
                    public ExecutionBlotterData GetExecutionsList(ExecutionBlotterProperty blotterProperty)
                    {
	                    using (SophisLogger sophisLogger = LogManager.GetDefaultLogger())
	                    {
		                    ExecutionBlotterData executionBlotterData = GetBlotterData(blotterProperty.ID);
		                    if (executionBlotterData == null)
		                    {
			                    sophisLogger.Debug("Getting execution from blotter filter...");
			                    IFilteredEnumerable<OrderExecution> orderExecutions = OrderManagerConnector.Instance.GetOrderManager().GetOrderExecutions(blotterProperty.FilterString);
			                    sophisLogger.DebugFormat("Got {0} executions.", orderExecutions.Count());
			                    executionBlotterData = new ExecutionBlotterData(orderExecutions);
			                    AddBlotterData(blotterProperty.ID, executionBlotterData);
		                    }
		                    return executionBlotterData;
	                    }
                    }
                     */
                    //Old v713 code
                    //var executionList = ExecutionBlotterDataManager.Instance.GetExecutionsList(new ExecutionBlotterProperty("", "", filterStr, 1, true));
                    Stream orderExecutions = OrderManagerConnector.Instance.GetOrderManagerStream().GetOrderExecutions(filterStr);
                    IFilteredEnumerable<OrderExecution> filteredEnumerable = new NetDataContractSerializer().ReadObject(orderExecutions) as IFilteredEnumerable<OrderExecution>;
                    ExecutionBlotterProperty blotterProperty = new ExecutionBlotterProperty("", "", filterStr, 1);
                    ExecutionBlotterData executionList = new ExecutionBlotterData(filteredEnumerable);

                    foreach (var exec in executionList)
                    {
                        SingleOrder singleOrder = exec.Order as SingleOrder;
                        if (singleOrder == null) continue;
                        if (singleOrder.Target == null) continue;

                        if (ShouldBeFiltered(singleOrder) == true)
                        {
                            continue;
                        }

                        var toAdd = GetAllocations(exec.OrderExecution, singleOrder);
                        if (toAdd.Count > 0)
                            res.AddRange(toAdd);
                    }

                }
                catch (Exception ex)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_error, ex.Message);
                    MessageBox.Show(ex.Message);
                    throw;
                }

                LOG.End();
                return res;
            }
        }

        public static List<CSxCustomAllocation> GetAllocations(OrderExecution exec, SingleOrder singleOrder)
        {
            List<CSxCustomAllocation> res = new List<CSxCustomAllocation>();
            if (exec == null) return res;
            if (IsExecutionHedgedOrFunded(exec)) return res;
            foreach (var allocation in exec.Allocations)
            {
                int fundCCY = 0;
                Tools.CSxUtils.GetFundCurrency(allocation.FolioID, out fundCCY);
                if (fundCCY != 0 && singleOrder.Target.Currency != 0 && fundCCY != singleOrder.Target.Currency)
                {
                    CSxCustomAllocation customAlloc = new CSxCustomAllocation(singleOrder, allocation, exec);
                    if (!res.Contains(customAlloc))
                        res.Add(customAlloc);
                }
            }
            return res;
        }

        public static bool CreateHedgingFundingOrders(List<CSxCustomAllocation> allocatedExecutions)
        {
            using (var LOG = new CSMLog())
            {
                bool res = false;
                LOG.Begin("CSxDataFacade", MethodBase.GetCurrentMethod().Name);
                var newOrders = new List<IOrder>();
                try
                {
                    if (allocatedExecutions.IsNullOrEmpty()) return res;
                    if (IOrderManager == null)
                    {
                        LOG.Write(CSMLog.eMVerbosity.M_error, "Failed to get order manager service");
                        MessageBox.Show("Failed to get order manager service");
                    }

                    var lstOrders = new List<IOrder>();
                    var fxspots = CSxCustomAllocation.CreateSpotOrders(allocatedExecutions);
                    foreach (var fxspot in fxspots)
                    {
                        if (fxspot.QuantityData.OrderedQty != 0.0)
                            lstOrders.Add(fxspot);
                    }
                    var fwd = CSxCustomAllocation.CreateOneFwdOrder(allocatedExecutions);
                    if (fwd != null)
                    {
                        if (fwd.QuantityData.OrderedQty != 0)
                            lstOrders.Add(fwd);
                    }
                    if (!lstOrders.IsNullOrEmpty())
                    {
                        InitInstrument(lstOrders);
                        newOrders = IOrderManager.CreateOrders(lstOrders, false, lstOrders.FirstOrDefault().CreationInfo).ToList();
                        UpdateOrders(newOrders, allocatedExecutions.FirstOrDefault().SingleOrder.ID);
                        newOrders = IOrderManager.CreateOrders(newOrders, true, lstOrders.FirstOrDefault().CreationInfo).ToList();
                        res = true;
                    }
                    else
                        LOG.Write(CSMLog.eMVerbosity.M_warning,
                            "FX spot and/or forward order were not initialised properly. No order created.");
                }
                catch (Exception ex)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_error, ex.Message);
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (newOrders.Count != 0)
                    {
                        UpdateParentOrderExecutions(allocatedExecutions.Select(x => x.Execution).Distinct().ToList());
                        string msg = String.Format("{0} order(s) have been created! Press OK to preview.",
                            newOrders.Count);
                        DialogResult result = MessageBox.Show(msg, "Orders creation", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Information);
                        if (result == DialogResult.OK)
                        {
                            var report = GetOrderReports(newOrders);
                            CSxOrderSendingReport.Display(report);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No order has been created!", "Orders creation", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                LOG.End();
                return res;
            }
        }

        public static List<DateTime> GetListOfForwarDates()
        {
            using (var LOG = new CSMLog())
            {
                List<DateTime> res = new List<DateTime>();
                LOG.Begin("CSxDataFacade", "GetListOfForwarDates");
                string bucketName = MEDIO_CUSTOM_PARAM.HEDGING_FUNDING_ORDERS.Instance.BucketSetModelName;
                string sql = String.Format("select low_end_point from bucket_intervals " +
                                           "where BUCKET_ID = (select id from buckets " +
                                           "where BUCKET_SET_ID = (select id from bucket_sets " +
                                           "where name = '{0}'))", bucketName);
                var dates = CSxDBHelper.GetMultiRecords(sql);
                int i = 0;
                foreach (var one in dates)
                {
                    var date = MEDIO.CORE.Tools.CSxUtils.GetDateFromSophisTime(Convert.ToInt32(one));
                    if (!res.Contains(date))
                        res.Add(date);
                    // Caching first date
                    if (i == 0)
                        _defautForwardDate = date;
                    i++;
                }
                return res;
            }
        }

        public static List<CSxFXRollDataModel> GetFxRollViewModels(DateTime startDay, DateTime endDay, DateTime? forwardDate = null)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxDataFacade", MethodBase.GetCurrentMethod().Name);
                List<CSxFXRollDataModel> res = new List<CSxFXRollDataModel>();
                try
                {
                    var pExtraction = CreateFXRollExtraction();
                    CSMPortfolio portfolio = CSMPortfolio.GetRootPortfolio(pExtraction);
                    while (portfolio != null)
                    {
                        CSxFXRollDataModel toAdd = new CSxFXRollDataModel(portfolio, pExtraction);
                        if (forwardDate != null) toAdd.SetForwardDate(forwardDate.Value);
                        if (toAdd.Level == 1)
                            res.Add(toAdd);
                        else if (toAdd.ExpiryDate >= startDay && toAdd.ExpiryDate <= endDay && toAdd.IsRealPosition())
                            res.Add(toAdd);
                        portfolio = portfolio.GetNextPortfolio();
                    }
                }
                catch (Exception e)
                {
                    CSMLog.Write("CSxDataFacade", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_warning, "Could not init FX roll view data! " + e.ToString());
                    return res;
                }
                return res;
            }
        }

        /// <summary>
        /// Default: the first date in dates bucket from TODAY() + 5 working days
        /// </summary>
        /// <returns></returns>
        public static DateTime GetFirstOrDefaulgtForwardDate()
        {
            if (_defautForwardDate == DateTime.MinValue)
            {
                var dates = GetListOfForwarDates();
                var selectedDates = new List<DateTime>();
                foreach (var date in dates)
                {
                    if (date >= (DateTime.Today.AddDays(5)))
                        selectedDates.Add(date);
                }
                if (!selectedDates.IsNullOrEmpty())
                    _defautForwardDate = dates.Min();
                else
                    _defautForwardDate = DateTime.Now.AddMonths(1);

                while (_defautForwardDate.DayOfWeek == DayOfWeek.Saturday ||
                       _defautForwardDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    _defautForwardDate = _defautForwardDate.AddDays(1);
                }
            }
            return _defautForwardDate;
        }

        public static List<CSxOrderReport> GetOrderReports(IList<IOrder> newOrders)
        {
            List<CSxOrderReport> res = new List<CSxOrderReport>();
            foreach (var order in newOrders)
            {
                CSxOrderReport one = new CSxOrderReport();
                SingleOrder singleOrder = order as SingleOrder;
                one.OrderID = order.ID;
                one.Instrument = Tools.CSxUtils.GetInstrumentName(singleOrder.Target.SecurityID);
                one.Amount = singleOrder.QuantityData.OrderedQty;
                one.Currency = Tools.CSxUtils.GetCurrencyName(singleOrder.Target.Currency);
                one.Info = singleOrder.Side.ToString();
                res.Add(one);
            }
            return res;
        }

        public static void UpdateOrders(List<IOrder> newOrders, int originalOrderID)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxDataFacade", MethodBase.GetCurrentMethod().Name);
                foreach (var order in newOrders)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Order {0} has been successfully created.", order.ID));
                    if (originalOrderID != 0)
                    {
                        SetCCY2AsFixedCurrency(order as SingleOrder);
                        SetParentOrderIDProperty(originalOrderID, order);

                        // BUG: set order thirdparty upon creation is not working, update after it's created!
                        var parentOrder = IOrderManager.GetOrderById(originalOrderID);
                        foreach (var third in parentOrder.ThirdParties)
                        {
                            order.SetThirdParty(third.ThirdPartyID, third.Role);
                        }
                    }
                    OrderCreationValidatorManager.Instance.Validate(order, true);

                }
                //CSxOrderHelper.UpdateOrders(newOrders);
            }
        }

        public static void SetParentOrderIDProperty(int originalOrderID, IOrder order)
        {
            bool found = false;
            string propName = MedioConstants.MEDIO_ORDER_PROPERTY_PARENT_ORDERID;
            foreach (var prop in order.Properties)
            {
                if (prop.Definition.Name == propName)
                {
                    found = true;
                }
            }
            if (!found)
            {
                OrderProperty prop = new OrderProperty();
                prop.Definition = new OrderPropertyDefinition();
                string sql = String.Format("select id from order_property where name = '{0}'", propName);
                prop.Definition.ID = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql));
                sql = String.Format("select category from order_property where name = '{0}'", propName);
                prop.Definition.Category = Convert.ToString(CSxDBHelper.GetOneRecord(sql));
                sql = String.Format("select datatype from order_property where name = '{0}'", propName);
                prop.Definition.DataType = Convert.ToString(CSxDBHelper.GetOneRecord(sql));
                prop.Definition.Name = propName;
                prop.Definition.Visibility = EOrderPropertyVisibility.Visible;
                order.Properties.Add(prop);
            }
            CSxOrderHelper.SetOrderProperty(order.Properties, originalOrderID, propName);
        }

        private static void InitInstrument(IList<IOrder> newOrders)
        {
            foreach (var iorder in newOrders)
            {
                OESingleOrder oeSingleOrder = OEOrderFactory.GetHandler(iorder, iorder.Kind) as OESingleOrder;
                if (oeSingleOrder != null)
                {
                    oeSingleOrder.InitializeTargetInstrument(oeSingleOrder.GetInstrumentCode());
                }
            }
        }

        private static void UpdateParentOrderExecutions(List<OrderExecution> OrderExecutions)
        {
            if (OrderExecutions != null && OrderExecutions.Any())
            {
                UpdateExecPropInDB(OrderExecutions, MedioConstants.MEDIO_EXEC_PROPERTY_ISHEDGEDFUNDED);
                ExecutionBlotterDataManager executionBlotterDataManager = ExecutionBlotterDataManager.Instance;
                executionBlotterDataManager.AddOrUpdateExecutions(OrderExecutions);
                //Sophis.AaaS.Coherency.ICoherencyMessageSender coherencyMessageSender = ServicesProvider.Instance.GetService<Sophis.AaaS.Coherency.ICoherencyMessageSender>();
                CSMEventManager.Instance.SendEvent(Sophis.OMS.Messaging.CoherencyEventId.OMS_EXECUTION_UPDATED, OrderExecutions);
                //coherencyMessageSender.SendMessage(sophis.oms.Coherency.CoherencyEventId.OMS_EXECUTION_UPDATED, OrderExecutions);
            }
        }

        private static bool IsExecutionHedgedOrFunded(OrderExecution exec)
        {
            bool res = false;
            if (exec.Properties != null)
            {
                ExecutionProperty prop = CSxOrderHelper.GetExecProperty(exec.Properties, MedioConstants.MEDIO_EXEC_PROPERTY_ISHEDGEDFUNDED);
                if (prop != null)
                {
                    res = prop.GetValueAsBoolean();
                    // Try again - from db
                    if (!res)
                    {
                        string sql = String.Format("select value from EXEC_PROPERTY_ASSOCIATION where sophisexecid = {0} and propertyid = {1}", exec.SophisExecID, prop.Definition.ID);
                        res = CSxDBHelper.GetOneRecord<string>(sql).ToUpper().Equals("TRUE");
                    }
                }
            }
            return res;
        }

        private static CSMExtraction CreateFXRollExtraction()
        {
            ArrayList entrypoint = new ArrayList() {1};
            ArrayList criteria = new ArrayList();
            criteria.Add(CSMCriterium.GetCriteriumType("Currency"));
            criteria.Add(CSMCriterium.GetCriteriumType("Instrument Code"));
            criteria.Add(CSMCriterium.GetCriteriumType("Fund"));
            criteria.Add(CSMCriterium.GetCriteriumType("Parent Order ID"));
            criteria.Add(CSMCriterium.GetCriteriumType("Portfolio name"));
            var res = new CSMExtractionCriteria(criteria, entrypoint, true);
            res.SetHierarchicCriteria(true);
            res.KeepPositionId(true);
            CMString query = "sicovam in (select sicovam from titres where type in ('X','K','E'))";
            res.SetQuery(query);
            res.SetFilteredDeals(CSMExtraction.eMFilteredDeals.M_eNoAccess);
            res.Create();
            return res;
        }

        /// <summary>
        /// FX Spot: Receive USD vs EUR 
        /// FX FWD: Pay USD vs EUR 
        /// </summary>
        /// <param name="order"></param>
        public static void SetCCY2AsFixedCurrency(SingleOrder order)
        {
            SingleOrder singleOrder = order as SingleOrder;
            if (singleOrder != null)
            {
                CSMForexSpot fx = CSMForexSpot.GetInstance(singleOrder.Target.SecurityID);
                CSMForexFuture fxfwd = CSMInstrument.GetInstance(singleOrder.Target.SecurityID);
                if (fx != null)
                {
                    var settlementDate = order.SettlementDate;
                    OESingleOrder oeSingleOrder = OEOrderFactory.GetHandler(order, order.Kind) as OESingleOrder;
                    int ccy2 = fx.GetForex2();
                    oeSingleOrder.SetForexFixedCurrency(ccy2); // this will reset SettlementDate!
                    oeSingleOrder.SettlementDate = settlementDate; // so we revert
                    oeSingleOrder.UpdateSettlementDate();
                }
                else if (fxfwd != null)
                {
                    OESingleOrder oeSingleOrder = OEOrderFactory.GetHandler(order, order.Kind) as OESingleOrder;
                    int ccy2 = fxfwd.GetCurrencyCode();
                    oeSingleOrder.SetForexFixedCurrency(ccy2);
                }
            }
        }

        /// <summary>
        /// CSxOrderHelper.SetExecProperty and CoherencyEventId.OMS_EXECUTION_UPDATED not working 
        /// We have to insert into db manually
        /// </summary>
        /// <param name="orderExecutions"></param>
        /// <param name="execPropertyName"></param>
        private static void UpdateExecPropInDB(IList<OrderExecution> orderExecutions, String execPropertyName)
        {
            foreach (var exec in orderExecutions)
            {
                if (exec.Properties != null)
                {
                    ExecutionProperty prop = CSxOrderHelper.GetExecProperty(exec.Properties, execPropertyName);
                    if (prop != null)
                    {
                        string sql = "Update EXEC_PROPERTY_ASSOCIATION set value = :value where sophisexecid = :execid and propertyid = :propid";
                        OracleParameter parameter = new OracleParameter(":execid", exec.SophisExecID);
                        OracleParameter parameter2 = new OracleParameter(":propid", prop.Definition.ID);
                        OracleParameter parameter3 = new OracleParameter(":value", true.ToString());
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter, parameter2, parameter3 };
                        CSxDBHelper.Execute(sql, parameters);
                        prop.SetValue(true);
                    }
                }
            }
        }

    }
}

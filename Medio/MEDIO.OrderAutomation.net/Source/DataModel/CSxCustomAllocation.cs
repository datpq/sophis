using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using DevExpress.Data;
using DevExpress.Data.Linq;
using DevExpress.XtraGrid;
using Iesi.Collections.Generic;
using MEDIO.MEDIO_CUSTOM_PARAM;
using MEDIO.OrderAutomation.net.Source.Data;
using MEDIO.OrderAutomation.net.Source.Tools;
//using NSREnums;
using sophis.instrument;
using sophis.market_data;
using sophis.oms;
using sophis.oms.entry;
using Sophis.OMS.Util;
//using sophis.oms.entry.gui;
using sophis.oms.execution;
//using Sophis.OMS.Executions;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
//using sophisOrderBlotters;
using SophisAMDotNetTools;
using Sophis.OrderBookCompliance;
using sophisTools;
using Sophis.AaaS;
using sophis.value;

namespace MEDIO.OrderAutomation.net.Source.DataModel
{
    public class CSxCustomAllocation : SingleOrder
    {
        #region Fields
        /// <summary>
        ///  GUI Fields
        /// </summary>
        public string InstrumentRef { get; set; }

        public int OrderID { get; set; }
        public bool Checked { get; set; }
        public bool ToBeFunded { get; set; }
        public bool ToBeHedged { get; set; }
        public double ExecAmount { get; set; }
        public double ExecQty { get; set; }
        public string ExecSide { get; set; }
        public double ExecPrice { get; set; }
        public int ExecutionState { get; set; }
        public int InstrumentCCYCode { get; set; }
        public string InstrumentCCY { get; set; }
        public DateTime ForwardDate { get; set; }
        public DateTime OrderCreationDate
        {
            get { return SingleOrder.CreationInfo.TimeStamp; }
        }
        public string OrderCreationUser { get; set; }
        public DateTime ExecutionDate { get; set; }
        public DateTime ExecutionValueDate { get; set; }
        public string FolioPath { get; set; }
        public string FolioName { get; set; }
        public int FolioCCYCode { get; set; }
        public string FolioCCY { get; set; }
        public int ExecutionID { get; set; }

        public double HedgingAmountCCY1
        {
            get
            {
                this._hedgingAmountCCY1 = GetCCY1Amount(_hedgingAmountCCY2);
                return Math.Round(this._hedgingAmountCCY1, 2);
            }
            set
            {
                this._hedgingAmountCCY1 = value;
                this._hedgingAmountCCY2 = GetCCY2Amount(_hedgingAmountCCY1);
            }
        }

        public double HedgingAmountCCY2
        {
            get { return Math.Round(this._hedgingAmountCCY2, 2); }
            set { this._hedgingAmountCCY2 = value; }
        }

        public double FundingAmountCCY1
        {
            get
            {
                this._fundingAmountCCY1 = GetCCY1Amount(_fundingAmountCCY2);
                return Math.Round(this._fundingAmountCCY1, 2);
            }
            set
            {
                this._fundingAmountCCY1 = value;
                this._fundingAmountCCY2 = GetCCY2Amount(_fundingAmountCCY1);
            }
        }

        public double FundingAmountCCY2
        {
            get { return Math.Round(this._fundingAmountCCY2, 2); }
            set { this._fundingAmountCCY2 = value; }
        }

        public double FundingProportion { get; set; }
        public double HedgingProportion { get; set; }
        public double AlloactionPct { get; set; }
        public double AlloactionQty { get; set; }
        public AllocationRule AllocationRule { get; set; }
        public double originalHedgingAmountCCY1 = 0;
        public double originalHedgingAmountCCY2 = 0;
        public double originalFundingAmountCCY1 = 0;
        public double originalFundingAmountCCY2 = 0;

        /// <summary>
        ///  %NAV - Funding : ([Funding%*executed amount ]/NAV)*100%
        /// </summary>
        public double NAVFunding { get; set; }

        /// <summary>
        ///  %NAV – Hedging : ([Hedging%*executed amount]/NAV )*100%
        /// </summary>
        public double NAVHedging { get; set; }

        public OrderExecution Execution
        {
            get { return _execution; }
        }

        /// <summary>
        /// private
        /// </summary>
        public readonly SingleOrder SingleOrder;

        public readonly OrderExecutionAllocation _executionAllocation;
        private readonly OrderExecution _execution;

        private double _hedgingAmountCCY1 { get; set; }
        private double _hedgingAmountCCY2 { get; set; }
        private double _fundingAmountCCY1 { get; set; }
        private double _fundingAmountCCY2 { get; set; }
        #endregion

        public CSxCustomAllocation()
        {
            this.Checked = true;
            this.ToBeHedged = true;
            this.ToBeFunded = true;
            this.ForwardDate = CSxDataFacade.GetFirstOrDefaulgtForwardDate();
        }

        public CSxCustomAllocation(SingleOrder order, OrderExecutionAllocation allocation, OrderExecution execution)
        {
            this.ToBeHedged = true;
            this.ToBeFunded = true;
            this.SingleOrder = order;
            this._executionAllocation = allocation;
            this._execution = execution;
            this.Checked = true;
            this.ForwardDate = CSxDataFacade.GetFirstOrDefaulgtForwardDate();
            this.FundingProportion = 100;
            this.HedgingProportion = 100;
            this.CloneSingleOrder();
            this.CloneAllocation();
            this.ExecutionState = order.QuantityData.OrderedQty == order.QuantityData.AllocatedQty ?
                (int)MedioConstants.EOrderExecutionState.TotallyExecuted : (int)MedioConstants.EOrderExecutionState.PartiallyExecuted;
        }

        static SortedDictionary<uint, string> _userNameVsId = new SortedDictionary<uint, string>();

        protected string GetUserName(uint userId)
        {
            string userName;
            if (_userNameVsId.TryGetValue(userId, out userName))
            {
                return userName;
            }
            //This method costs a lot. Cache the result
            CSMUserRights user = new CSMUserRights(userId);
            userName = user.GetName();
            _userNameVsId[userId] = userName;
            
            return userName;
        }

        protected void CloneSingleOrder()
        {
            // inherent from parent order 
            this.InstrumentCCYCode = SingleOrder.Target.Currency;
            this.InstrumentCCY = CSxUtils.GetCurrencyName(this.InstrumentCCYCode);
            this.ID = SingleOrder.ID;
            this.OrderID = SingleOrder.ID;
            this.AllocationRulesSet = SingleOrder.AllocationRulesSet;
            this.Assignation = SingleOrder.Assignation;
            this.BestExecutionRules = SingleOrder.BestExecutionRules;
            this.BookingType = SingleOrder.BookingType;
            this.BusinessEventId = SingleOrder.BusinessEventId;
            this.Comments = SingleOrder.Comments;
            this.ComplianceSession = SingleOrder.ComplianceSession;
            this.EffectiveTime = SingleOrder.EffectiveTime;
            this.ExternalId = SingleOrder.ExternalId;
            this.ExternalSystem = SingleOrder.ExternalSystem;
            this.GroupID = SingleOrder.GroupID;
            this.IsAudit = SingleOrder.IsAudit;
            this.IsMerged = SingleOrder.IsMerged;
            this.Kind = SingleOrder.Kind;
            this.LastPlacementId = SingleOrder.LastPlacementId;
            this.NHVersion = SingleOrder.NHVersion;
            this.OrderType = SingleOrder.OrderType;
            this.OriginationStrategy = SingleOrder.OriginationStrategy;
            this.OwnerInfo = SingleOrder.OwnerInfo;
            this.Properties = SingleOrder.Properties;
            this.QuantityData = SingleOrder.QuantityData;
            this.SessionID = SingleOrder.SessionID;
            this.SettlementDate = _execution.ValueDate.GetValueOrDefault();
            this.SettlementType = SingleOrder.SettlementType;
            this.Side = SingleOrder.Side;
            this.Target = SingleOrder.Target;
            this.TimeInForce = SingleOrder.TimeInForce;
            this.TimeInForce.Type = ETimeInForce.Day;
            this.TradingSessions = SingleOrder.TradingSessions;
            this.UpdateInfo = SingleOrder.UpdateInfo;
            this.Workflow = SingleOrder.Workflow;

            // New info
            this.CreationInfo = new CreationInfo();
            uint userid = (uint)SingleOrder.CreationInfo.UserID;
            OrderCreationUser = GetUserName(userid);

            this.OrderCreationUser = "";

            CSMInstrument targetInstrument = CSMInstrument.GetInstance(SingleOrder.Target.SecurityID);
            //Instrument name
            if (targetInstrument != null)
            {
                using (CMString instrumentReference = targetInstrument.GetName())
                {
                    this.InstrumentRef = instrumentReference;
                }
            }
        }

        protected void CloneAllocation()
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                int ccy = 0;
                this.FolioCCY = CSxUtils.GetFundCurrency(_executionAllocation.FolioID, out ccy);
                this.FolioPath = CSxUtils.GetFullFolioPath(_executionAllocation.FolioID);
                this.FolioName = CSxUtils.GetFolioName(_executionAllocation.FolioID);
                this.FolioCCYCode = ccy;

                // ExecQty = Math.Round((double)_execution.Placement.ExecutedQuantity,2); // Does not work
                foreach (var alloc in _execution.Allocations)
                {
                    if (alloc != null)
                        ExecQty += alloc.Qty;
                }
                // getting net amount from from an execution is not convenient - as it may include accrued amount 
                try
                {
                    CSMTransaction trade = sophis.oms.OMSUtils.GetTransactionFromExecution(_execution, false);
                    if (trade != null)
                    {
                        //ExecPrice = ExecQty == 0 ? 0 : Math.Round(ExecAmount / ExecQty, 2);
                        ExecPrice = _executionAllocation.Price ?? 0;
                        ExecAmount = trade.GetNetAmount();
                    }
                    else
                    {
                        ExecAmount = _execution.Amount ?? 0;
                    }
                }
                catch //(Exception e)
                {
                    // in case of deal not found exception is throw
                    ExecAmount = _execution.Amount ?? 0;
                }
                //ExecPrice = ExecQty == 0 ? 0 : Math.Round(ExecAmount / ExecQty, 2);
                ExecPrice = _executionAllocation.Price ?? 0;
                ExecSide = Enum.GetName(typeof(ESide), SingleOrder.Side);
                double nav = CSxUtils.GetFundNAV(_executionAllocation.FolioID);
                NAVFunding = nav == 0 ? 0 : ExecAmount * FundingProportion / 100 / nav;
                NAVHedging = nav == 0 ? 0 : ExecAmount * HedgingProportion / 100 / nav;
                this.AlloactionPct = ExecAmount == 0 ? 0 : _executionAllocation.Qty / ExecQty * 100;
                this.AlloactionQty = _executionAllocation.Qty;
                ExecutionID = _execution.SophisExecID;
                ExecutionDate = _execution.CreationTimeStamp;
                ExecutionValueDate = _execution.ValueDate.HasValue ? _execution.ValueDate.Value : _execution.CreationTimeStamp;
                this._hedgingAmountCCY2 = this._fundingAmountCCY2 = this.ExecAmount * AlloactionPct / 100;
                this.originalHedgingAmountCCY2 = this.originalFundingAmountCCY2 = this.ExecAmount * AlloactionPct / 100;
                this.originalHedgingAmountCCY1 = this.originalFundingAmountCCY1 = this.FundingAmountCCY1;
                CheckCurrency();
                LOG.End();
            }
        }

        private void CheckCurrency()
        {
            var payCcy = CSxUtils.GetCurrencyName(_execution.PaymentCurrency);
            var ccyUpper = payCcy.ToUpper();
            var ccyCode = CSMCurrency.StringToCurrency(ccyUpper);
            if (!ccyUpper.Equals(payCcy))
            {
                this.originalHedgingAmountCCY2 /= 100;
                this.originalFundingAmountCCY2 /= 100;
                this.HedgingAmountCCY2 /= 100;
                this.FundingAmountCCY2 /= 100;
            }
            this.InstrumentCCY = ccyUpper;
            this.InstrumentCCYCode = ccyCode;
        }

        private double GetCCY1Amount(double ccy2Amount)
        {
            double res = 0;
            CSMForexSpot forexSpot = CSMForexSpot.GetCSRForexSpot(FolioCCYCode, InstrumentCCYCode);
            if (forexSpot != null)
            {
                double fx = CSMMarketData.GetCurrentMarketData().GetDayForex(InstrumentCCYCode, FolioCCYCode);
                if (fx != 0)
                    res = fx * ccy2Amount;
                else
                    res = ccy2Amount;
            }
            return res;
        }

        private double GetCCY2Amount(double ccy1Amount)
        {
            double res = 0;
            CSMForexSpot forexSpot = CSMForexSpot.GetCSRForexSpot(InstrumentCCYCode, FolioCCYCode);
            if (forexSpot != null)
            {
                double fx = CSMMarketData.GetCurrentMarketData().GetDayForex(InstrumentCCYCode, FolioCCYCode);
                if (fx != 0)
                    res = ccy1Amount / fx;
                else
                    res = ccy1Amount;
            }
            return res;
        }

        public static List<IOrder> CreateSpotOrders(List<CSxCustomAllocation> allocatedExecutions)
        {
            List<IOrder> res = new List<IOrder>();
            var groupedAllocations = allocatedExecutions.GroupBy(x => new { x._execution.ValueDate }).Select( p => p.ToList());
            foreach (var group in groupedAllocations)
            {
                var toAdd = CreateOneSpotOrder(group);
                if (toAdd != null) res.Add(toAdd);
            }
            return res;
        }

        public static IOrder CreateOneSpotOrder(List<CSxCustomAllocation> allocatedExecutions)
        {
            SingleOrder res = new SingleOrder();
            if (allocatedExecutions.IsNullOrEmpty()) return null; // Null has to be handled later 
            var _singleOrder = allocatedExecutions[0].SingleOrder;
            var firstAllocation = allocatedExecutions[0];
            res.AllocationRulesSet = new AllocationRulesSet();
            res.AllocationRulesSet.Allocations = new List<AllocationRule>();
            res.AllocationRulesSet.QuantityType = EQuantityType.Amount;
            res.AllocationRulesSet.RulesSetID = _singleOrder.AllocationRulesSet.RulesSetID;
            res.SettlementDate = firstAllocation.SettlementDate >= DateTime.Today ? firstAllocation.SettlementDate : DateTime.Today.AddDays(2);
      
            res.SettlementType = _singleOrder.SettlementType;
            res.EffectiveTime = DateTime.Now;
            res.OriginationStrategy = HEDGING_FUNDING_ORDERS.Instance.OriginationStrat;
            res.Properties = new List<OrderProperty>();
            foreach (var p in _singleOrder.Properties)
            {
                var prop = p.Clone();
                res.Properties.Add(prop);
            }
            res.SetCounterParty(_singleOrder.GetCounterParty());
            res.ExternalSystem = _singleOrder.ExternalSystem;
            res.BusinessEventId = _singleOrder.BusinessEventId;
            res.TimeInForce = _singleOrder.TimeInForce;
            res.TimeInForce.Type = ETimeInForce.Day;
            res.TradingSessions = _singleOrder.TradingSessions;
            res.BookingType = EBookingType.Regular;
            res.CreationInfo = new CreationInfo();
            res.CreationInfo.TimeStamp = DateTime.Now;
            // Set current user
            CSMUserRights user = new CSMUserRights();
            res.CreationInfo.UserID = user.GetIdent();
            res.CreationInfo.ProfileID = user.GetAllProfiles().Cast<int>().ToList().FirstOrDefault();
            res.CreationInfo.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            res.CreationInfo.WorkStation = Environment.MachineName;
            res.Assignation = new Assignation();
            res.Assignation.TimeStamp = DateTime.Now;
            res.Assignation.UserID = user.GetIdent();
            res.Assignation.ProfileID = res.CreationInfo.ProfileID;
            res.Assignation.WindowsIdentity = res.CreationInfo.WindowsIdentity;
            res.Assignation.WorkStation = res.CreationInfo.WorkStation;
            res.OrderType = new OrderType();
            res.OrderType = _singleOrder.OrderType;
            res.OwnerInfo = new OwnerInfo();
            res.OwnerInfo = _singleOrder.OwnerInfo;
            res.ThirdParties.Clear();
            res.BestExecutionRules = new List<BestExecutionRule>();
            foreach (BestExecutionRule exec in res.BestExecutionRules)
            {
                var item = (BestExecutionRule)exec.Clone();
                res.BestExecutionRules.Add(item);
            }
            res.QuantityData = new Quantity();
            res.QuantityData.ExecutedAmount = 0;
            res.QuantityData.ExecutedQty = 0;
            res.QuantityData.ExecutionsCount = 0;
            res.QuantityData.IsTotallyExecuted = false;
            res.Side = ESide.Sell;

            //TODO test and cleanup
            //int displayInstrCode = CSAMInstrumentDotNetTools.GetSettlementReportingInstrumentId(firstAllocation.FolioCCYCode,
            //    firstAllocation.InstrumentCCYCode, res.SettlementDate,
            //    EForexNDFCreationBehaviour.UseCcySettings, true);
            int displayInstrCode = 
                CSAMInstrumentDotNetTools.GetForexInstrument(firstAllocation.FolioCCYCode,firstAllocation.InstrumentCCYCode, sophis.amCommon.DateUtils.ToInt(res.SettlementDate));
            CSMForexFuture fxfwd = CSMInstrument.GetInstance(displayInstrCode);
            CSMForexSpot forexSpot = CSMInstrument.GetInstance(displayInstrCode);
            if (fxfwd != null)
            {
                var target = new ForexTarget();
                target.Expiry = res.SettlementDate;
                target.SecurityType = ESecurityType.Forex;
                target.SecurityID = displayInstrCode;
                target.Currency = firstAllocation.InstrumentCCYCode;
                target.Allotment = fxfwd.GetAllotment();
                target.Market = fxfwd.GetMarketCode();
                res.Target = target;
            }
            else if (forexSpot != null)
            {
                res.SettlementDate = firstAllocation.SettlementDate >= DateTime.Today ? firstAllocation.SettlementDate : 
                    MEDIO.CORE.Tools.CSxUtils.GetDateFromSophisTime(forexSpot.GetSettlementDate(MEDIO.CORE.Tools.CSxUtils.ToSophisDate(DateTime.Today)));

                var target = new ForexTarget();
                target.Expiry = res.SettlementDate;
                target.SecurityType = ESecurityType.Forex;
                target.SecurityID = displayInstrCode;
                target.Currency = firstAllocation.InstrumentCCYCode;
                target.Allotment = forexSpot.GetAllotment();
                target.Market = forexSpot.GetMarketCode();
                res.Target = target;
            }
            var groupedAllocations = allocatedExecutions.GroupBy(x => new { x.FolioPath, x.SettlementDate }).Select(p=>p.ToList());

            res.QuantityData.AllocatedQty = 0.0;
            foreach (var group in groupedAllocations)
            {
                var alloc = new AllocationRule();
                foreach (var allocation in group)
                {
                    alloc.Quantity += allocation.FundingAmountCCY2;
                    res.QuantityData.OrderedQty += allocation.FundingAmountCCY2;
                    //res.QuantityData.AllocatedQty += allocation.FundingAmountCCY2;
                    alloc.PortfolioID = allocation._executionAllocation.FolioID;
                    alloc.PrimeBrokerID = _singleOrder.GetBroker();
                    alloc.EntityID = allocation._executionAllocation.Entity;
                }
                res.AllocationRulesSet.Allocations.Add(alloc);
            }
            if (res.QuantityData.OrderedQty < 0)
            {
                res.QuantityData.OrderedQty = Math.Abs(res.QuantityData.OrderedQty);
                res.Side = ESide.Buy;
            }

            SetDefaultParameters(res);
            return res;
        }

        public static IOrder CreateOneFwdOrder(List<CSxCustomAllocation> allocatedExecutions)
        {
            SingleOrder res = new SingleOrder();
            if (allocatedExecutions.IsNullOrEmpty()) return null;
            var _singleOrder = allocatedExecutions[0].SingleOrder;
            var firstAllocation = allocatedExecutions[0];
            res.AllocationRulesSet = new AllocationRulesSet();
            res.AllocationRulesSet.Allocations = new List<AllocationRule>();
            res.AllocationRulesSet.QuantityType = _singleOrder.AllocationRulesSet.QuantityType;
            res.AllocationRulesSet.RulesSetID = _singleOrder.AllocationRulesSet.RulesSetID;
            res.SettlementDate = firstAllocation.SettlementDate;
            res.SettlementType = firstAllocation.SettlementType;
            res.EffectiveTime = DateTime.Now;
            res.OriginationStrategy = HEDGING_FUNDING_ORDERS.Instance.OriginationStrat;
            res.Properties = new List<OrderProperty>();
            foreach (var p in _singleOrder.Properties)
            {
                var prop = p.Clone();
                res.Properties.Add(prop);
            }
            res.SetCounterParty(_singleOrder.GetCounterParty());
            res.ExternalSystem = _singleOrder.ExternalSystem;
            res.BusinessEventId = _singleOrder.BusinessEventId;
            res.TimeInForce = _singleOrder.TimeInForce;
            res.TimeInForce.Type = ETimeInForce.Day;
            res.TradingSessions = _singleOrder.TradingSessions;
            res.BookingType = EBookingType.Regular;
            res.CreationInfo = new CreationInfo();
            res.CreationInfo.TimeStamp = DateTime.Now;
            // Set current user
            CSMUserRights user = new CSMUserRights();
            res.CreationInfo.UserID = user.GetIdent();
            res.CreationInfo.ProfileID = user.GetAllProfiles().Cast<int>().ToList().FirstOrDefault();
            res.CreationInfo.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            res.CreationInfo.WorkStation = Environment.MachineName;
            res.Assignation = new Assignation();
            res.Assignation.TimeStamp = DateTime.Now;
            res.Assignation.UserID = user.GetIdent();
            res.Assignation.ProfileID = res.CreationInfo.ProfileID;
            res.Assignation.WindowsIdentity = res.CreationInfo.WindowsIdentity;
            res.Assignation.WorkStation = res.CreationInfo.WorkStation;
            res.OrderType = new OrderType();
            res.OrderType = _singleOrder.OrderType;
            res.OwnerInfo = new OwnerInfo();
            res.OwnerInfo = _singleOrder.OwnerInfo;
            res.Side = ESide.Buy;
            res.BestExecutionRules = new List<BestExecutionRule>();
            foreach (BestExecutionRule exec in res.BestExecutionRules)
            {
                var item = (BestExecutionRule)exec.Clone();
                res.BestExecutionRules.Add(item);
            }
            res.ThirdParties.Clear();
            foreach (var thirdpt in _singleOrder.ThirdParties)
            {
                res.ThirdParties.Add(thirdpt);
            }
            res.QuantityData = new Quantity();
            res.QuantityData.ExecutedAmount = 0;
            res.QuantityData.ExecutedQty = 0;
            res.QuantityData.ExecutionsCount = 0;
            res.QuantityData.IsTotallyExecuted = false;
            //TODO test and cleanup
            //int displayInstrCode = CSAMInstrumentDotNetTools.GetSettlementReportingInstrumentId(firstAllocation.FolioCCYCode,
            //    firstAllocation.InstrumentCCYCode, firstAllocation.ForwardDate,
            //        EForexNDFCreationBehaviour.CreateAsForward, true);
            int displayInstrCode = 
                CSAMInstrumentDotNetTools.GetForexInstrument(firstAllocation.FolioCCYCode, firstAllocation.InstrumentCCYCode, 
                    sophis.amCommon.DateUtils.ToInt(firstAllocation.ForwardDate));
            CSMForexFuture fxfwd = CSMInstrument.GetInstance(displayInstrCode);
            if (fxfwd != null)
            {
                res.Target = new ForexTarget();
                res.Target.SecurityType = ESecurityType.ForexForward;
                res.Target.SecurityID = displayInstrCode;
                res.Target.Currency = firstAllocation.InstrumentCCYCode;
                res.Target.Allotment = fxfwd.GetAllotment();
                res.Target.Market = fxfwd.GetMarketCode();
                CommentInfo comment = new CommentInfo();
                comment.Value = "Order is generated by FX hedging/funding tool (toolkit)";
            }
            else
            {
                CSMForexSpot forexSpot = CSMForexSpot.GetCSRForexSpot(firstAllocation.FolioCCYCode, firstAllocation.InstrumentCCYCode);
                res.Target = new ForexTarget();
                res.Target.SecurityType = ESecurityType.ForexForward;
                res.Target.SecurityID = forexSpot.GetCode();
                res.Target.Currency = firstAllocation.InstrumentCCYCode;
                res.Target.Allotment = forexSpot.GetAllotment();
                res.Target.Market = forexSpot.GetMarketCode();
                res.Comments = new List<CommentInfo>();
                CommentInfo comment = new CommentInfo();
                comment.Value = String.Format("[Toolkit] Failed to retrive the fx foward with forward date {0}! Please manually adjust on the order", firstAllocation.ForwardDate);
            }

            var groupedAllocations = allocatedExecutions.GroupBy(x => new { x.FolioPath});
            res.QuantityData.AllocatedQty = 0.0;
            foreach (var group in groupedAllocations)
            {
                var alloc = new AllocationRule();
                foreach (var allocation in group)
                {
                    //-= else the qty is wrong in portfolio as we are on the forward part
                    alloc.Quantity -= allocation.HedgingAmountCCY2;
                    res.QuantityData.OrderedQty += allocation.HedgingAmountCCY2;
                    //res.QuantityData.AllocatedQty += allocation.HedgingAmountCCY2;
                    alloc.PortfolioID = allocation._executionAllocation.FolioID;
                    alloc.PrimeBrokerID = _singleOrder.GetBroker();
                    alloc.EntityID = allocation._executionAllocation.Entity;
                }
                res.AllocationRulesSet.Allocations.Add(alloc);
            }
            if (res.QuantityData.OrderedQty < 0)
            {
                res.QuantityData.OrderedQty = Math.Abs(res.QuantityData.OrderedQty);
                res.Side = ESide.Sell;
            }

            SetDefaultParameters(res);
            return res;
        }

        public static bool SetDefaultParameters(SingleOrder order)
        {
            CSMInstrument instrument = CSMInstrument.GetInstance(order.Target.SecurityID);
            if (instrument != null)
            {
                int typeInstr = order.Target.SecurityType;
                List<int> entities = new List<int>();
                foreach (var alloc in order.AllocationRulesSet.Allocations)
                {
                    entities.Add(alloc.EntityID);
                }
                DefaultParametersSelectorConfigOutput output;
                return SetDefaultParameters(order, instrument, typeInstr, instrument.GetCurrency(), instrument.GetMarketCode(), instrument.GetAllotment(), entities, out output);
            }
            return false;
        }

        protected static bool SetDefaultParameters(SingleOrder order, CSMInstrument instrument, int omsSecurityType, int instrumentCurrency, int marketCode, int instrumentAllotment, List<int> entitiesID, out DefaultParametersSelectorConfigOutput output)
        {
            bool result = true;

            CSMLog.Write("CSxCustomAllocation", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Initialize default parameter input");

            DefaultParametersSelectorConfigInput input = new DefaultParametersSelectorConfigInput
                (marketCode, omsSecurityType, instrumentCurrency, instrumentAllotment, entitiesID, order.Kind);

            CSMLog.Write("CSxCustomAllocation", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Find default parameters to apply");
            //see http://marsbuild:8080/source/xref/develop-main/Value/source/SophisOrderEntry/orderKindHandlers/OEOrder.cs#3376
            output = ServicesProvider.Instance.GetService<IDefaultParametersSelectorFactory>().
                GetSelector(CSMAmUserUtils.GetCurrentUserId(), CSMAmUserUtils.GetUserGroupId(CSMAmUserUtils.GetCurrentUserId())).GetEntryBoxSelectorConfig(input);
            if (output != null)
            {
                //External System
                order.ExternalSystem = output.ExternalSystem;

                //BestExecutionRules
                order.BestExecutionRules.Clear();
                if (output.BestExecRule != null)
                {
                    CSMLog.Write("CSxCustomAllocation", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Apply best execution rules");
                    BestExecutionRule bestExecutionRule = new BestExecutionRule() { ID = output.BestExecRule.Value };
                    order.BestExecutionRules.Add(bestExecutionRule);
                }

                // BusinessEventId
                if (output.BusinessEvent != null)
                {
                    CSMLog.Write("CSxCustomAllocation", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Apply Business Event");
                    order.BusinessEventId = output.BusinessEvent;
                }

                // TradingAccount
                CSMLog.Write("CSxCustomAllocation", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Apply trading account");
                foreach (var alloc in order.AllocationRulesSet.Allocations)
                {
                    alloc.TradingAccount = output.TradingAccount;
                }

                // OrderType
                if (order.OrderType != null && output.OrderType != null && order.OrderType.Equals(output.OrderType) == false)
                {
                    CSMLog.Write("CSxCustomAllocation", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Apply order type");
                    order.OrderType.TypeId = output.OrderType.Value;
                }
            }
            return result;
        }
    }
}

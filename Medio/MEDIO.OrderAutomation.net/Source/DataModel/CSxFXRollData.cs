using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using MEDIO.CORE.Tools;
using MEDIO.MEDIO_CUSTOM_PARAM;
using MEDIO.OrderAutomation.net.Source.Criteria;
using MEDIO.OrderAutomation.net.Source.Data;
using MEDIO.OrderAutomation.net.Source.DataModel;
using MEDIO.OrderAutomation.NET.Source.GUI;
using sophis.instrument;
using sophis.market_data;
using sophis.misc;
using sophis.oms;
using sophis.oms.entry;
using Sophis.OMS.Util;
using sophis.orderadapter;
using sophis.portfolio;
using sophis.utils;
using sophis.value;
using SophisAMDotNetTools;
using CSxUtils = MEDIO.OrderAutomation.net.Source.Tools.CSxUtils;
using MessageBox = System.Windows.MessageBox;
using Sophis.OrderBookCompliance;
using Sophis.AaaS;
using sophisTools;
using ForexUtils;

namespace MEDIO.OrderAutomation.NET.Source.DataModel
{
    /// <summary>
    /// FX Roll GUI data model 
    /// Should be only initilized in an extraction:
    /// -> Currency
    ///    -> Instrument Code
    ///       -> Fund
    ///          -> Parent Order ID
    ///             -> Portfolio name
    /// </summary>
    public class CSxFXRollDataModel
    {
        #region GUI Fields

        public int ID { get; set; }
        public int ParentID { get; set; }
        public string Name { get; set; }
        public double ReceiveAmount { get; set; }
        public string CCY1 { get; set; }
        public int CCY1Code { get; set; }
        public int CCY2Code { get; set; }
        public double PayAmount { get; set; }
        public string CCY2 { get; set; }
        public double FXRate { get; set; }
        public double EstFXRate { get; set; }
        public double EstRecvAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime ForwardDate { get; set; }
        public int FolioID { get; set; }
        public bool WillRoll { get; set; }
        public bool IsMarketWay { get; set; }
        public List<int> Children { get; set; }
        public int Level { get; set; }
        public string FixedCurrency { get; set; }
        public string FolioName { get; set; }

        private bool _IsOrUnderFXForward = false;
        private CSMForexFuture _FXFwd = null;
        private CSMPortfolio _Folio;
        private CSMExtraction _Extraction;
        //TODO clean and remove
        //private List<CSMPosition> _RollablePositions;
        private bool isMAML;
        private bool isRealPosition;
        #endregion

        public CSxFXRollDataModel(CSMPortfolio folio, CSMExtraction extraction)
        {
            this._Folio = folio;
            this._Extraction = extraction;
            this.isMAML = false; // Display positions under MAML only
            InitialiseFields();
        }

        protected void InitialiseFields()
        {
            _IsOrUnderFXForward = IsOrUnderFXForward(_Folio, _Extraction, out _FXFwd);
            SetExpiryDate();
            SetMarketWay();
            SetPayAmountAmount();
            SetReceiveAmountAmount();
            SetCCY1();
            SetCCY2();
            SetEstFXRate();
            SetID();
            SetName();
            SetParentID();
            SetFXRate();
            SetFolioID();
//            SetMarketWay();
            SetChildren();
            SetLevel();
            SetDefaultForwardDate();
            SetEstAmount();
            Filter();
            SetRealPosition();
            SetFixedCurrency();
        }

        private void Filter()
        {
            if (_Folio.GetLevel() == 5)
            {
                int realFolioCode = CSMTransaction.solve_book_string(_Folio.GetName());
                CSMPortfolio realFolio = CSMPortfolio.GetCSRPortfolio(realFolioCode);
                if (realFolio != null)
                {
                    CMString name = new CMString();
                    realFolio.GetFullName(name);
                    isMAML = name.StringValue.ToUpper().Contains(CSxDBHelper.GetTargetTradingFolio());//"MAML"); //TOCHANGE
                }
                #region a more robust way to get real folio should be from a trade, however the performance will be terrible

                //CSMTransactionVector transactionVector = new CSMTransactionVector();
                //pos.GetTransactions(transactionVector);
                //if (transactionVector.Count > 0)
                //{
                //    CSMTransaction trade = (CSMTransaction)transactionVector[0];
                //    if (trade != null)
                //    {
                //        CSMPortfolio realFolio = CSMPortfolio.GetCSRPortfolio(trade.GetFolioCode());
                //        if (realFolio != null)
                //        {
                //            CMString name = "";
                //            realFolio.GetFullName(name);
                //            FolioName = name;
                //        }
                //    }
                //}

                #endregion
            }
        }

        private void SetDefaultForwardDate()
        {
            this.ForwardDate = CSxDataFacade.GetFirstOrDefaulgtForwardDate();
        }

        private void SetPayAmountAmount()
        {
            string columnName = MedioConstants.MEDIO_COLUMN_STANDARD_NOMINALCCY1;
            this.PayAmount = GetAggregatedValueUnder(_Folio, _Extraction, columnName);
        }

        private void SetReceiveAmountAmount()
        {
            string columnName = MedioConstants.MEDIO_COLUMN_STANDARD_NOMINALCCY2;
            this.ReceiveAmount = GetAggregatedValueUnder(_Folio, _Extraction, columnName);
        }

        private void SetCCY1()
        {
            CSMNonDeliverableForexForward fxNdFwd = _FXFwd;
            if (_FXFwd != null)
            {
                int expiryCcy = _FXFwd.GetExpiryCurrency();
                int ccy = _FXFwd.GetCurrencyCode();
                if (fxNdFwd != null)
                {
                    int deliverableCcy = fxNdFwd.GetSettlementCurrency();
                    int nonDeliverableCcy = deliverableCcy == ccy ? expiryCcy : ccy;
                    if (IsMarketWay)
                        this.CCY1Code = nonDeliverableCcy;
                    else
                        this.CCY1Code = deliverableCcy;
                    this.CCY1 = CSxUtils.GetCurrencyName(this.CCY1Code);

                }
                else
                {
                    this.CCY1 = CSxUtils.GetCurrencyName(expiryCcy);
                    this.CCY1Code = expiryCcy;
                }
            }
        }

        private void SetCCY2()
        {
            CSMNonDeliverableForexForward fxNdFwd = _FXFwd;
            if (_FXFwd != null)
            {
                int expiryCcy = _FXFwd.GetExpiryCurrency();
                int ccy = _FXFwd.GetCurrencyCode();
                if (fxNdFwd != null)
                {
                    int deliverableCcy = fxNdFwd.GetSettlementCurrency();
                    int nonDeliverableCcy = deliverableCcy == ccy ? expiryCcy : ccy;
                    if (IsMarketWay)
                        this.CCY2Code = deliverableCcy;
                    else
                        this.CCY2Code = nonDeliverableCcy;
                    this.CCY2 = CSxUtils.GetCurrencyName(this.CCY2Code);

                }
                else
                {
                    this.CCY2 = CSxUtils.GetCurrencyName(_FXFwd.GetCurrencyCode());
                    this.CCY2Code = _FXFwd.GetCurrencyCode();
                }
            }
        }

        public void SetFixedCurrency()
        {
            this.FixedCurrency = "CCY2";
        }
        public void SetEstAmount()
        {
            if(IsMarketWay)
                this.EstRecvAmount = -1 * this.PayAmount * this.EstFXRate;
            else
                this.EstRecvAmount = -1 * this.PayAmount / this.EstFXRate;
        }

        public bool IsUnderMAML()
        {
            return isMAML;
        }

        public bool IsRealPosition()
        {
            return isRealPosition;
        }

        public void SetUnderMAML(bool ismaml)
        {
            isMAML = ismaml;
        }

        /// <summary>
        /// Spot rate
        /// </summary>
        private void SetEstFXRate()
        {
            if (_FXFwd != null)
            {
                this.EstFXRate = CSMForexFuture.GetValue(CSMMarketData.GetCurrentMarketData(),
                    _FXFwd.GetExpiryCurrency(), _FXFwd.GetCurrencyCode(), _FXFwd.GetExpiry());
            }
        }

        public void SetEstFXRate(int expiry)
        {
            if (_FXFwd != null)
            {
                this.EstFXRate = CSMForexFuture.GetValue(CSMMarketData.GetCurrentMarketData(),
                    _FXFwd.GetExpiryCurrency(), _FXFwd.GetCurrencyCode(), expiry);

                if (!this.IsMarketWay)
                    this.EstFXRate = this.EstFXRate != 0.0 ? 1 / this.EstFXRate : this.EstFXRate;
            }
        }

        /// <summary>
        /// Transactions average spot rate
        /// </summary>
        /// <param name="folio"></param>
        private void SetFXRate()
        {
            try
            {
                if (_IsOrUnderFXForward)
                {
                    if (_Folio.GetLevel() == 5)
                    {
                        int posCount = _Folio.GetTreeViewPositionCount();
                        int tradeCount = 0;
                        for (int i = 0; i < posCount; i++)
                        {
                            var pos = _Folio.GetNthTreeViewPosition(i);
                            if (pos != null && !CSxFXRollManager.IsVirtualPosition(pos))
                            {
                                CSMTransactionVector transactionList = new CSMTransactionVector();
                                pos.GetTransactions(transactionList);
                                foreach (CSMTransaction trans in transactionList)
                                {
                                    this.FXRate += trans.GetSpot();
                                    tradeCount++;
                                }
                            }
                        }
                        this.FXRate = tradeCount != 0 ? this.FXRate / tradeCount : 0; // mean average
                    }
                }
            }
            catch (Exception e)
            {
                // Throw an exception when bad trades are retreived by pos.GetTransactions(transactionList)  
                MessageBox.Show("Error occured : " + e.ToString(), "Error", MessageBoxButton.OK);
            }
        }

        private void SetID()
        {
            if (_Folio.GetLevel() == 1 || _IsOrUnderFXForward)
                this.ID = _Folio.GetCode();
        }

        private void SetName()
        {
            this.Name = _Folio.GetName();
            // Parent Order ID
            if (_IsOrUnderFXForward && _Folio.GetLevel() == 4)
            {
                if (_Folio.GetName() != "N/A")
                {
                    int orderID = 0;
                    Int32.TryParse(_Folio.GetName(), out orderID);
                    if (orderID != 0)
                    {
                        int sicovam = CSxHedgingFundingCriterium.GetSicovam(orderID);
                        var inst = CSMInstrument.GetInstance(sicovam);
                        if (inst != null)
                        {
                            this.Name = inst.GetReference() + " - " + _Folio.GetName();
                        }
                    }
                }
            }
        }

        private void SetParentID()
        {
            if (_Folio.GetLevel() == 1)
            {
                this.ParentID = 0;
            }
            else if (_IsOrUnderFXForward)
            {
                this.ParentID = _Folio.GetParentCode();
            }
        }

        private void SetFolioID()
        {
            if (_Folio.GetLevel() == 3)
            {
                CSMAmPortfolio amfolio = CSMAmPortfolio.GetCSRPortfolio(_Folio.GetCode(), _Extraction);
                if (amfolio != null)
                {
                    var rootFund = amfolio.GetFundRootPortfolio();
                    if (rootFund != null)
                    {
                        this.FolioID = rootFund.GetCode();
                    }
                }
            }
            else if (_Folio.GetLevel() == 5)
            {
                int posCount = _Folio.GetTreeViewPositionCount();
                int tradeCount = 0;
                for (int i = 0; i < posCount; i++)
                {
                    var pos = _Folio.GetNthTreeViewPosition(i);
                    this.FolioID = pos.GetIdentifier();
                    break;
                }
            }
        }

/*
        private void SetMarketWay()
        {
            if (_FXFwd != null)
            {
                CSMForexSpot spot = CSMForexSpot.GetCSRForexSpot(_FXFwd.GetExpiryCurrency(), _FXFwd.GetCurrencyCode());
                if (spot != null)
                {
                    this.IsMarketWay = spot.GetMarketWay() != -1;
                    if (!this.IsMarketWay)
                        this.EstFXRate = this.EstFXRate != 0.0 ? 1 / this.EstFXRate : this.EstFXRate;
                }   
            }
        }
*/
        private void SetMarketWay()
        {
            CSMNonDeliverableForexForward fxNdFwd = _FXFwd;
            CSMForexSpot spot = null;
            if (_FXFwd != null)
            {
                int expiryCcy = _FXFwd.GetExpiryCurrency();
                int ccy = _FXFwd.GetCurrencyCode();
                if (fxNdFwd != null)
                {
                    int deliverableCcy = _FXFwd.GetSettlementCurrency();
                    int nonDeliverableCcy = deliverableCcy == expiryCcy ? ccy : expiryCcy;
                    spot = CSMForexSpot.GetCSRForexSpot(nonDeliverableCcy, deliverableCcy);
                }
                else
                {
                    spot = CSMForexSpot.GetCSRForexSpot(expiryCcy, ccy);
                }
                if (spot != null)
                {
                    this.IsMarketWay = spot.GetMarketWay() != -1;
                    if (!this.IsMarketWay)
                        this.EstFXRate = this.EstFXRate != 0.0 ? 1 / this.EstFXRate : this.EstFXRate;
                }
            }
        }

        private void SetChildren()
        {
            if(this.Children == null) this.Children = new List<int>();

            ArrayList childList = new ArrayList();
            _Folio.GetChildren(childList);
            foreach (CSMPortfolio child in childList)
            {
                if (!this.Children.Contains(child.GetCode()))
                    this.Children.Add(child.GetCode());
            }
        }

        private void SetLevel()
        {
            this.Level = _Folio.GetLevel();
        }

        private void SetExpiryDate()
        {
            if (_FXFwd != null)
            {
                this.ExpiryDate = CORE.Tools.CSxUtils.GetDateFromSophisTime(_FXFwd.GetExpiry()).Date;
            }
        }

        public void SetForwardDate(DateTime date)
        {
            if (_FXFwd != null)
            {
                this.ForwardDate = date;
            }
        }

        public DateTime GetExpiryDate()
        {
            return this.ExpiryDate;
        }

        public CSMForexFuture GetFXForward()
        {
            return _FXFwd;
        }

        public static bool IsOrUnderFXForward(CSMPortfolio folio, CSMExtraction extraction, out CSMForexFuture fxFwd)
        { 
            if (folio.GetLevel() == 1)
            {
                fxFwd = null;
                return false;
            }
            int nbPos = folio.GetFlatViewPositionCount();
            for (int i = 0; i < nbPos; i++ )
            {
                CSMPosition onePos = folio.GetNthFlatViewPosition(i);
                int sicovam = onePos.GetInstrumentCode();
                fxFwd = CSMInstrument.GetInstance(sicovam);
                if( fxFwd != null)
                {
                    return true;
                }
            }
            fxFwd = null;
            return false;
            
            //if (CSxUtils.IsVirtualPortfolioFXForward(folio, out fxFwd))
            //{
            //    return true;
            //}
            //else if (folio.GetLevel() == 1)
            //{
            //    return false;
            //}
            //else
            //{
            //    var parentFolio = CSMPortfolio.GetCSRPortfolio(folio.GetParentCode(), extraction);
            //    if(parentFolio == null)
            //        return false;
            //    else
            //        return IsOrUnderFXForward(parentFolio, extraction, out fxFwd);
            //}
        }

        private void SetRealPosition()
        {
            int posCount = _Folio.GetTreeViewPositionCount();
            for (int i = 0; i < posCount; i++)
            {
                var pos = _Folio.GetNthTreeViewPosition(i);
                if (pos.GetIdentifier() > 0)
                {
                    this.isRealPosition = true;
                    return;
                }
            }

            foreach (var childID in Children)
            {
                var child = CSMPortfolio.GetCSRPortfolio(childID, _Extraction);
                if (child != null)
                {
                    posCount = child.GetTreeViewPositionCount();
                    for (int i = 0; i < posCount; i++)
                    {
                        var pos = child.GetNthTreeViewPosition(i);
                        if (pos.GetIdentifier() > 0)
                        {
                            this.isRealPosition = true;
                            return;
                        }
                    }
                }
            }
        }


        public bool RollFXForward()
        {
            return false;
        }

        private double GetAggregatedValueUnder(CSMPortfolio folio, CSMExtraction extraction, string columnName)
        {
            if(folio.GetLevel() == 1) return 0;

            int posCount = folio.GetTreeViewPositionCount();
            if (posCount > 0)
            {
                double res = 0;
                for (int i = 0; i < posCount; i++)
                {
                    var pos = folio.GetNthTreeViewPosition(i);
                    if (pos.GetIdentifier() < 0) continue;
                    var columnValue = CSxColumnHelper.GetPositionColumn(pos, folio.GetCode(), extraction, columnName);
                    res += columnValue.doubleValue;
                }
                return res;
            }
            else
            {
                double res = 0;
                ArrayList childList = new ArrayList();
                folio.GetChildren(childList);
                foreach (CSMPortfolio child in childList)
                {
                    if (child.GetChildCount() == 0)
                    {
                        res += GetAggregatedValueUnder(child, extraction, columnName);
                    }
                }
                return res;
            }
        }

        public bool IsAtLowestLevel()
        {
            return _Folio.GetLevel() == 5;
        }

        public CSMPortfolio GetFolio()
        {
            return this._Folio;
        }

        public CSMExtraction GetExtraction()
        {
            return this._Extraction;
        }

    }

    public class CSxFXRollManager
    {
        
        private static CSxFXRollManager _Instance = null;

        public static CSxFXRollManager Instance
        {
            get
            {
                return _Instance ??
                       (_Instance = new CSxFXRollManager());
            }
        }

        public bool GenerateAndSendOrders(List<CSxFXRollDataModel> treeNodesToRoll, double closingRatio, double openingRatio)
        {
            using (var LOG = new CSMLog())
            {
                bool res = false;
                LOG.Begin("CSxFXRollManager", MethodBase.GetCurrentMethod().Name);
                var newOrders = new List<IOrder>();
                try
                {
                    if (treeNodesToRoll.IsNullOrEmpty()) return res;
                    if (CSxDataFacade.IOrderManager == null)
                    {
                        LOG.Write(CSMLog.eMVerbosity.M_error, "Failed to get order manager service");
                        MessageBox.Show("Failed to get order manager service");
                    }
                    var ordersToSend = CreateOrders(treeNodesToRoll, closingRatio, openingRatio);

                    newOrders = CSxDataFacade.IOrderManager.CreateOrders(ordersToSend, false, ordersToSend.FirstOrDefault().CreationInfo).ToList();
                    newOrders.ForEach(x => OrderCreationValidatorManager.Instance.Validate(x, true));
                    newOrders = CSxDataFacade.IOrderManager.CreateOrders(newOrders, true, ordersToSend.FirstOrDefault().CreationInfo).ToList();
                    res = true;
                }
                catch (Exception ex)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_error, ex.Message);
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (newOrders.Count != 0)
                    {
                        string msg = String.Format("{0} order(s) have been created! Press OK to preview.",
                            newOrders.Count);
                        DialogResult result = System.Windows.Forms.MessageBox.Show(msg, "Orders creation", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Information);
                        if (result == DialogResult.OK)
                        {
                            var report = CSxDataFacade.GetOrderReports(newOrders);
                            CSxOrderSendingReport.Display(report);
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("No order has been created!", "Orders creation", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }

                LOG.End();
                return res;
            }
        }

        private List<IOrder> CreateOrders(List<CSxFXRollDataModel> treeNodesToRoll, double closingRatio, double openingRatio)
        {
            List<IOrder> res = new List<IOrder>();
            try
            {
                // We only care about the lowest level 
                var lowestLevel = treeNodesToRoll.Where(x => x.IsAtLowestLevel());
                IEnumerable<IGrouping<int, CSxFXRollDataModel>> toRollPerParentOrder = lowestLevel.GroupBy(x => x.ParentID);
                // to do check if rolling on CCY1 or CCY2....
                bool rollOnCCY1 = false;
                //MessageBoxResult mr = MessageBox.Show("Rolling on CCY1 ?","Currency to Roll", MessageBoxButton.YesNo);
                //switch (mr)
                //{
                //    case MessageBoxResult.Yes:
                //        rollOnCCY1 = true;
                //        break;
                //    case MessageBoxResult.No:
                //        break;
                //}

                // One order per parent order 
                foreach (var oneParentOrder in toRollPerParentOrder)
                {
                    var ListOfAllocations = new List<CSxFXRollAllocation>();
                    // Aggregate under  
                    foreach (var alloc in oneParentOrder)
                    {
                        CSxFXRollAllocation toAdd = new CSxFXRollAllocation();
                        int posCount = alloc.GetFolio().GetTreeViewPositionCount();
                        if (posCount > 0)
                        {
                            for (int i = 0; i < posCount; i++)
                            {
                                var pos = alloc.GetFolio().GetNthTreeViewPosition(i);
                                if (pos != null)
                                {
                                    if (IsVirtualPosition(pos)) continue;

                                    rollOnCCY1 = alloc.FixedCurrency == "CCY1";
                                    toAdd.RollOnCCY1 = rollOnCCY1;
                                    toAdd.Sicovam = pos.GetInstrumentCode();
                                    SSMCellValue columnValue = new SSMCellValue();
                                    if (rollOnCCY1 == true)
                                        //var columnValue = CSxColumnHelper.GetPositionColumn(pos, alloc.GetFolio().GetCode(), alloc.GetExtraction(), MedioConstants.MEDIO_COLUMN_STANDARD_NOMINALCCY1);
                                        columnValue = CSxColumnHelper.GetPositionColumn(pos, alloc.GetFolio().GetCode(), alloc.GetExtraction(), MedioConstants.MEDIO_COLUMN_STANDARD_NOMINALCCY1);
                                    // Potential fix for FX Roll wrong amount and CCY , expected in case was ccy2...
                                    else
                                        //var columnValue = CSxColumnHelper.GetPositionColumn(pos, alloc.GetFolio().GetCode(), alloc.GetExtraction(), MedioConstants.MEDIO_COLUMN_STANDARD_NOMINALCCY2);
                                        columnValue = CSxColumnHelper.GetPositionColumn(pos, alloc.GetFolio().GetCode(), alloc.GetExtraction(), MedioConstants.MEDIO_COLUMN_STANDARD_NOMINALCCY2);

                                    toAdd.CCY1Amount += columnValue.doubleValue; // we keep it as is for now....knowing this should be CCY2 Amount...


                                    toAdd.OriginalFXFoward = alloc.GetFXForward();
                                    toAdd.ExpiryDate = alloc.ExpiryDate;
                                    CSMTransactionVector transactionList = new CSMTransactionVector();
                                    pos.GetTransactions(transactionList);
                                    foreach (CSMTransaction trans in transactionList)
                                    {
                                        toAdd.CounterParty = trans.GetCounterparty();
                                        toAdd.Broker = trans.GetBroker();
                                        toAdd.OriginalParentOrderID = CSxHedgingFundingCriterium.GetTradeParentOrderID(trans.getInternalCode());
                                        break;
                                    }
                                    toAdd.Folio = CSMAmPortfolioTools.GetRealFolioCodeFromPosition(pos);
                                    if (rollOnCCY1 == true)
                                    {
                                        toAdd.CCY1 = alloc.CCY1Code;
                                        toAdd.CCY2 = alloc.CCY2Code;
                                    }
                                    else
                                    {
                                        // reversed...
                                        toAdd.CCY1 = alloc.CCY2Code;
                                        toAdd.CCY2 = alloc.CCY1Code;
                                    }

                                    toAdd.ClosingRatio = closingRatio;
                                    toAdd.OpeningRatio = openingRatio;
                                    toAdd.Entity = pos.GetEntity();
                                    toAdd.ForwardDate = alloc.ForwardDate;
                                }
                            }
                            ListOfAllocations.Add(toAdd);
                        }
                    }

                    //TODO : Check in Application if the created orders takes the expected ccies...

                    if (ListOfAllocations.Any(x => x.OpeningRatio > 0))
                        res.Add(CreateOneOpeningOrder(ListOfAllocations));

                    if (ListOfAllocations.Any(x => x.ClosingRatio > 0))
                        res.Add(CreateOneClosingOrder(ListOfAllocations));
                }
            }
            catch (Exception e)
            {
                // Throw an exception when bad trades in db and retreived by pos.GetTransactions(transactionList)  
                MessageBox.Show("Error occured : " + e.ToString(), "Error", MessageBoxButton.OK);
            }

            return res;
        }

        public static bool IsVirtualPosition(CSMPosition pos)
        {
            if (pos != null)
            {
                return pos.GetIdentifier() < 0;
            }
            return false;
        }

        private IOrder CreateOneOpeningOrder(List<CSxFXRollAllocation> allocatedExecutions)
        {
            IOrder order = null;
            
            SingleOrder res = new SingleOrder();
            if (allocatedExecutions.IsNullOrEmpty()) return null;
            var oneAlloc = allocatedExecutions[0];

            res.AllocationRulesSet = new AllocationRulesSet();
            res.AllocationRulesSet.Allocations = new List<AllocationRule>();
            res.AllocationRulesSet.QuantityType = EQuantityType.Amount;

            res.SettlementDate = oneAlloc.ForwardDate;
            res.SettlementType = ESettlementType.Regular;
            res.EffectiveTime = DateTime.Now;
            res.OriginationStrategy = sophis.oms.OriginationStrategy.ToolkitBase;
            res.Properties = new List<OrderProperty>();

            CSxDataFacade.SetParentOrderIDProperty(oneAlloc.OriginalParentOrderID, res);
            res.SetCounterParty(oneAlloc.CounterParty);
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
            res.OwnerInfo = new OwnerInfo();
            res.Side = ESide.Sell;
            res.BestExecutionRules = new List<BestExecutionRule>();
            foreach (BestExecutionRule exec in res.BestExecutionRules)
            {
                var item = (BestExecutionRule)exec.Clone();
                res.BestExecutionRules.Add(item);
            }
            res.ThirdParties.Clear();
            ThirdParty thirdParty = new ThirdParty();
            thirdParty.ThirdPartyID = oneAlloc.CounterParty;
            res.ThirdParties.Add(thirdParty);
            res.BusinessEventId = 1;
            res.ExternalSystem = "Manual";

            TimeInForce timeInForce = new TimeInForce();
            timeInForce.Type = ETimeInForce.GoodTillCancel;
            timeInForce.ExpiryDate = DateTime.Today;

            res.TimeInForce = timeInForce;
            res.QuantityData = new Quantity();
            res.QuantityData.ExecutedAmount = 0;
            res.QuantityData.ExecutedQty = 0;
            res.QuantityData.ExecutionsCount = 0;
            res.QuantityData.IsTotallyExecuted = false;

            //TODO test cleanup
            //int displayInstrCode = CSAMInstrumentDotNetTools.GetSettlementReportingInstrumentId(oneAlloc.CCY1,
            //    oneAlloc.CCY2, res.SettlementDate,
            //    EForexNDFCreationBehaviour.CreateAsForward, true);
           
            int positionInstrCode = oneAlloc.Sicovam;
            int ccyLeft = oneAlloc.CCY1;
            int ccyRight = oneAlloc.CCY2;
            if (oneAlloc.RollOnCCY1 == false)
            {
                ccyLeft = oneAlloc.CCY2;
                ccyRight = oneAlloc.CCY1;
            }

            // Reversing
            // int displayInstrCode = CSAMInstrumentDotNetTools.GetSettlementReportingInstrumentId(oneAlloc.CCY2,
            //   oneAlloc.CCY1, oneAlloc.ForwardDate,
            //     EForexNDFCreationBehaviour.CreateAsForward, true);

            CSMNonDeliverableForexForward ndfOrg = CSMNonDeliverableForexForward.GetInstance(positionInstrCode);      
            CSMForexFuture fxfwd = CSMInstrument.GetInstance(positionInstrCode);

            if (ndfOrg!=null)
            {
                int ndfInstr=CLIForexUtils.GetNDFInstrCode(ccyLeft, ccyRight, sophis.amCommon.DateUtils.ToInt(res.SettlementDate));

                timeInForce.Type = ETimeInForce.Day;
                timeInForce.ExpiryDate = DateTime.Today;

                res.TimeInForce = timeInForce;
                ForexTarget fxTarget = new ForexTarget();
                fxTarget.NonDeliverableForward = true;
                fxTarget.SecurityType = ESecurityType.ForexNDF;
                fxTarget.SecurityID = ndfInstr;
                fxTarget.Currency = oneAlloc.CCY1;
                fxTarget.Allotment = ndfOrg.GetAllotment();
                fxTarget.Market = ndfOrg.GetMarketCode();

                res.Target = fxTarget;

            }
            else if (fxfwd != null)
            {
                int fwdInstr = CLIForexUtils.GetFWDInstrCode(ccyLeft, ccyRight, sophis.amCommon.DateUtils.ToInt(res.SettlementDate));
                res.Target = new ForexTarget();
                res.Target.SecurityType = ESecurityType.ForexForward;
                res.Target.SecurityID = fwdInstr;
                res.Target.Currency = oneAlloc.CCY1;
                res.Target.Allotment = fxfwd.GetAllotment();
                res.Target.Market = fxfwd.GetMarketCode();          
            }
            else
            {
                CSMForexSpot forexSpot = CSMForexSpot.GetCSRForexSpot(ccyLeft, ccyRight);
                res.Target = new ForexTarget();
                res.Target.SecurityType = ESecurityType.Forex;
                res.Target.SecurityID = forexSpot.GetCode();
                res.Target.Currency = oneAlloc.CCY1;
                res.Target.Allotment = forexSpot.GetAllotment();
                res.Target.Market = forexSpot.GetMarketCode();        
            }

            res.QuantityData.AllocatedQty = 0.0;
            foreach (var allocation in allocatedExecutions)
            {
                var allocRule = new AllocationRule();
                allocRule.Quantity = allocation.CCY1Amount * allocation.OpeningRatio;
                res.QuantityData.OrderedQty += allocRule.Quantity;
                //res.QuantityData.AllocatedQty = res.QuantityData.OrderedQty;
                allocRule.PortfolioID = allocation.Folio;
                allocRule.PrimeBrokerID = allocation.Broker;
                allocRule.EntityID = allocation.Entity;
                res.AllocationRulesSet.Allocations.Add(allocRule);
            }

            if ((res.QuantityData.OrderedQty < 0) == oneAlloc.RollOnCCY1)
            {
               // res.QuantityData.OrderedQty = -res.QuantityData.OrderedQty;
                res.Side = ESide.Sell;
            }
            else
                res.Side = ESide.Buy;

            res.QuantityData.OrderedQty = Math.Abs(res.QuantityData.OrderedQty);
            SetDefaultParameters(res);
            return res;
        }

        private IOrder CreateOneClosingOrder(List<CSxFXRollAllocation> allocatedExecutions)
        {
            SingleOrder res = new SingleOrder();
            if (allocatedExecutions.IsNullOrEmpty()) return null;
            var oneAlloc = allocatedExecutions[0];

            res.AllocationRulesSet = new AllocationRulesSet();
            res.AllocationRulesSet.Allocations = new List<AllocationRule>();
            res.AllocationRulesSet.QuantityType = EQuantityType.Amount;

            res.SettlementDate = oneAlloc.ExpiryDate;
            res.SettlementType = ESettlementType.Regular;
            res.EffectiveTime = DateTime.Now;
            res.OriginationStrategy = sophis.oms.OriginationStrategy.ToolkitBase;
            res.Properties = new List<OrderProperty>();

            CSxDataFacade.SetParentOrderIDProperty(oneAlloc.OriginalParentOrderID, res);
            res.SetCounterParty(oneAlloc.CounterParty);
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
            res.OwnerInfo = new OwnerInfo();

            res.BestExecutionRules = new List<BestExecutionRule>();
            foreach (BestExecutionRule exec in res.BestExecutionRules)
            {
                var item = (BestExecutionRule)exec.Clone();
                res.BestExecutionRules.Add(item);
            }
            res.ThirdParties.Clear();
            ThirdParty thirdParty = new ThirdParty();
            thirdParty.ThirdPartyID = oneAlloc.CounterParty;
            res.ThirdParties.Add(thirdParty);
            res.BusinessEventId = 1;
            res.ExternalSystem = "Manual";

            TimeInForce timeInForce = new TimeInForce();
            timeInForce.Type = ETimeInForce.GoodTillCancel;
            timeInForce.ExpiryDate = DateTime.Today;
            res.TimeInForce = timeInForce;
            res.QuantityData = new Quantity();
            res.QuantityData.ExecutedAmount = 0;
            res.QuantityData.ExecutedQty = 0;
            res.QuantityData.ExecutionsCount = 0;
            res.QuantityData.IsTotallyExecuted = false;

            int displayInstrCode = oneAlloc.Sicovam;
            CSMForexFuture fxfwd = CSMInstrument.GetInstance(displayInstrCode);
            CSMNonDeliverableForexForward ndf = CSMNonDeliverableForexForward.GetInstance(displayInstrCode);

            int ccyLeft = oneAlloc.CCY1;
            int ccyRight = oneAlloc.CCY2;
            if (oneAlloc.RollOnCCY1 == false)
            {
                ccyLeft = oneAlloc.CCY2;
                ccyRight = oneAlloc.CCY1;
            }

            if (ndf != null)
            {
                timeInForce.Type = ETimeInForce.Day;
                timeInForce.ExpiryDate = DateTime.Today;

                res.TimeInForce = timeInForce;

                res.Target = new ForexTarget();
                res.Target.SecurityType = ESecurityType.ForexNDF;
                res.Target.SecurityID = displayInstrCode;
                res.Target.Currency = oneAlloc.CCY1;
                res.Target.Allotment = ndf.GetAllotment();
                res.Target.Market = ndf.GetMarketCode();
            }
            else if (fxfwd != null)
            {
                //instrument on existing position is a fwd but we need to decide if we close with spot or fwd
                int instrIdent = CSAMInstrumentDotNetTools.GetForexInstrument(ccyLeft, ccyRight, sophis.amCommon.DateUtils.ToInt(oneAlloc.ExpiryDate));
                CSMForexFuture forexInstr = CSMInstrument.GetInstance(instrIdent);

                if (forexInstr == null)
                {
                    // original
                    CSMForexSpot forexSpot = CSMForexSpot.GetCSRForexSpot(ccyLeft, ccyRight);
                    res.Target = new ForexTarget();
                    res.Target.SecurityType = ESecurityType.Forex;
                    res.Target.SecurityID = forexSpot.GetCode();
                    res.Target.Currency = oneAlloc.CCY1;
                    res.Target.Allotment = forexSpot.GetAllotment();
                    res.Target.Market = forexSpot.GetMarketCode();
                }
                else
                {
                    res.Target = new ForexTarget();
                    res.Target.SecurityType = ESecurityType.ForexForward;
                    res.Target.SecurityID = fxfwd.GetCode();
                    res.Target.Currency = oneAlloc.CCY1;
                    res.Target.Allotment = fxfwd.GetAllotment();
                    res.Target.Market = fxfwd.GetMarketCode();
                }
            }
          
            res.QuantityData.AllocatedQty = 0.0;
            foreach (var allocation in allocatedExecutions)
            {
                var allocRule = new AllocationRule();
                //JIRA . Else qty of orders in the GUI in not correct. 
                //The order looks OK but the sign in ORDER_ALLOCATION.QUANTITY is NOK.
                allocRule.Quantity = allocation.CCY1Amount * allocation.ClosingRatio;
                
                //allocRule.Quantity = -allocation.CCY1Amount * allocation.ClosingRatio;
                res.QuantityData.OrderedQty += allocation.CCY1Amount * allocation.ClosingRatio;
                //res.QuantityData.AllocatedQty = res.QuantityData.OrderedQty;
                allocRule.PortfolioID = allocation.Folio;
                allocRule.PrimeBrokerID = allocation.Broker;
                allocRule.EntityID = allocation.Entity;
                res.AllocationRulesSet.Allocations.Add(allocRule);
            }
            if ((res.QuantityData.OrderedQty < 0) == oneAlloc.RollOnCCY1)
            {
                res.Side = ESide.Buy;
               // res.QuantityData.OrderedQty = -res.QuantityData.OrderedQty;
            }
            else
                res.Side = ESide.Sell;

            res.QuantityData.OrderedQty = Math.Abs(res.QuantityData.OrderedQty);
            SetDefaultParameters(res);

            return res;
        }

        public bool SetDefaultParameters(SingleOrder order)
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
                order.OriginationStrategy = FXROLL_ORDERS.Instance.OriginationStrat;
                DefaultParametersSelectorConfigOutput output;
                return SetDefaultParameters(order, instrument, typeInstr, instrument.GetCurrency(), instrument.GetMarketCode(), instrument.GetAllotment(), entities, out output);
            }
            return false;
        }

        protected virtual bool SetDefaultParameters(SingleOrder order, CSMInstrument instrument, int omsSecurityType, int instrumentCurrency, int marketCode, int instrumentAllotment, List<int> entitiesID, out DefaultParametersSelectorConfigOutput output)
        {
            bool result = true;

            CSMLog.Write("CSxFXRollManager", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Initialize default parameter input");

            DefaultParametersSelectorConfigInput input = new DefaultParametersSelectorConfigInput
                (marketCode, omsSecurityType, instrumentCurrency, instrumentAllotment, entitiesID, order.Kind);

            CSMLog.Write("CSxFXRollManager", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Find default parameters to apply");
            //output = DefaultParametersSelectorFactory.GetInstance().GetSelector().GetEntryBoxSelectorConfig(input);
            //see http://marsbuild:8080/source/xref/develop-main/Value/source/SophisOrderEntry/orderKindHandlers/OEOrder.cs#3376
            output = ServicesProvider.Instance.GetService<IDefaultParametersSelectorFactory>().GetSelector(CSMAmUserUtils.GetCurrentUserId(), CSMAmUserUtils.GetUserGroupId(CSMAmUserUtils.GetCurrentUserId())).GetEntryBoxSelectorConfig(input);
            if (output != null)
            {
                // External System
                order.ExternalSystem = output.ExternalSystem;

                // BestExecutionRules
                order.BestExecutionRules.Clear();
                if (output.BestExecRule != null)
                {
                    CSMLog.Write("CSxFXRollManager", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Apply best execution rules");
                    BestExecutionRule bestExecutionRule = new BestExecutionRule() { ID = output.BestExecRule.Value };
                    order.BestExecutionRules.Add(bestExecutionRule);
                }

                // BusinessEventId
                if (output.BusinessEvent != null)
                {
                    CSMLog.Write("CSxFXRollManager", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Apply Business Event");
                    order.BusinessEventId = output.BusinessEvent;
                }

                // TradingAccount
                CSMLog.Write("CSxFXRollManager", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Apply trading account");
                foreach (var alloc in order.AllocationRulesSet.Allocations)
                {
                    alloc.TradingAccount = output.TradingAccount;
                    if (output.PrimeBroker != null)
                    {
                        alloc.PrimeBrokerID = output.PrimeBroker.Value;
                    }
                }
               
                // OrderType
                if (order.OrderType != null && output.OrderType != null && order.OrderType.Equals(output.OrderType) == false)
                {
                    CSMLog.Write("CSxFXRollManager", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Apply order type");
                    order.OrderType.TypeId = output.OrderType.Value;
                }

            }
            return result;
        }
        
    }


    public class CSxFXRollAllocation
    {
        public double CCY1Amount; // we only care about the fixed amount of the 1st leg
        public int Folio;
        public int Broker;
        public int Entity;
        public int CounterParty;
        public DateTime ForwardDate;
        public DateTime ExpiryDate; 
        public int OriginalParentOrderID;
        public int CCY1;
        public int CCY2;
        public CSMForexFuture OriginalFXFoward;
        public double ClosingRatio;
        public double OpeningRatio;
        public bool RollOnCCY1;
        public int Sicovam;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using sophis.instrument;
using sophis.portfolio;
using sophis.strategy;
using sophis.utils;
using sophis.value;
using sophis.static_data;
using sophisTools;
using sophis.xaml;
using sophis.market_data;
using DevExpress.XtraEditors.Repository;
using System.Reflection;
using System.Threading;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public partial class CSxSummaryScreen : sophis.guicommon.basicDialogs.DefaultEmbeddableControl
    {
        private static bool _OnlyExternalFunds = false;
        private static string _ClassName = "CSxSummaryScreen";
        private ArrayList _folios;

        public CSxSummaryScreen(ArrayList fundsInScope=null)
        {
            _OnlyExternalFunds = false;
            InitializeComponent();
            CSxSummaryController.Init();
            if (fundsInScope != null)
            {
                _folios = fundsInScope;
                LoadButton.PerformClick();           
            }
            
        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeList1.ClearNodes();
            if (radioGroup1.SelectedIndex == 0 || radioGroup1.SelectedIndex==2) //Delegates/Sleeves
            {
                _OnlyExternalFunds = false;
                treeList1.Columns["Cash Transfer"].Visible = true;
                treeList1.Columns["Transfer CCY"].Visible = true;
                treeList1.Columns["Trade Date"].Visible = true;
                treeList1.Columns["Settlement Date"].Visible = true;
                treeList1.Columns["New Actual Position(EUR)"].Visible = true;
                treeList1.Columns["New Actual Weight%"].Visible = true;
                treeList1.Columns["New Deviation%"].Visible = true;
                btnSendForApproval.Visible = true;
                btnReviewApprovals.Visible = true;
                btnAccounts.Visible = true;
                btnSendSR.Visible = true;
            }
            else if (radioGroup1.SelectedIndex == 1)//ETFs/TargetFund
            {
                _OnlyExternalFunds = true;
                treeList1.Columns["Cash Transfer"].Visible = false;
                treeList1.Columns["Transfer CCY"].Visible = false;
                treeList1.Columns["Trade Date"].Visible = false;
                treeList1.Columns["Settlement Date"].Visible = false;
                treeList1.Columns["New Actual Position(EUR)"].Visible = false;
                treeList1.Columns["New Actual Weight%"].Visible = false;
                treeList1.Columns["New Deviation%"].Visible = false;
                
                btnSendForApproval.Visible = false;
                btnReviewApprovals.Visible = false;
                btnAccounts.Visible = false;
                btnSendSR.Visible = false;
            }
        }

        private void btnReviewApprovals_Click(object sender, EventArgs e)
        {
            CSMBoKernelBlotterOpener.OpenTradeBlotter(CSxSummaryController._BlotterPendingApproval);
        }

        private ArrayList getLoadedFolios()
        {
            ArrayList list = new ArrayList();
            ArrayList results = new ArrayList();
            CSMPortfolio root = CSMPortfolio.GetRootPortfolio();
            if (root != null)
            {
                int counter = root.GetChildCount();
                root.GetChildren(list);
                IEnumerator myEnum = list.GetEnumerator();
                while (myEnum.MoveNext())
                {
                    CSMPortfolio itemFolio = (CSMPortfolio)myEnum.Current;
                    if (itemFolio != null)
                    {
                        int code = itemFolio.GetCode();
                        using (CSMAmPortfolio fundFolio = CSMAmPortfolio.GetCSRPortfolio(code))
                        {
                            if (fundFolio != null)
                            {
                                using (CSMAmPortfolio fundRoot = fundFolio.GetFundRootPortfolio())
                                {
                                    if (fundRoot != null && fundRoot.GetCode() == code)
                                    {
                                        if (fundRoot.IsLoaded())
                                        {
                                            results.Add(code);
                                            string mainStrategy = CSxSummaryController.GetMIFLMainStrat(code);
                                            if (CSxSummaryController._MIFLStrategiesMap.ContainsKey(code) == false && mainStrategy != "")
                                            {
                                                CSxSummaryController._MIFLStrategiesMap.Add(code, mainStrategy);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }

        private void LoadSummary(bool onlyExtFunds,ArrayList fundsInScope=null)
        {
            try
            {
                treeList1.OptionsBehavior.ReadOnly = false;
                treeList1.ClearNodes();
                loadImages();
                ArrayList _LoadedFolios = new ArrayList();
                if (fundsInScope != null)
                {
                    _LoadedFolios = fundsInScope;
                }
                else
                {
                    _LoadedFolios = getLoadedFolios();
                }
                ArrayList criteria = new ArrayList();
                criteria.Add(CSMCriterium.GetCriteriumType("Fund"));
                criteria.Add(CSMCriterium.GetCriteriumType("Strategy"));
                using (var tktExtraction = new CSMExtractionCriteria(criteria, _LoadedFolios, true))
                {
                    tktExtraction.SetHierarchicCriteria(true);
                    tktExtraction.KeepPositionId(true);
                    tktExtraction.SetFilteredDeals(CSMExtraction.eMFilteredDeals.M_eNoAccess);
                    tktExtraction.SetCashPerCurrencyParameter(eMExtractionCashPerCurrencyType.M_ecpcUserPref);

                    if (_LoadedFolios.Count != 0)
                    {
                        tktExtraction.Create();                
                    }
                    else
                    {
                        return;
                    }
                    
                    int nbOfFolios = tktExtraction.GetFolioCount();
                    int folId = 0;

                    CSMPortfolioColumn colWeightInFundPercent = CSMPortfolioColumn.GetCSRPortfolioColumn(CSxSummaryController._colWeightInFundPercentName);
                    CSMPortfolioColumn colStratTargetWeightPercent = CSMPortfolioColumn.GetCSRPortfolioColumn(CSxSummaryController._colStratTargetWeightPercentName);
                    CSMPortfolioColumn colMedioNav = CSMPortfolioColumn.GetCSRPortfolioColumn(CSxSummaryController._colMedioNavName);
                    CSMPortfolioColumn colAvailableCashEUR = CSMPortfolioColumn.GetCSRPortfolioColumn(CSxSummaryController._colAvailableCashEURName);
                    
                    SSMCellValue val = new SSMCellValue();
                    SSMCellStyle cellStyle = new SSMCellStyle();
                    this.treeList1.BeginUnboundLoad();

                    int currDate = CSMMarketData.GetCurrentMarketData().GetDate();                  
                    CSMCurrency currencyEUR = CSMCurrency.GetCSRCurrency(CSxSummaryController.ccyEUR);

                    int adjTradeDate = CSxSummaryController.GetNextBusinessDay(currencyEUR, currDate+1);
                    CSMDay tradeDateDay = new CSMDay(adjTradeDate);
                    DateTime tradeD = new DateTime(tradeDateDay.fYear, tradeDateDay.fMonth, tradeDateDay.fDay);

                    int stlDate = CSxSummaryController.GetNextBusinessDay(currencyEUR, adjTradeDate+1);
                    stlDate = CSxSummaryController.GetNextBusinessDay(currencyEUR, stlDate+1);
                    
                    int adjSettleDate = CSxSummaryController.GetNextBusinessDay(currencyEUR, stlDate);
                    CSMDay settleDateDay = new CSMDay(adjSettleDate);
                    DateTime settleD = new DateTime(settleDateDay.fYear, settleDateDay.fMonth, settleDateDay.fDay);

                    CMString fundName = "";
                    for (int folItem = 0; folItem < nbOfFolios; folItem++)
                    {
                        fundName = "";
                        folId = tktExtraction.GetNthPortfolioId(folItem);
                        using (CSMAmPortfolio tktFolio = CSMAmPortfolio.GetCSRPortfolio(folId, tktExtraction))
                        {

                            tktFolio.GetName(fundName);
                            double medioFundNav = 0;
                            if (tktFolio.GetParentCode() == 1 && tktFolio.GetCode() != 1)
                            {

                                int fundCode = tktFolio.GetCode();
                                colWeightInFundPercent.GetPortfolioCell(fundCode, fundCode, tktExtraction, ref val, cellStyle, true);
                                double fundWeightPercent = val.doubleValue;

                                colStratTargetWeightPercent.GetPortfolioCell(fundCode, fundCode, tktExtraction, ref val, cellStyle, true);
                                double targetWeightPercent = val.doubleValue;
                                double deviationWeightPercent = targetWeightPercent - fundWeightPercent;

                                colMedioNav.GetPortfolioCell(fundCode, fundCode, tktExtraction, ref val, cellStyle, true);
                                medioFundNav = val.doubleValue;


                                double fundWeightValue = (medioFundNav / 100.0) * fundWeightPercent;
                                double targetWeightValue = (medioFundNav / 100.0) * targetWeightPercent;
                                double deviationWeightValue = fundWeightValue - targetWeightValue;

                                int folioCcy = tktFolio.GetCurrency();
                                CMString ccyStr = "";
                                CMString fundBaseCcy = "";
                                ccyStr = CSMCurrency.CurrencyToString(folioCcy);
                                fundBaseCcy = CSMCurrency.CurrencyToString(folioCcy);

                                var nodeItemObj = new object[] {
                                            fundName.StringValue,
                                            null,
                                            null,
                                            null,
                                            ccyStr,
                                            targetWeightValue,
                                            medioFundNav,
                                            deviationWeightValue
                                        };


                                TreeListNode fundNode = treeList1.AppendNode(nodeItemObj, -1);

                                int stratNb = tktFolio.GetSiblingCount();
                                CMString stratName = "";
                                double cashPercent = 0;
                                for (int i = 0; i < stratNb; i++)
                                {
                                    using (CSMPortfolio stratFolio = tktFolio.GetNthSibling(i))
                                    {
                                        if (stratFolio != null)
                                        {                          
                                            stratFolio.GetName(stratName);
                                            if (stratName == "Cash")
                                            {
                                           
                                            int stratCode = stratFolio.GetCode();
                                            colWeightInFundPercent.GetPortfolioCell(stratCode, stratCode, tktExtraction, ref val, cellStyle, true);
                                            cashPercent = val.doubleValue;

                                            break;
                                            }
                                        }
                                    }
                                }

                                for (int i = 0; i < stratNb; i++)
                                {
                                    using (CSMPortfolio stratFolio = tktFolio.GetNthSibling(i))
                                    {
                                        if (stratFolio != null)
                                        {
                                            stratName = "";
                                            stratFolio.GetName(stratName);
                                            int stratCode = stratFolio.GetCode();

                                            int stratCcy = stratFolio.GetCurrency();
                                            ccyStr = "";
                                            ccyStr = CSMCurrency.CurrencyToString(stratCcy);

                                            colWeightInFundPercent.GetPortfolioCell(stratCode, stratCode, tktExtraction, ref val, cellStyle, true);
                                            double stratWeightPercent = val.doubleValue;

                                            colStratTargetWeightPercent.GetPortfolioCell(stratCode, stratCode, tktExtraction, ref val, cellStyle, true);
                                            double stratTargetWeightPercent = val.doubleValue;
                                            stratTargetWeightPercent = stratTargetWeightPercent * (1 - cashPercent/100);
                                            double stratDeviationWeightPercent = stratWeightPercent - stratTargetWeightPercent;

                                            colMedioNav.GetPortfolioCell(stratCode, stratCode, tktExtraction, ref val, cellStyle, true);
                                            double medioStratNav = val.doubleValue;

                                            double stratWeightValue = (medioFundNav / 100.0) * stratWeightPercent;
                                            double stratTargetWeightValue = (medioFundNav / 100.0) * stratTargetWeightPercent;
                                            double stratDevWeightValue = stratWeightValue - stratTargetWeightValue;

                                            if (medioStratNav != 0)
                                            {
                                                string tranferCcy = "EUR";

                                                var stratNodeObj = new object[] {
                                            stratName.StringValue,
                                            stratTargetWeightPercent,
                                            stratWeightPercent,
                                            stratDeviationWeightPercent,
                                            ccyStr,
                                            stratTargetWeightValue,
                                            medioStratNav,
                                            stratDevWeightValue,
                                            null,
                                            null,
                                            null,
                                            null,
                                            tranferCcy,
                                            tradeD,
                                            settleD
                                         };

                                                DevExpress.XtraTreeList.Nodes.TreeListNode stratNode = new DevExpress.XtraTreeList.Nodes.TreeListNode();
                                                if (stratName != "Cash")
                                                {
                                                    stratNode = treeList1.AppendNode(stratNodeObj, fundNode.Id);
                                                }
                                                if (onlyExtFunds == true)
                                                {
                                                    if (CSxSummaryController._MIFLStrategiesMap.ContainsValue(stratName))
                                                    {
                                                        fundNode.Nodes.Clear();
                                                        stratNode = treeList1.AppendNode(stratNodeObj, fundNode.Id);

                                                        int posCount = stratFolio.GetTreeViewPositionCount();

                                                        for (int k = 0; k < posCount; k++)
                                                        {
                                                            using (CSMPosition pos = stratFolio.GetNthTreeViewPosition(k))
                                                            {
                                                                if (pos != null)
                                                                {
                                                                    if (pos.IsOpen())
                                                                    {

                                                                        int instrCode = pos.GetInstrumentCode();
                                                                        using (CSMInstrument instr = CSMInstrument.GetInstance(instrCode))
                                                                        {
                                                                            if (instr != null)
                                                                            {
                                                                                CMString name = instr.GetName();

                                                                                char instrType = instr.GetType_API();
                                                                                string allotName = instr.GetAllotmentName();

                                                                                if (instrType == 'Z')
                                                                                {

                                                                                    if (CSxSummaryController._AllotmentsExtFunds.Contains(allotName))
                                                                                    {
                                                                                        colMedioNav.GetPositionCell(pos, pos.GetPortfolioCode(), pos.GetPortfolioCode(), tktExtraction, instrCode, instrCode, ref val, cellStyle, true);
                                                                                        double posNav = val.doubleValue;

                                                                                        colWeightInFundPercent.GetPositionCell(pos, pos.GetPortfolioCode(), pos.GetPortfolioCode(), tktExtraction, instrCode, instrCode, ref val, cellStyle, true);
                                                                                        double posWeightPercent = val.doubleValue;
                                                                                        double posTargetWeightPercent = 0;


                                                                                        int posCcy = pos.GetCurrency();
                                                                                        ccyStr = "";
                                                                                        ccyStr = CSMCurrency.CurrencyToString(posCcy);
                                                                                        try
                                                                                        {                               
                                                                                            CSMPortfolioColumn.GetHierarchicalPositionCell(pos.GetIdentifier(),CSxSummaryController._colPosTargetWeightPercentName, ref val);
                                                                                            posTargetWeightPercent = val.doubleValue;
                                                                                        }
                                                                                        catch (Exception ex)
                                                                                        {
                                                                                            string mess = ex.Message;
                                                                                        }

                                                                                        double posTargetWeightValue = (medioFundNav / 100.0) * posTargetWeightPercent;
                                                                                        double posWeightValue = (posNav / 100.0) * posWeightPercent;
                                                                                        double posDevWeightValue = posNav-posTargetWeightValue;
                                                                                        double posDeviationWeightPercent = posWeightPercent - posTargetWeightPercent;


                                                                                        var posNodeObj = new object[] {
                                                                                                  name,
                                                                                               posTargetWeightPercent,
                                                                                               posWeightPercent,
                                                                                                posDeviationWeightPercent,
                                                                                                ccyStr,
                                                                                                posTargetWeightValue,
                                                                                                posNav,
                                                                                                posDevWeightValue
                                                                                                };
                                                                                        treeList1.AppendNode(posNodeObj, stratNode.Id);
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                       break;
                                                    }

                                                }
                                                else
                                                {
                                                    int posCount = stratFolio.GetTreeViewPositionCount();
                                                    for (int k = 0; k < posCount; k++)
                                                    {
                                                        using (CSMPosition pos = stratFolio.GetNthTreeViewPosition(k))
                                                        {
                                                            if (pos != null)
                                                            {
                                                                int instrCode = pos.GetInstrumentCode();
                                                                using (CSMInstrument instr = CSMInstrument.GetInstance(instrCode))
                                                                {
                                                                    if (instr != null)
                                                                    {
                                                                        CMString name = instr.GetName();
                                                                        int posCcy = pos.GetCurrency();
                                                                        ccyStr = "";
                                                                        ccyStr = CSMCurrency.CurrencyToString(posCcy);

                                                                        char instrType = instr.GetType_API();
                                                                        string allotName = instr.GetAllotmentName();
                                                                        if (instrType == 'C' && allotName == "CASH")
                                                                        {
                                                                            colAvailableCashEUR.GetPositionCell(pos, pos.GetPortfolioCode(), pos.GetPortfolioCode(), tktExtraction, instrCode, instrCode, ref val, cellStyle, true);
                                                                            double posNav = val.doubleValue;                                                                   
                                                                           
                                                                            CSMPortfolioColumn.GetHierarchicalPositionCell(pos.GetIdentifier(), CSxSummaryController._colPosTargetWeightPercentName, ref val);

                                                                            double cashModelWeightPercent = val.doubleValue;
                                                                            double cashActualWeightPercent = 0;
                                                                            if (medioFundNav != 0)
                                                                            {
                                                                                int nodeCcyIdent = CSMCurrency.StringToCurrency(ccyStr);
                                                                                double fxRate = 1;
                                                                                fxRate = CSMMarketData.GetCurrentMarketData().GetForex(nodeCcyIdent, CSxSummaryController.ccyEUR);
                                                                                double fxPosNav = posNav * fxRate;

                                                                                cashActualWeightPercent = 100 * (fxPosNav / medioFundNav);
                                                                            }

                                                                            double cashDeviationWeightPercent = cashActualWeightPercent - cashModelWeightPercent;
                                                                            double cashModelWeight = (medioFundNav / 100) * cashModelWeightPercent;
                                                                            double cashDeviation= posNav- cashModelWeight;


                                                                            if (posNav != 0)
                                                                            {
                                                                                var cashNodeObj = new object[] {
                                                                                ccyStr,
                                                                                cashModelWeightPercent,
                                                                                cashActualWeightPercent,
                                                                                cashDeviationWeightPercent,
                                                                                null,
                                                                                cashModelWeight,
                                                                                posNav,
                                                                                cashDeviation
                                                                            };

                                                                                if (stratName != "Cash")
                                                                                {
                                                                                    treeList1.AppendNode(cashNodeObj, stratNode.Id);
                                                                                }
                                                                            }

                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                }
                                            }

                                        }
                                    }

                                }


                            }
                        }

                    }
                }
                this.treeList1.EndUnboundLoad();

            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }
       
        private Dictionary<string, int> GetFundStrategies(int fundCode)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            string stratName = "";
            try
            {
                using (CSMAmPortfolio fundFolio = CSMAmPortfolio.GetCSRPortfolio(fundCode))
                {
                    if (fundFolio != null)
                    {
                        ArrayList list = new ArrayList();
                        fundFolio.GetChildren(list);
                        IEnumerator myEnum = list.GetEnumerator();
                        while (myEnum.MoveNext())
                        {
                            CSMPortfolio itemFolio = (CSMPortfolio)myEnum.Current;
                            if (itemFolio != null)
                            {
                                using (CSMAmPortfolio amFolio = CSMAmPortfolio.GetCSRPortfolio(itemFolio.GetCode()))
                                {
                                    if (amFolio != null)
                                    {
                                        int stratId = amFolio.GetStrategyIdHier();
                                        int folioIdent = amFolio.GetCode();
                                        using (CSMAmFolioStrategy strat = new CSMAmFolioStrategy())
                                        {
                                            if (strat.Load(stratId) == true)
                                            {
                                                stratName = strat.GetName();
                                                if (result.ContainsKey(stratName) == false)
                                                {
                                                    result.Add(stratName, folioIdent);
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return result;
        }


        private void LoadButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoadSummary(_OnlyExternalFunds, _folios);
                CSxSummaryController.nodesForProcessing.Clear();
                CSxSummaryController.underThersholdStrat.Clear();
                CSxSummaryController.miflTransfers.Clear();
                treeList1.ExpandToLevel(0);
                treeList1.Refresh();
                CSxAccountController.GetAccountsList();
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }


        private void OnGetStateImage(object sender, GetStateImageEventArgs e)
        {
            e.Node.StateImageIndex = e.Node.Level;
            if (_OnlyExternalFunds == false && e.Node.Level == 2)
            {
                e.Node.StateImageIndex = 3;
            }

            if (e.Node[1] != null)
            {
                if (e.Node[1].ToString() == "Cash")
                {
                    e.Node.StateImageIndex = 3;
                }
            }
        }

        private void loadImages()
        {
            try
            {
                // Add the images manually to .resx every time the controller is modified 
                var resources = new System.ComponentModel.ComponentResourceManager(typeof(CSxSummaryScreen));

                var image = (System.Drawing.Image)(resources.GetObject("fund"));
                if (image != null) imageList.Images.Add(image);
                image = (System.Drawing.Image)(resources.GetObject("folder"));
                if (image != null) imageList.Images.Add(image);
                image = (System.Drawing.Image)(resources.GetObject("strat"));
                if (image != null) imageList.Images.Add(image);
                image = (System.Drawing.Image)(resources.GetObject("currency"));
                if (image != null) imageList.Images.Add(image);
                image = (System.Drawing.Image)(resources.GetObject("position"));
                if (image != null) imageList.Images.Add(image);

            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        private void btnSendSR_Click(object sender, EventArgs e)
        {
            CSMBoKernelBlotterOpener.OpenTradeBlotter(CSxSummaryController._BlotterApproved);
        }

        private void treeList1_NodeCellStyle(object sender, GetCustomNodeCellStyleEventArgs e)
        {
           
            if (e.Column.FieldName != "Cash Transfer") return;
            try
            {             
                if (e.Node.GetValue("Cash Transfer") != null)
                {
                    double amount = 0;
                    Double.TryParse(e.Node.GetValue("Cash Transfer").ToString(), out amount);
                    if (amount > 0)
                    {
                        e.Appearance.BackColor = Color.Green;
                        e.Appearance.ForeColor = Color.White;
                        e.Appearance.FontStyleDelta = FontStyle.Bold;
                    }
                    else if (amount < 0)
                    {
                        e.Appearance.BackColor = Color.Red;
                        e.Appearance.ForeColor = Color.White;
                        e.Appearance.FontStyleDelta = FontStyle.Bold;

                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }


        private void btnSendForApproval_Click(object sender, EventArgs e)
        {
            try
            {
                List<TreeListNode> list = treeList1.GetNodeList();
                int uident = 0;
                int gident = 0;
                CSMPreference.GetUserID(ref uident, ref gident);

                if (radioGroup1.SelectedIndex == 2)//DIM-TO-DIM
                {
                    if (CSxSummaryController.CreateDIMtoDIMTransfers(list, uident))
                    {
                        treeList1.OptionsBehavior.ReadOnly = true;
                    }
                }
                else
                {
                    if (CSxSummaryController.CreateCashTransfers(list, uident))
                    {
                        treeList1.OptionsBehavior.ReadOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        private void treeList1_CellValueChanged_1(object sender, CellValueChangedEventArgs e)
        {
            try
            {             
                if (e.Column.FieldName == "Cash Transfer")
                {              
                    double fundMasterNav = 0;

                    if (e.Node.Level == 1)
                    {

                        if (e.Node.ParentNode.Level == 0)
                        {
                            Double.TryParse(e.Node.ParentNode.GetValue("FundNAV").ToString(), out fundMasterNav);
                        }
                        long amount = 0;
                        if (e.Node.GetValue(e.Column.AbsoluteIndex) != null)
                        {
                            string myVal = e.Node.GetValue(e.Column.AbsoluteIndex).ToString();
                            if (e.Node.GetValue(e.Column.AbsoluteIndex).ToString() != "")
                            {
                                if (myVal.ToUpper().Contains('K'))
                                {
                                    string nbVal = myVal.ToUpper().TrimEnd('K');
                                    double number = 0;
                                    if (Double.TryParse(nbVal, out number))
                                    {
                                        number = number * 1000;
                                        e.Node.SetValue(e.Column.AbsoluteIndex, number);
                                    }

                                }
                                else if (myVal.ToUpper().Contains('M'))
                                {
                                    string nbVal = myVal.ToUpper().TrimEnd('M');
                                    double number = 0;
                                    if (Double.TryParse(nbVal, out number))
                                    {
                                        number = number * 1000000;
                                        e.Node.SetValue(e.Column.AbsoluteIndex, number);
                                    }

                                }

                            }
                        }

                        if (Int64.TryParse(e.Node.GetValue(e.Column.AbsoluteIndex).ToString(), out amount) == true)
                        {
                            e.Node.SetValue(e.Column.AbsoluteIndex, amount);                        
                        }

                        string stratName = "";
                        double sum = 0;
                        long sumItm = 0;
                        int mainIndex = 0;

                        if (radioGroup1.SelectedIndex == 0)//MIFL-TO-DIM only
                        {
                            //Populate dynamic colums at strategy level for the edited node
                            computeDynamicColumns(e.Node, fundMasterNav);

                            foreach (TreeListNode strategyNode in e.Node.ParentNode.Nodes)//compute the transfer amount at fund level
                            {
                                stratName = strategyNode.GetValue("FundName").ToString();
                                if (CSxSummaryController._MIFLStrategiesMap.ContainsValue(stratName))
                                {
                                    mainIndex = strategyNode.Id;
                                }
                                else
                                {
                                    if (strategyNode.GetValue(e.Column.AbsoluteIndex) != null)
                                    {
                                        if (Int64.TryParse(strategyNode.GetValue(e.Column.AbsoluteIndex).ToString(), out sumItm))
                                        {
                                            sum += sumItm;
                                        }

                                    }
                                }
                            }

                            foreach (TreeListNode miflNode in e.Node.ParentNode.Nodes)
                            {
                                if (miflNode.Id == mainIndex)
                                {
                                    double miflAmount = (-1) * sum;
                                    miflNode.SetValue(e.Column.AbsoluteIndex, miflAmount);                                   
                                    computeDynamicColumns(miflNode, fundMasterNav);

                                    break;
                                }
                            }

                        }
                        else if (radioGroup1.SelectedIndex == 2)//DIM-TO-DIM 
                        {
                            computeDynamicColumns(e.Node, fundMasterNav);
                        }
                    }
                }
                else if (e.Column.FieldName == "Transfer CCY")
                {                 
                    if (e.Node.Level == 1)
                    {
                        double fundMasterNav = 0;

                            if (e.Node.ParentNode.Level == 0)
                            {
                                Double.TryParse(e.Node.ParentNode.GetValue("FundNAV").ToString(), out fundMasterNav);
                            }

                            if (e.Node.GetValue(e.Column.AbsoluteIndex) != null)
                        {
                            if (e.Node.GetValue("Transfer CCY") != null)
                            {
                                string nodeCcy = e.Node.GetValue("Transfer CCY").ToString();
                                if (nodeCcy != "")
                                {
                                    using (CSMCurrency rowCCY = CSMCurrency.GetCSRCurrency(CSMCurrency.StringToCurrency(nodeCcy)))
                                    {
                                        
                                    int currDate = CSMMarketData.GetCurrentMarketData().GetDate();
                                    int adjTradeDate = CSxSummaryController.GetNextBusinessDay(rowCCY, currDate + 1);
                                    int stlDate = CSxSummaryController.GetNextBusinessDay(rowCCY, adjTradeDate + 1);
                                    stlDate = CSxSummaryController.GetNextBusinessDay(rowCCY, stlDate + 1);
                                        
                                    int adjSettleDate = CSxSummaryController.GetNextBusinessDay(rowCCY, stlDate);

                                    using (CSMDay tradeDateDay = new CSMDay(adjTradeDate))
                                    using (CSMDay settleDateDay = new CSMDay(adjSettleDate))
                                    {
                                        DateTime tradeD = new DateTime(tradeDateDay.fYear, tradeDateDay.fMonth, tradeDateDay.fDay);
                                        DateTime settleD = new DateTime(settleDateDay.fYear, settleDateDay.fMonth, settleDateDay.fDay);

                                        e.Node.SetValue("Trade Date", tradeD);
                                        e.Node.SetValue("Settlement Date", settleD);
                                    }
                                }
                                }
                            }

                            foreach (TreeListNode currentNode in e.Node.ParentNode.Nodes)
                            {                              
                                currentNode.SetValue(e.Column.AbsoluteIndex, e.Node.GetValue("Transfer CCY").ToString());
                                computeDynamicColumns(currentNode, fundMasterNav);
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        private void treeList1_CustomNodeCellEdit(object sender, GetCustomNodeCellEditEventArgs e)
        {
            if (e.Column.FieldName == "Cash Transfer" || e.Column.FieldName == "Transfer CCY" || e.Column.FieldName == "Trade Date" || e.Column.FieldName == "Settlement Date")
            {
                using (RepositoryItemTextEdit edit = new RepositoryItemTextEdit())
                {
                    if (e.Node.Level == 1)
                    {
                        e.RepositoryItem.ReadOnly = false;

                        if (CSxSummaryController.nodesForProcessing.Contains(e.Node.Id))
                        {
                            edit.ReadOnly = true;
                            e.RepositoryItem = edit;
                        }
                    }
                    else if (e.Node.Level == 2 || e.Node.Level == 0)
                    {                   
                            edit.ReadOnly = true;
                            e.RepositoryItem = edit;
                    }
                }
            }
        }

        private void btnAccounts_Click(object sender, EventArgs e)
        {
            XSRWinFormsAdapter<AccountsListForm> adapter = XSRWinFormsAdapter<AccountsListForm>.GetUniqueDialog("Accounts Threshold", true);
            if (!adapter.IsVisible)
                adapter.ShowWindow();
        }

        private void computeDynamicColumns(TreeListNode stratNode, double fundMasterNav)
        {
            try
            {
                long transferAmount = 0;
                double fundNav = 0;
                double fxRate = 1;
                double newActualPosition = 0;
                double modelWeight = 0;
                string nodeCcy = "";

                if (stratNode.GetValue("Cash Transfer") != null)
                {
                    Int64.TryParse(stratNode.GetValue("Cash Transfer").ToString(), out transferAmount);
                }

                nodeCcy = stratNode.GetValue("Transfer CCY").ToString();

                if (transferAmount != 0)
                {
                    if (Double.TryParse(stratNode.GetValue("FundNAV").ToString(), out fundNav))
                    {

                        int nodeCcyIdent = CSMCurrency.StringToCurrency(nodeCcy);
                        fxRate = CSMMarketData.GetCurrentMarketData().GetForex(nodeCcyIdent, CSxSummaryController.ccyEUR);
                        double fxCashTransfer = transferAmount * fxRate;
                        newActualPosition = fundNav + fxCashTransfer;
                        stratNode.SetValue("New Actual Position(EUR)", newActualPosition);
                    }

                    if (fundMasterNav != 0)
                    {
                        double newActWeight = newActualPosition * 100 / fundMasterNav;
                        stratNode.SetValue("New Actual Weight%", newActWeight);

                        Double.TryParse(stratNode.GetValue("ModelWeightPercent").ToString(), out modelWeight);
                        stratNode.SetValue("New Deviation%", newActWeight - modelWeight);

                    }

                    foreach (TreeListNode cashNode in stratNode.Nodes)
                    {
                        if (cashNode.GetValue("FundName").ToString() == nodeCcy)
                        {
                            double cashNav = 0;

                            if (Double.TryParse(cashNode.GetValue("FundNAV").ToString(), out cashNav))
                            {
                                newActualPosition = cashNav + transferAmount;
                                cashNode.SetValue("New Actual Position(EUR)", newActualPosition);

                                double newActualWeight = 0;
                                double newModelWeight = 0;

                                newActualWeight = newActualPosition * fxRate * 100 / fundMasterNav;
                                cashNode.SetValue("New Actual Weight%", newActualWeight);

                                Double.TryParse(cashNode.GetValue("ModelWeightPercent").ToString(), out newModelWeight);
                                cashNode.SetValue("New Deviation%", newActualWeight - newModelWeight);

                            }
                        }
                        else
                        {
                            double nav = 0;
                            double actWeight = 0;
                            double deviation = 0;

                            if (Double.TryParse(cashNode.GetValue("FundNAV").ToString(), out nav))
                            {
                                cashNode.SetValue("New Actual Position(EUR)", nav);
                            }
                            if (Double.TryParse(cashNode.GetValue("ActualWeightPercent").ToString(), out actWeight))
                            {
                                cashNode.SetValue("New Actual Weight%", actWeight);
                            }
                            if (Double.TryParse(cashNode.GetValue("Deviation").ToString(), out deviation))
                            {
                                cashNode.SetValue("New Deviation%", deviation);
                            }

                        }
                    }

                }
                else
                {
                    stratNode.SetValue("Cash Transfer", "");
                    stratNode.SetValue("New Actual Position(EUR)", "");
                    stratNode.SetValue("New Actual Weight%", "");
                    stratNode.SetValue("New Deviation%", "");

                    foreach (TreeListNode cashNode in stratNode.Nodes)
                    {
                        cashNode.SetValue("New Actual Position(EUR)", "");
                        cashNode.SetValue("New Actual Weight%", "");
                        cashNode.SetValue("New Deviation%", "");
                    }
                }
            }
            catch (Exception ex)
            {
             
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using DevExpress.XtraPrinting.Native;
using MEDIO.CORE.Tools;
using sophis.instrument;
using sophis.oms;
using Sophis.OMS.Util;
using sophis.OrderGeneration;
using sophis.OrderGeneration.PortfolioColumn;
using sophis.portfolio;
using sophis.scenario;
using sophis.utils;
using sophis.value;
using sophis.xaml;
using Sophis.GUI.Portfolios;
using Sophis.Portfolio.Internals;
using MEDIO.OrderAutomation.NET.Source.GUI;
using System.Windows.Forms;
using sophis.gui;
using sophis.static_data;
using sophisTools;
using SophisAMDotNetTools;
// for checking if folio is a strategy
using sophis.strategy;
using sophis.OrderGeneration.DOB.Builders;

namespace MEDIO.OrderAutomation.NET.Source.Scenarios
{

    class CSxDynamicOrderBuilderMAMLCtx : CSMPositionCtxMenu
    {
        public override bool IsAuthorized(ArrayList positionList)
        {
            return true;
        }

        /// <summary>
        /// If positions AND folios are selected on this method is called
        /// </summary>
        /// <param name="positionList"></param>
        /// <param name="ActionName"></param>
        public override void Action(CSMExtraction extraction, ArrayList positionList, CMString ActionName)
        {
            List<CSMPortfolio> selectedPortfolios = new List<CSMPortfolio>();
            int sicovam = -1;
            foreach (CSMPosition position in positionList)
            {
                if (sicovam > 0 && sicovam != position.GetInstrumentCode())
                {
                    MessageBox.Show("Cannot select positions on multiple instruments");
                    return;
                }
                sicovam = position.GetInstrumentCode();
              
                var folio = position.GetPortfolio();
                if (folio != null)
                {
                    var mamlList = CSxDynamicOrderBuilderMAML.GetMAMLFolio(folio.GetCode(), folio.GetExtraction());
                    if (!mamlList.IsNullOrEmpty()) selectedPortfolios.AddRange(mamlList);
                }
            }
            CSxDynamicOrderBuilderMAML.DisplayDOB(sicovam, selectedPortfolios);
        }

        public override bool IsFolioAuthorized(ArrayList folioList)
        {
            return true;
        }

        protected int fSicovam = -1;

        /// <summary>
        /// This method is called if portfolio are selected only
        /// </summary>
        /// <param name="positionList"></param>
        /// <param name="ActionName"></param>
        public override void FolioAction(ArrayList portfolioList, CMString ActionName)
        {
            List<CSMPortfolio> selectedPortfolios = new List<CSMPortfolio>();
            foreach (CSMPortfolio folio in portfolioList)
            {
                var mamlList = CSxDynamicOrderBuilderMAML.GetMAMLFolio(folio.GetCode(), folio.GetExtraction());
                if (!mamlList.IsNullOrEmpty()) selectedPortfolios.AddRange(mamlList);
            }
            CSxDynamicOrderBuilderMAML.DisplayDOB(fSicovam, selectedPortfolios);
        }
    }

    class CSxDynamicOrderBuilderMAMLFXCtx : CSxDynamicOrderBuilderMAMLCtx
    {
        public override bool IsAuthorized(ArrayList positionList)
        {
            foreach (CSMPosition position in positionList)
            {
                if (false == CSxDynamicOrderBuilderMAMLFX.IsAForex(position.GetInstrumentCode()))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// This method is called if portfolio are selected only
        /// </summary>
        /// <param name="positionList"></param>
        /// <param name="ActionName"></param>
        public override void FolioAction(ArrayList portfolioList, CMString ActionName)
        {
            if (true == CSxDynamicOrderBuilderMAMLFX.SelectForex(ref fSicovam) )
            {
                base.FolioAction(portfolioList, ActionName);
                fSicovam = -1;
            }
        }  
    }

    class CSxDynamicOrderBuilderMAML : CSMScenario
    {
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pScenario;
        }

        static internal void DisplayDOB(int sicovam, List<CSMPortfolio> selectedPortfolios)
        {
            if (selectedPortfolios.Count > 0)
            {
                var emptyPositions = new List<CSMPosition>();
                if (!DynamicOrderBuilder.Instance.IsSessionActive)
                {
                    //TODO Test and cleanup
                    //OrderBuilderEntryPoint entryPoint = new OrderBuilderEntryPoint(emptyPositions, selectedPortfolios,
                    //    selectedPortfolios[0].GetExtraction(), eOrderBuilderSessionType.DynamicOrderBuilder);
                    //OrderBuilderDialog.Display(entryPoint, null);
                    if (DynamicOrderBuilder.Instance.IsSessionActive == false)
                    {
                        if (!sophis.OrderGeneration.DOBSessionForm.Instance.StartDOBSession())
                            MessageBox.Show("Error while creating DOB Session");
                    }
                }
                if (sicovam > 0)
                {
               
                    AdjustExposureDialog.Display(sicovam, selectedPortfolios[0].GetExtraction(), selectedPortfolios, false);
                }
                else
                {
                    AdjustExposureDialog.Display(selectedPortfolios[0].GetExtraction(), selectedPortfolios, new List<CSMPosition>());
                }
            }                    

        }

        public override void Run()
        {
            List<CSMPortfolio> selectedPortfolios = new List<CSMPortfolio>();
            int sicovam = -1;
            if (true == GetPortfoliosAndInstrument(ref sicovam, selectedPortfolios))
            {
                DisplayDOB(sicovam, selectedPortfolios);
            }
        }

        protected virtual bool GetPortfoliosAndInstrument(ref int sicovam, List<CSMPortfolio> selectedPortfolios)
        {
            var selectionCount = this.GetNbofSelection();
            if (selectionCount > 0) // Portfolio view
            {
                for (int i = 0; i < selectionCount; i++)
                {
                    SSMSelectedRowGUI selection = new SSMSelectedRowGUI();
                    this.GetSelectedRow(i, selection);
                    if (selection.position._position != null)
                    {
                        if (sicovam > 0 && sicovam != selection.position._position.GetInstrumentCode())
                        {
                            MessageBox.Show("Cannot select positions on multiple instruments");
                            return false;
                        }
                        sicovam = selection.position._position.GetInstrumentCode();
                    }
                    var folio = selection.position._portfolio;
                    if (folio != null)
                    {
                        var mamlList = GetMAMLFolio(folio.GetCode(), folio.GetExtraction());
                        if (!mamlList.IsNullOrEmpty()) selectedPortfolios.AddRange(mamlList);
                    }
                }
            }
            else // Navigation 
            {
                FrontPortfolioViewInfo info = SophisPortfoliosGUI_Main.GetFrontPortfolioViewInfo();
                if (info != null)
                {
                    var folios = info.SelectedPortfolios;
                    foreach (var folio in folios)
                    {
                        var mamlList = GetMAMLFolio(folio.GetCode(), folio.GetExtraction());
                        if (!mamlList.IsNullOrEmpty()) selectedPortfolios.AddRange(mamlList);
                    }
                }
            }
            return true;
        }

        static internal List<CSMPortfolio> GetMAMLFolio(int portfolioCode, CSMExtraction extraction)
        {
            var res = new List<CSMPortfolio>();
            var folio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
            if (folio != null)
            {
                CMString name = "";
                folio.GetName(name);
                if (name.StringValue.ToUpper().StartsWith(CSxDBHelper.GetTargetTradingFolio()))//"MAML")) //TOCHANGE
                {
                    if (!res.Contains(folio))
                        if (!name.StringValue.ToUpper().Contains("CASH") || name.StringValue.ToUpper().Contains("CASH") && Convert.ToBoolean(CSxDBHelper.UseCashTargetTradingFolio()))
                        res.Add(folio);
                }

                CSMAmPortfolio amFolio = CSMAmPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (amFolio != null)
                {
                    var fund = amFolio.GetFundRootPortfolio();
                    if (fund != null)
                    {
                        ArrayList folios = new ArrayList();
                        fund.GetRealPortfoliosFromFundAndStrategy(folios);

                        foreach (int folioid in folios)
                        {
                            CSMPortfolio realFolio = CSMPortfolio.GetCSRPortfolio(folioid);
                            if (realFolio == null) continue;

                            for (int i = 0; i < realFolio.GetChildCount(); i++)
                            {
                                var child = realFolio.GetNthChild(i);
                                if (child != null)
                                {
                                    CSMAmPortfolio strategyFolio = CSMAmPortfolio.GetCSRPortfolio(child.GetCode());

                                    if (strategyFolio.IsAStrategy())
                                    {
                                        child.GetName(name);
                                        // if (name.StringValue.ToUpper().Equals(CSxDBHelper.GetTargetTradingFolio()))//"MAML"))//TOCHANGE
                                        if (name.StringValue.ToUpper().StartsWith(CSxDBHelper.GetTargetTradingFolio()))//"MAML"))//TOCHANGE
                                        {

                                            if (!res.Contains(child))
                                                if (!name.StringValue.ToUpper().Contains("CASH") || name.StringValue.ToUpper().Contains("CASH") && Convert.ToBoolean(CSxDBHelper.UseCashTargetTradingFolio()))
                                                    res.Add(child);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public override void Done()
        {
        }
    }

    /// <summary>
    /// Same as CSxDynamicOrderBuilderMAML for FX
    /// If the instrument selected is an FX do the same as CSxDynamicOrderBuilderMAML
    /// Why this dev? It is related to NDF currencies that we cannot set as NDF in the setup
    /// Else we have a cash issue
    /// We receive transactions in non deliverable currencies
    /// but if we set them as non deliverable we do not reconcile the cash
    /// test case  :
    /// buy asset A in BRL
    /// Add FWD NDF BUY BRL SELL USD
    /// transaction 1 = - 100 BRL
    /// transaction 2 = + 100 BRL - 140 USD
    /// if BRLUSD was NDF, we would not see in the report +100 BRL
    /// instead of -140 USD, we would have -100 BRL et -140 BRL
    /// So all the ccys are deliverables in the system
    /// But we have a table MEDIO_NDF_CURRENCY
    /// </summary>
    class CSxDynamicOrderBuilderMAMLFX :CSxDynamicOrderBuilderMAML
    {
        internal static List<char> impactedInstruments = new List<char> { 'X', 'E', 'K' };

        private static List<int> _NDFCurrencyList;
        public static List<int> NDFCurrencyList
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
        public static void InitNDFCurrencies(ref List<int> NDFCurrencyList)
        {
            string sql = "select STR_TO_DEVISE(currency) from MEDIO_NDF_CURRENCY";
            NDFCurrencyList = CSxDBHelper.GetMultiRecords(sql).ConvertAll(x => Convert.ToInt32(x.ToString()));
        }

        static internal bool SelectForex(ref int sicovam)
        {
            DialogResult res;
            WinformContainer.eDialogMode mode = sophis.gui.WinformContainer.eDialogMode.eModal;

            SelectFXInstrument modalForm = new SelectFXInstrument();
            res = sophis.gui.WinformContainer.DoDialog(modalForm, mode);
            if (res == DialogResult.OK)
            {
                int ccy1 = CSMCurrency.StringToCurrency(modalForm.comboBox_ccy1.Text);
                int ccy2 = CSMCurrency.StringToCurrency(modalForm.comboBox_ccy2.Text);

                if (ccy1 > 0 && ccy2 > 0)
                {
                    DateTime dt = modalForm.valueDateEdit.DateTime;
                    if (modalForm.checkBox_forward.Checked == true && dt - DateTime.Now < new TimeSpan(1, 0, 0, 0))
                    {
                        MessageBox.Show("InvalidDate. Date must be greater than Today + 1");
                        return false;
                    }
                    if (modalForm.checkBox_forward.Checked == false)
                    {
                        CSMForexSpot forexSpot = CSMForexSpot.GetCSRForexSpot(ccy1, ccy2);
                        if (forexSpot != null)
                        {
                            sicovam = forexSpot.GetCode();
                        }
                    }
                    else
                    {
                        sicovam = GetForwardInstrument(ccy1, ccy2, dt);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool IsAForex(int sicovam)
        {
            CSMInstrument inst = CSMInstrument.GetInstance(sicovam);
            if (inst == null || impactedInstruments.Contains(inst.GetInstrumentType()) == false)
            {
                return false;
            }
            return true;
        }

        protected override bool GetPortfoliosAndInstrument(ref int sicovam, List<CSMPortfolio> selectedPortfolios)
        {
            base.GetPortfoliosAndInstrument(ref sicovam, selectedPortfolios);
            if (sicovam > 0)
            {
                if (false == IsAForex(sicovam))
                {
                    MessageBox.Show("Invalid selection. You must select an FX position.");
                    return false;
                }
            }
            else
            {
                return SelectForex(ref sicovam);
            }
            return true;
        }

        private static int GetForwardInstrument(int ccy1, int ccy2, DateTime date)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxDynamicOrderBuilderMAMLFX", "GetForwardInstrument");
                int newSicovam;
                if (NDFCurrencyList.Contains(ccy1) || NDFCurrencyList.Contains(ccy2))
                {
                    LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("{0} or {1} are in the NDF currency list", ccy1, ccy2));
                    //newSicovam = CSAMInstrumentDotNetTools.GetSettlementReportingInstrumentId(ccy1,
                    //                    ccy2, date,
                    //                    EForexNDFCreationBehaviour.CreateAsNDF, true);
                    //TODO test and clenaup uh-oh, this is created as NDF?!?
                    newSicovam = CSAMInstrumentDotNetTools.GetForexInstrument(ccy1,ccy2, sophis.amCommon.DateUtils.ToInt(date));
                }
                else
                {
                    LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Both {0} and {1} are not in the NDF currency list", ccy1, ccy2));
                    //newSicovam = CSAMInstrumentDotNetTools.GetSettlementReportingInstrumentId(ccy1,
                    //                    ccy2, date,
                    //                    EForexNDFCreationBehaviour.CreateAsForward, true);
                    //TODO test and clenaup
                    newSicovam = CSAMInstrumentDotNetTools.GetForexInstrument(ccy1, ccy2, sophis.amCommon.DateUtils.ToInt(date));
                }
                LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("New sicovam created " + newSicovam));
                LOG.End();
                return newSicovam;
            }
        
        }
    }
    
}

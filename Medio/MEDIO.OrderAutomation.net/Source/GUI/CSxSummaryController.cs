using DevExpress.XtraTreeList.Nodes;
using MEDIO.CORE.Tools;
using Mediolanum_RMA_FILTER;
using Mediolanum_RMA_FILTER.Data;
using Mediolanum_RMA_FILTER.TicketCreator;
using Mediolanum_RMA_FILTER.Tools;
using Oracle.DataAccess.Client;
using sophis.market_data;
using sophis.misc;
using sophis.portfolio;
using sophis.static_data;
using sophis.strategy;
using sophis.tools;
using sophis.utils;
using sophis.value;
using sophisTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public class CSxSummaryController
    {
        public static Dictionary<string/*account_at_custodian*/, int/*folio ID*/> _RootStratIdent;
        public static List<Tuple<int /*accountid*/, int/*entityid*/, string /*account_at_custodian*/, string /*account_name*/>> _Delegatemanagers;
        public static List<Tuple<string /*account_at_custodian*/,int /*accountid*/, string/*PTG id*/, string  /*account_name*/>> _stratAccountDetails;
        public static CSxCashCreator _cashItem;
        public static int _eventId;
        public static string _miflFolName = "";
        public static string _ExtFundsAllotmentList = "";
        public static List<string> _AllotmentsExtFunds;
        public static string _BlotterPendingApproval = "";
        public static string _BlotterApproved = "";
        public static int _businessEvtId;
        public static Dictionary<int, string> _MIFLStrategiesMap = new Dictionary<int, string>();
        public static int ccyEUR = sophis.static_data.CSMCurrency.StringToCurrency("EUR");

        public static List<int> nodesForProcessing = new List<int>();
        public static List<string> underThersholdStrat = new List<string>();
        public static Dictionary<string, double> miflTransfers = new Dictionary<string, double>();
        private static string _ClassName = "CSxSummaryController";

        public static string _colWeightInFundPercentName = "Medio Weight in Fund";
        public static string _colStratTargetWeightPercentName = "Strategic Ratio";
        public static string _colMedioNavName = "Medio Market Value curr. global";
        public static string _colAvailableCashEURName = "Avail. Cash At Result Date";
        public static string _colPosTargetWeightPercentName = "Strategy Benchmark Weights";

        
        public static void Init()
        {
            try
            {
                //new
                CSxCachingDataManager.Instance.AddItem(eMedioCachedData.RootStrategies.ToString(), CSxRBCHelper.GetRootStrategiesIdent());
                CSxCachingDataManager.Instance.AddItem(eMedioCachedData.StratAccountDetails.ToString(), CSxRBCHelper.GetStratAccountDetails());
                //loading existing functionalities
                CSxCachingDataManager.Instance.AddItem(eMedioCachedData.Delegatemanagers.ToString(), CSxRBCHelper.GetDelegateManagers());
                CSxCachingDataManager.Instance.AddItem(eMedioCachedData.Businessevents.ToString(), CSxRBCHelper.GetBusinessEventsList());
                CSxCachingDataManager.Instance.AddItem(eMedioCachedData.Rootportfolios.ToString(), CSxRBCHelper.GetAccountList());

                _Delegatemanagers = CSxCachingDataManager.Instance.GetItem(eMedioCachedData.Delegatemanagers.ToString()) as List<Tuple<int, int, string, string>>;
                _RootStratIdent = CSxCachingDataManager.Instance.GetItem(eMedioCachedData.RootStrategies.ToString()) as Dictionary<string, int>;
                _stratAccountDetails = CSxCachingDataManager.Instance.GetItem(eMedioCachedData.StratAccountDetails.ToString()) as List<Tuple<string, int, string, string>>;
                _cashItem = new CSxCashCreator(eRBCTicketType.Cash);

                CSMConfigurationFile.getEntryValue("CashAutomation", "CreateCashTransfersEventId", ref _eventId, 1680);
                CSMConfigurationFile.getEntryValue("CashAutomation", "MedioFolName", ref _miflFolName, "MIFL");
                CSMConfigurationFile.getEntryValue("CashAutomation", "ExtFundAllotmentList", ref _ExtFundsAllotmentList, "GFP FUNDS;ETF");
                CSMConfigurationFile.getEntryValue("CashAutomation", "PendingApprovalBlotterName", ref _BlotterPendingApproval, "Subs/Reds Pending Blotter");
                CSMConfigurationFile.getEntryValue("CashAutomation", "ApprovedBlotterName", ref _BlotterApproved, "Subs/Reds Approved Blotter");
                CSMConfigurationFile.getEntryValue("CashAutomation", "BusinessEventId", ref _businessEvtId, 499);


                _AllotmentsExtFunds = _ExtFundsAllotmentList.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        public static int GetNextBusinessDay(CSMCurrency ccy, int date)
        {
            while (ccy.IsABankHolidayDay(date))
            {
                date++;
            }

            return date;
        }
        public static Dictionary<string, int> GetFundStrategies(int fundCode)
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
        public static string GetMIFLMainStrat(int fundCode)
        {
            CSMConfigurationFile.getEntryValue("CashAutomation", "MedioFolName", ref _miflFolName, "MIFL");
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
                            using (CSMPortfolio itemFolio = (CSMPortfolio)myEnum.Current)
                            {
                                if (itemFolio != null)
                                {
                                    CMString folName = "";
                                    itemFolio.GetName(folName);

                                    if (folName == CSxSummaryController._miflFolName)
                                    {
                                        using (CSMAmPortfolio amFolio = CSMAmPortfolio.GetCSRPortfolio(itemFolio.GetCode()))
                                        {
                                            if (amFolio != null)
                                            {
                                                int stratId = amFolio.GetStrategyIdHier();
                                                using (CSMAmFolioStrategy strat = new CSMAmFolioStrategy())
                                                {
                                                    strat.Load(stratId);
                                                    stratName = strat.GetName();
                                                    break;
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
            return stratName;
        }

        public static string GetStrategyCommonIdentifier(string strategyName, int rootFundIdent)
        {
            string result = "";
            Dictionary<string, int> fundStrategies = GetFundStrategies(rootFundIdent);
            try
            {
                if (fundStrategies.ContainsKey(strategyName))
                {
                    int currentNodeStratId = fundStrategies[strategyName];

                    if (_RootStratIdent.ContainsValue(currentNodeStratId))
                    {
                        var key = _RootStratIdent.FirstOrDefault(x => x.Value == currentNodeStratId).Key;

                        if (key != null)
                        {
                            result = key.ToString();
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

        public static int GetCashFolioIdent(string commonIdentifier)
        {
            int result = 0;
            try
            {
                string sql = "SELECT ACCOUNT_LEVEL_FOLIO FROM BO_TREASURY_ACCOUNT WHERE ID = (SELECT ID FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = :ACCOUNT_AT_CUSTODIAN)";
                using (OracleParameter parameter = new OracleParameter(":ACCOUNT_AT_CUSTODIAN", commonIdentifier))
                {
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                    int res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    if (res != 0)
                    {
                        result = res;
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return result;
        }

        public static int GetAccountIdent(string commonIdentifier)
        {
            int result = 0;
            try
            {
                Tuple<string, int, string, string> miflMainAccount = _stratAccountDetails.Find(x => (x.Item1.ToSafeString().ToUpper().CompareTo(commonIdentifier.ToUpper()) == 0));
                if (miflMainAccount != null)
                {
                    int accountId = miflMainAccount.Item2;
                    result = accountId;
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return result;
        }
        public static string GetMIFLAccountName(string commonIdentifier)
        {
            string result = "";
            try
            {
                Tuple<string, int, string, string> miflMainAccount = _stratAccountDetails.Find(x => (x.Item1.ToSafeString().ToUpper().CompareTo(commonIdentifier.ToUpper()) == 0));
                if (miflMainAccount != null)
                {
                    string accountName = miflMainAccount.Item4;
                    result = accountName.Trim();
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return result;
        }

        public static string GetAccountPTG(string commonIdentifier)
        {
            string result = "";
            try
            {
                Tuple<string, int, string,string> miflMainAccount = _stratAccountDetails.Find(x => (x.Item1.ToSafeString().ToUpper().CompareTo(commonIdentifier.ToUpper()) == 0));
                if (miflMainAccount != null)
                {
                    string ptgId = miflMainAccount.Item3;
                    result = ptgId.Trim();
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return result;
        }



        public static bool CreateDIMtoDIMTransfers(List<TreeListNode> list, int userId)
        {
            bool successFlag = false;
            try
            {           
                Dictionary <int,string> funds= new Dictionary<int, string>();

                using (CSMEventVector eventVector = new CSMEventVector())
                {
                    eventVector.ActivateNativeLifeCycle();
                    CSMTransaction.StartMultiInsertion();
                    bool isFirstTrade = true;
                    long firstDIMTradeId = 0;
                    string firstDIMAccountId = "";
                    double deltaAmount = 0;
                    string firstDIMAccountName = "";
                    int firstCCYId = 0;
                    long firstTradeDate = 0;
                    long firstStlDate = 0;
                    bool sameCCy = true;
                    bool MIFLStratUsed = false;
                    bool sameDates = true;

                    foreach (var t in list)
                    {
                        int DIMSicovam = 0;
                        string DIMCommonIdentifier = "";
                        int DIMCashFolio = 0;
                        int DIMAccountId = 0;
                        int DIMDepositary = 0;
                        int DIMBroker = 0;
                        string DIMPTG = "";
                        string DIMAccountName = "";

                        TreeListNode myNode = (TreeListNode)t;
                        if (myNode.Level == 1)
                        {
                            double transferAmt = 0;
                            if (myNode.GetValue("Cash Transfer") != null)
                            {
                                Double.TryParse(myNode.GetValue("Cash Transfer").ToString(), out transferAmt);
                                if (transferAmt != 0)
                                {
                                    DateTime stdDate = DateTime.Today;
                                    int tradeDate = 0;
                                    int settlementDate = 0;
                                    if (myNode.GetValue("Trade Date") != null)
                                    {
                                        if (DateTime.TryParse(myNode.GetValue("Trade Date").ToString(), out stdDate))
                                        {
                                            using (CSMDay trDate = new CSMDay(stdDate.Day, stdDate.Month, stdDate.Year))
                                            {
                                                tradeDate = trDate.toLong();
                                            }
                                        }
                                        if (DateTime.TryParse(myNode.GetValue("Settlement Date").ToString(), out stdDate))
                                        {
                                            using (CSMDay trDate = new CSMDay(stdDate.Day, stdDate.Month, stdDate.Year))
                                            {
                                                settlementDate = trDate.toLong();
                                            }
                                        }
                                    }

                                    string fundName = myNode.ParentNode.GetValue("FundName").ToString().Trim();
                                    string stratName = myNode.GetValue("FundName").ToString();
                                    int realFolioCode = CSMTransaction.solve_book_string(fundName);
                                    if (funds.ContainsKey(realFolioCode) == false)
                                        funds.Add(realFolioCode, fundName);

                                    using (CSMPortfolio fundFolio = CSMPortfolio.GetCSRPortfolio(realFolioCode))
                                    {
                                        if (fundFolio != null)
                                        {
                                            string transferCcy = "EUR";
                                            int entityId = fundFolio.GetEntity();
                                            if (myNode.GetValue("Transfer CCY") != null)
                                            {
                                                transferCcy = myNode.GetValue("Transfer CCY").ToString();
                                            }
                                            int ccyIdent = sophis.static_data.CSMCurrency.StringToCurrency(transferCcy);
                                            DIMCommonIdentifier = GetStrategyCommonIdentifier(stratName, realFolioCode);
                                            if (DIMCommonIdentifier == "")
                                            {
                                                continue;
                                            }

                                            DIMCashFolio = GetCashFolioIdent(DIMCommonIdentifier);
                                            DIMSicovam = _cashItem.GetCashInstrumentSicovam(transferCcy, RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, DIMCommonIdentifier, DIMCommonIdentifier, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, DIMCashFolio, null, DIMCommonIdentifier);
                                            DIMDepositary = _cashItem.GetDepositary(DIMCommonIdentifier);
                                            DIMPTG = GetAccountPTG(DIMCommonIdentifier);
                                            if (_MIFLStrategiesMap.ContainsValue(stratName))
                                            {
                                                DIMAccountId = GetAccountIdent(DIMCommonIdentifier);
                                                DIMAccountName = GetMIFLAccountName(DIMCommonIdentifier);
                                                MIFLStratUsed = true;
                                            }
                                            else
                                            {
                                                Tuple<int, int, string, string> delegatemanager = _Delegatemanagers.Find(x => (x.Item3.ToSafeString().ToUpper().CompareTo(DIMCommonIdentifier.ToUpper()) == 0));
                                                if (delegatemanager != null)
                                                {
                                                    DIMAccountId = delegatemanager.Item1;
                                                    DIMBroker = delegatemanager.Item2;
                                                    DIMAccountName = delegatemanager.Item4;
                                                }
                                            }

                                            if (CSxAccountController._thresholdAccMap.ContainsKey(DIMAccountId))
                                            {
                                                double amount = transferAmt;
                                                int threshold = CSxAccountController._thresholdAccMap[DIMAccountId];
                                                if (transferCcy != "EUR")
                                                {
                                                    double fxRate = CSMMarketData.GetCurrentMarketData().GetForex(ccyIdent, ccyEUR);
                                                    amount = amount * fxRate;
                                                }
                                                if (Math.Abs(amount) < threshold)
                                                {
                                                    underThersholdStrat.Add(stratName);
                                                }
                                            }

                                            using (CSMTransaction trans = CSMTransaction.newCSRTransaction())
                                            {

                                                trans.SetInstrumentCode(DIMSicovam);
                                                trans.SetQuantity(transferAmt);
                                                trans.SetSpot(1);
                                                trans.SetNetAmountOnly(-trans.GetQuantity());
                                                trans.SetFolioCode(DIMCashFolio);
                                                trans.SetTransactionDate(tradeDate);
                                                trans.SetSettlementDate(settlementDate);
                                                trans.SetSettlementCurrency(ccyIdent);
                                                trans.SetTransactionType((eMTransactionType)_businessEvtId);
                                                trans.SetNostroCashId(DIMAccountId);
                                                trans.SetDepositary(DIMDepositary);
                                                trans.SetCounterparty(DIMDepositary);
                                                trans.SetBroker(DIMBroker);
                                                trans.SetEntity(entityId);
                                                trans.SetOperator(userId);
                                                trans.SetTransactionTime(CSMMarketData.GetCurrentMarketData().GetTime());


                                                if (isFirstTrade == true)
                                                {
                                                    trans.DoAction(_eventId, eventVector, true);
                                                    firstDIMTradeId = trans.getInternalCode();
                                                    firstDIMAccountId = DIMPTG;
                                                    firstDIMAccountName = DIMAccountName;
                                                    firstCCYId = ccyIdent;
                                                    firstTradeDate = tradeDate;
                                                    firstStlDate = settlementDate;
                                                    isFirstTrade = false;
                                                }
                                                else
                                                {
                                                    if (firstTradeDate != tradeDate || firstStlDate != settlementDate)
                                                    {
                                                        sameDates = false;
                                                    }
                                                    if (firstCCYId != ccyIdent)
                                                    {
                                                        sameCCy = false;
                                                    }
                                                    CMString info = firstDIMAccountId.ToString() + "|" + firstDIMAccountName;
                                                    trans.SetBackOfficeInfos(info);
                                                    info = firstDIMTradeId.ToString();
                                                    trans.SetComment(info);
                                                    trans.DoAction(_eventId, eventVector, true);
                                                }

                                                deltaAmount += transferAmt;
                                            }
                                            nodesForProcessing.Add(myNode.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (nodesForProcessing.Count != 0)
                    {
                        DialogResult resultDlg = MessageBox.Show("Do you want to proceed with the cash transfers?", "DIM to DIM transfer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (resultDlg == DialogResult.No)
                        {
                            CSMTransaction.EndMultiInsertionBad();
                            nodesForProcessing.Clear();
                            underThersholdStrat.Clear();

                        }
                        else
                        {
                            if (funds.Count > 1)
                            {
                                DialogResult result = MessageBox.Show("Transfers on multiple funds are not allowed.\nPlease select only one fund!", "DIM to DIM transfer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                CSMTransaction.EndMultiInsertionBad();
                                nodesForProcessing.Clear();
                                underThersholdStrat.Clear();
                            }
                            else if (nodesForProcessing.Count != 2)
                            {
                                DialogResult result = MessageBox.Show("Only two transfer allowed under the same fund.\nNumber of transfers is " + nodesForProcessing.Count, "DIM to DIM transfer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                CSMTransaction.EndMultiInsertionBad();
                                nodesForProcessing.Clear();
                                underThersholdStrat.Clear();
                            }
                            else if (MIFLStratUsed == true)
                            {
                                DialogResult result = MessageBox.Show("Only DIM to DIM transfer allowed!", "DIM to DIM transfer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                CSMTransaction.EndMultiInsertionBad();
                                nodesForProcessing.Clear();
                                underThersholdStrat.Clear();
                            }
                            else if (sameCCy == false)
                            {
                                DialogResult result = MessageBox.Show("Transfer currencies are not matching!", "DIM to DIM transfer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                CSMTransaction.EndMultiInsertionBad();
                                nodesForProcessing.Clear();
                                underThersholdStrat.Clear();
                            }
                            else if (sameDates == false)
                            {
                                DialogResult result = MessageBox.Show("Trade dates or settlement dates are not matching!", "DIM to DIM transfer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                CSMTransaction.EndMultiInsertionBad();
                                nodesForProcessing.Clear();
                                underThersholdStrat.Clear();
                            }
                            else if (deltaAmount != 0)
                            {
                                DialogResult result = MessageBox.Show("Transfer amounts are not matching.\n Please check the difference: " + deltaAmount, "DIM to DIM transfer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                CSMTransaction.EndMultiInsertionBad();
                                nodesForProcessing.Clear();
                                underThersholdStrat.Clear();
                            }
                            else
                            {
                                if (underThersholdStrat.Count != 0)
                                {
                                    string mess = "Transferred amounts must be higher than the configured threshold value.\nPlease check the threshold setup for the folowing delegates:\n";
                                    foreach (string s in underThersholdStrat)
                                    {
                                        mess += s + "\n";
                                    }
                                    MessageBox.Show(mess, "Threshold breach!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    CSMTransaction.EndMultiInsertionBad();
                                    nodesForProcessing.Clear();
                                    underThersholdStrat.Clear();
                                }
                                else
                                {
                                    CSMTransaction.EndMultiInsertionOK(eventVector);
                                    MessageBox.Show("Transfers succesfully initiated!", "DIM to DIM transfer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    successFlag = true;
                                    nodesForProcessing.Clear();
                                    underThersholdStrat.Clear();
                                }
                            }
                        }
                    }
                    nodesForProcessing.Clear();
                    underThersholdStrat.Clear();
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return successFlag;
        }


        public static bool CreateCashTransfers(List<TreeListNode> list, int userId)
        {
            bool successFlag = false;
            try
            {
                using (CSMEventVector eventVector = new CSMEventVector())
                {
                    eventVector.ActivateNativeLifeCycle();
                    CSMTransaction.StartMultiInsertion();

                    foreach (var t in list)
                    {
                        int DIMSicovam = 0;
                        string DIMCommonIdentifier = "";
                        int DIMCashFolio = 0;
                        int DIMAccountId = 0;
                        int DIMDepositary = 0;
                        int DIMBroker = 0;

                        string mainStratName = "";
                        string mainCommonIdentifier = "";
                        string mainAccountName = "";
                        int mainCashFolioId = 0;
                        int mainSicovam = 0;
                        int mainAccountId = 0;
                        string mainPTGId = "";
                        int mainDepositary = 0;
                        

                        TreeListNode myNode = (TreeListNode)t;
                        if (myNode.Level == 1)
                        {
                            double transferAmt = 0;
                            if (myNode.GetValue("Cash Transfer") != null)
                            {
                                Double.TryParse(myNode.GetValue("Cash Transfer").ToString(), out transferAmt);
                                if (transferAmt != 0)
                                {
                                    DateTime stdDate = DateTime.Today;
                                    int tradeDate = 0;
                                    int settlementDate = 0;
                                    if (myNode.GetValue("Trade Date") != null)
                                    {
                                        if (DateTime.TryParse(myNode.GetValue("Trade Date").ToString(), out stdDate))
                                        {
                                            using (CSMDay trDate = new CSMDay(stdDate.Day, stdDate.Month, stdDate.Year))
                                            {
                                                tradeDate = trDate.toLong();
                                            }

                                        }
                                        if (DateTime.TryParse(myNode.GetValue("Settlement Date").ToString(), out stdDate))
                                        {
                                            using (CSMDay trDate = new CSMDay(stdDate.Day, stdDate.Month, stdDate.Year))
                                            {
                                                settlementDate = trDate.toLong();
                                            }
                                        }
                                    }

                                    string fundName = myNode.ParentNode.GetValue("FundName").ToString().Trim();
                                    string stratName = myNode.GetValue("FundName").ToString();
                                    int realFolioCode = CSMTransaction.solve_book_string(fundName);

                                    using (CSMPortfolio fundFolio = CSMPortfolio.GetCSRPortfolio(realFolioCode))
                                    {
                                        if (fundFolio != null)
                                        {
                                            string transferCcy = "EUR";
                                            int entityId = fundFolio.GetEntity();
                                            if (myNode.GetValue("Transfer CCY") != null)
                                            {
                                                transferCcy = myNode.GetValue("Transfer CCY").ToString();
                                            }
                                            int ccyIdent = sophis.static_data.CSMCurrency.StringToCurrency(transferCcy);

                                            if (_MIFLStrategiesMap.ContainsKey(realFolioCode))
                                            {
                                                mainStratName = _MIFLStrategiesMap[realFolioCode];
                                                mainCommonIdentifier = GetStrategyCommonIdentifier(mainStratName, realFolioCode);
                                                mainCashFolioId = GetCashFolioIdent(mainCommonIdentifier);
                                                mainSicovam = _cashItem.GetCashInstrumentSicovam(transferCcy, RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, mainCommonIdentifier, mainCommonIdentifier, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, mainCashFolioId, null, mainCommonIdentifier);
                                                mainAccountId = GetAccountIdent(mainCommonIdentifier);
                                                mainDepositary =_cashItem.GetDepositary(mainCommonIdentifier);
                                                mainPTGId = GetAccountPTG(mainCommonIdentifier);
                                                mainAccountName = GetMIFLAccountName(mainCommonIdentifier);
                                            }

                                            DIMCommonIdentifier = GetStrategyCommonIdentifier(stratName, realFolioCode);
                                            if (DIMCommonIdentifier == "")
                                            {
                                                continue;
                                            }

                                            DIMCashFolio = GetCashFolioIdent(DIMCommonIdentifier);
                                            DIMSicovam = _cashItem.GetCashInstrumentSicovam(transferCcy, RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, DIMCommonIdentifier, DIMCommonIdentifier, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, DIMCashFolio, null, DIMCommonIdentifier);
                                            DIMDepositary = _cashItem.GetDepositary(DIMCommonIdentifier);
                                         
                                            Tuple<int, int, string,string> delegatemanager = _Delegatemanagers.Find(x => (x.Item3.ToSafeString().ToUpper().CompareTo(DIMCommonIdentifier.ToUpper()) == 0));
                                            if (delegatemanager != null)
                                            {
                                                DIMAccountId = delegatemanager.Item1;
                                                DIMBroker = delegatemanager.Item2;
                                            }

                                            if (mainStratName == stratName || mainStratName == "")
                                            {                                      
                                                continue;
                                            }

                                            if (CSxAccountController._thresholdAccMap.ContainsKey(DIMAccountId))
                                            {
                                                double amount = transferAmt;
                                                int threshold = CSxAccountController._thresholdAccMap[DIMAccountId];
                                                if (transferCcy != "EUR")
                                                {
                                                    double fxRate = CSMMarketData.GetCurrentMarketData().GetForex(ccyIdent, CSxSummaryController.ccyEUR);
                                                    amount = amount * fxRate;
                                                }
                                                if (Math.Abs(amount) < threshold)
                                                {
                                                    underThersholdStrat.Add(stratName);
                                                }
                                            }
                                            using (CSMTransaction miflTrade = CSMTransaction.newCSRTransaction())
                                            using (CSMTransaction trans = CSMTransaction.newCSRTransaction())
                                            {

                                                miflTrade.SetInstrumentCode(mainSicovam);
                                                miflTrade.SetQuantity(-transferAmt);
                                                miflTrade.SetSpot(1);
                                                miflTrade.SetNetAmountOnly(-miflTrade.GetQuantity());
                                                miflTrade.SetFolioCode(mainCashFolioId);
                                                miflTrade.SetTransactionDate(tradeDate);
                                                miflTrade.SetSettlementDate(settlementDate);
                                                miflTrade.SetSettlementCurrency(ccyIdent);
                                                miflTrade.SetTransactionType((eMTransactionType)_businessEvtId);
                                                miflTrade.SetNostroCashId(mainAccountId);
                                                miflTrade.SetDepositary(mainDepositary);
                                                miflTrade.SetCounterparty(mainDepositary);
                                                miflTrade.SetEntity(entityId);
                                                miflTrade.SetTransactionTime(CSMMarketData.GetCurrentMarketData().GetTime());
                                                miflTrade.SetOperator(userId);
                                                miflTrade.DoAction(_eventId, eventVector, true);

                                                long miflTradeId = 0;
                                                miflTradeId = miflTrade.getInternalCode();
                                               
                                                trans.SetInstrumentCode(DIMSicovam);
                                                trans.SetQuantity(transferAmt);
                                                trans.SetSpot(1);
                                                trans.SetNetAmountOnly(-trans.GetQuantity());
                                                trans.SetFolioCode(DIMCashFolio);
                                                trans.SetTransactionDate(tradeDate);
                                                trans.SetSettlementDate(settlementDate);
                                                trans.SetSettlementCurrency(ccyIdent);
                                                trans.SetTransactionType((eMTransactionType)_businessEvtId);
                                                trans.SetNostroCashId(DIMAccountId);
                                                trans.SetDepositary(DIMDepositary);
                                                trans.SetCounterparty(DIMDepositary);
                                                trans.SetBroker(DIMBroker);
                                                CMString info = mainPTGId.ToString() + "|" + mainAccountName;
                                                trans.SetBackOfficeInfos(info);
                                                trans.SetComment(miflTradeId.ToString());
                                                trans.SetEntity(entityId);
                                                trans.SetOperator(userId);
                                                trans.SetTransactionTime(CSMMarketData.GetCurrentMarketData().GetTime());
                                                trans.DoAction(CSxSummaryController._eventId, eventVector, true);
                                            }

                                            
                                            nodesForProcessing.Add(myNode.Id);
                                            string ccyName = myNode.GetValue("Transfer CCY").ToString();
                                            string keyStrat = mainStratName + " " + ccyName;
                                            if (miflTransfers.ContainsKey(keyStrat))
                                            {
                                                miflTransfers[keyStrat] += -transferAmt;
                                            }
                                            else
                                            {
                                                miflTransfers.Add(keyStrat, -transferAmt);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (nodesForProcessing.Count != 0)
                    {
                        string miflMess = "";
                        if (miflTransfers.Count != 1)
                        {
                            miflMess = "Transfers on MIFL strategies:\n";
                            foreach (KeyValuePair<string, double> itm in miflTransfers)
                            {
                                int index = itm.Key.LastIndexOf(" ");
                                string ccy = itm.Key.Substring(index);
                                string stratName = itm.Key.Substring(0, index);
                                miflMess += stratName + " : " + itm.Value + " " + ccy + "\n";
                            }
                        }

                        DialogResult result = MessageBox.Show("Do you want to create " + nodesForProcessing.Count * 2 + " cash transfers?\n(" + nodesForProcessing.Count + " Subscriptions and " + nodesForProcessing.Count + " Redemptions)" + "\n" + miflMess, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.No)
                        {
                            CSMTransaction.EndMultiInsertionBad();
                            nodesForProcessing.Clear();
                            underThersholdStrat.Clear();
                            miflTransfers.Clear();
                        }
                        else
                        {
                            if (underThersholdStrat.Count != 0)
                            {
                                string mess = "Transferred amounts must be higher than the configured threshold value.\nPlease check the threshold setup for the folowing delegates:\n";
                                foreach (string s in underThersholdStrat)
                                {
                                    mess += s + "\n";
                                }
                                MessageBox.Show(mess, "Threshold breach!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                CSMTransaction.EndMultiInsertionBad();
                                nodesForProcessing.Clear();
                                underThersholdStrat.Clear();
                                miflTransfers.Clear();
                            }
                            else
                            {
                                CSMTransaction.EndMultiInsertionOK(eventVector);
                                MessageBox.Show("Transfers succesfully initiated!", "Manager to Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                successFlag = true;

                                nodesForProcessing.Clear();
                                underThersholdStrat.Clear();
                                miflTransfers.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(_ClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Error occurred : " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }

            return successFlag;
        }
    }
}

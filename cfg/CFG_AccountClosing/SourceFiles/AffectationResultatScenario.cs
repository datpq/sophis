/*
** Includes
*/
using System;
using sophis;
using sophis.scenario;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using sophis.value;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using sophis.market_data;
using sophisTools;
using sophis.misc;
using sophis.tools;
using sophis.utils;
using System.Windows.Forms;
using sophis.gui;


namespace CFG_AccountClosing.SourceFiles
{
	/// <summary>
    /// This class derived from <c>sophis.portfolio.CSMScenario</c> can be overloaded to create a new scenario
	/// </summary>
	public class AffectationResultatScenario : sophis.scenario.CSMScenario
	{
        /// <summary>This method specifies the context in which it can be launched.</summary>
        /// <returns> Returns the type of the scenario. It returns <c>sophis.scenario.eMProcessingType.M_pScenario</c> by default.</returns>
        /// <remarks>
        /// <para>The various types of scenarios are: </para>
        /// <para>M_pScenario: Will be available through the Analysis menu when the porfolio is launched or when an instrument is opened.</para>
        /// <para>M_pManagerPreference: Will be available through the Manager menu without any condition.</para>
        /// <para>M_pUserPreference: Will be available through the User menu (or the Manager menu if available) without any condition.</para>
        /// <para>M_pInstrument: Will be available through the Data menu when an instrument is opened or selected.</para>
        /// <para>M_pCounterparty: Will be available through the Data menu when a third party is opened or selected.</para>
        /// <para>M_pPortfolio: Will be available through the Data menu when a folio is selected.</para>
        /// <para>M_pBeforeEndOfDayProcedure: Will be launched automatically before the end of day procedure.</para>
        /// <para>M_pAfterEndOfDayProcedure: Will be launched automatically after the end of day procedure.</para>
        /// <para>M_pNightBatch: Will be launched automatically in the night batch.</para>
        /// <para>M_pBeforeReporting: Will be launched automatically before every reporting.</para>
        /// <para>M_pAfterReporting: Will be launched automatically after every reporting.</para>
        /// <para>M_pMarketData: </para>
        /// <para>M_pData: </para>
        /// <para>M_pEndOfDayConditionnal: Add to the end of day in a conditional form.</para>
        /// <para>M_pAfterAllInitialisation: Will be executed after all initialiation.</para>
        /// <para>M_pOther: Will be added in the prototype but never used. May be used for a scenario on Calculation server.</para>
        /// <para>M_pMultiSiteEODBeforePortfolioLoading: Will be executed before the portfolio loading during a MultiSite End Of Day</para>
        /// <para>M_pAccounting: Will be available through the Accounting menu without any condition.</para>
        /// <para>M_pBalanceEngineBeforePnL: </para>
        /// <para>M_pBalanceEngineAfterPnL: </para>
        /// <para>M_pPNLEngine: </para>
        /// <para>M_pAuxiliaryLedger: </para>
        /// <para>M_pSendToGL: </para>
        /// </remarks>
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pUserPreference;
        }

        /// <summary>To do all your initialisation. Typically, it may open a GUI to get data from the user.</summary>
        public override void Initialise()
        {
            /// Add your code here
        }
        public static int GetAffectationPostingType()
        {
            int postingType = 0;
             
            using (OracleCommand myCommand = DBContext.Connection.CreateCommand())
            {
                myCommand.CommandText = "select id from account_posting_types where name='Distribution'";//A CREER DANS VALUE
                Object postingTypeObj = myCommand.ExecuteScalar();
                if (postingTypeObj != null)
                {
                    int.TryParse(postingTypeObj.ToString(), out  postingType);
                }
                else
                {
                    MessageBox.Show("Please, Create 'Distribution' posting type");
                }
            }
                             
            return postingType;
        }

        /// <summary>To run your scenario. this method is mandatory otherwise RISQUE will not do anything.</summary>
        public override void Run()
        {
            try
            {
                CFG_AccountClosing.SourceFiles.AffectationManager theManager = new AffectationManager();                
                WinformContainer.DoDialog(theManager, WinformContainer.eDialogMode.eModal);
            }            
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
                CSMLog.Write("AffectationResultatScenario", "Run", CSMLog.eMVerbosity.M_error, e.ToString());
            }                                         
        }

        /// <summary>Free initiliased memory after scenario is processed.</summary>
        public override void Done()
        {
            /// Add your code here
        }

        public static int[] GetAccountListAffectation()
        {
            int[] results = null;
            string ListAccounts = "";
            string[] ListA;
            sophis.misc.CSMConfigurationFile.getEntryValue("CFG_AccountingClosing", "AffectationAccountsList", ref ListAccounts, "0");
            ListA = ListAccounts.Split(',');
            results = new int[ListA.Length];
            for (int i = 0; i < ListA.Length; i++)
            {
                int.TryParse(ListA[i], out results[i]);
            }
            return results;
        }     

        public static DataTable GetLastAccountingDateofAllfunds()
        {
            DataTable Results = new DataTable();
            Results.Columns.Add("ID Entite", typeof(Int32));
            Results.Columns.Add("Last Accounting Date", typeof(Int32));
            try
            {
                using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                {
                    cmd.CommandText = "select E.id, max(date_to_num(P.posting_date)) from account_entity E, account_posting P where  E.ID = P.account_entity_id and E.record_type=1 and E.name not like 'REFERENCE%%' and P.rule_type in (2,3) and P.status in (2,4) group by E.id";
                    using (OracleDataReader MyReader = cmd.ExecuteReader())
                    {
                        while (MyReader.Read())
                        {
                            DataRow Row = Results.NewRow();
                            string id = MyReader[0].ToString();
                            Int32 ID = 0;
                            Int32.TryParse(id, out ID);
                            string date = MyReader[1].ToString();
                            Int32 DATE = 0;
                            Int32.TryParse(date, out DATE);
                            Row["ID Entite"] = ID;
                            Row["Last Accounting Date"] = DATE;
                            Results.Rows.Add(Row);
                        }                        
                    }                    
                }
            }
            catch (Exception excep)
            {
                System.Windows.Forms.MessageBox.Show(excep.ToString());
            }
            return Results;
        }

        public static DateTime GetLastAccountingDate(sophis.utils.CMString Name, DataTable dates)
        {
            int nb = dates.Rows.Count;
            Int32 Date = 0;
            bool found = false;
            try
            {
                for (int i = 0; i < nb; i++)
                {
                    using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "select name from account_entity where id=:0";
                        cmd.Parameters.Add(":0", OracleDbType.Int32);
                        cmd.Parameters[":0"].Value = (Int32)dates.Rows[i]["ID Entite"];
                        using (OracleDataReader Myreader = cmd.ExecuteReader())
                        {
                            while (Myreader.Read())
                            {
                                sophis.utils.CMString FName = Myreader[0].ToString();
                                if (Name == FName)
                                {
                                    found = true;
                                    Date = (Int32)dates.Rows[i]["Last Accounting Date"];
                                }
                            }                            
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
            if (found)
            {
                CSMDay datelast = new CSMDay(Date);
                return new DateTime(datelast.fYear, datelast.fMonth, datelast.fDay);
            }
            else
                return new DateTime(1904, 1, 1);

        }

        public static void ProceedToAffectation(int[] SicoFonds)
        {
            CSMLog ALogger = new CSMLog();
            ALogger.Begin("AffectationResultatScenario", "ProceedToAffectation");
            //Getting the list of funds to proceed
            int[] FundsList = SicoFonds;

            DataTable AccountingDates = GetLastAccountingDateofAllfunds();

            for (int i = 0; i < FundsList.Length; i++)
            {

                //retrieving Fund Name and closing date information
                CSMAmFund internFund = CSMAmFund.GetInstance(FundsList[i]);
                if (internFund == null)
                {
                    string mess = "Internal Fund with sicovam " + FundsList[i] + "not found";
                    ALogger.Write(CSMLog.eMVerbosity.M_debug, mess);
                    // System.Windows.Forms.MessageBox.Show(mess);
                    continue;
                }

                sophis.utils.CMString Fundname = "";
                //DPH
                Fundname = internFund.GetName();
                //internFund.GetName(Fundname, 128);
                DateTime Date = DateTime.Today;

                DistributionDatePicker DatePicker = new DistributionDatePicker(Fundname);                
                WinformContainer.DoDialog(DatePicker, WinformContainer.eDialogMode.eModal);
                DateTime DistribDate = DatePicker.GetDistributionDate();
                CSMDay distribDateDay = new CSMDay(DistribDate.ToString("yyyyMMdd"));
                //int[]ComptesASolder = new int[5] {180,1702,160,1601,1608};
                //Get the list of accounts to settle from ini file
                string listOfAccountsToSettleStr = "";
                CSMConfigurationFile.getEntryValue("CFG_AccountingClosing", "AccountsToSettle", ref listOfAccountsToSettleStr, "180,1702,160,1601,1608,17011");
                string[] listOfAccountsToSettleStrTable = listOfAccountsToSettleStr.Split(',');
                List<int> ComptesASolder = new List<int>();

                foreach (string accountStr in listOfAccountsToSettleStrTable)
                {
                    int accountToSettle = 0;
                    if (int.TryParse(accountStr, out accountToSettle))
                    {
                        ComptesASolder.Add(accountToSettle);
                    }                    
                }
                
                int[] ComptesACrediter = new int[] { 430,111,1601,1608};
                
                DateTime LastAccountingDate = GetLastAccountingDate(Fundname, AccountingDates);

                DataTable AffectationAccounts = new DataTable();
                AffectationAccounts.Columns.Add("Account Name", typeof(string));
                AffectationAccounts.Columns.Add("AccountNumber", typeof(int));
                AffectationAccounts.Columns.Add("Balance before Distribution", typeof(double));
                AffectationAccounts.Columns.Add("Balance after Distribution", typeof(double));
                AffectationAccounts.Columns.Add("AccountType", typeof(string));                

                //Retrieving info for accounts to be settled.
                for (int j = 0; j < ComptesASolder.Count; j++)
                {                   
                    DataRow theRow = AffectationAccounts.NewRow();
                    theRow["AccountNumber"] = ComptesASolder[j];
                    theRow["Balance after Distribution"] = 0;
                    theRow["AccountType"] = "To settle";
                    try
                    {
                        using (OracleCommand NameCommand = DBContext.Connection.CreateCommand())
                        {
                            NameCommand.CommandText = "select a.name from account_name a  where a.id in (select account_name_id from account_map where account_number=:0)";
                            NameCommand.Parameters.Add(":0", OracleDbType.Varchar2);
                            NameCommand.Parameters[":0"].Value = ComptesASolder[j].ToString();

                            theRow["Account Name"] = "";

                            Object accountNameObj = NameCommand.ExecuteScalar();
                            if (accountNameObj != null)
                            {
                                theRow["Account Name"] = accountNameObj.ToString();
                            }                                                        
                        }
                        
                        using (OracleCommand AmountCommand = DBContext.Connection.CreateCommand())
                        {
                            theRow["Balance before Distribution"] = 0;
                            AmountCommand.CommandText = "SELECT SUM(DECODE(a.CREDIT_DEBIT,'C',1,-1) * a.AMOUNT) from account_posting a where a.account_entity_ID in (select ID from account_entity where name = :0) and a.posting_date <=num_to_date(:1) and a.account_number = :2";
                            AmountCommand.Parameters.Add(":0", OracleDbType.Varchar2);
                            AmountCommand.Parameters.Add(":1", OracleDbType.Int32);
                            AmountCommand.Parameters.Add(":2", OracleDbType.Varchar2);
                            AmountCommand.Parameters[":0"].Value = Fundname;
                            AmountCommand.Parameters[":1"].Value = distribDateDay.toLong();
                            AmountCommand.Parameters[":2"].Value = ComptesASolder[j].ToString();
                            using (OracleDataReader reader = AmountCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    double quantity = 0;
                                    string res = reader[0].ToString();
                                    double.TryParse(res, out quantity);
                                    theRow["Balance before Distribution"] = Math.Round(-quantity, 2);
                                }                                
                            }                            
                        }
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }
                    AffectationAccounts.Rows.Add(theRow);
                }
                //Doing the same for accounts to be distributed.

                for (int j = 0; j < ComptesACrediter.Length; j++)
                {
                    DataRow theRow = AffectationAccounts.NewRow();
                    theRow["AccountNumber"] = ComptesACrediter[j];
                    theRow["Balance after Distribution"] = 0;
                    theRow["AccountType"] = "To credit";
                    try
                    {
                        using (OracleCommand NameCommand = DBContext.Connection.CreateCommand())
                        {
                            NameCommand.CommandText = "select a.name from account_name a  where a.id in (select account_name_id from account_map where account_number=:0)";
                            NameCommand.Parameters.Add(":0", OracleDbType.Varchar2);
                            NameCommand.Parameters[":0"].Value = ComptesACrediter[j].ToString();
                            using (OracleDataReader reader = NameCommand.ExecuteReader())
                            {
                                theRow["Account Name"] = "";
                                while (reader.Read())
                                {
                                    theRow["Account Name"] = reader[0];
                                }                                
                            }                            
                        }
                        if (j < 2)
                        {
                            using (OracleCommand AmountCommand = DBContext.Connection.CreateCommand())
                            {
                                theRow["Balance before Distribution"] = 0;
                                AmountCommand.CommandText = "SELECT SUM(DECODE(a.CREDIT_DEBIT,'C',1,-1) * a.AMOUNT) from account_posting a where a.account_entity_ID in (select ID from account_entity where name = :0) and a.posting_date <= num_to_date(:1) and a.account_number = :2";
                                AmountCommand.Parameters.Add(":0", OracleDbType.Varchar2);
                                AmountCommand.Parameters.Add(":1", OracleDbType.Int32);
                                AmountCommand.Parameters.Add(":2", OracleDbType.Varchar2);
                                AmountCommand.Parameters[":0"].Value = Fundname;
                                AmountCommand.Parameters[":1"].Value = distribDateDay.toLong();
                                AmountCommand.Parameters[":2"].Value = ComptesACrediter[j].ToString();
                                using (OracleDataReader reader = AmountCommand.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        double quantity = 0;
                                        string res = reader[0].ToString();
                                        double.TryParse(res, out quantity);
                                        theRow["Balance before Distribution"] = Math.Round(-quantity, 2);
                                        theRow["Balance after Distribution"] = Math.Round(-quantity, 2);
                                    }                                    
                                }                                
                            }
                        }
                        else
                        {
                            theRow["Balance before Distribution"] = 0;
                        }

                        if (j == 3)
                        {
                            double total = 0;
                            for (int u = 0; u < ComptesASolder.Count; u++)
                            {
                                total += (double)(double)AffectationAccounts.Rows[u]["Balance before Distribution"];
                            }
                            theRow["Balance after Distribution"] = Math.Round(total);
                        }

                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }
                    AffectationAccounts.Rows.Add(theRow);
                }                               

                CFG_AccountClosing.SourceFiles.InterfaceAffectationResultat InterfaceAffectation = new InterfaceAffectationResultat(AffectationAccounts, internFund, DistribDate);                
                WinformContainer.DoDialog(InterfaceAffectation, WinformContainer.eDialogMode.eModal);

                if (InterfaceAffectation.GetSkippedAll())
                    break;
            }
            ALogger.End();

        }

    }
}

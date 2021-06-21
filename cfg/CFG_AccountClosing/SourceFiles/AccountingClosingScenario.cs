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
using sophis.static_data;
using sophis.gui;
using System.Windows.Forms;


namespace CFG_AccountClosing.SourceFiles
{
    ///    //For closing and opening accounts(account posting insertion)
    public struct AccountPostingInfo
    {
        public int _accountNameId;
      
        public int _accountEntityId;
       

        public AccountPostingInfo(int accountNameId,  int accountEntityId)
        {
            _accountNameId = accountNameId;         
            _accountEntityId = accountEntityId;          
        }
    }

	/// <summary>
    /// This class derived from <c>sophis.portfolio.CSMScenario</c> can be overloaded to create a new scenario
	/// </summary>
	public class AccountingClosingScenario : sophis.scenario.CSMScenario
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
        ///   static Dictionary<string, AccountPostingInfo> accountInfoList;
        ///   
        /// 
        private CSMLog _logger = new CSMLog();
        public static Dictionary<string,int> accountInfoList;
        public static Dictionary<string,int> accountEntityInfoList;
        public static void InitializeAccountPostingInfoForAllAccounts()
        {
           accountInfoList = new Dictionary<string, int>();

           string query = "select ACCOUNT_NUMBER, ACCOUNT_NAME_ID from ACCOUNT_MAP where  ACCOUNT_NAME_ID in (select ID from ACCOUNT_NAME where RECORD_TYPE=1)";

           using (OracleCommand Command = DBContext.Connection.CreateCommand())
           {
               Command.CommandText = query;
               using (OracleDataReader reader = Command.ExecuteReader())
               {
                   while (reader.Read())
                   {
                       int _accountNameId = 0;
                       string accountNumber = reader[0].ToString();
                       string testReader = reader[1].ToString();
                       Int32.TryParse(testReader, out _accountNameId);

                       try
                       {
                           if (!accountInfoList.ContainsKey(accountNumber))

                               accountInfoList.Add(accountNumber, _accountNameId);
                       }
                       catch (Exception e)
                       {
                           System.Windows.Forms.MessageBox.Show(e.ToString());
                       }

                   }                   
               }               
           }
       }

       public static void InitializeAccountEntitiesInfoForAllFunds()
       {
            accountEntityInfoList=new Dictionary<string, int>();
          

            string query = "select name, id from account_entity where record_type =1";
            using (OracleCommand Command = DBContext.Connection.CreateCommand())
            {
                Command.CommandText = query;
                using (OracleDataReader reader = Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int _accountEntityId = 0;
                        string fundName = reader[0].ToString();
                        string testReader = reader[1].ToString();
                        Int32.TryParse(testReader, out _accountEntityId);

                        try
                        {
                            accountEntityInfoList.Add(fundName, _accountEntityId);
                        }
                        catch (Exception e)
                        {
                            System.Windows.Forms.MessageBox.Show(e.ToString());
                        }

                    }                    
                }                
            }
        }

        public static int GetAccountPostingInfoForAccount(string accountNumber)
        {
            foreach (KeyValuePair<string, int> entry in accountInfoList)
            {
                if (entry.Key.ToString() == accountNumber)
                {
                    return (int)entry.Value;
                }
             }
           return 0;
         }
             
        public static int GetAccountAccountEntityInfoForFund(string fundName)
        {
            foreach (KeyValuePair<string, int> entry in accountEntityInfoList)
            {
                if (entry.Key.ToString() == fundName)
                {
                    return (int)entry.Value;
                }
            }
            return 0;
        }
        
        public static int GetClosingPostingType()
        {
            int postingType = 0;
            
            using (OracleCommand myCommand = DBContext.Connection.CreateCommand())
            {
                myCommand.CommandText = "select id from account_posting_types where name='Closing'";
                using (OracleDataReader reader1 = myCommand.ExecuteReader())
                {
                    try
                    {
                        reader1.Read();
                        string testReader = reader1[0].ToString();
                        Int32.TryParse(testReader, out  postingType);
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString() + "Please, Create 'Closing' posting type");
                    }                    
                }
            }            
            
            return postingType;
        }
        
        public static int GetCFGCurrency()
        {
            int currency = CSMCurrency.StringToCurrency("MAD");         
                                    
            return currency;
        }
        
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pUserPreference;
        }

        /// <summary>To do all your initialisation. Typically, it may open a GUI to get data from the user.</summary>
        public override void Initialise()
        {
            /// Add your code here
        }

        /// <summary>To run your scenario. this method is mandatory otherwise RISQUE will not do anything.</summary>
        public override void Run()
        {
            //_logger.Begin("AccountClosingScenario", "Run");
            try
            {
                CFG_AccountClosing.SourceFiles.ClosingManager theManager = new ClosingManager();
                WinformContainer.DoDialog(theManager, WinformContainer.eDialogMode.eModal);
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
                CSMLog.Write("AccountingClosingScenario", "Run", CSMLog.eMVerbosity.M_error, e.ToString());
            }
            
                                            
            //_logger.End();
        }                     

        //Getting the list of Accounts to proceed for each fund
        public static int[] GetAccountListClosing()
        {                      
            int[] results = null;
            string ListAccounts = "";
            string[] ListA;
            sophis.misc.CSMConfigurationFile.getEntryValue("CFG_AccountingClosing", "ClosingAccountsList", ref ListAccounts, "0");
            ListA = ListAccounts.Split(',');
            results = new int[ListA.Length];
            for (int i = 0; i < ListA.Length; i++)
            {
                int.TryParse(ListA[i], out results[i]);
            }
            return results;
        }

       //Getting the last Accounting date of all funds
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

        //Getting the last accounting date of a specific fund
        public static DateTime GetLastAccountingDate(string Name, DataTable dates)
        {
            int nb = dates.Rows.Count;
            Int32 Date = 0;
            bool found = false;
            try
            {               
                for (int i = 0; i < nb;i++ )
                {                   
                    using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText =   "select name from account_entity where id=:0";
                        cmd.Parameters.Add(":0", OracleDbType.Int32);
                        cmd.Parameters[":0"].Value = (Int32)dates.Rows[i]["ID Entite"];
                        using (OracleDataReader Myreader = cmd.ExecuteReader())
                        {
                            while (Myreader.Read())
                            {
                                string FName = Myreader[0].ToString();
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

        public static void ProceedToClosing(int[] SicoFonds)
        {
            CSMLog Alogger = new CSMLog();
            Alogger.Begin("AccountClosingScenario", "ProceedToClosing");
            int[] FundsList = SicoFonds;           

            //Getting the last accounting date of all funds
            DataTable AccountingDates = GetLastAccountingDateofAllfunds();
           
            // Creating the list of funds for alarms
            List<string> AlarmesFond = new List<string>();            

            // beginning of the processing for each fund closing

            for (int i = 0; i < FundsList.Length; i++)
            {
                //retrieving Fund Name 
                CSMAmFund internFund = CSMAmFund.GetInstance(FundsList[i]);
                if (internFund == null)
                {
                    string mess = "Internal Fund with sicovam " + FundsList[i] + " not found";
                    Alogger.Write(CSMLog.eMVerbosity.M_debug,mess);                   
                    
                    continue;
                }
                sophis.utils.CMString Fundname="";
                //DPH
                Fundname = internFund.GetName();
                //internFund.GetName(Fundname, 128);
                string theName = (string)Fundname;


                // Retrieving Closing Date, Nav date and last accounting date for the fund
                System.Collections.ArrayList Indicators = internFund.GetUserIndicators();
                string Date="";
                DateTime ClosingDate=DateTime.Today;
                foreach (SSMUserIndicator Indic in Indicators)
                {
                    if (Indic.name == "Next Closing Date")
                        Date = Indic.comment;
                }
                if (Date == "")
                {
                    string mess = "Next Closing Date Not Found for the Fund " + Fundname;
                    Alogger.Write(CSMLog.eMVerbosity.M_debug, mess);
                    System.Windows.Forms.MessageBox.Show(mess);
                   
                    continue;
                }
                else
                    DateTime.TryParse(Date, out ClosingDate);

                DateTime LastAccountingDate = GetLastAccountingDate(theName,AccountingDates);
                LastAccountingDate=LastAccountingDate.AddHours(-LastAccountingDate.Hour);
                LastAccountingDate = LastAccountingDate.AddMinutes(-LastAccountingDate.Minute);
                LastAccountingDate= LastAccountingDate.AddSeconds(-LastAccountingDate.Second);
                LastAccountingDate= LastAccountingDate.AddMilliseconds(-LastAccountingDate.Millisecond);
               
                ClosingDate = ClosingDate.AddHours(-ClosingDate.Hour);
                ClosingDate = ClosingDate.AddMinutes(-ClosingDate.Minute);
                ClosingDate = ClosingDate.AddSeconds(-ClosingDate.Second);
                ClosingDate = ClosingDate.AddMilliseconds(-ClosingDate.Millisecond);
                
                Alogger.Write(CSMLog.eMVerbosity.M_debug, "Last Accounting Date for Fund " + Fundname + " = " + LastAccountingDate.ToLongDateString() + " " + LastAccountingDate.ToLongTimeString());
                Alogger.Write(CSMLog.eMVerbosity.M_debug, "Closing Date for Fund " + Fundname + " = " + ClosingDate.ToLongDateString() + " " + ClosingDate.ToLongTimeString());
                
                int NextNavDate=internFund.GetNAVDateAfter(sophis.market_data.CSMMarketData.GetCurrentMarketData().GetDate());
                CSMDay DayNextNavDate = new CSMDay(NextNavDate);
                DateTime NavDate = new DateTime(DayNextNavDate.fYear, DayNextNavDate.fMonth, DayNextNavDate.fDay);


                Alogger.Write(CSMLog.eMVerbosity.M_debug, "navDate for fund "+Fundname+" : "+NavDate.ToShortDateString());                

                // Creating datatables for accounts information
                int[] AccountsClosingList = GetAccountListClosing();

                DataTable ClosingAccounts = new DataTable();
                ClosingAccounts.Columns.Add("Account Name",typeof(string));
                ClosingAccounts.Columns.Add("Account Number", typeof(int));
                ClosingAccounts.Columns.Add("Balance before closing", typeof(double));
                ClosingAccounts.Columns.Add("Balance after closing", typeof(double));
                DataTable Compte180 = new DataTable();
                Compte180.Columns.Add("Account Name", typeof(string));
                Compte180.Columns.Add("Account Number", typeof(int));
                Compte180.Columns.Add("Balance before closing", typeof(double));
                Compte180.Columns.Add("Balance after closing", typeof(double));


                //and then getting information for each account of the fund
                //first for accounts that must be settled

                bool add = true;
                for (int j = 0; j < AccountsClosingList.Length; j++)
                {
                    add = true;
                    DataRow theRow = ClosingAccounts.NewRow();
                    theRow["Account Number"] = AccountsClosingList[j];
                    theRow["Balance after closing"] = 0;
                    
                    try
                    {

                        using (OracleCommand NameCommand = DBContext.Connection.CreateCommand())
                        {
                            NameCommand.CommandText = "select a.name from account_name a  where a.id in (select account_name_id from account_map where account_number=:0)";
                            NameCommand.Parameters.Add(":0", OracleDbType.Varchar2);
                            NameCommand.Parameters[":0"].Value = AccountsClosingList[j].ToString();
                            using (OracleDataReader reader = NameCommand.ExecuteReader())
                            {
                                theRow["Account Name"] = "";
                                while (reader.Read())
                                {
                                    theRow["Account Name"] = reader[0];
                                }                                
                            }                            
                        }
                        using (OracleCommand AmountCommand = DBContext.Connection.CreateCommand())
                        {
                            theRow["Balance before closing"] = 0;
                            AmountCommand.CommandText = "SELECT SUM(DECODE(a.CREDIT_DEBIT,'C',1,-1) * a.AMOUNT) from account_posting a where a.account_entity_ID in (select ID from account_entity where name = :0) and a.posting_date <=to_date(:1) and a.account_number = :2";
                            AmountCommand.Parameters.Add(":0", OracleDbType.Varchar2);
                            AmountCommand.Parameters.Add(":1", OracleDbType.Varchar2);
                            AmountCommand.Parameters.Add(":2", OracleDbType.Varchar2);
                            AmountCommand.Parameters[":0"].Value = Fundname;
                            AmountCommand.Parameters[":1"].Value = Date;
                            AmountCommand.Parameters[":2"].Value = AccountsClosingList[j].ToString();
                            using (OracleDataReader reader = AmountCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    double quantity = 0;
                                    string res = reader[0].ToString();
                                    double.TryParse(res, out quantity);
                                    theRow["Balance before closing"] = quantity;
                                    if (quantity == 0.0)
                                        add = false;
                                }                                
                            }                            
                        }
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }
                    if(add)
                         ClosingAccounts.Rows.Add(theRow);
                }
                Alogger.Write(CSMLog.eMVerbosity.M_debug, "Info from accounts to settle for fund " +Fundname+" retrieved.");                
                
                // And then for account 180, that receives the balance of previous settled accounts

                DataRow Compte180Row=Compte180.NewRow();
                try
                {
                    using (OracleCommand AmountCommand = DBContext.Connection.CreateCommand())
                    {
                        Compte180Row["Balance before closing"] = 0;
                        AmountCommand.CommandText = "SELECT SUM(DECODE(a.CREDIT_DEBIT,'C',1,-1) * a.AMOUNT) from account_posting a where a.posting_date <=to_date(:0) and a.account_number = :1";
                        AmountCommand.CommandText = "SELECT SUM(DECODE(a.CREDIT_DEBIT,'C',1,-1) * a.AMOUNT) from account_posting a where a.account_entity_ID in (select ID from account_entity where name = :0) and a.posting_date <=to_date(:1) and a.account_number = :2";
                        AmountCommand.Parameters.Add(":0", OracleDbType.Varchar2);
                        AmountCommand.Parameters.Add(":1", OracleDbType.Varchar2);
                        AmountCommand.Parameters.Add(":2", OracleDbType.Varchar2);
                        AmountCommand.Parameters[":0"].Value = Fundname;
                        AmountCommand.Parameters[":1"].Value = Date;
                        AmountCommand.Parameters[":2"].Value = "180";
                        using (OracleDataReader reader = AmountCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                double quantity = 0;
                                string res = reader[0].ToString();
                                double.TryParse(res, out quantity);
                                Compte180Row["Balance before closing"] = quantity;
                            }                            
                        }
                        
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }
                Compte180Row["Balance after closing"] = 0;
                double totalSolde = 0.0;
                for (int k = 0; k <ClosingAccounts.Rows.Count; k++)
                {
                    totalSolde -= (double)ClosingAccounts.Rows[k]["Balance before closing"];
                }
                Compte180Row["Balance after closing"] = totalSolde + (double)Compte180Row["Balance before closing"];
                Compte180Row["Account Number"]=180;
                Compte180Row["Account Name"] = "RESULTAT EN INSTANCE D'AFFECTATION";
                Compte180.Rows.Add(Compte180Row);
                Alogger.Write(CSMLog.eMVerbosity.M_debug, "Information from account 180 retrieved.");                

                //once each datatable is filled, we can show the dialog

                CFG_AccountClosing.SourceFiles.InterfaceFermeture InterfaceClosing = new InterfaceFermeture(ClosingAccounts, Compte180, internFund, LastAccountingDate, ClosingDate);
                WinformContainer.DoDialog(InterfaceClosing,WinformContainer.eDialogMode.eModal);
                if (InterfaceClosing.GetSkippedAll())
                    break;
            
        }
          
        Alogger.End();        
    }
  }
}

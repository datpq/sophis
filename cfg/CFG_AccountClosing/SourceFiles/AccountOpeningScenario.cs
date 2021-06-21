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
using System.Globalization;
using System.Threading;
using System.Data;
using sophis.market_data;
using sophisTools;
using sophis.misc;
using sophis.tools;
using sophis.utils;
using sophis.static_data;
using System.Windows.Forms;
using sophis.gui;


namespace CFG_AccountClosing.SourceFiles
{
	/// <summary>
    /// This class derived from <c>sophis.portfolio.CSMScenario</c> can be overloaded to create a new scenario
	/// </summary>
	public class AccountOpeningScenario : sophis.scenario.CSMScenario
	{
        /// <summary>This method specifies the context in which it can be launched.</summary>
        /// <returns> Returns the type of the scenario. It returns <c>sophis.scenario.eMProcessingType.M_pScenario</c> by default.</returns>
        /// <remarks>
        /// <para>The various types of scenarios are: </para>
        /// <para>M_pScenario: Will be available through the Analysis menu when the portfolio is launched or when an instrument is opened.</para>
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
        /// <para>M_pAfterAllInitialisation: Will be executed after all initialization.</para>
        /// <para>M_pOther: Will be added in the prototype but never used. May be used for a scenario on Calculation server.</para>
        /// <para>M_pMultiSiteEODBeforePortfolioLoading: Will be executed before the portfolio loading during a MultiSite End Of Day</para>
        /// <para>M_pAccounting: Will be available through the Accounting menu without any condition.</para>
        /// <para>M_pBalanceEngineBeforePnL: </para>
        /// <para>M_pBalanceEngineAfterPnL: </para>
        /// <para>M_pPNLEngine: </para>
        /// <para>M_pAuxiliaryLedger: </para>
        /// <para>M_pSendToGL: </para>
        /// </remarks>
        /// 
        private CSMLog _logger = new CSMLog();
        public static int GetOpenPostingType()
        {
            int postingType = 0;
             
            using (OracleCommand myCommand = DBContext.Connection.CreateCommand())
            {
                myCommand.CommandText = "select id from account_posting_types where name='Opening'";//A CREER DANS VALUE
                Object idObj = myCommand.ExecuteScalar();

                if (idObj != null)
                {
                    int.TryParse(idObj.ToString(), out  postingType);
                }
                else
                {
                    MessageBox.Show("Please, Create 'Opening' posting type");
                }
            }                        
            
            return postingType;
        }
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pUserPreference;
        }

        /// <summary>To do all your initialization. Typically, it may open a GUI to get data from the user.</summary>
        public override void Initialise()
        {
            /// Add your code here
        }

        /// <summary>To run your scenario. this method is mandatory otherwise RISQUE will not do anything.</summary>
        public override void Run()
        {
            _logger.Begin("AccountOpeningScenario", "Run");

            try
            {
                CFG_AccountClosing.SourceFiles.OpeningManager theManager = new OpeningManager();
                WinformContainer.DoDialog(theManager, WinformContainer.eDialogMode.eModal);
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
                CSMLog.Write("AccountOpeningScenario", "Run", CSMLog.eMVerbosity.M_error, e.ToString());
            }            
          
            _logger.End();
        }

        /// <summary>Free initialized memory after scenario is processed.</summary>
        public override void Done()
        {
            /// Add your code here
        }

        

        public static int[] GetAccountListOpening()
        {
            int[] results = null;
            string ListAccounts = "";
            string[] ListA;
            sophis.misc.CSMConfigurationFile.getEntryValue("CFG_AccountingClosing", "OpeningAccountsList", ref ListAccounts, "0");
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

        public static DateTime GetLastAccountingDate(string Name, DataTable dates)
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

        public static void ProceedToOpening(int[] SicoFonds)
        {
            CSMLog ALogger = new CSMLog();
            ALogger.Begin("AccountOpeningScenario", "ProceedToOpening");
            //Getting the list of funds to proceed
            int[] FundsList = SicoFonds;

            //getting last account date for all funds
            DataTable AccountingDates = GetLastAccountingDateofAllfunds();

            //creating alarms structures
            List<string> AlarmesFond = new List<string>();
            string[] AlarmesFondArray;

            
            //and then proceeding to the opening of each fund
            for (int i = 0; i < FundsList.Length; i++)
            {

                //retrieving Fund Name and closing date information
                CSMAmFund internFund = CSMAmFund.GetInstance(FundsList[i]);
                if (internFund == null)
                {
                    string mess = "Internal Fund with sicovam " + FundsList[i] + " not found";
                    ALogger.Write(CSMLog.eMVerbosity.M_debug, mess);
                    // System.Windows.Forms.MessageBox.Show(mess);
                   
                    continue;
                }

                //retrieving fund name
                sophis.utils.CMString Fundname = "";
                //DPH
                Fundname = internFund.GetName();
                //internFund.GetName(Fundname, 128);
                string AName = (string)Fundname;

                //retrieving fund's nav date, opening and closing date
                System.Collections.ArrayList Indicators = internFund.GetUserIndicators();
                string Date = "";
                string DateC = "";
                DateTime OpeningDate = DateTime.Today;
                DateTime Closing_Date = DateTime.Today;
                foreach (SSMUserIndicator Indic in Indicators)
                {
                    //if (Indic.name == "Next Opening Date")
                    //    Date = Indic.comment;
                    if (Indic.name == "Next Closing Date")
                        DateC = Indic.comment;
                }


                using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                {
                    cmd.CommandText = "select max(a.posting_date) from account_posting a, titres t, account_entity b where posting_type in (select id from account_posting_types where name = 'Closing') and a.account_entity_id =b.id and t.libelle=b.name and t.sicovam=:0";
                    cmd.Parameters.Add(":0", FundsList[i]);

                    Object postingDateObj = cmd.ExecuteScalar();

                    if (postingDateObj != null)
                    {
                        Date = postingDateObj.ToString();
                    }                    
                }

                // [CR] 2012/12/10 v1.0.5.2
                DateTime DateTest = new DateTime();
                DateTime.TryParse(Date, out DateTest);

                // Force the date format to DD/MM/YYYY
                CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                Date = DateTest.AddDays(-1).ToShortDateString();
                //Date = Date.ToShortDateString();

                // Set back the date format
                Thread.CurrentThread.CurrentCulture = originalCulture;


                //if (Date == "")
                //{
                //    string mess = "Next Opening Date Not Found for the Fund " + Fundname;
                //    System.Windows.Forms.MessageBox.Show(mess);
                //    ALogger.Write(CSMLog.eMVerbosity.M_debug, mess);
                    
                //    continue;
                //}
                //else
                //    DateTime.TryParse(Date, out OpeningDate);
                if (DateC == "")
                {
                    string mess = "Next Closing Date Not Found for the Fund " + Fundname;
                    System.Windows.Forms.MessageBox.Show(mess);
                    ALogger.Write(CSMLog.eMVerbosity.M_debug, mess);
                    
                    continue;
                }
                else
                    DateTime.TryParse(DateC, out Closing_Date);


                DateTime LastClosingDate = new DateTime(1904, 1, 1);
                try
                {
                    using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "select max(a.posting_date) from account_posting a, titres t, account_entity b where posting_type in (select id from account_posting_types where name = 'Closing') and a.account_entity_id =b.id and t.libelle=b.name and t.sicovam=:0";
                        cmd.Parameters.Add(":0", FundsList[i]);
                        using (OracleDataReader thereader = cmd.ExecuteReader())
                        {
                            while (thereader.Read())
                           {
                                string laDate = thereader[0].ToString();
                                DateTime.TryParse(laDate, out LastClosingDate);
                            }                            
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }

                //CSMDay openingDateDay = new CSMDay(LastClosingDate.ToString("yyyymmdd"));
                CSMDay openingDateDay = new CSMDay(LastClosingDate.Day, LastClosingDate.Month, LastClosingDate.Year);
                
                int postingDate = openingDateDay.toLong();

                // CR [27/11/2012] v1.0.5.0
                //CSMCurrency ccy = CSMCurrency.GetCSRCurrency(AccountingClosingScenario.GetCFGCurrency());
                //if (ccy != null)
                //{
                //    postingDate = ccy.AddNumberOfDays(postingDate, 1);
                //}
                CSMDay postingDateDay = new CSMDay(postingDate);
                OpeningDate = new DateTime(postingDateDay.fYear,postingDateDay.fMonth, postingDateDay.fDay);                 

                //retrieving last accounting date for the current fund
                DateTime LastAccountingDate = GetLastAccountingDate(AName, AccountingDates);

                ALogger.Write(CSMLog.eMVerbosity.M_debug, "Last Accounting Date for Fund " + Fundname + " = " + LastAccountingDate.ToLongDateString() + " " + LastAccountingDate.ToLongTimeString());
                ALogger.Write(CSMLog.eMVerbosity.M_debug, "Closing Date for Fund " + Fundname + " = " + Closing_Date.ToLongDateString() + " " + Closing_Date.ToLongTimeString());
                ALogger.Write(CSMLog.eMVerbosity.M_debug, "Opening Date for Fund " + Fundname + " = " + OpeningDate.ToLongDateString() + " " + OpeningDate.ToLongTimeString());



                LastAccountingDate = LastAccountingDate.AddHours(-LastAccountingDate.Hour);
                LastAccountingDate = LastAccountingDate.AddMinutes(-LastAccountingDate.Minute);
                LastAccountingDate = LastAccountingDate.AddSeconds(-LastAccountingDate.Second);
                LastAccountingDate = LastAccountingDate.AddMilliseconds(-LastAccountingDate.Millisecond);
               
                Closing_Date = Closing_Date.AddHours(-Closing_Date.Hour);
                Closing_Date = Closing_Date.AddMinutes(-Closing_Date.Minute);
                Closing_Date = Closing_Date.AddSeconds(-Closing_Date.Second);
                Closing_Date = Closing_Date.AddMilliseconds(-Closing_Date.Millisecond);
                
                OpeningDate = OpeningDate.AddHours(-OpeningDate.Hour);
                OpeningDate = OpeningDate.AddMinutes(-OpeningDate.Minute);
                OpeningDate = OpeningDate.AddSeconds(-OpeningDate.Second);
                OpeningDate = OpeningDate.AddMilliseconds(-OpeningDate.Millisecond);


                //if (LastAccountingDate < OpeningDate)
                //{
                //    string messEOD = "Please perform EOD first at opening date before launching the opening scenario for fund " + Fundname;
                //    System.Windows.Forms.MessageBox.Show(messEOD);

                //    continue;
                //}
                //if (OpeningDate > Closing_Date)
                //{
                //    string messEOD = "Please perform closing of the fund fisrt before launching the opening scenario for fund " + Fundname;
                //    System.Windows.Forms.MessageBox.Show(messEOD);
                    
                //    continue;
                //}

                int NextNavDate = internFund.GetNAVDateAfter(sophis.market_data.CSMMarketData.GetCurrentMarketData().GetDate());
                CSMDay DayNextNavDate = new CSMDay(NextNavDate);
                DateTime NavDate = new DateTime(DayNextNavDate.fYear, DayNextNavDate.fMonth, DayNextNavDate.fDay);
                ALogger.Write(CSMLog.eMVerbosity.M_debug, "Nav date for fund "+Fundname+" : "+NavDate.ToShortDateString());

                //checking if there must be an alarm
                if (NavDate > Closing_Date.AddDays(1))
                {
                    AlarmesFond.Add(Fundname);
                    string Mess = "The next NAV Date is after the closing date + 1 day for the fund " + Fundname;
                    System.Windows.Forms.MessageBox.Show(Mess);
                    ALogger.Write(CSMLog.eMVerbosity.M_debug, Mess);
                    
                }

                // Retrieving Accounts Information
                int[] AccountsClosingList = GetAccountListOpening();


                //creating structures for accounts information
                DataTable OpeningAccounts = new DataTable();
                OpeningAccounts.Columns.Add("Account Name", typeof(string));
                OpeningAccounts.Columns.Add("Account Number", typeof(int));
                OpeningAccounts.Columns.Add("Balance before opening", typeof(double));
                OpeningAccounts.Columns.Add("Balance after opening", typeof(double));
                DataTable Compte111 = new DataTable();
                Compte111.Columns.Add("Account Name", typeof(string));
                Compte111.Columns.Add("Account Number", typeof(int));
                Compte111.Columns.Add("Balance before opening", typeof(double));
                Compte111.Columns.Add("Balance after opening", typeof(double));

                //retrieving information of accounts to be settled

                bool add = true;
                for (int j = 0; j < AccountsClosingList.Length; j++)
                {
                    add = true;
                    DataRow theRow = OpeningAccounts.NewRow();
                    theRow["Account Number"] = AccountsClosingList[j];
                    theRow["Balance after opening"] = 0;

                    try
                    {

                        using (OracleCommand NameCommand = DBContext.Connection.CreateCommand())
                        {
                            NameCommand.CommandText = "select a.name from account_name a  where a.id in  (select account_name_id from account_map where account_number=:0)";

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
                            theRow["Balance before opening"] = 0;
                            AmountCommand.CommandText = "SELECT SUM(DECODE(a.CREDIT_DEBIT,'C',1,-1) * a.AMOUNT) from account_posting a where a.account_entity_ID in (select ID from account_entity where name = :0) and a.posting_date <=to_date(:1, 'DD/MM/YYYY') and a.account_number = :2";
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
                                    theRow["Balance before opening"] = quantity;
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
                        OpeningAccounts.Rows.Add(theRow);
                    
                }
                ALogger.Write(CSMLog.eMVerbosity.M_debug,"Information from accounts to settled for fund "+Fundname+" retrieved.");

                //same thing for the account that receives the balance of previous settled accounts
                DataRow Compte111Row = Compte111.NewRow();
                try
                {
                    using (OracleCommand AmountCommand = DBContext.Connection.CreateCommand())
                    {
                        ALogger.Write(CSMLog.eMVerbosity.M_debug, String.Format("Date Used = {0}", Date));
                        Compte111Row["Balance before opening"] = 0;
                        AmountCommand.CommandText = "SELECT SUM(DECODE(a.CREDIT_DEBIT,'C',1,-1) * a.AMOUNT) from account_posting a where a.account_entity_ID in (select ID from account_entity where name = :0) and a.posting_date <=to_date(:1, 'DD/MM/YYYY') and a.account_number = :2";
                        AmountCommand.Parameters.Add(":0", OracleDbType.Varchar2);
                        AmountCommand.Parameters.Add(":1", OracleDbType.Varchar2);
                        AmountCommand.Parameters.Add(":2", OracleDbType.Varchar2);
                        AmountCommand.Parameters[":0"].Value = Fundname;
                        AmountCommand.Parameters[":1"].Value = Date;
                        AmountCommand.Parameters[":2"].Value = "111";
                        using (OracleDataReader reader = AmountCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                double quantity = 0;
                                string res = reader[0].ToString();
                                double.TryParse(res, out quantity);
                                Compte111Row["Balance before opening"] = quantity;
                            }                            
                        }                        
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }
                Compte111Row["Balance after opening"] = 0;
                double totalSolde = 0.0;
                for (int k = 0; k < OpeningAccounts.Rows.Count; k++)
                {
                    totalSolde += (double)OpeningAccounts.Rows[k]["Balance before opening"];
                }
                Compte111Row["Balance after opening"] = totalSolde + (double)Compte111Row["Balance before opening"];
                Compte111Row["Account Number"] = 111;
                Compte111Row["Account Name"] = "CAPITAL EN DEBUT D'EXERCICE";
                Compte111.Rows.Add(Compte111Row);
                ALogger.Write(CSMLog.eMVerbosity.M_debug, "Information from account 101 retrieved.");
                

                CFG_AccountClosing.SourceFiles.InterfaceOuverture InterfaceOpening = new InterfaceOuverture(OpeningAccounts, Compte111, internFund, LastAccountingDate, OpeningDate);                
                WinformContainer.DoDialog(InterfaceOpening, WinformContainer.eDialogMode.eModal);
                if (InterfaceOpening.GetSkippedAll())
                    break;


            }

            //once the opening of each fund is done (or not, depending on user's choice), we re-show previous alarms...
            AlarmesFondArray = AlarmesFond.ToArray();
            string TextAlarm = "The following funds have NAV date superior to their closing date + 1 day : ";
            for (int i = 0; i < AlarmesFondArray.Length - 1; i++)
            {
                TextAlarm += AlarmesFondArray[i] + ", ";
            }
            if (AlarmesFondArray.Length > 0)
            {
                TextAlarm += AlarmesFondArray[AlarmesFondArray.Length - 1];
                System.Windows.Forms.MessageBox.Show(TextAlarm);
            }
            ALogger.End();
        }


    }
}

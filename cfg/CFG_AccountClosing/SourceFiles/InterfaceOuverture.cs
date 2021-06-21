using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using System.Collections;
using sophis.value;
using sophis.market_data;
using sophisTools;
using sophis.misc;
using sophis.tools;
using sophis.utils;



namespace CFG_AccountClosing.SourceFiles
{
    public partial class InterfaceOuverture : Form
    {
        public InterfaceOuverture()
        {
            InitializeComponent();
        }
        private CSMAmFund _TheFund;
        private DataTable _AccountsTable = null;
        private DataTable _ReceivingAccounts = null;
        private bool _Launched = false, _Skipped = false, _SkippedAll = false;
        private sophis.utils.CMString _FundName = "";
        private DateTime _LastAccountingEOD = DateTime.Today;
        private DateTime _OpeningDate = DateTime.Today;

        public InterfaceOuverture(DataTable Accounts, DataTable ReceivingAccounts, CSMAmFund Fund, DateTime LastAccountingEOD, DateTime OpeningDate)
        {
            _TheFund = Fund;
            _AccountsTable = Accounts;
            _ReceivingAccounts = ReceivingAccounts;
            //Fund.GetName(_FundName);
            _FundName = Fund.GetName();
           
            _LastAccountingEOD = LastAccountingEOD;
            _OpeningDate = OpeningDate;
            InitializeComponent();
            this.Text = "Opening for the fund "+_FundName ;
            this.Load += new EventHandler(InterfaceOuverture_Load);
        }
        private void InterfaceOuverture_Load(object sender, EventArgs e)
        {
            if (_AccountsTable != null)
                this.GridControlAccounts.DataSource = _AccountsTable;
            if (_ReceivingAccounts != null)
                this.gridControlReceivingAccount.DataSource = _ReceivingAccounts;
            this.FundNameEdit.Text = _FundName;
            this.LastAccountingEdit.Text = _LastAccountingEOD.ToShortDateString();
            this.OpeningDateEdit.Text = _OpeningDate.ToShortDateString();
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView2.OptionsView.ShowGroupPanel = false;
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView2.OptionsBehavior.Editable = false;
            this.gridView1.Columns["Balance before opening"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns["Balance before opening"].DisplayFormat.FormatString = "{0:n}";
            this.gridView2.Columns["Balance before opening"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView2.Columns["Balance before opening"].DisplayFormat.FormatString = "{0:n}";
            this.gridView1.Columns["Balance after opening"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns["Balance after opening"].DisplayFormat.FormatString = "{0:n}";
            this.gridView2.Columns["Balance after opening"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView2.Columns["Balance after opening"].DisplayFormat.FormatString = "{0:n}";
        }

        private void Launch_Click(object sender, EventArgs e)
        {
            _Launched = true;

            EcrireEntreesComptables(_TheFund, _OpeningDate, _AccountsTable, _ReceivingAccounts);

            try
            {
                using (OracleCommand Cmd = DBContext.Connection.CreateCommand())
                {
                    Cmd.CommandText = "UPDATE INDICATORS_FUND_PARAMETERS SET Comments=:0 where Id=:1 and NAME='Next Opening Date'";
                    Cmd.Parameters.Add(":0", OracleDbType.Varchar2);
                    Cmd.Parameters.Add(":1", OracleDbType.Int32);
                    Cmd.Parameters[":0"].Value = _OpeningDate.AddYears(1).ToShortDateString();
                    Cmd.Parameters[":1"].Value = _TheFund.GetCode();
                    Cmd.ExecuteNonQuery();
                    sophis.instrument.CSMInstrument.Free(_TheFund.GetCode());
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }


            this.Hide();
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            _Skipped = true;
            this.Hide();
        }

        private void SkipAll_Click(object sender, EventArgs e)
        {
            _SkippedAll = true;
            this.Hide();
        }

        public bool GetLaunched()
        {
            return _Launched;
        }

        public bool GetSkipped()
        {
            return _Skipped;
        }
        public bool GetSkippedAll()
        {
            return _SkippedAll;
        }
        public void EcrireEntreesComptables(CSMAmFund Fund, DateTime OpeningDate, DataTable ComptesSoldes, DataTable Compte101)
        {
             AccountingClosingScenario.InitializeAccountPostingInfoForAllAccounts();
             AccountingClosingScenario.InitializeAccountEntitiesInfoForAllFunds();
             CMString name = new CMString();
             //DPH
             //Fund.GetName(name);
             name = Fund.GetName();
             int accountEntityId = AccountingClosingScenario.GetAccountAccountEntityInfoForFund(name.ToString());
             int accountCurrency=AccountingClosingScenario.GetCFGCurrency();
             int d = CSMDay.GetSystemDate();
             CSMDay today = new CSMDay(d);
             DateTime generationDate = new DateTime(today.fYear, today.fMonth, today.fDay);

            double GeneralSolde = 0;
            foreach (DataRow myRow in ComptesSoldes.Rows)
            {
                double solde = InsertPosting(name.ToString(), myRow,accountEntityId ,accountCurrency, OpeningDate, generationDate, 0,false);
                GeneralSolde = GeneralSolde + solde;
            }
          

            InsertPosting(name.ToString(), Compte101.Rows[0],accountEntityId ,accountCurrency, OpeningDate,  generationDate,GeneralSolde,true);//General Solde is value to display in the gui
        }


        double InsertPosting(string fundName, DataRow myRow, int accountEntityId, int accountCurrency, DateTime OpeningDate,DateTime generationDate, double GeneralSolde,bool compte101)
        {
            double amount = 0;
          
            string accountNumber = myRow[1].ToString();
            int accountNameId = AccountingClosingScenario.GetAccountPostingInfoForAccount(accountNumber);
           
            //Get the posting ID
            try
            {                
                using (OracleCommand myCommand = DBContext.Connection.CreateCommand())
                {
                    int postingId = 0;

                    //DPH
                    //myCommand.CommandText = "select SEQACCOUNT.nextval from dual";
                    myCommand.CommandText = "select ACCT_POSTING_SEQ.nextval from dual";
                    Object postingIdObj = myCommand.ExecuteScalar();

                    if (postingIdObj != null)
                    {
                        int.TryParse(postingIdObj.ToString(), out postingId);
                    }

                    //Get the posting type
                    int postingtype = AccountOpeningScenario.GetOpenPostingType();//A creer dans value;          

                    //Get the amount posting
                    
                    char creditAmount = 'C';
                    if (!compte101)
                    {
                        amount = (double)myRow[2];
                    }
                    else
                    {
                        amount = -GeneralSolde;
                    }
                    if (amount >= 0)
                        creditAmount = 'D';

                    //build the comment           
                    CSMDay OpeningDateDay = new CSMDay(OpeningDate.Day, OpeningDate.Month, OpeningDate.Year);
                    string comment = "Fund: " + fundName + " Opening Date:" + OpeningDate.ToString("dd/MM/yyyy") + "Account: " + accountNumber;

                    CSMDay generationDateDay = new CSMDay(generationDate.Day, generationDate.Month, generationDate.Year);

                    myCommand.CommandType = System.Data.CommandType.Text;


                    myCommand.CommandText = "Insert into ACCOUNT_POSTING" +
                                 "(ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE," +
                                 "GENERATION_DATE,POSTING_DATE," +
                                 "ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT," +

                                 "THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT," +
                                 "COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,"

                                + " ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID," +
                                 "LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,"
                                + " ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,"
                                + "INSTRUCTION_ID,PAYMENT_ID,NOSTRO_ID,AMOUNT_CURRENCY,TRADE_TYPE) values"

                                + "(" + postingId + "," + accountEntityId + ",null,null, null ," + postingtype

                                + ", num_to_date(" + generationDateDay.toLong().ToString() + "), num_to_date(" + OpeningDateDay.toLong().ToString() + "),"

                                + accountNumber + "," + accountCurrency + ",:amount,'" + creditAmount + "',"

                                + "null,null,null,null,'+',null,null,"
                                + "'" + comment + "'" + ",'Cloture',4,null,"
                                + accountNameId + ",null,null,"
                                + " null,8,null,null,null,null,"
                                + "null ,null,null,null,: amountCurrency,null,null)";


                    OracleParameter Param = new OracleParameter(); //***Oracle.DataAccess.Client.OracleParameter();
                    Param.ParameterName = "amount";
                    Param.OracleDbType = Oracle.DataAccess.Client.OracleDbType.Double;
                    Param.Direction = System.Data.ParameterDirection.Input;
                    Param.Value = Math.Abs(amount);

                    myCommand.Parameters.Add(Param);

                    OracleParameter Param2 = new OracleParameter();
                    Param2.ParameterName = "amountCurrency";
                    Param2.OracleDbType = Oracle.DataAccess.Client.OracleDbType.Double;
                    Param2.Direction = System.Data.ParameterDirection.Input;
                    Param2.Value = Math.Abs(amount);
                    myCommand.Parameters.Add(Param2);

                    try
                    {
                        myCommand.ExecuteNonQuery();
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show("Exception during posting account creation: " + ex.Message + " amount:" + amount);

                    }

                    myCommand.CommandText = "commit";
                    myCommand.ExecuteNonQuery();
                }                                                  
            }

            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("0000000000000000: " + ex.Message);
                amount = 0;
            }
            
            return amount;
        }                      
    }
}

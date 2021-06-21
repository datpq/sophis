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
    public partial class InterfaceAffectationResultat : Form
    {
        public InterfaceAffectationResultat()
        {
            InitializeComponent();
        }

        private CSMAmFund _TheFund;
        private DataTable _AccountsTable = null;
        //private DataTable _CopyAccountTable = null;
        //private DataTable _ReceivingAccounts = null;
        private bool _Launched = false, _Skipped = false, _SkippedAll = false;
        private sophis.utils.CMString _FundName = "";
        private DateTime _DistribDate = DateTime.Today;
        //private DateTime _AffectationDate = DateTime.Today;
        private double ToBeDisributed = 0;
        private int SharesNumber = 0;
        private double DistributedAmount = 0;
        private double CapitalizedAmount = 0;
        //private bool _distriboverflow = false;
        //private bool _capitaloverflow = false;

        public InterfaceAffectationResultat(DataTable Accounts,  CSMAmFund Fund,  DateTime DistribDate)
        {
            try
            {
                InitializeComponent();
                _TheFund = Fund;
                _AccountsTable = Accounts;
                //_CopyAccountTable = Accounts;

                //DPH
                //Fund.GetName(_FundName);
                _FundName = Fund.GetName();
                _DistribDate = DistribDate;
                for (int i = 0; i < _AccountsTable.Rows.Count; i++)
                {
                    if (_AccountsTable.Rows[i]["AccountType"].ToString() == "To settle")
                    {
                        ToBeDisributed += (double)_AccountsTable.Rows[i]["Balance before Distribution"];
                    }
                }

                ToBeDisributed = Math.Round(ToBeDisributed, 2);
                SharesNumber = GetShareNumber();

                this.Text = "Result distribution for the fund " + _FundName;
                this.Load += new EventHandler(InterfaceAffectationResultat_Load);
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }                         
        }
        private void InterfaceAffectationResultat_Load(object sender, EventArgs e)
        {
            try
            {
                if (_AccountsTable != null)
                    this.GridControlAccounts.DataSource = _AccountsTable;
                this.AffectationDatePicker.Value = _DistribDate;
                this.FundNameEdit.Text = _FundName;
                this.AmountToDistributeEdit.Text = ToBeDisributed.ToString();
                this.ShareNbEdit.Text = SharesNumber.ToString();
                this.AmountDistributedEdit.Text = DistributedAmount.ToString();
                this.CapitalizedAmountEdit.Text = CapitalizedAmount.ToString();

                gridView1.OptionsView.ShowGroupPanel = false;
                this.gridView1.OptionsBehavior.Editable = false;
                this.gridView1.Columns["Balance before Distribution"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                this.gridView1.Columns["Balance before Distribution"].DisplayFormat.FormatString = "{0:n}";
                this.gridView1.Columns["Balance after Distribution"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                this.gridView1.Columns["Balance after Distribution"].DisplayFormat.FormatString = "{0:n}";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }                       
        }

        private int GetShareNumber()
        {
            int nb = 10;
            try
            {
                using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                {
                    cmd.CommandText = "select sum(decode(SIGN,'+',quantity,'-',-1*quantity)) from account_posting where account_number in ('1123','1124') and DATE_TO_NUM(posting_date) <= :0 and account_entity_id = (select id from account_entity where record_type=1 and name like (select libelle from titres where sicovam=:1))";
                    CSMDay distribDateDay = new CSMDay(_DistribDate.ToString("yyyyMMdd"));
                    cmd.Parameters.Add(":0", distribDateDay.toLong());
                    cmd.Parameters.Add(":1", _TheFund.GetCode());
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string test = reader[0].ToString();
                            int.TryParse(test, out nb);
                        }                        
                    }
                }
            }
            catch (Exception e)
            {
                nb = 0;
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }

            return nb;
        }


        private void AmountDistributed_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                string amount = AmountDistributedEdit.Text;
                amount = amount.Replace('.', ',');                

                double.TryParse(amount, System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.CultureInfo("fr-FR"), out DistributedAmount);

                if (Math.Abs((decimal)(DistributedAmount + CapitalizedAmount)) > Math.Abs((decimal)ToBeDisributed))
                {
                    System.Windows.Forms.MessageBox.Show("Capitalized amount + Distributed amount can't be greater than the balance to be distributed");
                }
                else
                {
                    Recompute();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void AmountCapitalized_EditValueChanged(object sender, EventArgs e)
        {
            
            try
            {
                string amount = CapitalizedAmountEdit.Text;
                amount = amount.Replace('.', ',');
                double.TryParse(amount, System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.CultureInfo("fr-FR"), out CapitalizedAmount);

                DataRow[] foundRows = _AccountsTable.Select("AccountNumber = 430 and AccountType = 'To credit'");
                DataRow account430Row = foundRows[0];
                double amount430After = 0;

                if (account430Row != null)
                {
                    amount430After = (double)account430Row["Balance after Distribution"];
                }                
                
                if (Math.Abs((decimal)(DistributedAmount + CapitalizedAmount)) > Math.Abs((decimal)ToBeDisributed))
                {
                    System.Windows.Forms.MessageBox.Show("Capitalized amount + Distributed amount can't be greater than the balance to be distributed");
                }
                else
                {
                    Recompute();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Recompute()
        {            
            try
            {
                double Amount430 = SharesNumber * (Math.Floor(DistributedAmount / SharesNumber * 100) / 100);
                double Amount111 = CapitalizedAmount;
                double Amount1601 = DistributedAmount - Amount430;
                //double Amount1608 = Math.Abs(ToBeDisributed) - Math.Abs(CapitalizedAmount) - Math.Abs(Amount430);
                double Amount1608 = Math.Abs(ToBeDisributed) - Math.Abs(CapitalizedAmount) - Math.Abs(Amount430) - Math.Abs(Amount1601);

                double Amount430before = 0;
                DataRow[] foundRows = _AccountsTable.Select("AccountNumber = 430 and AccountType = 'To credit'");
                DataRow account430Row = foundRows[0];

                if (account430Row != null)
                {
                    Amount430before = (double)account430Row["Balance before Distribution"];
                }

                double Amount111before = 0;

                foundRows = _AccountsTable.Select("AccountNumber = 111 and AccountType = 'To credit'");
                DataRow account111Row = foundRows[0];

                if (account111Row != null)
                {
                    Amount111before = (double)account111Row["Balance before Distribution"];
                }

                account430Row["Balance after Distribution"] = Math.Round(-Amount430 + Amount430before, 2);
                account111Row["Balance after Distribution"] = Math.Round(-Amount111 + Amount111before, 2);

                foundRows = _AccountsTable.Select("AccountNumber = 1601 and AccountType = 'To credit'");
                DataRow account1601Row = foundRows[0];
                if (account1601Row != null)
                {
                    account1601Row["Balance after Distribution"] = Math.Round(-Amount1601, 2);
                }

                foundRows = _AccountsTable.Select("AccountNumber = 1608 and AccountType = 'To credit'");
                DataRow account1608Row = foundRows[0];
                if (account1608Row != null)
                {
                    account1608Row["Balance after Distribution"] = Math.Round(-Amount1608, 2);
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }            
        }

        private void Launch_Click(object sender, EventArgs e)
        {
            try
            {
                if (Math.Abs((decimal)(DistributedAmount + CapitalizedAmount)) > Math.Abs((decimal)ToBeDisributed))
                {

                    System.Windows.Forms.MessageBox.Show("Capitalized amount + Distributed amount can't be greater than the balance to be distributed");
                    return;
                }

                _Launched = true;

                this.EcrireEntreesComptables(_TheFund, _DistribDate, _AccountsTable);

                try
                {
                    //using (OracleCommand Cmd = DBContext.Connection.CreateCommand())
                    //{
                    //    Cmd.CommandText = "UPDATE INDICATORS_FUND_PARAMETERS SET Comments=:0 where Id=:1 and NAME='Next Opening Date'";
                    //    Cmd.Parameters.Add(":0", OracleDbType.Varchar2);
                    //    Cmd.Parameters.Add(":1", OracleDbType.Int32);
                    //    Cmd.Parameters[":0"].Value = _OpeningDate.AddYears(1).ToShortDateString();
                    //    Cmd.Parameters[":1"].Value = _TheFund.GetCode();
                    //    Cmd.ExecuteNonQuery();
                    //    sophis.instrument.CSMInstrument.Free(_TheFund.GetCode());
                    //}
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }

                this.Hide();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
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


        public void EcrireEntreesComptables(CSMAmFund Fund, DateTime DistribDate, DataTable TheAccountsTable)
        {

            AccountingClosingScenario.InitializeAccountPostingInfoForAllAccounts();
            AccountingClosingScenario.InitializeAccountEntitiesInfoForAllFunds();
            CMString name = new CMString();
            //DPH
            //Fund.GetName(name);
            name = Fund.GetName();
            int accountEntityId = AccountingClosingScenario.GetAccountAccountEntityInfoForFund(name.ToString());
            int accountCurrency = AccountingClosingScenario.GetCFGCurrency();
            int d = CSMDay.GetSystemDate();
            CSMDay today = new CSMDay(d);
            DateTime generationDate = new DateTime(today.fYear, today.fMonth, today.fDay);
            foreach (DataRow myRow in TheAccountsTable.Rows)
            {              
               double solde = InsertPosting(name.ToString(), myRow, accountEntityId, accountCurrency, DistribDate, generationDate, 0, false);//A mettre AffectationDate                     
            }
           
        }

        //public void EcrireEntreesComptables(CSMAmFund Fund, DateTime AffectationDate, DataTable ComptesSoldes, DataTable CompteCredite)
        //{
        //    AccountingClosingScenario.InitializeAccountPostingInfoForAllAccounts();
        //    AccountingClosingScenario.InitializeAccountEntitiesInfoForAllFunds();
        //    CMString name = new CMString();
        //    Fund.GetName(name);
        //    int accountEntityId = AccountingClosingScenario.GetAccountAccountEntityInfoForFund(name.ToString());
        //    int accountCurrency = AccountingClosingScenario.GetCFGCurrency();
        //    int d = CSMDay.GetSystemDate();
        //    CSMDay today = new CSMDay(d);
        //    DateTime generationDate = new DateTime(today.fYear, today.fMonth, today.fDay);

        //    double GeneralSolde = 0;
        //    foreach (DataRow myRow in ComptesSoldes.Rows)
        //    {
        //        double solde = InsertPosting(name.ToString(), myRow, accountEntityId, accountCurrency, AffectationDate, generationDate, 0, false);//A mettre AffectationDate
        //        GeneralSolde = GeneralSolde + solde;
        //    }

        //    InsertPosting(name.ToString(), CompteCredite.Rows[0], accountEntityId, accountCurrency, AffectationDate, generationDate, GeneralSolde, true);//General Solde is value to display in the gui
        //}


        double InsertPosting(string fundName, DataRow myRow, int accountEntityId, int accountCurrency, DateTime affectationDate, DateTime generationDate, double GeneralSolde, bool compteCredit)
        {
            double amount = 0;
          
           //int id = (int)myRow[1];
            double before = (double)myRow[2];
            double after = (double)myRow[3];

            string accountNumber = myRow[1].ToString();
            int accountNameId = AccountingClosingScenario.GetAccountPostingInfoForAccount(accountNumber);     
          

            //Get the posting ID
            int postingId = 0;

            using (OracleCommand myCommand = DBContext.Connection.CreateCommand())
            {
                //DPH
                //myCommand.CommandText = "select SEQACCOUNT.nextval from dual";
                myCommand.CommandText = "select ACCT_POSTING_SEQ.nextval from dual";

                Object nextValObj = myCommand.ExecuteScalar();
                if (nextValObj != null)
                {
                    int.TryParse(nextValObj.ToString(), out postingId);
                }

                //Get the posting type
                int postingtype = AffectationResultatScenario.GetAffectationPostingType();//A creer dans value;               

                //Get the posting amount
                
                char creditAmount = 'C';
                amount = after - before;

                if (amount >= 0)
                    creditAmount = 'D';

                //build the comment

                CSMDay affectationDateDay = new CSMDay(affectationDate.Day, affectationDate.Month, affectationDate.Year);
                string comment = "Fund: " + fundName + ", Distribution Date: " + affectationDate.ToString("dd/MM/yyyy") + " , Account: " + accountNumber;

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

                            + ", num_to_date(" + generationDateDay.toLong().ToString() + "),num_to_date(" + affectationDateDay.toLong().ToString() + "),"

                            + accountNumber + "," + accountCurrency + ",:amount,'" + creditAmount + "',"

                            + "null,null,null,null,'+',null,null,"
                            + "'" + comment + "'" + ",'Cloture',4,null,"
                            + accountNameId + ",null,null,"
                            + " null,8,null,null,null,null,"
                            + "null ,null,null,null,:amountCurrency,null,null)";

                OracleParameter Param = new OracleParameter(); //***Oracle.DataAccess.Client.OracleParameter();
                Param.ParameterName = "amount";
                Param.OracleDbType = Oracle.DataAccess.Client.OracleDbType.Double;
                Param.Direction = System.Data.ParameterDirection.Input;
                Param.Value = Math.Abs(amount);

                myCommand.Parameters.Add(Param);
                OracleParameter Param2 = new OracleParameter(); //***Oracle.DataAccess.Client.OracleParameter();
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
            
            return amount;
        }

        private void AffectationDatePicker_ValueChanged(object sender, EventArgs e)
        {
            _DistribDate = AffectationDatePicker.Value;
        }

    }
}

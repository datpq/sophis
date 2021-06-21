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
using sophis.market_data;
using sophisTools;
using sophis.misc;
using sophis.tools;
using sophis.utils;


namespace CFG_AccountClosing.SourceFiles
{
    public partial class AffectationManager : Form
    {
        public AffectationManager()
        {
            InitializeComponent();
            _FundsList = GetFundsList();
            _FundsInfo = GetFundsInformation(_FundsList);

            this.Load += new EventHandler(AffectationManager_Load);
        }

        private Dictionary<int, string> _FundsList;
        private DataTable _FundsInfo = null;
        private bool _cancel = false;
        private CSMLog _logger = new CSMLog();



        private void AffectationManager_Load(object sender, EventArgs e)
        {
            if (_FundsInfo != null)
                this.FundsGridControl.DataSource = _FundsInfo;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsBehavior.Editable = false;
            
        }

        private Dictionary<int, string> GetFundsList()
        {
            Dictionary<int, string> laListe = new Dictionary<int, string>();
            using (OracleCommand cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = "select t.sicovam, a.Name from account_entity a,titres t where record_type=1 and name not like 'REF%%' and a.name=t.libelle";
                using (OracleDataReader Thereader = cmd.ExecuteReader())
                {
                    while (Thereader.Read())
                    {
                        string TheSico = Thereader[0].ToString();
                        int Sico = 0;
                        int.TryParse(TheSico, out Sico);
                        string Name = Thereader[1].ToString();
                        laListe.Add(Sico, Name);
                    }                    
                }
            }
            return laListe;
        }

        private DataTable GetFundsInformation(Dictionary<int, string> Fonds)
        {
            _logger.Begin("AffectationManager", "GetFundsInformation");
            DataTable LaTable = new DataTable();
            Dictionary<int, DateTime> EODDates = GetLastAccountingDates();
            LaTable.Columns.Add("Fund", typeof(string));
            LaTable.Columns.Add("Last Opening Date", typeof(DateTime));
            LaTable.Columns.Add("Last EOD Date", typeof(DateTime));
            foreach (KeyValuePair<int, string> Fond in Fonds)
            {
                DataRow theRow = LaTable.NewRow();
                theRow["Fund"] = Fond.Value;
                foreach (KeyValuePair<int, DateTime> Date in EODDates)
                {
                    if (Fond.Key == Date.Key)
                    {
                        theRow["Last EOD Date"] = Date.Value;
                        _logger.Write(CSMLog.eMVerbosity.M_debug, "Last Accounting Date for Fund " + Fond.Value + " = " + Date.Value.ToLongDateString() + " " + Date.Value.ToLongTimeString());
                    }
                }
                DateTime theOpeningDate = new DateTime();
                try
                {
                    using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "select max(a.posting_date) from account_posting a, titres t, account_entity b where posting_type in (select id from account_posting_types where name = 'Opening') and a.account_entity_id =b.id and t.libelle=b.name and t.sicovam=:0";
                        cmd.Parameters.Add(":0", Fond.Key);
                        Object laDateObj = cmd.ExecuteScalar();
                        if (laDateObj != null)
                        {
                            DateTime.TryParse(laDateObj.ToString(), out theOpeningDate);
                        }                        
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }
                theRow["Last Opening Date"] = theOpeningDate;

                sophis.value.CSMAmFund Lefond = sophis.value.CSMAmFund.GetFund(Fond.Key);
                if (Lefond == null)
                    continue;

                DateTime lastDistributionDate = new DateTime();
                try
                {
                    using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "select max(a.posting_date) from account_posting a, titres t, account_entity b where posting_type in (select id from account_posting_types where name = 'Distribution') and a.account_entity_id =b.id and t.libelle=b.name and t.sicovam=:0";
                        cmd.Parameters.Add(":0", Fond.Key);
                        Object laDateObj = cmd.ExecuteScalar();
                        if (laDateObj != null)
                        {
                            DateTime.TryParse(laDateObj.ToString(), out lastDistributionDate);
                        }                        
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }

                //Case 00656236
                //if (lastDistributionDate >= theOpeningDate)
                //    continue;

                LaTable.Rows.Add(theRow);
            }
            _logger.End();
            return LaTable;
        }


        public Dictionary<int, DateTime> GetLastAccountingDates()
        {
            Dictionary<int, DateTime> Results = new Dictionary<int, DateTime>();
            try
            {
                using (OracleCommand cmd = DBContext.Connection.CreateCommand())
                {
                    cmd.CommandText = "select t.sicovam, max(date_to_num(P.posting_date)) from account_entity E, account_posting P ,titres t where  E.ID = P.account_entity_id and E.record_type=1 and E.name not like 'REFERENCE%%' and P.rule_type in (2,3) and P.status in (2,4) and t.libelle=e.name group by t.sicovam, E.id";
                    using (OracleDataReader MyReader = cmd.ExecuteReader())
                    {
                        while (MyReader.Read())
                        {

                            string id = MyReader[0].ToString();
                            Int32 ID = 0;
                            Int32.TryParse(id, out ID);
                            string date = MyReader[1].ToString();
                            Int32 DATE = 0;
                            Int32.TryParse(date, out DATE);
                            CSMDay datelast = new CSMDay(DATE);
                            DateTime thedate = new DateTime(datelast.fYear, datelast.fMonth, datelast.fDay);
                            Results.Add(ID, thedate);
                        }                        
                    }
                    
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
            }
            return Results;
        }

        private void proceedButton_Click(object sender, EventArgs e)
        {
            int [] selectedrows=gridView1.GetSelectedRows();
            List<int>sicoFunds=new List<int>();
       
            for(int i =0;i<selectedrows.Length;i++)
            {
                string Name =(string) _FundsInfo.Rows[selectedrows[i]]["Fund"];
                foreach (KeyValuePair<int, string> Fond in _FundsList)
                {
                    if (Fond.Value == Name)
                    {
                        sicoFunds.Add(Fond.Key);
                    }
                }
            }
            int[] SicoFundsToProceed=sicoFunds.ToArray();
            CFG_AccountClosing.SourceFiles.AffectationResultatScenario.ProceedToAffectation(SicoFundsToProceed);
            this.Hide();
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            _cancel = true;
            this.Hide();
        }
        public bool getcancelled()
        {
            return _cancel;
        }
    }
}

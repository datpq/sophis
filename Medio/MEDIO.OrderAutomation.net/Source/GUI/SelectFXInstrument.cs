using DevExpress.XtraEditors;
using sophis.guicommon.basicDialogs;
using sophis.instrument;
using sophis.static_data;
using sophis.utils;
using sophis.xaml;
using sophisTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public partial class SelectFXInstrument : Form
    {
        public SelectFXInstrument()
        {
            InitializeComponent();

            for(int i =0; i < CSMCurrency.GetCurrencyCount(); i++)
            {
                CSMCurrency curr = CSMCurrency.GetNthCurrency(i);
                using (CMString nameCcy = new CMString())
                {
                    CSMCurrency.CurrencyToString(curr.GetIdent(),nameCcy);
                    comboBox_ccy1.Items.Add(nameCcy.StringValue);
                    comboBox_ccy2.Items.Add(nameCcy.StringValue);
                }
            }
        }

        bool ValidateGUI(ref int ccy1Id, ref int ccy2Id)
        {
            string ccy1Upper = comboBox_ccy1.Text.ToUpper();
            comboBox_ccy1.Text = ccy1Upper;
            string ccy2Upper = comboBox_ccy2.Text.ToUpper();
            comboBox_ccy2.Text = ccy2Upper;
            ccy1Id = CSMCurrency.StringToCurrency(ccy1Upper);
            CSMCurrency ccy1 = CSMCurrency.GetCSRCurrency(ccy1Id);
            if (ccy1 == null)
            {
                MessageBox.Show("Invalid receiving ccy");
                return false;
               
            }
            ccy2Id = CSMCurrency.StringToCurrency(ccy2Upper);
            CSMCurrency ccy2 = CSMCurrency.GetCSRCurrency(ccy2Id);
            if (ccy2 == null)
            {
                MessageBox.Show("Invalid paying ccy");
                return false;
            }

            return true;
        }

        private void UpdateValueDate()
        {
            if(checkBox_forward.Checked == false)
            {
                return;
            }

            int ccyId1 = 0;
            int ccyId2 = 0;
            if (true == ValidateGUI(ref ccyId1, ref ccyId2))
            {
               
                // Today
                DateTime today = DateTime.Now;
                CSMDay csmDate = new CSMDay(today.Day, today.Month, today.Year);
                // Settlement date, asking the corresponding forex spot
                CSMForexSpot forexSpot = CSMForexSpot.GetCSRForexSpot(ccyId1, ccyId2);
                if (forexSpot != null)
                    csmDate = new CSMDay(forexSpot.GetSettlementDate(csmDate.toLong()));
                else // Default value : now + two days
                    csmDate.addDay(2);

                // Matching business day (calendar = currency2)
                CSMCurrency csmCurrency2 = CSMCurrency.GetCSRCurrency(ccyId2);
                CSMDay csmSettlementDate = new CSMDay(csmCurrency2.MatchingBusinessDay(csmDate));
                valueDateEdit.DateTime = new DateTime(
                                        csmSettlementDate.fYear,
                                        csmSettlementDate.fMonth,
                                        csmSettlementDate.fDay);
                 
            }
            else
            {
                valueDateEdit.Reset();
            }
            
        }
        private void checkBox_forward_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if( cb != null && cb.Checked == true)
            {
                this.valueDateEdit.Enabled = true;
                UpdateValueDate();
            }
            else
            {
                this.valueDateEdit.Enabled = false;
            }
        }

        private void comboBox_ccy1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //UpdateValueDate();
        }

        private void comboBox_ccy2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //UpdateValueDate();
        }

        bool CheckBusinessDate(CSMCurrency ccy, int dayForward)
        {
            if( ccy.IsABankHolidayDay(dayForward) )
            {
                using (CMString nameCcy = new CMString())
                {
                    CSMCurrency.CurrencyToString(ccy.GetIdent(), nameCcy);
                    MessageBox.Show("The select date is a Bank Holiday for ccy " + nameCcy);
                }
                return false;
            }
            return true;
        }
        private void valueDateEdit_EditValueChanged(object sender, EventArgs e)
        {
            DateEdit de = sender as DateEdit;
            if( de!=null)
            {
                CSMDay dayForward = new CSMDay(de.DateTime.Day, de.DateTime.Month, de.DateTime.Year);
                CSMCurrency ccy1 = CSMCurrency.GetCSRCurrency(CSMCurrency.StringToCurrency(comboBox_ccy1.Text));
                if (ccy1 != null)
                {
                    if (CheckBusinessDate(ccy1, dayForward.toLong()) == true)
                    {
                        CSMCurrency ccy2 = CSMCurrency.GetCSRCurrency(CSMCurrency.StringToCurrency(comboBox_ccy2.Text));
                        if (ccy2 != null)
                        {
                            CheckBusinessDate(ccy2, dayForward.toLong());
                        }
                    }
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int ccy1Id = -1;
            int ccy2Id = -1;
            if( false == ValidateGUI(ref ccy1Id, ref ccy2Id))
            {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            CSMForexSpot spot = CSMForexSpot.GetCSRForexSpot(ccy1Id, ccy2Id);
            if( spot == null)
            {
                spot = CSMForexSpot.new_CSRForexSpot(ccy1Id, ccy2Id);
            }
            
        }

        private void comboBox_ccy1_Leave(object sender, EventArgs e)
        {
            comboBox_ccy1.Text = comboBox_ccy1.Text.ToUpper();
        }

        private void comboBox_ccy2_Leave(object sender, EventArgs e)
        {
            comboBox_ccy2.Text = comboBox_ccy2.Text.ToUpper();
        }

      
    }
}

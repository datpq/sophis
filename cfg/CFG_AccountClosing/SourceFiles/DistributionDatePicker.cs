using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CFG_AccountClosing.SourceFiles
{
    public partial class DistributionDatePicker : Form
    {
        public DistributionDatePicker()
        {
            InitializeComponent();
        }

        public DistributionDatePicker(string Name)
        {
            InitializeComponent();
            FundName = Name;
            this.TextDatePick.Text = "Please choose a distribution date for fund " + FundName + " :";
        }

        private string FundName;
        private DateTime DistribDate=DateTime.Today;


        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DistribDate = dateTimePicker1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        public DateTime GetDistributionDate()
        {
            return DistribDate;
        }


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Eff.Utils;

namespace Eff.Utils
{
    public partial class ColumnExplainerForm : Form
    {
        public ColumnExplainerForm()
        {
            InitializeComponent();
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ColumnExplainerForm_Load(object sender, EventArgs e)
        {

        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            ColumnExplainer.ReloadFromDatabase();
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using sophis.guicommon.basicDialogs;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public partial class AccountsListForm : DefaultEmbeddableControl
    {
        public AccountsListForm()
        {
            InitializeComponent();
            gridControl1.DataSource = CSxAccountController.GetAccountsList();
            gridControl1.RefreshDataSource();
        }

    }
}
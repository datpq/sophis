using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MEDIO.OrderAutomation.net.Source.DataModel;
using sophis.amGuiCommon;
using sophis.guicommon.basicDialogs;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public partial class CSxOrderSendingReport : DefaultEmbeddableControl
    {
        public CSxOrderSendingReport()
        {
            InitializeComponent();
        }

        public CSxOrderSendingReport(List<CSxOrderReport> orderReports)
        {
            InitializeComponent();
            this.gridControl1.DataSource = orderReports;
            this.textBoxTotal.Text = orderReports.Count.ToString();
            this.textBoxSent.Text = orderReports.Count.ToString();
        }


        public static void Display(List<CSxOrderReport> orderReports)
        {
            if (orderReports == null)
                return;
            try
            {
                CSxOrderSendingReport dialog = new CSxOrderSendingReport(orderReports);
                XSAMWinFormsAdapter<CSxOrderSendingReport> adapter = XSAMWinFormsAdapter<CSxOrderSendingReport>.OpenAmMDIDialog(dialog, "Medio Order Sending Report", null, 8000, null, null);
                if (adapter == null)
                    return;
                adapter.ShowWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while opening the Order Sending Report: \n" + ex.Message);
            }
        }


    }
}

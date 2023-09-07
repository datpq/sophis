using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
//using MEDIO.CORE.Tools;
using MEDIO.OrderAutomation.net.Source.Tools;
using MEDIO.OrderAutomation.NET.Source.DataModel;
using sophis.guicommon.basicDialogs;
using Sophis.Windows.Integration;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public partial class CSxFXAutomationSetting : DefaultEmbeddableControl
    {
        private ICollection<FxAutoMarket> lstMarkets;
        private Dictionary<string, ICollection<FxAutoCurrency>> marketCurrencies = new Dictionary<string, ICollection<FxAutoCurrency>>();
        //private ICollection<FxAutoSleeve> lstSleeves;
        private ICollection<FXAutoSleeveFolio> lstSleevesFolio;

        private bool marketsUpdated = false;
        private bool currenciesUpdated = false;
        private bool sleevesUpdated = false;

        public CSxFXAutomationSetting()
        {
            InitializeComponent();
        }

        public static void Display()
        {
            var frm = new CSxFXAutomationSetting();
            var adapter = sophis.xaml.XSRWinFormsAdapter<CSxFXAutomationSetting>.GetUniqueDialog(
                new WindowKey(12976, 0, 0), frm.Title, () => frm, true);
            if (adapter == null) return;
            //adapter.Closing += frm.ReportingMainForm_FormClosing;
            adapter.ShowWindow();
            //sophis.gui.WinformContainer.DoDialog(new ReportingMainForm(),
            //    sophis.gui.WinformContainer.eDialogMode.eModeless); 
        }

        private void cmdOk_Click(object sender, System.EventArgs e)
        {
            if (currenciesUpdated)
            {
                lstMarkets.ToList().ForEach(market =>
                {
                    //CSxDBHelper.Execute($"DELETE MEDIO_FXAUTO_CURRENCIES WHERE MARKET = '{market.Code}'");
                    CSxUtils.ExecuteNonQuery($"DELETE MEDIO_FXAUTO_CURRENCIES WHERE MARKET = '{market.Code}'");
                    marketCurrencies[market.Code].Where(x => x.Active).ToList().ForEach(currency =>
                    {
                        //CSxDBHelper.Execute($"INSERT INTO MEDIO_FXAUTO_CURRENCIES(MARKET, CODE) VALUES('{market.Code}', '{currency.Code}')");
                        CSxUtils.ExecuteNonQuery($"INSERT INTO MEDIO_FXAUTO_CURRENCIES(MARKET, CODE) VALUES('{market.Code}', '{currency.Code}')");
                    });
                });
            }
            if (marketsUpdated)
            {
                lstMarkets.ToList().ForEach(market => {
                    //CSxDBHelper.Execute($"UPDATE MEDIO_FXAUTO_MARKETS SET ACTIVE = {(market.Active ? 1 : 0)} WHERE CODE = '{market.Code}'");
                    CSxUtils.ExecuteNonQuery($"UPDATE MEDIO_FXAUTO_MARKETS SET ACTIVE = {(market.Active ? 1 : 0)} WHERE CODE = '{market.Code}'");
                });
            }
            if (sleevesUpdated)
            {
                //CSxDBHelper.Execute($"DELETE MEDIO_FXAUTO_SLEEVES");
                CSxUtils.ExecuteNonQuery($"DELETE MEDIO_FXAUTO_SLEEVES");

                lstSleevesFolio.Where(x => x.Active).ToList().ForEach(x => {
                    //CSxDBHelper.Execute($"INSERT INTO MEDIO_FXAUTO_SLEEVES(IDENT) VALUES({x.ID})");
                    CSxUtils.ExecuteNonQuery($"INSERT INTO MEDIO_FXAUTO_SLEEVES(IDENT) VALUES({x.ID})");
                });

                //lstSleeves.Where(x=>x.Active).ToList().ForEach(x =>
                //{
                //    //CSxDBHelper.Execute($"INSERT INTO MEDIO_FXAUTO_SLEEVES(FUND_ID, IDENT) VALUES({x.FundId}, {x.Id})");
                //    CSxUtils.ExecuteNonQuery($"INSERT INTO MEDIO_FXAUTO_SLEEVES(FUND_ID, IDENT) VALUES({x.FundId}, {x.Id})");
                //});
            }
            MessageBox.Show("Saved with success", Title, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        private void CSxFXAutomationSetting_Load(object sender, System.EventArgs e)
        {
            lstMarkets = null;
            marketCurrencies.Clear();
            lstSleevesFolio = null;

            lstMarkets = FxAutoMarket.GetData();
            lstMarkets.ToList().ForEach(x => {
                if (!marketCurrencies.ContainsKey(x.Code)) marketCurrencies.Add(x.Code, FxAutoCurrency.GetData(x.Code));
            });
            gridMarkets.DataSource = lstMarkets;

            lstSleevesFolio = FXAutoSleeveFolio.GetData();
            treeSleeves.DataSource = lstSleevesFolio;

            //if (lstSleeves == null) lstSleeves = FxAutoSleeve.GetData();
            //gridSleeves.DataSource = lstSleeves;
            marketsUpdated = currenciesUpdated = sleevesUpdated = false;
        }

        private void viewMarkets_FocusedRowObjectChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowObjectChangedEventArgs e)
        {
            var market = e.Row as FxAutoMarket;
            if (market == null) return;
            if (market != null) gridCurrencies.DataSource = marketCurrencies[market.Code];
        }

        private void viewMarkets_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            marketsUpdated = true;
        }

        private void viewCurrencies_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            currenciesUpdated = true;
        }

        private void viewSleeves_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            sleevesUpdated = true;
        }

        private void treeSleeves_GetStateImage(object sender, DevExpress.XtraTreeList.GetStateImageEventArgs e)
        {
            e.NodeImageIndex = e.Node.HasChildren ? 0 : 1;
            //if (treeSleeves.FocusedNode == e.Node) e.NodeImageIndex = 0;
        }

        private void treeSleeves_NodeCellStyle(object sender, DevExpress.XtraTreeList.GetCustomNodeCellStyleEventArgs e)
        {
            var sleeve = treeSleeves.GetDataRecordByNode(e.Node) as FXAutoSleeveFolio;
            e.Appearance.BackColor = (sleeve != null && sleeve.Active) ? Color.Turquoise : Color.White;
        }

        private void cmdRefresh_Click(object sender, System.EventArgs e)
        {
            CSxFXAutomationSetting_Load(null, null);
        }

        private void treeSleeves_BeforeFocusNode(object sender, DevExpress.XtraTreeList.BeforeFocusNodeEventArgs e)
        {
            e.CanFocus = !e.Node.HasChildren;
            var sleeve = treeSleeves.GetDataRecordByNode(e.Node) as FXAutoSleeveFolio;
            //e.CanFocus = sleeve != null && sleeve.Active;
        }

        private void treeSleeves_CellValueChanged(object sender, DevExpress.XtraTreeList.CellValueChangedEventArgs e)
        {
            sleevesUpdated = true;
        }
    }
}
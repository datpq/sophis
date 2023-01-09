using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using MEDIO.OrderAutomation.net.Source.Data;
using MEDIO.OrderAutomation.net.Source.Tools;
using MEDIO.OrderAutomation.NET.Source.DataModel;
using sophis.static_data;
using sophis.xaml;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public partial class CSxFXRollingForm : sophis.guicommon.basicDialogs.DefaultEmbeddableControl
    {
        string datePattern = "d";
        private int currenExpandLevel = 0;
        private DateTime? _forwardDate = null;
        private List<CSxFXRollDataModel> _dataSource = new List<CSxFXRollDataModel>();
        private List<CSxFXRollDataModel> _dataSourceClone = new List<CSxFXRollDataModel>();

        //this boolean will be checked in the transaction event to check if the GUI is opened 
        public static bool isOpened = false;

        public CSxFXRollingForm()
        {
            try
            {
                InitializeComponent();
                loadForwardDates();
                treeList1.BestFitColumns();
                // init ComboBox
                //repositoryItemFixedCurrencyCombo.Items.AddRange(new string[] { "CCY2", "CCY1" });
                //treeList1.Columns[12].ColumnEdit = repositoryItemFixedCurrencyCombo;

                isOpened = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Exception Occured: " + ex.Message);
            }
        }

        #region GUI call back

        private void barEditItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            DateTime date = Convert.ToDateTime(e.Item.Caption);
            dropDownButton1.Text = date.Date.ToString(datePattern, CultureInfo.CurrentCulture);
            _dataSource.ForEach(x => x.SetEstFXRate(CORE.Tools.CSxUtils.ToSophisDate(date)));
            _dataSource.ForEach(x => x.SetForwardDate(date));
            _dataSource.ForEach(x => x.SetEstAmount());
            _dataSource.ForEach(x => repositoryItemFixedCurrencyCombo.Items.AddRange(new string[] { x.CCY2, x.CCY1 }));
            treeList1.RefreshDataSource();
        }

        private void RepositoryItemDateEdit1OnDateTimeChanged(object sender, EventArgs e)
        {
            DateTime date = (sender as DateEdit).DateTime;
            _forwardDate = date;
            dropDownButton1.Text = date.Date.ToString(datePattern, CultureInfo.CurrentCulture);
            _dataSource.ForEach(x => x.SetEstFXRate(CORE.Tools.CSxUtils.ToSophisDate(date)));
            _dataSource.ForEach(x => x.SetForwardDate(date));
            _dataSource.ForEach(x => x.SetEstAmount());
            treeList1.RefreshDataSource();
        }

        private void OnGetStateImage(object sender, GetStateImageEventArgs e)
        {
            e.Node.StateImageIndex = e.Node.Level;
        }

        private void TreeList1OnCustomDrawNodeCell(object sender, CustomDrawNodeCellEventArgs e)
        {
            if (e.Node.Level == 0 && e.Column != treeListColumnName)
            {
                e.CellText = string.Empty;
            }
        }

        private void TreeList1OnNodeCellStyle(object sender, GetCustomNodeCellStyleEventArgs e)
        {
            if (e.Node.Level == 1)
            {
                e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
            }

            if (e.Column == treeListColumnCCY1 || e.Column == treeListColumnPayAmount)
            {
                var ccy = CSMCurrency.StringToCurrency(e.Node.GetDisplayText(treeListColumnCCY1));
                e.Appearance.ForeColor = getColour(ccy, e.Appearance.ForeColor);
            }
            if (e.Column == treeListColumnCCY2 || e.Column == treeListColumnRecvAmount ||
                e.Column == treeListColumnEstRecvAmount)
            {
                var ccy = CSMCurrency.StringToCurrency(e.Node.GetDisplayText(treeListColumnCCY2));
                e.Appearance.ForeColor = getColour(ccy, e.Appearance.ForeColor);
            }
            if (Convert.ToBoolean(e.Node.GetValue(treeListColumnWillRoll)))
            {
                e.Appearance.BackColor = Color.LightGoldenrodYellow;
            }
        }

        private void TreeList1OnCellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column == treeListColumnWillRoll)
            {
                var id = e.Node.GetDisplayText(treeListColumnID).ToInt32();
                var currentNode = _dataSource.Single(x => x.ID == id);
                if (currentNode != null)
                {
                    currentNode.WillRoll = Convert.ToBoolean(e.Value);
                    foreach (var one in _dataSource.Where(x => currentNode.Children.Contains(x.ID)))
                    {
                        one.WillRoll = Convert.ToBoolean(e.Value);
                    }
                    var parent = _dataSource.FirstOrDefault(x => x.ID == currentNode.ParentID);
                    if (parent != null)
                    {
                        var children = _dataSource.Where(x => parent.Children.Contains(x.ID));
                        if (parent != null && children.Count(l => l.Level == currentNode.Level) == 1)
                        {
                            parent.WillRoll = Convert.ToBoolean(e.Value);
                        }

                    }
                }
            }
            else if (e.Column == treeListColumnFixedCurrency)
            {
                var id = e.Node.GetDisplayText(treeListColumnID).ToInt32();
                var currentNode = _dataSource.Single(x => x.ID == id);
                if (currentNode != null)
                    currentNode.FixedCurrency = e.Value.ToString();
                foreach (var one in _dataSource.Where(x => currentNode.Children.Contains(x.ID)))
                {
                    one.FixedCurrency = e.Value.ToString();
                }
                var parent = _dataSource.FirstOrDefault(x => x.ID == currentNode.ParentID);
                if (parent != null)
                {
                    var children = _dataSource.Where(x => parent.Children.Contains(x.ID));
                    if (parent != null && children.Count(l => l.Level == currentNode.Level) == 1)
                    {
                        parent.FixedCurrency = e.Value.ToString();
                    }
                }
            }
            else return;

            treeList1.RefreshDataSource();
        }

        private void TreeListOnKeyUp(object sender, KeyEventArgs e)
        {
            if (treeList1.FocusedColumn == treeListColumnWillRoll) return;
            if (e.KeyCode == Keys.Space)
            {
                treeList1.FocusedNode.Expanded = !treeList1.FocusedNode.Expanded;
            }
        }

        private void OnExpiryDateEdit(object sender, CustomDisplayTextEventArgs e)
        {
            if (Convert.ToDateTime(e.Value) == DateTime.MinValue)
            {
                e.DisplayText = String.Empty;
            }
        }


        private void OnFixedCurrency(object sender, CustomDisplayTextEventArgs e)
        {
            e.DisplayText = repositoryItemFixedCurrencyCombo.Items[0].ToString();
        }


        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _dataSource = CSxDataFacade.GetFxRollViewModels(this.dateTimePickerStart.Value, this.dateTimePickerEnd.Value, _forwardDate);
            _dataSourceClone = _dataSource;
            if (this.checkBoxMAML.Checked) filterOnMAMLPositions(ref _dataSource);
            treeList1.DataSource = _dataSource;
            loadImages();
            treeList1.BestFitColumns();
            HideWarningAndEnableOrderRaising();
        }

        private void SimpleButtonExpand_Click(object sender, EventArgs e)
        {
            foreach (TreeListNode node in treeList1.Nodes)
            {
                node.Expanded = true;
            }
        }

        private void SimpleButtonCollapse_Click(object sender, EventArgs e)
        {
            foreach (TreeListNode node in treeList1.Nodes)
            {
                node.Expanded = false;
            }
        }


        private void buttonCreateOrders_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(FindForm(), "Do you want to roll the selected positions?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            double closingRatio = Convert.ToDouble(this.textEditClosing.Text) / 100.0;
            double openingRatio = Convert.ToDouble(this.textEditOpening.Text) / 100.0;


            if (CSxFXRollManager.Instance.GenerateAndSendOrders(_dataSource.Where(x => x.WillRoll).ToList(), closingRatio, openingRatio))
            {
                _dataSource.ForEach(x => x.WillRoll = false);
                treeList1.RefreshDataSource();
            }
            DisplayWarningAndDisableOrderRaising();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            XSRWinFormsAdapter<CSxFXRollingForm> adapter =
                XSRWinFormsAdapter<CSxFXRollingForm>.GetUniqueDialog("Medio FX Forward Roll", true);
            if (adapter.IsVisible)
                adapter.CloseWindow();
            //this boolean will be checked in the transaction event to check if the GUI is opened 
            isOpened = false;
        }

        private void checkBoxMAML_CheckedChanged(object sender, EventArgs e)
        {
            _dataSource = _dataSourceClone;
            if (this.checkBoxMAML.Checked) filterOnMAMLPositions(ref _dataSource);
            treeList1.DataSource = _dataSource;
        }

        #endregion

        #region Internal methods

        private Color getColour(int currency, Color defaultCouColor)
        {
            CSMCurrency ccy = CSMCurrency.GetCSRCurrency(currency);
            if (ccy != null)
            {
                var colour = new sophis.gui.SSMRgbColor();
                ccy.GetRGBColor(colour);
                return Color.FromArgb((int)Math.Round(colour.red * 255.0 / 65335.0, 0),
                    (int)Math.Round(colour.green * 255.0 / 65335.0, 0),
                    (int)Math.Round(colour.blue * 255.0 / 65335.0, 0));
            }
            return defaultCouColor;
        }

        private void loadForwardDates()
        {
            foreach (var date in CSxDataFacade.GetListOfForwarDates())
            {
                BarButtonItem item = new BarButtonItem()
                {
                    Caption = date.Date.ToString(datePattern, CultureInfo.CurrentCulture)
                };
                item.ItemClick += barEditItem1_ItemClick;
                popupMenu1.AddItem(item);
            }
            dropDownButton1.Text = CSxDataFacade.GetFirstOrDefaulgtForwardDate().ToString(datePattern, CultureInfo.CurrentCulture);
            this.dateTimePickerEnd.Value = DateTime.Today.AddMonths(3);
            this.dateTimePickerStart.Value = DateTime.Today;
        }

        private void filterOnMAMLPositions(ref List<CSxFXRollDataModel> dataList)
        {
            foreach (var mamlNode in dataList.FindAll(x => x.IsUnderMAML()))
            {
                findAllMAMLnodes(dataList, mamlNode);
            }
            dataList = dataList.FindAll(x => x.IsUnderMAML());
        }

        private void loadImages()
        {
            // Add the images manually to .resx every time the controller is modified 
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(CSxFXRollingForm));
            var imgage = (System.Drawing.Image)(resources.GetObject("currency"));
            if (imgage != null) imageList.Images.Add(imgage);
            imgage = (System.Drawing.Image)(resources.GetObject("fx_exposure"));
            if (imgage != null) imageList.Images.Add(imgage);
            imgage = (System.Drawing.Image)(resources.GetObject("fund"));
            if (imgage != null) imageList.Images.Add(imgage);
            imgage = (System.Drawing.Image)(resources.GetObject("parentOrder"));
            if (imgage != null) imageList.Images.Add(imgage);
            imgage = (System.Drawing.Image)(resources.GetObject("folder"));
            if (imgage != null) imageList.Images.Add(imgage);
            treeList1.StateImageList = imageList;
        }

        private bool hasSiblings(List<CSxFXRollDataModel> nodes, CSxFXRollDataModel currentNode)
        {
            if (currentNode != null)
            {
                var parent = nodes.FirstOrDefault(x => x.ID == currentNode.ParentID);
                if (parent != null)
                {
                    var children = nodes.FindAll(x => parent.Children.Contains(x.ID) && x.Level == (parent.Level - 1));
                    return children.Count > 1;
                }
            }
            return false;
        }

        private void findAllMAMLnodes(List<CSxFXRollDataModel> nodes, CSxFXRollDataModel mamlNode)
        {
            if (hasSiblings(nodes, mamlNode))
                return;
            else
            {
                var parent = nodes.FirstOrDefault(x => x.ID == mamlNode.ParentID);
                if (parent != null)
                {
                    if (hasSiblings(nodes, parent))
                    {
                        return;
                    }
                    else
                    {
                        parent.SetUnderMAML(mamlNode.IsUnderMAML());
                        findAllMAMLnodes(nodes, parent);
                    }
                }
            }
        }

        private void DisableWarningText()
        {
            this.richTextBox1.Hide();
        }

        private void EnableWarningText()
        {
            this.richTextBox1.Visible = true;
        }

        private void DisableOrderCreationButton()
        {
            this.buttonCreateOrders.Enabled = false;
        }

        private void EnableOrderCreationButton()
        {
            this.buttonCreateOrders.Enabled = true;
        }

        // Disable the capablity to create orders and alert the user, if refresh is required
        internal void DisplayWarningAndDisableOrderRaising()
        {
            EnableWarningText();
            DisableOrderCreationButton();
        }

        // Normal case: user will be able to raise orders, i.e. positions are synced as in portfolio
        internal void HideWarningAndEnableOrderRaising()
        {
            DisableWarningText();
            EnableOrderCreationButton();
        }


        #endregion

    }
}

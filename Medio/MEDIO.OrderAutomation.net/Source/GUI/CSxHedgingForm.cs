using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using MEDIO.OrderAutomation.net.Source.Criteria;
using MEDIO.OrderAutomation.net.Source.Data;
using MEDIO.OrderAutomation.net.Source.DataModel;
using MEDIO.OrderAutomation.NET.Source.Data;
using MEDIO.OrderAutomation.NET.Source.DataModel;
using sophis;
using sophis.oms;
using Sophis.OMS;
using Sophis.OMS.Messaging;
//using Sophis.OMS.entry.gui;
using Sophis.OMS.Executions;
using Sophis.OMS.Util;
using sophis.static_data;
using sophis.utils;
using sophis.xaml;
using sophis.oms.entry.gui;
//using sophisOrderBlotters;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{

    public partial class CSxHedgingForm : sophis.guicommon.basicDialogs.DefaultEmbeddableControl
    {
        private CSxCustomAllocationData allocations = new CSxCustomAllocationData();
        private int hotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        private CSxCustomAllocation _selectedAllocation = new CSxCustomAllocation();
        //string datePattern = "dd/MM/yyyy";
        string datePattern = "d";

        private int HotTrackRow
        {
            get { return hotTrackRow; }
            set
            {
                if (hotTrackRow != value)
                {
                    int prevHotTrackRow = hotTrackRow;
                    hotTrackRow = value;
                    gridView1.RefreshRow(prevHotTrackRow);
                    gridView1.RefreshRow(hotTrackRow);

                    if (hotTrackRow >= 0)
                        gridControl1.Cursor = Cursors.Hand;
                    else
                        gridControl1.Cursor = Cursors.Default;
                }
            }
        }

        public CSxHedgingForm()
        {
            InitializeComponent();
            this.splitContainerControl2.SplitterPosition = (int) (this.splitContainerControl2.Height * 0.93);
            allocations = CSxGUIDataManager.Instance.GetAllocations();
            this.gridControl1.DataSource = allocations.Executions;
            this.gridView1.BestFitColumns();
            LoadForwardDates();
            CSMEventManager.Instance.AddHandler(CoherencyEventId.OMS_ORDERPLACEMENT_UPDATED, OnExecCreated);
        }

        #region call backs

        private void gridView1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(new System.Drawing.Point(e.X, e.Y));
            if (info.InRowCell)
                HotTrackRow = info.RowHandle;
            else
                HotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void simpleButtonRefresh_Click(object sender, EventArgs e)
        {
            var selectedRows = GetSelectedAllocations();
            if (selectedRows.Count == 0)
            {
                allocations.Refresh();
            }
            else
            {
                foreach (var row in selectedRows)
                {
                    var newExec = OrderExecutionManager.Instance.GetExecutionById(row.Execution.SophisExecID);
                    var newOrder = OrderManagerConnector.Instance.GetOrderManager().GetOrderById(row.SingleOrder.ID);
                    var toAdd = CSxDataFacade.GetAllocations(newExec, (SingleOrder)newOrder);
                    allocations.UpdateExecutionFromCache(toAdd);
                }
            }
            this.gridControl1.DataSource = allocations.Executions;
            gridControl1.RefreshDataSource();
        }

        private void gridView1_RowClick(object sender, RowClickEventArgs e)
        {
            try
            {
                gridView1.GetDataSourceRowIndex(e.RowHandle);
                int row = Convert.ToInt32(e.RowHandle);
                if (row < 0) return;

                var alloc = allocations[gridView1.GetDataSourceRowIndex(e.RowHandle)];
                if (alloc == null)
                    return;

                if (gridView1.FocusedColumn.FieldName == "Checked")
                {
                    if (alloc.Checked)
                        alloc.Checked = false;
                    else
                        alloc.Checked = true;
                }
                _selectedAllocation = alloc;
                if (e.Clicks == 2 && e.RowHandle >= 0 && e.Button == MouseButtons.Left)
                {
                    OrderAdapter adapter = OrderAdapter.Adapt(_selectedAllocation.SingleOrder);
                    if (adapter != null && _selectedAllocation.Execution != null)
                        ExecutionDlg.Display(_selectedAllocation.Execution, adapter);
                }
            }
            catch ///TODO remove (Exception exception)
            {
                // continue
            }
        }

        // Upper panel right click
        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == GridMenuType.Row)
            {
                e.Menu.Items.Clear();
                DXMenuItem item = new DXMenuCheckItem("Create orders", false, null, new EventHandler(popupMenu_Click));
                e.Menu.Items.Add(item);
                e.Menu.ShowHotKeyPrefix = true;        
            }
        }

        private void popupMenu_Click(object sender, EventArgs e)
        {
            createOrderButton_Click(sender, e);
        }

        private void toolTipController1_GetActiveObjectInfo(object sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            ToolTipControlInfo info = null;
            GridView view = gridControl1.GetViewAt(e.ControlMousePosition) as GridView;
            if (view == null) return;
            GridHitInfo hi = view.CalcHitInfo(e.ControlMousePosition);
            
            if (hi.RowHandle >= 0)
            {
                var row = allocations[hi.RowHandle];
                object o = hi.HitTest.ToString() + hi.RowHandle.ToString();
                string text = "";
                if (hi.Column == gridColumnFundingAmountCCY1 || hi.Column == gridColumnHedgingAmountCCY1)
                    text = row.FolioCCY;
                else if (hi.Column == gridColumnFundingAmountCCY2 || hi.Column == gridColumnHedgingAmountCCY2)
                    text = row.InstrumentCCY;
                info = new ToolTipControlInfo(o, text);
                if (info != null)
                    info.IconType = ToolTipIconType.Information;
                e.Info = info;
            }
        }

        private void repositoryItemComboBoxExecutionState_CustomDisplayText(object sender, CustomDisplayTextEventArgs e)
        {
            if ((int)e.Value == (int) MedioConstants.EOrderExecutionState.TotallyExecuted)
            {
                e.DisplayText = "Totally Executed";
            }
            else if ((int)e.Value == (int) MedioConstants.EOrderExecutionState.PartiallyExecuted)
            {
                e.DisplayText = "Partially Executed";
            }
            else
            {
                e.DisplayText = "Unkwnown";
            }
        }

        private void simpleButtonApplyAll_Click(object sender, EventArgs e)
        {
            foreach (var allocation in allocations.Executions)
            {
                allocation.HedgingProportion = Convert.ToDouble(this.textEditHedging.Text);
                allocation.FundingProportion = Convert.ToDouble(this.textEditFunding.Text);
                allocation.ForwardDate = DateTime.Parse(dropDownButton1.Text.Trim());
                allocation.HedgingAmountCCY1 = allocation.originalHedgingAmountCCY1 * allocation.HedgingProportion / 100;
                allocation.FundingAmountCCY1 = allocation.originalFundingAmountCCY1 * allocation.FundingProportion / 100;
                allocation.HedgingAmountCCY2 = allocation.originalHedgingAmountCCY2 * allocation.HedgingProportion / 100;
                allocation.FundingAmountCCY2 = allocation.originalFundingAmountCCY2 * allocation.FundingProportion / 100;
            }
            this.gridControl1.DataSource = allocations.Executions;
            gridControl1.RefreshDataSource();
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            foreach (var allocation in GetSelectedAllocations())
            {
                allocation.HedgingProportion = Convert.ToDouble(this.textEditHedging.Text);
                allocation.FundingProportion = Convert.ToDouble(this.textEditFunding.Text);
                allocation.ForwardDate = DateTime.Parse(dropDownButton1.Text.Trim());
                allocation.HedgingAmountCCY1 = allocation.originalHedgingAmountCCY1 * allocation.HedgingProportion / 100;
                allocation.FundingAmountCCY1 = allocation.originalFundingAmountCCY1 * allocation.FundingProportion / 100;
                allocation.HedgingAmountCCY2 = allocation.originalHedgingAmountCCY2 * allocation.HedgingProportion / 100;
                allocation.FundingAmountCCY2 = allocation.originalFundingAmountCCY2 * allocation.FundingProportion / 100;
            }
            this.gridControl1.DataSource = allocations.Executions;
            gridControl1.RefreshDataSource();
        }

        private void createOrderButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(FindForm(), "Do you want to create the hedging/funding orders?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            var rows = GetSelectedAllocations();
            if (!ValidateSelectedAllocations(rows)) return;

            if (CSxDataFacade.CreateHedgingFundingOrders(rows))
            {
                allocations.RemoveExecutionFromCache(rows);
            }
            this.gridControl1.DataSource = allocations.Executions;
            this.gridControl1.RefreshDataSource();
        }

        private void OnExecCreated(String eventType, Object eveventParams)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                LOG.Write(CSMLog.eMVerbosity.M_info, "order execution received! event type = " + eventType);

                var orderPlacements = eveventParams as IList<OrderPlacement>;
                foreach (var orderPlacement in orderPlacements)
                {
                    var singleOrder = OrderManager.Instance.GetOrderById(orderPlacement.OrderId) as SingleOrder;
                    if (singleOrder == null) return;
                    
                    if( CSxDataFacade.ShouldBeFiltered(singleOrder) == true)
                    {
                        return;
                    }
                  
                    var executions = OrderExecutionManager.Instance.GetExecutionsForOrders(new[] { orderPlacement.OrderId });

                    foreach (var exec in executions)
                    {
                        var toAdd = CSxDataFacade.GetAllocations(exec, singleOrder);
                        allocations.UpdateExecutionFromCache(toAdd);
                    }
                }
            }
            this.gridControl1.DataSource = allocations.Executions;
            this.gridControl1.RefreshDataSource();
        }

        private void GridControl1OnDataSourceChanged(object sender, EventArgs eventArgs)
        {
            this.gridControl1.RefreshDataSource();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            XSRWinFormsAdapter<CSxHedgingForm> adapter = XSRWinFormsAdapter<CSxHedgingForm>.GetUniqueDialog("Medio Hedging & Funding Blotter", true);
            if (adapter.IsVisible)
                adapter.CloseWindow();
        }

        private void gridView1_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            var alloc = GetAllocationByRowHandle(e.RowHandle);
            if (alloc == null) return;
            if (e.Column == gridColumnSpotPct)
            {
                alloc.FundingProportion = FormattingStringToDouble(e.Value);
                alloc.FundingAmountCCY1 = alloc.originalFundingAmountCCY1 * alloc.FundingProportion/100;
                alloc.FundingAmountCCY2 = alloc.originalFundingAmountCCY2 * alloc.FundingProportion/100;
            }
            else if (e.Column == gridColumnHedgingPct)
            {
                alloc.HedgingProportion = FormattingStringToDouble(e.Value);
                alloc.HedgingAmountCCY1 = alloc.originalHedgingAmountCCY1 * alloc.HedgingProportion/100;
                alloc.HedgingAmountCCY2 = alloc.originalHedgingAmountCCY2 * alloc.HedgingProportion/100;
            }
            else if (e.Column == gridColumnFundingAmountCCY1)
            {
                alloc.FundingAmountCCY1 = FormattingStringToDouble(e.Value);
                alloc.FundingProportion = alloc.originalFundingAmountCCY1 == 0 ? 0 : Math.Round(alloc.FundingAmountCCY1 / alloc.originalFundingAmountCCY1 * 100,2);
            }
            else if (e.Column == gridColumnFundingAmountCCY2)
            {
                alloc.FundingAmountCCY2 = FormattingStringToDouble(e.Value);
                alloc.FundingProportion = alloc.originalFundingAmountCCY2 == 0 ? 0 : Math.Round(alloc.FundingAmountCCY2 / alloc.originalFundingAmountCCY2 * 100,2);
            }
            else if (e.Column == gridColumnHedgingAmountCCY1)
            {
                alloc.HedgingAmountCCY1 = FormattingStringToDouble(e.Value);
                alloc.HedgingProportion = alloc.originalHedgingAmountCCY1 == 0 ? 0 : Math.Round(alloc.HedgingAmountCCY1 / alloc.originalHedgingAmountCCY1 * 100, 2);
            }
            else if (e.Column == gridColumnHedgingAmountCCY2)
            {
                alloc.HedgingAmountCCY2 = FormattingStringToDouble(e.Value);
                alloc.HedgingProportion = alloc.originalHedgingAmountCCY2 == 0 ? 0 : Math.Round(alloc.HedgingAmountCCY2 / alloc.originalHedgingAmountCCY2 * 100, 2);
            }
            this.gridControl1.DataSource = allocations.Executions;
            this.gridControl1.RefreshDataSource();
        }

        private void GridView1OnValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (       view.FocusedColumn == gridColumnFundingAmountCCY1
                    || view.FocusedColumn == gridColumnFundingAmountCCY2
                    || view.FocusedColumn == gridColumnHedgingAmountCCY1
                    || view.FocusedColumn == gridColumnHedgingAmountCCY2
                )
            {
                e.Value = FormattingStringToDouble(e.Value);
            }
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            var row = GetAllocationByRowHandle(e.RowHandle);
            if (row == null) return;

            int ccy1 = row.FolioCCYCode;
            int ccy2 = row.InstrumentCCYCode;
            if (e.Column == gridColumnCCY1)
            {
                e.Appearance.ForeColor = GetColour(ccy1);
            }
            else if (e.Column == gridColumnCCY2)
            {
                e.Appearance.ForeColor = GetColour(ccy2);
            }
            else if(e.Column == gridColumnFundingAmountCCY1)
            {
                e.Appearance.ForeColor = GetColour(ccy1);
            }
            else if (e.Column == gridColumnHedgingAmountCCY1)
            {
                e.Appearance.ForeColor = GetColour(ccy1);
            }
            else if (e.Column == gridColumnFundingAmountCCY2)
            {
                e.Appearance.ForeColor = GetColour(ccy2);
            }
            else if (e.Column == gridColumnHedgingAmountCCY2)
            {
                e.Appearance.ForeColor = GetColour(ccy2);
            }
        }

        private void simpleButton1_SizeChanged(object sender, EventArgs e)
        {
            simpleButtonApply.Image = new Bitmap(this.simpleButtonApply.Image, new Size(simpleButtonApply.Size.Width - 8, simpleButtonApply.Size.Height - 5));
        }

        private void barEditItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            DateTime date = Convert.ToDateTime(e.Item.Caption);
            dropDownButton1.Text = date.Date.ToString(datePattern, CultureInfo.CurrentCulture);
        }

        private void RepositoryItemDateEdit1OnDateTimeChanged(object sender, EventArgs e)
        {
            DateTime date = (sender as DateEdit).DateTime;
            dropDownButton1.Text = date.Date.ToString(datePattern, CultureInfo.CurrentCulture);
        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (gridView1.GetSelectedRows().IsNullOrEmpty()) return;
                var alloc = allocations[gridView1.GetDataSourceRowIndex(gridView1.GetSelectedRows()[0])];
                if (alloc == null)
                    return;
                if (alloc.SingleOrder != null)
                    EntryBox.Display(alloc.SingleOrder);
            }
            catch //TODO clean (Exception exception)
            {
               // continue
            }
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (gridView1.GetSelectedRows().IsNullOrEmpty()) return;
                var alloc = allocations[gridView1.GetDataSourceRowIndex(gridView1.GetSelectedRows()[0])];
                if (alloc == null)
                    return;
                OrderAdapter adapter = OrderAdapter.Adapt(alloc.SingleOrder);
                if (alloc.Execution != null && adapter != null)
                    ExecutionDlg.Display(alloc.Execution, adapter);
            }
            catch //TODO remove (Exception exception)
            {
                // continue
            }
        }
        #endregion

        #region Internal methods
        private Color GetColour(int currency)
        {
            CSMCurrency ccy = CSMCurrency.GetCSRCurrency(currency);
            if (ccy != null)
            {
                var colour = new sophis.gui.SSMRgbColor();
                ccy.GetRGBColor(colour);
                return System.Drawing.Color.FromArgb((int)Math.Round(colour.red * 255.0 / 65335.0, 0), (int)Math.Round(colour.green * 255.0 / 65335.0, 0), (int)Math.Round(colour.blue * 255.0 / 65335.0, 0));
            }
            return Color.Black;
        }

        private List<CSxCustomAllocation> GetSelectedAllocations()
        {
            int[] ids = gridView1.GetSelectedRows();
            var selectedAllocations = new List<CSxCustomAllocation>();
            foreach (var id in ids)
            {
                if (gridView1.IsGroupRow(id))
                    GetChildRows(gridView1, id, selectedAllocations);
                else if (id >= 0)
                {
                    var alloc = allocations[gridView1.GetDataSourceRowIndex(id)];
                    if (!selectedAllocations.Contains(alloc))
                        selectedAllocations.Add(alloc);
                }
            }
            return selectedAllocations;
        }

        private void GetChildRows(GridView view, int groupRowHandle, List<CSxCustomAllocation> childRows)
        {
            if (!view.IsGroupRow(groupRowHandle)) return;
            // Get the number of immediate children 
            int childCount = view.GetChildRowCount(groupRowHandle);
            for (int i = 0; i < childCount; i++)
            {
                //Get the handle of a child row with the required index 
                int childHandle = view.GetChildRowHandle(groupRowHandle, i);
                //If the child is a group row, then add its children to the list 
                if (view.IsGroupRow(childHandle))
                    GetChildRows(view, childHandle, childRows);
                else
                {
                    // The child is a data row.  
                    // Add it to the childRows as long as the row wasn't added before 
                    if (childHandle >= 0)
                    {
                        var alloc = allocations[gridView1.GetDataSourceRowIndex(childHandle)];
                        if (alloc != null)
                        {
                            if (!childRows.Contains(alloc))
                                childRows.Add(alloc);
                        }
                    }
                }
            }
        }

        private CSxCustomAllocation GetAllocationByRowHandle(int rowHandle)
        {
            rowHandle = gridView1.GetDataSourceRowIndex(rowHandle);
            if (rowHandle >= 0)
            {
                if (allocations.Count - 1 < rowHandle) return null;
                return allocations[rowHandle];
            }
            return null;
        }

        private void LoadForwardDates()
        {
            foreach (var date in CSxDataFacade.GetListOfForwarDates())
            {
                BarButtonItem item = new BarButtonItem() { Caption = date.Date.ToString(datePattern, CultureInfo.CurrentCulture) };
                item.ItemClick += barEditItem1_ItemClick;
                popupMenu1.AddItem(item);
            }
            dropDownButton1.Text = CSxDataFacade.GetFirstOrDefaulgtForwardDate().ToString(datePattern, CultureInfo.CurrentCulture);
        }

        private double FormattingStringToDouble(object obj)
        {
            double res = 0;
            string str = Convert.ToString(obj);
            if (str.Contains('k') || str.Contains('K'))
            {
                var regex = new Regex("k", RegexOptions.IgnoreCase);
                res = Convert.ToDouble(regex.Replace(str, "").Trim()) * 1000;
            }
            else if (str.Contains('m') || str.Contains('M'))
            {
                var regex = new Regex("m", RegexOptions.IgnoreCase);
                res = Convert.ToDouble(regex.Replace(str, "").Trim()) * 1000000;
            }
            else if (str.Contains('b') || str.Contains('B'))
            {
                var regex = new Regex("b", RegexOptions.IgnoreCase);
                res = Convert.ToDouble(regex.Replace(str, "").Trim()) * 1000000000;
            }
            else
            {
                res = Convert.ToDouble(obj);
            }
            return Math.Round(res, 2);
        }

        private bool ValidateSelectedAllocations(List<CSxCustomAllocation> selectAllocations)
        {
            List<CSxCustomAllocation> res = selectAllocations;
            string warning = "Warning";
            
            // Cross orders check 
            int orderCount = selectAllocations.Select(x => x.OrderID).Distinct().Count();
            if (orderCount != 1)
            {
                MessageBox.Show("Creating hedging/funding orders across different parent orders is not allowed, please check.", warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            // Allocation folio ccy check
            int folioCCYcount = selectAllocations.Select(x => x.FolioCCYCode).Distinct().Count();
            if (folioCCYcount != 1)
            {
                MessageBox.Show(FindForm(), "Selected allocations have different folio currencies, please check.", warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Forward dates check
            int forwardDatesCount = selectAllocations.Select(x => x.ForwardDate).Distinct().Count();
            if (forwardDatesCount != 1)
            {
                MessageBox.Show(FindForm(), "Selected allocations have different forward dates, please check.", warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Funding amount check
            {
            }

            // Hegding amount check
            {
            }

            return true;
        }
        #endregion

      

        

    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    partial class CSxHedgingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CSxHedgingForm));
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.simpleButtonApplyAll = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButtonRefresh = new DevExpress.XtraEditors.SimpleButton();
            this.dropDownButton1 = new DevExpress.XtraEditors.DropDownButton();
            this.popupMenu1 = new DevExpress.XtraBars.PopupMenu(this.components);
            this.barListItem1 = new DevExpress.XtraBars.BarListItem();
            this.barEditItem4 = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemDateEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.barEditItem1 = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemCalcEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCalcEdit();
            this.barEditItem2 = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.barEditItem3 = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit3 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.simpleButtonApply = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.textEditHedging = new DevExpress.XtraEditors.TextEdit();
            this.textEditFunding = new DevExpress.XtraEditors.TextEdit();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.splitContainerControl2 = new DevExpress.XtraEditors.SplitContainerControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumnOrderID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.gridColumnInstr = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnOrderCreationDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnOrderUser = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnExecID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnExecutionDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnExecSide = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnExecAmount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnExecQty = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnInitPct = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnExecPrice = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnFolioPath = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnFolioName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnFolioCCY = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnFolioQty = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnSpotPct = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnHedgingPct = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnNAVFunding = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnNAVHedging = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnFundingAmountCCY1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnHedgingAmountCCY1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnCCY1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnFundingAmountCCY2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnHedgingAmountCCY2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnCCY2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnForwardDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemDateEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.gridColumnExecState = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem3 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.gridColumnExecutionValueDate = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1.VistaTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCalcEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditHedging.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditFunding.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl2)).BeginInit();
            this.splitContainerControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2.VistaTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem3)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Horizontal = false;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 27);
            this.splitContainerControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.Controls.Add(this.groupControl1);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.Controls.Add(this.splitContainerControl2);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(1892, 764);
            this.splitContainerControl1.SplitterPosition = 104;
            this.splitContainerControl1.TabIndex = 4;
            this.splitContainerControl1.Text = "splitContainerControl1";
            // 
            // groupControl1
            // 
            this.groupControl1.AllowDrop = true;
            this.groupControl1.Controls.Add(this.simpleButtonApplyAll);
            this.groupControl1.Controls.Add(this.simpleButtonRefresh);
            this.groupControl1.Controls.Add(this.dropDownButton1);
            this.groupControl1.Controls.Add(this.simpleButtonApply);
            this.groupControl1.Controls.Add(this.labelControl10);
            this.groupControl1.Controls.Add(this.labelControl8);
            this.groupControl1.Controls.Add(this.labelControl7);
            this.groupControl1.Controls.Add(this.textEditHedging);
            this.groupControl1.Controls.Add(this.textEditFunding);
            this.groupControl1.Controls.Add(this.labelControl6);
            this.groupControl1.Controls.Add(this.labelControl5);
            this.groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl1.Location = new System.Drawing.Point(0, 0);
            this.groupControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(1892, 104);
            this.groupControl1.TabIndex = 2;
            this.groupControl1.Text = "Funding and Hedging";
            // 
            // simpleButtonApplyAll
            // 
            this.simpleButtonApplyAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.simpleButtonApplyAll.Image = ((System.Drawing.Image)(resources.GetObject("simpleButtonApplyAll.Image")));
            this.simpleButtonApplyAll.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.simpleButtonApplyAll.Location = new System.Drawing.Point(796, 53);
            this.simpleButtonApplyAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.simpleButtonApplyAll.Name = "simpleButtonApplyAll";
            this.simpleButtonApplyAll.Size = new System.Drawing.Size(60, 55);
            this.simpleButtonApplyAll.TabIndex = 32;
            this.simpleButtonApplyAll.Text = "Apply";
            this.simpleButtonApplyAll.ToolTip = "Apply the changes to all the rows";
            this.simpleButtonApplyAll.Click += new System.EventHandler(this.simpleButtonApplyAll_Click);
            // 
            // simpleButtonRefresh
            // 
            this.simpleButtonRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.simpleButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("simpleButtonRefresh.Image")));
            this.simpleButtonRefresh.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.simpleButtonRefresh.Location = new System.Drawing.Point(877, 52);
            this.simpleButtonRefresh.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.simpleButtonRefresh.Name = "simpleButtonRefresh";
            this.simpleButtonRefresh.Size = new System.Drawing.Size(60, 55);
            this.simpleButtonRefresh.TabIndex = 31;
            this.simpleButtonRefresh.ToolTip = "Reload selected positions or all if none is selected";
            this.simpleButtonRefresh.Click += new System.EventHandler(this.simpleButtonRefresh_Click);
            // 
            // dropDownButton1
            // 
            this.dropDownButton1.AllowDrop = true;
            this.dropDownButton1.AllowFocus = false;
            this.dropDownButton1.DropDownControl = this.popupMenu1;
            this.dropDownButton1.Location = new System.Drawing.Point(487, 84);
            this.dropDownButton1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dropDownButton1.Name = "dropDownButton1";
            this.dropDownButton1.Size = new System.Drawing.Size(160, 23);
            this.dropDownButton1.TabIndex = 30;
            this.dropDownButton1.Text = " ";
            // 
            // popupMenu1
            // 
            this.popupMenu1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barListItem1),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barEditItem4, DevExpress.XtraBars.BarItemPaintStyle.Standard)});
            this.popupMenu1.Manager = this.barManager1;
            this.popupMenu1.Name = "popupMenu1";
            // 
            // barListItem1
            // 
            this.barListItem1.Caption = "barListItem1";
            this.barListItem1.Id = 1;
            this.barListItem1.Name = "barListItem1";
            // 
            // barEditItem4
            // 
            this.barEditItem4.Edit = this.repositoryItemDateEdit1;
            this.barEditItem4.Id = 4;
            this.barEditItem4.Name = "barEditItem4";
            // 
            // repositoryItemDateEdit1
            // 
            this.repositoryItemDateEdit1.AutoHeight = false;
            this.repositoryItemDateEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemDateEdit1.Name = "repositoryItemDateEdit1";
            this.repositoryItemDateEdit1.VistaTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.repositoryItemDateEdit1.DateTimeChanged += new System.EventHandler(this.RepositoryItemDateEdit1OnDateTimeChanged);
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barEditItem1,
            this.barListItem1,
            this.barEditItem2,
            this.barEditItem3,
            this.barEditItem4,
            this.barButtonItem1,
            this.barButtonItem2});
            this.barManager1.MainMenu = this.bar1;
            this.barManager1.MaxItemId = 7;
            this.barManager1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCalcEdit1,
            this.repositoryItemTextEdit2,
            this.repositoryItemTextEdit3,
            this.repositoryItemDateEdit1});
            // 
            // bar1
            // 
            this.bar1.BarName = "Custom 2";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem1),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItem2, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.OptionsBar.MultiLine = true;
            this.bar1.OptionsBar.UseWholeRow = true;
            this.bar1.Text = "Custom 2";
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "Edit Order";
            this.barButtonItem1.Id = 5;
            this.barButtonItem1.Name = "barButtonItem1";
            this.barButtonItem1.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem1_ItemClick);
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Caption = "Edit Execution";
            this.barButtonItem2.Id = 6;
            this.barButtonItem2.Name = "barButtonItem2";
            this.barButtonItem2.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem2_ItemClick);
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.barDockControlTop.Size = new System.Drawing.Size(1892, 27);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 791);
            this.barDockControlBottom.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.barDockControlBottom.Size = new System.Drawing.Size(1892, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 27);
            this.barDockControlLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 764);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1892, 27);
            this.barDockControlRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.barDockControlRight.Size = new System.Drawing.Size(0, 764);
            // 
            // barEditItem1
            // 
            this.barEditItem1.Caption = "barEditItem1";
            this.barEditItem1.Edit = this.repositoryItemCalcEdit1;
            this.barEditItem1.Id = 0;
            this.barEditItem1.Name = "barEditItem1";
            this.barEditItem1.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barEditItem1_ItemClick);
            // 
            // repositoryItemCalcEdit1
            // 
            this.repositoryItemCalcEdit1.AutoHeight = false;
            this.repositoryItemCalcEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemCalcEdit1.Name = "repositoryItemCalcEdit1";
            // 
            // barEditItem2
            // 
            this.barEditItem2.Caption = "barEditItem2";
            this.barEditItem2.Edit = this.repositoryItemTextEdit2;
            this.barEditItem2.Id = 2;
            this.barEditItem2.Name = "barEditItem2";
            // 
            // repositoryItemTextEdit2
            // 
            this.repositoryItemTextEdit2.AutoHeight = false;
            this.repositoryItemTextEdit2.Name = "repositoryItemTextEdit2";
            // 
            // barEditItem3
            // 
            this.barEditItem3.Caption = "barEditItem3";
            this.barEditItem3.Edit = this.repositoryItemTextEdit3;
            this.barEditItem3.Id = 3;
            this.barEditItem3.Name = "barEditItem3";
            // 
            // repositoryItemTextEdit3
            // 
            this.repositoryItemTextEdit3.AutoHeight = false;
            this.repositoryItemTextEdit3.Name = "repositoryItemTextEdit3";
            // 
            // simpleButtonApply
            // 
            this.simpleButtonApply.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.simpleButtonApply.Image = ((System.Drawing.Image)(resources.GetObject("simpleButtonApply.Image")));
            this.simpleButtonApply.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.simpleButtonApply.Location = new System.Drawing.Point(713, 53);
            this.simpleButtonApply.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.simpleButtonApply.Name = "simpleButtonApply";
            this.simpleButtonApply.Size = new System.Drawing.Size(60, 55);
            this.simpleButtonApply.TabIndex = 26;
            this.simpleButtonApply.Text = "Apply";
            this.simpleButtonApply.ToolTip = "Apply the changes to only selected rows";
            this.simpleButtonApply.SizeChanged += new System.EventHandler(this.simpleButton1_SizeChanged);
            this.simpleButtonApply.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // labelControl10
            // 
            this.labelControl10.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelControl10.Location = new System.Drawing.Point(340, 88);
            this.labelControl10.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.labelControl10.Name = "labelControl10";
            this.labelControl10.Size = new System.Drawing.Size(93, 17);
            this.labelControl10.TabIndex = 16;
            this.labelControl10.Text = "Forward date";
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(233, 90);
            this.labelControl8.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(12, 16);
            this.labelControl8.TabIndex = 13;
            this.labelControl8.Text = "%";
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(233, 52);
            this.labelControl7.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(12, 16);
            this.labelControl7.TabIndex = 12;
            this.labelControl7.Text = "%";
            // 
            // textEditHedging
            // 
            this.textEditHedging.EditValue = "100";
            this.textEditHedging.Location = new System.Drawing.Point(119, 86);
            this.textEditHedging.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textEditHedging.Name = "textEditHedging";
            this.textEditHedging.Size = new System.Drawing.Size(98, 22);
            this.textEditHedging.TabIndex = 11;
            // 
            // textEditFunding
            // 
            this.textEditFunding.EditValue = "100";
            this.textEditFunding.Location = new System.Drawing.Point(119, 50);
            this.textEditFunding.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textEditFunding.Name = "textEditFunding";
            this.textEditFunding.Size = new System.Drawing.Size(98, 22);
            this.textEditFunding.TabIndex = 10;
            // 
            // labelControl6
            // 
            this.labelControl6.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelControl6.Location = new System.Drawing.Point(12, 90);
            this.labelControl6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(58, 17);
            this.labelControl6.TabIndex = 9;
            this.labelControl6.Text = "Hedging";
            // 
            // labelControl5
            // 
            this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelControl5.Location = new System.Drawing.Point(14, 52);
            this.labelControl5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(56, 17);
            this.labelControl5.TabIndex = 8;
            this.labelControl5.Text = "Funding";
            // 
            // splitContainerControl2
            // 
            this.splitContainerControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl2.FixedPanel = DevExpress.XtraEditors.SplitFixedPanel.None;
            this.splitContainerControl2.Horizontal = false;
            this.splitContainerControl2.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainerControl2.Name = "splitContainerControl2";
            this.splitContainerControl2.Panel1.Controls.Add(this.gridControl1);
            this.splitContainerControl2.Panel1.Text = "Panel1";
            this.splitContainerControl2.Panel2.Controls.Add(this.layoutControl1);
            this.splitContainerControl2.Panel2.Text = "Panel2";
            this.splitContainerControl2.Size = new System.Drawing.Size(1892, 655);
            this.splitContainerControl2.SplitterPosition = 583;
            this.splitContainerControl2.TabIndex = 4;
            this.splitContainerControl2.Text = "splitContainerControl2";
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemTextEdit1,
            this.repositoryItemComboBox1,
            this.repositoryItemDateEdit2});
            this.gridControl1.Size = new System.Drawing.Size(1892, 583);
            this.gridControl1.TabIndex = 3;
            this.gridControl1.UseEmbeddedNavigator = true;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Appearance.OddRow.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.gridView1.Appearance.OddRow.Options.UseBackColor = true;
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumnOrderID,
            this.gridColumnInstr,
            this.gridColumnOrderCreationDate,
            this.gridColumnOrderUser,
            this.gridColumnExecID,
            this.gridColumnExecutionDate,
            this.gridColumnExecutionValueDate,
            this.gridColumnExecSide,
            this.gridColumnExecAmount,
            this.gridColumnExecQty,
            this.gridColumnInitPct,
            this.gridColumnExecPrice,
            this.gridColumnFolioPath,
            this.gridColumnFolioName,
            this.gridColumnFolioCCY,
            this.gridColumnFolioQty,
            this.gridColumnSpotPct,
            this.gridColumnHedgingPct,
            this.gridColumnNAVFunding,
            this.gridColumnNAVHedging,
            this.gridColumnFundingAmountCCY1,
            this.gridColumnHedgingAmountCCY1,
            this.gridColumnCCY1,
            this.gridColumnFundingAmountCCY2,
            this.gridColumnHedgingAmountCCY2,
            this.gridColumnCCY2,
            this.gridColumnForwardDate,
            this.gridColumnExecState});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.HorzScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Always;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsView.ColumnAutoWidth = false;
            this.gridView1.OptionsView.ShowAutoFilterRow = true;
            this.gridView1.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            this.gridView1.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gridView1_RowCellStyle);
            this.gridView1.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView1_PopupMenuShowing);
            this.gridView1.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridView1_CellValueChanged);
            this.gridView1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gridView1_MouseMove);
            this.gridView1.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.GridView1OnValidatingEditor);
            // 
            // gridColumnOrderID
            // 
            this.gridColumnOrderID.Caption = "Order ID";
            this.gridColumnOrderID.ColumnEdit = this.repositoryItemTextEdit1;
            this.gridColumnOrderID.FieldName = "OrderID";
            this.gridColumnOrderID.Name = "gridColumnOrderID";
            this.gridColumnOrderID.OptionsColumn.AllowEdit = false;
            this.gridColumnOrderID.Visible = true;
            this.gridColumnOrderID.VisibleIndex = 0;
            this.gridColumnOrderID.Width = 88;
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // gridColumnInstr
            // 
            this.gridColumnInstr.Caption = "Instrument";
            this.gridColumnInstr.FieldName = "InstrumentRef";
            this.gridColumnInstr.Name = "gridColumnInstr";
            this.gridColumnInstr.OptionsColumn.AllowEdit = false;
            this.gridColumnInstr.Visible = true;
            this.gridColumnInstr.VisibleIndex = 1;
            // 
            // gridColumnOrderCreationDate
            // 
            this.gridColumnOrderCreationDate.Caption = "Order Creation Date";
            this.gridColumnOrderCreationDate.FieldName = "OrderCreationDate";
            this.gridColumnOrderCreationDate.Name = "gridColumnOrderCreationDate";
            this.gridColumnOrderCreationDate.OptionsColumn.AllowEdit = false;
            this.gridColumnOrderCreationDate.Visible = true;
            this.gridColumnOrderCreationDate.VisibleIndex = 2;
            this.gridColumnOrderCreationDate.Width = 110;
            // 
            // gridColumnOrderUser
            // 
            this.gridColumnOrderUser.Caption = "Order Creation User";
            this.gridColumnOrderUser.FieldName = "OrderCreationUser";
            this.gridColumnOrderUser.Name = "gridColumnOrderUser";
            this.gridColumnOrderUser.OptionsColumn.AllowEdit = false;
            this.gridColumnOrderUser.Visible = true;
            this.gridColumnOrderUser.VisibleIndex = 3;
            this.gridColumnOrderUser.Width = 65;
            // 
            // gridColumnExecID
            // 
            this.gridColumnExecID.Caption = "Execution ID";
            this.gridColumnExecID.FieldName = "ExecutionID";
            this.gridColumnExecID.Name = "gridColumnExecID";
            this.gridColumnExecID.OptionsColumn.AllowEdit = false;
            this.gridColumnExecID.Visible = true;
            this.gridColumnExecID.VisibleIndex = 4;
            // 
            // gridColumnExecutionDate
            // 
            this.gridColumnExecutionDate.Caption = "Execution Date";
            this.gridColumnExecutionDate.FieldName = "ExecutionDate";
            this.gridColumnExecutionDate.Name = "gridColumnExecutionDate";
            this.gridColumnExecutionDate.OptionsColumn.AllowEdit = false;
            this.gridColumnExecutionDate.Visible = true;
            this.gridColumnExecutionDate.VisibleIndex = 6;
            this.gridColumnExecutionDate.Width = 65;
            // 
            // gridColumnExecSide
            // 
            this.gridColumnExecSide.Caption = "Execution Side";
            this.gridColumnExecSide.FieldName = "ExecSide";
            this.gridColumnExecSide.Name = "gridColumnExecSide";
            this.gridColumnExecSide.OptionsColumn.AllowEdit = false;
            this.gridColumnExecSide.Visible = true;
            this.gridColumnExecSide.VisibleIndex = 7;
            this.gridColumnExecSide.Width = 65;
            // 
            // gridColumnExecAmount
            // 
            this.gridColumnExecAmount.Caption = "Executed Amount";
            this.gridColumnExecAmount.FieldName = "ExecAmount";
            this.gridColumnExecAmount.Name = "gridColumnExecAmount";
            this.gridColumnExecAmount.OptionsColumn.AllowEdit = false;
            this.gridColumnExecAmount.Visible = true;
            this.gridColumnExecAmount.VisibleIndex = 8;
            this.gridColumnExecAmount.Width = 82;
            // 
            // gridColumnExecQty
            // 
            this.gridColumnExecQty.Caption = "Executed Qty";
            this.gridColumnExecQty.FieldName = "ExecQty";
            this.gridColumnExecQty.Name = "gridColumnExecQty";
            this.gridColumnExecQty.OptionsColumn.AllowEdit = false;
            this.gridColumnExecQty.Visible = true;
            this.gridColumnExecQty.VisibleIndex = 9;
            // 
            // gridColumnInitPct
            // 
            this.gridColumnInitPct.Caption = "Executed Qty in %";
            this.gridColumnInitPct.DisplayFormat.FormatString = "{0:n2}";
            this.gridColumnInitPct.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumnInitPct.FieldName = "AlloactionPct";
            this.gridColumnInitPct.Name = "gridColumnInitPct";
            this.gridColumnInitPct.OptionsColumn.AllowEdit = false;
            this.gridColumnInitPct.Visible = true;
            this.gridColumnInitPct.VisibleIndex = 10;
            // 
            // gridColumnExecPrice
            // 
            this.gridColumnExecPrice.Caption = "Execution Price";
            this.gridColumnExecPrice.FieldName = "ExecPrice";
            this.gridColumnExecPrice.Name = "gridColumnExecPrice";
            this.gridColumnExecPrice.OptionsColumn.AllowEdit = false;
            this.gridColumnExecPrice.Visible = true;
            this.gridColumnExecPrice.VisibleIndex = 11;
            // 
            // gridColumnFolioPath
            // 
            this.gridColumnFolioPath.Caption = "Portfolio path";
            this.gridColumnFolioPath.FieldName = "FolioPath";
            this.gridColumnFolioPath.Name = "gridColumnFolioPath";
            this.gridColumnFolioPath.OptionsColumn.AllowEdit = false;
            this.gridColumnFolioPath.Visible = true;
            this.gridColumnFolioPath.VisibleIndex = 12;
            this.gridColumnFolioPath.Width = 119;
            // 
            // gridColumnFolioName
            // 
            this.gridColumnFolioName.Caption = "Portfolio";
            this.gridColumnFolioName.FieldName = "FolioName";
            this.gridColumnFolioName.Name = "gridColumnFolioName";
            this.gridColumnFolioName.OptionsColumn.AllowEdit = false;
            this.gridColumnFolioName.Visible = true;
            this.gridColumnFolioName.VisibleIndex = 13;
            this.gridColumnFolioName.Width = 123;
            // 
            // gridColumnFolioCCY
            // 
            this.gridColumnFolioCCY.Caption = "Currency";
            this.gridColumnFolioCCY.FieldName = "FolioCCY";
            this.gridColumnFolioCCY.Name = "gridColumnFolioCCY";
            this.gridColumnFolioCCY.OptionsColumn.AllowEdit = false;
            this.gridColumnFolioCCY.Visible = true;
            this.gridColumnFolioCCY.VisibleIndex = 14;
            this.gridColumnFolioCCY.Width = 157;
            // 
            // gridColumnFolioQty
            // 
            this.gridColumnFolioQty.Caption = "Portfolio allocated qty";
            this.gridColumnFolioQty.FieldName = "AlloactionQty";
            this.gridColumnFolioQty.Name = "gridColumnFolioQty";
            this.gridColumnFolioQty.OptionsColumn.AllowEdit = false;
            this.gridColumnFolioQty.Visible = true;
            this.gridColumnFolioQty.VisibleIndex = 15;
            // 
            // gridColumnSpotPct
            // 
            this.gridColumnSpotPct.Caption = "Spot Order Prop.";
            this.gridColumnSpotPct.FieldName = "FundingProportion";
            this.gridColumnSpotPct.Name = "gridColumnSpotPct";
            this.gridColumnSpotPct.Visible = true;
            this.gridColumnSpotPct.VisibleIndex = 16;
            // 
            // gridColumnHedgingPct
            // 
            this.gridColumnHedgingPct.Caption = "Hedging Order Prop.";
            this.gridColumnHedgingPct.FieldName = "HedgingProportion";
            this.gridColumnHedgingPct.Name = "gridColumnHedgingPct";
            this.gridColumnHedgingPct.Visible = true;
            this.gridColumnHedgingPct.VisibleIndex = 17;
            // 
            // gridColumnNAVFunding
            // 
            this.gridColumnNAVFunding.Caption = "%NAV - Funding";
            this.gridColumnNAVFunding.DisplayFormat.FormatString = "{0:n2}";
            this.gridColumnNAVFunding.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumnNAVFunding.FieldName = "NAVFunding";
            this.gridColumnNAVFunding.Name = "gridColumnNAVFunding";
            this.gridColumnNAVFunding.Visible = true;
            this.gridColumnNAVFunding.VisibleIndex = 18;
            // 
            // gridColumnNAVHedging
            // 
            this.gridColumnNAVHedging.Caption = "%NAV- Hedging";
            this.gridColumnNAVHedging.DisplayFormat.FormatString = "{0:n2}";
            this.gridColumnNAVHedging.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumnNAVHedging.FieldName = "NAVHedging";
            this.gridColumnNAVHedging.Name = "gridColumnNAVHedging";
            this.gridColumnNAVHedging.Visible = true;
            this.gridColumnNAVHedging.VisibleIndex = 19;
            // 
            // gridColumnFundingAmountCCY1
            // 
            this.gridColumnFundingAmountCCY1.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridColumnFundingAmountCCY1.AppearanceCell.Options.UseFont = true;
            this.gridColumnFundingAmountCCY1.Caption = "Funding Amount CCY1";
            this.gridColumnFundingAmountCCY1.DisplayFormat.FormatString = "{0:n2}";
            this.gridColumnFundingAmountCCY1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumnFundingAmountCCY1.FieldName = "FundingAmountCCY1";
            this.gridColumnFundingAmountCCY1.Name = "gridColumnFundingAmountCCY1";
            this.gridColumnFundingAmountCCY1.Visible = true;
            this.gridColumnFundingAmountCCY1.VisibleIndex = 20;
            // 
            // gridColumnHedgingAmountCCY1
            // 
            this.gridColumnHedgingAmountCCY1.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridColumnHedgingAmountCCY1.AppearanceCell.Options.UseFont = true;
            this.gridColumnHedgingAmountCCY1.Caption = "Hedging Amount CCY1";
            this.gridColumnHedgingAmountCCY1.DisplayFormat.FormatString = "{0:n2}";
            this.gridColumnHedgingAmountCCY1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumnHedgingAmountCCY1.FieldName = "HedgingAmountCCY1";
            this.gridColumnHedgingAmountCCY1.Name = "gridColumnHedgingAmountCCY1";
            this.gridColumnHedgingAmountCCY1.Visible = true;
            this.gridColumnHedgingAmountCCY1.VisibleIndex = 21;
            // 
            // gridColumnCCY1
            // 
            this.gridColumnCCY1.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridColumnCCY1.AppearanceCell.Options.UseFont = true;
            this.gridColumnCCY1.AppearanceCell.Options.UseTextOptions = true;
            this.gridColumnCCY1.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridColumnCCY1.Caption = " CCY1";
            this.gridColumnCCY1.FieldName = "FolioCCY";
            this.gridColumnCCY1.Name = "gridColumnCCY1";
            this.gridColumnCCY1.OptionsColumn.AllowEdit = false;
            this.gridColumnCCY1.Visible = true;
            this.gridColumnCCY1.VisibleIndex = 22;
            // 
            // gridColumnFundingAmountCCY2
            // 
            this.gridColumnFundingAmountCCY2.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridColumnFundingAmountCCY2.AppearanceCell.Options.UseFont = true;
            this.gridColumnFundingAmountCCY2.Caption = "Funding Amount CCY2";
            this.gridColumnFundingAmountCCY2.DisplayFormat.FormatString = "{0:n2}";
            this.gridColumnFundingAmountCCY2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumnFundingAmountCCY2.FieldName = "FundingAmountCCY2";
            this.gridColumnFundingAmountCCY2.Name = "gridColumnFundingAmountCCY2";
            this.gridColumnFundingAmountCCY2.Visible = true;
            this.gridColumnFundingAmountCCY2.VisibleIndex = 23;
            // 
            // gridColumnHedgingAmountCCY2
            // 
            this.gridColumnHedgingAmountCCY2.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridColumnHedgingAmountCCY2.AppearanceCell.Options.UseFont = true;
            this.gridColumnHedgingAmountCCY2.Caption = "Hedging Amount CCY2";
            this.gridColumnHedgingAmountCCY2.DisplayFormat.FormatString = "{0:n2}";
            this.gridColumnHedgingAmountCCY2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumnHedgingAmountCCY2.FieldName = "HedgingAmountCCY2";
            this.gridColumnHedgingAmountCCY2.Name = "gridColumnHedgingAmountCCY2";
            this.gridColumnHedgingAmountCCY2.Visible = true;
            this.gridColumnHedgingAmountCCY2.VisibleIndex = 24;
            // 
            // gridColumnCCY2
            // 
            this.gridColumnCCY2.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridColumnCCY2.AppearanceCell.Options.UseFont = true;
            this.gridColumnCCY2.AppearanceCell.Options.UseTextOptions = true;
            this.gridColumnCCY2.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridColumnCCY2.Caption = " CCY2";
            this.gridColumnCCY2.FieldName = "InstrumentCCY";
            this.gridColumnCCY2.Name = "gridColumnCCY2";
            this.gridColumnCCY2.OptionsColumn.AllowEdit = false;
            this.gridColumnCCY2.Visible = true;
            this.gridColumnCCY2.VisibleIndex = 25;
            // 
            // gridColumnForwardDate
            // 
            this.gridColumnForwardDate.Caption = "Forward date";
            this.gridColumnForwardDate.ColumnEdit = this.repositoryItemDateEdit2;
            this.gridColumnForwardDate.FieldName = "ForwardDate";
            this.gridColumnForwardDate.Name = "gridColumnForwardDate";
            this.gridColumnForwardDate.Visible = true;
            this.gridColumnForwardDate.VisibleIndex = 26;
            // 
            // repositoryItemDateEdit2
            // 
            this.repositoryItemDateEdit2.AutoHeight = false;
            this.repositoryItemDateEdit2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemDateEdit2.Name = "repositoryItemDateEdit2";
            this.repositoryItemDateEdit2.VistaTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            // 
            // gridColumnExecState
            // 
            this.gridColumnExecState.Caption = "Execution State";
            this.gridColumnExecState.ColumnEdit = this.repositoryItemComboBox1;
            this.gridColumnExecState.FieldName = "ExecutionState";
            this.gridColumnExecState.Name = "gridColumnExecState";
            this.gridColumnExecState.OptionsColumn.AllowEdit = false;
            this.gridColumnExecState.Visible = true;
            this.gridColumnExecState.VisibleIndex = 27;
            // 
            // repositoryItemComboBox1
            // 
            this.repositoryItemComboBox1.AutoHeight = false;
            this.repositoryItemComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox1.Name = "repositoryItemComboBox1";
            this.repositoryItemComboBox1.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(this.repositoryItemComboBoxExecutionState_CustomDisplayText);
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.simpleButton2);
            this.layoutControl1.Controls.Add(this.simpleButton1);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(623, 267, 250, 350);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1892, 67);
            this.layoutControl1.TabIndex = 8;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // simpleButton2
            // 
            this.simpleButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.simpleButton2.Location = new System.Drawing.Point(1699, 12);
            this.simpleButton2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(133, 23);
            this.simpleButton2.StyleController = this.layoutControl1;
            this.simpleButton2.TabIndex = 7;
            this.simpleButton2.Text = "Close";
            this.simpleButton2.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(1452, 12);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(174, 23);
            this.simpleButton1.StyleController = this.layoutControl1;
            this.simpleButton1.TabIndex = 6;
            this.simpleButton1.Text = "Generate orders";
            this.simpleButton1.Click += new System.EventHandler(this.createOrderButton_Click);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.emptySpaceItem1,
            this.emptySpaceItem2,
            this.emptySpaceItem3});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(1892, 67);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.simpleButton1;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(1440, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(178, 47);
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.simpleButton2;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(1687, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(137, 47);
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(1618, 0);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(69, 47);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.CustomizationFormText = "emptySpaceItem2";
            this.emptySpaceItem2.Location = new System.Drawing.Point(0, 0);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(1440, 47);
            this.emptySpaceItem2.Text = "emptySpaceItem2";
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem3
            // 
            this.emptySpaceItem3.AllowHotTrack = false;
            this.emptySpaceItem3.CustomizationFormText = "emptySpaceItem3";
            this.emptySpaceItem3.Location = new System.Drawing.Point(1824, 0);
            this.emptySpaceItem3.Name = "emptySpaceItem3";
            this.emptySpaceItem3.Size = new System.Drawing.Size(48, 47);
            this.emptySpaceItem3.Text = "emptySpaceItem3";
            this.emptySpaceItem3.TextSize = new System.Drawing.Size(0, 0);
            // 
            // gridColumnExecutionValueDate
            // 
            this.gridColumnExecutionValueDate.Caption = "Execution Value Date";
            this.gridColumnExecutionValueDate.FieldName = "ExecutionValueDate";
            this.gridColumnExecutionValueDate.Name = "gridColumnExecutionValueDate";
            this.gridColumnExecutionValueDate.OptionsColumn.AllowEdit = false;
            this.gridColumnExecutionValueDate.Visible = true;
            this.gridColumnExecutionValueDate.VisibleIndex = 5;
            // 
            // CSxHedgingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "CSxHedgingForm";
            this.Size = new System.Drawing.Size(1892, 791);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1.VistaTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCalcEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditHedging.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditFunding.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl2)).EndInit();
            this.splitContainerControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2.VistaTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem3)).EndInit();
            this.ResumeLayout(false);

        }

       

        #endregion

        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl2;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.SimpleButton simpleButtonApply;
        private DevExpress.XtraEditors.LabelControl labelControl10;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraEditors.TextEdit textEditHedging;
        private DevExpress.XtraEditors.TextEdit textEditFunding;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnOrderID;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnInstr;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnOrderCreationDate;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnOrderUser;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExecID;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExecutionDate;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExecSide;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExecAmount;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExecQty;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnInitPct;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExecPrice;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnFolioPath;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnFolioName;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnFolioCCY;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnFolioQty;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnSpotPct;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnHedgingPct;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnNAVFunding;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnNAVHedging;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnFundingAmountCCY1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnHedgingAmountCCY1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnCCY1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnFundingAmountCCY2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnHedgingAmountCCY2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnCCY2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnForwardDate;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit repositoryItemDateEdit2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExecState;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox1;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarEditItem barEditItem1;
        private DevExpress.XtraEditors.Repository.RepositoryItemCalcEdit repositoryItemCalcEdit1;
        private DevExpress.XtraBars.BarListItem barListItem1;
        private DevExpress.XtraBars.BarEditItem barEditItem2;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit2;
        private DevExpress.XtraBars.BarEditItem barEditItem3;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit3;
        private DevExpress.XtraBars.BarEditItem barEditItem4;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit repositoryItemDateEdit1;
        private DevExpress.XtraBars.PopupMenu popupMenu1;
        private DevExpress.XtraEditors.DropDownButton dropDownButton1;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem3;
        private SimpleButton simpleButtonRefresh;
        private SimpleButton simpleButtonApplyAll;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExecutionValueDate;

    }
}
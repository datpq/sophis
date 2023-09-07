namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    partial class CSxFXAutomationSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CSxFXAutomationSetting));
            this.cmdOk = new DevExpress.XtraEditors.SimpleButton();
            this.tabPaneMain = new DevExpress.XtraBars.Navigation.TabPane();
            this.tabCurrency = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.gridMarkets = new DevExpress.XtraGrid.GridControl();
            this.viewMarkets = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colActive = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridCurrencies = new DevExpress.XtraGrid.GridControl();
            this.viewCurrencies = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colCcy = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCcyActive = new DevExpress.XtraGrid.Columns.GridColumn();
            this.tabSleeve = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.treeSleeves = new DevExpress.XtraTreeList.TreeList();
            this.tlColID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.tlColName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.tlColParentID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.tlColActive = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.imgColl = new DevExpress.Utils.ImageCollection(this.components);
            this.gridSleeves = new DevExpress.XtraGrid.GridControl();
            this.viewSleeves = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colSlFundId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSlFundName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSlIdent = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSlName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSlActive = new DevExpress.XtraGrid.Columns.GridColumn();
            this.cmdRefresh = new DevExpress.XtraEditors.SimpleButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.tabPaneMain)).BeginInit();
            this.tabPaneMain.SuspendLayout();
            this.tabCurrency.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridMarkets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewMarkets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCurrencies)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewCurrencies)).BeginInit();
            this.tabSleeve.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeSleeves)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgColl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridSleeves)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewSleeves)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOk
            // 
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(3, 34);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(87, 23);
            this.cmdOk.TabIndex = 0;
            this.cmdOk.Text = "Save";
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // tabPaneMain
            // 
            this.tabPaneMain.Controls.Add(this.tabCurrency);
            this.tabPaneMain.Controls.Add(this.tabSleeve);
            this.tabPaneMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPaneMain.Location = new System.Drawing.Point(3, 3);
            this.tabPaneMain.Name = "tabPaneMain";
            this.tabPaneMain.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            this.tabCurrency,
            this.tabSleeve});
            this.tabPaneMain.RegularSize = new System.Drawing.Size(938, 521);
            this.tabPaneMain.SelectedPage = this.tabCurrency;
            this.tabPaneMain.Size = new System.Drawing.Size(938, 521);
            this.tabPaneMain.TabIndex = 7;
            this.tabPaneMain.Text = "tabCurrencyText";
            // 
            // tabCurrency
            // 
            this.tabCurrency.Caption = "Markets & Currencies";
            this.tabCurrency.Controls.Add(this.tableLayoutPanel2);
            this.tabCurrency.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("tabCurrency.ImageOptions.Image")));
            this.tabCurrency.ItemShowMode = DevExpress.XtraBars.Navigation.ItemShowMode.ImageAndText;
            this.tabCurrency.Name = "tabCurrency";
            this.tabCurrency.Properties.ShowMode = DevExpress.XtraBars.Navigation.ItemShowMode.ImageAndText;
            this.tabCurrency.Size = new System.Drawing.Size(938, 492);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.gridMarkets, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.gridCurrencies, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(938, 492);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // gridMarkets
            // 
            this.gridMarkets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridMarkets.Location = new System.Drawing.Point(3, 3);
            this.gridMarkets.MainView = this.viewMarkets;
            this.gridMarkets.Name = "gridMarkets";
            this.gridMarkets.Size = new System.Drawing.Size(463, 486);
            this.gridMarkets.TabIndex = 6;
            this.gridMarkets.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.viewMarkets});
            // 
            // viewMarkets
            // 
            this.viewMarkets.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colName,
            this.colActive});
            this.viewMarkets.GridControl = this.gridMarkets;
            this.viewMarkets.Name = "viewMarkets";
            this.viewMarkets.OptionsView.ShowAutoFilterRow = true;
            this.viewMarkets.OptionsView.ShowGroupPanel = false;
            this.viewMarkets.FocusedRowObjectChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowObjectChangedEventHandler(this.viewMarkets_FocusedRowObjectChanged);
            this.viewMarkets.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.viewMarkets_RowUpdated);
            // 
            // colName
            // 
            this.colName.FieldName = "Name";
            this.colName.Name = "colName";
            this.colName.OptionsColumn.AllowFocus = false;
            this.colName.Visible = true;
            this.colName.VisibleIndex = 0;
            this.colName.Width = 254;
            // 
            // colActive
            // 
            this.colActive.AppearanceHeader.Options.UseTextOptions = true;
            this.colActive.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colActive.Caption = "Selected";
            this.colActive.FieldName = "Active";
            this.colActive.Name = "colActive";
            this.colActive.Visible = true;
            this.colActive.VisibleIndex = 1;
            this.colActive.Width = 67;
            // 
            // gridCurrencies
            // 
            this.gridCurrencies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCurrencies.Location = new System.Drawing.Point(472, 3);
            this.gridCurrencies.MainView = this.viewCurrencies;
            this.gridCurrencies.Name = "gridCurrencies";
            this.gridCurrencies.Size = new System.Drawing.Size(463, 486);
            this.gridCurrencies.TabIndex = 7;
            this.gridCurrencies.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.viewCurrencies});
            // 
            // viewCurrencies
            // 
            this.viewCurrencies.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colCcy,
            this.colCcyActive});
            this.viewCurrencies.GridControl = this.gridCurrencies;
            this.viewCurrencies.Name = "viewCurrencies";
            this.viewCurrencies.OptionsView.ShowAutoFilterRow = true;
            this.viewCurrencies.OptionsView.ShowGroupPanel = false;
            this.viewCurrencies.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.viewCurrencies_RowUpdated);
            // 
            // colCcy
            // 
            this.colCcy.FieldName = "Code";
            this.colCcy.Name = "colCcy";
            this.colCcy.OptionsColumn.AllowFocus = false;
            this.colCcy.Visible = true;
            this.colCcy.VisibleIndex = 0;
            this.colCcy.Width = 254;
            // 
            // colCcyActive
            // 
            this.colCcyActive.AppearanceHeader.Options.UseTextOptions = true;
            this.colCcyActive.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colCcyActive.Caption = "Selected";
            this.colCcyActive.FieldName = "Active";
            this.colCcyActive.Name = "colCcyActive";
            this.colCcyActive.Visible = true;
            this.colCcyActive.VisibleIndex = 1;
            this.colCcyActive.Width = 67;
            // 
            // tabSleeve
            // 
            this.tabSleeve.Caption = "Funds/Sleeves";
            this.tabSleeve.Controls.Add(this.treeSleeves);
            this.tabSleeve.Controls.Add(this.gridSleeves);
            this.tabSleeve.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("tabSleeve.ImageOptions.Image")));
            this.tabSleeve.ItemShowMode = DevExpress.XtraBars.Navigation.ItemShowMode.ImageAndText;
            this.tabSleeve.Name = "tabSleeve";
            this.tabSleeve.PageVisible = false;
            this.tabSleeve.Properties.ShowMode = DevExpress.XtraBars.Navigation.ItemShowMode.ImageAndText;
            this.tabSleeve.Size = new System.Drawing.Size(938, 492);
            // 
            // treeSleeves
            // 
            this.treeSleeves.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.tlColID,
            this.tlColName,
            this.tlColParentID,
            this.tlColActive});
            this.treeSleeves.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeSleeves.Location = new System.Drawing.Point(0, 0);
            this.treeSleeves.Name = "treeSleeves";
            this.treeSleeves.OptionsFilter.ExpandNodesOnFiltering = true;
            this.treeSleeves.OptionsView.ShowAutoFilterRow = true;
            this.treeSleeves.Size = new System.Drawing.Size(938, 492);
            this.treeSleeves.StateImageList = this.imgColl;
            this.treeSleeves.TabIndex = 1;
            this.treeSleeves.GetStateImage += new DevExpress.XtraTreeList.GetStateImageEventHandler(this.treeSleeves_GetStateImage);
            this.treeSleeves.NodeCellStyle += new DevExpress.XtraTreeList.GetCustomNodeCellStyleEventHandler(this.treeSleeves_NodeCellStyle);
            this.treeSleeves.BeforeFocusNode += new DevExpress.XtraTreeList.BeforeFocusNodeEventHandler(this.treeSleeves_BeforeFocusNode);
            this.treeSleeves.CellValueChanged += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.treeSleeves_CellValueChanged);
            // 
            // tlColID
            // 
            this.tlColID.Caption = "ID";
            this.tlColID.FieldName = "ID";
            this.tlColID.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("tlColID.ImageOptions.SvgImage")));
            this.tlColID.Name = "tlColID";
            this.tlColID.Width = 197;
            // 
            // tlColName
            // 
            this.tlColName.Caption = "Sleeve";
            this.tlColName.FieldName = "Name";
            this.tlColName.Name = "tlColName";
            this.tlColName.OptionsColumn.AllowEdit = false;
            this.tlColName.OptionsColumn.ReadOnly = true;
            this.tlColName.Visible = true;
            this.tlColName.VisibleIndex = 0;
            this.tlColName.Width = 825;
            // 
            // tlColParentID
            // 
            this.tlColParentID.Caption = "ParentID";
            this.tlColParentID.FieldName = "ParentID";
            this.tlColParentID.Name = "tlColParentID";
            this.tlColParentID.Width = 360;
            // 
            // tlColActive
            // 
            this.tlColActive.AppearanceHeader.Options.UseTextOptions = true;
            this.tlColActive.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.tlColActive.Caption = "Selected";
            this.tlColActive.FieldName = "Active";
            this.tlColActive.Name = "tlColActive";
            this.tlColActive.Visible = true;
            this.tlColActive.VisibleIndex = 1;
            this.tlColActive.Width = 93;
            // 
            // imgColl
            // 
            this.imgColl.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imgColl.ImageStream")));
            this.imgColl.Images.SetKeyName(0, "Folio");
            this.imgColl.Images.SetKeyName(1, "Sleeve");
            // 
            // gridSleeves
            // 
            this.gridSleeves.Location = new System.Drawing.Point(60, 392);
            this.gridSleeves.MainView = this.viewSleeves;
            this.gridSleeves.Name = "gridSleeves";
            this.gridSleeves.Size = new System.Drawing.Size(680, 202);
            this.gridSleeves.TabIndex = 0;
            this.gridSleeves.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.viewSleeves});
            // 
            // viewSleeves
            // 
            this.viewSleeves.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colSlFundId,
            this.colSlFundName,
            this.colSlIdent,
            this.colSlName,
            this.colSlActive});
            this.viewSleeves.GridControl = this.gridSleeves;
            this.viewSleeves.GroupFormat = "[#image]{1} {2}";
            this.viewSleeves.Name = "viewSleeves";
            this.viewSleeves.OptionsBehavior.AutoExpandAllGroups = true;
            this.viewSleeves.OptionsView.ShowAutoFilterRow = true;
            this.viewSleeves.OptionsView.ShowGroupPanel = false;
            this.viewSleeves.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.viewSleeves_RowUpdated);
            // 
            // colSlFundId
            // 
            this.colSlFundId.FieldName = "FundId";
            this.colSlFundId.Name = "colSlFundId";
            this.colSlFundId.OptionsColumn.AllowFocus = false;
            this.colSlFundId.Width = 140;
            // 
            // colSlFundName
            // 
            this.colSlFundName.Caption = "Fund";
            this.colSlFundName.FieldName = "FundName";
            this.colSlFundName.Name = "colSlFundName";
            this.colSlFundName.OptionsColumn.AllowFocus = false;
            this.colSlFundName.Visible = true;
            this.colSlFundName.VisibleIndex = 0;
            this.colSlFundName.Width = 186;
            // 
            // colSlIdent
            // 
            this.colSlIdent.FieldName = "Id";
            this.colSlIdent.Name = "colSlIdent";
            this.colSlIdent.OptionsColumn.AllowFocus = false;
            this.colSlIdent.Width = 140;
            // 
            // colSlName
            // 
            this.colSlName.Caption = "Sleeve";
            this.colSlName.FieldName = "Name";
            this.colSlName.Name = "colSlName";
            this.colSlName.OptionsColumn.AllowFocus = false;
            this.colSlName.Visible = true;
            this.colSlName.VisibleIndex = 1;
            this.colSlName.Width = 240;
            // 
            // colSlActive
            // 
            this.colSlActive.AppearanceHeader.Options.UseTextOptions = true;
            this.colSlActive.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colSlActive.FieldName = "Active";
            this.colSlActive.Name = "colSlActive";
            this.colSlActive.Visible = true;
            this.colSlActive.VisibleIndex = 2;
            this.colSlActive.Width = 77;
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.Location = new System.Drawing.Point(3, 63);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(87, 23);
            this.cmdRefresh.TabIndex = 8;
            this.cmdRefresh.Text = "Refresh";
            this.cmdRefresh.Click += new System.EventHandler(this.cmdRefresh_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 101F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabPaneMain, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1045, 527);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmdRefresh);
            this.panel1.Controls.Add(this.cmdOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(947, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(95, 521);
            this.panel1.TabIndex = 0;
            // 
            // CSxFXAutomationSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CSxFXAutomationSetting";
            this.Size = new System.Drawing.Size(1045, 527);
            this.Title = "FX Automation Setting";
            this.Load += new System.EventHandler(this.CSxFXAutomationSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tabPaneMain)).EndInit();
            this.tabPaneMain.ResumeLayout(false);
            this.tabCurrency.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridMarkets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewMarkets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCurrencies)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewCurrencies)).EndInit();
            this.tabSleeve.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeSleeves)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgColl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridSleeves)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewSleeves)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton cmdOk;
        private DevExpress.XtraBars.Navigation.TabPane tabPaneMain;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabCurrency;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabSleeve;
        private DevExpress.XtraGrid.GridControl gridMarkets;
        private DevExpress.XtraGrid.Views.Grid.GridView viewMarkets;
        private DevExpress.XtraGrid.Columns.GridColumn colName;
        private DevExpress.XtraGrid.Columns.GridColumn colActive;
        private DevExpress.XtraGrid.GridControl gridCurrencies;
        private DevExpress.XtraGrid.Views.Grid.GridView viewCurrencies;
        private DevExpress.XtraGrid.Columns.GridColumn colCcy;
        private DevExpress.XtraGrid.Columns.GridColumn colCcyActive;
        private DevExpress.XtraGrid.GridControl gridSleeves;
        private DevExpress.XtraGrid.Views.Grid.GridView viewSleeves;
        private DevExpress.XtraGrid.Columns.GridColumn colSlFundId;
        private DevExpress.XtraGrid.Columns.GridColumn colSlFundName;
        private DevExpress.XtraGrid.Columns.GridColumn colSlIdent;
        private DevExpress.XtraGrid.Columns.GridColumn colSlName;
        private DevExpress.XtraGrid.Columns.GridColumn colSlActive;
        private DevExpress.XtraTreeList.TreeList treeSleeves;
        private DevExpress.XtraTreeList.Columns.TreeListColumn tlColID;
        private DevExpress.XtraTreeList.Columns.TreeListColumn tlColName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn tlColParentID;
        private DevExpress.XtraTreeList.Columns.TreeListColumn tlColActive;
        private DevExpress.Utils.ImageCollection imgColl;
        private DevExpress.XtraEditors.SimpleButton cmdRefresh;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}
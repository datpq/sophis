using DevExpress.XtraEditors.Repository;
using sophis.amGuiCommon;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    partial class CSxSummaryScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CSxSummaryScreen));
            DevExpress.XtraEditors.Controls.RadioGroupItem radioGroupItem16 = new DevExpress.XtraEditors.Controls.RadioGroupItem();
            DevExpress.XtraEditors.Controls.RadioGroupItem radioGroupItem17 = new DevExpress.XtraEditors.Controls.RadioGroupItem();
            DevExpress.XtraEditors.Controls.RadioGroupItem radioGroupItem18 = new DevExpress.XtraEditors.Controls.RadioGroupItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.LoadButton = new DevExpress.XtraEditors.SimpleButton();
            this.btnSendForApproval = new DevExpress.XtraEditors.SimpleButton();
            this.radioGroup1 = new DevExpress.XtraEditors.RadioGroup();
            this.btnReviewApprovals = new DevExpress.XtraEditors.SimpleButton();
            this.FundName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.DelegateName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.FundNAV = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ModelWeightPercent = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ActualWeightPercent = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.Deviation = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ModelWeightCcy = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ActualWeightCcy = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.DeviationCcy = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.CashTransfer = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.sidePanel1 = new DevExpress.XtraEditors.SidePanel();
            this.btnSendSR = new DevExpress.XtraEditors.SimpleButton();
            this.btnAccounts = new DevExpress.XtraEditors.SimpleButton();
            this.treeList1 = new DevExpress.XtraTreeList.TreeList();
            this.Summary = new DevExpress.XtraTreeList.Columns.TreeListBand();
            this.OrgCurrency = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.NewActualPos = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.NewActualWeight = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.NewDeviation = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.TransferCCY = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ccyCombo = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.TradeDate = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.repDateEdit = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.SettlementDate = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.FundID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.RepCcyComboBox = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.sidePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ccyCombo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repDateEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repDateEdit.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RepCcyComboBox)).BeginInit();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "fund.bmp");
            this.imageList.Images.SetKeyName(1, "strat.PNG");
            this.imageList.Images.SetKeyName(2, "position.PNG");
            this.imageList.Images.SetKeyName(3, "currency.bmp");
            this.imageList.Images.SetKeyName(4, "folder.png");
            // 
            // LoadButton
            // 
            this.LoadButton.Location = new System.Drawing.Point(26, 7);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(247, 35);
            this.LoadButton.TabIndex = 0;
            this.LoadButton.Text = "Load Summary";
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // btnSendForApproval
            // 
            this.btnSendForApproval.Location = new System.Drawing.Point(306, 7);
            this.btnSendForApproval.Name = "btnSendForApproval";
            this.btnSendForApproval.Size = new System.Drawing.Size(198, 35);
            this.btnSendForApproval.TabIndex = 3;
            this.btnSendForApproval.Text = "Send For Approval";
            this.btnSendForApproval.Click += new System.EventHandler(this.btnSendForApproval_Click);
            // 
            // radioGroup1
            // 
            this.radioGroup1.EditValue = "Delegates/Sleeves";
            this.radioGroup1.Location = new System.Drawing.Point(26, 47);
            this.radioGroup1.Name = "radioGroup1";
            // 
            // 
            // 
            this.radioGroup1.Properties.Columns = 1;
            this.radioGroup1.Properties.GlyphAlignment = DevExpress.Utils.HorzAlignment.Default;
            radioGroupItem16.AccessibleName = "MIFL to DIM cash transfers";
            radioGroupItem16.Description = "MIFL to DIM cash transfers";
            radioGroupItem16.Value = "Delegates/Sleeves";
            radioGroupItem17.AccessibleName = "TargetFunds/ETFs";
            radioGroupItem17.Description = "TargetFunds/ETFs";
            radioGroupItem17.Value = "TargetFunds/ETFs";
            radioGroupItem18.AccessibleName = "DIM to DIM cash transfers";
            radioGroupItem18.Description = "DIM to DIM cash transfers";
            radioGroupItem18.Value = "DIM to DIM";
            this.radioGroup1.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            radioGroupItem16,
            radioGroupItem17,
            radioGroupItem18});
            this.radioGroup1.Size = new System.Drawing.Size(247, 66);
            this.radioGroup1.TabIndex = 8;
            this.radioGroup1.SelectedIndexChanged += new System.EventHandler(this.radioGroup1_SelectedIndexChanged);
            // 
            // btnReviewApprovals
            // 
            this.btnReviewApprovals.Location = new System.Drawing.Point(510, 7);
            this.btnReviewApprovals.Name = "btnReviewApprovals";
            this.btnReviewApprovals.Size = new System.Drawing.Size(198, 35);
            this.btnReviewApprovals.TabIndex = 4;
            this.btnReviewApprovals.Text = "View Pending Approval Subs/Reds";
            this.btnReviewApprovals.Click += new System.EventHandler(this.btnReviewApprovals_Click);
            // 
            // FundName
            // 
            this.FundName.Caption = "Fund/Mandate Name";
            this.FundName.FieldName = "FundName";
            this.FundName.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("FundName.ImageOptions.Image")));
            this.FundName.Name = "FundName";
            this.FundName.OptionsColumn.AllowEdit = false;
            this.FundName.Visible = true;
            this.FundName.VisibleIndex = 0;
            this.FundName.Width = 141;
            // 
            // DelegateName
            // 
            this.DelegateName.Caption = "Delegate";
            this.DelegateName.FieldName = "DelegateName";
            this.DelegateName.Name = "DelegateName";
            this.DelegateName.OptionsColumn.AllowEdit = false;
            this.DelegateName.Width = 120;
            // 
            // FundNAV
            // 
            this.FundNAV.Caption = "Fund NAV(EUR)";
            this.FundNAV.FieldName = "FundNAV";
            this.FundNAV.Format.FormatString = "n0";
            this.FundNAV.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.FundNAV.Name = "FundNAV";
            this.FundNAV.OptionsColumn.AllowEdit = false;
            this.FundNAV.Visible = true;
            this.FundNAV.VisibleIndex = 6;
            this.FundNAV.Width = 138;
            // 
            // ModelWeightPercent
            // 
            this.ModelWeightPercent.Caption = " Adj.Model weight %";
            this.ModelWeightPercent.FieldName = "ModelWeightPercent";
            this.ModelWeightPercent.Format.FormatString = "n2";
            this.ModelWeightPercent.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.ModelWeightPercent.Name = "ModelWeightPercent";
            this.ModelWeightPercent.OptionsColumn.AllowEdit = false;
            this.ModelWeightPercent.Visible = true;
            this.ModelWeightPercent.VisibleIndex = 1;
            this.ModelWeightPercent.Width = 80;
            // 
            // ActualWeightPercent
            // 
            this.ActualWeightPercent.Caption = "Actual weight %";
            this.ActualWeightPercent.FieldName = "ActualWeightPercent";
            this.ActualWeightPercent.Format.FormatString = "n2";
            this.ActualWeightPercent.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.ActualWeightPercent.Name = "ActualWeightPercent";
            this.ActualWeightPercent.OptionsColumn.AllowEdit = false;
            this.ActualWeightPercent.Visible = true;
            this.ActualWeightPercent.VisibleIndex = 2;
            this.ActualWeightPercent.Width = 80;
            // 
            // Deviation
            // 
            this.Deviation.Caption = "Deviation %";
            this.Deviation.FieldName = "Deviation";
            this.Deviation.Format.FormatString = "n2";
            this.Deviation.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.Deviation.Name = "Deviation";
            this.Deviation.OptionsColumn.AllowEdit = false;
            this.Deviation.Visible = true;
            this.Deviation.VisibleIndex = 3;
            this.Deviation.Width = 80;
            // 
            // ModelWeightCcy
            // 
            this.ModelWeightCcy.Caption = "Model weight(EUR)";
            this.ModelWeightCcy.FieldName = "ModelWeightCcy";
            this.ModelWeightCcy.Format.FormatString = "n0";
            this.ModelWeightCcy.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.ModelWeightCcy.Name = "ModelWeightCcy";
            this.ModelWeightCcy.OptionsColumn.AllowEdit = false;
            this.ModelWeightCcy.Visible = true;
            this.ModelWeightCcy.VisibleIndex = 5;
            this.ModelWeightCcy.Width = 103;
            // 
            // ActualWeightCcy
            // 
            this.ActualWeightCcy.Caption = "Actual weight(EUR)";
            this.ActualWeightCcy.FieldName = "ActualWeightCcy";
            this.ActualWeightCcy.Format.FormatString = "n2";
            this.ActualWeightCcy.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.ActualWeightCcy.Name = "ActualWeightCcy";
            this.ActualWeightCcy.OptionsColumn.AllowEdit = false;
            this.ActualWeightCcy.Width = 133;
            // 
            // DeviationCcy
            // 
            this.DeviationCcy.Caption = "Deviation(EUR)";
            this.DeviationCcy.FieldName = "DeviationCcy";
            this.DeviationCcy.Format.FormatString = "n0";
            this.DeviationCcy.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.DeviationCcy.Name = "DeviationCcy";
            this.DeviationCcy.OptionsColumn.AllowEdit = false;
            this.DeviationCcy.Visible = true;
            this.DeviationCcy.VisibleIndex = 7;
            this.DeviationCcy.Width = 125;
            // 
            // CashTransfer
            // 
            this.CashTransfer.Caption = "Cash Transfer";
            this.CashTransfer.FieldName = "Cash Transfer";
            this.CashTransfer.Format.FormatString = "n0";
            this.CashTransfer.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.CashTransfer.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("CashTransfer.ImageOptions.Image")));
            this.CashTransfer.Name = "CashTransfer";
            this.CashTransfer.Visible = true;
            this.CashTransfer.VisibleIndex = 8;
            this.CashTransfer.Width = 121;
            // 
            // groupControl2
            // 
            this.groupControl2.AppearanceCaption.BackColor = System.Drawing.SystemColors.Control;
            this.groupControl2.AppearanceCaption.Options.UseBackColor = true;
            this.groupControl2.AutoSize = true;
            this.groupControl2.CaptionLocation = DevExpress.Utils.Locations.Top;
            this.groupControl2.Controls.Add(this.sidePanel1);
            this.groupControl2.Controls.Add(this.treeList1);
            this.groupControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl2.Location = new System.Drawing.Point(0, 0);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.ShowCaption = false;
            this.groupControl2.Size = new System.Drawing.Size(1550, 540);
            this.groupControl2.TabIndex = 0;
            this.groupControl2.Text = " ";
            // 
            // sidePanel1
            // 
            this.sidePanel1.AllowResize = false;
            this.sidePanel1.Controls.Add(this.btnSendSR);
            this.sidePanel1.Controls.Add(this.LoadButton);
            this.sidePanel1.Controls.Add(this.btnAccounts);
            this.sidePanel1.Controls.Add(this.radioGroup1);
            this.sidePanel1.Controls.Add(this.btnSendForApproval);
            this.sidePanel1.Controls.Add(this.btnReviewApprovals);
            this.sidePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.sidePanel1.Location = new System.Drawing.Point(2, 2);
            this.sidePanel1.Name = "sidePanel1";
            this.sidePanel1.Size = new System.Drawing.Size(1546, 117);
            this.sidePanel1.TabIndex = 12;
            this.sidePanel1.Text = "sidePanel1";
            // 
            // btnSendSR
            // 
            this.btnSendSR.Location = new System.Drawing.Point(510, 68);
            this.btnSendSR.Name = "btnSendSR";
            this.btnSendSR.Size = new System.Drawing.Size(198, 35);
            this.btnSendSR.TabIndex = 9;
            this.btnSendSR.Text = "View Approved Subs/Reds";
            this.btnSendSR.Click += new System.EventHandler(this.btnSendSR_Click);
            // 
            // btnAccounts
            // 
            this.btnAccounts.Location = new System.Drawing.Point(306, 68);
            this.btnAccounts.Name = "btnAccounts";
            this.btnAccounts.Size = new System.Drawing.Size(198, 35);
            this.btnAccounts.TabIndex = 10;
            this.btnAccounts.Text = "View Accounts Threshold";
            this.btnAccounts.Click += new System.EventHandler(this.btnAccounts_Click);
            // 
            // treeList1
            // 
             this.treeList1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeList1.Appearance.Row.ForeColor = System.Drawing.Color.Blue;
            this.treeList1.Appearance.Row.Options.UseForeColor = true;
            this.treeList1.Appearance.Row.Options.UseImage = true;
            this.treeList1.Appearance.SelectedRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.treeList1.Appearance.SelectedRow.Options.UseForeColor = true;
            this.treeList1.AutoFillColumn = this.FundName;
            this.treeList1.Bands.AddRange(new DevExpress.XtraTreeList.Columns.TreeListBand[] {
            this.Summary});
            this.treeList1.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.FundName,
            this.ModelWeightPercent,
            this.ActualWeightPercent,
            this.Deviation,
            this.OrgCurrency,
            this.ModelWeightCcy,
            this.FundNAV,
            this.DeviationCcy,
            this.CashTransfer,
            this.NewActualPos,
            this.NewActualWeight,
            this.NewDeviation,
            this.TransferCCY,
            this.TradeDate,
            this.SettlementDate,
            this.FundID,
            this.DelegateName,
            this.ActualWeightCcy});
            this.treeList1.CustomizationFormBounds = new System.Drawing.Rectangle(1055, 381, 250, 204);
            this.treeList1.HorzScrollVisibility = DevExpress.XtraTreeList.ScrollVisibility.Always;
            this.treeList1.Location = new System.Drawing.Point(28, 125);
            this.treeList1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.treeList1.Name = "treeList1";
            this.treeList1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.ccyCombo,
            this.repDateEdit});
            this.treeList1.Size = new System.Drawing.Size(1507, 530);
            this.treeList1.OptionsBehavior.PopulateServiceColumns = true;
            this.treeList1.StateImageList = this.imageList;
            this.treeList1.TabIndex = 0;
            this.treeList1.GetStateImage += new DevExpress.XtraTreeList.GetStateImageEventHandler(this.OnGetStateImage);
            this.treeList1.CustomNodeCellEdit += new DevExpress.XtraTreeList.GetCustomNodeCellEditEventHandler(this.treeList1_CustomNodeCellEdit);
            this.treeList1.NodeCellStyle += new DevExpress.XtraTreeList.GetCustomNodeCellStyleEventHandler(this.treeList1_NodeCellStyle);
            this.treeList1.CellValueChanged += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.treeList1_CellValueChanged_1);
            // 
            // Summary
            // 
            this.Summary.Caption = "Summary";
            this.Summary.Columns.Add(this.FundName);
            this.Summary.Columns.Add(this.DelegateName);
            this.Summary.Columns.Add(this.ModelWeightPercent);
            this.Summary.Columns.Add(this.ActualWeightPercent);
            this.Summary.Columns.Add(this.Deviation);
            this.Summary.Columns.Add(this.OrgCurrency);
            this.Summary.Columns.Add(this.ModelWeightCcy);
            this.Summary.Columns.Add(this.FundNAV);
            this.Summary.Columns.Add(this.ActualWeightCcy);
            this.Summary.Columns.Add(this.DeviationCcy);
            this.Summary.Columns.Add(this.CashTransfer);
            this.Summary.Columns.Add(this.NewActualPos);
            this.Summary.Columns.Add(this.NewActualWeight);
            this.Summary.Columns.Add(this.NewDeviation);
            this.Summary.Columns.Add(this.TransferCCY);
            this.Summary.Columns.Add(this.TradeDate);
            this.Summary.Columns.Add(this.SettlementDate);
            this.Summary.Name = "Summary";
            // 
            // OrgCurrency
            // 
            this.OrgCurrency.Caption = "Original CCY";
            this.OrgCurrency.FieldName = "Original CCY";
            this.OrgCurrency.Name = "OrgCurrency";
            this.OrgCurrency.Visible = true;
            this.OrgCurrency.OptionsColumn.AllowEdit = false;
            this.OrgCurrency.VisibleIndex = 4;
            this.OrgCurrency.Width = 91;
            // 
            // NewActualPos
            // 
            this.NewActualPos.AppearanceCell.BackColor = System.Drawing.Color.AliceBlue;
            this.NewActualPos.AppearanceCell.Options.UseBackColor = true;
            this.NewActualPos.AppearanceHeader.BackColor = System.Drawing.Color.AliceBlue;
            this.NewActualPos.Caption = "New Actual Position(EUR)";
            this.NewActualPos.FieldName = "New Actual Position(EUR)";
            this.NewActualPos.Format.FormatString = "n0";
            this.NewActualPos.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.NewActualPos.Name = "NewActualPos";
            this.NewActualPos.OptionsColumn.AllowEdit = false;
            this.NewActualPos.Visible = true;
            this.NewActualPos.VisibleIndex = 9;
            this.NewActualPos.Width = 121;
            // 
            // NewActualWeight
            // 
            this.NewActualWeight.AppearanceCell.BackColor = System.Drawing.Color.AliceBlue;
            this.NewActualWeight.AppearanceCell.Options.UseBackColor = true;
            this.NewActualWeight.AppearanceHeader.BackColor = System.Drawing.Color.AliceBlue;
            this.NewActualWeight.Caption = "New Actual Weight%";
            this.NewActualWeight.FieldName = "New Actual Weight%";
            this.NewActualWeight.Format.FormatString = "n2";
            this.NewActualWeight.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.NewActualWeight.Name = "NewActualWeight";
            this.NewActualWeight.OptionsColumn.AllowEdit = false;
            this.NewActualWeight.Visible = true;
            this.NewActualWeight.VisibleIndex = 10;
            this.NewActualWeight.Width = 121;
            // 
            // NewDeviation
            // 
            this.NewDeviation.AppearanceCell.BackColor = System.Drawing.Color.AliceBlue;
            this.NewDeviation.AppearanceCell.Options.UseBackColor = true;
            this.NewDeviation.AppearanceHeader.BackColor = System.Drawing.Color.AliceBlue;
            this.NewDeviation.Caption = "New Deviation%";
            this.NewDeviation.FieldName = "New Deviation%";
            this.NewDeviation.Format.FormatString = "n2";
            this.NewDeviation.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.NewDeviation.Name = "NewDeviation";
            this.NewDeviation.OptionsColumn.AllowEdit = false;
            this.NewDeviation.Visible = true;
            this.NewDeviation.VisibleIndex = 11;
            this.NewDeviation.Width = 121;
            // 
            // TransferCCY
            // 
            this.TransferCCY.Caption = "Transfer CCY";
            this.TransferCCY.ColumnEdit = this.ccyCombo;
            this.TransferCCY.FieldName = "Transfer CCY";
            this.TransferCCY.Name = "TransferCCY";
            this.TransferCCY.Visible = true;
            this.TransferCCY.VisibleIndex = 12;
            this.TransferCCY.Width = 83;
            // 
            // ccyCombo
            // 
            this.ccyCombo.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.ccyCombo.Items.AddRange(new object[] {
            "EUR",
            "USD"});
            this.ccyCombo.Name = "ccyCombo";
            // 
            // TradeDate
            // 
            this.TradeDate.Caption = "Trade Date";
            this.TradeDate.ColumnEdit = this.repDateEdit;
            this.TradeDate.FieldName = "Trade Date";
            this.TradeDate.Format.FormatString = "d";
            this.TradeDate.Format.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.TradeDate.Name = "TradeDate";
            this.TradeDate.Visible = true;
            this.TradeDate.VisibleIndex = 13;
            this.TradeDate.Width = 97;
            // 
            // repDateEdit
            // 
            this.repDateEdit.AutoHeight = false;
            this.repDateEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            // 
            // 
            // 
            this.repDateEdit.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repDateEdit.EditFormat.FormatString = "dd/mm/yyyy";
            this.repDateEdit.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.repDateEdit.Name = "repDateEdit";
            // 
            // SettlementDate
            // 
            this.SettlementDate.Caption = "Settlement Date";
            this.SettlementDate.ColumnEdit = this.repDateEdit;
            this.SettlementDate.FieldName = "Settlement Date";
            this.SettlementDate.Format.FormatString = "d";
            this.SettlementDate.Format.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.SettlementDate.Name = "SettlementDate";
            this.SettlementDate.Visible = true;
            this.SettlementDate.VisibleIndex = 14;
            this.SettlementDate.Width = 91;
            // 
            // FundID
            // 
            this.FundID.Caption = "FundID";
            this.FundID.FieldName = "FundID";
            this.FundID.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.FundID.Name = "FundID";
            // 
            // RepCcyComboBox
            // 
            this.RepCcyComboBox.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.RepCcyComboBox.Name = "RepCcyComboBox";
            // 
            // CSxSummaryScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupControl2);
            this.Name = "CSxSummaryScreen";
            this.Size = new System.Drawing.Size(1550, 540);
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.sidePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ccyCombo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repDateEdit.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repDateEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RepCcyComboBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton LoadButton;
        private DevExpress.XtraEditors.SimpleButton btnSendForApproval;
        private DevExpress.XtraEditors.SimpleButton btnReviewApprovals;
        private DevExpress.XtraEditors.RadioGroup radioGroup1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn FundName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn DelegateName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn FundNAV;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ModelWeightPercent;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ActualWeightPercent;
        private DevExpress.XtraTreeList.Columns.TreeListColumn Deviation;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ModelWeightCcy;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ActualWeightCcy;
        private DevExpress.XtraTreeList.Columns.TreeListColumn DeviationCcy;

        private DevExpress.XtraEditors.GroupControl groupControl2;
        private System.Windows.Forms.ImageList imageList;
        private DevExpress.XtraTreeList.Columns.TreeListColumn CashTransfer;
        private DevExpress.XtraTreeList.TreeList treeList1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn OrgCurrency;
        private DevExpress.XtraTreeList.Columns.TreeListColumn TransferCCY;
        private DevExpress.XtraTreeList.Columns.TreeListColumn FundID;
        private RepositoryItemLookUpEdit RepCcyComboBox;
        private RepositoryItemComboBox ccyCombo;
        private DevExpress.XtraTreeList.Columns.TreeListColumn TradeDate;
        private RepositoryItemDateEdit repDateEdit;
        private DevExpress.XtraTreeList.Columns.TreeListBand Summary;
        private DevExpress.XtraTreeList.Columns.TreeListColumn SettlementDate;
        private DevExpress.XtraEditors.SimpleButton btnAccounts;
        private DevExpress.XtraEditors.SimpleButton btnSendSR;
        private DevExpress.XtraEditors.SidePanel sidePanel1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn NewActualPos;
        private DevExpress.XtraTreeList.Columns.TreeListColumn NewDeviation;
        private DevExpress.XtraTreeList.Columns.TreeListColumn NewActualWeight;
    }
}
namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    partial class CSxFXAutomationForm
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CSxFXAutomationForm));
            this.colBalance = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.lblDate = new DevExpress.XtraEditors.LabelControl();
            this.dateParam = new DevExpress.XtraEditors.DateEdit();
            this.cmdRaiseOrders = new DevExpress.XtraEditors.SimpleButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.cboMarkets = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.lookupSleeves = new DevExpress.XtraEditors.TreeListLookUpEdit();
            this.treeSleeves = new DevExpress.XtraTreeList.TreeList();
            this.tlColName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.tlColID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.tlColParentID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.imgColl = new DevExpress.Utils.ImageCollection(this.components);
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.cmdGenDCB = new DevExpress.XtraEditors.SimpleButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.treeDCB = new DevExpress.XtraTreeList.TreeList();
            this.colID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colParentID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colCurrency = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.currencyCbo = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.colAmount = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colAmountCurGlobal = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colMedioMarketValueCurGlobal = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colBPS = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colThreshold = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colWeightNav = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colDateSettlement = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colAmountRaised = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colNodeType = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.panel4 = new System.Windows.Forms.Panel();
            this.cmdCopy = new DevExpress.XtraEditors.SimpleButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cmdRefreshSleeves = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.dateParam.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateParam.Properties)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboMarkets.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookupSleeves.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeSleeves)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgColl)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeDCB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.currencyCbo)).BeginInit();
            this.panel4.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // colBalance
            // 
            this.colBalance.AppearanceHeader.Options.UseTextOptions = true;
            this.colBalance.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colBalance.Caption = "Balance";
            this.colBalance.FieldName = "BalanceRounded";
            this.colBalance.Name = "colBalance";
            this.colBalance.Visible = true;
            this.colBalance.VisibleIndex = 1;
            this.colBalance.Width = 90;
            // 
            // lblDate
            // 
            this.lblDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDate.Location = new System.Drawing.Point(3, 3);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(22, 20);
            this.lblDate.TabIndex = 2;
            this.lblDate.Text = "Date:";
            // 
            // dateParam
            // 
            this.dateParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateParam.EditValue = null;
            this.dateParam.Enabled = false;
            this.dateParam.Location = new System.Drawing.Point(31, 3);
            this.dateParam.Name = "dateParam";
            this.dateParam.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateParam.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateParam.Properties.DateTimeChanged += new System.EventHandler(this.dateParam_Properties_DateTimeChanged);
            this.dateParam.Size = new System.Drawing.Size(84, 20);
            this.dateParam.TabIndex = 1;
            // 
            // cmdRaiseOrders
            // 
            this.cmdRaiseOrders.Location = new System.Drawing.Point(3, 70);
            this.cmdRaiseOrders.Name = "cmdRaiseOrders";
            this.cmdRaiseOrders.Size = new System.Drawing.Size(111, 23);
            this.cmdRaiseOrders.TabIndex = 1;
            this.cmdRaiseOrders.Text = "Raise Orders";
            this.cmdRaiseOrders.Click += new System.EventHandler(this.cmdRaiseOrders_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1191, 45);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters:";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 7;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel3.Controls.Add(this.cboMarkets, 6, 0);
            this.tableLayoutPanel3.Controls.Add(this.lblDate, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelControl3, 5, 0);
            this.tableLayoutPanel3.Controls.Add(this.dateParam, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.lookupSleeves, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelControl1, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmdRefreshSleeves, 4, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1185, 26);
            this.tableLayoutPanel3.TabIndex = 12;
            // 
            // cboMarkets
            // 
            this.cboMarkets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboMarkets.Location = new System.Drawing.Point(914, 3);
            this.cboMarkets.Name = "cboMarkets";
            this.cboMarkets.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboMarkets.Properties.DisplayMember = "Name";
            this.cboMarkets.Properties.ValueMember = "Code";
            this.cboMarkets.Size = new System.Drawing.Size(268, 20);
            this.cboMarkets.TabIndex = 9;
            // 
            // labelControl3
            // 
            this.labelControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl3.Location = new System.Drawing.Point(869, 3);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(39, 20);
            this.labelControl3.TabIndex = 7;
            this.labelControl3.Text = "Markets:";
            // 
            // lookupSleeves
            // 
            this.lookupSleeves.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lookupSleeves.EditValue = "";
            this.lookupSleeves.Location = new System.Drawing.Point(205, 3);
            this.lookupSleeves.Name = "lookupSleeves";
            this.lookupSleeves.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookupSleeves.Properties.DisplayMember = "Name";
            this.lookupSleeves.Properties.TreeList = this.treeSleeves;
            this.lookupSleeves.Properties.BeforePopup += new System.EventHandler(this.lookupSleeves_Properties_BeforePopup);
            this.lookupSleeves.Size = new System.Drawing.Size(633, 20);
            this.lookupSleeves.TabIndex = 3;
            this.lookupSleeves.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(this.lookupSleeves_CustomDisplayText);
            // 
            // treeSleeves
            // 
            this.treeSleeves.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.tlColName,
            this.tlColID,
            this.tlColParentID});
            this.treeSleeves.Location = new System.Drawing.Point(0, 0);
            this.treeSleeves.Name = "treeSleeves";
            this.treeSleeves.OptionsBehavior.AllowRecursiveNodeChecking = true;
            this.treeSleeves.OptionsView.CheckBoxStyle = DevExpress.XtraTreeList.DefaultNodeCheckBoxStyle.Check;
            this.treeSleeves.OptionsView.ShowAutoFilterRow = true;
            this.treeSleeves.Size = new System.Drawing.Size(400, 200);
            this.treeSleeves.StateImageList = this.imgColl;
            this.treeSleeves.TabIndex = 0;
            this.treeSleeves.GetStateImage += new DevExpress.XtraTreeList.GetStateImageEventHandler(this.treeSleeves_GetStateImage);
            this.treeSleeves.AfterCheckNode += new DevExpress.XtraTreeList.NodeEventHandler(this.treeSleeves_AfterCheckNode);
            // 
            // tlColName
            // 
            this.tlColName.Caption = "Sleeve";
            this.tlColName.FieldName = "Name";
            this.tlColName.Name = "tlColName";
            this.tlColName.Visible = true;
            this.tlColName.VisibleIndex = 0;
            // 
            // tlColID
            // 
            this.tlColID.FieldName = "ID";
            this.tlColID.Name = "tlColID";
            // 
            // tlColParentID
            // 
            this.tlColParentID.FieldName = "ParentID";
            this.tlColParentID.Name = "tlColParentID";
            // 
            // imgColl
            // 
            this.imgColl.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imgColl.ImageStream")));
            this.imgColl.Images.SetKeyName(0, "Folio");
            this.imgColl.Images.SetKeyName(1, "Sleeve");
            this.imgColl.Images.SetKeyName(2, "Currency");
            this.imgColl.Images.SetKeyName(3, "Date");
            this.imgColl.Images.SetKeyName(4, "Position");
            // 
            // labelControl1
            // 
            this.labelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl1.Location = new System.Drawing.Point(121, 3);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(78, 20);
            this.labelControl1.TabIndex = 5;
            this.labelControl1.Text = "Funds/Sleeves:";
            // 
            // cmdGenDCB
            // 
            this.cmdGenDCB.Location = new System.Drawing.Point(3, 25);
            this.cmdGenDCB.Name = "cmdGenDCB";
            this.cmdGenDCB.Size = new System.Drawing.Size(111, 23);
            this.cmdGenDCB.TabIndex = 8;
            this.cmdGenDCB.Text = "Generate";
            this.cmdGenDCB.Click += new System.EventHandler(this.cmdGenDCB_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.treeDCB);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 54);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1191, 503);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Detailed Cash Balance";
            // 
            // treeDCB
            // 
            this.treeDCB.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.colID,
            this.colParentID,
            this.colName,
            this.colBalance,
            this.colCurrency,
            this.colAmount,
            this.colAmountCurGlobal,
            this.colMedioMarketValueCurGlobal,
            this.colBPS,
            this.colThreshold,
            this.colWeightNav,
            this.colDateSettlement,
            this.colAmountRaised,
            this.colNodeType});
            this.treeDCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeDCB.Location = new System.Drawing.Point(3, 16);
            this.treeDCB.Name = "treeDCB";
            this.treeDCB.OptionsBehavior.AllowRecursiveNodeChecking = true;
            this.treeDCB.OptionsClipboard.AllowCopy = DevExpress.Utils.DefaultBoolean.True;
            this.treeDCB.OptionsClipboard.AllowExcelFormat = DevExpress.Utils.DefaultBoolean.True;
            this.treeDCB.OptionsClipboard.ClipboardMode = DevExpress.Export.ClipboardMode.Formatted;
            this.treeDCB.OptionsView.AutoWidth = false;
            this.treeDCB.OptionsView.CheckBoxStyle = DevExpress.XtraTreeList.DefaultNodeCheckBoxStyle.Check;
            this.treeDCB.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.currencyCbo});
            this.treeDCB.Size = new System.Drawing.Size(1185, 484);
            this.treeDCB.StateImageList = this.imgColl;
            this.treeDCB.TabIndex = 2;
            this.treeDCB.GetStateImage += new DevExpress.XtraTreeList.GetStateImageEventHandler(this.treeDCB_GetStateImage);
            this.treeDCB.NodeCellStyle += new DevExpress.XtraTreeList.GetCustomNodeCellStyleEventHandler(this.treeDCB_NodeCellStyle);
            this.treeDCB.AfterCheckNode += new DevExpress.XtraTreeList.NodeEventHandler(this.treeDCB_AfterCheckNode);
            this.treeDCB.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.treeDCB_ValidatingEditor);
            this.treeDCB.CellValueChanged += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.treeDCB_CellValueChanged);
            this.treeDCB.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.treeDCB_ShowingEditor);
            // 
            // colID
            // 
            this.colID.Caption = "ID";
            this.colID.FieldName = "ID";
            this.colID.Name = "colID";
            // 
            // colParentID
            // 
            this.colParentID.Caption = "ParentID";
            this.colParentID.FieldName = "ParentID";
            this.colParentID.Name = "colParentID";
            // 
            // colName
            // 
            this.colName.Caption = "Name";
            this.colName.FieldName = "Name";
            this.colName.Name = "colName";
            this.colName.Visible = true;
            this.colName.VisibleIndex = 0;
            this.colName.Width = 431;
            // 
            // colCurrency
            // 
            this.colCurrency.AppearanceCell.Options.UseTextOptions = true;
            this.colCurrency.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colCurrency.AppearanceHeader.Options.UseTextOptions = true;
            this.colCurrency.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colCurrency.Caption = "Currency";
            this.colCurrency.ColumnEdit = this.currencyCbo;
            this.colCurrency.FieldName = "Currency";
            this.colCurrency.Name = "colCurrency";
            this.colCurrency.Visible = true;
            this.colCurrency.VisibleIndex = 2;
            this.colCurrency.Width = 60;
            // 
            // currencyCbo
            // 
            this.currencyCbo.AutoHeight = false;
            this.currencyCbo.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.currencyCbo.Name = "currencyCbo";
            // 
            // colAmount
            // 
            this.colAmount.AppearanceHeader.Options.UseTextOptions = true;
            this.colAmount.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colAmount.Caption = "Amount";
            this.colAmount.FieldName = "AmountRounded";
            this.colAmount.Format.FormatString = "N2";
            this.colAmount.Format.FormatType = DevExpress.Utils.FormatType.Custom;
            this.colAmount.Name = "colAmount";
            this.colAmount.Visible = true;
            this.colAmount.VisibleIndex = 3;
            this.colAmount.Width = 90;
            // 
            // colAmountCurGlobal
            // 
            this.colAmountCurGlobal.AppearanceHeader.Options.UseTextOptions = true;
            this.colAmountCurGlobal.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colAmountCurGlobal.Caption = "Amount cur. global";
            this.colAmountCurGlobal.FieldName = "AmountCurGlb";
            this.colAmountCurGlobal.Format.FormatString = "N2";
            this.colAmountCurGlobal.Format.FormatType = DevExpress.Utils.FormatType.Custom;
            this.colAmountCurGlobal.Name = "colAmountCurGlobal";
            this.colAmountCurGlobal.Visible = true;
            this.colAmountCurGlobal.VisibleIndex = 4;
            this.colAmountCurGlobal.Width = 105;
            // 
            // colMedioMarketValueCurGlobal
            // 
            this.colMedioMarketValueCurGlobal.Caption = "Medio Market Value cur. global";
            this.colMedioMarketValueCurGlobal.FieldName = "MedioMarketValueCurGlb";
            this.colMedioMarketValueCurGlobal.Format.FormatString = "N2";
            this.colMedioMarketValueCurGlobal.Format.FormatType = DevExpress.Utils.FormatType.Custom;
            this.colMedioMarketValueCurGlobal.Name = "colMedioMarketValueCurGlobal";
            this.colMedioMarketValueCurGlobal.Visible = true;
            this.colMedioMarketValueCurGlobal.VisibleIndex = 5;
            this.colMedioMarketValueCurGlobal.Width = 164;
            // 
            // colBPS
            // 
            this.colBPS.AppearanceCell.Options.UseTextOptions = true;
            this.colBPS.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colBPS.AppearanceHeader.Options.UseTextOptions = true;
            this.colBPS.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colBPS.Caption = "BPS";
            this.colBPS.FieldName = "BPS";
            this.colBPS.Name = "colBPS";
            this.colBPS.Visible = true;
            this.colBPS.VisibleIndex = 6;
            this.colBPS.Width = 42;
            // 
            // colThreshold
            // 
            this.colThreshold.AppearanceHeader.Options.UseTextOptions = true;
            this.colThreshold.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colThreshold.Caption = "Threshold";
            this.colThreshold.FieldName = "Threshold";
            this.colThreshold.Format.FormatString = "N2";
            this.colThreshold.Format.FormatType = DevExpress.Utils.FormatType.Custom;
            this.colThreshold.Name = "colThreshold";
            this.colThreshold.Visible = true;
            this.colThreshold.VisibleIndex = 7;
            this.colThreshold.Width = 90;
            // 
            // colWeightNav
            // 
            this.colWeightNav.AppearanceHeader.Options.UseTextOptions = true;
            this.colWeightNav.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colWeightNav.Caption = "WeightNav";
            this.colWeightNav.FieldName = "WeightNav";
            this.colWeightNav.Format.FormatString = "N2";
            this.colWeightNav.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colWeightNav.Name = "colWeightNav";
            this.colWeightNav.Visible = true;
            this.colWeightNav.VisibleIndex = 8;
            this.colWeightNav.Width = 90;
            // 
            // colDateSettlement
            // 
            this.colDateSettlement.AppearanceCell.Options.UseTextOptions = true;
            this.colDateSettlement.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colDateSettlement.AppearanceHeader.Options.UseTextOptions = true;
            this.colDateSettlement.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colDateSettlement.Caption = "Settlement Date";
            this.colDateSettlement.FieldName = "DateSettlement";
            this.colDateSettlement.Name = "colDateSettlement";
            this.colDateSettlement.Visible = true;
            this.colDateSettlement.VisibleIndex = 9;
            this.colDateSettlement.Width = 90;
            // 
            // colAmountRaised
            // 
            this.colAmountRaised.AppearanceHeader.Options.UseTextOptions = true;
            this.colAmountRaised.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colAmountRaised.Caption = "Raised Amount";
            this.colAmountRaised.FieldName = "AmountRaised";
            this.colAmountRaised.Format.FormatString = "N2";
            this.colAmountRaised.Format.FormatType = DevExpress.Utils.FormatType.Custom;
            this.colAmountRaised.Name = "colAmountRaised";
            this.colAmountRaised.Visible = true;
            this.colAmountRaised.VisibleIndex = 10;
            this.colAmountRaised.Width = 90;
            // 
            // colNodeType
            // 
            this.colNodeType.Caption = "NodeType";
            this.colNodeType.FieldName = "NodeType";
            this.colNodeType.Name = "colNodeType";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.cmdCopy);
            this.panel4.Controls.Add(this.cmdGenDCB);
            this.panel4.Controls.Add(this.cmdRaiseOrders);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(1206, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(117, 560);
            this.panel4.TabIndex = 10;
            // 
            // cmdCopy
            // 
            this.cmdCopy.Location = new System.Drawing.Point(3, 117);
            this.cmdCopy.Name = "cmdCopy";
            this.cmdCopy.Size = new System.Drawing.Size(111, 23);
            this.cmdCopy.TabIndex = 9;
            this.cmdCopy.Text = "Copy to Clipboard";
            this.cmdCopy.Click += new System.EventHandler(this.cmdCopy_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tableLayoutPanel1.Controls.Add(this.panel4, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1326, 566);
            this.tableLayoutPanel1.TabIndex = 11;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1197, 560);
            this.tableLayoutPanel2.TabIndex = 11;
            // 
            // cmdRefreshSleeves
            // 
            this.cmdRefreshSleeves.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRefreshSleeves.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("cmdRefreshSleeves.ImageOptions.Image")));
            this.cmdRefreshSleeves.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cmdRefreshSleeves.Location = new System.Drawing.Point(844, 3);
            this.cmdRefreshSleeves.Name = "cmdRefreshSleeves";
            this.cmdRefreshSleeves.Size = new System.Drawing.Size(19, 20);
            this.cmdRefreshSleeves.TabIndex = 10;
            this.cmdRefreshSleeves.Click += new System.EventHandler(this.cmdRefreshSleeves_Click);
            // 
            // CSxFXAutomationForm
            // 
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CSxFXAutomationForm";
            this.Size = new System.Drawing.Size(1326, 566);
            this.Title = "FX Automation";
            this.Load += new System.EventHandler(this.CSxFXAutomationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dateParam.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateParam.Properties)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboMarkets.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookupSleeves.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeSleeves)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgColl)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeDCB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.currencyCbo)).EndInit();
            this.panel4.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.SimpleButton cmdRaiseOrders;
        private DevExpress.XtraEditors.LabelControl lblDate;
        private DevExpress.XtraEditors.DateEdit dateParam;
        private System.Windows.Forms.GroupBox groupBox1;
        private DevExpress.XtraEditors.SimpleButton cmdGenDCB;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private System.Windows.Forms.GroupBox groupBox2;
        private DevExpress.XtraEditors.CheckedComboBoxEdit cboMarkets;
        private DevExpress.XtraEditors.TreeListLookUpEdit lookupSleeves;
        private DevExpress.XtraTreeList.TreeList treeSleeves;
        private DevExpress.XtraTreeList.Columns.TreeListColumn tlColID;
        private DevExpress.XtraTreeList.Columns.TreeListColumn tlColName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn tlColParentID;
        private DevExpress.Utils.ImageCollection imgColl;
        private DevExpress.XtraTreeList.TreeList treeDCB;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colID;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colParentID;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colBalance;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colCurrency;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colAmount;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colWeightNav;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colNodeType;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colDateSettlement;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox currencyCbo;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colAmountCurGlobal;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colBPS;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colThreshold;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colMedioMarketValueCurGlobal;
        private DevExpress.XtraEditors.SimpleButton cmdCopy;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colAmountRaised;
        private DevExpress.XtraEditors.SimpleButton cmdRefreshSleeves;
    }
}

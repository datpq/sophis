using System;
using System.Windows.Forms;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTreeList;
using MEDIO.CORE.Tools;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    partial class CSxFXRollingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CSxFXRollingForm));
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.checkBoxMAML = new System.Windows.Forms.CheckBox();
            this.dropDownButton1 = new DevExpress.XtraEditors.DropDownButton();
            this.popupMenu1 = new DevExpress.XtraBars.PopupMenu(this.components);
            this.barListItem1 = new DevExpress.XtraBars.BarListItem();
            this.barEditItem4 = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemDateEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.repositoryItemFixedCurrencyCombo = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox(); //for Fixed CCY
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
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
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SimpleButtonCollapse = new DevExpress.XtraEditors.SimpleButton();
            this.SimpleButtonExpand = new DevExpress.XtraEditors.SimpleButton();
            this.label3 = new System.Windows.Forms.Label();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.SimpleButtonRefresh = new DevExpress.XtraEditors.SimpleButton();
            this.splitContainerControl2 = new DevExpress.XtraEditors.SplitContainerControl();
            this.treeList1 = new DevExpress.XtraTreeList.TreeList();
            this.treeListColumnName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnPayAmount = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnCCY1 = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnRecvAmount = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnCCY2 = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnRate = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnEstRate = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnEstRecvAmount = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnExpiryDate = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.repositoryItemDateEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.treeListColumnFolioID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnWillRoll = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnIsMarketWay = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnFixedCurrency = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumnID = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.textEditOpening = new DevExpress.XtraEditors.TextEdit();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textEditClosing = new DevExpress.XtraEditors.TextEdit();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonCreateOrders = new System.Windows.Forms.Button();
            this.layoutControlItem11 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem6 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem8 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem9 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem11 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem12 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem9 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem10 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem12 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem7 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem3 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.emptySpaceItem4 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem5 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem10 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1.VistaTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCalcEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl2)).BeginInit();

            this.splitContainerControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2.VistaTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemFixedCurrencyCombo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditOpening.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditClosing.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();

            this.SuspendLayout();
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Horizontal = false;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 22);
            this.splitContainerControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.Controls.Add(this.splitContainerControl2);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainerControl1.Size = new System.Drawing.Size(2188, 1187);
            this.splitContainerControl1.SplitterPosition = 95;
            this.splitContainerControl1.TabIndex = 0;
            this.splitContainerControl1.Text = "splitContainerControl1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.richTextBox1);
            this.groupBox1.Controls.Add(this.checkBoxMAML);
            this.groupBox1.Controls.Add(this.dropDownButton1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.SimpleButtonCollapse);
            this.groupBox1.Controls.Add(this.SimpleButtonExpand);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dateTimePickerStart);
            this.groupBox1.Controls.Add(this.dateTimePickerEnd);
            this.groupBox1.Controls.Add(this.SimpleButtonRefresh);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(2188, 95);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.LightYellow;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.ForeColor = System.Drawing.Color.Red;
            this.richTextBox1.Location = new System.Drawing.Point(793, 31);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.Size = new System.Drawing.Size(621, 68);
            this.richTextBox1.TabIndex = 42;
            this.richTextBox1.Text = "New trade(s) were inserted. Please reload the positions by clicking the Reload bu" +
    "tton. \nOrder creation is disabled, and will be re-activated once the reload is d" +
    "one. ";
            // 
            // checkBoxMAML
            // 
            this.checkBoxMAML.AutoSize = true;
            this.checkBoxMAML.Checked = true;
            this.checkBoxMAML.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMAML.Location = new System.Drawing.Point(1852, 75);
            this.checkBoxMAML.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxMAML.Name = "checkBoxMAML";
            this.checkBoxMAML.Size = new System.Drawing.Size(235, 24);
            this.checkBoxMAML.TabIndex = 41;
            this.checkBoxMAML.Text = "Display " + CSxDBHelper.GetTargetTradingFolio() +/* MAML*/" positions only";
            this.checkBoxMAML.UseVisualStyleBackColor = true;
            this.checkBoxMAML.CheckedChanged += new System.EventHandler(this.checkBoxMAML_CheckedChanged);
            // 
            // dropDownButton1
            // 
            this.dropDownButton1.AllowDrop = true;
            this.dropDownButton1.AllowFocus = false;
            this.dropDownButton1.DropDownControl = this.popupMenu1;
            this.dropDownButton1.Location = new System.Drawing.Point(584, 68);
            this.dropDownButton1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dropDownButton1.Name = "dropDownButton1";
            this.dropDownButton1.Size = new System.Drawing.Size(159, 29);
            this.dropDownButton1.TabIndex = 40;
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
            // repositoryItemCCYCombo
            // 
            this.repositoryItemFixedCurrencyCombo.AutoHeight = false;
            this.repositoryItemFixedCurrencyCombo.Name = "repositoryItemFixedCurrencyCombo";
            this.repositoryItemFixedCurrencyCombo.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
                new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemFixedCurrencyCombo.Items.AddRange(new string[] { "CCY2", "CCY1" });
            this.repositoryItemFixedCurrencyCombo.AutoComplete = true;
            //this.repositoryItemFixedCurrencyCombo.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(this.OnFixedCurrency);
            this.repositoryItemFixedCurrencyCombo.TextEditStyle = TextEditStyles.DisableTextEditor;
            this.repositoryItemFixedCurrencyCombo.AllowDropDownWhenReadOnly = DevExpress.Utils.DefaultBoolean.True;
            //this.repositoryItemFixedCurrencyCombo.Editable = true;
            this.treeList1.RepositoryItems.Add(repositoryItemFixedCurrencyCombo);


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
            this.barEditItem4});
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
            this.bar1.OptionsBar.MultiLine = true;
            this.bar1.OptionsBar.UseWholeRow = true;
            this.bar1.Text = "Custom 2";
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.barDockControlTop.Size = new System.Drawing.Size(2188, 22);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 1209);
            this.barDockControlBottom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.barDockControlBottom.Size = new System.Drawing.Size(2188, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 22);
            this.barDockControlLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 1187);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(2188, 22);
            this.barDockControlRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.barDockControlRight.Size = new System.Drawing.Size(0, 1187);
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
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(579, 25);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 25);
            this.label4.TabIndex = 35;
            this.label4.Text = "Forward Date";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(152, 72);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 20);
            this.label1.TabIndex = 32;
            this.label1.Text = "and";
            // 
            // SimpleButtonCollapse
            // 
            this.SimpleButtonCollapse.Image = ((System.Drawing.Image)(resources.GetObject("SimpleButtonCollapse.Image")));
            this.SimpleButtonCollapse.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.SimpleButtonCollapse.Location = new System.Drawing.Point(500, 29);
            this.SimpleButtonCollapse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SimpleButtonCollapse.Name = "SimpleButtonCollapse";
            this.SimpleButtonCollapse.Size = new System.Drawing.Size(51, 69);
            this.SimpleButtonCollapse.TabIndex = 34;
            this.SimpleButtonCollapse.Text = "SimpleButtonCollapse";
            this.SimpleButtonCollapse.ToolTip = "Close";
            this.SimpleButtonCollapse.Click += new System.EventHandler(this.SimpleButtonCollapse_Click);
            // 
            // SimpleButtonExpand
            // 
            this.SimpleButtonExpand.Image = ((System.Drawing.Image)(resources.GetObject("SimpleButtonExpand.Image")));
            this.SimpleButtonExpand.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.SimpleButtonExpand.Location = new System.Drawing.Point(426, 29);
            this.SimpleButtonExpand.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SimpleButtonExpand.Name = "SimpleButtonExpand";
            this.SimpleButtonExpand.Size = new System.Drawing.Size(51, 69);
            this.SimpleButtonExpand.TabIndex = 33;
            this.SimpleButtonExpand.Text = "SimpleButtonExpand";
            this.SimpleButtonExpand.ToolTip = "Roll";
            this.SimpleButtonExpand.Click += new System.EventHandler(this.SimpleButtonExpand_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(20, 25);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(285, 25);
            this.label3.TabIndex = 31;
            this.label3.Text = "Filter forwards expiring between";
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.CustomFormat = "dd/MM/yyyy";
            this.dateTimePickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStart.Location = new System.Drawing.Point(22, 68);
            this.dateTimePickerStart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.Size = new System.Drawing.Size(121, 26);
            this.dateTimePickerStart.TabIndex = 30;
            this.dateTimePickerStart.Value = new System.DateTime(2017, 8, 30, 0, 0, 0, 0);
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.CustomFormat = "dd/MM/yyyy";
            this.dateTimePickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEnd.Location = new System.Drawing.Point(195, 68);
            this.dateTimePickerEnd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.Size = new System.Drawing.Size(121, 26);
            this.dateTimePickerEnd.TabIndex = 38;
            this.dateTimePickerEnd.Value = new System.DateTime(2017, 8, 10, 0, 0, 0, 0);
            // 
            // SimpleButtonRefresh
            // 
            this.SimpleButtonRefresh.Appearance.ForeColor = System.Drawing.Color.Black;
            this.SimpleButtonRefresh.Appearance.Options.UseForeColor = true;
            this.SimpleButtonRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.SimpleButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("SimpleButtonRefresh.Image")));
            this.SimpleButtonRefresh.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.SimpleButtonRefresh.Location = new System.Drawing.Point(351, 29);
            this.SimpleButtonRefresh.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SimpleButtonRefresh.Name = "SimpleButtonRefresh";
            this.SimpleButtonRefresh.Size = new System.Drawing.Size(51, 69);
            this.SimpleButtonRefresh.TabIndex = 36;
            this.SimpleButtonRefresh.Text = "SimpleButtonReFresh";
            this.SimpleButtonRefresh.ToolTip = "Refresh & Cancel Orders";
            this.SimpleButtonRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // splitContainerControl2
            // 
            this.splitContainerControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl2.FixedPanel = DevExpress.XtraEditors.SplitFixedPanel.Panel2;
            this.splitContainerControl2.Horizontal = false;
            this.splitContainerControl2.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl2.Name = "splitContainerControl2";
            this.splitContainerControl2.Panel1.Controls.Add(this.treeList1);
            this.splitContainerControl2.Panel1.Text = "Panel1";
            this.splitContainerControl2.Panel2.Controls.Add(this.layoutControl1);
            this.splitContainerControl2.Panel2.Text = "Panel2";
            this.splitContainerControl2.Size = new System.Drawing.Size(2188, 1087);
            this.splitContainerControl2.SplitterPosition = 82;
            this.splitContainerControl2.TabIndex = 2;
            this.splitContainerControl2.Text = "splitContainerControl2";
            // 
            // treeList1
            // 
            this.treeList1.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.treeListColumnName,
            this.treeListColumnPayAmount,
            this.treeListColumnCCY1,
            this.treeListColumnRecvAmount,
            this.treeListColumnCCY2,
            this.treeListColumnRate,
            this.treeListColumnEstRate,
            this.treeListColumnEstRecvAmount,
            this.treeListColumnExpiryDate,
            this.treeListColumnFolioID,
            this.treeListColumnWillRoll,
            this.treeListColumnIsMarketWay,
            this.treeListColumnFixedCurrency,
            this.treeListColumnID});
            this.treeList1.CustomizationFormBounds = new System.Drawing.Rectangle(1055, 381, 216, 204);
            this.treeList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeList1.HorzScrollVisibility = DevExpress.XtraTreeList.ScrollVisibility.Always;
            this.treeList1.Location = new System.Drawing.Point(0, 0);
            this.treeList1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.treeList1.Name = "treeList1";
            this.treeList1.OptionsBehavior.EnableFiltering = true;
            this.treeList1.OptionsBehavior.PopulateServiceColumns = true;
            this.treeList1.OptionsBehavior.UseTabKey = true;
            this.treeList1.OptionsSelection.MultiSelect = true;
            this.treeList1.OptionsView.AutoWidth = false;
            this.treeList1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemDateEdit2});
            this.treeList1.Size = new System.Drawing.Size(2188, 1000);
            this.treeList1.StateImageList = this.imageList;
            this.treeList1.TabIndex = 1;
            this.treeList1.GetStateImage += new DevExpress.XtraTreeList.GetStateImageEventHandler(this.OnGetStateImage);
            this.treeList1.NodeCellStyle += new DevExpress.XtraTreeList.GetCustomNodeCellStyleEventHandler(this.TreeList1OnNodeCellStyle);
            this.treeList1.CellValueChanging += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.TreeList1OnCellValueChanging);
            this.treeList1.CustomDrawNodeCell += TreeList1OnCustomDrawNodeCell;
            this.treeList1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TreeListOnKeyUp);
            // 
            // treeListColumnName
            // 
            this.treeListColumnName.FieldName = "Name";
            this.treeListColumnName.MinWidth = 33;
            this.treeListColumnName.Name = "treeListColumnName";
            this.treeListColumnName.OptionsColumn.AllowEdit = false;
            this.treeListColumnName.OptionsColumn.ReadOnly = true;
            this.treeListColumnName.Visible = true;
            this.treeListColumnName.VisibleIndex = 0;
            this.treeListColumnName.Width = 213;
            // 
            // treeListColumnPayAmount
            // 
            this.treeListColumnPayAmount.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnPayAmount.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnPayAmount.Caption = "CCY1 Amount";
            this.treeListColumnPayAmount.FieldName = "PayAmount";
            this.treeListColumnPayAmount.Format.FormatString = "{0:n2}";
            this.treeListColumnPayAmount.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.treeListColumnPayAmount.Name = "treeListColumnPayAmount";
            this.treeListColumnPayAmount.OptionsColumn.AllowEdit = false;
            this.treeListColumnPayAmount.OptionsColumn.ReadOnly = true;
            this.treeListColumnPayAmount.Visible = true;
            this.treeListColumnPayAmount.VisibleIndex = 1;
            this.treeListColumnPayAmount.Width = 114;
            // 
            // treeListColumnCCY1
            // 
            this.treeListColumnCCY1.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnCCY1.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnCCY1.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnCCY1.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnCCY1.Caption = "CCY1";
            this.treeListColumnCCY1.FieldName = "CCY1";
            this.treeListColumnCCY1.Name = "treeListColumnCCY1";
            this.treeListColumnCCY1.OptionsColumn.AllowEdit = false;
            this.treeListColumnCCY1.OptionsColumn.ReadOnly = true;
            this.treeListColumnCCY1.Visible = true;
            this.treeListColumnCCY1.VisibleIndex = 2;
            this.treeListColumnCCY1.Width = 66;
            // 
            // treeListColumnRecvAmount
            // 
            this.treeListColumnRecvAmount.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnRecvAmount.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnRecvAmount.Caption = "CCY2 Amount";
            this.treeListColumnRecvAmount.FieldName = "ReceiveAmount";
            this.treeListColumnRecvAmount.Format.FormatString = "{0:n2}";
            this.treeListColumnRecvAmount.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.treeListColumnRecvAmount.Name = "treeListColumnRecvAmount";
            this.treeListColumnRecvAmount.OptionsColumn.AllowEdit = false;
            this.treeListColumnRecvAmount.OptionsColumn.ReadOnly = true;
            this.treeListColumnRecvAmount.Visible = true;
            this.treeListColumnRecvAmount.VisibleIndex = 3;
            this.treeListColumnRecvAmount.Width = 142;
            // 
            // treeListColumnCCY2
            // 
            this.treeListColumnCCY2.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnCCY2.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnCCY2.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnCCY2.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnCCY2.Caption = "CCY2";
            this.treeListColumnCCY2.FieldName = "CCY2";
            this.treeListColumnCCY2.Name = "treeListColumnCCY2";
            this.treeListColumnCCY2.OptionsColumn.AllowEdit = false;
            this.treeListColumnCCY2.OptionsColumn.ReadOnly = true;
            this.treeListColumnCCY2.Visible = true;
            this.treeListColumnCCY2.VisibleIndex = 4;
            this.treeListColumnCCY2.Width = 68;
            // 
            // treeListColumnRate
            // 
            this.treeListColumnRate.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnRate.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnRate.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnRate.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnRate.Caption = "Avg. FX rate";
            this.treeListColumnRate.FieldName = "FXRate";
            this.treeListColumnRate.Format.FormatString = "n4";
            this.treeListColumnRate.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.treeListColumnRate.Name = "treeListColumnRate";
            this.treeListColumnRate.OptionsColumn.AllowEdit = false;
            this.treeListColumnRate.OptionsColumn.ReadOnly = true;
            this.treeListColumnRate.Visible = true;
            this.treeListColumnRate.VisibleIndex = 5;
            this.treeListColumnRate.Width = 108;
            // 
            // treeListColumnEstRate
            // 
            this.treeListColumnEstRate.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnEstRate.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnEstRate.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnEstRate.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnEstRate.Caption = "Est. FX rate";
            this.treeListColumnEstRate.FieldName = "EstFXRate";
            this.treeListColumnEstRate.Format.FormatString = "n4";
            this.treeListColumnEstRate.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.treeListColumnEstRate.Name = "treeListColumnEstRate";
            this.treeListColumnEstRate.OptionsColumn.AllowEdit = false;
            this.treeListColumnEstRate.OptionsColumn.ReadOnly = true;
            this.treeListColumnEstRate.Visible = true;
            this.treeListColumnEstRate.VisibleIndex = 6;
            this.treeListColumnEstRate.Width = 137;
            // 
            // treeListColumnEstRecvAmount
            // 
            this.treeListColumnEstRecvAmount.Caption = "Est. Receive Amount";
            this.treeListColumnEstRecvAmount.FieldName = "EstRecvAmount";
            this.treeListColumnEstRecvAmount.Format.FormatString = "n2";
            this.treeListColumnEstRecvAmount.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.treeListColumnEstRecvAmount.Name = "treeListColumnEstRecvAmount";
            this.treeListColumnEstRecvAmount.OptionsColumn.AllowEdit = false;
            this.treeListColumnEstRecvAmount.OptionsColumn.ReadOnly = true;
            this.treeListColumnEstRecvAmount.Visible = true;
            this.treeListColumnEstRecvAmount.VisibleIndex = 7;
            this.treeListColumnEstRecvAmount.Width = 168;
            // 
            // treeListColumnExpiryDate
            // 
            this.treeListColumnExpiryDate.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnExpiryDate.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnExpiryDate.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnExpiryDate.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnExpiryDate.Caption = "Expiry Date";
            this.treeListColumnExpiryDate.ColumnEdit = this.repositoryItemDateEdit2;
            this.treeListColumnExpiryDate.FieldName = "ExpiryDate";
            this.treeListColumnExpiryDate.Format.FormatString = "d";
            this.treeListColumnExpiryDate.Format.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.treeListColumnExpiryDate.Name = "treeListColumnExpiryDate";
            this.treeListColumnExpiryDate.OptionsColumn.AllowEdit = false;
            this.treeListColumnExpiryDate.Visible = true;
            this.treeListColumnExpiryDate.VisibleIndex = 8;
            this.treeListColumnExpiryDate.Width = 109;
            // 
            // repositoryItemDateEdit2
            // 
            this.repositoryItemDateEdit2.AutoHeight = false;
            this.repositoryItemDateEdit2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemDateEdit2.Name = "repositoryItemDateEdit2";
            this.repositoryItemDateEdit2.VistaTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.repositoryItemDateEdit2.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(this.OnExpiryDateEdit);
            // 
            // treeListColumnFolioID
            // 
            this.treeListColumnFolioID.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnFolioID.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnFolioID.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnFolioID.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnFolioID.Caption = "Ident";
            this.treeListColumnFolioID.FieldName = "FolioID";
            this.treeListColumnFolioID.Format.FormatString = "n0";
            this.treeListColumnFolioID.Format.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.treeListColumnFolioID.Name = "treeListColumnFolioID";
            this.treeListColumnFolioID.OptionsColumn.AllowEdit = false;
            this.treeListColumnFolioID.OptionsColumn.ReadOnly = true;
            this.treeListColumnFolioID.Visible = true;
            this.treeListColumnFolioID.VisibleIndex = 9;
            this.treeListColumnFolioID.Width = 77;
            // 
            // treeListColumnWillRoll
            // 
            this.treeListColumnWillRoll.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnWillRoll.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnWillRoll.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnWillRoll.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnWillRoll.Caption = "Roll";
            this.treeListColumnWillRoll.FieldName = "WillRoll";
            this.treeListColumnWillRoll.Name = "treeListColumnWillRoll";
            this.treeListColumnWillRoll.Visible = true;
            this.treeListColumnWillRoll.VisibleIndex = 10;
            this.treeListColumnWillRoll.Width = 83;
            // 
            // treeListColumnIsMarketWay
            // 
            this.treeListColumnIsMarketWay.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnIsMarketWay.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnIsMarketWay.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnIsMarketWay.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnIsMarketWay.Caption = "Is Market Way";
            this.treeListColumnIsMarketWay.FieldName = "IsMarketWay";
            this.treeListColumnIsMarketWay.Name = "treeListColumnIsMarketWay";
            this.treeListColumnIsMarketWay.OptionsColumn.AllowEdit = false;
            this.treeListColumnIsMarketWay.OptionsColumn.ReadOnly = true;
            this.treeListColumnIsMarketWay.Visible = true;
            this.treeListColumnIsMarketWay.VisibleIndex = 11;
            this.treeListColumnIsMarketWay.Width = 128;

            // 
            // treeListColumnFixedCurrency
            // 
            this.treeListColumnFixedCurrency.AppearanceCell.Options.UseTextOptions = true;
            this.treeListColumnFixedCurrency.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnFixedCurrency.AppearanceHeader.Options.UseTextOptions = true;
            this.treeListColumnFixedCurrency.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.treeListColumnFixedCurrency.Caption = "Fixed Currency";
            this.treeListColumnFixedCurrency.FieldName = "FixedCurrency";
            this.treeListColumnFixedCurrency.Name = "treeListColumnFixedCurrency";
            //this.treeListColumnFixedCurrency.ColumnEdit = repositoryItemDateEdit2; 
            this.treeListColumnFixedCurrency.ColumnEdit = repositoryItemFixedCurrencyCombo;
            this.treeListColumnFixedCurrency.OptionsColumn.AllowEdit = true;
            this.treeListColumnFixedCurrency.OptionsColumn.ReadOnly = false;
            this.treeListColumnFixedCurrency.Visible = true;
            this.treeListColumnFixedCurrency.VisibleIndex = 12;
            this.treeListColumnFixedCurrency.Width = 96;


            // 
            // treeListColumnID
            // 
            this.treeListColumnID.Caption = "treeListColumn1";
            this.treeListColumnID.FieldName = "ID";
            this.treeListColumnID.Name = "treeListColumnID";
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.textEditOpening);
            this.layoutControl1.Controls.Add(this.label8);
            this.layoutControl1.Controls.Add(this.label7);
            this.layoutControl1.Controls.Add(this.textEditClosing);
            this.layoutControl1.Controls.Add(this.label6);
            this.layoutControl1.Controls.Add(this.label5);
            this.layoutControl1.Controls.Add(this.buttonClose);
            this.layoutControl1.Controls.Add(this.buttonCreateOrders);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.HiddenItems.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem11});
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(437, 187, 687, 520);
            this.layoutControl1.Root = this.layoutControlGroup2;
            this.layoutControl1.Size = new System.Drawing.Size(2188, 82);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // textEditOpening
            // 
            this.textEditOpening.EditValue = "100";
            this.textEditOpening.Location = new System.Drawing.Point(183, 42);
            this.textEditOpening.MenuManager = this.barManager1;
            this.textEditOpening.Name = "textEditOpening";
            this.textEditOpening.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.textEditOpening.Size = new System.Drawing.Size(91, 26);
            this.textEditOpening.StyleController = this.layoutControl1;
            this.textEditOpening.TabIndex = 17;

            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(278, 43);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(196, 25);
            this.label8.TabIndex = 16;
            this.label8.Text = "%";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(278, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(196, 27);
            this.label7.TabIndex = 16;
            this.label7.Text = "%";
            // 
            // textEditClosing
            // 
            this.textEditClosing.EditValue = "100";
            this.textEditClosing.Location = new System.Drawing.Point(183, 12);
            this.textEditClosing.MenuManager = this.barManager1;
            this.textEditClosing.Name = "textEditClosing";
            this.textEditClosing.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.textEditClosing.Size = new System.Drawing.Size(91, 26);
            this.textEditClosing.StyleController = this.layoutControl1;
            this.textEditClosing.TabIndex = 13;

            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 43);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(167, 25);
            this.label6.TabIndex = 9;
            this.label6.Text = "Opening Order Ratio";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(167, 27);
            this.label5.TabIndex = 8;
            this.label5.Text = "Closing Order Ratio";
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(1337, 22);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(134, 36);
            this.buttonClose.TabIndex = 5;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonCreateOrders
            // 
            this.buttonCreateOrders.Location = new System.Drawing.Point(1160, 22);
            this.buttonCreateOrders.Name = "buttonCreateOrders";
            this.buttonCreateOrders.Size = new System.Drawing.Size(174, 36);
            this.buttonCreateOrders.TabIndex = 4;
            this.buttonCreateOrders.Text = "Create Orders";
            this.buttonCreateOrders.UseVisualStyleBackColor = true;
            this.buttonCreateOrders.Click += new System.EventHandler(this.buttonCreateOrders_Click);
            // 
            // layoutControlItem11
            // 
            this.layoutControlItem11.CustomizationFormText = "layoutControlItem11";
            this.layoutControlItem11.Location = new System.Drawing.Point(227, 30);
            this.layoutControlItem11.Name = "layoutControlItem11";
            this.layoutControlItem11.Size = new System.Drawing.Size(203, 30);
            this.layoutControlItem11.Text = "layoutControlItem11";
            this.layoutControlItem11.TextSize = new System.Drawing.Size(50, 20);
            this.layoutControlItem11.TextToControlDistance = 5;
            // 
            // layoutControlGroup2
            // 
            this.layoutControlGroup2.CustomizationFormText = "Root";
            this.layoutControlGroup2.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup2.GroupBordersVisible = false;
            this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem7,
            this.layoutControlItem5,
            this.emptySpaceItem6,
            this.emptySpaceItem8,
            this.emptySpaceItem9,
            this.emptySpaceItem11,
            this.emptySpaceItem12,
            this.layoutControlItem6,
            this.layoutControlItem9,
            this.layoutControlItem10,
            this.layoutControlItem12,
            this.emptySpaceItem7});
            this.layoutControlGroup2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup2.Name = "Root";
            this.layoutControlGroup2.Size = new System.Drawing.Size(2188, 82);
            this.layoutControlGroup2.Text = "Root";
            // 
            // layoutControlItem3
            // 
            //this.layoutControlItem3.Control = this.buttonCreateOrders;
            this.layoutControlItem3.CustomizationFormText = "layoutControlItem3";
            this.layoutControlItem3.Location = new System.Drawing.Point(1148, 10);
            this.layoutControlItem3.MaxSize = new System.Drawing.Size(180, 40);
            this.layoutControlItem3.MinSize = new System.Drawing.Size(125, 40);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(180, 40);
            this.layoutControlItem3.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem3.Text = "layoutControlItem3";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextToControlDistance = 0;
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.buttonClose;
            this.layoutControlItem4.CustomizationFormText = "layoutControlItem4";
            this.layoutControlItem4.Location = new System.Drawing.Point(1325, 10);
            this.layoutControlItem4.MaxSize = new System.Drawing.Size(138, 40);
            this.layoutControlItem4.MinSize = new System.Drawing.Size(138, 40);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(843, 40);
            this.layoutControlItem4.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem4.Text = "layoutControlItem4";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextToControlDistance = 0;
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.label5;
            this.layoutControlItem7.CustomizationFormText = "layoutControlItem7";
            this.layoutControlItem7.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem7.MaxSize = new System.Drawing.Size(171, 31);
            this.layoutControlItem7.MinSize = new System.Drawing.Size(171, 31);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(171, 31);
            this.layoutControlItem7.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem7.Text = "layoutControlItem7";
            this.layoutControlItem7.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem7.TextToControlDistance = 0;
            this.layoutControlItem7.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.label6;
            this.layoutControlItem5.CustomizationFormText = "layoutControlItem5";
            this.layoutControlItem5.Location = new System.Drawing.Point(0, 31);
            this.layoutControlItem5.MaxSize = new System.Drawing.Size(171, 29);
            this.layoutControlItem5.MinSize = new System.Drawing.Size(171, 29);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(171, 31);
            this.layoutControlItem5.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem5.Text = "layoutControlItem5";
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextToControlDistance = 0;
            this.layoutControlItem5.TextVisible = false;
            // 
            // emptySpaceItem6
            // 
            this.emptySpaceItem6.AllowHotTrack = false;
            this.emptySpaceItem6.CustomizationFormText = "emptySpaceItem6";
            this.emptySpaceItem6.Location = new System.Drawing.Point(466, 0);
            this.emptySpaceItem6.MaxSize = new System.Drawing.Size(682, 60);
            this.emptySpaceItem6.MinSize = new System.Drawing.Size(682, 60);
            this.emptySpaceItem6.Name = "emptySpaceItem6";
            this.emptySpaceItem6.Size = new System.Drawing.Size(682, 62);
            this.emptySpaceItem6.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.emptySpaceItem6.Text = "emptySpaceItem6";
            this.emptySpaceItem6.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem8
            // 
            this.emptySpaceItem8.AllowHotTrack = false;
            this.emptySpaceItem8.CustomizationFormText = "emptySpaceItem8";
            this.emptySpaceItem8.Location = new System.Drawing.Point(1148, 0);
            this.emptySpaceItem8.MaxSize = new System.Drawing.Size(125, 10);
            this.emptySpaceItem8.MinSize = new System.Drawing.Size(125, 10);
            this.emptySpaceItem8.Name = "emptySpaceItem8";
            this.emptySpaceItem8.Size = new System.Drawing.Size(125, 10);
            this.emptySpaceItem8.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.emptySpaceItem8.Text = "emptySpaceItem8";
            this.emptySpaceItem8.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem9
            // 
            this.emptySpaceItem9.AllowHotTrack = false;
            this.emptySpaceItem9.CustomizationFormText = "emptySpaceItem9";
            this.emptySpaceItem9.Location = new System.Drawing.Point(1148, 50);
            this.emptySpaceItem9.Name = "emptySpaceItem9";
            this.emptySpaceItem9.Size = new System.Drawing.Size(125, 12);
            this.emptySpaceItem9.Text = "emptySpaceItem9";
            this.emptySpaceItem9.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem11
            // 
            this.emptySpaceItem11.AllowHotTrack = false;
            this.emptySpaceItem11.CustomizationFormText = "emptySpaceItem11";
            this.emptySpaceItem11.Location = new System.Drawing.Point(1325, 0);
            this.emptySpaceItem11.MaxSize = new System.Drawing.Size(138, 10);
            this.emptySpaceItem11.MinSize = new System.Drawing.Size(138, 10);
            this.emptySpaceItem11.Name = "emptySpaceItem11";
            this.emptySpaceItem11.Size = new System.Drawing.Size(843, 10);
            this.emptySpaceItem11.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.emptySpaceItem11.Text = "emptySpaceItem11";
            this.emptySpaceItem11.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem12
            // 
            this.emptySpaceItem12.AllowHotTrack = false;
            this.emptySpaceItem12.CustomizationFormText = "emptySpaceItem12";
            this.emptySpaceItem12.Location = new System.Drawing.Point(1325, 50);
            this.emptySpaceItem12.Name = "emptySpaceItem12";
            this.emptySpaceItem12.Size = new System.Drawing.Size(843, 12);
            this.emptySpaceItem12.Text = "emptySpaceItem12";
            this.emptySpaceItem12.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.textEditClosing;
            this.layoutControlItem6.CustomizationFormText = "layoutControlItem6";
            this.layoutControlItem6.Location = new System.Drawing.Point(171, 0);
            this.layoutControlItem6.MaxSize = new System.Drawing.Size(95, 30);
            this.layoutControlItem6.MinSize = new System.Drawing.Size(95, 30);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(95, 30);
            this.layoutControlItem6.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem6.Text = "layoutControlItem6";
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextToControlDistance = 0;
            this.layoutControlItem6.TextVisible = false;
            // 
            // layoutControlItem9
            // 
            this.layoutControlItem9.Control = this.label7;
            this.layoutControlItem9.CustomizationFormText = "layoutControlItem9";
            this.layoutControlItem9.Location = new System.Drawing.Point(266, 0);
            this.layoutControlItem9.MaxSize = new System.Drawing.Size(200, 31);
            this.layoutControlItem9.MinSize = new System.Drawing.Size(200, 31);
            this.layoutControlItem9.Name = "layoutControlItem9";
            this.layoutControlItem9.Size = new System.Drawing.Size(200, 31);
            this.layoutControlItem9.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem9.Text = "layoutControlItem9";
            this.layoutControlItem9.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem9.TextToControlDistance = 0;
            this.layoutControlItem9.TextVisible = false;
            // 
            // layoutControlItem10
            // 
            this.layoutControlItem10.Control = this.label8;
            this.layoutControlItem10.CustomizationFormText = "layoutControlItem10";
            this.layoutControlItem10.Location = new System.Drawing.Point(266, 31);
            this.layoutControlItem10.MaxSize = new System.Drawing.Size(200, 29);
            this.layoutControlItem10.MinSize = new System.Drawing.Size(200, 29);
            this.layoutControlItem10.Name = "layoutControlItem10";
            this.layoutControlItem10.Size = new System.Drawing.Size(200, 31);
            this.layoutControlItem10.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem10.Text = "layoutControlItem10";
            this.layoutControlItem10.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem10.TextToControlDistance = 0;
            this.layoutControlItem10.TextVisible = false;
            // 
            // layoutControlItem12
            // 
            this.layoutControlItem12.Control = this.textEditOpening;
            this.layoutControlItem12.CustomizationFormText = "layoutControlItem12";
            this.layoutControlItem12.Location = new System.Drawing.Point(171, 30);
            this.layoutControlItem12.MaxSize = new System.Drawing.Size(95, 30);
            this.layoutControlItem12.MinSize = new System.Drawing.Size(95, 30);
            this.layoutControlItem12.Name = "layoutControlItem12";
            this.layoutControlItem12.Size = new System.Drawing.Size(95, 32);
            this.layoutControlItem12.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem12.Text = "layoutControlItem12";
            this.layoutControlItem12.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem12.TextToControlDistance = 0;
            this.layoutControlItem12.TextVisible = false;
            // 
            // emptySpaceItem7
            // 
            this.emptySpaceItem7.AllowHotTrack = false;
            this.emptySpaceItem7.CustomizationFormText = "emptySpaceItem7";
            this.emptySpaceItem7.Location = new System.Drawing.Point(1273, 0);
            this.emptySpaceItem7.MaxSize = new System.Drawing.Size(52, 60);
            this.emptySpaceItem7.MinSize = new System.Drawing.Size(52, 60);
            this.emptySpaceItem7.Name = "emptySpaceItem7";
            this.emptySpaceItem7.Size = new System.Drawing.Size(52, 62);
            this.emptySpaceItem7.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.emptySpaceItem7.Text = "emptySpaceItem7";
            this.emptySpaceItem7.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 667);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(902, 26);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem3
            // 
            this.emptySpaceItem3.AllowHotTrack = false;
            this.emptySpaceItem3.CustomizationFormText = "emptySpaceItem3";
            this.emptySpaceItem3.Location = new System.Drawing.Point(1019, 667);
            this.emptySpaceItem3.Name = "emptySpaceItem3";
            this.emptySpaceItem3.Size = new System.Drawing.Size(118, 26);
            this.emptySpaceItem3.Text = "emptySpaceItem3";
            this.emptySpaceItem3.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.CustomizationFormText = "emptySpaceItem2";
            this.emptySpaceItem2.Location = new System.Drawing.Point(1283, 662);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(70, 31);
            this.emptySpaceItem2.Text = "emptySpaceItem2";
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.emptySpaceItem1,
            this.emptySpaceItem2,
            this.emptySpaceItem3});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(1419, 54);
            this.layoutControlGroup1.Text = "Root";
            this.layoutControlGroup1.TextVisible = false;
            // 
            // emptySpaceItem4
            // 
            this.emptySpaceItem4.AllowHotTrack = false;
            this.emptySpaceItem4.CustomizationFormText = "emptySpaceItem2";
            this.emptySpaceItem4.Location = new System.Drawing.Point(0, 0);
            this.emptySpaceItem4.Name = "emptySpaceItem2";
            this.emptySpaceItem4.Size = new System.Drawing.Size(1621, 72);
            this.emptySpaceItem4.Text = "emptySpaceItem2";
            this.emptySpaceItem4.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(1621, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(201, 72);
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
            // 
            // emptySpaceItem5
            // 
            this.emptySpaceItem5.AllowHotTrack = false;
            this.emptySpaceItem5.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem5.Location = new System.Drawing.Point(1822, 0);
            this.emptySpaceItem5.Name = "emptySpaceItem1";
            this.emptySpaceItem5.Size = new System.Drawing.Size(78, 72);
            this.emptySpaceItem5.Text = "emptySpaceItem1";
            this.emptySpaceItem5.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(1900, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(154, 72);
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            // 
            // emptySpaceItem10
            // 
            this.emptySpaceItem10.AllowHotTrack = false;
            this.emptySpaceItem10.CustomizationFormText = "emptySpaceItem8";
            this.emptySpaceItem10.Location = new System.Drawing.Point(1084, 0);
            this.emptySpaceItem10.Name = "emptySpaceItem8";
            this.emptySpaceItem10.Size = new System.Drawing.Size(144, 12);
            this.emptySpaceItem10.Text = "emptySpaceItem8";
            this.emptySpaceItem10.TextSize = new System.Drawing.Size(0, 0);
            // 
            // layoutControlItem8
            // 
            this.layoutControlItem8.Control = this.textEditClosing;
            this.layoutControlItem8.CustomizationFormText = "layoutControlItem6";
            this.layoutControlItem8.Location = new System.Drawing.Point(167, 0);
            this.layoutControlItem8.Name = "layoutControlItem6";
            this.layoutControlItem8.Size = new System.Drawing.Size(194, 30);
            this.layoutControlItem8.Text = "layoutControlItem6";
            this.layoutControlItem8.TextSize = new System.Drawing.Size(137, 19);
            this.layoutControlItem8.TextToControlDistance = 5;
            // 
            // CSxFXRollingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "CSxFXRollingForm";
            this.Size = new System.Drawing.Size(2188, 1209);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1.VistaTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCalcEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl2)).EndInit();
            this.splitContainerControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2.VistaTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemFixedCurrencyCombo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.textEditOpening.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditClosing.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();

            this.ResumeLayout(false);

        }



        #endregion

        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.SimpleButton SimpleButtonCollapse;
        private DevExpress.XtraEditors.SimpleButton SimpleButtonExpand;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.DateTimePicker dateTimePickerEnd;
        private DevExpress.XtraEditors.SimpleButton SimpleButtonRefresh;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem3;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
        private DevExpress.XtraBars.PopupMenu popupMenu1;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
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
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemFixedCurrencyCombo;
        private DevExpress.XtraEditors.DropDownButton dropDownButton1;
        private System.Windows.Forms.ImageList imageList;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl2;
        private TreeList treeList1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnPayAmount;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnCCY1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnRecvAmount;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnCCY2;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnRate;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnEstRate;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnEstRecvAmount;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnExpiryDate;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnFolioID;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnWillRoll;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnIsMarketWay;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnFixedCurrency;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumnID;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private Button buttonClose;
        private Button buttonCreateOrders;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit repositoryItemDateEdit2;
        private Label label6;
        private Label label5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem6;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem8;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem9;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem11;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem12;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem10;
        private Label label8;
        private Label label7;
        private DevExpress.XtraEditors.TextEdit textEditClosing;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem11;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem9;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem10;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem8;
        private DevExpress.XtraEditors.TextEdit textEditOpening;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem12;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem7;
        private CheckBox checkBoxMAML;
        private RichTextBox richTextBox1;
    }
}
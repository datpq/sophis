namespace CFG_AccountClosing.SourceFiles
{
    partial class InterfaceAffectationResultat
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InterfaceAffectationResultat));
            this.SkipAll = new DevExpress.XtraEditors.SimpleButton();
            this.Skip = new DevExpress.XtraEditors.SimpleButton();
            this.Launch = new DevExpress.XtraEditors.SimpleButton();
            this.GridControlAccounts = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.FundNameEdit = new DevExpress.XtraEditors.TextEdit();
            this.Fund = new System.Windows.Forms.Label();
            this.AffectationDatePicker = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.AmountToDistributeEdit = new DevExpress.XtraEditors.TextEdit();
            this.label3 = new System.Windows.Forms.Label();
            this.ShareNbEdit = new DevExpress.XtraEditors.TextEdit();
            this.label4 = new System.Windows.Forms.Label();
            this.AmountDistributedEdit = new DevExpress.XtraEditors.TextEdit();
            this.label5 = new System.Windows.Forms.Label();
            this.CapitalizedAmountEdit = new DevExpress.XtraEditors.TextEdit();
            ((System.ComponentModel.ISupportInitialize)(this.GridControlAccounts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FundNameEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AmountToDistributeEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShareNbEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AmountDistributedEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CapitalizedAmountEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // SkipAll
            // 
            this.SkipAll.Location = new System.Drawing.Point(485, 457);
            this.SkipAll.Name = "SkipAll";
            this.SkipAll.Size = new System.Drawing.Size(75, 23);
            this.SkipAll.TabIndex = 32;
            this.SkipAll.Text = "Skip All";
            this.SkipAll.Click += new System.EventHandler(this.SkipAll_Click);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(274, 457);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(75, 23);
            this.Skip.TabIndex = 31;
            this.Skip.Text = "Skip";
            this.Skip.Click += new System.EventHandler(this.Skip_Click);
            // 
            // Launch
            // 
            this.Launch.Location = new System.Drawing.Point(39, 457);
            this.Launch.Name = "Launch";
            this.Launch.Size = new System.Drawing.Size(157, 23);
            this.Launch.TabIndex = 30;
            this.Launch.Text = "Launch Accounting Distribution";
            this.Launch.Click += new System.EventHandler(this.Launch_Click);
            // 
            // GridControlAccounts
            // 
            this.GridControlAccounts.Location = new System.Drawing.Point(12, 71);
            this.GridControlAccounts.MainView = this.gridView1;
            this.GridControlAccounts.Name = "GridControlAccounts";
            this.GridControlAccounts.Size = new System.Drawing.Size(658, 366);
            this.GridControlAccounts.TabIndex = 28;
            this.GridControlAccounts.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.GridControlAccounts;
            this.gridView1.Name = "gridView1";
            // 
            // FundNameEdit
            // 
            this.FundNameEdit.Enabled = false;
            this.FundNameEdit.Location = new System.Drawing.Point(12, 33);
            this.FundNameEdit.Name = "FundNameEdit";
            this.FundNameEdit.Size = new System.Drawing.Size(170, 20);
            this.FundNameEdit.TabIndex = 23;
            // 
            // Fund
            // 
            this.Fund.AutoSize = true;
            this.Fund.Location = new System.Drawing.Point(9, 8);
            this.Fund.Name = "Fund";
            this.Fund.Size = new System.Drawing.Size(68, 13);
            this.Fund.TabIndex = 22;
            this.Fund.Text = "Fund Name :";
            // 
            // AffectationDatePicker
            // 
            this.AffectationDatePicker.Enabled = false;
            this.AffectationDatePicker.Location = new System.Drawing.Point(716, 87);
            this.AffectationDatePicker.Name = "AffectationDatePicker";
            this.AffectationDatePicker.Size = new System.Drawing.Size(200, 20);
            this.AffectationDatePicker.TabIndex = 33;
            this.AffectationDatePicker.ValueChanged += new System.EventHandler(this.AffectationDatePicker_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(715, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 34;
            this.label1.Text = "Distribution Date :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(716, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 35;
            this.label2.Text = "Amount to distribute :";
            // 
            // AmountToDistributeEdit
            // 
            this.AmountToDistributeEdit.Enabled = false;
            this.AmountToDistributeEdit.Location = new System.Drawing.Point(719, 130);
            this.AmountToDistributeEdit.Name = "AmountToDistributeEdit";
            this.AmountToDistributeEdit.Size = new System.Drawing.Size(170, 20);
            this.AmountToDistributeEdit.TabIndex = 36;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(716, 162);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 37;
            this.label3.Text = "Number of Shares :";
            // 
            // ShareNbEdit
            // 
            this.ShareNbEdit.Enabled = false;
            this.ShareNbEdit.Location = new System.Drawing.Point(719, 178);
            this.ShareNbEdit.Name = "ShareNbEdit";
            this.ShareNbEdit.Size = new System.Drawing.Size(170, 20);
            this.ShareNbEdit.TabIndex = 38;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(716, 265);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 39;
            this.label4.Text = "Amount distributed :";
            // 
            // AmountDistributedEdit
            // 
            this.AmountDistributedEdit.Location = new System.Drawing.Point(719, 281);
            this.AmountDistributedEdit.Name = "AmountDistributedEdit";
            this.AmountDistributedEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.AmountDistributedEdit.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.AmountDistributedEdit.Size = new System.Drawing.Size(170, 20);
            this.AmountDistributedEdit.TabIndex = 40;
            this.AmountDistributedEdit.EditValueChanged += new System.EventHandler(this.AmountDistributed_EditValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(716, 333);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(102, 13);
            this.label5.TabIndex = 41;
            this.label5.Text = "Amount capitalized :";
            // 
            // CapitalizedAmountEdit
            // 
            this.CapitalizedAmountEdit.Location = new System.Drawing.Point(719, 349);
            this.CapitalizedAmountEdit.Name = "CapitalizedAmountEdit";
            this.CapitalizedAmountEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.CapitalizedAmountEdit.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.CapitalizedAmountEdit.Size = new System.Drawing.Size(170, 20);
            this.CapitalizedAmountEdit.TabIndex = 42;
            this.CapitalizedAmountEdit.EditValueChanged += new System.EventHandler(this.AmountCapitalized_EditValueChanged);
            // 
            // InterfaceAffectationResultat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(928, 503);
            this.Controls.Add(this.CapitalizedAmountEdit);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.AmountDistributedEdit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ShareNbEdit);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AmountToDistributeEdit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AffectationDatePicker);
            this.Controls.Add(this.SkipAll);
            this.Controls.Add(this.Skip);
            this.Controls.Add(this.Launch);
            this.Controls.Add(this.GridControlAccounts);
            this.Controls.Add(this.FundNameEdit);
            this.Controls.Add(this.Fund);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InterfaceAffectationResultat";
            this.Text = "Distribution Resultat";
            ((System.ComponentModel.ISupportInitialize)(this.GridControlAccounts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FundNameEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AmountToDistributeEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShareNbEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AmountDistributedEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CapitalizedAmountEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton SkipAll;
        private DevExpress.XtraEditors.SimpleButton Skip;
        private DevExpress.XtraEditors.SimpleButton Launch;
        private DevExpress.XtraGrid.GridControl GridControlAccounts;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.TextEdit FundNameEdit;
        private System.Windows.Forms.Label Fund;
        private System.Windows.Forms.DateTimePicker AffectationDatePicker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private DevExpress.XtraEditors.TextEdit AmountToDistributeEdit;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraEditors.TextEdit ShareNbEdit;
        private System.Windows.Forms.Label label4;
        private DevExpress.XtraEditors.TextEdit AmountDistributedEdit;
        private System.Windows.Forms.Label label5;
        private DevExpress.XtraEditors.TextEdit CapitalizedAmountEdit;
    }
}
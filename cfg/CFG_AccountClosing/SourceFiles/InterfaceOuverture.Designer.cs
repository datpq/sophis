namespace CFG_AccountClosing.SourceFiles
{
    partial class InterfaceOuverture
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InterfaceOuverture));
            this.SkipAll = new DevExpress.XtraEditors.SimpleButton();
            this.Skip = new DevExpress.XtraEditors.SimpleButton();
            this.Launch = new DevExpress.XtraEditors.SimpleButton();
            this.gridControlReceivingAccount = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.GridControlAccounts = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.OpeningDateEdit = new DevExpress.XtraEditors.TextEdit();
            this.LastAccountingEdit = new DevExpress.XtraEditors.TextEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.FundNameEdit = new DevExpress.XtraEditors.TextEdit();
            this.Fund = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlReceivingAccount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GridControlAccounts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OpeningDateEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LastAccountingEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FundNameEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // SkipAll
            // 
            this.SkipAll.Location = new System.Drawing.Point(481, 434);
            this.SkipAll.Name = "SkipAll";
            this.SkipAll.Size = new System.Drawing.Size(75, 23);
            this.SkipAll.TabIndex = 21;
            this.SkipAll.Text = "Skip All";
            this.SkipAll.Click += new System.EventHandler(this.SkipAll_Click);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(270, 434);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(75, 23);
            this.Skip.TabIndex = 20;
            this.Skip.Text = "Skip";
            this.Skip.Click += new System.EventHandler(this.Skip_Click);
            // 
            // Launch
            // 
            this.Launch.Location = new System.Drawing.Point(35, 434);
            this.Launch.Name = "Launch";
            this.Launch.Size = new System.Drawing.Size(142, 23);
            this.Launch.TabIndex = 19;
            this.Launch.Text = "Launch Accounting Opening";
            this.Launch.Click += new System.EventHandler(this.Launch_Click);
            // 
            // gridControlReceivingAccount
            // 
            this.gridControlReceivingAccount.Location = new System.Drawing.Point(9, 354);
            this.gridControlReceivingAccount.MainView = this.gridView2;
            this.gridControlReceivingAccount.Name = "gridControlReceivingAccount";
            this.gridControlReceivingAccount.Size = new System.Drawing.Size(658, 55);
            this.gridControlReceivingAccount.TabIndex = 18;
            this.gridControlReceivingAccount.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            // 
            // gridView2
            // 
            this.gridView2.GridControl = this.gridControlReceivingAccount;
            this.gridView2.Name = "gridView2";
            // 
            // GridControlAccounts
            // 
            this.GridControlAccounts.Location = new System.Drawing.Point(9, 75);
            this.GridControlAccounts.MainView = this.gridView1;
            this.GridControlAccounts.Name = "GridControlAccounts";
            this.GridControlAccounts.Size = new System.Drawing.Size(658, 263);
            this.GridControlAccounts.TabIndex = 17;
            this.GridControlAccounts.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.GridControlAccounts;
            this.gridView1.Name = "gridView1";
            // 
            // OpeningDateEdit
            // 
            this.OpeningDateEdit.Enabled = false;
            this.OpeningDateEdit.Location = new System.Drawing.Point(497, 37);
            this.OpeningDateEdit.Name = "OpeningDateEdit";
            this.OpeningDateEdit.Size = new System.Drawing.Size(122, 20);
            this.OpeningDateEdit.TabIndex = 16;
            // 
            // LastAccountingEdit
            // 
            this.LastAccountingEdit.Enabled = false;
            this.LastAccountingEdit.Location = new System.Drawing.Point(270, 37);
            this.LastAccountingEdit.Name = "LastAccountingEdit";
            this.LastAccountingEdit.Size = new System.Drawing.Size(140, 20);
            this.LastAccountingEdit.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(478, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Opening Date :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(241, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Last Accounting Date :";
            // 
            // FundNameEdit
            // 
            this.FundNameEdit.Enabled = false;
            this.FundNameEdit.Location = new System.Drawing.Point(35, 37);
            this.FundNameEdit.Name = "FundNameEdit";
            this.FundNameEdit.Size = new System.Drawing.Size(170, 20);
            this.FundNameEdit.TabIndex = 12;
            // 
            // Fund
            // 
            this.Fund.AutoSize = true;
            this.Fund.Location = new System.Drawing.Point(6, 12);
            this.Fund.Name = "Fund";
            this.Fund.Size = new System.Drawing.Size(68, 13);
            this.Fund.TabIndex = 11;
            this.Fund.Text = "Fund Name :";
            // 
            // InterfaceOuverture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 469);
            this.Controls.Add(this.SkipAll);
            this.Controls.Add(this.Skip);
            this.Controls.Add(this.Launch);
            this.Controls.Add(this.gridControlReceivingAccount);
            this.Controls.Add(this.GridControlAccounts);
            this.Controls.Add(this.OpeningDateEdit);
            this.Controls.Add(this.LastAccountingEdit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FundNameEdit);
            this.Controls.Add(this.Fund);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InterfaceOuverture";
            this.Text = "InterfaceOuverture";
            ((System.ComponentModel.ISupportInitialize)(this.gridControlReceivingAccount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GridControlAccounts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OpeningDateEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LastAccountingEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FundNameEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton SkipAll;
        private DevExpress.XtraEditors.SimpleButton Skip;
        private DevExpress.XtraEditors.SimpleButton Launch;
        private DevExpress.XtraGrid.GridControl gridControlReceivingAccount;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraGrid.GridControl GridControlAccounts;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.TextEdit OpeningDateEdit;
        private DevExpress.XtraEditors.TextEdit LastAccountingEdit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.TextEdit FundNameEdit;
        private System.Windows.Forms.Label Fund;
    }
}
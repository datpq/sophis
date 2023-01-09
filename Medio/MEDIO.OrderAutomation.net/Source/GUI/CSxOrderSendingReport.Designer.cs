namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    partial class CSxOrderSendingReport
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
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumnInstrument = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnOrderID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnAmount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnCurrency = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnInfo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxError = new System.Windows.Forms.TextBox();
            this.textBoxSent = new System.Windows.Forms.TextBox();
            this.textBoxTotal = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1013, 477);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumnInstrument,
            this.gridColumnOrderID,
            this.gridColumnAmount,
            this.gridColumnCurrency,
            this.gridColumnInfo});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // gridColumnInstrument
            // 
            this.gridColumnInstrument.Caption = "Instrument";
            this.gridColumnInstrument.FieldName = "Instrument";
            this.gridColumnInstrument.Name = "gridColumnInstrument";
            this.gridColumnInstrument.OptionsColumn.AllowEdit = false;
            this.gridColumnInstrument.Visible = true;
            this.gridColumnInstrument.VisibleIndex = 0;
            // 
            // gridColumnOrderID
            // 
            this.gridColumnOrderID.Caption = "Order ID";
            this.gridColumnOrderID.FieldName = "OrderID";
            this.gridColumnOrderID.Name = "gridColumnOrderID";
            this.gridColumnOrderID.OptionsColumn.AllowEdit = false;
            this.gridColumnOrderID.Visible = true;
            this.gridColumnOrderID.VisibleIndex = 1;
            // 
            // gridColumnAmount
            // 
            this.gridColumnAmount.Caption = "Amount";
            this.gridColumnAmount.DisplayFormat.FormatString = "{0:n2}";
            this.gridColumnAmount.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumnAmount.FieldName = "Amount";
            this.gridColumnAmount.Name = "gridColumnAmount";
            this.gridColumnAmount.OptionsColumn.AllowEdit = false;
            this.gridColumnAmount.Visible = true;
            this.gridColumnAmount.VisibleIndex = 2;
            // 
            // gridColumnCurrency
            // 
            this.gridColumnCurrency.Caption = "Currency";
            this.gridColumnCurrency.FieldName = "Currency";
            this.gridColumnCurrency.Name = "gridColumnCurrency";
            this.gridColumnCurrency.OptionsColumn.AllowEdit = false;
            this.gridColumnCurrency.Visible = true;
            this.gridColumnCurrency.VisibleIndex = 3;
            // 
            // gridColumnInfo
            // 
            this.gridColumnInfo.Caption = "Side";
            this.gridColumnInfo.FieldName = "Info";
            this.gridColumnInfo.Name = "gridColumnInfo";
            this.gridColumnInfo.OptionsColumn.AllowEdit = false;
            this.gridColumnInfo.Visible = true;
            this.gridColumnInfo.VisibleIndex = 4;
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Horizontal = false;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.Controls.Add(this.gridControl1);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(1013, 564);
            this.splitContainerControl1.SplitterPosition = 82;
            this.splitContainerControl1.TabIndex = 1;
            this.splitContainerControl1.Text = "splitContainerControl1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxError);
            this.groupBox1.Controls.Add(this.textBoxSent);
            this.groupBox1.Controls.Add(this.textBoxTotal);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(997, 79);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Orders Nb";
            // 
            // textBoxError
            // 
            this.textBoxError.BackColor = System.Drawing.SystemColors.Info;
            this.textBoxError.Location = new System.Drawing.Point(596, 29);
            this.textBoxError.Name = "textBoxError";
            this.textBoxError.ReadOnly = true;
            this.textBoxError.Size = new System.Drawing.Size(111, 20);
            this.textBoxError.TabIndex = 4;
            // 
            // textBoxSent
            // 
            this.textBoxSent.BackColor = System.Drawing.SystemColors.Info;
            this.textBoxSent.Location = new System.Drawing.Point(333, 29);
            this.textBoxSent.Name = "textBoxSent";
            this.textBoxSent.ReadOnly = true;
            this.textBoxSent.Size = new System.Drawing.Size(111, 20);
            this.textBoxSent.TabIndex = 3;
            // 
            // textBoxTotal
            // 
            this.textBoxTotal.BackColor = System.Drawing.SystemColors.Info;
            this.textBoxTotal.Location = new System.Drawing.Point(88, 29);
            this.textBoxTotal.Name = "textBoxTotal";
            this.textBoxTotal.ReadOnly = true;
            this.textBoxTotal.Size = new System.Drawing.Size(111, 20);
            this.textBoxTotal.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(502, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Errors : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(259, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Sent : ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Total :";
            // 
            // CSxOrderSendingReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerControl1);
            this.Name = "CSxOrderSendingReport";
            this.Size = new System.Drawing.Size(1013, 564);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnInstrument;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnOrderID;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnAmount;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnCurrency;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnInfo;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxError;
        private System.Windows.Forms.TextBox textBoxSent;
        private System.Windows.Forms.TextBox textBoxTotal;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}
namespace CFG_AccountClosing.SourceFiles
{
    partial class AffectationManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AffectationManager));
            this.Cancelbutton = new DevExpress.XtraEditors.SimpleButton();
            this.proceedButton = new DevExpress.XtraEditors.SimpleButton();
            this.FundsGridControl = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.FundsGridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // Cancelbutton
            // 
            this.Cancelbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Cancelbutton.Location = new System.Drawing.Point(305, 360);
            this.Cancelbutton.Name = "Cancelbutton";
            this.Cancelbutton.Size = new System.Drawing.Size(75, 23);
            this.Cancelbutton.TabIndex = 8;
            this.Cancelbutton.Text = "Cancel";
            this.Cancelbutton.Click += new System.EventHandler(this.Cancelbutton_Click);
            // 
            // proceedButton
            // 
            this.proceedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.proceedButton.Location = new System.Drawing.Point(12, 360);
            this.proceedButton.Name = "proceedButton";
            this.proceedButton.Size = new System.Drawing.Size(237, 23);
            this.proceedButton.TabIndex = 7;
            this.proceedButton.Text = "Proceed to Result Distribution of selected funds";
            this.proceedButton.Click += new System.EventHandler(this.proceedButton_Click);
            // 
            // FundsGridControl
            // 
            this.FundsGridControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FundsGridControl.Location = new System.Drawing.Point(12, 12);
            this.FundsGridControl.MainView = this.gridView1;
            this.FundsGridControl.Name = "FundsGridControl";
            this.FundsGridControl.Size = new System.Drawing.Size(444, 320);
            this.FundsGridControl.TabIndex = 6;
            this.FundsGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.FundsGridControl;
            this.gridView1.Name = "gridView1";
            // 
            // AffectationManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 398);
            this.Controls.Add(this.Cancelbutton);
            this.Controls.Add(this.proceedButton);
            this.Controls.Add(this.FundsGridControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AffectationManager";
            this.Text = "Result Distribution of Funds";
            ((System.ComponentModel.ISupportInitialize)(this.FundsGridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton Cancelbutton;
        private DevExpress.XtraEditors.SimpleButton proceedButton;
        private DevExpress.XtraGrid.GridControl FundsGridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
    }
}
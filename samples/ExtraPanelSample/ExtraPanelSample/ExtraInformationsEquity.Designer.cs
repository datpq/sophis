namespace ExtraPanelSample
{
    partial class ExtraInformationsEquity
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
            this.TxtFolio = new DevExpress.XtraEditors.TextEdit();
            this.LCFolio = new DevExpress.XtraEditors.LabelControl();
            this.LCQuantity = new DevExpress.XtraEditors.LabelControl();
            this.TxtQuantity = new DevExpress.XtraEditors.TextEdit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtFolio.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtQuantity.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // TxtFolio
            // 
            this.TxtFolio.Location = new System.Drawing.Point(64, 23);
            this.TxtFolio.Name = "TxtFolio";
            this.TxtFolio.Properties.ReadOnly = true;
            this.TxtFolio.Size = new System.Drawing.Size(100, 20);
            this.TxtFolio.TabIndex = 0;
            // 
            // LCFolio
            // 
            this.LCFolio.Location = new System.Drawing.Point(4, 26);
            this.LCFolio.Name = "LCFolio";
            this.LCFolio.Size = new System.Drawing.Size(54, 13);
            this.LCFolio.TabIndex = 1;
            this.LCFolio.Text = "Portfolio ID";
            // 
            // LCQuantity
            // 
            this.LCQuantity.Location = new System.Drawing.Point(4, 61);
            this.LCQuantity.Name = "LCQuantity";
            this.LCQuantity.Size = new System.Drawing.Size(42, 13);
            this.LCQuantity.TabIndex = 3;
            this.LCQuantity.Text = "Quantity";
            // 
            // TxtQuantity
            // 
            this.TxtQuantity.Location = new System.Drawing.Point(64, 58);
            this.TxtQuantity.Name = "TxtQuantity";
            this.TxtQuantity.Properties.ReadOnly = true;
            this.TxtQuantity.Size = new System.Drawing.Size(100, 20);
            this.TxtQuantity.TabIndex = 2;
            // 
            // ExtraInformationsEquity
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LCQuantity);
            this.Controls.Add(this.TxtQuantity);
            this.Controls.Add(this.LCFolio);
            this.Controls.Add(this.TxtFolio);
            this.Name = "ExtraInformationsEquity";
            this.Size = new System.Drawing.Size(193, 363);
            ((System.ComponentModel.ISupportInitialize)(this.TxtFolio.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtQuantity.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.TextEdit TxtFolio;
        private DevExpress.XtraEditors.LabelControl LCFolio;
        private DevExpress.XtraEditors.LabelControl LCQuantity;
        private DevExpress.XtraEditors.TextEdit TxtQuantity;
    }
}

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    partial class SelectFXInstrument
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectFXInstrument));
            this.comboBox_ccy1 = new System.Windows.Forms.ComboBox();
            this.comboBox_ccy2 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.valueDateEdit = new DevExpress.XtraEditors.DateEdit();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox_forward = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.valueDateEdit.Properties.VistaTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueDateEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox_ccy1
            // 
            this.comboBox_ccy1.FormattingEnabled = true;
            this.comboBox_ccy1.Location = new System.Drawing.Point(92, 12);
            this.comboBox_ccy1.Name = "comboBox_ccy1";
            this.comboBox_ccy1.Size = new System.Drawing.Size(92, 24);
            this.comboBox_ccy1.TabIndex = 0;
            this.comboBox_ccy1.SelectedIndexChanged += new System.EventHandler(this.comboBox_ccy1_SelectedIndexChanged);
            this.comboBox_ccy1.Leave += new System.EventHandler(this.comboBox_ccy1_Leave);
            // 
            // comboBox_ccy2
            // 
            this.comboBox_ccy2.FormattingEnabled = true;
            this.comboBox_ccy2.Location = new System.Drawing.Point(92, 48);
            this.comboBox_ccy2.Name = "comboBox_ccy2";
            this.comboBox_ccy2.Size = new System.Drawing.Size(92, 24);
            this.comboBox_ccy2.TabIndex = 1;
            this.comboBox_ccy2.SelectedIndexChanged += new System.EventHandler(this.comboBox_ccy2_SelectedIndexChanged);
            this.comboBox_ccy2.Leave += new System.EventHandler(this.comboBox_ccy2_Leave);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(28, 199);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(121, 27);
            this.button1.TabIndex = 3;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // valueDateEdit
            // 
            this.valueDateEdit.EditValue = null;
            this.valueDateEdit.Enabled = false;
            this.valueDateEdit.Location = new System.Drawing.Point(92, 117);
            this.valueDateEdit.Name = "valueDateEdit";
            this.valueDateEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.valueDateEdit.Properties.VistaTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.valueDateEdit.Size = new System.Drawing.Size(160, 22);
            this.valueDateEdit.TabIndex = 9;
            this.valueDateEdit.EditValueChanged += new System.EventHandler(this.valueDateEdit_EditValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Receiving";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Paying";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 17);
            this.label3.TabIndex = 12;
            this.label3.Text = "Value Date";
            // 
            // checkBox_forward
            // 
            this.checkBox_forward.AutoSize = true;
            this.checkBox_forward.Location = new System.Drawing.Point(12, 90);
            this.checkBox_forward.Name = "checkBox_forward";
            this.checkBox_forward.Size = new System.Drawing.Size(77, 21);
            this.checkBox_forward.TabIndex = 13;
            this.checkBox_forward.Text = "forward";
            this.checkBox_forward.UseVisualStyleBackColor = true;
            this.checkBox_forward.CheckedChanged += new System.EventHandler(this.checkBox_forward_CheckedChanged);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(178, 199);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(121, 27);
            this.button2.TabIndex = 14;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // SelectFXInstrument
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 238);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.checkBox_forward);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.valueDateEdit);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox_ccy2);
            this.Controls.Add(this.comboBox_ccy1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SelectFXInstrument";
            this.Text = "Forex";
            ((System.ComponentModel.ISupportInitialize)(this.valueDateEdit.Properties.VistaTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueDateEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ComboBox comboBox_ccy1;
        public System.Windows.Forms.ComboBox comboBox_ccy2;
        private System.Windows.Forms.Button button1;
        public DevExpress.XtraEditors.DateEdit valueDateEdit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.CheckBox checkBox_forward;
        private System.Windows.Forms.Button button2;
    }
}
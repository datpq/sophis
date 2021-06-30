namespace ToolkitReporting.NET
{
    partial class ParamColMapForm
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
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.gbMapping = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lvwSources = new System.Windows.Forms.ListView();
            this.clnSourceName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnSourceType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmdAddManual = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtColumnName = new System.Windows.Forms.TextBox();
            this.cmdRemoveAll = new System.Windows.Forms.Button();
            this.cmdRemove = new System.Windows.Forms.Button();
            this.cmdAdd = new System.Windows.Forms.Button();
            this.lstMap = new System.Windows.Forms.ListBox();
            this.lstColumns = new System.Windows.Forms.ListBox();
            this.lstParams = new System.Windows.Forms.ListBox();
            this.txtExcelTemplateFile = new System.Windows.Forms.TextBox();
            this.lblExcelTemplateFile = new System.Windows.Forms.Label();
            this.cmdUp = new System.Windows.Forms.Button();
            this.cmdDown = new System.Windows.Forms.Button();
            this.gbMapping.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOk
            // 
            this.cmdOk.Location = new System.Drawing.Point(617, 519);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(101, 31);
            this.cmdOk.TabIndex = 3;
            this.cmdOk.Text = "OK";
            this.cmdOk.UseVisualStyleBackColor = true;
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(736, 519);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(101, 31);
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // gbMapping
            // 
            this.gbMapping.Controls.Add(this.cmdDown);
            this.gbMapping.Controls.Add(this.cmdUp);
            this.gbMapping.Controls.Add(this.label5);
            this.gbMapping.Controls.Add(this.lvwSources);
            this.gbMapping.Controls.Add(this.label4);
            this.gbMapping.Controls.Add(this.label3);
            this.gbMapping.Controls.Add(this.label2);
            this.gbMapping.Controls.Add(this.cmdAddManual);
            this.gbMapping.Controls.Add(this.label1);
            this.gbMapping.Controls.Add(this.txtColumnName);
            this.gbMapping.Controls.Add(this.cmdRemoveAll);
            this.gbMapping.Controls.Add(this.cmdRemove);
            this.gbMapping.Controls.Add(this.cmdAdd);
            this.gbMapping.Controls.Add(this.lstMap);
            this.gbMapping.Controls.Add(this.lstColumns);
            this.gbMapping.Controls.Add(this.lstParams);
            this.gbMapping.Location = new System.Drawing.Point(12, 50);
            this.gbMapping.Name = "gbMapping";
            this.gbMapping.Size = new System.Drawing.Size(1386, 463);
            this.gbMapping.TabIndex = 2;
            this.gbMapping.TabStop = false;
            this.gbMapping.Text = "Mapping";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(975, 27);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(119, 15);
            this.label5.TabIndex = 12;
            this.label5.Text = "Mapping per Source";
            // 
            // lvwSources
            // 
            this.lvwSources.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnSourceName,
            this.clnSourceType});
            this.lvwSources.FullRowSelect = true;
            this.lvwSources.HideSelection = false;
            this.lvwSources.Location = new System.Drawing.Point(257, 48);
            this.lvwSources.MultiSelect = false;
            this.lvwSources.Name = "lvwSources";
            this.lvwSources.Size = new System.Drawing.Size(340, 407);
            this.lvwSources.TabIndex = 3;
            this.lvwSources.UseCompatibleStateImageBehavior = false;
            this.lvwSources.View = System.Windows.Forms.View.Details;
            this.lvwSources.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvwSources_ItemSelectionChanged);
            // 
            // clnSourceName
            // 
            this.clnSourceName.Text = "Name";
            this.clnSourceName.Width = 225;
            // 
            // clnSourceType
            // 
            this.clnSourceType.Text = "Type";
            this.clnSourceType.Width = 93;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(604, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Columns";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(254, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Sources";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Tags to map";
            // 
            // cmdAddManual
            // 
            this.cmdAddManual.Location = new System.Drawing.Point(863, 423);
            this.cmdAddManual.Name = "cmdAddManual";
            this.cmdAddManual.Size = new System.Drawing.Size(101, 31);
            this.cmdAddManual.TabIndex = 11;
            this.cmdAddManual.Text = ">>";
            this.cmdAddManual.UseVisualStyleBackColor = true;
            this.cmdAddManual.Click += new System.EventHandler(this.cmdAddManual_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(604, 413);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "Column name:";
            // 
            // txtColumnName
            // 
            this.txtColumnName.Location = new System.Drawing.Point(607, 435);
            this.txtColumnName.Name = "txtColumnName";
            this.txtColumnName.Size = new System.Drawing.Size(241, 20);
            this.txtColumnName.TabIndex = 10;
            this.txtColumnName.Enter += new System.EventHandler(this.txtColumnName_Enter);
            this.txtColumnName.MouseCaptureChanged += new System.EventHandler(this.txtColumnName_MouseCaptureChanged);
            // 
            // cmdRemoveAll
            // 
            this.cmdRemoveAll.Location = new System.Drawing.Point(863, 249);
            this.cmdRemoveAll.Name = "cmdRemoveAll";
            this.cmdRemoveAll.Size = new System.Drawing.Size(101, 31);
            this.cmdRemoveAll.TabIndex = 8;
            this.cmdRemoveAll.Text = "<<<";
            this.cmdRemoveAll.UseVisualStyleBackColor = true;
            this.cmdRemoveAll.Click += new System.EventHandler(this.cmdRemoveAll_Click);
            // 
            // cmdRemove
            // 
            this.cmdRemove.Location = new System.Drawing.Point(863, 212);
            this.cmdRemove.Name = "cmdRemove";
            this.cmdRemove.Size = new System.Drawing.Size(101, 31);
            this.cmdRemove.TabIndex = 7;
            this.cmdRemove.Text = "<<";
            this.cmdRemove.UseVisualStyleBackColor = true;
            this.cmdRemove.Click += new System.EventHandler(this.cmdRemove_Click);
            // 
            // cmdAdd
            // 
            this.cmdAdd.Location = new System.Drawing.Point(863, 175);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(101, 31);
            this.cmdAdd.TabIndex = 6;
            this.cmdAdd.Text = ">>";
            this.cmdAdd.UseVisualStyleBackColor = true;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // lstMap
            // 
            this.lstMap.FormattingEnabled = true;
            this.lstMap.Location = new System.Drawing.Point(978, 48);
            this.lstMap.Name = "lstMap";
            this.lstMap.Size = new System.Drawing.Size(336, 407);
            this.lstMap.TabIndex = 13;
            // 
            // lstColumns
            // 
            this.lstColumns.FormattingEnabled = true;
            this.lstColumns.Location = new System.Drawing.Point(607, 48);
            this.lstColumns.Name = "lstColumns";
            this.lstColumns.Size = new System.Drawing.Size(241, 342);
            this.lstColumns.TabIndex = 5;
            // 
            // lstParams
            // 
            this.lstParams.FormattingEnabled = true;
            this.lstParams.Location = new System.Drawing.Point(10, 48);
            this.lstParams.Name = "lstParams";
            this.lstParams.Size = new System.Drawing.Size(241, 407);
            this.lstParams.TabIndex = 1;
            // 
            // txtExcelTemplateFile
            // 
            this.txtExcelTemplateFile.Location = new System.Drawing.Point(126, 12);
            this.txtExcelTemplateFile.Name = "txtExcelTemplateFile";
            this.txtExcelTemplateFile.ReadOnly = true;
            this.txtExcelTemplateFile.Size = new System.Drawing.Size(764, 20);
            this.txtExcelTemplateFile.TabIndex = 1;
            // 
            // lblExcelTemplateFile
            // 
            this.lblExcelTemplateFile.AutoSize = true;
            this.lblExcelTemplateFile.Location = new System.Drawing.Point(13, 15);
            this.lblExcelTemplateFile.Name = "lblExcelTemplateFile";
            this.lblExcelTemplateFile.Size = new System.Drawing.Size(107, 15);
            this.lblExcelTemplateFile.TabIndex = 0;
            this.lblExcelTemplateFile.Text = "Excel template file";
            // 
            // cmdUp
            // 
            this.cmdUp.Location = new System.Drawing.Point(1320, 175);
            this.cmdUp.Name = "cmdUp";
            this.cmdUp.Size = new System.Drawing.Size(60, 31);
            this.cmdUp.TabIndex = 14;
            this.cmdUp.Text = "Up";
            this.cmdUp.UseVisualStyleBackColor = true;
            // 
            // cmdDown
            // 
            this.cmdDown.Location = new System.Drawing.Point(1320, 212);
            this.cmdDown.Name = "cmdDown";
            this.cmdDown.Size = new System.Drawing.Size(60, 31);
            this.cmdDown.TabIndex = 15;
            this.cmdDown.Text = "Down";
            this.cmdDown.UseVisualStyleBackColor = true;
            // 
            // ParamColMapForm
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(1410, 557);
            this.Controls.Add(this.txtExcelTemplateFile);
            this.Controls.Add(this.lblExcelTemplateFile);
            this.Controls.Add(this.gbMapping);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ParamColMapForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Template tags and source columns mapping";
            this.gbMapping.ResumeLayout(false);
            this.gbMapping.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.GroupBox gbMapping;
        private System.Windows.Forms.ListBox lstMap;
        private System.Windows.Forms.ListBox lstColumns;
        private System.Windows.Forms.ListBox lstParams;
        private System.Windows.Forms.TextBox txtExcelTemplateFile;
        private System.Windows.Forms.Label lblExcelTemplateFile;
        private System.Windows.Forms.Button cmdRemove;
        private System.Windows.Forms.Button cmdAdd;
        private System.Windows.Forms.Button cmdRemoveAll;
        private System.Windows.Forms.Button cmdAddManual;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtColumnName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView lvwSources;
        private System.Windows.Forms.ColumnHeader clnSourceName;
        private System.Windows.Forms.ColumnHeader clnSourceType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button cmdDown;
        private System.Windows.Forms.Button cmdUp;
    }
}
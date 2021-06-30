namespace Eff
{
    namespace ToolkitReporting.NET
    {
        partial class ReportingMainForm
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
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabGeneration = new System.Windows.Forms.TabPage();
            this.pnlGeneration = new System.Windows.Forms.Panel();
            this.cboReportsGen = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pgReportGen = new System.Windows.Forms.PropertyGrid();
            this.tabConfig = new System.Windows.Forms.TabPage();
            this.pnlConfiguration = new System.Windows.Forms.Panel();
            this.cmdBrowseOutputFile = new System.Windows.Forms.Button();
            this.cmdBrowseInputTemplate = new System.Windows.Forms.Button();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.lblOutputFile = new System.Windows.Forms.Label();
            this.txtExcelTemplate = new System.Windows.Forms.TextBox();
            this.lblExcelTemplate = new System.Windows.Forms.Label();
            this.cboReportsCfg = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pgReportConfig = new System.Windows.Forms.PropertyGrid();
            this.pnlCommands = new System.Windows.Forms.Panel();
            this.cmdColumnsMapping = new System.Windows.Forms.Button();
            this.cmdGenerateXslt = new System.Windows.Forms.Button();
            this.cmdApply = new System.Windows.Forms.Button();
            this.cmdPreview = new System.Windows.Forms.Button();
            this.cmdGenerate = new System.Windows.Forms.Button();
            this.cmdClose = new System.Windows.Forms.Button();
            this.tabMain.SuspendLayout();
            this.tabGeneration.SuspendLayout();
            this.pnlGeneration.SuspendLayout();
            this.tabConfig.SuspendLayout();
            this.pnlConfiguration.SuspendLayout();
            this.pnlCommands.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabGeneration);
            this.tabMain.Controls.Add(this.tabConfig);
            this.tabMain.Location = new System.Drawing.Point(12, 12);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(787, 465);
            this.tabMain.TabIndex = 0;
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
            this.tabMain.SizeChanged += new System.EventHandler(this.tabMain_SizeChanged);
            // 
            // tabGeneration
            // 
            this.tabGeneration.Controls.Add(this.pnlGeneration);
            this.tabGeneration.Controls.Add(this.pgReportGen);
            this.tabGeneration.Location = new System.Drawing.Point(4, 22);
            this.tabGeneration.Name = "tabGeneration";
            this.tabGeneration.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneration.Size = new System.Drawing.Size(779, 439);
            this.tabGeneration.TabIndex = 0;
            this.tabGeneration.Text = "Generation";
            this.tabGeneration.UseVisualStyleBackColor = true;
            // 
            // pnlGeneration
            // 
            this.pnlGeneration.Controls.Add(this.cboReportsGen);
            this.pnlGeneration.Controls.Add(this.label2);
            this.pnlGeneration.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGeneration.Location = new System.Drawing.Point(3, 3);
            this.pnlGeneration.Name = "pnlGeneration";
            this.pnlGeneration.Size = new System.Drawing.Size(773, 45);
            this.pnlGeneration.TabIndex = 3;
            // 
            // cboReportsGen
            // 
            this.cboReportsGen.FormattingEnabled = true;
            this.cboReportsGen.Location = new System.Drawing.Point(126, 6);
            this.cboReportsGen.Name = "cboReportsGen";
            this.cboReportsGen.Size = new System.Drawing.Size(644, 21);
            this.cboReportsGen.TabIndex = 2;
            this.cboReportsGen.SelectedIndexChanged += new System.EventHandler(this.cboReportsGen_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 14);
            this.label2.TabIndex = 1;
            this.label2.Text = "Select a report :";
            // 
            // pgReportGen
            // 
            this.pgReportGen.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pgReportGen.Location = new System.Drawing.Point(3, 108);
            this.pgReportGen.Name = "pgReportGen";
            this.pgReportGen.Size = new System.Drawing.Size(773, 328);
            this.pgReportGen.TabIndex = 2;
            // 
            // tabConfig
            // 
            this.tabConfig.Controls.Add(this.pnlConfiguration);
            this.tabConfig.Controls.Add(this.pgReportConfig);
            this.tabConfig.Location = new System.Drawing.Point(4, 22);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tabConfig.Size = new System.Drawing.Size(779, 439);
            this.tabConfig.TabIndex = 1;
            this.tabConfig.Text = "Configuration";
            this.tabConfig.UseVisualStyleBackColor = true;
            // 
            // pnlConfiguration
            // 
            this.pnlConfiguration.Controls.Add(this.cmdBrowseOutputFile);
            this.pnlConfiguration.Controls.Add(this.cmdBrowseInputTemplate);
            this.pnlConfiguration.Controls.Add(this.txtOutputFile);
            this.pnlConfiguration.Controls.Add(this.lblOutputFile);
            this.pnlConfiguration.Controls.Add(this.txtExcelTemplate);
            this.pnlConfiguration.Controls.Add(this.lblExcelTemplate);
            this.pnlConfiguration.Controls.Add(this.cboReportsCfg);
            this.pnlConfiguration.Controls.Add(this.label1);
            this.pnlConfiguration.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlConfiguration.Location = new System.Drawing.Point(3, 3);
            this.pnlConfiguration.Name = "pnlConfiguration";
            this.pnlConfiguration.Size = new System.Drawing.Size(773, 99);
            this.pnlConfiguration.TabIndex = 6;
            // 
            // cmdBrowseOutputFile
            // 
            this.cmdBrowseOutputFile.Location = new System.Drawing.Point(738, 60);
            this.cmdBrowseOutputFile.Name = "cmdBrowseOutputFile";
            this.cmdBrowseOutputFile.Size = new System.Drawing.Size(32, 23);
            this.cmdBrowseOutputFile.TabIndex = 0;
            this.cmdBrowseOutputFile.Text = "...";
            this.cmdBrowseOutputFile.UseVisualStyleBackColor = true;
            this.cmdBrowseOutputFile.Click += new System.EventHandler(this.cmdBrowseOutputFile_Click);
            // 
            // cmdBrowseInputTemplate
            // 
            this.cmdBrowseInputTemplate.Location = new System.Drawing.Point(738, 33);
            this.cmdBrowseInputTemplate.Name = "cmdBrowseInputTemplate";
            this.cmdBrowseInputTemplate.Size = new System.Drawing.Size(32, 23);
            this.cmdBrowseInputTemplate.TabIndex = 4;
            this.cmdBrowseInputTemplate.Text = "...";
            this.cmdBrowseInputTemplate.UseVisualStyleBackColor = true;
            this.cmdBrowseInputTemplate.Click += new System.EventHandler(this.cmdBrowseInputTemplate_Click);
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Location = new System.Drawing.Point(126, 60);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.Size = new System.Drawing.Size(606, 21);
            this.txtOutputFile.TabIndex = 7;
            this.txtOutputFile.TextChanged += new System.EventHandler(this.txtOutputFile_TextChanged);
            // 
            // lblOutputFile
            // 
            this.lblOutputFile.AutoSize = true;
            this.lblOutputFile.Location = new System.Drawing.Point(3, 63);
            this.lblOutputFile.Name = "lblOutputFile";
            this.lblOutputFile.Size = new System.Drawing.Size(0, 14);
            this.lblOutputFile.TabIndex = 5;
            // 
            // txtExcelTemplate
            // 
            this.txtExcelTemplate.Location = new System.Drawing.Point(126, 33);
            this.txtExcelTemplate.Name = "txtExcelTemplate";
            this.txtExcelTemplate.Size = new System.Drawing.Size(606, 21);
            this.txtExcelTemplate.TabIndex = 3;
            this.txtExcelTemplate.TextChanged += new System.EventHandler(this.txtExcelTemplate_TextChanged);
            // 
            // lblExcelTemplate
            // 
            this.lblExcelTemplate.AutoSize = true;
            this.lblExcelTemplate.Location = new System.Drawing.Point(3, 36);
            this.lblExcelTemplate.Name = "lblExcelTemplate";
            this.lblExcelTemplate.Size = new System.Drawing.Size(0, 14);
            this.lblExcelTemplate.TabIndex = 2;
            // 
            // cboReportsCfg
            // 
            this.cboReportsCfg.FormattingEnabled = true;
            this.cboReportsCfg.Location = new System.Drawing.Point(126, 6);
            this.cboReportsCfg.Name = "cboReportsCfg";
            this.cboReportsCfg.Size = new System.Drawing.Size(644, 21);
            this.cboReportsCfg.TabIndex = 1;
            this.cboReportsCfg.SelectedIndexChanged += new System.EventHandler(this.cboReportsCfg_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select a report :";
            // 
            // pgReportConfig
            // 
            this.pgReportConfig.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pgReportConfig.Location = new System.Drawing.Point(3, 153);
            this.pgReportConfig.Name = "pgReportConfig";
            this.pgReportConfig.Size = new System.Drawing.Size(773, 283);
            this.pgReportConfig.TabIndex = 1;
            // 
            // pnlCommands
            // 
            this.pnlCommands.Controls.Add(this.cmdColumnsMapping);
            this.pnlCommands.Controls.Add(this.cmdGenerateXslt);
            this.pnlCommands.Controls.Add(this.cmdApply);
            this.pnlCommands.Controls.Add(this.cmdPreview);
            this.pnlCommands.Controls.Add(this.cmdGenerate);
            this.pnlCommands.Controls.Add(this.cmdClose);
            this.pnlCommands.Location = new System.Drawing.Point(819, 34);
            this.pnlCommands.Name = "pnlCommands";
            this.pnlCommands.Size = new System.Drawing.Size(120, 443);
            this.pnlCommands.TabIndex = 5;
            // 
            // cmdColumnsMapping
            // 
            this.cmdColumnsMapping.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdColumnsMapping.Location = new System.Drawing.Point(0, 108);
            this.cmdColumnsMapping.Name = "cmdColumnsMapping";
            this.cmdColumnsMapping.Size = new System.Drawing.Size(110, 30);
            this.cmdColumnsMapping.TabIndex = 3;
            this.cmdColumnsMapping.Text = "Columns Mapping";
            this.cmdColumnsMapping.UseVisualStyleBackColor = true;
            this.cmdColumnsMapping.Click += new System.EventHandler(this.cmdColumnsMapping_Click);
            // 
            // cmdGenerateXslt
            // 
            this.cmdGenerateXslt.Location = new System.Drawing.Point(0, 0);
            this.cmdGenerateXslt.Name = "cmdGenerateXslt";
            this.cmdGenerateXslt.Size = new System.Drawing.Size(110, 30);
            this.cmdGenerateXslt.TabIndex = 0;
            this.cmdGenerateXslt.Text = "Generate Xslt";
            this.cmdGenerateXslt.UseVisualStyleBackColor = true;
            this.cmdGenerateXslt.Click += new System.EventHandler(this.cmdGenerateXslt_Click);
            // 
            // cmdApply
            // 
            this.cmdApply.Location = new System.Drawing.Point(0, 144);
            this.cmdApply.Name = "cmdApply";
            this.cmdApply.Size = new System.Drawing.Size(110, 30);
            this.cmdApply.TabIndex = 4;
            this.cmdApply.Text = "Apply";
            this.cmdApply.UseVisualStyleBackColor = true;
            this.cmdApply.Click += new System.EventHandler(this.cmdApply_Click);
            // 
            // cmdPreview
            // 
            this.cmdPreview.Location = new System.Drawing.Point(0, 36);
            this.cmdPreview.Name = "cmdPreview";
            this.cmdPreview.Size = new System.Drawing.Size(110, 30);
            this.cmdPreview.TabIndex = 1;
            this.cmdPreview.Text = "Preview";
            this.cmdPreview.UseVisualStyleBackColor = true;
            this.cmdPreview.Click += new System.EventHandler(this.cmdPreview_Click);
            // 
            // cmdGenerate
            // 
            this.cmdGenerate.Location = new System.Drawing.Point(0, 72);
            this.cmdGenerate.Name = "cmdGenerate";
            this.cmdGenerate.Size = new System.Drawing.Size(110, 30);
            this.cmdGenerate.TabIndex = 2;
            this.cmdGenerate.Text = "Generate";
            this.cmdGenerate.UseVisualStyleBackColor = true;
            this.cmdGenerate.Click += new System.EventHandler(this.cmdGenerate_Click);
            // 
            // cmdClose
            // 
            this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdClose.Location = new System.Drawing.Point(0, 180);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(110, 30);
            this.cmdClose.TabIndex = 5;
            this.cmdClose.Text = "Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // ReportingMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlCommands);
            this.Controls.Add(this.tabMain);
            this.Name = "ReportingMainForm";
            this.Size = new System.Drawing.Size(942, 488);
            this.Load += new System.EventHandler(this.ReportingMainForm_Load);
            this.SizeChanged += new System.EventHandler(this.ReportingMainForm_SizeChanged);
            this.tabMain.ResumeLayout(false);
            this.tabGeneration.ResumeLayout(false);
            this.pnlGeneration.ResumeLayout(false);
            this.pnlGeneration.PerformLayout();
            this.tabConfig.ResumeLayout(false);
            this.pnlConfiguration.ResumeLayout(false);
            this.pnlConfiguration.PerformLayout();
            this.pnlCommands.ResumeLayout(false);
            this.ResumeLayout(false);

            }

            #endregion

            private System.Windows.Forms.TabControl tabMain;
            private System.Windows.Forms.TabPage tabGeneration;
            private System.Windows.Forms.TabPage tabConfig;
            private System.Windows.Forms.PropertyGrid pgReportConfig;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.ComboBox cboReportsCfg;
            private System.Windows.Forms.PropertyGrid pgReportGen;
            private System.Windows.Forms.Label label2;
            private System.Windows.Forms.ComboBox cboReportsGen;
            private System.Windows.Forms.Panel pnlCommands;
            private System.Windows.Forms.Button cmdApply;
            private System.Windows.Forms.Button cmdPreview;
            private System.Windows.Forms.Button cmdGenerate;
            private System.Windows.Forms.Button cmdClose;
            private System.Windows.Forms.Panel pnlGeneration;
            private System.Windows.Forms.Panel pnlConfiguration;
            private System.Windows.Forms.Button cmdGenerateXslt;
            private System.Windows.Forms.Button cmdBrowseOutputFile;
            private System.Windows.Forms.Button cmdBrowseInputTemplate;
            private System.Windows.Forms.TextBox txtOutputFile;
            private System.Windows.Forms.Label lblOutputFile;
            private System.Windows.Forms.TextBox txtExcelTemplate;
            private System.Windows.Forms.Label lblExcelTemplate;
            private System.Windows.Forms.Button cmdColumnsMapping;
        }
    }
}
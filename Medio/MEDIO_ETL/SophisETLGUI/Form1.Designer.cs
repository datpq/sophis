using System;
using System.ComponentModel;

namespace SophisETLGUI
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxSophisETL = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxInputFilter = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBoxInput = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.richTextBoxGeneralMessage = new System.Windows.Forms.RichTextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button5 = new System.Windows.Forms.Button();
            this.textBoxTemplate = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonShowOptions = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBoxComp = new System.Windows.Forms.CheckBox();
            this.checkBoxCompPrice = new System.Windows.Forms.CheckBox();
            this.checkBoxBenchPrice = new System.Windows.Forms.CheckBox();
            this.checkBoxOverwritePrice = new System.Windows.Forms.CheckBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.buttonBenchmarkOutput = new System.Windows.Forms.Button();
            this.textBoxBenchmarkOutput = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.richTextBoxBenchmarkMessage = new System.Windows.Forms.RichTextBox();
            this.buttonBenchmarkRun = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxCompFilter = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxBenchmarkFilter = new System.Windows.Forms.TextBox();
            this.buttonBenchmarkInput = new System.Windows.Forms.Button();
            this.textBoxBenchmarkInput = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.textBoxSophisETL);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.SlateGray;
            this.groupBox1.Location = new System.Drawing.Point(55, 21);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(589, 72);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Step 1 : Select a sophis_etl.xml file";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Transparent;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.DimGray;
            this.button1.Location = new System.Drawing.Point(491, 30);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(65, 21);
            this.button1.TabIndex = 1;
            this.button1.Text = "Select...";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBoxSophisETL
            // 
            this.textBoxSophisETL.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSophisETL.Location = new System.Drawing.Point(19, 32);
            this.textBoxSophisETL.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSophisETL.Name = "textBoxSophisETL";
            this.textBoxSophisETL.Size = new System.Drawing.Size(437, 20);
            this.textBoxSophisETL.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBoxInputFilter);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.textBoxInput);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.SlateGray;
            this.groupBox2.Location = new System.Drawing.Point(55, 208);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(589, 94);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Step 3 : Specify the input file folder";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 65);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filters";
            // 
            // textBoxInputFilter
            // 
            this.textBoxInputFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxInputFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxInputFilter.Location = new System.Drawing.Point(59, 65);
            this.textBoxInputFilter.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxInputFilter.Name = "textBoxInputFilter";
            this.textBoxInputFilter.Size = new System.Drawing.Size(396, 20);
            this.textBoxInputFilter.TabIndex = 2;
            this.textBoxInputFilter.TextChanged += new System.EventHandler(this.textBoxInputFilter_TextChanged);
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.DimGray;
            this.button2.Location = new System.Drawing.Point(491, 23);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(65, 21);
            this.button2.TabIndex = 1;
            this.button2.Text = "Select...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBoxInput
            // 
            this.textBoxInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxInput.Location = new System.Drawing.Point(19, 25);
            this.textBoxInput.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Size = new System.Drawing.Size(437, 20);
            this.textBoxInput.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.richTextBoxGeneralMessage);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.ForeColor = System.Drawing.Color.MediumSeaGreen;
            this.groupBox3.Location = new System.Drawing.Point(55, 432);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(589, 96);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Step 5 : Run";
            // 
            // richTextBoxGeneralMessage
            // 
            this.richTextBoxGeneralMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxGeneralMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxGeneralMessage.ForeColor = System.Drawing.Color.White;
            this.richTextBoxGeneralMessage.Location = new System.Drawing.Point(133, 29);
            this.richTextBoxGeneralMessage.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBoxGeneralMessage.Name = "richTextBoxGeneralMessage";
            this.richTextBoxGeneralMessage.ReadOnly = true;
            this.richTextBoxGeneralMessage.Size = new System.Drawing.Size(446, 54);
            this.richTextBoxGeneralMessage.TabIndex = 3;
            this.richTextBoxGeneralMessage.Text = "";
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.White;
            this.button3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button3.BackgroundImage")));
            this.button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Location = new System.Drawing.Point(4, 29);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(111, 54);
            this.button3.TabIndex = 0;
            this.button3.TabStop = false;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.Color.Transparent;
            this.groupBox4.Controls.Add(this.button4);
            this.groupBox4.Controls.Add(this.textBoxOutput);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.ForeColor = System.Drawing.Color.SlateGray;
            this.groupBox4.Location = new System.Drawing.Point(55, 330);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox4.Size = new System.Drawing.Size(589, 72);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Step 4 : Specify the outpot folder (for log files and reports)";
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.ForeColor = System.Drawing.Color.DimGray;
            this.button4.Location = new System.Drawing.Point(491, 33);
            this.button4.Margin = new System.Windows.Forms.Padding(2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(65, 21);
            this.button4.TabIndex = 1;
            this.button4.Text = "Select...";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOutput.Location = new System.Drawing.Point(19, 35);
            this.textBoxOutput.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(437, 20);
            this.textBoxOutput.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button5);
            this.groupBox5.Controls.Add(this.textBoxTemplate);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.ForeColor = System.Drawing.Color.SlateGray;
            this.groupBox5.Location = new System.Drawing.Point(55, 110);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox5.Size = new System.Drawing.Size(589, 72);
            this.groupBox5.TabIndex = 2;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Step 2 : Select a template file (xml)";
            // 
            // button5
            // 
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.ForeColor = System.Drawing.Color.DimGray;
            this.button5.Location = new System.Drawing.Point(491, 32);
            this.button5.Margin = new System.Windows.Forms.Padding(2);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(65, 21);
            this.button5.TabIndex = 1;
            this.button5.Text = "Select...";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // textBoxTemplate
            // 
            this.textBoxTemplate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTemplate.Location = new System.Drawing.Point(19, 32);
            this.textBoxTemplate.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxTemplate.Name = "textBoxTemplate";
            this.textBoxTemplate.Size = new System.Drawing.Size(437, 20);
            this.textBoxTemplate.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(713, 450);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.AutoScroll = true;
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox5);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(705, 424);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Gernal";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.AutoScroll = true;
            this.tabPage2.Controls.Add(this.groupBox9);
            this.tabPage2.Controls.Add(this.groupBox8);
            this.tabPage2.Controls.Add(this.groupBox7);
            this.tabPage2.Controls.Add(this.groupBox6);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(705, 424);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Benchmark";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.splitContainer1);
            this.groupBox9.Location = new System.Drawing.Point(53, 14);
            this.groupBox9.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox9.Size = new System.Drawing.Size(589, 114);
            this.groupBox9.TabIndex = 5;
            this.groupBox9.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(2, 15);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.buttonShowOptions);
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.checkBoxOverwritePrice);
            this.splitContainer1.Size = new System.Drawing.Size(585, 97);
            this.splitContainer1.SplitterDistance = 68;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 3;
            // 
            // buttonShowOptions
            // 
            this.buttonShowOptions.AutoSize = true;
            this.buttonShowOptions.BackColor = System.Drawing.Color.WhiteSmoke;
            this.buttonShowOptions.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonShowOptions.Cursor = System.Windows.Forms.Cursors.PanSouth;
            this.buttonShowOptions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonShowOptions.FlatAppearance.BorderColor = System.Drawing.Color.Gainsboro;
            this.buttonShowOptions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonShowOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonShowOptions.ForeColor = System.Drawing.Color.Teal;
            this.buttonShowOptions.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonShowOptions.Location = new System.Drawing.Point(0, 43);
            this.buttonShowOptions.Margin = new System.Windows.Forms.Padding(2);
            this.buttonShowOptions.Name = "buttonShowOptions";
            this.buttonShowOptions.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.buttonShowOptions.Size = new System.Drawing.Size(585, 25);
            this.buttonShowOptions.TabIndex = 7;
            this.buttonShowOptions.Text = "Options";
            this.buttonShowOptions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonShowOptions.UseVisualStyleBackColor = false;
            this.buttonShowOptions.Click += new System.EventHandler(this.buttonShowOptions_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.checkBoxComp);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxCompPrice);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxBenchPrice);
            this.flowLayoutPanel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(585, 23);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // checkBoxComp
            // 
            this.checkBoxComp.AutoSize = true;
            this.checkBoxComp.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.checkBoxComp.Location = new System.Drawing.Point(2, 2);
            this.checkBoxComp.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxComp.Name = "checkBoxComp";
            this.checkBoxComp.Size = new System.Drawing.Size(212, 21);
            this.checkBoxComp.TabIndex = 0;
            this.checkBoxComp.Text = "Load benchmark composition";
            this.checkBoxComp.UseVisualStyleBackColor = true;
            this.checkBoxComp.CheckedChanged += new System.EventHandler(this.checkBoxComp_CheckedChanged);
            // 
            // checkBoxCompPrice
            // 
            this.checkBoxCompPrice.AutoSize = true;
            this.checkBoxCompPrice.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.checkBoxCompPrice.Location = new System.Drawing.Point(218, 2);
            this.checkBoxCompPrice.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxCompPrice.Name = "checkBoxCompPrice";
            this.checkBoxCompPrice.Size = new System.Drawing.Size(180, 21);
            this.checkBoxCompPrice.TabIndex = 1;
            this.checkBoxCompPrice.Text = "Load composition prices";
            this.checkBoxCompPrice.UseVisualStyleBackColor = true;
            this.checkBoxCompPrice.CheckedChanged += new System.EventHandler(this.checkBoxCompPrice_CheckedChanged);
            // 
            // checkBoxBenchPrice
            // 
            this.checkBoxBenchPrice.AutoSize = true;
            this.checkBoxBenchPrice.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.checkBoxBenchPrice.Location = new System.Drawing.Point(402, 2);
            this.checkBoxBenchPrice.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxBenchPrice.Name = "checkBoxBenchPrice";
            this.checkBoxBenchPrice.Size = new System.Drawing.Size(168, 21);
            this.checkBoxBenchPrice.TabIndex = 2;
            this.checkBoxBenchPrice.Text = "Load benchmark price";
            this.checkBoxBenchPrice.UseVisualStyleBackColor = true;
            this.checkBoxBenchPrice.CheckedChanged += new System.EventHandler(this.checkBoxBenchPrice_CheckedChanged);
            // 
            // checkBoxOverwritePrice
            // 
            this.checkBoxOverwritePrice.AutoSize = true;
            this.checkBoxOverwritePrice.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.checkBoxOverwritePrice.Location = new System.Drawing.Point(2, 8);
            this.checkBoxOverwritePrice.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxOverwritePrice.Name = "checkBoxOverwritePrice";
            this.checkBoxOverwritePrice.Size = new System.Drawing.Size(102, 17);
            this.checkBoxOverwritePrice.TabIndex = 3;
            this.checkBoxOverwritePrice.Text = "Overwrite prices";
            this.checkBoxOverwritePrice.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.BackColor = System.Drawing.Color.Transparent;
            this.groupBox8.Controls.Add(this.buttonBenchmarkOutput);
            this.groupBox8.Controls.Add(this.textBoxBenchmarkOutput);
            this.groupBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox8.ForeColor = System.Drawing.Color.SlateGray;
            this.groupBox8.Location = new System.Drawing.Point(57, 346);
            this.groupBox8.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox8.Size = new System.Drawing.Size(583, 72);
            this.groupBox8.TabIndex = 4;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Step 2 : Specify the outpot folder (for log files and reports)";
            // 
            // buttonBenchmarkOutput
            // 
            this.buttonBenchmarkOutput.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonBenchmarkOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBenchmarkOutput.ForeColor = System.Drawing.Color.DimGray;
            this.buttonBenchmarkOutput.Location = new System.Drawing.Point(491, 33);
            this.buttonBenchmarkOutput.Margin = new System.Windows.Forms.Padding(2);
            this.buttonBenchmarkOutput.Name = "buttonBenchmarkOutput";
            this.buttonBenchmarkOutput.Size = new System.Drawing.Size(65, 21);
            this.buttonBenchmarkOutput.TabIndex = 1;
            this.buttonBenchmarkOutput.Text = "Select...";
            this.buttonBenchmarkOutput.UseVisualStyleBackColor = true;
            this.buttonBenchmarkOutput.Click += new System.EventHandler(this.buttonBenchmarkOutput_Click);
            // 
            // textBoxBenchmarkOutput
            // 
            this.textBoxBenchmarkOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBenchmarkOutput.Location = new System.Drawing.Point(19, 35);
            this.textBoxBenchmarkOutput.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxBenchmarkOutput.Name = "textBoxBenchmarkOutput";
            this.textBoxBenchmarkOutput.Size = new System.Drawing.Size(437, 20);
            this.textBoxBenchmarkOutput.TabIndex = 0;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.richTextBoxBenchmarkMessage);
            this.groupBox7.Controls.Add(this.buttonBenchmarkRun);
            this.groupBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox7.ForeColor = System.Drawing.Color.MediumSeaGreen;
            this.groupBox7.Location = new System.Drawing.Point(57, 451);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox7.Size = new System.Drawing.Size(602, 102);
            this.groupBox7.TabIndex = 3;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Step 3 : Run";
            // 
            // richTextBoxBenchmarkMessage
            // 
            this.richTextBoxBenchmarkMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxBenchmarkMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxBenchmarkMessage.ForeColor = System.Drawing.Color.White;
            this.richTextBoxBenchmarkMessage.Location = new System.Drawing.Point(127, 27);
            this.richTextBoxBenchmarkMessage.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBoxBenchmarkMessage.Name = "richTextBoxBenchmarkMessage";
            this.richTextBoxBenchmarkMessage.ReadOnly = true;
            this.richTextBoxBenchmarkMessage.Size = new System.Drawing.Size(456, 54);
            this.richTextBoxBenchmarkMessage.TabIndex = 2;
            this.richTextBoxBenchmarkMessage.Text = "";
            // 
            // buttonBenchmarkRun
            // 
            this.buttonBenchmarkRun.BackColor = System.Drawing.Color.White;
            this.buttonBenchmarkRun.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonBenchmarkRun.BackgroundImage")));
            this.buttonBenchmarkRun.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonBenchmarkRun.FlatAppearance.BorderSize = 0;
            this.buttonBenchmarkRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBenchmarkRun.Location = new System.Drawing.Point(5, 27);
            this.buttonBenchmarkRun.Margin = new System.Windows.Forms.Padding(2);
            this.buttonBenchmarkRun.Name = "buttonBenchmarkRun";
            this.buttonBenchmarkRun.Size = new System.Drawing.Size(108, 54);
            this.buttonBenchmarkRun.TabIndex = 0;
            this.buttonBenchmarkRun.TabStop = false;
            this.buttonBenchmarkRun.UseVisualStyleBackColor = true;
            this.buttonBenchmarkRun.Click += new System.EventHandler(this.buttonBenchmarkRun_Click);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label3);
            this.groupBox6.Controls.Add(this.textBoxCompFilter);
            this.groupBox6.Controls.Add(this.label2);
            this.groupBox6.Controls.Add(this.textBoxBenchmarkFilter);
            this.groupBox6.Controls.Add(this.buttonBenchmarkInput);
            this.groupBox6.Controls.Add(this.textBoxBenchmarkInput);
            this.groupBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.ForeColor = System.Drawing.Color.SlateGray;
            this.groupBox6.Location = new System.Drawing.Point(55, 175);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox6.Size = new System.Drawing.Size(587, 134);
            this.groupBox6.TabIndex = 2;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Step 1 : Specify the input file folder";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 99);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Composition filter";
            // 
            // textBoxCompFilter
            // 
            this.textBoxCompFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxCompFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCompFilter.Location = new System.Drawing.Point(129, 98);
            this.textBoxCompFilter.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxCompFilter.Name = "textBoxCompFilter";
            this.textBoxCompFilter.Size = new System.Drawing.Size(427, 20);
            this.textBoxCompFilter.TabIndex = 4;
            this.textBoxCompFilter.TextChanged += new System.EventHandler(this.textBoxCompFilter_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 65);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Benchmark filter";
            // 
            // textBoxBenchmarkFilter
            // 
            this.textBoxBenchmarkFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxBenchmarkFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBenchmarkFilter.Location = new System.Drawing.Point(129, 64);
            this.textBoxBenchmarkFilter.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxBenchmarkFilter.Name = "textBoxBenchmarkFilter";
            this.textBoxBenchmarkFilter.Size = new System.Drawing.Size(427, 20);
            this.textBoxBenchmarkFilter.TabIndex = 2;
            this.textBoxBenchmarkFilter.TextChanged += new System.EventHandler(this.textBoxBenchmarkFilter_TextChanged);
            // 
            // buttonBenchmarkInput
            // 
            this.buttonBenchmarkInput.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonBenchmarkInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBenchmarkInput.ForeColor = System.Drawing.Color.DimGray;
            this.buttonBenchmarkInput.Location = new System.Drawing.Point(491, 23);
            this.buttonBenchmarkInput.Margin = new System.Windows.Forms.Padding(2);
            this.buttonBenchmarkInput.Name = "buttonBenchmarkInput";
            this.buttonBenchmarkInput.Size = new System.Drawing.Size(65, 21);
            this.buttonBenchmarkInput.TabIndex = 1;
            this.buttonBenchmarkInput.Text = "Select...";
            this.buttonBenchmarkInput.UseVisualStyleBackColor = true;
            this.buttonBenchmarkInput.Click += new System.EventHandler(this.buttonBenchmarkInput_Click);
            // 
            // textBoxBenchmarkInput
            // 
            this.textBoxBenchmarkInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBenchmarkInput.Location = new System.Drawing.Point(19, 25);
            this.textBoxBenchmarkInput.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxBenchmarkInput.Name = "textBoxBenchmarkInput";
            this.textBoxBenchmarkInput.Size = new System.Drawing.Size(437, 20);
            this.textBoxBenchmarkInput.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(713, 450);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Mediolanum ETL";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);

        }

      

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBoxSophisETL;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBoxInput;
        private System.Windows.Forms.TextBox textBoxInputFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox textBoxTemplate;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBoxComp;
        private System.Windows.Forms.CheckBox checkBoxCompPrice;
        private System.Windows.Forms.CheckBox checkBoxBenchPrice;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button buttonBenchmarkRun;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxCompFilter;     
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxBenchmarkFilter;
        private System.Windows.Forms.Button buttonBenchmarkInput;
        private System.Windows.Forms.TextBox textBoxBenchmarkInput;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Button buttonBenchmarkOutput;
        private System.Windows.Forms.TextBox textBoxBenchmarkOutput;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.CheckBox checkBoxOverwritePrice;
        private System.Windows.Forms.Button buttonShowOptions;
        private System.Windows.Forms.RichTextBox richTextBoxGeneralMessage;
        private System.Windows.Forms.RichTextBox richTextBoxBenchmarkMessage;

    }
}


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SophisETLGUI
{
    public partial class Form1 : Form
    {
        private readonly string MEDIOLOGNAME = "SophisETL_MEDIO.log";
        private readonly string MEDIOREPORTNAME = "SophisETL_MEDIO.csv";
        private readonly string INICONFIGFILE = Directory.GetCurrentDirectory() +@"\parameters.ini";
        private string BENCH_PRICE_ETL_XML = "";
        private string BENCH_PRICE_TEMP_XML = "";
        private string BENCH_COMPPRICE_ETL_XML = "";
        private string BENCH_COMPPRICE_TEMP_XML = "";
        private string BENCH_COMP_ETL_XML = "";
        private string BENCH_COMP_TEMP_XML = "";
        private List<string> _CSVFilesPerSession = new List<string>();

        public Form1()
        {
            InitializeComponent();
            textBoxSophisETL.Text = ConfigurationManager.AppSettings["sophisETLxml"];
            textBoxInput.Text = ConfigurationManager.AppSettings["inputFolder"];
            textBoxInputFilter.Text = ConfigurationManager.AppSettings["inputFilter"];
            textBoxOutput.Text = ConfigurationManager.AppSettings["outputFolder"];
            textBoxTemplate.Text = ConfigurationManager.AppSettings["templateFile"];
            textBoxBenchmarkFilter.Text = ConfigurationManager.AppSettings["BenchmarkFilter"];
            textBoxCompFilter.Text = ConfigurationManager.AppSettings["CompositionFilter"];
            textBoxBenchmarkInput.Text = ConfigurationManager.AppSettings["BenchmarkInputFolder"];
            textBoxBenchmarkOutput.Text = ConfigurationManager.AppSettings["BenchmarkOutputFolder"];
            
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            BENCH_COMPPRICE_ETL_XML = config.AppSettings.Settings["BenchmarkCompPriceETLXML"].Value;
            BENCH_COMPPRICE_TEMP_XML = config.AppSettings.Settings["BenchmarkCompPriceTemplate"].Value;
            BENCH_COMP_ETL_XML = config.AppSettings.Settings["BenchmarkCompETLXML"].Value;
            BENCH_COMP_TEMP_XML = config.AppSettings.Settings["BenchmarkCompTemplate"].Value;
            BENCH_PRICE_ETL_XML = config.AppSettings.Settings["BenchmarkPriceETLXML"].Value;
            BENCH_PRICE_TEMP_XML = config.AppSettings.Settings["BenchmarkPriceTemplate"].Value;

            splitContainer1.Panel2Collapsed = true;
        }

        #region Callbacks 

        #region General 

        // button Sophis_ETL.xml
        private void button1_Click(object sender, EventArgs e)
        {
            var FD = new System.Windows.Forms.OpenFileDialog() { AutoUpgradeEnabled = false };
            FD.InitialDirectory = Directory.GetCurrentDirectory();
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxSophisETL.Text = FD.FileName;

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["sophisETLxml"].Value = textBoxSophisETL.Text;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        // button Template
        private void button5_Click(object sender, EventArgs e)
        {
            var FD = new System.Windows.Forms.OpenFileDialog() { AutoUpgradeEnabled = false };
            FD.InitialDirectory = Directory.GetCurrentDirectory();
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxTemplate.Text = FD.FileName;
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["templateFile"].Value = textBoxTemplate.Text;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        // button Input folder 
        private void button2_Click(object sender, EventArgs e)
        {
            SetPath(textBoxInput);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["inputFolder"].Value = textBoxInput.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        // button Output folder
        private void button4_Click(object sender, EventArgs e)
        {
            SetPath(textBoxOutput);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["outputFolder"].Value = textBoxOutput.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        // button Run
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (backgroundWorker1.IsBusy != true)
                {
                    if (UpdateTemplateFile(textBoxSophisETL.Text, textBoxTemplate.Text))
                    {
                        var csvList = GetListOfFiles(textBoxInput.Text, textBoxInputFilter.Text);
                        foreach (var csv in csvList)
                        {
                            this.richTextBoxGeneralMessage.Text = "Processing file " + csv + " ...";
                            SaveParametersIniFile(csv, textBoxOutput.Text, MEDIOLOGNAME, MEDIOREPORTNAME);
                            var arr = new string[] { "-C", INICONFIGFILE, "-X", textBoxSophisETL.Text };
                            string errorMessage = "";
                            if (!SophisETL.Program.RunGUIMode(arr, ref errorMessage))
                            {
                                MessageBox.Show(errorMessage, "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                            else
                                this.richTextBoxGeneralMessage.Text = "File " + csv + " is processed!";
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MessageBox.Show("The upload has been successfully completed!", "Finished",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.richTextBoxGeneralMessage.Text = "";
            }
        }

        private void textBoxInputFilter_TextChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["inputFilter"].Value = textBoxInputFilter.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion
        
        #region Benchmark
        // Checkboxs
        private void checkBoxComp_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBoxComp.Checked)
            {
                // Check and alert 
                if (String.IsNullOrEmpty(BENCH_COMP_ETL_XML) || String.IsNullOrEmpty(BENCH_COMP_TEMP_XML))
                {
                    MessageBox.Show("sophis_etl.xml or template file is missing for this task. Please check the .config", "Configuration",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.checkBoxComp.Checked = false;
                }
            }
        }

        private void checkBoxCompPrice_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBoxCompPrice.Checked)
            {
                // Check and alert 
                if (String.IsNullOrEmpty(BENCH_COMPPRICE_ETL_XML) || String.IsNullOrEmpty(BENCH_COMPPRICE_TEMP_XML))
                {
                    MessageBox.Show("sophis_etl.xml or template file is missing for this task. Please check the .config", "Configuration",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.checkBoxCompPrice.Checked = false;
                }
            }
        }

        private void checkBoxBenchPrice_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBoxBenchPrice.Checked)
            {
                // Check and alert 
                if (String.IsNullOrEmpty(BENCH_PRICE_ETL_XML) || String.IsNullOrEmpty(BENCH_PRICE_TEMP_XML))
                {
                    MessageBox.Show("sophis_etl.xml or template file is missing for this task. Please check the .config", "Configuration",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.checkBoxBenchPrice.Checked = false;
                }
            }
        }

        private void textBoxBenchmarkFilter_TextChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["BenchmarkFilter"].Value = textBoxBenchmarkFilter.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void textBoxCompFilter_TextChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["CompositionFilter"].Value = textBoxCompFilter.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void buttonBenchmarkOutput_Click(object sender, EventArgs e)
        {
            SetPath(textBoxBenchmarkOutput);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["BenchmarkOutputFolder"].Value = textBoxBenchmarkOutput.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void buttonBenchmarkInput_Click(object sender, EventArgs e)
        {
            SetPath(textBoxBenchmarkInput);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["BenchmarkInputFolder"].Value = textBoxBenchmarkInput.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void buttonShowOptions_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
            // this.splitContainer1.Panel2.Visible = !this.splitContainer1.Panel2.Visible;
        }

        private void buttonBenchmarkRun_Click(object sender, EventArgs e)
        {
            int successfulCount = 0;
            _CSVFilesPerSession.Clear();
            this.richTextBoxBenchmarkMessage.Text = "";

            if (checkBoxComp.Checked)
            {
                if (RunOneETLTask(BENCH_COMP_ETL_XML, BENCH_COMP_TEMP_XML, textBoxBenchmarkInput.Text, textBoxCompFilter.Text, textBoxBenchmarkOutput.Text))
                    successfulCount++;
            }
            if (checkBoxCompPrice.Checked)
            {
                if (UpdateETLFile(BENCH_COMPPRICE_ETL_XML, "turnOn", (!this.checkBoxOverwritePrice.Checked).ToString().ToLower()))
                if(RunOneETLTask(BENCH_COMPPRICE_ETL_XML, BENCH_COMPPRICE_TEMP_XML, textBoxBenchmarkInput.Text,textBoxCompFilter.Text, textBoxBenchmarkOutput.Text))
                    successfulCount++;
            }
            if (checkBoxBenchPrice.Checked)
            {
                if(RunOneETLTask(BENCH_PRICE_ETL_XML, BENCH_PRICE_TEMP_XML, textBoxBenchmarkInput.Text,textBoxBenchmarkFilter.Text, textBoxBenchmarkOutput.Text))
                    successfulCount++;
            }
            if (!checkBoxComp.Checked && !checkBoxCompPrice.Checked && !checkBoxBenchPrice.Checked)
            {
                MessageBox.Show("Please select at least one task to run", "ETL Benchmark upload",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (successfulCount > 0)
            {
                MessageBox.Show(successfulCount + " task(s) have been successfully completed! (" + _CSVFilesPerSession.Count + " files have been processed)", "Finished",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                foreach (var csv in _CSVFilesPerSession)
                    MoveFile(csv);
            }
            else
            {
                MessageBox.Show("There are errors while running the task(s). Please check the logs", "Finished",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
        #endregion
        
        #endregion
      
        #region internal 

        private void SetPath(TextBox textbox)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Directory.GetCurrentDirectory();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                DirectoryInfo d = new DirectoryInfo(dialog.SelectedPath);
                if (!d.Exists)
                {
                    MessageBox.Show("Cannot find folder  " + dialog.SelectedPath, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                textbox.Text = dialog.SelectedPath;
            }
        }

        private void SaveParametersIniFile(string csvFile, string outputDir, string logFileName, string reportFileName)
        {
            try
            {
                // Check 
                if (!File.Exists(csvFile))
                {
                    throw new FileNotFoundException("Cannot find csv file " + csvFile);
                }
                if (String.IsNullOrEmpty(outputDir))
                {
                    throw new FileNotFoundException("Output folder cannot be null or empty");
                }
                DirectoryInfo d = new DirectoryInfo(outputDir);
                if (!d.Exists)
                {
                    throw new FileNotFoundException("Cannot find folder  " + outputDir);
                }
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(INICONFIGFILE))
                {
                    file.WriteLine("csv_file = " + csvFile);
                    file.WriteLine("log_file = " + outputDir  + @"\"+ logFileName);
                    file.WriteLine("report_file = " + outputDir + @"\" + reportFileName);
                }
            }
            catch (Exception e)
            {
                throw new IOException("Error : " + e.Message);
            }
        }

        private List<string> GetListOfFiles(string directory, string filter)
        {
            var res = new List<string>();
            DirectoryInfo d = new DirectoryInfo(directory);
            if (!d.Exists)
            {
                MessageBox.Show("Folder " + directory + " does not exist. Please make sure it's a valid file", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return res;
            }
            string[] Files = Directory.GetFiles(directory, filter, SearchOption.AllDirectories);
            foreach (var file in Files)
            {
                res.Add(file);
            }
            return res;
        }

        private bool UpdateTemplateFile(string sophisEtl, string template)
        {
            bool res = false;
            try
            {
                // Check 
                if (!File.Exists(sophisEtl))
                {
                    MessageBox.Show("sophis_etl.xml file " + sophisEtl + " does not exist. Please make sure it's a valid file", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (!File.Exists(template))
                {
                    MessageBox.Show("Template file " + template + " does not exist. Please make sure it's a valid file", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                XmlDocument xml = new XmlDocument();
                xml.Load(sophisEtl);
                XmlNodeList elemList = xml.GetElementsByTagName("template");
                for (int i = 0; i < elemList.Count; i++)
                {
                    // only 1 node is expected
                    elemList[i].InnerXml = template;
                    xml.Save(sophisEtl);
                    break;
                }  
                res = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
            return res;
        }

        private bool UpdateETLFile(string sophisEtl, string key, string value)
        {
            bool res = false;
            try
            {
                // Check 
                if (!File.Exists(sophisEtl))
                {
                    MessageBox.Show("sophis_etl.xml file " + sophisEtl + " does not exist. Please make sure it's a valid file", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                XmlDocument xml = new XmlDocument();
                xml.Load(sophisEtl);
                XmlNodeList elemList = xml.GetElementsByTagName(key);
                for (int i = 0; i < elemList.Count; i++)
                {
                    // only 1 node is expected
                    elemList[i].InnerXml = value;
                    xml.Save(sophisEtl);
                    break;
                }
                res = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
            return res;
        }

        private bool RunOneETLTask(string etlxml, string templateFile, string inputDirectory, string filter, string outputDir)
        {
            bool res = true;
            try
            {
                if (UpdateTemplateFile(etlxml, templateFile))
                {
                    var csvList = GetListOfFiles(inputDirectory, filter);
                    foreach (var csv in csvList)
                    {
                        this.richTextBoxBenchmarkMessage.Text = "Processing file " + csv + " ...";
                        SaveParametersIniFile(csv, outputDir, MEDIOLOGNAME, MEDIOREPORTNAME);
                        var arr = new string[] { "-C", INICONFIGFILE, "-X", etlxml };
                        string errorMessage = "";
                        if (SophisETL.Program.RunGUIMode(arr, ref errorMessage))
                        {
                            this.richTextBoxBenchmarkMessage.Text = "File " + csv + " is processed!";
                            _CSVFilesPerSession.Add(csv);
                        }
                        else
                            res = false;
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return res;
        }

        private void MoveFile(string sourceFileName)
        {
            try
            {
                var fileName = Path.GetFileName(sourceFileName);
                var targetDir = Directory.GetCurrentDirectory() + @"\Output";
                var targetFile = Path.Combine(targetDir,fileName);

                if (!Directory.Exists(targetDir))
                {
                    DirectoryInfo di = Directory.CreateDirectory(targetDir);
                }

                // Ensure that the target does not exist.
                if (File.Exists(targetFile))
                    File.Delete(targetFile);

                // Move the file.
                File.Move(sourceFileName, targetFile);
            }
            catch (Exception e)
            {
                    MessageBox.Show("Error while moving csv files : " + e.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

   
    }
}

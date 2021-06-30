using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Eff.ToolkitReporting.NET.Interfaces;
using Eff.ToolkitReporting.NET.Models;
using Eff.ToolkitReporting.NET.PropertyGrid;
using Eff.Utils;
using Microsoft.Office.Interop.Excel;
using sophis.guicommon.basicDialogs;
using sophis.reporting;
using Sophis.Windows.Forms.Input;
using ToolkitReporting.NET;

// ReSharper disable once CheckNamespace
namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public partial class ReportingMainForm : XtraUserControl, ISMMdiEmbeddable
        {
            private readonly IReportService _reportService;
            private ReportingObject _currentReportConfig;
            private PropertyGridObject _currentReportGenConfig;
            private bool _configChanged;
            public sophis.xaml.XSRWinFormsAdapter<ReportingMainForm> Adapter { get; set; }

            #region ISMMdiEmbeddable Members

            public bool OnCloseDialog()
            {
                return ReportingMainForm_FormClosing();
            }

            public void OnDelete(){}

            public void OnEditCopy(){}

            public void OnEditCut(){}

            public void OnEditPaste(){}

            public void OnEditSelectAll(){}

            public void OnEditUndo(){}

            public void OnEditXMLCopy(){}

            public void OnFileNew(){}

            public void OnFileSave(){}

            public void OnLoaded(){}

            public void OnUnloaded(){}

            public bool CanFileNew() { return false; }

            public bool CanFileSave() { return false; }

            public bool CanDelete() { return false; }

            #endregion

            #region ICommandBindings Members

            public FormCommandBindingCollection CommandBindings
            {
                get { return null; }
            }

            #endregion

            #region ITitled Members

            public string Title
            {
                get { return MainClass.Caption; }
            }

            public event Action<Sophis.Data.Utils.ITitled> TitleChanged;

            public Sophis.Data.Utils.IImageDesc Icon
            {
                get { return null; }
            }

            public event Action<Sophis.Data.Utils.ITitled> IconChanged;

            #endregion

            #region IEmbeddableComponent

            public void OnActivated()
            {
            }

            public void OnDeactivated()
            {
            }
            #endregion

            #region IAssociatedMFCWindow Members

            public IntPtr AssociatedMFCWindow { get; set; }

            #endregion

            public ReportingMainForm(IReportService reportService)
            {
                _reportService = reportService;
                InitializeComponent();
            }

            private void cmdClose_Click(object sender, EventArgs e)
            {
                if (Adapter != null)
                {
                    Adapter.CloseWindow();
                }
            }

            private void ReportingMainForm_Load(object sender, EventArgs ea)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    var arrReports = _reportService.GetReports();
                    foreach (var report in arrReports)
                    {
                        cboReportsGen.Items.Add(report.Name);
                        cboReportsCfg.Items.Add(report.Name);
                    }

                    lblExcelTemplate.Text = Resource.PropertyExcelTemplateDisp + ": ";
                    lblOutputFile.Text = Resource.PropertyOutputFileDisp + ": ";
                    RefreshButtons();
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = DefaultCursor;
                }
            }

            private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
            {
                RefreshButtons();
            }

            private void cboReportsCfg_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (_configChanged)
                {
                    if (MessageBox.Show(Resource.MsgSaveChangesQuestion, MainClass.Caption, MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        cmdApply.PerformClick();
                    }
                }
                _currentReportConfig = new ReportingObject(cboReportsCfg.Text, _reportService);
                _currentReportConfig.PropertyChanged += (o, args) =>
                {
                    _configChanged = true;
                    RefreshButtons();
                };
                pgReportConfig.SelectedObject = _currentReportConfig;
                txtExcelTemplate.Text = _currentReportConfig.GetMember(ReportingObject.SettingExcelTemplate).ToString();
                txtOutputFile.Text = _currentReportConfig.GetMember(ReportingObject.SettingExcelOutputFile).ToString();
                _configChanged = false;
                RefreshButtons();
            }

            private void cboReportsGen_SelectedIndexChanged(object sender, EventArgs e)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    _currentReportGenConfig = new PropertyGridObject();
                    foreach (var param in _reportService.GetParametersByReportName(cboReportsGen.Text))
                    {
                        if (param.Type == ParamType.Date)
                        {
                            _currentReportGenConfig.AddNewProperty(param.Name,
                                string.IsNullOrEmpty(param.Value)
                                    ? DateTime.Today
                                    : DateTime.ParseExact(param.Value, param.Format, null));
                        }
                        else
                        {
                            _currentReportGenConfig.AddNewProperty(param.Name, param.Value);
                        }
                    }
                    pgReportGen.SelectedObject = _currentReportGenConfig;
                    RefreshButtons();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error: {0}", ex.Message), MainClass.Caption, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }

            private void RefreshButtons()
            {
                cmdGenerate.Enabled = cmdPreview.Enabled = tabMain.SelectedTab == tabGeneration && cboReportsGen.Text != string.Empty;
                cmdApply.Enabled = tabMain.SelectedTab == tabConfig && _currentReportConfig != null && _configChanged;
                cmdColumnsMapping.Enabled = tabMain.SelectedTab == tabConfig && _currentReportConfig != null;
                cmdGenerateXslt.Enabled = tabMain.SelectedTab == tabConfig && _currentReportConfig != null && !_configChanged;
                txtOutputFile.Enabled = txtExcelTemplate.Enabled = cmdBrowseInputTemplate.Enabled =
                    cmdBrowseOutputFile.Enabled = _currentReportConfig != null;
                pgReportConfig.Enabled = txtExcelTemplate.Text != string.Empty;
            }

            //private void ReportingMainForm_FormClosing(object sender, FormClosingEventArgs e)
            //public void ReportingMainForm_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
            private bool ReportingMainForm_FormClosing()
            {
                var e = new System.ComponentModel.CancelEventArgs();
                if (_configChanged)
                {
                    switch (MessageBox.Show(Resource.MsgSaveChangesQuestion, MainClass.Caption,
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
                    {
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            break;
                        case DialogResult.Yes:
                            cmdApply.PerformClick();
                            e.Cancel = false;
                            break;
                        case DialogResult.No:
                            e.Cancel = false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    e.Cancel = false;
                }

                return e.Cancel == false;
            }

            private void cmdApply_Click(object sender, EventArgs e)
            {
                if (_currentReportConfig == null) return;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    var reportSettings = _reportService.GetSettingsByReportName(_currentReportConfig.ReportTemplateName);
                    foreach (var setting in reportSettings)
                    {
                        setting.Value = _currentReportConfig.GetMember(setting.Name).ToString();
                    }
                    _reportService.SaveSettings(_currentReportConfig.ReportTemplateName, reportSettings);

                    _configChanged = false;
                    RefreshButtons();
                }
                catch (Exception ex)
                {
                    EmcLog.Error(ex.ToString());
                    MessageBox.Show(string.Format("Error: {0}", ex.Message), MainClass.Caption, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }

            private void cmdGenerate_Click(object sender, EventArgs ea)
            {
                GenerateReport(false);
            }

            private void cmdPreview_Click(object sender, EventArgs e)
            {
                GenerateReport(true);
            }

            private void GenerateReport(bool previewMode)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    var reportName = cboReportsGen.Text;
                    var reportParameters = _reportService.GetParametersByReportName(reportName);
                    foreach (var param in reportParameters)
                    {
                        var paramValue = _currentReportGenConfig.GetMember(param.Name);
                        if (param.Type == ParamType.Date)
                        {
                            param.Value = ((DateTime) paramValue).ToString(param.Format);
                        }
                        else
                        {
                            param.Value = paramValue.ToString();
                        }
                    }

                    var reportSettings = _reportService.GetSettingsByReportName(reportName);

                    var outputFile = previewMode
                        ? string.Format("{0}{1}_{2:yyyyMMdd_HHmmss}.xml", Path.GetTempPath(), reportName, DateTime.Now)
                        : reportSettings.Single(x => x.Name == ReportingObject.SettingExcelOutputFile).Value.ReplaceTokens();

                    var outputFileExt = Path.GetExtension(outputFile);
                    var outputFileWithoutExt = Path.Combine(Path.GetDirectoryName(outputFile), Path.GetFileNameWithoutExtension(outputFile));

                    var outputXmlFile = outputFileWithoutExt + ".xml";
                    _reportService.GenerateReport(reportName, reportParameters, outputXmlFile);
                    File.Copy(outputXmlFile, string.Format("{0}_.xml", outputFileWithoutExt));//Backup of the original file just after generated

                    if (File.Exists(outputXmlFile))
                    {
                        var excel = new Microsoft.Office.Interop.Excel.Application { Visible = true, DisplayAlerts = false };
                        var workbook = excel.Workbooks.Open(outputXmlFile);
                        excel.Calculate();
                        workbook.Save();
                        if (!previewMode)
                        {
                            if (outputFileExt != null)
                            {
                                outputFileExt = outputFileExt.ToUpper();
                                if (outputFileExt != ".XML")
                                {
                                    switch (outputFileExt)
                                    {
                                        case ".PDF":
                                            workbook.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF,
                                                outputFileWithoutExt);
                                            break;
                                        case ".XLSX":
                                            workbook.SaveAs(outputFileWithoutExt, XlFileFormat.xlWorkbookDefault,
                                                Missing.Value,
                                                Missing.Value, false, false, XlSaveAsAccessMode.xlNoChange,
                                                XlSaveConflictResolution.xlUserResolution, true, Missing.Value,
                                                Missing.Value, Missing.Value);
                                            break;
                                    }
                                }
                            }
                            workbook.Close(0);
                            excel.Quit();
                        }
                    }
                    else
                    {
                        MessageBox.Show(Resource.FileNotGenerated, MainClass.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error: {0}", ex.Message), MainClass.Caption, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = DefaultCursor;
                }
            }

            private void ReportingMainForm_SizeChanged(object sender, EventArgs e)
            {
                try
                {
                    pnlCommands.Left = Width - 120;
                    tabMain.Width = pnlCommands.Left - 30;
                    tabMain.Height = Height - 30;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            private void tabMain_SizeChanged(object sender, EventArgs e)
            {
                try
                {
                    cboReportsGen.Width = cboReportsCfg.Width = tabMain.SelectedTab.Width - 135;
                    cmdBrowseInputTemplate.Left = cmdBrowseOutputFile.Left = tabMain.SelectedTab.Width - 40;
                    txtExcelTemplate.Width = txtOutputFile.Width = tabMain.SelectedTab.Width - 170;
                    pgReportConfig.Height = tabMain.SelectedTab.Height - pnlConfiguration.Height - 10;
                    pgReportGen.Height = tabMain.SelectedTab.Height - pnlGeneration.Height - 10;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            private readonly SaveFileDialog _xsltFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                OverwritePrompt = true,
                //FileName = string.Format("{0}_{1:yyyyMMdd_HHmmss}.xml", reportName, DateTime.Now),
                Filter = Resource.GenerateXsltFilter,
                Title = Resource.GenerateXsltTitle
            };
            private void cmdGenerateXslt_Click(object sender, EventArgs e)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    var reportName = cboReportsCfg.Text;
                    if (_xsltFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _reportService.GenerateXslt(reportName, _xsltFileDialog.FileName);
                        if (File.Exists(_xsltFileDialog.FileName))
                        {
                            MessageBox.Show(Resource.FileXsltGenerated, MainClass.Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(Resource.FileXsltNotGenerated, MainClass.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error: {0}", ex.Message), MainClass.Caption, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = DefaultCursor;
                }
            }

            private void txtExcelTemplate_TextChanged(object sender, EventArgs e)
            {
                if (_currentReportConfig == null) return;
                _currentReportConfig.SetMember(ReportingObject.SettingExcelTemplate, txtExcelTemplate.Text);
                pgReportConfig.Refresh();
                RefreshButtons();
            }

            private void txtOutputFile_TextChanged(object sender, EventArgs e)
            {
                if (_currentReportConfig == null) return;
                _currentReportConfig.SetMember(ReportingObject.SettingExcelOutputFile, txtOutputFile.Text);
                pgReportConfig.Refresh();
                RefreshButtons();
            }

            private readonly FileDialog _excelTemplateFileDialog = new OpenFileDialog
            {
                CheckFileExists = false,
                //OverwritePrompt = true,
                //FileName = string.Format("{0}_{1:yyyyMMdd_HHmmss}.xml", reportName, DateTime.Now),
                Filter = Resource.ExcelTemplateFilter,
                Title = Resource.ExcelTemplateTitle
            };
            private void cmdBrowseInputTemplate_Click(object sender, EventArgs e)
            {
                if (_excelTemplateFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtExcelTemplate.Text = _excelTemplateFileDialog.FileName;
                }
            }

            private readonly FileDialog _outputFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                OverwritePrompt = true,
                //FileName = string.Format("{0}_{1:yyyyMMdd_HHmmss}.xml", reportName, DateTime.Now),
                Filter = Resource.ExcelTemplateFilter,
                Title = Resource.ExcelTemplateTitle
            };
            private void cmdBrowseOutputFile_Click(object sender, EventArgs e)
            {
                if (_outputFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFile.Text = _outputFileDialog.FileName;
                }
            }

            private void cmdColumnsMapping_Click(object sender, EventArgs e)
            {
                if (_currentReportConfig == null) return;
                if (new ParamColMapForm(_currentReportConfig).ShowDialog() == DialogResult.OK)
                {
                    pgReportConfig.Refresh();
                };
            }
        }
    }
}

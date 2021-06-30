using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Eff.ToolkitReporting.NET;
using Eff.ToolkitReporting.NET.PropertyGrid;
using sophis.reporting;

namespace ToolkitReporting.NET
{
    public partial class ParamColMapForm : Form
    {
        private readonly ReportingObject _reportingObject;
        private readonly string[] _arrParams;
        private readonly string[] _arrTables;
        public const char MapValuesSeparator = ';';
        public const char MapNameValueSeparator = '=';

        public Dictionary<string, string> ColMapValue { get; private set; }

        public ParamColMapForm(ReportingObject reportingObject)
        {
            _reportingObject = reportingObject;
            InitializeComponent();

            txtExcelTemplateFile.Text = _reportingObject.GetMember(ReportingObject.SettingExcelTemplate) as string;
            gbMapping.Text = _reportingObject.ReportTemplateName + @" Mapping";

            _arrParams = _reportingObject.GetParamTags();
            lstParams.Items.Clear();
            lstParams.Items.AddRange(_arrParams);
            _arrTables = _reportingObject.GetTableTags();
            lstParams.Items.AddRange(_arrTables);

            using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
            {
                using (var reportTemplate = reportTemplateManager.GetReportTemplateWithName(_reportingObject.ReportTemplateName))
                {
                    if (reportTemplate.GetXMLSourceWithNameNullPossible(Utils.ParametersSourceName) == null)
                    {
                        reportTemplate.GenerateParametersSource();
                    }

                    var xmlSourceList = reportTemplate.GetXMLSourceList();
                    ColMapValue = new Dictionary<string, string>();
                    foreach (var xmlSource in xmlSourceList)
                    {
                        if (new[] {Utils.SourceTypePortfolio, Utils.SourceTypeTrade, Utils.SourceTypeSql}.Contains(
                            xmlSource.SourceType))
                        {
                            var lvwItem = lvwSources.Items.Add(xmlSource.Name);
                            var propertyName = xmlSource.Name + ReportingObject.SettingParamColMap;
                            ColMapValue[propertyName] = (string)reportingObject.GetMember(propertyName);
                            lvwItem.SubItems.Add(xmlSource.SourceType);
                            if (!string.IsNullOrEmpty(ColMapValue[propertyName]))
                            {
                                foreach (var nameValue in ColMapValue[propertyName].Split(MapValuesSeparator))
                                {
                                    var name = nameValue.Split(MapNameValueSeparator)[0];
                                    lstParams.Items.Remove(name);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            foreach (var kvp in ColMapValue)
            {
                _reportingObject.SetMember(kvp.Key, kvp.Value);
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            if (lstParams.SelectedIndex >= 0 && lvwSources.SelectedItems.Count > 0 && lstColumns.SelectedIndex >= 0)
            {
                var propertyName = lvwSources.SelectedItems[0].Text + ReportingObject.SettingParamColMap;
                var itemValue = string.Format("{0}{1}{2}", lstParams.SelectedItem, MapNameValueSeparator, lstColumns.SelectedItem);
                lstMap.Items.Add(itemValue);
                lstParams.Items.RemoveAt(lstParams.SelectedIndex);
                ColMapValue[propertyName] = string.Join(MapValuesSeparator.ToString(), lstMap.Items.OfType<string>().ToArray());
            }
            else
            {
                MessageBox.Show(Resource.MsgSelectParamColumns, MainClass.Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmdAddManual_Click(object sender, EventArgs e)
        {
            if (lstParams.SelectedIndex >= 0 && lvwSources.SelectedItems.Count > 0 && txtColumnName.Text != string.Empty)
            {
                var propertyName = lvwSources.SelectedItems[0].Text + ReportingObject.SettingParamColMap;
                var itemValue = string.Format("{0}{1}{2}", lstParams.SelectedItem, MapNameValueSeparator, txtColumnName.Text);
                lstMap.Items.Add(itemValue);
                lstParams.Items.RemoveAt(lstParams.SelectedIndex);
                ColMapValue[propertyName] = string.Join(MapValuesSeparator.ToString(), lstMap.Items.OfType<string>().ToArray());
            }
            else
            {
                MessageBox.Show(Resource.MsgSelectParamColumnManual, MainClass.Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmdRemove_Click(object sender, EventArgs e)
        {
            if (lstMap.SelectedIndex >=0 )
            {
                var param = ((string)lstMap.SelectedItem).Split(MapNameValueSeparator)[0];
                if (_arrParams.Contains(param))
                {
                    lstParams.Items.Add(param);
                }
                lstMap.Items.RemoveAt(lstMap.SelectedIndex);
                var propertyName = lvwSources.SelectedItems[0].Text + ReportingObject.SettingParamColMap;
                ColMapValue[propertyName] = string.Join(MapValuesSeparator.ToString(), lstMap.Items.OfType<string>().ToArray());
            }
        }

        private void cmdRemoveAll_Click(object sender, EventArgs e)
        {
            if (lstMap.Items.Count == 0) return;
            foreach (string item in lstMap.Items)
            {
                var param = item.Split(MapNameValueSeparator)[0];
                if (_arrParams.Contains(param))
                {
                    lstParams.Items.Add(param);
                }
            }
            lstMap.Items.Clear();
            var propertyName = lvwSources.SelectedItems[0].Text + ReportingObject.SettingParamColMap;
            ColMapValue[propertyName] = string.Empty;
        }

        private void txtColumnName_Enter(object sender, EventArgs e)
        {
            txtColumnName.SelectAll();
        }

        private void txtColumnName_MouseCaptureChanged(object sender, EventArgs e)
        {
            txtColumnName.SelectAll();
        }

        private void lvwSources_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected) return;
            try
            {
                lstColumns.Items.Clear();
                using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
                {
                    using (var reportTemplate =
                        reportTemplateManager.GetReportTemplateWithName(_reportingObject.ReportTemplateName))
                    {
                        var xmlSource = reportTemplate.GetXMLSourceWithName(lvwSources.SelectedItems[0].Text);
                        lstColumns.Items.AddRange(xmlSource.GetSourceColumns());
                        lstMap.Items.Clear();
                        var propertyName = lvwSources.SelectedItems[0].Text + ReportingObject.SettingParamColMap;
                        //var propertyValue = (string)_reportingObject.GetMember(propertyName);
                        var propertyValue = ColMapValue[propertyName];
                        if (!string.IsNullOrEmpty(propertyValue))
                        {
                            lstMap.Items.AddRange(propertyValue.Split(MapValuesSeparator));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error: " + ex.Message);
            }
        }
    }
}

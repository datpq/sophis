using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using Eff.ToolkitReporting.NET.Interfaces;
using Eff.Utils;
using sophis.reporting;
using ToolkitReporting.NET;

// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET.PropertyGrid
{
    public class ReportingObject : PropertyGridObject
    {
        public const string SettingExcelTemplate = "ExcelTemplate";
        public const string SettingExcelOutputFile = "ExcelOutputFile";
        public const string SettingXmlOutputFile = "XmlOutputFile";
        public const string SettingWorksheet = ".Worksheet";
        public const string SettingTableTag = ".TableTag";
        public const string SettingShowHeader = ".ShowHeader";
        public const string SettingParamColMap = ".ParamColMap";

        private readonly string _reportTemplateName;
        public string ReportTemplateName
        {
            get { return _reportTemplateName; }
        }

        public ReportingObject(string reportTemplateName, IReportService reportService)
        {
            try
            {
                _reportTemplateName = reportTemplateName;
                var settings = reportService.GetSettingsByReportName(_reportTemplateName);
                using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
                {
                    using (var reportTemplate = reportTemplateManager.GetReportTemplateWithName(reportTemplateName))
                    {
                        var xmlSourceList = reportTemplate.GetXMLSourceList();
                        foreach (var reportSetting in settings)
                        {
                            string category = null; //Misc
                            if (reportSetting.Name.IndexOf('.') >= 0)
                            {
                                var xmlSource = xmlSourceList.FirstOrDefault(x => x.Name == reportSetting.Name.Split('.')[0]);
                                if (xmlSource != null)
                                {
                                    category = string.Format("{0} {1}", xmlSource.SourceType, xmlSource.Name);
                                }
                            }
                            if (reportSetting.Name == SettingExcelTemplate)
                            {
                                AddNewProperty(reportSetting.Name, reportSetting.Value,
                                    Resource.PropertyExcelTemplateDisp, Resource.PropertyExcelTemplateDesc,
                                    //ExcelTemplateEditor.CreateEditorAttribute(), CategoryMisc);
                                    null, category, true);
                            }
                            else if (reportSetting.Name == SettingExcelOutputFile)
                            {
                                AddNewProperty(reportSetting.Name, reportSetting.Value,
                                    Resource.PropertyOutputFileDisp, Resource.PropertyOutputFileDesc,
                                    //ExcelTemplateEditor.CreateEditorAttribute(), CategoryMisc);
                                    null, category, true);
                            }
                            else if (reportSetting.Name.EndsWith(SettingShowHeader))
                            {
                                bool showHeader;
                                bool.TryParse(reportSetting.Value, out showHeader);
                                AddNewProperty(reportSetting.Name, showHeader,
                                    Resource.PropertyShowHeaderDisp, Resource.PropertyShowHeaderDesc,
                                    null, category);
                            }
                            else if (reportSetting.Name.EndsWith(SettingWorksheet))
                            {
                                AddNewProperty(reportSetting.Name, reportSetting.Value,
                                    Resource.PropertyWorksheetDisp, Resource.PropertyWorksheetDesc,
                                    GridComboBox.CreateEditorAttribute(), category);
                            }
                            else if (reportSetting.Name.EndsWith(SettingTableTag))
                            {
                                AddNewProperty(reportSetting.Name, reportSetting.Value,
                                    Resource.PropertyTableTagDisp, Resource.PropertyTableTagDesc,
                                    GridComboBox.CreateEditorAttribute(), category);
                            }
                            else if (reportSetting.Name.EndsWith(SettingParamColMap))
                            {

                                AddNewProperty(reportSetting.Name, reportSetting.Value,
                                    Resource.PropertyParamColMapDisp, Resource.PropertyParamColMapDesc,
                                    //PortfolioSourceParamColMap.CreateEditorAttribute(),
                                    null, category, true);
                            }
                        }

                        //var categories = settings.Where(x => x.Name.Contains(".")).Select(x =>
                        //    x.Name.Substring(0, x.Name.IndexOf(".", StringComparison.Ordinal))).Distinct();
                        //XML output file
                        AddNewProperty(SettingXmlOutputFile, reportTemplate.GetXMLFilename(),
                            Resource.PropertyXmlOutputFileDisp, Resource.PropertyXmlOutputFileDesc,
                            null, null, true); //Misc sourceName
                        //Queries of SQL sources
                        foreach (var sqlSource in xmlSourceList.Where(x => x.SourceType == Utils.SourceTypeSql))
                        {
                            var category = string.Format("{0} {1}", sqlSource.SourceType, sqlSource.Name);
                            AddNewProperty(string.Format("{0}.Query", sqlSource.Name), sqlSource.GetSourceSqlQuery(),
                                Resource.PropertyQueryDisp, Resource.PropertyQueryDesc,
                                new EditorAttribute(typeof(MultilineStringEditor), typeof(UITypeEditor)),
                                category, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public string[] GetWorkSheets()
        {
            var excelTemplateFile = (string)GetMember(SettingExcelTemplate);
            return ExcelUtils.GetWorkSheets(excelTemplateFile);
        }

        public string[] GetParamTags()
        {
            var excelTemplateFile = (string)GetMember(SettingExcelTemplate);
            return ExcelUtils.GetParamTags(excelTemplateFile);
        }

        public string[] GetTableTagsBySource(string sourceName)
        {
            var worksheetParamName = string.Format("{0}{1}", sourceName, SettingWorksheet);
            var worksheetName = (string)GetMember(worksheetParamName);
            return GetTableTags(worksheetName);
        }

        public string[] GetTableTags(string worksheetName = null)
        {
            var excelTemplateFile = (string)GetMember(SettingExcelTemplate);
            return ExcelUtils.GetTableTags(excelTemplateFile, worksheetName);
        }
    }
}

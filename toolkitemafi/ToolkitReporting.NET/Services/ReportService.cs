using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Dapper;
using Eff.ToolkitReporting.NET.Interfaces;
using Eff.ToolkitReporting.NET.Models;
using Eff.Utils;
using sophis.reporting;
using Sophis.Reporting.Controls;
using ToolkitReporting.NET;

// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET.Services
{
    public class ReportService : IReportService
    {
        private const string ParamSystem = "@SystemParameter";

        private const string SettingExcelTemplate = "ExcelTemplate";
        private const string SettingExcelOutputFile = "ExcelOutputFile";
        private static readonly string[] SettingBySourceSql = { "{0}.Worksheet", "{0}.TableTag", "{0}.ShowHeader", "{0}.ParamColMap" };
        private static readonly string[] SettingBySourcePortfolioTrade = { "{0}.ParamColMap" };
        private const char MapValuesSeparator = ';';
        private const char MapNameValueSeparator = '=';

        #region IReportService implementation

        public ICollection<Report> GetReports()
        {
            try
            {
                EmcLog.Debug("BEGIN");
                ICollection<Report> result = new List<Report>();
                var connection = Sophis.DataAccess.DBContext.Connection;
                var sqlQuery = "SELECT GROUP_PATH FROM REPORT_TEMPLATE WHERE NAME = :ReportName";
                using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
                {
                    var arrReportTemplates = reportTemplateManager.GetReportTemplateList();
                    foreach (var reportTemplate in arrReportTemplates)
                    {
                        if (reportTemplate.GetXmlSourceListByTypeSql().Length == 0
                            && reportTemplate.GetXmlSourceListByTypePortfolio().Length == 0
                            && reportTemplate.GetXmlSourceListByTypeTrade().Length == 0) continue;
                        var groupName = connection.ExecuteScalar<string>(sqlQuery,
                            new { ReportName = reportTemplate.Name }, null, null, null);
                        result.Add(new Report
                        {
                            Name = reportTemplate.Name,
                            Group = groupName
                        });
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                throw;
            }
            finally
            {
                EmcLog.Debug("END");
            }
        }

        public ICollection<ReportParameter> GetParametersByReportName(string reportName)
        {
            try
            {
                EmcLog.Debug("BEGIN(reportName={0}", reportName);
                ICollection<ReportParameter> result = new List<ReportParameter>();
                using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
                {
                    using (var reportTemplate = reportTemplateManager.GetReportTemplateWithName(reportName))
                    {
                        foreach (CSMParameter param in reportTemplate.GetParametersList())
                        {
                            if (param.GetName().IsSystemParameter() || param.GetName() == string.Empty) continue;
                            var paramType = param.GetType() == "Date" || param.GetType() == "Calendar" ? ParamType.Date : ParamType.String;
                            var paramFormat = param.GetSettingFormat();
                            if (paramType == ParamType.Date)
                            {
                                if (string.IsNullOrEmpty(paramFormat) || paramFormat.ToUpper() == "D")
                                    paramFormat = Utils.TokenDateFormat;
                                paramFormat = paramFormat.Replace('D', 'd').Replace('Y', 'y');//sql format to .NET format
                            }
                            result.Add(new ReportParameter
                            {
                                Name = param.GetName(),
                                Value = param.GetValue(),
                                Type = paramType,
                                Format = paramFormat
                            });
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                throw;
            }
            finally
            {
                EmcLog.Debug("END");
            }
        }

        public ICollection<ReportSetting> GetSettingsByReportName(string reportName)
        {
            try
            {
                EmcLog.Debug("BEGIN(reportName={0}", reportName);
                ICollection<ReportSetting> result = new List<ReportSetting>();
                using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
                {
                    using (var reportTemplate = reportTemplateManager.GetReportTemplateWithName(reportName))
                    {
                        var sysParam = reportTemplate.GetParameterWithNameSafely(ParamSystem);
                        foreach (var s in new[] { SettingExcelTemplate, SettingExcelOutputFile })
                        {
                            result.Add(new ReportSetting
                            {
                                Name = s,
                                Value = sysParam.GetSetting(s)
                            });
                        }

                        //SQL sources
                        var arrXmlSources = reportTemplate.GetXmlSourceListByTypeSql();
                        AddMoreSettings(sysParam, result, arrXmlSources, SettingBySourceSql);

                        //Portfolio sources
                        arrXmlSources = reportTemplate.GetXmlSourceListByTypePortfolio();
                        AddMoreSettings(sysParam, result, arrXmlSources, SettingBySourcePortfolioTrade);

                        //Trade sources
                        arrXmlSources = reportTemplate.GetXmlSourceListByTypeTrade();
                        AddMoreSettings(sysParam, result, arrXmlSources, SettingBySourcePortfolioTrade);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                throw;
            }
            finally
            {
                EmcLog.Debug("END");
            }
        }

        public void SaveSettings(string reportName, ICollection<ReportSetting> reportSettings)
        {
            try
            {
                EmcLog.Debug("BEGIN(reportName={0}", reportName);
                using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
                {
                    using (var reportTemplate = reportTemplateManager.GetReportTemplateWithName(reportName))
                    {
                        using (var param = reportTemplate.GetParameterWithNameSafely(ParamSystem))
                        {
                            var settings = param.GetSettingParameters();
                            settings.Clear();
                            foreach (var s in reportSettings)
                            {
                                settings.Add(s.Name, s.Value);
                            }
                        }
                        reportTemplate.SaveToDatabaseSafely();
                    }
                }
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                throw;
            }
            finally
            {
                EmcLog.Debug("END");
            }
        }

        public void GenerateXslt(string reportName, string outputXsltFile)
        {
            try
            {
                EmcLog.Debug("BEGIN(reportName={0}, outputXsltFile={1})", reportName, outputXsltFile);
                if (string.IsNullOrEmpty(outputXsltFile))
                {
                    throw new Exception(Resource.ExOutputFileNotDefined);
                }

                using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
                {
                    using (var reportTemplate = reportTemplateManager.GetReportTemplateWithName(reportName))
                    {
                        var reportSettings = GetSettingsByReportName(reportName);
                        var excelTemplate = reportSettings.Single(x => x.Name == SettingExcelTemplate).Value;
                        if (string.IsNullOrEmpty(excelTemplate))
                        {
                            throw new Exception(Resource.ExExcelTemplateNotDefined);
                        }
                        excelTemplate = excelTemplate.ReplaceTokens();

                        CreateXsltFileFromExcelTemplate(excelTemplate, outputXsltFile, reportSettings, reportTemplate);
                    }
                }
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                throw;
            }
            finally
            {
                EmcLog.Debug("END");
            }
        }

        public void GenerateReport(string reportName, ICollection<ReportParameter> reportParameters, string outputFile)
        {
            CSMReportTemplate reportTemplate = null;
            CSMReportTemplateManager reportTemplateManager = null;
            CSMXMLTransformation excelTrans = null;
            try
            {
                EmcLog.Debug("BEGIN(reportName={0}, outputFile={1})", reportName, outputFile);
                if (string.IsNullOrEmpty(outputFile))
                {
                    throw new Exception(Resource.ExOutputFileNotDefined);
                }
                reportTemplateManager = CSMReportTemplateManager.GetInstance();
                reportTemplate = reportTemplateManager.GetReportTemplateWithName(reportName);

                var xsltFile = string.Format("{0}{1}_{2:yyyyMMdd_HHmmss}.xslt", Path.GetTempPath(), reportName, DateTime.Now);
                GenerateXslt(reportName, xsltFile);

                excelTrans = Utils.CreateStyleSheetTransformation(xsltFile, outputFile);
                //remove old configuration if exist
                if (reportTemplate.RemoveXmlTransformation(excelTrans))
                {
                    EmcLog.Warning("Remove old transformation {0}", excelTrans.GetNameParameter().GetValue());
                    reportTemplate.SaveToDatabase();
                }
                reportTemplate.AddXMLTransformation(excelTrans);

                //Set parameters values
                foreach (CSMParameter param in reportTemplate.GetParametersList())
                {
                    var reportParameter = reportParameters.SingleOrDefault(x => x.Name == param.GetName());
                    if (reportParameter != null)
                    {
                        param.SetValue(reportParameter.Value);
                    }
                }

                new Thread(() => 
                {
                    try
                    {
                        Thread.CurrentThread.IsBackground = true;
                        CSMAskParameters csmAskParameters = null;
                        var sleepCount = 0;
                        while (csmAskParameters == null && sleepCount++ < 100)
                        {
                            Thread.Sleep(10);
                            EmcLog.Debug("sleeping, waiting for CSMAskParameters count={0}", sleepCount);
                            csmAskParameters = Utils.GetAskParametersDlg();
                        }

                        if (csmAskParameters == null) return;
                        EmcLog.Debug("got CSMAskParameters");
                        //csmAskParameters.Hide();
                        //Set parameters values
                        //foreach (var csmGridParameter in csmAskParameters.GetParameters())
                        //{
                        //    if (csmGridParameter.ApiParameter.IsSystem()) continue;
                        //    csmGridParameter.Value = _currentReportGenConfig.GetMember(csmGridParameter.Name).ToString();
                        //}
                        csmAskParameters.Invoke(new Action(() => csmAskParameters._ButtonOK.PerformClick()));
                    }
                    catch (Exception e)
                    {
                        EmcLog.Error(e.ToString());
                        MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }).Start();

                //backup sql queries first
                var sqlXmlSources = reportTemplate.GetXmlSourceListByTypeSql();
                var queries = new string[sqlXmlSources.Length];
                for (var i = 0; i < sqlXmlSources.Length; i++)
                {
                    queries[i] = sqlXmlSources[i].GetSourceSqlQuery();
                }
                reportTemplate.GenerateDocument();
                //restore sql queries
                for (var i = 0; i < sqlXmlSources.Length; i++)
                {
                    sqlXmlSources[i].SetSourceSqlQuery(queries[i]);
                }

                var count = 0;
                while (count++ < 15)
                {
                    if (File.Exists(outputFile))
                    {
                        EmcLog.Debug("File generated: {0}", outputFile);
                        break;
                    }
                    Thread.Sleep(1000);
                }

                if (!File.Exists(outputFile))
                {
                    EmcLog.Warning("No file generated found: {0}", outputFile);
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Interruption by User")
                {
                    return;
                }
                EmcLog.Error(e.ToString());
                throw;
            }
            finally
            {
                if (reportTemplate != null)
                {
                    if (excelTrans != null)
                    {
                        reportTemplate.RemoveXmlTransformation(excelTrans);
                    }
                    //reportTemplate.SaveToDatabase();
                    reportTemplate.Dispose();
                }

                if (reportTemplateManager != null) reportTemplateManager.Dispose();
                EmcLog.Debug("END");
            }
        }

        #endregion

        #region private functions

        private static void AddMoreSettings(CSMParameter sysParam,
            ICollection<ReportSetting> result, CSMXMLSource[] arrXmlSources, string[] arrSettingNames)
        {
            foreach (var sqlSource in arrXmlSources)
            {
                foreach (var x in arrSettingNames)
                {
                    var s = string.Format(x, sqlSource.Name);
                    result.Add(new ReportSetting
                    {
                        Name = s,
                        Value = sysParam.GetSetting(s)
                    });
                }
            }
        }

        private static void CreateXsltFileFromExcelTemplate(string excelTemplate, string xsltFile,
            ICollection<ReportSetting> reportSettings, CSMReportTemplate reportTemplate)
        {
            EmcLog.Debug("BEGIN(excelTemplate={0}, xsltFile={1})", excelTemplate, xsltFile);
            try
            {
                var docExcelTemplate = XDocument.Load(excelTemplate);
                XNamespace msxslNs = "urn:schemas-microsoft-com:xslt";
                XNamespace reportingNs = "http://www.sophis.net/reporting";
                docExcelTemplate.ReadNamespaces();
                var doc = new XDocument(
                    docExcelTemplate.Declaration,
                    new XElement(ReportingExtensions.XslNs + "stylesheet",
                        new XAttribute("version", "1.0"),
                        new XAttribute("exclude-result-prefixes", "msxsl"),
                        new XAttribute(XNamespace.Xmlns + "xsl", ReportingExtensions.XslNs.NamespaceName),
                        //new XAttribute(XNamespace.Xmlns + "ss", ReportingExtensions.SsNs.NamespaceName),
                        new XAttribute(XNamespace.Xmlns + "msxsl", msxslNs.NamespaceName),
                        new XAttribute(XNamespace.Xmlns + "reporting", reportingNs.NamespaceName),
                        new XElement(ReportingExtensions.XslNs + "variable",
                            new XAttribute("name", "apostrophe"), "'"), // apostrophe variable to be used in concat string function
                        new XElement(ReportingExtensions.XslNs + "output",
                            new XAttribute("method", "xml"),
                            new XAttribute("indent", "yes")),
                        new XElement(ReportingExtensions.XslNs + "template",
                            new XAttribute("match", "/"),
                            docExcelTemplate.Nodes().Where(x => x.NodeType == XmlNodeType.ProcessingInstruction),
                            docExcelTemplate.Root)
                    ));
                if (doc.Root == null) return; // to avoid all ReSharper warning
                //ssNsAttr.Remove();

                //paramName, portfolioSourceName, columnName
                var arrParamColMaps = new List<Tuple<string, string, string>>();

                var xmlSources = reportSettings.Where(x => x.Name.Contains(".")).Select(x =>
                    x.Name.Substring(0, x.Name.IndexOf(".", StringComparison.Ordinal))).Distinct();

                //When you find the tag in all SQL source column by the same name
                //var arrOneRowSqlSources = new List<string>();
                foreach (var source in xmlSources)
                {
                    var isSqlSource = SettingBySourceSql.All(x => reportSettings.Any(y => y.Name == string.Format(x, source)))
                                      && SettingBySourceSql.Length == reportSettings.Count(x => x.Name.StartsWith(source + "."));
                    var isPortfolioTradeSource = SettingBySourcePortfolioTrade.All(x => reportSettings.Any(y => y.Name == string.Format(x, source)))
                                                 && SettingBySourcePortfolioTrade.Length == reportSettings.Count(x => x.Name.StartsWith(source + "."));
                    if (isSqlSource)
                    {
                        var tableTag = reportSettings.Single(z => z.Name == string.Format(SettingBySourceSql[1], source)).Value;
                        if (string.IsNullOrEmpty(tableTag))
                        {
                            //When you find the tag in all SQL source column by the same name
                            //arrOneRowSqlSources.Add(source);
                            var paramColMap = reportSettings.Single(z => z.Name == string.Format(SettingBySourceSql[3], source)).Value;
                            if (!string.IsNullOrEmpty(paramColMap))
                            {
                                var localSource = source;
                                arrParamColMaps.AddRange(from map in paramColMap.Split(MapValuesSeparator)
                                    select map.Split(MapNameValueSeparator) into arrParts
                                    let param = arrParts[0] let col = arrParts[1]
                                    select Tuple.Create(param, localSource, col));
                            }
                            continue;
                        }

                        var worksheetTag = reportSettings.Single(z => z.Name == string.Format(SettingBySourceSql[0], source)).Value;

                        var worksheetElem = doc.Descendants(ReportingExtensions.SsNs + "Worksheet").FirstOrDefault(
                            x => x.Attributes(ReportingExtensions.SsNs + "Name").Any(y => y.Value == worksheetTag));

                        //if worksheet is not defined or not found --> find in the whole document
                        var elems = worksheetElem == null
                            ? doc.Descendants(ReportingExtensions.SsNs + "Worksheet").Descendants(ReportingExtensions.SsNs + "Data")
                            : worksheetElem.Descendants(ReportingExtensions.SsNs + "Data");

                        //get the elements where to put data in first TableTag: Data, Row, Table
                        var dataElem = elems.FirstOrDefault(x => x.Value == tableTag);
                        if (dataElem == null) continue; //if not TableTag found, continue to the next sql source
                        var rowElem = dataElem.Ancestors(ReportingExtensions.SsNs + "Row").Single();
                        var tableElem = dataElem.Ancestors(ReportingExtensions.SsNs + "Table").Single();

                        //remove ExpandedRowCount attribute
                        var expandedColumnCountElem = tableElem
                            .Attributes(ReportingExtensions.SsNs + "ExpandedColumnCount").SingleOrDefault();
                        if (expandedColumnCountElem != null) expandedColumnCountElem.Remove();
                        //remove ExpandedRowCount attribute
                        //tableElem.Attributes().Single(x => x.Name.LocalName == "ExpandedRowCount").Remove();
                        //modify the attribute ExpandedRowCount of the table
                        tableElem.IncrementAttribute("ExpandedRowCount",
                            string.Format("count(reporting:root/reporting:{0}/reporting:{0}Result)", source));

                        rowElem.AddBeforeSelf(new XElement(ReportingExtensions.XslNs + "comment",
                            new XElement(ReportingExtensions.XslNs + "value-of", new XAttribute("select", string.Format("'{0}.BEGIN'", source)))));
                        rowElem.AddAfterSelf(new XElement(ReportingExtensions.XslNs + "comment",
                            new XElement(ReportingExtensions.XslNs + "value-of", new XAttribute("select", string.Format("'{0}.END'", source)))));

                        bool showHeader;
                        bool.TryParse(reportSettings.Single(
                                z => z.Name == string.Format(SettingBySourceSql[2], source)).Value, out showHeader);
                        if (showHeader)
                        {
                            //create title row
                            rowElem.AddBeforeSelf(rowElem);
                            var titleRowElem = rowElem.ElementsBeforeSelf().Last();
                            var titleCellElem = titleRowElem.Descendants(ReportingExtensions.SsNs + "Cell").Single();
                            titleCellElem.ReplaceWith(new XElement(ReportingExtensions.XslNs + "for-each",
                                new XAttribute("select", string.Format("reporting:root/reporting:{0}/reporting:{0}Result[1]/*",
                                    source)), titleCellElem));

                            //populate Titles
                            titleCellElem = titleRowElem.Descendants(ReportingExtensions.SsNs + "Cell").Single();
                            titleCellElem.IncrementAttribute("Index", "position()-1"); //Column's Index
                            dataElem = titleRowElem.Descendants(ReportingExtensions.SsNs + "Data").Single();
                            dataElem.Value = string.Empty;
                            dataElem.Add(new XElement(ReportingExtensions.XslNs + "value-of",
                                new XAttribute("select", "local-name()")));
                        }

                        //populate data
                        rowElem.IncrementAttribute("Index",
                            string.Format("position(){0}", showHeader ? string.Empty : "-1")); //Row's Index
                        var tempElem = new XElement(ReportingExtensions.XslNs + "for-each",
                            new XAttribute("select",
                                string.Format("reporting:root/reporting:{0}/reporting:{0}Result[position()>=1]", source)), rowElem);
                        rowElem.ReplaceWith(tempElem);
                        rowElem = tempElem;
                        var cellElem = rowElem.Descendants(ReportingExtensions.SsNs + "Cell").Single();
                        cellElem.IncrementAttribute("Index", "position()-1"); //Column's Index
                        cellElem.ReplaceWith(new XElement(ReportingExtensions.XslNs + "for-each",
                            new XAttribute("select", "*"), cellElem));
                        dataElem = rowElem.Descendants(ReportingExtensions.SsNs + "Data").Single();
                        dataElem.Value = string.Empty;
                        dataElem.Add(new XElement(ReportingExtensions.XslNs + "value-of", new XAttribute("select", ".")));

                        //Increase Index of all Rows after table data
                        foreach (var x in rowElem.ElementsAfterSelf().DescendantsAndSelf(ReportingExtensions.SsNs + "Row"))
                        {
                            x.IncrementAttribute("Index", string.Format("count(//reporting:{0}Result){1}",
                                source, showHeader ? string.Empty : "-1"));
                        }
                    }
                    else if (isPortfolioTradeSource)
                    {
                        var paramColMap = reportSettings.Single(z => z.Name == string.Format(SettingBySourcePortfolioTrade[0], source)).Value;
                        if (!string.IsNullOrEmpty(paramColMap))
                        {
                            var localSource = source;
                            arrParamColMaps.AddRange(from map in paramColMap.Split(MapValuesSeparator)
                                select map.Split(MapNameValueSeparator) into arrParts
                                let param = arrParts[0] let col = arrParts[1]
                                select Tuple.Create(param, localSource, col));
                        }
                    }
                }

                //find all parameters {ParamName} in the template file, and declare empty variable in XSLT file.
                var reg = new Regex(@"(^|.*[^\{])(\{[a-zA-Z0-9_]*\})([^\}].*|$)"); //match "{Param1} and {Param2}" not "{{Param}}"
                var paramElements = doc.Descendants(ReportingExtensions.SsNs + "Data").Where(x => reg.IsMatch(x.Value)).ToList();
                var arrVariables = new List<string>();
                var arrParamDataElems = new List<KeyValuePair<string, XElement>>();
                foreach (var paramDataElem in paramElements)
                {
                    var paramDataValue = paramDataElem.Value;
                    //paramDataValue = paramDataValue.Replace("\"", "&quot;"); //replace double quote
                    //replace single quotes by concatenation of $apostrophe variable
                    var arrQuoteSeparatedParts = paramDataValue.Split('\'');
                    if (arrQuoteSeparatedParts.Length > 1)
                    {
                        paramDataValue = arrQuoteSeparatedParts[0];
                        for (var i = 1; i < arrQuoteSeparatedParts.Length; i++)
                        {
                            paramDataValue = string.Format("{0}', $apostrophe, '{1}", paramDataValue, arrQuoteSeparatedParts[i]);
                        }
                    }

                    var m = reg.Match(paramDataValue);
                    var variableName = m.Groups[2].Value.ToUpper();
                    variableName = variableName.Substring(1, variableName.Length - 2);
                    if (paramDataValue == string.Format("{{{0}}}", variableName)) // contains only the parameter tag
                    {
                        //declare global variables to be used in the whole document
                        if (!arrVariables.Contains(variableName))
                        {
                            doc.Root.AddFirst(new XElement(ReportingExtensions.XslNs + "variable", new XAttribute("name", variableName)));
                            arrVariables.Add(variableName);
                        }

                        //store the data element and correspondent variable in order to change the data type later
                        arrParamDataElems.Add(new KeyValuePair<string, XElement>(variableName, paramDataElem));
                        paramDataElem.Value = string.Empty;
                        paramDataElem.Add(new XElement(ReportingExtensions.XslNs + "value-of",
                            new XAttribute("select", string.Format("${0}", variableName))));
                    }
                    else
                    { //it's a text contains the parameter
                        while (m.Success)
                        {
                            variableName = m.Groups[2].Value.ToUpper();
                            variableName = variableName.Substring(1, variableName.Length - 2);
                            //declare global variables to be used in the whole document
                            if (!arrVariables.Contains(variableName))
                            {
                                doc.Root.AddFirst(new XElement(ReportingExtensions.XslNs + "variable", new XAttribute("name", variableName)));
                                arrVariables.Add(variableName);
                            }

                            paramDataValue = reg.Replace(paramDataValue, string.Format("$1', ${0}, '$3", variableName));
                            m = reg.Match(paramDataValue);
                        }
                        paramDataElem.Value = string.Empty;
                        paramDataElem.Add(new XElement(ReportingExtensions.XslNs + "value-of",
                            new XAttribute("select", string.Format("concat('{0}')", paramDataValue))));
                    }
                }

                //making up the value of declared variables
                arrVariables.ForEach(variableName =>
                {
                    var variableElem = doc.Root.Elements(ReportingExtensions.XslNs + "variable").SingleOrDefault(x => x.Attribute("name").Value == variableName);
                    if (variableElem != null)
                    {
                        var paramColMap = arrParamColMaps.FirstOrDefault(x => x.Item1 == string.Format("{{{0}}}", variableName));
                        if (paramColMap != null) // variable take the value of a portfolio, trade or SQL source's column
                        {
                            var sourceName = paramColMap.Item2;
                            var sourceColName = paramColMap.Item3;
                            var xmlSource = reportTemplate.GetXMLSourceWithName(sourceName);
                            if (xmlSource.SourceType == Utils.SourceTypePortfolio) //Portfolio source
                            {
                                var rowToCol = xmlSource.GetFieldValueByName(Utils.SourcePortfolioFldRowToCol) == "1";
                                if (rowToCol)
                                {
                                    //create new variable contains the column name when rowToCol setting is 1
                                    //rowToColColumnName variable
                                    doc.Root.AddFirst(new XElement(ReportingExtensions.XslNs + "variable",
                                        new XAttribute("name", string.Format("rowToCol{0}", variableName)),
                                        new XElement(ReportingExtensions.XslNs + "value-of",
                                            new XAttribute("select", string.Format("reporting:root/reporting:{0}/reporting:header/reporting:configuration/reporting:columns/reporting:column[@reporting:name='{1}']/@reporting:tag", sourceName, sourceColName)))));
                                    //ColumnName variable
                                    variableElem.Add(new XElement(ReportingExtensions.XslNs + "value-of",
                                        new XAttribute("select", string.Format("sum(reporting:root/reporting:{0}/reporting:window/reporting:folio/*[local-name()=$rowToCol{1}][number(.) = .])", sourceName, variableName))));
                                }
                                else
                                {
                                    //ColumnName variable
                                    variableElem.Add(new XElement(ReportingExtensions.XslNs + "value-of",
                                        new XAttribute("select", string.Format("sum(reporting:root/reporting:{0}/reporting:window/reporting:folio/reporting:portfolioColumn[@reporting:name='{1}'][number(.) = .])", sourceName, sourceColName))));
                                }
                                //All the column in Portfolio is Number. {AUM}=Net Worth --> $AUM is Number
                                foreach (var paramDataElem in arrParamDataElems.Where(x => x.Key == string.Format("{{{0}}}", variableName)).Select(x => x.Value))
                                {
                                    paramDataElem.Attribute(ReportingExtensions.SsNs + "Type").Value = "Number";
                                }
                            }
                            else if (xmlSource.SourceType == Utils.SourceTypeTrade) // Trade source
                            {
                                var rowToCol = xmlSource.GetFieldValueByName(Utils.SourceTradeFldRowToCol) == "1";
                                if (rowToCol)
                                {
                                    //ColumnName variable
                                    variableElem.Add(new XElement(ReportingExtensions.XslNs + "value-of",
                                        new XAttribute("select", string.Format("sum(reporting:root/reporting:{0}/reporting:result/reporting:tradegroup/reporting:trades/reporting:trade/reporting:{1}[number(.) = .])", sourceName, sourceColName))));
                                }
                                else
                                {
                                    //ColumnName variable
                                    variableElem.Add(new XElement(ReportingExtensions.XslNs + "value-of",
                                        new XAttribute("select", string.Format("sum(reporting:root/reporting:{0}/reporting:result/reporting:tradegroup/reporting:trades/reporting:trade/reporting:column[@reporting:name='{1}'][number(.) = .])", sourceName, sourceColName))));
                                }
                                //All the column in Portfolio is Number. {AUM}=Net Worth --> $AUM is Number
                                foreach (var paramDataElem in arrParamDataElems.Where(x => x.Key == string.Format("{{{0}}}", variableName)).Select(x => x.Value))
                                {
                                    paramDataElem.Attribute(ReportingExtensions.SsNs + "Type").Value = "Number";
                                }
                            }
                            else if (xmlSource.SourceType == Utils.SourceTypeSql)
                            {
                                variableElem.Add(new XElement(ReportingExtensions.XslNs + "value-of",
                                    new XAttribute("select", string.Format("reporting:root/reporting:{0}/reporting:{0}Result/reporting:{1}", sourceName, sourceColName.ToUpper()))));
                            }
                        }
                        //else // variable take the value of a SQL source's column
                        //{
                            //When you find the tag in all SQL source column by the same name
                            //foreach (var oneRowSqlSource in arrOneRowSqlSources)
                            //{
                            //    variableElem.Add(new XElement(ReportingExtensions.XslNs + "choose",
                            //        new XElement(ReportingExtensions.XslNs + "when", new XAttribute("test",
                            //                string.Format("count(reporting:root/reporting:{0}/reporting:{0}Result) = 1 and reporting:root/reporting:{0}/reporting:{0}Result/reporting:{1}", oneRowSqlSource, variableName)),
                            //            new XElement(ReportingExtensions.XslNs + "value-of", new XAttribute("select", string.Format("reporting:root/reporting:{0}/reporting:{0}Result/reporting:{1}", oneRowSqlSource, variableName))))));
                            //}
                        //}
                    }
                    else
                    {
                        EmcLog.Error("There is something wrong with the variable declaration in XSLT");
                    }
                });
                doc.Save(xsltFile);
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                throw;
            }
            EmcLog.Debug("END");
        }

        #endregion
    }
}

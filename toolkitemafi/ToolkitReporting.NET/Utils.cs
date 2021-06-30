using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using DevExpress.XtraGrid.Views.Base;
using Sophis.Reporting.Controls.CommonTypes;
using sophis.reporting;
using sophis.misc;
using Eff.Utils;
using Sophis.Reporting.Controls;
using TSQL;
using TSQL.Statements;
using TSQL.Tokens;

// ReSharper disable once CheckNamespace
namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public static class Utils
        {
            public const string SectionName = "EMC";
            public const string ReportingAskParamWindowTitle = "Reporting Parameter";
            public const string SourceTypeSql = "SQL";
            public const string SourceTypePortfolio = "Portfolio";
            public const string SourceTypeTrade = "Trade";

            public const string ParameterTypeString = "String";
            public const string ParameterTypeBoolean = "Boolean";
            public const string ParameterTypeDate = "Date";
            public const string ParameterTypeFolio = "Folio";
            public const string ParameterTypeInstrument = "Instrument";
            public const string ParameterTypeInteger = "Integer";
            public const string ParameterTypeDecimal = "Decimal";
            public const string ParameterTypeThirdParty = "ThirdParty";

            public const string ParametersSourceName = "parametersSource";

            public const string SourceSqlFldQuery = "fSqlRequest";
            public const string SourcePortfolioFldColumnSet = "Column Set";
            public const string SourcePortfolioFldRowToCol = "RowToCol";
            public const string SourceTradeFldExpCols = "Exported Columns";
            public const string SourceTradeFldRowToCol = "RowToCol";

            public const string SettingFormat = "Format";
            public const string SettingFormula = "Formula";
            public const string SettingDefault = "Default";

            public static readonly string[] Tokens = { "%DATE_FORMAT%", "%WORKING_DIRECTORY%", "%USERNAME%" };
            public static readonly string[][] TokensDateTime = {
                new[] {"%YYYY%", "yyyy"}, new[] {"%YY%", "yy"}, new[] {"%MM%", "MM"}, new[] {"%DD%", "dd"},
                new[] {"%HH%", "HH"}, new[] {"%MnMn%", "mm"}, new[] {"%SS%", "ss"},
                new[] {"%START_DATE%", "yyyy-MM-dd"}, new[] {"%START_TIME%", "HH-mm"},
                new[] {"%START_HOUR%", "HH"}, new[] {"%START_MINUTE%", "mm"}, new[] {"%START_SECOND%", "ss"} };
            public static IDictionary<string, string> TokenValues = new Dictionary<string, string>();
            public static string TokenDateFormat;

            public static void Init()
            {
                using (var reportTemplate = CSMReportTemplateManager.GetInstance().GetReportTemplateList()[0])
                {
                    CSMTokenManager.GetInstance().Evaluate("%DATE_FORMAT%", out TokenDateFormat, reportTemplate);
                    foreach (var token in Tokens)
                    {
                        string tokenVal;
                        CSMTokenManager.GetInstance().Evaluate(token, out tokenVal, reportTemplate);
                        TokenValues.Add(token, tokenVal);
                    }
                }
            }

            public static CSMAskParameters GetAskParametersDlg()
            {
                var windowHandle = WindowApi.FindWindow(default(string), ReportingAskParamWindowTitle);
                var csmAskParameters = Control.FromHandle(windowHandle) as CSMAskParameters;
                return csmAskParameters;
            }

            public static CSMXMLTransformation CreateStyleSheetTransformation(string xslFile, string outputFile)
            {
                var result = CSMXMLTransformation.CreateInstance("Style Sheet");
                for (var i = 0; i < result.GetParameterCount(); i++)
                {
                    var parameter = result.GetNthParameter(i);
                    switch (parameter.GetTitle())
                    {
                        case "Name":
                            parameter.SetValue("EMC ToExcel"); //0
                            break;
                        case "Input File":
                            parameter.SetValue("=DEFAULT_XML"); //1
                            break;
                        case "XSL File":
                            parameter.SetValue(xslFile); //2
                            break;
                        case "Output File":
                            parameter.SetValue(outputFile); //3
                            break;
                    }
                }

                return result;
            }

            public static bool StopAskParam = false;
            private static CSMAskParameters _currentAskParameters;
            public static readonly string[] DefaultValueParamTypes = { "String", "Integer" };//, "Decimal", "Boolean", "Date"
            public const string AskParametersSleep = "AskParametersSleep";
            public static void ScanAskParameters()
            {
                EmcLog.Debug("BEGIN");
                Thread.CurrentThread.IsBackground = true;
                var askParamsSleep = 100;
                CSMConfigurationFile.getEntryValue(SectionName, AskParametersSleep, ref askParamsSleep, 100);
                while (!StopAskParam)
                {
                    var csmAskParameters = GetAskParametersDlg();
                    if (csmAskParameters != null && !csmAskParameters.Equals(_currentAskParameters))
                    {
                        _currentAskParameters = csmAskParameters;
                        var arrParams = _currentAskParameters.GetParameters();
                        foreach (var csmGridParameter in arrParams)
                        {
                            if (csmGridParameter.Value == string.Empty
                                && DefaultValueParamTypes.Contains(csmGridParameter.Type))
                            {
                                csmGridParameter.Value = csmGridParameter.ApiParameter.GetSettingDefault();
                                //csmAskParameters.Invoke((MethodInvoker)(() =>
                                //{
                                //    csmGridParameter.Value = defaultValue;
                                //}));
                            }
                        }

                        //hide system parameters (which has name starts with @)
                        var gridView =
                            ((DevExpress.XtraGrid.GridControl) ((SplitContainer) _currentAskParameters.Controls[0])
                                .Panel1.Controls[0]).MainView as DevExpress.XtraGrid.Views.Grid.GridView;
                        if (gridView != null)
                            gridView.CustomRowFilter += (sender, args) =>
                            {
                                var colView = sender as ColumnView;
                                if (colView == null) return;
                                var paramName = colView.GetListSourceRowCellValue(args.ListSourceRow, colView.Columns[0]).ToString();
                                //if (arrParams[args.ListSourceRow].Name.IsSystemParameter())
                                if (paramName.IsSystemParameter())
                                {
                                    args.Visible = false;
                                    args.Handled = true;
                                }
                            };
                        _currentAskParameters.RefreshGridView();
                        //Application.DoEvents();
                    }
                    Thread.Sleep(askParamsSleep);
                }
                EmcLog.Debug("END");
            }

            #region Report Api extensions

            public static string GetSetting(this CSMParameter param, string setting)
            {
                //var value = string.Empty;
                //param.GetSettingParameters().GetParameter(setting, ref value);
                string value;
                param.GetSettingParameters().GetParameter(setting, out value);
                return value;
            }

            public static string GetSettingFormat(this CSMParameter param)
            {
                return param.GetSetting(SettingFormat);
            }

            public static string GetSettingDefault(this CSMParameter param)
            {
                return param.GetSetting(SettingDefault);
            }

            public static string GetSettingFormula(this CSMParameter param)
            {
                return param.GetSetting(SettingFormula);
            }

            public static CSMGridParameter[] GetParameters(this CSMAskParameters csmAskParameters)
            {
                var result = new CSMGridParameter[] {};
                var gridControl =
                    csmAskParameters.Controls[0].Controls[0].Controls[0] as DevExpress.XtraGrid.GridControl;
                if (gridControl == null) return result;
                result = gridControl.DataSource as CSMGridParameter[];
                return result;
            }

            /// <summary>
            /// This method replaces CSMXMLTransformation.Clone() which returns null exception reference
            /// </summary>
            /// <param name="transformation"></param>
            /// <returns></returns>
            public static CSMXMLTransformation CloneTransformation(this CSMXMLTransformation transformation)
            {
                var result = CSMXMLTransformation.CreateInstance(transformation.GetTypeOf());
                for (int i = 0; i < transformation.GetParameterCount(); i++)
                {
                    result.GetNthParameter(i).SetValue(transformation.GetNthParameter(i).GetValue());
                }
                return result;
            }

            private static int GetFieldIdxByName(this CSMXMLSource xmlSource, string fieldName)
            {
                var result = -1;
                for (var i = 0; i < xmlSource.GetFieldCount(); i++)
                {
                    if (!xmlSource.GetFieldName(i).Equals(fieldName)) continue;
                    result = i;
                    break;
                }
                return result;
            }

            public static string GetFieldValueByName(this CSMXMLSource xmlSource, string fieldName)
            {
                var fieldIdx = xmlSource.GetFieldIdxByName(fieldName);
                var result = fieldIdx < 0 ? null : xmlSource.GetFieldValue(fieldIdx);
                return result;
            }

            public static void SetFieldValueByName(this CSMXMLSource xmlSource, string fieldName, string fieldValue)
            {
                var fieldIdx = xmlSource.GetFieldIdxByName(fieldName);
                if (fieldIdx < 0)
                {
                    throw new Exception(string.Format("Field not found in XMLSource: {0}", fieldValue));
                }
                xmlSource.SetFieldValue(fieldIdx, fieldValue);
            }

            public static string[] GetSourceColumns(this CSMXMLSource xmlSource)
            {
                string[] result;
                string fieldValue;
                switch (xmlSource.SourceType)
                {
                    case SourceTypePortfolio:
                        fieldValue = xmlSource.GetFieldValueByName(SourcePortfolioFldColumnSet);
                        result = fieldValue.Split('|').Where(x => x != string.Empty).ToArray();
                        break;
                    case SourceTypeTrade:
                        fieldValue = xmlSource.GetFieldValueByName(SourceTradeFldExpCols);
                        result = fieldValue.Split(',').Where(x => x != string.Empty).ToArray();
                        break;
                    case SourceTypeSql:
                        var sqlQuery = xmlSource.GetFieldValueByName(SourceSqlFldQuery);
                        var reg = new Regex(@"(^|.*[^\[])\[\[([a-zA-Z0-9_^:]*):([a-zA-Z0-9_^:]*)\]\]([^\]].*|$)"); //match "[[param:defaultvalue]
                        var unParameterizedQuery = sqlQuery;
                        var m = reg.Match(unParameterizedQuery);
                        while (m.Success)
                        {
                            unParameterizedQuery = reg.Replace(unParameterizedQuery, "$1$4");
                            m = reg.Match(unParameterizedQuery);
                        }

                        var tSqlStatements = TSQLStatementReader.ParseStatements(unParameterizedQuery);
                        var selectStatement = tSqlStatements.First(x => x is TSQLSelectStatement);
                        var columnList = new List<string>();
                        for (var i = 0; i < selectStatement.Tokens.Count; i++)
                        {
                            if (selectStatement.Tokens[i].Type == TSQLTokenType.Identifier && i < selectStatement.Tokens.Count - 1)
                            {
                                //test if next token is Comma or From
                                if ((selectStatement.Tokens[i + 1].Type == TSQLTokenType.Character &&
                                    selectStatement.Tokens[i + 1].AsCharacter.Character == TSQLCharacters.Comma) ||
                                    (selectStatement.Tokens[i + 1].Type == TSQLTokenType.Keyword &&
                                     selectStatement.Tokens[i + 1].AsKeyword.Keyword == TSQLKeywords.FROM))
                                {
                                    columnList.Add(selectStatement.Tokens[i].AsIdentifier.Name);
                                }
                            }
                        }
                        //foreach (var token in selectStatement.Tokens)
                        //{
                        //    EmcLog.Debug("type: {0}, value: {1}", token.Type.ToString(), token.Text);
                        //}

                        result = columnList.ToArray();
                        break;
                    default:
                        throw new Exception("Invalid XMLSource type");
                }
                return result;
            }

            public static string GetSourceSqlQuery(this CSMXMLSource xmlSource)
            {
                if (xmlSource.SourceType != SourceTypeSql)
                {
                    throw new Exception("Invalid XMLSource type");
                }
                var fieldValue = xmlSource.GetFieldValueByName(SourceSqlFldQuery);
                var result = fieldValue ?? string.Empty;
                return result;
            }

            public static void SetSourceSqlQuery(this CSMXMLSource xmlSource, string query)
            {
                if (xmlSource.SourceType != SourceTypeSql)
                {
                    throw new Exception("Invalid XMLSource type");
                }
                xmlSource.SetFieldValueByName(SourceSqlFldQuery, query);
            }

            public static string ReplaceTokens(this string inputStr)
            {
                inputStr = Tokens.Aggregate(inputStr, (current, token) => current.Replace(token, TokenValues[token]));
                var reportDate = DateTime.Now;
                inputStr = TokensDateTime.Aggregate(inputStr, (current, token) => current.Replace(token[0], reportDate.ToString(token[1])));
                return inputStr;
            }

            public static bool IsSystemParameter(this string paramName)
            {
                return paramName.StartsWith("@");
            }

            #endregion

            #region CSMReportTemplate extensions

            public static CSMXMLSource[] GetXmlSourceListByType(this CSMReportTemplate reportTemplate, string sourceKind)
            {
                var result = new CSMXMLSource[] { };
                try
                {
                    var sourcesList = reportTemplate.GetXMLSourceList(); // may cause NullPointerException
                    result = sourcesList.Where(x => x.SourceType == sourceKind).ToArray();
                    //this method may cause CRASH, so do the GetXMLSourceList first, that may returns in NullPointerException
                    //result = reportTemplate.GetXMLSourceListWithType(sourceKind);
                }
                catch (Exception e)
                {
                    EmcLog.Debug("reportTemplate = {0}", reportTemplate.Name);
                    EmcLog.Warning(e.ToString());
                }
                return result;
            }

            public static CSMXMLSource[] GetXmlSourceListByTypeSql(this CSMReportTemplate reportTemplate)
            {
                return reportTemplate.GetXmlSourceListByType(SourceTypeSql);
            }

            public static CSMXMLSource[] GetXmlSourceListByTypePortfolio(this CSMReportTemplate reportTemplate)
            {
                return reportTemplate.GetXmlSourceListByType(SourceTypePortfolio);
            }

            public static CSMXMLSource[] GetXmlSourceListByTypeTrade(this CSMReportTemplate reportTemplate)
            {
                return reportTemplate.GetXmlSourceListByType(SourceTypeTrade);
            }

            public static CSMParameter GetParameterWithNameSafely(this CSMReportTemplate reportTemplate, string paramName)
            {
                foreach (CSMParameter param in reportTemplate.GetParametersList())
                {
                    if (param.GetName() == paramName)
                    {
                        return param;
                    }
                }
                //if not found, create a new parameter
                EmcLog.Debug("Parameter {0} not found in report {1}. Create a new one.", paramName, reportTemplate.Name);
                var newParam = new CSMParameter(paramName, string.Empty);
                reportTemplate.AddParameter(newParam);
                return newParam;
            }

            public static CSMXMLSource GetXMLSourceWithNameNullPossible(this CSMReportTemplate reportTemplate, string sourceName)
            {
                CSMXMLSource result = null;
                try
                {
                    result = reportTemplate.GetXMLSourceWithName(ParametersSourceName);
                }
                catch (Exception e)
                {
                    EmcLog.Debug("reportTemplate={0}, sourceName={1}", reportTemplate.Name, sourceName);
                    EmcLog.Warning(e.ToString());
                }

                return result;
            }

            public static void SaveToDatabaseSafely(this CSMReportTemplate reportTemplate)
            {
                reportTemplate.GetXmlSourceListByTypeSql();//It's a trick to not lose the Source after saving parameters
                reportTemplate.SaveToDatabase();
            }

            public static void GenerateParametersSource(this CSMReportTemplate reportTemplate)
            {
                reportTemplate.DeleteXMLSource(ParametersSourceName);
                var parametersSource = reportTemplate.CreateXMLSource(ParametersSourceName, SourceTypeSql);
                var sqlQuery = string.Empty;
                foreach (CSMParameter param in reportTemplate.GetParametersList())
                {
                    var paramName = param.GetName();
                    if (paramName.IsSystemParameter()) continue;
                    var fieldAdded = true;
                    switch (param.GetType())
                    {
                        case ParameterTypeString:
                            sqlQuery = string.Format("{0}, '[[{1}:]]' {1}", sqlQuery, paramName);
                            break;
                        case ParameterTypeDate:
                            sqlQuery = string.Format("{0}, TO_CHAR(TO_DATE('[[{1}:]]', '%DATE_FORMAT%'), 'DD/MM/YYYY') {1}_DDMMYYYY", sqlQuery, paramName);
                            sqlQuery = string.Format("{0}, TO_CHAR(TO_DATE('[[{1}:]]', '%DATE_FORMAT%'), 'MM/DD/YYYY') {1}_MMDDYYYY", sqlQuery, paramName);
                            break;
                        case ParameterTypeBoolean:
                        case ParameterTypeFolio:
                        case ParameterTypeInstrument:
                        case ParameterTypeInteger:
                        case ParameterTypeDecimal:
                        case ParameterTypeThirdParty:
                            sqlQuery = string.Format("{0}, [[{1}:]] {1}", sqlQuery, paramName);
                            break;
                        default:
                            fieldAdded = false;
                            EmcLog.Warning("Parameter type {0} is not supported.", paramName);
                            break;
                    }
                    if (fieldAdded)
                    {
                        parametersSource.GetBindings().Add(string.Format("[[{0}]]", paramName), paramName);
                    }
                }
                if (sqlQuery == string.Empty) return;
                sqlQuery = string.Format("SELECT {0} FROM DUAL", sqlQuery.Substring(2));
                parametersSource.SetSourceSqlQuery(sqlQuery);
                reportTemplate.SaveToDatabaseSafely();
            }

            public static bool RemoveXmlTransformation(this CSMReportTemplate reportTemplate, CSMXMLTransformation transformation)
            {
                var result = false;
                var lstXmlTransformations = new List<CSMXMLTransformation>();
                for (var i = 0; i < reportTemplate.GetXMLTransformationCount(); i++)
                {
                    var nthTransformation = reportTemplate.GetNthXMLTransformation(i);
                    if (nthTransformation.GetNameParameter().GetValue().Equals(transformation.GetNameParameter().GetValue()) &&
                        nthTransformation.GetTypeOf().Equals(transformation.GetTypeOf()))
                    {
                        result = true;
                    }
                    else
                    {
                        lstXmlTransformations.Add(nthTransformation.CloneTransformation());
                    }
                }

                if (result)
                {
                    reportTemplate.ClearXMLTransformations();
                    lstXmlTransformations.ForEach(reportTemplate.AddXMLTransformation);
                }
                return result;
            }

            #endregion
        }
    }
}
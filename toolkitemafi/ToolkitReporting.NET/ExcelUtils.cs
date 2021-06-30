using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Eff.Utils;

// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET
{
    public static class ExcelUtils
    {
        public static string[] GetWorkSheets(string excelFilePath)
        {
            try
            {
                excelFilePath = excelFilePath.ReplaceTokens();
                var doc = XDocument.Load(excelFilePath);
                doc.ReadNamespaces();
                var result = doc.Descendants(ReportingExtensions.SsNs + "Worksheet")
                    .Attributes(ReportingExtensions.SsNs + "Name").Select(x => x.Value).ToArray();
                return result;
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                return new string[] { };
            }
        }

        public static string[] GetParamTags(string excelFilePath)
        {
            try
            {
                var reg = new Regex(@"(^|.*[^\{])(\{[a-zA-Z0-9_]*\})([^\}].*|$)"); //match "{Param1} and {Param2}" not "{{Param}}"
                excelFilePath = excelFilePath.ReplaceTokens();
                var doc = XDocument.Load(excelFilePath);
                doc.ReadNamespaces();
                var paramElements = doc.Descendants(ReportingExtensions.SsNs + "Data").Where(x => reg.IsMatch(x.Value)).ToList();
                var arrVariables = new List<string>();
                foreach (var paramDataElem in paramElements)
                {
                    var paramDataValue = paramDataElem.Value;
                    var m = reg.Match(paramDataValue);
                    while (m.Success)
                    {
                        var variableName = m.Groups[2].Value.ToUpper();
                        if (!arrVariables.Contains(variableName))
                        {
                            arrVariables.Add(variableName);
                        }
                        variableName = variableName.Substring(1, variableName.Length - 2);
                        paramDataValue = reg.Replace(paramDataValue, string.Format("$1', ${0}, '$3", variableName));
                        m = reg.Match(paramDataValue);
                    }
                }
                return arrVariables.ToArray();
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                return new string[] { };
            }
        }

        public static string[] GetTableTags(string excelFilePath, string worksheetName)
        {
            try
            {
                var reg = new Regex(@"^\{\{[a-zA-Z0-9_]*\}\}$");//{{TableTag}}
                excelFilePath = excelFilePath.ReplaceTokens();
                var doc = XDocument.Load(excelFilePath);
                doc.ReadNamespaces();
                var worksheetAttr = doc.Descendants(ReportingExtensions.SsNs + "Worksheet")
                    .Attributes(ReportingExtensions.SsNs + "Name").FirstOrDefault(x => x.Value == worksheetName);
                var worksheetElem = worksheetAttr == null ? null : worksheetAttr.Parent;
                //if worksheet is not defined or not found --> find in the whole document
                var elems = worksheetElem == null ?
                    doc.Descendants(ReportingExtensions.SsNs + "Worksheet").Descendants(ReportingExtensions.SsNs + "Data") : worksheetElem.Descendants(ReportingExtensions.SsNs + "Data");
                //var allWorksheetElems = doc.Descendants().Where(x => x.Name.LocalName == "Worksheet")
                //    .Select(x => x.Descendants()).SelectMany(x => x); //all elements of all worksheets
                //if worksheet is not defined or not found --> find in the whole document
                //var elems = worksheetElem == null ? allWorksheetElems : worksheetElem.Descendants();
                var result = elems.Where(x => reg.IsMatch(x.Value)).Select(x => x.Value).ToArray();
                return result;
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                return new string[] { };
            }
        }
    }
}

using System.Collections.Generic;
using Eff.ToolkitReporting.NET.Models;

// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET.Interfaces
{
    public interface IReportService
    {
        ICollection<Report> GetReports();
        ICollection<ReportParameter> GetParametersByReportName(string reportName);
        ICollection<ReportSetting> GetSettingsByReportName(string reportName);
        void SaveSettings(string reportName, ICollection<ReportSetting> reportSettings);
        void GenerateXslt(string reportName, string outputXsltFile);
        void GenerateReport(string reportName, ICollection<ReportParameter> reportParameters, string outputFile);
    }
}

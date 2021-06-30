// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET.Models
{
    public enum ParamType
    {
        Date,
        String
    }

    public class ReportParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public ParamType Type { get; set; }
        public string Format { get; set; }
    }
}

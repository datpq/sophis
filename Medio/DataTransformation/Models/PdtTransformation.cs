namespace DataTransformation.Models
{
    public enum TransType
    {
        Csv2Xml,
        Csv2Csv,
        Xml2Csv
    } 

    public class PdtTransformation
    {
        public string name { get; set; }
        public TransType type { get; set; }
        public string label { get; set; }
        public string category { get; set; }
        public string templateFile { get; set; }
        public string repeatingRootPath { get; set; }
        public string repeatingChildrenPath { get; set; }
        public int csvSkipLines { get; set; }
        public char csvSrcSeparator { get; set; } // empty --> fixed length file
        public char csvDestSeparator { get; set; }
        public int bunchSize { get; set; }
        public string[] uniqueConstraints { get; set; }
        public string[] checkConstraints { get; set; }
        public string processingCondition { get; set; }
        public PdtVariable[] variables { get; set; }
        public string[] groupBy { get; set; }

        public PdtColumn[] columns { get; set; }
    }
}

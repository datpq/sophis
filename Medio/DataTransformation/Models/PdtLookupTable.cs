namespace DataTransformation.Models
{
    public class PdtLookupTable
    {
        public string Name { get; set; }
        public string File { get; set; }
        public int Expires { get; set; }
        public char csvSeparator { get; set; }
        public string processingCondition { get; set; }
        public string keyExpression { get; set; }
        public string[] columnsExpression { get; set; }
    }
}

namespace DataTransformation.Models
{
    public class PdtLookupTable
    {
        public string Name { get; set; }
        public string File { get; set; }
        public string keyExpression { get; set; }
        public string[] columnsExpression { get; set; }
    }
}

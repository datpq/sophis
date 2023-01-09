namespace DataTransformation.Models
{
    public class PdtVariable
    {
        public string name { get; set; }
        public string expressionBefore { get; set; }
        public string expressionAfter { get; set; }
        public string expressionStorage { get; set; }
        public PdtColumnLookup Lookup { get; set; }
    }
}

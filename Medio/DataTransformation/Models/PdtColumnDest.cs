namespace DataTransformation.Models
{
    public class PdtColumnDest
    {
        public string path { get; set; }
        public string expression { get; set; }
        public string processingCondition { get; set; }
        public PdtColumnLookup Lookup { get; set; }
        public string aggregation { get; set; }
    }
}

using System;

namespace DataTransformation.Models
{
    public class PdtColumnLookup
    {
        public string File { get; set; }
        public string Expression { get; set; }
        public string Table { get; set; }
        public string ColumnIndex { get; set; }
        public PdtColumnLookup Lookup { get; set; }
    }
}

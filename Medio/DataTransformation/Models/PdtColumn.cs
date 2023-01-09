namespace DataTransformation.Models
{
    public class PdtColumn
    {
        public string name { get; set; }
        public int len { get; set; }
        public bool isRequired { get; set; }
        public bool isRelativeToRootNode { get; set; }
        public PdtColumnDest[] destPaths { get; set; }
    }
}

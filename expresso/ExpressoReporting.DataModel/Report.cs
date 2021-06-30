using System.Collections.Generic;

namespace ExpressoReporting.DataModel
{
    public class Report
    {
        public string Name { get; set; }
        public IEnumerable<Parameter> Parameters { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTT.Models
{
    public class TttColumn
    {
        public string name { get; set; }
        public bool isRequired { get; set; }
        public bool isRelativeToRootNode { get; set; }
        public string[] destPaths { get; set; }
    }
}

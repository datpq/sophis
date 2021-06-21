using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTT.Models
{
    public class TttTransformation
    {
        public string name { get; set; }
        public string label { get; set; }
        public string category { get; set; }
        public string templateFile { get; set; }
        public string repeatingRootPath { get; set; }
        public string repeatingChildrenPath { get; set; }

        public TttColumn[] columns { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEDIO.BackOffice.net.src.Allotment
{
    public class CSxAllotConditionModel
    {
        public string ConditionName { get; set; }
        public string InstrumentType { get; set; }
        public string InstrumentProperty { get; set; }
        public string ConditionValues { get; set; }

        public List<string> GetConditionValueList()
        {
            char[] delimiters = new [] { ',', ';' };  // List of your delimiters
            var splittedList = ConditionValues.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();
            return splittedList;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MEDIO.OrderAutomation.net.Source.DataModel;

namespace MEDIO.OrderAutomation.NET.Source.DataModel
{
    public class CSxExecutionAllocationArgs : EventArgs
    {
        public CSxExecutionAllocationArgs() { }
        public CSxExecutionAllocationArgs(IEnumerable<CSxCustomAllocation> execList)
        {
            ExecList = execList;
        }

        public CSxExecutionAllocationArgs(IEnumerable<CSxCustomAllocation> execList, bool isNewItem)
        {
            ExecList = execList;
            IsNewItem = isNewItem;
        }

        public IEnumerable<CSxCustomAllocation> ExecList { get; set; }

        public bool IsNewItem { get; set; }

        public int OrderCount { get; set; }
    }


}

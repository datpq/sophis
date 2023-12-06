using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public class CSxAccountDataModel
    {
        public string AccountName { get; set; }
       // public string AccountId { get; set; }
        public int Threshold { get; set; }

        public CSxAccountDataModel()
        {
            this.AccountName = "";
          //  this.AccountId = "";
            this.Threshold = 0; 
        }
    }
}

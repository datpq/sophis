using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEDIO.FXCompliance.net
{
    class Utils
    {
        //this is need because Medio beside normal currencies uses small currencies like GBp (GBP cents) and portfolio column names are not case sensitive
        public static string GetCCYName(string sCCY)
        {
            if (sCCY.ToUpper() != sCCY)
                return sCCY + "cents";
            return sCCY;
        }
    }
}

using MEDIO.OrderAutomation.net.Source.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace MEDIO.OrderAutomation.NET.Source.DataModel
{
    public class FxAutoMarket
    {
        public string Code { get; set; }

        [ReadOnly(true)]
        [DisplayName("Market")]
        public string Name { get; set; }

        public bool Active { get; set; }

        public static ICollection<FxAutoMarket> GetData(bool activeOnly = false)
        {
            var whereClause = activeOnly ? "WHERE ACTIVE = 1" : string.Empty;
            var sqlQuery = $"SELECT * FROM MEDIO_FXAUTO_MARKETS {whereClause}";
            return sqlQuery.BindFromQuery((IDataReader r) => 
                new FxAutoMarket
                {
                    Code = r["CODE"].ToString(),
                    Name = r["NAME"].ToString(),
                    Active = Convert.ToInt32(r["ACTIVE"]) == 1
                });
        }
    }
}

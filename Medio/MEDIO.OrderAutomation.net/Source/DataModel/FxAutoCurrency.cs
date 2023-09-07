using MEDIO.OrderAutomation.net.Source.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace MEDIO.OrderAutomation.NET.Source.DataModel
{
    public class FxAutoCurrency
    {
        public string Market { get; set; }

        [ReadOnly(true)]
        [DisplayName("Currency")]
        public string Code { get; set; }

        public bool Active { get; set; }

        public static ICollection<FxAutoCurrency> GetData(string market, bool activeOnly = false)
        {
            var whereClause = activeOnly ? "AND C.CODE IS NOT NULL" : string.Empty;
            var sqlQuery = $@"
SELECT DEVISE_TO_STR(D.CODE) CURRENCY, CASE WHEN C.CODE IS NULL THEN 0 ELSE 1 END ACTIVE
FROM DEVISEV2 D
    LEFT JOIN MEDIO_FXAUTO_CURRENCIES C ON C.CODE = DEVISE_TO_STR(D.CODE) AND C.MARKET = '{market}'
WHERE NOT REGEXP_LIKE(DEVISE_TO_STR(D.CODE), '^[[:digit:]]+$') {whereClause}
ORDER BY 2 DESC, 1";
            return sqlQuery.BindFromQuery((IDataReader r) => 
                new FxAutoCurrency
                {
                    Market = market,
                    Code = r["CURRENCY"].ToString(),
                    Active = Convert.ToInt32(r["ACTIVE"]) == 1
                });
        }

        public static ICollection<string> GetCurrencies()
        {
            return "SELECT DEVISE_TO_STR(CODE) CURRENCY FROM DEVISEV2".BindFromQuery((IDataReader r) => r["CURRENCY"].ToString());
        }
    }
}

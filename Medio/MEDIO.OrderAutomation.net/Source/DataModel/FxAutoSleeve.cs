using System;
using MEDIO.OrderAutomation.net.Source.Tools;
using System.ComponentModel;
using System.Data;
using System.Collections.Generic;

namespace MEDIO.OrderAutomation.NET.Source.DataModel
{
    public class FxAutoSleeve
    {
        public int FundId { get; set; }

        [ReadOnly(true)]
        public string FundName { get; set; }

        public int Id { get; set; }

        [ReadOnly(true)]
        public string Name { get; set; }

        [ReadOnly(true)]
        public string Strategy { get; set; }

        public bool Active { get; set; }

        public static ICollection<FxAutoSleeve> GetData()
        {
            var sqlQuery = @"
SELECT DISTINCT P.FUND_ID, F.NAME FUND, FO.IDENT, FO.NAME SLEEVE, S.NAME STRATEGY,
    CASE WHEN SL.IDENT IS NULL THEN 0 ELSE 1 END ACTIVE
FROM PFR_MODEL_LINK P
    JOIN FUNDS F ON F.SICOVAM = P.FUND_ID
    JOIN FOLIO FO ON FO.IDENT = P.FOLIO
    JOIN AM_STRATEGY S ON S.ID = P.STRATEGY
    LEFT JOIN MEDIO_FXAUTO_SLEEVES SL ON SL.IDENT = FO.IDENT
ORDER BY 6 DESC, 2, 4";
            return sqlQuery.BindFromQuery((IDataReader r) => 
                new FxAutoSleeve
                {
                    FundId = Convert.ToInt32(r["FUND_ID"]),
                    FundName = r["FUND"].ToString(),
                    Id = Convert.ToInt32(r["IDENT"]),
                    Name = r["SLEEVE"].ToString(),
                    Strategy = r["STRATEGY"].ToString(),
                    Active = Convert.ToInt32(r["ACTIVE"]) == 1
                });
        }
    }
}

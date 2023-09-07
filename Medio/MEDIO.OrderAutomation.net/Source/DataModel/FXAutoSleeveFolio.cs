using System;
using MEDIO.OrderAutomation.net.Source.Tools;
using System.ComponentModel;
using System.Data;
using System.Collections.Generic;

namespace MEDIO.OrderAutomation.NET.Source.DataModel
{
    public class FXAutoSleeveFolio
    {
        public int ID { get; set; }

        public int ParentID { get; set; }

        [ReadOnly(true)]
        public string Name { get; set; }

        public bool Active { get; set; }

        public static ICollection<FXAutoSleeveFolio> GetData(bool activeOnly = false)
        {
            var whereClause = activeOnly ? "SELECT IDENT FROM MEDIO_FXAUTO_SLEEVES" : "SELECT FOLIO FROM PFR_MODEL_LINK";
            var sqlQuery = $@"
SELECT NAME, ID, PARENTID, CASE WHEN SL.IDENT IS NULL THEN 0 ELSE 1 END ACTIVE
FROM (SELECT DISTINCT CONNECT_BY_ROOT FO.NAME NAME, CONNECT_BY_ROOT FO.IDENT ID, CONNECT_BY_ROOT FO.MGR PARENTID
    FROM FOLIO FO
    WHERE FO.IDENT IN ({whereClause})
    CONNECT BY PRIOR FO.IDENT = FO.MGR) T
    LEFT JOIN MEDIO_FXAUTO_SLEEVES SL ON SL.IDENT = T.ID
ORDER BY CASE WHEN SL.IDENT IS NULL THEN 0 ELSE 1 END DESC";
            return sqlQuery.BindFromQuery((IDataReader r) => 
                new FXAutoSleeveFolio
                {
                    ID = Convert.ToInt32(r["ID"]),
                    Name = r["NAME"].ToString(),
                    ParentID = r["PARENTID"] == DBNull.Value ? 0 : Convert.ToInt32(r["PARENTID"]),
                    Active = Convert.ToInt32(r["ACTIVE"]) == 1
                });
        }

        public static ICollection<FXAutoSleeveFolio> GetDataByTradeDate(DateTime date)
        {
            CSxUtils.InitSleeveIDsByDate(date);
            var sqlQuery = @"
SELECT NAME, ID, PARENTID, 1 ACTIVE
FROM (SELECT DISTINCT CONNECT_BY_ROOT FO.NAME NAME, CONNECT_BY_ROOT FO.IDENT ID, CONNECT_BY_ROOT FO.MGR PARENTID
    FROM FOLIO FO
    WHERE FO.IDENT IN (SELECT VALNUM FROM MEDIO_VALUES)
    CONNECT BY PRIOR FO.IDENT = FO.MGR) T";
            return sqlQuery.BindFromQuery((IDataReader r) =>
                new FXAutoSleeveFolio
                {
                    ID = Convert.ToInt32(r["ID"]),
                    Name = r["NAME"].ToString(),
                    ParentID = r["PARENTID"] == DBNull.Value ? 0 : Convert.ToInt32(r["PARENTID"]),
                    Active = true
                });
        }
    }
}

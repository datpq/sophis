using System.Collections.Generic;
using Dapper;
using System.Linq;
using TkoPortfolioColumn.ConnectionProvider;
using System.Data.SqlClient;
using System;

namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
        public static class DbrNextCallDate
        {
            public class InstrumentToUpdate
            {
                public string reference    { get; set; }
                public int    sicovam      { get; set; }
                public string libelle      { get; set; }
            }

            public class InstrumentToWithNextCallDate
            {
                public string reference { get; set; }
                public int sicovam { get; set; }
                public string  NXT_CALL_DT_BBG { get; set; }
                public string NXT_CALL_DT_SOPHIS { get; set; }
            }

            public static List<InstrumentToUpdate> InstrumentList = new List<InstrumentToUpdate>();
            public static List<InstrumentToWithNextCallDate> InstrumentListNextCallDate = new List<InstrumentToWithNextCallDate>();

            private static string _QueryAmericanBondClauseRetrieve = @"select reference,sicovam,libelle from titres t  where t.type = 'O'  
                                                   and exists  (select 1 from clause C where C.sicovam = T.sicovam and C.TYPE = 2 and C.com1 = 'AMERICAN')
                                                   and exists  (select 1 from JOIN_POSITION_HISTOMVTS h where h.type in (select ID  from BUSINESS_EVENTS S where compta = 1 ) and h.sicovam = t.sicovam 
                                                   and H.opcvm in (select ident from folio start with ident in ('16641') connect by mgr = prior ident)
                                                   group by h.mvtident having sum(h.QUANTITE)>0)";

            private static string _QueryBloombergNextCallDate = @"select T.sicovam, T.reference,  
                                                                  to_char(least(min(decode(sign(DATEDEB-trunc(sysdate)),-1,DATEFIN,DATEDEB)), min(DATEFIN)),'YYYY/MM/DD') NXT_CALL_DT_SOPHIS, 
                                                                  to_char(num_to_date(h.TKO_NXT_CALL_DT), 'YYYY/MM/DD') NXT_CALL_DT_BBG 
                                                                  from titres T
                                                                  LEFT JOIN clause C ON T.sicovam = C.sicovam and C.TYPE = 2 and (C.DATEDEB >= trunc(sysdate) OR C.DATEFIN >= trunc(sysdate) ) and C.com1 like 'AMERICAN'
                                                                  JOIN JOIN_POSITION_HISTOMVTS h on h.sicovam = T.sicovam and h.jour = trunc(sysdate-1 )
                                                                  where  exists  
                                                                  (---Positions ouvertes
                                                                                select 1 from JOIN_POSITION_HISTOMVTS h where h.type in (select ID  from BUSINESS_EVENTS S where compta = 1 ) and h.sicovam = t.sicovam 
                                                                                and H.opcvm in (select ident from folio start with ident in ('16641') connect by mgr = prior ident)--Choix de Folio (Exp OPEN-ENDED STRATEGIES)
                                                                                group by h.mvtident having sum(h.QUANTITE)>0
                                                                    )
                                                                    group by T.sicovam, T.reference,h.TKO_NXT_CALL_DT having  least(min(decode(sign(DATEDEB-trunc(sysdate)),-1,DATEFIN,DATEDEB)), min(DATEFIN)) != num_to_date(h.TKO_NXT_CALL_DT)";

            private static string _QueryBloombergNextCallDateView = "SELECT * FROM TKO_SYNCHRO_NEXTCALLDATE_BBG";

            //private static string _QueryUpdateClauseDate = @"UPDATE CLAUSE SET DATEDEB = :datedeb , DATEFIN = :datefin, COM1= :comment WHERE SICOVAM = :sicovam AND DATEDEB = :datedeb1 AND DATEFIN = :datefin1 AND CODE = :code";

            public static List<InstrumentToUpdate> RetrieveInstrumentToUpdate()
            {
                InstrumentList.Clear();
                InstrumentList = ConnectionManager.OpenRisqueConnection().Query<InstrumentToUpdate>(_QueryAmericanBondClauseRetrieve, null).AsList();
                return InstrumentList.AsList();
            }


            public static List<InstrumentToWithNextCallDate> RetrieveNextCallDate()
            {
                InstrumentListNextCallDate.Clear();
                InstrumentListNextCallDate = ConnectionManager.OpenRisqueConnection().Query<InstrumentToWithNextCallDate>(_QueryBloombergNextCallDateView, null).AsList();
                return InstrumentListNextCallDate.AsList();
            }


            public static int UpdateClause(int sicovam, DateTime datedeb, DateTime datefin, int code, string comment, DateTime datedeb1, DateTime datefin1)
            {
                var datefinstr  = datefin.ToString("dd-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                var datedebstr  = datedeb.ToString("dd-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                var datefin1str = datefin1.ToString("dd-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                var datedeb1str = datedeb1.ToString("dd-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);

                string _QueryUpdateClauseDate = String.Format("UPDATE CLAUSE SET DATEDEB = '{0}' , DATEFIN = '{1}', COM1= '{2}' WHERE SICOVAM = {3} AND DATEDEB = '{4}' AND DATEFIN = '{5}' AND CODE = {6}", datedebstr, datefinstr, comment, sicovam, datedeb1str, datefin1str, code);
                return ConnectionManager.OpenRisqueConnection().Execute(_QueryUpdateClauseDate,null);
                //return ConnectionManager.OpenRisqueConnection().Execute(_QueryUpdateClauseDate, new { datedeb = datedeb, datefin = datefin, comment = comment, sicovam = sicovam, datedeb1 = datedeb1, datefin1 = datefin1, code = code});
            }

            //public static List<InstrumentToWithNextCallDate> RetrieveNextCallDateProcSTock()
            //{
            //    try
            //    {
            //        InstrumentListNextCallDate.Clear();

            //        var p = new OracleDynamicParameters();
            //        p.Add("InstrumentRefCursor", null, Oracle.DataAccess.Client.OracleDbType.RefCursor, System.Data.ParameterDirection.Output);
            //        var ret = ConnectionManager.OpenRisqueConnection().Query<InstrumentToWithNextCallDate>("TKO_HELPER_FUNCTION.TkoUpdateNextCallDate", p, commandType: System.Data.CommandType.StoredProcedure);
            //        if (ret.Any())
            //        {
            //            InstrumentListNextCallDate = ret.AsList();
            //        }
            //        return InstrumentListNextCallDate;
            //    }
            //    catch(SqlException ex )
            //    {
            //        return null;
            //    }
            //}
        }
    }
}
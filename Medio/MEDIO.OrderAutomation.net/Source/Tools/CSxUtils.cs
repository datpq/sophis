using System;
using System.Collections.Generic;
using System.Data;
using Oracle.DataAccess.Client;
using sophis.instrument;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
using sophis.value;
using Sophis.DataAccess;

namespace MEDIO.OrderAutomation.net.Source.Tools
{
    public static class CSxUtils
    {
        public static int ToInt32(this string str)
        {
            int res = 0;
            res = Int32.TryParse(str, out res) ? res : 0;
            return res;
        }

        public static string GetFundCurrency(int folioId, out int ccy)
        {
            CMString res = "";
            ccy = 0;
            CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                CSMCurrency currency = CSMCurrency.GetCSRCurrency(folio.GetCurrency());
                if (currency != null)
                {
                    ccy = folio.GetCurrency();
                    res = GetCurrencyName(ccy);
                }
            }
            return res;
        }

        public static string GetFundName(int folioId)
        {
            CMString res = "";
            CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                CSMAmPortfolio fund = folio.GetFundRootPortfolio();
                if (fund != null)
                {
                    fund.GetName(res);
                }
            }
            return res;
        }

        //?? CSMCurrency.StringToCurrency
        //public static int StringToCurrency(string name)
        //{
        //    int res = 0;
        //    try
        //    {
        //        string sql = "select STR_TO_DEVISE(:name) from dual";
        //        OracleParameter parameter = new OracleParameter(":name", name);
        //        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
        //        res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
        //    }
        //    catch (Exception ex)
        //    {
        //        return res;
        //    }
        //    return res;
        //}

        public static double GetFundNAV(int folioId)
        {
            double res = 0;
            CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                CSMAmPortfolio fund = folio.GetFundRootPortfolio();
                if (fund != null)
                {                    
                    //if (!fund.IsLoaded())
                    //    fund.Load();
                    //fund.Compute();
                    res = fund.GetNetAssetValue();
                }
            }
            return res;
        }

        public static string GetCurrencyName(int? ccy)
        {
            int id = 0;
            CMString res = "";
            if (ccy == null || ccy <= 0)
                return res;
            id = ccy.Value;
            CSMCurrency.CurrencyToString(id, res);
            return res;
        }

        public static string GetFullFolioPath(int folioId)
        {
            CMString fullName = "";
            CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                folio.GetFullName(fullName);
            }
            return fullName;
        }

        public static string GetFolioName(int folioId)
        {
            CMString name = "";
            CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                folio.GetName(name);
            }
            return name;
        }

        public static string GetInstrumentName(int sicovam)
        {
            string res = "";
            CSMInstrument targetInstrument = CSMInstrument.GetInstance(sicovam);
            //Instrument name
            using (CMString instrumentName = targetInstrument.GetName())
            {
                res = instrumentName;
            }
            return res;
        }

        /// <summary>
        /// Should be used only in an Extraction 
        /// </summary>
        /// <returns></returns>
        public static bool IsVirtualPortfolioFXForward(CSMPortfolio portfolio, out CSMForexFuture fxFwd)
        {
            int sicovam = CSMInstrument.GetCodeWithName(portfolio.GetName());
            sicovam = sicovam == 0 ? CSMInstrument.GetCodeWithReference(portfolio.GetName()) : sicovam;
            fxFwd = CSMInstrument.GetInstance(sicovam);
            return fxFwd != null;
        }

        public static int ExecuteNonQuery(this string sqlQuery)
        {
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                return cmd.ExecuteNonQuery();
            }
        }

        public static int GenerateFXSleeves(DateTime dateTime, int[] sleeveIds, string[] ccys)
        {
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = "PH.GEN_FX_SLEEVES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new OracleParameter("p_Date", OracleDbType.Date, dateTime, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("p_Idents", OracleDbType.Varchar2, string.Join(",", sleeveIds), ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("p_Ccys", OracleDbType.Varchar2, string.Join(",", ccys), ParameterDirection.Input));
                return cmd.ExecuteNonQuery();
            }
        }

        public static int InitSleeveIDsByDate(DateTime dateTime)
        {
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = "PH.INIT_SLEEVE_IDS_BY_DATE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new OracleParameter("p_Date", OracleDbType.Date, dateTime, ParameterDirection.Input));
                return cmd.ExecuteNonQuery();
            }
        }

        public static ICollection<T> BindFromQuery<T>(this string sqlQuery, Func<IDataReader, T> projection)
        {
            var result = new List<T>();
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(projection(reader));
                    }
                }
            }
            return result;
        }

        public static T ExecuteScalar<T>(this string sqlQuery)
        {
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                var val = cmd.ExecuteScalar();
                T returnObject = (val == null || val == DBNull.Value) ? default(T) : (T)val;
                return returnObject;
            }
        }

        public static double GetExchangeRate(string curSrc, string curDest, DateTime dt)
        {
            string dtStr = dt.ToString("yyyyMMdd");
            var rateSrc = $@"SELECT D from HISTORIQUE
WHERE SICOVAM = STR_TO_DEVISE('{curSrc}')
    AND JOUR = (SELECT MAX(D.JOUR) FROM HISTORIQUE D
        WHERE D.SICOVAM = STR_TO_DEVISE('{curSrc}')
    AND D.JOUR <= TO_DATE('{dtStr}', 'YYYYMMDD'))".ExecuteScalar<decimal>();
            var rateDest = $@"SELECT D from HISTORIQUE
WHERE SICOVAM = STR_TO_DEVISE('{curDest}')
    AND JOUR = (SELECT MAX(D.JOUR) FROM HISTORIQUE D
        WHERE D.SICOVAM = STR_TO_DEVISE('{curDest}')
    AND D.JOUR <= TO_DATE('{dtStr}', 'YYYYMMDD'))".ExecuteScalar<decimal>();
            return (double)(rateSrc / rateDest);
        }
    }
}

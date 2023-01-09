using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.MEDIO_CUSTOM_PARAM;
using Oracle.DataAccess.Client;
using sophis.backoffice_cash;
using sophis.backoffice_kernel;
using sophis.instrument;
using sophis.misc;
using Sophis.OMS.Util;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
using Sophis.DataAccess;
using Sophis.Portfolio;
using Sophis.DataAccess.NH;

namespace MEDIO.BackOffice.net.src.Thirdparty
{
    public enum eConditionTpe
    {
        settlePlace,
        country
    }

    public class CSxThirdpartySettlementCondition : CSMSettlementRulesCondition
    {
        private readonly string _StrToCompare = "";
        private readonly eConditionTpe _type;

        /// <summary>
        /// Must be initialised when loading sophis
        /// </summary>
        private static List<CacheDataModel> _cache = new List<CacheDataModel>();

        public CSxThirdpartySettlementCondition(string value, eConditionTpe type)
        {
            _StrToCompare = value;
            _type = type;
        }
        
        //TODO Migration - only one method is now available to be overriden
        public override bool checkCondition(CSMTransaction trans, CSMInstrument instr, int linePriority, int lineID, int thirdPartyID)
        {
            return Check(instr);
        }

        private bool Check(CSMInstrument instr)
        {
            bool res = false;
            using (var log = new CSMLog())
            {
                log.Begin(GetType().Name, MethodBase.GetCurrentMethod().Name);
                if (instr == null)
                {
                    log.Write(CSMLog.eMVerbosity.M_warning, "Unable to find the instrument");
                    return res;
                }
                int market = instr.GetMarketCode();
                int ccy = instr.GetCurrencyCode();
                var found = _cache.FindLast(model => model.Market == market && model.CCY == ccy);
                if(found == null)
                {
                    log.Write(CSMLog.eMVerbosity.M_debug, "Cannot find a record in the cache with market = " + market + " & ccy = "+ccy);
                    return res;
                }
                if (_type == eConditionTpe.settlePlace)
                {
                    res = found.SettlePlaceCode == _StrToCompare;
                    log.Write(CSMLog.eMVerbosity.M_debug, "Is market = " + market + " & ccy = " + ccy + " linked to settle place " + _StrToCompare + "? " + res.ToString());
                }
                else if (_type == eConditionTpe.country)
                {
                    res = found.Country == _StrToCompare;
                    log.Write(CSMLog.eMVerbosity.M_debug, "Is market = " + market + " & ccy = " + ccy + " linked to country " + _StrToCompare + "? " + res.ToString());
                }
                log.End();
                return res;
            }
        }

        /// <summary>
        /// This function has to be called while initializing a sophis process
        /// </summary>
        public static void InitializeCache()
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxThirdpartySettlementCondition", MethodBase.GetCurrentMethod().Name);

                if (!_cache.IsNullOrEmpty())
                {
                    log.Write(CSMLog.eMVerbosity.M_debug, "Cache has been initialised! Do nothing ...");
                    return;
                }

                string sql =
                       "SELECT M.CODEDEVISE as CCY, M.MNEMOMARCHE as Market, T.EXTERNREF as SettlePlaceCode, T.DOMICILE as Country"
                    + " FROM MARCHE M"
                    + " LEFT JOIN EXTRNL_REF_MARKET_DEFINITION D ON D.REF_NAME = :REF_NAME"
                    + " LEFT JOIN EXTRNL_REF_MARKET_VALUE E ON M.CODEDEVISE  = E.CURRENCY AND M.MNEMOMARCHE = E.MARKET AND E.REF_IDENT = D.REF_IDENT"
                    + " LEFT JOIN TIERS T ON T.REFERENCE = E.VALUE";

                OracleParameter parameter = new OracleParameter(":REF_NAME", CSxToolkitCustomParameter.Instance.BO_THIRDPARTY_SETTLEPLACEREF);
                var parameters = new List<OracleParameter>() { parameter };
                
                _cache = LoadAll(sql, parameters);
                log.End();
            }
        }

        public static List<CacheDataModel> GetCache()
        {
            return _cache;
        }

        /// <summary>
        /// To be replaced by an nhibernate approach
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static List<CacheDataModel> LoadAll(string sqlQuery, List<OracleParameter> parameters = null)
        {
            List<CacheDataModel> result = new List<CacheDataModel>();
            using (var log = new CSMLog())
            {
                log.Begin("CSxThirdpartySettlementCondition", MethodBase.GetCurrentMethod().Name);
                try
                {
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = sqlQuery;
                        if (!parameters.IsNullOrEmpty())
                        {
                            cmd.BindByName = true;
                            cmd.Parameters.Clear();
                            foreach (var parameter in parameters)
                            {
                                cmd.Parameters.Add(parameter);
                            }
                        }
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CacheDataModel toAdd = new CacheDataModel();
                                toAdd.CCY = reader[0] != DBNull.Value ? Convert.ToInt32(reader[0]) : 0;
                                toAdd.Market = reader[1] != DBNull.Value ? Convert.ToInt32(reader[1]) : 0;
                                toAdd.SettlePlaceCode = reader[2] != DBNull.Value ? Convert.ToString(reader[2]) : "";
                                toAdd.Country = reader[3] != DBNull.Value ? Convert.ToString(reader[3]) : "";
                                if (!result.Contains(toAdd)) result.Add(toAdd);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("LoadAll exception! {0}", e.Message));
                }
            }
            return result;
        }
    }

    public class CacheDataModel
    {
        public int CCY { get; set; }
        public int Market { get; set; }
        public string SettlePlaceCode { get; set; }
        public string Country { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Oracle.DataAccess.Client;
using sophis.backoffice_cash;
using Sophis.OMS.Util;
using sophis.utils;
using Sophis.DataAccess;
using sophis.configuration;

namespace MEDIO.CORE.Tools
{
    public class CSxDBHelper
    {
        
        public static void InitDBConnection()
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
                try
                {
                    var conf = (DatabaseConnectionConfiguration)ProgramConfiguration.Current.Configuration.GetSection("Common/RisqueDatabase");
                    var connection = new OracleConnection(conf.ConnectionString);

                    FieldInfo sharedConnectionField =
                        typeof(DBContext).GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                            .FirstOrDefault(f => f.FieldType == typeof(OracleConnection));

                    sharedConnectionField.SetValue(null, connection);
                    DBContext.Connection.Open();
                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, "Exception Caught while initializing connection : "+ex.Message+"Stack :"+ex.StackTrace);
                }
            }
        }

        static public string GetTargetTradingFolio()
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);

                string retval = "";

                if (DBContext.Connection == null)
                {
                    log.Write(CSMLog.eMVerbosity.M_debug, "No database connection initialize, creating one connection");

                    InitDBConnection();
                }
                
                string query = "select CONFIG_VALUE  from MEDIO_TKT_CONFIG WHERE CONFIG_NAME= 'Trading_Target_Folio_Name'";
                retval = GetOneRecord(query).ToString();
                log.Write(CSMLog.eMVerbosity.M_info, "Returning Value :" + retval);
            
            return retval;
            }
        }

        static public int UseCashTargetTradingFolio()
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);

                int retval = 0;

                if (DBContext.Connection == null)
                {
                    log.Write(CSMLog.eMVerbosity.M_info, "No database connection initialize, creating one connection");

                    InitDBConnection();
                }

                string query = "select CONFIG_VALUE  from MEDIO_TKT_CONFIG WHERE CONFIG_NAME= 'Use_Trading_Target_Folio_Cash'";
                retval = Convert.ToInt32(GetOneRecord(query));
                log.Write(CSMLog.eMVerbosity.M_debug, "Returning Value :" + retval);

                return retval;
            }
        }
        static public string GetBOCashAccountNamePattern()
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);

                string retval = "";

                if (DBContext.Connection == null)
                {
                    log.Write(CSMLog.eMVerbosity.M_info, "No database connection initialize, creating one connection");

                    InitDBConnection();
                }

                string query = "select CONFIG_VALUE  from MEDIO_TKT_CONFIG WHERE CONFIG_NAME= 'BO_CASH_ACCOUNT_NAME_PATTERN'";
                retval = GetOneRecord(query).ToString();
                log.Write(CSMLog.eMVerbosity.M_info, "Returning Value :" + retval);

                return retval;
            }
        }

        public static T GetOneRecord<T>(string sqlQuery, List<OracleParameter> parameters = null)
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
                try
                {
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = sqlQuery;
                        cmd.Parameters.Clear();
                        if (!parameters.IsNullOrEmpty())
                        {
                            /// 1) a query whose parameters are bound is faster than the same one with non-bound parameters 
                            /// 2) statistics on server side cannot be grouped by queries if non bound values are different
                            cmd.BindByName = true;
                            foreach (var parameter in parameters)
                            {
                                if (!cmd.Parameters.Contains(parameter))
                                    cmd.Parameters.Add(parameter);
                            }
                        }
                        T returnObject = cmd.ExecuteScalar() == DBNull.Value ? default(T) : (T)cmd.ExecuteScalar();
                        return returnObject;
                    }
                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetOneRecord exception! {0}. SQL = {1}", ex.StackTrace, sqlQuery));
                }
            }
            return default(T);
        }

        public static object GetOneRecord(string sqlQuery, List<OracleParameter> parameters = null)
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
                try
                {
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = sqlQuery;
                        if (!parameters.IsNullOrEmpty())
                        {
                            /// 1) a query whose parameters are bound is faster than the same one with non-bound parameters 
                            /// 2) statistics on server side cannot be grouped by queries if non bound values are different
                            cmd.BindByName = true;
                            cmd.Parameters.Clear();
                            foreach (var parameter in parameters)
                            {
                                if (!cmd.Parameters.Contains(parameter))
                                    cmd.Parameters.Add(parameter);
                            }
                        }
                        var returnObject = cmd.ExecuteScalar() == DBNull.Value ? 0 : cmd.ExecuteScalar();
                        return returnObject;
                    }
                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetOneRecord exception! {0}. SQL = {1}", ex.StackTrace, sqlQuery));
                }
            }
            return null;
        }

        public static List<object> GetMultiRecords(string sqlQuery, List<OracleParameter> parameters = null)
        {
            var result = new List<object>();
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
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
                                result.Add(reader[0] == DBNull.Value ? 0 : reader[0]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetMultiRecord exception! {0}", e.Message));
                }
            }
            return result;
        }

        public static Dictionary<T1/*Index*/, List<T2>/*values*/> GetMultiRecordDictionary<T1, T2>(string sqlQuery, List<OracleParameter> parameters = null)
        {
            Dictionary<T1, List<T2>> res = new Dictionary<T1, List<T2>>();

            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
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
                                var index = reader[0] != DBNull.Value ? (T1)Convert.ChangeType(reader[0], typeof(T1)) : default(T1);
                                var value = reader[1] != DBNull.Value ? (T2)Convert.ChangeType(reader[1], typeof(T2)) : default(T2);
                                if (!res.ContainsKey(index))
                                {
                                    res[index] = new List<T2>() { value };
                                }
                                else
                                {
                                    if (!res[index].Contains(value))
                                    {
                                        res[index].Add(value);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetMultiRecord exception! {0}", e.Message));
                }
            }
            return res;
        }

        public static List<KeyValuePair<T1, T2>> GetMultiRecords<T1, T2>(string sqlQuery, List<OracleParameter> parameters = null)
        {
            var result = new List<KeyValuePair<T1, T2>>();
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
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
                                var key   = reader[0] != DBNull.Value ? (T1)Convert.ChangeType(reader[0], typeof(T1)) : default(T1);
                                var value = reader[1] != DBNull.Value ? (T2)Convert.ChangeType(reader[1], typeof(T2)) : default(T2);
                                result.Add(new KeyValuePair<T1, T2>(key, value));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetMultiRecord exception! {0}", e.Message));
                }
            }
            return result;
        }

        public static Dictionary<T1/*Index*/, T2/*values*/> GetDictionary<T1, T2>(string sqlQuery, List<OracleParameter> parameters = null)
        {
            Dictionary<T1, T2> res = new Dictionary<T1, T2>();

            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
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
                                var index = reader[0] != DBNull.Value ? (T1)Convert.ChangeType(reader[0], typeof(T1)) : default(T1);
                                var value = reader[1] != DBNull.Value ? (T2)Convert.ChangeType(reader[1], typeof(T2)) : default(T2);
                                res[index] = value;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetDictionary exception! {0}", e.Message));
                }
            }
            return res;
        }

        public static List<Tuple<T1, T2, T3>> GeTupleList<T1, T2, T3>(string sqlQuery, List<OracleParameter> parameters = null)
        {
            var res = new List<Tuple<T1, T2, T3>>();

            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
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
                                var v1 = reader[0] != DBNull.Value ? (T1)Convert.ChangeType(reader[0], typeof(T1)) : default(T1);
                                var v2 = reader[1] != DBNull.Value ? (T2)Convert.ChangeType(reader[1], typeof(T2)) : default(T2);
                                var v3 = reader[2] != DBNull.Value ? (T3)Convert.ChangeType(reader[2], typeof(T3)) : default(T3);
                                res.Add(new Tuple<T1, T2, T3>(v1, v2, v3));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetDictionary exception! {0}", e.Message));
                }
            }
            return res;
        }

        public static List<Tuple<T1, T2, T3,T4>> GeTupleList<T1, T2, T3,T4>(string sqlQuery, List<OracleParameter> parameters = null)
        {
            var res = new List<Tuple<T1, T2, T3,T4>>();

            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
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
                                var v1 = reader[0] != DBNull.Value ? (T1)Convert.ChangeType(reader[0], typeof(T1)) : default(T1);
                                var v2 = reader[1] != DBNull.Value ? (T2)Convert.ChangeType(reader[1], typeof(T2)) : default(T2);
                                var v3 = reader[2] != DBNull.Value ? (T3)Convert.ChangeType(reader[2], typeof(T3)) : default(T3);
                                var v4 = reader[3] != DBNull.Value ? (T4)Convert.ChangeType(reader[3], typeof(T4)) : default(T4);

                                res.Add(new Tuple<T1, T2, T3,T4>(v1, v2, v3,v4));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetDictionary exception! {0}", e.Message));
                }
            }
            return res;
        }


        public static DataSet GetDataSet(string sqlQuery, List<OracleParameter> parameters = null)
        {
            DataSet dataSet = new DataSet();

            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
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
                        using (OracleDataAdapter dataAdapter = new OracleDataAdapter())
                        {
                            dataAdapter.SelectCommand = cmd;
                            dataAdapter.Fill(dataSet);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("GetDataSet exception! {0}", e.Message));
                }
            }
            return dataSet;
        }

        public static bool Execute(string sqlQuery, List<OracleParameter> parameters = null)
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
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
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("Execute exception! {0}", ex.Message));
                    return false;
                }
                return true;
            }
        }
    }
}

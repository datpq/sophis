using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.utils;
using sophis.misc;
using System.Configuration;
//@DPH
using Oracle.DataAccess.Client;
//using System.Data.OracleClient;

namespace SampleOrderActivity
{
	 public static class OracleConnectionManager 
     {
            //public static readonly OracleConnection     _SophisConnectionPool = CSMSophisConnectionFactory.CreateOpenRisqueConnection();
            //public static readonly string               _connectionString = _SophisConnectionPool.ConnectionString;
            public static readonly Object               _lock                       = new Object() ;
            public static readonly Object               _SophisLock                 = new Object();

            public static OracleConnection _Connection;

            public static OracleConnection OpenRisqueConnection()
            {
                //@DPH
                return Sophis.DataAccess.DBContext.Connection;
            }

            //public static OracleConnection OpenConnection()
            //{
            //    try
            //    {
            //        if (_Connection == null)
            //        {
            //            lock(_lock)
            //            {
            //                if (_Connection == null)
            //                {
            //                    _Connection = new OracleConnection(_connectionString);
            //                }
            //            }
            //        }
            //        return _Connection;
            //    }
            //    catch(Exception ex)
            //    {
            //        CSMLog.Write("OracleConnectionManager", "OpenConnection", CSMLog.eMVerbosity.M_error, "exception when Openning an Oracle Connection");
            //        throw ex;
            //    }
            //}
     }
}

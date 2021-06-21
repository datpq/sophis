using System;
using System.Collections.Generic;
using System.Linq;
using sophis.misc;
using System.Configuration;
using System.Data.OracleClient;
using sophis.utils;


namespace TkoPortfolioColumn
{
    namespace ConnectionProvider
    {
        public static class ConnectionManager 
        {
            public static readonly OracleConnection     _SophisConnectionPool       = CSMSophisConnectionFactory.CreateOpenRisqueConnection();
            public static readonly string               _connectionString           = _SophisConnectionPool.ConnectionString;
            public static readonly Object               _lock                       = new Object() ;
            public static readonly Object               _SophisLock                 = new Object();

            public static OracleConnection _Connection;

            public static OracleConnection OpenRisqueConnection()
            {
                DbRequester.DbrStringQuery.Initialize();
                return _SophisConnectionPool;
            }

            public static OracleConnection OpenConnection(string name = "OracleConnectionString")
            {
                try
                {
                    var connectionStrings = ConfigurationManager.ConnectionStrings[name].ConnectionString;

                    if (_Connection == null)
                    {
                        lock(_lock)
                        {
                            if (_Connection == null)
                            {
                                _Connection = new OracleConnection(connectionStrings);
                            }
                        }
                    }
                    //DbrStringQuery.Initialize();
                    return _Connection;
                }
                catch(Exception ex)
                {
                    CSMLog.Write("ConnectionManager", "OpenConnection", CSMLog.eMVerbosity.M_error, "exception when Openning an Oracle Connection");
                    throw ex;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using sophis.misc;
//@DPH
using Oracle.DataAccess.Client;
//using System.Data.OracleClient;
using System.Data;
using System.Configuration;

namespace TkoPortfolioColumn
{

    namespace ConnectionProvider
    {
        public static class ConnectionManager
        {
            public static readonly Object _lock = new Object();
            public static readonly Object _SophisLock = new Object();

            public static IDbConnection _Connection;

            public static IDbConnection OpenConnection(string name = "OracleConnectionString")
            {
                try
                {
                    var connectionStrings = ConfigurationManager.ConnectionStrings[name].ConnectionString;
                    if (_Connection == null)
                    {
                        lock (_lock)
                        {
                            if (_Connection == null)
                            {
                                _Connection = new OracleConnection(connectionStrings);
                            }
                        }
                    }
                    return _Connection;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            //@DPH
            public static OracleConnection OpenRisqueConnection()
            {
                return Sophis.DataAccess.DBContext.Connection;
            }
        }
    }
}

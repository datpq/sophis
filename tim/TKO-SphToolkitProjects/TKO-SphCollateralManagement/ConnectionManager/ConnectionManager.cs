using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.OracleClient;
using sophis.misc;


namespace TKO_SophisCollateralManagement
{
    namespace ConnectionProvider
    {
        public static class ConnectionManager
        {
            public static readonly OracleConnection _SophisConnectionPool = CSMSophisConnectionFactory.CreateOpenRisqueConnection();

            public static OracleConnection OpenRisqueConnection()
            {
                return _SophisConnectionPool;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using sophis.misc;
using System.Data.OracleClient;

namespace TKOSphTikehauScenario
{
    namespace ConnectionProvider
    {
        public static class ConnectionManager 
        {
            public static readonly OracleConnection _SophisConnectionPool = CSMSophisConnectionFactory.CreateOpenRisqueConnection();

            public static OracleConnection _Connection;

            public static OracleConnection OpenRisqueConnection()
            {
                return _SophisConnectionPool;
            }
        }
    }
}

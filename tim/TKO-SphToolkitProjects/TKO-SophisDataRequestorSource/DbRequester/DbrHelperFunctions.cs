using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Data;
using TkoPortfolioColumn.ConnectionProvider;

namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
        public static class DbrHelperFunctions
        {
            static string storedProcedure = "TKO_HELPER_FUNCTION";

            static string function = storedProcedure + "." + "TKOGETNUMRATING" ;

            public static int DefineNumerating(String notation)
            {
                try
                {
                    var p = new DynamicParameters();
                    p.Add("@PRating", notation.ToString(), DbType.String, ParameterDirection.Input, size : 2048);
                    p.Add("@LRating", -1 ,DbType.Double, ParameterDirection.Output, sizeof(Double),null,null);

                    ConnectionManager.OpenRisqueConnection().Execute(function, p , CommandType.StoredProcedure);
                    
                    double? num = p.Get<double?>("LRating");

                    return (int)num;
                }
                catch (Exception ex)
                {
                    return -1;
                }
            }

            public static int RetrieveInstrumentList(String notation)
            {
                try
                {
                    var p = new DynamicParameters();
                    p.Add("@PRating", notation.ToString(), DbType.String, ParameterDirection.Input, size: 2048);
                    p.Add("@LRating", -1, DbType.Double, ParameterDirection.Output, sizeof(Double), null, null);

                    ConnectionManager.OpenRisqueConnection().Execute(function, p, CommandType.StoredProcedure);

                    double? num = p.Get<double?>("LRating");

                    return (int)num;
                }
                catch (Exception ex)
                {
                    return -1;
                }
            }


            public static int NextCallDate(String notation)
            {
                try
                {
                    var p = new DynamicParameters();
                    p.Add("@PRating", notation.ToString(), DbType.String, ParameterDirection.Input, size: 2048);
                    p.Add("@LRating", -1, DbType.Double, ParameterDirection.Output, sizeof(Double), null, null);


                    ConnectionManager.OpenRisqueConnection().Execute(function, p, CommandType.StoredProcedure);

                    double? num = p.Get<double?>("LRating");

                    return (int)num;
                }
                catch (Exception ex)
                {
                    return -1;
                }
            }


        }
    }

}

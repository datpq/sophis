using System.Collections.Generic;
using System.Linq;
using System;
using TkoPortfolioColumn.ConnectionProvider;
using Dapper;

namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
        public static class DbrHistomvts
        {
            private static string _QuerySector = @"select distinct opcvm from POSITION where mvtident = :mvtident";

            public static int RetrievePortfolioCodeWithPositionIdentifier(int positionidentifier)
            {
                try
                {
                    var ret = ConnectionManager.OpenRisqueConnection().Query<int>(_QuerySector, new { mvtident = positionidentifier });
                    if (ret == null)
                    {
                        return -1;
                    }
                    else
                    {
                        return ret.First();
                    }
                }
                catch (Exception ex)
                {
                    return -1;
                }
            }
        }
    }
}
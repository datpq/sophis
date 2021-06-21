using System;
using System.Collections.Generic;
using System.Linq;
using TkoPortfolioColumn.ConnectionProvider;
using Dapper;

namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
        public static class DbrSector
        {
            public class SECTORS
            {
                public double? CHECKING	 { get; set; }
                public string  CODE	     { get; set; }
                public double? DEPT	     { get; set; }
                public double  ID	     { get; set; }
                public double? INDICE	 { get; set; }
                public string  NAME	     { get; set; }
                public string  NAME_FR	 { get; set; }
                public string  PARENT	 { get; set; }
                public string  REFERENCE { get; set; }
            }

            private static string _QuerySector = @"select * from sectors  sec
                                                   start with sec.Id  = :sectorid 
                                                   connect by prior sec.Id = sec.PARENT 
                                                   order by sec.DEPT";

            public static List<SECTORS> RetriveSectorHierachie(double id)
            {
                return ConnectionManager.OpenRisqueConnection().Query<SECTORS>(_QuerySector, new { sectorid = id }).ToList();
            }

        }
    }

}

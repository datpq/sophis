using System.Collections.Generic;
using Dapper;
using System.Linq;
using TkoPortfolioColumn.ConnectionProvider;

namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
        public static class DbrPortFolio
        {
            public static List<FOLIO> FolioNameList = new List<FOLIO>();

            public class FOLIO
            {
                public string name  { get; set; }
                public int    ident { get; set; }
                public int    mgr   { get; set; }
            }

            private static string _QuerySector = @"select * from folio";


            public static List<FOLIO> RetrieveFolioCodeFromFolioName(string name)
            {
                if (!FolioNameList.Any())
                {
                   FolioNameList = ConnectionManager.OpenRisqueConnection().Query<FOLIO>(_QuerySector, null).AsList();
                }
                return FolioNameList.Where(p => p.name == name).AsList() ;
            }
        }
    }
}
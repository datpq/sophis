using System;
using System.Collections.Generic;
using System.Linq;
using TkoPortfolioColumn.ConnectionProvider;
using Dapper;

namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
        public static class DbrPerfAttribMapping
        {
            public static List<TIKEHAU_PERFATTRIB_MAPPING> PerfAttribFolioMapping = new List<TIKEHAU_PERFATTRIB_MAPPING>();

            public class TIKEHAU_PERFATTRIB_MAPPING
            {
                public string   NAME                 { get; set; }
                public int      PORTFOLIOCODE        { get; set; }
                public string   PORTFOLOINAME        { get; set; }
                public string   PORTFOLIOMAPPINGNAME { get; set; }
                public int      LEVEL                { get; set; }
                public string   DESCRIPTION          { get; set; }
            }

            private static string _QuerySector = @"select * from TIKEHAU_PERFATTRIB_MAPPING";

            private static void Load()
            {
                try
                {
                    if (!PerfAttribFolioMapping.Any())
                    {
                        PerfAttribFolioMapping = ConnectionManager.OpenRisqueConnection().Query<TIKEHAU_PERFATTRIB_MAPPING>(_QuerySector, null).AsList();
                    }
                }
                catch(Exception ex)
                {

                }
            }


            public static void SetColumnConfig(IInputProvider input)
            {
                Load();
                input.PerfAttribMappingConfigDic = PerfAttribFolioMapping.Where(p => p.NAME == input.Column).ToDictionary(p => p.PORTFOLIOCODE);
            }

        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using TkoPortfolioColumn.ConnectionProvider;
using Dapper;

namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
        public static class DbrPerfAttribCriteria
        {
            public static List<TIKEHAU_PERFATTRIB_CRITERIA> PerfAttribCriteriaConfigList = new List<TIKEHAU_PERFATTRIB_CRITERIA>();

            public class TIKEHAU_PERFATTRIB_CRITERIA
            {
                public string COLUMNNAME            { get; set; }
                public int    CONDITIONID           { get; set; }
                public string LEVEL                 { get; set; }
                public string REFCOLUMN             { get; set; }
                public string CONDITIONTYPE         { get; set; }
                public string VALEURCONDITIONTYPE   { get; set; }
                public string VALEURCONDITION       { get; set; }
                public string OUTPUT                { get; set; }
                public int    RATIO                 { get; set; }
                public string OPERATION             { get; set; }
            }

            private static string _QuerySector = @"select * from TIKEHAU_PERFATTRIB_CRITERIA";

            private static void Load()
            {
                try
                {
                    if (!PerfAttribCriteriaConfigList.Any())
                    {
                        PerfAttribCriteriaConfigList = ConnectionManager.OpenRisqueConnection().Query<TIKEHAU_PERFATTRIB_CRITERIA>(_QuerySector, null).AsList();
                    }
                }
                catch (Exception ex)
                {

                }
            }


            //public static void SetColumnConfig(InputProvider input)
            //{
            //    Load();
            //    input.PerfAttribCriteriaConfigDic = PerfAttribCriteriaConfigList.Where(p => p.COLUMNNAME == input.Column).ToDictionary(p => p.CONDITIONID);
            //}

        }
    }

}

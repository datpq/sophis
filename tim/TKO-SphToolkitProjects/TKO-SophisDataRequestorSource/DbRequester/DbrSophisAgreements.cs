using System;
using System.Collections.Generic;
using TkoPortfolioColumn.ConnectionProvider;
using Dapper;


namespace TKO_SophisCollateralManagement
{
    namespace DbRequester
    {
        public class bo_pe_agreement_main
        {
            public int CTPY_ID             { get; set; }
            public int ENTITY_ID           { get; set; }
            public int PERIMETER_ID        { get; set; }
            public string CONTRACT_NAME       { get; set; }
        }

        public static class Dbrbo_pe_agreement_main
        {
            private static string _QueryTruncate = @"select ctpy_id,entity_id,perimeter_id from bo_pe_agreement_main";

            public static List<bo_pe_agreement_main> LoadLBA()
            {
                return ConnectionManager.OpenRisqueConnection().Query<bo_pe_agreement_main>(_QueryTruncate, null).AsList();
            }
        }
    }
}
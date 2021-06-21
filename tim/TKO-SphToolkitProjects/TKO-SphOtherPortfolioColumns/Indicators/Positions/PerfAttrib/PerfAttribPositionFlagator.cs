using System.Text;
using sophis.portfolio;
using System.Collections;
using TkoPortfolioColumn.DbRequester;
using System.Linq;


namespace TkoPortfolioColumn
{
    public static class PerfAttribPositionFlagator
    {
        public static double TkoPerfAttribFlagPosition(this CSMPosition position, InputProvider input, CSMPortfolio portfolio)
        {
            input.StringIndicatorValue = "";
            input.TmpPortfolioColName = input.Column;
            input.Column = "TKO STRATEGY";
            DbrPerfAttribMapping.SetColumnConfig(input);

            input.Column = input.TmpPortfolioColName;
            var listOfConfig = input.PerfAttribMappingConfigDic;
            DbRequester.DbrPerfAttribMapping.TIKEHAU_PERFATTRIB_MAPPING value = null;
            if (listOfConfig.TryGetValue(input.PortFolioCode, out value))
            {
                input.StringIndicatorValue = " " + value.PORTFOLIOMAPPINGNAME + " ";
                value = null;
            }
            else
            {
                var cpt = listOfConfig.Count;
                int j = 0;
                var parentCode = 0 ;
                while (value == null && j < cpt)
                {
                    if (portfolio != null)
                    {
                        parentCode = portfolio.GetParentCode();
                        portfolio = CSMPortfolio.GetCSRPortfolio(parentCode);
                        if (listOfConfig.TryGetValue(parentCode, out value))
                        {
                            input.StringIndicatorValue = " " + value.PORTFOLIOMAPPINGNAME + " ";
                        }
                        j++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return 0;
        }
    }
}
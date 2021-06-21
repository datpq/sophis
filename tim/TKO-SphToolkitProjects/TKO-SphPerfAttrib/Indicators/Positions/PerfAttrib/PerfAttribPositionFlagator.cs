using System;
using System.Text;
using sophis.portfolio;
using System.Collections;
using TkoPortfolioColumn.DbRequester;
using System.Linq;
using sophis.value;
using sophis.utils;
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class PerfAttribPositionFlagator
    {
        public static double TkoPerfAttribFlagPosition(this CSMPosition position, InputProvider input, CSMPortfolio portfolio)
        {
            try
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                }
                CSMAmFund fund = CSMAmFund.GetFundFromFolio(portfolio);
                if (fund != null && !portfolio.IsAPortfolio())
                {
                    input.PortFolioCode = fund.GetTradingPortfolio();
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "it's not a portfolio. Take the portfolio code of the fund {0}", input.PortFolioCode);
                    }
                    //portfolio = CSMPortfolio.GetCSRPortfolio(input.PortFolioCode);
                    //input.PortFolio = portfolio;
                }

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
                    var parentCode = 0;
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
            catch (Exception e)
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(input={0})", input.ToString());
                }
            }
        }
    }
}
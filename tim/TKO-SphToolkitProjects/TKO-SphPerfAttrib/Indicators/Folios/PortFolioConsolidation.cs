using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.portfolio;
using System.Collections;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using TkoPortfolioColumn.DataCache;
using sophis.static_data;
using TkoPortfolioColumn.DbRequester;
using System.ComponentModel;
using sophis.value;
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class PortFoliosExtentionMethods
    {
        #region Perf Attribution Colonne

        public static double TkoPerfAttribFlagFolio(this CSMPortfolio portfolio, InputProvider input)
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

                input.StringIndicatorValue = " ";
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
                    var parentCode = 0;
                    var cpt = listOfConfig.Count;
                    int j = 0;
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

        public static double TkoPerfAttribFolioName(this CSMPortfolio portfolio, InputProvider input)
        {
            input.StringIndicatorValue = input.PortFolioName;
            return 0;
            return 0;
        }

        #endregion
    }
}

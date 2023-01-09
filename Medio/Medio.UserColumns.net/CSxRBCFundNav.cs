using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.portfolio;
using sophis.instrument;
using sophis.gui;
using sophis.static_data;
using Sophis.Logging;
using sophis.value;
using NSREnums;

namespace MEDIO.UserColumns.NET.Source.Column
{
    public class CSxRBCFundNav : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode, CSMExtraction extraction, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                CSMAmPortfolio fundFolio = CSMAmPortfolio.GetCSRPortfolio(portfolioCode);
                if (fundFolio != null)
                {
                    CSMAmPortfolio rootFolio = fundFolio.GetFundRootPortfolio();
                    if (portfolioCode == rootFolio.GetCode())
                    {
                        int fundId = fundFolio.GetHedgeFund();
                        // CSMAmFundBase fund = CSMAmFundBase.GetInstance(fundId);

                        eMHedgeFundType fundType = fundFolio.GetHedgeFundType();
                        if (fundType == eMHedgeFundType.M_hfInternal)
                        {
                            cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                            cellStyle.kind = eMDataType.M_dDouble;
                            cellStyle.style = eMTextStyleType.M_tsNormal;
                            cellStyle.alignment = eMAlignmentType.M_aCenter;
                            double last = CSMInstrument.GetLast(fundId);
                            cellValue.doubleValue = last;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxRBCFundNav::GetPositionCell error 1 : " + ex);
            }
        }
    }
}

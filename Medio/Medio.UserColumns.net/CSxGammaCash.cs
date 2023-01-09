using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.portfolio;
using sophis.instrument;
using sophis.gui;
using sophis.static_data;
using sophis.utils;
using Sophis.Logging;
using NSREnums;

namespace MEDIO.UserColumns.NET.Source.Column
{
    public class CSxGammaCash : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode, CSMExtraction extraction, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                CSMPortfolio fundFolio = CSMPortfolio.GetCSRPortfolio(portfolioCode);
                double gammaCash = 0.0;
                if (fundFolio != null)
                {
                    for (int i = 0; i < fundFolio.GetTreeViewPositionCount(); i++)
                    {
                        CSMPosition pos = fundFolio.GetNthTreeViewPosition(i);
                        int posCcy = pos.GetCurrency();
                        int globalCcy = CSMCurrency.GetGlobalMasterCurrency();
                        CSMForexSpot fx = new CSMForexSpot(new SSMFxPair(globalCcy, posCcy));
                        double posGammaCash = pos.GetGammaCash();
                        double fxRate = fx.GetLast();
                        gammaCash += (posGammaCash * fxRate);
                    }
                    if (gammaCash != 0.0)
                    {
                        cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                        cellStyle.@decimal = 2;
                        cellStyle.kind = eMDataType.M_dDouble;
                        cellStyle.style = eMTextStyleType.M_tsNormal;
                        cellStyle.alignment = eMAlignmentType.M_aCenter;
                        cellValue.doubleValue = gammaCash;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxGammaCash::GetPortfolioCell error 1 : " + ex);
            }
        }

        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                double gammaCash = 0.0;
                CSMInstrument inst = CSMInstrument.GetInstance(instrumentCode);
                CSMOption opt = inst;
                if (opt != null)
                {
                    CMString modelName = new CMString();
                    opt.GetModelName(modelName);
                    CSMForexSpot ul = opt.GetUnderlyingInstrument();
                    if (ul != null)         // inst is fx option
                    {
                        int posCcy = opt.GetCurrencyCode();
                        int globalCcy = CSMCurrency.GetGlobalMasterCurrency();
                        CSMForexSpot fx = new CSMForexSpot(new SSMFxPair(globalCcy, posCcy));
                        double fxRate = fx.GetLast();
                        gammaCash = position.GetGammaCash();
                        gammaCash *= fxRate;
                    }
                    else if (modelName == "Clause Builder")
                        gammaCash = position.GetGammaCash() / 100;
                    else if (opt.IsADigital())
                        gammaCash = position.GetGammaCash() / 100;
                    else if (opt.GetOptionFlagType() == eMOptionFlagType.M_ofAsian)     // only way to find out if this is an Asian
                        gammaCash = position.GetGammaCash() / 100;
                    else
                        gammaCash = position.GetGammaCash();
                }

                if (gammaCash != 0.0)
                {
                    cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                    cellStyle.@decimal = 6;
                    cellStyle.kind = eMDataType.M_dDouble;
                    cellStyle.style = eMTextStyleType.M_tsNormal;
                    cellStyle.alignment = eMAlignmentType.M_aCenter;
                    cellValue.doubleValue = gammaCash;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxGammaCash::GetPositionCell error 1 : " + ex);
            }
        }
    }
}

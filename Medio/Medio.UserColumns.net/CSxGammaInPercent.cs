using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.portfolio;
using sophis.instrument;
using sophis.gui;
using Sophis.Logging;
using sophis.utils;
using sophis;
using NSREnums;

namespace MEDIO.UserColumns.NET.Source.Column
{
    public class CSxGammaInPercent : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                CSMInstrument inst = CSMInstrument.GetInstance(instrumentCode);
                CSMOption option = inst;
                double gammaInPercent = 0.0;
                if (option != null)
                {
                    CMString modelName = new CMString();
                    option.GetModelName(modelName);
                    if (modelName == "Clause Builder")
                    {
                        double gammaCash = position.GetGammaCash();
                        double nominal = position.GetInstrumentCount();
                        gammaInPercent = gammaCash / nominal;               // the value displayed by the user column is divided by 1000
                                                                            // because the cash gamma column from which it is calculated
                                                                            // is divided by 1000
                    }
                    else if (option.IsADigital())      // TYPEDERIVE=4
                    {
                        CSMComputationResults result = CSMInstrument.GetComputationResults(instrumentCode);
                        int ulCode = 0;
                        double gamma = option.GetGamma(result, ref ulCode);
                        double ulPrice = CSMInstrument.GetLast(ulCode);
                        double nominal = option.GetNotional();
                        double numSecurities = position.GetInstrumentCount();
                        gammaInPercent = gamma * (ulPrice / (nominal));
                    }
                    else
                    {
                        CSMComputationResults result = CSMInstrument.GetComputationResults(instrumentCode);
                        int ulCode = 0;
                        gammaInPercent = option.GetGamma(result, ref ulCode);
                    }
                    if (gammaInPercent != 0.0)
                    {
                        cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                        cellStyle.@decimal = 6;
                        cellStyle.kind = eMDataType.M_dDouble;
                        cellStyle.style = eMTextStyleType.M_tsNormal;
                        cellStyle.alignment = eMAlignmentType.M_aCenter;
                        cellValue.doubleValue = gammaInPercent;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxGammaInPercent::GetPositionCell error 1 : " + ex);
            }
        }
    }
}
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
    public class CSxDeltaInPercent : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                CSMInstrument inst = CSMInstrument.GetInstance(instrumentCode);
                CSMOption option = inst;
                double deltaInPercent = 0.0;
                if (option!=null)
                {
                    CMString modelName = new CMString();
                    option.GetModelName(modelName);
                    if (modelName == "Clause Builder")
                    {
                        double deltaCash = position.GetDeltaCash();
                        double nominal = position.GetInstrumentCount();
                        deltaInPercent = deltaCash / nominal;               // the value displayed by the user column is divided by 1000
                                                                            // because the cash delta column from which it is calculated
                                                                            // is divided by 1000
                    }
                    else if (option.IsADigital())      // TYPEDERIVE=4
                    {
                        CSMComputationResults result = CSMInstrument.GetComputationResults(instrumentCode);
                        int ulCode = 0;
                        double delta = option.GetDelta(result, ref ulCode);
                        double ulPrice = CSMInstrument.GetLast(ulCode);
                        double nominal = option.GetNotional();
                        double numSecurities = position.GetInstrumentCount();
                        deltaInPercent = delta * (ulPrice / (nominal));
                    }
                    else
                    {
                        CSMComputationResults result = CSMInstrument.GetComputationResults(instrumentCode);
                        int ulCode = 0;
                        deltaInPercent = option.GetDelta(result, ref ulCode);
                    }
                    if (deltaInPercent != 0.0)
                    {
                        cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                        cellStyle.@decimal = 6;
                        cellStyle.kind = eMDataType.M_dDouble;
                        cellStyle.style = eMTextStyleType.M_tsNormal;
                        cellStyle.alignment = eMAlignmentType.M_aCenter;
                        cellValue.doubleValue = deltaInPercent;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxDeltaInPercent::GetPositionCell error 1 : " + ex);
            }
        }
    }
}
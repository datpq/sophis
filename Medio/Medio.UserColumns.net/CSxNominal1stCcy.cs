using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.portfolio;
using sophis.instrument;
using sophis.gui;
using Sophis.Logging;
using NSREnums;

namespace MEDIO.UserColumns.NET.Source.Column
{
    public class CSxNominal1stCcy : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                CSMInstrument inst = CSMInstrument.GetInstance(instrumentCode);
                char instType = inst.GetType_API();
                double notional = 0.0;
                double realised = position.GetRealised();
                if (instType == 'D')
                {
                    CSMInstrument ul = inst.GetUnderlyingInstrument();
                    instType = ul.GetType_API();
                }
                if (instType == 'K')
                {
                    CSMNonDeliverableForexForward fwd = inst;
                    int expiryCcy = fwd.GetExpiryCurrency();
                    int ccy = fwd.GetCurrencyCode();
                    int deliverableCcy = fwd.GetSettlementCurrency();
                    int nonDeliverableCcy = ccy == deliverableCcy ? expiryCcy : ccy;
                    CSMForexSpot fxSpot = new CSMForexSpot(new SSMFxPair(nonDeliverableCcy, deliverableCcy));
                    int mktWay = fxSpot.GetMarketWay();
                    double avgPrice = mktWay != 1 ? position.GetAveragePrice() : 1.0;
                    double noInstruments = position.GetInstrumentCount();
                    notional = realised - (noInstruments * avgPrice);
                }

                if (instType == 'E' || instType == 'X')
                {
                    notional = position.GetInstrumentCount();
                }
                cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                cellStyle.kind = eMDataType.M_dDouble;
                cellStyle.style = eMTextStyleType.M_tsNormal;
                cellStyle.alignment = eMAlignmentType.M_aCenter;
                // cellValue.SetString("");
                cellValue.doubleValue = realised + notional;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxNominal1stCcy::GetPositionCell error 1 : " + ex);
            }


        }
    }
}

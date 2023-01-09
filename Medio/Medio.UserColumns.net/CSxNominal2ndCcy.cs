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
    public class CSxNominal2ndCcy : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();

        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                CSMInstrument inst = CSMInstrument.GetInstance(instrumentCode);
                char instType = inst.GetType_API();
                // E is CSMForexSpot; K is CSMNonDeliverableForexForward; X is CSMForexFuture
                CSMForexSpot fxSpot = null;
                double numSecurities = position.GetInstrumentCount();
                double avgPrice = position.GetAveragePrice();
                double realised = position.GetRealised();
                double notional = 0.0;
                if (instType == 'K')   // CSMNonDeliverableForexForward
                {
                    CSMNonDeliverableForexForward fwd = inst;
                    int ccy = fwd.GetCurrencyCode();
                    int expiryCcy = fwd.GetExpiryCurrency();
                    int deliverableCcy = fwd.GetSettlementCurrency();
                    int nonDeliverableCcy = deliverableCcy == ccy ? expiryCcy : ccy;
                    int mktWay = 0;
                    fxSpot = new CSMForexSpot(new SSMFxPair(nonDeliverableCcy, deliverableCcy));
                    double last = fxSpot.GetLast();
                    mktWay = fxSpot.GetMarketWay();
                    avgPrice = mktWay == 1 ? avgPrice : 1.0;
                    notional = realised + (numSecurities * avgPrice);
                }
                else
                {
                    if (instType == 'E')
                        fxSpot = inst;
                    else if (instType == 'X')   // CSMForexFuture
                    {
                        CSMForexFuture future = inst;
                        int sellCcy = future.GetCurrency();
                        int buyCcy = future.GetExpiryCurrency();
                        fxSpot = new CSMForexSpot(new SSMFxPair(buyCcy, sellCcy));
                    }
                    else if (instType == 'D')   // CSMOption
                    {
                        CSMOption opt = inst;
                        fxSpot = opt.GetUnderlyingInstrument();
                    }
                    if (fxSpot != null)
                    {

                        if (fxSpot.GetMarketWay() == 1)
                            notional = realised - (numSecurities * avgPrice);
                        else
                            notional = realised - (numSecurities / avgPrice);
                    }
                }
                cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                cellStyle.kind = eMDataType.M_dDouble;
                cellStyle.style = eMTextStyleType.M_tsNormal;
                cellStyle.alignment = eMAlignmentType.M_aCenter;
                cellValue.doubleValue = notional;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxNominal2ndCcy::GetPositionCell error 1 : " + ex);
            }
        }
    }
}

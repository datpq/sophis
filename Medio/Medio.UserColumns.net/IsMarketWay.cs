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
    public class IsMarketWay : CSMPortfolioColumn
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
                if (instType == 'E')
                    fxSpot = inst;
                else if (instType == 'K')   // CSMNonDeliverableForexForward
                {
                    CSMNonDeliverableForexForward fwd = inst;
                    int sellCcy = fwd.GetCurrency();
                    int buyCcy = fwd.GetExpiryCurrency();
                    int deliverableCcy = fwd.GetSettlementCurrency();
                    int nonDeliverableCcy = (deliverableCcy == sellCcy) ? buyCcy : sellCcy;
                    fxSpot = new CSMForexSpot(new SSMFxPair(nonDeliverableCcy, deliverableCcy));
                }
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
                    int marketWay = fxSpot.GetMarketWay();
                    double last = fxSpot.GetLast();
                    cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                    cellStyle.kind = eMDataType.M_dInt;
                    cellStyle.style = eMTextStyleType.M_tsNormal;
                    cellStyle.alignment = eMAlignmentType.M_aCenter;
                    // cellValue.SetString("");
                    cellValue.integerValue = marketWay;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("IsMarketWay::GetPositionCell error 1 : " + ex);
            }
        }
    }
}

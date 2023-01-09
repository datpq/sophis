using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mediolanum_RMA_FILTER.TicketCreator.AbstractBase;
using Mediolanum_RMA_FILTER.Tools;
using RichMarketAdapter.ticket;
using sophis.log;
using transaction;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxForexHedgeCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxForexHedgeCreator).Name;

        public CSxForexHedgeCreator(eRBCTicketType type)
            : base(type)
        {
        }


        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.ForexHedgeColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.FOREX);
                bool sideBuy = fields.GetValue(RBCTicketType.ForexHedgeColumns.Direction).ToUpper().Equals("SELL") ? false : true;
                double quantity = fields.GetValue(RBCTicketType.ForexHedgeColumns.ContraAmount).ToDouble();
                quantity = !sideBuy ? -Math.Abs(quantity) : Math.Abs(quantity);
                string shareClass = fields.GetValue( RBCTicketType.ForexHedgeColumns.ShareClass);
                double clientSpotRate = fields.GetValue(RBCTicketType.ForexHedgeColumns.ClientAllInRate).ToDouble();
                int tradeDate = fields.GetValue(RBCTicketType.ForexHedgeColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.ForexDateFormats);
                int settlementDate = fields.GetValue(RBCTicketType.ForexHedgeColumns.MaturityDate).GetDateInAnyFormat(CSxRBCHelper.ForexDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlementDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlementDate);
                ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, fields.GetValue(RBCTicketType.ForexHedgeColumns.CounterCurrency));
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, clientSpotRate != 0.0 ? 1 / clientSpotRate : 0.0);
                ticketMessage.SetTicketField(FieldId.FX_RATE_WAY_NAME, string.Format("{0}/{1}",fields.GetValue(RBCTicketType.ForexHedgeColumns.FixedCurrency),fields.GetValue(RBCTicketType.ForexHedgeColumns.CounterCurrency)));
                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, quantity);
                SetDepositary(ref ticketMessage, ExtFundId);
                ticketMessage.add(FieldId.SPOTTYPE_PROPERTY_NAME, SpotTypeConstants.IN_PRICE);
                SetFolioID(ref ticketMessage, ExtFundId, "Hedge", shareClass, true);
                SetBrokerID(ref ticketMessage, ExtFundId);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpyID));
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, -1);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, generatedHash); 
                return false;
            }
        }

        public override void ValidateTicketFields(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                #region Validate amount
                if (RBCCustomParameters.Instance.ValidateForexAmount)
                {
                    
                    double receivedFixedAmount = fields.GetValue(RBCTicketType.ForexHedgeColumns.FixedAmount).ToDouble();
                    double receivedContraAmount = fields.GetValue(RBCTicketType.ForexHedgeColumns.ContraAmount).ToDouble();
                    double receivedFxRate = fields.GetValue(RBCTicketType.ForexHedgeColumns.ClientSpotRate).ToDouble();
                    double calculatedFixedAmount = receivedFxRate != 0.0 ? receivedContraAmount * receivedFxRate: 0;

                    if (Math.Abs(receivedFixedAmount - calculatedFixedAmount) > RBCCustomParameters.Instance.ForexAmountEpsilon)
                    {
                        logger.log(Severity.error, "ForexHedge sold amount validation failure: |receivedSold - calculatedSold| > " + RBCCustomParameters.Instance.ForexAmountEpsilon + " , calculatedFixedAmount=" + calculatedFixedAmount + " receivedFixedAmount=" + receivedFixedAmount);
                        ticketMessage.SetTicketField(FieldId.BACKOFFICEINFOS_PROPERTY_NAME, "Warning: ForexHedge sold amount validation failure");
                    }
                }
                #endregion
            }
        }

    }
}

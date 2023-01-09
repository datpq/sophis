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
    class CSxTACashCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxBond2Creator).Name;

        public CSxTACashCreator(eRBCTicketType type)
            : base(type)
        {

        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.TACashColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                SetDepositary(ref ticketMessage, ExtFundId);
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                int folioID = SetFolioID(ref ticketMessage, ExtFundId, "Cash", null, true);
                SetBrokerID(ref ticketMessage, ExtFundId);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, null);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpyID));
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 1);
                double quantity = fields.GetValue(RBCTicketType.TACashColumns.ConfirmedAmountLocalCurrency).ToDouble();
                quantity = quantity == 0.0 ? fields.GetValue(RBCTicketType.TACashColumns.ProjectedAmountLocalCurrency).ToDouble() : quantity;
                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, quantity);
                string ccy = fields.GetValue(RBCTicketType.TACashColumns.AccountCurrency);
                ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, ccy);
                int sicovam = GetCashInstrumentSicovam(ccy, RBCCustomParameters.Instance.TACashInstrumentNameFormat, null, ExtFundId, ExtFundId, RBCCustomParameters.Instance.TACashInstrumentNameFormat, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, folioID, null, ExtFundId);
                ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.TACashBusinessEvent))
                    ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, _BusinessEvents[RBCCustomParameters.Instance.TACashBusinessEvent]);
                else
                    logger.log(Severity.warning, "TACashBusinessEvent: " + RBCCustomParameters.Instance.TACashBusinessEvent + " not found in the db");
                int tradeDate = fields.GetValue(RBCTicketType.TACashColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.CashDateFormats);
                int settlDate = fields.GetValue(RBCTicketType.TACashColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.CashDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlDate);
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, -1);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, generatedHash); 
                return false;
            }
        }

    }
}

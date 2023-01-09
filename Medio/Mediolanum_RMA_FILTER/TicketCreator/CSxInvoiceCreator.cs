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
    class CSxInvoiceCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxBond2Creator).Name;
        private string InvoiceBusinessEvent = RBCCustomParameters.Instance.InvoiceBusinessEvent;

        public CSxInvoiceCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.InvoiceColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.InvoiceColumns.TransactionID));
                bool reversalFlag = fields.GetValue(RBCTicketType.InvoiceColumns.ReversalFlag).ToUpper().Equals("Y") ? true : false;

                int businessEventID = 0;
                if (InvoiceBusinessEvent.ValidateNotEmpty())
                {
                    if (_BusinessEvents.ContainsKey(InvoiceBusinessEvent))
                        businessEventID = _BusinessEvents[InvoiceBusinessEvent];
                    else
                        logger.log(Severity.warning, "InvoiceBusinessEvent cannot be found! ");
                }
                if (businessEventID != 0)
                {
                    ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, businessEventID);
                }
                SetDepositary(ref ticketMessage, ExtFundId);
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                int folioID = SetCashFolioID(ref ticketMessage, ExtFundId);
                SetBrokerID(ref ticketMessage, ExtFundId);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpyID));
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, -1);
                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, fields.GetValue(RBCTicketType.InvoiceColumns.TradeAmount), reversalFlag);
                string ccy = fields.GetValue(RBCTicketType.InvoiceColumns.Currency);
                ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, ccy);
                int sicovam = GetCashInstrumentSicovam(ccy, RBCCustomParameters.Instance.InvoiceInstrumentNameFormat, fields.GetValue(RBCTicketType.InvoiceColumns.FeeType), ExtFundId, ExtFundId, RBCCustomParameters.Instance.InvoiceBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, folioID, null, ExtFundId);
                ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                int tradeDate = fields.GetValue(RBCTicketType.InvoiceColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.CashDateFormats);
                int settlDate = fields.GetValue(RBCTicketType.InvoiceColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.CashDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlDate);
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, RBCTicketType.InvoiceColumns.ReversalFlag);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : ("")) + generatedHash); 
                return false;
            }
        }
        
    }
}

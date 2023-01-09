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
using System.Text.RegularExpressions;
using sophis.instrument;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxCashCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxCashCreator).Name;

        public CSxCashCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.CashColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                if (CheckBBHFundId(ExtFundId) && fields.GetValue(RBCTicketType.CashColumns.UCITSVCode) == "MARG")
                {
                    logger.log(Severity.warning, "Ignoring the ticket because MEDIO_BBH_FUNDFILTER parameter is enabled and the cash for [ " + ExtFundId + ", MARG ] will be done in the BBH Upload process");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.CashColumns.TransactionID));
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCUcitsCode, fields.GetValue(RBCTicketType.CashColumns.UCITSVCode));
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransType, fields.GetValue(RBCTicketType.CashColumns.TransactionType));
                string transactionType = fields.GetValue(RBCTicketType.CashColumns.TransactionType);
                if (!CheckAcceptedTransactionType(transactionType))
                {
                    logger.log(Severity.warning, "Ignoring trade because transaction type [ " + transactionType + " ] is part of the unaccepted transaction type list");
                    return true;
                }
                SetDepositary(ref ticketMessage, ExtFundId);
                bool reversalFlag = fields.GetValue(RBCTicketType.CashColumns.ReversalFlag).ToUpper().Equals("Y");
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                int folioID = SetCashFolioID(ref ticketMessage, ExtFundId);
                SetBrokerID(ref ticketMessage, ExtFundId);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpyID));
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 1);
                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, fields.GetValue(RBCTicketType.CashColumns.TradeAmount));
                string ccy = fields.GetValue(RBCTicketType.CashColumns.Currency);
                ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, ccy);

                int businessEventID = GetCashBusinessEvent(ExtFundId, transactionType);
                if (businessEventID != 0)
                    ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, businessEventID);

                bool UCITSVCodeCheck = fields.GetValue(RBCTicketType.CashColumns.UCITSVCode).ToUpper().Equals("BKFM");

                if (UCITSVCodeCheck == true)
                {
                    string refISINValue = fields.GetValue(RBCTicketType.CashColumns.Identifier);
                      
                    SSMComplexReference ISIN = new SSMComplexReference();
                    ISIN.type = "ISIN";
                    ISIN.value = refISINValue;
                    //force price type= "In Amount" for all cash trades 
                    ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, transaction.SpotTypeConstants.IN_PRICE);
                     

                    int sico = CSMInstrument.GetCodeWithMultiReference(ISIN);                   
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sico);
                    //load cash on root positions
                    SetFolioID(ref ticketMessage, ExtFundId);
                }
                else
                {
                    int sicovam = GetCashInstrumentSicovam(ccy, RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, ExtFundId, ExtFundId, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, folioID, null, ExtFundId);
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                }
                int tradeDate = fields.GetValue(RBCTicketType.CashColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.CashDateFormats);
                int settlDate = fields.GetValue(RBCTicketType.CashColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.CashDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlDate);
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, RBCTicketType.CashColumns.ReversalFlag);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : ("")) + generatedHash); 
                return false;
            }
        }
        
    }
}

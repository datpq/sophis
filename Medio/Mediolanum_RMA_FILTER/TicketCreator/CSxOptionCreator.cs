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
    class CSxOptionCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxBond2Creator).Name;

        public CSxOptionCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.OptionColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                if (CheckBBHFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because MEDIO_BBH_FUNDFILTER parameter is enabled and the option for [ " + ExtFundId + " ] will be done in the BBH Upload process");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.OptionColumns.TransactionCode));
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.OPTION);
                string optionType = fields.GetValue(RBCTicketType.OptionColumns.OptionType);
                if (optionType.ValidateNotEmpty())
                {
                    if (optionType.ToUpper().Equals("C"))
                        ticketMessage.SetTicketField(FieldId.MA_OPTION_TYPE, MAOptionTypeConstants.CALL);
                    else if (optionType.ToUpper().Equals("P"))
                        ticketMessage.SetTicketField(FieldId.MA_OPTION_TYPE, MAOptionTypeConstants.PUT);
                }
                string exerciseType = fields.GetValue(RBCTicketType.OptionColumns.Style);
                if (exerciseType.ValidateNotEmpty())
                {
                    if (optionType.ToUpper().Equals("EUROPEAN"))
                    {
                        ticketMessage.SetTicketField(FieldId.MA_EXERCISE_TYPE, ExerciseTypeConstants.EUROPEAN);
                    }
                    else if (optionType.ToUpper().Equals("AMERICAN"))
                    {
                        ticketMessage.SetTicketField(FieldId.MA_EXERCISE_TYPE, ExerciseTypeConstants.AMERICAN);
                    }
                    else if (optionType.ToUpper().Equals("BERMUDA"))
                    {
                        ticketMessage.SetTicketField(FieldId.MA_EXERCISE_TYPE, ExerciseTypeConstants.BERMUDA);
                    }
                }

                ticketMessage.SetTicketField(FieldId.MA_COMPLEX_REFERENCE_TYPE, RBCCustomParameters.Instance.ToolkitDefaultUniversal);
                ticketMessage.SetTicketField(FieldId.MA_STRIKE_VALUE, fields.GetValue(RBCTicketType.OptionColumns.OptionStrike));
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, fields.GetValue(RBCTicketType.OptionColumns.Premium));
                string bloomberg_code = fields.GetValue(RBCTicketType.OptionColumns.BloombergCode);
                if (bloomberg_code.ValidateNotEmpty())
                {
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, bloomberg_code);
                }
                else
                {
                    logger.log(Severity.warning, "Instrument code (BloombergCode) was null or empty");
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultErrorInstrumentID);
                }
                int tradeDate = fields.GetValue(RBCTicketType.OptionColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.OptionsDateFormats);
                int valueDate = fields.GetValue(RBCTicketType.OptionColumns.NAVDate).GetDateInAnyFormat(CSxRBCHelper.OptionsDateFormats);
                int settlementDate = fields.GetValue(RBCTicketType.OptionColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.OptionsDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, valueDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlementDate);

                SetFolioID(ref ticketMessage, ExtFundId);
                string BICCode = fields.GetValue(RBCTicketType.OptionColumns.BICCode);
                SetBrokerID(ref ticketMessage, ExtFundId, BICCode);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, BICCode);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, BICCode + " - " + GetEntityNameByID(ctpyID));
                ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.OptionColumns.OptionCurrency));
                ticketMessage.SetTicketField(FieldId.INSTRUMENTTYPEHINT, "Option");
                ticketMessage.SetTicketField(FieldId.BROKERFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.OptionColumns.CommissionAmount));
                ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, DefaultSpotType);
                SetDepositary(ref ticketMessage, ExtFundId);
                string side = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.BuyOrSell]);
                if (side.ValidateNotEmpty())
                {
                    double quantity = fields.GetValue(RBCTicketType.OptionColumns.Quantity).ToDouble();
                    if (side.ToUpper().Equals("BUY"))
                    {
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity));
                    }
                    else if (side.ToUpper().Equals("SELL"))
                    {
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity));
                    }
                }
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, -1);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, generatedHash); 
                return false;
            }
        }
      

    }
}

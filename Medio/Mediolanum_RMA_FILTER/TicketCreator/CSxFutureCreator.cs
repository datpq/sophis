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

using Oracle.DataAccess.Client;
using MEDIO.CORE.Tools;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxFutureCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxFutureCreator).Name;

        public CSxFutureCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            bool result = false;
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.FutureColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                if (CheckBBHFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because MEDIO_BBH_FUNDFILTER parameter is enabled and the future for [ " + ExtFundId + " ] will be done in the BBH Upload process");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.FutureColumns.FutureTradeCode));
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.FUTURE);
                string side = fields.GetValue(RBCTicketType.FutureColumns.BuyOrSell);
                double quantity = fields.GetValue(RBCTicketType.FutureColumns.Quantity).ToDouble();
                if (!String.IsNullOrEmpty(side))
                {
                    if (side.ToUpper().Equals("BUY"))
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity));
                    else if (side.ToUpper().Equals("SELL"))
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity));
                }
                ticketMessage.SetTicketField(FieldId.MA_COMPLEX_REFERENCE_TYPE, RBCCustomParameters.Instance.ToolkitDefaultUniversal);
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, fields.GetValue(RBCTicketType.FutureColumns.Price));
                string bloombergCode = fields.GetValue(RBCTicketType.FutureColumns.BloombergCode);

                string model = "";
                string maturity = "";
                if (bloombergCode.ValidateNotEmpty())
                {
                    // if reference exists : set it as trade instrument
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, bloombergCode);

                    //Trying to get the model name via single record query based on bloombergCode
                    try
                    {
                        string sql = "select MODELE from TITRES where REFERENCE = :ref";
                        OracleParameter param0 = new OracleParameter(":ref", bloombergCode);
                        List<OracleParameter> parameters = new List<OracleParameter>() { param0 };
                        model = CSxDBHelper.GetOneRecord(sql, parameters) == null ? "" : CSxDBHelper.GetOneRecord(sql, parameters).ToString();
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred during Getting Model Name: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }

                    try
                    {
                        string sql2 = "select to_char(echeance,'DD/MM/YYYY') from titres where reference= :ref";
                        OracleParameter param0 = new OracleParameter(":ref", bloombergCode);
                        List<OracleParameter> parameters = new List<OracleParameter>() { param0 };
                        maturity = CSxDBHelper.GetOneRecord(sql2, parameters) == null ? "" : CSxDBHelper.GetOneRecord(sql2, parameters).ToString();
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred during Getting Instrument Maturity: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                else
                {
                    logger.log(Severity.warning, "Instrument code: " + Enum.GetName(typeof(InboundCSVFields), InboundCSVFields.BloombergCode) + " was null or empty");
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultErrorInstrumentID);
                }

                int dateInAnyFormat = fields.GetValue(16).GetDateInAnyFormat(CSxRBCHelper.FuturesDateFormats);
                //int dateInAnyFormat2 = fields.GetValue(13).GetDateInAnyFormat(CSxRBCHelper.FuturesDateFormats);
                int dateInAnyFormat2 = maturity.GetDateInAnyFormat(CSxRBCHelper.FuturesDateFormats);
                logger.log(Severity.info, "Future Maturity Date = " + dateInAnyFormat2.ToString() + ". Trade Date =  " + dateInAnyFormat.ToString());
                if ( dateInAnyFormat2 >0 && dateInAnyFormat > dateInAnyFormat2)
                {
                    logger.log(Severity.error, "Trade Date > Maturity Date.  Rejecting trade for instrument "+bloombergCode);
                    result = true;
                }
                else
                {
                    if(dateInAnyFormat2 == 0 )
                        logger.log(Severity.error, "Missing Instrument " + bloombergCode+ " Try to process the deal for automatic creation with Data Service.");

                    int tradeDate = fields.GetValue(RBCTicketType.FutureColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.FuturesDateFormats);
                    int accountancyDate = fields.GetValue(RBCTicketType.FutureColumns.NAVDate).GetDateInAnyFormat(CSxRBCHelper.FuturesDateFormats);
                    int settlementDate = fields.GetValue(RBCTicketType.FutureColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.FuturesDateFormats);
                    ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                    //From 7.1.3.19, this field is taken into account (SRQ-65441)
                    //So we remove it from the toolkit code
                    //ticketMessage.SetTicketField(FieldId.ACCOUNTANCYDATE_PROPERTY_NAME, accountancyDate);
                    ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlementDate);
                    ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlementDate);
                    SetFolioID(ref ticketMessage, ExtFundId);
                    string BICCode = fields.GetValue(RBCTicketType.FutureColumns.BICCode);
                    SetBrokerID(ref ticketMessage, ExtFundId, BICCode);
                    int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, BICCode);
                    ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, BICCode + " - " + GetEntityNameByID(ctpyID));
                    SetDepositary(ref ticketMessage, ExtFundId);
                    ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.FutureColumns.FutureCurrency));
                    ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, model.StartsWith("SFE", StringComparison.InvariantCulture) ? FutureSFESpotType : DefaultSpotType);
                    ticketMessage.SetTicketField(FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.FutureColumns.FeesAmountInTransactionCurrency));
                    string gti = fields.GetValue(RBCTicketType.FutureColumns.GTIDescription);
                    if (gti.ToUpper().Contains("INDICES"))
                    {
                        ticketMessage.SetTicketField(FieldId.INSTRUMENTTYPEHINT, "IndexFuture");
                    }
                    else if (gti.ToUpper().Contains("BONDS"))
                    {
                        ticketMessage.SetTicketField(FieldId.INSTRUMENTTYPEHINT, "NotionalFuture");
                    }
                    SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                    string generatedHash = GenerateSha1Hash(fields, -1);
                    ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, generatedHash);
                    result = false;
                }
                return result;
            }
        }


    }
}

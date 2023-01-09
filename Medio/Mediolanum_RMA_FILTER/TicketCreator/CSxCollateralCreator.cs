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

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxCollateralCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxCollateralCreator).Name;

        public CSxCollateralCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.CollateralColumns.ExternalFundIdentifier);
                
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning,
                        "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId +
                        " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.CollateralColumns.TransactionID));
                string reversalStr = fields.GetValue(RBCTicketType.CollateralColumns.ReversalFlag);
                bool reversalFlag = reversalStr.ToUpper().Equals("Y");
                string transDescb = fields.GetValue(RBCTicketType.CollateralColumns.TransactionDescription);
                string instDescb = fields.GetValue(RBCTicketType.CollateralColumns.SecurityDescription);
                eCollateralType type = eCollateralType.Equity;
                if (transDescb.ToUpper().Contains("CASH"))
                    type = eCollateralType.Cash;
                else
                {
                    if (instDescb.ToUpper().Contains("BOND") || instDescb.ToUpper().Contains("MONEY MARKET"))
                        type = eCollateralType.Bond;
                }

                string instrumentName = fields.GetValue((type == eCollateralType.Bond)
                    ? RBCTicketType.CollateralColumns.ISINCode
                    : RBCTicketType.CollateralColumns.BloombergCode);

                string ccy = fields.GetValue(RBCTicketType.CollateralColumns.Currency);
                double spot = fields.GetValue(RBCTicketType.CollateralColumns.Price).ToDouble();
                double grossAmount = fields.GetValue(RBCTicketType.CollateralColumns.GrossAmount).ToDouble();
                double quantity = fields.GetValue(RBCTicketType.CollateralColumns.Quantity).ToDouble();
                spot = quantity != 0.0 ? grossAmount / quantity : spot;
                
                if (type == eCollateralType.Equity)
                {
                    //foreach (var item in RBCCustomParameters.Instance.CurrenciesForSharesPricedInSubunitsList)
                    //{
                    //    if (item.Key == ccy.ToUpper() && instrumentName.Contains(item.Value))
                    //        spot *= 100; break;
                    //}
                    //Take into accout market quotation type (in hundrendth...) and currency in pence
                    string sphCcy = GetInstrumentCCYByRef("", instrumentName);
                    int sphMarket = GetMarketByRef("", instrumentName);
                    CSxMarketQuotation marketQtn = new CSxMarketQuotation(sphCcy, sphMarket);
                    spot *= marketQtn.GetQuotationFactor();
                }
                else if (type == eCollateralType.Bond)
                    spot *= 100;

                if (type == eCollateralType.Cash)
                {
                    string transactionType = fields.GetValue(RBCTicketType.CollateralColumns.TransactionType);
                    ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 1);
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                    string accountId = fields.GetValue(RBCTicketType.CollateralColumns.ExternalFundIdentifier);
                    int folioId = SetCashFolioID(ref ticketMessage, ExtFundId);
                    int sicovam = GetCashInstrumentSicovam(ccy, RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, accountId, accountId, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, folioId, null, ExtFundId);
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                    SetBrokerID(ref ticketMessage, ExtFundId);
                    string bicCode = fields.GetValue(RBCTicketType.CollateralColumns.CollateralCounterpartyBICCode);
                    int ctpyId = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, bicCode);
                    ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpyId));
                    double netAmount = fields.GetValue(RBCTicketType.CollateralColumns.NetAmount).ToDouble();
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, netAmount);
                    ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, ccy);

                    int businessEventID = GetCashBusinessEvent(ExtFundId, transactionType);
                    if (businessEventID != 0)
                        ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, businessEventID);
                }
                else // Equity/Bond
                {
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.STOCK);
                    ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, spot);
                    ticketMessage.SetTicketField(FieldId.INSTRUMENT_SOURCE, fields.GetValue(RBCTicketType.CollateralColumns.SecurityDescription));
                    ticketMessage.SetTicketField(FieldId.MA_COMPLEX_REFERENCE_TYPE, (type == eCollateralType.Equity) ? RBCCustomParameters.Instance.ToolkitDefaultUniversal : RBCCustomParameters.Instance.ToolkitBondUniversal);
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTTYPEHINT, (type == eCollateralType.Equity) ? "Equity" : "Bond");
                    if (!String.IsNullOrEmpty(instrumentName))
                        ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, instrumentName);
                    else
                    {
                        logger.log(Severity.warning, "Instrument code: " + ((type == eCollateralType.Equity) ? (Enum.GetName(typeof(RBCTicketType.CollateralColumns), RBCTicketType.CollateralColumns.BloombergCode)) : (Enum.GetName(typeof(RBCTicketType.CollateralColumns), RBCTicketType.CollateralColumns.ISINCode))) + " was null or empty");
                        ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultErrorInstrumentID);
                    }
                    int folioID = SetFolioID(ref ticketMessage, ExtFundId, fields.GetValue(RBCTicketType.CollateralColumns.SecurityDescription));
                    string brokerBIC = fields.GetValue(RBCTicketType.CollateralColumns.CollateralCounterpartyBICCode);
                    SetBrokerID(ref ticketMessage, ExtFundId, brokerBIC);
                    int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, brokerBIC);
                    ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, brokerBIC + " - " + GetEntityNameByID(ctpyID));
                    ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.CollateralColumns.Currency));
                    ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, (type == eCollateralType.Bond) ? (BondSpotType) : (DefaultSpotType));
                    if (type == eCollateralType.Bond)
                        ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity);
                    else
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, quantity);

                    string side = fields.GetValue(RBCTicketType.CollateralColumns.TransactionDescription);
                    if (side.ValidateNotEmpty())
                    {
                        if (side.ToUpper().Equals("SECURITIES PURCHASE"))
                            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                        else if (side.ToUpper().Equals("SECURITIES SELL"))
                            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME,  -Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                        else if (side.ToUpper().Equals("SECURITIES DIRECT ENTRY"))
                            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME,  Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                        else if (side.ToUpper().Equals("SECURITIES WITHDRAWAL"))
                            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME,  -Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                    }
                    ticketMessage.SetTicketField(FieldId.BROKERFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.CollateralColumns.Brokerage));
                    ticketMessage.SetTicketField(FieldId.MARKETFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.CollateralColumns.Tax));
                    ticketMessage.SetTicketField(FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.CollateralColumns.Expenses));
                }
                int tradeDate = fields.GetValue(RBCTicketType.CollateralColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);
                int settleDate = fields.GetValue(RBCTicketType.CollateralColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settleDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settleDate);
                SetDepositary(ref ticketMessage, ExtFundId);
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, RBCTicketType.CollateralColumns.ReversalFlag);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : ("")) + generatedHash);
                return false;
            }
        }

        

    }
}

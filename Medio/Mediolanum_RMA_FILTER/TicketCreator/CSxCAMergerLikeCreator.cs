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
    class CSxCAMergerLikeCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxCorporateActionCreator).Name;

        public CSxCAMergerLikeCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);

                string caTypeStr = fields.GetValue(RBCTicketType.CAColumns.LSTType);
                int caTypeInt = caTypeStr.ToInt32();
                eCorporateActionType caType = (eCorporateActionType)caTypeInt;
                logger.log(Severity.debug,
                    "Received corporate action type: " + Enum.GetName(typeof(eCorporateActionType), caType));

                switch (caType)
                {
                    /*  Set price and OutQuantity 
                    Check: for bonds -> new ISIN / old ISIN
                    Equity: new bbg / old bbg
                    Warn if no instrument found for either trade */
                    case eCorporateActionType.Split:
                    case eCorporateActionType.CapitalIncreaseVsPayment:
                    case eCorporateActionType.CapitalReductionWithPayment:
                    case eCorporateActionType.Conversion: // to be checked
                    case eCorporateActionType.SpinOff: // keep partially the old position
                    case eCorporateActionType.WarrantExercise:
                    case eCorporateActionType.RightsEntitlement:
                    case eCorporateActionType.Amalgamation:
                    case eCorporateActionType.ExchangeOrExchangeOffer:
                    case eCorporateActionType.Merger:
                        {
                            string ExtFundId = fields.GetValue(RBCTicketType.CAColumns.ExternalFundIdentifier);
                            if (!CheckAllowedListExtFundId(ExtFundId))
                            {
                                logger.log(Severity.info,
                                    "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId +
                                    " ] is not part of allowed external fund identifier list.");
                                return true;
                            }
                            ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName,
                                fields.GetValue(RBCTicketType.CAColumns.TransactionID));
                            ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCCapsRef, fields.GetValue(RBCTicketType.CAColumns.CAPSRef));
                            ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCComment, fields.GetValue(RBCTicketType.CAColumns.Comment));
                            SetDepositary(ref ticketMessage, ExtFundId);
                            ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME,
                                Enum.GetName(typeof(eCorporateActionType), caType) ?? "CorporateAction");

                            // Select instrument and universal
                            string newType = fields.GetValue(RBCTicketType.CAColumns.NewType); // ISIN / TICKER
                            string oldBBGcode = fields.GetValue(RBCTicketType.CAColumns.OldBloombergCode);
                            string oldISIN = fields.GetValue(RBCTicketType.CAColumns.OldISIN);
                            string newBBGcode = fields.GetValue(RBCTicketType.CAColumns.NewBloombergCode);
                            string newISIN = fields.GetValue(RBCTicketType.CAColumns.NewISIN);
                            string universal = RBCCustomParameters.Instance.ToolkitDefaultUniversal; // Ticker by default
                            bool isBond = false;

                            if (newType.ValidateNotEmpty())
                            {
                                if (newBBGcode.ValidateNotEmpty())
                                {
                                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, newBBGcode);
                                    ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME,
                                        oldBBGcode + " -> " + newBBGcode);
                                }
                                if (newType.ToUpper().Contains("BOND"))
                                {
                                    isBond = true;
                                    universal = RBCCustomParameters.Instance.ToolkitBondUniversal;
                                    if (!String.IsNullOrEmpty(newISIN))
                                    {
                                        ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, newISIN);
                                        ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME,
                                            oldISIN + " -> " + newISIN);
                                    }
                                }
                            }
                            else
                            {
                                logger.log(Severity.info, "Could not find any Bloomberg or ISIN code in the input CSV");
                            }

                            ticketMessage.SetTicketField(FieldId.MA_COMPLEX_REFERENCE_TYPE, universal);
                            int tradeDate = fields.GetValue(RBCTicketType.CAColumns.ExDate).GetDateInAnyFormat(CSxRBCHelper.CADateFormats);
                            int settlementDate = fields.GetValue(RBCTicketType.CAColumns.PaymentDate).GetDateInAnyFormat(CSxRBCHelper.CADateFormats);
                            ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                            ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlementDate);
                            ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlementDate);
                            double inQuantity = fields.GetValue(RBCTicketType.CAColumns.InQuantity).ToDouble();
                            double quantity = inQuantity;
                            double outQuantity = fields.GetValue(RBCTicketType.CAColumns.OutQuantity).ToDouble();

                            double grossAmount = fields.GetValue(RBCTicketType.CAColumns.GrossAmount).ToDouble();
                            int businesseventID = GetBussinessEvent(caType);
                            if (caType == eCorporateActionType.CapitalIncreaseVsPayment)
                            {
                                if (Math.Abs(inQuantity) > 0 && Math.Abs(grossAmount) > 0 && Math.Abs(outQuantity) > 0)
                                    businesseventID = GetBussinessEvent(eCorporateActionType.SecuritiesDeposit);
                            }
                            if (businesseventID == 0 &&
                                !String.IsNullOrEmpty(RBCCustomParameters.Instance.CADefaultBusinessEvent))
                            {
                                if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.CADefaultBusinessEvent))
                                    businesseventID = _BusinessEvents[RBCCustomParameters.Instance.CADefaultBusinessEvent];
                            }
                            if (businesseventID != 0)
                                ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, businesseventID);

                            int folio_id = SetFolioID(ref ticketMessage, ExtFundId);
                            int broker_id = SetBrokerID(ref ticketMessage, ExtFundId);
                            int ctpy_id = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType);
                            OverrideDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                            ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME,
                                fields.GetValue(RBCTicketType.CAColumns.NewCurrency));
                            string transactionStatus = fields.GetValue(RBCTicketType.CAColumns.TransactionStatus);
                            bool reversalFlag = RBCCustomParameters.Instance.CorporateActionReversalCodeList.Contains(transactionStatus);

                            // Set a non-zero quantity in case of no tiker, to prevent from IS crashing...
                            if (!newBBGcode.ValidateNotEmpty() && !newISIN.ValidateNotEmpty() && inQuantity == 0.0)
                                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, 1);
                            else
                                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, inQuantity, reversalFlag);

                            string generatedHash = GenerateSha1Hash(fields, -1);
                            ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME,
                                ((reversalFlag) ? ("R") : ("")) + generatedHash + "CAReplay");

                            string instName = ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME);
                            string refName = ticketMessage.getString(FieldId.MA_COMPLEX_REFERENCE_TYPE);
                            //Take into accout market quotation type (in hundrendth...) and currency in pence
                            string ccy = GetInstrumentCCYByRef(refName, instName);
                            int market = GetMarketByRef(refName, instName);
                            CSxMarketQuotation marketQtn = new CSxMarketQuotation(ccy, market);
                            double price = fields.GetValue(RBCTicketType.CAColumns.Price).ToDouble();
                            if (isBond)
                            {
                                //For bonds quotation factor is not expected
                                //To be modified if we have a test case
                                price *= 100;
                            }
                            else
                            {
                                price *= marketQtn.GetQuotationFactor();
                            }

                            ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, price);

                            string paymentCCY = ticketMessage.getString(FieldId.PAYMENTCURRENCY_PROPERTY_NAME);
                            if (GetInstrumentCCYByRef(refName, instName).ToUpper() != paymentCCY.ToUpper())
                                ticketMessage.SetTicketField(FieldId.SPOTPAYMENTCURR_PROPERTY_NAME, PaymentCurrencyType.SETTLEMENT);

                            if (isBond)
                            {
                                ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, SpotTypeConstants.IN_PERCENTAGE);
                                ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity, reversalFlag);

                                switch (caType)
                                {
                                    case eCorporateActionType.SecuritiesDeposit:
                                    case eCorporateActionType.SecuritiesWithdrawal:
                                    case eCorporateActionType.RightsEntitlement:
                                    case eCorporateActionType.ExchangeOrExchangeOffer:
                                    case eCorporateActionType.PublicPurchaseOffer:
                                        {
                                            ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, 0);
                                        } break;
                                }
                            }

                        } break;
                    case eCorporateActionType.BonusOrScripIssue:
                        {
                            string ExtFundId = fields.GetValue(RBCTicketType.CAColumns.ExternalFundIdentifier);
                            if (!CheckAllowedListExtFundId(ExtFundId))
                            {
                                logger.log(Severity.info,
                                    "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId +
                                    " ] is not part of allowed external fund identifier list.");
                                return true;
                            }
                            ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName,
                                fields.GetValue(RBCTicketType.CAColumns.TransactionID));
                            ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCCapsRef, fields.GetValue(RBCTicketType.CAColumns.CAPSRef));
                            ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCComment, fields.GetValue(RBCTicketType.CAColumns.Comment));

                            SetDepositary(ref ticketMessage, ExtFundId);
                            ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME,
                                Enum.GetName(typeof(eCorporateActionType), caType) ?? "CorporateAction");

                            // Select instrument and universal
                            string oldType = fields.GetValue(RBCTicketType.CAColumns.OldType); // ISIN / TICKER
                            string oldBBGcode = fields.GetValue(RBCTicketType.CAColumns.OldBloombergCode);
                            string oldISIN = fields.GetValue(RBCTicketType.CAColumns.OldISIN);
                            string newBBGcode = fields.GetValue(RBCTicketType.CAColumns.NewBloombergCode);
                            string newISIN = fields.GetValue(RBCTicketType.CAColumns.NewISIN);
                            string universal = RBCCustomParameters.Instance.ToolkitDefaultUniversal; // Ticker by default

                            bool isBond = false;

                            if (oldType.ValidateNotEmpty())
                            {
                                if (oldBBGcode.ValidateNotEmpty())
                                {
                                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, oldBBGcode);
                                    ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME,
                                        oldBBGcode + " -> " + newBBGcode);
                                }
                                if (oldType.ToUpper().Contains("BOND"))
                                {
                                    isBond = true;
                                    universal = RBCCustomParameters.Instance.ToolkitBondUniversal;
                                    if (!String.IsNullOrEmpty(oldISIN))
                                    {
                                        ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, oldISIN);
                                        ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME,
                                            oldISIN + " -> " + newISIN);
                                    }
                                }
                            }
                            else
                            {
                                logger.log(Severity.info, "Could not find any Bloomberg or ISIN code in the input CSV");
                            }

                            ticketMessage.SetTicketField(FieldId.MA_COMPLEX_REFERENCE_TYPE, universal);
                            int tradeDate = fields.GetValue(RBCTicketType.CAColumns.ExDate).GetDateInAnyFormat(CSxRBCHelper.CADateFormats);
                            int settlementDate = fields.GetValue(RBCTicketType.CAColumns.PaymentDate).GetDateInAnyFormat(CSxRBCHelper.CADateFormats);
                            ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                            ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlementDate);
                            ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlementDate);
                            double inQuantity = fields.GetValue(RBCTicketType.CAColumns.InQuantity).ToDouble();
                            double quantity = inQuantity;
                            double outQuantity = fields.GetValue(RBCTicketType.CAColumns.OutQuantity).ToDouble();
                            double price = fields.GetValue(RBCTicketType.CAColumns.Price).ToDouble();

                            double netAmount = fields.GetValue(RBCTicketType.CAColumns.NetAmount).ToDouble();
                            string instName = ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME);
                            string refName = ticketMessage.getString(FieldId.MA_COMPLEX_REFERENCE_TYPE);

                            //Take into accout market quotation type (in hundrendth...) and currency in pence
                            string ccy = GetInstrumentCCYByRef(refName, instName);
                            int market = GetMarketByRef(refName, instName);
                            CSxMarketQuotation marketQtn = new CSxMarketQuotation(ccy, market);

                            if (inQuantity > 0)
                            {
                                price = Math.Abs(netAmount);
                                price = isBond ? price * 100 : price;
                                quantity = 1 * marketQtn.GetQuotationFactor();
                            }
                            if (outQuantity > 0)
                            {
                                price = -Math.Abs(netAmount);
                                price = isBond ? price * 100 : price;
                                quantity = 1 * marketQtn.GetQuotationFactor();
                            }

                            int businesseventID = GetBussinessEvent(eCorporateActionType.CashCredit);
                            if (businesseventID == 0 && !String.IsNullOrEmpty(RBCCustomParameters.Instance.CADefaultBusinessEvent))
                            {
                                if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.CADefaultBusinessEvent))
                                    businesseventID = _BusinessEvents[RBCCustomParameters.Instance.CADefaultBusinessEvent];
                            }
                            if (businesseventID != 0)
                                ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, businesseventID);

                            int folio_id = SetFolioID(ref ticketMessage, ExtFundId);
                            int broker_id = SetBrokerID(ref ticketMessage, ExtFundId);
                            int ctpy_id = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType);
                            OverrideDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                            ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.CAColumns.NewCurrency));
                            string transactionStatus = fields.GetValue(RBCTicketType.CAColumns.TransactionStatus);
                            bool reversalFlag = RBCCustomParameters.Instance.CorporateActionReversalCodeList.Contains(transactionStatus);
                            // Set a non-zero quantity in case of no tiker, to prevent from IS crashing...
                            if (!oldBBGcode.ValidateNotEmpty() && !oldISIN.ValidateNotEmpty() && inQuantity == 0.0)
                                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, 1);
                            else
                                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, quantity, reversalFlag);
                            ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, price);
                            string generatedHash = GenerateSha1Hash(fields, -1);
                            ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME,
                                ((reversalFlag) ? ("R") : ("")) + generatedHash + "CAReplay");

                            string paymentCCY = ticketMessage.getString(FieldId.PAYMENTCURRENCY_PROPERTY_NAME);
                            if (GetInstrumentCCYByRef(refName, instName).ToUpper() != paymentCCY.ToUpper())
                                ticketMessage.SetTicketField(FieldId.SPOTPAYMENTCURR_PROPERTY_NAME, PaymentCurrencyType.SETTLEMENT);

                        } break;
                    default:
                        {
                            logger.log(Severity.info, "CA type " + Enum.GetName(typeof(eCorporateActionType), caType) + " is not supported!");
                            return true;
                        }
                }
                return false;
            }
        }


    }

}

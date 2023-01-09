using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mediolanum_RMA_FILTER.TicketCreator.AbstractBase;
using Mediolanum_RMA_FILTER.Tools;
using MEDIO.CORE.Tools;
using Oracle.DataAccess.Client;
using RichMarketAdapter.ticket;
using sophis.log;
using transaction;
using sophisTools;
using sophis.static_data;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    public enum eCurrencyPriceQuotation
    {
        UnDefined = 0,
        Currency = 1,
        Hundreth = 2,
        Thousandth = 3,
        Tenth = 4,
        TenThousandth = 5
    }

    public class CSxMarketQuotation
    {
        private eCurrencyPriceQuotation type;
        private readonly string _ccy;
        private readonly int _market;
        private double factor;

        public CSxMarketQuotation(string ccy, int market)
        {
            _ccy = ccy;
            _market = market;
            SetQuotationFactor();
        }

        private void SetQuotationFactor()
        {
            using (Logger logger = new Logger("CSxMarketCCYQuotation", MethodBase.GetCurrentMethod().Name))
            {
                factor = 1;
                if (_ccy.ToUpper() != _ccy)
                {
                    int ccyIdent = CSMCurrency.StringToCurrency(_ccy);
                    CSMCurrency ccyInstance = CSMCurrency.GetCSRCurrency(ccyIdent);
                    if (ccyInstance != null)
                    {
                        ccyInstance.GetBaseCurrency(ref factor);//Base FX Rate field
                        logger.log(Severity.debug, "Factor(Base FX Rate) = " + factor);
                    }
                    else
                    {
                        factor = 100;
                    }
                   
                }
                type = RetrievePriceQuotionType();
                switch (type)
                {
                    case eCurrencyPriceQuotation.Tenth:
                        factor *= Math.Pow(10, 1);
                        break;
                    case eCurrencyPriceQuotation.Hundreth:
                        factor *= Math.Pow(10, 2);
                        break;
                    case eCurrencyPriceQuotation.Thousandth:
                        factor *= Math.Pow(10, 3);
                        break;
                    case eCurrencyPriceQuotation.TenThousandth:
                        factor *= Math.Pow(10, 4);
                        break;
                    default:
                        factor *= Math.Pow(10, 0);
                        break;
                }
                logger.log(Severity.debug, "Type = " + type + "; factor = " + factor);
            }
        }

        private eCurrencyPriceQuotation RetrievePriceQuotionType()
        {
            using (Logger logger = new Logger("CSxMarketCCYQuotation", MethodBase.GetCurrentMethod().Name))
            {
                eCurrencyPriceQuotation res = 0;
                try
                {
                    string sql = "select UNITEDECOTATION from marche where DEVISE_TO_STR(CODEDEVISE) = :ccy and MNEMOMARCHE = :market";
                    OracleParameter param0 = new OracleParameter(":ccy", _ccy);
                    OracleParameter param1 = new OracleParameter(":market", _market);
                    List<OracleParameter> parameters = new List<OracleParameter>() { param0, param1 };
                    res = (eCurrencyPriceQuotation)Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred during RetrievePriceQuotionType: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                logger.log(Severity.debug, "Type found by ccy [" + _ccy + "] and market [" + _market + "] = " + res);
                return res;
            }
        }

        public double GetQuotationFactor()
        {
            return this.factor;
        }

        /// <summary>
        /// Hash only CCY + Market 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int ccyMarketHash = _ccy.GetHashCode() + _market.GetHashCode();
            return ccyMarketHash.GetHashCode();
        }

        /// <summary>
        /// Override the Equals function in conjunction with GetHashCode()
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            CSxMarketQuotation compare = (CSxMarketQuotation)obj;

            if (_market != compare._market)
                return false;

            return String.Equals(_ccy, compare._ccy);
        }
    }

    class CSxCorporateActionCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxCorporateActionCreator).Name;

        public CSxCorporateActionCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                string caTypeStr = fields.GetValue(RBCTicketType.CAColumns.LSTType);
                int caTypeInt = caTypeStr.ToInt32();
                eCorporateActionType caType = (eCorporateActionType)caTypeInt;
                logger.log(Severity.debug, "Received corporate action type: " + Enum.GetName(typeof(eCorporateActionType), caType));
                string ExtFundId = fields.GetValue(RBCTicketType.CAColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.CAColumns.TransactionID));
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCCapsRef, fields.GetValue(RBCTicketType.CAColumns.CAPSRef));
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCComment, fields.GetValue(RBCTicketType.CAColumns.Comment));

                SetDepositary(ref ticketMessage, ExtFundId);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, Enum.GetName(typeof(eCorporateActionType), caType) ?? "CorporateAction");

                // Select instrument and universal
                bool isBond = false;
                string oldType = fields.GetValue(RBCTicketType.CAColumns.OldType); // ISIN / TICKER
                string oldBBGcode = fields.GetValue(RBCTicketType.CAColumns.OldBloombergCode);
                string oldISIN = fields.GetValue(RBCTicketType.CAColumns.OldISIN);
                string newBBGcode = fields.GetValue(RBCTicketType.CAColumns.NewBloombergCode);
                string newISIN = fields.GetValue(RBCTicketType.CAColumns.NewISIN);
                string universal = RBCCustomParameters.Instance.ToolkitDefaultUniversal; // Ticker by default
                if (oldType.ValidateNotEmpty())
                {
                    if (oldBBGcode.ValidateNotEmpty())
                    {
                        ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, oldBBGcode);
                        ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, oldBBGcode + " -> " + newBBGcode);
                    }
                    if (oldType.ToUpper().Contains("BOND"))
                    {
                        universal = RBCCustomParameters.Instance.ToolkitBondUniversal;
                        if (oldISIN.ValidateNotEmpty())
                        {
                            isBond = true;
                            ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, oldISIN);
                            ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, oldISIN + " -> " + newISIN);
                        }
                    }
                }
                else
                    logger.log(Severity.error, "Could not find any Bloomberg or ISIN code in the input CSV");

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
                string instName = ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME);
                string refName = ticketMessage.getString(FieldId.MA_COMPLEX_REFERENCE_TYPE);

                //Take into accout market quotation type (in hundrendth...) and currency in pence
                string ccy = GetInstrumentCCYByRef(refName, instName);
                int market = GetMarketByRef(refName, instName);
                CSxMarketQuotation marketQtn = new CSxMarketQuotation(ccy, market);
                if (isBond)
                    price *= 100;
                else
                    price *= marketQtn.GetQuotationFactor();

                // Use RBC price 
                double grossAmount = fields.GetValue(RBCTicketType.CAColumns.GrossAmount).ToDouble();

                int businesseventID = GetBussinessEvent(caType);
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
                int wfEvent = OverrideDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.CAColumns.OldCurrency));
                switch (caType)
                {
                    /* Set default price and in quantity */
                    case eCorporateActionType.Subscription:
                    case eCorporateActionType.DistributionInSecurities:
                    case eCorporateActionType.ReverseSplit: // to follow up with JB
                        {
                        } break;
                    /* Use in quantity if in quantity != 0 otherwise out quantity */
                    case eCorporateActionType.PublicPurchaseOffer:
                        {
                            if (inQuantity != 0.0) quantity = inQuantity;
                            else quantity = outQuantity;
                        } break;
                    // log/info: not supported in the toolkit - should be done in sophis

                    case eCorporateActionType.AdditionalSplit:
                //    case eCorporateActionType.Dividend: //TODO remove that one.
                    case eCorporateActionType.Interest:
                    case eCorporateActionType.FinalRedemption:
                        {
                            string str = String.Format("Warning: The CA type {0} is not supported by the toolkit!", caTypeStr);
                            ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, str);
                        } break;
                    case eCorporateActionType.EarlyRedemption:
                        {
                            price /= 100;
                            quantity = outQuantity;
                        } break;
                    /* Set gross amount as price and -1 as qty*/
                    case eCorporateActionType.CashCredit:
                        {
                            price = -grossAmount;
                            price = isBond ? price * 100 : price;
                            quantity = -1 * marketQtn.GetQuotationFactor();
                        } break;
                    /* Set gross amount as price and 1 as qty */
                    case eCorporateActionType.CashDebit:
                        {
                            price = grossAmount;
                            price = isBond ? price * 100 : price;
                            quantity = 1 * marketQtn.GetQuotationFactor();

                        } break;
                    /* Set price as 0 and abs(in quantity) */
                    case eCorporateActionType.SecuritiesDeposit:
                        {
                            if (newBBGcode.ValidateNotEmpty())
                                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, newBBGcode);
                            else if (isBond)
                            {
                                if (newISIN.ValidateNotEmpty())
                                {
                                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, newISIN);
                                    ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, SpotTypeConstants.IN_PERCENTAGE);
                                }
                            }
                            price = 0; quantity = Math.Abs(inQuantity);
                        } break;
                    /* Set price as 0 and -abs(out quantity) */
                    case eCorporateActionType.SecuritiesWithdrawal:
                        {
                            price = 0; quantity = -Math.Abs(outQuantity);
                        } break;
                    /* check gross amount:
                        1) > 0 then same as cash debit 
                        2) < 0 then same as cash credit
                        3) = 0 SecuritiesDeposit if inamount > 0
                        4) = 0 SecuritiesWithdrawal if outamount > 0 */
                    case eCorporateActionType.BonusOrScripIssue:
                        {
                            if (grossAmount > 0)
                                goto case eCorporateActionType.CashDebit;
                            else if (grossAmount < 0)
                                goto case eCorporateActionType.CashCredit;
                            else
                            {
                                // only reach actually
                                if (inQuantity > 0)
                                    goto case eCorporateActionType.SecuritiesDeposit;
                                if (outQuantity > 0)
                                    goto case eCorporateActionType.SecuritiesWithdrawal;
                            }
                        } break;
                    /* Set price and InQuantity */
                    // Check: for bonds -> new ISIN / old ISIN
                    // Equity: new bbg / old bbgs
                    // Warn if no instrument found for either trade
                    case eCorporateActionType.CapitalIncreaseVsPayment:
                        {
                            price = 0;
                            quantity = outQuantity;
                            if (Math.Abs(inQuantity) > 0 && Math.Abs(grossAmount) > 0 && Math.Abs(outQuantity) > 0)
                            {
                                // CA Adjustment (Securities)
                                businesseventID = GetBussinessEvent(eCorporateActionType.SecuritiesDeposit);
                                if (businesseventID == 0 && !String.IsNullOrEmpty(RBCCustomParameters.Instance.CADefaultBusinessEvent))
                                {
                                    if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.CADefaultBusinessEvent))
                                        businesseventID = _BusinessEvents[RBCCustomParameters.Instance.CADefaultBusinessEvent];
                                }
                                if (businesseventID != 0)
                                    ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, businesseventID);
                            }
                        } break;
                    case eCorporateActionType.Split:
                    case eCorporateActionType.CapitalReductionWithPayment:
                    case eCorporateActionType.Conversion: // to be checked
                    case eCorporateActionType.SpinOff: // keep partially the old position
                    case eCorporateActionType.WarrantExercise:
                    case eCorporateActionType.RightsEntitlement:
                    case eCorporateActionType.Amalgamation:
                    case eCorporateActionType.ExchangeOrExchangeOffer:
                    case eCorporateActionType.Merger:
                        {
                            price = 0;
                            quantity = outQuantity;
                        } break;

                    case eCorporateActionType.Dividend:
                        {
                            //DateTime now = DateTime.Now;
                            //CSMDay today = new CSMDay(now.Day, now.Month, now.Year);

                            //int sysDateLong = today.toLong();

                            //tradeDate = Math.Min(sysDateLong, settlementDate);
                            tradeDate = fields.GetValue(RBCTicketType.CAColumns.ExDate).GetDateInAnyFormat(CSxRBCHelper.CADateFormats);
                            ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);

                            quantity = fields.GetValue(RBCTicketType.CAColumns.InAdvanceQuantity).ToDouble();

                            logger.log(Severity.info, "Comparing field value : " + ticketMessage.getLong(FieldId.CREATION_UPDATE_EVENT_ID) + " With config : " + RBCCustomParameters.Instance.MAMLTradeCreationEventId);
                            // It compares 2 same things !! What's the point ??
                            //if (ticketMessage.getLong(FieldId.CREATION_UPDATE_EVENT_ID) == RBCCustomParameters.Instance.MAMLTradeCreationEventId)
                            //{
                            //    logger.log(Severity.warning, "Ignoring the tickest with MAML Creation Event");
                            //    return true;
                            //}

                            if (quantity != 0.0)
                            {
                                price = marketQtn.GetQuotationFactor() * fields.GetValue(RBCTicketType.CAColumns.NetAmount).ToDouble() / quantity;
                            }
                            else
                            {
                                logger.log(Severity.error, "Quantity null for dividend CA !");
                                break;
                            }
                            break;
                        }
                    default:
                        {
                            logger.log(Severity.error, "No implementation exists for this type of Corporate Action");
                            
                        } break;
                }
                string transactionStatus = fields.GetValue(RBCTicketType.CAColumns.TransactionStatus);
                bool reversalFlag = RBCCustomParameters.Instance.CorporateActionReversalCodeList.Contains(transactionStatus);

                // Set a non-zero quantity in case of no tiker, to prevent from IS crashing...
                if (ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME).Equals(0.ToString()) && quantity == 0.0)
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, 1);
                else
                {
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
                            case eCorporateActionType.CashCredit:
                            case eCorporateActionType.CashDebit:
                            {
                                ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, 0);
                            } break;
                        }
                    }
                    else
                    {
                        string paymentCCY = ticketMessage.getString(FieldId.PAYMENTCURRENCY_PROPERTY_NAME);
                        if (GetInstrumentCCYByRef(refName, instName).ToUpper() != paymentCCY.ToUpper())
                            ticketMessage.SetTicketField(FieldId.SPOTPAYMENTCURR_PROPERTY_NAME, PaymentCurrencyType.SETTLEMENT);
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, quantity, reversalFlag);
                    }
                }

                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, price);
                string generatedHash = GenerateSha1Hash(fields, -1);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : ("")) + generatedHash);
                return false;
            }
        }


    }
}




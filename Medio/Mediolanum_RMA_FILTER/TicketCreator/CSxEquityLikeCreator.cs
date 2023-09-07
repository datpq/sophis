using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mediolanum_RMA_FILTER.TicketCreator.AbstractBase;
using Mediolanum_RMA_FILTER.Tools;
using RichMarketAdapter.ticket;
using sophis.log;
using sophis.utils;
using transaction;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxEquityLikeCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxEquityLikeCreator).Name;

        public CSxEquityLikeCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.EquityColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }   
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.EquityColumns.TransactionID));
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.STOCK);
                double quantity = fields.GetValue(RBCTicketType.EquityColumns.Quantity).ToDouble();
                string reversalStr = fields.GetValue(RBCTicketType.EquityColumns.ReversalFlag);
                bool reservalFlag = reversalStr.ToUpper().Equals("Y");

                if (this._RbcTicketType == eRBCTicketType.Bond)
                {
                    //ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity, reservalFlag);
                    //28/09/2017: S Amet & C Benyahia
                    //Reversal flag removed after non vanilla fund migration 
                    ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity);
					   //in scope only for Loans (DIM_TRADEFILE_OTHERS file)
                    logger.log(Severity.debug, "Setting accrued as 0");
                    ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, 0.0);
                }
                else
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, quantity);

                string instrumentName = fields.GetValue((_RbcTicketType == eRBCTicketType.Equity || _RbcTicketType == eRBCTicketType.Fund)
                    ? RBCTicketType.EquityColumns.BloombergCode
                    : RBCTicketType.EquityColumns.ISINCode);

                string instrCcy = GetInstrumentCCYByRef("", instrumentName);
                string paymentCcy = fields.GetValue(RBCTicketType.EquityColumns.Currency);
                double spot = fields.GetValue(RBCTicketType.EquityColumns.Price).ToDouble();

                double grossAmount = fields.GetValue(RBCTicketType.EquityColumns.GrossAmount).ToDouble();

                // We do not use rbc price, instead we compute the price for the sake of reconciliation  
                spot = quantity != 0.0 ? grossAmount / quantity : spot;
                if ((_RbcTicketType == eRBCTicketType.Fund || _RbcTicketType == eRBCTicketType.Equity))
                {
                    //foreach (var item in RBCCustomParameters.Instance.CurrenciesForSharesPricedInSubunitsList)
                    //{
                    //    if (item.Key == ccy.ToUpper() && instrumentName.Contains(item.Value))
                    //    {
                    //        spot *= 100; break;
                    //    }
                    //}
                    //Take into accout market quotation type (in hundrendth...) and currency in pence
                    int sphMarket = GetMarketByRef("", instrumentName);
                    CSxMarketQuotation marketQtn = new CSxMarketQuotation(instrCcy, sphMarket);
                    spot *= marketQtn.GetQuotationFactor();
                }
                else if (_RbcTicketType == eRBCTicketType.Bond) spot *= 100;

                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, spot);
                ticketMessage.SetTicketField(FieldId.INSTRUMENT_SOURCE, fields.GetValue(RBCTicketType.EquityColumns.SecurityDescription));
                ticketMessage.SetTicketField(FieldId.MA_COMPLEX_REFERENCE_TYPE,
                    (_RbcTicketType == eRBCTicketType.Fund || _RbcTicketType == eRBCTicketType.Equity)
                        ? RBCCustomParameters.Instance.ToolkitDefaultUniversal
                        : RBCCustomParameters.Instance.ToolkitBondUniversal);
                ticketMessage.SetTicketField(FieldId.INSTRUMENTTYPEHINT,
                    (_RbcTicketType == eRBCTicketType.Fund || _RbcTicketType == eRBCTicketType.Equity) ?
                        "Equity" : "Bond");

                if (_RbcTicketType == eRBCTicketType.Fund)
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTTYPEHINT, RBCCustomParameters.Instance.FundBloombergRequestType);

                if (instrumentName.ValidateNotEmpty())
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, instrumentName);
                else
                {
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultErrorInstrumentID);
                    logger.log(Severity.warning, "Instrument code is null or empty! A default instrument is used = " + RBCCustomParameters.Instance.DefaultErrorInstrumentID);
                }

                int tradeDate = fields.GetValue(RBCTicketType.EquityColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);
                int settlementDate = fields.GetValue(RBCTicketType.EquityColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);

                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlementDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlementDate);
                // APP 3525, change brokerage to counterparty fees...
                // ticketMessage.SetTicketField(FieldId.BROKERFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.Bond2Columns.Brokerage));
                ticketMessage.SetTicketField(FieldId.BROKERFEES_PROPERTY_NAME, 0);
                // SF - 0255189 Market Fees should be Tax + expenses...
                double tax = Convert.ToDouble(fields.GetValue(RBCTicketType.EquityColumns.Expenses));
                double expenses = Convert.ToDouble(fields.GetValue(RBCTicketType.EquityColumns.Tax));
                double mFees = tax + expenses;
                ticketMessage.SetTicketField(FieldId.MARKETFEES_PROPERTY_NAME, mFees);
                // ticketMessage.SetTicketField(FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.Bond2Columns.Expenses));
                ticketMessage.SetTicketField(FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.EquityColumns.Brokerage));
                SetDepositary(ref ticketMessage, ExtFundId);
                string brokerBIC = fields.GetValue(RBCTicketType.EquityColumns.BrokerBICCode);
                SetBrokerID(ref ticketMessage, ExtFundId, brokerBIC);
                SetFolioID(ref ticketMessage, ExtFundId, fields.GetValue(RBCTicketType.EquityColumns.SecurityDescription));
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, brokerBIC);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, brokerBIC + " - " + GetEntityNameByID(ctpyID));
                ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, paymentCcy);
                //New request to put price in settlement currency (if same as underlying no changes):
                if (instrCcy.ToUpper() != paymentCcy.ToUpper())
                {
                    ticketMessage.SetTicketField(FieldId.SPOTPAYMENTCURR_PROPERTY_NAME, PaymentCurrencyType.SETTLEMENT);
                }
                ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, (_RbcTicketType == eRBCTicketType.Bond) ? (BondSpotType) : (DefaultSpotType));
                if (_RbcTicketType == eRBCTicketType.Loan)
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTTYPE_PROPERTY_NAME, PositionTypeConstants.LENDED);

                string side = fields.GetValue(RBCTicketType.EquityColumns.TransactionDescription);
                if (side.ValidateNotEmpty())
                {
                    if (_RbcTicketType != eRBCTicketType.Loan)
                    {
                        if (side.ToUpper().Equals("SECURITIES PURCHASE"))
                            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reservalFlag) ? (-1) : (1)));
                        else if (side.ToUpper().Equals("SECURITIES SELL"))
                            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity) * ((reservalFlag) ? (-1) : (1)));
                    }
                    else
                    {
                        if (side.ToUpper().Equals("SECURITIES DIRECT ENTRY"))
                            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reservalFlag) ? (-1) : (1)));
                        else if (side.ToUpper().Equals("SECURITIES WITHDRAWAL"))
                            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity) * ((reservalFlag) ? (-1) : (1)));
                    }
                }
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, RBCTicketType.EquityColumns.ReversalFlag);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, ((reservalFlag) ? ("R") : ("")) + generatedHash);
                return false;
            }
        }

        public override void ValidateTicketFields(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                double quantity = fields.GetValue(RBCTicketType.EquityColumns.Quantity).ToDouble();
                double spot = fields.GetValue(RBCTicketType.EquityColumns.Price).ToDouble();
                double receivedGrossAmount = fields.GetValue(RBCTicketType.EquityColumns.GrossAmount).ToDouble();
                double calculatedGrossAmount = 0.0;
                if (RBCCustomParameters.Instance.ValidateGrossAmount || RBCCustomParameters.Instance.ValidateNetAmount) // Calculate gross amount
                {
                    calculatedGrossAmount = quantity * spot;
                }
                if (RBCCustomParameters.Instance.ValidateGrossAmount)
                {
                    if (RBCCustomParameters.Instance.GrossAmountEpsilon < Math.Abs(calculatedGrossAmount - receivedGrossAmount))
                    {
                        logger.log(Severity.warning, "Gross amount validation failure: |calculatedGA - receivedGA| > " + RBCCustomParameters.Instance.GrossAmountEpsilon + " , calculatedGA=" + calculatedGrossAmount +
                            " receivedGA=" + receivedGrossAmount);
                        ticketMessage.SetTicketField(FieldId.BACKOFFICEINFOS_PROPERTY_NAME, "Warning: Gross amount validation failure");
                    }
                }
                double receivedNetAmount = fields.GetValue(RBCTicketType.EquityColumns.NetAmount).ToDouble();
                double calculatedNetAmount = 0.0;
                if (RBCCustomParameters.Instance.ValidateNetAmount)
                {
                    double brokerFees = ticketMessage.getString(FieldId.BROKERFEES_PROPERTY_NAME).ToDouble();
                    double marketFees = ticketMessage.getString(FieldId.MARKETFEES_PROPERTY_NAME).ToDouble();
                    double counterpartyFees = ticketMessage.getString(FieldId.COUNTERPARTYFEES_PROPERTY_NAME).ToDouble();
                    calculatedNetAmount = calculatedGrossAmount + brokerFees + marketFees + counterpartyFees;
                    if (RBCCustomParameters.Instance.NetAmountEpsilon < Math.Abs(calculatedNetAmount - receivedNetAmount))
                    {
                        logger.log(Severity.warning, "Net amount validation failure: |calculatedNA - receivedNA| > " + RBCCustomParameters.Instance.NetAmountEpsilon + " , calculatedNA=" + calculatedNetAmount +
                            " receivedNA=" + receivedNetAmount);
                        ticketMessage.SetTicketField(FieldId.BACKOFFICEINFOS_PROPERTY_NAME,"Warning: Net amount validation failure");
                    }
                }
            }
        }
    }
}

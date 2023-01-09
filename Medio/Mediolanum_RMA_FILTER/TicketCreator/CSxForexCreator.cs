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

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxForexCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxForexCreator).Name;
        private bool buyCCYisMaster = false;
        private bool sellCCYisMaster = false;
        private double receivedPurchased = 0;
        private double receivedSold = 0;
        private double receivedFxRate = 0;

        public CSxForexCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.ForexColumns.ExternalFundIdentifier);
                if(!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.ForexColumns.TransactionID));
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.FOREX);
             
                string reversalStr = fields.GetValue(RBCTicketType.ForexColumns.ReversalFlag);
                bool reversalFlag = reversalStr.ToUpper().Equals("Y");
                //if (fields.GetValue(RBCTicketType.ForexColumns.NDFFlag).ToUpper().Equals("Y"))
                //    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.NDF);
                
                string buyCCYStr = fields.GetValue(RBCTicketType.ForexColumns.BuyCurrency);
                string sellCCYStr = fields.GetValue(RBCTicketType.ForexColumns.SellCurrency);
                int buyCCY = CSxRBCHelper.StringToCurrency(buyCCYStr);
                int sellCCY = CSxRBCHelper.StringToCurrency(sellCCYStr);
                bool isFXReversed = IsFXPairReversed(buyCCY, sellCCY);
                int masterCCY1 = GetMasterCurrency(buyCCY);
                int masterCCY2 = GetMasterCurrency(sellCCY);
                if (masterCCY1 != 0 && masterCCY1 == sellCCY)
                    sellCCYisMaster = true;
                else if (masterCCY2 != 0 && masterCCY2 == buyCCY)
                    buyCCYisMaster = true;
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, fields.GetValue(RBCTicketType.ForexColumns.FXRate));
                ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, transaction.SpotTypeConstants.IN_PRICE);
                receivedSold = fields.GetValue(RBCTicketType.ForexColumns.SoldAmount).ToDouble();
                receivedFxRate = fields.GetValue(RBCTicketType.ForexColumns.FXRate).ToDouble();
                receivedPurchased = fields.GetValue(RBCTicketType.ForexColumns.PurchasedAmount).ToDouble();
                ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, buyCCYStr);

                if (buyCCYisMaster || isFXReversed == false)
                {
                    buyCCYisMaster = true;
                    logger.log(Severity.debug, "Buy currency is master currency: " + buyCCYStr + " (" + buyCCY + ")");
                    ticketMessage.SetTicketField(FieldId.FX_RATE_WAY_NAME, string.Format("{0}/{1}", buyCCYStr, sellCCYStr));
                    double purchasedAmount = fields.GetValue(RBCTicketType.ForexColumns.PurchasedAmount).ToDouble();
                    
                    // For reconciliation purpose, we calculate the fx rate instead of using the rbc rate (slight rounding discrepancy if so)
                    double fx = receivedPurchased != 0.0 ? receivedSold / receivedPurchased : CSxValidationUtil.ForexMappings[InboundCSVFields.FXRate];
                    ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, fx);
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, purchasedAmount * ((reversalFlag) ? (-1) : (1)));

                    // Test
                    ticketMessage.SetTicketField(FieldId.FX_FAR_LEG_CURRENCY_NAME, buyCCYStr);
                    ticketMessage.SetTicketField(FieldId.FX_FAR_LEG_RATE_WAY_NAME, string.Format("{0}/{1}", buyCCYStr, sellCCYStr));
                    ticketMessage.SetTicketField(FieldId.FX_FAR_LEG_QUANTITY_PROPERTY_NAME, -purchasedAmount * ((reversalFlag) ? (-1) : (1)));
                }
                else if (sellCCYisMaster || isFXReversed == true)
                {
                    sellCCYisMaster = true;
                    logger.log(Severity.debug, "Sell currency is master currency: " + sellCCYStr + " (" + sellCCY + ")");
                    ticketMessage.SetTicketField(FieldId.FX_RATE_WAY_NAME, string.Format("{0}/{1}", sellCCYStr, buyCCYStr));
                    double purchasedAmount = fields.GetValue(RBCTicketType.ForexColumns.PurchasedAmount).ToDouble();
                    double fx = receivedPurchased != 0.0 ? receivedPurchased / receivedSold : CSxValidationUtil.ForexMappings[InboundCSVFields.FXRate];
                    ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, fx);
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, purchasedAmount * ((reversalFlag) ? (-1) : (1)) * -1); // multiply by -1 as 21.3 Behaviour
                }

                 if (fields.GetValue(RBCTicketType.ForexColumns.NDFFlag).ToUpper().Equals("Y"))
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.NDF);

                int tradeDate = fields.GetValue(RBCTicketType.ForexColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.ForexDateFormats);
                int settlementDate = fields.GetValue(RBCTicketType.ForexColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.ForexDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlementDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlementDate);
                SetFolioID(ref ticketMessage, ExtFundId);
                string BICCode =  fields.GetValue(RBCTicketType.ForexColumns.BrokerBICCode);
                SetBrokerID(ref ticketMessage, ExtFundId, BICCode);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, BICCode);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, BICCode + " - " + GetEntityNameByID(ctpyID));
                SetDepositary(ref ticketMessage, ExtFundId);
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, RBCTicketType.ForexColumns.ReversalFlag);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : ("")) + generatedHash); 
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
                    if (buyCCYisMaster) //deal is buy
                    {
                        double calculatedSold = receivedPurchased * receivedFxRate;
                        if (Math.Abs(receivedSold - calculatedSold) > RBCCustomParameters.Instance.ForexAmountEpsilon)
                        {
                            logger.log(Severity.warning, "Forex sold amount validation failure: |receivedSold - calculatedSold| > " + RBCCustomParameters.Instance.ForexAmountEpsilon + " , calculatedSold=" + calculatedSold + " receivedSold=" + receivedSold);
                            ticketMessage.SetTicketField(FieldId.BACKOFFICEINFOS_PROPERTY_NAME,"Warning: Forex sold amount validation failure");
                        }
                    }
                    else if (sellCCYisMaster) //deal is sell
                    {
                        double calculatedPurchased = receivedSold * receivedFxRate;
                        if (Math.Abs(receivedPurchased - calculatedPurchased) > RBCCustomParameters.Instance.ForexAmountEpsilon)
                        {
                            logger.log(Severity.warning, "Forex purchased amount validation failure: |receivedPurchased - calculatedPurchased| > " + RBCCustomParameters.Instance.ForexAmountEpsilon + " , calculatedPurchased=" + calculatedPurchased + " receivedPurchased=" + receivedPurchased);
                            ticketMessage.SetTicketField( FieldId.BACKOFFICEINFOS_PROPERTY_NAME, "Warning: Forex purchased amount validation failure");
                        }
                    }
                }
                #endregion
            }
        }
    }
}

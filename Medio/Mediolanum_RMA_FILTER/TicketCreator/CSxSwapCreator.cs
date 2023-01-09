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
    class CSxSwapCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxBond2Creator).Name;

        public CSxSwapCreator(eRBCTicketType type)
            : base(type)
        {
        }


        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.SwapColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.SwapColumns.FusionRef));
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                int tradeDate = fields.GetValue(RBCTicketType.SwapColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.SwapsDateFormats);
                int settlementDate = fields.GetValue(RBCTicketType.SwapColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.SwapsDateFormats);
                int maturityDate = fields.GetValue(RBCTicketType.SwapColumns.MaturityDate).GetDateInAnyFormat(CSxRBCHelper.SwapsDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.MA_MATURITY_DATE, maturityDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlementDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlementDate);
                ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, DefaultSpotType);
                SetFolioID(ref ticketMessage, ExtFundId);
                string BICCode = fields.GetValue(RBCTicketType.SwapColumns.BrokerBICCode);
                SetBrokerID(ref ticketMessage, ExtFundId, BICCode);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, BICCode);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, BICCode + " - " + GetEntityNameByID(ctpyID));
                SetDepositary(ref ticketMessage, ExtFundId);
                ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultErrorInstrumentID); // initialize with zero
                bool isSicovam = true;
                string fusionRef = fields.GetValue(RBCTicketType.SwapColumns.FusionRef);
                int fusionRefInt = fields.GetValue(RBCTicketType.SwapColumns.FusionRef).ToInt32();
                if (CheckIfSicovamExists(fusionRefInt))
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, fusionRefInt);
                else
                {
                    isSicovam = false;
                    logger.log(Severity.warning, "Instrument identifier (sicovam) = " + fusionRefInt + " does not exist in the database");
                }
                int sicovam = 0;
                if (!isSicovam && !fusionRef.Contains('-'))
                {
                    logger.log(Severity.debug, "FusionRef does not contain '-' character");
                    if (GetSicovamByName(fusionRef, out sicovam))
                    {
                        ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                    }
                }
                else if (!isSicovam && fusionRef.Contains('-'))
                {
                    logger.log(Severity.debug, "FusionRef contains '-' character");
                    string[] fusionRefArray = fusionRef.Split('-');
                    if (fusionRefArray.Length >= 2)
                    {
                        string instrumentRef = fusionRefArray[fusionRefArray.Length - 1].Trim(); //get last element of array
                        if (GetSicovamByName(instrumentRef, out sicovam))
                        {
                            ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                        }
                    }
                }
                string swapWay = fields.GetValue(RBCTicketType.SwapColumns.UpfrontAmountFlag); // R or P
                if (swapWay.ToUpper().Equals("P"))
                {
                    double nominalPayable = fields.GetValue(RBCTicketType.SwapColumns.NominalPayableLeg).ToDouble();
                    ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 0.0, false); //Math.Abs(nominalPayable));
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(nominalPayable), false);//.Sign(nominalPayable));
                    ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.SwapColumns.CurrencyPayableLeg));
                    ticketMessage.SetTicketField(FieldId.ACCRUED_PROPERTY_NAME, 0.0, false); // new for swap lates CR : accrued interest should be 0.
                }
                else
                {
                    double nominalReceivable = fields.GetValue(RBCTicketType.SwapColumns.NominalReceivableLeg).ToDouble();
                    ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 0.0, false);// Math.Abs(nominalReceivable));
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(nominalReceivable));//Math.Sign(nominalReceivable));
                    ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.SwapColumns.CurrencyReceivableLeg));
                    ticketMessage.SetTicketField(FieldId.ACCRUED_PROPERTY_NAME, 0.0, false); // new for swap lates CR : accrued interest should be 0.
                }

                if (fields.GetValue(RBCTicketType.SwapColumns.GTILabel).ToUpper().Contains("CFD"))
                {
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.CFD);
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME,fields.GetValue(RBCTicketType.SwapColumns.Quantity));
                    ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, fields.GetValue(RBCTicketType.SwapColumns.CostPriceLocalCurrency));
                    ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.SwapColumns.Currency));
                }
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, -1);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, generatedHash); 
                return false;
            }
        }

    }

}

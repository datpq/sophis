using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mediolanum_RMA_FILTER.Tools;
using RichMarketAdapter.interfaces;
using RichMarketAdapter.ticket;
using sophis.misc;
using sophis.utils;
using transaction;

namespace Mediolanum_RMA_FILTER
{
    class CSxCAMergerFilter : RBC_Filter
    {
        /// <summary>
        /// Used ONLY for RMA 'Error replay' server whereas actually on successful tickets 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool filter(IMessageWrapper message)
        {
            DateTime startTime = DateTime.Now;
            if (message == null)
                return true;
            
            ITicketMessage ticketMessage = null;
            try
            {
                ticketMessage = (ITicketMessage)message.Message;
            }
            catch (Exception ex)
            {
                CSMLog.Write(this.GetType().Name, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Failed to cast IMessageWrapper to ITicketMessage: " + ex.Message);
                return true;
            }
            if (ticketMessage == null)
            {
                CSMLog.Write(this.GetType().Name, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "ticketMessage is null");
                return true;
            }
            // Validate CSV document class
            List<string> fields = CSxValidationUtil.splitCSV(ticketMessage.TextData);
            CSVDocumentClass documentClass = CSxValidationUtil.GetDocumentClass(fields);
            CSMLog.Write(this.GetType().Name, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "CSV document class: " + Enum.GetName(typeof(CSVDocumentClass), documentClass));
            ticketMessage.remove(FieldId.QUANTITY_PROPERTY_NAME); //Quantity
            ticketMessage.remove(FieldId.SPOT_PROPERTY_NAME); // Price
            ticketMessage.remove(FieldId.INSTRUMENT_SOURCE); //UNIVERSAL 'external' mapping value
            ticketMessage.remove(FieldId.MA_COMPLEX_REFERENCE_TYPE); //UNIVERSAL (ISIN, BLOOMBERG, etc.)
            ticketMessage.remove(FieldId.INSTRUMENTTYPE_PROPERTY_NAME); //Bond, Equity, Forex, etc.
            ticketMessage.remove(FieldId.MA_INSTRUMENT_NAME); //ISIN Code or BLOOMBERG Code (depends on MA_COMPLEX_REFERENCE_TYPE)
            ticketMessage.remove(FieldId.NEGOTIATIONDATE_PROPERTY_NAME); //Trade date
            if(documentClass == CSVDocumentClass.CorporateAction )
            {
                string caTypeStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.LSTType]);
                int caTypeInt = 0;
                Int32.TryParse(caTypeStr, out caTypeInt);
                CorporateActionType caType = (CorporateActionType)caTypeInt;
                CSMLog.Write(this.GetType().Name, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Received corporate action type: " + Enum.GetName(typeof(CorporateActionType), caType));
                switch (caType)
                {
                    /*  Set price and OutQuantity 
                    Check: for bonds -> new ISIN / old ISIN
                    Equity: new bbg / old bbg
                    Warn if no instrument found for either trade */
                    case CorporateActionType.Split:
                    case CorporateActionType.CapitalIncreaseVsPayment:
                    case CorporateActionType.CapitalReductionWithPayment:
                    case CorporateActionType.Conversion: // to be checked
                    case CorporateActionType.SpinOff: // keep partially the old position
                    case CorporateActionType.WarrantExercise:
                    case CorporateActionType.RightsEntitlement:
                    case CorporateActionType.Amalgamation:
                    case CorporateActionType.ExchangeOrExchangeOffer:
                    case CorporateActionType.Merger:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write(this.GetType().Name, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.TransactionID]));
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                       
                        // Select instrument and universal
                        string oldBBGcode = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OldBloombergCode]);
                        string oldISIN = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OldISIN]);
                        string newType = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewType]); // ISIN / TICKER
                        string newBBGcode = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewBloombergCode]);
                        string newISIN = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewISIN]);
                        string universal = ToolkitDefaultUniversal; // Ticker by default
                        if (!String.IsNullOrEmpty(newType))
                        {
                            if (!String.IsNullOrEmpty(newBBGcode))
                            {
                                TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, newBBGcode);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, oldBBGcode + " -> " + newBBGcode);
                            }
                            if (newType.ToUpper().Contains("BOND"))
                            {
                                universal = ToolkitBondUniversal;
                                if (!String.IsNullOrEmpty(newISIN))
                                {
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, newISIN);
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, oldISIN + " -> " + newISIN);
                                }
                            }
                        }
                        else
                            CSMLog.Write("CSxCAMergerFilter", "filter()", CSMLog.eMVerbosity.M_error, "Could not find any Bloomberg or ISIN code in the input CSV");

                        TrySetTicketField<string>(ref ticketMessage, FieldId.MA_COMPLEX_REFERENCE_TYPE, universal);
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExDate]), CSxValidationUtil.CADateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.PaymentDate]), CSxValidationUtil.CADateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        string inQuantityStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.InQuantity]);
                        double inQuantity = 0.0;
                        Double.TryParse(inQuantityStr, out inQuantity);
                        string priceStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.Price]);
                        double price = 0.0;
                        Double.TryParse(priceStr, out price);
                        string outQuantityStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OutQuantity]);
                        double outQuantity = 0.0;
                        Double.TryParse(outQuantityStr, out outQuantity);
                        string grossAmountStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.GrossAmount]);
                        double grossAmount = 0.0;
                        Double.TryParse(grossAmountStr, out grossAmount);

                        int businesseventID = GetBussinessEvent(caType);
                        if (caType == CorporateActionType.CapitalIncreaseVsPayment)
                        {
                            if (Math.Abs(inQuantity) > 0 && Math.Abs(grossAmount) > 0 && Math.Abs(outQuantity) > 0)
                                businesseventID = GetBussinessEvent(CorporateActionType.SecuritiesDeposit);
                        }
                        if (businesseventID == 0 && !String.IsNullOrEmpty(CADefaultBusinessEvent))
                        {
                            if (businessevents.ContainsKey(CADefaultBusinessEvent))
                                businesseventID = businessevents[CADefaultBusinessEvent];
                        }
                        if (businesseventID != 0)
                            TrySetTicketField<int>(ref ticketMessage, FieldId.TRADETYPE_PROPERTY_NAME, businesseventID);

                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                        GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]), CSVDocumentClass.CorporateAction, null, folio_id);
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, Enum.GetName(typeof(CorporateActionType), caType) ?? "CorporateAction");
                        TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewCurrency]);
                        string transactionStatus = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.TransactionStatus]);

                        bool reversalFlag = CorporateActionReversalCodeList.Contains(transactionStatus);
                        TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, inQuantity * ((reversalFlag) ? -1 : 1));
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, price);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : (""))+(GenerateSha1Hash(fields) + "CAReplay"));
                    }break;
                    case CorporateActionType.BonusOrScripIssue:
                    {
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.TransactionID]));
                        int depo_id = SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("CSxCAMergerFilter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, Enum.GetName(typeof(CorporateActionType), caType) ?? "CorporateAction");
                        // Select instrument and universal
                        string newType = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewType]); // ISIN / TICKER
                        string oldBBGcode = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OldBloombergCode]);
                        string oldISIN = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OldISIN]);
                        string newBBGcode = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewBloombergCode]);
                        string newISIN = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewISIN]);
                        string universal = ToolkitDefaultUniversal; // Ticker by default
                        if (!String.IsNullOrEmpty(newType))
                        {
                            if (!String.IsNullOrEmpty(newBBGcode))
                            {
                                TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, newBBGcode);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, oldBBGcode + " -> " + newBBGcode);
                            }
                            if (newType.ToUpper().Contains("BOND"))
                            {
                                universal = ToolkitBondUniversal;
                                if (!String.IsNullOrEmpty(newISIN))
                                {
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, newISIN);
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, oldISIN + " -> " + newISIN);
                                }
                            }
                        }
                        else
                            CSMLog.Write("CSxCAMergerFilter", "filter", CSMLog.eMVerbosity.M_error, "Could not find any Bloomberg or ISIN code in the input CSV");

                        TrySetTicketField<string>(ref ticketMessage, FieldId.MA_COMPLEX_REFERENCE_TYPE, universal);
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExDate]), CSxValidationUtil.CADateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.PaymentDate]), CSxValidationUtil.CADateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        string outQuantityStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OutQuantity]);
                        double outQuantity = 0.0;
                        Double.TryParse(outQuantityStr, out outQuantity);
                        string inQuantityStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.InQuantity]);
                        double inQuantity = 0.0;
                        Double.TryParse(inQuantityStr, out inQuantity);
                        double quantity = inQuantity;
                        string priceStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.Price]);
                        double price = 0.0;
                        Double.TryParse(priceStr, out price);
                        // Use RBC price 
                        string grossAmountStr = CSxValidationUtil.TryAccessListValue(fields,
                        CSxValidationUtil.CAMappings[InboundCSVFields.GrossAmount]);
                        double grossAmount = 0.0;
                        Double.TryParse(grossAmountStr, out grossAmount);

                        int businesseventID = GetBussinessEvent(CorporateActionType.CashCredit);
                        if (businesseventID == 0 && !String.IsNullOrEmpty(CADefaultBusinessEvent))
                        {
                            if (businessevents.ContainsKey(CADefaultBusinessEvent))
                                businesseventID = businessevents[CADefaultBusinessEvent];
                        }
                        if (businesseventID != 0)
                            TrySetTicketField<int>(ref ticketMessage, FieldId.TRADETYPE_PROPERTY_NAME, businesseventID);

                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int broker_id = GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]), CSVDocumentClass.CorporateAction, null, folio_id);
                        int wfEvent = OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                        TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.CAMappings[InboundCSVFields.OldCurrency]);
                        string netAmountStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NetAmount]);
                        double netAmount = 0.0;
                        Double.TryParse(netAmountStr, out netAmount);
                        if (inQuantity > 0)
                        {
                            price = 1;
                            quantity = Math.Abs(netAmount);
                        }
                        if (outQuantity > 0)
                        {
                            price = 1; 
                            quantity = -Math.Abs(netAmount);
                        }
                        string transactionStatus = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.TransactionStatus]);
                        bool reversalFlag = CorporateActionReversalCodeList.Contains(transactionStatus);
                        
                        if (String.IsNullOrEmpty(oldBBGcode) && String.IsNullOrEmpty(oldISIN) && quantity == 0.0)
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, 1);
                        else
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, quantity * ((reversalFlag) ? -1 : 1));
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, price);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : (""))+(GenerateSha1Hash(fields) + "CAReplay"));
                    }break;
                    default:
                    {
                        CSMLog.Write(this.GetType().Name, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_warning, "CA type "+ Enum.GetName(typeof(CorporateActionType), caType)+" is not support of the replay ");
                        DateTime endTime = DateTime.Now;
                        CSxRBCHelper.LogFieldsToFile(ticketMessage, "old_rbc_filter_CA.txt", (endTime - startTime).TotalMilliseconds);
                        return true;
                    }
                }
                DateTime endTime1 = DateTime.Now;
                CSxRBCHelper.LogFieldsToFile(ticketMessage, "old_rbc_filter_CA.txt", (endTime1 - startTime).TotalMilliseconds);
                return false;
            }
            else
            {
                CSMLog.Write(this.GetType().Name, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_warning, "This file type is not supported!");
                DateTime endTime = DateTime.Now;
                CSxRBCHelper.LogFieldsToFile(ticketMessage, "old_rbc_filter_CA.txt", (endTime - startTime).TotalMilliseconds);
                return true;
            }

        }
    }
}

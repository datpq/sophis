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
using Mediolanum_RMA_FILTER.Filters;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxBond2Creator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxBond2Creator).Name;

        private string boComments = "";

        public CSxBond2Creator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.Bond2Columns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.Bond2Columns.TransactionID));
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.STOCK);
                string reversalStr = fields.GetValue(RBCTicketType.Bond2Columns.ReversalFlag);
                bool reversalFlag = reversalStr.ToUpper().Equals("Y");
                double quantity = fields.GetValue(RBCTicketType.Bond2Columns.Quantity).ToDouble();
                ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity);
               
                double price = fields.GetValue(RBCTicketType.Bond2Columns.Price).ToDouble();
                double grossAmount = fields.GetValue(RBCTicketType.Bond2Columns.GrossAmount).ToDouble();
                double spot = 0.0;
                bool qtyModifiedForUnitTraded = false;
                bool isAbsBond = false;
                double nominalDB = 1.0;

                if (fields.Count == RBCTicketType.Bond3Columns.TotalCount)
                {
                    double f = fields.GetValue(RBCTicketType.Bond3Columns.Factor).ToDouble();
                    double adjustedQty = quantity * f;
                    spot = adjustedQty != 0.0 ? (100 * grossAmount / adjustedQty ) : price;
                }

                else
                {
                    spot = quantity != 0.0 ? (100 * grossAmount / quantity) : price;
                }
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, spot);
                ticketMessage.SetTicketField(FieldId.INSTRUMENT_SOURCE, fields.GetValue(RBCTicketType.Bond2Columns.SecurityDescription));
                ticketMessage.SetTicketField(FieldId.MA_COMPLEX_REFERENCE_TYPE, RBCCustomParameters.Instance.ToolkitBondUniversal);
                ticketMessage.SetTicketField(FieldId.INSTRUMENTTYPEHINT, "Bond");

                string ISIN = fields.GetValue(RBCTicketType.Bond2Columns.ISINCode);
                string addPoolFactor = "";
                if (ISIN.ValidateNotEmpty())
                {
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, fields.GetValue(RBCTicketType.Bond2Columns.ISINCode));

                    if (RBCCustomParameters.Instance.ABSBondNotionalOne)
                    {
                        try
                        {
                         
                            logger.log(Severity.debug, "Checking if ticket on ABS Bond");
                            int absData = 0;
                            string sql = "select count(*) from TITRES T, ABS_DATA A where T.SICOVAM = A.SICOVAM and T.REFERENCE = :ref";
                            OracleParameter param0 = new OracleParameter(":ref", ISIN);
                            List<OracleParameter> parameters = new List<OracleParameter>() { param0 };
                            absData = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                            nominalDB = 1.0;
                            sql = "select nominal from titres where reference=:ref1";
                            OracleParameter param1 = new OracleParameter(":ref1", ISIN);
                            List<OracleParameter> newParameters = new List<OracleParameter>() { param1 };
                            nominalDB = Convert.ToDouble(CSxDBHelper.GetOneRecord(sql, newParameters));
                            double prevQuantity = quantity;

                            if (absData > 0)
                            {
                              
                                quantity = quantity * nominalDB;
                                logger.log(Severity.debug, "Ticket on ABS Bond, Setting Quantity : " + quantity.ToString() + " , Spot : " + price.ToString());
                                // ABS:
                                // Quantity same as file quantity:
                                // adjusted by nominal from query
                                // double adjQuantity = 
                                ///ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, adjQuantity);
                                // Spot same as file price:
                                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, price);
                                // Pool info computed set as GrossAmount / Quantity / price in % , place in comments...
                                double poolFactor = 1;

                                // if (fields.Count == RBCTicketType.Bond3Columns.TotalCount)
                                // {
                                //     poolFactor = fields.GetValue(RBCTicketType.Bond3Columns.Factor).ToDouble();
                                //     logger.log(Severity.info, " Added pool Factor : " + poolFactor);
                                //}
                                //  else
                                // {
                                if (price * quantity != 0)
                                    poolFactor = grossAmount / prevQuantity / (price * 0.01);
                                logger.log(Severity.info, " Added pool Factor : " + poolFactor);
                                //}

                                addPoolFactor = "|PoolFactor=" + poolFactor.ToString();
                                ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity);

                            }
                        }
                        catch (Exception ex)
                        {
                            logger.log(Severity.error, "Error occurred during checking for ABS Bond: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                        }
                    }


                   
                    int absDataCheck = 0;
                    string sqlAbs = "select count(*) from TITRES T, ABS_DATA A where T.SICOVAM = A.SICOVAM and T.REFERENCE = :ref";
                    OracleParameter paramabs0 = new OracleParameter(":ref", ISIN);
                    List<OracleParameter> parametersAbs = new List<OracleParameter>() { paramabs0 };
                    absDataCheck = Convert.ToInt32(CSxDBHelper.GetOneRecord(sqlAbs, parametersAbs));
                    if (absDataCheck > 0)
                    {
                        logger.log(Severity.debug, "Found ticket on ABS Bond.Do not apply quantity adjustments.");
                        isAbsBond = true;
                    }

                        bool isUnitTraded = CSxFIXTSOXQtyTypeTag854Filter.CheckIfBondIsUnitTraded(ISIN);
                        if (isAbsBond == false && isUnitTraded == true)
                        {

                            string sql = "select nominal from titres where reference=:ref1";
                            OracleParameter param1 = new OracleParameter(":ref1", ISIN);
                            List<OracleParameter> newParameters = new List<OracleParameter>() { param1 };
                            nominalDB = Convert.ToDouble(CSxDBHelper.GetOneRecord(sql, newParameters));
                            quantity = quantity * nominalDB;
                            qtyModifiedForUnitTraded = true;
                            logger.log(Severity.debug, "Ticket on unit traded bond " + ISIN + " ;Setting quantity: " + quantity);
                            double spotforUnitTraded = 0.0;
                            spotforUnitTraded = quantity != 0.0 ? (100 * grossAmount / quantity) : price;
                            ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, spotforUnitTraded);
                            logger.log(Severity.debug, "Ticket on unit traded bond " + ISIN + " ;Setting spot: " + spotforUnitTraded);
                            ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity);
                        }
                    
                   
                }
                else
                {
                    logger.log(Severity.debug, "Instrument code: " + Enum.GetName(typeof(RBCTicketType.Bond2Columns), RBCTicketType.Bond2Columns.ISINCode) + " was null or empty");
                    ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, RBCCustomParameters.Instance.DefaultErrorInstrumentID);
                }
                //ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity, reversalFlag);
                //28/09/2017: S Amet & C Benyahia
                //Reversal flag removed after non vanilla fund migration 
               // ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, quantity);
                int tradeDate = fields.GetValue(RBCTicketType.Bond2Columns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);
                int settlDate = fields.GetValue(RBCTicketType.Bond2Columns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlDate);
                // APP 3525, change brokerage to counterparty fees...
               // ticketMessage.SetTicketField(FieldId.BROKERFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.Bond2Columns.Brokerage));
                ticketMessage.SetTicketField(FieldId.BROKERFEES_PROPERTY_NAME, 0);
                double tax = Convert.ToDouble(fields.GetValue(RBCTicketType.Bond2Columns.Expenses));
                double expenses = Convert.ToDouble(fields.GetValue(RBCTicketType.Bond2Columns.Tax));
                double mFees = tax + expenses;
                ticketMessage.SetTicketField(FieldId.MARKETFEES_PROPERTY_NAME, mFees);
               // ticketMessage.SetTicketField(FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.Bond2Columns.Expenses));
                ticketMessage.SetTicketField(FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields.GetValue(RBCTicketType.Bond2Columns.Brokerage));
                SetDepositary(ref ticketMessage, ExtFundId);
                SetFolioID(ref ticketMessage, ExtFundId, fields.GetValue(RBCTicketType.Bond2Columns.SecurityDescription));
                string brokerBIC = fields.GetValue(RBCTicketType.Bond2Columns.BrokerBICCode);
                SetBrokerID(ref ticketMessage, ExtFundId, brokerBIC);
                int ctpyID  = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, brokerBIC);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, brokerBIC + " - " + GetEntityNameByID(ctpyID)+addPoolFactor);
                ticketMessage.SetTicketField(FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields.GetValue(RBCTicketType.Bond2Columns.Currency));
                ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, BondSpotType);

                // APP-3121
                string instName = ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME);
                string refName = ticketMessage.getString(FieldId.MA_COMPLEX_REFERENCE_TYPE);
                string paymentCCY = ticketMessage.getString(FieldId.PAYMENTCURRENCY_PROPERTY_NAME);
                if (GetInstrumentCCYByRef(refName, instName).ToUpper() != paymentCCY.ToUpper())
                {
                    // Setting the proper exchange rate from the file...default in GUI would come from the current day, and could be different.
                    // if (price != 0)
                    // {
                    double fxRate = spot / price;
                    ticketMessage.SetTicketField(FieldId.EXCHANGERATE_PROPERTY_NAME, fxRate);

                    //}
                    //else we use default...should never happen though

                    if (RBCCustomParameters.Instance.PriceInUnderlyingCCY == false)
                    {
                        ticketMessage.SetTicketField(FieldId.SPOTPAYMENTCURR_PROPERTY_NAME, PaymentCurrencyType.SETTLEMENT);
                        double accrued = fields.GetValue(RBCTicketType.Bond2Columns.BondInterest).ToDouble();
                        ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, accrued);
                    }
                    else
                    {
                        ticketMessage.SetTicketField(FieldId.SPOTPAYMENTCURR_PROPERTY_NAME, PaymentCurrencyType.UNDERLYING);
                        ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, price);
                        //   double accrued = fields.GetValue(RBCTicketType.Bond2Columns.BondInterest).ToDouble();
                        //   ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, accrued * fxRate);
                    }

                    //Use accrued from file (converted in settlement ccy not same as default displayed in gui is in instrument ccy)
                    // double accrued = fields.GetValue(RBCTicketType.Bond2Columns.BondInterest).ToDouble();
                    // ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, accrued);


                }
                else
                {
                    double accrued = fields.GetValue(RBCTicketType.Bond2Columns.BondInterest).ToDouble();
                    if (qtyModifiedForUnitTraded == true)
                    {
                        logger.log(Severity.debug, "Ticket on unit traded bond or ABS bond " + ISIN + " ;Setting  accrued computed as BondInterest column: " + accrued);
                        ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, accrued);
                    }
                }
                

                string side = fields.GetValue(RBCTicketType.Bond2Columns.TransactionDescription);
                if (!String.IsNullOrEmpty(side))
                {
                    if (side.ToUpper().Equals("SECURITIES PURCHASE"))
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                    else if (side.ToUpper().Equals("SECURITIES SELL"))
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                }
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                string generatedHash = GenerateSha1Hash(fields, RBCTicketType.Bond2Columns.ReversalFlag);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : ("")) + generatedHash); 
                return false;
            }
        }

        public override void ValidateTicketFields(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                #region Calculating Gross Amount
                double quantity = fields.GetValue(RBCTicketType.Bond2Columns.Quantity).ToDouble();
                double spot = fields.GetValue(RBCTicketType.Bond2Columns.Price).ToDouble();
                double receivedGrossAmount = fields.GetValue(RBCTicketType.Bond2Columns.GrossAmount).ToDouble();
                double receivedNetAmount = fields.GetValue(RBCTicketType.Bond2Columns.NetAmount).ToDouble();
                double calculatedGrossAmount = 0.0;

                if (RBCCustomParameters.Instance.ValidateGrossAmount || RBCCustomParameters.Instance.ValidateNetAmount) //Calculate gross amount
                    calculatedGrossAmount = quantity * spot;
                #endregion

                #region Validating Gross Amount
                if (RBCCustomParameters.Instance.ValidateGrossAmount)
                {
                    if (RBCCustomParameters.Instance.GrossAmountEpsilon < Math.Abs(calculatedGrossAmount - receivedGrossAmount))
                    {
                        logger.log(Severity.warning, "Gross amount validation failure: |calculatedGA - receivedGA| > " + RBCCustomParameters.Instance.GrossAmountEpsilon + " , calculatedGA=" + calculatedGrossAmount + " receivedGA=" + receivedGrossAmount);
                        ticketMessage.SetTicketField(FieldId.BACKOFFICEINFOS_PROPERTY_NAME, "Warning: Gross amount validation failure");
                    }
                }
                #endregion

                #region Validating Net Amount
                if (RBCCustomParameters.Instance.ValidateNetAmount)
                {
                    double brokerFees = ticketMessage.getString(FieldId.BROKERFEES_PROPERTY_NAME).ToDouble();
                    double marketFees = ticketMessage.getString(FieldId.MARKETFEES_PROPERTY_NAME).ToDouble();
                    double counterpartyFees = ticketMessage.getString(FieldId.COUNTERPARTYFEES_PROPERTY_NAME).ToDouble();
                    double calculatedNetAmount = calculatedGrossAmount + brokerFees + marketFees + counterpartyFees;
                    if (RBCCustomParameters.Instance.NetAmountEpsilon < Math.Abs(calculatedNetAmount - receivedNetAmount))
                    {
                        logger.log(Severity.warning, "Net amount validation failure: |calculatedNA - receivedNA| > " + RBCCustomParameters.Instance.NetAmountEpsilon + " , calculatedNA=" + calculatedNetAmount + " receivedNA=" + receivedNetAmount);
                        ticketMessage.SetTicketField(FieldId.BACKOFFICEINFOS_PROPERTY_NAME, "Warning: Net amount validation failure");
                    }
                }
                #endregion

            }
        }

        //protected string GetUnderlyingCCY(string refName, string refValue)
        //{
        //    string res = "";
        //    using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
        //    {
        //        try
        //        {
        //            // first check
        //            string sql = "select DEVISE_TO_STR(DEVISECTT) from titres where REFERENCE = :REFERENCE";
        //            OracleParameter parameter = new OracleParameter(":REFERENCE", refValue);
        //            List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
        //            res = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));

        //            // second check
        //            if (String.IsNullOrEmpty(res))
        //            {
        //                sql = "select DEVISE_TO_STR(t.DEVISECTT) from titres t"
        //                        + "inner join EXTRNL_REFERENCES_INSTRUMENTS ei"
        //                        + "on t.sicovam = ei.SOPHIS_IDENT"
        //                        + "inner join EXTRNL_REFERENCES_DEFINITION ed"
        //                        + "on ei.ref_ident = ed.ref_ident and ed.ref_name = :refName and ei.value = :refValue";

        //                OracleParameter parameter1 = new OracleParameter(":refName", refName);
        //                OracleParameter parameter2 = new OracleParameter(":refValue", refValue);
        //                parameters = new List<OracleParameter>() { parameter1, parameter2 };
        //                res = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.log(Severity.error, "Error occurred during GetUnderlyingCCY: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
        //        }
        //        logger.log(Severity.debug, "Underlying ccy found by instrument " + refValue + " = " + res);
        //    }
        //    return res;
        //}
    }
}

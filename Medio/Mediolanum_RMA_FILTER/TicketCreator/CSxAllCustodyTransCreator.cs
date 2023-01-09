using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mediolanum_RMA_FILTER.TicketCreator.AbstractBase;
using Mediolanum_RMA_FILTER.Tools;
using RichMarketAdapter.ticket;
using sophis.log;
using transaction;
using System.Globalization;
using Sophis.DataAccess;
using Oracle.DataAccess.Client;
using MEDIO.CORE.Tools;
using sophis.instrument;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CashConfigTable
    {
        public string TransDesc;
        public string SSTransType;
        public string RelatedTradeId;
        public string ManagerGroup;
    }

    class CSxAllCustodyTransCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxAllCustodyTransCreator).Name;
        //private static HashSet<string> uniqueKeyValues = new HashSet<string>();

        private static List<CashConfigTable> MiscCash = new List<CashConfigTable>();
        private static void LoadMiscCash()
        {
            if (MiscCash.Any()) return;
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TRANS_DESC, SS_TRANS_TYPE, MANAGER_GROUP FROM MEDIO_SSB_MISC_CASH";
                        logger.log(Severity.debug, $"cmd.CommandText={cmd.CommandText}");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                var ms = new CashConfigTable();
                                ms.TransDesc = reader[0] == DBNull.Value ? string.Empty : (string)reader[0];
                                ms.SSTransType = reader[1] == DBNull.Value ? string.Empty : (string)reader[1];
                                ms.ManagerGroup = reader[2] == DBNull.Value ? string.Empty : ((string)reader[2]).ToUpper();
                                logger.log(Severity.debug, $"MiscCash: {ms.TransDesc}-{ms.SSTransType}-{ms.ManagerGroup}");
                                MiscCash.Add(ms);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while LoadMiscCash from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
        }

        private static List<CashConfigTable> CleanCash = new List<CashConfigTable>();
        private static void LoadCleanCash()
        {
            if (CleanCash.Any()) return;
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT RELATED_TRADE_ID, MANAGER_GROUP FROM MEDIO_SSB_CLEAN_CASH";
                        logger.log(Severity.debug, $"cmd.CommandText={cmd.CommandText}");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var cs = new CashConfigTable();
                                cs.RelatedTradeId = reader[0] == DBNull.Value ? string.Empty : (string)reader[0];
                                cs.ManagerGroup = reader[1] == DBNull.Value ? string.Empty : ((string)reader[1]).ToUpper();
                                logger.log(Severity.debug, $"CleanCash: {cs.RelatedTradeId}-{cs.ManagerGroup}");
                                CleanCash.Add(cs);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while LoadCleanCash from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
        }

        private static bool IsMIFL(string fund)
        {
            return ",GFJG,GFJH,GFJL,GFJN,".IndexOf("," + fund + ",") > 0;
        }
        private static bool IsDIM(string fund)
        {
            return ",GFJM,GFJX,GFJQ,GFJR,GFJH,GFJW,GFJV,GFJL,GFJY,GFJS,GFJP,GFJU,".IndexOf("," + fund + ",") > 0;
        }

        private static bool IsCleanCash(string transDesc, string relatedTradeId, string fund)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                LoadCleanCash();
                bool result = false;
                var matchRelatedTradeId = CleanCash.FirstOrDefault(x => x.RelatedTradeId.Equals(relatedTradeId));
                var matchTransDesc = transDesc.Equals("CLEAN CASH DISBURSEMENT") || transDesc.Equals("CLEAN CASH RECEIPT");
                if (matchTransDesc && matchRelatedTradeId != null && string.IsNullOrEmpty(matchRelatedTradeId.ManagerGroup)) {
                    result = false;
                } else if (matchTransDesc && matchRelatedTradeId != null && matchRelatedTradeId.ManagerGroup.Equals("BOTH")) {
                    result = true;
                } else if (matchTransDesc && matchRelatedTradeId != null && matchRelatedTradeId.ManagerGroup.Equals("MIFL") && IsMIFL(fund)) {
                    result = true;
                } else if (matchTransDesc && matchRelatedTradeId != null && matchRelatedTradeId.ManagerGroup.Equals("DIM") && IsDIM(fund)) {
                    result = true;
                }
                logger.log(Severity.debug, $"transDesc={transDesc}, relatedTradeId={relatedTradeId}, fund={fund}, result = {result}");
                return result;
            }
        }

        private static bool IsMiscCash(string transDesc, string ssTransType, string fund)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                LoadMiscCash();
                var matchAll = MiscCash.FirstOrDefault(x => x.TransDesc.Equals(transDesc) && x.SSTransType.Equals(ssTransType));
                var matchTransDesc = MiscCash.FirstOrDefault(x => x.TransDesc.Equals(transDesc));
                var matchSSTransType = MiscCash.FirstOrDefault(x => x.SSTransType.Equals(ssTransType));
                bool result = false;
                if (matchAll != null && string.IsNullOrEmpty(matchAll.ManagerGroup)) {
                    result = false;
                } else if (matchAll != null && matchAll.ManagerGroup.Equals("BOTH")) {
                    result = true;
                } else if (matchAll != null && matchAll.ManagerGroup.Equals("MIFL") && IsMIFL(fund)) {
                    result = true;
                } else if (matchAll != null && matchAll.ManagerGroup.Equals("DIM") && IsDIM(fund)) {
                    result = true;
                } else if (matchTransDesc != null && string.IsNullOrEmpty(matchTransDesc.ManagerGroup)) {
                    result = false;
                } else if (matchTransDesc != null && matchTransDesc.ManagerGroup.Equals("BOTH")) {
                    result = true;
                } else if (matchTransDesc != null && matchTransDesc.ManagerGroup.Equals("MIFL") && IsMIFL(fund)) {
                    result = true;
                } else if (matchTransDesc != null && matchTransDesc.ManagerGroup.Equals("DIM") && IsDIM(fund)) {
                    result = true;
                } else if (matchSSTransType != null && string.IsNullOrEmpty(matchSSTransType.ManagerGroup)) {
                    result = false;
                } else if (matchSSTransType != null && matchSSTransType.ManagerGroup.Equals("BOTH")) {
                    result = true;
                } else if (matchSSTransType != null && matchSSTransType.ManagerGroup.Equals("MIFL") && IsMIFL(fund)) {
                    result = true;
                } else if (matchSSTransType != null && matchSSTransType.ManagerGroup.Equals("DIM") && IsDIM(fund)) {
                    result = true;
                }
                logger.log(Severity.debug, $"transDesc={transDesc}, ssTransType={ssTransType}, fund={fund}, result = {result}");
                return result;
            }
        }

        private static string ReverseSign(string amountStr)
        {
            return amountStr.StartsWith("-") ? amountStr.Substring(1) : "-" + amountStr;
        }

        public CSxAllCustodyTransCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);
                string _SSStatus = fields.GetValue(RBCTicketType.AllCustodyTransColumns.SSStatus);
                DateTime _MainframeTimestamp = DateTime.ParseExact(fields.GetValue(RBCTicketType.AllCustodyTransColumns.MainframeTimeStamp), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                string _TransactionDescription = fields.GetValue(RBCTicketType.AllCustodyTransColumns.TransactionDescription);
                string _Fund = fields.GetValue(RBCTicketType.AllCustodyTransColumns.Fund).Trim();
                string _SSTradeID = fields.GetValue(RBCTicketType.AllCustodyTransColumns.SSTradeID).Trim();
                string _ActualPaySettleDate = fields.GetValue(RBCTicketType.AllCustodyTransColumns.ActualPaySettleDate).Trim();
                string _TradeRecordDate = fields.GetValue(RBCTicketType.AllCustodyTransColumns.TradeRecordDate).Trim();
                string _RelatedTradeID = fields.GetValue(RBCTicketType.AllCustodyTransColumns.RelatedTradeID).Trim();
                string _Currency = fields.GetValue(RBCTicketType.AllCustodyTransColumns.Currency);
                string _ActualNetAmount = fields.GetValue(RBCTicketType.AllCustodyTransColumns.ActualNetAmount);
                string _SSTransType = fields.GetValue(RBCTicketType.AllCustodyTransColumns.SSTransType);
                string _Isin = fields.GetValue(RBCTicketType.AllCustodyTransColumns.Isin);
                logger.log(Severity.debug, $"Processing line: _SSStatus={_SSStatus}, _TransactionDescription={_TransactionDescription}, _Fund={_Fund}, _SSTradeID={_SSTradeID}, _ActualPaySettleDate={_ActualPaySettleDate}, _TradeRecordDate={_TradeRecordDate}, _RelatedTradeID={_RelatedTradeID}, _Currency={_Currency}, _ActualNetAmount={_ActualNetAmount}, _SSTransType={_SSTransType}, _Isin={_Isin}");
                /*
                if (_SSStatus != "PAID") {
                    logger.log(Severity.warning, "Ignoring not PAID ticket");
                    return true;
                }
                if (_TransactionDescription == "INTERNAL TRANSACTION")
                {
                    logger.log(Severity.warning, "Ignoring INTERNAL TRANSACTION ticket");
                    return true;
                }
                if (string.IsNullOrEmpty(_Fund) || string.IsNullOrEmpty(_ActualPaySettleDate) || string.IsNullOrEmpty(_Currency)
                    || string.IsNullOrEmpty(_ActualNetAmount) || string.IsNullOrEmpty(_SSTradeID))
                {
                    logger.log(Severity.error, "One of the fields (Fund, ActualPaySettleDate, Currency, ActualNetAmount, SSTradeID) is empty");
                    return true;
                }
                string uniqueKeyVal = $"{_Fund}@{_SSTradeID}@{_TransactionDescription}@{_ActualPaySettleDate}";
                if (uniqueKeyValues.Contains(uniqueKeyVal))
                {
                    logger.log(Severity.error, $"Found duplicate key = {uniqueKeyVal}");
                    return true;
                }

                //Check Mainframe Timestamp
                if (_MainframeTimestamp <= RBCCustomParameters.Instance.SSBAllCustLastMfTS)
                {
                    logger.log(Severity.error, $"MainframeTimestamp is earlier than the last one. {_MainframeTimestamp} <= {RBCCustomParameters.Instance.SSBAllCustLastMfTS}");
                    return true;
                }
                if (_MainframeTimestamp > RBCCustomParameters.Instance.SSBAllCustMaxMfTS)
                {
                    RBCCustomParameters.Instance.SSBAllCustMaxMfTS = _MainframeTimestamp;
                    CSxRBCHelper.SaveCurMaxMainframeTimestamp(RBCCustomParameters.Instance.SSBAllCustMaxMfTS);
                    logger.log(Severity.debug, $"Updating CurMaxMfTS ({RBCCustomParameters.Instance.SSBAllCustMaxMfTS})");
                }
                */

                bool isMiroir = _Fund.StartsWith("MIR_");
                if (isMiroir)
                {
                    _Fund = _Fund.Substring(4);
                    logger.log(Severity.debug, $"Loading Miroir Fund={_Fund}...");
                }
                string ExtFundId = _Fund;

                //bool isCollateral = false;
                //bool isMargin = false;
                //bool isMiscCash = false;
                if ($",{RBCCustomParameters.Instance.SSBAllCustCollateralCashRelatedTradeIDs},".IndexOf($",{_RelatedTradeID.WithMaxLength(4)},") >= 0)
                {
                    //Collateral Cash
                    //isCollateral = true;
                    logger.log(Severity.debug, "Loading Collateral Cash...");
                    ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, 
                        isMiroir ? _BusinessEvents[RBCCustomParameters.Instance.CollateralOutBusinessEvent] :
                        _BusinessEvents[RBCCustomParameters.Instance.CashTransferBusinessEvent]); //Possible Miroir
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, isMiroir ? ReverseSign(_ActualNetAmount) : _ActualNetAmount); //Possible Miroir
                    ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, "Collateral");
                    int folioID = 0;
                    if (isMiroir)
                    {
                        OracleParameter parameter = new OracleParameter(":ACCOUNT_AT_CUSTODIAN", ExtFundId);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                        folioID = Convert.ToInt32(CSxDBHelper.GetOneRecord(@"
SELECT IDENT FROM FOLIO F WHERE NAME='Collateral Posted'
START WITH IDENT IN (
    SELECT MGR FROM FOLIO WHERE IDENT = (SELECT ACCOUNT_LEVEL_FOLIO FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = :ACCOUNT_AT_CUSTODIAN))
CONNECT BY PRIOR IDENT = MGR", parameters));
                        ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, folioID);
                    }
                    else
                    {
                        folioID = SetCashFolioID(ref ticketMessage, ExtFundId);
                    }
                    int sicovam = GetCashInstrumentSicovam(_Currency, RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, ExtFundId, ExtFundId, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, folioID, null, ExtFundId);
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                    //}
                    //else if ($",{RBCCustomParameters.Instance.SSBAllCustCollateralCashRelatedTradeIDs},".IndexOf($",{_RelatedTradeID.WithMaxLength(4)},") >= 0)
                    //{
                    //    //Margin
                    //    isMargin = true;
                    //    logger.log(Severity.debug, "Loading Margin...");
                    //    ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, _BusinessEvents["Clearer Margin Call"]);
                }
                else if (!isMiroir && IsCleanCash(_TransactionDescription, _RelatedTradeID.WithMaxLength(4), _Fund))
                {
                    logger.log(Severity.debug, "Loading Clean Cash...");
                    ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, _BusinessEvents["Misc Cash"]);
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, _ActualNetAmount);
                    ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, _TransactionDescription + " " + _RelatedTradeID);

                    int folioID = SetCashFolioID(ref ticketMessage, ExtFundId);
                    int sicovam = GetCashInstrumentSicovam(_Currency, RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, ExtFundId, ExtFundId, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, folioID, null, ExtFundId);
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                }
                else if (!isMiroir && IsMiscCash(_TransactionDescription, _SSTransType, _Fund))
                {
                    //MiscCash
                    //isMiscCash = true;
                    logger.log(Severity.debug, "Loading Miscellaneous Cash...");
                    ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, _BusinessEvents["Misc Cash"]);
                    ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, _TransactionDescription);

                    int folioID = SetCashFolioID(ref ticketMessage, ExtFundId);
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, _ActualNetAmount);

                    SSMComplexReference ISIN = new SSMComplexReference();
                    ISIN.type = "ISIN";
                    ISIN.value = _Isin;
                    //force price type= "In Amount"
                    ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, transaction.SpotTypeConstants.IN_PRICE);
                    int sico = CSMInstrument.GetCodeWithMultiReference(ISIN);
                    ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sico);

                    //ticketMessage.SetTicketField(FieldId.MA_INSTRUMENT_NAME, _Isin);
                }
                else
                {
                    logger.log(Severity.debug, "Unknown line...");
                    return true;
                }

                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, _SSTradeID);
                SetDepositary(ref ticketMessage, ExtFundId);
                SetBrokerID(ref ticketMessage, ExtFundId);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType);
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 1);
                ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, _Currency);

                int tradeDate = (string.IsNullOrEmpty(_TradeRecordDate) ? _ActualPaySettleDate : _TradeRecordDate).GetDateInAnyFormat(CSxRBCHelper.SSBAllCustodyDateFormats);
                int settlDate = _ActualPaySettleDate.GetDateInAnyFormat(CSxRBCHelper.SSBAllCustodyDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlDate);
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);

                //string generatedHash = GenerateSha1Hash(fields);
                string generatedHash = GenerateSha1Hash(new List<string> { isMiroir ? "MIR" : "ORG", _Fund, _SSTradeID, _TransactionDescription, _ActualPaySettleDate });
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, generatedHash);

                //if (!uniqueKeyValues.Contains(uniqueKeyVal)) uniqueKeyValues.Add(uniqueKeyVal);

                return false;
            }
        }
        
    }
}

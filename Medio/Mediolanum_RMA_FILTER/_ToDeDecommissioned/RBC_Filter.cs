using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichMarketAdapter.interfaces;
using RichMarketAdapter.ticket;
using sophis.utils;
using transaction;
using sophis.portfolio;
using sophis.value;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using System.Collections;
using System.Runtime.InteropServices;
using sophis.misc;
using sophis.static_data;
using System.Diagnostics;

using CorporateActionUtil;
using ForexUtils;
using System.Reflection;
using System.Security.Cryptography;
using Mediolanum_RMA_FILTER.Filters;
using Mediolanum_RMA_FILTER.Tools;
using MEDIO.CORE.Tools;
using sophis.value;
using sophis.backoffice_kernel;

namespace Mediolanum_RMA_FILTER
{
    class RBC_Filter : IFilter
    {
        #region Paramters
        protected static bool IsInitialized = false;
        protected static string ToolkitDefaultUniversal = "TICKER"; //Custom parameter
        protected static string ToolkitBondUniversal = "ISIN"; //Custom parameter
        protected static bool useStrategySubfolios = false; //Custom parameter
        protected static bool validateGrossAmount = false; //Custom parameter
        protected static bool validateNetAmount = false; //Custom parameter
        protected static bool validateForexAmount = false; //Custom parameter
        protected static bool useDefaultDepositary = true; //Custom parameter
        protected static bool useDefaultCounterparty = true; //Custom parameter
        protected static bool overwriteBORemarks = false; //Custom parameter
        protected static int defaultDepositaryId = 0; //Custom parameter
        protected static int defaultCounterpartyId = 0; //Custom parameter
        protected static int defaultFXHedgeCounterpartyId = 0; //Custom parameter
        protected static double grossAmountEpsilon = 0.1; //Custom parameter
        protected static double netAmountEpsilon = 0.1; //Custom parameter
        protected static double forexAmountEpsilon = 0.1; //Custom parameter
        protected static string CashTransferInstrumentNameFormat = "Cash for currency '%CURRENCY%'"; //Custom parameter
        protected static string InvoiceInstrumentNameFormat = "%FEETYPE% %CURRENCY%"; //Custom parameter
        protected static string TACashInstrumentNameFormat = "Cash for currency '%CURRENCY%'"; //Custom parameter
        protected static string defaultDepositaryStr = "RBC Custody";
        protected static string defaultCounterpartyStr = "DELEGATE";
        protected static string defaultFXHedgeCounterpartyIdStr = "FX Hedge Counterparty";
        protected static string CashTransferBusinessEvent = ""; 
        protected static string InterestPaymentBusinessEvent = ""; 
        protected static string InterestPaymentTypeName = "";
        protected static List<string> InterestPaymentTypeNameList = new List<string>() {"S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8"}; 
        protected static string InvoiceBusinessEvent = ""; 
        protected static string TACashBusinessEvent = ""; 
        protected static string CADefaultBusinessEvent = ""; 
        protected static bool CommentUpdaterEnabled = false;
        protected static int CommentUpdaterDelay = 300; //Custom parameter
        protected static string CommentUpdaterSource = "RBCUploader"; //Custom parameter
        protected static int DefaultErrorFolio = 0; //Custom parameter
        protected static int DefaultErrorInstrumentID = 0;
        protected static string FundBloombergRequestType = "Equity"; //Custom parameter
        protected static List<string> MAMLZCodesList; //Custom parameter
        protected static Dictionary<CorporateActionType, string> CorporateActionsBusinessEvents; //Custom parameter
        protected static string CurrenciesForSharesPricedInSubunits = ""; //Custom parameter
        protected static List<KeyValuePair<string, string>> CurrenciesForSharesPricedInSubunitsList;
        protected static string RBCTransactionIDName = "TKT_RBC_TRADE_ID";
        public static Dictionary<string, int> rootportfolios;
        public static List<Tuple<int, int, string>> subfolios; //RootPortfolioID,SubfolioID,SubfolioName
        public static List<Tuple<int, int, string>> delegatemanagers; //accountid,entityid,account_at_custodian
        public static Dictionary<string, int> businessevents;
        public static ArrayList fund_list;
        int DefaultSpotType = transaction.SpotTypeConstants.IN_PRICE;
        int BondSpotType = transaction.SpotTypeConstants.IN_PERCENTAGE;
        protected static string DefaultBOKernelEvent = "New deal accept"; //Custom parameter
        public static string ExtFundIdFilterFile = ""; //Custom parameter
        public static List<string> AllowedExtFundIds;
        public static List<string> UnacceptedTransactionTypeList;
        public static string DelegateTradeCreationEvent = ""; //Custom parameter
        public static string MAMLTradeCreationEvent = ""; //Custom parameter
        public static int DelegateTradeCreationEventId = 0;
        public static int MAMLTradeCreationEventId = 0;
        public static bool OverrideCreationEvent = false; //Custom parameter
        protected static List<string> CorporateActionReversalCodeList = new List<string>(); //Custom parameter
        private static PostitUpdater postitUpdater;

        public Dictionary<string, int> GetBusinessEventsList()
        {
            Dictionary<string, int> retval = new Dictionary<string, int>();
            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = "SELECT ID,NAME FROM BUSINESS_EVENTS";
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = (string)reader["NAME"];
                            int ident = Convert.ToInt32(reader["ID"]);
                            retval[name] = ident;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "GetAccountList", CSMLog.eMVerbosity.M_error, "Error occurred while trying to read business events from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return retval;
        }

        public List<Tuple<int, int, string>> GetDelegateManagers()
        {
            List<Tuple<int, int, string>> retval = new List<Tuple<int, int, string>>();
            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = "SELECT R.ACC_ID, R.VALUE, A.ACCOUNT_AT_CUSTODIAN FROM  BO_TREASURY_ACCOUNT A, BO_TREASURY_EXT_REF R, BO_TREASURY_EXT_REF_DEF D WHERE A.ID = R.ACC_ID AND R.REF_ID = D.REF_ID AND D.REF_NAME = 'DelegateManagerID'";
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int accountid = reader["ACC_ID"] != DBNull.Value? Convert.ToInt32(reader["ACC_ID"]) : 0;
                            int entityid = reader["VALUE"] != DBNull.Value ? Convert.ToInt32(reader["VALUE"]) : 0;
                            string account = reader["ACCOUNT_AT_CUSTODIAN"] != DBNull.Value ? (string)reader["ACCOUNT_AT_CUSTODIAN"] : "";
                            if (!string.IsNullOrEmpty(account))
                            {
                                retval.Add(new Tuple<int, int, string>(accountid, entityid, account));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "GetDelegateManagers", CSMLog.eMVerbosity.M_error, "Error occurred while trying to read delegate managers from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return retval;
        }

        public Dictionary<string, int> GetAccountList()
        {
            Dictionary<string, int> retval = new Dictionary<string, int>();
            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = "SELECT A.ACCOUNT_AT_CUSTODIAN, R.VALUE AS ROOTPORTFOLIO FROM BO_TREASURY_ACCOUNT A, BO_TREASURY_EXT_REF R, BO_TREASURY_EXT_REF_DEF D  WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ID = R.ACC_ID AND R.REF_ID = D.REF_ID AND D.REF_NAME = 'RootPortfolio'";
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string account = (string)reader["ACCOUNT_AT_CUSTODIAN"];
                            int folio = Convert.ToInt32(reader["ROOTPORTFOLIO"]);
                            retval[account] = folio;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                CSMLog.Write("RBC_Filter", "GetAccountList", CSMLog.eMVerbosity.M_error, "Error occurred while trying to read accounts from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return retval;
        }

        public List<Tuple<int, int, string>> GetSubFolioDictionary()
        {
            List<Tuple<int, int, string>> retval = new List<Tuple<int, int, string>>();
            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = "SELECT NAME,MGR,IDENT FROM FOLIO WHERE MGR in (SELECT R.VALUE AS ROOTPORTFOLIO FROM BO_TREASURY_ACCOUNT A, BO_TREASURY_EXT_REF R, BO_TREASURY_EXT_REF_DEF D  WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ID = R.ACC_ID AND R.REF_ID = D.REF_ID AND D.REF_NAME = 'RootPortfolio')";
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int mgr = Convert.ToInt32(reader["MGR"]);
                            int ident = Convert.ToInt32(reader["IDENT"]);
                            string name = (string)reader["NAME"];
                            if (name != null)
                            {
                                retval.Add(new Tuple<int, int, string>(mgr, ident, name));
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                CSMLog.Write("RBC_Filter", "GetSubFolioDictionary", CSMLog.eMVerbosity.M_error, "Error occurred while trying to read subfolios from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return retval;
        }

        public void LoadBuffer()
        {
            rootportfolios = GetAccountList();
            if (useStrategySubfolios)
            {
                subfolios = GetSubFolioDictionary();
            }
            else
            {
                subfolios = new List<Tuple<int, int, string>>();
            }
            delegatemanagers = GetDelegateManagers();
            businessevents = GetBusinessEventsList();
            fund_list = new ArrayList();
            CSMAmFund.GetInternalFundList(fund_list, true);
        }

        public string GetDefaultParametersString()
        {
            StringBuilder strb = new StringBuilder("Loaded config values: UseStrategyFolios=");
            strb.Append(useStrategySubfolios);
            strb.Append(" ValidateGrossAmount=");
            strb.Append(validateGrossAmount);
            strb.Append(" ValidateNetAmount=");
            strb.Append(validateNetAmount);
            strb.Append(" ValidateForexAmount=");
            strb.Append(validateForexAmount);
            strb.Append(" GrossAmountEpsilon=");
            strb.Append(grossAmountEpsilon);
            strb.Append(" NetAmountEpsilon=");
            strb.Append(netAmountEpsilon);
            strb.Append(" ForexAmountEpsilon=");
            strb.Append(forexAmountEpsilon);
            strb.Append(" UseDefaultDepositary=");
            strb.Append(useDefaultDepositary);
            strb.Append(" UseDefaultCounterparty=");
            strb.Append(useDefaultCounterparty);
            strb.Append(" DefaultCounterparty=");
            strb.Append(defaultCounterpartyId);
            strb.Append(" DefaultDepositary=");
            strb.Append(defaultDepositaryId);
            strb.Append(" ReplaceBORemarks=");
            strb.Append(overwriteBORemarks);

            strb.Append(" CashTransferInstrumentNameFormat=");
            strb.Append(CashTransferInstrumentNameFormat);
            strb.Append(" InvoiceInstrumentNameFormat=");
            strb.Append(InvoiceInstrumentNameFormat);
            strb.Append(" TACashInstrumentNameFormat=");
            strb.Append(TACashInstrumentNameFormat);

            strb.Append(" DefaultErrorFolio=");
            strb.Append(DefaultErrorFolio);

            strb.Append(" MAMLZCodes = [ ");
            for (int i = 0; i < MAMLZCodesList.Count; i++)
            {
                strb.Append(MAMLZCodesList[i]);
                strb.Append(" ");
            }
            strb.Append("]");

            strb.Append(" CurrenciesForSharesPricedInSubunitsList = [ ");
            for (int i = 0; i < CurrenciesForSharesPricedInSubunitsList.Count; i++)
            {
                strb.Append(CurrenciesForSharesPricedInSubunitsList[i]);
                strb.Append(" ");
            }
            strb.Append("]");

            return strb.ToString();
        }
        #endregion

        public void Initialise()
        {
            try
            {
                string grossAmountEpsilonStr = "0.1";
                string netAmountEpsilonStr = "0.1";
                string forexAmountEpsilonStr = "0.01";
                string MAMLZCodes = "";
                string CorporateActionReversalCodes = "";

                CSxValidationUtil.initApi();
                LoadBuffer();
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "UseStrategyFolios", ref useStrategySubfolios, false);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ValidateGrossAmount", ref validateGrossAmount, false);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ValidateNetAmount", ref validateNetAmount, false);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "GrossAmountEpsilon", ref grossAmountEpsilonStr, "0.1");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "NetAmountEpsilon", ref netAmountEpsilonStr, "0.1");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ValidateForexAmount", ref validateForexAmount, true);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ForexAmountEpsilon", ref forexAmountEpsilonStr, "0.01");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "UseDefaultCounterparty", ref useDefaultCounterparty, false);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "UseDefaultDepositary", ref useDefaultDepositary, true);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ReplaceBORemarks", ref overwriteBORemarks, false);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "DefaultDepositary", ref defaultDepositaryStr, "RBC Custody");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "DefaultCounterparty", ref defaultCounterpartyStr, "DELEGATE");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "DefaultHedgeCounterparty", ref defaultFXHedgeCounterpartyIdStr, "FX Hedge Counterparty");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "MAMLZCodes", ref MAMLZCodes, "Z8719429;Z8730529;Z8730528;Z8894216;Z8730525;Z5950209;Z8894216;Z8730525;Z5950209;Z5950206;Z5950206");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ToolkitDefaultUniversal", ref ToolkitDefaultUniversal, "TICKER");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ToolkitBondUniversal", ref ToolkitBondUniversal, "ISIN");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "DefaultErrorFolio", ref DefaultErrorFolio, 0);
                Double.TryParse(grossAmountEpsilonStr, out grossAmountEpsilon);
                Double.TryParse(netAmountEpsilonStr, out netAmountEpsilon);
                Double.TryParse(forexAmountEpsilonStr, out forexAmountEpsilon);

                MAMLZCodesList = CSxValidationUtil.splitCSV(MAMLZCodes);

                defaultCounterpartyId = GetEntityIDByName(defaultCounterpartyStr);
                defaultDepositaryId = GetEntityIDByName(defaultDepositaryStr);
                defaultFXHedgeCounterpartyId = GetEntityIDByName(defaultFXHedgeCounterpartyIdStr);

                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CashTransferInstrumentNameFormat", ref CashTransferInstrumentNameFormat, "Cash for currency '%CURRENCY%'");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "InvoiceInstrumentNameFormat", ref InvoiceInstrumentNameFormat, "%FEETYPE% %CURRENCY%");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "TACashInstrumentNameFormat", ref TACashInstrumentNameFormat, "S/R for fund '%FUND%'");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CashTransferBusinessEvent", ref CashTransferBusinessEvent, "Cash Transfer");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "InterestPaymentBusinessEvent", ref InterestPaymentBusinessEvent, "Interest Payment");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "InterestPaymentTypeName", ref InterestPaymentTypeName, "Interest Payment");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "InvoiceBusinessEvent", ref InvoiceBusinessEvent, "Fee");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "TACashBusinessEvent", ref TACashBusinessEvent, "Subscription/Redemption");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CADefaultBusinessEvent", ref CADefaultBusinessEvent, "CA Adjustment (Securities)");

                string SpinOffBusinessEvent = "";
                string MergerBusinessEvent = "";
                string AmalgamationBusinessEvent = "";
                string BonusOrScripIssueBusinessEvent = "";
                string SubscriptionBusinessEvent = "";
                string RightsEntitlementBusinessEvent = "";
                string SecuritiesWithdrawalBusinessEvent = "";
                string ReverseSplitBusinessEvent = "";
                string SecuritiesDepositBusinessEvent = "";
                string ConversionBusinessEvent = "";
                string CashCreditBusinessEvent = "";
                string CapitalReductionWithPaymentBusinessEvent = "";
                string CapitalIncreaseVsPaymentBusinessEvent = "";
                string DistributionInSecuritiesBusinessEvent = "";
                string PublicPurchaseOfferBusinessEvent = "";
                string ExchangeOrExchangeOfferBusinessEvent = "";
                string AdditionalSplitBusinessEvent = "";
                string SplitBusinessEvent = "";
                string CashDebitBusinessEvent = "";
                string DividendBusinessEvent = "";
                string InterestBusinessEvent = "";
                string FinalRedemptionBusinessEvent = "";
                string EarlyRedemptionBusinessEvent = "";
                string WarrantExerciseBusinessEvent = "";
                string UnacceptedTransactions = "";

                CSMConfigurationFile.getEntryValue("MediolanumRMA", "SpinOffBusinessEvent", ref SpinOffBusinessEvent, "SpinOff");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "MergerBusinessEvent", ref MergerBusinessEvent, "Merger");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "AmalgamationBusinessEvent", ref AmalgamationBusinessEvent, "Merger");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "BonusOrScripIssueBusinessEvent", ref BonusOrScripIssueBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "SubscriptionBusinessEvent", ref SubscriptionBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "RightsEntitlementBusinessEvent", ref RightsEntitlementBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "SecuritiesWithdrawalBusinessEvent", ref SecuritiesWithdrawalBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ReverseSplitBusinessEvent", ref ReverseSplitBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "SecuritiesDepositBusinessEvent", ref SecuritiesDepositBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ConversionBusinessEvent", ref ConversionBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CashCreditBusinessEvent", ref CashCreditBusinessEvent, "CA Adjustment (Cash)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CapitalReductionWithPaymentBusinessEvent", ref CapitalReductionWithPaymentBusinessEvent, "CA Adjustment (Cash)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CapitalIncreaseVsPaymentBusinessEvent", ref CapitalIncreaseVsPaymentBusinessEvent, "CA Adjustment (Cash)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "DistributionInSecuritiesBusinessEvent", ref DistributionInSecuritiesBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "PublicPurchaseOfferBusinessEvent", ref PublicPurchaseOfferBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ExchangeOrExchangeOfferBusinessEvent", ref ExchangeOrExchangeOfferBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "AdditionalSplitBusinessEvent", ref AdditionalSplitBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "SplitBusinessEvent", ref SplitBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CashDebitBusinessEvent", ref CashDebitBusinessEvent, "CA Adjustment (Cash)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "DividendBusinessEvent", ref DividendBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "InterestBusinessEvent", ref InterestBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "FinalRedemptionBusinessEvent", ref FinalRedemptionBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "EarlyRedemptionBusinessEvent", ref EarlyRedemptionBusinessEvent, "Early Redemption");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "WarrantExerciseBusinessEvent", ref WarrantExerciseBusinessEvent, "CA Adjustment (Securities)");
                CorporateActionsBusinessEvents = new Dictionary<CorporateActionType, string>()
                {
                    {CorporateActionType.Subscription,SubscriptionBusinessEvent},
                    {CorporateActionType.RightsEntitlement,RightsEntitlementBusinessEvent},
                    {CorporateActionType.SecuritiesWithdrawal,SecuritiesWithdrawalBusinessEvent},
                    {CorporateActionType.ReverseSplit,ReverseSplitBusinessEvent},
                    {CorporateActionType.SpinOff, SpinOffBusinessEvent},
                    {CorporateActionType.SecuritiesDeposit,SecuritiesDepositBusinessEvent},
                    {CorporateActionType.Conversion,ConversionBusinessEvent},
                    {CorporateActionType.CashCredit,CashCreditBusinessEvent},
                    {CorporateActionType.CapitalReductionWithPayment,CapitalReductionWithPaymentBusinessEvent},
                    {CorporateActionType.CapitalIncreaseVsPayment,CapitalIncreaseVsPaymentBusinessEvent},
                    {CorporateActionType.DistributionInSecurities,DistributionInSecuritiesBusinessEvent},
                    {CorporateActionType.BonusOrScripIssue, BonusOrScripIssueBusinessEvent},
                    {CorporateActionType.PublicPurchaseOffer,PublicPurchaseOfferBusinessEvent},
                    {CorporateActionType.ExchangeOrExchangeOffer,ExchangeOrExchangeOfferBusinessEvent},
                    {CorporateActionType.Merger, MergerBusinessEvent},
                    {CorporateActionType.AdditionalSplit,AdditionalSplitBusinessEvent},
                    {CorporateActionType.Split,SplitBusinessEvent},
                    {CorporateActionType.CashDebit,CashDebitBusinessEvent},
                    {CorporateActionType.Dividend,DividendBusinessEvent},
                    {CorporateActionType.Interest,InterestBusinessEvent},
                    {CorporateActionType.FinalRedemption,FinalRedemptionBusinessEvent},
                    {CorporateActionType.EarlyRedemption,EarlyRedemptionBusinessEvent},
                    {CorporateActionType.Amalgamation, AmalgamationBusinessEvent},
                    {CorporateActionType.WarrantExercise,WarrantExerciseBusinessEvent},
                };
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "FundBloombergRequestType", ref FundBloombergRequestType, "Equity");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "SharesPricedInSubunits", ref CurrenciesForSharesPricedInSubunits, "");
                CurrenciesForSharesPricedInSubunitsList = new List<KeyValuePair<string, string>>();
                foreach (var item in CSxValidationUtil.splitCSV(CurrenciesForSharesPricedInSubunits).ToList())
                {
                    var currencyMarket = item.Split('-').ToList();
                    if (currencyMarket.Count == 1)
                    {
                        CurrenciesForSharesPricedInSubunitsList.Add(new KeyValuePair<string, string>(currencyMarket[0], ""));
                    }
                    if (currencyMarket.Count == 2)
                    {
                        CurrenciesForSharesPricedInSubunitsList.Add(new KeyValuePair<string, string>(currencyMarket[0], " " + currencyMarket[1] + " ")); // matched by space + string + space, eg " LN "
                    }
                }
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "UnacceptedTransactionTypes", ref UnacceptedTransactions, "");

                UnacceptedTransactionTypeList = CSxValidationUtil.splitCSV(UnacceptedTransactions); // split ';' delimited strings containing currencies and remove spaces
                if (UnacceptedTransactionTypeList != null)
                    UnacceptedTransactionTypeList = UnacceptedTransactionTypeList.ConvertAll(s => s.ToUpper());
                else
                    UnacceptedTransactionTypeList = new List<string>();

                CSMConfigurationFile.getEntryValue("MediolanumRMA", "DefaultBOKernelEvent", ref DefaultBOKernelEvent, "New deal accept");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "MAMLTradeCreationEvent", ref MAMLTradeCreationEvent, "");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "DelegateTradeCreationEvent", ref DelegateTradeCreationEvent, "");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "OverrideCreationEvent", ref OverrideCreationEvent, false);

                MAMLTradeCreationEventId = GetKernelWorkflowEventIDByName(MAMLTradeCreationEvent);
                DelegateTradeCreationEventId = GetKernelWorkflowEventIDByName(DelegateTradeCreationEvent);

                CSMConfigurationFile.getEntryValue("MediolanumRMA", "ExtFundIdFilterFile", ref ExtFundIdFilterFile, "");
                AllowedExtFundIds = new List<string>();
                bool fileReadSuccess = true;
                if (!String.IsNullOrEmpty(ExtFundIdFilterFile))
                {
                    string line;
                    try
                    {
                        System.IO.StreamReader file = new System.IO.StreamReader(@ExtFundIdFilterFile);
                        while ((line = file.ReadLine()) != null)
                        {
                            if (!String.IsNullOrEmpty(line))
                            {
                                string trimmedLine = line.Trim();
                                if (!String.IsNullOrEmpty(trimmedLine))
                                {
                                    AllowedExtFundIds.Add(trimmedLine.ToUpper());
                                }
                            }
                        }
                        file.Close();
                    }
                    catch (Exception ex)
                    {
                        fileReadSuccess = false;
                        CSMLog.Write("RBC_Filter", "RBC_Filter()", CSMLog.eMVerbosity.M_error, "Failed to read allowed external fund identifiers from file: " + @ExtFundIdFilterFile + " . Exception: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                else
                {
                    fileReadSuccess = false;
                }
                if (fileReadSuccess)
                {
                    CSMLog.Write("RBC_Filter", "RBC_Filter()", CSMLog.eMVerbosity.M_info, "Successfully read " + AllowedExtFundIds.Count + " external fund identifiers from: " + @ExtFundIdFilterFile);
                }

                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CommentUpdaterEnabled", ref CommentUpdaterEnabled, false);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CommentUpdaterDelay", ref CommentUpdaterDelay, 300);
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CommentUpdaterSource", ref CommentUpdaterSource, "RBCUploader");
                CSMConfigurationFile.getEntryValue("MediolanumRMA", "CorporateActionReversalCodes", ref CorporateActionReversalCodes, "");
                CorporateActionReversalCodeList = CSxValidationUtil.splitCSV(CorporateActionReversalCodes); 
                
                postitUpdater = new PostitUpdater(CommentUpdaterEnabled, CommentUpdaterDelay, CommentUpdaterSource);

                CSMLog.Write("RBC_Filter", "RBC_Filter()", CSMLog.eMVerbosity.M_debug, GetDefaultParametersString());

                //sophis.HandleEventDelegate delegateHandler = CoherencyEventHandler;
                //sophis.CSMEventManager.Instance.AddHandler("AJTI", delegateHandler);
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "RBC_Filter()", CSMLog.eMVerbosity.M_error, "Failed to initialize RBC_Filter filter: " + ex.Message + ". InnerException: " + ex.InnerException + ". Stack trace: " + ex.StackTrace);
            }
            finally
            {
                IsInitialized = true;
            }
        }
        /* TODO: Update buffers on coherency events
        public void CoherencyEventHandler(string in_eventType, object in_event)
        {
            CSMLog.Write("RBC_Filter", "CoherencyEventHandler()", CSMLog.eMVerbosity.M_debug, "Received coherency event: " + in_eventType);
        }
        */
        public virtual bool filter(IMessageWrapper message)
        {
            if(!IsInitialized)
            {
                Initialise();
            }
            DateTime startTime = DateTime.Now;
            //bool isReversal = false;
            if (message == null)
            {
                return true;
            }
            ITicketMessage ticketMessage = null;
            try
            {
                ticketMessage = (ITicketMessage)message.Message;
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_error, "Failed to cast IMessageWrapper to ITicketMessage: " + ex.Message);
                return true;
            }
            if (ticketMessage == null)
            {
                return true;
            }
            //Validate CSV document class
            List<string> fields = CSxValidationUtil.splitCSV(ticketMessage.TextData);
            CSVDocumentClass documentClass = CSxValidationUtil.GetDocumentClass(fields);
            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_debug, "CSV document class: " + Enum.GetName(typeof(CSVDocumentClass), documentClass));

            ticketMessage.remove(FieldId.QUANTITY_PROPERTY_NAME); //Quantity
            ticketMessage.remove(FieldId.SPOT_PROPERTY_NAME); // Price
            ticketMessage.remove(FieldId.INSTRUMENT_SOURCE); //UNIVERSAL 'external' mapping value
            ticketMessage.remove(FieldId.MA_COMPLEX_REFERENCE_TYPE); //UNIVERSAL (ISIN, BLOOMBERG, etc.)
            ticketMessage.remove(FieldId.INSTRUMENTTYPE_PROPERTY_NAME); //Bond, Equity, Forex, etc.
            ticketMessage.remove(FieldId.MA_INSTRUMENT_NAME); //ISIN Code or BLOOMBERG Code (depends on MA_COMPLEX_REFERENCE_TYPE)
            ticketMessage.remove(FieldId.NEGOTIATIONDATE_PROPERTY_NAME); //Trade date
            int reversal_flag_column_id = -1;
            bool reversalFlag = false;
            //int generic_quantity_column_id = -1;

            switch (documentClass)
            {
                case CSVDocumentClass.Bond:
                case CSVDocumentClass.Equity:
                case CSVDocumentClass.Loan:
                case CSVDocumentClass.Fund:
                    {
                        #region Populating fields
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.TransactionID]));
                        reversal_flag_column_id = CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ReversalFlag];
                        string reversalStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ReversalFlag]);
                        if (reversalStr.ToUpper().Equals("Y"))
                        {
                            reversalFlag = true;
                        }
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.STOCK);
                        if (documentClass != CSVDocumentClass.Bond)
                        {
                            TrySetDoubleFromList(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Quantity]);
                        }
                        else
                        {
                            //TrySetDoubleFromList(ref ticketMessage, FieldId.NOTIONAL_PROPERTY_NAME, fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Quantity],reversalFlag);
                            //28/09/2017: S Amet & C Benyahia
                            //Reversal flag removed after non vanilla fund migration 
                            TrySetDoubleFromList(ref ticketMessage, FieldId.NOTIONAL_PROPERTY_NAME, fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Quantity]);
                        }
                        string instrument_name_fields = CSxValidationUtil.TryAccessListValue(fields, (documentClass == CSVDocumentClass.Equity || documentClass == CSVDocumentClass.Fund) ? (CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.BloombergCode]) : (CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ISINCode]));
                        string ccy = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Currency]);
                        string spotStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Price]);
                        string grossAmountStr = CSxValidationUtil.TryAccessListValue(fields,
                            CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.GrossAmount]);
                        string quantityStr = CSxValidationUtil.TryAccessListValue(fields,
                           CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Quantity]);
                        double spot = 0.0;
                        double grossAmount = 0.0;
                        double quantity = 0.0;
                        Double.TryParse(spotStr, out spot);
                        Double.TryParse(grossAmountStr, out grossAmount);
                        Double.TryParse(quantityStr, out quantity);
                        spot = quantity != 0.0 ? grossAmount / quantity : spot;
                        if ((documentClass == CSVDocumentClass.Fund || documentClass == CSVDocumentClass.Equity))
                        {
                            foreach (var item in CurrenciesForSharesPricedInSubunitsList)
                            {
                                if (item.Key == ccy.ToUpper() && instrument_name_fields.Contains(item.Value))
                                {
                                    spot *= 100; break;
                                }
                            }
                        }
                        else if (documentClass == CSVDocumentClass.Bond)
                        {
                            spot *= 100;
                        }
                        
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, spot);
                        TrySetTicketField(ref ticketMessage, FieldId.INSTRUMENT_SOURCE, fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.SecurityDescription]);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.MA_COMPLEX_REFERENCE_TYPE, (documentClass == CSVDocumentClass.Equity || documentClass == CSVDocumentClass.Fund) ? ToolkitDefaultUniversal : ToolkitBondUniversal);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.INSTRUMENTTYPEHINT, (documentClass == CSVDocumentClass.Equity || documentClass == CSVDocumentClass.Fund) ? "Equity" : "Bond");
                        if(documentClass == CSVDocumentClass.Fund)
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.INSTRUMENTTYPEHINT, FundBloombergRequestType);
                        }
                        //TrySetTicketField(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, fields, (documentClass == CSVDocumentClass.Equity), CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.BloombergCode], CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ISINCode]);
                        
                        if (!String.IsNullOrEmpty(instrument_name_fields))
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, instrument_name_fields);
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_debug, "Instrument code: " + ((documentClass == CSVDocumentClass.Equity || documentClass == CSVDocumentClass.Fund) ? (Enum.GetName(typeof(InboundCSVFields), InboundCSVFields.BloombergCode)) : (Enum.GetName(typeof(InboundCSVFields), InboundCSVFields.ISINCode))) + " was null or empty");
                            TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, DefaultErrorInstrumentID);
                        }
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.TradeDate]), CSxValidationUtil.CommonDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.CommonDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField(ref ticketMessage, FieldId.BROKERFEES_PROPERTY_NAME, fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Brokerage]);
                        TrySetTicketField(ref ticketMessage, FieldId.MARKETFEES_PROPERTY_NAME, fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Tax]);
                        TrySetTicketField(ref ticketMessage, FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Expenses]);
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.SecurityDescription]));
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.BrokerBICCode]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.BrokerBICCode]), folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.BrokerBICCode]) + " - " + GetEntityNameByID(ctpy_id));
                        TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.Currency]);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SPOTTYPE_PROPERTY_NAME, (documentClass == CSVDocumentClass.Bond)?(BondSpotType):(DefaultSpotType));
                        if (documentClass == CSVDocumentClass.Loan)
                        {
                            TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTTYPE_PROPERTY_NAME, PositionTypeConstants.LENDED);
                        }

                        string side = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.TransactionDescription]);
                        if (!String.IsNullOrEmpty(side))
                        {
                            if (documentClass != CSVDocumentClass.Loan)
                            {
                                if (side.ToUpper().Equals("SECURITIES PURCHASE"))
                                {
                                    TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                                }
                                else if (side.ToUpper().Equals("SECURITIES SELL"))
                                {
                                    TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                                }
                            }
                            else
                            {
                                if (side.ToUpper().Equals("SECURITIES DIRECT ENTRY"))
                                {
                                    TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                                }
                                else if (side.ToUpper().Equals("SECURITIES WITHDRAWAL"))
                                {
                                    TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                                }
                            }
                        }
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.ExternalFundIdentifier]));
                        #endregion
                        #region Calculating Gross Amount
                        double receivedGrossAmount = 0.0;
                        double calculatedGrossAmount = 0.0;
                        if (validateGrossAmount || validateNetAmount) //Calculate gross amount
                        {
                            Double.TryParse(quantityStr, out quantity);
                            Double.TryParse(spotStr, out spot);
                            calculatedGrossAmount = quantity * spot;
                        }
                        #endregion

                        #region Validating Gross Amount
                        if (validateGrossAmount)
                        {
                            Double.TryParse(CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.GrossAmount]), out receivedGrossAmount);
                            if (grossAmountEpsilon < Math.Abs(calculatedGrossAmount - receivedGrossAmount))
                            {
                                CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "Gross amount validation failure: |calculatedGA - receivedGA| > " + grossAmountEpsilon + " , calculatedGA=" + calculatedGrossAmount + " receivedGA=" + receivedGrossAmount);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.BACKOFFICEINFOS_PROPERTY_NAME, "Warning: gross amount validation failure");
                                //return true;
                            }
                        }
                        #endregion

                        #region Validating Net Amount
                        double receivedNetAmount = 0.0;
                        double calculatedNetAmount = 0.0;
                        Double.TryParse(CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.NetAmount]), out receivedNetAmount);
                        if (validateNetAmount)
                        {
                            string brokerFeesStr = ticketMessage.getString(FieldId.BROKERFEES_PROPERTY_NAME);
                            string marketFeesStr = ticketMessage.getString(FieldId.MARKETFEES_PROPERTY_NAME);
                            string counterpartyFeesStr = ticketMessage.getString(FieldId.COUNTERPARTYFEES_PROPERTY_NAME);
                            double brokerFees = 0.0;
                            double marketFees = 0.0;
                            double counterpartyFees = 0.0;
                            Double.TryParse(brokerFeesStr, out brokerFees);
                            Double.TryParse(marketFeesStr, out marketFees);
                            Double.TryParse(counterpartyFeesStr, out counterpartyFees);
                            calculatedNetAmount = calculatedGrossAmount + brokerFees + marketFees + counterpartyFees;
                            if (netAmountEpsilon < Math.Abs(calculatedNetAmount - receivedNetAmount))
                            {
                                CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "Net amount validation failure: |calculatedNA - receivedNA| > " + netAmountEpsilon + " , calculatedNA=" + calculatedNetAmount + " receivedNA=" + receivedNetAmount);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.BACKOFFICEINFOS_PROPERTY_NAME, "Warning: net amount validation failure");
                                //return true;
                            }
                        }
                        #endregion

                    } break;
                case CSVDocumentClass.Bond2:
                    {
                        #region Populating fields
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.TransactionID]));
                        reversal_flag_column_id = CSxValidationUtil.Bond2Mappings[InboundCSVFields.ReversalFlag];
                        string reversalStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ReversalFlag]);
                        if (reversalStr.ToUpper().Equals("Y"))
                        {
                            reversalFlag = true;
                            //TrySetKind(ref ticketMessage, TicketType.CANCEL);
                        }
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.STOCK);

                        // TrySetDoubleFromList(ref ticketMessage, FieldId.NOTIONAL_PROPERTY_NAME, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.Quantity],reversalFlag);
                        //28/09/2017: S Amet & C Benyahia
                        //Reversal flag removed after non vanilla fund migration 
                        TrySetDoubleFromList(ref ticketMessage, FieldId.NOTIONAL_PROPERTY_NAME, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.Quantity]);
                        string spotStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.Price]);
                        string grossAmountStr = CSxValidationUtil.TryAccessListValue(fields,
                            CSxValidationUtil.Bond2Mappings[InboundCSVFields.GrossAmount]);
                        string quantityStr = CSxValidationUtil.TryAccessListValue(fields,
                           CSxValidationUtil.Bond2Mappings[InboundCSVFields.Quantity]);
                        double spot = 0.0;
                        double grossAmount = 0.0;
                        double quantity = 0.0;
                        Double.TryParse(spotStr, out spot);
                        Double.TryParse(grossAmountStr, out grossAmount);
                        Double.TryParse(quantityStr, out quantity);
                        spot = quantity != 0.0 ? (100 * grossAmount / quantity) : spot;
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, spot);
                        TrySetTicketField(ref ticketMessage, FieldId.INSTRUMENT_SOURCE, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.SecurityDescription]);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.MA_COMPLEX_REFERENCE_TYPE, ToolkitBondUniversal);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.INSTRUMENTTYPEHINT, "Bond");
                        //TrySetTicketField(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ISINCode]);
                        string isin_code = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ISINCode]);
                        if (!String.IsNullOrEmpty(isin_code))
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, isin_code);
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_debug, "Instrument code: " + Enum.GetName(typeof(InboundCSVFields), InboundCSVFields.ISINCode) + " was null or empty");
                            TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, DefaultErrorInstrumentID);
                        }
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.TradeDate]), CSxValidationUtil.CommonDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.CommonDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        //TrySetTicketField(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.BrokerName]);
                        TrySetTicketField(ref ticketMessage, FieldId.BROKERFEES_PROPERTY_NAME, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.Brokerage]);
                        TrySetTicketField(ref ticketMessage, FieldId.MARKETFEES_PROPERTY_NAME, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.Tax]);
                        TrySetTicketField(ref ticketMessage, FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.Expenses]);
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ExternalFundIdentifier]));
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.SecurityDescription]));
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.BrokerBICCode]));
                        int ctpy_id =  GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.BrokerBICCode]), folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.BrokerBICCode]) + " - " + GetEntityNameByID(ctpy_id));
                        TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.Currency]);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SPOTTYPE_PROPERTY_NAME, BondSpotType);
                        /*
                        string commonId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!String.IsNullOrEmpty(commonId))
                        {
                            Tuple<int, int, string> delegatemanager = delegatemanagers.Find(x => (x.Item3.ToUpper().CompareTo(commonId.ToUpper()) == 0));
                            if (delegatemanager != null)
                            {
                                int account_id = GetAccountIDFromDelegateManager(commonId, delegatemanager.Item2);
                                ticketMessage.add(FieldId.SECURITYACCOUNTID_PROPERTY_NAME, account_id);
                                ticketMessage.add(FieldId.BACKACCOUNT_PROPERTY_NAME, account_id);
                            }
                        }*/
                        /*
                        string transaction_id = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.TransactionID]);
                        if (!String.IsNullOrEmpty(transaction_id) && overwriteBORemarks)
                        {
                            //ticketMessage.add(FieldId.EXTERNALREF_PROPERTY_NAME, transaction_id);
                            TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, transaction_id);
                        }
                        */
                        string side = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.TransactionDescription]);
                        if (!String.IsNullOrEmpty(side))
                        {
                            if (side.ToUpper().Equals("SECURITIES PURCHASE"))
                            {
                                //ticketMessage.add(FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity));
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                            }
                            else if (side.ToUpper().Equals("SECURITIES SELL"))
                            {
                                //ticketMessage.add(FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity));
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                            }
                        }

                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.ExternalFundIdentifier]));
                        #endregion

                        #region Calculating Gross Amount
                        double receivedGrossAmount = 0.0;
                        double calculatedGrossAmount = 0.0;

                        if (validateGrossAmount || validateNetAmount) //Calculate gross amount
                        {
                            Double.TryParse(quantityStr, out quantity);
                            Double.TryParse(spotStr, out spot);
                            calculatedGrossAmount = quantity * spot;
                        }

                        #endregion

                        #region Validating Gross Amount
                        if (validateGrossAmount)
                        {
                            Double.TryParse(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.GrossAmount]), out receivedGrossAmount);
                            if (grossAmountEpsilon < Math.Abs(calculatedGrossAmount - receivedGrossAmount))
                            {
                                CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "Gross amount validation failure: |calculatedGA - receivedGA| > " + grossAmountEpsilon + " , calculatedGA=" + calculatedGrossAmount + " receivedGA=" + receivedGrossAmount);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.BACKOFFICEINFOS_PROPERTY_NAME, " Warning: Gross amount validation failure");
                                //return true;
                            }
                        }
                        #endregion

                        #region Validating Net Amount
                        double receivedNetAmount = 0.0;
                        double calculatedNetAmount = 0.0;
                        Double.TryParse(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.Bond2Mappings[InboundCSVFields.NetAmount]), out receivedNetAmount);
                        if (validateNetAmount)
                        {
                            string brokerFeesStr = ticketMessage.getString(FieldId.BROKERFEES_PROPERTY_NAME);
                            string marketFeesStr = ticketMessage.getString(FieldId.MARKETFEES_PROPERTY_NAME);
                            string counterpartyFeesStr = ticketMessage.getString(FieldId.COUNTERPARTYFEES_PROPERTY_NAME);
                            double brokerFees = 0.0;
                            double marketFees = 0.0;
                            double counterpartyFees = 0.0;
                            Double.TryParse(brokerFeesStr, out brokerFees);
                            Double.TryParse(marketFeesStr, out marketFees);
                            Double.TryParse(counterpartyFeesStr, out counterpartyFees);
                            calculatedNetAmount = calculatedGrossAmount + brokerFees + marketFees + counterpartyFees;
                            if (netAmountEpsilon < Math.Abs(calculatedNetAmount - receivedNetAmount))
                            {
                                CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "Net amount validation failure: |calculatedNA - receivedNA| > " + netAmountEpsilon + " , calculatedNA=" + calculatedNetAmount + " receivedNA=" + receivedNetAmount);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.BACKOFFICEINFOS_PROPERTY_NAME, " Warning: Net amount validation failure");
                                //return true;
                            }
                        }
                        #endregion

                    } break;
                case CSVDocumentClass.Forex:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.TransactionID]));
                        reversal_flag_column_id = CSxValidationUtil.ForexMappings[InboundCSVFields.ReversalFlag];
                        string reversalStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.ReversalFlag]);
                        if (reversalStr.ToUpper().Equals("Y"))
                        {
                            reversalFlag = true;
                            //TrySetKind(ref ticketMessage, TicketType.CANCEL);
                        }

                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.FOREX);
                        //string ndf = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.NDFFlag]);
                        //if (!String.IsNullOrEmpty(ndf))
                        //{
                        //    if (ndf.ToUpper().Equals("Y"))
                        //    {
                        //        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.NDF);
                        //    }
                        //}

                        bool buyCCYisMaster = false;
                        bool sellCCYisMaster = false;
                        string buyCCYStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.BuyCurrency]);
                        string sellCCYStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.SellCurrency]);
                        int buyCCY = CSMCurrency.StringToCurrency(CMString.FromWideString(buyCCYStr));
                        int sellCCY = CSMCurrency.StringToCurrency(CMString.FromWideString(sellCCYStr));
                        bool isFXReversed = CLIForexUtils.IsFXPairReversed(buyCCY, sellCCY);
                        //int globalMasterCCY = CSMCurrency.GetGlobalMasterCurrency(); // GlobalMasterCCY/CCY2
                        CSMCurrency buyCCYObj = CSMCurrency.GetCSRCurrency(buyCCY);
                        CSMCurrency sellCCYObj = CSMCurrency.GetCSRCurrency(sellCCY);
                        
                        int masterCCY1 = buyCCYObj.GetMasterCurrency();
                        int masterCCY2 = sellCCYObj.GetMasterCurrency();
                        if (masterCCY1 != 0 && masterCCY1 == sellCCY)
                            sellCCYisMaster = true;
                        else if (masterCCY2 != 0 && masterCCY2 == buyCCY)
                            buyCCYisMaster = true;

                        TrySetTicketField(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, fields, CSxValidationUtil.ForexMappings[InboundCSVFields.FXRate]);
                        TrySetTicketField(ref ticketMessage, FieldId.SPOTTYPE_PROPERTY_NAME, transaction.SpotTypeConstants.IN_PRICE);

                        string SoldAmount = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.SoldAmount]);
                        string FXRate = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.FXRate]);
                        string PurchasedAmount = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.PurchasedAmount]);
                        double receivedSold = 0.0;
                        double receivedPurchased = 0.0;
                        double receivedFxRate = 0.0;
                        double calculatedSold = 0.0;
                        double calculatedPurchased = 0.0;
                        Double.TryParse(SoldAmount, out receivedSold);
                        Double.TryParse(PurchasedAmount, out receivedPurchased);
                        Double.TryParse(FXRate, out receivedFxRate);
                        
                        if (buyCCYisMaster || isFXReversed == false)
                        {
                            buyCCYisMaster = true;
                            //buyCCYisGlobal = true;
                            CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_debug, "Buy currency is master currency: " + buyCCYStr + " (" + buyCCY + ")");
                            //TrySetTicketField<string>(ref ticketMessage, FieldId.FX_CURRENCY_NAME, sellCCYStr);
                            TrySetTicketField<string>(ref ticketMessage, FieldId.FX_CURRENCY_NAME, buyCCYStr);
                            TrySetTicketField<string>(ref ticketMessage, FieldId.FX_RATE_WAY_NAME, string.Format("{0}/{1}", buyCCYStr, sellCCYStr));
                            string purchasedAmountStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.PurchasedAmount]);
                            double purchasedAmount = 0.0;
                            Double.TryParse(purchasedAmountStr, out purchasedAmount);
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, purchasedAmount * ((reversalFlag) ? (-1) : (1)));
                            // For reconciliation purpose, we calculate the fx rate instead of using the rbc rate (slightl rounding discrepancy if so)
                            double fx = receivedPurchased != 0.0
                                ? receivedSold / receivedPurchased
                                : CSxValidationUtil.ForexMappings[InboundCSVFields.FXRate];
                            TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, fx);
                            // Test
                            TrySetTicketField<string>(ref ticketMessage, FieldId.FX_FAR_LEG_CURRENCY_NAME, buyCCYStr);
                            TrySetTicketField<string>(ref ticketMessage, FieldId.FX_FAR_LEG_RATE_WAY_NAME, string.Format("{0}/{1}", buyCCYStr, sellCCYStr));
                            TrySetTicketField<double>(ref ticketMessage, FieldId.FX_FAR_LEG_QUANTITY_PROPERTY_NAME, -purchasedAmount * ((reversalFlag) ? (-1) : (1)));
                        }
                        else if (sellCCYisMaster || isFXReversed == true)
                        {
                            sellCCYisMaster = true;
                            //sellCCYisGlobal = true;
                            CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_debug, "Sell currency is master currency: " + sellCCYStr + " (" + sellCCY + ")");
                            TrySetTicketField<string>(ref ticketMessage, FieldId.FX_CURRENCY_NAME, buyCCYStr);
                            TrySetTicketField<string>(ref ticketMessage, FieldId.FX_RATE_WAY_NAME, string.Format("{0}/{1}", sellCCYStr, buyCCYStr));
                            string purchasedAmountStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.PurchasedAmount]);
                            double purchasedAmount = 0.0;
                            Double.TryParse(purchasedAmountStr, out purchasedAmount);
                            double fx = receivedPurchased != 0.0
                                  ? receivedPurchased / receivedSold
                                  : CSxValidationUtil.ForexMappings[InboundCSVFields.FXRate];
                            TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, fx);
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, purchasedAmount * ((reversalFlag) ? (-1) : (1)));
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "Could not find a master currency");
                            TrySetTicketField(ref ticketMessage, FieldId.FX_CURRENCY_NAME, fields, CSxValidationUtil.ForexMappings[InboundCSVFields.BuyCurrency]);
                            TrySetTicketField<string>(ref ticketMessage, FieldId.FX_RATE_WAY_NAME, string.Format("{0}/{1}", CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.SellCurrency]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.BuyCurrency])));
                            TrySetDoubleFromList(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, fields, CSxValidationUtil.ForexMappings[InboundCSVFields.PurchasedAmount],reversalFlag);
                            string FXRateStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.FXRate]);
                            double fxRate = 0.0;
                            Double.TryParse(FXRateStr, out fxRate);
                            if (fxRate != 0.0)
                            {
                                TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, 1 / fxRate);
                            }
                        }

                        if (IsNDF(ticketMessage.getString(FieldId.FX_RATE_WAY_NAME)))
                            TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.NDF);

                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.TradeDate]), CSxValidationUtil.ForexDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.ForexDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        /*
                        int trade_date = CSxValidationUtil.GetDateInFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.TradeDate]));
                        int settlement_date = CSxValidationUtil.GetDateInFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.SettlementDate]));
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        */

                        //TrySetTicketField(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, fields, CSxValidationUtil.ForexMappings[InboundCSVFields.BrokerName]);

                        /*
                        string transaction_id = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.ForexMappings[InboundCSVFields.TransactionID]);
                        if (!String.IsNullOrEmpty(transaction_id) && overwriteBORemarks)
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, transaction_id);
                        }
                        */
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.ForexMappings[InboundCSVFields.ExternalFundIdentifier]));
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.ForexMappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.ForexMappings[InboundCSVFields.BrokerBICCode]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.BrokerBICCode]), folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.BrokerBICCode]) + " - " + GetEntityNameByID(ctpy_id));
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.ExternalFundIdentifier]));
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexMappings[InboundCSVFields.ExternalFundIdentifier]));
                

                        #region Validate amount
                        if (validateForexAmount)
                        {
                            if (buyCCYisMaster) //deal is buy
                            {
                                calculatedSold = receivedPurchased * receivedFxRate;
                                if (Math.Abs(receivedSold - calculatedSold) > forexAmountEpsilon)
                                {
                                    CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "Forex sold amount validation failure: |receivedSold - calculatedSold| > " + forexAmountEpsilon + " , calculatedSold=" + calculatedSold + " receivedSold=" + receivedSold);
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.BACKOFFICEINFOS_PROPERTY_NAME,
                                    " Warning: Forex sold amount validation failure");
                                    // return true;
                                }
                            }
                            else if (sellCCYisMaster) //deal is sell
                            {
                                calculatedPurchased = receivedSold * receivedFxRate;
                                if (Math.Abs(receivedPurchased - calculatedPurchased) > forexAmountEpsilon)
                                {
                                    CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "Forex purchased amount validation failure: |receivedPurchased - calculatedPurchased| > " + forexAmountEpsilon + " , calculatedPurchased=" + calculatedPurchased + " receivedPurchased=" + receivedPurchased);
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.BACKOFFICEINFOS_PROPERTY_NAME,
                                    " Warning: Forex purchased amount validation failure");
                                    //return true;
                                }
                            }
                            else
                            {

                            }
                        }
                        #endregion
                    } break;
                case CSVDocumentClass.Option:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.TransactionCode]));
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.OPTION);
                        string optionType = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.OptionMappings[InboundCSVFields.OptionType]);
                        if(!String.IsNullOrEmpty(optionType))
                        {
                            if(optionType.ToUpper().Equals("C"))
                            {
                                TrySetTicketField<int>(ref ticketMessage, FieldId.MA_OPTION_TYPE, MAOptionTypeConstants.CALL);
                            }
                            else if(optionType.ToUpper().Equals("P"))
                            {
                                TrySetTicketField<int>(ref ticketMessage, FieldId.MA_OPTION_TYPE, MAOptionTypeConstants.PUT);
                            }
                        }
                        string exerciseType = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.OptionMappings[InboundCSVFields.Style]);
                        if (!String.IsNullOrEmpty(exerciseType))
                        {
                            if (optionType.ToUpper().Equals("EUROPEAN"))
                            {
                                TrySetTicketField<string>(ref ticketMessage, FieldId.MA_EXERCISE_TYPE, ExerciseTypeConstants.EUROPEAN);
                            }
                            else if (optionType.ToUpper().Equals("AMERICAN"))
                            {
                                TrySetTicketField<string>(ref ticketMessage, FieldId.MA_EXERCISE_TYPE, ExerciseTypeConstants.AMERICAN);
                            }
                            else if (optionType.ToUpper().Equals("BERMUDA"))
                            {
                                TrySetTicketField<string>(ref ticketMessage, FieldId.MA_EXERCISE_TYPE, ExerciseTypeConstants.BERMUDA);
                            }
                        }
                        TrySetTicketField<string>(ref ticketMessage, FieldId.MA_COMPLEX_REFERENCE_TYPE, ToolkitDefaultUniversal);
                        TrySetTicketField(ref ticketMessage, FieldId.MA_STRIKE_VALUE, fields, CSxValidationUtil.OptionMappings[InboundCSVFields.OptionStrike]);
                        TrySetTicketField(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, fields, CSxValidationUtil.OptionMappings[InboundCSVFields.Premium]);
                        string bloomberg_code = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.BloombergCode]);
                        if (!String.IsNullOrEmpty(bloomberg_code))
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, bloomberg_code);
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_debug, "Instrument code: " + Enum.GetName(typeof(InboundCSVFields), InboundCSVFields.BloombergCode) + " was null or empty");
                            TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, DefaultErrorInstrumentID);
                        }
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.TradeDate]),CSxValidationUtil.OptionsDateFormats);
                        int value_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.NAVDate]), CSxValidationUtil.OptionsDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.OptionsDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, value_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.OptionMappings[InboundCSVFields.ExternalFundIdentifier]));
                        GetBrokerID(ref ticketMessage,CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.OptionMappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.OptionMappings[InboundCSVFields.BICCode]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.BICCode]), folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.BICCode]) + " - " + GetEntityNameByID(ctpy_id));
                        TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.OptionMappings[InboundCSVFields.OptionCurrency]);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.INSTRUMENTTYPEHINT, "Option");
                        TrySetTicketField(ref ticketMessage, FieldId.BROKERFEES_PROPERTY_NAME, fields, CSxValidationUtil.OptionMappings[InboundCSVFields.CommissionAmount]);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SPOTTYPE_PROPERTY_NAME, DefaultSpotType);
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.ExternalFundIdentifier]));
                        string side = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.OptionMappings[InboundCSVFields.BuyOrSell]);
                        if (!String.IsNullOrEmpty(side))
                        {
                            string quantityStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.Quantity]);
                            double quantity = 0.0;
                            Double.TryParse(quantityStr, out quantity);
                            if (side.ToUpper().Equals("BUY"))
                            {
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity));
                            }
                            else if (side.ToUpper().Equals("SELL"))
                            {
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity));
                            }
                        }
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.OptionMappings[InboundCSVFields.ExternalFundIdentifier]));
                    } break;
                case CSVDocumentClass.Future:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.FutureTradeCode]));
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.FUTURE);
                        //TrySetTicketField<string>(ref ticketMessage, FieldId.INSTRUMENTTYPEHINT, "Future");
                        //TrySetTicketField(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, fields, CSxValidationUtil.FutureMappings[InboundCSVFields.BloombergCode]);
                        string side = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.FutureMappings[InboundCSVFields.BuyOrSell]);
                        string quantityStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.Quantity]);
                        double quantity = 0.0;
                        if (!String.IsNullOrEmpty(side))
                        {
                            Double.TryParse(quantityStr, out quantity);
                            if (side.ToUpper().Equals("BUY"))
                            {
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity));
                            }
                            else if (side.ToUpper().Equals("SELL"))
                            {
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity));
                            }
                        }
                        TrySetTicketField(ref ticketMessage, FieldId.MA_COMPLEX_REFERENCE_TYPE, ToolkitDefaultUniversal);
                        TrySetTicketField(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, fields, CSxValidationUtil.FutureMappings[InboundCSVFields.Price]);
                        //TrySetTicketField(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, fields, CSxValidationUtil.FutureMappings[InboundCSVFields.CounterpartyOrBrokerDescription]);
                        //TrySetTicketField(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, fields, CSxValidationUtil.FutureMappings[InboundCSVFields.FutureTradeCode]);
                        TrySetTicketField(ref ticketMessage, FieldId.BROKERFEES_PROPERTY_NAME, fields, CSxValidationUtil.FutureMappings[InboundCSVFields.FeesAmountInTransactionCurrency]);
                        string bloomberg_code = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.BloombergCode]);
                        if (!String.IsNullOrEmpty(bloomberg_code))
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, bloomberg_code);
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_debug, "Instrument code: " + Enum.GetName(typeof(InboundCSVFields), InboundCSVFields.BloombergCode) + " was null or empty");
                            TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, DefaultErrorInstrumentID);
                            if (quantity == 0.0)
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, 1);
                        }
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.TradeDate]),CSxValidationUtil.FuturesDateFormats);
                        int accountancy_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.NAVDate]), CSxValidationUtil.FuturesDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.FuturesDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.ACCOUNTANCYDATE_PROPERTY_NAME, accountancy_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.ExternalFundIdentifier]));
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.BICCode]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.BICCode]), folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.BICCode]) + " - " + GetEntityNameByID(ctpy_id));
                        
                        TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.FutureMappings[InboundCSVFields.FutureCurrency]);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SPOTTYPE_PROPERTY_NAME, DefaultSpotType);
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.ExternalFundIdentifier]));
                        string gti = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.GTIDescription]);
                        if(gti.ToUpper().Contains("INDICES"))
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.INSTRUMENTTYPEHINT, "IndexFuture");
                        }
                        else if (gti.ToUpper().Contains("BONDS"))
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.INSTRUMENTTYPEHINT, "NotionalFuture");
                        }
                        else
                        {

                        }
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.FutureMappings[InboundCSVFields.ExternalFundIdentifier]));
                        /*
                        string transaction_id = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.FutureMappings[InboundCSVFields.TransactionID]);
                        if (!String.IsNullOrEmpty(transaction_id) && overwriteBORemarks)
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, transaction_id);
                        }
                         */ 
                    } break;
                case CSVDocumentClass.Cash:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_warning, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        string transactionType = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.TransactionType]);
                        if (!CheckAcceptedTransactionType(transactionType))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_warning, "Ignoring trade because transaction type [ " + transactionType + " ] is part of the unaccepted transaction type list");
                            return true;
                        }
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.ExternalFundIdentifier]));
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.TransactionID]));
                        reversal_flag_column_id = CSxValidationUtil.CashMappings[InboundCSVFields.ReversalFlag];
                        string reversalStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.ReversalFlag]);
                        if (reversalStr.ToUpper().Equals("Y"))
                        {
                            reversalFlag = true;
                            //TrySetKind(ref ticketMessage, TicketType.CANCEL);
                        }
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                        //GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.ExternalFundIdentifier]), "Cash", null, true);
                        //TrySetTicketField(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, fields, CSxValidationUtil.CashMappings[InboundCSVFields.IBAN]);
                        int folio_id = ResolveCashFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.ExternalFundIdentifier]));
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.CashMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, null, folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpy_id));
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, 1);
                        TrySetDoubleFromList(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, fields, CSxValidationUtil.CashMappings[InboundCSVFields.TradeAmount]);
                        string ccy = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.CashMappings[InboundCSVFields.Currency]);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.FX_CURRENCY_NAME, ccy);
                        int businessEventID = 0;

                        if (InterestPaymentTypeNameList.Contains(transactionType.Trim().ToUpper()))
                        {
                            if (businessevents.ContainsKey(InterestPaymentBusinessEvent))
                                businessEventID = businessevents[InterestPaymentBusinessEvent];
                        }
                        else if (!String.IsNullOrEmpty(CashTransferBusinessEvent))
                        {
                            if (businessevents.ContainsKey(CashTransferBusinessEvent))
                                businessEventID = businessevents[CashTransferBusinessEvent];
                        }
                        if (businessEventID != 0)
                        {
                            TrySetTicketField<int>(ref ticketMessage, FieldId.TRADETYPE_PROPERTY_NAME, businessEventID);
                        }
                        //TrySetTicketField<int>(ref ticketMessage, FieldId.BO_REFERENCE_PROPERTY_NAME, businessEventID);
                        string accountId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.ExternalFundIdentifier]);
                        int sicovam = GetCashInstrumentSicovam(ccy, CashTransferInstrumentNameFormat, null, accountId, accountId, CashTransferBusinessEvent, defaultCounterpartyStr, null,folio_id,null,ExtFundId);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                        if (ticketMessage.getString(FieldId.INSTRUMENTREF_PROPERTY_NAME).Equals(0.ToString()) && CSxValidationUtil.CashMappings[InboundCSVFields.TradeAmount] == 0.0)
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, 1);

                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.TradeDate]), CSxValidationUtil.CashDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.CashDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CashMappings[InboundCSVFields.ExternalFundIdentifier]));
                        /*
                        string transaction_id = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.CashMappings[InboundCSVFields.TransactionID]);
                        if (!String.IsNullOrEmpty(transaction_id) && overwriteBORemarks)
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, transaction_id);
                        }
                         */ 
                    } break;
                case CSVDocumentClass.Invoice:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }

                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.TransactionID]));

                        reversal_flag_column_id = CSxValidationUtil.InvoiceMappings[InboundCSVFields.ReversalFlag];
                        string reversalStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ReversalFlag]);
                        if (reversalStr.ToUpper().Equals("Y"))
                        {
                            reversalFlag = true;
                            //TrySetKind(ref ticketMessage, TicketType.CANCEL);
                        }

                        int businessEventID = 0;
                        if (!String.IsNullOrEmpty(InvoiceBusinessEvent))
                        {
                            try
                            {
                                businessEventID = businessevents[InvoiceBusinessEvent];
                            }
                            catch (Exception ex)
                            {
                                CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_warning, "Exception occurred while trying to get InvoiceBusinessEvent: " + ex.Message);
                            }
                        }
                        if (businessEventID != 0)
                        {
                            TrySetTicketField<int>(ref ticketMessage, FieldId.TRADETYPE_PROPERTY_NAME, businessEventID);
                        }
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ExternalFundIdentifier]));
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                        int folio_id = ResolveCashFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ExternalFundIdentifier]));
                        //int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ExternalFundIdentifier]), "Cash", null, true);
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, null, folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpy_id));
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, -1);
                        TrySetDoubleFromList(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.TradeAmount],reversalFlag);
                        string ccy = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.Currency]);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.FX_CURRENCY_NAME, ccy);
                        string accountId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ExternalFundIdentifier]);
                        int sicovam = GetCashInstrumentSicovam(ccy, InvoiceInstrumentNameFormat, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.FeeType]),accountId,accountId,InvoiceBusinessEvent,defaultCounterpartyStr,null,folio_id,null,ExtFundId);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                        if (ticketMessage.getString(FieldId.INSTRUMENTREF_PROPERTY_NAME).Equals(0.ToString()) && CSxValidationUtil.InvoiceMappings[InboundCSVFields.TradeAmount] == 0.0)
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, 1);

                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.TradeDate]),CSxValidationUtil.CashDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.CashDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        //TrySetTicketField(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.FeeType]);
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.InvoiceMappings[InboundCSVFields.ExternalFundIdentifier]));
                        /*
                        string transaction_id = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.InvoiceMappings[InboundCSVFields.TransactionID]);
                        if (!String.IsNullOrEmpty(transaction_id) && overwriteBORemarks)
                        {
                            TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, transaction_id);
                        }
                         */ 
                    } break;
                case CSVDocumentClass.ForexHedge:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.FOREX);
                        //TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTTYPE_PROPERTY_NAME, PositionTypeConstants.VIRTUAL_FOREX);
                        bool side_buy = true;
                        string side = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.Direction]);
                        if (!String.IsNullOrEmpty(side))
                        {
                            if (side.ToUpper().Equals("SELL"))
                            {
                                side_buy = false;
                            }
                        }
                        string quantityStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ContraAmount]);
                        double quantity = 0.0;
                        Double.TryParse(quantityStr, out quantity);
                        if (!side_buy)
                            quantity = -Math.Abs(quantity);
                        else
                            quantity = Math.Abs(quantity);
                        string shareClass = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ShareClass]);
                        
                        string clientSpotRateStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ClientAllInRate]);
                        double clientSpotRate = 0.0;
                        Double.TryParse(clientSpotRateStr, out clientSpotRate);
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.TradeDate]), CSxValidationUtil.ForexDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.MaturityDate]), CSxValidationUtil.ForexDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);

                        TrySetTicketField(ref ticketMessage, FieldId.FX_CURRENCY_NAME, fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.CounterCurrency]);
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, clientSpotRate != 0.0 ? 1 / clientSpotRate : 0.0);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.FX_RATE_WAY_NAME, string.Format("{0}/{1}", CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.FixedCurrency]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.CounterCurrency])));
                        TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, quantity);

                        //string instType = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.IntrumentType]);
                        //if (instType.ToUpper() == "FORWARD")
                        //{
                        //    // Test far leg
                        //    TrySetTicketField<double>(ref ticketMessage, FieldId.FX_FAR_LEG_SPOT_PROPERTY_NAME, clientSpotRate != 0.0 ? clientSpotRate : 0.0);
                        //    TrySetTicketField<double>(ref ticketMessage, FieldId.FX_FAR_LEG_QUANTITY_PROPERTY_NAME,   quantity);
                        //    TrySetTicketField(ref ticketMessage, FieldId.FX_FAR_LEG_CURRENCY_NAME, fields,
                        //        CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.CounterCurrency]);
                        //    TrySetTicketField<int>(ref ticketMessage, FieldId.FX_FAR_LEG_VALUEDATE_PROPERTY_NAME,settlement_date);
                        //    TrySetTicketField<string>(ref ticketMessage, FieldId.FX_FAR_LEG_RATE_WAY_NAME, string.Format("{0}/{1}", CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.FixedCurrency]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.CounterCurrency])));
                        //}
                        //TrySetTicketField(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ClientName]);
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ExternalFundIdentifier]));
                        ticketMessage.add(transaction.FieldId.SPOTTYPE_PROPERTY_NAME, transaction.SpotTypeConstants.IN_PRICE);
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ExternalFundIdentifier]), "Hedge" , shareClass, true);
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, null, folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpy_id));
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ExternalFundIdentifier]));
                        #region Validate amount
                        if (validateForexAmount)
                        {
                            string FixedAmountStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.FixedAmount]);
                            string FXRateStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ClientSpotRate]);
                            string ContraAmountStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.ForexHedgeMappings[InboundCSVFields.ContraAmount]);
                            double receivedFixedAmount = 0.0;
                            double receivedContraAmount = 0.0;
                            double receivedFxRate = 0.0;
                            double calculatedFixedAmount = 0.0;
                            Double.TryParse(FixedAmountStr, out receivedFixedAmount);
                            Double.TryParse(ContraAmountStr, out receivedContraAmount);
                            Double.TryParse(FXRateStr, out receivedFxRate);
                            if (receivedFxRate != 0.0)
                            {
                                calculatedFixedAmount = receivedContraAmount * receivedFxRate;
                            }
                            if (Math.Abs(receivedFixedAmount - calculatedFixedAmount) > forexAmountEpsilon)
                            {
                                CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "ForexHedge sold amount validation failure: |receivedSold - calculatedSold| > " + forexAmountEpsilon + " , calculatedFixedAmount=" + calculatedFixedAmount + " receivedFixedAmount=" + receivedFixedAmount);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.BACKOFFICEINFOS_PROPERTY_NAME,
                                   " Warning: ForexHedge sold amount validation failure");
                                //return true;
                            }
                        }
                        #endregion
                    } break;
                case CSVDocumentClass.TACash:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.ExternalFundIdentifier]));
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.ExternalFundIdentifier]),"Cash", null, true);
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, null, folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpy_id));
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, 1);
                        TrySetTicketField(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, fields, CSxValidationUtil.TACashMappings[InboundCSVFields.ConfirmedAmountLocalCurrency]);
                        string ccy = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.AccountCurrency]);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.FX_CURRENCY_NAME, ccy);
                        string accountId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.ExternalFundIdentifier]);
                        int sicovam = GetCashInstrumentSicovam(ccy, TACashInstrumentNameFormat, null, accountId, accountId, TACashBusinessEvent, defaultCounterpartyStr, null, folio_id, null, ExtFundId);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                        if (ticketMessage.getString(FieldId.INSTRUMENTREF_PROPERTY_NAME).Equals(0.ToString()) && CSxValidationUtil.TACashMappings[InboundCSVFields.ConfirmedAmountLocalCurrency] == 0.0)
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, 1);

                        if (businessevents.ContainsKey(TACashBusinessEvent))
                        {
                            TrySetTicketField<int>(ref ticketMessage, FieldId.TRADETYPE_PROPERTY_NAME, businessevents[TACashBusinessEvent]);
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_warning, "TACashBusinessEvent: " + TACashBusinessEvent + " not found in the db");
                        }
                        
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.TradeDate]), CSxValidationUtil.CashDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.CashDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);

                        //TrySetTicketField(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, fields, CSxValidationUtil.TACashMappings[InboundCSVFields.CashFlowTypeDescription]);
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.TACashMappings[InboundCSVFields.ExternalFundIdentifier]));
                    } break;
                case CSVDocumentClass.Swap:
                    {
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.FusionRef]));
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                        SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.ExternalFundIdentifier]));
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.BrokerBICCode]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.BrokerBICCode]), folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.BrokerBICCode]) + " - " + GetEntityNameByID(ctpy_id));
                        int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.TradeDate]), CSxValidationUtil.SwapsDateFormats);
                        int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.SwapsDateFormats);
                        int maturity_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.MaturityDate]), CSxValidationUtil.SwapsDateFormats);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_MATURITY_DATE, maturity_date);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SPOTTYPE_PROPERTY_NAME, DefaultSpotType);
                        //Find FusionRef
                        TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, DefaultErrorInstrumentID); // initialize with zero
                        string fusionRef = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.FusionRef]);
                        int fusionRefInt = 0;
                        bool isSicovam = true;
                        if (Int32.TryParse(fusionRef, out fusionRefInt))
                        {
                            if (CheckIfSicovamExists(fusionRefInt))
                            {
                                TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, fusionRefInt);
                            }
                            else
                            {
                                isSicovam = false;
                                CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_debug, "Instrument identifier (sicovam) = " + fusionRefInt + " does not exist in the database");
                            }
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_debug, "Failed to parse FusionRef as int");
                            isSicovam = false;
                        }

                        int sicovam = 0;
                        if (!isSicovam && !fusionRef.Contains('-'))
                        {
                            CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_debug, "FusionRef does not contain '-' character");
                            if (GetSicovamByName(fusionRef, out sicovam))
                            {
                                TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                            }
                        }
                        else if (!isSicovam && fusionRef.Contains('-'))
                        {
                            CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_debug, "FusionRef contains '-' character");
                            string[] fusionRefArray = fusionRef.Split('-');
                            if (fusionRefArray.Length >= 2)
                            {
                                string instrumentRef = fusionRefArray[fusionRefArray.Length - 1].Trim(); //get last element of array
                                if (GetSicovamByName(instrumentRef, out sicovam))
                                {
                                    TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                                }
                            }
                        }

                        string swapWay = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.UpfrontAmountFlag]); // R or P
                        if (swapWay.ToUpper().Equals("P"))
                        {
                            string nominalPayableStr = CSxValidationUtil.TryAccessListValue(fields,CSxValidationUtil.SwapMappings[InboundCSVFields.NominalPayableLeg]);
                            double nominalPayable = 0.0;
                            Double.TryParse(nominalPayableStr, out nominalPayable);
                            TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, Math.Abs(nominalPayable));
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Sign(nominalPayable));
                            TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.SwapMappings[InboundCSVFields.CurrencyPayableLeg]);
                        }
                        else
                        {
                            string nominalReceivableStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.NominalPayableLeg]);
                            double nominalReceivable = 0.0;
                            Double.TryParse(nominalReceivableStr, out nominalReceivable);
                            TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, Math.Abs(nominalReceivable));
                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Sign(nominalReceivable));
                            TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.SwapMappings[InboundCSVFields.CurrencyReceivableLeg]);
                        }

                        if (CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.GTILabel]).ToUpper().Contains("CFD"))
                        {
                            TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.CFD);
                            TrySetTicketField(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, fields,CSxValidationUtil.SwapMappings[InboundCSVFields.Quantity]);
                            TrySetTicketField(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, fields, CSxValidationUtil.SwapMappings[InboundCSVFields.CostPriceLocalCurrency]);
                            TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.SwapMappings[InboundCSVFields.Currency]);
                        }
                        OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.SwapMappings[InboundCSVFields.ExternalFundIdentifier]));
                    } break;
                case CSVDocumentClass.CorporateAction:
                    {
                        string caTypeStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.LSTType]);
                        int caTypeInt = 0;
                        Int32.TryParse(caTypeStr, out caTypeInt);
                        CorporateActionType caType = (CorporateActionType) caTypeInt;
                        CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_debug, "Received corporate action type: " + Enum.GetName(typeof(CorporateActionType), caType));
                        TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.TransactionID]));
                        int depo_id = SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]));
                        string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.ExternalFundIdentifier]);
                        if (!CheckAllowedListExtFundId(ExtFundId))
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                            return true;
                        }
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, Enum.GetName(typeof(CorporateActionType), caType) ?? "CorporateAction");
                        // Select instrument and universal
                        string oldType = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OldType]); // ISIN / TICKER
                        string oldBBGcode = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OldBloombergCode]);
                        string oldISIN = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.OldISIN]);
                        string newBBGcode = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewBloombergCode]);
                        string newISIN = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NewISIN]);
                        string universal = ToolkitDefaultUniversal; // Ticker by default
                        if (!String.IsNullOrEmpty(oldType))
                        {
                            if (!String.IsNullOrEmpty(oldBBGcode))
                            {
                                TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, oldBBGcode);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME,
                                    oldBBGcode + " -> " + newBBGcode);
                            }
                            else
                            {
                                TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, 0.ToString());
                            }
                            if (oldType.ToUpper().Contains("BOND"))
                            {
                                universal = ToolkitBondUniversal;
                                if (!String.IsNullOrEmpty(oldISIN))
                                {
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, oldISIN);
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME,
                                        oldISIN + " -> " + newISIN);
                                }
                                else
                                {
                                    TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, 0.ToString());
                                }
                            }
                        }
                        else
                            CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_error, "Could not find any Bloomberg or ISIN code in the input CSV");
                        
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

                        int businesseventID = GetBussinessEvent(caType);
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

                        switch (caType)
                        {
                            /* Set default price and in quantity */
                            case CorporateActionType.Subscription:
                            case CorporateActionType.DistributionInSecurities:
                            case CorporateActionType.ReverseSplit: // to follow up with JB
                            {
                            } break;
                            /* Use in quantity if in quantity != 0 otherwise out quantity */
                            case CorporateActionType.PublicPurchaseOffer:
                            {
                                if (inQuantity != 0.0) quantity = inQuantity;
                                else quantity = outQuantity;
                            } break;
                            // log/info: not supported in the toolkit - should be done in sophis
                           
                            case CorporateActionType.AdditionalSplit: 
                            case CorporateActionType.Dividend:
                            case CorporateActionType.Interest:
                            case CorporateActionType.FinalRedemption:
                            {
                                string str = String.Format("Warning: The CA type {0} is not supported by the toolkit!", caTypeStr);
                                TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, str);
                            } break;
                            case CorporateActionType.EarlyRedemption:
                            {
                                quantity = outQuantity;
                            } break;
                            /* Set gross amount as -price and -1 as qty*/
                            case CorporateActionType.CashCredit:
                            {
                                price = -grossAmount; quantity = -1;
                            } break;
                            /* Set gross amount as price and 1 as qty */
                            case CorporateActionType.CashDebit:
                            {
                                price = grossAmount; quantity = 1;
                            } break;
                            /* Set price as 0 and abs(in quantity) */
                            case CorporateActionType.SecuritiesDeposit:
                            {
                                price = 0; quantity = Math.Abs(inQuantity);
                            } break;
                            /* Set price as 0 and -abs(out quantity) */
                            case CorporateActionType.SecuritiesWithdrawal:
                            {
                                price = 0; quantity = -Math.Abs(outQuantity);
                            } break;
                            /* check gross amount:
                                1) > 0 then same as cash debit 
                                2) < 0 then same as cash credit
                                3) = 0 SecuritiesDeposit if inamount > 0
                                4) = 0 SecuritiesWithdrawal if outamount > 0 */
                            case CorporateActionType.BonusOrScripIssue:
                                {
                                    if (grossAmount > 0)
                                        goto case CorporateActionType.CashDebit;
                                    else if (grossAmount < 0)
                                        goto case CorporateActionType.CashCredit;
                                    else
                                    {
                                        string netAmountStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.NetAmount]);
                                        double netAmount = 0.0;
                                        Double.TryParse(netAmountStr, out netAmount);
                                        if (inQuantity > 0)
                                        {
                                            goto case CorporateActionType.SecuritiesDeposit;
                                        }
                                        if (outQuantity > 0)
                                        {
                                            goto case CorporateActionType.SecuritiesWithdrawal;
                                        }
                                    }
                                } break;
                            case CorporateActionType.CapitalIncreaseVsPayment:
                            {
                                price = 0;
                                quantity = outQuantity;
                                if (Math.Abs(inQuantity) > 0 && Math.Abs(grossAmount) > 0 && Math.Abs(outQuantity) > 0)
                                {
                                    // CA Adjustment (Securities)
                                    businesseventID = GetBussinessEvent(CorporateActionType.SecuritiesDeposit);
                                    if (businesseventID == 0 && !String.IsNullOrEmpty(CADefaultBusinessEvent))
                                    {
                                        if (businessevents.ContainsKey(CADefaultBusinessEvent))
                                            businesseventID = businessevents[CADefaultBusinessEvent];
                                    }
                                    if (businesseventID != 0)
                                        TrySetTicketField<int>(ref ticketMessage, FieldId.TRADETYPE_PROPERTY_NAME, businesseventID);
                                }
                            } break;
                            /* Set price and InQuantity */
                            // Check: for bonds -> new ISIN / old ISIN
                            // Equity: new bbg / old bbgs
                            // Warn if no instrument found for either trade
                            case CorporateActionType.Split:
                            case CorporateActionType.CapitalReductionWithPayment:
                            case CorporateActionType.Conversion: // to be checked
                            case CorporateActionType.SpinOff: // keep partially the old position
                            case CorporateActionType.WarrantExercise:
                            case CorporateActionType.RightsEntitlement:
                            case CorporateActionType.Amalgamation:
                            case CorporateActionType.ExchangeOrExchangeOffer:
                            case CorporateActionType.Merger:
                            {
                                price = 0;
                                quantity = outQuantity;
                            } break;
                            default:
                            {
                                CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_debug, "No implementation exists for this type of Corporate Action");
                            } break;
                    }

                    string transactionStatus = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CAMappings[InboundCSVFields.TransactionStatus]);
                    reversalFlag = CorporateActionReversalCodeList.Contains(transactionStatus);

                    if (ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME).Equals(0.ToString()) && quantity == 0.0)
                    {
                        // Set a non-zero quantity in case of no tiker, to prevent from IS crashing...
                        TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, 1);
                    }
                    else
                    {
                        if (caType == CorporateActionType.EarlyRedemption)
                        {
                            ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, Math.Abs(quantity), reversalFlag);
                            ticketMessage.SetTicketField(FieldId.GROSSAMOUNT_PROPERTY_NAME, Math.Abs(grossAmount), reversalFlag);
                            ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, SpotTypeConstants.IN_PERCENT_WITH_ACCRUED);
                            ticketMessage.SetTicketField(FieldId.ACCRUED_PROPERTY_NAME, 0);
                        }
                        else
                        {
                            string instName = ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME);
                            string refName = ticketMessage.getString(FieldId.MA_COMPLEX_REFERENCE_TYPE);
                            string paymentCCY = ticketMessage.getString(FieldId.PAYMENTCURRENCY_PROPERTY_NAME);
                            if (GetUnderlyingCCY(refName, instName).ToUpper() != paymentCCY.ToUpper())
                                ticketMessage.SetTicketField(FieldId.SPOTPAYMENTCURR_PROPERTY_NAME, PaymentCurrencyType.SETTLEMENT); 

                            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, quantity * ((reversalFlag) ? -1 : 1));
                        }
                    }
                    TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, price);
                } break;
                case CSVDocumentClass.Collateral:
                {
                    string ExtFundId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]);
                    if (!CheckAllowedListExtFundId(ExtFundId))
                    {
                        CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_info, "Ignoring trade because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                        return true;
                    }
                    TrySetUserField(ref ticketMessage, RBCTransactionIDName, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.TransactionID]));

                    reversal_flag_column_id = CSxValidationUtil.CollateralMappings[InboundCSVFields.ReversalFlag];
                    string reversalStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ReversalFlag]);
                    reversalFlag = reversalStr.ToUpper().Equals("Y") ? true : false;

                    string transDescb = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.TransactionDescription]);
                    string instDescb = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.SecurityDescription]);
                    eCollateralType type = eCollateralType.Equity;
                    if (transDescb.ToUpper().Contains("CASH"))
                        type = eCollateralType.Cash;
                    else
                    {
                        if (instDescb.ToUpper().Contains("BOND") || instDescb.ToUpper().Contains("MONEY MARKET"))
                            type = eCollateralType.Bond;
                    }

                    string instrumentName = CSxValidationUtil.TryAccessListValue(fields, (type == eCollateralType.Bond) ? (CSxValidationUtil.CollateralMappings[InboundCSVFields.ISINCode]) : (CSxValidationUtil.CollateralMappings[InboundCSVFields.BloombergCode]));
                    string ccy = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Currency]);
                    string spotStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Price]);
                    string grossAmountStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.GrossAmount]);
                    string quantityStr = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Quantity]);
                    double spot = 0.0;
                    double grossAmount = 0.0;
                    double quantity = 0.0;
                    Double.TryParse(spotStr, out spot);
                    Double.TryParse(grossAmountStr, out grossAmount);
                    Double.TryParse(quantityStr, out quantity);
                    spot = quantity != 0.0 ? grossAmount / quantity : spot;

                    if (type == eCollateralType.Equity)
                    {
                        foreach (var item in CurrenciesForSharesPricedInSubunitsList)
                        {
                            if (item.Key == ccy.ToUpper() && instrumentName.Contains(item.Value))
                                spot *= 100; break;
                        }
                    }
                    else if (type == eCollateralType.Bond)
                        spot *= 100;

                    if (type == eCollateralType.Cash)
                    {
                        string transactionType = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.TransactionType]);
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, 1);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                        string accountId = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]);
                        int folio_id = ResolveCashFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int sicovam = GetCashInstrumentSicovam(ccy, CashTransferInstrumentNameFormat, null, accountId, accountId, CashTransferBusinessEvent, defaultCounterpartyStr, null, folio_id, null, ExtFundId);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.CollateralCounterpartyBICCode]), folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpy_id));
                        TrySetDoubleFromList(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.NetAmount]);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.FX_CURRENCY_NAME, ccy);
                        int businessEventID = 0;
                        if (InterestPaymentTypeNameList.Contains(transactionType.Trim().ToUpper()))
                        {
                            if (businessevents.ContainsKey(InterestPaymentBusinessEvent))
                                businessEventID = businessevents[InterestPaymentBusinessEvent];
                        }
                        else if (!String.IsNullOrEmpty(CashTransferBusinessEvent))
                        {
                            if (businessevents.ContainsKey(CashTransferBusinessEvent))
                                businessEventID = businessevents[CashTransferBusinessEvent];
                        }
                        if (businessEventID != 0)
                            TrySetTicketField<int>(ref ticketMessage, FieldId.TRADETYPE_PROPERTY_NAME, businessEventID);
                    }
                    else // Equity/Bond
                    {
                        TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.STOCK);
                        TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, spot);
                        TrySetTicketField(ref ticketMessage, FieldId.INSTRUMENT_SOURCE, fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.SecurityDescription]);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.MA_COMPLEX_REFERENCE_TYPE, (type == eCollateralType.Equity) ? ToolkitDefaultUniversal : ToolkitBondUniversal);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.INSTRUMENTTYPEHINT, (type == eCollateralType.Equity) ? "Equity" : "Bond");

                        if (!String.IsNullOrEmpty(instrumentName))
                            TrySetTicketField<string>(ref ticketMessage, FieldId.MA_INSTRUMENT_NAME, instrumentName);
                        else
                        {
                            CSMLog.Write("RBC_Filter", "filter", CSMLog.eMVerbosity.M_debug, "Instrument code: " + ((type == eCollateralType.Equity) ? (Enum.GetName(typeof(InboundCSVFields), InboundCSVFields.BloombergCode)) : (Enum.GetName(typeof(InboundCSVFields), InboundCSVFields.ISINCode))) + " was null or empty");
                            TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, DefaultErrorInstrumentID);
                        }
                        int folio_id = GetFolioID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.EquityAndBondMappings[InboundCSVFields.SecurityDescription]));
                        GetBrokerID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]), CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.CollateralCounterpartyBICCode]));
                        int ctpy_id = GetCounterpartyID(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]), documentClass, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.CollateralCounterpartyBICCode]), folio_id);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.CollateralCounterpartyBICCode]) + " - " + GetEntityNameByID(ctpy_id));
                        TrySetTicketField(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Currency]);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.SPOTTYPE_PROPERTY_NAME, (type == eCollateralType.Bond) ? (BondSpotType) : (DefaultSpotType));
                        if (type == eCollateralType.Bond)
                            TrySetDoubleFromList(ref ticketMessage, FieldId.NOTIONAL_PROPERTY_NAME, fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Quantity]);
                        else
                            TrySetDoubleFromList(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Quantity]);
                        
                        string side = CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.TransactionDescription]);
                        if (!String.IsNullOrEmpty(side))
                        {
                            if (side.ToUpper().Equals("SECURITIES PURCHASE"))
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                            else if (side.ToUpper().Equals("SECURITIES SELL"))
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                            else if (side.ToUpper().Equals("SECURITIES DIRECT ENTRY"))
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                            else if (side.ToUpper().Equals("SECURITIES WITHDRAWAL"))
                                TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, -Math.Abs(quantity) * ((reversalFlag) ? (-1) : (1)));
                        }
                        TrySetTicketField(ref ticketMessage, FieldId.BROKERFEES_PROPERTY_NAME, fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Brokerage]);
                        TrySetTicketField(ref ticketMessage, FieldId.MARKETFEES_PROPERTY_NAME, fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Tax]);
                        TrySetTicketField(ref ticketMessage, FieldId.COUNTERPARTYFEES_PROPERTY_NAME, fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.Expenses]);
                    }
                    int trade_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.TradeDate]), CSxValidationUtil.CommonDateFormats);
                    int settlement_date = CSxValidationUtil.GetDateInAnyFormat(CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.SettlementDate]), CSxValidationUtil.CommonDateFormats);
                    TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
                    TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
                    TrySetTicketField<int>(ref ticketMessage, FieldId.VALUEDATE_PROPERTY_NAME, settlement_date);
                    SetDepositary(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]));
                    OverrideDefaultKernelWorkflow(ref ticketMessage, CSxValidationUtil.TryAccessListValue(fields, CSxValidationUtil.CollateralMappings[InboundCSVFields.ExternalFundIdentifier]));

                }break;
                case CSVDocumentClass.Unknown:
                    {
                        CSMLog.Write("RBC_Filter", "filter()", CSMLog.eMVerbosity.M_warning, "Unknown document class CSV received");
                        SetDefaultFailureMessageFileds(ref ticketMessage);
                    } break;
                default:
                    {

                    } break;
            }

            if (documentClass != CSVDocumentClass.Unknown)
            {
                string generatedHash = GenerateSha1Hash(fields,reversal_flag_column_id);
                TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : ("")) + generatedHash);
            }

            DateTime endTime = DateTime.Now;

            CSxRBCHelper.LogFieldsToFile(ticketMessage, "old_rbc_filter.txt", (endTime - startTime).TotalMilliseconds);
            return false;
        }

        #region Methods for default fields

        void SetDefaultSuccessMessageFileds(ref ITicketMessage ticketMessage, bool doNotSetExtRef = false)
        {
            TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.FOREX);
            TrySetTicketField<string>(ref ticketMessage, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, "EUR");
            TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, DefaultErrorFolio);
            //TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
            TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, 1.0);
            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, 0.0);
            TrySetTicketField<string>(ref ticketMessage, FieldId.FX_RATE_WAY_NAME, "EUR/USD");

            string tradeDate = DateTime.Now.ToString(CSxValidationUtil.DefaultDateFormat);
            string settlementDate = DateTime.Now.ToString(CSxValidationUtil.DefaultDateFormat);
            int trade_date = CSxValidationUtil.GetDateInFormat(tradeDate, CSxValidationUtil.DefaultDateFormat);
            int settlement_date = CSxValidationUtil.GetDateInFormat(settlementDate, CSxValidationUtil.DefaultDateFormat);

            TrySetTicketField<int>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, trade_date);
            TrySetTicketField<int>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, settlement_date);
            if (overwriteBORemarks && !doNotSetExtRef)
            {
                TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, GenerateSha1Hash());
            }
        }

        void SetDefaultFailureMessageFileds(ref ITicketMessage ticketMessage, bool doNotSetExtRef = false)
        {
            TrySetTicketField<int>(ref ticketMessage, FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
            TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, DefaultErrorFolio);
            TrySetTicketField<double>(ref ticketMessage, FieldId.SPOT_PROPERTY_NAME, 1);
            TrySetTicketField<int>(ref ticketMessage, FieldId.INSTRUMENTREF_PROPERTY_NAME, DefaultErrorInstrumentID);
            TrySetTicketField<double>(ref ticketMessage, FieldId.QUANTITY_PROPERTY_NAME, 0.0);
            TrySetTicketField<string>(ref ticketMessage, FieldId.NEGOTIATIONDATE_PROPERTY_NAME, "Invalid");
            TrySetTicketField<string>(ref ticketMessage, FieldId.SETTLDATE_PROPERTY_NAME, "Invalid");

            if (overwriteBORemarks && !doNotSetExtRef)
            {
                TrySetTicketField<string>(ref ticketMessage, FieldId.EXTERNALREF_PROPERTY_NAME, GenerateSha1Hash());
            }
        }

        public int GetFolioID(ref ITicketMessage ticketMessage, string commonIdentifier, string targetStrategy = null, string targetSubStrategy = null, bool forceStrategyFolio = false)
        {
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                if (rootportfolios.Keys.Contains(commonIdentifier))
                {
                    if ((useStrategySubfolios || forceStrategyFolio) && !String.IsNullOrEmpty(targetStrategy))
                    {
                        int fundFolioID = rootportfolios[commonIdentifier];
                        int strategyFolioID = GetStrategyFolioID(fundFolioID, targetStrategy, targetSubStrategy);
                        if (strategyFolioID != 0)
                        {
                            CSMLog.Write("RBC_Filter", "GetFolioID()", CSMLog.eMVerbosity.M_debug, "Selected (strategy) folio with id = " + strategyFolioID + " and name = " + targetStrategy + ((!String.IsNullOrEmpty(targetSubStrategy)) ? (" (" + targetSubStrategy + ")") : ("")));
                            TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, strategyFolioID);
                            return strategyFolioID;
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "GetFolioID()", CSMLog.eMVerbosity.M_error, "Could not find a valid strategy folio with name = " + targetStrategy + ((!String.IsNullOrEmpty(targetSubStrategy)) ? (" (" + targetSubStrategy + ")") : ("")) + ", selecting fund folio with ID = " + fundFolioID);
                            TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, fundFolioID);
                            return fundFolioID;
                        }
                    }
                    else
                    {
                        CSMLog.Write("RBC_Filter", "GetFolioID()", CSMLog.eMVerbosity.M_debug, "Selected folio with id = " + rootportfolios[commonIdentifier]);
                        TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, rootportfolios[commonIdentifier]);
                        return rootportfolios[commonIdentifier];
                    }
                }
                else
                {
                    CSMLog.Write("RBC_Filter", "GetFolioID()", CSMLog.eMVerbosity.M_error, "Could not find a valid portfolio associated with CommonIdentifier: " + commonIdentifier);
                }
            }
            else
            {
                CSMLog.Write("RBC_Filter", "GetFolioID()", CSMLog.eMVerbosity.M_error, "Invalid argument, CommonIdentifier cannot be null or empty");
            }
            return 0;
        }

        public bool CheckAllowedListExtFundId(string commonIdentifier)
        {
            bool retval = false;
            if (String.IsNullOrEmpty(ExtFundIdFilterFile))
            {
                retval = true; //allowed
            }
            else if (!String.IsNullOrEmpty(commonIdentifier))
            {
                if (AllowedExtFundIds.Contains(commonIdentifier.ToUpper()))
                {
                    retval = true;
                }
            }
            else
            {
                retval = true;
            }
            return retval;
        }

        protected bool CheckAcceptedTransactionType(string type)
        {
            bool res = true;
            if (!String.IsNullOrEmpty(type))
            {
                if (UnacceptedTransactionTypeList.Contains(type.ToUpper()))
                    res = false; // not accepted
            }
            return res;
        }

        public int OverrideDefaultKernelWorkflow(ref ITicketMessage ticketMessage, string commonIdentifier)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                if (OverrideCreationEvent)
                {
                    CSMLog.Write("RBC_Filter", "OverrideDefaultKernelWorkflow()", CSMLog.eMVerbosity.M_debug, "OverrideCreationEvent parameter enebled");
                    if (IsMAMLCommonIdentifeir(commonIdentifier) && MAMLTradeCreationEventId != 0)
                    {
                        CSMLog.Write("RBC_Filter", "OverrideDefaultKernelWorkflow()", CSMLog.eMVerbosity.M_debug, "Overriding creation event to MAMLTradeCreationEvent");
                        TrySetTicketField<int>(ref ticketMessage, FieldId.CREATION_UPDATE_EVENT_ID, MAMLTradeCreationEventId);
                        retval = MAMLTradeCreationEventId;
                    }
                    else if (DelegateTradeCreationEventId != 0)
                    {
                        CSMLog.Write("RBC_Filter", "OverrideDefaultKernelWorkflow()", CSMLog.eMVerbosity.M_debug, "Overriding creation event to DelegateTradeCreationEvent");
                        TrySetTicketField<int>(ref ticketMessage, FieldId.CREATION_UPDATE_EVENT_ID, DelegateTradeCreationEventId);
                        retval = DelegateTradeCreationEventId;
                    }
                }
            }
            return retval;
        }

        public bool IsMAMLCommonIdentifeir(string commonIdentifier)
        {
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                string last8chars = GetLast8Characters(commonIdentifier);
                if (!String.IsNullOrEmpty(last8chars))
                {
                    CSMLog.Write("RBC_Filter", "IsMAMLCommonIdentifeir()", CSMLog.eMVerbosity.M_debug, "Checking if " + last8chars + " is a MAML Z Code");
                    if (MAMLZCodesList.Contains(last8chars.ToUpper()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetPrimeBrokerIDofFund(int folioID)
        {
            try
            {
                CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioID);
                if (folio != null)
                {
                    if (!folio.IsLoaded())
                        folio.Load();
                }
                else
                {
                    CSMLog.Write("RBC_Filter", "GetPrimeBrokerIDofFund()", CSMLog.eMVerbosity.M_error, "Invalid argument: portfolio does not exist!");
                    return 0;
                }
                CSMAmFund fund = CSMAmFund.GetFundFromFolio(folioID); //this function might not work correctly if the folio is not loaded
                ArrayList primeBrokers = fund.GetPrimeBrokers();
                if (primeBrokers.Count > 0)
                {
                    return (int)primeBrokers[0];
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "GetPrimeBrokerIDofFund()", CSMLog.eMVerbosity.M_error, "Exception occured while trying to find the prime broker of the fund: " + ex.Message);
            }
            return 0;
        }

        public int GetCounterpartyID(ref ITicketMessage ticketMessage, string commonIdentifier, CSVDocumentClass documentClass, string brokerBIC = null, int folio_id = 0)
        {
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                if (IsMAMLCommonIdentifeir(commonIdentifier) || (!IsMAMLCommonIdentifeir(commonIdentifier) && !useDefaultCounterparty))
                {
                    switch (documentClass)
                    {
                        case CSVDocumentClass.Cash:
                        case CSVDocumentClass.TACash:
                        case CSVDocumentClass.Invoice:
                            {
                                // use fund prime broker as counterparty
                                int primebrokerID = GetPrimeBrokerIDofFund(folio_id);
                                if (primebrokerID != 0)
                                {
                                    TrySetTicketField<int>(ref ticketMessage, FieldId.COUNTERPARTYID_PROPERTY_NAME, primebrokerID);
                                    return primebrokerID;
                                }
                                else
                                {
                                    CSMLog.Write("RBC_Filter", "GetCounterpartyID()", CSMLog.eMVerbosity.M_debug, "Failed to find a valid Prime Broker");
                                    return 0;
                                }
                            } break;
                        case CSVDocumentClass.ForexHedge :
                            {
                                //use default hedge counterparty
                                if(defaultFXHedgeCounterpartyId != 0)
                                {
                                    TrySetTicketField<int>(ref ticketMessage, FieldId.COUNTERPARTYID_PROPERTY_NAME, defaultFXHedgeCounterpartyId);
                                }
                                return 0;
                            } break;
                        default :
                            {
                                if (!String.IsNullOrEmpty(brokerBIC))
                                {
                                    CSMLog.Write("RBC_Filter", "GetCounterpartyID()", CSMLog.eMVerbosity.M_debug, "Getting counterparty by BICCode (SWIFT)");
                                    int counterparty_id = GetEntityFromSWIFT(brokerBIC);
                                    if (counterparty_id != 0)
                                    {
                                        CSMLog.Write("RBC_Filter", "GetCounterpartyID()", CSMLog.eMVerbosity.M_debug, "Found counterparty id = " + counterparty_id + " with BICCode (SWIFT) = " + brokerBIC);
                                        TrySetTicketField<int>(ref ticketMessage, FieldId.COUNTERPARTYID_PROPERTY_NAME, counterparty_id);
                                        return counterparty_id;
                                    }
                                    else
                                    {
                                        CSMLog.Write("RBC_Filter", "GetCounterpartyID()", CSMLog.eMVerbosity.M_error, "Could not find counterparty with BICCode (SWIFT) = " + brokerBIC);
                                        return 0;
                                    }
                                }
                            } break;
                    }
                }
                else if (defaultCounterpartyId != 0 && useDefaultCounterparty)
                {
                    TrySetTicketField<int>(ref ticketMessage, FieldId.COUNTERPARTYID_PROPERTY_NAME, defaultCounterpartyId);
                    return defaultCounterpartyId;
                }
            }
            else
            {
                CSMLog.Write("RBC_Filter", "GetCounterpartyID()", CSMLog.eMVerbosity.M_error, "Invalid argument, CommonIdentifier cannot be null or empty");
                return 0;
            }
            return 0;
        }

        public int GetBrokerID(ref ITicketMessage ticketMessage, string commonIdentifier, string brokerBIC = null)
        {
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                string last8chars = GetLast8Characters(commonIdentifier);
                if (!String.IsNullOrEmpty(last8chars))
                {
                    CSMLog.Write("RBC_Filter", "GetBrokerID()", CSMLog.eMVerbosity.M_debug, "Checking if " + last8chars + " is a MAML Z Code");
                    if (MAMLZCodesList.Contains(last8chars.ToUpper()) && !String.IsNullOrEmpty(brokerBIC))
                    {
                        return 0;
                        /* Feature removed, will not set anything for MAML trades
                        //Use BICCode
                        CSMLog.Write("RBC_Filter", "GetBrokerID()", CSMLog.eMVerbosity.M_debug, "Getting broker by BICCode (SWIFT)");
                        int broker_id = GetEntityFromSWIFT(brokerBIC);
                        if (broker_id != 0)
                        {
                            CSMLog.Write("RBC_Filter", "GetBrokerID()", CSMLog.eMVerbosity.M_debug, "Found broker id = " + broker_id + " with BICCode (SWIFT) = " + brokerBIC);
                            TrySetTicketField<int>(ref ticketMessage, FieldId.BROKERID_PROPERTY_NAME, broker_id);
                            return broker_id;
                        }
                        else
                        {
                            CSMLog.Write("RBC_Filter", "GetBrokerID()", CSMLog.eMVerbosity.M_error, "Could not find broker with BICCode (SWIFT) = " + brokerBIC);
                            return 0;
                        }
                         * */
                    }
                }
                CSMLog.Write("RBC_Filter", "GetBrokerID()", CSMLog.eMVerbosity.M_debug, "Getting broker by DelegeteManagerID");
                //Use DelelgateManagerId
                Tuple<int, int, string> delegatemanager = delegatemanagers.Find(x => (x.Item3.ToUpper().CompareTo(commonIdentifier.ToUpper()) == 0));
                if (delegatemanager != null)
                {
                    CSMLog.Write("RBC_Filter", "GetBrokerID()", CSMLog.eMVerbosity.M_debug, "Found (delegate manager) broker id = " + delegatemanager.Item2 + " with account = " + commonIdentifier);
                    TrySetTicketField<int>(ref ticketMessage, FieldId.BROKERID_PROPERTY_NAME, delegatemanager.Item2);
                    return delegatemanager.Item2;
                }
                else
                {
                    CSMLog.Write("RBC_Filter", "GetBrokerID()", CSMLog.eMVerbosity.M_error, "Could not find broker with account = " + commonIdentifier);
                    return 0;
                }
            }
            else
            {
                CSMLog.Write("RBC_Filter", "GetBrokerID()", CSMLog.eMVerbosity.M_error, "Invalid argument, CommonIdentifier cannot be null or empty");
                return 0;
            }
        }

        #endregion

        #region Instrument query methods

        protected string GetUnderlyingCCY(string refName, string refValue)
        {
            string res = "";
            try
            {
                // first check
                string sql = "select DEVISE_TO_STR(DEVISECTT) from titres where REFERENCE = :REFERENCE";
                OracleParameter parameter = new OracleParameter(":REFERENCE", refValue);
                List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                res = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));

                // second check
                if (String.IsNullOrEmpty(res))
                {
                    sql = "select DEVISE_TO_STR(t.DEVISECTT) from titres t"
                            + " inner join EXTRNL_REFERENCES_INSTRUMENTS ei"
                            + " on t.sicovam = ei.SOPHIS_IDENT"
                            + " inner join EXTRNL_REFERENCES_DEFINITION ed"
                            + " on ei.ref_ident = ed.ref_ident and ed.ref_name = :refName and ei.value = :refValue";

                    OracleParameter parameter1 = new OracleParameter(":refName", refName);
                    OracleParameter parameter2 = new OracleParameter(":refValue", refValue);
                    parameters = new List<OracleParameter>() { parameter1, parameter2 };
                    res = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "GetUnderlyingCCY", CSMLog.eMVerbosity.M_error, "Error occurred during GetUnderlyingCCY: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            CSMLog.Write("RBC_Filter", "GetUnderlyingCCY", CSMLog.eMVerbosity.M_debug, "Underlying ccy found by instrument " + refValue + " = " + res);
            return res;
        }

        protected int GetDepositary(string commonIdentifier)
        {
            int res = 0;
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                try
                {
                    CSMLog.Write("RBC_Filter", "GetDepositaryByNostroAccount", CSMLog.eMVerbosity.M_debug, "Trying to get depositary ID by commonIdentifier");
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "select depositary from BO_TREASURY_ACCOUNT where ACCOUNT_AT_CUSTODIAN = '" + commonIdentifier + "'";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                res = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetDepositaryByNostroAccount", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get depositary from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return res;
        }

        protected int GetFundSicovam(int folioID)
        {
            int res = 0;
            if (folioID != 0)
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "select sicovam from funds where TRADINGFOLIO = " + folioID;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                res = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetFundSicovam", CSMLog.eMVerbosity.M_error,
                        "Error occurred while trying to get fund sicovam: " + ex.Message + ". InnerException: " +
                        ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return res;

        }

        protected string GetFundReference(int folioID)
        {
            string res = "";
            if (folioID != 0)
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "select reference from funds where TRADINGFOLIO = " + folioID;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                res = (string)reader["REFERENCE"];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetFundReference", CSMLog.eMVerbosity.M_error,
                        "Error occurred while trying to get fund reference: " + ex.Message + ". InnerException: " +
                        ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return res;

        }

        protected int GetRootFundFolio(int folioID)
        {
            int res = 0;
            if (folioID != 0)
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "select root_trading_folio from fund_folios where ident = " + folioID;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                res = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetRootFundFolio", CSMLog.eMVerbosity.M_error,
                        "Error occurred while trying to get entity: " + ex.Message + ". InnerException: " +
                        ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return res;
        }

        private bool IsNDF(string ccyPair)
        {
            bool res = false;
            string sql = "SELECT count(*) FROM TITRES T WHERE T.TYPE = 'E' AND OWN_TYPE=1 and reference = :reference";
            OracleParameter parameter = new OracleParameter(":reference", ccyPair);
            List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
            int count = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
            res = count == 1;
            CSMLog.Write("RBC_Filter", "GetPrimeBrokerBySicovam", CSMLog.eMVerbosity.M_debug, "Is the pair " + ccyPair + " is an NDF? Res =" + res.ToString());
            return res;
        }

        protected int GetPrimeBrokerBySicovam(int sicovam)
        {
            int res = 0;
            if (sicovam != 0)
            {
                try
                {
                    CSMLog.Write("RBC_Filter", "GetPrimeBrokerBySicovam", CSMLog.eMVerbosity.M_debug, "Trying to get prime broker by sicovam");
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "select ident from fund_primebrokers where sicovam = " + sicovam;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                res = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetPrimeBrokerBySicovam", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get prime broker: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return res;
        }

        protected int GetPrimeBroker(string commonIdentifier)
        {
            int res = 0;
            try
            {
                int rootFundFolio = GetRootFundFolio(rootportfolios[commonIdentifier]);
                if (rootFundFolio > 0)
                {
                    int sicovam = GetFundSicovam(rootFundFolio);
                    if (sicovam > 0)
                    {
                        res = GetPrimeBrokerBySicovam(sicovam);
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "GetPrimeBroker", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get prime broker: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return res;
        }

        protected int SetDepositary(ref ITicketMessage ticketMessage, string commonIdentifier)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                int depo = GetDepositary(commonIdentifier);
                CSMLog.Write("RBC_Filter", "SetDepositary", CSMLog.eMVerbosity.M_debug, "Depositary get from nostro account = " + commonIdentifier);
                if (depo > 0)
                {
                    TrySetTicketField<int>(ref ticketMessage, FieldId.DEPOSITARYID_PROPERTY_NAME, depo);
                    retval = depo;
                }
                else
                {
                    depo = GetPrimeBroker(commonIdentifier); // from root fund
                    CSMLog.Write("RBC_Filter", "SetDepositary", CSMLog.eMVerbosity.M_debug, "Prime broker get from fund = " + commonIdentifier);
                    if (depo > 0)
                    {
                        TrySetTicketField<int>(ref ticketMessage, FieldId.DEPOSITARYID_PROPERTY_NAME, depo);
                        retval = depo;
                    }
                }
            }
            return retval;
        }

        public int GetSicovamByNameOrExternalReference(string nameOrExtRef,string extRefType)
        {
            int retval = 0;
            CSMLog.Write("RBC_Filter", "GetSicovamByNameOrExternalReference()", CSMLog.eMVerbosity.M_debug, "Trying to get SICOVAM by name or reference: " + nameOrExtRef);
            retval = GetSicovamByExternalReference(nameOrExtRef, extRefType);
            if (retval == 0)
            {
                GetSicovamByName(nameOrExtRef, out retval);
            }   
            return retval;
        }

        public int GetSicovamByExternalReference(string extRef, string extRefType)
        {
            int retval = 0;
            CSMLog.Write("RBC_Filter", "GetSicovamByExternalReference()", CSMLog.eMVerbosity.M_debug, "Trying to get SICOVAM by external reference: " + extRef);
            if (!String.IsNullOrEmpty(extRef) && !String.IsNullOrEmpty(extRefType))
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT I.SOPHIS_IDENT,D.REF_NAME,I.VALUE FROM EXTRNL_REFERENCES_INSTRUMENTS I, EXTRNL_REFERENCES_DEFINITION D WHERE I.REF_IDENT = D.REF_IDENT AND D.REF_NAME = '" + extRefType + "' AND I.VALUE ='" + extRef + "'";
                        CSMLog.Write("RBC_Filter", "GetSicovamByExternalReference()", CSMLog.eMVerbosity.M_debug, "Trying to execute query: " + cmd.CommandText);
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                retval = Convert.ToInt32(reader["SOPHIS_IDENT"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetAccountIDFromDelegateManager", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get sicovam by " + extRefType + " from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        public bool CheckIfSicovamExists(int sicovam)
        {
            CSMLog.Write("RBC_Filter", "CheckIfSicovamExists()", CSMLog.eMVerbosity.M_debug, "Checking if SICOVAM exists: " + sicovam);
            bool retval = false;
            int foundvalue = 0;
            if (sicovam != 0)
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT SICOVAM FROM TITRES WHERE SICOVAM = '" + sicovam + "'";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                foundvalue = Convert.ToInt32(reader["SICOVAM"]);
                            }
                        }
                    }
                    if (foundvalue == sicovam)
                    {
                        retval = true;
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "CheckIfSicovamExists", CSMLog.eMVerbosity.M_error, "Error occurred while trying to check if sicovam exists in database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        public bool CheckIfHashExists(string dbHash)
        {
            CSMLog.Write("RBC_Filter", "CheckIfHashExists()", CSMLog.eMVerbosity.M_debug, "Checking if hash exists: " + dbHash);
            bool retval = false;
            int foundvalue = 0;
            if (!String.IsNullOrEmpty(dbHash))
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT COUNT(*) FROM AJUSTEMENTS WHERE COMM = '" + dbHash + "'";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                foundvalue = Convert.ToInt32(reader["SICOVAM"]);
                            }
                        }
                    }
                    if (foundvalue > 0)
                    {
                        retval = true;
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "CheckIfHashExists", CSMLog.eMVerbosity.M_error, "Error occurred while trying to check if hash exists in database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        public bool GetSicovamByName(string name, out int sicovam) //By name or by reference
        {
            CSMLog.Write("RBC_Filter", "GetSicovamByName()", CSMLog.eMVerbosity.M_debug, "Trying to get SICOVAM by name: " + name);
            bool retval = false;
            sicovam = 0;
            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT SICOVAM FROM TITRES WHERE LIBELLE = '" + name + "' OR REFERENCE = '" + name + "'";
                        CSMLog.Write("RBC_Filter", "GetSicovamByName()", CSMLog.eMVerbosity.M_debug, "Trying to execute query: " + cmd.CommandText);
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool success = true;
                                try
                                {
                                    sicovam = Convert.ToInt32(reader["SICOVAM"]);
                                }
                                catch (Exception Ex)
                                {
                                    CSMLog.Write("RBC_Filter", "GetSicovamByName()", CSMLog.eMVerbosity.M_error, "Failed to convert sicovam to int: " + Ex.Message + ". StackTrace: " + Ex.StackTrace);
                                    success = false;
                                }
                                retval = success;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetSicovamByName", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get sicovam by instrument name from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        public int GetCashInstrumentSicovam(string Currency, string nameFormat, string feeName = null, string dstAccount = null, string srcAccount = null, string businessEvent = null, string counterparty = null, string allotment = null, int folio_id = 0,string fund = null, string commonIdentifier = null)
        {
            int retval = 0;
            CSMLog.Write("RBC_Filter", "GetCashInstrumentSicovam()", CSMLog.eMVerbosity.M_debug, "Trying to get cash instrument SICOVAM by name: " + Currency);
            if (!String.IsNullOrEmpty(Currency))
            {
                if (!String.IsNullOrEmpty(nameFormat))
                {
                    nameFormat = nameFormat.Replace("%CURRENCY%", Currency);
                }
                if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(feeName))
                {
                    nameFormat = nameFormat.Replace("%FEETYPE%", feeName);
                }
                if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(dstAccount))
                {
                    nameFormat = nameFormat.Replace("%DESTINATIONACCOUNT%", dstAccount);
                }
                if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(srcAccount))
                {
                    nameFormat = nameFormat.Replace("%SOURCEACCOUNT%", srcAccount);
                }
                if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(businessEvent))
                {
                    nameFormat = nameFormat.Replace("%BUSINESSEVENT%", businessEvent);
                }
                if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(counterparty))
                {
                    nameFormat = nameFormat.Replace("%COUNTERPARTY%", counterparty);
                }
                if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(allotment))
                {
                    nameFormat = nameFormat.Replace("%ALLOTMENT%", allotment);
                }
                if (!String.IsNullOrEmpty(nameFormat) && (!String.IsNullOrEmpty(fund) || folio_id != 0))
                {
                    if(!String.IsNullOrEmpty(fund))
                    {
                        nameFormat = nameFormat.Replace("%FUND%", fund);
                    }
                    if (folio_id != 0)
                    {
                        int fund_folio = GetRootFundFolio(folio_id);
                        string reference = GetFundReference(fund_folio);
                        if (!String.IsNullOrEmpty(reference))
                        {
                            nameFormat = nameFormat.Replace("%FUND%", reference);
                        }
                    }
                }
                if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(commonIdentifier))
                {
                    nameFormat = nameFormat.Replace("%EXTFUNDID%", commonIdentifier);
                    nameFormat = nameFormat.Replace("%COMMON_ID%", commonIdentifier);
                }
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT MAX(SICOVAM) FROM TITRES WHERE TYPE = 'C' AND STR_TO_DEVISE('" + Currency + "') = DEVISECTT AND LIBELLE like q'<%" + nameFormat + "%>'";
                        CSMLog.Write("RBC_Filter", "GetCashInstrumentSicovam", CSMLog.eMVerbosity.M_debug, "About to run query: " + cmd.CommandText);
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                retval = reader[0] != DBNull.Value ? Convert.ToInt32(reader[0]) : 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetCashInstrumentSicovam", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get instrument from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        protected int GetBussinessEvent(CorporateActionType caType)
        {
            int res = 0;
            if (CorporateActionsBusinessEvents.ContainsKey(caType))
            {
                if (businessevents.ContainsKey(CorporateActionsBusinessEvents[caType]))
                    res = businessevents[CorporateActionsBusinessEvents[caType]];
                else
                    CSMLog.Write("RBC_Filter", "GetBussinessEvent", CSMLog.eMVerbosity.M_debug, "Could not find bus event from cache by CA type" + Enum.GetName(typeof(CorporateActionType), caType));
            }
            else
                CSMLog.Write("RBC_Filter", "GetBussinessEvent", CSMLog.eMVerbosity.M_debug, "Could not find bus event from cache by CA type" + Enum.GetName(typeof(CorporateActionType), caType));
            return res;
        }

        #endregion

        #region Other query methods

        public int GetKernelWorkflowEventIDByName(string name)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT ID FROM BO_KERNEL_EVENTS WHERE NAME = '" + name + "'";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                retval = Convert.ToInt32(reader["ID"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetKernelWorkflowEventIDByName", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get kernel workflow event id from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        public int GetAccountIDFromDelegateManager(string CommonIdentifier, int DelegateManagerID)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(CommonIdentifier))
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT T.CODE, S.BO_TREASURY_ACCOUNT_ID, S.ACCOUNT_AT_CUSTODIAN, S.ACCOUNT_AT_AGENT FROM BO_SSI_PATH S, TIERSSETTLEMENT T WHERE T.SSI_PATH_ID = S.SSI_PATH_ID AND S.BO_TREASURY_ACCOUNT_ID != 0 AND  S.ACCOUNT_AT_CUSTODIAN = '" + CommonIdentifier + "' AND T.CODE = '" + DelegateManagerID + "'";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                retval = Convert.ToInt32(reader["BO_TREASURY_ACCOUNT_ID"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetAccountIDFromDelegateManager", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get account id from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        public int GetEntityFromSWIFT(string BICCode)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(BICCode))
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT CODE FROM TIERSGENERAL WHERE SWIFT = '" + BICCode + "'";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                retval = Convert.ToInt32(reader["CODE"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetEntityFromSWIFT", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get entity by SWIFT from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        public int GetStrategyFolioID(int RootFundFolioID,string strategyFolioName,string subStrategyFolioName = null)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(strategyFolioName))
            {
                try
                {
                    if (!String.IsNullOrEmpty(subStrategyFolioName))
                    {
                        using (var cmd = new OracleCommand())
                        {
                            cmd.Connection = DBContext.Connection;
                            cmd.CommandText = "SELECT IDENT FROM FOLIO WHERE NAME = '" + subStrategyFolioName + "' AND MGR IN (SELECT IDENT FROM FOLIO WHERE MGR = '" + RootFundFolioID + "' AND NAME = '" + strategyFolioName + "')";
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    retval = Convert.ToInt32(reader["IDENT"]);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var cmd = new OracleCommand())
                        {
                            cmd.Connection = DBContext.Connection;
                            cmd.CommandText = "SELECT IDENT FROM FOLIO WHERE NAME like '%" + strategyFolioName + "%' AND MGR = " + RootFundFolioID + " and rownum <= 1";
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    retval = Convert.ToInt32(reader["IDENT"]);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetStrategyFolioID", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get folio ID from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            return retval;
        }

        public int GetEntityIDByName(string name)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT IDENT FROM TIERS WHERE NAME = '" + name + "'";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                retval = Convert.ToInt32(reader["IDENT"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetEntityIDByName", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get entity from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            else
            {
                CSMLog.Write("RBC_Filter", "GetEntityIDByName()", CSMLog.eMVerbosity.M_error, "Invalid argument, name cannot be null or empty");
            }
            return retval;
        }

        public string GetEntityNameByID(int id)
        {
            string retval = "";
            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = "SELECT NAME FROM TIERS WHERE IDENT = " + id;
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            retval = (string)reader["NAME"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "GetEntityNameByID", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get entity from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return retval;
        }

        public int ResolveCashFolioID(ref ITicketMessage ticketMessage, string commonIdentifier)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                try
                {
                    CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_debug, "Trying to get Cash folio ID by ACCOUNT_LEVEL_FOLIO");
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT ACCOUNT_LEVEL_FOLIO FROM BO_TREASURY_ACCOUNT WHERE ID = (SELECT ID FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = '" + commonIdentifier + "')";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                retval = Convert.ToInt32(reader["ACCOUNT_LEVEL_FOLIO"]);
                                if (retval != 0)
                                {
                                    TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, retval);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get ACCOUNT_LEVEL_FOLIO from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                if (retval == 0)
                {
                    try
                    {
                        CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_debug, "Trying to get Cash folio ID by AlternativeCashFolioID");
                        using (var cmd = new OracleCommand())
                        {
                            cmd.Connection = DBContext.Connection;
                            cmd.CommandText = "SELECT VALUE FROM BO_TREASURY_EXT_REF WHERE REF_ID = (SELECT REF_ID FROM BO_TREASURY_EXT_REF_DEF WHERE REF_NAME = 'AlternativeCashFolioID') AND ACC_ID = (SELECT ID FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = '" + commonIdentifier + "')";
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    retval = Convert.ToInt32(reader["VALUE"]);
                                    if (retval != 0)
                                    {
                                        TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, retval);
                                    }
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get BO_TREASURY_EXT_REF value from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                if (retval == 0)
                {
                    try
                    {
                        CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_debug, "Trying to get Cash folio ID by name");
                        using (var cmd = new OracleCommand())
                        {
                            cmd.Connection = DBContext.Connection;
                            cmd.CommandText = "SELECT NAME,IDENT FROM FOLIO WHERE MGR = (SELECT VALUE FROM BO_TREASURY_EXT_REF WHERE REF_ID = (SELECT REF_ID FROM BO_TREASURY_EXT_REF_DEF WHERE REF_NAME = 'RootPortfolio')  AND ACC_ID = (SELECT ID FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = '" + commonIdentifier + "'))";
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string name = (string)reader["NAME"];
                                    if (!String.IsNullOrEmpty(name))
                                    {
                                        if (name.ToUpper().Contains("CASH"))
                                        {
                                            retval = Convert.ToInt32(reader["IDENT"]);
                                            TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, retval);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get Cash folio ID by name from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }

                if (retval == 0)
                {
                    CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_debug, "Trying to get Cash folio ID by MediolanumRMA.DefaultErrorFolio custom parameter");
                    if (DefaultErrorFolio != 0)
                    {
                        retval = DefaultErrorFolio;
                        TrySetTicketField<int>(ref ticketMessage, FieldId.BOOKID_PROPERTY_NAME, retval);
                        TrySetTicketField<string>(ref ticketMessage, FieldId.COMMENTS_PROPERTY_NAME, "Unable to find folio for cash account");
                    }
                    else
                    {
                        CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_error, "MediolanumRMA.DefaultErrorFolio custom parameter not set");
                    }
                    //retval = GetFolioID(ref ticketMessage, commonIdentifier, "Cash", null, true);
                }
            }
            else
            {
                CSMLog.Write("RBC_Filter", "ResolveCashFolioID", CSMLog.eMVerbosity.M_error, "Invalid argument, commonIdentifier cannot be null or empty");
            }
            return retval;
        }

        protected int GetUserID(string username)
        {
            int retval = 0;
            if (!String.IsNullOrEmpty(username))
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "SELECT IDENT FROM riskusers WHERE NAME = '" + username + "'";
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                retval = reader[0] != DBNull.Value ? Convert.ToInt32(reader[0]) : 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetUserID", CSMLog.eMVerbosity.M_error, "Error occurred while trying to get username from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
            else
            {
                CSMLog.Write("RBC_Filter", "GetEntityIDByName()", CSMLog.eMVerbosity.M_error, "Invalid argument, name cannot be null or empty");
            }
            return retval;
        }

        #endregion

        #region Field setting methods
        public static void TrySetTicketField<T>(ref ITicketMessage ticketMessage, FieldId fieldId, T value)
        {
            bool success = true;
            try
            {
                ticketMessage.add(fieldId, value);
            }
            catch (Exception ex)
            {
                success = false;
                CSMLog.Write("RBC_Filter", "TrySetTicketField", CSMLog.eMVerbosity.M_error, "Failed to set value = " + value + " to message (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")). Exception: " + ex.Message + ". Stack trace: " + ex.StackTrace);
            }
            finally
            {
                if (success)
                {
                    CSMLog.Write("RBC_Filter", "TrySetTicketField", CSMLog.eMVerbosity.M_debug, "Successfully added (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) value = " + value + " to message");
                }
            }
        }

        public static void TrySetTicketField<T>(ref ITicketMessage ticketMessage, FieldId fieldId, bool compare, T value1, T value2)
        {
            if (compare)
            {
                TrySetTicketField<T>(ref ticketMessage, fieldId, value1);
            }
            else
            {
                TrySetTicketField<T>(ref ticketMessage, fieldId, value2);
            }
        }

        public static void TrySetTicketField(ref ITicketMessage ticketMessage, FieldId fieldId, List<string> list, int index)
        {
            string value = CSxValidationUtil.TryAccessListValue(list, index);
            bool success = true;
            try
            {
                if (!String.IsNullOrEmpty(value))
                {
                    ticketMessage.add(fieldId, value);
                }
                else
                {
                    success = false;
                    CSMLog.Write("RBC_Filter", "TrySetTicketField", CSMLog.eMVerbosity.M_debug, "Value = " + value + " from list index = " + index + " was not assigned to message (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) because it was null or empty");
                }
            }
            catch (Exception ex)
            {
                success = false;
                CSMLog.Write("RBC_Filter", "TrySetTicketField", CSMLog.eMVerbosity.M_error, "Failed to set value = " + value + " from list index = " + index + " to message (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")). Exception: " + ex.Message + ". Stack trace: " + ex.StackTrace);
            }
            finally
            {
                if (success)
                {
                    CSMLog.Write("RBC_Filter", "TrySetTicketField", CSMLog.eMVerbosity.M_debug, "Successfully added (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) value = " + value + " from list index = " + index + " to message");
                }
            }
        }

        public static void TrySetTicketField(ref ITicketMessage ticketMessage, FieldId fieldId, List<string> list, bool compare, int index1, int index2)
        {
            if (compare)
            {
                TrySetTicketField(ref ticketMessage, fieldId, list, index1);
            }
            else
            {
                TrySetTicketField(ref ticketMessage, fieldId, list, index2);
            }
        }

        public static void TrySetDoubleFromList(ref ITicketMessage ticketMessage, FieldId fieldId, List<string> list, int index, bool negateValue = false)
        {
            string value = CSxValidationUtil.TryAccessListValue(list, index);
            double doubleValue = 0.0;
            bool success = true;
            try
            {
                if (!String.IsNullOrEmpty(value))
                {
                    doubleValue = Double.Parse(value);
                    if (!negateValue)
                    {
                        ticketMessage.add(fieldId, doubleValue);
                    }
                    else
                    {
                        ticketMessage.add(fieldId, -doubleValue);
                    }
                    //ticketMessage.add(fieldId, value);
                }
                else
                {
                    success = false;
                    CSMLog.Write("RBC_Filter", "TrySetDoubleFromList", CSMLog.eMVerbosity.M_debug,  ((negateValue)?("[Reversal/Negative value]"):("")) + "Value = " + value + " from list index = " + index + " was not assigned to message (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) because it was null or empty");
                }
            }
            catch (Exception ex)
            {
                success = false;
                CSMLog.Write("RBC_Filter", "TrySetDoubleFromList", CSMLog.eMVerbosity.M_error, ((negateValue) ? ("[Reversal/Negative value]") : ("")) + "Failed to set value = " + value + " from list index = " + index + " to message (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")). Exception: " + ex.Message + ". Stack trace: " + ex.StackTrace);
            }
            finally
            {
                if (success)
                {
                    CSMLog.Write("RBC_Filter", "TrySetDoubleFromList", CSMLog.eMVerbosity.M_debug, ((negateValue) ? ("[Reversal/Negative value]") : ("")) + "Successfully added (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) value = " + ((negateValue) ? (-doubleValue) : (doubleValue)) + " from list index = " + index + " to message");
                }
            }
        }

        #endregion

        #region Other methods
        public string GetLast8Characters(string str)
        {
            string retval = "";
            if (!String.IsNullOrEmpty(str))
            {
                try
                {
                    retval = str.Substring((str.Length - 8), 8);
                }
                catch (Exception ex)
                {
                    CSMLog.Write("RBC_Filter", "GetLast8Characters()", CSMLog.eMVerbosity.M_error, "Failed to get last 8 characters from commonIdentifier: " + ex.Message);
                }
            }
            else
            {
                CSMLog.Write("RBC_Filter", "GetLast8Characters()", CSMLog.eMVerbosity.M_error, "NULL or empty argument");
            }
            return retval;
        }

        public string GenerateSha1Hash(List<string> inputList,int ReversalFlagColumnID = -1)
        {
            CSMLog.Write("RBC_Filter", "GenerateSha1Hash()", CSMLog.eMVerbosity.M_debug, "Trying to generate SHA1 hash");
            string retval = "";
            if (inputList == null)
            {
                CSMLog.Write("RBC_Filter", "GenerateSha1Hash()", CSMLog.eMVerbosity.M_error, "NULL argument provided. Generating hash from current timestamp.");
                return GenerateSha1Hash();
            }
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                string stringtohash = "";
                if (ReversalFlagColumnID != -1)
                {
                    CSMLog.Write("RBC_Filter", "GenerateSha1Hash()", CSMLog.eMVerbosity.M_debug, "ReversalFlagColumnID is NOT -1");
                    StringBuilder strb = new StringBuilder();
                    for (int i = 0; i < inputList.Count; i++)
                    {
                        if (i != ReversalFlagColumnID)
                        {
                            strb.Append(inputList[i]);
                        }
                    }
                    stringtohash = strb.ToString();
                }
                else
                {
                    stringtohash = String.Join("", inputList);
                }
                CSMLog.Write("RBC_Filter", "GenerateSha1Hash()", CSMLog.eMVerbosity.M_debug, "String to hash = " + stringtohash);
                byte[] sha1Hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(stringtohash));
                retval = BitConverter.ToString(sha1Hash).Replace("-", "");
            }
            CSMLog.Write("RBC_Filter", "GenerateSha1Hash()", CSMLog.eMVerbosity.M_debug, "SHA1 hash from CSV fields " + ((ReversalFlagColumnID != -1)?("[Ignored reversal flag]"):("")) + " : " + retval);
            return retval;
        }
       
        public string GenerateSha1Hash()
        {
            string retval = "";
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                string timtstamp = DateTime.Now.ToString();
                Random rnd = new Random();
                timtstamp += rnd.Next();
                byte[] sha1Hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(timtstamp));
                retval = BitConverter.ToString(sha1Hash).Replace("-", "");
            }
            CSMLog.Write("RBC_Filter", "GenerateSha1Hash()", CSMLog.eMVerbosity.M_debug, "SHA1 hash from timestamp: " + retval);
            return retval;
        }

        #endregion

        protected void TrySetUserField(ref ITicketMessage ticketMessage, string fieldName, string value)
        {
            try
            {
                ticketMessage.addUserField(fieldName, value);
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "TrySetUserField", CSMLog.eMVerbosity.M_error, "Failed to add user field. Error: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        void TrySetKind(ref ITicketMessage ticketMessage,RichMarketAdapter.ticket.TicketType ticketType)
        {
            try
            {
                RichMarketAdapter.xml.ticket.XMLTicket tk2 = (RichMarketAdapter.xml.ticket.XMLTicket) ticketMessage;
                tk2.Type = ticketType;
            }
            catch (Exception ex)
            {
                CSMLog.Write("RBC_Filter", "TrySetKind", CSMLog.eMVerbosity.M_error, "Failed to modify RMA ticket type. Error: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        
    }
}

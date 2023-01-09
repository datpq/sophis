using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using Mediolanum_RMA_FILTER.Tools;
using sophis.log;
using sophis.misc;
using sophis.utils;

namespace Mediolanum_RMA_FILTER
{
    public enum eRBCTicketType
    {
        Equity,
        Bond,
        Loan,
        Fund,
        Forex,
        Option,
        Future,
        Cash,
        Invoice,
        ForexHedge,
        TACash,
        Bond2,
        Bond3,//new format to include factor provided
        Swap,
        CorporateAction,
        Collateral,
        TermDeposit,
        AllCustodyTrans,
        GenericTrade,
        Unknown
    };

    public enum eCorporateActionType
    {
        Subscription = 9,
        RightsEntitlement = 13,
        SecuritiesWithdrawal = 14,
        ReverseSplit = 15,
        SpinOff = 16,
        SecuritiesDeposit = 17,
        Conversion = 33,
        CashCredit = 34,
        CapitalReductionWithPayment = 36,
        CapitalIncreaseVsPayment = 51,
        DistributionInSecurities = 52,
        BonusOrScripIssue = 55,
        PublicPurchaseOffer = 56,
        ExchangeOrExchangeOffer = 58,
        Merger = 59,
        AdditionalSplit = 60,
        Split = 61,
        CashDebit = 68,
        Dividend = 74,
        Interest = 75,
        FinalRedemption = 76,
        EarlyRedemption = 77,
        Amalgamation = 83,
        WarrantExercise = 90
    };

    public enum eMedioCachedData
    {
        Rootportfolios,
        Delegatemanagers,
        Businessevents,
        FundList
    };

    public enum eCollateralType
    {
        Equity,
        Bond,
        Cash
    };

    public static class RBCTicketType
    {
        public static Dictionary<eRBCTicketType, int /*ColumnCount*/> RBCFileColumnCount =
            new Dictionary<eRBCTicketType, int>()
            {
                {eRBCTicketType.Equity, EquityColumns.TotalCount},
                {eRBCTicketType.Bond, EquityColumns.TotalCount},
                {eRBCTicketType.Loan, EquityColumns.TotalCount},
                {eRBCTicketType.Fund, EquityColumns.TotalCount},
                {eRBCTicketType.Forex, ForexColumns.TotalCount},
                {eRBCTicketType.Option, OptionColumns.TotalCount},
                {eRBCTicketType.Future, FutureColumns.TotalCount},
                {eRBCTicketType.Cash, CashColumns.TotalCount},
                {eRBCTicketType.Invoice, InvoiceColumns.TotalCount},
                {eRBCTicketType.ForexHedge, ForexHedgeColumns.TotalCount},
                {eRBCTicketType.TACash, TACashColumns.TotalCount},
                {eRBCTicketType.Bond2, Bond2Columns.TotalCount},
                {eRBCTicketType.Swap, SwapColumns.TotalCount},
                {eRBCTicketType.CorporateAction, CAColumns.TotalCount},
                {eRBCTicketType.Collateral, CollateralColumns.TotalCount},
                {eRBCTicketType.TermDeposit, TermDepositColumns.TotalCount},
                {eRBCTicketType.AllCustodyTrans, AllCustodyTransColumns.TotalCount},
                {eRBCTicketType.GenericTrade, GenericTrade.TotalCount},
                {eRBCTicketType.Unknown, 0},
            };

        public static class EquityColumns
        {
            public const int ConsolidatedFundCode = 0;
            public const int ExternalFundIdentifier = 1;
            public const int FundName = 2;
            public const int TradeDate = 3;
            public const int SettlementDate = 4;
            public const int TransactionID = 5;
            public const int FundManagerReference = 6;
            public const int TransactionType = 7;
            public const int TransactionDescription = 8;
            public const int ReversalFlag = 9;
            public const int SecurityName = 10;
            public const int BrokerName = 11;
            public const int ISINCode = 12;
            public const int SecurityDescription = 13;
            public const int Quantity = 14;
            public const int Currency = 15;
            public const int Price = 16;
            public const int GrossAmount = 17;
            public const int Brokerage = 18;
            public const int Tax = 19;
            public const int Expenses = 20;
            public const int NetAmount = 21;
            public const int BrokerBICCode = 22;
            public const int FundManagerBICCode = 23;
            public const int BloombergCode = 24;
            public const int Type = 25;
            public const int TotalCount = 26;
        }

        public static class ForexColumns
        {
            public const int FundCustodyCode = 0;
            public const int ExternalFundIdentifier = 1;
            public const int FundName = 2;
            public const int TradeDate = 3;
            public const int SettlementDate = 4;
            public const int TransactionID = 5;
            public const int ReversalFlag = 6;
            public const int BuyCurrency = 7;
            public const int PurchasedAmount = 8;
            public const int SellCurrency = 9;
            public const int SoldAmount = 10;
            public const int FXRate = 11;
            public const int BrokerBICCode = 12;
            public const int BrokerName = 13;
            public const int FundManagerBICCode = 14;
            public const int NDFFlag = 15;
            public const int Type = 16;
            public const int TotalCount = 17;
        }

        public static class OptionColumns
        {
            public const int ConsolidatedFundCode = 0;
            public const int FundCode = 1;
            public const int ExternalFundIdentifier = 2;
            public const int FundName = 3;
            public const int FundCurrency = 4;
            public const int ManagerCode = 5;
            public const int CounterpartyOrBrokerCode = 6;
            public const int CounterpartyOrBrokerDescription = 7;
            public const int GTICode = 8;
            public const int GTIDescription = 9;
            public const int OptionType = 10;
            public const int OptionDescription = 11;
            public const int OptionRBCDISCode = 12;
            public const int OptionCurrency = 13;
            public const int OptionStrike = 14;
            public const int ContractSize = 15;
            public const int ContractNumber = 16;
            public const int TradeDate = 17;
            public const int NAVDate = 18;
            public const int SettlementDate = 19;
            public const int Quantity = 20;
            public const int Premium = 21;
            public const int TradeAmount = 22;
            public const int CommissionAmount = 23;
            public const int FeesAmountInTransactionCurrency = 24;
            public const int TransactionStatusCode = 25;
            public const int TransactionCode = 26;
            public const int ContractStatus = 27;
            public const int BrokerCode = 28;
            public const int BrokerName = 29;
            public const int Style = 30;
            public const int BloombergCode = 31;
            public const int FusionRef = 32;
            public const int UnderlyingAsset = 33;
            public const int BICCode = 34;
            public const int BuyOrSell = 35;
            public const int FundManagerBICCode = 36;
            public const int Type = 37;
            public const int TotalCount = 38;
        }

        public static class FutureColumns
        {
            public const int ConsolidatedFundCode = 0;
            public const int FundCode = 1;
            public const int ExternalFundIdentifier = 2;
            public const int FundName = 3;
            public const int FundCurrency = 4;
            public const int ManagerCode = 5;
            public const int CounterpartyOrBrokerCode = 6;
            public const int CounterpartyOrBrokerDescription = 7;
            public const int GTICode = 8;
            public const int GTIDescription = 9;
            public const int FutureRBCDISCode = 10;
            public const int UnderlyingISIN = 11;
            public const int FutureDescription = 12;
            public const int FutureMaturityDate = 13;
            public const int FutureCurrency = 14;
            public const int ContractNumber = 15;
            public const int TradeDate = 16;
            public const int NAVDate = 17;
            public const int SettlementDate = 18;
            public const int TransactionDescription = 19;
            public const int Quantity = 20;
            public const int Price = 21;
            public const int ContractSize = 22;
            public const int TradeAmount = 23;
            public const int CommissionAmount = 24;
            public const int FeesAmountInTransactionCurrency = 25;
            public const int TransactionCode = 26;
            public const int TransactionStatusCode = 27;
            public const int ContractStatus = 28;
            public const int FutureTradeCode = 29;
            public const int BloombergCode = 30;
            public const int UnderlyingAsset = 31;
            public const int BICCode = 32;
            public const int BuyOrSell = 33;
            public const int FundManagerBICcode = 34;
            public const int Type = 35;
            public const int TotalCount = 36;
        }

        public static class CashColumns
        {
            public const int FundCustodyCode = 0;
            public const int ExternalFundIdentifier = 1;
            public const int FundName = 2;
            public const int TradeDate = 3;
            public const int SettlementDate = 4;
            public const int IBAN = 5;
            public const int AccountType = 6;
            public const int Currency = 7;
            public const int TradeAmount = 8;
            public const int ReversalFlag = 9;
            public const int TransactionType = 10;
            public const int TransactionID = 11;
            public const int UCITSVCode = 12;
            public const int Identifier = 13;
            public const int TotalCount = 14;
        }

        public static class InvoiceColumns
        {
            public const int FundCustodyCode = 0;
            public const int ExternalFundIdentifier = 1;
            public const int FundName = 2;
            public const int TradeDate = 3;
            public const int SettlementDate = 4;
            public const int FeeType = 5;
            public const int Currency = 6;
            public const int TradeAmount = 7;
            public const int TransactionID = 8;
            public const int ReversalFlag = 9;
            public const int TotalCount = 10;
        }

        public static class ForexHedgeColumns
        {
            public const int LegalEntity = 0;
            public const int COSId = 1;
            public const int EntityCode = 2;
            public const int ShareClass = 3;
            public const int BaseCurrency = 4;
            public const int ClientName = 5;
            public const int BasicContractOrAccount = 6;
            public const int FixedCurrency = 7;
            public const int CounterCurrency = 8;
            public const int Direction = 9;
            public const int IntrumentType = 10;
            public const int TradeDate = 11;
            public const int MaturityDate = 12;
            public const int FixedAmount = 13;
            public const int ClientSpotRate = 14;
            public const int ClientAllInPoints = 15;
            public const int ClientAllInRate = 16;
            public const int ContraAmount = 17;
            public const int Status = 18;
            public const int MandatedExecutionTime = 19;
            public const int ExternalFundIdentifier = 20;
            public const int FeesCurrency = 21;
            public const int FeesInAmount = 22;
            public const int TotalCount = 23;
        }

        public static class TACashColumns
        {
            public const int FundCustodyCode = 0;
            public const int CashFlowTypeId = 1;
            public const int CashFlowTypeDescription = 2;
            public const int AccountCurrency = 3;
            public const int ConfirmedAmountLocalCurrency = 4;
            public const int ConfirmedAmountFundCurrency = 5;
            public const int ProjectedAmountLocalCurrency = 6;
            public const int ProjectedAmountFundCurrency = 7;
            public const int TradeDate = 8;
            public const int CreationDate = 9;
            public const int SettlementDate = 10;
            public const int ReportDate = 11;
            public const int MFCode = 12;
            public const int FundManagerIdentifier = 13;
            public const int FundAdministrationCode = 14;
            public const int FundName = 15;
            public const int ExternalFundIdentifier = 16;
            public const int TotalCount = 17;
        }

        public static class Bond2Columns
        {
            public const int FundCustodyCode = 0;
            public const int ExternalFundIdentifier = 1;
            public const int FundName = 2;
            public const int TradeDate = 3;
            public const int SettlementDate = 4;
            public const int TransactionID = 5;
            public const int FundManagerReference = 6;
            public const int TransactionType = 7;
            public const int TransactionDescription = 8;
            public const int ReversalFlag = 9;
            public const int SecurityName = 10;
            public const int BrokerName = 11;
            public const int ISINCode = 12;
            public const int SecurityDescription = 13;
            public const int Quantity = 14;
            public const int Currency = 15;
            public const int Price = 16;
            public const int GrossAmount = 17;
            public const int BondInterest = 18;
            public const int Brokerage = 19;
            public const int Tax = 20;
            public const int Expenses = 21;
            public const int NetAmount = 22;
            public const int Rate = 23;
            public const int MaturityDate = 24;
            public const int DayCount = 25;
            public const int CuponFrequency = 26;
            public const int BrokerBICCode = 27;
            public const int FundManagerBICCode = 28;
            public const int Type = 29;
            public const int TotalCount = 30;
        }

        public static class Bond3Columns
        {
            public const int FundCustodyCode = 0;
            public const int ExternalFundIdentifier = 1;
            public const int FundName = 2;
            public const int TradeDate = 3;
            public const int SettlementDate = 4;
            public const int TransactionID = 5;
            public const int FundManagerReference = 6;
            public const int TransactionType = 7;
            public const int TransactionDescription = 8;
            public const int ReversalFlag = 9;
            public const int SecurityName = 10;
            public const int BrokerName = 11;
            public const int ISINCode = 12;
            public const int SecurityDescription = 13;
            public const int Quantity = 14;
            public const int Currency = 15;
            public const int Price = 16;
            public const int GrossAmount = 17;
            public const int BondInterest = 18;
            public const int Brokerage = 19;
            public const int Tax = 20;
            public const int Expenses = 21;
            public const int NetAmount = 22;
            public const int Rate = 23;
            public const int MaturityDate = 24;
            public const int DayCount = 25;
            public const int CuponFrequency = 26;
            public const int BrokerBICCode = 27;
            public const int FundManagerBICCode = 28;
            public const int Type = 29;
            public const int Factor = 30;
            public const int TotalCount = 31;
        }
        public static class SwapColumns
        {
            public const int ConsolidatedFundCode = 0;
            public const int FundCode = 1;
            public const int ExternalFundIdentifier = 2;
            public const int FundName = 3;
            public const int TradeDate = 4;
            public const int SettlementDate = 5;
            public const int MaturityDate = 6;
            public const int FusionRef = 7;
            public const int Currency = 8;
            public const int Quantity = 9;
            public const int CurrencyPayableLeg = 10;
            public const int NominalPayableLeg = 11;
            public const int CurrencyReceivableLeg = 12;
            public const int NominalReceivableLeg = 13;
            public const int UpfrontAmountFlag = 14;
            public const int CostPriceLocalCurrency = 15;
            public const int CostPriceFundCurrency = 16;
            public const int GTICode = 17;
            public const int GTILabel = 18;
            public const int RBCContractNumber = 19;
            public const int TransactionStatusNumber = 20;
            public const int BrokerBICCode = 21;
            public const int Premium = 22;
            public const int GrossAmount = 23;
            public const int NetAmount = 24;
            public const int Fees = 25;
            public const int TotalCount = 26;
        }

        public static class CAColumns
        {
            public const int MultifondCode = 0;
            public const int FundCustodyCode = 1;
            public const int FundName = 2;
            public const int TransactionID = 3;
            public const int TransactionType = 4;
            public const int TransactionStatus = 5;
            public const int LSTType = 6;
            public const int LSTName = 7;
            public const int ExDate = 8;
            public const int PaymentDate = 9;
            public const int OldISIN = 10;
            public const int OldName = 11;
            public const int OldGTIName = 12;
            public const int OldCurrency = 13;
            public const int NewISIN = 14;
            public const int NewName = 15;
            public const int NewGTIName = 16;
            public const int NewCurrency = 17;
            public const int WithholdingTaxRate = 18;
            public const int InAdvanceQuantity = 19;
            public const int OutQuantity = 20;
            public const int InQuantity = 21;
            public const int FractionalQuantity = 22;
            public const int OddQuantity = 23;
            public const int GrossAmount = 24;
            public const int WithholdingTaxAmount = 25;
            public const int NetAmount = 26;
            public const int NewRatio = 27;
            public const int OldRatio = 28;
            public const int SettlementType = 29;
            public const int Price = 30;
            public const int ExternalFundIdentifier = 31;
            public const int OldType = 32;
            public const int NewType = 33;
            public const int OldBloombergCode = 34;
            public const int NewBloombergCode = 35;
            public const int Comment = 36;
            public const int CAPSRef = 37;
            public const int TotalCount = 38;
        }

        public static class CollateralColumns
        {
            public const int FundCustodyCode = 0;
            public const int ExternalFundIdentifier = 1;
            public const int PTGName = 2;
            public const int TradeDate = 3;
            public const int SettlementDate = 4;
            public const int MaturityDate = 5;
            public const int TransactionID = 6;
            public const int FundManagerReference = 7;
            public const int TransactionType = 8;
            public const int TransactionDescription = 9;
            public const int ReversalFlag = 10;
            public const int SecurityName = 11;
            public const int CollateralCounterpartyName = 12;
            public const int ISINCode = 13;
            public const int SecurityDescription = 14;
            public const int Quantity = 15;
            public const int Currency = 16;
            public const int Price = 17;
            public const int GrossAmount = 18;
            public const int Interest = 19;
            public const int Brokerage = 20;
            public const int Tax = 21;
            public const int Expenses = 22;
            public const int NetAmount = 23;
            public const int BuyCurrency = 24;
            public const int PurchasedAmount = 25;
            public const int SellCurrency = 26;
            public const int SoldAmount = 27;
            public const int FXRate = 28;
            public const int DepositAmount = 29;
            public const int DepositRate = 30;
            public const int NoOfDays = 31;
            public const int CollateralCounterpartyBICCode = 32;
            public const int FundManagerBICCode = 33;
            public const int BloombergCode = 34;
            public const int TotalCount = 35;
        }

        public static class TermDepositColumns
        {
            public const int FundCustodyCode = 0;
            public const int ExternalFundIdentifier = 1;
            public const int PTGName = 2;
            public const int TradeDate = 3;
            public const int SettlementDate = 4;
            public const int MaturityDate = 5;
            public const int TransactionID = 6;
            public const int FundManagerReference = 7;
            public const int TransactionType = 8;
            public const int TransactionDescription = 9;
            public const int ReversalFlag = 10;
            public const int BrokerName = 11;
            public const int Currency = 12;
            public const int DepositAmount = 13;
            public const int Interest = 14;
            public const int InterestRate = 15;
            public const int NoOfDays = 16;
            public const int BrokerBICCode = 17;
            public const int FundManagerBICCode = 18;
            public const int Type = 19;
            public const int TotalCount = 20;
        }

        public static class AllCustodyTransColumns
        {
            public const int Fund = 0;
            public const int SSAssetID = 1;
            public const int ShareQuantity = 2;
            public const int ActualNetAmount = 3;
            public const int Principal = 4;
            public const int Interest = 5;
            public const int SettleLoc = 6;
            public const int SSStatus = 7;
            public const int MainframeTimeStamp = 8;
            public const int TransactionDescription = 9;
            public const int ActualPaySettleDate = 10;
            public const int ExecutingBroker = 11;
            public const int CurrentFactor = 12;
            public const int PriorFactor = 13;
            public const int Currency = 14;
            public const int TradeRecordDate = 15;
            public const int RelatedTradeID = 16;
            public const int Comments = 17;
            public const int Clientreference = 18;
            public const int SSTradeID = 19;
            public const int SSTransType = 20;
            public const int Isin = 21;
            public const int TotalCount = 22;
        }

        public static class GenericTrade
        {
            public const int TradeType = 0;
            public const int InstrumentRef = 1;
            public const int BookId = 2;
            public const int ExternalRef = 3;
            public const int Quantity = 4;
            public const int Spot = 5;
            public const int SpotType = 6;
            public const int TradeDate = 7;
            public const int ValueDate = 8;
            public const int CounterpartyId = 9;
            public const int DepositaryId = 10;
            public const int BrokerId = 11;
            public const int Entity = 12;
            public const int EventId = 13;
            public const int Currency = 14;
            public const int Comments = 15;
            public const int UserId = 16;
            public const int ExtraFields = 17;
            public const int TotalCount = 18;
        }
    }

    public class RBCCustomParameters
    {
        #region const
        protected const string MEDIO_RMA_CUSTOM_SECTION = "MediolanumRMA";
        private const string _className = "RBCCustomParameters";
        private static object syncObj = new Object();
        #endregion

        #region custom parameter 
        public string DefaultDepositaryStr = "RBC Custody";
        public string DefaultCounterpartyStr = "DELEGATE";
        public string ToolkitDefaultUniversal = "TICKER"; 
        public string ToolkitBondUniversal = "ISIN"; 
        public bool UseStrategySubfolios = false; 
        public bool ValidateGrossAmount = false; 
        public bool ValidateNetAmount = false; 
        public bool ValidateForexAmount = false; 
        public bool UseDefaultDepositary = true; 
        public bool UseDefaultCounterparty = true; 
        public bool OverwriteBORemarks = false; 
        public int DefaultDepositaryId = 0; 
        public int DefaultCounterpartyId = 0; 
        public int DefaultFXHedgeCounterpartyId = 0; 
        public double GrossAmountEpsilon = 0.1; 
        public double NetAmountEpsilon = 0.1; 
        public double ForexAmountEpsilon = 0.1; 
        public string CashTransferInstrumentNameFormat = "Cash for currency '%CURRENCY%'"; 
        public string InvoiceInstrumentNameFormat = "%FEETYPE% %CURRENCY%"; 
        public string TACashInstrumentNameFormat = "Cash for currency '%CURRENCY%'";
        public string TermDepositInstrumentNameFormat = "T %StartDate% %Rate% %EndDate%"; 
        public string CashTransferBusinessEvent = "";
        public string InterestPaymentBusinessEvent = "";
        public string InterestPaymentTypeName = "";
        public string InvoiceBusinessEvent = "";
        public string CollateralInBusinessEvent = "";
        public string CollateralOutBusinessEvent = "";
        public string TACashBusinessEvent = "";
        public string CADefaultBusinessEvent = "";
        public bool CommentUpdaterEnabled = false;
        public int CommentUpdaterDelay = 300; 
        public string CommentUpdaterSource = "RBCUploader"; 
        public int DefaultErrorFolio = 0; 
        public int DefaultErrorInstrumentID = 0;
        public string FundBloombergRequestType = "Equity"; 
        public List<string> MAMLZCodesList; 
        public Dictionary<eCorporateActionType, string> CorporateActionsBusinessEvents; 
        public List<KeyValuePair<string, string>> CurrenciesForSharesPricedInSubunitsList;
        public string RBCTransactionIDName = "TKT_RBC_TRADE_ID";
        public int DefaultSpotType = transaction.SpotTypeConstants.IN_PRICE;
        public int BondSpotType = transaction.SpotTypeConstants.IN_PERCENTAGE;
        public string DefaultBOKernelEvent = "New deal accept"; 
        public string ExtFundIdFilterFile = ""; 
        public List<string> AllowedExtFundIds;
        public IEnumerable<string> BBHFundIds;
        public List<string> UnacceptedTransactionTypeList;
        public string DelegateTradeCreationEvent = ""; 
        public string MAMLTradeCreationEvent = ""; 
        public int DelegateTradeCreationEventId = 0;
        public int MAMLTradeCreationEventId = 0;
        public bool OverrideCreationEvent = false;
        public List<string> InterestPaymentTypeNameList = new List<string>() { "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8" };
        public List<string> CorporateActionReversalCodeList = new List<string>();
        public string RBCOrderTolerance = "0.003";
        public string RBCComment = "TKT_RBC_COMMENT";
        public string RBCCapsRef = "TKT_RBC_CAPS_REF";
        public string RBCUcitsCode = "TKT_RBC_UCITSVCODE";
        public string RBCTransType = "TKT_RBC_TRANSTYPE";
        public string SSBAllCustCollateralCashRelatedTradeIDs = string.Empty;
        public string SSBAllCustMarginRelatedTradeIDs = string.Empty;

        string grossAmountEpsilonStr = "0.1";
        string netAmountEpsilonStr = "0.1";
        string forexAmountEpsilonStr = "0.01";
        string MAMLZCodes = "";
        string DefaultFXHedgeCounterpartyIdStr = "FX Hedge Counterparty";
        // CA
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
        string CurrenciesForSharesPricedInSubunits = "";
        string CorporateActionReversalCodes = "";

        public bool PriceInUnderlyingCCY = false;
        public bool ABSBondNotionalOne = false;

        #endregion

        #region Singleton
        private static RBCCustomParameters gInstance;
        public static RBCCustomParameters Instance
        {
            get
            {
                if (gInstance == null)
                {
                    lock (syncObj) // thread safe
                    {
                        gInstance = new RBCCustomParameters();
                    }
                }
                return gInstance;
            }
        }

        private RBCCustomParameters()
        {
            using (Logger logger = new Logger(_className, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "UseStrategyFolios", ref UseStrategySubfolios, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ValidateGrossAmount", ref ValidateGrossAmount, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ValidateNetAmount", ref ValidateNetAmount, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "GrossAmountEpsilon", ref grossAmountEpsilonStr, "0.1");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "NetAmountEpsilon", ref netAmountEpsilonStr, "0.1");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ValidateForexAmount", ref ValidateForexAmount, true);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ForexAmountEpsilon", ref forexAmountEpsilonStr, "0.01");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "UseDefaultCounterparty", ref UseDefaultCounterparty, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "UseDefaultDepositary", ref UseDefaultDepositary, true);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ReplaceBORemarks", ref OverwriteBORemarks, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "DefaultDepositary", ref DefaultDepositaryStr, "RBC Custody");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "DefaultCounterparty", ref DefaultCounterpartyStr, "DELEGATE");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "DefaultHedgeCounterparty", ref DefaultFXHedgeCounterpartyIdStr, "FX Hedge Counterparty");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "MAMLZCodes", ref MAMLZCodes, "Z8719429;Z8730529;Z8730528;Z8894216;Z8730525;Z5950209;Z8894216;Z8730525;Z5950209;Z5950206;Z5950206");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ToolkitDefaultUniversal", ref ToolkitDefaultUniversal, "TICKER");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ToolkitBondUniversal", ref ToolkitBondUniversal, "ISIN");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "DefaultErrorFolio", ref DefaultErrorFolio, 0);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CashTransferInstrumentNameFormat", ref CashTransferInstrumentNameFormat, "Cash for currency '%CURRENCY%'");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "InvoiceInstrumentNameFormat", ref InvoiceInstrumentNameFormat, "%FEETYPE% %CURRENCY%");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "TACashInstrumentNameFormat", ref TACashInstrumentNameFormat, "S/R for fund '%FUND%'");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "TermDepositInstrumentNameFormat", ref TermDepositInstrumentNameFormat, TermDepositInstrumentNameFormat);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CashTransferBusinessEvent", ref CashTransferBusinessEvent, "Cash Transfer");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "InterestPaymentBusinessEvent", ref InterestPaymentBusinessEvent, "Interest Payment");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "InterestPaymentTypeName", ref InterestPaymentTypeName, "Interest Payment");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "InvoiceBusinessEvent", ref InvoiceBusinessEvent, "Fee");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CollateralInBusinessEvent", ref CollateralInBusinessEvent, "Collateral In");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CollateralOutBusinessEvent", ref CollateralOutBusinessEvent, "Collateral Out");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "TACashBusinessEvent", ref TACashBusinessEvent, "Subscription/Redemption");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CADefaultBusinessEvent", ref CADefaultBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "SpinOffBusinessEvent", ref SpinOffBusinessEvent, "SpinOff");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "MergerBusinessEvent", ref MergerBusinessEvent, "Merger");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "AmalgamationBusinessEvent", ref AmalgamationBusinessEvent, "Merger");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "BonusOrScripIssueBusinessEvent", ref BonusOrScripIssueBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "SubscriptionBusinessEvent", ref SubscriptionBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "RightsEntitlementBusinessEvent", ref RightsEntitlementBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "SecuritiesWithdrawalBusinessEvent", ref SecuritiesWithdrawalBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ReverseSplitBusinessEvent", ref ReverseSplitBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "SecuritiesDepositBusinessEvent", ref SecuritiesDepositBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ConversionBusinessEvent", ref ConversionBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CashCreditBusinessEvent", ref CashCreditBusinessEvent, "CA Adjustment (Cash)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CapitalReductionWithPaymentBusinessEvent", ref CapitalReductionWithPaymentBusinessEvent, "CA Adjustment (Cash)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CapitalIncreaseVsPaymentBusinessEvent", ref CapitalIncreaseVsPaymentBusinessEvent, "CA Adjustment (Cash)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "DistributionInSecuritiesBusinessEvent", ref DistributionInSecuritiesBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "PublicPurchaseOfferBusinessEvent", ref PublicPurchaseOfferBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ExchangeOrExchangeOfferBusinessEvent", ref ExchangeOrExchangeOfferBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "AdditionalSplitBusinessEvent", ref AdditionalSplitBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "SplitBusinessEvent", ref SplitBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CashDebitBusinessEvent", ref CashDebitBusinessEvent, "CA Adjustment (Cash)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "DividendBusinessEvent", ref DividendBusinessEvent, "Dividend");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "InterestBusinessEvent", ref InterestBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "FinalRedemptionBusinessEvent", ref FinalRedemptionBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "EarlyRedemptionBusinessEvent", ref EarlyRedemptionBusinessEvent, "Early Redemption");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "WarrantExerciseBusinessEvent", ref WarrantExerciseBusinessEvent, "CA Adjustment (Securities)");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "FundBloombergRequestType", ref FundBloombergRequestType, "Equity");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "SharesPricedInSubunits", ref CurrenciesForSharesPricedInSubunits, "");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "UnacceptedTransactionTypes", ref UnacceptedTransactions, "");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "DefaultBOKernelEvent", ref DefaultBOKernelEvent, "New deal accept");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "MAMLTradeCreationEvent", ref MAMLTradeCreationEvent, "");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "DelegateTradeCreationEvent", ref DelegateTradeCreationEvent, "");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "OverrideCreationEvent", ref OverrideCreationEvent, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ExtFundIdFilterFile", ref ExtFundIdFilterFile, "");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CommentUpdaterEnabled", ref CommentUpdaterEnabled, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CommentUpdaterDelay", ref CommentUpdaterDelay, 300);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CommentUpdaterSource", ref CommentUpdaterSource, "RBCUploader");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "CorporateActionReversalCodes", ref CorporateActionReversalCodes, "");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "RBCOrderTolerance", ref RBCOrderTolerance, RBCOrderTolerance);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "PriceInUnderlyingCCY", ref PriceInUnderlyingCCY, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "ABSBondNotionalOne", ref ABSBondNotionalOne, false);
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "SSBAllCustCollateralCashRelatedTradeIDs", ref SSBAllCustCollateralCashRelatedTradeIDs, "CCPM,CCPC,SWCC,SWBC,TBCC");
                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "SSBAllCustMarginRelatedTradeIDs", ref SSBAllCustMarginRelatedTradeIDs, "MARG,MGCC");

                Double.TryParse(grossAmountEpsilonStr, out GrossAmountEpsilon);
                Double.TryParse(netAmountEpsilonStr, out NetAmountEpsilon);
                Double.TryParse(forexAmountEpsilonStr, out ForexAmountEpsilon);
               
                MAMLZCodesList = MAMLZCodes.SplitCSV();
                DefaultCounterpartyId = CSxRBCHelper.GetEntityIDByName(DefaultCounterpartyStr);
                DefaultDepositaryId = CSxRBCHelper.GetEntityIDByName(DefaultDepositaryStr);
                DefaultFXHedgeCounterpartyId = CSxRBCHelper.GetEntityIDByName(DefaultFXHedgeCounterpartyIdStr);
                CorporateActionsBusinessEvents = new Dictionary<eCorporateActionType, string>()
                {
                    {eCorporateActionType.Subscription,SubscriptionBusinessEvent},
                    {eCorporateActionType.RightsEntitlement,RightsEntitlementBusinessEvent},
                    {eCorporateActionType.SecuritiesWithdrawal,SecuritiesWithdrawalBusinessEvent},
                    {eCorporateActionType.ReverseSplit,ReverseSplitBusinessEvent},
                    {eCorporateActionType.SpinOff, SpinOffBusinessEvent},
                    {eCorporateActionType.SecuritiesDeposit,SecuritiesDepositBusinessEvent},
                    {eCorporateActionType.Conversion,ConversionBusinessEvent},
                    {eCorporateActionType.CashCredit,CashCreditBusinessEvent},
                    {eCorporateActionType.CapitalReductionWithPayment,CapitalReductionWithPaymentBusinessEvent},
                    {eCorporateActionType.CapitalIncreaseVsPayment,CapitalIncreaseVsPaymentBusinessEvent},
                    {eCorporateActionType.DistributionInSecurities,DistributionInSecuritiesBusinessEvent},
                    {eCorporateActionType.BonusOrScripIssue, BonusOrScripIssueBusinessEvent},
                    {eCorporateActionType.PublicPurchaseOffer,PublicPurchaseOfferBusinessEvent},
                    {eCorporateActionType.ExchangeOrExchangeOffer,ExchangeOrExchangeOfferBusinessEvent},
                    {eCorporateActionType.Merger, MergerBusinessEvent},
                    {eCorporateActionType.AdditionalSplit,AdditionalSplitBusinessEvent},
                    {eCorporateActionType.Split,SplitBusinessEvent},
                    {eCorporateActionType.CashDebit,CashDebitBusinessEvent},
                    {eCorporateActionType.Dividend,DividendBusinessEvent},
                    {eCorporateActionType.Interest,InterestBusinessEvent},
                    {eCorporateActionType.FinalRedemption,FinalRedemptionBusinessEvent},
                    {eCorporateActionType.EarlyRedemption,EarlyRedemptionBusinessEvent},
                    {eCorporateActionType.Amalgamation, AmalgamationBusinessEvent},
                    {eCorporateActionType.WarrantExercise,WarrantExerciseBusinessEvent},
                };
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
                UnacceptedTransactionTypeList = UnacceptedTransactions.SplitCSV().ConvertAll(s => s.ToUpper()); ; // split ';' delimited strings containing currencies and remove spaces
                MAMLTradeCreationEventId = CSxRBCHelper.GetKernelWorkflowEventIDByName(MAMLTradeCreationEvent);
                DelegateTradeCreationEventId = CSxRBCHelper.GetKernelWorkflowEventIDByName(DelegateTradeCreationEvent);
                AllowedExtFundIds = CSxRBCHelper.ReadFile(ExtFundIdFilterFile);
                BBHFundIds = CSxDBHelper.GetMultiRecords("SELECT FUNDID FROM MEDIO_BBH_FUNDFILTER").OfType<string>();
                logger.log(Severity.info, $"BBHFundIds.Count={BBHFundIds.Count()}");
                CorporateActionReversalCodeList = CorporateActionReversalCodes.SplitCSV();
            }
            catch (Exception ex)
            {
                logger.log(Severity.error, "Failed to initialize RBCCustomParameters : " + ex.Message + ". InnerException: " + ex.InnerException + ". Stack trace: " + ex.StackTrace);
            }
            }
        }

        public void LogCustomParameters()
        {
            using (Logger logger = new Logger(_className, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.info,GetDefaultParametersString());
            }
        }

        private string GetDefaultParametersString()
        {
            StringBuilder strb = new StringBuilder("Loaded config values: ToolkitDefaultUniversal=");
            strb.Append(ToolkitDefaultUniversal);
            strb.Append(" ToolkitBondUniversal=");
            strb.Append(ToolkitBondUniversal);
            strb.Append(" UseStrategySubfolios=");
            strb.Append(UseStrategySubfolios);
            strb.Append(" ValidateGrossAmount=");
            strb.Append(ValidateGrossAmount);
            strb.Append(" ValidateNetAmount=");
            strb.Append(ValidateNetAmount);
            strb.Append(" ValidateForexAmount=");
            strb.Append(ValidateForexAmount);
            strb.Append(" ForexAmountEpsilon=");
            strb.Append(ForexAmountEpsilon);
            strb.Append(" UseDefaultDepositary=");
            strb.Append(UseDefaultDepositary);
            strb.Append(" UseDefaultCounterparty=");
            strb.Append(UseDefaultCounterparty);
            strb.Append(" OverwriteBORemarks=");
            strb.Append(OverwriteBORemarks);
            strb.Append(" DefaultDepositaryId=");
            strb.Append(DefaultDepositaryId);
            strb.Append(" DefaultCounterpartyId=");
            strb.Append(DefaultCounterpartyId);
            strb.Append(" DefaultFXHedgeCounterpartyId=");
            strb.Append(DefaultFXHedgeCounterpartyId);
            strb.Append(" GrossAmountEpsilon=");
            strb.Append(GrossAmountEpsilon);
            strb.Append(" NetAmountEpsilon=");
            strb.Append(NetAmountEpsilon);
            strb.Append(" CashTransferInstrumentNameFormat=");
            strb.Append(CashTransferInstrumentNameFormat);
            strb.Append(" InvoiceInstrumentNameFormat=");
            strb.Append(InvoiceInstrumentNameFormat);
            strb.Append(" TACashInstrumentNameFormat=");
            strb.Append(TACashInstrumentNameFormat);
            strb.Append(" CashTransferBusinessEvent=");
            strb.Append(CashTransferBusinessEvent);
            strb.Append(" InterestPaymentBusinessEvent=");
            strb.Append(InterestPaymentBusinessEvent);
            strb.Append(" InterestPaymentTypeName=");
            strb.Append(InterestPaymentTypeName);
            strb.Append(" InvoiceBusinessEvent=");
            strb.Append(InvoiceBusinessEvent);
            strb.Append(" CollateralInBusinessEvent=");
            strb.Append(CollateralInBusinessEvent);
            strb.Append(" CollateralOutBusinessEvent=");
            strb.Append(CollateralOutBusinessEvent);
            strb.Append(" TACashBusinessEvent=");
            strb.Append(TACashBusinessEvent);
            strb.Append(" CADefaultBusinessEvent=");
            strb.Append(CADefaultBusinessEvent);
            strb.Append(" CommentUpdaterEnabled=");
            strb.Append(CommentUpdaterEnabled);
            strb.Append(" CommentUpdaterDelay=");
            strb.Append(CommentUpdaterDelay);
            strb.Append(" CommentUpdaterSource=");
            strb.Append(CommentUpdaterSource);
            strb.Append(" DefaultErrorFolio=");
            strb.Append(DefaultErrorFolio);
            strb.Append(" DefaultErrorInstrumentID=");
            strb.Append(DefaultErrorInstrumentID);
            strb.Append(" FundBloombergRequestType=");
            strb.Append(FundBloombergRequestType);
            strb.Append(" grossAmountEpsilonStr=");
            strb.Append(GrossAmountEpsilon);
            strb.Append(" netAmountEpsilonStr=");
            strb.Append(NetAmountEpsilon);
            strb.Append(" forexAmountEpsilonStr=");
            strb.Append(ForexAmountEpsilon);
            strb.Append(" DefaultDepositaryStr=");
            strb.Append(DefaultDepositaryStr);
            strb.Append(" DefaultCounterpartyStr=");
            strb.Append(DefaultCounterpartyStr);
            strb.Append(" DefaultFXHedgeCounterpartyIdStr=");
            strb.Append(DefaultFXHedgeCounterpartyId);
            strb.Append(" SpinOffBusinessEvent=");
            strb.Append(SpinOffBusinessEvent);
            strb.Append(" MergerBusinessEvent=");
            strb.Append(MergerBusinessEvent);
            strb.Append(" AmalgamationBusinessEvent=");
            strb.Append(AmalgamationBusinessEvent);
            strb.Append(" BonusOrScripIssueBusinessEvent=");
            strb.Append(BonusOrScripIssueBusinessEvent);
            strb.Append(" SubscriptionBusinessEvent=");
            strb.Append(SubscriptionBusinessEvent);
            strb.Append(" RightsEntitlementBusinessEvent=");
            strb.Append(RightsEntitlementBusinessEvent);
            strb.Append(" SecuritiesWithdrawalBusinessEvent=");
            strb.Append(SecuritiesWithdrawalBusinessEvent);
            strb.Append(" ReverseSplitBusinessEvent=");
            strb.Append(ReverseSplitBusinessEvent);
            strb.Append(" SecuritiesDepositBusinessEvent=");
            strb.Append(SecuritiesDepositBusinessEvent);
            strb.Append(" ConversionBusinessEvent=");
            strb.Append(ConversionBusinessEvent);
            strb.Append(" CashCreditBusinessEvent=");
            strb.Append(CashCreditBusinessEvent);
            strb.Append(" CapitalReductionWithPaymentBusinessEvent=");
            strb.Append(CapitalReductionWithPaymentBusinessEvent);
            strb.Append(" CapitalIncreaseVsPaymentBusinessEvent=");
            strb.Append(CapitalIncreaseVsPaymentBusinessEvent);
            strb.Append(" DistributionInSecuritiesBusinessEvent=");
            strb.Append(DistributionInSecuritiesBusinessEvent);
            strb.Append(" PublicPurchaseOfferBusinessEvent=");
            strb.Append(PublicPurchaseOfferBusinessEvent);
            strb.Append(" ExchangeOrExchangeOfferBusinessEvent=");
            strb.Append(ExchangeOrExchangeOfferBusinessEvent);
            strb.Append(" AdditionalSplitBusinessEvent=");
            strb.Append(AdditionalSplitBusinessEvent);
            strb.Append(" SplitBusinessEvent=");
            strb.Append(SplitBusinessEvent);
            strb.Append(" CashDebitBusinessEvent=");
            strb.Append(CashDebitBusinessEvent);
            strb.Append(" DividendBusinessEvent=");
            strb.Append(DividendBusinessEvent);
            strb.Append(" InterestBusinessEvent=");
            strb.Append(InterestBusinessEvent);
            strb.Append(" FinalRedemptionBusinessEvent=");
            strb.Append(FinalRedemptionBusinessEvent);
            strb.Append(" EarlyRedemptionBusinessEvent=");
            strb.Append(EarlyRedemptionBusinessEvent);
            strb.Append(" WarrantExerciseBusinessEvent=");
            strb.Append(WarrantExerciseBusinessEvent);
            strb.Append(" SharesPricedInSubunits=");
            strb.Append(CurrenciesForSharesPricedInSubunits);
            strb.Append(" UnacceptedTransactions=");
            strb.Append(UnacceptedTransactions);
            strb.Append(" DefaultBOKernelEvent=");
            strb.Append(DefaultBOKernelEvent);
            strb.Append(" MAMLTradeCreationEvent=");
            strb.Append(MAMLTradeCreationEvent);
            strb.Append(" MAMLTradeCreationEventID=");
            strb.Append(MAMLTradeCreationEventId);
            strb.Append(" DelegateTradeCreationEvent=");
            strb.Append(DelegateTradeCreationEvent);
            strb.Append(" DelegateTradeCreationEventID=");
            strb.Append(DelegateTradeCreationEventId);
            strb.Append(" OverrideCreationEvent=");
            strb.Append(OverrideCreationEvent);
            strb.Append(" ExtFundIdFilterFile=");
            strb.Append(ExtFundIdFilterFile);
            strb.Append(" CorporateActionReversalCodes=");
            strb.Append(CorporateActionReversalCodes);

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
    }

}



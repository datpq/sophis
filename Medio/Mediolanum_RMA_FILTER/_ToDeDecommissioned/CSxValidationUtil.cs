using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using sophis.utils;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using RichMarketAdapter.ticket;
using transaction;

namespace Mediolanum_RMA_FILTER
{
    enum CSVDocumentClass
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
        Swap,
        CorporateAction,
        Collateral,
        Unknown
    };

    enum InboundCSVFields
    {
        //Bond and equity fields
        FundCustodyCode,
        ExternalFundIdentifier,
        FundName,
        TradeDate,
        SettlementDate,
        TransactionID,
        FundManagerReference,
        TransactionType,
        TransactionDescription,
        ReversalFlag,
        SecurityName,
        BrokerName,
        ISINCode,
        SecurityDescription,
        Quantity,
        Currency,
        Price,
        GrossAmount,
        Brokerage,
        Tax,
        Expenses,
        NetAmount,
        BrokerBICCode,
        FundManagerBICCode,
        BloombergCode,
        // Collateral specific 
        PTGName,
        CollateralCounterpartyName,
        Interest,
        DepositAmount,
        DepositRate,
        NoOfDays,
        CollateralCounterpartyBICCode,
        //Forex specific fields
        BuyCurrency,
        PurchasedAmount,
        SellCurrency,
        SoldAmount,
        FXRate,
        NDFFlag,
        //Options specific
        ConsolidatedFundCode,
        FundCode,
        //FundCode == fund custody code ?
        FundCurrency,
        ManagerCode,
        CounterpartyOrBrokerCode,
        CounterpartyOrBrokerDescription,
        GTICode,
        GTIDescription,
        OptionType,
        OptionDescription,
        OptionRBCDISCode,
        OptionCurrency,
        OptionStrike,
        ContractSize,
        ContractNumber,
        NAVDate,
        Premium,
        TradeAmount,
        CommissionAmount,
        FeesAmountInTransactionCurrency,
        TransactionStatusCode,
        TransactionCode,
        ContractStatus,
        BrokerCode,
        Style,
        FusionRef,
        UnderlyingAsset,
        BICCode,
        BuyOrSell,
        //Futures specific
        FutureRBCDISCode,
        UnderlyingISIN,
        FutureDescription,
        FutureMaturityDate,
        FutureCurrency,
        FutureTradeCode,
        //Cash specific
        IBAN,
        AccountType,
        //Invoice specific
        FeeType,
        //Forex Hedge specific
        LegalEntity,
        COSId,
        EntityCode,
        ShareClass,
        BaseCurrency,
        ClientName,
        BasicContractOrAccount,
        FixedCurrency,
        CounterCurrency,
        Direction,
        IntrumentType,
        FixedAmount,
        ClientSpotRate,
        ClientAllInPoints,
        ClientAllInRate,
        ContraAmount,
        Status,
        MandatedExecutionTime,
        FeesCurrency,
        MaturityDate,
        FeesInAmount,
        //TACAsh specific
        CashFlowTypeId,
        CashFlowTypeDescription,
        AccountCurrency,
        ConfirmedAmountLocalCurrency,
        ConfirmedAmountFundCurrency,
        ProjectedAmountLocalCurrency,
        ProjectedAmountFundCurrency,
        CreationDate,
        ReportDate,
        MFCode,
        FundManagerIdentifier,
        FundAdministrationCode,
        //Bond specific
        BondInterest,
        DayCount,
        CuponFrequency,
        Rate,
        //Swap specific
        CurrencyPayableLeg,
        NominalPayableLeg,
        CurrencyReceivableLeg,
        NominalReceivableLeg,
        UpfrontAmountFlag,
        CostPriceLocalCurrency,
        CostPriceFundCurrency,
        GTILabel,
        RBCContractNumber,
        TransactionStatusNumber,
        Fees,
        //CA Specific
        MultifondCode,
        TransactionStatus,
        LSTType,
        LSTName,
        ExDate,
        PaymentDate,
        OldISIN,
        OldName,
        OldGTIName,
        OldCurrency,
        NewISIN,
        NewName,
        NewGTIName,
        NewCurrency,
        WithholdingTaxRate,
        InAdvanceQuantity,
        OutQuantity,
        InQuantity,
        FractionalQuantity,
        OddQuantity,
        WithholdingTaxAmount,
        NewRatio,
        OldRatio,
        SettlementType,
        OldType,
        NewType,
        OldBloombergCode,
        NewBloombergCode
    };

    enum CorporateActionType
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

    static class CSxValidationUtil
    {
        public static Dictionary<InboundCSVFields, int> EquityAndBondMappings;
        public static Dictionary<InboundCSVFields, int> ForexMappings;
        public static Dictionary<InboundCSVFields, int> OptionMappings;
        public static Dictionary<InboundCSVFields, int> FutureMappings;
        public static Dictionary<InboundCSVFields, int> CashMappings;
        public static Dictionary<InboundCSVFields, int> InvoiceMappings;
        public static Dictionary<InboundCSVFields, int> ForexHedgeMappings;
        public static Dictionary<InboundCSVFields, int> TACashMappings;
        public static Dictionary<InboundCSVFields, int> Bond2Mappings;
        public static Dictionary<InboundCSVFields, int> SwapMappings;
        public static Dictionary<InboundCSVFields, int> CAMappings;
        public static Dictionary<InboundCSVFields, int> CollateralMappings;
        public static HashSet<string> Currencies;
        public const string DefaultDateFormat = "dMMyyyy";
        public const string DateFormat2 = "d/M/yyyy";
        public const string DateFormat3 = "d-MMM-yy";
        public const string DateFormat4 = "M/d/yyyy";
        public static CultureInfo DefaultCultureInfo = null;
        public static bool ApiInit = false;

        public static string[] CommonDateFormats = { DefaultDateFormat, DateFormat2 }; //Bonds, equities, loans, funds
        public static string[] OptionsDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] FuturesDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] SwapsDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] ForexDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] CashDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] CADateFormats = { DefaultDateFormat, DateFormat2, DateFormat3 };

        static CSxValidationUtil()
        {
            EquityAndBondMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.ConsolidatedFundCode,0},
                {InboundCSVFields.ExternalFundIdentifier,1},
                {InboundCSVFields.FundName,2},
                {InboundCSVFields.TradeDate,3},
                {InboundCSVFields.SettlementDate,4},
                {InboundCSVFields.TransactionID,5},
                {InboundCSVFields.FundManagerReference,6},
                {InboundCSVFields.TransactionType,7},
                {InboundCSVFields.TransactionDescription,8},
                {InboundCSVFields.ReversalFlag,9},
                {InboundCSVFields.SecurityName,10},
                {InboundCSVFields.BrokerName,11},
                {InboundCSVFields.ISINCode,12},
                {InboundCSVFields.SecurityDescription,13},
                {InboundCSVFields.Quantity,14},
                {InboundCSVFields.Currency,15},
                {InboundCSVFields.Price,16},
                {InboundCSVFields.GrossAmount,17},
                {InboundCSVFields.Brokerage,18},
                {InboundCSVFields.Tax,19},
                {InboundCSVFields.Expenses,20},
                {InboundCSVFields.NetAmount,21},
                {InboundCSVFields.BrokerBICCode,22},
                {InboundCSVFields.FundManagerBICCode,23},
                {InboundCSVFields.BloombergCode,24}
            };
            ForexMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.FundCustodyCode,0},
                {InboundCSVFields.ExternalFundIdentifier,1},
                {InboundCSVFields.FundName,2},
                {InboundCSVFields.TradeDate,3},
                {InboundCSVFields.SettlementDate,4},
                {InboundCSVFields.TransactionID,5},
                {InboundCSVFields.ReversalFlag,6},
                {InboundCSVFields.BuyCurrency,7},
                {InboundCSVFields.PurchasedAmount,8},
                {InboundCSVFields.SellCurrency,9},
                {InboundCSVFields.SoldAmount,10},
                {InboundCSVFields.FXRate,11},
                {InboundCSVFields.BrokerBICCode,12},
                {InboundCSVFields.BrokerName,13},
                {InboundCSVFields.FundManagerBICCode,14},
                {InboundCSVFields.NDFFlag,15}
            };
            OptionMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.ConsolidatedFundCode,0},
                {InboundCSVFields.FundCode,1},
                {InboundCSVFields.ExternalFundIdentifier,2},
                {InboundCSVFields.FundName,3},
                {InboundCSVFields.FundCurrency,4},
                {InboundCSVFields.ManagerCode,5},
                {InboundCSVFields.CounterpartyOrBrokerCode,6},
                {InboundCSVFields.CounterpartyOrBrokerDescription,7},
                {InboundCSVFields.GTICode,8},
                {InboundCSVFields.GTIDescription,9},
                {InboundCSVFields.OptionType,10},
                {InboundCSVFields.OptionDescription,11},
                {InboundCSVFields.OptionRBCDISCode,12},
                {InboundCSVFields.OptionCurrency,13},
                {InboundCSVFields.OptionStrike,14},
                {InboundCSVFields.ContractSize,15},
                {InboundCSVFields.ContractNumber,16},
                {InboundCSVFields.TradeDate,17},
                {InboundCSVFields.NAVDate,18},
                {InboundCSVFields.SettlementDate,19},
                {InboundCSVFields.Quantity,20},
                {InboundCSVFields.Premium,21},
                {InboundCSVFields.TradeAmount,22},
                {InboundCSVFields.CommissionAmount,23},
                {InboundCSVFields.FeesAmountInTransactionCurrency,24},
                {InboundCSVFields.TransactionStatusCode,25},
                {InboundCSVFields.TransactionCode,26},
                {InboundCSVFields.ContractStatus,27},
                {InboundCSVFields.BrokerCode,28},
                {InboundCSVFields.BrokerName,29},
                {InboundCSVFields.Style,30},
                {InboundCSVFields.BloombergCode,31},
                {InboundCSVFields.FusionRef,32},
                {InboundCSVFields.UnderlyingAsset,33},
                {InboundCSVFields.BICCode,34},
                {InboundCSVFields.BuyOrSell,35},
                {InboundCSVFields.FundManagerBICCode,36}
            };
            FutureMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.ConsolidatedFundCode,0},
                {InboundCSVFields.FundCode,1},
                {InboundCSVFields.ExternalFundIdentifier,2},
                {InboundCSVFields.FundName,3},
                {InboundCSVFields.FundCurrency,4},
                {InboundCSVFields.ManagerCode,5},
                {InboundCSVFields.CounterpartyOrBrokerCode,6},
                {InboundCSVFields.CounterpartyOrBrokerDescription,7},
                {InboundCSVFields.GTICode,8},
                {InboundCSVFields.GTIDescription,9},
                {InboundCSVFields.FutureRBCDISCode,10},
                {InboundCSVFields.UnderlyingISIN,11},
                {InboundCSVFields.FutureDescription,12},
                {InboundCSVFields.FutureMaturityDate,13},
                {InboundCSVFields.FutureCurrency,14},
                {InboundCSVFields.ContractNumber,15},
                {InboundCSVFields.TradeDate,16},
                {InboundCSVFields.NAVDate,17},
                {InboundCSVFields.SettlementDate,18},
                {InboundCSVFields.TransactionDescription,19},
                {InboundCSVFields.Quantity,20},
                {InboundCSVFields.Price,21},
                {InboundCSVFields.ContractSize,22},
                {InboundCSVFields.TradeAmount,23},
                {InboundCSVFields.CommissionAmount,24},
                {InboundCSVFields.FeesAmountInTransactionCurrency,25},
                {InboundCSVFields.TransactionCode,26},
                {InboundCSVFields.TransactionStatusCode,27},
                {InboundCSVFields.ContractStatus,28},
                {InboundCSVFields.FutureTradeCode,29},
                {InboundCSVFields.BloombergCode,30},
                {InboundCSVFields.UnderlyingAsset,31},
                {InboundCSVFields.BICCode,32},
                {InboundCSVFields.BuyOrSell,33}
            };
            CashMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.FundCustodyCode,0},
                {InboundCSVFields.ExternalFundIdentifier,1},
                {InboundCSVFields.FundName,2},
                {InboundCSVFields.TradeDate,3},
                {InboundCSVFields.SettlementDate,4},
                {InboundCSVFields.IBAN,5},
                {InboundCSVFields.AccountType,6},
                {InboundCSVFields.Currency,7},
                {InboundCSVFields.TradeAmount,8},
                {InboundCSVFields.ReversalFlag,9},
                {InboundCSVFields.TransactionType,10},
                {InboundCSVFields.TransactionID,11}
            };
            InvoiceMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.FundCustodyCode,0},
                {InboundCSVFields.ExternalFundIdentifier,1},
                {InboundCSVFields.FundName,2},
                {InboundCSVFields.TradeDate,3},
                {InboundCSVFields.SettlementDate,4},
                {InboundCSVFields.FeeType,5},
                {InboundCSVFields.Currency,6},
                {InboundCSVFields.TradeAmount,7},
                {InboundCSVFields.TransactionID,8},
                {InboundCSVFields.ReversalFlag,9}
            };
            ForexHedgeMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.LegalEntity,0},
                {InboundCSVFields.COSId,1},
                {InboundCSVFields.EntityCode,2},
                {InboundCSVFields.ShareClass,3},
                {InboundCSVFields.BaseCurrency,4},
                {InboundCSVFields.ClientName,5},
                {InboundCSVFields.BasicContractOrAccount,6},
                {InboundCSVFields.FixedCurrency,7},
                {InboundCSVFields.CounterCurrency,8},
                {InboundCSVFields.Direction,9},
                {InboundCSVFields.IntrumentType,10},
                {InboundCSVFields.TradeDate,11},
                {InboundCSVFields.MaturityDate,12},
                {InboundCSVFields.FixedAmount,13},
                {InboundCSVFields.ClientSpotRate,14},
                {InboundCSVFields.ClientAllInPoints,15},
                {InboundCSVFields.ClientAllInRate,16},
                {InboundCSVFields.ContraAmount,17},
                {InboundCSVFields.Status,18},
                {InboundCSVFields.MandatedExecutionTime,19},
                {InboundCSVFields.ExternalFundIdentifier,20},
                {InboundCSVFields.FeesCurrency,21},
                {InboundCSVFields.FeesInAmount,22}
            };
            TACashMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.FundCustodyCode,0}, //SARA code
                {InboundCSVFields.CashFlowTypeId,1},
                {InboundCSVFields.CashFlowTypeDescription,2},
                {InboundCSVFields.AccountCurrency,3},
                {InboundCSVFields.ConfirmedAmountLocalCurrency,4},
                {InboundCSVFields.ConfirmedAmountFundCurrency,5},
                {InboundCSVFields.ProjectedAmountLocalCurrency,6},
                {InboundCSVFields.ProjectedAmountFundCurrency,7},
                {InboundCSVFields.TradeDate,8},
                {InboundCSVFields.CreationDate,9},
                {InboundCSVFields.SettlementDate,10},
                {InboundCSVFields.ReportDate,11},
                {InboundCSVFields.MFCode,12},
                {InboundCSVFields.FundManagerIdentifier,13},
                {InboundCSVFields.FundAdministrationCode,14},
                {InboundCSVFields.FundName,15},
                {InboundCSVFields.ExternalFundIdentifier,16}
            };
            Bond2Mappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.FundCustodyCode,0},
                {InboundCSVFields.ExternalFundIdentifier,1},
                {InboundCSVFields.FundName,2},
                {InboundCSVFields.TradeDate,3},
                {InboundCSVFields.SettlementDate,4},
                {InboundCSVFields.TransactionID,5},
                {InboundCSVFields.FundManagerReference,6},
                {InboundCSVFields.TransactionType,7},
                {InboundCSVFields.TransactionDescription,8},
                {InboundCSVFields.ReversalFlag,9},
                {InboundCSVFields.SecurityName,10},
                {InboundCSVFields.BrokerName,11},
                {InboundCSVFields.ISINCode,12},
                {InboundCSVFields.SecurityDescription,13},
                {InboundCSVFields.Quantity,14},
                {InboundCSVFields.Currency,15},
                {InboundCSVFields.Price,16},
                {InboundCSVFields.GrossAmount,17},
                {InboundCSVFields.BondInterest,18},
                {InboundCSVFields.Brokerage,19},
                {InboundCSVFields.Tax,20},
                {InboundCSVFields.Expenses,21},
                {InboundCSVFields.NetAmount,22},
                {InboundCSVFields.Rate,23},
                {InboundCSVFields.MaturityDate,24},
                {InboundCSVFields.DayCount,25},
                {InboundCSVFields.CuponFrequency,26},
                {InboundCSVFields.BrokerBICCode,27},
                {InboundCSVFields.FundManagerBICCode,28}
            };
            SwapMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.ConsolidatedFundCode,0},
                {InboundCSVFields.FundCode,1},
                {InboundCSVFields.ExternalFundIdentifier,2},
                {InboundCSVFields.FundName,3},
                {InboundCSVFields.TradeDate,4},
                {InboundCSVFields.SettlementDate,5},
                {InboundCSVFields.MaturityDate,6},
                {InboundCSVFields.FusionRef,7},
                {InboundCSVFields.Currency,8},
                {InboundCSVFields.Quantity,9},
                {InboundCSVFields.CurrencyPayableLeg,10},
                {InboundCSVFields.NominalPayableLeg,11},
                {InboundCSVFields.CurrencyReceivableLeg,12},
                {InboundCSVFields.NominalReceivableLeg,13},
                {InboundCSVFields.UpfrontAmountFlag,14},
                {InboundCSVFields.CostPriceLocalCurrency,15},
                {InboundCSVFields.CostPriceFundCurrency,16},
                {InboundCSVFields.GTICode,17},
                {InboundCSVFields.GTILabel,18},
                {InboundCSVFields.RBCContractNumber,19},
                {InboundCSVFields.TransactionStatusNumber,20},
                {InboundCSVFields.BrokerBICCode,21},
                {InboundCSVFields.Premium,22},
                {InboundCSVFields.GrossAmount,23},
                {InboundCSVFields.NetAmount,24},
                {InboundCSVFields.Fees,25}
            };
            CAMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.MultifondCode,0},
                {InboundCSVFields.FundCustodyCode,1},
                {InboundCSVFields.FundName,2},
                {InboundCSVFields.TransactionID,3},
                {InboundCSVFields.TransactionType,4},
                {InboundCSVFields.TransactionStatus,5},
                {InboundCSVFields.LSTType,6},
                {InboundCSVFields.LSTName,7},
                {InboundCSVFields.ExDate,8},
                {InboundCSVFields.PaymentDate,9},
                {InboundCSVFields.OldISIN,10},
                {InboundCSVFields.OldName,11},
                {InboundCSVFields.OldGTIName,12},
                {InboundCSVFields.OldCurrency,13},
                {InboundCSVFields.NewISIN,14},
                {InboundCSVFields.NewName,15},
                {InboundCSVFields.NewGTIName,16},
                {InboundCSVFields.NewCurrency,17},
                {InboundCSVFields.WithholdingTaxRate,18},
                {InboundCSVFields.InAdvanceQuantity,19},
                {InboundCSVFields.OutQuantity,20},
                {InboundCSVFields.InQuantity,21},
                {InboundCSVFields.FractionalQuantity,22},
                {InboundCSVFields.OddQuantity,23},
                {InboundCSVFields.GrossAmount,24},
                {InboundCSVFields.WithholdingTaxAmount,25},
                {InboundCSVFields.NetAmount,26},
                {InboundCSVFields.NewRatio,27},
                {InboundCSVFields.OldRatio,28},
                {InboundCSVFields.SettlementType,29},
                {InboundCSVFields.Price,30},
                {InboundCSVFields.ExternalFundIdentifier,31},
                {InboundCSVFields.OldType,32},
                {InboundCSVFields.NewType,33},
                {InboundCSVFields.OldBloombergCode,34},
                {InboundCSVFields.NewBloombergCode,35}
            };
            CollateralMappings = new Dictionary<InboundCSVFields, int>()
            {
                {InboundCSVFields.FundCustodyCode,0},
                {InboundCSVFields.ExternalFundIdentifier,1},
                {InboundCSVFields.PTGName,2},
                {InboundCSVFields.TradeDate,3},
                {InboundCSVFields.SettlementDate,4},
                {InboundCSVFields.MaturityDate,5},
                {InboundCSVFields.TransactionID,6},
                {InboundCSVFields.FundManagerReference,7},
                {InboundCSVFields.TransactionType,8},
                {InboundCSVFields.TransactionDescription,9},
                {InboundCSVFields.ReversalFlag,10},
                {InboundCSVFields.SecurityName,11},
                {InboundCSVFields.CollateralCounterpartyName,12},
                {InboundCSVFields.ISINCode,13},
                {InboundCSVFields.SecurityDescription,14},
                {InboundCSVFields.Quantity,15},
                {InboundCSVFields.Currency,16},
                {InboundCSVFields.Price,17},
                {InboundCSVFields.GrossAmount,18},
                {InboundCSVFields.Interest,19},
                {InboundCSVFields.Brokerage,20},
                {InboundCSVFields.Tax,21},
                {InboundCSVFields.Expenses,22},
                {InboundCSVFields.NetAmount,23},
                {InboundCSVFields.BuyCurrency,24},
                {InboundCSVFields.PurchasedAmount,25},
                {InboundCSVFields.SellCurrency,26},
                {InboundCSVFields.SoldAmount,27},
                {InboundCSVFields.FXRate,28},
                {InboundCSVFields.DepositAmount,29},
                {InboundCSVFields.DepositRate,30},
                {InboundCSVFields.NoOfDays,31},
                {InboundCSVFields.CollateralCounterpartyBICCode,32},
                {InboundCSVFields.FundManagerBICCode,33},
                {InboundCSVFields.BloombergCode,34}
            };
            initApi();
            Currencies = GetCurrencies();
            DefaultCultureInfo = new CultureInfo("en-US");
        }

        #region Database util
        public static bool initApi()
        {
            if (CSMApi.GetInstance() != null)
                ApiInit = true;
            if (!ApiInit)
            {
                CSMApi api = null;
                try
                {
                    api = new CSMApi();
                }
                catch (Exception ex)
                {
                    CSMLog.Write("CSxValidationUtil", "initApi", CSMLog.eMVerbosity.M_error, "Failed to create a new CSMApi object: " + ex.Message);
                    ApiInit = false;
                    return false;
                }
                if (api != null)
                {
                    try
                    {
                        api.Initialise();
                    }
                    catch (Exception ex)
                    {
                        CSMLog.Write("CSxValidationUtil", "initApi", CSMLog.eMVerbosity.M_error, "Failed to initialize API: " + ex.Message);
                        ApiInit = false;
                        return false;
                    }
                }
                ApiInit = true;
                return true;
            }
            else
            {
                return true;
            }
        }
        public static HashSet<string> GetCurrencies()
        {
            HashSet<string> retval = new HashSet<string>();
            using (var cmd = new OracleCommand())
            {
                cmd.Connection = DBContext.Connection;
                cmd.CommandText = "SELECT DEVISE_TO_STR(CODE) AS CCY from DEVISEV2";
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string currency = (string)reader["CCY"];
                        retval.Add(currency);
                    }
                }
            }
            return retval;
        }
        #endregion

        #region CSVUtil
        public static void removeLastEmptyElements(List<string> fields) //Slow performance
        {
            if (fields != null)
            {
                int i = 0;
                for (i = fields.Count - 1; i >= 0; i--)
                {
                    string str = fields[i];
                    if (str.Length != 0)
                    {
                        break;
                    }
                }
                if (i != fields.Count - 1)
                {
                    fields.RemoveRange(i + 1, fields.Count - 1);
                }
            }
        }

        public static List<string> splitCSV(string csv)
        {
            if (csv != null)
            {
                List<string> fields = csv.Split(';').ToList();
                for (int i = 0; i < fields.Count; i++)
                {
                    fields[i] = fields[i].Replace(@"""", "").Replace(@"''", "").Trim();
                }
                return fields;
            }
            return null;
        }

        public static int GetDateInFormat(string input, string format = DefaultDateFormat)
        {
            int retval = 0;
            try
            {
                DateTime dateTime;
                DateTime.TryParseExact(input, format, DefaultCultureInfo, DateTimeStyles.AssumeLocal, out dateTime);
                sophisTools.CSMDay date = new sophisTools.CSMDay(dateTime.Day, dateTime.Month, dateTime.Year);
                retval = date.toLong();
            }
            catch (Exception ex)
            {
                CSMLog.Write("CSxValidationUtil", "GetDateFormat1904", CSMLog.eMVerbosity.M_error, "Failed to convert date format. Exception: " + ex.Message + ". Stack trace: " + ex.StackTrace);
            }
            return retval;
        }

        public static string TryAccessListValue(List<string> list, int index)
        {
            string retval = "";
            int count = 0;
            if (list != null)
            {
                count = list.Count;
                if (index < count && index >= 0)
                {
                    try
                    {
                        retval = list[index];
                    }
                    catch (Exception ex)
                    {
                        CSMLog.Write("CSxValidationUtil", "TryAccessListValue", CSMLog.eMVerbosity.M_error, "Exception occurred while trying to access list element at index = " + index + ". Message: " + ex.Message);
                    }
                }
            }
            return retval;
        }

        #endregion

        #region Document field validation
        public static bool ValidateCommonIdentifierFormat(string commonIdentifier)
        {
            /* Disabled for debug purposes
            if (!String.IsNullOrEmpty(commonIdentifier))
            {
                char[] arr = commonIdentifier.ToCharArray();
                if (arr.Length == 14)
                {
                    for (int i = 0; i < 14; i++)
                    {
                        if (!Char.IsLetter(arr[i]) && i < 2)
                        {
                            return false;
                        }
                        else if (arr[6] != 'Z')
                        {
                            return false;
                        }
                        else if (!Char.IsDigit(arr[i]))
                        {
                            return false;
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
             */ 
            return true;
        }

        public static bool ValidateDate(string input, string format = DefaultDateFormat)
        {
            try
            {
                DateTime.ParseExact(input, format, DefaultCultureInfo);
            }
            catch(Exception ex)
            {
                CSMLog.Write("CSxValidationUtil", "ValidateDate", CSMLog.eMVerbosity.M_debug, "Exception " + ex.Message);
                return false;
            }
            return true;
        }

        public static int GetDateInAnyFormat(string input, params string[] allowedFormats)
        {
            int retval = 0;
            string dateFormat = GetDateFormat(input, allowedFormats);
            if (!String.IsNullOrEmpty(dateFormat))
            {
                retval = GetDateInFormat(input, dateFormat);
            }
            return retval;
        }

        public static string GetDateFormat(string input, params string[] allowedFormats)
        {
            string retval = "";
            if (allowedFormats != null)
            {
                for (int i = 0; i < allowedFormats.Length; i++)
                {
                    bool success = true;
                    try
                    {
                        DateTime.ParseExact(input, allowedFormats[i], DefaultCultureInfo);
                    }
                    catch (Exception ex)
                    {
                        success = false;
                    }
                    if (success)
                    {
                        retval = allowedFormats[i];
                        break;
                    }
                }
            }
            return retval;
        }

        public static bool ValidateMultiDate(string input, params string[] allowedFormats)
        {
            bool retval = false;
            string dateFormat = GetDateFormat(input, allowedFormats);
            if (!String.IsNullOrEmpty(dateFormat))
            {
                retval = true;
            }
            return retval;
        }

        public static bool ValidateCurrency(string currency)
        {
            if (!String.IsNullOrEmpty(currency))
            {
                if (!Currencies.Contains(currency.ToUpper())) //case insensitive validation
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool ValidateInteger(string number)
        {
            try
            {
                Int64.Parse(number);
            }
            catch (Exception ex)
            {
                CSMLog.Write("CSxValidationUtil", "ValidateInteger", CSMLog.eMVerbosity.M_debug, "Exception " + ex.Message);
                return false;
            }
            return true;
        }

        public static bool ValidateDouble(string number)
        {
            try
            {
                Double.Parse(number);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool ValidateNotEmpty(string str)
        {
            return !String.IsNullOrEmpty(str);
        }

        #endregion

        #region Document class validation
        public static bool isEquityOrBond(string csv)
        {
            List<string> fields = splitCSV(csv).ToList();
            return isEquityOrBond(fields);
        }

        public static bool isBond(string csv)
        {
            List<string> fields = splitCSV(csv).ToList();
            return isBond(fields);
        }

        public static bool isEquity(string csv)
        {
            List<string> fields = splitCSV(csv).ToList();
            return isEquity(fields);
        }

        public static bool isCollateral(List<string> fields)
        {
            if (fields == null)
            {
                return false;
            }

            if (!ValidateNotEmpty(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.ExternalFundIdentifier])))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                return false;
            }
            if (!ValidateNotEmpty(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.FundCustodyCode])))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "FundCustodyCode validation failed, invalid format");
                return false;
            }
            if (!ValidateNotEmpty(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.PTGName])))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "PTGName validation failed, invalid format");
                return false;
            }
            if (!ValidateMultiDate(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.TradeDate]), CommonDateFormats))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CommonDateFormats) + " Value: " + TryAccessListValue(fields, CollateralMappings[InboundCSVFields.TradeDate]));
                return false;
            }
            if(!ValidateMultiDate(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.SettlementDate]), CommonDateFormats))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CommonDateFormats) + " Value: " + TryAccessListValue(fields, CollateralMappings[InboundCSVFields.SettlementDate]));
                return false;
            }
            if (!ValidateCurrency(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.Currency])))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                return false;
            }
            if (!ValidateDouble(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.Quantity]))
                && !ValidateDouble(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.NetAmount])))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "NetAmount and Quantity validation failed, invalid format");
                return false;
            }
            if (!ValidateNotEmpty(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.CollateralCounterpartyBICCode])))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "CollateralCounterpartyBICCode validation failed, invalid format");
                return false;
            }
            if (!ValidateNotEmpty(TryAccessListValue(fields, CollateralMappings[InboundCSVFields.TransactionDescription])))
            {
                CSMLog.Write("CSxValidationUtil", "isCollateral", CSMLog.eMVerbosity.M_debug, "TransactionDescription validation failed, this field cannot be empty");
                return false;
            }

            return true;
        }

        public static bool isForex(string csv)
        {
            List<string> fields = splitCSV(csv).ToList();
            return isForex(fields);
        }

        public static bool isEquityOrBond(List<string> fields)
        {
            if (fields != null)
            {
                //removeLastEmptyElements(fields);
                //if (fields.Count >= EquityAndBondMappings[InboundCSVFields.BloombergCode]) //last mandatory field(column)
                //{
                    if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.ExternalFundIdentifier])))
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquityOrBond", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!ValidateMultiDate(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.TradeDate]),CommonDateFormats))
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquityOrBond", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CommonDateFormats) + " Value: " + TryAccessListValue(fields, EquityAndBondMappings[InboundCSVFields.TradeDate]));
                        return false;
                    }
                    if (!ValidateMultiDate(TryAccessListValue(fields, EquityAndBondMappings[InboundCSVFields.SettlementDate]), CommonDateFormats))
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquityOrBond", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CommonDateFormats) + " Value: " + TryAccessListValue(fields, EquityAndBondMappings[InboundCSVFields.SettlementDate]));
                        return false;
                    }
                    if (!ValidateDouble(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.Quantity])))
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquityOrBond", CSMLog.eMVerbosity.M_debug, "Quantity validation failed, cannot cast to double");
                        return false;
                    }
                    if (!ValidateDouble(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.Price])))
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquityOrBond", CSMLog.eMVerbosity.M_debug, "Price validation failed, cannot cast to double");
                        return false;
                    }
                    if (!ValidateCurrency(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.Currency])))
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquityOrBond", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                    if (!ValidateNotEmpty(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.TransactionDescription])))
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquityOrBond", CSMLog.eMVerbosity.M_debug, "TransactionDescription validation failed, this field cannot be empty");
                        return false;
                    }
                    if (!ValidateNotEmpty(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.SecurityDescription])))
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquityOrBond", CSMLog.eMVerbosity.M_debug, "SecurityDescription validation failed, this field cannot be empty");
                        return false;
                    }
                //}
                //else
                //{
                //    return false;
                //}
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isBond(List<string> fields)
        {
            if (!isEquityOrBond(fields))
            {
                return false;
            }
            else
            {
                if (fields != null)
                {
                    if (!String.IsNullOrEmpty(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.SecurityDescription])))
                    {
                        string str = TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.SecurityDescription]);
                        if (str.ToUpper().CompareTo("BOND") == 0 || str.ToUpper().CompareTo("BONDS") == 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        CSMLog.Write("CSxValidationUtil", "isBond", CSMLog.eMVerbosity.M_error, "Failed to find a SecurityDescription field");
                        return false;
                    }
                }
                else
                {
                    CSMLog.Write("CSxValidationUtil", "isBond", CSMLog.eMVerbosity.M_error, "Fatal error: failed to parse CSV for validation");
                    return false;
                }
            }
        }

        public static bool isEquity(List<string> fields)
        {
            if (!isEquityOrBond(fields))
            {
                return false;
            }
            else
            {
                if (fields != null)
                {
                    if (!String.IsNullOrEmpty(TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.SecurityDescription])))
                    {
                        string str = TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.SecurityDescription]);
                        if (str.ToUpper().CompareTo("EQUITIES") == 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        CSMLog.Write("CSxValidationUtil", "isEquity", CSMLog.eMVerbosity.M_error, "Failed to find a SecurityDescription field");
                        return false;
                    }
                }
                else
                {
                    CSMLog.Write("CSxValidationUtil", "isEquity", CSMLog.eMVerbosity.M_error, "Fatal error: failed to parse CSV for validation");
                    return false;
                }
            }
        }

        public static CSVDocumentClass isEquityOrBondOrLoan(List<string> fields)
        {
            //Loan is a special type of bond
            if (!isEquityOrBond(fields))
            {
                return CSVDocumentClass.Unknown;
            }
            else
            {
                string security_desc = TryAccessListValue(fields,EquityAndBondMappings[InboundCSVFields.SecurityDescription]);
                if (!String.IsNullOrEmpty(security_desc))
                {
                    switch (security_desc.ToUpper())
                    {
                        case "EQUITIES" :
                            {
                                return CSVDocumentClass.Equity;
                            } break;
                        case "BOND":
                        case "BONDS" :
                            {
                                return CSVDocumentClass.Bond;
                            } break;
                        case "FUND" :
                        case "FUNDS":
                        case "ETF":
                        case "ETFS":
                            {
                                return CSVDocumentClass.Fund;
                            } break;
                        case "MEMMM":
                            {
                                return CSVDocumentClass.Loan;
                            } break;
                        default:
                            {
                                return CSVDocumentClass.Loan;
                            } break;
                    }
                }
                return CSVDocumentClass.Unknown;
            }
        }

        public static bool isForex(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields, ForexMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForex", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, ForexMappings[InboundCSVFields.TradeDate]), ForexDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isForex", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(ForexDateFormats) + " Value: " + TryAccessListValue(fields, ForexMappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, ForexMappings[InboundCSVFields.SettlementDate]), ForexDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isForex", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(ForexDateFormats) + " Value: " + TryAccessListValue(fields, ForexMappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields, ForexMappings[InboundCSVFields.BuyCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForex", CSMLog.eMVerbosity.M_debug, "BuyCurrency validation failed, could not find a valid currency");
                    return false;
                }
                if (!ValidateDouble(TryAccessListValue(fields, ForexMappings[InboundCSVFields.PurchasedAmount])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForex", CSMLog.eMVerbosity.M_debug, "PurchasedAmount validation failed, cannot cast to double");
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields, ForexMappings[InboundCSVFields.SellCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForex", CSMLog.eMVerbosity.M_debug, "SellCurrency validation failed, could not find a valid currency");
                    return false;
                }
                if (!ValidateDouble(TryAccessListValue(fields, ForexMappings[InboundCSVFields.SoldAmount])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForex", CSMLog.eMVerbosity.M_debug, "SoldAmount validation failed, cannot cast to double");
                    return false;
                }
                if (!ValidateDouble(TryAccessListValue(fields, ForexMappings[InboundCSVFields.FXRate])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForex", CSMLog.eMVerbosity.M_debug, "FXRate validation failed, cannot cast to double");
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isOption(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields,OptionMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isOption", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, OptionMappings[InboundCSVFields.TradeDate]),OptionsDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isOption", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(OptionsDateFormats) + " Value: " + TryAccessListValue(fields, OptionMappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, OptionMappings[InboundCSVFields.SettlementDate]), OptionsDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isOption", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(OptionsDateFormats) + " Value: " + TryAccessListValue(fields, OptionMappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, OptionMappings[InboundCSVFields.NAVDate]), OptionsDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isOption", CSMLog.eMVerbosity.M_debug, "NAVDate validation failed, invalid format; expected format: " + PrintDateFormatArray(OptionsDateFormats) + " Value: " + TryAccessListValue(fields, OptionMappings[InboundCSVFields.NAVDate]));
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields,OptionMappings[InboundCSVFields.OptionCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isOption", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isFuture(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields,FutureMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isFuture", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, FutureMappings[InboundCSVFields.TradeDate]), FuturesDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isFuture", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(FuturesDateFormats) + " Value: " + TryAccessListValue(fields, FutureMappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, FutureMappings[InboundCSVFields.SettlementDate]), FuturesDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isFuture", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(FuturesDateFormats) + " Value: " + TryAccessListValue(fields, FutureMappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                //if (!ValidateDate(TryAccessListValue(fields,FutureMappings[InboundCSVFields.NAVDate])))
                if (!ValidateMultiDate(TryAccessListValue(fields, FutureMappings[InboundCSVFields.NAVDate]), FuturesDateFormats))
                {
                    //CSMLog.Write("CSxValidationUtil", "isFuture", CSMLog.eMVerbosity.M_debug, "NAVDate validation failed, invalid format");
                    CSMLog.Write("CSxValidationUtil", "isFuture", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(FuturesDateFormats) + " Value: " + TryAccessListValue(fields, FutureMappings[InboundCSVFields.NAVDate]));
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields,FutureMappings[InboundCSVFields.FutureCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isFuture", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isCash(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields,CashMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isCash", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields,CashMappings[InboundCSVFields.TradeDate]),CashDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isCash", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CashDateFormats) + " Value: " + TryAccessListValue(fields, CashMappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, CashMappings[InboundCSVFields.SettlementDate]), CashDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isCash", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CashDateFormats) + " Value: " + TryAccessListValue(fields, CashMappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields,CashMappings[InboundCSVFields.Currency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isCash", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isInvoice(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields, InvoiceMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isInvoice", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, InvoiceMappings[InboundCSVFields.TradeDate]), CashDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isInvoice", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CashDateFormats) + " Value: " + TryAccessListValue(fields, InvoiceMappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, InvoiceMappings[InboundCSVFields.SettlementDate]), CashDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isInvoice", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CashDateFormats) + " Value: " + TryAccessListValue(fields, InvoiceMappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields, InvoiceMappings[InboundCSVFields.Currency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isInvoice", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isForexHedge(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields, ForexHedgeMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForexHedge", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, ForexHedgeMappings[InboundCSVFields.TradeDate]), ForexDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isForexHedge", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(ForexDateFormats) + " Value: " + TryAccessListValue(fields, ForexHedgeMappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, ForexHedgeMappings[InboundCSVFields.MaturityDate]), ForexDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isForexHedge", CSMLog.eMVerbosity.M_debug, "MaturityDate validation failed, invalid format; expected format: " + PrintDateFormatArray(ForexDateFormats) + " Value: " + TryAccessListValue(fields, ForexHedgeMappings[InboundCSVFields.MaturityDate]));
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields, ForexHedgeMappings[InboundCSVFields.CounterCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForexHedge", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields, ForexHedgeMappings[InboundCSVFields.FixedCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isForexHedge", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isTACash(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields, TACashMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isTACash", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, TACashMappings[InboundCSVFields.TradeDate]), CashDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isTACash", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CashDateFormats) + " Value: " + TryAccessListValue(fields, TACashMappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, TACashMappings[InboundCSVFields.SettlementDate]), CashDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isTACash", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CashDateFormats) + " Value: " + TryAccessListValue(fields, TACashMappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields, TACashMappings[InboundCSVFields.AccountCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isTACash", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
                /*
                if (!ValidateCurrency(TryAccessListValue(fields, TACashMappings[InboundCSVFields.ConfirmedAmountLocalCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isInvoice", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
                 */ 
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isBond2(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields, Bond2Mappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isBond2", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, Bond2Mappings[InboundCSVFields.TradeDate]), CommonDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isBond2", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CommonDateFormats) + " Value: " + TryAccessListValue(fields, Bond2Mappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, Bond2Mappings[InboundCSVFields.SettlementDate]), CommonDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isBond2", CSMLog.eMVerbosity.M_debug, "SettlementDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CommonDateFormats) + " Value: " + TryAccessListValue(fields, Bond2Mappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields, Bond2Mappings[InboundCSVFields.Currency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isBond2", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, Bond2Mappings[InboundCSVFields.MaturityDate]), CommonDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isBond2", CSMLog.eMVerbosity.M_debug, "MaturityDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CommonDateFormats) + " Value: " + TryAccessListValue(fields, Bond2Mappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                /*
                if (!ValidateCurrency(TryAccessListValue(fields, TACashMappings[InboundCSVFields.ConfirmedAmountLocalCurrency])))
                {
                    CSMLog.Write("CSxValidationUtil", "isInvoice", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
                 */
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isSwap(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields, SwapMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isSwap", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, SwapMappings[InboundCSVFields.TradeDate]), SwapsDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isSwap", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(SwapsDateFormats) + " Value: " + TryAccessListValue(fields, SwapMappings[InboundCSVFields.TradeDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, SwapMappings[InboundCSVFields.SettlementDate]), SwapsDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isSwap", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(SwapsDateFormats) + " Value: " + TryAccessListValue(fields, SwapMappings[InboundCSVFields.SettlementDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, SwapMappings[InboundCSVFields.MaturityDate]), SwapsDateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isSwap", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(SwapsDateFormats) + " Value: " + TryAccessListValue(fields, SwapMappings[InboundCSVFields.MaturityDate]));
                    return false;
                }
                /*
                if (!ValidateCurrency(TryAccessListValue(fields, SwapMappings[InboundCSVFields.CurrencyPayableLeg])))
                {
                    CSMLog.Write("CSxValidationUtil", "isSwap", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
                if (!ValidateCurrency(TryAccessListValue(fields, SwapMappings[InboundCSVFields.CurrencyReceivableLeg])))
                {
                    CSMLog.Write("CSxValidationUtil", "isSwap", CSMLog.eMVerbosity.M_debug, "Currency validation failed, could not find a valid currency");
                    return false;
                }
                 */ 
                /*
                if (!ValidateDouble(TryAccessListValue(fields, SwapMappings[InboundCSVFields.Quantity])))
                {
                    CSMLog.Write("CSxValidationUtil", "isSwap", CSMLog.eMVerbosity.M_debug, "Quantity validation failed, cannot cast to double");
                    return false;
                }
                if (!ValidateDouble(TryAccessListValue(fields, SwapMappings[InboundCSVFields.Premium])))
                {
                    CSMLog.Write("CSxValidationUtil", "isSwap", CSMLog.eMVerbosity.M_debug, "Premium validation failed, could not find a valid currency");
                    return false;
                }
                 * */
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool isCA(List<string> fields)
        {
            if (fields != null)
            {
                if (!ValidateCommonIdentifierFormat(TryAccessListValue(fields, CAMappings[InboundCSVFields.ExternalFundIdentifier])))
                {
                    CSMLog.Write("CSxValidationUtil", "isCA", CSMLog.eMVerbosity.M_debug, "ExternalFundIdentifier validation failed, invalid format");
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, CAMappings[InboundCSVFields.ExDate]), CADateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isCA", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CADateFormats) + " Value: " + TryAccessListValue(fields, CAMappings[InboundCSVFields.ExDate]));
                    return false;
                }
                if (!ValidateMultiDate(TryAccessListValue(fields, CAMappings[InboundCSVFields.PaymentDate]), CADateFormats))
                {
                    CSMLog.Write("CSxValidationUtil", "isCA", CSMLog.eMVerbosity.M_debug, "TradeDate validation failed, invalid format; expected format: " + PrintDateFormatArray(CADateFormats) + " Value: " + TryAccessListValue(fields, CAMappings[InboundCSVFields.PaymentDate]));
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static CSVDocumentClass GetDocumentClass(List<string> fields)
        {
            try
            {
                if (CSxValidationUtil.isBond2(fields))
                {
                    return CSVDocumentClass.Bond2;
                }
                else
                {
                    CSVDocumentClass doc_class = isEquityOrBondOrLoan(fields);
                    if (doc_class != CSVDocumentClass.Unknown)
                    {
                        return doc_class;
                    }
                    else if (CSxValidationUtil.isForex(fields))
                    {
                        return CSVDocumentClass.Forex;
                    }
                    else if (CSxValidationUtil.isOption(fields))
                    {
                        return CSVDocumentClass.Option;
                    }
                    else if (CSxValidationUtil.isFuture(fields))
                    {
                        return CSVDocumentClass.Future;
                    }
                    else if (CSxValidationUtil.isCash(fields))
                    {
                        return CSVDocumentClass.Cash;
                    }
                    else if (CSxValidationUtil.isInvoice(fields))
                    {
                        return CSVDocumentClass.Invoice;
                    }
                    else if (CSxValidationUtil.isForexHedge(fields))
                    {
                        return CSVDocumentClass.ForexHedge;
                    }
                    else if (CSxValidationUtil.isTACash(fields))
                    {
                        return CSVDocumentClass.TACash;
                    }
                    else if (CSxValidationUtil.isSwap(fields))
                    {
                        return CSVDocumentClass.Swap;
                    }
                    else if (CSxValidationUtil.isCA(fields))
                    {
                        return CSVDocumentClass.CorporateAction;
                    }
                    else if (CSxValidationUtil.isCollateral(fields))
                    {
                        return CSVDocumentClass.Collateral;
                    }
                    else
                    {
                        return CSVDocumentClass.Unknown;
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("CSxValidationUtil", "GetDocumentClass", CSMLog.eMVerbosity.M_error, "Error occurred while trying to validate document class: " + ex.Message + ". InnerException: " + ex.InnerException + ". Stack trace: " + ex.StackTrace);
                return CSVDocumentClass.Unknown;
            }
        }

        #endregion

        private static string PrintDateFormatArray(params string[] dateFormats)
        {
            string retval = null;
            if (dateFormats != null)
            {
                StringBuilder strb = new StringBuilder();
                strb.Append("[ ");
                for (int i = 0; i < dateFormats.Length; i++)
                {
                    strb.Append(dateFormats[i]);
                    if (i != dateFormats.Length - 1)
                    {
                        strb.Append(" or ");
                    }
                }
                strb.Append(" ]");
                retval = strb.ToString();
            }
            return retval;
        }
    }
}
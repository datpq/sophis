using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using DataTransformation.Models;
//using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Collections;
using System.Text.RegularExpressions;
using DataTransformation.Settings;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace DataTransformation
{
    public enum TransName
    {
        ThirdPartyName,
        ThirdPartyEntity,
        Share,
        YieldCurve,
        Benchmark,
        MIL_MarketData,
        MIL_NavFund,
        MIL_Trade_RMA,
        MIL_Trade_RMA_Alert, //Send email with reversed processing condition
        MIL_Cash_RMA,
        DIM_OTC_Trade_RMA,
        DIM_OTC_IRS,
        DIM_OTC_FXOPTIONS,
        REFI_CA_ISIN_CREATION,
        REFI_CA,
        REFI_CA_SR,
        REFI_CA_Cancel,
        REFI_CA_Update_MT566_Mandatory,
        BBH_Fee,
        BBH_JE,
        BBH_IM_Orig,
        BBH_IM_Mirr,
        BBH_IM_Orig_Rev,
        BBH_IM_Mirr_Rev,
        BBH_MI,
        BBH_MC_Orig,
        BBH_MC_Mirr,
        BBH_DIM_Trade,
        BBH_DIM_Trade_RMA,
        BBH_DIM_Trade_Cancel,
        BBH_DIM_Trade_Ack,
        BBH_DIM_Cash_Orig,
        BBH_DIM_Cash_Mirr,
        FileConnector_ErrorParser,
        SSBAllCustody_Prep,
        SSBAllCustody_Prep_Mirr,
        SSBAllCustody_CollateralCash,
        SSB2Rbc_CashCollateralTrade,
        SSB2Rbc_CashMonthlyInterest,
        SSB2Rbc_CorporateAction,
        SSB2Rbc_CorporateActionFR,
        SSB2Rbc_SR,
        SSB2Rbc_CashTrade,
        SSB2Rbc_EquityTrade,
        SSB2Rbc_FixedIncomeTrade,
        SSB2Rbc_FxTrade,
        SSB2Rbc_OptionTrade,
        SSB2Rbc_FutureTrade,
        SSB2Rbc_SwapTrade,
        SSB2Rbc_NavFlow,
        SSB2Rbc_NavFund,
        SSB2Rbc_NavStrategy,
        FundSettle2Rbc_OrderExec,
        Notify_CASH_MGR_NACK_Reponse,//SEA
        SWIFT_ACK_NACK,//SEA
        SWIFT_ACK_NACK_Mir,//SEA
        BBH_DIM_Corporate_Action_SBRI,
        BBH_DIM_Corporate_Action_SD,
        BBH_DIM_Corporate_Action_SS,
        BBH_DIM_Corporate_Action_RHTS,//BBH_DIM_Corporate_Action_SRI,
        BBH_DIM_Corporate_Action_EXRI,//BBH_DIM_Corporate_Action_SRI,
        BBH_DIM_Corporate_Action_SCDR,
        BBH_DIM_Corporate_Action_SEO,
        BBH_DIM_Corporate_Action_STO,
        BBH_DIM_Corporate_Action_SMA,
		SSBOTC_MARGIN_Parser,
        SSB_File_Sequencing
    }

    public static class Transformation
    {
        public const char DEFAULT_CSV_SEPARATOR = ';';
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static string GetCsvVal(string[] csvHeaders, string[] csvVals, string columnName)
        {
            int idx = Enumerable.Range(0, csvHeaders.Length)
                    .Where(i => columnName.Equals(csvHeaders[i])).FirstOrDefault();

            return csvVals[idx];
        }

        public static PdtTransformationSetting InitializeAndSaveConfig(string configFile)
        {
            Logger.Warn("BEGIN");
            Logger.Warn("YOU ARE IN DEBUG MODE. SWITCH TO PRODUCTION MODE BEFORE GOING LIVE.");
            var Setting = new PdtTransformationSetting
            {
                Tables = new[] {
                    new PdtLookupTable{
                        Name = "MIFL_ME_01ANATIT",
                        File = "MIFL_ME_01ANATIT_*",
                        keyExpression = "lineVal.Substring(0, 10).Trim()",
                        columnsExpression = new[] {
                            "lineVal.Substring(50, 4).Trim()",
                            "lineVal.Substring(70, 2).Trim()",
                            "lineVal.Substring(72, 12).Trim()",
                            "lineVal.Substring(87, 20).Trim()",
                            "lineVal.Substring(107, 20).Trim()",
                            "lineVal.Substring(167, 8).Trim()",
                            "lineVal.Substring(260, 2).Trim()",
                            "lineVal.Substring(262, 4).Trim()",
                            "lineVal.Substring(276, 40).Trim()",
                            "lineVal.Substring(438, 1).Trim()",
                            "lineVal.Substring(437, 1) + lineVal.Substring(421, 15 - int.Parse(lineVal.Substring(436, 1))) + \".\" + lineVal.Substring(436 - int.Parse(lineVal.Substring(436, 1)), int.Parse(lineVal.Substring(436, 1)))",
                        }
                    },
                    new PdtLookupTable
                    {
                        Name = "MIFL_ME_11MOVCN",
                        File = "MIFL_ME_11MOVCN_2",
                        processingCondition = "lineVal.Substring(84, 4).Trim()==\"TDA\"",
                        keyExpression = "lineVal.Substring(88, 12) + lineVal.Substring(0, 4).Trim()",
                    },
                    new PdtLookupTable {
                        Name = "DIM_OTC_TRADE_INSTRUMENT",
                        File = "OTC_Lookup_InstrumentRef_*.txt",
                        csvSeparator = ';',
                        keyExpression = @"lineVal.Split(';')[0]",//Trade Reference
                        columnsExpression = new[] {
                            "lineVal.Split(';')[1]", //Sicovam
                        }
                    },
                    new PdtLookupTable {
                        Name = "REFI_CA_TABLE_BY_ID",
                        File = "REFI_CA_LOOKUP*",
                        Expires = 60,
                        csvSeparator = ',',
                        processingCondition = @"lineVal.Split(';')[4] == ""CAP"" || (lineVal.Split(';')[4] == ""DIV"" && lineVal.Split(';')[8] != """")",
                        keyExpression = @"lineVal.Split(';')[4]+""_""+lineVal.Split(';')[41]",//Corporate Actions Type_Corporate Actions ID
                        columnsExpression = new[] {
                            "lineVal.Split(';')[18] == \"\" ? \"1\" : lineVal.Split(';')[18]",//CAP.numerator
                            "lineVal.Split(';')[17] == \"\" ? \"1\" : lineVal.Split(';')[17]",//CAP.denumerator
                            "System.DateTime.ParseExact(lineVal.Split(';')[29], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")",//CAP.Subscription Period Start Date
                            "System.DateTime.ParseExact(lineVal.Split(';')[30], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")",//CAP.Subscription Period End Date
                            "lineVal.Split(';')[8] == \"\" ? \"1\" : lineVal.Split(';')[8]",//DIV.Dividend Rate
                            //"System.DateTime.ParseExact(lineVal.Split(';')[14], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")",//CAP.Effective Date
                        }
                    },
                    new PdtLookupTable {
                        Name = "REFI_CA_TABLE_BY_ISIN",
                        File = "REFI_CA_LOOKUP*",
                        csvSeparator = ',',
                        Expires = 60,
                        processingCondition = @"lineVal.Split(';')[4] == ""CAP""",
                        keyExpression = @"lineVal.Split(';')[4]+""_""+lineVal.Split(';')[0]",//Corporate Actions Type_ISIN
                        columnsExpression = new[] {
                            "lineVal.Split(';')[42]",//CapType
                        }
                    },
                    new PdtLookupTable {
                        Name = "MEDIO_BBH_FUNDFILTER",
                        File = "SQL",
                        keyExpression = "SELECT FUNDID FROM MEDIO_BBH_FUNDFILTER",
                    }
                },
                Transformations = new[] {
                new PdtTransformation {
                    name = TransName.ThirdPartyName.ToString(),
                    type = TransType.Csv2Xml,
                    label = "ThirdParty Names",
                    templateFile = "Import_ThirdParty_names.xml",
                    category = "Third Parties",
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    columns = new [] {
                        new PdtColumn {
                            name = "Reference",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'reference')]/text()" } }
//										"//*:partyId[contains(@*:partyIdScheme, 'reference')]"
                        },
                        new PdtColumn {
                            name = "Name",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'name')]/text()" },
                                new PdtColumnDest { path = "//*[local-name() = 'partyName']/text()" } }
                        },
                        new PdtColumn {
                            name = "Location",
                            isRequired = false,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'location']" } }
//										"//party:location"
                        }
                    }
                },
                new PdtTransformation {
                    name = TransName.ThirdPartyEntity.ToString(),
                    type = TransType.Csv2Xml,
                    label = "ThirdParty Entities",
                    templateFile = "Import_ThirdParty_Enty.xml",
                    category = "Third Parties",
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                },
                new PdtTransformation {
                    name = TransName.Share.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Share",
                    templateFile = "Import_Share.xml",
                    category = "Instruments",
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    columns = new [] {
                        new PdtColumn {
                            name = "Reference",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'identifier']/*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Reference']/text()" } }
                        },
                        new PdtColumn {
                            name = "Name",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'share']/*[local-name() = 'name']/text()" } }
                        }
                    }
                },
                new PdtTransformation {
                    name = TransName.YieldCurve.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Yield Curve",
                    templateFile = "importYieldCurve.xml",
                    category = "Market Data",
                    repeatingRootPath = "//*[local-name() = 'points']",
                    repeatingChildrenPath = "//*[local-name() = 'points']/*[local-name() = 'point']",
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    columns = new [] {
                        new PdtColumn {
                            name = "CURVE_NAME",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'name']/text()" } }
                        },
                        new PdtColumn {
                            name = "MARKET_FAMILY",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'family']/text()" } }
                        },
                        new PdtColumn {
                            name = "CURVE_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'date']/text()" } }
                        },
                        new PdtColumn {
                            name = "CURVE_YIELD",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'yield']/text()" } }
                        },
                        new PdtColumn {
                            name = "CURVE_ISBOND",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'isBond']/text()" } }
                        }
                    }
                },
                new PdtTransformation {
                    name = TransName.Benchmark.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Benchmark",
                    templateFile = "Import_Benchmark.xml",
                    category = "Instruments",
                    repeatingRootPath = "//*[local-name() = 'standardComponents']",
                    repeatingChildrenPath = "//*[local-name() = 'instrumentStdComponent']",
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    columns = new [] {
                        new PdtColumn {
                            name = "Reference",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'identifier']/*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Sophisref']/text()" } }
                                //"//*[local-name() = 'identifier']/*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Reference']/text()",
                        },
                        new PdtColumn {
                            name = "Name",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'name']/text()" } }
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'currency']/text()" } }
                        },
                        new PdtColumn {
                            name = "Market",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'market']/*[local-name() = 'sophis']/text()" } }
                        },
                        new PdtColumn {
                            name = "Definition_type",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'definitionType']/text()" } }
                        },
                        new PdtColumn {
                            name = "Is_drifted",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'drifted']/text()" } }
                        },
                        new PdtColumn {
                            name = "Pricing",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'pricingMethod']/text()" } }
                        },
                        new PdtColumn {
                            name = "Hedge_ratio",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'hedgeRatio']/text()" } }
                        },
                        new PdtColumn {
                            name = "Record_date",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'recordDate']/text()" } }
                        },
                        new PdtColumn {
                            name = "Return_computation",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'standardComposition']/*[local-name() = 'useComponentsReturn']/text()" } }
                        },
                        new PdtColumn {
                            name = "Cash_computation",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'standardComposition']/*[local-name() = 'includeCashSinceRecordStart']/text()" } }
                        },
                        new PdtColumn {
                            name = "Resize",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'standardComposition']/*[local-name() = 'resize']/text()" } }
                        },
                        new PdtColumn {
                            name = "Resize_to",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'standardComposition']/*[local-name() = 'resizingType']/text()" } }
                        },
                        new PdtColumn {
                            name = "Instrument",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'instrumentStdComponent']/*[local-name() = 'instrument']/*[local-name() = 'sophis']/text()" } }
                        },
                        new PdtColumn {
                            name = "Weight",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'instrumentStdComponent']/*[local-name() = 'weight']/text()" } }
                        }
                    }
                },
                new PdtTransformation
                {
                    name = TransName.MIL_MarketData.ToString(),
                    type = TransType.Csv2Xml,
                    label = "MIL MarketData",
                    templateFile = "importMarketData.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'quotations']",
                    repeatingChildrenPath = "//*[local-name() = 'quotations']/*[local-name() = 'quotationsByInstrument']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,
                    bunchSize = 5,
                    columns = new [] {
                        new PdtColumn { name = "FUND_NAME", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn {
                            name = "FUND_ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT SICOVAM FROM TITRES WHERE EXTERNREF = '\" + colVal + \"'\"",
                                    },
                                    path = "//*[local-name() = 'sophis']"
                                } }
                        },
                        new PdtColumn { name = "FUND_CURRENCY", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "VARIABLE_SECURITIES", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "FIXED_INTEREST", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "MARGIN", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "DERIVATIVES_OPEN_FORWARD_FX", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "CASH_BALANCE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "CAPTIAL_RECEIVABLE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "CAPITAL_PAYABLE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "OS_SETTLEMENT", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "INCOME_RECEIVABLE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "CHARGES_DUE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "OTHER", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "TOTAL_FUND_VALUE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "TOTAL_FUND_UNITS", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn {
                            name = "EXACT_PRICE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'value']" } }
                        },
                        new PdtColumn {
                            name = "NAV_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "//*[local-name() = 'quotationDate']",
                                expression = "System.DateTime.ParseExact(colVal, \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                            } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.MIL_NavFund.ToString(),
                    type = TransType.Csv2Csv,
                    label = "MIL to RBC Nav Fund",
                    templateFile = "Rbc_NavFund.csv",
                    category = "Medio",
                    csvSrcSeparator = ',',
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    csvSkipLines = 1,
                    //rowClowing = new PdtRowCloning {
                    //    Ntimes = 5,
                    //    Expression = "lineVal.Substring(0, lineVal.LastIndexOf(\",\")+1) + (DateTime.ParseExact(lineVal.Substring(lineVal.LastIndexOf(\",\")+1), \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).AddDays($cloneIdx)).ToString(\"dd/MM/yyyy\")"
                    //},
                    columns = new []
                    {
                        new PdtColumn { name = "FUND_NAME", destPaths = new [] { new PdtColumnDest { path = "Fund name" } } },
                        new PdtColumn {
                            name = "FUND_ID",
                            destPaths = new [] {
                                new PdtColumnDest {
                                    //Lookup = new PdtColumnLookup {
                                    //    Table = "SQL",
                                    //    Expression = "\"SELECT SICOVAM FROM TITRES WHERE EXTERNREF = '\" + colVal + \"'\"",
                                    //},
                                    path = "Fund Code"
                                },
                            }
                        },
                        new PdtColumn { name = "FUND_CURRENCY", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "VARIABLE_SECURITIES", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "FIXED_INTEREST", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "MARGIN", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "DERIVATIVES_OPEN_FORWARD_FX", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "CASH_BALANCE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "CAPTIAL_RECEIVABLE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "CAPITAL_PAYABLE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "OS_SETTLEMENT", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "INCOME_RECEIVABLE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "CHARGES_DUE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "OTHER", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn {
                            name = "TOTAL_FUND_VALUE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Nav Net",
                                    expression = "double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn { name = "TOTAL_FUND_UNITS", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn { name = "EXACT_PRICE", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn {
                            name = "NAV_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Date Nav",
                                    expression = "System.DateTime.ParseExact(colVal, \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).AddDays(@\"$InputFile\".Substring(@\"$InputFile\".Length - 6, 1) == \"_\" ? int.Parse(@\"$InputFile\".Substring(@\"$InputFile\".Length - 5, 1)) : 0).ToString(\"dd-MMM-yyyy\").ToUpper()"
                                }
                            }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.MIL_Trade_RMA.ToString(),
                    type = TransType.Excel2Csv,
                    label = "MIL Trade RMA Preparation",
                    templateFile = "RMA_GenericTrade.csv",
                    category = "Medio",
                    csvSrcSeparator = DEFAULT_CSV_SEPARATOR,
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    csvSkipLines = 1,
                    variables = new[] {
                        new PdtVariable { name = "TransactionType", expressionBefore = "lineVal.Split('$CsvSrcSep')[1]" },
                        new PdtVariable { name = "TradeType", expressionBefore = @""",Sell,Buy,ForwardFX,SpotFX,"".IndexOf("",$TransactionType,"") >= 0 ? ""Purchase/Sale"" : ""Cash Transfer""" },
                        new PdtVariable { name = "Investment", expressionBefore = "lineVal.Split('$CsvSrcSep')[7]" },
                        new PdtVariable { name = "Quantity", expressionBefore = "lineVal.Split('$CsvSrcSep')[8]" },
                        new PdtVariable { name = "SettleNetAmount", expressionBefore = "lineVal.Split('$CsvSrcSep')[9]" },
                        new PdtVariable { name = "SettleCurrency", expressionBefore = "lineVal.Split('$CsvSrcSep')[11]" },
                        new PdtVariable { name = "UserTranID1", expressionBefore = "lineVal.Split('$CsvSrcSep')[21]" },
                    },
                    processingCondition = "\",Sell,Buy,Withdraw,Deposit,AccountingRelated,GrossAmountDividend,ForwardFX,SpotFX,Repo,ReverseRepo,\".IndexOf(\",$TransactionType,\") >= 0 && \",211,212MASTER,213,214,215,216,217,218,219MASTER,220,221,222MASTER,223,224,225,226,227,228,229,230,236,237,300MASTER,301MASTER,302MASTER,303MASTER,315MASTER,611,612MASTER,614,617,626,627,628,629,631,642,646,647,648,650,651,652,653,654,656,657,658,659,660,661,662,664,677,682,683,684,685,686,687,688,689,690,691,692,693,694,695,696,697,698,699,700,701,703,704,705,706,707,708,709,710,711,712,713,714,715,717,719,\".IndexOf(\",\" + lineVal.Split('$CsvSrcSep')[5] + \",\") >= 0 && !(\"$TransactionType\" == \"AccountingRelated\" && (\"$UserTranID1\".IndexOf(\"Accrual\") >= 0 || \"$UserTranID1\".IndexOf(\"Day\") >= 0 || \"$UserTranID1\".IndexOf(\"support\") >= 0)) && !(\"$TransactionType\" == \"GrossAmountDividend\" && \"$UserTranID1\".IndexOf(\"div_reinvestment\") >= 0)",
                    columns = new [] {
                        new PdtColumn { name = "Tran ID", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "ExternalRef",
                                    expression = "\"SS&C TRADE ID: \" + colVal"
                                },
                                new PdtColumnDest {
                                    path = "Info",
                                    expression = "\"MIL_Trade_RMA\""
                                },
                            }
                        },
                        new PdtColumn { name = "Trans Type", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "TradeType",
                                    expression = "\"$TradeType\""
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT ID FROM BO_KERNEL_EVENTS WHERE NAME = 'BBH Upload'\""
                                    },
                                    path = "EventId"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT IDENT FROM RISKUSERS WHERE NAME = 'BBHUploader'\""
                                    },
                                    path = "UserId"
                                },
                            }
                        },
                        new PdtColumn { name = "Trade Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "TradeDate",
                                    expression = "System.Text.RegularExpressions.Regex.IsMatch(colVal, @\"^\\d+$\") ? (new System.DateTime(1900, 1, 1).AddDays(int.Parse(colVal)-2)).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Settle Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "ValueDate",
                                    expression = "System.Text.RegularExpressions.Regex.IsMatch(colVal, @\"^\\d+$\") ? (new System.DateTime(1900, 1, 1).AddDays(int.Parse(colVal)-2)).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Actual Settle Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Portfolio", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT IDENT FROM (
SELECT IDENT, NAME FROM FOLIO F
    START WITH IDENT IN (SELECT MNEMO FROM TITRES WHERE MNEMO_V2 = '"" + colVal + @""')
CONNECT BY PRIOR IDENT = MGR)
WHERE NAME IN ('Investments', 'Investments MIFL')"""
                                    },
                                    path = "BookId"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT F.ENTITE FROM FOLIO F JOIN TITRES T ON T.MNEMO = F.IDENT AND T.MNEMO_V2 = '"" + colVal + ""'"""
                                    },
                                    path = "Entity"
                                },
                            }
                        },
                        new PdtColumn { name = "Strategy", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Investment", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"
""$TradeType"" == ""Cash Transfer"" ? ""SELECT SICOVAM FROM TITRES WHERE LIBELLE='Cash for currency ''$SettleCurrency'''"" :
"",ForwardFX,SpotFX,"".IndexOf("",$TransactionType,"") >= 0 ? @""
SELECT SICOVAM FROM TITRES WHERE TYPE = 'E' AND QUOTATION_TYPE = 1
    AND ((MARCHE=STR_TO_DEVISE('"" + colVal.Substring(0, 3) + ""') AND DEVISECTT=STR_TO_DEVISE('"" + ""$SettleCurrency"".Substring(0, 3) + ""')) OR (MARCHE=STR_TO_DEVISE('"" + ""$SettleCurrency"".Substring(0, 3) + ""') AND DEVISECTT=STR_TO_DEVISE('"" + colVal.Substring(0, 3) + ""')))"" : @""
SELECT SICOVAM FROM TITRES WHERE REFERENCE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM
FROM EXTRNL_REFERENCES_DEFINITION ERD
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
    JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT
WHERE ERD.REF_NAME = 'ISIN' AND ERI.VALUE = '"" + colVal + ""'""" },
                                    path = "InstrumentRef"
                                },
                                new PdtColumnDest { path = "Isin"}
                            }
                        },
                        new PdtColumn { name = "Quantity", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "Quantity",
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT CASE WHEN '$TransactionType' NOT IN ('Sell', 'Buy') THEN
        CASE WHEN '$TransactionType' IN ('ForwardFX', 'SpotFX') THEN CASE WHEN '$SettleCurrency'='EUR.F' THEN -1 * $SettleNetAmount ELSE TO_NUMBER(COALESCE('"" + colVal + @""', '0')) END
        ELSE (CASE WHEN '$TransactionType' IN ('Deposit', 'GrossAmountDividend', 'ReverseRepo') THEN -1 ELSE 1 END) * $SettleNetAmount END
    ELSE (CASE WHEN '$TransactionType' IN ('Sell') THEN -1 ELSE 1 END) * "" + (colVal=="""" ? ""0"" : colVal) + @"" /
        CASE WHEN NOT EXISTS (
            SELECT * FROM EXTRNL_REFERENCES_DEFINITION ERD
                JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
                JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT 
                    AND T.AFFECTATION IN ('1900', '1660','1420','13','12','1204','1560')
            WHERE ERD.REF_NAME = 'ISIN'
                AND ERI.VALUE = '$Investment')
                AND NOT EXISTS (SELECT * FROM TITRES WHERE REFERENCE = '$Investment'
                    AND AFFECTATION IN ('1900', '1660','1420','13','12','1204','1560')) THEN 1 ELSE
        (SELECT NOMINAL FROM TITRES WHERE REFERENCE = '$Investment'
        UNION
        SELECT T.NOMINAL FROM EXTRNL_REFERENCES_DEFINITION ERD
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
            JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT 
        WHERE ERD.REF_NAME = 'ISIN'
            AND ERI.VALUE = '$Investment') END END QUANTITY
FROM DUAL"""
                                },
                            } }
                        },
                        new PdtColumn { name = "Settle Net Amount", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Amount",
                                    expression = @""",ForwardFX,SpotFX,"".IndexOf("",$TransactionType,"") >= 0 && ""$SettleCurrency""==""EUR.F"" ? ""-$Quantity"" : ("",Buy,ForwardFX,SpotFX,Withdraw,AccountingRelated,Repo,"".IndexOf("",$TransactionType,"") >= 0 ? """" : ""-"") + colVal"
                                }
                            }
                        },
                        new PdtColumn { name = "Unit Price", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Spot",
                                    expression = @""",Sell,Buy,ForwardFX,SpotFX,"".IndexOf("",$TransactionType,"") >= 0 ? double.Parse(colVal) : 1"
                                },
                                new PdtColumnDest {
                                    path = "SpotType",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT CASE WHEN '$TransactionType' NOT IN ('Sell', 'Buy') THEN 0
    WHEN NOT EXISTS (
        SELECT * FROM EXTRNL_REFERENCES_DEFINITION ERD
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
            JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT 
                AND T.AFFECTATION IN ('1900', '1660','1420','13','12','1204','1540','1560')
        WHERE ERD.REF_NAME = 'ISIN'
            AND ERI.VALUE = '$Investment')
            AND NOT EXISTS (SELECT * FROM TITRES WHERE REFERENCE = '$Investment'
                AND AFFECTATION IN ('1900', '1660','1420','13','12','1204','1540','1560')) THEN 0
    ELSE (SELECT NVL(QUOTATION_TYPE, 0) FROM TITRES WHERE REFERENCE = '$Investment'
        UNION
        SELECT NVL(T.QUOTATION_TYPE, 0)
        FROM EXTRNL_REFERENCES_DEFINITION ERD
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
            JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT 
        WHERE ERD.REF_NAME = 'ISIN'
            AND ERI.VALUE = '$Investment') END SPOT_TYPE
FROM DUAL"""
                                    },
                                }
                            }
                        },
                        new PdtColumn { name = "Settle Currency", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Currency",
                                    expression = "colVal.Substring(0, 3)"
                                }
                            }
                        },
                        new PdtColumn { name = "Book Amount", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "FX Rate", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Broker", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Comments",
                                    expression = "\",Sell,Buy,\".IndexOf(\",$TransactionType,\") >= 0 ? \"$TransactionType_MIL TA: \" + colVal : \"$TransactionType\""
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT CODE FROM TIERSPROPERTIES WHERE NAME = 'MIL TA' AND VALUE = '\" + colVal + \"'\"",
                                    },
                                    path = "CounterpartyId"
                                }
                            }
                        },
                        new PdtColumn { name = "Custodian", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT IDENT FROM TIERS WHERE NAME = '\" + colVal + \"'\"",
                                    },
                                    path = "DepositaryId"
                                }
                            }
                        },
                        new PdtColumn { name = "Cust Account", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Currency", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Accrued Interest", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Input Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Trans Desc", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "User Tran ID 1", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "User Tran ID 2", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Fund Structure", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Cash Source", isRequired = true, isRelativeToRootNode = true},
                    }
                },
                new PdtTransformation
                {
                    name = TransName.MIL_Trade_RMA_Alert.ToString(),
                    type = TransType.Excel2Csv,
                    label = "MIL Trade RMA Preparation Alert",
                    templateFile = "SSC_Transactions_template.csv",
                    category = "Medio",
                    csvSrcSeparator = DEFAULT_CSV_SEPARATOR,
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    csvSkipLines = 1,
                    UseHeaderColumnNames = true,
                    variables = new[] {
                        new PdtVariable {
                            name = "TransactionType",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[1]",
                        },
                        new PdtVariable {
                            name = "UserTranID1",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[21]",
                        },
                    },
                    processingCondition = "!(\",Sell,Buy,Withdraw,Deposit,AccountingRelated,GrossAmountDividend,ForwardFX,SpotFX,Repo,ReverseRepo,\".IndexOf(\",$TransactionType,\") >= 0 && \",211,212MASTER,213,214,215,216,217,218,219MASTER,220,221,222MASTER,223,224,225,226,227,228,229,230,236,237,300MASTER,301MASTER,302MASTER,303MASTER,315MASTER,611,612MASTER,614,617,626,627,628,629,631,642,646,647,648,650,651,652,653,654,656,657,658,659,660,661,662,664,677,682,683,684,685,686,687,688,689,690,691,692,693,694,695,696,697,698,699,700,701,703,704,705,706,707,708,709,710,711,712,713,714,715,717,719,\".IndexOf(\",\" + lineVal.Split('$CsvSrcSep')[5] + \",\") >= 0 && !(\"$TransactionType\" == \"AccountingRelated\" && (\"$UserTranID1\".IndexOf(\"Accrual\") >= 0 || \"$UserTranID1\".IndexOf(\"Day\") >= 0 || \"$UserTranID1\".IndexOf(\"support\") >= 0)) && !(\"$TransactionType\" == \"GrossAmountDividend\" && \"$UserTranID1\".IndexOf(\"div_reinvestment\") >= 0))",
                    columns = new []
                    {
                        new PdtColumn { name = "Tran ID", destPaths = new [] { new PdtColumnDest { path = "Tran ID" } } },
                        new PdtColumn { name = "Trans Type", destPaths = new [] { new PdtColumnDest { path = "Trans Type" } } },
                        new PdtColumn { name = "Trade Date", destPaths = new [] { new PdtColumnDest { path = "Trade Date" } } },
                        new PdtColumn { name = "Settle Date", destPaths = new [] { new PdtColumnDest { path = "Settle Date" } } },
                        new PdtColumn { name = "Actual Settle Date", destPaths = new [] { new PdtColumnDest { path = "Actual Settle Date" } } },
                        new PdtColumn { name = "Portfolio", destPaths = new [] { new PdtColumnDest { path = "Portfolio" } } },
                        new PdtColumn { name = "Strategy", destPaths = new [] { new PdtColumnDest { path = "Strategy" } } },
                        new PdtColumn { name = "Investment", destPaths = new [] { new PdtColumnDest { path = "Investment" } } },
                        new PdtColumn { name = "Quantity", destPaths = new [] { new PdtColumnDest { path = "Quantity" } } },
                        new PdtColumn { name = "Settle Net Amount", destPaths = new [] { new PdtColumnDest { path = "Settle Net Amount" } } },
                        new PdtColumn { name = "Unit Price", destPaths = new [] { new PdtColumnDest { path = "Unit Price" } } },
                        new PdtColumn { name = "Settle Currency", destPaths = new [] { new PdtColumnDest { path = "Settle Currency" } } },
                        new PdtColumn { name = "Book Amount", destPaths = new [] { new PdtColumnDest { path = "Book Amount" } } },
                        new PdtColumn { name = "FX Rate", destPaths = new [] { new PdtColumnDest { path = "FX Rate" } } },
                        new PdtColumn { name = "Broker", destPaths = new [] { new PdtColumnDest { path = "Broker" } } },
                        new PdtColumn { name = "Custodian", destPaths = new [] { new PdtColumnDest { path = "Custodian" } } },
                        new PdtColumn { name = "Cust Account", destPaths = new [] { new PdtColumnDest { path = "Cust Account" } } },
                        new PdtColumn { name = "Currency", destPaths = new [] { new PdtColumnDest { path = "Currency" } } },
                        new PdtColumn { name = "Accrued Interest", destPaths = new [] { new PdtColumnDest { path = "Accrued Interest" } } },
                        new PdtColumn { name = "Input Date", destPaths = new [] { new PdtColumnDest { path = "Input Date" } } },
                        new PdtColumn { name = "Trans Desc", destPaths = new [] { new PdtColumnDest { path = "Trans Desc" } } },
                        new PdtColumn { name = " User Tran ID 1", destPaths = new [] { new PdtColumnDest { path = " User Tran ID 1" } } },
                        new PdtColumn { name = " User Tran ID 2", destPaths = new [] { new PdtColumnDest { path = " User Tran ID 2" } } },
                        new PdtColumn { name = " Fund Structure", destPaths = new [] { new PdtColumnDest { path = " Fund Structure" } } },
                        new PdtColumn { name = " Cash Source", destPaths = new [] { new PdtColumnDest { path = " Cash Source" } } },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.MIL_Cash_RMA.ToString(),
                    type = TransType.Csv2Csv,
                    label = "MIL Cash RMA Preparation",
                    templateFile = "RMA_GenericTrade.csv",
                    category = "Medio",
                    csvSrcSeparator = ',',
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable {
                            name = "TransactionType",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[15]",
                        },
                    },
                    processingCondition = "\"$TransactionType\" == \"Withdraw\" || \"$TransactionType\" == \"Deposit\"",
                    columns = new [] {
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Currency",
                                    expression = "colVal == \"European Euro\" ? \"EUR\" : colVal"
                                },
                                new PdtColumnDest {
                                    path = "Info",
                                    expression = "\"MIL_Cash_RMA\""
                                },
                            }
                        },
                        new PdtColumn { name = "Group1", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "CurrBegBalLocal", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "CurrEndBalLocal", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "CurrBegBalBook", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "CurrEndBalBook", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Group1BegBalLocal", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Group1EndBalLocal", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Group1BegBalBook", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Group1EndBalBook", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Acct Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "TradeDate",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"M/d/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Settle Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "ValueDate",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"M/d/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Tran ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "ExternalRef" } }
                        },
                        new PdtColumn { name = "Internal ID", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn {
                            name = "Tran Description",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "TradeType",
                                    expression = "\"Cash Transfer\""
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT ID FROM BO_KERNEL_EVENTS WHERE NAME = 'BBH Upload'\""
                                    },
                                    path = "EventId"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT IDENT FROM RISKUSERS WHERE NAME = 'BBHUploader'\""
                                    },
                                    path = "UserId"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Investment",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT SICOVAM FROM TITRES WHERE REFERENCE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM
FROM EXTRNL_REFERENCES_DEFINITION ERD
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
    JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT
WHERE ERD.REF_NAME = 'ISIN' AND ERI.VALUE = '"" + colVal + ""'"""
                                },
                                path = "InstrumentRef"
                            } }
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "Quantity",
                                expression = "-1 * double.Parse(colVal)"
                            } }
                        },
                        new PdtColumn {
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Spot",
                                    expression = "1"
                                },
                                new PdtColumnDest {
                                    path = "SpotType",
                                    expression = "0"
                                }
                            }
                        },
                        new PdtColumn { name = "Net Local", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Net Book", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Local Balance", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Book Balance", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "TotalCashBook", isRequired = true, isRelativeToRootNode = true},
                    }
                },
                new PdtTransformation
                {
                    name = TransName.DIM_OTC_Trade_RMA.ToString(),
                    type = TransType.Csv2Csv,
                    label = "DIM OTC Trade RMA Preparation",
                    templateFile = "RMA_GenericTrade.csv",
                    category = "Medio",
                    csvSrcSeparator = ',',
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    csvSkipLines = 1,
                    UseHeaderColumnNames = true,
                    variables = new [] {
                        new PdtVariable { name = "Sicovam", expressionBefore = "lineVal.Split('$CsvSrcSep')[3]", Lookup = new PdtColumnLookup { Table = "DIM_OTC_TRADE_INSTRUMENT", ColumnIndex = "0" } },
                    },
                    processingCondition = @"lineVal.Split('$CsvSrcSep')[0] == ""V"" && "",INTRTSWP,FXOPTIONS,"".IndexOf(lineVal.Split('$CsvSrcSep')[9]) >= 0 && ""$Sicovam"" != """"",
                    columns = new [] {
                        new PdtColumn { name = "TRADE REFERENCE", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "InstrumentRef", expression = @"""$Sicovam""" } }
                        },
                        new PdtColumn { name = "Trade Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "TradeDate",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "SETTLEMENT DATE", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "ValueDate",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Trade Sequence Number", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "ExternalRef" },
                                new PdtColumnDest { path = "TradeType", expression = "\"Purchase/Sale\"" },
                                new PdtColumnDest { path = "Info", expression = "\"DIM_OTC_Trade_RMA\"" },
                                //new PdtColumnDest { path = "EventId", 
                                //    Lookup = new PdtColumnLookup { Table = "SQL",
                                //        Expression = "\"SELECT ID FROM BO_KERNEL_EVENTS WHERE NAME = 'BBH Upload'\""
                                //    },
                                //},
                                new PdtColumnDest { path = "UserId", 
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT IDENT FROM RISKUSERS WHERE NAME = 'BBHUploader'\""
                                    },
                                },
                            }
                        },
                        new PdtColumn { name = "Quantity", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "Quantity" },
                                new PdtColumnDest { path = "Spot", expression = "0" },
                                new PdtColumnDest { path = "SpotType", expression = "2" }
                            }
                        },
                        new PdtColumn { name = "Account ID", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "BookId",
                                    Lookup = new PdtColumnLookup { Table = "SQL",
                                        Expression = @"""SELECT ACCOUNT_LEVEL_FOLIO FROM BO_TREASURY_ACCOUNT WHERE ID IN (SELECT ACC_ID FROM BO_TREASURY_EXT_REF WHERE VALUE = '"" + colVal + ""')"""
                                    }
                                },
                                new PdtColumnDest { path = "DepositaryId",
                                    Lookup = new PdtColumnLookup { Table = "SQL",
                                        Expression = @"""SELECT CUSTODIAN FROM BO_TREASURY_ACCOUNT WHERE ID IN (SELECT ACC_ID FROM BO_TREASURY_EXT_REF WHERE VALUE = '"" + colVal + ""')"""
                                    }
                                },
                                new PdtColumnDest { path = "Entity",
                                    Lookup = new PdtColumnLookup { Table = "SQL",
                                        Expression = @"""SELECT ENTITY FROM BO_TREASURY_ACCOUNT WHERE ID IN (SELECT ACC_ID FROM BO_TREASURY_EXT_REF WHERE VALUE = '"" + colVal + ""')"""
                                    }
                                },
                            }
                        },
                        new PdtColumn { name = "Ccy", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "Currency,PaymentCcy" } }
                        },
                        new PdtColumn { name = "FEE/PREMIUM AMOUNT", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "CounterPartyFees" } }
                        },
                        new PdtColumn { name = "Counterparty", isRequired = true, isRelativeToRootNode = true},
                    }
                },
                new PdtTransformation
                {
                    name = TransName.DIM_OTC_IRS.ToString(),
                    type = TransType.Csv2Xml,
                    label = "DIM OTC IRS",
                    templateFile = "IRS.xml",
                    category = "Medio",
                    fileBreakExpression = @"lineVal.Split(',')[3]", //TRADE REFERENCE
                    //repeatingRootPath = "//*[local-name() = 'import']",
                    //repeatingChildrenPath = "//*[local-name() = 'swap']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,
                    UseHeaderColumnNames = true,
                    variables = new[] {
                        new PdtVariable { name = "MaturityDate", expressionBefore = "lineVal.Split('$CsvSrcSep')[18]"},
                        new PdtVariable { name = "FixedRate", expressionBefore = "lineVal.Split('$CsvSrcSep')[19]"},
                        new PdtVariable { name = "ReceiveLegType", expressionBefore = "lineVal.Split('$CsvSrcSep')[123]"},
                        new PdtVariable { name = "PayLegType", expressionBefore = "lineVal.Split('$CsvSrcSep')[124]"},
                    },
                    processingCondition = @"lineVal.Split('$CsvSrcSep')[9] == ""INTRTSWP""",
                    postProcessEvent = @"
        var nodePaths = new List<string>();
        if (""$PayLegType"" == ""FLOAT"") { //Paying Template
            nodePaths.Add(""//*[local-name() = 'receivingLeg']//*[local-name() = 'resetDates']"");
            nodePaths.Add(""//*[local-name() = 'receivingLeg']//*[local-name() = 'paymentDates']"");
            nodePaths.Add(""//*[local-name() = 'receivingLeg']//*[local-name() = 'floatingRateCalculation']"");
            nodePaths.Add(""//*[local-name() = 'payingLeg']//*[local-name() = 'fixedRateSchedule']"");
        }
        if (""$ReceiveLegType"" == ""FLOAT"") { //Receiving Template
            nodePaths.Add(""//*[local-name() = 'payingLeg']//*[local-name() = 'paymentDates']"");
            nodePaths.Add(""//*[local-name() = 'payingLeg']//*[local-name() = 'floatingRateCalculation']"");
            nodePaths.Add(""//*[local-name() = 'receivingLeg']//*[local-name() = 'fixedRateSchedule']"");
        }
        foreach(string path in nodePaths) {
            var nodes = doc.SelectNodes(path);
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes.Item(i);
                node.ParentNode.RemoveChild(node);
            }
        }",
                    columns = new [] {
                        new PdtColumn { name = "Ccy", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'swap']/*[local-name() = 'currency'] | //*[local-name() = 'swap']/*[local-name() = 'notional']/*[local-name() = 'currency']" },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Sophisref'] | //*[local-name() = 'name']",
                                    //path = "//*[local-name() = 'swap']/*[local-name() = 'identifier']/*[local-name() = 'reference'] | //*[local-name() = 'name']",
                                    expression = @"""IRS "" + (double.Parse(""$FixedRate"")*100).ToString() + "" "" + System.DateTime.ParseExact(""$MaturityDate"", ""dd/MM/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""dd.MM.yy"") + "" "" + colVal"
                                }
                            }
                        },
                        new PdtColumn { name = "Underlying ID", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'payingLeg']//*[local-name() = 'identifier']/*[local-name() = 'reference']",
                                    expression = "colVal + \" Index\""
                                }
                            }
                        },
                        new PdtColumn { name = "Effective Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'effectiveDate']//*[local-name() = 'unadjustedDate']",
                                    expression = @"System.DateTime.ParseExact(colVal, ""dd/MM/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                }
                            }
                        },
                        new PdtColumn { name = "Maturity Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'terminationDate']//*[local-name() = 'unadjustedDate']",
                                    expression = @"System.DateTime.ParseExact(colVal, ""dd/MM/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                }
                            }
                        },
                        new PdtColumn { name = "Fixed Rate", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'fixedRateSchedule']/*[local-name() = 'initialValue']" } }
                        },
                        new PdtColumn { name = "SETTLEMENT CURRENCY ", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'deliveryCurrency']" } }
                        },
                        new PdtColumn { name = "Pay Leg CCY", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'payingLeg']//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn { name = "Receive Leg CCY", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn { name = "Paying Payment Freq", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'payingLeg']//*[local-name() = 'calculationPeriodFrequency']/*[local-name() = 'periodEnum']",
                                    expression = @""",Quarterly,Half Yearly,Monthly,"".IndexOf("","" + colVal + "","") >= 0 ? ""Month"" : colVal==""Weekly"" ? ""Week"" : (colVal==""Daily"" || colVal.StartsWith(""Every"")) ? ""Day"" : colVal==""Yearly"" ? ""Year"" : colVal==""Straight"" ? ""Term"" : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'payingLeg']//*[local-name() = 'calculationPeriodFrequency']/*[local-name() = 'periodMultiplier']",
                                    expression = @""",Yearly,Monthly,Weekly,Daily,"".IndexOf("","" + colVal + "","") >= 0 ? ""1"" : colVal==""Quarterly"" ? ""3"" : colVal==""Half Yearly"" ? ""6"" : colVal==""Straight"" ? ""0"" : colVal.StartsWith(""Every"") ? colVal.Substring(6, colVal.IndexOf(' ', 6) - 6) : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'indexTenor']/*[local-name() = 'periodEnum']",
                                    expression = @"""$PayLegType""!=""FLOAT"" ? """" : "",Quarterly,Half Yearly,Monthly,"".IndexOf("","" + colVal + "","") >= 0 ? ""Month"" : colVal==""Weekly"" ? ""Week"" : (colVal==""Daily"" || colVal.StartsWith(""Every"")) ? ""Day"" : colVal==""Yearly"" ? ""Year"" : colVal==""Straight"" ? ""Term"" : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'indexTenor']/*[local-name() = 'periodMultiplier']",
                                    expression = @"""$PayLegType""!=""FLOAT"" ? """" : "",Yearly,Monthly,Weekly,Daily,"".IndexOf("","" + colVal + "","") >= 0 ? ""1"" : colVal==""Quarterly"" ? ""3"" : colVal==""Half Yearly"" ? ""6"" : colVal==""Straight"" ? ""0"" : colVal.StartsWith(""Every"") ? colVal.Substring(6, colVal.IndexOf(' ', 6) - 6) : """""
                                },
                            }
                        },
                        new PdtColumn { name = "Receiving Payment Freq", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'calculationPeriodFrequency']/*[local-name() = 'periodEnum']",
                                    expression = @""",Quarterly,Half Yearly,Monthly,"".IndexOf("","" + colVal + "","") >= 0 ? ""Month"" : colVal==""Weekly"" ? ""Week"" : (colVal==""Daily"" || colVal.StartsWith(""Every"")) ? ""Day"" : colVal==""Yearly"" ? ""Year"" : colVal==""Straight"" ? ""Term"" : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'calculationPeriodFrequency']/*[local-name() = 'periodMultiplier']",
                                    expression = @""",Yearly,Monthly,Weekly,Daily,"".IndexOf("","" + colVal + "","") >= 0 ? ""1"" : colVal==""Quarterly"" ? ""3"" : colVal==""Half Yearly"" ? ""6"" : colVal==""Straight"" ? ""0"" : colVal.StartsWith(""Every"") ? colVal.Substring(6, colVal.IndexOf(' ', 6) - 6) : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'indexTenor']/*[local-name() = 'periodEnum']",
                                    expression = @"""$ReceiveLegType""!=""FLOAT"" ? """" : "",Quarterly,Half Yearly,Monthly,"".IndexOf("","" + colVal + "","") >= 0 ? ""Month"" : colVal==""Weekly"" ? ""Week"" : (colVal==""Daily"" || colVal.StartsWith(""Every"")) ? ""Day"" : colVal==""Yearly"" ? ""Year"" : colVal==""Straight"" ? ""Term"" : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'indexTenor']/*[local-name() = 'periodMultiplier']",
                                    expression = @"""$ReceiveLegType""!=""FLOAT"" ? """" : "",Yearly,Monthly,Weekly,Daily,"".IndexOf("","" + colVal + "","") >= 0 ? ""1"" : colVal==""Quarterly"" ? ""3"" : colVal==""Half Yearly"" ? ""6"" : colVal==""Straight"" ? ""0"" : colVal.StartsWith(""Every"") ? colVal.Substring(6, colVal.IndexOf(' ', 6) - 6) : """""
                                },
                            }
                        },
                        new PdtColumn { name = "Paying Basis", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'payingLeg']//*[local-name() = 'dayCountFraction']",
                                    expression = @"colVal==""ACT/365"" ? ""ACT/365 FIXED"" : colVal==""ACT/360"" ? ""ACT/360"" : ""unknown"""
                                }
                            }
                        },
                        new PdtColumn { name = "Receiving Basis", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'dayCountFraction']",
                                    expression = @"colVal==""ACT/365"" ? ""ACT/365 FIXED"" : colVal==""ACT/360"" ? ""ACT/360"" : ""unknown"""
                                }
                            }
                        },
                        new PdtColumn { name = "Paying Roll Convention", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'payingLeg']//*[local-name() = 'calculationPeriodDatesAdjustments']/*[local-name() = 'businessDayConvention']",
                                    expression = @"colVal==""MODIFIED FOLLOWING"" ? ""MODFOLLOWING"" : colVal==""FOLLOWING"" ? ""FOLLOWING"" : ""unknown"""
                                }
                            }
                        },
                        new PdtColumn { name = "Receiving Roll Convention", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'receivingLeg']//*[local-name() = 'calculationPeriodDatesAdjustments']/*[local-name() = 'businessDayConvention']",
                                    expression = @"colVal==""MODIFIED FOLLOWING"" ? ""MODFOLLOWING"" : colVal==""FOLLOWING"" ? ""FOLLOWING"" : ""unknown"""
                                }
                            }
                        },
                        new PdtColumn { name = "CCP Flag", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'isCleared']",
                                    expression = @"colVal==""CCP"" ? ""true"" : """""
                                }
                            }
                        },
                        new PdtColumn { name = "Underlying Ticker", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.SICOVAM FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM
    JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
        AND ERD.REF_NAME IN ('BBG_B3_ID', 'CUSIP', 'TICKER', 'ID_BB_UNIQUE')
WHERE ERI.VALUE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM FROM TITRES T WHERE T.REFERENCE = '"" + colVal + "" Index'"""
                                    },
                                    path = "//*[local-name() = 'sophis']",
                                }
                            }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.DIM_OTC_FXOPTIONS.ToString(),
                    type = TransType.Csv2Xml,
                    label = "DIM OTC FXOptions",
                    templateFile = "FxOptions.xml",
                    category = "Medio",
                    fileBreakExpression = @"lineVal.Split(',')[3]", //TRADE REFERENCE
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,
                    UseHeaderColumnNames = true,
                    variables = new[] {
                        new PdtVariable { name = "MaturityDate", expressionBefore = "lineVal.Split('$CsvSrcSep')[18]"},
                        new PdtVariable { name = "Strike", expressionBefore = "lineVal.Split('$CsvSrcSep')[20]"},
                        new PdtVariable { name = "OptionType", expressionBefore = "lineVal.Split('$CsvSrcSep')[23]"},
                        new PdtVariable { name = "ExerciseType", expressionBefore = "lineVal.Split('$CsvSrcSep')[76]"},
                        new PdtVariable { name = "CCYPair", expressionBefore = "lineVal.Split('$CsvSrcSep')[116]"},
                    },
                    processingCondition = @"lineVal.Split('$CsvSrcSep')[9] == ""FXOPTIONS""",
                    postProcessEvent = @"
        var nodePaths = new List<string>();
        if (""$ExerciseType"" == ""A"") { //American
            nodePaths.Add(""//*[local-name() = 'bermudaExercise']"");
            nodePaths.Add(""//*[local-name() = 'europeanExercise']"");
        } else if (""$ExerciseType"" == ""B"") { //Bermudan
            nodePaths.Add(""//*[local-name() = 'americanExercise']"");
            nodePaths.Add(""//*[local-name() = 'europeanExercise']"");
        } else if (""$ExerciseType"" == ""E"") { //European
            nodePaths.Add(""//*[local-name() = 'americanExercise']"");
            nodePaths.Add(""//*[local-name() = 'bermudaExercise']"");
        }
        foreach(XmlNode node in doc.SelectNodes(""//*[local-name() = 'isCleared'][.='False']""))
        {
            node.ParentNode.RemoveChild(node);
        }
        foreach(string path in nodePaths) {
            var nodes = doc.SelectNodes(path);
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes.Item(i);
                node.ParentNode.RemoveChild(node);
            }
        }",
                    columns = new [] {
                        new PdtColumn { name = "Maturity Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'expirationDate']//*[local-name() = 'unadjustedDate']",
                                    expression = @"System.DateTime.ParseExact(colVal, ""dd/MM/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                }
                            }
                        },
                        new PdtColumn { name = "Strike", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'strike']//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn { name = "Option Type", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'optionType']", expression = @"colVal == ""PUT"" ? ""Put"" : colVal" } }
                        },
                        new PdtColumn { name = "Settle Mode", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'settlementType']", expression = @"colVal == ""PHYSICAL"" ? ""Currency"" : colVal == ""Cash"" ? ""Cash"" : ""Unknown""" } }
                        },
                        new PdtColumn { name = "Expiry Payment Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'settlementDate']//*[local-name() = 'unadjustedDate']",
                                    expression = @"System.DateTime.ParseExact(colVal, ""dd/MM/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                }
                            }
                        },
                        new PdtColumn { name = "Exercise Type", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'feature']",
                                    expression = @"colVal==""A"" ? ""Average-No Average|Barrier-None|Option Style-American|Vanilla|Without Market"" : colVal==""B"" ? ""Average-No Average|Barrier-None|Option Style-Bermudan|Vanilla|Without Market"" : colVal==""E"" ? ""Average-No Average|Barrier-None|Option Style-European|Vanilla|Without Market"" : ""Unknown"""
                                }
                            }
                        },
                        new PdtColumn { name = "CCY Pair", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'fxOption']/*[local-name() = 'currency'] | //*[local-name() = 'FixedNotionalCurrency']",
                                    expression = "colVal.Substring(0, 3)"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'notional']//*[local-name() = 'currency'] | //*[local-name() = 'strike']//*[local-name() = 'currency'] | //*[local-name() = 'exercise']//*[local-name() = 'settlementCurrency']",
                                    expression = "colVal.Substring(4, 3)"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Sophisref'] | //*[local-name() = 'name']",
                                    expression = @"colVal.Substring(0, 3) + ""$OptionType"".Substring(0, 1) + "" "" + colVal.Substring(4, 3) + (""$OptionType"" == ""CALL"" ? ""P"" : ""C"") + "" $Strike "" + System.DateTime.ParseExact(""$MaturityDate"", ""dd/MM/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""dd.MM.yy"")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'sophis']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"""SELECT SICOVAM FROM TITRES WHERE LIBELLE = '"" + colVal.Substring(0, 3) + "" versus "" + colVal.Substring(4, 3) + ""' AND TYPE = 'E'""",
                                    }
                                },
                            }
                        },
                        new PdtColumn { name = "CCP Flag", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'isCleared']", expression = @"colVal != """"" }
                            }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.REFI_CA_ISIN_CREATION.ToString(),
                    type = TransType.Csv2Csv,
                    label = "Refinitiv Corporate Action Isin Creation",
                    templateFile = "GenericList1.txt",
                    category = "Medio",
                    csvSkipLines = 1,
                    csvSrcSeparator = ',',
                    UseHeaderColumnNames = true,
                    variables = new[] {
                        new PdtVariable { name = "CAType", expressionBefore = "lineVal.Split(';')[4]"},
                        new PdtVariable { name = "CapType", expressionBefore = "lineVal.Split(';')[42]"},
                        new PdtVariable { name = "ExchangeCode", expressionBefore = "lineVal.Split(';')[49]"},
                        new PdtVariable { name = "XmlType", expressionBefore = @"
""$CAType"" == ""CAP"" ?
    (""$CapType"" ==  ""scrip issue in same stock"" ? ""SCRIP in Same Stock"" :
    ""$CapType"" ==  ""Non-renounceable scrip issue in same stock"" ? ""Non-renounceable SCRIP in same stock"" :
    ""$CapType"" ==  ""scrip issue in different stock"" ? ""SCRIP in Different Stock"" :
    ""$CapType"" ==  ""Non-renounceable scrip issue in different stock"" ? ""Non-renounceable SCRIP in Diff stock"" :
    ""$CapType"" ==  ""Return of capital"" ? ""Standard Return of Capital"" :
    ""$CapType"" ==  ""Stock split"" ? ""Stock Split"" :
    ""$CapType"" ==  ""Stock consolidation"" ? ""Reverse Stock Split"" :
    ""$CapType"" ==  ""Capital Reduction (CRD)"" ? ""Reverse Stock Split"" : //Standard Share Capital Consolidation
    ""$CapType"" ==  ""Demerger (DEM)"" ? ""Standard Spin-Off"" : ""Unknown"") : ""Unknown"""},
                        new PdtVariable { name = "Sicovam", expressionBefore = "lineVal.Split(';')[21]",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT T.SICOVAM
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND INSTR(':' || ERI_REFI.VALUE || ':', ':$ExchangeCode:') > 0
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MEDIO_POSITION_QUANTITY(MVTIDENT, SYSDATE) > 0)"""
                            },
                        }
                    },
                    processingCondition = @"
""@SCRIP in Different Stock@Non-renounceable SCRIP in Diff stock@Standard Spin-Off@"".IndexOf(""@$XmlType@"") >= 0 && ""$CAType"" == ""CAP"" && ""$Sicovam"" == """"",
                    columns = new []
                    {
                        new PdtColumn { name = "Capital Change New Security ISIN", destPaths = new [] { new PdtColumnDest { path = "Item1" } } },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.REFI_CA.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Refinitiv Corporate Action",
                    templateFile = "REFI_CA.xml",
                    category = "Medio",
                    fileBreakExpression = @"lineVal.Split(';')[0]+""_$XmlType_$CorporateActionsID""",
                    //repeatingRootPath = "//*[local-name() = 'corporateActionList']",
                    //repeatingChildrenPath = "//*[local-name() = 'corporateAction']",
                    csvSkipLines = 1,
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable { name = "CAType", expressionBefore = "lineVal.Split(';')[4]"},
                        new PdtVariable { name = "DivType", expressionBefore = "lineVal.Split(';')[24]"},
                        //new PdtVariable { name = "BaseRate", expressionBefore = "lineVal.Split(';')[32]",
                        //    Lookup = new PdtColumnLookup { Table = "SQL", Expression = @"""SELECT COURSFIGE FROM DEVISEV2 WHERE CODE = STR_TO_DEVISE('"" + colVal + @""')"""},
                        //    expressionAfter = @"colVal == """" || colVal == ""0"" ? ""1"" : colVal",
                        //},
                        new PdtVariable { name = "CorporateActionsID", expressionBefore = "lineVal.Split(';')[41]"},
                        new PdtVariable { name = "CapType", expressionBefore = "lineVal.Split(';')[42]"},
                        new PdtVariable { name = "ExchangeCode", expressionBefore = "lineVal.Split(';')[49]"},
                        new PdtVariable { name = "XmlType", expressionBefore = @"
""$CAType"" == ""DIV"" ? (
    ""$DivType"" == ""Cash Dividend"" && lineVal.Split(';')[32] == ""JPY"" && lineVal.Split(';')[48]!=""Forecast"" ? ""Standard Dividend Japanese"" :
    ""$DivType"" == ""Cash Dividend"" && lineVal.Split(';')[12] != """" && lineVal.Split(';')[12] != lineVal.Split(';')[32] ? ""Standard Dividend In Another Currency"" :
    ""$DivType"" == ""Cash Dividend"" && lineVal.Split(';')[32] != ""JPY"" && lineVal.Split(';')[43]==""Mandatory"" ? ""Standard Dividend"" :
    //""$DivType"" == ""Cash with Stock Alternative"" ? ""Standard Dividend with option"" :
    ""$DivType"" == ""Stock Dividend"" ? ""Stock Dividend"" : ""Unknown"") :
    (""$CapType"" ==  ""scrip issue in same stock"" ? ""SCRIP in Same Stock"" :
    ""$CapType"" ==  ""Non-renounceable scrip issue in same stock"" ? ""Non-renounceable SCRIP in same stock"" :
    ""$CapType"" ==  ""scrip issue in different stock"" ? ""SCRIP in Different Stock"" :
    ""$CapType"" ==  ""Non-renounceable scrip issue in different stock"" ? ""Non-renounceable SCRIP in Diff stock"" :
    ""$CapType"" ==  ""Return of capital"" ? ""Standard Return of Capital"" :
    ""$CapType"" ==  ""Stock split"" ? ""Stock Split"" :
    ""$CapType"" ==  ""Stock consolidation"" ? ""Reverse Stock Split"" :
    ""$CapType"" ==  ""Capital Reduction (CRD)"" ? ""Reverse Stock Split"" : //Standard Share Capital Consolidation
    ""$CapType"" ==  ""Demerger (DEM)"" ? ""Standard Spin-Off"" :""Unknown"")"},
                        new PdtVariable {
                            name = "CAPSeen",
                            expressionBefore = @"""CAP_$CorporateActionsID""",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "-1" },
                        },
                        new PdtVariable {
                            name = "numerator",
                            expressionBefore = @"""CAP_$CorporateActionsID""",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "0" },
                            expressionAfter=@"""@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@Stock Split@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? (lineVal.Split(';')[18]=="""" ? ""1"" : lineVal.Split(';')[18]) : (colVal=="""" ? ""1"" : colVal)",
                        },
                        new PdtVariable {
                            name = "denominator",
                            expressionBefore = @"""CAP_$CorporateActionsID""",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "1" },
                            expressionAfter=@"""@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@Stock Split@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? (lineVal.Split(';')[17]=="""" ? ""1"" : lineVal.Split(';')[17]) : (colVal=="""" ? ""1"" : colVal)",
                        },
                        new PdtVariable {
                            name = "SubscriptionPeriodStartDate",
                            expressionBefore = @"""CAP_$CorporateActionsID""",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "2" },
                        },
                        new PdtVariable {
                            name = "SubscriptionPeriodEndDate",
                            expressionBefore = @"""CAP_$CorporateActionsID""",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "3" },
                        },
                        new PdtVariable {
                            name = "DividendRate",
                            expressionBefore = @"""DIV_$CorporateActionsID""",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "4" },
                        },
                        //new PdtVariable {
                        //    name = "EffectiveDate",
                        //    expressionBefore = @"""CAP_$CorporateActionsID""",
                        //    Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "5" },
                        //},
                        new PdtVariable {
                            name = "Sicovam1",
                            expressionBefore = "lineVal.Split(';')[0]",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT SICOVAM FROM
(SELECT T.SICOVAM, ROWNUM IDX
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND INSTR(':' || ERI_REFI.VALUE || ':', ':$ExchangeCode:') > 0
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MEDIO_POSITION_QUANTITY(MVTIDENT, SYSDATE) > 0)
) WHERE IDX = 1"""
                            },
                        },
                        new PdtVariable {
                            name = "Sicovam2",
                            expressionBefore = "lineVal.Split(';')[0]",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT SICOVAM FROM
(SELECT T.SICOVAM, ROWNUM IDX
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND INSTR(':' || ERI_REFI.VALUE || ':', ':$ExchangeCode:') > 0
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MEDIO_POSITION_QUANTITY(MVTIDENT, SYSDATE) > 0)
) WHERE IDX = 2"""
                            },
                        },
                        new PdtVariable {
                            name = "Sicovam3",
                            expressionBefore = "lineVal.Split(';')[0]",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT SICOVAM FROM
(SELECT T.SICOVAM, ROWNUM IDX
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND INSTR(':' || ERI_REFI.VALUE || ':', ':$ExchangeCode:') > 0
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MEDIO_POSITION_QUANTITY(MVTIDENT, SYSDATE) > 0)
) WHERE IDX = 3"""
                            },
                        },
                        new PdtVariable {
                            name = "Sicovam4",
                            expressionBefore = "lineVal.Split(';')[0]",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT SICOVAM FROM
(SELECT T.SICOVAM, ROWNUM IDX
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND INSTR(':' || ERI_REFI.VALUE || ':', ':$ExchangeCode:') > 0
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MEDIO_POSITION_QUANTITY(MVTIDENT, SYSDATE) > 0)
) WHERE IDX = 4"""
                            },
                        },
                        new PdtVariable {
                            name = "Sicovam5",
                            expressionBefore = "lineVal.Split(';')[0]",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT SICOVAM FROM
(SELECT T.SICOVAM, ROWNUM IDX
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND INSTR(':' || ERI_REFI.VALUE || ':', ':$ExchangeCode:') > 0
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MEDIO_POSITION_QUANTITY(MVTIDENT, SYSDATE) > 0)
) WHERE IDX = 5"""
                            },
                        },
                    },
                    processingCondition = @"
((""@Standard Dividend@"".IndexOf(""@$XmlType@"") >= 0 && ""$CAType"" == ""DIV"" && ""$CAPSeen""=="""") || // check numerator empty to make sure there's no CAP of the same CorporateActionsID
(""@Standard Dividend Japanese@Standard Dividend with option@Stock Dividend@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 && ""$CAType"" == ""DIV"") ||
(""@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@Standard Return of Capital@Stock Split@Standard Renaming@Reverse Stock Split@Standard Bonus Right Issue@Standard Spin-Off@"".IndexOf(""@$XmlType@"") >= 0 && ""$CAType"" == ""CAP""))",
                    postProcessEvent = @"
        var nodePaths = new List<string>();
        if (""$XmlType"" == ""Standard Dividend"") {
            nodePaths.Add(""//*[local-name() = 'conversionRatio']"");
            nodePaths.Add(""//*[local-name() = 'announcementDate']"");
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'noImpactOnNetAmountIfRounding']"");
            nodePaths.Add(""//*[local-name() = 'instrumentUnavailable']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent1']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        if (""$XmlType"" == ""Standard Dividend In Another Currency"") {
            nodePaths.Add(""//*[local-name() = 'conversionRatio']"");
            nodePaths.Add(""//*[local-name() = 'announcementDate']"");
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        if (""$XmlType"" == ""Standard Dividend Japanese"") {
            nodePaths.Add(""//*[local-name() = 'conversionRatio']"");
            nodePaths.Add(""//*[local-name() = 'announcementDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        if (""$XmlType"" == ""Stock Dividend"") {
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'instrumentUnavailable']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
        }
        if (""@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@"".IndexOf(""@$XmlType@"") >= 0) {
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'instrumentUnavailable']"");
            if (""@SCRIP in Same Stock@Non-renounceable SCRIP in same stock@"".IndexOf(""@$XmlType@"") >= 0) nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'coefficient']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
        }
        if (""$XmlType"" == ""Standard Return of Capital"") {
            nodePaths.Add(""//*[local-name() = 'announcementDate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'noImpactOnNetAmountIfRounding']"");
            nodePaths.Add(""//*[local-name() = 'instrumentUnavailable']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'conversionRatio']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        if (""$XmlType"" == ""Stock Split"") {
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'coefficient']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'exdivDate']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
        }
        if (""$XmlType"" == ""Standard Spin-Off"") {
            nodePaths.Add(""//*[local-name() = 'announcementDate']"");
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'paymentDate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent1']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'coefficient']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        if (""$XmlType"" == ""Reverse Stock Split"") {
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'exdivDate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'coefficient']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
        }
        if (""$XmlType"" == ""Standard Dividend with option"") {
            nodePaths.Add(""//*[local-name() = 'announcementDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'noImpactOnNetAmountIfRounding']"");
            nodePaths.Add(""//*[local-name() = 'instrumentUnavailable']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent1']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'conversionRatio']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
        }
        { // remove part with empty sicovam
            var nodes = doc.SelectNodes(""//*[local-name() = 'reference'][.='']/../../../.."");
            for (int i = nodes.Count-1; i >= 0; i--)
            {
                var node = nodes.Item(i);
                node.ParentNode.RemoveChild(node);
            }
        }
        foreach(string path in nodePaths) {
            var nodes = doc.SelectNodes(path);
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes.Item(i);
                node.ParentNode.RemoveChild(node);
            }
        }",
                    columns = new []
                    {
                        new PdtColumn {
                            name = "ISIN",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'corporateActions'][1]/*[local-name() = 'identifier']/*[local-name() = 'sophis']",
                                    expression = @"""$Sicovam1"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'corporateActions'][2]/*[local-name() = 'identifier']/*[local-name() = 'sophis']",
                                    expression = @"""$Sicovam2"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'corporateActions'][3]/*[local-name() = 'identifier']/*[local-name() = 'sophis']",
                                    expression = @"""$Sicovam3"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'corporateActions'][4]/*[local-name() = 'identifier']/*[local-name() = 'sophis']",
                                    expression = @"""$Sicovam4"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'corporateActions'][5]/*[local-name() = 'identifier']/*[local-name() = 'sophis']",
                                    expression = @"""$Sicovam5"""
                                },
//                                new PdtColumnDest {
//                                    Lookup = new PdtColumnLookup {
//                                        File = "REFI_CA_ISO.csv",
//                                        Expression = @"lineVal.Split(';')[0].Trim('""')==""ISIN "" + colVal ? (
//lineVal.Split(';')[2].Trim('""')==""CHAN//NAME"" ? ""Name"" :
//lineVal.Split(';')[0]==lineVal.Split(';')[5] && lineVal.Split(';')[6].Trim('""').StartsWith(""/TS/"") && lineVal.Split(';')[7].Trim('""').StartsWith(""/TS/"") && lineVal.Split(';')[6]!=lineVal.Split(';')[7] ? ""TICKER"" :
//lineVal.Split(';')[0]!=lineVal.Split(';')[5] && lineVal.Split(';')[0].Trim('""').StartsWith(""ISIN"") && lineVal.Split(';')[5].Trim('""').StartsWith(""ISIN"") && lineVal.Split(';')[6]==lineVal.Split(';')[7] ? ""ISIN"" : ""Unknown"") : null",
//                                    },
//                                    path = "//*[local-name() = 'nameChange']",
//                                },
//                                new PdtColumnDest {
//                                    Lookup = new PdtColumnLookup {
//                                        File = "REFI_CA_ISO.csv",
//                                        Expression = @"lineVal.Split(';')[0].Trim('""')==""ISIN "" + colVal ? (
//lineVal.Split(';')[2].Trim('""')==""CHAN//NAME"" ? lineVal.Split(';')[1].Trim('""').Substring(6) :
//lineVal.Split(';')[0]==lineVal.Split(';')[5] && lineVal.Split(';')[6].Trim('""').StartsWith(""/TS/"") && lineVal.Split(';')[7].Trim('""').StartsWith(""/TS/"") && lineVal.Split(';')[6]!=lineVal.Split(';')[7] ? lineVal.Split(';')[7].Trim('""').Split('/')[2] :
//lineVal.Split(';')[0]!=lineVal.Split(';')[5] && lineVal.Split(';')[0].Trim('""').StartsWith(""ISIN"") && lineVal.Split(';')[5].Trim('""').StartsWith(""ISIN"") && lineVal.Split(';')[6]==lineVal.Split(';')[7] ? lineVal.Split(';')[5].Trim('""').Substring(5) : ""Unknown"") : null",
//                                    },
//                                    path = "//*[local-name() = 'newNameOfCompany']",
//                                },
                            }
                        },
                        new PdtColumn { name = "CUSIP", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "SEDOL", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Ticker", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Corporate Actions Type", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Ex Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'type']",
                                    expression = "\"$XmlType\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']//*[local-name() = 'ticketToGenerate']",
                                    expression = @"""Both"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'businessEvent1']",
                                    expression = @"
""@Standard Return of Capital@"".IndexOf(""@$XmlType@"") >= 0 ? ""Cash Adjustment CA"" :
""@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 ? ""Dividend"" :
""@Stock Split@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""Split"" :
""@SCRIP in Same Stock@SCRIP in Different Stock@Stock Dividend@"".IndexOf(""@$XmlType@"") >= 0 ? ""CA Security Adjustment"" :
""@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@"".IndexOf(""@$XmlType@"") >= 0 ? ""Free Shares"" :
""@Standard Dividend Japanese@"".IndexOf(""@$XmlType@"") >= 0 ? ""Dividend"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'businessEvent1']",
                                    expression = @"
""@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@Stock Dividend@Stock Split@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""Fractional Cash out"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent2']",
                                    expression = @"
""@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 ? ""Dividend"" :
""@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""Split"" :
""@Standard Dividend Japanese@"".IndexOf(""@$XmlType@"") >= 0 ? ""Dividend"" : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'corporateActionType']",
                                    expression = @"
""$XmlType"".StartsWith(""Standard Dividend"") || ""$XmlType""==""Standard Return of Capital"" ? ""Dividend"" :
""@SCRIP in Same Stock@Non-renounceable SCRIP in same stock@Stock Dividend@"".IndexOf(""@$XmlType@"") >= 0 ? ""Free attribution"" :
""@Stock Split@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""Merger"" :
""@SCRIP in Different Stock@Non-renounceable SCRIP in Diff stock@Standard Spin-Off@"".IndexOf(""@$XmlType@"") >= 0 ? ""Demerger"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'corporateActionType']",
                                    expression = @"
""$XmlType""==""Standard Dividend with option"" ? ""Free attribution"" :
""@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@Stock Dividend@Stock Split@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""Post Rounding"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'corporateActionType']",
                                    expression = @"
""$XmlType""==""Standard Dividend with option"" ? ""Free attribution"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'roundingType']",
                                    expression = @"""Truncated"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'remark']",
                                    expression = @"""@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""CashedOutAtClosingPrice"" : ""$XmlType""==""Standard Dividend In Another Currency"" ? ""DividendInStockCurrency"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'exdivDate']",
                                    expression = "\"$CAType\" == \"DIV\" ? System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(lineVal.Split(';')[20], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Record Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Pay Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'paymentDate']",
                                    expression = @"
""@Standard Dividend Japanese@Standard Dividend In Another Currency@Standard Dividend with option@Standard Dividend@Stock Dividend@"".IndexOf(""@$XmlType@"") >= 0 ?
System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
//max(Effective Date, Capital Change Ex Date)
""@Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? (new DateTime(Math.Max(System.DateTime.ParseExact(lineVal.Split(';')[14], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).Ticks, System.DateTime.ParseExact(lineVal.Split(';')[20], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).Ticks))).ToString(""yyyy-MM-dd"") : 
System.DateTime.ParseExact(lineVal.Split(';')[14], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Rate", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'coefficient']",
                                    expression = @"
""@Standard Return of Capital@"".IndexOf(""@$XmlType@"") >= 0 ? ""$DividendRate"" :
//""@Standard Dividend@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 && "",ZAr,KWd,ILs,GBp,"".IndexOf(lineVal.Split(';')[32]) >= 0 ? (double.Parse(colVal) / $BaseRate).ToString() :
""@Standard Dividend with option@Standard Dividend@Standard Dividend Japanese@Standard Return of Capital@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 ? colVal :
""@Stock Dividend@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@"".IndexOf(""@$XmlType@"") >= 0 ? ((double)$numerator/$denominator).ToString() :
""@Stock Split@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? lineVal.Split(';')[18] : ""0"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'cash']",
                                    expression = @"""@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 ? colVal : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'coefficient']",
                                    expression = @"""@Standard Dividend with option@"".IndexOf(""@$XmlType@"") >= 0 ? ((double)$numerator/$denominator).ToString() : ""1"""
                                },
                            }
                        },
                        new PdtColumn { name = "Annualised Dividend Gross Amount", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Annualised Dividend Net Amount", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Declared Rate", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'currencyRate']",
                                    expression = "colVal != \"\" ? colVal : \"1\""
                                }
                            }
                        },
                        new PdtColumn { name = "Declared Rate Currency", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Corporate Action Notes", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Effective Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Record Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Actual Adjustment Factor", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'rfactor']",
                                    expression = @"""@Standard Spin-Off@"".IndexOf(""@$XmlType@"") >= 0 ? colVal : ""0.000000"""
                                }
                            }
                        },
                        new PdtColumn { name = "Old Shares Terms", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'denominator']",
                                    expression = "$denominator"
                                },
                            }
                        },
                        new PdtColumn { name = "New Shares Terms", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numerator']",
                                    expression = "$numerator"
                                },
                            }
                        },
                        new PdtColumn { name = "Round Lot Size", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Capital Change Ex Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'EffectiveDate']",
                                    expression = @"""@Standard Return of Capital@"".IndexOf(""@$XmlType@"") >= 0 ? System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") : """""
                                }
                            }
                        },
                        new PdtColumn { name = "Capital Change New Security ISIN", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"
""@Stock Split@Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? @""
SELECT T.SICOVAM
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + lineVal.Split(';')[0] + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND INSTR(':' || ERI_REFI.VALUE || ':', ':$ExchangeCode:') > 0
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MEDIO_POSITION_QUANTITY(MVTIDENT, SYSDATE) > 0)"" : @""
SELECT T.SICOVAM
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'"""
                                    },
                                    path = "//*[local-name() = 'diffusedCode']/*[local-name() = 'sophis']"
                                }
                            }
                        },
                        new PdtColumn { name = "Capital Change New Security PILC", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Payment Type", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Payment Type Description", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Rights Identifier", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Rights ISIN", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Offer Price", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Offer Price Currency", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Subscription Period Start Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'electionStartDate']",
                                    expression = "\"$CAType\" == \"DIV\" ? \"$SubscriptionPeriodStartDate\" : System.DateTime.ParseExact(lineVal.Split(';')[14], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Subscription Period End Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'electionEndDate']",
                                    expression = "\"$CAType\" == \"DIV\" ? \"$SubscriptionPeriodEndDate\" : System.DateTime.ParseExact(lineVal.Split(';')[14], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Announcement Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'resultDate']",
                                    expression = @"
""@Stock Split@Reverse Stock Split@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@"".IndexOf(""@$XmlType@"") >= 0 ? System.DateTime.ParseExact(lineVal.Split(';')[14], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
""@Standard Dividend Japanese@"".IndexOf(""@$XmlType@"") >= 0 ? System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
//Dividend Pay Date
System.DateTime.ParseExact(lineVal.Split(';')[7], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'announcementDate']",
                                    expression = @"
""@Stock Split@Reverse Stock Split@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@"".IndexOf(""@$XmlType@"") >= 0 ?
//Capital Change Announcement Date
System.DateTime.ParseExact(lineVal.Split(';')[45], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Currency", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                                //expression = @""",ZAr,KWd,ILs,GBp,"".IndexOf(colVal) >= 0 ? colVal.ToUpper() : colVal" } }
                        },
                        new PdtColumn { name = "Dividend Currency Description", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Ex Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'recordDate']",
                                    expression = @"
""@Standard Dividend In Another Currency@Stock Dividend@Standard Dividend@Standard Dividend Japanese@Standard Dividend with option@"".IndexOf(""@$XmlType@"") >= 0 ?
System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
""@Standard Spin-Off@"".IndexOf(""@$XmlType@"") >= 0 ?
//Record Date
System.DateTime.ParseExact(lineVal.Split(';')[15], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
//min(Effective Date, Capital Change Ex Date)
""@Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? (new DateTime(Math.Min(System.DateTime.ParseExact(lineVal.Split(';')[14], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).Ticks, System.DateTime.ParseExact(lineVal.Split(';')[20], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).Ticks))).ToString(""yyyy-MM-dd"") : 
//Capital Change Ex Date //@Reverse Stock Split@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@
System.DateTime.ParseExact(lineVal.Split(';')[20], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'corporateActionDate']",
                                    expression = @"
""@Standard Dividend In Another Currency@Stock Dividend@Standard Dividend@Standard Dividend Japanese@Standard Dividend with option@"".IndexOf(""@$XmlType@"") >= 0 ?
System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
//min(Effective Date, Capital Change Ex Date)
""@Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? (new DateTime(Math.Min(System.DateTime.ParseExact(lineVal.Split(';')[14], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).Ticks, System.DateTime.ParseExact(lineVal.Split(';')[20], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).Ticks))).ToString(""yyyy-MM-dd"") : 
//Capital Change Ex Date //@Reverse Stock Split@SCRIP in Same Stock@SCRIP in Different Stock@Non-renounceable SCRIP in same stock@Non-renounceable SCRIP in Diff stock@
System.DateTime.ParseExact(lineVal.Split(';')[20], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Pay Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Payment Type", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Record Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Delete Marker", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Market Event ID", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Capital Change Event Type", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Corporate Actions ID", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'corporateActions'][1]//*[local-name() = 'reference']", expression = @"""$Sicovam1""=="""" ? """" : colVal + ""_$Sicovam1""" },
                                new PdtColumnDest { path = "//*[local-name() = 'corporateActions'][2]//*[local-name() = 'reference']", expression = @"""$Sicovam2""=="""" ? """" : colVal + ""_$Sicovam2""" },
                                new PdtColumnDest { path = "//*[local-name() = 'corporateActions'][3]//*[local-name() = 'reference']", expression = @"""$Sicovam3""=="""" ? """" : colVal + ""_$Sicovam3""" },
                                new PdtColumnDest { path = "//*[local-name() = 'corporateActions'][4]//*[local-name() = 'reference']", expression = @"""$Sicovam4""=="""" ? """" : colVal + ""_$Sicovam4""" },
                                new PdtColumnDest { path = "//*[local-name() = 'corporateActions'][5]//*[local-name() = 'reference']", expression = @"""$Sicovam5""=="""" ? """" : colVal + ""_$Sicovam5""" },
                            }
                        },
                        new PdtColumn { name = "Capital Change Event Type Description", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Mandatory/Voluntary Description", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'category']",
                                    expression = "colVal == \"Choice\" ? \"Mandatory with choice\" : \"Mandatory\""
                                }
                            }
                        },
                        new PdtColumn { name = "Mandatory/Voluntary Indicator", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Capital Change Announcement Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Event Status", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Type Marker", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Type Marker Description", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Exchange Code", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Exchange Description", isRequired = true, isRelativeToRootNode = true},
                    }
                },
                new PdtTransformation
                {
                    name = TransName.REFI_CA_SR.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Refinitiv Corporate Action Standard Renaming",
                    templateFile = "REFI_CA.xml",
                    category = "Medio",
                    //fileBreakExpression = @"lineVal.Split(';')[0].Substring(5)+""_""+""$XmlType"".Replace("" "", """")",
                    fileBreakExpression = @"lineVal.Split(';')[3].Substring(6)",
                    repeatingRootPath = "//*[local-name() = 'corporateActionList']",
                    repeatingChildrenPath = "//*[local-name() = 'corporateAction']",
                    csvSkipLines = 1,
                    csvSrcSeparator = ';',
                    variables = new[] {
                        new PdtVariable { name = "XmlType", expressionBefore = "\"Standard Renaming\""},
                        new PdtVariable { name = "nameChange", expressionBefore = @"lineVal.Split(';')[2]==""CHAN//NAME"" ? ""Name"" :
lineVal.Split(';')[0]==lineVal.Split(';')[5] && lineVal.Split(';')[6].StartsWith(""/TS/"") && lineVal.Split(';')[7].StartsWith(""/TS/"") && lineVal.Split(';')[6]!=lineVal.Split(';')[7] ? ""TICKER"" :
lineVal.Split(';')[0]!=lineVal.Split(';')[5] && lineVal.Split(';')[0].StartsWith(""ISIN"") && lineVal.Split(';')[5].StartsWith(""ISIN"") && lineVal.Split(';')[6]==lineVal.Split(';')[7] ? ""ISIN"" : ""Unknown"""},
                        new PdtVariable { name = "newNameOfCompany", expressionBefore = @"lineVal.Split(';')[2]==""CHAN//NAME"" ? lineVal.Split(';')[1].Substring(6) :
lineVal.Split(';')[0]==lineVal.Split(';')[5] && lineVal.Split(';')[6].StartsWith(""/TS/"") && lineVal.Split(';')[7].StartsWith(""/TS/"") && lineVal.Split(';')[6]!=lineVal.Split(';')[7] ? lineVal.Split(';')[7].Split('/')[2] :
lineVal.Split(';')[0]!=lineVal.Split(';')[5] && lineVal.Split(';')[0].StartsWith(""ISIN"") && lineVal.Split(';')[5].StartsWith(""ISIN"") && lineVal.Split(';')[6]==lineVal.Split(';')[7] ? lineVal.Split(';')[5].Substring(5) : ""Unknown"""},
                        new PdtVariable { name = "DuplicateXmlType",
                            expressionBefore = @"""CAP_""+lineVal.Split(';')[0].Substring(5)",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ISIN", ColumnIndex = "0" },
                            expressionAfter=@"""Stock consolidation"" == colVal ? ""Reverse Stock Split"" : ""Unknown""",
                        },
                    },
                    processingCondition = @"""$DuplicateXmlType"" != ""Reverse Stock Split"" && ""$nameChange"" != ""Unknown""",
                    postProcessEvent = @"
        var nodePaths = new List<string>();
        if (""$XmlType"" == ""Standard Renaming"") {
            nodePaths.Add(""//*[local-name() = 'conversionRatio']"");
            nodePaths.Add(""//*[local-name() = 'announcementDate']"");
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'paymentDate']"");
            nodePaths.Add(""//*[local-name() = 'exdivDate']"");
            nodePaths.Add(""//*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'coefficient']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            if (""Name,ISIN,TICKER"".IndexOf(doc.SelectSingleNode(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'nameChange']"").InnerText) < 0) {
                nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'nameChange']"");
                nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'newNameOfCompany']"");
            }
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        foreach(string path in nodePaths) {
            var nodes = doc.SelectNodes(path);
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes.Item(i);
                node.ParentNode.RemoveChild(node);
            }
        }",
                    columns = new []
                    {
                        new PdtColumn { name = "ISIN", isRequired = true, isRelativeToRootNode = false,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.SICOVAM
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + (colVal.StartsWith(""ISIN"") ? colVal.Substring(5) : """") + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MEDIO_POSITION_QUANTITY(MVTIDENT, SYSDATE) > 0)"""
                                    },
                                    path = "//*[local-name() = 'identifier']/*[local-name() = 'sophis']"
                                },
                            }
                        },
                        new PdtColumn { name = "70E", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "22F", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'type'] | //*[local-name() = 'corporateActionType']",
                                    expression = "\"$XmlType\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent1']",
                                    expression = "\"Renaming\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'category']",
                                    expression = "\"Mandatory\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'nameChange']",
                                    expression = @"""$nameChange"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'newNameOfCompany']",
                                    expression = @"""$newNameOfCompany"""
                                },
                            }
                        },
                        new PdtColumn { name = "20C", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "colVal.Substring(6)"
                                }
                            }
                        },
                        new PdtColumn { name = "98A", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'recordDate'] | //*[local-name() = 'corporateActionDate']",
                                    expression = @"System.DateTime.ParseExact(colVal.Substring(6), ""yyyyMMdd"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                }
                            }
                        },
                    }
                },
        //        new PdtTransformation
        //        {
        //            name = TransName.REFI_CA_Cancel.ToString(),
        //            type = TransType.Csv2Xml,
        //            label = "Refinitiv Corporate Action Cancel",
        //            templateFile = "TradeUpdate.xml",
        //            category = "Medio",
        //            fileBreakExpression = @"lineVal.Split(';')[0]",
        //            repeatingRootPath = "//*[local-name() = 'import']",
        //            repeatingChildrenPath = "//*[local-name() = 'trade']",
        //            csvSkipLines = 1,
        //            csvSrcSeparator = ',',
        //            UseHeaderColumnNames = true,
        //            variables = new[] {
        //                new PdtVariable { name = "EventStatus", expressionBefore = "lineVal.Split(';')[46]"},
        //            },
        //            processingCondition = "\"$EventStatus\" == \"Rescinded\"",
        //            postProcessEvent = @"
        //var nodePaths = new List<string>();
        //nodePaths.Add(""//*[local-name() = 'tradeProduct']"");
        //nodePaths.Add(""//*[local-name() = 'tradeDate']"");
        //nodePaths.Add(""//*[local-name() = 'paymentDate']"");
        //nodePaths.Add(""//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], '/id')]"");
        //foreach (string path in nodePaths) {
        //    var nodes = doc.SelectNodes(path);
        //    for (int i = 0; i < nodes.Count; i++)
        //    {
        //        var node = nodes.Item(i);
        //        node.ParentNode.RemoveChild(node);
        //    }
        //}",
        //            columns = new []
        //            {
        //                new PdtColumn { name = "Corporate Actions ID", isRequired = true, isRelativeToRootNode = true,
        //                    destPaths = new [] {
        //                        new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'Ref_Corporate_Action_ID')]" },
        //                        new PdtColumnDest {
        //                            path = "//*[local-name() = 'boComment']",
        //                            expression = "\"$EventStatus\""
        //                        },
        //                        new PdtColumnDest {
        //                            path = "//*[local-name() = 'trade']/@*[local-name() = 'updateWorkflowEventName']",
        //                            expression = "\"Refinitiv Cancelled\""
        //                        },
        //                    }
        //                },
        //            }
        //        },
                new PdtTransformation
                {
                    name = TransName.REFI_CA_Cancel.ToString(),
                    type = TransType.Csv2Csv,
                    label = "Refinitiv Corporate Action Cancel",
                    templateFile = "GenericList1.txt",
                    category = "Medio",
                    csvSkipLines = 1,
                    csvSrcSeparator = ',',
                    UseHeaderColumnNames = true,
                    variables = new[] {
                        new PdtVariable { name = "EventStatus", expressionBefore = "lineVal.Split(';')[46]"},
                    },
                    processingCondition = "\"$EventStatus\" == \"Rescinded\"",
                    columns = new []
                    {
                        new PdtColumn { name = "Corporate Actions ID", destPaths = new [] { new PdtColumnDest { path = "Item1" } } },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.REFI_CA_Update_MT566_Mandatory.ToString(),
                    type = TransType.Csv2Xml,
                    label = "MT566 Update Mandatory CA",
                    templateFile = "TradeUpdate.xml",
                    category = "Medio",
                    fileBreakExpression = "\"$SecurityID_$CorporateActionReference_$CorporateActionsOptionNumber\"",
                    csvSkipLines = 1,
                    csvSrcSeparator = ',',
                    ExtraEvalCode = @"
    private static string GetSecurityId(string sId)
    {
        string ans = sId;
        if (sId.StartsWith(""ISIN "")) ans = sId.Substring(5); //ISIN
        else if (sId.StartsWith(""/GB/"")) ans = sId.Substring(4); //SEDOL
        else if (sId.StartsWith(""/US/"")) ans = sId.Substring(4); //CUSIP
        return ans;
    }",
                    UseHeaderColumnNames = true,
                    variables = new[] {
                        new PdtVariable { name = "CAEventInd", expressionBefore = "lineVal.Split('$CsvSrcSep')[2]"},
                        new PdtVariable { name = "SafekeepingAccount", expressionBefore = "lineVal.Split('$CsvSrcSep')[3]"},
                        new PdtVariable { name = "SecurityID", expressionBefore = "GetSecurityId(lineVal.Split('$CsvSrcSep')[4])"},//.Substring(5)
                        new PdtVariable { name = "Quantity", expressionBefore = "lineVal.Split('$CsvSrcSep')[6]"},
                        new PdtVariable { name = "ExDate", expressionBefore = "lineVal.Split('$CsvSrcSep')[7]"},
                        new PdtVariable { name = "EffDate", expressionBefore = "lineVal.Split('$CsvSrcSep')[8]"},
                        new PdtVariable { name = "PayDate", expressionBefore = "lineVal.Split('$CsvSrcSep')[9]"},
                        new PdtVariable { name = "Price", expressionBefore = "lineVal.Split('$CsvSrcSep')[11]"},
                        new PdtVariable { name = "Rate", expressionBefore = "lineVal.Split('$CsvSrcSep')[12]"},
                        new PdtVariable { name = "CAOptionCode", expressionBefore = "lineVal.Split('$CsvSrcSep')[13]"},
                        new PdtVariable { name = "SecurityCreditDebitInd1", expressionBefore = "lineVal.Split('$CsvSrcSep')[14]"},
                        new PdtVariable { name = "SecEntitlementID1", expressionBefore = "GetSecurityId(lineVal.Split('$CsvSrcSep')[15])"},//.Substring(5)
                        new PdtVariable { name = "ShareEntitlement1", expressionBefore = "lineVal.Split('$CsvSrcSep')[17]"},
                        new PdtVariable { name = "PostingDateforSecMove1", expressionBefore = "lineVal.Split('$CsvSrcSep')[18]"},
                        new PdtVariable { name = "ValueDateforSecMove1", expressionBefore = "lineVal.Split('$CsvSrcSep')[19]"},
                        new PdtVariable { name = "CashEntitlement1", expressionBefore = "lineVal.Split('$CsvSrcSep')[20]"},
                        new PdtVariable { name = "SecurityCreditDebitInd2", expressionBefore = "lineVal.Split('$CsvSrcSep')[21]"},
                        new PdtVariable { name = "SecEntitlementID2", expressionBefore = "GetSecurityId(lineVal.Split('$CsvSrcSep')[22])"},//.Substring(5)
                        new PdtVariable { name = "ShareEntitlement2", expressionBefore = "lineVal.Split('$CsvSrcSep')[24]"},
                        new PdtVariable { name = "PostingDateforSecMove2", expressionBefore = "lineVal.Split('$CsvSrcSep')[25]"},
                        new PdtVariable { name = "ValueDateforSecMove2", expressionBefore = "lineVal.Split('$CsvSrcSep')[26]"},
                        new PdtVariable { name = "CashEntitlement2", expressionBefore = "lineVal.Split('$CsvSrcSep')[27]"},
                        new PdtVariable { name = "PostingDateforCashMove1", expressionBefore = "lineVal.Split('$CsvSrcSep')[28]"},
                        new PdtVariable { name = "ValueDateforCashMove1", expressionBefore = "lineVal.Split('$CsvSrcSep')[29]"},
                        new PdtVariable { name = "PostingDateforCashMove2", expressionBefore = "lineVal.Split('$CsvSrcSep')[30]"},
                        new PdtVariable { name = "CorporateActionReference", expressionBefore = "lineVal.Split('$CsvSrcSep')[47]"},
                        new PdtVariable { name = "UniqueCAEventMarketIdentifier", expressionBefore = "lineVal.Split('$CsvSrcSep')[48]"},
                        new PdtVariable { name = "CorporateActionsOptionNumber", expressionBefore = "lineVal.Split('$CsvSrcSep')[49]"},
                        new PdtVariable { name = "CashCreditDebitInd1", expressionBefore = "lineVal.Split('$CsvSrcSep')[52]"},
                        new PdtVariable { name = "CashCreditDebitInd2", expressionBefore = "lineVal.Split('$CsvSrcSep')[53]"},
                        new PdtVariable { name = "tradeIdVolSQL", expressionBefore = @"@""
SELECT DISTINCT(A.SOPHIS_IDENT)
    FROM EXTRNL_REFERENCES_TRADES A
    JOIN EXTRNL_REFERENCES_TRADES B ON A.SOPHIS_IDENT=B.SOPHIS_IDENT
    JOIN EXTRNL_REFERENCES_TRADES C ON B.SOPHIS_IDENT=C.SOPHIS_IDENT
    --JOIN EXTRNL_REFERENCES_TRADES D ON C.SOPHIS_IDENT=D.SOPHIS_IDENT
    JOIN HISTOMVTS H ON H.REFCON = A.SOPHIS_IDENT
    JOIN TIERSSETTLEMENT T ON T.SSI_PATH_ID = H.NOSTRO_CASH_ID AND T.ACCOUNT_ROUTER = '$SafekeepingAccount'
    JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE
    JOIN JOIN_POSITION_HISTOMVTS P ON P.REFCON=A.SOPHIS_IDENT
    JOIN EXTRNL_REFERENCES_INSTRUMENTS E ON E.SOPHIS_IDENT=P.SICOVAM --AND E.REF_IDENT=1 --ISIN=1, SEDOL=1, CUSIP=3
WHERE (A.ORIGIN = 'SWIFT_CAEV' AND A.VALUE = '$CAEventInd')
    AND (B.ORIGIN =  'SWIFT_CORP' AND B.VALUE = '$CorporateActionReference')
    AND (C.ORIGIN = 'SWIFT_CAON' AND C.VALUE = TO_NUMBER('$CorporateActionsOptionNumber'))
    --AND (D.ORIGIN = 'CA_UNIQUE_MKT_REF' AND D.VALUE = '$UniqueCAEventMarketIdentifier')
    {0}
""" },
                        new PdtVariable { name = "tradeIdVol1", Lookup = new PdtColumnLookup { Table = "SQL", Expression = @"string.Format(@""$tradeIdVolSQL"",
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""AND E.VALUE='$SecEntitlementID2' AND BE.NAME IN ('Security Adjustment') AND H.INFOS='Exercise the right'"" :
""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"" && ""$SecEntitlementID2""=="""" ? ""AND E.VALUE='$SecEntitlementID1' AND BE.NAME IN ('Security Adjustment')"" :
"",BONU,EXOF,RHTS,EXRI,"".IndexOf("",$CAEventInd,"")>=0 && ""$CAOptionCode""==""CASH"" ? ""AND E.VALUE='$SecEntitlementID1' AND BE.NAME IN ('Fractional Cash out', 'Dividend')"" :
 ""$CAEventInd""==""DRIP"" && ""$CAOptionCode""==""CASH"" ? ""AND E.VALUE='$SecurityID' AND BE.NAME='Dividend'"" :
""$CAEventInd""==""DRIP"" && ""$CAOptionCode""==""SECU"" ? ""AND E.VALUE='$SecEntitlementID1' AND BE.NAME='Security Adjustment'"" :
""$CAEventInd""==""DVCA"" ? ""AND E.VALUE='$SecurityID' AND BE.NAME IN ('Dividend')"" :
"",SPLF,SPLR,"".IndexOf("",$CAEventInd,"")>=0 ? ""AND E.VALUE='$SecurityID' AND BE.NAME IN ('Split')"" :
"",TEND,BIDS,DTCH,"".IndexOf("",$CAEventInd,"")>=0 ? ""AND E.VALUE='$SecEntitlementID1'"" :
""$CAOptionCode""==""CASH"" ? ""AND E.VALUE='$SecurityID'"" :
""AND E.VALUE='$SecEntitlementID1'""
)" } },
                        new PdtVariable { name = "tradeIdVol2", Lookup = new PdtColumnLookup { Table = "SQL", Expression = @"string.Format(@""$tradeIdVolSQL"",
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""AND E.VALUE='$SecEntitlementID1' AND BE.NAME IN ('Security Adjustment') AND H.INFOS='Open the right'"" :
"",SPLF,SPLR,"".IndexOf("",$CAEventInd,"")>=0 ? ""AND E.VALUE='$SecurityID' AND BE.NAME IN ('Split')"" :
"",EXOF,MRGR,"".IndexOf("",$CAEventInd,"")>=0 ? ""AND E.VALUE='$SecEntitlementID2'"" :
""AND 1=0""
)" } },
                        new PdtVariable { name = "tradeIdVol3", Lookup = new PdtColumnLookup { Table = "SQL", Expression = @"string.Format(@""$tradeIdVolSQL"",
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""AND E.VALUE='$SecEntitlementID1' AND BE.NAME IN ('Security Adjustment') AND H.INFOS='Close the right'"" :
""AND 1=0""
)" } },
                        new PdtVariable { name = "tradeIdManSQL", expressionBefore = @"@""
SELECT H.REFCON 
FROM CORPORATE_ACTION CA
    JOIN CORPORATE_ACTION_TYPE CAT ON CAT.CA_TYPE_ID = CA.CA_TYPE_ID
    JOIN AJUSTEMENTS AJ ON AJ.CA_ID = CA.CA_ID
		{0} {1} AND AJ.JOUR = TO_DATE('{2}', 'YYYYMMDD')
    JOIN EXTRNL_REFERENCES_INSTRUMENTS E ON E.SOPHIS_IDENT = AJ.SICOVAM AND E.REF_IDENT = 1
        AND E.VALUE = '{3}'
    JOIN EXTRNL_REFERENCES_TRADES A ON E.SOPHIS_IDENT = AJ.SICOVAM AND E.REF_IDENT = 1
        AND A.ORIGIN = 'FUS_CA_ID' AND A.VALUE = CA.CA_NAME
    JOIN HISTOMVTS H ON H.REFCON = A.SOPHIS_IDENT {4}
    JOIN TIERSSETTLEMENT T ON T.SSI_PATH_ID = H.NOSTRO_CASH_ID AND T.ACCOUNT_ROUTER = '$SafekeepingAccount'
    JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE AND BE.NAME IN ({5})
WHERE CAT.CA_TYPE_NAME IN ({6})
""" },
                        new PdtVariable { name = "businessEvent1",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""SELECT BE.NAME FROM HISTOMVTS H JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE WHERE REFCON = '$tradeIdVol1'"""
                            },
                        },
                        new PdtVariable { name = "businessEvent2",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""SELECT BE.NAME FROM HISTOMVTS H JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE WHERE REFCON = '$tradeIdVol2'"""
                            },
                        },
                        new PdtVariable { name = "businessEvent3",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""SELECT BE.NAME FROM HISTOMVTS H JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE WHERE REFCON = '$tradeIdVol3'"""
                            },
                        },
                        new PdtVariable { name = "payeStr1", expressionBefore = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""$PostingDateforSecMove2"" :
"",BONU,DVCA,"".IndexOf("",$CAEventInd,"") >= 0 && "",Dividend,Fractional Cash out,"".IndexOf("",$businessEvent1,"") >= 0 ? ""$PostingDateforCashMove1"" :
"",DVSE,DVCA,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$PayDate"" : 
""$CAEventInd""==""DVSC"" ? ""$ValueDateforSecMove1"" : 
"",SPLF,SPLR,CAPD,BONU,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$EffDate"" : 
"",EXOF,MRGR,DVOP,EXRI,"".IndexOf("",$CAEventInd,"")>=0 ? (""$CAOptionCode""==""CASH"" ? ""$PostingDateforCashMove1"" : ""$PostingDateforSecMove1"") : 
"",TEND,BIDS,DTCH,"".IndexOf("",$CAEventInd,"")>=0 ? ""$PostingDateforCashMove1"" : 
""$CAEventInd""==""SOFF"" ? """" :
"",Dividend,Fractional Cash out,"".IndexOf("",$businessEvent1,"") >= 0 ? ""$PostingDateforCashMove1"" : ""$PostingDateforSecMove1""" },
                        new PdtVariable { name = "payeStr2", expressionBefore = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""$PostingDateforSecMove1"" :
"",DVSE,DVCA,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$PayDate"" : 
""$CAEventInd""==""DVSC"" ? ""$ValueDateforSecMove1"" : 
"",SPLF,SPLR,CAPD,BONU,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$EffDate"" : 
"",EXOF,MRGR,DVOP,EXRI,"".IndexOf("",$CAEventInd,"")>=0 ? (""$CAOptionCode""==""CASH"" ? ""$PostingDateforCashMove2"" : ""$PostingDateforSecMove2"") : 
"",TEND,BIDS,DTCH,"".IndexOf("",$CAEventInd,"")>=0 ? ""$PostingDateforCashMove1"" : 
""$CAEventInd""==""SOFF"" ? """" : 
"",Dividend,Fractional Cash out,"".IndexOf("",$businessEvent2,"") >= 0 ? ""$PostingDateforCashMove1"" : ""$PostingDateforSecMove1""" },
                        new PdtVariable { name = "paymentDateStr1", expressionBefore = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""$PostingDateforSecMove2"" :
""$CAEventInd""==""DVSC"" ? ""$ValueDateforSecMove1"" : 
"",EXOF,MRGR,DVOP,EXRI,BONU,DVCA,SOFF,SPLF,SPLR,CAPD,"".IndexOf("",$CAEventInd,"")>=0 ? (""$CAOptionCode""==""CASH"" ? ""$PostingDateforCashMove1"" : ""$PostingDateforSecMove1"") : 
"",TEND,BIDS,DTCH,"".IndexOf("",$CAEventInd,"")>=0 ? ""$PostingDateforCashMove1"" : 
"",Dividend,Fractional Cash out,"".IndexOf("",$businessEvent1,"") >= 0 ? ""$PostingDateforCashMove1"" : ""$PostingDateforSecMove1""" },
                        new PdtVariable { name = "paymentDateStr2", expressionBefore = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""$PostingDateforSecMove1"" :
""$CAEventInd""==""DVSC"" ? ""$ValueDateforSecMove1"" : 
"",EXOF,MRGR,DVOP,EXRI,BONU,DVCA,SOFF,SPLF,SPLR,CAPD,"".IndexOf("",$CAEventInd,"")>=0 ? (""$CAOptionCode""==""CASH"" ? ""$PostingDateforCashMove2"" : ""$PostingDateforSecMove2"") : 
"",TEND,BIDS,DTCH,"".IndexOf("",$CAEventInd,"")>=0 ? ""$PostingDateforCashMove1"" : 
"",Dividend,Fractional Cash out,"".IndexOf("",$businessEvent2,"") >= 0 ? ""$PostingDateforCashMove1"" : ""$PostingDateforSecMove1""" },
                        new PdtVariable { name = "tradeIdMan1", Lookup = new PdtColumnLookup { Table = "SQL", Expression = @"string.Format(@""$tradeIdManSQL"",
"",SPLF,SPLR,"".IndexOf("",$CAEventInd,"") >= 0 ? """" : ""AND AJ.EXDIVDATE = TO_DATE('$ExDate', 'YYYYMMDD')"",
""$payeStr1""=="""" ? """" : ""AND AJ.PAYE = TO_DATE('$payeStr1', 'YYYYMMDD')"",
""$ExDate"",
"",DVSE,DVSC,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$SecEntitlementID1"" : "",SPLF,SPLR,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$SecEntitlementID1"" : "",DVCA,CAPD,SOFF,BONU,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$SecurityID"" : ""Unknown"",
"",SPLF,SPLR,"".IndexOf("",$CAEventInd,"") >= 0 && ""$CAOptionCode""==""SECU"" ? ""AND H.QUANTITE>0"" : """",
"",DVSE,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Free'"" : "",DVSC,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Free shares'"" : "",DVCA,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Dividend'"" : "",CAPD,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Cash Adjustment CA'"" : "",SOFF,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Security Adjustment'"" : "",SPLF,SPLR,"".IndexOf("",$CAEventInd,"") >= 0 ? (""$CAOptionCode""==""SECU"" ? ""'Split'"" : ""'Fractional Cash out'"") : "",BONU,"".IndexOf("",$CAEventInd,"") >= 0 ? (""$CAOptionCode""==""SECU"" ? ""'Free shares', 'Security Adjustment'"" : ""'Fractional Cash out'"") : ""'Unknown'"",
""$CAEventInd""==""DVSE"" ? ""'Stock Dividend'"" : ""$CAEventInd""==""DVSC"" ? ""'SCRIP in Same Stock'"" : ""$CAEventInd""==""DVCA"" ? ""'Standard Dividend', 'Standard Dividend Japanese', 'Standard Dividend In Another Currency'"" : ""$CAEventInd""==""CAPD"" ? ""'Standard Return of Capital'"" : ""$CAEventInd""==""SOFF"" ? ""'Standard Spin-Off'"" : ""$CAEventInd""==""SPLF"" ? ""'Stock Split'"" : ""$CAEventInd""==""SPLR"" ? ""'Reverse Stock Split'"" : ""$CAEventInd""==""BONU"" ? ""'SCRIP in Same Stock','SCRIP in Different Stock','Non-renounceable SCRIP in same stock','Non-renounceable SCRIP in Diff stock'"" : ""'Unknown'""
)" } },
                        new PdtVariable { name = "tradeIdMan2", Lookup = new PdtColumnLookup { Table = "SQL", Expression = @"string.Format(@""$tradeIdManSQL"",
"",SPLF,SPLR,"".IndexOf("",$CAEventInd,"") >= 0 ? """" : ""AND AJ.EXDIVDATE = TO_DATE('$ExDate', 'YYYYMMDD')"",
""$payeStr2""=="""" ? """" : ""AND AJ.PAYE = TO_DATE('$payeStr2', 'YYYYMMDD')"",
""$ExDate"",
"",DVSE,DVSC,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$SecEntitlementID1"" : "",SPLF,SPLR,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$SecEntitlementID2"" : "",DVCA,CAPD,SOFF,BONU,"".IndexOf("",$CAEventInd,"") >= 0 ? ""$SecurityID"" : ""Unknown"",
"",SPLF,SPLR,"".IndexOf("",$CAEventInd,"") >= 0 && ""$CAOptionCode""==""SECU"" ? ""AND H.QUANTITE<0"" : """",
"",DVSE,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Free'"" : "",DVSC,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Free shares'"" : "",DVCA,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Dividend'"" : "",CAPD,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Cash Adjustment CA'"" : "",SOFF,"".IndexOf("",$CAEventInd,"") >= 0 ? ""'Security Adjustment'"" : "",SPLF,SPLR,"".IndexOf("",$CAEventInd,"") >= 0 ? (""$CAOptionCode""==""SECU"" ? ""'Split'"" : ""'Fractional Cash out'"") : "",BONU,"".IndexOf("",$CAEventInd,"") >= 0 ? (""$CAOptionCode""==""SECU"" ? ""'Free shares', 'Security Adjustment'"" : ""'Fractional Cash out'"") : ""'Unknown'"",
""$CAEventInd""==""DVSE"" ? ""'Stock Dividend'"" : ""$CAEventInd""==""DVSC"" ? ""'SCRIP in Same Stock'"" : ""$CAEventInd""==""DVCA"" ? ""'Standard Dividend', 'Standard Dividend Japanese', 'Standard Dividend In Another Currency'"" : ""$CAEventInd""==""CAPD"" ? ""'Standard Return of Capital'"" : ""$CAEventInd""==""SOFF"" ? ""'Standard Spin-Off'"" : ""$CAEventInd""==""SPLF"" ? ""'Stock Split'"" : ""$CAEventInd""==""SPLR"" ? ""'Reverse Stock Split'"" : ""$CAEventInd""==""BONU"" ? ""'SCRIP in Same Stock','SCRIP in Different Stock','Non-renounceable SCRIP in same stock','Non-renounceable SCRIP in Diff stock'"" : ""'Unknown'""
)" } },
                        new PdtVariable { name = "tradeId1", expressionBefore = @"""$tradeIdVol1""!="""" ? ""$tradeIdVol1"" : ""$tradeIdMan1""!="""" ? ""$tradeIdMan1"" : """"" },
                        new PdtVariable { name = "tradeId2", expressionBefore = @"""$tradeIdVol2""!="""" ? ""$tradeIdVol2"" : ""$tradeIdMan2""!="""" ? ""$tradeIdMan2"" : """"" },
                        new PdtVariable { name = "tradeId3", expressionBefore = @"""$tradeIdVol3""" },
                    },
                    processingCondition = "\"$tradeId1\" != \"\" || \"$tradeId2\" != \"\"",
                    postProcessEvent = @"
        var nodePaths = new List<string>();
        nodePaths.Add(""//*[local-name() = 'extendedPartyTradeInformation']"");
        nodePaths.Add(""//*[local-name() = 'tradeDate']"");
        nodePaths.Add(""//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], '/Ref_Corporate_Action_ID')]"");
        if (((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""") {
            //
        } else if ("",SPLF,SPLR,MRGR,EXOF,"".IndexOf("",$CAEventInd,"")>=0) {
            nodePaths.Add(""//*[local-name() = 'trade'][3]"");
        } else {
            nodePaths.Add(""//*[local-name() = 'trade'][3]"");
            nodePaths.Add(""//*[local-name() = 'trade'][2]"");
        }
        foreach(XmlNode node in doc.SelectNodes(""//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], '/id')][.='']""))
        {
            var nodeParent = node.ParentNode.ParentNode.ParentNode;
            nodeParent.ParentNode.RemoveChild(nodeParent);
        }
        foreach(XmlNode node in doc.SelectNodes(""//*[local-name() = 'amount']/*[local-name() = 'amount'][.='']""))
        {
            var nodeParent = node.ParentNode;
            nodeParent.ParentNode.RemoveChild(nodeParent);
        }
        foreach(XmlNode node in doc.SelectNodes(""//*[local-name() = 'spot'][.='']""))
        {
            node.ParentNode.RemoveChild(node);
        }
        foreach(string path in nodePaths) {
            foreach(XmlNode node in doc.SelectNodes(path))
            {
                node.ParentNode.RemoveChild(node);
            }
        }",
                    columns = new []
                    {
                        new PdtColumn { name = "Record Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'paymentDate']",
                                    expression = "System.DateTime.ParseExact(\"$paymentDateStr1\", \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'paymentDate']",
                                    expression = "System.DateTime.ParseExact(\"$paymentDateStr2\", \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][3]//*[local-name() = 'paymentDate']",
                                    expression = "System.DateTime.ParseExact(\"$paymentDateStr2\", \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'sophis']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"""SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE REFCON='$tradeId1'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'sophis']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"""SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE REFCON='$tradeId2'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][3]//*[local-name() = 'sophis']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"""SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE REFCON='$tradeId3'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'updateWorkflowEventName']",
                                    expression = "\"MT566\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], '/id')]",
                                    expression = @"""$tradeId1"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], '/id')]",
                                    expression = @"""$tradeId2"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][3]//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], '/id')]",
                                    expression = @"""$tradeId3"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'numberOfSecurities']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? (""$SecurityCreditDebitInd2""==""CRED"" ? """" : ""-"") + ""$ShareEntitlement2"" :
"",BONU,RHTS,EXRI,DVSC,MRGR,"".IndexOf("",$CAEventInd,"")>=0 ? ""$ShareEntitlement1"" :
"",SPLF,SPLR,SOFF,DVSE,TEND,BIDS,DTCH,EXOF,"".IndexOf("",$CAEventInd,"")>=0 ? (""$SecurityCreditDebitInd1""==""CRED"" ? """" : ""-"") + ""$ShareEntitlement1"" :
"",DRIP,DVOP,"".IndexOf("",$CAEventInd,"")>=0 && ""$CAOptionCode""==""SECU"" ? (""$SecurityCreditDebitInd1""==""CRED"" ? """" : ""-"") + ""$ShareEntitlement1"" :
"",DRIP,DVOP,"".IndexOf("",$CAEventInd,"")>=0 && ""$CAOptionCode""==""CASH"" ? ""$Quantity"" :
"",DVCA,CAPD,"".IndexOf("",$CAEventInd,"")>=0 ? ""$Quantity"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'numberOfSecurities']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""$ShareEntitlement1"" :
"",BONU,RHTS,EXRI,DVSC,MRGR,"".IndexOf("",$CAEventInd,"")>=0 ? ""$ShareEntitlement2"" :
"",SPLF,SPLR,SOFF,DVSE,TEND,BIDS,DTCH,EXOF,"".IndexOf("",$CAEventInd,"")>=0 ? (""$SecurityCreditDebitInd2""==""CRED"" ? """" : ""-"") + ""$ShareEntitlement2"" :
"",DVCA,CAPD,"".IndexOf("",$CAEventInd,"")>=0 ? ""$Quantity"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][3]//*[local-name() = 'numberOfSecurities']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""-$ShareEntitlement1"" :
""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'amount']/*[local-name() = 'amount']/@*[local-name() = 'negative']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""$CashCreditDebitInd2""==""CRED"" :
""$CashCreditDebitInd1""==""CRED"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'amount']/*[local-name() = 'amount']/@*[local-name() = 'negative']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? false :
""$CashCreditDebitInd2""==""CRED"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][3]//*[local-name() = 'amount']/*[local-name() = 'amount']/@*[local-name() = 'negative']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? true :
""$CashCreditDebitInd2""==""CRED"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? (""$CashEntitlement2"".Length >= 3 ? ""$CashEntitlement2"".Substring(3) : ""$CashEntitlement2"") :
(""$CashEntitlement1"".Length >= 3 ? ""$CashEntitlement1"".Substring(3) : ""$CashEntitlement1"")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'amount']/*[local-name() = 'amount'] | //*[local-name() = 'trade'][3]//*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? (""$CashEntitlement1"".Length >= 3 ? ""$CashEntitlement1"".Substring(3) : ""$CashEntitlement1"") :
(""$CashEntitlement2"".Length >= 3 ? ""$CashEntitlement2"".Substring(3) : ""$CashEntitlement2"")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'currency']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? (""$CashEntitlement2"".Length >= 3 ? ""$CashEntitlement2"".Substring(0, 3) : ""$CashEntitlement2"") :
(""$CashEntitlement1"".Length >= 3 ? ""$CashEntitlement1"".Substring(0, 3) : ""$CashEntitlement1"")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'currency'] | //*[local-name() = 'trade'][3]//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'currency']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? (""$CashEntitlement1"".Length >= 3 ? ""$CashEntitlement1"".Substring(0, 3) : ""$CashEntitlement1"") :
(""$CashEntitlement2"".Length >= 3 ? ""$CashEntitlement2"".Substring(0, 3) : ""$CashEntitlement2"")"
                                },
                            }
                        },
                        new PdtColumn { name = "Price", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'principalSettlement']/*[local-name() = 'spot'] | //*[local-name() = 'trade'][3]//*[local-name() = 'principalSettlement']/*[local-name() = 'spot']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? ""0"" :
colVal=="""" ? ""0"" : Char.IsLetter(colVal[0]) ? colVal.Substring(3) : colVal"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][2]//*[local-name() = 'principalSettlement']/*[local-name() = 'spot']/@*[local-name() = 'currency'] | //*[local-name() = 'trade'][3]//*[local-name() = 'principalSettlement']/*[local-name() = 'spot']/@*[local-name() = 'currency']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? (""$Rate""=="""" || !Char.IsLetter(""$Rate""[0]) ? """" : ""$Rate"".Substring(0, 3)) :
colVal=="""" || !Char.IsLetter(colVal[0]) ? """" : colVal.Substring(0, 3)"
                                },
                            }
                        },
                        new PdtColumn { name = "Rate", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'principalSettlement']/*[local-name() = 'spot']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? (""$Price""=="""" || !Char.IsLetter(""$Price""[0]) ? ""$Price"" : ""$Price"".Substring(3)) :
("",DRIP,DVOP,RHTS,EXOF,MRGR,"".IndexOf("",$CAEventInd,"")>=0 && "",SECU,EXER,"".IndexOf("",$CAOptionCode,"")>=0) ? (""$Price""=="""" || !Char.IsLetter(""$Price""[0]) ? ""$Price"" : ""$Price"".Substring(3)) : (colVal=="""" || !Char.IsLetter(colVal[0]) ? colVal : colVal.Substring(3))"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade'][1]//*[local-name() = 'principalSettlement']/*[local-name() = 'spot']/@*[local-name() = 'currency']",
                                    expression = @"
((""$CAEventInd""==""BONU"" && ""$CAOptionCode""==""SECU"") || (""$CAEventInd""==""EXRI"" && ""$CAOptionCode""==""EXER"")) && ""$SecEntitlementID2""!="""" ? (""$Price""=="""" || !Char.IsLetter(""$Price""[0]) ? """" : ""$Price"".Substring(0, 3))  :
("",DRIP,DVOP,RHTS,EXOF,MRGR,"".IndexOf("",$CAEventInd,"")>=0 && "",SECU,EXER,"".IndexOf("",$CAOptionCode,"")>=0) ? (""$Price""=="""" || !Char.IsLetter(""$Price""[0]) ? """" : ""$Price"".Substring(0, 3)) : (colVal=="""" || !Char.IsLetter(colVal[0]) ? """" : colVal.Substring(0, 3))"
                                },
                            }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_Fee.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Fee",
                    templateFile = "BBH_Fee.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                        new PdtVariable {
                            name = "ClearingBrokerBIC",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[3]",
                        },
                    },
                    processingCondition = "\"$EventType\" == \"FE\" && (\"$ClearingBrokerBIC\" == \"GSILGB2X\" || (\"$ClearingBrokerBIC\" == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]" } }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new[] {
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"""$ClearingBrokerBIC"" == ""JPMSGB2L"" ? @""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + lineVal.Split('$CsvSrcSep')[5] + ""'"" : @""
SELECT IDENT FROM TIERS WHERE NAME = '"" + colVal + ""'"""
                                    },
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT R.VALUE FROM BO_TREASURY_EXT_REF R
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'RootPortfolio'
WHERE ACC_ID IN (SELECT ACC_ID FROM BO_TREASURY_EXT_REF WHERE VALUE = '"" + colVal + ""')""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'currency']" },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']/@*[local-name() = 'type']",
                                    expression = "colVal == \"AUD\" ? \"InRate\" : \"InAmount\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" && \"$ClearingBrokerBIC\" == \"JPMSGB2L\" && colVal==\"\" ? 0 : \"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? ((\"$ClearingBrokerBIC\" == \"JPMSGB2L\" ? 0 : double.Parse(lineVal.Split(',')[13])) + (\"$ClearingBrokerBIC\" == \"JPMSGB2L\" ? 0 : double.Parse(lineVal.Split(',')[14])) + double.Parse(lineVal.Split(',')[15])) : double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']",
                                expression = "\"$ClearingBrokerBIC\" == \"JPMSGB2L\" ? \"0\" : colVal"
                            } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']",
                                expression = "\"$ClearingBrokerBIC\" == \"JPMSGB2L\" ? \"0\" : colVal"
                            } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_JE.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Clearer Journal Entry",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"JE\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"$EventType\" == \"MC\" ? \"Clearer Margin Call\" : \"$EventType\" == \"JE\" ? \"Cash Transfer\" : \"Unknown\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'comment']",
                                    expression = "\"Clearer Journal Entry\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'origin']",
                                    expression = "\"Electronic\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]" } }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ACCOUNT_ADJUSTMENT_FOLIO
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'currency']" },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT SICOVAM FROM TITRES WHERE LIBELLE = 'Cash for currency ''"" + colVal + ""'''"""
                                    },
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Quantity", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : -double.Parse(colVal)"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : double.Parse(colVal)"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']",
                                    expression = "1"
                                }
                            }
                        },
                        new PdtColumn { name = "Bloomberg Ticker", isRequired = true, isRelativeToRootNode = true },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_IM_Orig.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Initial Margin Original",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"IM\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]" } }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT F_ORI.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN FOLIO F_MIR ON F_MIR.IDENT = A.ACCOUNT_ADJUSTMENT_FOLIO
    JOIN FOLIO F_ORI ON F_ORI.MGR = F_MIR.MGR AND F_ORI.NAME LIKE '% Cash'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_IM_Mirr.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Initial Margin Mirroring",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"IM\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]",
                                    expression = "colVal + \"_Mir\""
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A_MIR.ACCOUNT_ADJUSTMENT_FOLIO
FROM BO_TREASURY_ACCOUNT A_ORI
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A_ORI.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT A_MIR ON A_MIR.ACCOUNT_AT_CUSTODIAN = 
        CASE WHEN (A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'LU%')
            THEN SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 14) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 15)
        ELSE SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 4) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 5) END
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A_MIR.ID
FROM BO_TREASURY_ACCOUNT A_ORI
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A_ORI.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT A_MIR ON A_MIR.ACCOUNT_AT_CUSTODIAN = 
        CASE WHEN (A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'LU%')
            THEN SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 14) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 15)
        ELSE SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 4) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 5) END
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "10009350"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : -1 * double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_IM_Orig_Rev.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Initial Margin Original Reversal",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"IM\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]",
                                    expression = "colVal + \"_Rev\""
                                },
                                new PdtColumnDest
                                {
                                    path = "//*[local-name() = 'tradeDate']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT CASE WHEN TO_CHAR(H.DATECOMPTABLE, 'DY') = 'SAT' THEN TO_CHAR(H.DATECOMPTABLE+2, 'YYYY-MM-DD') ELSE TO_CHAR(H.DATECOMPTABLE, 'YYYY-MM-DD') END PNLDATE FROM HISTOMVTS H
JOIN EXTRNL_REFERENCES_TRADES R ON R.SOPHIS_IDENT = H.REFCON AND R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT CASE WHEN TO_CHAR(H.DATECOMPTABLE, 'DY') = 'SAT' THEN TO_CHAR(H.DATECOMPTABLE+2, 'YYYY-MM-DD') ELSE TO_CHAR(H.DATECOMPTABLE, 'YYYY-MM-DD') END PNLDATE FROM HISTOMVTS H
JOIN EXTRNL_REFERENCES_TRADES R ON R.SOPHIS_IDENT = H.REFCON AND R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT F_ORI.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN FOLIO F_MIR ON F_MIR.IDENT = A.ACCOUNT_ADJUSTMENT_FOLIO
    JOIN FOLIO F_ORI ON F_ORI.MGR = F_MIR.MGR AND F_ORI.NAME LIKE '% Cash'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            //destPaths = new [] {
                            //    new PdtColumnDest {
                            //        path = "//*[local-name() = 'tradeDate']",
                            //        expression = "\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                            //    },
                            //}
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            //destPaths = new [] {
                            //    new PdtColumnDest {
                            //        path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                            //        expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                            //    },
                            //}
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : -1 * double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_IM_Mirr.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Initial Margin Mirroring",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"IM\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]",
                                    expression = "colVal + \"_Mir\""
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A_MIR.ACCOUNT_ADJUSTMENT_FOLIO
FROM BO_TREASURY_ACCOUNT A_ORI
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A_ORI.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT A_MIR ON A_MIR.ACCOUNT_AT_CUSTODIAN = 
        CASE WHEN (A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'LU%')
            THEN SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 14) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 15)
        ELSE SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 4) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 5) END
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A_MIR.ID
FROM BO_TREASURY_ACCOUNT A_ORI
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A_ORI.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT A_MIR ON A_MIR.ACCOUNT_AT_CUSTODIAN = 
        CASE WHEN (A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'LU%')
            THEN SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 14) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 15)
        ELSE SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 4) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 5) END
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "10009350"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : -1 * double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_IM_Mirr_Rev.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Initial Margin Mirroring Reversal",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"IM\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]",
                                    expression = "colVal + \"_Mir_Rev\""
                                },
                                new PdtColumnDest
                                {
                                    path = "//*[local-name() = 'tradeDate']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT CASE WHEN TO_CHAR(H.DATECOMPTABLE, 'DY') = 'SAT' THEN TO_CHAR(H.DATECOMPTABLE+2, 'YYYY-MM-DD') ELSE TO_CHAR(H.DATECOMPTABLE, 'YYYY-MM-DD') END PNLDATE FROM HISTOMVTS H
JOIN EXTRNL_REFERENCES_TRADES R ON R.SOPHIS_IDENT = H.REFCON AND R.VALUE = '"" + colVal + ""' || '_Mir'""",
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT CASE WHEN TO_CHAR(H.DATECOMPTABLE, 'DY') = 'SAT' THEN TO_CHAR(H.DATECOMPTABLE+2, 'YYYY-MM-DD') ELSE TO_CHAR(H.DATECOMPTABLE, 'YYYY-MM-DD') END PNLDATE FROM HISTOMVTS H
JOIN EXTRNL_REFERENCES_TRADES R ON R.SOPHIS_IDENT = H.REFCON AND R.VALUE = '"" + colVal + ""' || '_Mir'""",
                                    },
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A_MIR.ACCOUNT_ADJUSTMENT_FOLIO
FROM BO_TREASURY_ACCOUNT A_ORI
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A_ORI.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT A_MIR ON A_MIR.ACCOUNT_AT_CUSTODIAN = 
        CASE WHEN (A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'LU%')
            THEN SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 14) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 15)
        ELSE SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 4) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 5) END
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A_MIR.ID
FROM BO_TREASURY_ACCOUNT A_ORI
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A_ORI.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT A_MIR ON A_MIR.ACCOUNT_AT_CUSTODIAN = 
        CASE WHEN (A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A_ORI.ACCOUNT_AT_CUSTODIAN LIKE 'LU%')
            THEN SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 14) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 15)
        ELSE SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 0, 4) || 'IM' || SUBSTR(A_ORI.ACCOUNT_AT_CUSTODIAN, 5) END
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "10009350"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            //destPaths = new [] {
                            //    new PdtColumnDest {
                            //        path = "//*[local-name() = 'tradeDate']",
                            //        expression = "\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                            //    },
                            //}
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            //destPaths = new [] {
                            //    new PdtColumnDest {
                            //        path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                            //        expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                            //    },
                            //}
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_MI.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Monthly Interest",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"MI\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]" } }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT F_ORI.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN FOLIO F_MIR ON F_MIR.IDENT = A.ACCOUNT_ADJUSTMENT_FOLIO
    JOIN FOLIO F_ORI ON F_ORI.MGR = F_MIR.MGR AND F_ORI.NAME LIKE '% Cash'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" ? -double.Parse(colVal) : double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_MC_Orig.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Margin Calls Original",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"MC\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]" } }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT F_ORI.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN FOLIO F_MIR ON F_MIR.IDENT = A.ACCOUNT_ADJUSTMENT_FOLIO
    JOIN FOLIO F_ORI ON F_ORI.MGR = F_MIR.MGR AND F_ORI.NAME LIKE '% Cash'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A_ORI.ID
FROM BO_TREASURY_ACCOUNT A_MIR
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A_MIR.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT A_ORI ON A_ORI.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A_MIR.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A_MIR.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A_MIR.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A_MIR.ACCOUNT_AT_CUSTODIAN, 0, 4) END
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "(\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" || lineVal.Split(',')[3] == \"JPMSGB2L\") ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_MC_Mirr.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary Margin Calls Mirroiring",
                    templateFile = "BBH_IM.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable {
                            name = "EventType",
                            expressionBefore = "lineVal.Split(',')[0] == \"Clearing Broker Fees\" ? \"FE\" : lineVal.Split(',')[0] == \"Initial Margin\" ? \"IM\" : lineVal.Split(',')[0] == \"Monthly Interest\" ? \"MI\" : lineVal.Split(',')[0] == \"Clearer Margin Calls\" ? \"MC\" : lineVal.Split(',')[0] == \"Clearer Journal Entry\" ? \"JE\" : \"Unknown\"",
                        },
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[6]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "\"$EventType\" == \"MC\" && (lineVal.Split('$CsvSrcSep')[3] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[3] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Event Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"$EventType\" == \"FE\" ? \"Clearing Broker Fees\" : \"$EventType\" == \"IM\" ? \"Initial Margin\" : \"$EventType\" == \"MI\" ? \"Clearing Broker Interests\" : \"Clearer Margin Call\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Sender Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]",
                                    expression = "colVal + \"_Mir\""
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Clearer Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ACCOUNT_ADJUSTMENT_FOLIO
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "CommonId",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "(\"$EventType\" == \"FE\" || \"$EventType\" == \"IM\" || \"$EventType\" == \"JE\" || lineVal.Split(',')[3] == \"JPMSGB2L\") ? (colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")) : (lineVal.Split(',')[9].Trim() == \"\" ? \"\" : System.DateTime.ParseExact(lineVal.Split(',')[9], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\"))"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? System.DateTime.ParseExact(lineVal.Split(',')[8], \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "\"$EventType\" == \"FE\" ? double.Parse(colVal) : 1",
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "\"$EventType\" == \"FE\" ? (double.Parse(lineVal.Split(',')[13]) + double.Parse(lineVal.Split(',')[14]) + double.Parse(lineVal.Split(',')[15])) : -1 * double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Bloomberg Ticker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference']",
                                    expression = "\"$EventType\" == \"FE\" ? colVal : (\"$EventType\" == \"IM\" ? \"INITIAL MARGIN \" : \"$EventType\" == \"MC\" ? \"MARGIN CALL \" : \"MONTHLY INTEREST \") + lineVal.Split(',')[7]"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'reference']/@*[local-name() = 'name']",
                                //    expression = "\"$EventType\" == \"FE\" ? (colVal.IndexOf(' ') >= 0 ? \"Ticker\" : \"BbgB3Id\") : \"Sophisref\""
                                //},
                            }
                        },
                        new PdtColumn {
                            name = "Broker fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'brokerFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Counterparty fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Market fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_DIM_Trade.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Trade",
                    templateFile = "BBH_DIM_Trade_Nostro.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = '|',
                    variables = new[] {
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[3]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                        new PdtVariable {
                            name = "TransactionType",
                            expressionBefore = "lineVal.Split('|')[5]",
                        },
                        new PdtVariable {
                            name = "SecurityType",
                            expressionBefore = "lineVal.Split('|')[6]",
                        },
                        new PdtVariable {
                            name = "DealAmount",
                            expressionBefore = "lineVal.Split('|')[16] == \"\" ? 0 : double.Parse(lineVal.Split('|')[16])",
                        },
                        new PdtVariable {
                            name = "Entity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT ENTITE FROM (
SELECT CONNECT_BY_ROOT IDENT IDENT, CONNECT_BY_ROOT MGR MGR, SYS_CONNECT_BY_PATH(NAME, ':') PATH, CONNECT_BY_ROOT ENTITE ENTITE, LEVEL
FROM FOLIO WHERE IDENT =
    (SELECT R.VALUE AS ROOTPORTFOLIO
    FROM BO_TREASURY_ACCOUNT A
        JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
        JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'RootPortfolio'
    WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ACCOUNT_AT_CUSTODIAN = '"" + lineVal.Split('|')[3] + @""')
CONNECT BY PRIOR IDENT = MGR
) WHERE """"LEVEL"""" = 3"""
                            },
                        },
                        new PdtVariable
                        {
                            name = "Ctpy",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + lineVal.Split('|')[42] + ""'"""
                            },
                        },
                        new PdtVariable
                        {
                            name = "Allotment",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT A.LIBELLE FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM
    JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
        AND ERD.REF_NAME IN ('BBG_B3_ID', 'CUSIP', 'TICKER', 'ID_BB_UNIQUE')
    JOIN AFFECTATION A ON A.IDENT = T.AFFECTATION
WHERE ERI.VALUE = '"" + lineVal.Split('|')[68] + @""'
UNION
SELECT A.LIBELLE FROM TITRES T
    JOIN AFFECTATION A ON A.IDENT = T.AFFECTATION
WHERE T.REFERENCE = '"" + lineVal.Split('|')[68] + ""'"""
                            },
                        }
                    },
                    processingCondition = "(\"$SecurityType\" == \"OPT\" || \"$SecurityType\" == \"FUT\") && lineVal.Split('|')[1] == \"N\" && (lineVal.Split('$CsvSrcSep')[40] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[40] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Sender's Message Reference Id",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]" } }
                        },
                        new PdtColumn {
                            name = "Message Type/Function",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'persistenceType']",
                                    expression = "\"UpdateOrCreate\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"Purchase/Sale\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'backofficeStatus']",
                                    expression = "\"No Status\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'comment']",
                                    expression = "DateTime.Now.ToString(\"yyyyMMdd-HH:mm:ss\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Previous Message Reference ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Portfolio Code/Custodian Safekeeping Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT R.VALUE AS ROOTPORTFOLIO
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'RootPortfolio'
WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ACCOUNT_AT_CUSTODIAN = '"" + colVal + ""'"""
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']"
                                },
//                                new PdtColumnDest {
//                                    Lookup = new PdtColumnLookup {
//                                        Table = "SQL",
//                                        Expression = @"@""
//SELECT F.NAME
//FROM BO_TREASURY_ACCOUNT A
//    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
//    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'RootPortfolio'
//    JOIN FOLIO F ON F.IDENT = R.VALUE
//WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ACCOUNT_AT_CUSTODIAN = '"" + colVal + ""'"""
//                                    },
//                                    path = "//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/name')]"
//                                },
//                                new PdtColumnDest {
//                                    Lookup = new PdtColumnLookup {
//                                        Table = "SQL",
//                                        Expression = @"@""
//SELECT SUBSTR(PATH, 2) FULLNAME FROM (
//SELECT CONNECT_BY_ROOT MGR MGR, SYS_CONNECT_BY_PATH(NAME, ':') PATH
//FROM FOLIO WHERE IDENT =
//    (SELECT R.VALUE AS ROOTPORTFOLIO
//    FROM BO_TREASURY_ACCOUNT A
//        JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
//        JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'RootPortfolio'
//    WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ACCOUNT_AT_CUSTODIAN = '"" + colVal + @""')
//CONNECT BY PRIOR IDENT = MGR
//) WHERE MGR IS NULL"""
//                                    },
//                                    path = "//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/fullName')]"
//                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'orderer']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "\"$Entity\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "\"$TransactionType\" == \"BY\" ? $Entity : $Ctpy"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "\"$TransactionType\" == \"SL\" ? $Entity : $Ctpy"
                                },
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'payerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                //    expression = "\"$Entity\""
                                //},
                                //new PdtColumnDest {
                                //    path = "//*[local-name() = 'receiverPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                //    expression = "\"$Entity\""
                                //},
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT DEPOSITARY FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = '\" + colVal + \"'\"",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "\"$Ctpy\""
                                },
                                //new PdtColumnDest { path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'reference') or contains(@*[local-name() = 'partyIdScheme'], 'externalReference')]" },
                            }
                        },
                        new PdtColumn {
                            name = "Account Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Transaction Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Security Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Security Identifier",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Security Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Sophisref']" } }
                        },
                        new PdtColumn {
                            name = "Issue Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Interest Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Maturity Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Original Face Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Trade Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "(\"$TransactionType\" == \"BY\" ? \"+\" : \"+\") + colVal"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Deal Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']",
                                    expression = "\"$Allotment\" == \"CCY FUTURE\" ? (lineVal.Split('|')[15]==\"GBP\" ? double.Parse(colVal)/100 : lineVal.Split('|')[15]==\"CHF\" ? double.Parse(colVal)/10 : double.Parse(colVal)) : double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "ISO Currency Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'amount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'grossAmount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'paymentAmount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'detailedFee']//*[local-name() = 'currency']" },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']/@*[local-name() = 'type']",
                                    expression = "\"$Allotment\" == \"INT RATE FUTURE\" && colVal == \"AUD\" ? \"InRate\" : \"InAmount\""
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Principal/Deal Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'grossAmount']//*[local-name() = 'amount']",
                                    expression = "$DealAmount == 0 ? double.Parse(lineVal.Split('|')[22]) : $DealAmount"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Commission Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            //destPaths = new [] {
                            //    new PdtColumnDest {
                            //        path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'couterpartyFees')]//*[local-name() = 'amount']",
                            //        expression = "colVal == \"\" ? \"0\" : colVal"
                            //    }
                            //}
                        },
                        new PdtColumn {
                            name = "Charges/Fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
//                            destPaths = new [] {
//                                new PdtColumnDest {
//                                    path = "//*[local-name() = 'otherPartyPayment'][contains(@*[local-name() = 'paymentTypeScheme'], 'marketFees')]//*[local-name() = 'amount']",
//                                    expression = @"
//(lineVal.Split('|')[18] == """" ? 0 : double.Parse(lineVal.Split('|')[18])) + 
//(lineVal.Split('|')[19] == """" ? 0 : double.Parse(lineVal.Split('|')[19])) + 
//(lineVal.Split('|')[20] == """" ? 0 : double.Parse(lineVal.Split('|')[20])) + 
//(lineVal.Split('|')[24] == """" ? 0 : double.Parse(lineVal.Split('|')[24])) + 
//(lineVal.Split('|')[25] == """" ? 0 : double.Parse(lineVal.Split('|')[25]))"
//                                }
//                            }
                        },
                        new PdtColumn {
                            name = "SEC/ Other Amounts",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Local Tax Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Accrued Interest Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Net Settlement Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            //destPaths = new [] {
                            //    new PdtColumnDest {
                            //        path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                            //        expression = "\"$SecurityType\" == \"OPT\" ? colVal : \"0\""
                            //    }
                            //}
                        },
                        new PdtColumn {
                            name = "Non Local Currency Settlement Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Stamp Duty Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Value Added Tax Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Net / Gain Loss Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate'] | //*[local-name() = 'unadjustedDate']",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'pnLDate']",
                                    expression = "DateTime.Today.ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Settlement Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate'] | //*[local-name() = 'settlementDate']",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Place of Settlement BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Country of Settlement ISO Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Place of Safekeeping BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Place of Trade",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker Local ID Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker Local ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker Beneficiary Account Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker Local ID Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker Local ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker Beneficiary Account Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT S.SSI_PATH_ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_SSI_PATH S ON S.BO_TREASURY_ACCOUNT_ID = A.ID AND S.SSI_PATH_ID IN (SELECT SSI_PATH_ID FROM TIERSSETTLEMENT)
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'broker']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT CUSR.VALUE
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN BO_TREASURY_EXT_REF CUSR ON CUSR.ACC_ID = CUS.ID
    JOIN BO_TREASURY_EXT_REF_DEF CUSD ON CUSD.REF_ID = CUSR.REF_ID AND CUSD.REF_NAME = 'DelegateManagerID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Receiver/ Deliverer’s Agent BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver/Deliverer’s Agent Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver/Deliverer’s Beneficiary Account Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Instruction Line",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "FX Instructions – Currency to Buy/Sell",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exchange Rate - First Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exchange Rate – Second Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exchange Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Tax Identifier",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Turnaround Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Pair Off /TBA Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Dirty/Clean Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Physical Trade Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Stock Loan Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Book Transfer Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Settlement System Override Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Registration Override Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Trade Transaction Condition Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Form of Securities Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Split Settlement",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Change of Beneficial Ownership",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Stamp Duty Type Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Short Sale/Buy to Cover Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Settlement System Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "FX Instruction Cancellation Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "ID of Financial Instrument – Listed Derivatives",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'reference']" } }
                        },
                        new PdtColumn {
                            name = "Currency of Denomination",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Split Settlement",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Expiration Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Open/Close Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Underlying Security ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Underlying Security Description",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Option Style",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Option Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exercise Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Contract Size",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Message Recipient",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Quantity Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Price Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Action Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Transaction Reference Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Second Leg Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Termination Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Spread Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Variable Rate Support",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Pricing Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Stock Loan Margin",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Securities Haircut",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Accrual Basis",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Rate Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Rate Change Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Total Pieces of Collateral",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Settlement Instruction Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Interest Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Termination Amount (P&I) per piece of collateral",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Total Termination Amount (P&I)",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Deal Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Forfeit Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Premium Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Link with Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Revaluation Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Second Leg Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account With Local ID Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account With Local ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account With BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account With Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Local ID Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Local ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Beneficiary Account Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Narrative Party",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Narrative Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Collateral Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exposure Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Partial Settlement",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Buy In",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Related Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Buy In Partial Successful",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "UTI- Unique Transaction ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        }
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_DIM_Trade_Cancel.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Trade Cancel",
                    templateFile = "BBH_DIM_Trade_Cancel_Ack.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = '|',
                    variables = new[] {
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[3]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                    },
                    processingCondition = "lineVal.Split('|')[1] == \"C\" && (lineVal.Split('$CsvSrcSep')[40] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[40] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Sender's Message Reference Id",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'updateWorkflowEventName']",
                                    expression = "\"BBH Upload\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Message Type/Function",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Previous Message Reference ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT SOPHIS_IDENT FROM EXTRNL_REFERENCES_TRADES WHERE VALUE = '\" + colVal + \"'\"",
                                    },
                                    path = "//*[local-name() = 'tradeId']"
                                }
                            }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_DIM_Trade_Ack.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Trade Ack",
                    templateFile = "BBH_DIM_Trade_Cancel_Ack.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = '|',
                    columns = new [] {
                        new PdtColumn {
                            name = "Sender's Message Reference Id",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new[]
                            {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'updateWorkflowEventName']",
                                    expression = "\"Ack Received\""
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT TRADE_ID FROM BO_MESSAGES WHERE IDENT = \" + colVal",
                                    },
                                    path = "//*[local-name() = 'tradeId']"
                                }
                            }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_DIM_Cash_Orig.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Cash Original",
                    templateFile = "BBH_DIM_Cash_Nostro.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = '|',
                    variables = new[] {
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[3]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                        new PdtVariable {
                            name = "MessageType",
                            expressionBefore = "lineVal.Split('|')[0]",
                        },
                        new PdtVariable {
                            name = "Entity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + lineVal.Split('|')[5] + ""'"""
                            },
                        },
                        new PdtVariable
                        {
                            name = "Ctpy",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + lineVal.Split('|')[5] + ""'"""
                            },
                        }
                    },
                    processingCondition = "($MessageType == 202 || $MessageType == 210) && lineVal.Split('|')[6] == \"MARG\" && (lineVal.Split('$CsvSrcSep')[40] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[40] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Message Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'persistenceType']",
                                    expression = "\"UpdateOrCreate\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"Clearer Margin Call\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'backofficeStatus']",
                                    expression = "\"No Status\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'comment']",
                                    expression = "DateTime.Now.ToString(\"yyyyMMdd-HH:mm:ss\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeTime']",
                                    expression = "DateTime.Now.ToString(\"HH:mm:ss\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "1"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']",
                                    expression = "0"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']/@*[local-name() = 'type']",
                                    expression = "\"InAmount\""
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Recipient",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Transaction/ Senders Reference Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]" } }
                        },
                        new PdtColumn {
                            name = "Time Indication Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Time Indication Time",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT F_ORI.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN FOLIO F_MIR ON F_MIR.IDENT = A.ACCOUNT_ADJUSTMENT_FOLIO
    JOIN FOLIO F_ORI ON F_ORI.MGR = F_MIR.MGR AND F_ORI.NAME LIKE '% Cash'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "\"$Entity\""
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "\"$Ctpy\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A_ORI.ID
FROM BO_TREASURY_ACCOUNT A_MIR
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A_MIR.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT A_ORI ON A_ORI.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A_MIR.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A_MIR.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A_MIR.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A_MIR.ACCOUNT_AT_CUSTODIAN, 0, 4) END
WHERE R.VALUE = '"" + colVal + ""'"""
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Related Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Related Reference of the Original Message",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Original Instruction Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate'] | //*[local-name() = 'unadjustedDate']",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'settlementDate'] | //*[local-name() = 'pnLDate']",
                                    expression = "DateTime.Today.ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Currency Code of Settled Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'amount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'grossAmount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'paymentAmount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'detailedFee']//*[local-name() = 'currency']" },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Sophisref']",
                                    expression = "\"MARGIN CALL \" + colVal"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Settled Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Ordering Customer BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Party Identifier Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Country Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Party Identifier",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Institution BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Institution Clearing System Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Institution Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Institution Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender's Correspondent BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender's Correspondent Location",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender's Correspondent Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver's Correspondent BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver's Correspondent Location",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver's Correspondent Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver's Correspondent Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Intermediary Institution BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Intermediary Institution Clearing System Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Intermediary Institution Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Intermediary Institution Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution Clearing System Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution Location",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Beneficiary Institution BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Beneficiary Institution Clearing Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Beneficiary Institution Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Beneficiary Institution Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information First line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information 2nd line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information 3rd line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information – 4th line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information – 5th line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information – 6th line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information for Cancel Message",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_DIM_Cash_Mirr.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Cash Mirroring",
                    templateFile = "BBH_DIM_Cash_Nostro.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = '|',
                    variables = new[] {
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[3]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                        new PdtVariable {
                            name = "MessageType",
                            expressionBefore = "lineVal.Split('|')[0]",
                        },
                        new PdtVariable {
                            name = "Entity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT A.ENTITY
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + lineVal.Split('|')[5] + ""'"""
                            },
                        },
                        new PdtVariable
                        {
                            name = "Ctpy",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN TIERS T ON T.REFERENCE = CUS.CUSTODIAN
WHERE R.VALUE = '"" + lineVal.Split('|')[5] + ""'"""
                            },
                        }
                    },
                    processingCondition = "($MessageType == 202 || $MessageType == 210) && lineVal.Split('|')[6] == \"MARG\" && (lineVal.Split('$CsvSrcSep')[40] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[40] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Message Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'persistenceType']",
                                    expression = "\"UpdateOrCreate\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"Clearer Margin Call\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'backofficeStatus']",
                                    expression = "\"No Status\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'comment']",
                                    expression = "DateTime.Now.ToString(\"yyyyMMdd-HH:mm:ss\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeTime']",
                                    expression = "DateTime.Now.ToString(\"HH:mm:ss\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "1"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']",
                                    expression = "0"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']/@*[local-name() = 'type']",
                                    expression = "\"InAmount\""
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Recipient",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Transaction/ Senders Reference Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]",
                                    expression = "colVal + \"_Mir\""
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Time Indication Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Time Indication Time",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ACCOUNT_ADJUSTMENT_FOLIO
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']",
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'buyerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entityPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'entity']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "\"$Entity\""
                                },
                                new PdtColumnDest {
                                    path = @"
//*[local-name() = 'counterparty']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')] |
//*[local-name() = 'sellerPartyReference']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                    expression = "\"$Ctpy\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'accountId']",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT A.ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Related Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Related Reference of the Original Message",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Original Instruction Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate'] | //*[local-name() = 'unadjustedDate']",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'settlementDate'] | //*[local-name() = 'pnLDate']",
                                    expression = "DateTime.Today.ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Currency Code of Settled Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'amount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'grossAmount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'paymentAmount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'detailedFee']//*[local-name() = 'currency']" },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Sophisref']",
                                    expression = "\"MARGIN CALL \" + colVal"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Settled Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']",
                                    expression = "-1 * double.Parse(colVal)"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Ordering Customer BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Party Identifier Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Country Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Party Identifier",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Customer Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Institution BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Institution Clearing System Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Institution Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Ordering Institution Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender's Correspondent BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender's Correspondent Location",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender's Correspondent Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver's Correspondent BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver's Correspondent Location",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver's Correspondent Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver's Correspondent Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Intermediary Institution BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Intermediary Institution Clearing System Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Intermediary Institution Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Intermediary Institution Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution Clearing System Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution Location",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Account with Institution Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Beneficiary Institution BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Beneficiary Institution Clearing Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Beneficiary Institution Name & Address",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Beneficiary Institution Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information First line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information 2nd line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information 3rd line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information – 4th line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information – 5th line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information – 6th line Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Information for Cancel Message",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.FileConnector_ErrorParser.ToString(),
                    type = TransType.Xml2Csv,
                    label = "FileConnector Error Parser",
                    category = "System",
                    csvSrcSeparator = DEFAULT_CSV_SEPARATOR,
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "External Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'elementRejected']//*[local-name() = 'tradeId']" } }
                        },
                        new PdtColumn
                        {
                            name = "Business Event",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'elementRejected']//*[local-name() = 'businessEvent']" } }
                        },
                        new PdtColumn
                        {
                            name = "Portfolio ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'elementRejected']//*[local-name() = 'portfolioName']" } }
                        },
                        new PdtColumn
                        {
                            name = "Instrument Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'elementRejected']//*[local-name() = 'tradeProduct']//*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Sophisref']" } }
                        },
                        new PdtColumn
                        {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'elementRejected']//*[local-name() = 'tradeDate']" } }
                        },
                        new PdtColumn
                        {
                            name = "Value Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'elementRejected']//*[local-name() = 'tradeHeader']//*[local-name() = 'paymentDate']" } }
                        },
                        new PdtColumn
                        {
                            name = "Exception",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'elementRejected']//*[local-name() = 'exception']" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.BBH_DIM_Trade_RMA.ToString(),
                    type = TransType.Csv2Csv,
                    label = "Infomediary DIM Trade RMA Preparation",
                    templateFile = "RMA_GenericTrade.csv",
                    category = "Medio",
                    csvSrcSeparator = '|',
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "OnBoarded",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[3]",
                            Lookup = new PdtColumnLookup {
                                Table = "MEDIO_BBH_FUNDFILTER",
                                ColumnIndex = "-1",
                            },
                        },
                        new PdtVariable {
                            name = "TransactionType",
                            expressionBefore = "lineVal.Split('|')[5]",
                        },
                        new PdtVariable {
                            name = "SecurityType",
                            expressionBefore = "lineVal.Split('|')[6]",
                        },
                        new PdtVariable
                        {
                            name = "Ctpy",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT T.IDENT
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN TIERS T ON T.REFERENCE = A.CUSTODIAN
WHERE R.VALUE = '"" + lineVal.Split('|')[42] + ""'"""
                            },
                        },
                        new PdtVariable
                        {
                            name = "Allotment",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT A.LIBELLE FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM
    JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
        AND ERD.REF_NAME IN ('BBG_B3_ID', 'CUSIP', 'TICKER', 'ID_BB_UNIQUE')
    JOIN AFFECTATION A ON A.IDENT = T.AFFECTATION
WHERE ERI.VALUE = '"" + lineVal.Split('|')[68] + @""'
UNION
SELECT A.LIBELLE FROM TITRES T
    JOIN AFFECTATION A ON A.IDENT = T.AFFECTATION
WHERE T.REFERENCE = '"" + lineVal.Split('|')[68] + ""'"""
                            },
                        }
                    },
                    processingCondition = "(\"$SecurityType\" == \"OPT\" || \"$SecurityType\" == \"FUT\") && lineVal.Split('|')[1] == \"N\" && (lineVal.Split('$CsvSrcSep')[40] == \"GSILGB2X\" || (lineVal.Split('$CsvSrcSep')[40] == \"JPMSGB2L\" && \"$OnBoarded\" != \"\"))",
                    columns = new [] {
                        new PdtColumn {
                            name = "Sender's Message Reference Id",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "ExternalRef" } }
                        },
                        new PdtColumn {
                            name = "Message Type/Function",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "TradeType",
                                    expression = "\"Purchase/Sale\""
                                },
                                new PdtColumnDest {
                                    path = "Info",
                                    expression = "\"BBH_DIM_Trade_RMA\""
                                },
                                new PdtColumnDest {
                                    path = "Comments",
                                    expression = "DateTime.Now.ToString(\"yyyyMMdd-HH:mm:ss\")"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT ID FROM BO_KERNEL_EVENTS WHERE NAME = 'BBH Upload'\""
                                    },
                                    path = "EventId"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT IDENT FROM RISKUSERS WHERE NAME = 'BBHUploader'\""
                                    },
                                    path = "UserId"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Previous Message Reference ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Portfolio Code/Custodian Safekeeping Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT R.VALUE AS ROOTPORTFOLIO
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'RootPortfolio'
WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ACCOUNT_AT_CUSTODIAN = '"" + colVal + ""'"""
                                    },
                                    path = "BookId"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT DEPOSITARY FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = '\" + colVal + \"'\"",
                                    },
                                    path = "DepositaryId",
                                },
                                new PdtColumnDest {
                                    path = "CounterpartyId",
                                    expression = "\"$Ctpy\""
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Account Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Transaction Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Security Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Security Identifier",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Security Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Issue Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Interest Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Maturity Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Original Face Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Trade Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "Quantity",
                                expression = "(\"$TransactionType\" == \"BY\" ? \"+\" : \"-\") + colVal"
                            } }
                        },
                        new PdtColumn {
                            name = "Deal Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "Spot",
                                expression = "\"$Allotment\" == \"CCY FUTURE\" ? (lineVal.Split('|')[15]==\"GBP\" ? double.Parse(colVal)/100 : lineVal.Split('|')[15]==\"CHF\" ? double.Parse(colVal)/10 : double.Parse(colVal)) : double.Parse(colVal)"
                            } }
                        },
                        new PdtColumn {
                            name = "ISO Currency Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "Currency" },
                                new PdtColumnDest {
                                    path = "SpotType",
                                    expression = "\"$Allotment\" == \"INT RATE FUTURE\" && colVal == \"AUD\" ? 1 : 0"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Principal/Deal Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Commission Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Charges/Fees",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "SEC/ Other Amounts",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Local Tax Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Accrued Interest Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Net Settlement Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Non Local Currency Settlement Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Stamp Duty Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Value Added Tax Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Net / Gain Loss Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "TradeDate",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Settlement Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "ValueDate",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Place of Settlement BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Country of Settlement ISO Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Place of Safekeeping BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Place of Trade",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker Local ID Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker Local ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Executing Broker Beneficiary Account Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker Local ID Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker Local ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Clearing Broker Beneficiary Account Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            {
                                new PdtColumnDest {
                                    path = "ExtraFields",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT 'NOSTRO_CASH_ID=' || S.SSI_PATH_ID
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_SSI_PATH S ON S.BO_TREASURY_ACCOUNT_ID = A.ID AND S.SSI_PATH_ID IN (SELECT SSI_PATH_ID FROM TIERSSETTLEMENT)
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                                new PdtColumnDest {
                                    path = "BrokerId",
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT CUSR.VALUE
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'CBAccountID'
    JOIN BO_TREASURY_ACCOUNT CUS ON CUS.ACCOUNT_AT_CUSTODIAN = CASE WHEN (A.ACCOUNT_AT_CUSTODIAN LIKE 'DB%' OR A.ACCOUNT_AT_CUSTODIAN LIKE 'LU%') THEN SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 14) ELSE SUBSTR(A.ACCOUNT_AT_CUSTODIAN, 0, 4) END
    JOIN BO_TREASURY_EXT_REF CUSR ON CUSR.ACC_ID = CUS.ID
    JOIN BO_TREASURY_EXT_REF_DEF CUSD ON CUSD.REF_ID = CUSR.REF_ID AND CUSD.REF_NAME = 'DelegateManagerID'
WHERE R.VALUE = '"" + colVal + ""'""",
                                    },
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Receiver/ Deliverer’s Agent BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver/Deliverer’s Agent Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Receiver/Deliverer’s Beneficiary Account Code",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Sender to Receiver Instruction Line",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "FX Instructions – Currency to Buy/Sell",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exchange Rate - First Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exchange Rate – Second Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exchange Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Tax Identifier",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Turnaround Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Pair Off /TBA Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Dirty/Clean Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Physical Trade Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Stock Loan Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Book Transfer Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Settlement System Override Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Registration Override Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Trade Transaction Condition Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Form of Securities Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Split Settlement",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Change of Beneficial Ownership",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Stamp Duty Type Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Short Sale/Buy to Cover Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Settlement System Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "FX Instruction Cancellation Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "ID of Financial Instrument – Listed Derivatives",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT T.SICOVAM FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM
    JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
        AND ERD.REF_NAME IN ('BBG_B3_ID', 'CUSIP', 'TICKER', 'ID_BB_UNIQUE')
WHERE ERI.VALUE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM FROM TITRES T WHERE T.REFERENCE = '"" + colVal + ""'"""
                                },
                                path = "InstrumentRef"
                            } }
                        },
                        new PdtColumn {
                            name = "Currency of Denomination",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Split Settlement",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Expiration Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Open/Close Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Underlying Security ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Underlying Security Description",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Option Style",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Option Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exercise Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Contract Size",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Message Recipient",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Quantity Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Price Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Action Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Transaction Reference Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Second Leg Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Termination Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Spread Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Variable Rate Support",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Pricing Rate",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Stock Loan Margin",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Securities Haircut",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Accrual Basis",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Rate Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Rate Change Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Total Pieces of Collateral",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Settlement Instruction Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Interest Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Termination Amount (P&I) per piece of collateral",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Total Termination Amount (P&I)",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Deal Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Forfeit Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Premium Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Link with Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Revaluation Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Repo Second Leg Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account With Local ID Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account With Local ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account With BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account With Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Local ID Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Local ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party BIC",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Name",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Beneficiary Account Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Cash Party Account Number",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Narrative Party",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Narrative Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Collateral Indicator",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Exposure Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Partial Settlement",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Buy In",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Related Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Buy In Partial Successful",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "UTI- Unique Transaction ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        }
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSBAllCustody_Prep.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSBAllCustody Preparation",
                    templateFile = "SSB_AllCustodyTransactions.csv",
                    category = "Medio",
                    csvSkipLines = 1,
                    csvSrcSeparator = ';',
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    //uniqueConstraints = new[]
                    //{
                    //    "lineVal.Split('$CsvSrcSep')[0] + lineVal.Split('$CsvSrcSep')[19] + lineVal.Split('$CsvSrcSep')[9] + lineVal.Split('$CsvSrcSep')[10]"
                    //},
                    checkConstraints = new[]
                    {
                        "!string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[0]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[19]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[10]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[14]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[3])"
                    },
                    variables = new[] {
                        new PdtVariable {
                            name = "LastMfTS",
                            expressionStorage = "NoStorage",
                        },
                        new PdtVariable {
                            name = "MfTS",
                            expressionBefore = "new DateTime(Math.Min(DateTime.Today.Ticks-1, DateTime.ParseExact(lineVal.Split('$CsvSrcSep')[8], \"MM/dd/yyyy HH:mm:ss\", CultureInfo.InvariantCulture).Ticks)).ToString(\"yyyyMMdd HHmmss\")",
                        },
                        new PdtVariable {
                            name = "MaxMfTS",
                            expressionStorage = "NoStorage",
                            expressionBefore = "string.Compare(\"$MfTS\", \"$MaxMfTS\") >= 0 && (lineVal.Split('$CsvSrcSep')[7] == \"PAID\" || lineVal.Split('$CsvSrcSep')[7] == \"STCS\") && lineVal.Split('$CsvSrcSep')[16] != \"ACCT\" && lineVal.Split('$CsvSrcSep')[9] != \"INTERNAL TRANSACTION\" ? \"$MfTS\" : \"$MaxMfTS\"",
                        },
                    },
                    processingCondition = "(lineVal.Split('$CsvSrcSep')[7] == \"PAID\" || lineVal.Split('$CsvSrcSep')[7] == \"STCS\") && lineVal.Split('$CsvSrcSep')[16] != \"ACCT\" && lineVal.Split('$CsvSrcSep')[9] != \"INTERNAL TRANSACTION\" && string.Compare(\"$MfTS\", \"$LastMfTS\") > 0 && string.Compare(\"$MfTS\", DateTime.Today.AddTicks(-1).ToString(\"yyyyMMdd HHmmss\")) < 0",
                    columns = new [] {
                        new PdtColumn {
                            name = "Fund",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Fund" } }
                        },
                        new PdtColumn {
                            name = "SS Asset ID",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "SS Asset ID" } }
                        },
                        new PdtColumn {
                            name = "Share Quantity",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Share Quantity" } }
                        },
                        new PdtColumn {
                            name = "Actual Net Amount",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Actual Net Amount" } }
                        },
                        new PdtColumn {
                            name = "Principal",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Principal" } }
                        },
                        new PdtColumn {
                            name = "Interest",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Interest" } }
                        },
                        new PdtColumn {
                            name = "Settle Loc",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Settle Loc" } }
                        },
                        new PdtColumn {
                            name = "SS Status",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "SS Status" } }
                        },
                        new PdtColumn {
                            name = "Mainframe Time Stamp",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Mainframe Time Stamp" } }
                        },
                        new PdtColumn {
                            name = "Transaction Description",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Transaction Description" } }
                        },
                        new PdtColumn {
                            name = "Actual Pay/Settle Date",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Actual Pay/Settle Date" } }
                       },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Executing Broker" } }
                        },
                        new PdtColumn {
                            name = "Current Factor",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Current Factor" } }
                        },
                        new PdtColumn {
                            name = "Prior Factor",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Prior Factor" } }
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Currency" } }
                        },
                        new PdtColumn {
                            name = "Trade/Record Date",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Trade/Record Date" } }
                        },
                        new PdtColumn {
                            name = "Related Trade ID",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Related Trade ID" } }
                        },
                        new PdtColumn {
                            name = "Comments",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Comments" } }
                        },
                        new PdtColumn {
                            name = "Client reference#",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Client reference#" } }
                        },
                        new PdtColumn {
                            name = "SS Trade ID",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "SS Trade ID" } }
                        },
                        new PdtColumn {
                            name = "SS Trans Type",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "SS Trans Type" } }
                        },
                        new PdtColumn {
                            name = "Isin",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Isin" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSBAllCustody_Prep_Mirr.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSBAllCustody Preparation Miroir",
                    templateFile = "SSB_AllCustodyTransactions.csv",
                    category = "Medio",
                    csvSkipLines = 1,
                    csvSrcSeparator = ';',
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    //uniqueConstraints = new[]
                    //{
                    //    "lineVal.Split('$CsvSrcSep')[0] + lineVal.Split('$CsvSrcSep')[19] + lineVal.Split('$CsvSrcSep')[9] + lineVal.Split('$CsvSrcSep')[10]"
                    //},
                    checkConstraints = new[]
                    {
                        "!string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[0]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[19]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[10]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[14]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[3])"
                    },
                    variables = new[] {
                        new PdtVariable {
                            name = "LastMfTS",
                            expressionStorage = "string.Compare(\"$MaxMfTS\", \"$LastMfTS\") >= 0 ? \"$MaxMfTS\" : \"$LastMfTS\""
                        },
                        new PdtVariable {
                            name = "MfTS",
                            expressionBefore = "new DateTime(Math.Min(DateTime.Today.Ticks-1, DateTime.ParseExact(lineVal.Split('$CsvSrcSep')[8], \"MM/dd/yyyy HH:mm:ss\", CultureInfo.InvariantCulture).Ticks)).ToString(\"yyyyMMdd HHmmss\")",
                        },
                        new PdtVariable {
                            name = "MaxMfTS",
                            expressionStorage = "NoStorage",
                            expressionBefore = "string.Compare(\"$MfTS\", \"$MaxMfTS\") >= 0 && (lineVal.Split('$CsvSrcSep')[7] == \"PAID\" || lineVal.Split('$CsvSrcSep')[7] == \"STCS\") && lineVal.Split('$CsvSrcSep')[16] != \"ACCT\" && lineVal.Split('$CsvSrcSep')[9] != \"INTERNAL TRANSACTION\" ? \"$MfTS\" : \"$MaxMfTS\"",
                        },
                    },
                    processingCondition = "(lineVal.Split('$CsvSrcSep')[7] == \"PAID\" || lineVal.Split('$CsvSrcSep')[7] == \"STCS\") && lineVal.Split('$CsvSrcSep')[16] != \"ACCT\" && lineVal.Split('$CsvSrcSep')[9] != \"INTERNAL TRANSACTION\" && string.Compare(\"$MfTS\", \"$LastMfTS\") > 0 && string.Compare(\"$MfTS\", DateTime.Today.AddTicks(-1).ToString(\"yyyyMMdd HHmmss\")) < 0",
                    columns = new [] {
                        new PdtColumn {
                            name = "Fund",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "Fund",
                                expression = "\"MIR_\" + colVal"
                            } }
                        },
                        new PdtColumn {
                            name = "SS Asset ID",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "SS Asset ID" } }
                        },
                        new PdtColumn {
                            name = "Share Quantity",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Share Quantity" } }
                        },
                        new PdtColumn {
                            name = "Actual Net Amount",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Actual Net Amount" } }
                        },
                        new PdtColumn {
                            name = "Principal",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Principal" } }
                        },
                        new PdtColumn {
                            name = "Interest",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Interest" } }
                        },
                        new PdtColumn {
                            name = "Settle Loc",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Settle Loc" } }
                        },
                        new PdtColumn {
                            name = "SS Status",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "SS Status" } }
                        },
                        new PdtColumn {
                            name = "Mainframe Time Stamp",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Mainframe Time Stamp" } }
                        },
                        new PdtColumn {
                            name = "Transaction Description",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Transaction Description" } }
                        },
                        new PdtColumn {
                            name = "Actual Pay/Settle Date",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Actual Pay/Settle Date" } }
                       },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Executing Broker" } }
                        },
                        new PdtColumn {
                            name = "Current Factor",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Current Factor" } }
                        },
                        new PdtColumn {
                            name = "Prior Factor",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Prior Factor" } }
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Currency" } }
                        },
                        new PdtColumn {
                            name = "Trade/Record Date",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Trade/Record Date" } }
                        },
                        new PdtColumn {
                            name = "Related Trade ID",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Related Trade ID" } }
                        },
                        new PdtColumn {
                            name = "Comments",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Comments" } }
                        },
                        new PdtColumn {
                            name = "Client reference#",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Client reference#" } }
                        },
                        new PdtColumn {
                            name = "SS Trade ID",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "SS Trade ID" } }
                        },
                        new PdtColumn {
                            name = "SS Trans Type",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "SS Trans Type" } }
                        },
                        new PdtColumn {
                            name = "Isin",
                            isRequired = true,
                            destPaths = new [] { new PdtColumnDest { path = "Isin" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSBAllCustody_CollateralCash.ToString(),
                    type = TransType.Csv2Xml,
                    label = "SSBAllCustody Collateral Cash",
                    templateFile = "BBH_DIM_Cash.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSkipLines = 1,
                    csvSrcSeparator = ';',
                    uniqueConstraints = new[]
                    {
                        "lineVal.Split('$CsvSrcSep')[0] + lineVal.Split('$CsvSrcSep')[19] + lineVal.Split('$CsvSrcSep')[9] + lineVal.Split('$CsvSrcSep')[10]"
                    },
                    checkConstraints = new[]
                    {
                        "!string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[0]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[19]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[10]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[14]) && !string.IsNullOrWhiteSpace(lineVal.Split('$CsvSrcSep')[3])"
                    },
                    variables = new[] {
                        new PdtVariable {
                            name = "LastMfTS",
                            expressionStorage = "string.Compare(\"$MaxMfTS\", \"$LastMfTS\") >= 0 ? \"$MaxMfTS\" : \"$LastMfTS\""
                        },
                        new PdtVariable {
                            name = "MfTS",
                            expressionBefore = "DateTime.ParseExact(lineVal.Split('$CsvSrcSep')[8], \"MM/dd/yyyy HH:mm:ss\", CultureInfo.InvariantCulture).ToString(\"yyyyMMdd HHmmss\")",
                        },
                        new PdtVariable {
                            name = "MaxMfTS",
                            expressionStorage = "NoStorage",
                            expressionBefore = "string.Compare(\"$MfTS\", \"$MaxMfTS\") >= 0 && (lineVal.Split('$CsvSrcSep')[7] == \"PAID\" || lineVal.Split('$CsvSrcSep')[7] == \"STCS\") && lineVal.Split('$CsvSrcSep')[16] != \"ACCT\" && lineVal.Split('$CsvSrcSep')[9] != \"INTERNAL TRANSACTION\" ? \"$MfTS\" : \"$MaxMfTS\"",
                        },
                    },
                    processingCondition = "(lineVal.Split('$CsvSrcSep')[7] == \"PAID\" || lineVal.Split('$CsvSrcSep')[7] == \"STCS\") && lineVal.Split('$CsvSrcSep')[16] != \"ACCT\" && lineVal.Split('$CsvSrcSep')[9] != \"INTERNAL TRANSACTION\" && string.Compare(\"$MfTS\", \"$LastMfTS\") > 0 && \",CCPM,CCPC,SWCC,SWBC,TBCC,\".IndexOf(\",\" + lineVal.Split('$CsvSrcSep')[16] + \",\") >= 0",
                    columns = new [] {
                        new PdtColumn {
                            name = "Fund",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT R.VALUE AS ROOTPORTFOLIO
FROM BO_TREASURY_ACCOUNT A
    JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID
    JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'RootPortfolio'
WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ACCOUNT_AT_CUSTODIAN = '"" + colVal + ""'"""
                                    },
                                    path = @"
//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')] |
//*[local-name() = 'externalReference']/*[local-name() = 'name' and text() = 'RootPortfolio']/../*[local-name() = 'value']"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT DEPOSITARY FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = '\" + colVal + \"'\"",
                                    },
                                    path = "//*[local-name() = 'depositary']//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'id')]",
                                },
                            }
                        },
                        new PdtColumn {
                            name = "SS Asset ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'tradeId'][contains(@*[local-name() = 'tradeIdScheme'], 'tradeId/externalReference')]" } }
                        },
                        new PdtColumn {
                            name = "Share Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'persistenceType']",
                                    expression = "\"UpdateOrCreate\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent']",
                                    expression = "\"Cash Transfer\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'backofficeStatus']",
                                    expression = "\"No Status\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'extendedPartyTradeInformation']//*[local-name() = 'comment']",
                                    expression = "\"Collateral\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeTime']",
                                    expression = "DateTime.Now.ToString(\"HH:mm:ss\")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'numberOfSecurities']",
                                    expression = "1"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']",
                                    expression = "0"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'spot']/@*[local-name() = 'type']",
                                    expression = "\"InAmount\""
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Actual Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']/*[local-name() = 'amount']/*[local-name() = 'amount']" } }
                        },
                        new PdtColumn {
                            name = "Principal",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Interest",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Settle Loc",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "SS Status",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Mainframe Time Stamp",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Transaction Description",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Actual Pay/Settle Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'unadjustedDate'] | //*[local-name() = 'adjustedPaymentDate'] | //*[local-name() = 'tradeHeader']/*[local-name() = 'paymentDate']",
                                    expression = "colVal.Trim() == \"\" ? \"\" : DateTime.ParseExact(colVal, \"MM/dd/yyyy\", CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                       },
                        new PdtColumn {
                            name = "Executing Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Current Factor",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Prior Factor",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'amount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'principalSettlement']//*[local-name() = 'grossAmount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'paymentAmount']//*[local-name() = 'currency']" },
                                new PdtColumnDest { path = "//*[local-name() = 'detailedFee']//*[local-name() = 'currency']" },
                            }
                        },
                        new PdtColumn {
                            name = "Trade/Record Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeDate']",
                                    expression = "colVal.Trim() != \"\" ? DateTime.ParseExact(colVal, \"MM/dd/yyyy\", CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : (lineVal.Split('$CsvSrcSep')[10].Trim() != \"\" ? DateTime.ParseExact(lineVal.Split('$CsvSrcSep')[10], \"MM/dd/yyyy\", CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : \"\")"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Related Trade ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Comments",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Client reference#",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "SS Trade ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "SS Trans Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Isin",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_CashCollateralTrade.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Cash Collateral Trade",
                    templateFile = "Rbc_CashCollateralTrade.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "SignOfTransaction",
                            expressionBefore = "lineVal.Substring(51, 1)",
                        },
                        new PdtVariable{
                            name = "TransactionType",
                            expressionBefore = "lineVal.Substring(47, 4).Trim()",
                        },
                    },
                    processingCondition = "\"$TransactionType\" == \"VMI\" || \"$TransactionType\" == \"PMI\" || \"$TransactionType\" == \"MADV\"",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Fund Custody Code" },
                                new PdtColumnDest { path = "PTG Name" },
                                new PdtColumnDest { path = "Fund Manager Reference" },
                                new PdtColumnDest { 
                                    path = "External Fund Identifier",
                                    expression = "colVal == \"M6\" ? \"GFJN\" : colVal == \"M4\" ? \"GFJG\" : colVal"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                            destPaths = new []{ new PdtColumnDest { path = "Currency" } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction ID" } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Settlement Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                            destPaths = new [] { 
                                new PdtColumnDest { 
                                    path = "Transaction Description",
                                    expression = "colVal == \"VMI\" ? \"CASH DIRECT ENTRY\" : colVal == \"PMI\" ? \"CASH WITHDRAWAL\" : \"CASH VARIATION MARGIN\""
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Collateral Counterparty Name",
                                    expression = "\"State Street Milan\"",
                                },
                                new PdtColumnDest {
                                    path = "Collateral Counterparty BIC Code",
                                    expression = "\"SBOSGB2\"",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Current Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Quantity",
                                    expression = "0"
                                },
                                new PdtColumnDest {
                                    path = "Net Amount",
                                    expression = "(\"$TransactionType\" == \"VMI\" ? \"+\" : \"$TransactionType\" == \"PMI\" ? \"-\" : \"$SignOfTransaction\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 10,
                        },
                        new PdtColumn
                        {
                            name = "Position Type",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "External reference",
                            len = 16,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_CashMonthlyInterest.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Cash Monthly Interest",
                    templateFile = "Rbc_CashMonthlyInterest.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    processingCondition = "lineVal.Substring(47, 4).Trim() == \"ITR\"",
                    variables = new[] {
                        new PdtVariable{
                            name = "SignOfTransaction",
                            expressionBefore = "lineVal.Substring(51, 1) == \"+\" ? \"-\" : \"+\"",
                        }
                    },
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Fund Custody Code" },
                                new PdtColumnDest { path = "External Fund Identifier" },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_10FLSNV_202*",
                                        Expression = "lineVal.Substring(0, 6).Trim() == colVal ? lineVal.Substring(6, 40).Trim() : \"\"", //PF_ACR
                                    },
                                    path = "Fund Name",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                            destPaths = new []{ new PdtColumnDest { path = "Currency" } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction ID" } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(39, 8).Trim()) == 1 ? lineVal.Substring(39, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Value Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                            destPaths = new [] { 
                                new PdtColumnDest { 
                                    path = "Transaction Type",
                                    expression = "\"S1\""
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 8,
                        },
                        new PdtColumn
                        {
                            name = "Current Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Amount",
                                    expression = "\"$SignOfTransaction\" + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 10,
                        },
                        new PdtColumn
                        {
                            name = "Position Type",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "External reference",
                            len = 16,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_CorporateAction.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Corporate Action",
                    templateFile = "Rbc_CorporateAction.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "SecurityDescription",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "1",
                                Lookup = new PdtColumnLookup {
                                    File = "MIFL_ME_15DOMIN_*",
                                    Expression = "lineVal.Substring(0, 20).Trim() == \"INST_GRP_COD\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                }
                            },
                        },
                        new PdtVariable{
                            name = "MaturityDate",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "5",
                            },
                        },
                    },
                    processingCondition = "\"$SecurityDescription\" == \"BONDS\" && lineVal.Substring(113, 4) == \"RTIT\" && string.Compare(\"$MaturityDate\", lineVal.Substring(76, 8).Trim()) > 0",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Fund Custody Code" },
                                new PdtColumnDest { path = "External Fund Identifier" },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 50,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Security Description",
                                    expression = "\"$SecurityDescription\" == \"EQUITIES\" ? \"EQUITIES\" : \"FUNDS\"",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "4",
                                    },
                                    path = "Security Name",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "0",
                                        Lookup = new PdtColumnLookup {
                                            File = "MIFL_ME_15DOMIN_*",
                                            Expression = "lineVal.Substring(0, 20).Trim() == \"INST_TYP\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                        }
                                    },
                                    path = "Type",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "OLD ISIN" } }
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Ex Date",
                                    expression = "System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(76, 8).Trim()) == 1 ? lineVal.Substring(76, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Payment Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction ID" } }
                        },
                        new PdtColumn
                        {
                            name = "External Transaction Number",
                            len = 16,
                        },
                        new PdtColumn
                        {
                            name = "Flag P/T",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                            destPaths = new [] { 
                                new PdtColumnDest {
                                    path = "LST Name",
                                    expression="colVal.Trim() == \"RTIT\" ? (string.Compare(\"$MaturityDate\", lineVal.Substring(76, 8).Trim()) > 0 ? \"Early Redemption\" : \"Final Redemption\") : \"\"",
                                },
                                new PdtColumnDest {
                                    path = "LST Type",
                                    expression="colVal.Trim() == \"RTIT\" ? \"77\" : \"\"",
                                },
                                new PdtColumnDest {
                                    path = "OLD Type",
                                    expression="colVal.Trim() == \"RTIT\" ? \"Bonds (Corp, Govie)\" : \"\"",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 20,
                        },
                        new PdtColumn
                        {
                            name = "Quantity",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Out Quantity",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Security Price",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Price",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Net Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Close Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Accrued Interest Amount",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Gross Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Net Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Gross Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Gross Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Settlement ISO Currency Code",
                            len = 3,
                            destPaths = new[] { 
                                new PdtColumnDest { path = "OLD Ccy" },
                                new PdtColumnDest { path = "NEW Ccy" },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Settlement Currency Exchange Rate",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Fees Amount",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Commission Broker",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Commission Others",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_CorporateActionFR.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Corporate Action Final Redemption",
                    templateFile = "Rbc_CorporateAction.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "SecurityDescription",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "1",
                                Lookup = new PdtColumnLookup {
                                    File = "MIFL_ME_15DOMIN_*",
                                    Expression = "lineVal.Substring(0, 20).Trim() == \"INST_GRP_COD\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                }
                            },
                        },
                        new PdtVariable{
                            name = "MaturityDate",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "5",
                            },
                        },
                    },
                    processingCondition = "\"$SecurityDescription\" == \"BONDS\" && lineVal.Substring(113, 4) == \"RTIT\" && string.Compare(\"$MaturityDate\", lineVal.Substring(76, 8).Trim()) <= 0",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] {
                                new PdtColumnDest { path = "Fund Custody Code" },
                                new PdtColumnDest { path = "External Fund Identifier" },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 50,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Security Description",
                                    expression = "\"$SecurityDescription\" == \"EQUITIES\" ? \"EQUITIES\" : \"FUNDS\"",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "4",
                                    },
                                    path = "Security Name",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "0",
                                        Lookup = new PdtColumnLookup {
                                            File = "MIFL_ME_15DOMIN_*",
                                            Expression = "lineVal.Substring(0, 20).Trim() == \"INST_TYP\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                        }
                                    },
                                    path = "Type",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "OLD ISIN" } }
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Ex Date",
                                    expression = "System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(76, 8).Trim()) == 1 ? lineVal.Substring(76, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Payment Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction ID" } }
                        },
                        new PdtColumn
                        {
                            name = "External Transaction Number",
                            len = 16,
                        },
                        new PdtColumn
                        {
                            name = "Flag P/T",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "LST Name",
                                    expression="colVal.Trim() == \"RTIT\" ? (string.Compare(\"$MaturityDate\", lineVal.Substring(76, 8).Trim()) > 0 ? \"Early Redemption\" : \"Final Redemption\") : \"\"",
                                },
                                new PdtColumnDest {
                                    path = "LST Type",
                                    expression="colVal.Trim() == \"RTIT\" ? \"77\" : \"\"",
                                },
                                new PdtColumnDest {
                                    path = "OLD Type",
                                    expression="colVal.Trim() == \"RTIT\" ? \"Bonds (Corp, Govie)\" : \"\"",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 8,
                        },
                        new PdtColumn
                        {
                            name = "Quantity",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Out Quantity",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Security Price",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Price",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Net Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Close Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Accrued Interest Amount",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Gross Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Net Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Gross Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Gross Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Settlement ISO Currency Code",
                            len = 3,
                            destPaths = new[] {
                                new PdtColumnDest { path = "OLD Ccy" },
                                new PdtColumnDest { path = "NEW Ccy" },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Settlement Currency Exchange Rate",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Fees Amount",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Commission Broker",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Commission Others",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_SR.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Subscription Redemption",
                    templateFile = "Rbc_SR.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    processingCondition = "lineVal.Substring(84, 4).Trim() == \"SOT\" || lineVal.Substring(84, 4).Trim() == \"RIS\"",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] {
                                new PdtColumnDest { path = "Fund Custody Code / SARA code" },
                                new PdtColumnDest { path = "MF code" },
                                new PdtColumnDest { path = "Fund Manager Identifier" },
                                new PdtColumnDest {
                                    path = "External Fund Reference",
                                    expression = "colVal == \"M6\" ? \"GFJN\" : colVal == \"M4\" ? \"GFJG\" : colVal"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Account Code",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name = "Account Name",
                            len = 50,
                        },
                        new PdtColumn
                        {
                            name = "NAV Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                                new PdtColumnDest {
                                    path = "Creation Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                                new PdtColumnDest {
                                    path = "ReportDate",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name= "Settlement Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Settlement Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Causal",
                            len = 4,
                        },
                        new PdtColumn
                        {
                            name = "Number Movement",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name = "Line Movement",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name = "Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Projected Amount Local ccy",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                },
                                new PdtColumnDest {
                                    path = "Projected Amount Fund ccy",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                },
                                new PdtColumnDest {
                                    path = "Account Currency(ISO)",
                                    expression = "\"EUR\""
                                },
                                new PdtColumnDest {
                                    path = "CashFlow Type Id",
                                    expression = "\"TA\""
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Sign",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Code  Reference",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name = "Class Code",
                            len = 10,
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 10,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_CashTrade.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Cash Trade",
                    templateFile = "Rbc_CashTrade.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "SignOfTransaction",
                            expressionBefore = "lineVal.Substring(129, 1)",
                        },
                        new PdtVariable{
                            name = "CurrentAmount",
                            expressionBefore = "lineVal.Substring(112, 17)",
                            expressionAfter = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                        },
                        new PdtVariable
                        {
                            name = "TDAFundM4",
                            expressionBefore = "lineVal.Substring(88, 12) + \"M4\"",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_11MOVCN",
                                ColumnIndex = "-1",
                            },
                        },
                        new PdtVariable
                        {
                            name = "TDAFundM6",
                            expressionBefore = "lineVal.Substring(88, 12) + \"M6\"",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_11MOVCN",
                                ColumnIndex = "-1",
                            },
                        }
                    },
                    processingCondition = "\",M4,M6,\".IndexOf(\",\" + lineVal.Substring(0, 4).Trim() + \",\") < 0 && lineVal.Substring(84, 4).Trim() == \"TDA\" && (\"$TDAFundM4\" != \"\" || \"$TDAFundM6\" != \"\")",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { new PdtColumnDest { path = "External Fund Identifier" } }
                        },
                        new PdtColumn
                        {
                            name = "Account Code",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name = "Account Name",
                            len = 50,
                        },
                        new PdtColumn
                        {
                            name = "NAV Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(76, 8).Trim()) == 1 ? lineVal.Substring(76, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name= "Settlement Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Value Date",
                                    expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Causal",
                            len = 4,
                        },
                        new PdtColumn
                        {
                            name = "Number Movement",
                            len = 12,
                            destPaths = new [] { new PdtColumnDest { path = "Transaction ID" } }
                        },
                        new PdtColumn
                        {
                            name = "Line Movement",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name = "Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Amount",
                                    expression = "('$SignOfTransaction' == 'U' ? \"-\" : \"+\") + \"$CurrentAmount\".Substring(1)"
                                },
                                new PdtColumnDest {
                                    path = "Currency",
                                    expression = "\"EUR\""
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Sign",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Code  Reference",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name = "Class Code",
                            len = 10,
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 10,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_EquityTrade.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Equity Trade",
                    templateFile = "Rbc_EquityTrade.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "SecurityDescription",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "1",
                                Lookup = new PdtColumnLookup {
                                    File = "MIFL_ME_15DOMIN_*",
                                    Expression = "lineVal.Substring(0, 20).Trim() == \"INST_GRP_COD\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                }
                            },
                        },
                    },
                    processingCondition = "\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") < 0 && ((\"$SecurityDescription\" == \"EQUITIES\" && (lineVal.Substring(113, 4) == \"ATIT\" || lineVal.Substring(113, 4) == \"VTIT\")) || lineVal.Substring(113, 4) == \"SOFO\" || lineVal.Substring(113, 4) == \"RIFO\")",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Fund Custody Code" },
                                new PdtColumnDest { path = "External Fund Identifier" }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 50,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Security Description",
                                    expression = "\"$SecurityDescription\" == \"EQUITIES\" ? \"EQUITIES\" : \"FUNDS\" ",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "3",
                                        Expression = "colVal != \"\" ? colVal + \" Equity\" : \"\"",
                                    },
                                    path = "Bloomberg Code",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "4",
                                    },
                                    path = "Security Name",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "0",
                                        Lookup = new PdtColumnLookup {
                                            File = "MIFL_ME_15DOMIN_*",
                                            Expression = "lineVal.Substring(0, 20).Trim() == \"INST_TYP\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                        }
                                    },
                                    path = "Type",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(76, 8).Trim()) == 1 ? lineVal.Substring(76, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Settlement Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction ID" } }
                        },
                        new PdtColumn
                        {
                            name = "External Transaction Number",
                            len = 16,
                        },
                        new PdtColumn
                        {
                            name = "Flag P/T",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                            destPaths = new []{
                                new PdtColumnDest {
                                    path = "Transaction Description",
                                    expression = "colVal == \"A\" ? \"Buy\" : \"Sell\""
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 20,
                            destPaths = new []{ 
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "CtpyBicCodes.txt",
                                        Expression = "lineVal.Split(';')[2].Trim() == colVal ? lineVal.Split(';')[1].Trim() : \"\"",
                                    },
                                    path = "Broker BIC Code" 
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_03ANACTP_*",
                                        Expression = "lineVal.Substring(0, 8).Trim() == colVal ? lineVal.Substring(8, 40).Trim() : \"\"",
                                    },
                                    path = "Broker Name",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Quantity",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Quantity",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Security Price",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Price",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Net Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Close Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Accrued Interest Amount",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Gross Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Net Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Gross Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Gross Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Settlement ISO Currency Code",
                            len = 3,
                            destPaths = new[] { new PdtColumnDest { path = "Currency" } }
                        },
                        new PdtColumn
                        {
                            name = "Settlement Currency Exchange Rate",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Fees Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Expenses",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Commission Broker",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Brokerage",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Commission Others",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Tax",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_FixedIncomeTrade.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC FixedIncome Trade",
                    templateFile = "Rbc_FixedIncome.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "SecurityDescription",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "1",
                                Lookup = new PdtColumnLookup {
                                    File = "MIFL_ME_15DOMIN_*",
                                    Expression = "lineVal.Substring(0, 20).Trim() == \"INST_GRP_COD\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                }
                            },
                        },
                    },
                    processingCondition = "\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") < 0 && (\"$SecurityDescription\" == \"BONDS\" || \"$SecurityDescription\" == \"CONVERTIBLES\") && (lineVal.Substring(113, 4) == \"ATIT\" || lineVal.Substring(113, 4) == \"VTIT\")",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Fund Custody Code" },
                                new PdtColumnDest { path = "External Fund Identifier" }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 50,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Security Description",
                                    expression = "\"$SecurityDescription\"",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "4",
                                    },
                                    path = "Security Name",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "0",
                                        Lookup = new PdtColumnLookup {
                                            File = "MIFL_ME_15DOMIN_*",
                                            Expression = "lineVal.Substring(0, 20).Trim() == \"INST_TYP\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                        }
                                    },
                                    path = "Type",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "5",
                                    },
                                    path = "Maturity Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "6",
                                    },
                                    path = "Day Count",
                                    expression = "colVal == \"0\" ? \"UNKNOWN\" : (colVal == \"1\" ? \"ACTUAL/ACTUAL\" : (colVal == \"2\" ? \"EXACT 1ECH/2\" : (colVal == \"3\" ? \"ACT/365\" : (colVal == \"4\" ? \"Actual/360\" : (colVal == \"5\" ? \"30/360(conditional)\" : (colVal == \"6\" ? \"30E+/360 MONTH OF 30D\" : (colVal == \"7\" ? \"30E+/360(DAY+1)\" : (colVal == \"8\" ? \"365/ACT\" : (colVal == \"9\" ? \"365/365\" : \"365/360\")))))))))",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "7",
                                    },
                                    path = "Coupon Frequency",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "ISIN Code" } }
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(76, 8).Trim()) == 1 ? lineVal.Substring(76, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Settlement Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction ID" } }
                        },
                        new PdtColumn
                        {
                            name = "External Transaction Number",
                            len = 16,
                        },
                        new PdtColumn
                        {
                            name = "Flag P/T",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                            destPaths = new [] { 
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_15DOMIN_*",
                                        Expression = "lineVal.Substring(0, 20).Trim() == \"XACT_TYP\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                    },
                                    path = "Transaction Type" 
                                } 
                            }
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                            destPaths = new []{
                                new PdtColumnDest {
                                    path = "Transaction Description",
                                    expression = "colVal == \"A\" ? \"Buy\" : \"Sell\""
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 20,
                            destPaths = new []{ 
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "CtpyBicCodes.txt",
                                        Expression = "lineVal.Split(';')[2].Trim() == colVal ? lineVal.Split(';')[1].Trim() : \"\"",
                                    },
                                    path = "Broker BIC Code" 
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_03ANACTP_*",
                                        Expression = "lineVal.Substring(0, 8).Trim() == colVal ? lineVal.Substring(8, 40).Trim() : \"\"",
                                    },
                                    path = "Broker Name",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Quantity",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Quantity",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Security Price",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Price",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Net Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Close Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Accrued Interest Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Bond Interest",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Gross Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Net Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Gross Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Gross Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Settlement ISO Currency Code",
                            len = 3,
                            destPaths = new[] { new PdtColumnDest { path = "Currency" } }
                        },
                        new PdtColumn
                        {
                            name = "Settlement Currency Exchange Rate",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Fees Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Expenses",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Commission Broker",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Brokerage",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Commission Others",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Tax",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_FxTrade.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Fx Trade",
                    templateFile = "Rbc_FxTrade.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    ExtraEvalCode = @"
    private static DateTime LastDayOfWeek(DateTime dt)
    {
        if (dt.DayOfWeek == DayOfWeek.Sunday) return dt.AddDays(-2);
        if (dt.DayOfWeek == DayOfWeek.Saturday) return dt.AddDays(-1);
        return dt;
    }",
                    variables = new[] {
                        new PdtVariable{
                            name = "TransactionType",
                            expressionBefore = "\",ADI,VDI,\".IndexOf(\",\" + lineVal.Substring(47, 4).Trim() + \",\") >= 0 ? \"SPOT\" : \",AVFW,AAFW,\".IndexOf(\",\" + lineVal.Substring(47, 4).Trim() + \",\") >= 0 ? \"FORWARD\" : \"UNKNOWN\""
                        },
                        new PdtVariable {
                            name = "TradeDateCorrected",
                            expressionBefore = "(string.Compare(lineVal.Substring(31, 8).Trim(), lineVal.Substring(39, 8).Trim()) == 1 ? System.DateTime.ParseExact(lineVal.Substring(39, 8).Trim(), \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).AddDays(\"$TransactionType\" == \"SPOT\" ? -2 : -3) : System.DateTime.ParseExact(lineVal.Substring(31, 8).Trim(), \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture)).ToString(\"yyyyMMdd\")"
                        },
                    },
                    processingCondition = "lineVal.Substring(51, 1) == \"+\" && ((\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") < 0 && ((\"$TransactionType\"==\"SPOT\" && lineVal.Substring(52, 8).Trim()!=\"SB\") || \"$TransactionType\"==\"FORWARD\")) || (\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") >= 0 && \"$TransactionType\"==\"SPOT\" && lineVal.Substring(52, 8).Trim()==\"SSBMI\"))",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Fund Custody Code" },
                                new PdtColumnDest { path = "External Fund Identifier" }
                            }
                        },
                        new PdtColumn { name = "ISO Currency Code", len = 3},
                        new PdtColumn { name = "Internal Transaction Number", len = 12,
                            destPaths = new []{ 
                                new PdtColumnDest {path = "Transaction ID" },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Expression = "lineVal.Substring(9, 12).Trim() == colVal && lineVal.Substring(51, 1) == \"+\" ? lineVal.Substring(6, 3) : \"\"",
                                    },
                                    path = "Buy Ccy"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Expression = "lineVal.Substring(9, 12).Trim() == colVal && lineVal.Substring(51, 1) == \"-\" ? lineVal.Substring(6, 3) : \"\"",
                                    },
                                    path = "Sell Ccy"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Expression = "lineVal.Substring(9, 12).Trim() == colVal && lineVal.Substring(51, 1) == \"+\" ? lineVal.Substring(60, 17) : \"\"",
                                    },
                                    path = "Purchased Amount",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Expression = "lineVal.Substring(9, 12).Trim() == colVal && lineVal.Substring(51, 1) == \"-\" ? lineVal.Substring(60, 17) : \"\"",
                                    },
                                    path = "Sold Amount",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                }
                            }
                        },
                        new PdtColumn { name = "Internal Contract Number", len = 10 },
                        new PdtColumn { name= "Transaction Date", len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "LastDayOfWeek(System.DateTime.ParseExact(\"$TradeDateCorrected\", \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture)).ToString(\"dd/MM/yyyy\")"
                                    //expression = "colVal.Trim() == \"\" ? \"\" : System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(39, 8).Trim()) == 1 ? lineVal.Substring(39, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn { name = "Settle Date", len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Value Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn { name = "Type of Transaction", len = 4,
                            destPaths = new []{
                                new PdtColumnDest { 
                                    path = "Type",
                                    expression = "\"$TransactionType\""
                                } 
                            }
                        },
                        new PdtColumn { name = "Sign of Transaction", len = 1 },
                        new PdtColumn { name = "Broker Code", len = 8,
                            destPaths = new [] {
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "CtpyBicCodes.txt",
                                        Expression = "lineVal.Split(';')[2].Trim() == colVal ? lineVal.Split(';')[1].Trim() : \"\"",
                                    },
                                    path = "Broker BIC Code" 
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_03ANACTP_*",
                                        Expression = "lineVal.Substring(0, 8).Trim() == colVal ? lineVal.Substring(8, 40).Trim() : \"\"",
                                    },
                                    path = "Broker Name",
                                },
                            }
                        },
                        new PdtColumn { name = "Current Amount", len = 17 },
                        new PdtColumn { name = "Internal Security Number", len = 10 },
                        new PdtColumn { name = "Position Type", len = 1 },
                        new PdtColumn { name = "External reference", len = 16 },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_OptionTrade.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Option Trade",
                    templateFile = "Rbc_Options.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "InstrumentType_OPTIONS",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "1",
                                Lookup = new PdtColumnLookup {
                                    File = "MIFL_ME_15DOMIN_*",
                                    Expression = "lineVal.Substring(0, 20).Trim() == \"INST_GRP_COD\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                }
                            },
                        },
                        new PdtVariable{
                            name = "BloombergCodeType",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "0",
                                Lookup = new PdtColumnLookup {
                                    File = "MIFL_ME_15DOMIN_*",
                                    Expression = "lineVal.Substring(0, 20).Trim() == \"INST_TYP\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                    Lookup = new PdtColumnLookup {
                                        File = "BloombergCodes.txt",
                                        Expression = "lineVal.Split(';')[2].Trim() == colVal ? lineVal.Split(';')[3].Trim() : \"\"",
                                    }
                                }
                            }
                        }
                    },
                    processingCondition = "\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") < 0 && \"$InstrumentType_OPTIONS\" == \"OPTIONS\" && (lineVal.Substring(113, 4).Trim() == \"AOP\" || lineVal.Substring(113, 4).Trim() == \"VOP\" || lineVal.Substring(113, 4).Trim() == \"AOC\" || lineVal.Substring(113, 4).Trim() == \"VOC\")",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Fund Code" },
                                new PdtColumnDest { path = "External Fund Identifier" },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_10FLSNV_*",
                                        Expression = "lineVal.Substring(0, 6).Trim() == colVal ? lineVal.Substring(6, 40).Trim() : \"\"", //PF_ACR
                                    },
                                    path = "Fund Name",
                                },
                                new PdtColumnDest { path = "Manager Code" },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 50,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "9",
                                        Expression = "colVal == \"C\" ? \"CALL\" : (colVal == \"P\" ? \"PUT\" : \"\") : \"\"", //INST_OPT_TYP
                                    },
                                    path = "Option Type",
                                },
                               new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "4",
                                    },
                                    path = "Option description",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "10",
                                    },
                                    path = "Option Strike",
                                },
                               new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "8",
                                    },
                                    path = "Underlying Asset",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "9",
                                        Expression = "colVal == \"C\" ? \"Options\" : (colVal == \"P\" ? \"Options\" : \"\")",
                                    },
                                    path = "Type",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "3",
                                    },
                                    path = "Bloomberg Code",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(76, 8).Trim()) == 1 ? lineVal.Substring(76, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "NAV Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                                new PdtColumnDest {
                                    path = "Transaction Value Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction Status Code" } }
                        },
                        new PdtColumn
                        {
                            name = "External Transaction Number",
                            len = 16,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction Code" } }
                        },
                        new PdtColumn
                        {
                            name = "Flag P/T",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                            destPaths = new []{
                                new PdtColumnDest {
                                    path = "Buy/Sell",
                                    expression = "colVal == \"A\" ? \"Buy\" : \"Sell\""
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 20,
                            destPaths = new []{ 
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_03ANACTP_*",
                                        Expression = "lineVal.Substring(0, 8).Trim() == colVal ? colVal + \"-\" + lineVal.Substring(8, 40).Trim() : \"\"", //BROKER_COD
                                    },
                                    path = "Counterparty/Broker Code" 
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_03ANACTP_*",
                                        Expression = "lineVal.Substring(0, 8).Trim() == colVal ? lineVal.Substring(8, 40).Trim() : \"\"", //BROKER_COD
                                    },
                                    path = "Counterparty/Broker Description",
                                },
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "CtpyBicCodes.txt",
                                        Expression = "lineVal.Split(';')[2].Trim() == colVal ? lineVal.Split(';')[1].Trim() : \"\"",
                                    },
                                    path = "BIC Code" 
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_03ANACTP_*",
                                        Expression = "lineVal.Substring(0, 8).Trim() == colVal ? lineVal.Substring(8, 40).Trim() : \"\"", //BROKER_COD
                                    },
                                    path = "Broker Name",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Quantity",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Quantity",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Security Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Net Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Close Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Accrued Interest Amount",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Gross Amount in Security Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Premium",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Gross Total Amount in Settlement Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Settlement ISO Currency Code",
                            len = 3,
                            destPaths = new[] { new PdtColumnDest { path = "Option Ccy" } }
                        },
                        new PdtColumn
                        {
                            name = "Settlement Currency Exchange Rate",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Fees Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Fees Amount in Transaction Currency",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Commission Broker",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Commission Amount",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Commission Others",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                            destPaths = new[] { new PdtColumnDest { path = "Contract Number" } }
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_FutureTrade.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Future Trade",
                    templateFile = "Rbc_Futures.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    variables = new[] {
                        new PdtVariable{
                            name = "BloombergCodeType",
                            expressionBefore = "lineVal.Substring(6, 50).Trim()",
                            Lookup = new PdtColumnLookup {
                                Table = "MIFL_ME_01ANATIT",
                                ColumnIndex = "0",
                                Lookup = new PdtColumnLookup {
                                    File = "MIFL_ME_15DOMIN_*",
                                    Expression = "lineVal.Substring(0, 20).Trim() == \"INST_TYP\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                    Lookup = new PdtColumnLookup {
                                        File = "BloombergCodes.txt",
                                        Expression = "lineVal.Split(';')[2].Trim() == colVal ? lineVal.Split(';')[3].Trim() : \"\"",
                                    }
                                }
                            }
                        }
                    },
                    processingCondition = "\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") < 0 && (lineVal.Substring(113, 4).Trim() == \"AFUT\" || lineVal.Substring(113, 4).Trim() == \"VFUT\")",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Fund Code" },
                                new PdtColumnDest { path = "External Fund Identifier" },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_10FLSNV_*",
                                        Expression = "lineVal.Substring(0, 6).Trim() == colVal ? lineVal.Substring(6, 40).Trim() : \"\"", //PF_ACR
                                    },
                                    path = "Fund Name",
                                },
                                new PdtColumnDest { path = "Manager Code" },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 50,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "1",
                                        Lookup = new PdtColumnLookup {
                                            File = "MIFL_ME_15DOMIN_*",
                                            Expression = "lineVal.Substring(0, 20).Trim() == \"INST_GRP_COD\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                                        }
                                    },
                                    path = "GTI Description",
                                    processingCondition = "colVal == \"FUTURES\""
                                },
                               new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "2",
                                    },
                                    path = "Underlying ISIN",
                                },
                                new PdtColumnDest {
                                    path = "Type",
                                    expression = "\"$BloombergCodeType\"",
                                },
                               new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "4",
                                    },
                                    path = "Future Description",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "5",
                                        Expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")", //MATURITY_DAT_END
                                    },
                                    path = "Future Maturity Date",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "3",
                                        Expression = "colVal != \"\" ? colVal + \" $BloombergCodeType\" : \"\"", //TICKER
                                    },
                                    path = "Bloomberg Code",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(76, 8).Trim()) == 1 ? lineVal.Substring(76, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Accounting Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                                new PdtColumnDest {
                                    path = "Transaction Value Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction Status Code" } }
                        },
                        new PdtColumn
                        {
                            name = "External Transaction Number",
                            len = 16,
                            destPaths = new []{ new PdtColumnDest { path = "Transaction Code" } }
                        },
                        new PdtColumn
                        {
                            name = "Flag P/T",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                            destPaths = new []{
                                new PdtColumnDest {
                                    path = "Buy/Sell",
                                    expression = "colVal == \"A\" ? \"Buy\" : \"Sell\""
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 20,
                            destPaths = new []{ 
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_03ANACTP_*",
                                        Expression = "lineVal.Substring(0, 8).Trim() == colVal ? colVal + \"-\" + lineVal.Substring(8, 40).Trim() : \"\"", //BROKER_COD
                                    },
                                    path = "Counterparty/Broker Code" 
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_03ANACTP_*",
                                        Expression = "lineVal.Substring(0, 8).Trim() == colVal ? lineVal.Substring(8, 40).Trim() : \"\"", //BROKER_COD
                                    },
                                    path = "Counterparty/Broker Description",
                                },
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "CtpyBicCodes.txt",
                                        Expression = "lineVal.Split(';')[2].Trim() == colVal ? lineVal.Split(';')[1].Trim() : \"\"",
                                    },
                                    path = "BIC Code" 
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Quantity",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Qty",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Security Price",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Price",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Net Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Close Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Accrued Interest Amount",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Gross Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Settlement Currency",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Amount",
                                    expression = "(lineVal.Substring(117, 1) == \"A\" ? \"+\" : \"-\") + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Gross Total Amount in Settlement Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Settlement ISO Currency Code",
                            len = 3,
                            destPaths = new[] { new PdtColumnDest { path = "Future Currency" } }
                        },
                        new PdtColumn
                        {
                            name = "Settlement Currency Exchange Rate",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Fees Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Fees Amount in Transaction Currency",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Commission Broker",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Commission Amount",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Commission Others",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                            destPaths = new[] { new PdtColumnDest { path = "Contract Number" } }
                        },
                    }
                },
                new PdtTransformation //Begin SEA
                {
                    name = TransName.SWIFT_ACK_NACK.ToString(),
                    type = TransType.Csv2Xml,
                    label = "SWIFT ACK NACK",
                    templateFile = "SWIFT_ACK_NACK.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'import']",
                    csvSkipLines = 1,
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ';',
                    variables = new[] {
                        new PdtVariable {
                            name = "MessageId",
                            expressionBefore = "(lineVal.Split(';')[0].Contains(\"FETALULLISV\") || lineVal.Split(';')[0].Contains(\"BILLIE2D\") || lineVal.Split(';')[0].Contains(\"SBOSITMLIMO\")) ?  lineVal.Split(';')[1].Substring(6) : lineVal.Split(';')[1]",
                        },
                        new PdtVariable {
                            name = "TradeId",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""select trade_id from bo_messages where ident = "" + $MessageId +"""""
                            },
                        },
                    },
                    columns = new [] {
                        new PdtColumn {
                            name = "Source",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Trade ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new[]
                            {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'tradeId']",
                                    expression = "$TradeId"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""Select ident from folio where ident = (select opcvm from join_position_histomvts where refcon = "" + $TradeId + "")""",
                                    },
                                    path = @"//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')]"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Status",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'updateWorkflowEventName']",
                                expression = "colVal == \"ACK\" ? \"Ack Received\" : \"Nack Received\""
                                } }
                        },
                        new PdtColumn {
                            name = "Remarks",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                    }
                },
                 new PdtTransformation
                {
                    name = TransName.SWIFT_ACK_NACK_Mir.ToString(),
                    type = TransType.Csv2Xml,
                    label = "SWIFT ACK NACK Mir",
                    templateFile = "SWIFT_ACK_NACK_Mir.xml",
                    category = "Medio",
                    csvSkipLines = 1,
                    repeatingRootPath = "//*[local-name() = 'import']",
                    repeatingChildrenPath = "//*[local-name() = 'trade']",
                    csvSrcSeparator = ';',
                    variables = new[] {
                        new PdtVariable {
                            name = "MessageId",
                            expressionBefore = "(lineVal.Split(';')[0].Contains(\"FETALULLISV\") || lineVal.Split(';')[0].Contains(\"BILLIE2D\") || lineVal.Split(';')[0].Contains(\"SBOSITMLIMO\")) ?  lineVal.Split(';')[1].Substring(6) : lineVal.Split(';')[1]",
                        },
                        new PdtVariable {
                            name = "TradeId",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""select trade_id from bo_messages where ident = "" + $MessageId +"""""
                            },
                        },
                    },
                    columns = new [] {
                        new PdtColumn {
                            name = "Source",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Trade ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new[]
                            {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""select infos from histomvts where refcon = "" + $TradeId +""""",
                                    },
                                    path = "//*[local-name() = 'tradeHeader']//*[local-name() = 'partyTradeIdentifier']//*[local-name() = 'tradeId']"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT opcvm  
FROM join_position_histomvts 
WHERE refcon = TO_NUMBER((SELECT infos FROM histomvts WHERE refcon = ""+ $TradeId +""), '99999999')""",
                                    },
                                    path = @"//*[local-name() = 'portfolioName'][contains(@*[local-name() = 'portfolioNameScheme'], 'portfolioName/id')]"
                                },
                            }
                        },
                        new PdtColumn {
                            name = "Status",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'trade']/@*[local-name() = 'updateWorkflowEventName']",
                                expression = "colVal == \"ACK\" ? \"Ack Received\" : \"Nack Received\""
                                } }
                        },
                        new PdtColumn {
                            name = "Remarks",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'userComment']//*[local-name() = 'tradeIdentifier']//*[local-name() = 'tradeId']",
                                expression = "lineVal.Split(';')[2] == \"NACK\" ? \"$TradeId\" : \"0000\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'boComment']",
                                expression = "lineVal.Split(';')[2] == \"NACK\" ? colVal : \"\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'comment']",
                                expression = "lineVal.Split(';')[2] == \"NACK\" ? colVal : \"Trade Acknowledged\""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'dateTime']",
                                expression = "System.DateTime.Now.ToString(\"o\")"
                                }}
                        },
                    }
                },
                new PdtTransformation 
                {
                    name = TransName.Notify_CASH_MGR_NACK_Reponse.ToString(),
                    type = TransType.Csv2Csv,
                    label = "Notify CASH MGR NACK Reponse",
                    templateFile = "NACK_Reponse.csv",
                    category = "System",
                    csvSkipLines = 1,
                    csvSrcSeparator = DEFAULT_CSV_SEPARATOR,
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    ClearEmptyOutput = true,
                    variables = new[] {
                        new PdtVariable {
                            name = "MessageId",
                            expressionBefore = "(lineVal.Split(';')[0].Contains(\"FETALULLISV\") || lineVal.Split(';')[0].Contains(\"BILLIE2D\") || lineVal.Split(';')[0].Contains(\"SBOSITMLIMO\")) ?  lineVal.Split(';')[1].Substring(6) : lineVal.Split(';')[1]",
                        },
                        new PdtVariable {
                            name = "TradeId",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""select trade_id from bo_messages where ident = "" + $MessageId +"""""
                            },
                        },
                        new PdtVariable {
                            name = "MirrorId",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""select infos from histomvts where refcon = "" + $TradeId +"""""
                            },
                        },
                        new PdtVariable {
                            name = "OrigninalDetails",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT DISTINCT
    hm.refcon || ';' || ts.name || ';' || str.name || ';' || bks.name || ';' || TO_CHAR(ah.DateModif, 'YYYYMMDD-HH24:MI:SS') || ';' || hm.DATENEG || ';' || hm.DATEVAL AS Combined_Column
FROM
    join_position_histomvts hm
JOIN
    audit_mvt ah ON ah.refcon = hm.refcon AND ah.version = hm.version
JOIN
    bo_kernel_status bks ON hm.backoffice = bks.id
JOIN
    tiers ts ON ts.ident = hm.entite
LEFT JOIN
    folio f ON f.ident = hm.opcvm
LEFT JOIN
    pfr_model_link pml ON pml.folio = (SELECT mgr FROM folio WHERE ident = f.mgr)
LEFT JOIN
    am_strategy str ON str.id = pml.strategy
WHERE
    hm.refcon = ""+ $TradeId+"""""
                            },
                        },
                        new PdtVariable {
                            name = "MirrorDetails",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
SELECT DISTINCT
    hm.refcon || ';' || ts.name || ';' || str.name || ';' || bks.name || ';' || TO_CHAR(ah.DateModif, 'YYYYMMDD-HH24:MI:SS') AS Combined_Column
FROM
    join_position_histomvts hm
JOIN
    audit_mvt ah ON ah.refcon = hm.refcon AND ah.version = hm.version
JOIN
    bo_kernel_status bks ON hm.backoffice = bks.id
JOIN
    tiers ts ON ts.ident = hm.entite
LEFT JOIN
    folio f ON f.ident = hm.opcvm
LEFT JOIN
    pfr_model_link pml ON pml.folio = (SELECT mgr FROM folio WHERE ident = f.mgr)
LEFT JOIN
    am_strategy str ON str.id = pml.strategy
WHERE
    hm.refcon = ""+ $MirrorId+"""""
                            },
                        },
                    },
                    processingCondition = "lineVal.Split(';')[2] == \"NACK\"",
                    columns = new []
                    {
                        new PdtColumn {
                            name = "Custodian",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Message ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "MessageID",
                                    expression = "\"$MessageId\""
                                },
                                new PdtColumnDest {
                                    path = "TradeID",
                                    expression = "\"$TradeId\""
                                },
                                new PdtColumnDest {
                                    path = "TradeID_Mir",
                                    expression = "\"$MirrorId\""
                                },
                                new PdtColumnDest {
                                    path = "Entity",
                                    expression = "\"$OrigninalDetails\".Split(';')[1]"
                                },
                                new PdtColumnDest {
                                    path = "Strategy",
                                    expression = "\"$OrigninalDetails\".Split(';')[2]"
                                },
                                new PdtColumnDest {
                                    path = "Strategy_Mir",
                                    expression = "\"$MirrorDetails\".Split(';')[2]"
                                },new PdtColumnDest {
                                    path = "Trade_Status",
                                    expression = "\"$OrigninalDetails\".Split(';')[3]"
                                },new PdtColumnDest {
                                    path = "Trade date",
                                    expression = "\"$OrigninalDetails\".Split(';')[5]"
                                },new PdtColumnDest {
                                    path = "Settlement date",
                                    expression = "\"$OrigninalDetails\".Split(';')[6]"
                                },new PdtColumnDest {
                                    path = "Date and time",
                                    expression = "\"$OrigninalDetails\".Split(';')[4]"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Response",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn
                        {
                            name = "Reason",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "NACK Error code" } }
                        },
                    }
                },//END SEA

                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_SwapTrade.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Swap Trade",
                    templateFile = "Rbc_Swaps.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    processingCondition = "\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") < 0 && \",ACS,ESW,\".IndexOf(\",\" + lineVal.Substring(113, 3).Trim() + \",\") >= 0",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { 
                                new PdtColumnDest { path = "Sub-fund Identifier" },
                                new PdtColumnDest { path = "External Fund Identifier" },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "MIFL_ME_10FLSNV_*",
                                        Expression = "lineVal.Substring(0, 6).Trim() == colVal ? lineVal.Substring(6, 40).Trim() : \"\"", //PF_ACR
                                    },
                                    path = "Fund Name",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Security Number",
                            len = 50,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "MIFL_ME_01ANATIT",
                                        ColumnIndex = "5",
                                        Expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")", //MATURITY_DAT_END
                                    },
                                    path = "Maturity Date",
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name= "Transaction Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Trade Date",
                                    expression = "System.DateTime.ParseExact(string.Compare(colVal, lineVal.Substring(76, 8).Trim()) == 1 ? lineVal.Substring(76, 8).Trim() : colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Settle Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Value Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Internal Transaction Number",
                            len = 12,
                            destPaths = new []{ new PdtColumnDest { path = "RBC Contract Number" } }
                        },
                        new PdtColumn
                        {
                            name = "External Transaction Number",
                            len = 16,
                        },
                        new PdtColumn
                        {
                            name = "Flag P/T",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Type of Transaction",
                            len = 4,
                        },
                        new PdtColumn
                        {
                            name = "Sign of Transaction",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Broker Code",
                            len = 20,
                            destPaths = new []{ 
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        File = "CtpyBicCodes.txt",
                                        Expression = "lineVal.Split(';')[2].Trim() == colVal ? lineVal.Split(';')[1].Trim() : \"\"",
                                    },
                                    path = "BIC Code Broker" 
                                },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Quantity",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Nominal Payable Leg",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                },
                                new PdtColumnDest {
                                    path = "Nominal Receivable Leg",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Security Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Net Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Close Price",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Accrued Interest Amount",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Gross Amount in Security Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "ISO Currency Code",
                            len = 3,
                        },
                        new PdtColumn
                        {
                            name = "Net Total Amount in Settlement Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Gross Total Amount in Settlement Currency",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Settlement ISO Currency Code",
                            len = 3,
                            destPaths = new[] {
                                new PdtColumnDest { path = "Currency Payable Leg" },
                                new PdtColumnDest { path = "Currency Receivable Leg" },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Settlement Currency Exchange Rate",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Fees Amount",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Fees",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Commission Broker",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Commission Others",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Internal Contract Number",
                            len = 10,
                        },
                    }
                },
                //new PdtTransformation
                //{
                //    name = TransName.SSB2Rbc_Invoice.ToString(),
                //    type = TransType.Csv2Csv,
                //    label = "SSB to RBC Invoice",
                //    templateFile = "Rbc_Invoice.csv",
                //    category = "Medio",
                //    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                //    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                //    columns = new []
                //    {
                //        new PdtColumn
                //        {
                //            name = "Portfolio Code",
                //            len = 6,
                //            destPaths = new [] { 
                //                new PdtColumnDest { path = "Fund Custody Code" },
                //                new PdtColumnDest { path = "External Fund Identifier" }
                //            }
                //        },
                //        new PdtColumn
                //        {
                //            name = "ISO Currency Code",
                //            len = 3,
                //            destPaths = new[] { new PdtColumnDest { path = "Currency" } }
                //        },
                //        new PdtColumn
                //        {
                //            name = "Internal Transaction Number",
                //            len = 12,
                //            destPaths = new []{ new PdtColumnDest { path = "Transaction ID" } }
                //        },
                //        new PdtColumn
                //        {
                //            name = "Internal Contract Number",
                //            len = 10,
                //        },
                //        new PdtColumn
                //        {
                //            name= "Transaction Date",
                //            len = 8,
                //            destPaths = new [] {
                //                new PdtColumnDest {
                //                    path = "Trade Date",
                //                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                //                } }
                //        },
                //        new PdtColumn
                //        {
                //            name = "Settle Date",
                //            len = 8,
                //            destPaths = new [] {
                //                new PdtColumnDest {
                //                    path = "Value Date",
                //                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\")"
                //                } }
                //        },
                //        new PdtColumn
                //        {
                //            name = "Type of Transaction",
                //            len = 4,
                //            destPaths = new []{
                //                new PdtColumnDest { 
                //                    Lookup = new PdtColumnLookup {
                //                        File = "MIFL_ME_15DOMIN_*",
                //                        Expression = "lineVal.Substring(0, 20).Trim() == \"XACT_TYP\" && lineVal.Substring(20, 10).Trim() == colVal ? lineVal.Substring(30, 40).Trim() : \"\"",
                //                    },
                //                    path = "Fee Type" 
                //                } 
                //            }
                //        },
                //        new PdtColumn
                //        {
                //            name = "Sign of Transaction",
                //            len = 1,
                //        },
                //        new PdtColumn
                //        {
                //            name = "Current Amount",
                //            len = 17,
                //            destPaths = new [] {
                //                new PdtColumnDest {
                //                    path = "Fee Amount",
                //                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                //                } }
                //        },
                //        new PdtColumn
                //        {
                //            name = "Internal Security Number",
                //            len = 10,
                //        },
                //        new PdtColumn
                //        {
                //            name = "Position type",
                //            len = 1,
                //        },
                //    }
                //},
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_NavFund.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Nav Fund",
                    templateFile = "Rbc_NavFund.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    processingCondition = "(lineVal.Substring(0, 6).Trim() == \"M4\" || lineVal.Substring(0, 6).Trim() == \"M6\") && lineVal.Substring(46, 12).Trim() != \"\"",
                    groupBy = new[] {"Fund Code", "Fund name", "Date Nav"},
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] { new PdtColumnDest { path = "Fund Code" } }
                        },
                        new PdtColumn
                        {
                            name = "Portfolio Name",
                            len = 40,
                            destPaths = new [] { new PdtColumnDest { path = "Fund name" } }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                            //destPaths = new [] { new PdtColumnDest { path = "C Id Isin" } }
                        },
                        new PdtColumn
                        {
                            name = "NAV Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Date Nav",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd-MMM-yyyy\").ToUpper()"
                                } }
                        },
                        new PdtColumn
                        {
                            name= "Next NAV Date",
                            len = 8,
                        },
                        new PdtColumn
                        {
                            name = "Net Value",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Nav Net",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))",
                                    aggregation = "SUM",
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Share Value",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Price Share",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))",
                                    aggregation = "AVG",
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Share Value Gross",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Number Share",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Quantity Outstanding",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))",
                                    aggregation = "SUM",
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Id. Official Quota",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Id. Validity Quota",
                            len = 1,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.SSB2Rbc_NavStrategy.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB to RBC Nav Strategy",
                    templateFile = "Rbc_NavStrategy.csv",
                    category = "Medio",
                    //csvSrcSeparator = (char)0, // empty --> fixed length csv
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    processingCondition = "lineVal.Substring(0, 6).Trim() != \"M4\" && lineVal.Substring(0, 6).Trim() != \"M6\"",
                    columns = new []
                    {
                        new PdtColumn
                        {
                            name = "Portfolio Code",
                            len = 6,
                            destPaths = new [] {
                                new PdtColumnDest { path = "Fund Code" },
                                new PdtColumnDest { path = "Manager No" },
                                new PdtColumnDest { path = "Manager Code" },
                            }
                        },
                        new PdtColumn
                        {
                            name = "Portfolio Name",
                            len = 40,
                            destPaths = new [] { new PdtColumnDest { path = "Fund" } }
                        },
                        new PdtColumn
                        {
                            name = "ISIN Code",
                            len = 12,
                        },
                        new PdtColumn
                        {
                            name = "NAV Date",
                            len = 8,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Date",
                                    expression = "System.DateTime.ParseExact(colVal, \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\").ToUpper()"
                                } }
                        },
                        new PdtColumn
                        {
                            name= "Next NAV Date",
                            len = 8,
                        },
                        new PdtColumn
                        {
                            name = "Net Value",
                            len = 17,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Market Value",
                                    expression = "colVal.Substring(16, 1) + colVal.Substring(0, 15 - int.Parse(colVal.Substring(15, 1))) + \".\" + colVal.Substring(15 - int.Parse(colVal.Substring(15, 1)), int.Parse(colVal.Substring(15, 1)))"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "Share Value",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Share Value Gross",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Number Share",
                            len = 17,
                        },
                        new PdtColumn
                        {
                            name = "Id. Official Quota",
                            len = 1,
                        },
                        new PdtColumn
                        {
                            name = "Id. Validity Quota",
                            len = 1,
                        },
                    }
                },
                new PdtTransformation
                {
                    name = TransName.FundSettle2Rbc_OrderExec.ToString(),
                    type = TransType.Csv2Csv,
                    label = "Fund Settle to RBC Order Execution",
                    templateFile = "Rbc_OrderExecution.csv",
                    category = "Medio",
                    csvSkipLines = 1,
                    csvSrcSeparator = ',',
                    csvDestSeparator = ',',
                    columns = new []
                    {
                        new PdtColumn { name = "Participant account number" },
                        new PdtColumn { name = "Participant name" },
                        new PdtColumn {
                            name = "ISIN",
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "fund-id-ext",
                                    expression = "\"ISIN:\" + colVal"
                                },
                                new PdtColumnDest { 
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT T.REFERENCE, T.SICOVAM, ERI.VALUE ISIN FROM EXTRNL_REFERENCES_DEFINITION ERD JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT WHERE ERD.REF_NAME = 'ISIN' AND ERI.VALUE = '\" + colVal + \"'\"",
                                    },
                                    path = "bloomberg-security-code",
                                },
                            }
                        },
                        new PdtColumn { name = "Fund name" },
                        new PdtColumn {
                            name = "FundSettle order number",
                            destPaths = new [] { new PdtColumnDest { path = "confirm-no" } }
                        },
                        new PdtColumn {
                            name = "Your reference",
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "all-shares",
                                    expression = "\"no\"",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = "\"SELECT QUANTITY FROM ORDER_PLACEMENT WHERE ORDERID IN (\" + colVal + \")\"",
                                    },
                                    path = "order-amt",
                                },
                                new PdtColumnDest { path = "trn-id" } 
                            }
                        },
                        new PdtColumn {
                            name = "Order type",
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "tran-code" ,
                                    expression = "colVal == \"Subscription\" ? \"CTB\" : \"BTC\""
                                } }
                        },
                        new PdtColumn {
                            name = "Order date",
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "order-date",
                                    expression = "colVal == \"\" ? \"\" : DateTime.Parse(colVal).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn { 
                            name = "Trade date",
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "entry-date",
                                    expression = "colVal == \"\" ? \"\" : DateTime.Parse(colVal).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn { 
                            name = "Contractual settlement date",
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "date-settled",
                                    expression = "colVal == \"\" ? \"\" : DateTime.Parse(colVal).ToString(\"dd/MM/yyyy\")"
                                } }
                        },
                        new PdtColumn { name = "Effective settlement date" },
                        new PdtColumn { name = "Expected contract note date" },
                        new PdtColumn {
                            name = "Life cycle step - Status",
                            destPaths = new [] { new PdtColumnDest { path = "h-status" } }
                        },
                        new PdtColumn { name = "Last life cycle step - Status timestamp" },
                        new PdtColumn {
                            name = "Number of shares",
                            destPaths = new [] { new PdtColumnDest { path = "receipt-shares" } }
                        },
                        new PdtColumn {
                            name = "Payment currency",
                            destPaths = new [] { new PdtColumnDest { path = "local-currency" } }
                        },
                        new PdtColumn {
                            name = "Price 1",
                            destPaths = new [] { new PdtColumnDest { path = "price-used" } }
                        },
                        new PdtColumn { name = "Price type" },
                        new PdtColumn { name = "Exchange currencies" },
                        new PdtColumn {
                            name = "Exchange rate",
                            destPaths = new [] { new PdtColumnDest { 
                                path = "loc-curr-rate",
                                expression = "1",
                            } }
                        },
                        new PdtColumn {
                            name = "Cash amount",
                            destPaths = new [] { 
                                new PdtColumnDest {
                                    path = "receipt-amt",
                                    expression = "(string.IsNullOrEmpty((new System.Text.RegularExpressions.Regex(\",(?=(?:[^\\\"]*\\\"[^\\\"]*\\\")*(?![^\\\"]*\\\"))\")).Split(lineVal)[14]) ? 0 : double.Parse((new System.Text.RegularExpressions.Regex(\",(?=(?:[^\\\"]*\\\"[^\\\"]*\\\")*(?![^\\\"]*\\\"))\")).Split(lineVal)[14])) * (string.IsNullOrEmpty((new System.Text.RegularExpressions.Regex(\",(?=(?:[^\\\"]*\\\"[^\\\"]*\\\")*(?![^\\\"]*\\\"))\")).Split(lineVal)[16]) ? 0 : double.Parse((new System.Text.RegularExpressions.Regex(\",(?=(?:[^\\\"]*\\\"[^\\\"]*\\\")*(?![^\\\"]*\\\"))\")).Split(lineVal)[16]))"
                                } }
                        },
                        new PdtColumn { name = "Settlement status" },
                        new PdtColumn { name = "Narrative text" },
                        new PdtColumn { name = "Last narrative timestamp" },
                        new PdtColumn { name = "By order of" },
                        new PdtColumn { name = "In favor of" },
                        new PdtColumn {
                            name = "Account Id",
                            destPaths = new [] { new PdtColumnDest { path = "account-no" } }
                        },
                    }
                },
                
                //BBH_DIM_Corporate_Action_SBRI
                    new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_SBRI.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action SBRI",
                    templateFile = "BBH_DIM_Corporate_Action_SBRI.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_BonusRightIssue']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    variables = new[] {

                        new PdtVariable {
                            name = "SicovamCash",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) > 1"""
                            },
                        },

                        new PdtVariable {
                            name = "SicovamSecurity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) > 1"""
                            },
                        },

                    },

                    processingCondition= "lineVal.Split(',')[7] == \"Standard Bonus Right Issue\" && lineVal.Split(',')[24] == \"SECU\"",

                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//tkt column
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ExDate']",
                                expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'SubscriptionStartDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'SubscriptionEndDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = @"//*[local-name() = 'PaymentDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {
                            name = "ISIN",//16
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT SICOVAM FROM TITRES WHERE REFERENCE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM
FROM EXTRNL_REFERENCES_DEFINITION ERD
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
    JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT
WHERE ERD.REF_NAME = 'ISIN' AND ERI.VALUE = '"" + colVal + ""'"""
                                },
                                path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'sophis']",
                            } }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "Out-Turn ISIN",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT SICOVAM FROM TITRES WHERE REFERENCE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM
FROM EXTRNL_REFERENCES_DEFINITION ERD
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
    JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT
WHERE ERD.REF_NAME = 'ISIN' AND ERI.VALUE = '"" + colVal + ""'"""
                                },
                                path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'sophis']",
                            } }
                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//25
                            name = "Ccy",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//26
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'CorporateActionOptions']//*[local-name() = 'Currency']" } }
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//32
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//33
                            name = "RatioNew",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'AdditionalShares']//*[local-name() = 'NewShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//34
                            name = "RatioOld",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'AdditionalShares']//*[local-name() = 'OldShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//35
                            name = "MESSAGE_TYPE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//36
                            name = "RECORD_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//37
                            name = "RHTS_ISIN_OF_INTERMEDIATE_SECURITY",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//38
                            name = "COMMENT_FIELD",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//39
                            name = "EFFECTIVE_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//40
                            name = "Disposition_Of_Fractions_Shares",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//41
                            name = "Right_Ratio_New",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'NewShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//42
                            name = "Right_Ratio_Old",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'OldShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//43
                            name = "Disposition_Of_Fractions_Rights",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'FractionalShares']//*[local-name() = 'Mode']",
                                    expression = "colVal == \"Round Up\" ? \"RoundUpForFree\" : colVal == \"Round Down\" ? \"RoundDown\" : \"CashedOutAtClosingPrice\""
                                }
                            }
                        },
                    }

                },
 
                    //BBH_DIM_Corporate_Action_SD    : Standard Dividend
                    new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_SD.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action SD",
                    templateFile = "BBH_DIM_Corporate_Action_SD.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_DVCA']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    variables = new[] {


                        new PdtVariable {
                            name = "SicovamCash",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) > 1"""
                            },
                        },

                    },


                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//tkt column
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,
                         },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ExDate']",
                                expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                         },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                         },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = @"//*[local-name() = 'PaymentDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {
                            name = "ISIN",//16
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'References']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamCash\""
                                },
                            }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "isin",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//25
                            name = "Ccy",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//26
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'CorporateActionOptions']//*[local-name() = 'Currency']" } }
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'Dividend_Amount']",
                                expression =  "colVal == \"\"? \"0\" : colVal"
                            } }
                        },
                        new PdtColumn {//32
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//33
                            name = "RatioNew",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//34
                            name = "RatioOld",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//35
                            name = "MESSAGE_TYPE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//36
                            name = "RECORD_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'RecordDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        }

                    }

                },
 
                    //BBH_DIM_Corporate_Action_SS  : Standard Scrip
                    new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_SS.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action SS",
                    templateFile = "BBH_DIM_Corporate_Action_SS.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_SCRIP']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    variables = new[] {

                        new PdtVariable {
                            name = "SicovamCash",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) > 1"""
                            },
                        },

                        new PdtVariable {
                            name = "SicovamSecurity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) > 1"""
                            },
                        },

                    },

                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//tkt column
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                           destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ExDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                         },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                                                        destPaths = new [] {
                                new PdtColumnDest {
                                    path = @"//*[local-name() = 'PaymentDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {
                            name = "ISIN",//16
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'References']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamCash\""
                                },
                            }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "isin",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//25
                            name = "Ccy",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//26
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//32
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//33
                            name = "RatioNew",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'NewShares']",
                                expression = "colVal == \"\"? \"1\" : colVal"
                            } }
                        },
                        new PdtColumn {//34
                            name = "RatioOld",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'OldShares']",
                                expression = "colVal == \"\"? \"1\" : colVal"
                            } }
                        },
                        new PdtColumn {//35
                            name = "MESSAGE_TYPE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//36
                            name = "RECORD_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'RecordDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                    }

                },
 

                    //BBH_DIM_Corporate_Action_SCDR
                    new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_SCDR.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action SCDR",
                    templateFile = "BBH_DIM_Corporate_Action_SCDR.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_DRIP']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    variables = new[] {

                        new PdtVariable {
                            name = "SicovamCash",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) > 1"""
                            },
                        },

                        new PdtVariable {
                            name = "SicovamSecurity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) > 1"""
                            },
                        },


                    },


                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//tkt column
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ExDate']",
                                expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                         },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = @"//*[local-name() = 'PaymentDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {
                            name = "ISIN",//16
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'References']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamCash\""
                                },
                            }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "Out-Turn ISIN",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//25
                            name = "Ccy",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path ="//*[local-name() = 'CashDividend']//*[local-name() = 'Currency']" } }
                        },

                        new PdtColumn {//2
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'CashDividend']//*[local-name() = 'Amount']",
                                expression =  "lineVal.Split(',')[22] ==\"Cash\" ?  lineVal.Split(',')[31] : lineVal.Split(',')[22] "
                            } }
                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//32
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//33
                            name = "RatioNew",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'NewShares']",
                                expression = "colVal == \"\"? \"0\" : colVal"
                            } }
                        },
                        new PdtColumn {//34
                            name = "RatioOld",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'OldShares']",
                                expression = "colVal == \"\"? \"0\" : colVal"
                            } }
                        },
                        new PdtColumn {//35
                            name = "MESSAGE_TYPE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//36
                            name = "RECORD_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'RecordDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },
                    }

                },
                    
                    //BBH_DIM_Corporate_Action_SRI
                    //RHTS is a type of Standard Right Issue
                    //EXER Option
                 new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_RHTS.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action RHTS",
                    templateFile = "BBH_DIM_Corporate_Action_SRI.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_RightIssue']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    variables = new[] {

                        new PdtVariable {
                            name = "SicovamCash",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) > 1"""
                            },
                        },

                        new PdtVariable {
                            name = "SicovamSecurity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) > 1"""
                            },
                        },

                    },


                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//tkt column
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ExDate']",
                                expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'SubscriptionStartDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'SubscriptionEndDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = @"//*[local-name() = 'PaymentDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {
                            name = "ISIN",//16
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'References']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamCash\""
                                },
                            }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "isin",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//25
                            name = "Ccy",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//26
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'SubscriptionPrice']//*[local-name() = 'Price']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//32
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//33
                            name = "RatioNew",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'AdditionalShares']//*[local-name() = 'NewShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//34
                            name = "RatioOld",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'AdditionalShares']//*[local-name() = 'OldShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//35
                            name = "MESSAGE_TYPE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//36
                            name = "RECORD_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//37
                            name = "RHTS_ISIN_OF_INTERMEDIATE_SECURITY",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT SICOVAM FROM TITRES WHERE REFERENCE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM
FROM EXTRNL_REFERENCES_DEFINITION ERD
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
    JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT
WHERE ERD.REF_NAME = 'ISIN' AND ERI.VALUE = '"" + colVal + ""'"""
                                },
                                path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'sophis']",
                            } }
                        },
                        new PdtColumn {//38
                            name = "COMMENT_FIELD",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//39
                            name = "EFFECTIVE_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//40
                            name = "Disposition_Of_Fractions_Shares",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//41
                            name = "Right_Ratio_New",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'NewShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//42
                            name = "Right_Ratio_Old",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'OldShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//43
                            name = "Disposition_Of_Fractions_Rights",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'FractionalShares']//*[local-name() = 'Mode']",
                                    expression = "colVal == \"Round Up\" ? \"RoundUpForFree\" : colVal == \"Round Down\" ? \"RoundDown\" : \"CashedOutAtClosingPrice\""
                                }
                            }
                        },
                        new PdtColumn {//44
                            name = "Place_of_Listing",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                    }

                },


                     //BBH_DIM_Corporate_Action_SRI
                    //EXRI is a type of Standard Right Issue
                    //EXER Option
                 new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_EXRI.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action EXRI",
                    templateFile = "BBH_DIM_Corporate_Action_SRI.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_RightIssue']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    processingCondition= "lineVal.Split(',')[22].ToUpper() == \"EXER\" && lineVal.Split(',')[45].ToUpper() == \"EXRI\"",



                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//1
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ExDate']",
                                expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'SubscriptionStartDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'SubscriptionEndDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = @"//*[local-name() = 'PaymentDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {//ISIN on the instr with CA
                            name = "ISIN",//16
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT SICOVAM FROM TITRES WHERE REFERENCE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM
FROM EXTRNL_REFERENCES_DEFINITION ERD
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
    JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT
WHERE ERD.REF_NAME = 'ISIN' AND ERI.VALUE = '"" + colVal + ""'"""
                                },
                                path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'sophis']",
                            } }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "isin",//Out turn ISIN
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT SICOVAM FROM TITRES WHERE REFERENCE = '"" + colVal + @""'
UNION
SELECT T.SICOVAM
FROM EXTRNL_REFERENCES_DEFINITION ERD
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
    JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT
WHERE ERD.REF_NAME = 'ISIN' AND ERI.VALUE = '"" + colVal + ""'"""
                                },
                                path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'References']//*[local-name() = 'sophis']",
                            } }


                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//25
                            name = "Ccy",//Currency of the option
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'SubscriptionPrice']//*[local-name() = 'Currency']"
                                }
                            }
                        },

                        new PdtColumn {//26
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'SubscriptionPrice']//*[local-name() = 'Price']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//32
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//33
                            name = "RatioNew",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'AdditionalShares']//*[local-name() = 'NewShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//34
                            name = "RatioOld",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'AdditionalShares']//*[local-name() = 'OldShares']",
                                    expression = "colVal == \"\"? \"0\" : colVal"
                                }
                            }
                        },
                        new PdtColumn {//35
                            name = "MESSAGE_TYPE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//36
                            name = "RECORD_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//37
                            name = "RHTS_ISIN_OF_INTERMEDIATE_SECURITY",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//38
                            name = "COMMENT_FIELD",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//39
                            name = "EFFECTIVE_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//40
                            name = "Disposition_Of_Fractions_Shares",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'Mode']",
                                    expression =  "colVal == \"Cash-in-lieu\"? \"CashedOutAtClosingPrice\" : (  colVal == \"Round Up\"? \"RoundUpForFree\" : \"RoundDown\" )"    //TODO : TEST
                                }
                            }
                        },
                        new PdtColumn {//41
                            name = "Right_Ratio_New",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
                                    select t1.ratio_new
                                    from medio_staging_corporateaction T1
                                    join medio_staging_corporateaction T2 on T1.isin_of_the_instrument_with_ca = T2.out_turn_isin
                                    where T1.CAEV = 'RHDI'
                                    and T2.CAEV ='EXRI'
                                    and T1.isin_of_the_instrument_with_ca = '"" + lineVal.Split(',')[23] + @""' and T1.sender_bic_code = '"" + lineVal.Split(',')[1] + @""'
                                    and T2.sender_bic_code = '"" + lineVal.Split(',')[1] + ""'"""
                                },
                                path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'NewShares']",
                            } }
                        },
                        new PdtColumn {//42
                            name = "Right_Ratio_Old",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
                                    select t1.ratio_old
                                    from medio_staging_corporateaction T1
                                    join medio_staging_corporateaction T2 on T1.isin_of_the_instrument_with_ca = T2.out_turn_isin
                                    where T1.CAEV = 'RHDI'
                                    and T2.CAEV ='EXRI'
                                    and T1.isin_of_the_instrument_with_ca = '"" + lineVal.Split(',')[23] + @""' and T1.sender_bic_code = '"" + lineVal.Split(',')[1] + @""'
                                    and T2.sender_bic_code = '"" + lineVal.Split(',')[1] + ""'"""
                                },
                                path ="//*[local-name() = 'IssuedRight']//*[local-name() = 'OldShares']",
                            } }
                        },

                    }

                },


                 //BBH_DIM_Corporate_Action_SEO
                 new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_SEO.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action SEO",
                    templateFile = "BBH_DIM_Corporate_Action_SEO.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_ExchangeOffer']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    variables = new[] {


                        new PdtVariable {
                            name = "SicovamCash",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) > 1"""
                            },
                        },

                        new PdtVariable {
                            name = "SicovamSecurity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) > 1"""
                            },
                        },

                    },


                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//tkt column
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ElectionStartDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ElectionEndDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = @"//*[local-name() = 'PaymentDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {
                            name = "ISIN",//16 //SEO
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'References']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamCash\""
                                },
                            }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "Out-Turn ISIN",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'ExchangedShares']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamSecurity\""
                                },
                            }
                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//25
                            name = "Ccy",//Currency of the option
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            {    new PdtColumnDest
                                {
                                    path ="//*[local-name() = 'UnitPrice']//*[local-name() = 'Currency']"
                                },

                                 new PdtColumnDest
                                {
                                    path ="//*[local-name() = 'TicketPriceForTheOldShare']",
                                    expression = "lineVal.Split(',')[25]== \"\" && lineVal.Split(',')[26]== \"\" ? \"No\" :   \"Yes\"",
                                }
                            }
                        },

                        new PdtColumn {//26
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'UnitPrice']//*[local-name() = 'Price']",
                                expression =  "colVal == \"\"? \"0\" : colVal"
                            } }
                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//32
                            name = "MarketResponseDeadline",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        }
                        ,
                        new PdtColumn {//33
                            name = "RatioNew",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'NewSharesForOldShares']//*[local-name() = 'NewShares']"
                            } }
                        }
                        ,
                        new PdtColumn {//34
                            name = "RatioOld",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'NewSharesForOldShares']//*[local-name() = 'OldShares']"
                            } }
                        }
                    }

                },

                 //BBH_DIM_Corporate_Action_STO
                 new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_STO.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action STO",
                    templateFile = "BBH_DIM_Corporate_Action_STO.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_TenderOffer']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    variables = new[] {


                        new PdtVariable {
                            name = "SicovamCash",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) > 1"""
                            },
                        },

                        new PdtVariable {
                            name = "SicovamSecurity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) > 1"""
                            },
                        },

                    },

                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//tkt column
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                         },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                         },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ElectionStartDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ElectionEndDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = @"//*[local-name() = 'PaymentDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {
                            name = "ISIN",//16
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'References']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamCash\""
                                },
                            }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "isin",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//25
                            name = "Ccy",//Currency of the option
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//26
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'PriceOffer']//*[local-name() = 'Currency']"
                            } }
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'PriceOffer']//*[local-name() = 'Price']",
                                expression =  "colVal == \"\"? \"0\" : colVal"
                            } }
                        },
                        new PdtColumn {//32
                            name = "MarkerResponseDeadline",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'ResultDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        }
                    }

                },

                //BBH_DIM_Corporate_Action_SMA
                 new PdtTransformation
                {
                    name = TransName.BBH_DIM_Corporate_Action_SMA.ToString(),//FP-1214
                    type = TransType.Csv2Xml,
                    label = "Infomediary DIM Corporate Action SMA",
                    templateFile = "BBH_DIM_Corporate_Action_SMA.xml",
                    category = "Medio",
                    repeatingRootPath = "//*[local-name() = 'importMarketData']",
                    repeatingChildrenPath = "//*[local-name() = 'CA_MergerAcquisition']",
                    csvSrcSeparator = ',',
                    csvSkipLines = 1,

                    variables = new[] {

                        new PdtVariable {
                            name = "SicovamCash",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[16] + @""'
                                    ) > 1"""
                            },
                        },

                        new PdtVariable {
                            name = "SicovamSecurity",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = @"@""
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) = 1
                                    UNION
                                    SELECT t1.sicovam
                                    FROM titres t1
                                        left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                    JOIN join_position_histomvts t2 ON t1.sicovam = t2.sicovam
                                    WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    AND (
                                        SELECT COUNT(*)
                                        FROM titres t1
                                            left JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T1.SICOVAM
                                        left JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT
                                        WHERE  ERI.value = '"" + lineVal.Split(',')[23] + @""'
                                    ) > 1"""
                            },
                        },

                    },


                    columns = new [] {
                        new PdtColumn {
                            name = "Message Topic Name",//0
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {//1
                            name = "Sender BIC Code",
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody CA Reference",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest {
                                path = @"//*[local-name() = 'References']//*[local-name() = 'reference'][contains(@*[local-name() = 'name'], 'SophisName')]",
                                expression = "lineVal.Split(',')[2]+\"_\"+lineVal.Split(',')[21]",
                            } }
                        },
                        new PdtColumn {
                            name = "Unique CA Event Market Identifier ",//3
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Custody Message Reference",//4
                            isRequired = true,
                            isRelativeToRootNode = true,

                        },
                        new PdtColumn {
                            name = "Message Status",//5
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Group",//6
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {
                            name = "Corporate Action Event Indicator",//7
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Processing Status",//8
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Default Processing Flag",//9
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        
                       #region Dates
                        new PdtColumn {
                            name = "Announcement Date",//10
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Ex Date",//11
                            isRequired = true,
                            isRelativeToRootNode = true,
                         },

                        new PdtColumn {
                            name = "Instruction Start Phase",//12
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Instruction End Phase",//13
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Payment/ Settlement Date",//14
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },


                        #endregion Dates
                        new PdtColumn {
                            name = "Description of the Security",//15
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                         new PdtColumn {
                            name = "ISIN",//16
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'UnderlyingSecurities']//*[local-name() = 'References']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamCash\""
                                },
                            }
                         },


                          new PdtColumn {
                            name = "Currency of the instrument",//17
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Party",//18
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Account Details",//19
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Place of Safekeeping",//20
                            isRequired = true,
                            isRelativeToRootNode = true,

                            },


                        new PdtColumn {
                            name = "CA Election Option Choice Number",//21
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {
                            name = "Type of Adjustment",//22
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//23
                            name = "Out-Turn ISIN",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path ="//*[local-name() = 'ReferenceOfTheNewCompany']//*[local-name() = 'sophis']",
                                    expression = "\"$SicovamSecurity\""
                                },
                            }
                        },
                        new PdtColumn {//24
                            name = "description",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'Renaming']//*[local-name() = 'NewNameOfCompany']"
                            } }
                        },
                        new PdtColumn {//25
                            name = "Ccy",//skip
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//26
                            name = "Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//27
                            name = "Unit Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//28
                            name = "Quantity Instructed",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//29
                            name = "Cash/Securities Account",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },

                        new PdtColumn {//30
                            name = "Cash Amount Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'CorporateActionOptions']//*[local-name() = 'Cash']//*[local-name() = 'Currency']" } }
                        },

                        new PdtColumn {//31
                            name = "Cash Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'Cash']//*[local-name() = 'Price']"
                            } }
                        },
                        new PdtColumn {//32
                            name = "Narrative",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//33
                            name = "RatioNew",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'NewSharesForOldShares']//*[local-name() = 'NewShares']"
                            } }
                        },
                        new PdtColumn {//34
                            name = "RatioOld",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new []
                            { new PdtColumnDest
                            {
                                path ="//*[local-name() = 'NewSharesForOldShares']//*[local-name() = 'OldShares']"
                            } }
                        },
                        new PdtColumn {//35
                            name = "MESSAGE_TYPE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//36
                            name = "RECORD_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//37
                            name = "RHTS_ISIN_OF_INTERMEDIATE_SECURITY",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//38
                            name = "COMMENT_FIELD",
                            isRequired = true,
                            isRelativeToRootNode = true,
                        },
                        new PdtColumn {//39
                            name = "EFFECTIVE_DATE",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'EffectiveDate']",
                                    expression="colVal == \"\"? \"1904-01-01\" : System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"} }
                        },
                        new PdtColumn {//40
                            name = "Disposition_Of_Fractions_Shares",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'FractionalShares']//*[local-name() = 'Mode']",
                                    expression = "colVal == \"Round Up\" ? \"RoundUpForFree\" : \"CashedOutAtClosingPrice\""
                                }
                            }
                        }
                    }

                },
                new PdtTransformation
                {
                    name = TransName.SSBOTC_MARGIN_Parser.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB OTC Margin Parser",
                    templateFile = "SSB_OTC_Margin_Calculation.csv",
                    category = "Medio",
                    processingCondition = "lineVal.Substring(0, 6).Trim() != \"M4\" && lineVal.Substring(0, 6).Trim() != \"M6\" && lineVal.Substring(29, 2) == \"03\" && (lineVal.Substring(17, 4) == \"CC23\" || lineVal.Substring(17, 4) == \"CSA.\" || lineVal.Substring(17, 7) == \"MIOTC23\")",
                    csvDestSeparator = DEFAULT_CSV_SEPARATOR,
                    groupBy = new[] {"Fund Code", "CUSIP Prefix"},
                    variables = new[]
                    {
                        new PdtVariable
                        {
                            name = "FundCode",
                            expressionBefore = "lineVal.Substring(0, 6).Trim()",
                        },
                        new PdtVariable
                        {
                            name = "CusipPrefix",
                            expressionBefore= "lineVal.Substring(17, 7) == \"MIOTC23\" ? \"MIOTC23\" : lineVal.Substring(17, 4)",
                        },
                        new PdtVariable
                        {
                            name = "CustodyFundCode",
                            Lookup = new PdtColumnLookup {
                                Table = "SQL",
                                Expression = "\"SELECT custody_fund_code from medio_ssb_otc_fund_codes where fa_fund_code = '\" + \"$FundCode\" + \"'\"",
                            },
                        }
                    },
                    columns = new[]
                    {
                        new PdtColumn
                        {
                            //Replace Fa Fund Code by Custody Fund Code in case of CSA. Ingestion and the Custody Fund Code is not Null
                            name = "Fund Code",
                            len = 6,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Fund Code",
                                    //(!string.IsNullOrEmpty(\"$CustodyFundCode\") && \"$CusipPrefix\" == \"CSA.\") ? \"$CustodyFundCode\" : \"$FundCode\"
                                    expression = "(!string.IsNullOrEmpty(\"$CustodyFundCode\") && \"$CusipPrefix\" == \"CSA.\") ? \"$CustodyFundCode\" : \"$FundCode\"",
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Date",
                            len = 11,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Date",
                                    expression = "System.DateTime.ParseExact(colVal.Substring(3, 8), \"yyyyMMdd\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"dd/MM/yyyy\").ToUpper()"
                                } }
                        },
                        new PdtColumn
                        {
                            name = "CUSIP Prefix",
                            len = 7,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "CUSIP Prefix",
                                    //expression = "colVal.Substring(0, 7) == \"MIOTC23\" ? \"MIOTC23\" : colVal.Substring(0, 4)",
                                    expression = "\"$CusipPrefix\"",
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Net Amount",
                            len = 47,
                            destPaths = new []
                            {
                                new PdtColumnDest {
                                    path = "Net Amount",
                                    expression = "long.Parse(colVal.Substring(28,17)) * System.Math.Pow(10,- int.Parse(colVal.Substring(45, 1))) * (colVal.Substring(46, 1) == \"+\" ? 1 : -1)",
                                    aggregation = "SUM",
                                }
                            }
                        },
                    },
                },
                new PdtTransformation
                {
                    name = TransName.SSB_File_Sequencing.ToString(),
                    type = TransType.Csv2Csv,
                    label = "SSB_File_Sequencing",
                    templateFile = "SSB_OTC_Margin_Calculation.csv",
                    category = "Medio",
                    csvSkipLines = 1,
                    csvDestSeparator = ';',
                    columns = new[]
                    {
                        new PdtColumn
                        {
                            name = "Fund Code",
                            destPaths = new[] {
                                new PdtColumnDest
                                {
                                    path = "Fund Code",
                                    expression = "lineVal.Split(';')[0]"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "CUSIP Prefix",
                            destPaths = new[] {
                                new PdtColumnDest
                                {
                                    path = "CUSIP Prefix",
                                    expression = "lineVal.Split(';')[1]"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Date",
                            destPaths = new[] {
                                new PdtColumnDest
                                {
                                    path = "Date",
                                    expression = "lineVal.Split(';')[2]"
                                }
                            }
                        },
                        new PdtColumn
                        {
                            name = "Net Amount",
                            destPaths = new[] {
                                new PdtColumnDest
                                {
                                    path = "Net Amount",
                                    expression = "lineVal.Split(';')[3]"
                                }
                            }
                        },
                    }
                }
            }
            };

            var serializer = new XmlSerializer(typeof(PdtTransformationSetting));
            TextWriter writer = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile));
            serializer.Serialize(writer, Setting);
            writer.Close();

            Logger.Warn("END");
            return Setting;
        }

        public static void Transform(TransformationIO transIO, PdtTransformationSetting Setting, string transName, string inputFile, string outputFile, string failureFile)
        {
            PdtTransformation trans = Setting.Transformations.Where(x => x.name == transName).FirstOrDefault();
            if (trans == null) throw new ArgumentException(string.Format("Transformation name not found: {0}", transName));
            if (trans.type == TransType.Csv2Csv)
                Transform2Csv(transIO, Setting, transName, inputFile, outputFile, failureFile);
            else if (trans.type == TransType.Csv2Xml)
                Transform2Xml(transIO, Setting, transName, inputFile, outputFile, failureFile);
            else if (trans.type == TransType.Xml2Csv)
                TransformXml2Csv(transIO, Setting, transName, inputFile, outputFile);
            else if (trans.type == TransType.Excel2Csv)
                Transform2Csv(transIO, Setting, transName, inputFile, outputFile, failureFile, true);
        }

        private static void Transform2Csv(TransformationIO transIO, PdtTransformationSetting Setting, string transName, string inputCsvFile, string outputCsvFile, string failureFile, bool isExcelFile=false)
        {
            PdtTransformation trans = Setting.Transformations.Where(x => x.name == transName).FirstOrDefault();
            if (trans == null) throw new ArgumentException(string.Format("Transformation name not found: {0}", transName));

            Logger.Debug($"BEGIN(name={trans.name}, inputCsvFile={inputCsvFile}, outputCsvFile={outputCsvFile}, failureFile={failureFile}, templateFile={trans.templateFile})");
            int auditId = Utils.AuditStart(transName, trans.type.ToString(), inputCsvFile, outputCsvFile, failureFile);

            string headerLine;
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, trans.templateFile)))
            {
                headerLine = File.ReadLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, trans.templateFile)).First();
            } else
            {
                var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + trans.templateFile));
                if (resourceName == null) throw new ArgumentException(string.Format("Template file not found in resource: {0}", trans.templateFile));

                using (var srTemplate = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
                {
                    headerLine = srTemplate.ReadLine();
                }
            }
            var csvHeaders = headerLine.Split(trans.csvDestSeparator);

            string[] lines;
            if (File.Exists(inputCsvFile))
            {
                if (!Utils.IsFileClosed(inputCsvFile))
                {
                    Logger.Error(string.Format("Can not open file: {0}", inputCsvFile));
                    return;
                }
                if (isExcelFile == false)
                {
                    lines = File.ReadAllLines(inputCsvFile);
                }
                else
                {
                
                    Excel.Application xlApp = new Excel.Application();
                    var inputExcelFile = Path.IsPathRooted(inputCsvFile) ? inputCsvFile : Path.Combine(AppContext.BaseDirectory, inputCsvFile);
                    Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(inputExcelFile);
                    Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                    Excel.Range xlRange = xlWorksheet.UsedRange;

                    int rowCount = xlRange.Rows.Count;
                    int colCount = xlRange.Columns.Count;
                    lines = new string[rowCount];                   
                    int a = 0;
                   
                    for (int i = 1; i <= rowCount; i++)
                    {
                        string myLine = "";
                        Excel.Range rngStart = (Excel.Range)xlWorksheet.Cells[i, 1];
                        Excel.Range rngFin = (Excel.Range)xlWorksheet.Cells[i, colCount];
                        Excel.Range rr = xlWorksheet.Range[rngStart, rngFin];

                        foreach (var Result in rr.Value2)
                        {
                            if (Result != null)
                            {
                                myLine += Result.ToString() + trans.csvSrcSeparator;
                            }
                            else
                                myLine += trans.csvSrcSeparator;
                        }

                        lines[a] = myLine.Substring(0, myLine.Length - 1);
                        a++;
                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Marshal.ReleaseComObject(xlRange);
                    Marshal.ReleaseComObject(xlWorksheet);
                    xlWorkbook.Close();
                    Marshal.ReleaseComObject(xlWorkbook);
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);
                }
            }
            else
            {
                Logger.Debug("InputCsv file does not exist. Finding in resource...");
                var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + inputCsvFile));
                if (resourceName == null) throw new ArgumentException(string.Format("InputCsv file not found in resource: {0}", inputCsvFile));
                lines = Utils.ReadLines(() => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName), Encoding.UTF8).ToArray();
            }

            var GlobalVariables = new Dictionary<string, string>() { { "CsvSrcSep", trans.csvSrcSeparator.ToString() }, { "InputFile", inputCsvFile } };
            if (trans.variables != null)
            {
                foreach (var Var in trans.variables.Where(x => !string.IsNullOrEmpty(x.expressionStorage)).ToList())
                {
                    string Val = ConfigurationManager.AppSettings[Var.name];
                    if (Val == null) Val = string.Empty;
                    Logger.Debug($"Getting global variable value from setting: {Var.name}={Val}");
                    GlobalVariables.Add(Var.name, Val);
                }
            }
            HashSet<string>[] uniqueKeys = null;

            string[] csvSrcHeaders = lines[0].Split(trans.csvSrcSeparator);
            lines = lines.Skip(trans.csvSkipLines).ToArray();
            if (trans.rowCloning != null)
            {
                Logger.Debug($"Cloning rows Length={lines.Length}");
                var clonedLines = new List<string>();
                foreach(var line in lines) {
                    for(int i=0; i<trans.rowCloning.Ntimes; i++)
                    {
                        var expression = trans.rowCloning.Expression.Replace("$cloneIdx", i.ToString());
                        var clonedLine = Helper.evaluateExpression($"cloning {i}", expression, null, string.Empty, line, trans.ExtraEvalCode);
                        clonedLines.Add(clonedLine);
                    }
                }
                lines = clonedLines.ToArray();
                Logger.Debug($"After cloning rows Length={lines.Length}");
            }
            var outputLines = new List<string[]>();
            string outputCsvFileTemp = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            Logger.Debug($"saving to temporary file: {outputCsvFileTemp}");
            var failureLines = new List<string[]>();
            var unprocessedLines = new List<string[]>();
            using (var swOut = new StreamWriter(outputCsvFileTemp))
            {
                swOut.WriteLine(headerLine);
                //using (var srIn = new StreamReader(inputCsvFile))
                //{
                    //foreach(var inputLine in lines)
                    for(int i=0; i<lines.Length; i++)
                    {
                        var inputLine = lines[i];
                    try
                    {
                        if (string.IsNullOrWhiteSpace(inputLine)) continue;
                        var doubleQuotesProcessed = false;
                        if (trans.csvSrcSeparator != '\0')
                        {
                            //var csvVals = inputLine.Split(trans.csvSrcSeparator);
                            //var csvVals = Regex.Matches(inputLine, @"[\""].+?[\""]|[^" + trans.csvSrcSeparator + "]+").Cast<Match>().Select(m => m.Value).ToArray();
                            var csvVals = Regex.Matches(inputLine, $"(?:^|{trans.csvSrcSeparator})(\"(?:[^\"]+|\"\")*\"|[^{trans.csvSrcSeparator}]*)").Cast<Match>().Select(m => m.Value.TrimStart(trans.csvSrcSeparator)).ToArray();

                            //TODO: Here we remove double quotes line by line. But we may need to do this for all lines as lines are used in computeLookup)
                            bool preProcessed = false;
                            for (int j = 0; j < csvVals.Length; j++)
                            {
                                if (csvVals[j].StartsWith("\"") && csvVals[j].EndsWith("\""))
                                {
                                csvVals[j] = csvVals[j].Substring(1, csvVals[j].Length - 2);
                                preProcessed = true;
                                }
                            }
                            if (preProcessed)
                            {
                                string newInputLine = string.Join(DEFAULT_CSV_SEPARATOR.ToString(), csvVals);
                                if (inputLine.StartsWith(trans.csvSrcSeparator.ToString()))
                                {
                                    Logger.Warn($"inputLine start with empty column, so add new empty column in newInputLine");
                                    newInputLine = DEFAULT_CSV_SEPARATOR.ToString() + newInputLine;
                                }
                                doubleQuotesProcessed = true;
                                Logger.Debug($"inputLine: {inputLine}");
                                Logger.Debug($"newInputLine: {newInputLine}");
                                inputLine = newInputLine;
                            }
                        }

                        Logger.Debug(string.Format("Processing line {0}/{1}: {2}", i+1, lines.Length, inputLine));
                        var Variables = new Dictionary<string, string>(GlobalVariables);

                        //evaluate variables
                        if (trans.variables != null) {
                            foreach(var Var in trans.variables) {
                                var variableInfo = string.Format("Variable={0}", Var.name);
                                Logger.Debug(variableInfo);
                                var Val = Variables.ContainsKey(Var.name) ? Variables[Var.name] : string.Empty;
                                Val = Helper.evaluateExpression(variableInfo, Var.expressionBefore, Variables, Val, inputLine, trans.ExtraEvalCode);
                                //if (Var.Lookup != null) {
                                //    var key = string.Format("colVal={0}, Expression={1}, Depth={2}", Val, Var.Lookup.Expression, Var.Lookup.Depth);
                                //    if (!CacheLookupValuesLines.ContainsKey(key))
                                //    {
                                        Val = Helper.computeLookup(variableInfo, Val, lines, Variables, Var.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine, trans.ExtraEvalCode);
                                //        CacheLookupValuesLines[key] = Val;
                                //    }
                                //    else
                                //    {
                                //        Val = CacheLookupValuesLines[key];
                                //        Logger.Debug(string.Format("Lookup return value from cache={0}, key={1}", Val, key));
                                //    }
                                //}
                                Val = Helper.evaluateExpression(variableInfo, Var.expressionAfter, Variables, Val, inputLine, trans.ExtraEvalCode);
                                Variables[Var.name] = Val;
                                if (GlobalVariables.ContainsKey(Var.name)) GlobalVariables[Var.name] = Val;
                                if (!string.IsNullOrEmpty(Var.expressionStorage) && Var.expressionStorage != "NoStorage" && i==lines.Length - 1)
                                {
                                    var storeVal = Helper.evaluateExpression(variableInfo, Var.expressionStorage, Variables, string.Empty, string.Empty, trans.ExtraEvalCode);
                                    Utils.AddOrUpdateAppSettings(Var.name, storeVal);
                                }
                            }
                        }

                        //checking constraints
                        if (trans.uniqueConstraints != null && trans.uniqueConstraints.Length > 0)
                        {
                            if (uniqueKeys == null)
                            {
                                uniqueKeys = new HashSet<string>[trans.uniqueConstraints.Length];
                                for (int ic = 0; ic < uniqueKeys.Length; ic++) uniqueKeys[ic] = new HashSet<string>();
                            }
                            bool violated = false;
                            for (int ic = 0; ic < trans.uniqueConstraints.Length; ic++)
                            {
                                var keyVal = Helper.evaluateExpression($"uniqueConstraint_{ic + 1}", trans.uniqueConstraints[ic], Variables, string.Empty, inputLine, trans.ExtraEvalCode);
                                if (uniqueKeys[ic].Contains(keyVal))
                                {
                                    Logger.Error($"UNIQUE CONSTRAINT VIOLATED. Line {i + 1}. Key={keyVal}");
                                    violated = true;
                                    break;
                                }
                                else
                                {
                                    uniqueKeys[ic].Add(keyVal);
                                }
                            }
                            if (violated) continue;
                        }
                        if (trans.checkConstraints != null && trans.checkConstraints.Length > 0)
                        {
                            bool violated = false;
                            for(int ic=0; ic<trans.checkConstraints.Length; ic++)
                            {
                                var result = Helper.evaluateExpression($"checkConstraint_{ic+1}", trans.checkConstraints[ic], Variables, string.Empty, inputLine, trans.ExtraEvalCode);
                                if (!bool.Parse(result))
                                {
                                    Logger.Error($"CHECK CONSTRAINT VIOLATED. Line {i + 1}. result={result}");
                                    failureLines.Add(new[] { inputLine, "Constraint Violated" });
                                    violated = true;
                                    break;
                                }
                            }
                            if (violated) continue;
                        }
                        var lineToProcess = true;
                        if (!string.IsNullOrEmpty(trans.processingCondition))
                        {
                            var result = Helper.evaluateExpression("processingCondition", trans.processingCondition, Variables, string.Empty, inputLine, trans.ExtraEvalCode);
                            Logger.Debug(string.Format("result={0}", result));
                            if (!bool.Parse(result))
                            {
                                if (transIO.EmailAttachedFile == TransAttachedFile.Unprocessed || transIO.EmailAttachedFile == TransAttachedFile.Both)
                                {
                                    unprocessedLines.Add(new[] { inputLine, "processingCondition returned false" });
                                    Logger.Debug($"Preparing to send email UnprocessedLines: {inputLine}");
                                }
                                continue;
                            }
                        }
                        string[] inputLineSplitted = null;
                        if (doubleQuotesProcessed) {
                            inputLineSplitted = inputLine.Split(DEFAULT_CSV_SEPARATOR);
                        } else if (trans.csvSrcSeparator != '\0') {
                            var CSVParser = new Regex(string.Format("{0}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", trans.csvSrcSeparator == '|' ? "\\|" : trans.csvSrcSeparator.ToString()));
                            inputLineSplitted = CSVParser.Split(inputLine); // separator placed within double quotes
                            //inputLineSplitted = inputLine.Split(trans.csvSrcSeparator);
                        }
                        string[] oneLine = new string[csvHeaders.Length];
                        int currentPos = 0;
                        foreach (var column in trans.columns)
                        {
                            try
                            {
                                Logger.Debug(string.Format("Column name={0}, currentPos={1}, len={2}", column.name, currentPos, column.len));
                                string originalVal;
                                if (trans.csvSrcSeparator != '\0')
                                {
                                    if (trans.UseHeaderColumnNames)
                                    {
                                        originalVal = GetCsvVal(csvSrcHeaders, inputLineSplitted, column.name);
                                    } else
                                    {
                                        originalVal = inputLineSplitted[currentPos];
                                        currentPos++;
                                    }
                                }
                                else
                                {
                                    originalVal = inputLine.Substring(currentPos, column.len).Trim();
                                    currentPos += column.len;
                                }

                                if (column.destPaths == null) continue;
                                foreach (var destCol in column.destPaths)
                                {
                                    if (string.IsNullOrEmpty(destCol.path)) continue;
                                    var destColInfo = string.Format("Column={0}", destCol.path);
                                    Logger.Debug(destColInfo);
                                    var Val = Helper.computeLookup(destColInfo, originalVal, lines, Variables, destCol.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine, trans.ExtraEvalCode);
                                    Val = Helper.evaluateExpression(destColInfo, destCol.expression, Variables, Val, inputLine, trans.ExtraEvalCode);

                                    if (!string.IsNullOrEmpty(destCol.processingCondition))
                                    {
                                        var result = Helper.evaluateExpression("processingCondition", destCol.processingCondition, Variables, Val, inputLine, trans.ExtraEvalCode);
                                        lineToProcess = bool.Parse(result);
                                        if (!lineToProcess)
                                        {
                                            Logger.Debug(string.Format("Stop processing the line because of the condition: {0}", destCol.processingCondition));
                                            break;
                                        }
                                    }
                                    foreach(var colPath in destCol.path.Split(','))
                                    {
                                    Logger.Debug($"Find and set Column={colPath}, Val={Val}");
                                    int idx = Enumerable.Range(0, csvHeaders.Length)
                                        .Where(x => colPath.Equals(csvHeaders[x])).FirstOrDefault();
                                    if (csvHeaders[idx] == colPath)
                                    {
                                        oneLine[idx] = Val;
                                    }
                                    else
                                    {
                                        throw new Exception(string.Format("Column '{0}' is mapped to an non-existent column '{1}'", column.name, colPath));
                                    }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                failureLines.Add(new[] { inputLine, $"Column={column.name} currentPos={currentPos}. " + e.Message });
                                Logger.Error(e, $"Error when processing column. Column={column.name}, currentPos={currentPos}");
                            }
                            if (!lineToProcess) break;
                        }
                        if (!lineToProcess)
                        {
                            Logger.Debug("Skip this line. Process the next one.");
                            continue;
                        }
                        outputLines.Add(oneLine);
                        Logger.Debug(string.Format("After tranformation {0}/{1}: {2}", i+1, lines.Length, string.Join(trans.csvDestSeparator.ToString(), oneLine)));
                    } catch(Exception e)
                    {
                        failureLines.Add(new[] { inputLine, e.Message });
                        Logger.Error(e, $"Error while processing line: {inputLine}");
                    }
                    }
                //}
                if (failureLines.Any())
                {
                    Logger.Debug($"writing failure file: {failureLines.Count} lines");
                    using (var swFailure = new StreamWriter(failureFile))
                    {
                        swFailure.WriteLine($"{headerLine};Error");
                        foreach (var fl in failureLines) swFailure.WriteLine($"{fl[0]};{fl[1]}");
                    }
                    if (transIO.SendFailureReport)
                    {
                        Logger.Debug($"Sending Failure Report email: TransName={transName}, File={failureFile}, lines={failureLines.Count}");
                        string errEmailubject = ConfigurationManager.AppSettings["ErrEmailSubject"];
                        errEmailubject = errEmailubject.Replace("$transName", transName);
                        Utils.SendErrorEmail(errEmailubject, failureFile);
                    }
                }
                Compiler.CleanUp();
                if (trans.groupBy == null)
                {
                    foreach (var oneLine in outputLines)
                    {
                        swOut.WriteLine(string.Join(trans.csvDestSeparator.ToString(), oneLine));
                    }
                }
                else
                {
                    var groupByOutputLines = new Dictionary<string, ArrayList>();
                    foreach (var oneLine in outputLines)
                    {
                        var key = string.Empty;
                        for (int i = 0; i < csvHeaders.Length; i++)
                        {
                            if (trans.groupBy.Contains(csvHeaders[i]))
                            {
                                key += (oneLine[i] + trans.csvDestSeparator.ToString());
                            }
                        }
                        if (!groupByOutputLines.ContainsKey(key))
                        {
                            var outputLine = new ArrayList { 1, oneLine };
                            groupByOutputLines.Add(key, outputLine);
                        }
                        else
                        {
                            var count = (int)groupByOutputLines[key][0];
                            groupByOutputLines[key][0] = count + 1;
                            var aggregationLine = (string[])groupByOutputLines[key][1];
                            for (int i = 0; i < csvHeaders.Length; i++)
                            {
                                if (trans.columns.Any(x => x.destPaths != null && x.destPaths.Any(y => y.path == csvHeaders[i] && !string.IsNullOrEmpty(y.aggregation)))) {
                                    var sum = double.Parse(aggregationLine[i]);
                                    sum += double.Parse(oneLine[i]);
                                    aggregationLine[i] = sum.ToString();
                                }
                            }
                        }
                    }

                    foreach(var lst in groupByOutputLines.Values)
                    {
                        var count = (int)lst[0];
                        var aggregationLine = (string[])lst[1];
                        for (int i = 0; i < csvHeaders.Length; i++)
                        {
                            trans.columns.Where(x => x.destPaths != null && x.destPaths.Any(y => y.path == csvHeaders[i] && !string.IsNullOrEmpty(y.aggregation))).ToList().ForEach(x => {
                                var destPath = x.destPaths.Single(y => y.path == csvHeaders[i] && !string.IsNullOrEmpty(y.aggregation));
                                var sum = double.Parse(aggregationLine[i]);
                                if (destPath.aggregation == "AVG")
                                {
                                    var avg = sum / count;
                                    aggregationLine[i] = avg.ToString();
                                }
                                else if (destPath.aggregation == "SUM")
                                {
                                    //already done
                                }
                                else if (destPath.aggregation == "COUNT")
                                {
                                    aggregationLine[i] = count.ToString();
                                }
                                else
                                {
                                    Logger.Error(string.Format("Aggregation not supported: {0}", destPath.aggregation));
                                }
                            });
                        }
                        swOut.WriteLine(string.Join(trans.csvDestSeparator.ToString(), aggregationLine));
                    }
                }
            }

            if (outputLines.Count()>0 || !trans.ClearEmptyOutput)
            {
                Logger.Debug($"Moving file: {Path.GetFileName(outputCsvFileTemp)} to {outputCsvFile}");
                File.Move(outputCsvFileTemp, outputCsvFile);

                if (!string.IsNullOrEmpty(transIO.EmailRecipientTo) && !string.IsNullOrEmpty(transIO.EmailSubject)
                    && !string.IsNullOrEmpty(transIO.EmailBody)
                    && (transIO.EmailAttachedFile==null || transIO.EmailAttachedFile == TransAttachedFile.Output || transIO.EmailAttachedFile == TransAttachedFile.Both))
                {
                    Logger.Debug($"Sending email to {transIO.EmailRecipientTo} ({outputCsvFile})");
                    try
                    {
                        Utils.SendEmail(transIO.EmailRecipientTo, transIO.EmailSubject, transIO.EmailBody, transIO.EmailRecipientCC, new[] { outputCsvFile });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Error when sending Output file email");
                    }
                }

                Utils.RunCommandLineWithOutputFile(transIO.PostTransCommandLine, transIO.PostTransCommandLineArgs, outputCsvFile);
            }
            else
            {
                Logger.Debug("Empty output csv file is ignored");
            }
            if (unprocessedLines.Count() > 0)
            {
                string unprocessedLinesCsvFileTemp = Path.GetTempPath() + transIO.Name + "_UnprocessedLines_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                Logger.Debug($"saving unprocessedLines {unprocessedLines.Count} lines to: {unprocessedLinesCsvFileTemp}");
                using (var swUnprocessed = new StreamWriter(unprocessedLinesCsvFileTemp))
                {
                    string csvSrcHeaderLine = string.Join(";", csvSrcHeaders);
                    swUnprocessed.WriteLine($"{csvSrcHeaderLine};Error");
                    foreach (var fl in unprocessedLines) swUnprocessed.WriteLine($"{fl[0]};{fl[1]}");
                }
                Logger.Debug($"Sending email to {transIO.EmailRecipientTo} ({unprocessedLinesCsvFileTemp})");
                try
                {
                    Utils.SendEmail(transIO.EmailRecipientTo, transIO.EmailSubject, transIO.EmailBody, transIO.EmailRecipientCC, new[] { unprocessedLinesCsvFileTemp });
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error when sending unprocessedLines email");
                }
            }
            Logger.Debug($"Processed {outputLines.Count}/{lines.Length} lines");
            Utils.AuditEnd(auditId, lines.Length, outputLines.Count, failureLines.Count);
            Logger.Debug("END");
        }

        private static void Transform2Xml(TransformationIO transIO, PdtTransformationSetting Setting, string transName, string inputCsvFile, string outputXmlFile, string failureFile)
        {
            PdtTransformation trans = Setting.Transformations.Where(x => x.name == transName).FirstOrDefault();
            if (trans == null) throw new ArgumentException(string.Format("Transformation name not found: {0}", transName));

            Logger.Debug($"BEGIN(name={trans.name}, inputCsvFile={inputCsvFile}, outputXmlFile={outputXmlFile}, failureFile={failureFile}, templateFile ={trans.templateFile})");
            int auditId = Utils.AuditStart(transName, trans.type.ToString(), inputCsvFile, outputXmlFile, failureFile);

            //Build DOM
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true; // to preserve the same formatting

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, trans.templateFile)))
            {
                doc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, trans.templateFile));
            }
            else
            {
                var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + trans.templateFile));
                if (resourceName == null) throw new ArgumentException(string.Format("Template file not found in resource: {0}", trans.templateFile));
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                doc.Load(stream);
            }

            string[] lines;
            if (File.Exists(inputCsvFile))
            {
                if (!Utils.IsFileClosed(inputCsvFile))
                {
                    Logger.Error(string.Format("Can not open file: {0}", inputCsvFile));
                    return;
                }
                lines = File.ReadAllLines(inputCsvFile);
            }
            else
            {
                Logger.Debug("InputCsv file does not exist. Finding in resource...");
                var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + inputCsvFile));
                if (resourceName == null) throw new ArgumentException(string.Format("InputCsv file not found in resource: {0}", inputCsvFile));
                lines = Utils.ReadLines(() => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName), Encoding.UTF8).ToArray();
            }

            string[] csvHeaders;
            if (trans.UseHeaderColumnNames) {
                csvHeaders = lines[0].Split(trans.csvSrcSeparator);
            } else {
                csvHeaders = trans.columns.Select(x => x.name).ToArray();
            }
            var failureLines = new List<string[]>();
            var unprocessedLines = new List<string[]>();
            var GlobalVariables = new Dictionary<string, string>() { { "CsvSrcSep", trans.csvSrcSeparator.ToString() }, { "InputFile", inputCsvFile } };
            if (trans.variables != null)
            {
                foreach (var Var in trans.variables.Where(x => !string.IsNullOrEmpty(x.expressionStorage)).ToList())
                {
                    string Val = ConfigurationManager.AppSettings[Var.name];
                    if (Val == null) Val = string.Empty;
                    Logger.Debug($"Getting global variable value from setting: {Var.name}={Val}");
                    GlobalVariables.Add(Var.name, Val);
                }
            }
            HashSet<string>[] uniqueKeys = null;

            lines = lines.Skip(trans.csvSkipLines).ToArray();
            int numOfProcessedLines = 0;

            //transformation for the absolute columns whose values are the same for all lines
            PdtColumn[] arrAbsoluteColumns = trans.columns.Where(x => !x.isRelativeToRootNode).ToArray();

            //transformation for the relative columns whose values are different between lines
            string repeatingRootPath = trans.repeatingRootPath;
            string repeatingChildrenPath = trans.repeatingChildrenPath;
            var docClone = (XmlDocument)doc.Clone();
            bool useRepeatingNodes = (repeatingRootPath != null && repeatingChildrenPath != null);
            XmlNode repeatingRootNode = null;
            XmlNodeList repeatingChildrenNodes = null;
            XmlNode templateNode = null;
            if (useRepeatingNodes)
            {
                repeatingRootNode = docClone.SelectSingleNode(repeatingRootPath); // un seul noeud
                repeatingChildrenNodes = docClone.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
                templateNode = repeatingChildrenNodes.Item(0);
            }

            string filePrimaryKeyVal = null;
            Dictionary<string, string> lastLineVariables = null;
            int bunchIndexPrimaryKey = 0;
            {
                //get list of columns whose values need to be filled in root node
                PdtColumn[] arrRelativeColumns = trans.columns.Where(x => x.isRelativeToRootNode && x.destPaths != null).ToArray();
                List<XmlNode> transformedNodes = new List<XmlNode>();
                for (int i = 0; i < lines.Length; i++)
                {
                    var inputLine = lines[i];
                    try
                    {
                        if (string.IsNullOrWhiteSpace(inputLine)) continue;
                        var doubleQuotesProcessed = false;
                        if (trans.csvSrcSeparator != '\0')
                        {
                            var csvVals = Regex.Matches(inputLine, $"(?:^|{trans.csvSrcSeparator})(\"(?:[^\"]+|\"\")*\"|[^{trans.csvSrcSeparator}]*)").Cast<Match>().Select(m => m.Value.TrimStart(trans.csvSrcSeparator)).ToArray();

                            //TODO: Here we remove double quotes line by line. But we may need to do this for all lines as lines are used in computeLookup)
                            bool preProcessed = false;
                            for (int j = 0; j < csvVals.Length; j++)
                            {
                                if (csvVals[j].StartsWith("\"") && csvVals[j].EndsWith("\""))
                                {
                                    csvVals[j] = csvVals[j].Substring(1, csvVals[j].Length - 2);
                                    preProcessed = true;
                                }
                            }
                            if (preProcessed)
                            {
                                string newInputLine = string.Join(DEFAULT_CSV_SEPARATOR.ToString(), csvVals);
                                if (inputLine.StartsWith(trans.csvSrcSeparator.ToString()))
                                {
                                    Logger.Warn($"inputLine start with empty column, so add new empty column in newInputLine");
                                    newInputLine = DEFAULT_CSV_SEPARATOR.ToString() + newInputLine;
                                }
                                doubleQuotesProcessed = true;
                                Logger.Debug($"inputLine: {inputLine}");
                                Logger.Debug($"newInputLine: {newInputLine}");
                                inputLine = newInputLine;
                            }
                        }

                        Logger.Debug(string.Format("Processing line {0}/{1}: {2}", i + 1, lines.Length, inputLine));

                        var Variables = new Dictionary<string, string>(GlobalVariables);

                        //checking constraints
                        if (trans.uniqueConstraints != null && trans.uniqueConstraints.Length > 0)
                        {
                            if (uniqueKeys == null)
                            {
                                uniqueKeys = new HashSet<string>[trans.uniqueConstraints.Length];
                                for (int ic = 0; ic < uniqueKeys.Length; ic++) uniqueKeys[ic] = new HashSet<string>();
                            }
                            bool violated = false;
                            for (int ic = 0; ic < trans.uniqueConstraints.Length; ic++)
                            {
                                var keyVal = Helper.evaluateExpression($"uniqueConstraint_{ic + 1}", trans.uniqueConstraints[ic], Variables, string.Empty, inputLine, trans.ExtraEvalCode);
                                if (uniqueKeys[ic].Contains(keyVal))
                                {
                                    Logger.Error($"UNIQUE CONSTRAINT VIOLATED. Line {i + 1}. Key={keyVal}");
                                    violated = true;
                                    break;
                                }
                                else
                                {
                                    uniqueKeys[ic].Add(keyVal);
                                }
                            }
                            if (violated) continue;
                        }
                        if (trans.checkConstraints != null && trans.checkConstraints.Length > 0)
                        {
                            bool violated = false;
                            for (int ic = 0; ic < trans.checkConstraints.Length; ic++)
                            {
                                var result = Helper.evaluateExpression($"checkConstraint_{ic + 1}", trans.checkConstraints[ic], Variables, string.Empty, inputLine, trans.ExtraEvalCode);
                                if (!bool.Parse(result))
                                {
                                    Logger.Error($"CHECK CONSTRAINT VIOLATED. Line {i + 1}. result={result}");
                                    failureLines.Add(new[] { inputLine, "Constraint Violated" });
                                    violated = true;
                                    break;
                                }
                            }
                            if (violated) continue;
                        }

                        var lineToProcess = true;
                        if (trans.variables != null)
                        {
                            foreach (var Var in trans.variables)
                            {
                                var variableInfo = string.Format("Variable={0}", Var.name);
                                Logger.Debug(variableInfo);
                                var Val = Variables.ContainsKey(Var.name) ? Variables[Var.name] : string.Empty;
                                Val = Helper.evaluateExpression(variableInfo, Var.expressionBefore, Variables, Val, inputLine, trans.ExtraEvalCode);
                                //if (Var.Lookup != null) {
                                //    var key = string.Format("colVal={0}, Expression={1}, Depth={2}", Val, Var.Lookup.Expression, Var.Lookup.Depth);
                                //    if (!CacheLookupValuesLines.ContainsKey(key))
                                //    {
                                Val = Helper.computeLookup(variableInfo, Val, lines, Variables, Var.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine, trans.ExtraEvalCode);
                                //        CacheLookupValuesLines[key] = Val;
                                //    }
                                //    else
                                //    {
                                //        Val = CacheLookupValuesLines[key];
                                //        Logger.Debug(string.Format("Lookup return value from cache={0}, key={1}", Val, key));
                                //    }
                                //}
                                Val = Helper.evaluateExpression(variableInfo, Var.expressionAfter, Variables, Val, inputLine, trans.ExtraEvalCode);
                                Variables[Var.name] = Val;
                                if (GlobalVariables.ContainsKey(Var.name)) GlobalVariables[Var.name] = Val;
                                if (!string.IsNullOrEmpty(Var.expressionStorage) && Var.expressionStorage != "NoStorage" && i == lines.Length - 1)
                                {
                                    var storeVal = Helper.evaluateExpression(variableInfo, Var.expressionStorage, Variables, string.Empty, string.Empty, trans.ExtraEvalCode);
                                    Utils.AddOrUpdateAppSettings(Var.name, storeVal);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(trans.processingCondition))
                        {
                            var result = Helper.evaluateExpression("processingCondition", trans.processingCondition, Variables, string.Empty, inputLine, trans.ExtraEvalCode);
                            Logger.Debug(string.Format("result={0}", result));
                            if (!bool.Parse(result))
                            {
                                if (transIO.EmailAttachedFile == TransAttachedFile.Unprocessed || transIO.EmailAttachedFile == TransAttachedFile.Both)
                                {
                                    unprocessedLines.Add(new[] { inputLine, "processingCondition returned false" });
                                    Logger.Debug($"Preparing to send email UnprocessedLines: {inputLine}");
                                }
                                continue;
                            }
                        }
                        numOfProcessedLines++;

                        string[] inputLineSplitted = null;
                        if (doubleQuotesProcessed)
                        {
                            inputLineSplitted = inputLine.Split(DEFAULT_CSV_SEPARATOR);
                        }
                        else if (trans.csvSrcSeparator != '\0')
                        {
                            var CSVParser = new Regex(string.Format("{0}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", trans.csvSrcSeparator == '|' ? "\\|" : trans.csvSrcSeparator.ToString()));
                            inputLineSplitted = CSVParser.Split(inputLine); // separator placed within double quotes
                                                                            //inputLineSplitted = inputLine.Split(trans.csvSrcSeparator);
                        }

                        var createNewFile = filePrimaryKeyVal == null;
                        var newFilePrimaryKeyVal = Helper.evaluateExpression("newFileBreakExpression", trans.fileBreakExpression, Variables, string.Empty, inputLine, trans.ExtraEvalCode);
                        createNewFile = createNewFile || newFilePrimaryKeyVal != filePrimaryKeyVal;
                        if (createNewFile)
                        {
                            if (string.IsNullOrEmpty(filePrimaryKeyVal))
                            {
                                for (int j = 1; j < repeatingChildrenNodes?.Count; j++)
                                {
                                    repeatingRootNode?.RemoveChild(repeatingChildrenNodes.Item(j));
                                }
                            }
                            else
                            {//new primary key found --> break into new file. save current file.
                                repeatingRootNode?.RemoveChild(templateNode);
                                for (int j = 0; j < transformedNodes.Count; j++)
                                {
                                    repeatingRootNode?.AppendChild(transformedNodes[j]);
                                }
                                transformedNodes.Clear();
                                //postProcessEvent
                                if (!string.IsNullOrEmpty(trans.postProcessEvent))
                                {
                                    Compiler.EvaluateDocEvent(trans.postProcessEvent, lastLineVariables, docClone);
                                }
                                var newFileBreakPath = Path.Combine(Path.GetDirectoryName(outputXmlFile),
                                    Path.GetFileNameWithoutExtension(outputXmlFile) + $"_{filePrimaryKeyVal}" + "_" + string.Format("{0:0000}", ++bunchIndexPrimaryKey) + Path.GetExtension(outputXmlFile));
                                Logger.Debug($"newFileBreak occurs. Saving to new file: {newFileBreakPath}");
                                docClone.Save(newFileBreakPath);
                                Utils.RunCommandLineWithOutputFile(transIO.PostTransCommandLine, transIO.PostTransCommandLineArgs, newFileBreakPath);
                                repeatingRootNode?.RemoveAll();
                                repeatingRootNode?.AppendChild(templateNode);
                                //create new one
                                docClone = (XmlDocument)doc.Clone();
                                if (useRepeatingNodes)
                                {
                                    repeatingRootNode = docClone.SelectSingleNode(repeatingRootPath); // un seul noeud
                                    repeatingChildrenNodes = docClone.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
                                    templateNode = repeatingChildrenNodes.Item(0);
                                }
                            }
                            filePrimaryKeyVal = newFilePrimaryKeyVal;
                            foreach (PdtColumn col in arrAbsoluteColumns)
                            {
                                if (col.destPaths == null) continue;
                                var destColInfo = $"Absolute Column={col.name}";
                                Logger.Debug(destColInfo);
                                string originalVal = GetCsvVal(csvHeaders, inputLineSplitted, col.name);
                                foreach (var destCol in col.destPaths)
                                {
                                    Logger.Debug($"destPaths: path={destCol.path}, expression={destCol.expression}");

                                    var Val = Helper.computeLookup(destColInfo, originalVal, lines, Variables, destCol.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine, trans.ExtraEvalCode);
                                    Val = Helper.evaluateExpression(destColInfo, destCol.expression, Variables, Val, inputLine, trans.ExtraEvalCode);

                                    XmlNodeList nodeList = docClone.SelectNodes(destCol.path);
                                    for (int j = 0; j < nodeList.Count; j++)
                                    {
                                        var nodeDest = nodeList.Item(j);
                                        Logger.Debug(string.Format("src={0}, dest={1}, textContent={2}", Val, nodeDest.Value, nodeDest.InnerText));
                                        nodeDest.InnerText = Val;
                                        Logger.Debug(string.Format("new value dest={0}, textContent={1}", nodeDest.Value, nodeDest.InnerText));
                                    }
                                }
                            }
                        }

                        foreach (PdtColumn col in arrRelativeColumns)
                        {
                            if (col.destPaths == null) continue;
                            string originalVal = GetCsvVal(csvHeaders, inputLineSplitted, col.name);
                            foreach (var destCol in col.destPaths)
                            {
                                if (string.IsNullOrEmpty(destCol.path)) continue;
                                var destColInfo = string.Format("Column={0}", destCol.path);
                                Logger.Debug(destColInfo);
                                var Val = Helper.computeLookup(destColInfo, originalVal, lines, Variables, destCol.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine, trans.ExtraEvalCode);
                                Val = Helper.evaluateExpression(destColInfo, destCol.expression, Variables, Val, inputLine, trans.ExtraEvalCode);

                                if (!string.IsNullOrEmpty(destCol.processingCondition))
                                {
                                    var result = Helper.evaluateExpression("processingCondition", destCol.processingCondition, Variables, Val, inputLine, trans.ExtraEvalCode);
                                    lineToProcess = bool.Parse(result);
                                    if (!lineToProcess)
                                    {
                                        Logger.Debug(string.Format("Stop processing the line because of the condition: {0}", destCol.processingCondition));
                                        break;
                                    }
                                }
                                if (Val == null)
                                {
                                    Logger.Warn($"path={destCol.path} keeps current value as Val is null");
                                }
                                else
                                {
                                    foreach (XmlNode nodeDest in docClone.SelectNodes(destCol.path))
                                    {
                                        nodeDest.InnerText = Val;
                                        Logger.Debug($"path={destCol.path}, Val={Val}");
                                    }
                                }
                            }
                        }
                        if (useRepeatingNodes)
                        {
                            transformedNodes.Add(templateNode.Clone());
                        }
                        lastLineVariables = Variables;
                    }
                    catch (Exception e)
                    {
                        failureLines.Add(new[] { inputLine, e.Message });
                        Logger.Error(e, $"Error while processing line: {inputLine}");
                    }
                }

                if (trans.bunchSize > 0)
                {
                    repeatingRootNode?.RemoveChild(templateNode);
                    int bunchIndex = 0;
                    for (int i=0; i<transformedNodes.Count; i++)
                    {
                        repeatingRootNode?.AppendChild(transformedNodes[i]);
                        if ((i+1)%trans.bunchSize == 0)
                        {
                            //postProcessEvent
                            if (!string.IsNullOrEmpty(trans.postProcessEvent))
                            {
                                Compiler.EvaluateDocEvent(trans.postProcessEvent, lastLineVariables, docClone);
                            }
                            var bunchFile = Path.Combine(Path.GetDirectoryName(outputXmlFile),
                                Path.GetFileNameWithoutExtension(outputXmlFile) + "_" + string.Format("{0:0000}", ++bunchIndex) + Path.GetExtension(outputXmlFile));
                            Logger.Debug($"Bunch size reached. Saving to bunch file: {bunchFile}");
                            docClone.Save(bunchFile);
                            Utils.RunCommandLineWithOutputFile(transIO.PostTransCommandLine, transIO.PostTransCommandLineArgs, bunchFile);
                            repeatingRootNode?.RemoveAll();
                            //create new one
                            docClone = (XmlDocument)doc.Clone();
                            if (useRepeatingNodes)
                            {
                                repeatingRootNode = docClone.SelectSingleNode(repeatingRootPath); // un seul noeud
                                repeatingChildrenNodes = docClone.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
                                templateNode = repeatingChildrenNodes.Item(0);
                            }
                        }
                    }
                    if (repeatingRootNode.HasChildNodes)
                    {
                        //postProcessEvent
                        if (!string.IsNullOrEmpty(trans.postProcessEvent))
                        {
                            Compiler.EvaluateDocEvent(trans.postProcessEvent, lastLineVariables, docClone);
                        }
                        var bunchFile = Path.Combine(Path.GetDirectoryName(outputXmlFile),
                            Path.GetFileNameWithoutExtension(outputXmlFile) + "_" + string.Format("{0:0000}", ++bunchIndex) + Path.GetExtension(outputXmlFile));
                        Logger.Debug($"Saving the rest to bunch file: {bunchFile}");
                        docClone.Save(bunchFile);
                        Utils.RunCommandLineWithOutputFile(transIO.PostTransCommandLine, transIO.PostTransCommandLineArgs, bunchFile);
                        repeatingRootNode?.RemoveAll();
                        //create new one
                        docClone = (XmlDocument)doc.Clone();
                        if (useRepeatingNodes)
                        {
                            repeatingRootNode = docClone.SelectSingleNode(repeatingRootPath); // un seul noeud
                            repeatingChildrenNodes = docClone.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
                            templateNode = repeatingChildrenNodes.Item(0);
                        }
                    }
                    repeatingRootNode?.AppendChild(templateNode);
                }
                foreach (XmlNode node in transformedNodes)
                {
                    repeatingRootNode?.InsertBefore(node, templateNode);
                }
                repeatingRootNode?.RemoveChild(templateNode);
            }

            if (failureLines.Any())
            {
                Logger.Debug($"writing failure file: {failureLines.Count} lines");
                //swFailure.WriteLine(headerLine);
                using (var swFailure = new StreamWriter(failureFile))
                {
                    //swFailure.WriteLine($"{headerLine};Error");
                    foreach (var fl in failureLines) swFailure.WriteLine($"{fl[0]};{fl[1]}");
                }
                if (transIO.SendFailureReport)
                {
                    Logger.Debug($"Sending Failure Report email: TransName={transName}, File={failureFile}, lines={failureLines.Count}");
                    string errEmailubject = ConfigurationManager.AppSettings["ErrEmailSubject"];
                    errEmailubject = errEmailubject.Replace("$transName", transName);
                    Utils.SendErrorEmail(errEmailubject, failureFile);
                }
            }
            if (unprocessedLines.Count() > 0)
            {
                string unprocessedLinesCsvFileTemp = Path.GetTempPath() + transIO.Name + "_UnprocessedLines_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                Logger.Debug($"saving unprocessedLines {unprocessedLines.Count} lines to: {unprocessedLinesCsvFileTemp}");
                using (var swUnprocessed = new StreamWriter(unprocessedLinesCsvFileTemp))
                {
                    string csvSrcHeaderLine = string.Join(";", csvHeaders);
                    swUnprocessed.WriteLine($"{csvSrcHeaderLine};Error");
                    foreach (var fl in unprocessedLines) swUnprocessed.WriteLine($"{fl[0]};{fl[1]}");
                }
                Logger.Debug($"Sending email to {transIO.EmailRecipientTo} ({unprocessedLinesCsvFileTemp})");
                try
                {
                    Utils.SendEmail(transIO.EmailRecipientTo, transIO.EmailSubject, transIO.EmailBody, transIO.EmailRecipientCC, new[] { unprocessedLinesCsvFileTemp });
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error when sending unprocessedLines email");
                }
            }
            Compiler.CleanUp();
            if ((useRepeatingNodes && repeatingRootNode.HasChildNodes) || !trans.ClearEmptyOutput || numOfProcessedLines > 0)
            {
                //postProcessEvent
                if (!string.IsNullOrEmpty(trans.postProcessEvent))
                {
                    Compiler.EvaluateDocEvent(trans.postProcessEvent, lastLineVariables, docClone);
                }
                if (!string.IsNullOrEmpty(filePrimaryKeyVal))
                {
                    var newFileBreakPath = Path.Combine(Path.GetDirectoryName(outputXmlFile),
                        Path.GetFileNameWithoutExtension(outputXmlFile) + $"_{filePrimaryKeyVal}" + "_" + string.Format("{0:0000}", ++bunchIndexPrimaryKey) + Path.GetExtension(outputXmlFile));
                    Logger.Debug($"Saving all to new file: {newFileBreakPath}");
                    docClone.Save(newFileBreakPath);
                    Utils.RunCommandLineWithOutputFile(transIO.PostTransCommandLine, transIO.PostTransCommandLineArgs, newFileBreakPath);
                    repeatingRootNode?.RemoveAll();
                    repeatingRootNode?.AppendChild(templateNode);
                }
                else
                {
                    // save the output file
                    Logger.Debug($"Saving to file: {outputXmlFile}");
                    docClone.Save(outputXmlFile);
                    Utils.RunCommandLineWithOutputFile(transIO.PostTransCommandLine, transIO.PostTransCommandLineArgs, outputXmlFile);
                }
            }
            else Logger.Debug("Empty output XML file is ignored");
            Logger.Debug($"Processed {numOfProcessedLines}/{lines.Length} lines");
            Utils.AuditEnd(auditId, lines.Length, numOfProcessedLines, failureLines.Count);
            Logger.Debug("END");
        }

        private static void TransformXml2Csv(TransformationIO transIO, PdtTransformationSetting Setting, string transName, string inputXmlFile, string outputCsvFile)
        {
            PdtTransformation trans = Setting.Transformations.Where(x => x.name == transName).FirstOrDefault();
            if (trans == null) throw new ArgumentException(string.Format("Transformation name not found: {0}", transName));

            Logger.Debug($"BEGIN(name={trans.name}, inputXmlFile={inputXmlFile}, outputCsvFile={outputCsvFile}, templateFile ={trans.templateFile})");
            int auditId = Utils.AuditStart(transName, trans.type.ToString(), inputXmlFile, outputCsvFile);

            //Build DOM
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true; // to preserve the same formatting
            doc.Load(inputXmlFile);

            PdtColumn[] arrRelativeColumns = trans.columns.Where(x => x.isRelativeToRootNode).ToArray();
            var outputLines = new List<string>[arrRelativeColumns.Length];
            for(int i=0; i<arrRelativeColumns.Length; i++)
            {
                var col = arrRelativeColumns[i];
                outputLines[i] = new List<string>();
                outputLines[i].Add(col.name); //header

                if (col.destPaths == null) continue;
                var destCol = col.destPaths[0];
                if (string.IsNullOrEmpty(destCol.path)) continue;
                Logger.Debug($"Column={col.name}, path={destCol.path}");
                foreach (XmlNode nodeDest in doc.SelectNodes(destCol.path))
                {
                    outputLines[i].Add(nodeDest.InnerText);
                    Logger.Debug($"Val={nodeDest.InnerText}");
                }
                if (i > 0 && outputLines[i].Count < outputLines[0].Count)
                {
                    Logger.Warn($"Column {col.name} has {outputLines[i].Count} values, we are adding empty values to make {outputLines[0].Count} values");
                    for (int j=outputLines[i].Count; j<outputLines[0].Count; j++) outputLines[i].Add(string.Empty);
                }
            }
            if (outputLines[0].Count > 1)
            {
                Logger.Debug($"Saving to the file: {outputCsvFile}");
                using (var swOut = new StreamWriter(outputCsvFile))
                {
                    for (int i = 0; i < outputLines[0].Count; i++)
                    {
                        for (int j = 0; j < outputLines.Length; j++)
                        {
                            swOut.Write(outputLines[j][i]);
                            if (j != outputLines.Length - 1) swOut.Write(trans.csvDestSeparator); else swOut.WriteLine();
                        }
                    }
                }
                if (!string.IsNullOrEmpty(transIO.EmailRecipientTo) && !string.IsNullOrEmpty(transIO.EmailSubject)
                    && !string.IsNullOrEmpty(transIO.EmailBody) && (transIO.EmailAttachedFile == null || transIO.EmailAttachedFile == TransAttachedFile.Output || transIO.EmailAttachedFile == TransAttachedFile.Both))
                {
                    Logger.Debug($"Sending email to {transIO.EmailRecipientTo} ({outputCsvFile})");
                    try
                    {
                        Utils.SendEmail(transIO.EmailRecipientTo, transIO.EmailSubject, transIO.EmailBody, transIO.EmailRecipientCC, new[] { outputCsvFile });
                    } catch(Exception e)
                    {
                        Logger.Error(e, "Error when sending Output file email");
                    }
                }

                Utils.RunCommandLineWithOutputFile(transIO.PostTransCommandLine, transIO.PostTransCommandLineArgs, outputCsvFile);
            }
            else
            {
                Logger.Debug("No line transformed.");
            }

            Utils.AuditEnd(auditId, outputLines[0].Count, outputLines[0].Count);
            Logger.Debug("END");
        }
    }
}

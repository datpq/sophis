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
using Oracle.DataAccess.Client;
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
        MIL_Cash_RMA,
        REFI_CA,
        REFI_CA_SR,
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
        SWIFT_ACK_NACK_Mir//SEA
    }

    public static class Transformation
    {
        private const char DEFAULT_CSV_SEPARATOR = ';';
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static string GetCsvVal(string[] csvHeaders, string[] csvVals, string columnName)
        {
            int idx = Enumerable.Range(0, csvHeaders.Length)
                    .Where(i => columnName.Equals(csvHeaders[i])).FirstOrDefault();

            return csvVals[idx];
        }

        public static PdtTransformationSetting InitializeAndSaveConfig(string configFile)
        {
            Logger.Warn("InitializeAndSaveConfig.BEGIN");
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
                    new PdtLookupTable{
                        Name = "REFI_CA_TABLE_BY_ID",
                        File = "REFI_CA_LOOKUP*",
                        csvSeparator = ',',
                        processingCondition = @"lineVal.Split(';')[4] == ""CAP"" || (lineVal.Split(';')[4] == ""DIV"" && lineVal.Split(';')[8] != """")",
                        keyExpression = @"lineVal.Split(';')[4]+""_""+lineVal.Split(';')[41]",//Corporate Actions Type_Corporate Actions ID
                        columnsExpression = new[] {
                            "lineVal.Split(';')[18] == \"\" ? \"1\" : lineVal.Split(';')[18]",//CAP.numerator
                            "lineVal.Split(';')[17] == \"\" ? \"1\" : lineVal.Split(';')[17]",//CAP.denumerator
                            "System.DateTime.ParseExact(lineVal.Split(';')[14], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")",//CAP.Effective Date
                            "System.DateTime.ParseExact(lineVal.Split(';')[29], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")",//CAP.Subscription Period Start Date
                            "System.DateTime.ParseExact(lineVal.Split(';')[30], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")",//CAP.Subscription Period End Date
                            "lineVal.Split(';')[8] == \"\" ? \"1\" : lineVal.Split(';')[8]",//DIV.Dividend Rate
                        }
                    },
                    new PdtLookupTable{
                        Name = "REFI_CA_TABLE_BY_ISIN",
                        File = "REFI_CA_LOOKUP*",
                        csvSeparator = ',',
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
                    variables = new[] {
                        new PdtVariable {
                            name = "TransactionType",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[1]",
                        },
                        new PdtVariable {
                            name = "TradeType",
                            expressionBefore = "\"$TransactionType\" == \"Sell\" || \"$TransactionType\" == \"Buy\" || \"$TransactionType\" == \"ForwardFX\" ? \"Purchase/Sale\" : \"Cash Transfer\"",
                        },
                        new PdtVariable {
                            name = "UserTranID1",
                            expressionBefore = "lineVal.Split('$CsvSrcSep')[21]",
                        },
                    },
                    processingCondition = "\",Sell,Buy,Withdraw,Deposit,AccountingRelated,GrossAmountDividend,ForwardFX,Repo,ReverseRepo,\".IndexOf(\",$TransactionType,\") >= 0 && \",211,212,213,214,215,216,217,218,219,220,221,222MASTER,223,224,225,226,227,228,229,230,236,237,300MASTER,301MASTER,302MASTER,303MASTER,611,612,614,617,626,627,628,629,631,642,646,647,648,650,651,652,653,654,656,657,658,659,660,661,662,664,677,682,683,684,685,686,687,688,689,690,691,692,693,694,695,696,697,698,699,700,701,703,704,705,706,707,708,709,710,711,712,713,714,715,\".IndexOf(\",\" + lineVal.Split('$CsvSrcSep')[5] + \",\") >= 0 && !(\"$TransactionType\" == \"AccountingRelated\" && (\"$UserTranID1\".IndexOf(\"Accrual\") >= 0 || \"$UserTranID1\".IndexOf(\"Day\") >= 0 || \"$UserTranID1\".IndexOf(\"support\") >= 0)) && !(\"$TransactionType\" == \"GrossAmountDividend\" && \"$UserTranID1\".IndexOf(\"div_reinvestment\") >= 0)",
                    columns = new [] {
                        new PdtColumn {
                            name = "Tran ID",
                            isRequired = true,
                            isRelativeToRootNode = true,
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
                        new PdtColumn {
                            name = "Trans Type",
                            isRequired = true,
                            isRelativeToRootNode = true,
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
                        new PdtColumn {
                            name = "Trade Date",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "TradeDate",
                                    expression = "System.Text.RegularExpressions.Regex.IsMatch(colVal, @\"^\\d+$\") ? (new System.DateTime(1900, 1, 1).AddDays(int.Parse(colVal)-2)).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
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
                                    expression = "System.Text.RegularExpressions.Regex.IsMatch(colVal, @\"^\\d+$\") ? (new System.DateTime(1900, 1, 1).AddDays(int.Parse(colVal)-2)).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(colVal, \"dd/MM/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Actual Settle Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn {
                            name = "Portfolio",
                            isRequired = true,
                            isRelativeToRootNode = true,
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
                        new PdtColumn {
                            name = "Investment",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"""$TradeType"" == ""Cash Transfer"" ? ""SELECT SICOVAM FROM TITRES WHERE LIBELLE='Cash for currency ''"" + lineVal.Split('$CsvSrcSep')[11] + ""'''"" : ""$TransactionType"" == ""ForwardFX"" ? @""
SELECT SICOVAM FROM TITRES WHERE TYPE = 'E' AND QUOTATION_TYPE = 1
    AND ((MARCHE=STR_TO_DEVISE('"" + colVal.Substring(0, 3) + ""') AND DEVISECTT=STR_TO_DEVISE('"" + lineVal.Split('$CsvSrcSep')[11].Substring(0, 3) + ""')) OR (MARCHE=STR_TO_DEVISE('"" + lineVal.Split('$CsvSrcSep')[11].Substring(0, 3) + ""') AND DEVISECTT=STR_TO_DEVISE('"" + colVal.Substring(0, 3) + ""')))"" : @""
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
                        new PdtColumn {
                            name = "Quantity",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest {
                                path = "Quantity",
                                Lookup = new PdtColumnLookup {
                                    Table = "SQL",
                                    Expression = @"@""
SELECT CASE WHEN '$TransactionType' NOT IN ('Sell', 'Buy') THEN
        CASE WHEN '$TransactionType' IN ('ForwardFX') THEN CASE WHEN '"" + lineVal.Split('$CsvSrcSep')[11] + @""'='EUR.F' THEN -1 * "" + lineVal.Split('$CsvSrcSep')[9] + @"" ELSE TO_NUMBER(COALESCE('"" + colVal + @""', '0')) END
        ELSE (CASE WHEN '$TransactionType' IN ('Deposit', 'GrossAmountDividend', 'ReverseRepo') THEN -1 ELSE 1 END) * "" + lineVal.Split('$CsvSrcSep')[9] + @"" END
    ELSE (CASE WHEN '$TransactionType' IN ('Sell') THEN -1 ELSE 1 END) * "" + (colVal=="""" ? ""0"" : colVal) + @"" /
        CASE WHEN NOT EXISTS (
            SELECT * FROM EXTRNL_REFERENCES_DEFINITION ERD
                JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
                JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT 
                    AND T.AFFECTATION IN ('1900', '1660','1420','13','12','1204','1540','1560')
            WHERE ERD.REF_NAME = 'ISIN'
                AND ERI.VALUE = '"" + lineVal.Split('$CsvSrcSep')[7] + @""')
                AND NOT EXISTS (SELECT * FROM TITRES WHERE REFERENCE = '"" + lineVal.Split('$CsvSrcSep')[7] + @""'
                    AND AFFECTATION IN ('1900', '1660','1420','13','12','1204','1540','1560')) THEN 1 ELSE
        (SELECT NOMINAL FROM TITRES WHERE REFERENCE = '"" + lineVal.Split('$CsvSrcSep')[7] + @""'
        UNION
        SELECT T.NOMINAL FROM EXTRNL_REFERENCES_DEFINITION ERD
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
            JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT 
        WHERE ERD.REF_NAME = 'ISIN'
            AND ERI.VALUE = '"" + lineVal.Split('$CsvSrcSep')[7] + @""') END END QUANTITY
FROM DUAL"""
                                },
                                //expression = "\"$TransactionType\" == \"AccountingRelated\" ? lineVal.Split('$CsvSrcSep')[9] : ((\"$TransactionType\" == \"Sell\" ? \"-\" : \"\") + colVal)"
                            } }
                        },
                        new PdtColumn {
                            name = "Settle Net Amount",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Amount",
                                    expression = "\"$TransactionType\"==\"ForwardFX\" && lineVal.Split('$CsvSrcSep')[11]==\"EUR.F\" ? \"-\" + lineVal.Split('$CsvSrcSep')[8] : (\",Buy,ForwardFX,Withdraw,AccountingRelated,Repo,\".IndexOf(\",$TransactionType,\") >= 0 ? \"\" : \"-\") + colVal"
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Unit Price",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Spot",
                                    expression = "\"$TransactionType\" == \"Sell\" || \"$TransactionType\" == \"Buy\" || \"$TransactionType\" == \"ForwardFX\" ? double.Parse(colVal) : 1"
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
            AND ERI.VALUE = '"" + lineVal.Split('$CsvSrcSep')[7] + @""')
            AND NOT EXISTS (SELECT * FROM TITRES WHERE REFERENCE = '"" + lineVal.Split('$CsvSrcSep')[7] + @""'
                AND AFFECTATION IN ('1900', '1660','1420','13','12','1204','1540','1560')) THEN 0
    ELSE (SELECT NVL(QUOTATION_TYPE, 0) FROM TITRES WHERE REFERENCE = '"" + lineVal.Split('$CsvSrcSep')[7] + @""'
        UNION
        SELECT NVL(T.QUOTATION_TYPE, 0)
        FROM EXTRNL_REFERENCES_DEFINITION ERD
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.REF_IDENT = ERD.REF_IDENT
            JOIN TITRES T ON T.SICOVAM = ERI.SOPHIS_IDENT 
        WHERE ERD.REF_NAME = 'ISIN'
            AND ERI.VALUE = '"" + lineVal.Split('$CsvSrcSep')[7] + @""') END SPOT_TYPE
FROM DUAL"""
                                    },
                                }
                            }
                        },
                        new PdtColumn {
                            name = "Settle Currency",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "Currency",
                                    expression = "colVal.Substring(0, 3)"
                                }
                            }
                        },
                        new PdtColumn { name = "Book Amount", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "FX Rate", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn {
                            name = "Broker",
                            isRequired = true,
                            isRelativeToRootNode = true,
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
                        new PdtColumn {
                            name = "Custodian",
                            isRequired = true,
                            isRelativeToRootNode = true,
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
                    name = TransName.REFI_CA.ToString(),
                    type = TransType.Csv2Xml,
                    label = "Refinitiv Corporate Action",
                    templateFile = "REFI_CA.xml",
                    category = "Medio",
                    //fileBreakExpression = @"lineVal.Split(';')[0]+""_""+""$XmlType"".Replace("" "", """")",
                    fileBreakExpression = @"lineVal.Split(';')[0]",
                    repeatingRootPath = "//*[local-name() = 'corporateActionList']",
                    repeatingChildrenPath = "//*[local-name() = 'corporateAction']",
                    csvSkipLines = 1,
                    csvSrcSeparator = ',',
                    variables = new[] {
                        new PdtVariable { name = "CAType", expressionBefore = "lineVal.Split(';')[4]"},
                        new PdtVariable { name = "DivType", expressionBefore = "lineVal.Split(';')[24]"},
                        new PdtVariable { name = "CapType", expressionBefore = "lineVal.Split(';')[42]"},
                        new PdtVariable { name = "XmlType", expressionBefore = @"
""$CAType"" == ""DIV"" ? (
    ""$DivType"" == ""Cash Dividend"" && lineVal.Split(';')[32] == ""JPY"" && lineVal.Split(';')[48]!=""Forecast"" ? ""Standard Dividend Japanese"" :
    ""$DivType"" == ""Cash Dividend"" && lineVal.Split(';')[12] != """" && lineVal.Split(';')[12] != lineVal.Split(';')[32] ? ""Standard Dividend In Another Currency"" :
    ""$DivType"" == ""Cash Dividend"" && lineVal.Split(';')[32] != ""JPY"" && lineVal.Split(';')[43]==""Mandatory"" ? ""Standard Dividend"" :
    //""$DivType"" == ""Cash with Stock Alternative"" ? ""Standard Dividend with option"" :
    ""$DivType"" == ""Stock Dividend"" ? ""Standard Stock Dividend"" : ""Unknown"") :
    (""$CapType"" == ""Non-renounceable scrip issue in same stock"" ? ""Standard SCRIP"" :
    ""$CapType"" ==  ""Return of capital"" ? ""Standard Return of Capital"" :
    ""$CapType"" ==  ""Stock split"" ? ""Standard Stock Split"" :
    ""$CapType"" ==  ""Stock consolidation"" ? ""Standard Reverse Stock Split"" :
    ""$CapType"" ==  ""Capital Reduction (CRD)"" ? ""Standard Share Capital Consolidation"" :
    //""$CapType"" ==  ""Non-renounceable scrip issue in same stock"" ? ""Standard Bonus Right Issue"" :
    ""$CapType"" ==  ""Demerger (DEM)"" ? ""Standard Spin-Off"" :""Unkown"")"},
                        new PdtVariable {
                            name = "numerator",
                            expressionBefore = @"""CAP_""+lineVal.Split(';')[41]",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "0" },
                            expressionAfter=@"""@Standard SCRIP@Standard Stock Split@Standard Reverse Stock Split@Standard Share Capital Consolidation@"".IndexOf(""$XmlType"") >= 0 ? (lineVal.Split(';')[18]=="""" ? ""1"" : lineVal.Split(';')[18]) : (colVal=="""" ? ""1"" : colVal)",
                        },
                        new PdtVariable {
                            name = "denominator",
                            expressionBefore = @"""CAP_""+lineVal.Split(';')[41]",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "1" },
                            expressionAfter=@"""@Standard SCRIP@Standard Stock Split@Standard Reverse Stock Split@Standard Share Capital Consolidation@"".IndexOf(""$XmlType"") >= 0 ? (lineVal.Split(';')[17]=="""" ? ""1"" : lineVal.Split(';')[17]) : (colVal=="""" ? ""1"" : colVal)",
                        },
                        new PdtVariable {
                            name = "EffectiveDate",
                            expressionBefore = @"""CAP_""+lineVal.Split(';')[41]",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "2" },
                        },
                        new PdtVariable {
                            name = "SubscriptionPeriodStartDate",
                            expressionBefore = @"""CAP_""+lineVal.Split(';')[41]",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "3" },
                        },
                        new PdtVariable {
                            name = "SubscriptionPeriodEndDate",
                            expressionBefore = @"""CAP_""+lineVal.Split(';')[41]",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "4" },
                        },
                        new PdtVariable {
                            name = "DividendRate",
                            expressionBefore = @"""DIV_""+lineVal.Split(';')[41]",
                            Lookup = new PdtColumnLookup { Table = "REFI_CA_TABLE_BY_ID", ColumnIndex = "5" },
                        },
                    },
                    processingCondition = @"
((""@Standard Dividend@Standard Dividend Japanese@Standard Dividend with option@Standard Stock Dividend@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 && ""$CAType"" == ""DIV"") ||
(""@Standard SCRIP@Standard Return of Capital@Standard Stock Split@Standard Reverse Stock Split@Standard Renaming@Standard Share Capital Consolidation@Standard Bonus Right Issue@Standard Spin-Off@"".IndexOf(""@$XmlType@"") >= 0 && ""$CAType"" == ""CAP""))",
                    postProcessEvent = @"
        var nodePaths = new List<string>();
        if (""$XmlType"" == ""Standard Dividend"") {
            nodePaths.Add(""//*[local-name() = 'conversionRatio']"");
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
            nodePaths.Add(""//*[local-name() = 'currency']"");
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
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
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
        if (""$XmlType"" == ""Standard Stock Dividend"") {
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
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
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        if (""$XmlType"" == ""Standard SCRIP"") {
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
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
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        if (""$XmlType"" == ""Standard Return of Capital"") {
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
            nodePaths.Add(""//*[local-name() = 'businessEvent1']"");
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
        if (""$XmlType"" == ""Standard Stock Split"") {
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
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'exdivDate']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]"");
        }
        if (""$XmlType"" == ""Standard Spin-Off"") {
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
        if (""$XmlType"" == ""Standard Reverse Stock Split"") {
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'paymentDate']"");
            nodePaths.Add(""//*[local-name() = 'exdivDate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'coefficient']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'newNameOfCompany']"");
            if (""Name,ISIN,TICKER"".IndexOf(doc.SelectSingleNode(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'nameChange']"").InnerText) < 0) {
                nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'nameChange']"");
                nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'newNameOfCompany']"");
            }
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'businessEvent1']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'conversionRatio']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'roundingType']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'roundingOnClosingPosition']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
        }
        if (""$XmlType"" == ""Standard Share Capital Consolidation"") {
            nodePaths.Add(""//*[local-name() = 'resultDate']"");
            nodePaths.Add(""//*[local-name() = 'result']"");
            nodePaths.Add(""//*[local-name() = 'electionStartDate']"");
            nodePaths.Add(""//*[local-name() = 'electionEndDate']"");
            nodePaths.Add(""//*[local-name() = 'EffectiveDate']"");
            nodePaths.Add(""//*[local-name() = 'paymentDate']"");
            nodePaths.Add(""//*[local-name() = 'exdivDate']"");
            nodePaths.Add(""//*[local-name() = 'Exchange_Rate']"");
            nodePaths.Add(""//*[local-name() = 'cash']"");
            nodePaths.Add(""//*[local-name() = 'coefficient']"");
            nodePaths.Add(""//*[local-name() = 'currency']"");
            nodePaths.Add(""//*[local-name() = 'currencyRate']"");
            nodePaths.Add(""//*[local-name() = 'businessEvent2']"");
            nodePaths.Add(""//*[local-name() = 'nameChange']"");
            nodePaths.Add(""//*[local-name() = 'newNameOfCompany']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'remark']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'rfactor']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'businessEvent1']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'businessEvent1']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'diffusedCode']"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][5]"");
            nodePaths.Add(""//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][4]"");
        }
        if (""$XmlType"" == ""Standard Dividend with option"") {
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
                                    Lookup = new PdtColumnLookup {
                                        Table = "SQL",
                                        Expression = @"@""
SELECT T.SICOVAM
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND ERI_REFI.VALUE = '"" + lineVal.Split(';')[49] + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MONTANT != 0)"""
                                    },
                                    path = "//*[local-name() = 'identifier']/*[local-name() = 'sophis']"
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "REFI_CA_ISO.csv",
                                        Expression = @"lineVal.Split(';')[0].Trim('""')==""ISIN "" + colVal ? (
lineVal.Split(';')[2].Trim('""')==""CHAN//NAME"" ? ""Name"" :
lineVal.Split(';')[0]==lineVal.Split(';')[5] && lineVal.Split(';')[6].Trim('""').StartsWith(""/TS/"") && lineVal.Split(';')[7].Trim('""').StartsWith(""/TS/"") && lineVal.Split(';')[6]!=lineVal.Split(';')[7] ? ""TICKER"" :
lineVal.Split(';')[0]!=lineVal.Split(';')[5] && lineVal.Split(';')[0].Trim('""').StartsWith(""ISIN"") && lineVal.Split(';')[5].Trim('""').StartsWith(""ISIN"") && lineVal.Split(';')[6]==lineVal.Split(';')[7] ? ""ISIN"" : ""Unknown"") : null",
                                    },
                                    path = "//*[local-name() = 'nameChange']",
                                },
                                new PdtColumnDest {
                                    Lookup = new PdtColumnLookup {
                                        File = "REFI_CA_ISO.csv",
                                        Expression = @"lineVal.Split(';')[0].Trim('""')==""ISIN "" + colVal ? (
lineVal.Split(';')[2].Trim('""')==""CHAN//NAME"" ? lineVal.Split(';')[1].Trim('""').Substring(6) :
lineVal.Split(';')[0]==lineVal.Split(';')[5] && lineVal.Split(';')[6].Trim('""').StartsWith(""/TS/"") && lineVal.Split(';')[7].Trim('""').StartsWith(""/TS/"") && lineVal.Split(';')[6]!=lineVal.Split(';')[7] ? lineVal.Split(';')[7].Trim('""').Split('/')[2] :
lineVal.Split(';')[0]!=lineVal.Split(';')[5] && lineVal.Split(';')[0].Trim('""').StartsWith(""ISIN"") && lineVal.Split(';')[5].Trim('""').StartsWith(""ISIN"") && lineVal.Split(';')[6]==lineVal.Split(';')[7] ? lineVal.Split(';')[5].Trim('""').Substring(5) : ""Unknown"") : null",
                                    },
                                    path = "//*[local-name() = 'newNameOfCompany']",
                                },
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
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'ticketToGenerate']",
                                    expression = @"""$XmlType""==""Standard Share Capital Consolidation"" ? ""ClosingTicket"" : ""Both"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'ticketToGenerate']",
                                    expression = @"""$XmlType""==""Standard Share Capital Consolidation"" ? ""OpeningTicket"" : ""Both"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'businessEvent1']",
                                    expression = @"
""@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 ? ""Final Dividend"" :
""@Standard Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""Purchase/Sale"" :
""@Standard Dividend Japanese@"".IndexOf(""@$XmlType@"") >= 0 ? ""Dividend Confirmed"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'businessEvent1']",
                                    expression = @"
""@Standard Share Capital Consolidation@"".IndexOf(""@$XmlType@"") >= 0 ? ""Exercise"" :
""@Standard Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""Split"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'businessEvent2']",
                                    expression = @"
""@Standard Dividend In Another Currency@"".IndexOf(""@$XmlType@"") >= 0 ? ""Estimated Dividend"" :
""@Standard Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? ""Split"" :
""@Standard Dividend Japanese@"".IndexOf(""@$XmlType@"") >= 0 ? ""Dividend To Be Confirmed"" : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'corporateActionType']",
                                    expression = @"
""$XmlType"".StartsWith(""Standard Dividend"") || ""$XmlType""==""Standard Return of Capital"" ? ""Dividend"" :
""@Standard SCRIP@Standard Stock Dividend@"".IndexOf(""@$XmlType@"") >= 0 ? ""Free attribution"" :
""$XmlType""==""Standard Stock Split"" ? ""Split"" :
""$XmlType""==""Standard Spin-Off"" ? ""Demerger"" :
""$XmlType""==""Standard Reverse Stock Split"" ? ""Post Rounding"" :
""$XmlType""==""Standard Share Capital Consolidation"" ? ""Merger Average Price"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'corporateActionType']",
                                    expression = @"
""$XmlType""==""Standard Dividend with option"" ? ""Free attribution"" :
""$XmlType""==""Standard Reverse Stock Split"" ? ""Split"" :
""$XmlType""==""Standard Share Capital Consolidation"" ? ""Post Rounding"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][3]/*[local-name() = 'corporateActionType']",
                                    expression = @"
""$XmlType""==""Standard Dividend with option"" ? ""Free attribution"" :
""$XmlType""==""Standard Reverse Stock Split"" ? ""Standard Renaming"" :
""$XmlType""==""Standard Share Capital Consolidation"" ? ""Merger Average Price"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'remark']",
                                    expression = @"""@Standard Reverse Stock Split@Standard Share Capital Consolidation@"".IndexOf(""@$XmlType@"") >= 0 ? ""CashedOutAtClosingPrice"" : ""$XmlType""==""Standard Dividend In Another Currency"" ? ""DividendInStockCurrency"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'remark']",
                                    expression = @"""@Standard Share Capital Consolidation@"".IndexOf(""@$XmlType@"") >= 0 ? ""CashedOutAtClosingPrice"" : ""Unknown"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'exdivDate']",
                                    expression = "\"$CAType\" == \"DIV\" ? System.DateTime.ParseExact(colVal, \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\") : System.DateTime.ParseExact(lineVal.Split(';')[20], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Record Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'recordDate']",
                                    expression = @"
""@Standard Dividend In Another Currency@Standard Stock Dividend@Standard Dividend@Standard Dividend Japanese@Standard Dividend with option@"".IndexOf(""@$XmlType@"") >= 0 ?
System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
""@Standard Stock Split@Standard Reverse Stock Split@Standard Share Capital Consolidation@"".IndexOf(""@$XmlType@"") >= 0 ?
//Effective Date
System.DateTime.ParseExact(lineVal.Split(';')[14], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
//Record Date
System.DateTime.ParseExact(lineVal.Split(';')[15], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'corporateActionDate']",
                                    expression = @"
""@Standard SCRIP@Standard Return of Capital@"".IndexOf(""@$XmlType@"") >= 0 ?
//Record Date
System.DateTime.ParseExact(lineVal.Split(';')[15], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
""@Standard Dividend Japanese@Standard Dividend In Another Currency@Standard Dividend with option@Standard Dividend@Standard Stock Dividend@"".IndexOf(""@$XmlType@"") >= 0 ?
//Dividend Record Date
System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
""@Standard Stock Split@Standard Reverse Stock Split@Standard Share Capital Consolidation@"".IndexOf(""@$XmlType@"") >= 0 ?
//Effective Date
System.DateTime.ParseExact(lineVal.Split(';')[14], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
""@Standard Spin-Off@Standard SCRIP@"".IndexOf(""@$XmlType@"") >= 0 ?
//Capital Change Ex Date
System.DateTime.ParseExact(lineVal.Split(';')[20], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") :
//Record Date
System.DateTime.ParseExact(lineVal.Split(';')[15], ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"")"
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Pay Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'paymentDate']",
                                    //No need to lookup for EffectiveDate as it's on the same line. But as lookup is already done --> use it
                                    expression = @"""@Standard Dividend Japanese@Standard Dividend In Another Currency@Standard Dividend with option@Standard Dividend@Standard Stock Dividend@"".IndexOf(""@$XmlType@"") >= 0 ? System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") : ""$EffectiveDate"""
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Rate", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'coefficient']",
                                    expression = @"
""@Standard Dividend with option@Standard Dividend@Standard Dividend Japanese@"".IndexOf(""$XmlType"") >= 0 ? colVal :
""@Standard Return of Capital@Standard Dividend In Another Currency@"".IndexOf(""$XmlType"") >= 0 ? ""$DividendRate"" :
""@Standard Stock Dividend@Standard SCRIP@"".IndexOf(""$XmlType"") >= 0 ? ((double)$numerator/$denominator).ToString() :
""@Standard Stock Split@Standard Reverse Stock Split@"".IndexOf(""@$XmlType@"") >= 0 ? lineVal.Split(';')[18] : ""0"""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][1]/*[local-name() = 'cash']",
                                    expression = @"""@Standard Dividend In Another Currency@"".IndexOf(""$XmlType"") >= 0 ? colVal : """""
                                },
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'adjustments']/*[local-name() = 'adjustment'][2]/*[local-name() = 'coefficient']",
                                    expression = @"""@Standard Dividend with option@"".IndexOf(""$XmlType"") >= 0 ? ((double)$numerator/$denominator).ToString() : ""1"""
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
                                        Expression = @"@""
SELECT T.SICOVAM
FROM TITRES T
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = T.SICOVAM AND ERI.VALUE = '"" + colVal + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_IDENT = ERI.REF_IDENT AND ERD.REF_NAME = 'ISIN'
    JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI_REFI ON ERI_REFI.SOPHIS_IDENT = T.SICOVAM AND ERI_REFI.VALUE = '"" + lineVal.Split(';')[49] + @""'
        JOIN EXTRNL_REFERENCES_DEFINITION ERD_REFI ON ERD_REFI.REF_IDENT = ERI_REFI.REF_IDENT AND ERD_REFI.REF_NAME = 'Refinitiv_Exch_Code'
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MONTANT != 0)"""
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
                                    path = "//*[local-name() = 'electionEndDate'] | //*[local-name() = 'resultDate']",
                                    expression = "\"$CAType\" == \"DIV\" ? \"$SubscriptionPeriodEndDate\" : System.DateTime.ParseExact(lineVal.Split(';')[14], \"MM/dd/yyyy\", System.Globalization.CultureInfo.InvariantCulture).ToString(\"yyyy-MM-dd\")"
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Announcement Date", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] {
                                new PdtColumnDest {
                                    path = "//*[local-name() = 'resultDate']",
                                    expression = @"""@Standard Dividend with option@"".IndexOf(""$XmlType"") >= 0 ? ""$EffectiveDate"" : ""$CAType"" == ""DIV"" ? System.DateTime.ParseExact(colVal, ""MM/dd/yyyy"", System.Globalization.CultureInfo.InvariantCulture).ToString(""yyyy-MM-dd"") : """""
                                },
                            }
                        },
                        new PdtColumn { name = "Dividend Currency", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'currency']" } }
                        },
                        new PdtColumn { name = "Dividend Currency Description", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Ex Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Pay Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Payment Type", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Record Date", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Delete Marker", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Dividend Market Event ID", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Capital Change Event Type", isRequired = true, isRelativeToRootNode = true},
                        new PdtColumn { name = "Corporate Actions ID", isRequired = true, isRelativeToRootNode = true,
                            destPaths = new [] { new PdtColumnDest { path = "//*[local-name() = 'reference']" } }
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
                            expressionAfter=@"""Stock consolidation"" == colVal ? ""Standard Reverse Stock Split"" : ""Unknown""",
                        },
                    },
                    processingCondition = @"""$DuplicateXmlType"" != ""Standard Reverse Stock Split"" && ""$nameChange"" != ""Unknown""",
                    postProcessEvent = @"
        var nodePaths = new List<string>();
        if (""$XmlType"" == ""Standard Renaming"") {
            nodePaths.Add(""//*[local-name() = 'conversionRatio']"");
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
WHERE T.SICOVAM IN (SELECT SICOVAM FROM JOIN_POSITION_HISTOMVTS WHERE MONTANT != 0)"""
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
                    processingCondition = "lineVal.Substring(51, 1) == \"+\" && ((\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") < 0 && (\"$TransactionType\"==\"SPOT\" || \"$TransactionType\"==\"FORWARD\")) || (\",GFJL,GFJH,GFJG,GFJN,\".IndexOf(\",\" + lineVal.Substring(0, 6).Trim() + \",\") >= 0 && \"$TransactionType\"==\"SPOT\" && lineVal.Substring(52, 8).Trim()==\"SSBMI\"))",
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
                            expressionBefore = "(lineVal.Split(';')[0].Contains(\"FETALULLISV\") || lineVal.Split(';')[0].Contains(\"BILLIE2D\")) ?  lineVal.Split(';')[1].Substring(6) : lineVal.Split(';')[1]",
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
                            expressionBefore = "(lineVal.Split(';')[0].Contains(\"FETALULLISV\") || lineVal.Split(';')[0].Contains(\"BILLIE2D\")) ?  lineVal.Split(';')[1].Substring(6) : lineVal.Split(';')[1]",
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
                            expressionBefore = "(lineVal.Split(';')[0].Contains(\"FETALULLISV\") || lineVal.Split(';')[0].Contains(\"BILLIE2D\")) ?  lineVal.Split(';')[1].Substring(6) : lineVal.Split(';')[1]",
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
                }
            }
            };

            var serializer = new XmlSerializer(typeof(PdtTransformationSetting));
            TextWriter writer = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile));
            serializer.Serialize(writer, Setting);
            writer.Close();

            Logger.Warn("InitializeAndSaveConfig.END");
            return Setting;
        }

        public static PdtTransformationSetting LoadConfigFromFile(string configFile)
        {
            try
            {
                Logger.Info("LoadConfigFromFile.BEGIN");
                PdtTransformationSetting Setting;
                XmlSerializer serializer = new XmlSerializer(typeof(PdtTransformationSetting));
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile)))
                {
                    StreamReader reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile));
                    Setting = (PdtTransformationSetting)serializer.Deserialize(reader);
                    reader.Close();
                }
                else
                {
                    Logger.Debug("Config file does not exist. Finding in resource...");
                    var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + configFile));
                    if (resourceName == null) throw new ArgumentException(string.Format("Config file not found in resource: {0}", configFile));
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                    Setting = (PdtTransformationSetting)serializer.Deserialize(stream);
                }
                Logger.Info("LoadConfigFromFile.END");
                return Setting;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        // {table name, {key, columns[]}}
        private static Dictionary<string, Dictionary<string, string[]>> lookupTables = new Dictionary<string, Dictionary<string, string[]>>();
        private static Dictionary<string, string> CacheSQLValues = new Dictionary<string, string>();
        private static string computeLookup(string name, string Val, string[] lines, Dictionary<string, string> Variables, PdtColumnLookup lookup, string dirName, PdtLookupTable[] tables, string inputLine)
        {
            while (lookup != null)
            {
                if (!string.IsNullOrEmpty(lookup.Table) && lookup.Table.StartsWith("SQL")) // Lookup by SQL query from database
                {
                    var sqlQuery = evaluateExpression(name, lookup.Expression, Variables, Val, inputLine);
                    Logger.Debug(string.Format("sqlQuery = {0}", sqlQuery));
                    if (CacheSQLValues.ContainsKey(sqlQuery))
                    {
                        Val = CacheSQLValues[sqlQuery];
                        Logger.Debug($"Return value from Cache: Val={Val}");
                    }
                    else
                    {
                        using (var command = new OracleCommand(sqlQuery, DataTransformationService.DbConnection))
                        {
                            var newVal = command.ExecuteScalar();
                            if (newVal != null)
                            {
                                Val = newVal.ToString();
                                Logger.Info("Val = ", Val);
                            }
                            else
                            {
                                //Val = string.Empty;
                                Val = newVal as string;
                                Logger.Warn("SQL query returned nothing. Set Val to null");
                            }
                            CacheSQLValues.Add(sqlQuery, Val);
                        }
                    }
                } else if (!string.IsNullOrEmpty(lookup.Table)) // Lookup by Table as a mapping cache stored in memory
                {
                    if (!lookupTables.ContainsKey(lookup.Table))
                    {
                        var table = tables.Single(x => x.Name == lookup.Table);
                        Logger.Debug(string.Format("Building table = {0}, from file = {1}", table.Name, table.File));
                        if (table.File == "SQL")
                        {
                            using (var command = new OracleCommand(table.keyExpression, DataTransformationService.DbConnection))
                            {
                                using (OracleDataReader reader = command.ExecuteReader())
                                {
                                    var rows = new Dictionary<string, string[]>();
                                    while (reader.Read())
                                    {
                                        var rowKey = reader.GetString(0);
                                        string[] rowCols = new string[reader.FieldCount - 1];
                                        for (int i = 1; i < reader.FieldCount; i++)
                                        {
                                            rowCols[i] = reader.GetString(i);
                                        }
                                        rows[rowKey] = rowCols;
                                    }
                                    lookupTables.Add(table.Name, rows);
                                    Logger.Debug(string.Format("Table {0}, {1} rows", table.Name, rows.Count));
                                }
                            }
                        }
                        else
                        {
                            var fileEntries = Directory.GetFiles(dirName, table.File);
                            if (fileEntries.Length == 0)
                            {
                                Logger.Debug(string.Format("Could not find file {0} in folder {1}. Try getting from base directory...", table.File, dirName));
                                fileEntries = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, table.File);
                            }
                            if (fileEntries.Length == 1)
                            {
                                var lookupLines = File.ReadAllLines(fileEntries[0]);
                                var rows = new Dictionary<string, string[]>();
                                foreach (var lineVal in lookupLines)
                                {
                                    string newLineVal = lineVal;
                                    if (table.csvSeparator != '\0')
                                    {
                                        var csvVals = Regex.Matches(newLineVal, $"(?:^|{table.csvSeparator})(\"(?:[^\"]+|\"\")*\"|[^{table.csvSeparator}]*)").Cast<Match>().Select(m => m.Value.TrimStart(table.csvSeparator)).ToArray();

                                        //bool preProcessed = false;
                                        for (int j = 0; j < csvVals.Length; j++)
                                        {
                                            if (csvVals[j].StartsWith("\"") && csvVals[j].EndsWith("\""))
                                            {
                                                csvVals[j] = csvVals[j].Substring(1, csvVals[j].Length - 2);
                                                //preProcessed = true;
                                            }
                                        }
                                        //if (preProcessed)
                                        //{
                                            newLineVal = string.Join(DEFAULT_CSV_SEPARATOR.ToString(), csvVals);
                                            Logger.Debug($"lineVal: {lineVal}");
                                            Logger.Debug($"newLineVal: {newLineVal}");
                                        //}
                                    }

                                    if (!string.IsNullOrEmpty(table.processingCondition))
                                    {
                                        var result = evaluateExpression("processingCondition", table.processingCondition, null, string.Empty, newLineVal);
                                        Logger.Debug(string.Format("result={0}", result));
                                        if (!bool.Parse(result)) continue;
                                    }

                                    //Logger.Debug(string.Format("lineVal={0}", lineVal));
                                    var rowKey = evaluateExpression(string.Format("Table {0}.Key", table.Name), table.keyExpression, null, string.Empty, newLineVal);
                                    if (table.columnsExpression != null)
                                    {
                                        string[] rowCols = new string[table.columnsExpression.Length];
                                        for (int i = 0; i < table.columnsExpression.Length; i++)
                                        {
                                            rowCols[i] = evaluateExpression(string.Format("Table {0}.Col.{1}", table.Name, i), table.columnsExpression[i], null, string.Empty, newLineVal);
                                        }
                                        rows[rowKey] = rowCols;
                                    } else rows[rowKey] = null;
                                }
                                lookupTables.Add(table.Name, rows);
                                Logger.Debug(string.Format("Table {0}, {1} rows", table.Name, rows.Count));
                            }
                            else
                            {
                                Logger.Error(string.Format("Error searching files {0}, count={1}", table.File, fileEntries.Length));
                            }
                        }
                    }
                    var newVal = string.Empty;
                    if (lookupTables[lookup.Table].ContainsKey(Val))
                    {
                        newVal = int.Parse(lookup.ColumnIndex) < 0 ? Val : lookupTables[lookup.Table][Val][int.Parse(lookup.ColumnIndex)];
                    } else
                    {
                        Logger.Warn($"Key {Val} is not found in the table {lookup.Table}.");
                    }
                    if (!string.IsNullOrEmpty(lookup.Expression))
                    {
                        Logger.Debug(string.Format("evaluate newVal={0}", newVal));
                        newVal = evaluateExpression(string.Format("newVal={0}", newVal), lookup.Expression, Variables, newVal);
                    }
                    Logger.Debug(string.Format("Getting value from Table={0}, Key={1}, ColumnIndex={2}, Value={3}", lookup.Table, Val, lookup.ColumnIndex, newVal));
                    Val = newVal;
                } else if (string.IsNullOrEmpty(lookup.File)) // Lookup by another File served as dictionary stored in disk
                {
                    Logger.Debug(string.Format("Evaluate Lookup: {0}, colVal={1}", lookup.Expression, Val));
                    foreach (var lineVal in lines)
                    {
                        var lookupResult = evaluateExpression(name, lookup.Expression, Variables, Val, lineVal);
                        if (!string.IsNullOrWhiteSpace(lookupResult))
                        {
                            Val = lookupResult;
                            Logger.Debug(string.Format("{0}, Val={1}", name, Val));
                            break;
                        }
                    }
                }
                else // Lookup in the same file
                {
                    var key = string.Format("File={0},colVal={1},Expression={2}", lookup.File, Val, lookup.Expression);
                    if (Variables != null)
                    {
                        foreach (var Var in Variables)
                        {
                            key = key.Replace("$" + Var.Key, Var.Value.ToLiteral());
                        }
                    }
                    if (!CacheLookupValuesFile.ContainsKey(key))
                    {
                        var fileEntries = Directory.GetFiles(dirName, lookup.File);
                        if (fileEntries.Length == 0)
                        {
                            Logger.Debug(string.Format("Could not find file {0} in folder {1}. Try getting from base directory...", lookup.File, dirName));
                            fileEntries = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, lookup.File);
                        }
                        if (fileEntries.Length == 1)
                        {
                            var lookupLines = File.ReadAllLines(fileEntries[0]);
                            Logger.Debug(string.Format("Evaluate Lookup: {0}, colVal={1}, File={2}", lookup.Expression, Val, lookup.File));
                            string lookupResult = null;
                            foreach (var lineVal in lookupLines)
                            {
                                lookupResult = evaluateExpression(name, lookup.Expression, Variables, Val, lineVal);
                                if (!string.IsNullOrWhiteSpace(lookupResult))
                                {
                                    Val = lookupResult;
                                    Logger.Debug(string.Format("{0}, Val={1}", name, Val));
                                    break;
                                }
                            }
                            if (string.IsNullOrWhiteSpace(lookupResult))
                            {
                                Logger.Warn($"lookup not found. Take the default value.");
                            }
                            Logger.Debug(string.Format("Evaluate Lookup END: Val={0}", Val));
                        }
                        else
                        {
                            Logger.Error(string.Format("Error searching files {0}, count={1}", lookup.File, fileEntries.Length));
                        }
                        CacheLookupValuesFile[key] = Val;
                    }
                    else
                    {
                        Val = CacheLookupValuesFile[key];
                        Logger.Debug(string.Format("Return value from cache={0}, key={1}", Val, key));
                    }
                }
                lookup = lookup.Lookup;
            }
            Logger.Debug($"computeLookup.END(Val={Val})");
            return Val;
        }

        const string E_ERROR = "ERROR: ";
        const string E_WARN = "WARN: ";
        private static string evaluateExpression(string name, string expression, Dictionary<string, string> Variables, string Val, string lineVal = "")
        {
            if (!string.IsNullOrEmpty(expression))
            {
                Logger.Debug(string.Format("{0}, Val={1}, Evaluate: {2}", name, Val, expression));
                if (Variables != null)
                {
                    foreach (var Var in Variables)
                    {
                        expression = expression.Replace("$" + Var.Key, Var.Value.ToLiteral());
                    }
                }
                try
                {
                    //Val = await CSharpScript.EvaluateAsync<string>(destCol.expression.Replace("colVal", $"\"{Val}\""));
                    Val = Compiler.Evaluate(expression, Val, lineVal);
                    Logger.Debug(string.Format("{0}, Val={1}", name, Val));
                    if (Val.StartsWith(E_ERROR)) throw new Exception(Val.Substring(E_ERROR.Length));
                    if (Val.StartsWith(E_WARN))
                    {
                        Val = string.Empty;
                        Logger.Warn("Set Val to empty");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error when evaluating expression");
                    throw e;
                }
            }
            return Val;
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

        private static Dictionary<string, string> CacheLookupValuesFile = new Dictionary<string, string>();
        //private static Dictionary<string, string> CacheLookupValuesLines = new Dictionary<string, string>();
      
        private static void Transform2Csv(TransformationIO transIO, PdtTransformationSetting Setting, string transName, string inputCsvFile, string outputCsvFile, string failureFile, bool isExcelFile=false)
        {
            PdtTransformation trans = Setting.Transformations.Where(x => x.name == transName).FirstOrDefault();
            if (trans == null) throw new ArgumentException(string.Format("Transformation name not found: {0}", transName));

            Logger.Debug($"Transform2Csv.BEGIN(name={trans.name}, inputCsvFile={inputCsvFile}, outputCsvFile={outputCsvFile}, failureFile={failureFile}, templateFile={trans.templateFile})");

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
                    lines = new string[rowCount-1];                   
                    int a = 0;
                   
                    for (int i = 2; i <= rowCount; i++)
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

            lines = lines.Skip(trans.csvSkipLines).ToArray();
            if (trans.rowCloning != null)
            {
                Logger.Debug($"Cloning rows Length={lines.Length}");
                var clonedLines = new List<string>();
                foreach(var line in lines) {
                    for(int i=0; i<trans.rowCloning.Ntimes; i++)
                    {
                        var expression = trans.rowCloning.Expression.Replace("$cloneIdx", i.ToString());
                        var clonedLine = evaluateExpression($"cloning {i}", expression, null, string.Empty, line);
                        clonedLines.Add(clonedLine);
                    }
                }
                lines = clonedLines.ToArray();
                Logger.Debug($"After cloing rows Length={lines.Length}");
            }
            var outputLines = new List<string[]>();
            string outputCsvFileTemp = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            Logger.Debug($"saving to temporary file: {outputCsvFileTemp}");
            using (var swOut = new StreamWriter(outputCsvFileTemp))
            {
                swOut.WriteLine(headerLine);
                var failureLines = new List<string[]>();
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
                                var keyVal = evaluateExpression($"uniqueConstraint_{ic + 1}", trans.uniqueConstraints[ic], Variables, string.Empty, inputLine);
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
                                var result = evaluateExpression($"checkConstraint_{ic+1}", trans.checkConstraints[ic], Variables, string.Empty, inputLine);
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
                        if (trans.variables != null) {
                            foreach(var Var in trans.variables) {
                                var variableInfo = string.Format("Variable={0}", Var.name);
                                Logger.Debug(variableInfo);
                                var Val = Variables.ContainsKey(Var.name) ? Variables[Var.name] : string.Empty;
                                Val = evaluateExpression(variableInfo, Var.expressionBefore, Variables, Val, inputLine);                                //if (Var.Lookup != null) {
                                //    var key = string.Format("colVal={0}, Expression={1}, Depth={2}", Val, Var.Lookup.Expression, Var.Lookup.Depth);
                                //    if (!CacheLookupValuesLines.ContainsKey(key))
                                //    {
                                        Val = computeLookup(variableInfo, Val, lines, Variables, Var.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine);
                                //        CacheLookupValuesLines[key] = Val;
                                //    }
                                //    else
                                //    {
                                //        Val = CacheLookupValuesLines[key];
                                //        Logger.Debug(string.Format("Lookup return value from cache={0}, key={1}", Val, key));
                                //    }
                                //}
                                Val = evaluateExpression(variableInfo, Var.expressionAfter, Variables, Val, inputLine);
                                Variables[Var.name] = Val;
                                if (GlobalVariables.ContainsKey(Var.name)) GlobalVariables[Var.name] = Val;
                                if (!string.IsNullOrEmpty(Var.expressionStorage) && Var.expressionStorage != "NoStorage" && i==lines.Length - 1)
                                {
                                    var storeVal = evaluateExpression(variableInfo, Var.expressionStorage, Variables, string.Empty);
                                    Utils.AddOrUpdateAppSettings(Var.name, storeVal);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(trans.processingCondition))
                        {
                            var result = evaluateExpression("processingCondition", trans.processingCondition, Variables, string.Empty, inputLine);
                            Logger.Debug(string.Format("result={0}", result));
                            if (!bool.Parse(result)) continue;
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
                                    originalVal = inputLineSplitted[currentPos];
                                    currentPos++;
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
                                    var Val = computeLookup(destColInfo, originalVal, lines, Variables, destCol.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine);
                                    Val = evaluateExpression(destColInfo, destCol.expression, Variables, Val, inputLine);

                                    if (!string.IsNullOrEmpty(destCol.processingCondition))
                                    {
                                        var result = evaluateExpression("processingCondition", destCol.processingCondition, Variables, Val, inputLine);
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
            } else
            {
                Logger.Debug("Empty output csv file is ignored");
            }
            Logger.Debug("Transform2Csv.END");
        }

        private static void Transform2Xml(TransformationIO transIO, PdtTransformationSetting Setting, string transName, string inputCsvFile, string outputXmlFile, string failureFile)
        {
            PdtTransformation trans = Setting.Transformations.Where(x => x.name == transName).FirstOrDefault();
            if (trans == null) throw new ArgumentException(string.Format("Transformation name not found: {0}", transName));

            Logger.Debug($"Transform2Xml.BEGIN(name={trans.name}, inputCsvFile={inputCsvFile}, outputXmlFile={outputXmlFile}, failureFile={failureFile}, templateFile ={trans.templateFile})");

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

            //if (trans.csvSrcSeparator != '\0') lines = lines.Skip(1).ToArray();
            //string[] csvHeaders = lines[0].Split(trans.csvDestSeparator);
            string[] csvHeaders = trans.columns.Select(x => x.name).ToArray();
            var failureLines = new List<string[]>();

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

            //transformation for the absolute columns whose values are the same for all lines
            PdtColumn[] arrAbsoluteColumns = trans.columns.Where(x => !x.isRelativeToRootNode).ToArray();

            //transformation for the relative columns whose values are different between lines
            string repeatingRootPath = trans.repeatingRootPath;
            string repeatingChildrenPath = trans.repeatingChildrenPath;
            var docClone = (XmlDocument)doc.Clone();
            XmlNode repeatingRootNode = docClone.SelectSingleNode(repeatingRootPath); // un seul noeud
            XmlNodeList repeatingChildrenNodes = docClone.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
            XmlNode templateNode = repeatingChildrenNodes.Item(0);

            string filePrimaryKeyVal = null;
            Dictionary<string, string> lastLineVariables = null;
            int bunchIndexPrimaryKey = 0;
            if (repeatingRootPath != null && !string.IsNullOrEmpty(repeatingRootPath)
                    && repeatingChildrenPath != null && !string.IsNullOrEmpty(repeatingChildrenPath))
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
                                var keyVal = evaluateExpression($"uniqueConstraint_{ic + 1}", trans.uniqueConstraints[ic], Variables, string.Empty, inputLine);
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
                                var result = evaluateExpression($"checkConstraint_{ic + 1}", trans.checkConstraints[ic], Variables, string.Empty, inputLine);
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
                                Val = evaluateExpression(variableInfo, Var.expressionBefore, Variables, Val, inputLine);
                                //if (Var.Lookup != null) {
                                //    var key = string.Format("colVal={0}, Expression={1}, Depth={2}", Val, Var.Lookup.Expression, Var.Lookup.Depth);
                                //    if (!CacheLookupValuesLines.ContainsKey(key))
                                //    {
                                Val = computeLookup(variableInfo, Val, lines, Variables, Var.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine);
                                //        CacheLookupValuesLines[key] = Val;
                                //    }
                                //    else
                                //    {
                                //        Val = CacheLookupValuesLines[key];
                                //        Logger.Debug(string.Format("Lookup return value from cache={0}, key={1}", Val, key));
                                //    }
                                //}
                                Val = evaluateExpression(variableInfo, Var.expressionAfter, Variables, Val, inputLine);
                                Variables[Var.name] = Val;
                                if (GlobalVariables.ContainsKey(Var.name)) GlobalVariables[Var.name] = Val;
                                if (!string.IsNullOrEmpty(Var.expressionStorage) && Var.expressionStorage != "NoStorage" && i == lines.Length - 1)
                                {
                                    var storeVal = evaluateExpression(variableInfo, Var.expressionStorage, Variables, string.Empty);
                                    Utils.AddOrUpdateAppSettings(Var.name, storeVal);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(trans.processingCondition))
                        {
                            var result = evaluateExpression("processingCondition", trans.processingCondition, Variables, string.Empty, inputLine);
                            Logger.Debug(string.Format("result={0}", result));
                            if (!bool.Parse(result)) continue;
                        }

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
                        var newFilePrimaryKeyVal = evaluateExpression("newFileBreakExpression", trans.fileBreakExpression, Variables, string.Empty, inputLine);
                        createNewFile = createNewFile || newFilePrimaryKeyVal != filePrimaryKeyVal;
                        if (createNewFile)
                        {
                            if (string.IsNullOrEmpty(filePrimaryKeyVal))
                            {
                                for (int j = 1; j < repeatingChildrenNodes.Count; j++)
                                {
                                    repeatingRootNode.RemoveChild(repeatingChildrenNodes.Item(j));
                                }
                            }
                            else
                            {//new primary key found --> break into new file. save current file.
                                repeatingRootNode.RemoveChild(templateNode);
                                for (int j = 0; j < transformedNodes.Count; j++)
                                {
                                    repeatingRootNode.AppendChild(transformedNodes[j]);
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
                                repeatingRootNode.RemoveAll();
                                repeatingRootNode.AppendChild(templateNode);
                                //create new one
                                docClone = (XmlDocument)doc.Clone();
                                repeatingRootNode = docClone.SelectSingleNode(repeatingRootPath); // un seul noeud
                                repeatingChildrenNodes = docClone.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
                                templateNode = repeatingChildrenNodes.Item(0);
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

                                    var Val = computeLookup(destColInfo, originalVal, lines, null, destCol.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine);
                                    Val = evaluateExpression(destColInfo, destCol.expression, null, Val, inputLine);

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
                                var Val = computeLookup(destColInfo, originalVal, lines, Variables, destCol.Lookup, Path.GetDirectoryName(inputCsvFile), Setting.Tables, inputLine);
                                Val = evaluateExpression(destColInfo, destCol.expression, Variables, Val, inputLine);

                                if (!string.IsNullOrEmpty(destCol.processingCondition))
                                {
                                    var result = evaluateExpression("processingCondition", destCol.processingCondition, Variables, Val, inputLine);
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
                        transformedNodes.Add(templateNode.Clone());
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
                    repeatingRootNode.RemoveChild(templateNode);
                    int bunchIndex = 0;
                    for (int i=0; i<transformedNodes.Count; i++)
                    {
                        repeatingRootNode.AppendChild(transformedNodes[i]);
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
                            repeatingRootNode.RemoveAll();
                            //create new one
                            docClone = (XmlDocument)doc.Clone();
                            repeatingRootNode = docClone.SelectSingleNode(repeatingRootPath); // un seul noeud
                            repeatingChildrenNodes = docClone.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
                            templateNode = repeatingChildrenNodes.Item(0);
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
                        repeatingRootNode.RemoveAll();
                        //create new one
                        docClone = (XmlDocument)doc.Clone();
                        repeatingRootNode = docClone.SelectSingleNode(repeatingRootPath); // un seul noeud
                        repeatingChildrenNodes = docClone.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
                        templateNode = repeatingChildrenNodes.Item(0);
                    }
                    repeatingRootNode.AppendChild(templateNode);
                }
                foreach (XmlNode node in transformedNodes)
                {
                    repeatingRootNode.InsertBefore(node, templateNode);
                }
                repeatingRootNode.RemoveChild(templateNode);
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
            Compiler.CleanUp();
            if (repeatingRootNode.HasChildNodes || !trans.ClearEmptyOutput)
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
                    repeatingRootNode.RemoveAll();
                    repeatingRootNode.AppendChild(templateNode);
                }
                else
                {
                    // save the output file
                    Logger.Debug($"Saving to file: {outputXmlFile}");
                    docClone.Save(outputXmlFile);
                }
            }
            else Logger.Debug("Empty output XML file is ignored");
            Logger.Debug("Transform2Xml.END");
        }

        private static void TransformXml2Csv(TransformationIO transIO, PdtTransformationSetting Setting, string transName, string inputXmlFile, string outputCsvFile)
        {
            PdtTransformation trans = Setting.Transformations.Where(x => x.name == transName).FirstOrDefault();
            if (trans == null) throw new ArgumentException(string.Format("Transformation name not found: {0}", transName));

            Logger.Debug($"TransformXml2Csv.BEGIN(name={trans.name}, inputXmlFile={inputXmlFile}, outputCsvFile={outputCsvFile}, templateFile ={trans.templateFile})");

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
                    && !string.IsNullOrEmpty(transIO.EmailBody))
                {
                    Logger.Debug($"Sending email to {transIO.EmailRecipientTo} ({outputCsvFile})");
                    try
                    {
                        Utils.SendEmail(transIO.EmailRecipientTo, transIO.EmailSubject, transIO.EmailBody, transIO.EmailRecipientCC, new[] { outputCsvFile });
                    } catch(Exception e)
                    {
                        Logger.Error(e, "Error when sending email");
                    }
                }
            }
            else
            {
                Logger.Debug("No line transformed.");
            }

            Logger.Debug("TransformXml2Csv.END");
        }
    }
}

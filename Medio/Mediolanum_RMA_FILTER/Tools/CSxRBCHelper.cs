using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using MEDIO.CORE.Tools;
using Oracle.DataAccess.Client;
using RichMarketAdapter.ticket;
using sophis.log;
using sophis.utils;
using transaction;

namespace Mediolanum_RMA_FILTER.Tools
{
    
    public static class CSxRBCHelper
    {
        #region const
        public static string _ClassName = typeof(CSxRBCHelper).Name;
        public const string DefaultDateFormat = "dMMyyyy";

        public const string DateFormat2 = "d/M/yyyy";
        public const string DateFormat3 = "d-MMM-yy";
        public const string DateFormat4 = "MM/dd/yyyy";

        public static string[] CommonDateFormats = { DefaultDateFormat, DateFormat2 }; //Bonds, equities, loans, funds
        public static string[] OptionsDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] FuturesDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] SwapsDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] ForexDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] CashDateFormats = { DefaultDateFormat, DateFormat2 };
        public static string[] SSBAllCustodyDateFormats = { DefaultDateFormat, DateFormat4 };
        public static string[] CADateFormats = { DefaultDateFormat, DateFormat2, DateFormat3 };
        #endregion

        private static HashSet<string> _currencies;
        private static bool _APIInit = false;
        static CSxRBCHelper()
        {
        }

        private static string GetDateFormatString(params string[] dateFormats)
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

        public static string WithMaxLength(this string value, int maxLength)
        {
            return value?.Substring(0, Math.Min(value.Length, maxLength));
        }

        #region RBC CSV format
        public static bool IsEquityOrBond(string csv)
        {
            return IsEquityOrBond(csv.SplitCSV());
        }
        public static bool IsEquityOrBond(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null && fields.Count > 0)
                {
                    if (!fields.GetValue(RBCTicketType.EquityColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.EquityColumns.TradeDate).ValidateDate(CommonDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " + GetDateFormatString(CommonDateFormats) + " Value: " + fields.GetValue(RBCTicketType.EquityColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.EquityColumns.SettlementDate).ValidateDate(CommonDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " + GetDateFormatString(CommonDateFormats) + " Value: " + fields.GetValue(RBCTicketType.EquityColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.EquityColumns.Quantity).ValidateDouble())
                    {
                        logger.log(Severity.debug, "Quantity validation failed, cannot cast to double");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.EquityColumns.Price).ValidateDouble())
                    {
                        logger.log(Severity.debug, "Price validation failed, cannot cast to double");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.EquityColumns.Currency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.EquityColumns.TransactionDescription).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "TransactionDescription validation failed, could not find a valid currency");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.EquityColumns.SecurityDescription).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "SecurityDescription validation failed, this field cannot be empty");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsBond(string csv)
        {
            return IsBond(csv.SplitCSV());
        }
        public static bool IsBond(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (!IsEquityOrBond(fields))
                {
                    return false;
                }
                else
                {
                    if (fields != null && fields.Count > 0)
                    {
                        string securityDescription = fields.GetValue(RBCTicketType.EquityColumns.SecurityDescription);
                        if (securityDescription.ValidateNotEmpty())
                        {
                            if (securityDescription.ToUpper().CompareTo("BOND") == 0)
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            logger.log(Severity.error, "Failed to find a SecurityDescription field");
                            return false;
                        }
                    }
                    else
                    {
                        logger.log(Severity.error, "Fatal error: failed to parse CSV for validation");
                        return false;
                    }
                }
            }
        }

        public static bool IsEquity(string csv)
        {
            return IsEquity(csv.SplitCSV());
        }
        public static bool IsEquity(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (!IsEquityOrBond(fields))
                {
                    return false;
                }
                else
                {
                    if (fields != null)
                    {
                        string securityDescription = fields.GetValue(RBCTicketType.EquityColumns.SecurityDescription);
                        if (securityDescription.ValidateNotEmpty())
                        {
                            if (securityDescription.ToUpper().CompareTo("EQUITIES") == 0)
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            logger.log(Severity.error, "Failed to find a SecurityDescription field");
                            return false;
                        }
                    }
                    else
                    {
                        logger.log(Severity.error, "Fatal error: failed to parse CSV for validation");
                        return false;
                    }
                }
            }
        }

        public static bool IsForex(string csv)
        {
            return IsForex(csv.SplitCSV());
        }
        public static bool IsForex(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null && fields.Count > 0)
                {
                    if (!fields.GetValue(RBCTicketType.ForexColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexColumns.TradeDate).ValidateDate(ForexDateFormats))
                    {
                        logger.log(Severity.error, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(ForexDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.ForexColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexColumns.SettlementDate).ValidateDate(ForexDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(ForexDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.ForexColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexColumns.BuyCurrency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "BuyCurrency validation failed, could not find a valid currency");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexColumns.PurchasedAmount).ValidateDouble())
                    {
                        logger.log(Severity.debug, "PurchasedAmount validation failed, cannot cast to double");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexColumns.SellCurrency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "SellCurrency validation failed, could not find a valid currency");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexColumns.SoldAmount).ValidateDouble())
                    {
                        logger.log(Severity.debug, "SoldAmount validation failed, cannot cast to double");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexColumns.FXRate).ValidateDouble())
                    {
                        logger.log(Severity.debug, "FXRate validation failed, cannot cast to double");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static eRBCTicketType IsEquityOrBondOrLoan(List<string> fields)
        {
            // Loan is a special type of bond
            if (!IsEquityOrBond(fields))
            {
                return eRBCTicketType.Unknown;
            }
            else
            {
                string securityDescription = fields.GetValue(RBCTicketType.EquityColumns.SecurityDescription);
                if (securityDescription.ValidateNotEmpty())
                {
                    switch (securityDescription.ToUpper())
                    {
                        case "EQUITIES":
                        {
                            return eRBCTicketType.Equity;
                        }
                            break;
                        case "BOND":
                        case "BONDS":
                        {
                            return eRBCTicketType.Bond;
                        }
                            break;
                        case "FUND":
                        case "FUNDS":
                        case "ETF":
                        case "ETFS":
                        {
                            return eRBCTicketType.Fund;
                        }
                            break;
                        case "MEMMM":
                        {
                            return eRBCTicketType.Loan;
                        }
                            break;
                        default:
                        {
                            return eRBCTicketType.Loan;
                        }
                            break;
                    }
                }
                return eRBCTicketType.Unknown;
            }
        }

        public static bool IsOption(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.OptionColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.OptionColumns.TradeDate).ValidateDate(OptionsDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(OptionsDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.OptionColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.OptionColumns.SettlementDate).ValidateDate(OptionsDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(OptionsDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.OptionColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.OptionColumns.NAVDate).ValidateDate(OptionsDateFormats))
                    {
                        logger.log(Severity.debug, "NAVDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(OptionsDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.OptionColumns.NAVDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.OptionColumns.OptionCurrency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsFuture(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {

                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.FutureColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }

                    if (!fields.GetValue(RBCTicketType.FutureColumns.TradeDate).ValidateDate(FuturesDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(FuturesDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.FutureColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.FutureColumns.SettlementDate).ValidateDate(FuturesDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(FuturesDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.FutureColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.FutureColumns.NAVDate).ValidateDate(FuturesDateFormats))
                    {
                        logger.log(Severity.debug, "NAVDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(FuturesDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.FutureColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.FutureColumns.FutureCurrency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsCash(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.CashColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed. Value : " + fields.GetValue(RBCTicketType.CashColumns.ExternalFundIdentifier));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CashColumns.TradeDate).ValidateDate(CashDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CashDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.CashColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CashColumns.SettlementDate).ValidateDate(CashDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CashDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.CashColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CashColumns.Currency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsInvoice(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.InvoiceColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.InvoiceColumns.TradeDate).ValidateDate(CashDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CashDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.InvoiceColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.InvoiceColumns.SettlementDate).ValidateDate(CashDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CashDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.InvoiceColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.InvoiceColumns.Currency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsForexHedge(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.ForexHedgeColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexHedgeColumns.TradeDate).ValidateDate(ForexDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(ForexDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.ForexHedgeColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexHedgeColumns.MaturityDate).ValidateDate(ForexDateFormats))
                    {
                        logger.log(Severity.debug, "MaturityDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(ForexDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.ForexHedgeColumns.MaturityDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexHedgeColumns.CounterCurrency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.ForexHedgeColumns.FixedCurrency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsTACash(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.TACashColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.TACashColumns.TradeDate).ValidateDate(CashDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CashDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.TACashColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.TACashColumns.SettlementDate).ValidateDate(CashDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CashDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.TACashColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.TACashColumns.AccountCurrency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsBond2(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.Bond2Columns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.Bond2Columns.TradeDate).ValidateDate(CommonDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CommonDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.Bond2Columns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.Bond2Columns.SettlementDate).ValidateDate(CommonDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CommonDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.Bond2Columns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.Bond2Columns.Currency).ValidateCurrency())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.Bond2Columns.MaturityDate).ValidateDate(CommonDateFormats))
                    {
                        logger.log(Severity.debug, "MaturityDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CommonDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.Bond2Columns.MaturityDate));
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsSwap(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.SwapColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.SwapColumns.TradeDate).ValidateDate(SwapsDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(SwapsDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.SwapColumns.TradeDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.SwapColumns.SettlementDate).ValidateDate(SwapsDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(SwapsDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.SwapColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.SwapColumns.MaturityDate).ValidateDate(SwapsDateFormats))
                    {
                        logger.log(Severity.debug, "MaturityDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(SwapsDateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.SwapColumns.MaturityDate));
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
          
        }

        public static bool IsCA(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.CAColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed. Value : " + fields.GetValue(RBCTicketType.CAColumns.ExternalFundIdentifier));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CAColumns.ExDate).ValidateDate(CADateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CADateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.CAColumns.ExDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CAColumns.PaymentDate).ValidateDate(CADateFormats))
                    {
                        logger.log(Severity.debug, "PaymentDate validation failed, invalid format; expected format: " +
                                                                                                                 GetDateFormatString(CADateFormats) + " Value: " +
                                                                                                                 fields.GetValue(RBCTicketType.CAColumns.PaymentDate));
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
           
        }

        public static bool IsCollateral(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (fields != null)
                {
                    if (!fields.GetValue(RBCTicketType.CollateralColumns.ExternalFundIdentifier).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "ExternalFundIdentifier validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CollateralColumns.FundCustodyCode).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "FundCustodyCode validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CollateralColumns.PTGName).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "PTGName validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CollateralColumns.TradeDate).ValidateDate(CommonDateFormats))
                    {
                        logger.log(Severity.debug, "TradeDate validation failed, invalid format; expected format: " +
                                                                           GetDateFormatString(CommonDateFormats) + " Value: " +
                                                                           fields.GetValue(RBCTicketType.CollateralColumns.TradeDate));
                        return false;
                    }

                    if (!fields.GetValue(RBCTicketType.CollateralColumns.SettlementDate).ValidateDate(CommonDateFormats))
                    {
                        logger.log(Severity.debug, "SettlementDate validation failed, invalid format; expected format: " +
                                                   GetDateFormatString(CommonDateFormats) + " Value: " +
                                                   fields.GetValue(RBCTicketType.CollateralColumns.SettlementDate));
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CollateralColumns.Currency).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "Currency validation failed, could not find a valid currency");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CollateralColumns.Quantity).ValidateNotEmpty()
                        && !fields.GetValue(RBCTicketType.CollateralColumns.NetAmount).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "NetAmount and Quantity validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CollateralColumns.CollateralCounterpartyBICCode).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "CollateralCounterpartyBICCode validation failed, invalid format");
                        return false;
                    }
                    if (!fields.GetValue(RBCTicketType.CollateralColumns.TransactionDescription).ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "TransactionDescription validation failed, invalid format");
                        return false;
                    }
                }
            }
            return true;
        }

        public static eRBCTicketType GetRBCTicketType(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    var appearance = RBCTicketType.RBCFileColumnCount.Count(x => x.Value == fields.Count);
                    if (appearance == 1)   // column count matched
                    {
                        var type = RBCTicketType.RBCFileColumnCount.FirstOrDefault(x => x.Value == fields.Count).Key;
                        logger.log(Severity.debug, "File column count = " + fields.Count + "; matches type " + type);
                        return type;
                    }
                    else if (appearance == 2) // same number of columns
                    {
                        var type = GetTypeOfTheSameNumOfColumns(fields);
                        logger.log(Severity.debug, "File column count = " + fields.Count + "; matches type " + type);
                        return type; 
                    }
                    else // go back to normal matching .. little chance to reach here 
                    {
                        logger.log(Severity.debug, "Cannot find any type by column number, try matching by fields value ... ");
                        if (IsBond2(fields) || RBCTicketType.Bond3Columns.TotalCount == fields.Count)
                        {
                            return eRBCTicketType.Bond2;
                        }
                        else
                        {
                            eRBCTicketType type = IsEquityOrBondOrLoan(fields);
                            if (type != eRBCTicketType.Unknown)
                            {
                                return type;
                            }
                            else if (IsForex(fields))
                            {
                                return eRBCTicketType.Forex;
                            }
                            else if (IsOption(fields))
                            {
                                return eRBCTicketType.Option;
                            }
                            else if (IsFuture(fields))
                            {
                                return eRBCTicketType.Future;
                            }
                            else if (IsCash(fields))
                            {
                                return eRBCTicketType.Cash;
                            }
                            else if (IsInvoice(fields))
                            {
                                return eRBCTicketType.Invoice;
                            }
                            else if (IsForexHedge(fields))
                            {
                                return eRBCTicketType.ForexHedge;
                            }
                            else if (IsTACash(fields))
                            {
                                return eRBCTicketType.TACash;
                            }
                            else if (IsSwap(fields))
                            {
                                return eRBCTicketType.Swap;
                            }
                            else if (IsCA(fields))
                            {
                                return eRBCTicketType.CorporateAction;
                            }
                            else if (IsCollateral(fields))
                            {
                                return eRBCTicketType.Collateral;
                            }
                            else
                            {
                                return eRBCTicketType.Unknown;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while trying to validate document class: " + ex.Message +
                                                                                                             ". InnerException: " + ex.InnerException + ". Stack trace: " + ex.StackTrace);
                    return eRBCTicketType.Unknown;
                }
            }
        }

        /// <summary>
        /// The idea of this function is to use the minimum effect to distinguish between files with the same number of columns 
        /// NOTE. this function will be carefully maintained every time a format change (num. of columns, new file etc) is introduced  
        /// Which also means, there's a potential risk involved due to hardcoding some of the varaibles 
        /// Ideally we would prefer our code to be designed as dynamic as possible, but .... 
        /// with this design we might utilize simpler logics thus achieve better performance
        /// If there are bugs in the future that are a result of failing to adapt to those changes, blame Charaf who came up with this idea :)
        /// </summary>
        /// <returns></returns>
        private static eRBCTicketType GetTypeOfTheSameNumOfColumns(List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int columnCount = fields.Count;
                //  CA or Options?
                if (columnCount == RBCTicketType.CAColumns.TotalCount) /* = RBCTicketType.OptionsColumn.TotalCount = 38*/
                {
                    if (fields.GetValue(8).ValidateDate(CADateFormats))
                        return eRBCTicketType.CorporateAction;
                    else
                        return eRBCTicketType.Option;
                }

                // Swap or Equity?
                if (columnCount == RBCTicketType.SwapColumns.TotalCount) /*RBCTicketType.EquityColumns.TotalCount = 26*/
                {
                    if (fields.GetValue(6).ValidateDate(SwapsDateFormats))
                        return eRBCTicketType.Swap;
                    else
                        return eRBCTicketType.Equity;
                }

                // FX or TACAsh?
                if (columnCount == RBCTicketType.TACashColumns.TotalCount) /*RBCTicketType.ForexColumns.TotalCount = 17*/
                {
                    if (fields.GetValue(3).ValidateDate(ForexDateFormats))
                        return eRBCTicketType.Forex;
                    else
                        return eRBCTicketType.TACash;
                }

                return eRBCTicketType.Unknown;
            }
        }

        #endregion

        #region Query DB & API
        public static HashSet<string> GetAllCurrencies()
        {
            if (_currencies == null)
            {
                string sql = "SELECT DEVISE_TO_STR(CODE) AS CCY from DEVISEV2";
                _currencies = new HashSet<string>(CSxDBHelper.GetMultiRecords(sql).ConvertAll(x => x.ToString()));
            }
            return _currencies;
        }

        public static int GetEntityIDByName(string name)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetEntityIDByName start");
                int res = 0;
                if (!String.IsNullOrEmpty(name))
                {
                    string sql = "SELECT IDENT FROM TIERS WHERE NAME = :name";
                    OracleParameter parameter = new OracleParameter(":name", name);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                    res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                }
                else
                {
                    logger.log(Severity.error, "Invalid argument, name cannot be null or empty");
                }
                return res;
            }
        }

        public static int GetKernelWorkflowEventIDByName(string name)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetKernelWorkflowEventIDByName start");
                int res = 0;
                if (!String.IsNullOrEmpty(name))
                {
                    string sql = "SELECT ID FROM BO_KERNEL_EVENTS WHERE NAME = :name";
                    OracleParameter parameter = new OracleParameter(":name", name);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                    res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    logger.log(Severity.debug, "Event ID got by name '"+name+"'" + " = " + res);
                }
                else
                {
                    logger.log(Severity.error, "Invalid argument, name cannot be null or empty");
                }
                return res;
            }
        }

        public static Dictionary<string, int> GetAccountList()
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                Dictionary<string, int> retval = new Dictionary<string, int>();
                try
                {
                    string sql = "SELECT A.ACCOUNT_AT_CUSTODIAN, R.VALUE AS ROOTPORTFOLIO FROM BO_TREASURY_ACCOUNT A, BO_TREASURY_EXT_REF R, BO_TREASURY_EXT_REF_DEF D  WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ID = R.ACC_ID AND R.REF_ID = D.REF_ID AND D.REF_NAME = 'RootPortfolio'";
                    retval = CSxDBHelper.GetDictionary<string, int>(sql);
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while trying to read accounts from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                return retval;
            }
        }

        public static List<Tuple<int, int, string>> GetDelegateManagers()
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                List<Tuple<int, int, string>> res = new List<Tuple<int, int, string>>();
                try
                {
                    string sql = "SELECT R.ACC_ID, R.VALUE, A.ACCOUNT_AT_CUSTODIAN FROM  BO_TREASURY_ACCOUNT A, BO_TREASURY_EXT_REF R, BO_TREASURY_EXT_REF_DEF D WHERE A.ID = R.ACC_ID AND R.REF_ID = D.REF_ID AND D.REF_NAME = 'DelegateManagerID'";
                    res = CSxDBHelper.GeTupleList<int, int, string>(sql);
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while trying to read delegate managers from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                return res;
            }
        }

        public static Dictionary<string, int> GetBusinessEventsList()
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                Dictionary<string, int> res = new Dictionary<string, int>();
                try
                {
                    string sql = "SELECT NAME, ID FROM BUSINESS_EVENTS";
                    res = CSxDBHelper.GetDictionary<string, int>(sql);
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while trying to read business events from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                return res;
            }
        }

        public static int StringToCurrency(string name)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "StringToCurrency start");
                int res = 0;
                try
                {
                    string sql = "select STR_TO_DEVISE(:name) from dual";
                    OracleParameter parameter = new OracleParameter(":name", name);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                    res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while trying to get currency code: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                return res;
            }
        }

        #endregion

        #region Others

        public static List<string> ReadFile(string fileName)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                List<string> res = new List<string>();
                bool fileReadSuccess = true;
                if (!String.IsNullOrEmpty(fileName))
                {
                    string line;
                    try
                    {
                        System.IO.StreamReader file = new System.IO.StreamReader(@fileName);
                        if (File.Exists(@fileName))
                        {
                            while ((line = file.ReadLine()) != null)
                            {
                                if (!String.IsNullOrEmpty(line))
                                {
                                    string trimmedLine = line.Trim();
                                    if (!String.IsNullOrEmpty(trimmedLine))
                                    {
                                        res.Add(trimmedLine.ToUpper());
                                    }
                                }
                            }
                            file.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        fileReadSuccess = false;
                        logger.log(Severity.error, "Failed to read allowed external fund identifiers from file: " + @fileName + " . Exception: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                else
                {
                    fileReadSuccess = false;
                }
                if (fileReadSuccess)
                {
                    logger.log(Severity.debug, "Successfully read " + res.Count + " external fund identifiers from: " + @fileName);
                }
                return res;
            }
        }

        public static void LogFieldsToFile(ITicketMessage ticketMessage, string path, double duration = 0.0)
        {
            Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name);
            try
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    string output = "";
                    foreach (FieldId field in Enum.GetValues(typeof(FieldId)))
                    {
                        output += Enum.GetName(typeof(FieldId), field) + ":" + ticketMessage.getString(field) +";";
                    }
                    if (duration != 0)
                    {
                        output += "TimeSpan" + ":" + duration + ";";
                    }
                    sw.WriteLine(output);
                    logger.log(Severity.debug, "LogFieldsToFile finished!");
                }
            }
            catch (Exception e)
            {
                logger.log(Severity.warning, "Exception : " + e.Message);
            }
        }

        #endregion
    }
}

using sophis.utils;
using sophis.instrument;
using sophis.portfolio;
using sophis.backoffice_kernel;
using sophis.static_data;
using sophisTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using sophis.database;
using Oracle.DataAccess.Client;

namespace BoThai_POC.Source.PortfolioColumns
{
    public class BoThai_NettedCF_Column : sophis.portfolio.CSMPortfolioColumn
    {
        public int currency { get; set; }
        public string currencyStr { get; set; }
        public int term { get; set; }
        public string termStr { get; set; }
        public int allo { get; set; }

        public static string GetColumnName(string currencyName, string term)
        {
            return "NETTED CF " + currencyName + " " + term;
        }

        public BoThai_NettedCF_Column()
        {
        }

        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction,
                                            int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                            bool onlyTheValue)
        {
            try
            {
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Starting GetPositionCell");
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;

                double output = 0;
                SSMCellStyle style = new SSMCellStyle();
                SSMCellValue value = new SSMCellValue();

                //check if is FX Forward allotment
                CSMInstrument instr = CSMInstrument.GetInstance(instrumentCode);
                if (instr.GetAllotment() == allo)
                //if (instr.GetAllotment() == 21)
                {
                    //check if maturity grid/tenor is less than the maturity of the FX
                    int today = CSMDay.GetSystemDate();
                    int posMaturity = 0;
                    posMaturity = instr.GetExpiry();
                    int nbOfDays = posMaturity - today;
                    if (CheckExactTerm(nbOfDays, termStr, currency))
                    {
                        //check if it is the FX Forward CCY1 or CCY2
                        CSMPortfolioColumn.GetCSRPortfolioColumn("FX CCY1").GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref value, style, true);
                        string CCY1 = value.GetString();
                        CSMPortfolioColumn.GetCSRPortfolioColumn("FX CCY2").GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref value, style, true);
                        string CCY2 = value.GetString();
                        if (CCY1 == currencyStr)
                        {
                            //check notional of ccy1 of FX position
                            CSMPortfolioColumn.GetCSRPortfolioColumn("FX Nominal CCY1").GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref value, style, true);
                        }
                        else if (CCY2 == currencyStr)
                        {
                            //Workaround from <Ana.Ghiuzan@misys.com> to correctly display the value on compliance getting the right position id.
                            //TODO remove after correction of SRQ-60230
                            double test = -1;
                            CSMPortfolioColumn.GetCSRPortfolioColumn("FX Nominal CCY2").GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref value, style, true);

                            if (value.doubleValue == 0)
                            {
                                using (OracleCommand command = new OracleCommand() { Connection = Sophis.DataAccess.DBContext.Connection })
                                {
                                    try
                                    {
                                        command.CommandText = "SELECT POSITION_CASH_ID FROM POSITION_SETTLEMENT WHERE POSITION_ID = " + position.GetIdentifier();
                                        using (var reader = command.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                int posID = Convert.ToInt32(command.ExecuteScalar());
                                                CSMPosition pos = CSMPosition.GetCSRPosition(posID);
                                                test = pos.GetInventoryAmount();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                                                    MethodBase.GetCurrentMethod().Name,
                                                    CSMLog.eMVerbosity.M_debug,
                                                     "Exception thrown: " + ex.Message + "; Query: " + command.CommandText);
                                    }
                                }
                                value.doubleValue = -test;
                            }
                        }
                    }
                }
                if (!onlyTheValue)
                {
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    cellStyle.currency = currency;
                    var curr = CSMCurrency.GetCSRCurrency(currency);
                    if (curr != null)
                        curr.GetRGBColor(cellStyle.color);
                }
                output = value.doubleValue;

                cellValue.doubleValue = output;
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Ending GetPositionCell");
            }
            catch (Exception ex)
            {
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_error, "Exception :" + ex.Message);
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Exception :" + ex.StackTrace);
            }

        }

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                             sophis.portfolio.CSMExtraction extraction,
                                             ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                             bool onlyTheValue)
        {
            try
            {
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Starting GetPortfolioCell");
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                using (CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction))
                {
                    double TotalNetted = 0;

                    if (folio != null)
                    {
                        long nbPos = folio.GetTreeViewPositionCount();
                        SSMCellStyle style = new SSMCellStyle();
                        SSMCellValue value = new SSMCellValue();

                        for (int i = 0; i < nbPos; i++)
                        {
                            CSMPosition pos = folio.GetNthTreeViewPosition(i);
                            GetPositionCell(pos, activePortfolioCode, portfolioCode, extraction,
                                                            pos.GetInstrumentCode(), pos.GetInstrumentCode(), ref value, style, false);

                            CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                                            MethodBase.GetCurrentMethod().Name,
                                            CSMLog.eMVerbosity.M_debug, "Position ID: " + pos.GetIdentifier().ToString());
                            CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                                            MethodBase.GetCurrentMethod().Name,
                                            CSMLog.eMVerbosity.M_debug, "Position Value: " + value.doubleValue.ToString());
                            TotalNetted += value.doubleValue;
                        }
                    }

                    if (!onlyTheValue)
                    {
                        cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                        cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                        cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                        cellStyle.currency = currency;
                        var curr = CSMCurrency.GetCSRCurrency(currency);
                        if (curr != null)
                            curr.GetRGBColor(cellStyle.color);
                    }
                    cellValue.doubleValue = TotalNetted;
                }
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Ending GetPortfolioCell");
            }
            catch (Exception ex)
            {
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                MethodBase.GetCurrentMethod().Name,
                CSMLog.eMVerbosity.M_error, "Exception :" + ex.Message);
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Exception :" + ex.StackTrace);
            }

        }

        private bool CheckExactTerm(int daysToMat, string termStr, int ccy)
        {
            int today = CSMDay.GetSystemDate();
            CSMCalendar cal = CSMCalendar.New(ccy);

            switch (termStr)
            {
                case "3m":
                    if ((-1 < daysToMat) && (cal.AddNumberOfMonths(today, 3) - today > daysToMat)) return true; break;
                case "6m":
                    if ((cal.AddNumberOfMonths(today, 3) - today < daysToMat) && (cal.AddNumberOfMonths(today, 6) - today > daysToMat)) return true; break;
                case "9m":
                    if ((cal.AddNumberOfMonths(today, 6) - today < daysToMat) && (cal.AddNumberOfMonths(today, 9) - today > daysToMat)) return true; break;
                case "1y":
                    if ((cal.AddNumberOfMonths(today, 9) - today < daysToMat) && (cal.AddNumberOfYears(today, 1) - today > daysToMat)) return true; break;
                case "2y":
                    if ((cal.AddNumberOfYears(today, 1) - today < daysToMat) && (cal.AddNumberOfYears(today, 2) - today > daysToMat)) return true; break;
                case "3y":
                    if ((cal.AddNumberOfYears(today, 2) - today < daysToMat) && (cal.AddNumberOfYears(today, 3) - today > daysToMat)) return true; break;
                case "4y":
                    if ((cal.AddNumberOfYears(today, 3) - today < daysToMat) && (cal.AddNumberOfYears(today, 4 - today) > daysToMat)) return true; break;
                case "5y":
                    if ((cal.AddNumberOfYears(today, 4) - today < daysToMat) && (cal.AddNumberOfYears(today, 5) - today > daysToMat)) return true; break;
                default:
                    break;
            }

            //switch (term)
            //{
            //    case 90:
            //        if (term - daysToMat < term && daysToMat < cal.AddNumberOfMonths(today,3)) return true; break;
            //    case 180:
            //        if (term - daysToMat < term && daysToMat > 90) return true; break;
            //    case 270:
            //        if (term - daysToMat < term && daysToMat > 180) return true; break;
            //    case 365:
            //        if (term - daysToMat < term && daysToMat > 270) return true; break;
            //    case 730:
            //        if (term - daysToMat < term && daysToMat > 365) return true; break;
            //    case 1095:
            //        if (term - daysToMat < term && daysToMat > 730) return true; break;
            //    case 1461:
            //        if (term - daysToMat < term && daysToMat > 1095) return true; break;
            //    case 1826:
            //        if (term - daysToMat < term && daysToMat > 1461) return true; break;
            //}
            return false;
        }
    }
}

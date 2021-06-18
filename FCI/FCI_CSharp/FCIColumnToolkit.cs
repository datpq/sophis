using sophis.gui;
using sophis.instrument;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
using sophis.market_data;
using System;

namespace FCI_CSharp
{
    public class FCIColumnToolkit : CSMPortfolioColumn
    {
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode,
            CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            var logger = new CSMLog();
            logger.Begin("FCIColumnToolkit", "GetPositionCell");

            try
            {
                logger.Write(CSMLog.eMVerbosity.M_debug, $"position Id={position.GetIdentifier()}, activePortfolioCode={activePortfolioCode}, portfolioCode={portfolioCode}, instrumentCode={instrumentCode}");
                var instrument = position.GetCSRInstrument();
                if (instrument == null)
                {
                    logger.Write(CSMLog.eMVerbosity.M_error, "No instrument found");
                    return;
                }

                var instrumentType = GetPositionColumn<string>("Instrument type", position, activePortfolioCode, portfolioCode, underlyingCode, instrumentCode, onlyTheValue);
                logger.Write(CSMLog.eMVerbosity.M_debug, $"instrumentType={instrumentType}");

                CSMCurrency currency;
                if (instrumentType.Equals("Interest Rate Swaps", StringComparison.InvariantCultureIgnoreCase))
                {
                    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                    cellStyle.@decimal = 8;
                    cellStyle.currency = instrument.GetCurrency();
                    currency = CSMCurrency.GetCSRCurrency(cellStyle.currency);
                    currency.GetRGBColor(cellStyle.color); // instrument currency color
                    CSMSwap irs = instrument;
                    var cashFlow = irs.new_CashFlowDiagram(CSMMarketData.GetCurrentMarketData());
                    cellValue.doubleValue = cashFlow.GetPresentValue(0);
                    logger.Write(CSMLog.eMVerbosity.M_debug, $"doubleValue={cellValue.doubleValue}");
                }
                else if (instrumentType.Equals("Bonds", StringComparison.InvariantCultureIgnoreCase))
                {
                    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                    cellStyle.@decimal = 8;
                    cellStyle.currency = instrument.GetCurrency();
                    currency = CSMCurrency.GetCSRCurrency(cellStyle.currency);
                    currency.GetRGBColor(cellStyle.color); // instrument currency color
                    cellValue.doubleValue = instrument.GetNotionalRate() * instrument.GetNotional();
                    logger.Write(CSMLog.eMVerbosity.M_debug, $"doubleValue={cellValue.doubleValue}");
                }
                else if (instrumentType.Equals("Shares", StringComparison.InvariantCultureIgnoreCase))
                {
                    cellStyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                    cellStyle.color = new SSMRgbColor
                    {
                        green = 0,
                        blue = 0,
                        red = 0
                    }; // black color;
                    var isin = instrument.GetExternalReference(); // make sure ISIN is setup properly in External and Universal Reference
                    cellValue.SetString(isin);
                    logger.Write(CSMLog.eMVerbosity.M_debug, $"stringValue={isin}");
                }
                else // all other instrument types
                {
                    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                    cellStyle.@decimal = 8;
                    cellStyle.currency = CSMPreference.GetCurrency();
                    currency = CSMCurrency.GetCSRCurrency(cellStyle.currency);
                    currency.GetRGBColor(cellStyle.color); // Global currency color
                    cellValue.doubleValue = GetPositionColumn<double>("Theoretical", position, activePortfolioCode, portfolioCode, underlyingCode, instrumentCode, onlyTheValue);
                    logger.Write(CSMLog.eMVerbosity.M_debug, $"doubleValue={cellValue.doubleValue}");
                }
            }
            catch (Exception e)
            {
                logger.Write(CSMLog.eMVerbosity.M_error, "error: " + e.Message);
                logger.Write(CSMLog.eMVerbosity.M_error, e.StackTrace);
            }
            finally
            {
                logger.End();
            }
        }

        public override short GetDefaultWidth()
        {
            return 8;
        }

        private T GetPositionColumn<T>(string columnName , CSMPosition position, int activePortfolioCode, int portfolioCode, int underlyingCode,
            int instrumentCode, bool onlyTheValue)
        {
            var cellValue = new SSMCellValue();
            var cellStyle = new SSMCellStyle();

            var returnType = typeof(T);
            if (returnType == typeof(double))
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
            }
            else if (returnType == typeof(string))
            {
                cellStyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;
            } else
            {
                throw new ArgumentException($"Type {returnType} is not supported");
            }

            var col = GetCSRPortfolioColumn(columnName);

            if (col != null)
            {
                col.GetPositionCell(position, activePortfolioCode, portfolioCode, null, underlyingCode, instrumentCode,
                    ref cellValue, cellStyle, onlyTheValue);
                if (returnType == typeof(double))
                {
                    return (T)Convert.ChangeType(cellValue.doubleValue, typeof(T));
                }
                else if (returnType == typeof(string))
                {
                    return (T)Convert.ChangeType(cellValue.GetString(), typeof(T));
                }
                else return default(T);
            }
            else return default(T);
        }
    }
}

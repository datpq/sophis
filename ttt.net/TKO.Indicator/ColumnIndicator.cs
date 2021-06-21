using System;
using System.Collections.Generic;
using System.Linq;
using NSREnums;
using sophis;
using sophis.market_data;
using sophis.portfolio;
using sophis.utils;

namespace TKO.Indicator
{
    class TkoIndicatorName
    {
        public string IndicatorName { get; set; }
        public string IndicatorDisplay { get; set; }
        public string DataType { get; set; }
        public int DecimalPrecision { get; set; }
    }

    class TkoIndicator
    {
        public double ValueNumber { get; set; }
        public string ValueText { get; set; }
    }

    public class ColumnIndicator : CSMCachedPortfolioColumn
    //public class ColumnIndicator : CSMPortfolioColumn
    {
        private static readonly Dictionary<string, TkoIndicatorName> _allIndicatorNames = new Dictionary<string, TkoIndicatorName>();
        private static readonly Dictionary<string, TkoIndicator> _allIndicators = new Dictionary<string, TkoIndicator>();

        private static int _currentDate; // 0 if today, otherwise is the current date
        private readonly string _indicatorName;
        private static DateTime _initializationTimeout = DateTime.MinValue;

        public ColumnIndicator(string indicatorName) : base(indicatorName, true, true, true, true, true)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Write(CSMLog.eMVerbosity.M_info, string.Format("Constructing ColumnIndicator {0}", indicatorName));
                _indicatorName = indicatorName;
                if (!_allIndicators.Any())
                {
                    Initialize();
                }
            }
        }

        public override CMString GetGroup()
        {
            return "TKO_INDICATOR";
        }

        public static void Register()
        {
            using (CSMLog logger = new CSMLog())
            {
                try
                {
                    logger.Write(CSMLog.eMVerbosity.M_info, "Register.BEGIN");
                    using (var cmd = Sophis.DataAccess.DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT INDICATOR_NAME, INDICATOR_DISPLAY, DATA_TYPE, DECIMAL_PRECISION FROM TKO_INDICATOR_NAME WHERE IS_ENABLED = 1";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var indicatorName = reader.GetString(0);
                                var indicatorDisplay = reader.IsDBNull(1) ? indicatorName : reader.GetString(1);
                                logger.Write(CSMLog.eMVerbosity.M_info, string.Format("Registering Indicator {0}...", indicatorName));
                                Register(indicatorDisplay, new ColumnIndicator(indicatorName));
                                _allIndicatorNames.Add(indicatorName, new TkoIndicatorName
                                {
                                    IndicatorName = indicatorName,
                                    IndicatorDisplay = indicatorDisplay,
                                    DataType = reader.GetString(2),
                                    DecimalPrecision = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Write(CSMLog.eMVerbosity.M_error, e.ToString());
                }
                finally
                {
                    logger.Write(CSMLog.eMVerbosity.M_info, "Register.END");
                }
            }
        }

        public static void Initialize()
        {
            if (_initializationTimeout > DateTime.Now) return;
            using (CSMLog logger = new CSMLog())
            {
                try
                {
                    logger.Write(CSMLog.eMVerbosity.M_info, "Initialize.BEGIN");
                    using (var cmd = Sophis.DataAccess.DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = _currentDate == 0
                            ? "SELECT T.INDICATOR_NAME, T.INSTRUMENT_ID, T.VALUE_NUMBER, T.VALUE_TEXT FROM TKO_INDICATOR T\n" +
                              "JOIN TKO_INDICATOR_NAME N ON T.INDICATOR_NAME = N.INDICATOR_NAME AND N.IS_ENABLED = 1"
                            : string.Format("WITH T AS (\n" +
                                            "SELECT INDICATOR_NAME, INSTRUMENT_ID, VALUE_NUMBER, VALUE_TEXT, ROW_NUMBER() OVER\n" +
                                            "(PARTITION BY INDICATOR_NAME, INSTRUMENT_ID ORDER BY TKO_INDICATOR_AUDIT_ID DESC) AS ROW_NUMBER\n" +
                                            "FROM TKO_INDICATOR_AUDIT\n" +
                                            "WHERE DATEVAL <= NUM_TO_DATE({0}))\n" +
                                            "SELECT T.INDICATOR_NAME, T.INSTRUMENT_ID, T.VALUE_NUMBER, T.VALUE_TEXT FROM T\n" +
                                            "JOIN TKO_INDICATOR_NAME N ON T.INDICATOR_NAME = N.INDICATOR_NAME AND N.IS_ENABLED = 1 AND T.ROW_NUMBER = 1",
                                _currentDate);
                        using (var reader = cmd.ExecuteReader())
                        {
                            _allIndicators.Clear();
                            while (reader.Read())
                            {
                                _allIndicators.Add(string.Format("{0}@{1}", reader.GetString(0), reader.GetInt32(1)),
                                    new TkoIndicator
                                    {
                                        ValueNumber = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                                        ValueText = reader.IsDBNull(3) ? null : reader.GetString(3)
                                    });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Write(CSMLog.eMVerbosity.M_error, e.ToString());
                }
                finally
                {
                    logger.Write(CSMLog.eMVerbosity.M_info,
                        string.Format("Initialize.END(IndicatorCount={0})", _allIndicators.Count));
                }
            }
            _initializationTimeout = DateTime.Now.AddMilliseconds(10);
        }

        //public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode, CSMExtraction extraction, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        //{
        //    using (CSMLog logger = new CSMLog())
        //    {
        //        logger.Write(CSMLog.eMVerbosity.M_info, string.Format("GetPortfolioCell instrumentCode={0}", 1));
        //    }
        //}

        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            using (CSMLog logger = new CSMLog())
            {
                var currentTimeContext = CSMCurrentTimeContext.Get();
                var currentDate = 0;
                if (currentTimeContext != 0)
                {
                    var marketData = CSMMarketData.GetCurrentMarketData();
                    currentDate = marketData.GetDate();
                }

                if (currentDate != _currentDate)
                {
                    logger.Write(CSMLog.eMVerbosity.M_info,
                        string.Format("CurrentDate switched from {0} to {1}", _currentDate, currentDate));
                    _currentDate = currentDate;
                    Initialize();
                }

                var indicatorKey = string.Format("{0}@{1}", _indicatorName, instrumentCode);
                //logger.Write(CSMLog.eMVerbosity.M_info, string.Format("GetPositionCell.BEGIN(indicatorKey={0})", indicatorKey));
                ComputeIndicator(indicatorKey, ref cellValue, cellStyle);
                //logger.Write(CSMLog.eMVerbosity.M_info, "GetPositionCell.END");
            }
        }

        //public override void ComputePortfolioCell(SSMCellKey key, ref SSMCellValue value, SSMCellStyle style)
        //{
        //    using (CSMLog logger = new CSMLog())
        //    {
        //        string indicatorKey = string.Format("{0}@{1}", _indicatorName, key.InstrumentCode());
        //        logger.Write(CSMLog.eMVerbosity.M_info, string.Format("ComputePortfolioCell.BEGIN(indicatorKey={0}, Name={1})",
        //            indicatorKey, key.InstrumentName()));
        //        ComputeIndicator(indicatorKey, ref value, style);
        //        logger.Write(CSMLog.eMVerbosity.M_info, "ComputePortfolioCell.END");
        //    }
        //}

        //public override void ComputePositionCell(SSMCellKey key, ref SSMCellValue value, SSMCellStyle style)
        //{
        //    using (CSMLog logger = new CSMLog())
        //    {
        //        string indicatorKey = string.Format("{0}@{1}", _indicatorName, key.InstrumentCode());
        //        logger.Write(CSMLog.eMVerbosity.M_info, string.Format("ComputePositionCell.BEGIN(indicatorKey={0}, Name={1})",
        //            indicatorKey, key.InstrumentName()));
        //        ComputeIndicator(indicatorKey, ref value, style);
        //        logger.Write(CSMLog.eMVerbosity.M_info, "ComputePositionCell.END");
        //    }
        //}

        private void ComputeIndicator(string indicatorKey, ref SSMCellValue value, SSMCellStyle style)
        {
            using (CSMLog logger = new CSMLog())
            {
                TkoIndicator indicatorVal;
                if (_allIndicators.TryGetValue(indicatorKey, out indicatorVal))
                {
                    var tkoIndicator = _allIndicatorNames[_indicatorName];
                    switch (tkoIndicator.DataType)
                    {
                        case "D":
                            value.doubleValue = indicatorVal.ValueNumber;
                            style.kind = NSREnums.eMDataType.M_dDouble;
                            style.@decimal = tkoIndicator.DecimalPrecision;
                            style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                            break;
                        case "T":
                            value.FromString(eMDataType.M_dNullTerminatedString, indicatorVal.ValueText);
                            style.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                            style.alignment = sophis.gui.eMAlignmentType.M_aLeft;
                            break;
                        default:
                            logger.Write(CSMLog.eMVerbosity.M_error, string.Format("Type not supported: {0}", tkoIndicator.DataType));
                            break;
                    }
                }
            }
        }

        public override bool InvalidationNeeded(ref MEvent.MType @event)
        {
            using (CSMLog logger = new CSMLog())
            {
                var result = @event == MEvent.MType.M_InstrumentUpdate;
                logger.Write(CSMLog.eMVerbosity.M_info, string.Format("InvalidationNeeded(@event={0}), result={1}, _currentDate={2}",
                    @event.ToString(), result, _currentDate));
                if (result && _currentDate == 0) Initialize(); // when M_InstrumentUpdate and today reload from database
                return result;
            }
        }
    }
}

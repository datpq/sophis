using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

using sophis.portfolio;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;

using Eff.UpgradeUtilities;
using TkoPortfolioColumn.DbRequester;
using TkoPortfolioColumn.Sectors;
using TkoPortfolioColumn.Ratings;
using TkoPortfolioColumn.DataCache;
using sophisTools;
using System.Collections;
using sophis.collateral;


namespace TkoPortfolioColumn
{
    namespace CallBack
    {
        public class PortFolioColumnCallbacker : sophis.portfolio.CSMColumnConsolidate
        {
            public string columnName;
            private static readonly DataSourcePorfolioIndicators DataSource = new DataSourcePorfolioIndicators();
            private static readonly List<string> ColumnsList = new List<string>() { "TkoComputeGearing", "TkoComputeGlobalRisk", "TkoComputeGlobalRiskLevrage","TkoComputeAmfExposure" };
            //dirty to be change with a correct pattern.
            private static readonly List<string> ColumnsListStringValue = new List<string>() { "TkoHandleInstrumentSectorBySectorType", "TkoPerfAttribFlagPosition", "TkoPerfAttribFlagFolio" };

            private static readonly List<string> ColumnsListIntegerValue = new List<string>() { "TkoGet1stCallDate" };
            public delegate void SophisPortfolioConsolidation(int activePortfolioCode, int portfolioCode, CSMExtraction extraction, ref SSMCellValue value, SSMCellStyle style, bool onlyTheValue);


             /// <summary>
            /// Constructor of the class.
            /// By default, the column group is "Uncatalogued".
            public PortFolioColumnCallbacker()
            {
                this.fWithForex = true;
                this.fWithForexColor = true;
            }

            /// <summary>
            /// Constructor of the class.
            /// Use this to set the section of the column.
            /// It will be used in the new column browser.
            /// </summary>
            /// <param name="section"> the name of the column group </param>
            public PortFolioColumnCallbacker(string sectionName)
            {
                fGroup = sectionName;
            }

            #region Initialize 

            internal static void Initialize()
            {
                PortFolioColumnCallbacker.ActivateColumns();
                PortFolioColumnCallbacker.DesactivateColumns();
                PortFolioColumnCallbacker.LoadColumnsConfig();
                sophis.portfolio.CSMColumnConsolidate.Refresh();
            }

            public static void LoadColumnsConfig()
            {
                DbrTikehau_Config.GetTikehauColumnConfig();
            }

            public static void ActivateColumns()
            {
                try
                {
                    var columnListToDesactivate = DbrTikehauPortFolioColumn.GetTikehauPortFolioColumnToActivate().Where(p => p.TOOLKIT == "TKO-SphOtherPortfolioColumns");
                    foreach (var col in columnListToDesactivate)
                    {
                        try
                        {
                            sophis.portfolio.CSMColumnConsolidate.Register(col.NAME, new PortFolioColumnCallbacker() { columnName = col.NAME, fGroup = col.COLUMNGROUP });
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    //CSMLog.Write("PortFolioColumnCallbacker", "ActivateColumn", CSMLog.eMVerbosity.M_warning, "Failed to Activate Tikehau PortFolioColumn [" + ex.Message + " ]");
                }

            }

            public static void DesactivateColumns()
            {
                try
                {
                    var columnListToDesactivate = DbrTikehauPortFolioColumn.GetTikehauPortFolioColumnToDesactivate();
                     foreach (var col in columnListToDesactivate)
                    {
                        sophis.portfolio.CSMColumnConsolidate.Erase(col.NAME);
                    }
                }
                catch (Exception ex)
                {
                    //CSMLog.Write("PortFolioColumnCallbacker", "DesactivateColumn", CSMLog.eMVerbosity.M_warning, "Failed to Activate Tikehau PortFolioColumn [" + ex.Message + " ]");
                }
            }

            #endregion 

            #region Computation Mode
            public static double ComputeIndicatorFromColName(InputProvider input, CSMPortfolio portfolio)
            {
                try
                {
                    var methods = DbrTikehauPortFolioColumn.GetTikehauColumnConfig(input.Column).PROVIDER.Split(',').ElementAt(0);
                    input.Methods = methods;
                    switch (methods)
                    {

                        //case "TkoComputeDurationValue":
                        //    return input.Instrument.TkoComputeDurationValue(input);
                        case "TkoComputeTKODurationIr":
                            return input.Instrument.TkoComputeTKODurationIr(input);
                        case "TkoComputeTreeYTMValue":
                            return input.Instrument.TkoComputeTreeYTMValue(input);
                        case "TkoComputeGearing":
                            return input.Instrument.TkoComputeGearing(input, portfolio);
                        case "TkoComputeGlobalRisk":
                            return input.Instrument.TkoComputeGlobalRisk(input);
                        case "TKOComputeReceivedCouponsLocalCCY":
                            return input.Instrument.TkoComputeReceivedCouponsLocalCCY(input);
                        case "TkoComputeReceivedCoupons":
                            return input.Instrument.TkoComputeReceivedCoupons(input);
                        case "TkoComputeDailyCarryCoupon":
                            return input.Instrument.TkoComputeDailyCarryCoupon(input);
                        case "TkoComputeDailyCarryAct":
                            return input.Instrument.TkoComputeDailyCarryAct(input);
                        case "TkoGet1stCallDate":
                            return input.Instrument.TkoGet1stCallDate(input);
                        case "TkoAvMonthNetReturn":
                            return input.Instrument.TkoAvMonthNetReturn(input, portfolio);
                        case "TkoStressPositionAssetValueByPrice":
                            return input.Position.TkoStressPositionAssetValueByPrice(input);
                        case "TkoComputeGlobalRiskLevrage":
                            return input.Instrument.TkoComputeGlobalRiskLevrage(input);
                        case "TkoComputeAmfExposure":
                            return input.Instrument.TkoComputeAmfExposure(input);
                        case "TkoHandleInstrumentSectorBySectorType":
                            return input.Instrument.TkoHandleInstrumentSectorBySectorType(input);
                        case "TKO Floating Rates":
                            return input.Instrument.TkoComputeFixedOrFloat(input);
                        case "TKO Inv Cash":
                            return input.Instrument.TkoComputeInvestedCash(input);
                        case "TKR Net Return Since Incep":
                            return input.Instrument.TkoNetReturnSinceInception(input);
                        case "TKR % of Positive Months":
                            return input.Instrument.TkoPositiveMonths(input);
                        case "TKO RatingComp":
                            return input.Instrument.TkoComputeRatingComp(input);
                        case "TkoGlobaDelta":
                            return input.Position.TkoGlobaDelta(input);
                        case "TkoPerfAttribFlagPosition":
                            return input.Position.TkoPerfAttribFlagPosition(input, portfolio);
                        case "TkoAvgPriceBase10Position":
                            return input.Position.TkoAvgPriceBase10Position(input);
                        //case "TKO Received Coupons CCY":
                        //    return input.Instrument.TkoComputeReceivedCouponsCCY(input);
                        default:
                            throw new NotImplementedException("Not Implemented column on toolkit TKO-SphOtherPortfolioColumns");
                    }
                }
                catch (Exception e)
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error,
                        string.Format("input={0}", input.ToString()));
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                    input.IndicatorValue = 0;
                    return 0;
                }
            }

            public static double ComputeIndicatorFromColNameWithPonderation(InputProvider input, SSMCellStyle cellStyle, CSMPortfolio portfolio)
            {
                try
                {
                    var methods = DbrTikehauPortFolioColumn.GetTikehauColumnConfig(input.Column).PROVIDER.Split(',').ElementAt(1);
                    input.Methods = methods;
                    switch (methods)
                    {
                        //case "TkoGetPondDurationCr":
                        //    return input.PortFolio.TkoGetPondDurationCr(input);
                        case "TkoGetPondDurationIr":
                            return portfolio.TkoGetPondDurationIr(input);
                        case "TkocGetPondYTM":
                            return portfolio.TkocGetPondYTM(input);
                        case "TkoGetPondGearing":
                            return portfolio.TkoGetPondGearing(input);
                        case "TkoGetPondGearingFixOption":
                            return portfolio.TkoGetPondGearingFixOption(input);
                        case "TkoGlobalRiskAmfPortFolioConsolidation":
                            return portfolio.TkoGlobalRiskAmfPortFolioConsolidation(input);
                        case "TkoGlobalRiskAmfPortFolioConsolidationWithoutBalance":
                            return portfolio.TkoGlobalRiskAmfPortFolioConsolidationWithoutBalance(input, cellStyle);
                        case "TkoTop3Positions":
                            return portfolio.TkoTop3Positions(input);
                        case "TkoTop5Positions":
                            return portfolio.TkoTop5Positions(input);
                        case "TkoGlobaDeltaConsolidate":
                            return portfolio.TkoGlobaDeltaConsolidate(input);
                        case "TkoPerfAttribFlagFolio":
                            return portfolio.TkoPerfAttribFlagFolio(input);
                        case "TKR Net Return Since Incep":
                            return input.Instrument.TkoNetReturnSinceInception(input);
                        case "TkoAvgPriceBase10Folio":
                            return 0;
                        default:
                            throw new NotImplementedException("Not Implemented column on toolkit TKO-SphOtherPortfolioColumns");

                    }
                }
                catch (Exception e)
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error,
                        string.Format("input={0}", input.ToString()));
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                    input.IndicatorValue = 0;
                    return 0;
                }
            }
            #endregion

            #region GetPositionCell
            public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle cellstyle, bool onlyTheValue)
            {
                try
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                        "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5}, columnName={6})",
                        activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier, columnName);
                    CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                    if (portfolio == null)
                        return;

                    var folioname = portfolio.GetName();
                    CheckExtractionMode(ref activePortfolioCode, ref portfolioCode, folioname, columnName);

                    cellstyle.kind = NSREnums.eMDataType.M_dDouble;
                    Value.doubleValue = 0.0;//get the portfolio from its code and the extraction

                    var inputProvider = new InputProvider(activePortfolioCode, portfolioCode, extraction, underlyingCode,
                                                           instrumentCode, positionType, positionIdentifier,
                                                           onlyTheValue, portfolio.GetName(), portfolio.GetRho(), columnName);

                    inputProvider.ReportingDate = extraction.GetLastReportingDate();
                    inputProvider.PortFolio = portfolio;
                    inputProvider.Methods = DbrTikehauPortFolioColumn.GetTikehauPositionMethodFromColName(inputProvider.Column);

                    if (inputProvider.Methods.ToUpper() == "NONE") return;
                    ThreadCallBackGetPositionCell(inputProvider, ref Value, cellstyle, portfolio);

                    //inputProvider.MarketDataDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));
                    inputProvider.SophisReportingDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(extraction.GetLastReportingDate()));
  
                }
                catch (Exception e)
                {
                    Value.doubleValue = 0;
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                }
                finally
                {
                    if (double.IsNaN(Value.doubleValue) || double.IsInfinity(Value.doubleValue))
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_warning, "NaN value replaced by Zero");
                        Value.doubleValue = 0;
                    }
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                }
            }

            public void ThreadCallBackGetPositionCell(Object input, ref SSMCellValue cellValue, SSMCellStyle cellstyle, CSMPortfolio portfolio)
            {
                try
                {
                    var inputProvider = input as InputProvider;
                    if (inputProvider == null)
                        return;

                    CSMPosition position;
                    if (inputProvider.PositionIdentifier == 0)
                    {
                        position = portfolio.GetFlatViewPosition(inputProvider.InstrumentCode, inputProvider.PositionType);
                        inputProvider.Position = position;
                        cellValue.doubleValue = 0;
                    }
                    else
                    {
                        //DPH
                        //position = inputProvider.PortFolio.GetTreeViewPosition(inputProvider.PositionIdentifier);
                        position = portfolio.GetTreeViewPosition(inputProvider.PositionIdentifier);
                        inputProvider.Instrument = CSMInstrument.GetInstance(inputProvider.InstrumentCode);
                        inputProvider.Position = position;
                        if (inputProvider.Position == null)
                            return;

                        if (VersionClass.CheckCacheVersion(inputProvider) ||
                            !DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition.ContainsKey(inputProvider.PositionIdentifier) ||
                            !DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition[inputProvider.PositionIdentifier].ContainsKey(inputProvider.Column)
                            )
                        {
                            if (ColumnsListStringValue.Contains(inputProvider.Methods))
                            {
                                ComputeIndicatorFromColName(inputProvider, portfolio);
                                if (inputProvider.StringIndicatorValue != null)
                                    cellValue.SetString(inputProvider.StringIndicatorValue);
                                DataSourcePorfolioIndicators.FillPositionCache(inputProvider);
                            }
                            else
                            {

                                double nominal = inputProvider.Position.GetInstrumentCount() * inputProvider.Instrument.GetNotional();
                                if (nominal != 0.0)
                                {
                                    cellValue.doubleValue = ComputeIndicatorFromColName(inputProvider, portfolio);
                                    inputProvider.IndicatorValue = cellValue.doubleValue;
                                    DataSourcePorfolioIndicators.FillPositionCache(inputProvider);
                                }
                                else
                                {
                                    if (ColumnsList.Contains(inputProvider.Methods))
                                    {
                                        cellValue.doubleValue = ComputeIndicatorFromColName(inputProvider, portfolio);
                                        inputProvider.IndicatorValue = cellValue.doubleValue;
                                        DataSourcePorfolioIndicators.FillPositionCache(inputProvider);
                                    }
                                    else
                                    {
                                        cellValue.doubleValue = 0.0;
                                        inputProvider.IndicatorValue = cellValue.doubleValue;
                                        DataSourcePorfolioIndicators.FillPositionCache(inputProvider);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ColumnsListStringValue.Contains(inputProvider.Methods))
                            {
                                var cellValuestr = DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition[inputProvider.PositionIdentifier][inputProvider.Column].StringIndicatorValue;
                                if (cellValuestr == null) cellValuestr = "";
                                cellValue.SetString(cellValuestr);
                            }
                            else if (ColumnsListIntegerValue.Contains(inputProvider.Methods))
                            {
                                cellValue.integerValue = (int)DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition[inputProvider.PositionIdentifier][inputProvider.Column].IndicatorValue;
                            }
                            else
                            {
                                cellValue.doubleValue = DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition[inputProvider.PositionIdentifier][inputProvider.Column].IndicatorValue;
                            }          
                        }
                    }

                    if (inputProvider.Position == null)
                        return;

                    if (!inputProvider.OnlyTheValue) SetPositionSophisCellStyle(cellstyle, inputProvider);
                }
                catch (Exception e)
                {
                    cellValue.doubleValue = 0;
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                }
            }
            #endregion

            #region GetPortfolioCell
            public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                  sophis.portfolio.CSMExtraction extraction,
                                  ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                  bool onlyTheValue)
            {
                try
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, columnName={3})",
                        activePortfolioCode, portfolioCode, extraction.GetModelName(), columnName);
                    // get the portfolio from its code and the extraction
                    CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                    if (portfolio == null)
                        return;

                    var folioname = portfolio.GetName();
                    CheckExtractionMode(ref activePortfolioCode, ref portfolioCode, folioname, columnName);


                    int positionNumber = portfolio.GetTreeViewPositionCount();
                    if (positionNumber == 0 && portfolio.GetChildCount() == 0)
                        return;

                    var inputProvider = new InputProvider(activePortfolioCode, portfolioCode, extraction,
                                                           onlyTheValue, portfolio, folioname, portfolio.GetRho(), columnName);
                    //Bug Fix tkoc-kiwf.dll (RepportingDate != MarketDataDate)
                    //inputProvider.ReportingDate = CSMMarketData.GetCurrentMarketData().GetDate();
                    inputProvider.ReportingDate = extraction.GetLastReportingDate();
                    
                    inputProvider.MarketDataDate = String.Format("{0:dd/MM/yyyy}",  Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));
                    inputProvider.PortFolio = portfolio;
                    inputProvider.delegateFolioConsolidation = base.GetPortfolioCell;

                    inputProvider.Methods = DbrTikehauPortFolioColumn.GetTikehauPortFolioMethodFromColName(inputProvider.Column);
                    if (inputProvider.Methods.ToUpper() == "NONE") return;
                    inputProvider.IndicatorValue = 0.0;

                   
                    ThreadCallBackGetPortFolioCell(inputProvider, ref cellValue, cellStyle, portfolio);
                    inputProvider.SophisReportingDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(extraction.GetLastReportingDate()));
                }
                catch (Exception e)
                {
                    cellValue.doubleValue = 0;
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                }
                finally
                {
                    if (double.IsNaN(cellValue.doubleValue) || double.IsInfinity(cellValue.doubleValue))
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_warning, "NaN value replaced by Zero");
                        cellValue.doubleValue = 0;
                    }
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                }
            }

            private static void CheckExtractionMode(ref int activePortfolioCode, ref int portfolioCode, 
                                                    CMString folioname,string column)
            {
                if (column == "TKO Forex Exposure")
                {
                    CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(portfolioCode);
                    if (folio == null)
                    {
                        //bug on extraction mode.
                        var res = DbrPortFolio.RetrieveFolioCodeFromFolioName(folioname);
                        if (res.Count > 1)
                        {
                            //you have to find a strong key.
                            //For the moment we keep the input value.
                        }
                        else
                        {
                            if (res.Count == 1)
                            {
                                portfolioCode = res.First().ident;
                                activePortfolioCode = res.First().mgr;
                            }
                        }
                    }
                }
            }

            private void ThreadCallBackGetPortFolioCell(Object input, ref SSMCellValue cellValue, SSMCellStyle cellStyle, CSMPortfolio portfolio)
            {
                try
                {
                    cellValue.doubleValue = 0.0;
                    var inputProvider = input as InputProvider;
                    if (inputProvider == null)
                        return;

                    if (VersionClass.CheckCacheVersion(inputProvider) || !DataSourcePorfolioIndicators.DataCacheIndiactorValueByFolio.ContainsKey(inputProvider.PortFolioCode)
                        || !DataSourcePorfolioIndicators.DataCacheIndiactorValueByFolio[inputProvider.PortFolioCode].ContainsKey(inputProvider.Column)
                       )
                    {
                        if (ColumnsListStringValue.Contains(inputProvider.Methods))
                        {
                            ComputeIndicatorFromColNameWithPonderation(inputProvider, cellStyle, portfolio);
                            if (inputProvider.StringIndicatorValue != null)
                                cellValue.SetString(inputProvider.StringIndicatorValue);
                            DataSourcePorfolioIndicators.FillFolioCache(inputProvider);
                        }
                        else
                        {
                            cellValue.doubleValue = ComputeIndicatorFromColNameWithPonderation(inputProvider, cellStyle, portfolio);
                            DataSourcePorfolioIndicators.FillFolioCache(inputProvider);
                        }
                    }
                    else
                    {
                        if (ColumnsListStringValue.Contains(inputProvider.Methods))
                        {
                            var cellValuestr = DataSourcePorfolioIndicators.DataCacheIndiactorValueByFolio[inputProvider.PortFolioCode][inputProvider.Column].StringIndicatorValue;
                            if (cellValuestr == null) cellValuestr = "";
                            cellValue.SetString(cellValuestr);
                        }
                        else
                        {
                            cellValue.doubleValue = DataSourcePorfolioIndicators.DataCacheIndiactorValueByFolio[inputProvider.PortFolioCode][inputProvider.Column].IndicatorValue;
                        }  
                       
                    }

                    if (!inputProvider.OnlyTheValue)
                    {
                        SetPortFolioSophisCellStyle(cellStyle, inputProvider, portfolio);
                    }
                }
                catch (Exception e)
                {
                    cellValue.doubleValue = 0;
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                }
            }
            #endregion 

            #region Cell Styles

            public static void SetPositionSophisCellStyle(SSMCellStyle cellStyle,InputProvider input)
            {
                //Set Configuration in DataBase => colname|ComputeMethod.
                switch (input.Methods)
                {
                    case "TkoComputeGearing":
                        SetSophisCellStyle3(cellStyle,input);
                        break;
                    case "TkoComputeTreeYTMValue":
                        SetSophisCellStyle5(cellStyle,input);
                        break;
                    case "TkoComputeDurationValue":
                        SetFolioCrAndIrDurationCellStyle(cellStyle, input);
                        break;
                    case "TkoComputeGlobalRisk":
                        SetPosistionSophisCellStyleGlobalAMF(cellStyle,input);
                        break;
                    case "TkoComputeGlobalRiskLevrage":
                        SetPosistionSophisCellStyleGlobalAMF(cellStyle, input);
                        break;
                    case "TkoHandleInstrumentSectorBySectorType":
                        SetStringIndicatorCellStyle(cellStyle, input);
                        break;
                    case "TkoPerfAttribFlagPosition" :
                        SetStringIndicatorCellStyle(cellStyle, input);
                        break;
                    case "TkoGet1stCallDate" :
                        SetSophisCellStyle2(cellStyle, input);
                        break;
                    default:
                        SetSophisCellStyle2(cellStyle,input);
                        break;

                }
            }

            public static void SetPortFolioSophisCellStyle(SSMCellStyle cellStyle, InputProvider input, CSMPortfolio portfolio)
            {
                //Set Configuration in DataBase => colname|ComputeMethod.
                switch (input.Methods)
                {
                    case "TkoGetPondGearing":
                        SetSophisCellStyle4(cellStyle,input);
                        break;
                    case "TkoGetPondDurationCr":
                        SetFolioCrAndIrDurationCellStyle(cellStyle,input);
                        break;
                    case "TkoGlobalRiskAmfPortFolioConsolidation":
                        SetFolioSophisCellStyleGlobalAMF(cellStyle,input, portfolio);
                        break;
                    case "TkoComputeGlobalRiskLevrage":
                        SetFolioSophisCellStyleGlobalAMF(cellStyle,input, portfolio);
                        break;
                    case "TkoPerfAttribFlagFolio" :
                        SetStringIndicatorCellStyle(cellStyle, input);
                        break;
                    default:
                        SetSophisCellStyle1(cellStyle,input, portfolio);
                        break;
                }
            }

            private static void SetSophisCellStyle1(SSMCellStyle cellStyle, InputProvider input, CSMPortfolio portfolio)
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                cellStyle.currency = portfolio.GetCurrency();
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                //cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 16384;
                col.blue = 32768;
                col.red = 0;
                cellStyle.color = col;
            }

            private static void SetFolioSophisCellStyleGlobalAMF(SSMCellStyle cellStyle, InputProvider input, CSMPortfolio portfolio)
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                cellStyle.currency = portfolio.GetCurrency();
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                //cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 16384;
                col.blue = 32768;
                col.red = 0;
                cellStyle.color = col;
            }

            private static void SetSophisCellStyle2(SSMCellStyle cellStyle, InputProvider input)
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                cellStyle.currency = input.Position.GetCurrency();
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                //cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                col.green = 16384;
                col.blue = 32768;
                col.red = 0;
                cellStyle.color = col;
            }

            private static void SetPosistionSophisCellStyleGlobalAMF(SSMCellStyle cellStyle,InputProvider input)
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.currency = input.Position.GetCurrency();
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZero;
                //cellStyle.currency = input.Position.GetCurrency();
                col.green = 16384;
                col.blue = 32768;
                col.red = 0;
                cellStyle.color = col;
            }


            private static void SetSophisCellStyle3(SSMCellStyle cellStyle, InputProvider input)
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                //cellStyle.currency = input.Position.GetCurrency();
                col.red = 0;
                col.green = 16384;
                col.blue = 32768;
                cellStyle.color = col;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
            }

            private static void SetSophisCellStyle4(SSMCellStyle cellStyle, InputProvider input)
            {
                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                //cellStyle.currency = input.PortFolio.GetCurrency();
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 16384;
                col.blue = 32768;
                col.red = 0;
                cellStyle.color = col;
            }

            private static void SetSophisCellStyle5(SSMCellStyle cellStyle, InputProvider input)
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                cellStyle.currency = input.Position.GetCurrency();
                //cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                col.red = 0;
                col.green = 16384;
                col.blue = 32768;
                cellStyle.color = col;
            }

            private static void SetFolioCrAndIrDurationCellStyle(SSMCellStyle cellStyle, InputProvider input)
            {
                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                //cellStyle.currency = input.PortFolio.GetCurrency();
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 16384;
                col.blue = 32768;
                col.red = 0;
                cellStyle.color = col;
            }


            private static void SetStringIndicatorCellStyle(SSMCellStyle cellStyle, InputProvider input)
            {
                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                //cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;
                //cellStyle.@null = sophis.gui.eMNullValueType.M_nvUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 102;
                col.blue = 0;
                col.red = 255;
                cellStyle.color = col;
            }

            private static void SetIntToDateIndicatorCellStyle(SSMCellStyle cellStyle, InputProvider input)
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDate;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
            }
            #endregion
        }
    }
}

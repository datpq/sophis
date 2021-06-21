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
using TkoPortfolioColumn.DataCache;
using sophisTools;
using System.Collections;
using sophis.collateral;
using sophis.value;


namespace TkoPortfolioColumn
{
    namespace CallBack
    {
        public class PortFolioColumnCallbacker : sophis.portfolio.CSMColumnConsolidate
        {
            public string columnName;
            private static readonly DataSourcePorfolioIndicators DataSource = new DataSourcePorfolioIndicators();
           
            //dirty to be change with a correct pattern.
            private static readonly List<string> ColumnsList = new List<string>() { "TkoComputeGearing", "TkoComputeGlobalRisk", "TkoComputeGlobalRiskLevrage", "TkoComputeAmfExposure" };
            private static readonly List<string> ColumnsListStringValue = new List<string>() { "TkoHandleInstrumentSectorBySectorType", "TkoPerfAttribFlagPosition", "TkoPerfAttribFlagFolio" };
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
                    var columnListToDesactivate = DbrTikehauPortFolioColumn.GetTikehauPortFolioColumnToActivate().Where(p => p.TOOLKIT == "TKO-SphPerfAttrib");
                    foreach (var col in columnListToDesactivate)
                    {
                        try
                        {
                            sophis.portfolio.CSMColumnConsolidate.Register(col.NAME, new PortFolioColumnCallbacker() { columnName = col.NAME, fGroup = col.COLUMNGROUP });
                        }
                        catch (Exception ex)
                        {
                            CSMLog.Write("PortFolioColumnCallbacker", "ActivateColumn", CSMLog.eMVerbosity.M_warning, "Failed to Activate Tikehau PortFolioColumn [" + ex.Message + " ]");
                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("PortFolioColumnCallbacker", "ActivateColumn", CSMLog.eMVerbosity.M_warning, "Failed to Activate Tikehau PortFolioColumn [" + ex.Message + " ]");
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
                    CSMLog.Write("PortFolioColumnCallbacker", "DesactivateColumn", CSMLog.eMVerbosity.M_warning, "Failed to Activate Tikehau PortFolioColumn [" + ex.Message + " ]");
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
                        case "TkoHandleInstrumentSectorBySectorType":
                            return input.Instrument.TkoHandleInstrumentSectorBySectorType(input);
                        case "TkoPerfAttribFlagPosition":
                            return input.Position.TkoPerfAttribFlagPosition(input, portfolio);
                        case "TkoPerfAttribFindPositionFolio":
                            return input.Position.TkoPerfAttribFindPositionFolio(input);
                        default:
                            throw new NotImplementedException("Not Implemented colonne implemented on tkoc_kiwf.dll");
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

            public static double ComputeIndicatorFromColNameWithPonderation(InputProvider input, CSMPortfolio portfolio)
            {
                try
                {
                    var methods = DbrTikehauPortFolioColumn.GetTikehauColumnConfig(input.Column).PROVIDER.Split(',').ElementAt(1);
                    input.Methods = methods;
                    switch (methods)
                    {
                        case "TkoPerfAttribFlagFolio":
                            return portfolio.TkoPerfAttribFlagFolio(input);
                        case "TkoPerfAttribFolioName":
                            return portfolio.TkoPerfAttribFolioName(input);
                        default:
                            throw new NotImplementedException("Not Implemented colonne implemented on tkoc_kiwf.dll");
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

                    //TKO Strategy problem. portfolioCode doesn't have a correct value when filtered by Fund. position.GetPortfolio() gives the correct value of portfolioCode
                    //CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                    CSMPortfolio portfolio = CSMPosition.GetCSRPosition(positionIdentifier).GetPortfolio();
                    if (portfolio == null)
                        return;

                    var folioname = portfolio.GetName();

                    cellstyle.kind = NSREnums.eMDataType.M_dDouble;
                    Value.doubleValue = 0.0;//get the portfolio from its code and the extraction

                    var inputProvider = new InputProvider(activePortfolioCode, portfolioCode, extraction, underlyingCode,
                                                           instrumentCode, positionType, positionIdentifier, 
                                                           onlyTheValue, portfolio.GetName(), portfolio.GetRho(), columnName);
                    inputProvider.PortFolio = portfolio;
                    //CSMAmFund fund = CSMAmFund.GetFundFromFolio(inputProvider.PortFolio);
                    CSMAmFund fund = CSMAmFund.GetFundFromFolio(portfolio);
                    //if (fund != null && !inputProvider.PortFolio.IsAPortfolio())
                    if (fund != null && !portfolio.IsAPortfolio())
                    {
                        inputProvider.PortFolioCode = fund.GetTradingPortfolio();
                    }

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

                        var identifier = position.GetIdentifier();
                        if (identifier > 0)
                        {
                            var ret = DbrHistomvts.RetrievePortfolioCodeWithPositionIdentifier(identifier);
                            if (ret > 0)
                            {
                                inputProvider.PortFolioCode = ret;
                                portfolio = CSMPortfolio.GetCSRPortfolio(inputProvider.PortFolioCode);
                                inputProvider.PortFolio = portfolio;
                            }
                            else
                            {
                            }
                        }

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

                    int positionNumber = portfolio.GetTreeViewPositionCount();
                    if (positionNumber == 0 && portfolio.GetChildCount() == 0)
                        return;

                    var inputProvider = new InputProvider(activePortfolioCode, portfolioCode, extraction,
                                                           onlyTheValue, portfolio, folioname, portfolio.GetRho(), columnName);

                    CSMAmFund fund = CSMAmFund.GetFundFromFolio(portfolio);
                    if (fund != null && !portfolio.IsAPortfolio()) inputProvider.PortFolioCode = fund.GetTradingPortfolio();
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

            private void ThreadCallBackGetPortFolioCell(Object input, ref SSMCellValue cellValue, SSMCellStyle cellStyle, CSMPortfolio portfolio)
            {
                try
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                    }
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
                            ComputeIndicatorFromColNameWithPonderation(inputProvider, portfolio);
                            if (inputProvider.StringIndicatorValue != null)
                                cellValue.SetString(inputProvider.StringIndicatorValue);
                            DataSourcePorfolioIndicators.FillFolioCache(inputProvider);
                        }
                        else
                        {
                            cellValue.doubleValue = ComputeIndicatorFromColNameWithPonderation(inputProvider, portfolio);
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
                finally
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                    }
                }
            }
            #endregion 

            #region Cell Styles

            public static void SetPositionSophisCellStyle(SSMCellStyle cellStyle,InputProvider input)
            {
                //Set Configuration in DataBase => colname|ComputeMethod.
                switch (input.Methods)
                {
                    case "TkoHandleInstrumentSectorBySectorType":
                        SetStringIndicatorCellStyle(cellStyle, input);
                        break;
                    case "TkoPerfAttribFlagPosition" :
                        SetStringIndicatorCellStyle(cellStyle, input);
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
            #endregion
        }
    }
}

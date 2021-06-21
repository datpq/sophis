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
            private static readonly List<string> ColumnsList = new List<string>() { "TkoComputeGlobalRisk", "TkoComputeGlobalRiskLevrage", "TkoComputeAmfExposure", "TkoForexForwardPaymentLeg", "TkoForexForwardReceivingLeg", "TkoForexForwardReceivedAndPaymentLegFxSpot" };
            public delegate void SophisPortfolioConsolidation(int activePortfolioCode, int portfolioCode, CSMExtraction extraction, ref SSMCellValue value, SSMCellStyle style, bool onlyTheValue);

            private static readonly List<string> ColumnsListStringValue = new List<string>() { "TkoForexForwardPaymentLegCurrency", "TkoForexForwardReceivedLegCurrency" };


            public PortFolioColumnCallbacker()
            {
                this.fWithForex = true;
                this.fWithForexColor = true;
            }

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
                    var columnListToDesactivate = DbrTikehauPortFolioColumn.GetTikehauPortFolioColumnToActivate().Where(p => p.TOOLKIT == "TKO-SphRisk");
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
            public static double ComputeIndicatorFromColName(InputProvider input)
            {
                try
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                    }
                    var methods = DbrTikehauPortFolioColumn.GetTikehauColumnConfig(input.Column).PROVIDER.Split(',').ElementAt(0);
                    input.Methods = methods;
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "methods={0}", methods);
                    }
                    switch (methods)
                    {
                        case "TkoComputeGlobalRisk":
                            return input.Instrument.TkoComputeGlobalRisk(input);
                        case "TkoGet1stCallDate":
                            return input.Instrument.TkoGet1stCallDate(input);
                        case "TkoComputeGlobalRiskLevrage":
                            return input.Instrument.TkoComputeGlobalRiskLevrage(input);
                        case "TkoComputeAmfExposure":
                            return input.Instrument.TkoComputeAmfExposure(input);
                        case "TKO Floating Rates":
                            return input.Instrument.TkoComputeFixedOrFloat(input);
                        case "TkoForexForwardPaymentLeg":
                            return input.Instrument.TkoForexForwardPaymentLeg(input);
                        case "TkoForexForwardReceivingLeg":
                            return input.Instrument.TkoForexForwardReceivingLeg(input);
                        case "TkoForexForwardPaymentLegCurrency":
                            return input.Instrument.TkoForexForwardPaymentLegCurrency(input);
                        case "TkoForexForwardReceivedLegCurrency":
                            return input.Instrument.TkoForexForwardReceivedLegCurrency(input);
                        case "TkoForexForwardReceivedAndPaymentLegFxSpot":
                            return input.Instrument.TkoForexForwardReceivedAndPaymentLegFxSpot(input);
                        default:
                            throw new NotImplementedException("Not Implemented colonne implemented on TKO-SphRisk");
                    }
                }
                catch (Exception e)
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                    input.IndicatorValue = 0;
                    return 0;
                }
                finally
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                    }
                }
            }

            public static double ComputeIndicatorFromColNameWithPonderation(InputProvider input, SSMCellStyle cellStyle, CSMPortfolio portfolio)
            {
                try
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                    }
                    var methods = DbrTikehauPortFolioColumn.GetTikehauColumnConfig(input.Column).PROVIDER.Split(',').ElementAt(1);
                    input.Methods = methods;
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "methods={0}", methods);
                    }
                    switch (methods)
                    {
                        case "TkoGlobalRiskAmfPortFolioConsolidation":
                            return portfolio.TkoGlobalRiskAmfPortFolioConsolidation(input);
                        case "TkoGlobalRiskAmfPortFolioConsolidationExposure":
                            return portfolio.TkoGlobalRiskAmfPortFolioConsolidationExposure(input);
                        case "TkoGlobalRiskAmfPortFolioConsolidationLevrage":
                            return portfolio.TkoGlobalRiskAmfPortFolioConsolidationLevrage(input);
                        case "TkoGlobalRiskAmfPortFolioConsolidationWithoutBalance":
                            return portfolio.TkoGlobalRiskAmfPortFolioConsolidationWithoutBalance(input, cellStyle);
                        case "TkoGlobaDeltaConsolidate":
                            return portfolio.TkoGlobaDeltaConsolidate(input);
                        default:
                            throw new NotImplementedException("Not Implemented colonne implemented on TKO-SphRisk");

                    }
                }
                catch (Exception e)
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                    input.IndicatorValue = 0;
                    return 0;
                }
                finally
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                    }
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
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                            "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5}, columnName={6})",
                            activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier, columnName);
                    }
                    CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                    if (portfolio == null)
                        return;

                    cellstyle.kind = NSREnums.eMDataType.M_dDouble;
                    Value.doubleValue = 0.0;//get the portfolio from its code and the extraction

                    var folioname = portfolio.GetName().StringValue;
                    var inputProvider = new InputProvider(activePortfolioCode, portfolioCode, extraction, underlyingCode,
                                                           instrumentCode, positionType, positionIdentifier,
                                                           onlyTheValue, portfolio.GetName(), portfolio.GetRho(), columnName);
                    inputProvider.PortFolio = portfolio;
                    //CSMAmFund fund = CSMAmFund.GetFundFromFolio(inputProvider.PortFolio);
                    CSMAmFund fund = CSMAmFund.GetFundFromFolio(portfolio);
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "fund={0}", fund.GetCode());
                    }
                    //if (fund != null && !inputProvider.PortFolio.IsAPortfolio())
                    if (fund != null && !portfolio.IsAPortfolio())
                    {
                        inputProvider.PortFolioCode = fund.GetTradingPortfolio();
                    }

                    inputProvider.ReportingDate = extraction.GetLastReportingDate();
                    //inputProvider.ReportingDate = CSMMarketData.GetCurrentMarketData().GetDate();
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
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                    }
                }
            }

            public void ThreadCallBackGetPositionCell(Object input, ref SSMCellValue cellValue, SSMCellStyle cellstyle, CSMPortfolio portfolio)
            {
                try
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                    }
                    var inputProvider = input as InputProvider;
                    if (inputProvider == null)
                        return;

                    CSMPosition position;
                    if (inputProvider.PositionIdentifier == 0)
                    {
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN GetFlatViewPosition(InstrumentCode={0}, PositionType={1})", inputProvider.InstrumentCode, inputProvider.PositionType);
                        }
                        position = portfolio.GetFlatViewPosition(inputProvider.InstrumentCode, inputProvider.PositionType);
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END GetFlatViewPosition");
                        }
                        inputProvider.Position = position;
                        inputProvider.PositionIdentifier = position.GetIdentifier();
                        if (inputProvider.PositionIdentifier == 0)
                        {
                            //dirty => we change the dictionnary with instrument code we hope that they will be no collision...
                            inputProvider.PositionIdentifier = inputProvider.InstrumentCode;
                        }
                    }
                    else
                    {
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN GetTreeViewPosition(PositionIdentifier={0})", inputProvider.PositionIdentifier);
                        }
                        position = portfolio.GetTreeViewPosition(inputProvider.PositionIdentifier);
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END GetTreeViewPosition");
                        }
                        inputProvider.Position = position;  
                    }

                    if (inputProvider.Position == null)
                        return;

                    inputProvider.Instrument = CSMInstrument.GetInstance(inputProvider.InstrumentCode);
                    string key = string.Format("{0}_{1}", inputProvider.PositionIdentifier, inputProvider.Column);
                    if (VersionClass.CheckCacheVersion(inputProvider) ||
                        !DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition.ContainsKey(key))
                    {
                        if (ColumnsListStringValue.Contains(inputProvider.Methods))
                        {
                            ComputeIndicatorFromColName(inputProvider);
                            if (inputProvider.StringIndicatorValue != null)
                                cellValue.SetString(inputProvider.StringIndicatorValue);
                            DataSourcePorfolioIndicators.FillPositionCache(inputProvider);
                        }
                        else
                        {

                            double nominal = inputProvider.Position.GetInstrumentCount() * inputProvider.Instrument.GetNotional();
                            if (nominal != 0.0)
                            {
                                cellValue.doubleValue = ComputeIndicatorFromColName(inputProvider);
                                inputProvider.IndicatorValue = cellValue.doubleValue;
                                DataSourcePorfolioIndicators.FillPositionCache(inputProvider);
                            }
                            else
                            {
                                if (ColumnsList.Contains(inputProvider.Methods))
                                {
                                    cellValue.doubleValue = ComputeIndicatorFromColName(inputProvider);
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
                            var cellValuestr = DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition[key].StringIndicatorValue;
                            if (cellValuestr == null) cellValuestr = "";
                            cellValue.SetString(cellValuestr);
                        }
                        else
                        {
                            cellValue.doubleValue = DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition[key].IndicatorValue;
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
                finally
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(input={0})", input.ToString());
                    }
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
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, columnName={3})",
                            activePortfolioCode, portfolioCode, extraction.GetModelName(), columnName);
                    }
                    // get the portfolio from its code and the extraction
                    CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                    if (portfolio == null)
                        return;

                    int positionNumber = portfolio.GetTreeViewPositionCount();
                    if (positionNumber == 0 && portfolio.GetChildCount() == 0)
                        return;

                    var folioname = portfolio.GetName().StringValue;
                    var inputProvider = new InputProvider(activePortfolioCode, portfolioCode, extraction,
                                                           onlyTheValue, portfolio, folioname, portfolio.GetRho(), columnName);

                    //CSMAmFund fund = CSMAmFund.GetFundFromFolio(inputProvider.PortFolio);
                    //if (fund != null && !inputProvider.PortFolio.IsAPortfolio()) inputProvider.PortFolioCode = fund.GetTradingPortfolio();
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
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                    }
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
                        cellValue.doubleValue = ComputeIndicatorFromColNameWithPonderation(inputProvider, cellStyle, portfolio);
                        DataSourcePorfolioIndicators.FillFolioCache(inputProvider);
                    }
                        else
                        {
                            cellValue.doubleValue = DataSourcePorfolioIndicators.DataCacheIndiactorValueByFolio[inputProvider.PortFolioCode][inputProvider.Column].IndicatorValue;
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
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(input={0})", input.ToString());
                    }
                }
            }
            #endregion  

            #region Cell Styles

            public static void SetPositionSophisCellStyle(SSMCellStyle cellStyle,InputProvider input)
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                }
                //Set Configuration in DataBase => colname|ComputeMethod.
                switch (input.Methods)
                {
                    case "TkoComputeGlobalRisk":
                        SetPosistionSophisCellStyleGlobalAMF(cellStyle,input);
                        break;
                    case "TkoComputeGlobalRiskLevrage":
                        SetPosistionSophisCellStyleGlobalAMF(cellStyle, input);
                        break;
                    case "TkoForexForwardPaymentLeg":
                        SetSophisCellStyle3(cellStyle, input);
                        break;
                    case "TkoForexForwardReceivingLeg":
                        SetSophisCellStyle3(cellStyle, input);
                        break;
                    case "TkoForexForwardReceivedAndPaymentLegFxSpot":
                        SetSophisCellStyle3(cellStyle, input);
                        break;
                    case "TkoForexForwardPaymentLegCurrency":
                        SetStringIndicatorCellStyle(cellStyle, input);
                        break;
                    case "TkoForexForwardReceivedLegCurrency":
                        SetStringIndicatorCellStyle(cellStyle, input);
                        break;
                    default:
                        SetSophisCellStyle2(cellStyle,input);
                        break;

                }
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(input={0})", input.ToString());
                }
            }

            public static void SetPortFolioSophisCellStyle(SSMCellStyle cellStyle, InputProvider input, CSMPortfolio portfolio)
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                }
                //Set Configuration in DataBase => colname|ComputeMethod.
                switch (input.Methods)
                {
                    case "TkoGlobalRiskAmfPortFolioConsolidation":
                        SetFolioSophisCellStyleGlobalAMF(cellStyle,input, portfolio);
                        break;
                    case "TkoComputeGlobalRiskLevrage":
                        SetFolioSophisCellStyleGlobalAMF(cellStyle,input, portfolio);
                        break;
                    case "TkoForexForwardPaymentLegCurrency" :
                        SetStringIndicatorCellStyle(cellStyle, input);
                        break;
                    case "TkoForexForwardReceivedLegCurrency" :
                        SetStringIndicatorCellStyle(cellStyle, input);
                        break;
                    default:
                        SetSophisCellStyle1(cellStyle,input, portfolio);
                        break;
                }
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(input={0})", input.ToString());
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

            private static void SetSophisCellStyle3(SSMCellStyle cellStyle, InputProvider input)
            {
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                cellStyle.currency = input.Position.GetCurrency();
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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

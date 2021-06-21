using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Eff.UpgradeUtilities;
using sophis;
using sophis.portfolio;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;
using sophis.utils;

namespace dnPortfolioColumn
{
    public class PC_tko_duration_taux : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceDurationTaux DataSource = DataSourceDurationTaux.GetInstance();

        /// <summary>
        /// Called by Risque to get the value to display in the position cell. The value should be
        /// added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        /// the value.
        /// </summary>
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                    "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier);
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // get the position
                CSMPosition position;
                if (positionIdentifier == 0) // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                else // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                if (position == null)
                    return;

                CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);

                // Set Value
                Value.doubleValue = DataSource.Get_tko_modDuration_taux(position, instrument);

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                      sophis.portfolio.CSMExtraction extraction,
                      ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                      bool onlyTheValue)
        {
            //e_tkodurationtaux:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;
                CSMInstrument instrument;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                double SumAssetValue = 0;
                double fxspot;
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        SumAssetValue += position.GetAssetValue() * fxspot;
                        cellValue.doubleValue += DataSource.Get_tko_modDuration_taux(position, instrument) * position.GetAssetValue() * fxspot;
                    }
                }
                cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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
    }
    
    public class PC_tko_duration_credit : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceDurationCredit DataSource = DataSourceDurationCredit.GetInstance();

        /// <summary>
        /// Called by Risque to get the value to display in the position cell. The value should be
        /// added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        /// the value.
        /// </summary>
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                    "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier);
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // get the position
                CSMPosition position;
                if (positionIdentifier == 0) // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                else // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                if (position == null)
                    return;

                CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);

                // Set Value
                Value.doubleValue = DataSource.Get_tko_modDuration_credit(position, instrument);

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                      sophis.portfolio.CSMExtraction extraction,
                      ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                      bool onlyTheValue)
        {
            //e_tkodurationcredit:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;
                CSMInstrument instrument;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                double fxspot;
                double SumAssetValue = 0;
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        SumAssetValue += position.GetAssetValue() * fxspot;
                        cellValue.doubleValue += DataSource.Get_tko_modDuration_credit(position, instrument) * position.GetAssetValue() * fxspot;
                    }
                }
                cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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
    }
    
    public class PC_tko_Riskduration_taux : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceRiskDurationTaux DataSource = DataSourceRiskDurationTaux.GetInstance();

        /// <summary>
        /// Called by Risque to get the value to display in the position cell. The value should be
        /// added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        /// the value.
        /// </summary>
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                    "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier);
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // get the position
                CSMPosition position;
                if (positionIdentifier == 0) // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                else // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                if (position == null)
                    return;

                CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);

                // Set Value
                Value.doubleValue = DataSource.Get_tko_modRiskDuration_taux(position, instrument);

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                          sophis.portfolio.CSMExtraction extraction,
                          ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                          bool onlyTheValue)
        {
            //e_tkordurationtaux:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;
                CSMInstrument instrument;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                double fxspot;
                double SumAssetValue = 0;
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        SumAssetValue += position.GetAssetValue() * fxspot;
                        cellValue.doubleValue += DataSource.Get_tko_modRiskDuration_taux(position, instrument) * position.GetAssetValue() * fxspot;
                    }
                }
                cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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
    }
    
    public class PC_tko_Riskduration_credit : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceRiskDurationCredit DataSource = DataSourceRiskDurationCredit.GetInstance();

        /// <summary>
        /// Called by Risque to get the value to display in the position cell. The value should be
        /// added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        /// the value.
        /// </summary>
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                    "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier);
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // get the position
                CSMPosition position;
                if (positionIdentifier == 0) // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                else // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                if (position == null)
                    return;

                CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);

                // Set Value
                Value.doubleValue = DataSource.Get_tko_modRiskDuration_credit(position, instrument);

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                      sophis.portfolio.CSMExtraction extraction,
                      ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                      bool onlyTheValue)
        {
            //e_rdurationcredit:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;
                CSMInstrument instrument;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                double fxspot;
                double SumAssetValue = 0;
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        SumAssetValue += position.GetAssetValue() * fxspot;
                        cellValue.doubleValue += DataSource.Get_tko_modRiskDuration_credit(position, instrument) * position.GetAssetValue() * fxspot;
                    }
                }
                cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 2;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
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
    }

    public class PC_DurationValue : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceDurationValue DataSource = DataSourceDurationValue.GetInstance();


        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                    "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier);
                style.kind = NSREnums.eMDataType.M_dDouble;
                Value.doubleValue = 0;//get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);

                }
                else
                {
                    // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    Value.doubleValue = DataSource.GetDurationValue(position, instrument);
                }
                if (position == null)
                    return;

                if (!onlyTheValue)
                {
                    //Set style

                    style.@decimal = 2;
                    style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    style.color = col;
                    style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                }
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


        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                   sophis.portfolio.CSMExtraction extraction,
                                   ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                   bool onlyTheValue)
        {
        //e_durvalue:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellValue.doubleValue = 0;
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;



                cellValue.doubleValue = FonctionAdd.GetPondDurationCr(portfolio, extraction, DataSource);

                if (!onlyTheValue)
                {

                    //Set style
                    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                    cellStyle.@decimal = 2;
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    cellStyle.color = col;
                }

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
    }

    public class PC_DurationValueir : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceDurationValueir DataSource = DataSourceDurationValueir.GetInstance();

        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            try
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                        "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5})",
                        activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode,
                        positionIdentifier);
                }

                style.kind = NSREnums.eMDataType.M_dDouble;
                Value.doubleValue = 0;
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                }
                else
                {
                    // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    Value.doubleValue = DataSource.GetDurationValueir(position, instrument);

                }
                if (position == null)
                    return;
                if (!onlyTheValue)
                {
                    //Set style

                    style.@decimal = 2;
                    style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    style.color = col;
                    style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;

                }
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


        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                   sophis.portfolio.CSMExtraction extraction,
                                   ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                   bool onlyTheValue)
        {
        //e_durvalueir:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellValue.doubleValue = 0;
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;



                cellValue.doubleValue = FonctionAdd.GetPondDurationIr(portfolio, extraction, DataSource);

                if (!onlyTheValue)
                {

                    //Set style
                    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                    cellStyle.@decimal = 2;
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    cellStyle.color = col;
                }

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
    }
    
    public class PC_DurationValueContrib : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceDurationValueContrib DataSource = DataSourceDurationValueContrib.GetInstance();


        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                    "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier);
                style.kind = NSREnums.eMDataType.M_dDouble;
                Value.doubleValue = 0;
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);

                }
                else
                {
                    // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    Value.doubleValue = DataSource.GetDurationValue(position, instrument);
                }
                if (position == null)
                    return;

                if (!onlyTheValue)
                {
                    //Set style

                    style.@decimal = 2;
                    style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    style.color = col;
                    style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                }
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


        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                   sophis.portfolio.CSMExtraction extraction,
                                   ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                   bool onlyTheValue)
        {
        //e_durvalue_2:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellValue.doubleValue = 0;
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                cellValue.doubleValue = FonctionAdd.GetPondGearingDurationCr(portfolio, extraction, DataSource);

                if (!onlyTheValue)
                {

                    //Set style
                    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                    cellStyle.@decimal = 2;
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    cellStyle.color = col;
                }

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
    }

    public class PC_DurationValueirContrib : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceDurationValueirContrib DataSource = DataSourceDurationValueirContrib.GetInstance();

        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                    "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2}, underlyingCode={3}, instrumentCode={4}, positionIdentifier={5})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName(), underlyingCode, instrumentCode, positionIdentifier);
                style.kind = NSREnums.eMDataType.M_dDouble;
                Value.doubleValue = 0;
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);

                }
                else
                {
                    // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    Value.doubleValue = DataSource.ComputeTKODurationIr(instrument, VersionClass.Get_ReportingDate(), position);

                }
                if (position == null)
                    return;
                if (!onlyTheValue)
                {
                    //Set style

                    style.@decimal = 2;
                    style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    style.color = col;
                    style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                }

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


        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                   sophis.portfolio.CSMExtraction extraction,
                                   ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                   bool onlyTheValue)
        {
        //e_durvalueir_2:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellValue.doubleValue = 0;
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                cellValue.doubleValue = FonctionAdd.GetPondGearingDurationIr(portfolio, extraction, DataSource);

                if (!onlyTheValue)
                {
                    //Set style
                    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                    cellStyle.@decimal = 2;
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    cellStyle.color = col;
                }
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
    }

   
}
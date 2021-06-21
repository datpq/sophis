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
    public class PC_OAS : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceOAS DataSource = DataSourceOAS.GetInstance();

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
                Value.doubleValue = DataSource.GetOAS(position, instrument);

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 1;
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

        //public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
        //                          sophis.portfolio.CSMExtraction extraction,
        //                          ref SSMCellValue cellValue, SSMCellStyle cellStyle,
        //                          bool onlyTheValue)
        //{
        //    // get the portfolio from its code and the extraction
        //    CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
        //    if (portfolio == null)
        //        return;

        //    CSMPosition position;
        //    CSMInstrument instrument;

        //    int positionNumber = portfolio.GetTreeViewPositionCount();
        //    double SumAssetValue = 0;
        //    for (int index = 0; index < positionNumber; index++)
        //    {
        //        position = portfolio.GetNthTreeViewPosition(index);
        //        if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
        //        {
        //            instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
        //            SumAssetValue += position.GetAssetValue();
        //            cellValue.doubleValue += DataSource.GetOAS(position, instrument) * position.GetAssetValue();
        //        }
        //    }
        //    cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
        //    if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

        //    //Set style
        //    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
        //    cellStyle.@decimal = 2;
        //    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
        //    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
        //    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
        //}

    }
    public class PC_ZSpread : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceZSpread DataSource = DataSourceZSpread.GetInstance();

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
                Value.doubleValue = DataSource.GetZSpread(position, instrument);

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 1;
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

        //public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
        //                          sophis.portfolio.CSMExtraction extraction,
        //                          ref SSMCellValue cellValue, SSMCellStyle cellStyle,
        //                          bool onlyTheValue)
        //{
        //    // get the portfolio from its code and the extraction
        //    CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
        //    if (portfolio == null)
        //        return;

        //    CSMPosition position;
        //    CSMInstrument instrument;

        //    int positionNumber = portfolio.GetTreeViewPositionCount();
        //    double SumAssetValue = 0;
        //    for (int index = 0; index < positionNumber; index++)
        //    {
        //        position = portfolio.GetNthTreeViewPosition(index);
        //        if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
        //        {
        //            instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
        //            SumAssetValue += position.GetAssetValue();
        //            cellValue.doubleValue += DataSource.GetZSpread(position, instrument) * position.GetAssetValue();
        //        }
        //    }
        //    cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
        //    if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

        //    //Set style
        //    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
        //    cellStyle.@decimal = 2;
        //    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
        //    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
        //    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
        //}

    }
    public class PC_1stCallDate : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSource1stCallDate DataSource = DataSource1stCallDate.GetInstance();

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
                Value.integerValue = DataSource.Get1stCallDate(position, instrument);

                // Set style
                style.kind = NSREnums.eMDataType.M_dDate;
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
    }
}


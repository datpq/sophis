using System;
using System.Collections;
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

    //Classe de la colonnes définissant l'issu de l'algo TKO

    public class PC_CarryAtInvDate : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceCarryAtInvDate DataSource = DataSourceCarryAtInvDate.GetInstance();

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
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                    Value.doubleValue = 0;
                }
                else
                {// We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    //Set Value
                    Value.doubleValue = DataSource.GetCarryAtInvDate(position, instrument);
                }
                if (position == null)
                    return;

                //Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                style.color = col;
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
            //e_carryatinvdate:
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
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        cellValue.doubleValue += DataSource.GetCarryAtInvDate(position, instrument);
                    }
                }
                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 0;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                cellStyle.color = col;
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
    
    public class PC_YtmAtInvDate : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceYtmAtInvDate DataSource = DataSourceYtmAtInvDate.GetInstance();
        private static DataSourceInvestedCash DataSourceInvCash = DataSourceInvestedCash.GetInstance();

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
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                    Value.doubleValue = 0;
                }
                else
                {// We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    //Set Value
                    Value.doubleValue = DataSource.GetInvYtm(position, instrument);
                }
                if (position == null)
                    return;

                //Set style
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


        //L'agrégation sur le portfeuille est la moyenne pondérée par le montant investi
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                              sophis.portfolio.CSMExtraction extraction,
                                              ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                              bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                //e_YtmAtInvDate:
                /////try
                /////{
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;
                CSMInstrument instrument;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                double SumInvCash = 0;
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        SumInvCash += DataSourceInvCash.GetInvestedCash(position, instrument);
                        cellValue.doubleValue += DataSource.GetInvYtm(position, instrument) * DataSourceInvCash.GetInvestedCash(position, instrument);
                    }
                }
                cellValue.doubleValue = cellValue.doubleValue / SumInvCash;
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
    
    public class PC_DCarryAct : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceDCarryActuarial DataSource = DataSourceDCarryActuarial.GetInstance();

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
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                    Value.doubleValue = 0;
                }
                else
                {// We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    //Set Value
                    Value.doubleValue = DataSource.GetDCarryActuarial(position, instrument);
                }
                if (position == null)
                    return;

                //Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                style.color = col;
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
            //e_dcarryact:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                //set the cell's display data type to double and its value to 0.
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellValue.doubleValue = 0;

                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;
                CSMInstrument instrument;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        cellValue.doubleValue += DataSource.GetDCarryActuarial(position, instrument);
                    }
                }
                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 0;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                cellStyle.color = col;
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
    
    public class PC_DCarryCoupon : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceDCarryCoupon DataSource = DataSourceDCarryCoupon.GetInstance();

        ///<summary>
        ////Called by Risque to get the value to display in the position cell. The value should be
        ////added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        ////the value.
        ///</summary>
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
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                    Value.doubleValue = 0;
                }
                else
                {// We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    //Set Value
                    Value.doubleValue = DataSource.GetDCarryCoupon(position, instrument);
                }
                if (position == null)
                    return;

                //Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                style.color = col;
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
            //e_dcarrycoupon:
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

                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        cellValue.doubleValue += DataSource.GetDCarryCoupon(position, instrument);
                    }
                }
                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 0;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                cellStyle.color = col;
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

    public class PC_Ytm : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceYtm DataSource = DataSourceYtm.GetInstance();

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
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                    Value.doubleValue = 0;
                }
                else
                {// We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    //Set Value
                    Value.doubleValue = 0;
                    Value.doubleValue = DataSource.GetYtm(position, instrument);
                }
                if (position == null)
                    return;

                //Set style
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
        
            //rl On est obligés de rejouer l'exception car sinon Sophis plante et n'arrive pas à dessiner la colonne
            //Soit c'est un bug, soit c'est le manque de temps - le calcul étant lourd sur le portefeuille
            //e_ytm:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                cellValue.doubleValue = 0;
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;
                CSMInstrument instrument;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                double fxspot = 0;
                double SumAssetValue = 0;
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        SumAssetValue += position.GetAssetValue() * fxspot;
                        cellValue.doubleValue += DataSource.GetYtm(position, instrument) * position.GetAssetValue() * fxspot;
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

    public class PC_Ytm_test : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceYtmTest DataSource = DataSourceYtmTest.GetInstance();

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
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                    Value.doubleValue = 0;
                }
                else
                {// We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    //Set Value
                    Value.doubleValue = 0;
                    Value.doubleValue = DataSource.GetYtm(position, instrument);
                }
                if (position == null)
                    return;

                //Set style
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
            //if (portfolioCode == 18649)
            //{
                //rl On est obligés de rejouer l'exception car sinon Sophis plante et n'arrive pas à dessiner la colonne
                //Soit c'est un bug, soit c'est le manque de temps - le calcul étant lourd sur le portefeuille
                //e_ytm:
                    try
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                            activePortfolioCode, portfolioCode, extraction.GetModelName());
                        // get the portfolio from its code and the extraction
                        cellValue.doubleValue = 0;
                        CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                        if (portfolio == null)
                            return;

                        CSMPosition position;
                        CSMInstrument instrument;

                        int positionNumber = portfolio.GetTreeViewPositionCount();
                        double fxspot = 0;
            
                        //Méthode de Newton
                        double x = 0; //le taux YTM recherché 
                        double eps = 1000; //la marge d'erreur
                        int count = 0;
                        double f_x = 0;
                        double fp_x = 0;

                        while (Math.Abs(eps) > 0.005 && count < 1000)
                        {
                            f_x = 0;
                            fp_x = 0;

                            for (int index = 0; index < positionNumber; index++)
                            {

                                position = portfolio.GetNthTreeViewPosition(index);
                                try
                                {
                                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                                    {
                                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                                        fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                                        f_x += position.GetAssetValue() * fxspot * DataSource.getNewtonValues(position, instrument, x)[0];
                                        fp_x += position.GetAssetValue() * fxspot * DataSource.getNewtonValues(position, instrument, x)[1];
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Hello");
                                }
                            }

                            x = x - f_x / fp_x;
                            eps = f_x;
                            count++;

                            //cellValue.doubleValue += DataSource.GetYtm(position, instrument) * position.GetAssetValue() * fxspot;
                            //cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
                        }
                        cellValue.doubleValue = x*100; //on multiplie par 100 pour avoir un resultat en %
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

    public class PC_AYtmTRI : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceAYtmTRI DataSource = DataSourceAYtmTRI.GetInstance();

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                              sophis.portfolio.CSMExtraction extraction,
                                              ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                              bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                cellValue.doubleValue = DataSource.GetYtmAgregate(portfolio);
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
    
    public class PC_AYtmApprox : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceAYtmApprox DataSource = DataSourceAYtmApprox.GetInstance();

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                              sophis.portfolio.CSMExtraction extraction,
                                              ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                              bool onlyTheValue)
        {
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                //YTM approché
                double ytmportfolioEstimate = 0;
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
                        ytmportfolioEstimate += DataSourceYtm.GetInstance().GetYtm(position, instrument) * position.GetAssetValue() * fxspot;
                    }
                }
                ytmportfolioEstimate = ytmportfolioEstimate / SumAssetValue / 100;
                //CSMApi.Log("ytm Estimation" + ytmportfolioEstimate, true);

                cellValue.doubleValue = DataSource.GetYtmAYtmApprox(portfolio, ytmportfolioEstimate);
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
   
    public class PC_XXYTMPercentNAV : sophis.portfolio.CSMColumnConsolidate
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                              sophis.portfolio.CSMExtraction extraction,
                                              ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                              bool onlyTheValue)
        {
            try {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                CSMPosition position;
                CSMInstrument instrument;
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
                        cellValue.doubleValue += DataSourceYtm.GetInstance().GetYtm(position, instrument) * position.GetAssetValue() * fxspot;
                    }
                }

                cellValue.doubleValue = cellValue.doubleValue / portfolio.GetNetAssetValue();//(SumAssetValue);
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

    public class PC_Ytm_Algo : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceTreeFindYT DataSource = DataSourceTreeFindYT.GetInstance();

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
                //get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;
                style.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                //get the position
                CSMPosition position;
                if (positionIdentifier == 0)
                { // We are in flat view
                    position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
                    Value.SetString("");
                }
                else
                {// We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);

                    //Set Value
                    //  Value.SetString(FonctionAdd.GetValuefromSophisString(position,instrument,"Allotment"));
                    Value.SetString(DataSource.GetTreeYtm(position, instrument));

                }
                if (position == null)
                    return;

                //Set style

                style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                style.color = col;
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
        //e_d:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);

                if (portfolio == null)
                    return;

                //Set style
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                cellStyle.color = col;
                cellValue.SetString("");
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

    /*
    public class PC_Ytm_AlgoValue : sophis.portfolio.CSMColumnConsolidate
    {

        private DataSourceTreeFindYTValue DataSource = DataSourceTreeFindYTValue.GetInstance();
        
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
           
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
                    Value.doubleValue = 0;
                    // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    Value.doubleValue = DataSource.GetTreeYtmValue(position, instrument);
                }
                if (position == null)
                    return;

                if (!onlyTheValue)
                {
                    //Set style
                    style.kind = NSREnums.eMDataType.M_dDouble;
                    //style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    style.@decimal = 2;
                    style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    style.color = col;
                }

            
        }

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                   sophis.portfolio.CSMExtraction extraction,
                                   ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                   bool onlyTheValue)
        {


        //e_ytmvalueerror:
            try
            {
                
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                cellValue.doubleValue = FonctionAdd.GetPondYTM(portfolio, DataSource);
                
                if (!onlyTheValue)
                {
                    //Set style
                    cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                    cellStyle.@decimal = 2;
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                    //cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    cellStyle.color = col;
                }


            }
            catch (Exception e)
            {
                CSMLog.Write("PC_Ytm_AlgoValue", "GetPortfolioCell", CSMLog.eMVerbosity.M_warning, "exception when computing Daily Carry Actuarial at portfolio level");
                //goto e_ytmvalueerror;
            }
            
        }

     }

    */
    public class PC_Ytm_AlgoValueContrib : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private DataSourceTreeFindYTValueContrib DataSource = DataSourceTreeFindYTValueContrib.GetInstance();

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
                    Value.doubleValue = 0;
                    // We are in hierchical view
                    position = portfolio.GetTreeViewPosition(positionIdentifier);
                    CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);
                    Value.doubleValue = DataSource.GetTreeYtmValue(position, instrument);
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
                }
            }
            catch(Exception e)
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
        //e_ytmvalueerror:
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


                cellValue.doubleValue = FonctionAdd.GetPondGearingYTM(portfolio, extraction, DataSource);

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

    public class PC_DateWorkOut : sophis.portfolio.CSMColumnConsolidate
    {
        //Get instance of the DataSource singleton
        private static DataSourceDateWorkOut DataSource = DataSourceDateWorkOut.GetInstance();

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
                    Value.doubleValue = DataSource.GetDateWorkOut(position, instrument);
                    Value.doubleValue = Value.doubleValue / 365;
                }
                if (position == null)
                    return;

                //Set style

                style.@decimal = 3;
                style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                style.color = col;
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
        //e_dateoutvalue:
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
             

                    CSMPosition position;
                    CSMInstrument instrument;
                    double coeffsum = 0;
                    double sumasset = 0;
                    double fxspot = 0;

                    int positionNumber = portfolio.GetTreeViewPositionCount();
                    for (int index = 0; index < positionNumber; index++)
                    {
                        position = portfolio.GetNthTreeViewPosition(index);
                        if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                        {
                            fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                            instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                            coeffsum += DataSource.GetDateWorkOut(position, instrument) * fxspot * position.GetAssetValue();
                            sumasset += position.GetAssetValue() * fxspot;
                        }
                    }

                    cellValue.doubleValue = coeffsum / sumasset;
                    cellValue.doubleValue = cellValue.doubleValue / 365;
                    if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }
                    //Set style
                    cellStyle.@decimal = 3;
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    cellStyle.color = col;
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

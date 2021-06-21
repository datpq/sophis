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
    public class PC_Internal_Gearing : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private  DataSourceInternalGearing DataSource = DataSourceInternalGearing.GetInstance();

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
                    Value.doubleValue = ComputeInternalGearing(position, instrument);

                }
                if (position == null)
                    return;

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 0;
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
        //e_internal_gearing:
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
                        cellValue.doubleValue += ComputeInternalGearing(position, instrument);
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

        public double ComputeInternalGearing(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double gearing = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                double assetvalue;
                double nominal;
                int PositionMvtident = PositionPtr.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "S"://Swap
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        if (nominal > 0)//acheteur de protection
                        {
                            sophis.static_data.eMDayCountBasisType DayCountBasisType = InstrumentPtr.GetMarketAIDayCountBasisType();
                            double RemainingTime = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, InstrumentPtr.GetExpiry());
                            CSMSwap Swap = CSMSwap.GetInstance(InstrumentPtr.GetCode());
                            CSMFixedLeg FixedLeg;
                            if (Swap.GetLegFlowType(0).ToString() == "M_lfCredit")//jmbe crédit
                            {
                                FixedLeg = Swap.GetLeg(1);
                            }
                            else//Jambe fixe
                            {
                                FixedLeg = Swap.GetLeg(0);
                            }
                            gearing = nominal * FixedLeg.GetFixedRate() * RemainingTime;

                        }
                        else { gearing = Math.Abs(nominal) + assetvalue; }//vendeur de protection

                        break;

                    case "P"://Repo
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "A"://Actions
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "Z"://Fund
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "F"://Futures
                        if (FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Instrument type") == "Index Futures")
                        {
                            assetvalue = PositionPtr.GetDeltaCash() * fxspot;
                            gearing = assetvalue;
                        }
                        else
                        {
                            assetvalue = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                            gearing = assetvalue;
                        }
                        break;

                    case "C"://Cash,fees
                        gearing = 0; //aucune exposition cash, on pourrait faire le CDS de la banque !!!
                        break;

                    case "E"://Forex
                        gearing = 0; //à revoir
                        break;

                    case "T"://Billets de treso
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();

                        /*
                        //différencier les différents cas ( Convertibles ou Options)
                        //Si Convertibles Bond
                        if (FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Allotment") == "Convertibles")
                        {
                            
                        }
                        else if //Si Options
                        {
                            CSMOption Option = CSMOption.GetInstance(InstrumentPtr.GetCode());
                            
                        }
                        else 
                        {
                            
                        }


                        
                        
                        */

                        if (FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Allotment") == "OTC Stock Derivatives" ||
                            FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Allotment") == "Listed Options" )
                        {



                            gearing = Math.Abs(PositionPtr.GetDeltaCash()) * fxspot;
                        }
                        else
                        {
                            if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                            {
                                assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                            else//option
                            {
                                assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                        }
                        break;

                    default:
                        break;
                }
                return gearing;
            }
            catch (Exception)
            {
                CSMLog.Write("", "", CSMLog.eMVerbosity.M_warning, "gearing cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
    }

    public class PC_Gearing : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private DataSourceGearing DataSource = DataSourceGearing.GetInstance();

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
                    Value.doubleValue = ComputeGearing(position, instrument);

                }
                if (position == null)
                    return;

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 0;
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
            //e_gearing:
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
                        cellValue.doubleValue += ComputeGearing(position, instrument);
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

        public double ComputeGearing(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double gearing = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                double assetvalue;
                double nominal;
                int PositionMvtident = PositionPtr.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "S"://Swap
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        gearing = Math.Abs(nominal) + assetvalue; 

                        break;

                    case "P"://Repo
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "A"://Actions
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "Z"://Fund
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "F"://Futures
                        if (FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Instrument type") == "Index Futures")
                        {
                            assetvalue = PositionPtr.GetDeltaCash() * fxspot;
                            gearing = assetvalue;
                        }
                        else
                        {
                            assetvalue = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                            gearing = assetvalue;
                        }
                        break;

                    case "C"://Cash,fees
                        gearing = 0; //aucune exposition cash, on pourrait faire le CDS de la banque !!!
                        break;

                    case "E"://Forex
                        gearing = 0; //à revoir
                        break;

                    case "T"://Billets de treso
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();

                        if (FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Allotment") == "OTC Stock Derivatives" ||
                             FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Allotment") == "Listed Options")
                        {
                            gearing = Math.Abs(PositionPtr.GetDeltaCash() * fxspot);
                        }
                        else
                        {
                            if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                            {
                                assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                            else//option
                            {
                                assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                        }
                        break;

                    default:
                        break;
                }
                return gearing;
            }
            catch (Exception)
            {
                CSMLog.Write("", "", CSMLog.eMVerbosity.M_warning, "gearing cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
    }
    
    public class PC_ImpliedCDS : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceImplCDS DataSource = DataSourceImplCDS.GetInstance();

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
                Value.doubleValue = DataSource.GetImplCDS(position, instrument);

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
            //e_implcds:
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
                        cellValue.doubleValue += DataSource.GetImplCDS(position, instrument) * position.GetAssetValue() * fxspot;
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
    
    public class PC_Basis : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceBasis DataSource = DataSourceBasis.GetInstance();

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
                Value.doubleValue = DataSource.GetBasis(position, instrument);

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
            //e_basis:
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
                        cellValue.doubleValue += DataSource.GetBasis(position, instrument) * position.GetAssetValue() * fxspot;
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
    
    public class PC_DefaultProba : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceDefaultProb DataSource = DataSourceDefaultProb.GetInstance();

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
                Value.doubleValue = DataSource.GetDefaultProb(position, instrument);

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
            //e_defaultproba:
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
                        cellValue.doubleValue += DataSource.GetDefaultProb(position, instrument) * position.GetAssetValue() * fxspot;
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

    ///////colonne Rating Comp letter

    // Fonction de création de la colonne RatingCompLetter
    public class PC_RatingCompLetter : sophis.portfolio.CSMColumnConsolidate
    {


        // Get instance of the DataSource singleton
        private static DataSourceRatingComp DataSource = DataSourceRatingComp.GetInstance();

        /// <summary>
        /// Called by Risque to get the value to display in the position cell. The value should be
        /// added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        /// the value
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
                MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
                style.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                // get the portfolio from its code and the extraction
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
                    Value.SetString(IMarketIndic.DefineRating(DataSource.GetRatingComp(position, instrument)));
                }
                if (position == null)
                    return;

                //Set style

                style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                style.style = sophis.gui.eMTextStyleType.M_tsBold;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                style.color = col;

                IMarketIndic.Close();
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
        //e_ratecompletter:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;
                if (portfolio.GetChildCount() == 0)
                {
                    MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
                    CSMPosition position;
                    CSMInstrument instrument;

                    int positionNumber = portfolio.GetTreeViewPositionCount();
                    double ratecalc = 0;
                    double SumAssetValue = 0;
                    double fxspot = 0;
                    for (int index = 0; index < positionNumber; index++)
                    {
                        position = portfolio.GetNthTreeViewPosition(index);
                        if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                        {
                            instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                            ratecalc = DataSource.GetRatingComp(position, instrument);

                            if (ratecalc > 0)
                            {
                                fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                                cellValue.doubleValue += ratecalc * position.GetAssetValue() * fxspot;
                                SumAssetValue += position.GetAssetValue() * fxspot;
                            }
                        }

                    }

                    cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
                    if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }
                    cellStyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                    cellValue.SetString(IMarketIndic.DefineRating(cellValue.doubleValue));
                    cellStyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    cellStyle.color = col;

                    IMarketIndic.Close();
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

    // Fonction de création de la colonne RatingComp 
    public class PC_RatingComp : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceRatingComp DataSource = DataSourceRatingComp.GetInstance();

        /// <summary>
        /// Called by Risque to get the value to display in the position cell. The value should be
        /// added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        /// the value
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
                    Value.doubleValue = DataSource.GetRatingComp(position, instrument);

                }
                if (position == null)
                    return;

                // Set style
                if (!onlyTheValue)
                {
                    style.kind = NSREnums.eMDataType.M_dDouble;
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
        //e_ratecomp:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;


                if (portfolio.GetChildCount() == 0)
                {
                    
                    CSMPosition position;
                    CSMInstrument instrument;

                    int positionNumber = portfolio.GetTreeViewPositionCount();
                    double ratecalc = 0;
                    double sumcoeff = 0;
                    double SumAssetValue = 0;
                    double fxspot = 0;

                    for (int index = 0; index < positionNumber; index++)
                    {

                        position = portfolio.GetNthTreeViewPosition(index);
                        if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                        {
                            instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                            ratecalc = DataSource.GetRatingComp(position, instrument);

                            if (ratecalc > 0 && instrument.GetInstrumentType() == 'O')
                            {
                                fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                                sumcoeff += ratecalc * position.GetAssetValue() * fxspot;
                                SumAssetValue += position.GetAssetValue() * fxspot;
                            }
                        }
                    }


                    cellValue.doubleValue = sumcoeff / SumAssetValue;
                    if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                    //Set style
                    if (!onlyTheValue)
                    {
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
                else return;
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

    // Fonction de création de la colonne RatingCompLetter
    public class PC_RatingSecondCompLetter : sophis.portfolio.CSMColumnConsolidate
    {


        // Get instance of the DataSource singleton
        private static DataSourceRatingSecondComp DataSource = DataSourceRatingSecondComp.GetInstance();

        /// <summary>
        /// Called by Risque to get the value to display in the position cell. The value should be
        /// added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        /// the value
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
                MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
                style.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                // get the portfolio from its code and the extraction
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
                    Value.SetString(IMarketIndic.DefineRating(DataSource.GetRatingComp(position, instrument)));
                }
                if (position == null)
                    return;

                //Set style

                style.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                style.style = sophis.gui.eMTextStyleType.M_tsBold;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                style.color = col;

                IMarketIndic.Close();
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
        //e_ratecompletter:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;
                if (portfolio.GetChildCount() == 0)
                {
                    MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
                    CSMPosition position;
                    CSMInstrument instrument;

                    int positionNumber = portfolio.GetTreeViewPositionCount();
                    double ratecalc = 0;
                    double SumAssetValue = 0;
                    double fxspot = 0;
                    for (int index = 0; index < positionNumber; index++)
                    {
                        position = portfolio.GetNthTreeViewPosition(index);
                        if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                        {
                            instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                            ratecalc = DataSource.GetRatingComp(position, instrument);

                            if (ratecalc > 0)
                            {
                                fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                                cellValue.doubleValue += ratecalc * position.GetAssetValue() * fxspot;
                                SumAssetValue += position.GetAssetValue() * fxspot;
                            }
                        }

                    }

                    cellValue.doubleValue = cellValue.doubleValue / SumAssetValue;
                    if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }
                    cellStyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                    cellValue.SetString(IMarketIndic.DefineRating(cellValue.doubleValue));
                    cellStyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aCenter;
                    cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                    col.green = 0;
                    col.blue = 65000;
                    col.red = 0;
                    cellStyle.color = col;

                    IMarketIndic.Close();
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

    // Fonction de création de la colonne RatingComp 
    public class PC_RatingSecondComp : sophis.portfolio.CSMColumnConsolidate
    {
        // Get instance of the DataSource singleton
        private static DataSourceRatingSecondComp DataSource = DataSourceRatingSecondComp.GetInstance();

        /// <summary>
        /// Called by Risque to get the value to display in the position cell. The value should be
        /// added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        /// the value
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
                    Value.doubleValue = DataSource.GetRatingComp(position, instrument);

                }
                if (position == null)
                    return;

                // Set style
                if (!onlyTheValue)
                {
                    style.kind = NSREnums.eMDataType.M_dDouble;
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
        //e_ratecomp:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;


                if (portfolio.GetChildCount() == 0)
                {

                    CSMPosition position;
                    CSMInstrument instrument;

                    int positionNumber = portfolio.GetTreeViewPositionCount();
                    double ratecalc = 0;
                    double sumcoeff = 0;
                    double SumAssetValue = 0;
                    double fxspot = 0;

                    for (int index = 0; index < positionNumber; index++)
                    {

                        position = portfolio.GetNthTreeViewPosition(index);
                        if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                        {
                            instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                            ratecalc = DataSource.GetRatingComp(position, instrument);

                            if (ratecalc > 0 && instrument.GetInstrumentType() == 'O')
                            {
                                fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                                sumcoeff += ratecalc * position.GetAssetValue() * fxspot;
                                SumAssetValue += position.GetAssetValue() * fxspot;
                            }
                        }
                    }


                    cellValue.doubleValue = sumcoeff / SumAssetValue;
                    if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                    //Set style
                    if (!onlyTheValue)
                    {
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
                else return;
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

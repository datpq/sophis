using System;
using System.Collections.Generic;
using System.Text;

using sophis;
using sophis.portfolio;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;

namespace dnPortfolioColumn
{
    public class PC_Coupon : sophis.portfolio.CSMPortfolioColumn
    {
        //Get instance of the DataSource singleton
        private static DataSourceCoupon DataSource = DataSourceCoupon.GetInstance();

        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            //get the portfolio from its code and the extraction
            CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
            if (portfolio == null)
                return;

            //get the position
            CSMPosition position;
            if (positionIdentifier == 0) // We are in flat view
                position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
            else // We are in hierchical view
                position = portfolio.GetTreeViewPosition(positionIdentifier);
            if (position == null)
                return;

            CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);

            //Set Value
            Value.doubleValue = DataSource.GetCoupon(position, instrument);

            //Set style
            style.kind = NSREnums.eMDataType.M_dDouble;
            style.@decimal = 3;
            style.alignment = sophis.gui.eMAlignmentType.M_aRight;
            style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
        }

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                      sophis.portfolio.CSMExtraction extraction,
                                      ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                      bool onlyTheValue)
        {
            e_coupon:
            try
            {
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
                        cellValue.doubleValue += DataSource.GetCoupon(position, instrument) * position.GetAssetValue() * fxspot;
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
            catch (Exception)
            {
                goto e_coupon;
            }
        }
    }
    
    public class PC_InvCash : sophis.portfolio.CSMPortfolioColumn
    {
        //Get instance of the DataSource singleton
        private static DataSourceInvestedCash DataSource = DataSourceInvestedCash.GetInstance();

        ///<summary>
        ///Called by Risque to get the value to display in the position cell. The value should be
        ///added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        ///the value.
        ///</summary>
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
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
                Value.doubleValue = DataSource.GetInvestedCash(position, instrument);
            }
            if (position == null)
                return;

            //Set style
            style.kind = NSREnums.eMDataType.M_dDouble;
            style.@decimal = 0;
            style.alignment = sophis.gui.eMAlignmentType.M_aRight;
            sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
            col.green = 0;
			col.blue = 65000;
			col.red = 0;
			style.color= col;
            style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
        }

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                     sophis.portfolio.CSMExtraction extraction,
                                     ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                     bool onlyTheValue)
        {
            e_invcash:
            try
            {
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
                        cellValue.doubleValue += DataSource.GetInvestedCash(position, instrument);
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
            catch (Exception)
            {
                goto e_invcash;
            }
        }
    }
    
    public class PC_ReceivedCoupons : sophis.portfolio.CSMPortfolioColumn
    {
        //Get instance of the DataSource singleton
        private static DataSourceReceivedCoupons DataSource = DataSourceReceivedCoupons.GetInstance();

        ///<summary>
        ///Called by Risque to get the value to display in the position cell. The value should be
        ///added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        ///the value.
        ///</summary>
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
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
                Value.doubleValue = DataSource.GetReceivedCoupons(position, instrument);
            }
            if (position == null)
                return;

            //Set style
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


        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                             sophis.portfolio.CSMExtraction extraction,
                             ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                             bool onlyTheValue)
        {
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
                    cellValue.doubleValue += DataSource.GetReceivedCoupons(position, instrument);
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
    }

    public class PC_ReceivedCouponsLocalCCY : sophis.portfolio.CSMPortfolioColumn
    {
        //Get instance of the DataSource singleton
        private static DataSourceReceivedCouponsLocalCCY DataSource = DataSourceReceivedCouponsLocalCCY.GetInstance();

        ///<summary>
        ///Called by Risque to get the value to display in the position cell. The value should be
        ///added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        ///the value.
        ///</summary>
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
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
                Value.doubleValue = DataSource.GetReceivedCouponsLocalCCY(position, instrument);
            }
            if (position == null)
                return;

            //Set style
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


        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                             sophis.portfolio.CSMExtraction extraction,
                             ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                             bool onlyTheValue)
        {
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
                    cellValue.doubleValue += DataSource.GetReceivedCouponsLocalCCY(position, instrument);
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
    }

    public class PC_TMZAccrued : sophis.portfolio.CSMPortfolioColumn
    {
        //Get instance of the DataSource singleton
        private static DataSourceTMZAccrued DataSource = DataSourceTMZAccrued.GetInstance();

        ///<summary>
        ///Called by Risque to get the value to display in the position cell. The value should be
        ///added to the SSMCellValue structure. The SSMCellStyle.kind property defines the type of
        ///the value.
        ///</summary>
        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
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
                Value.doubleValue = DataSource.GetTMZAccrued(position, instrument);
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
        
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                         sophis.portfolio.CSMExtraction extraction,
                         ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                         bool onlyTheValue)
        {
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
                    cellValue.doubleValue += DataSource.GetTMZAccrued(position, instrument);
                }
            }
            if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

            //Set style
            cellStyle.kind = NSREnums.eMDataType.M_dDouble;
            cellStyle.@decimal = 2;
            cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
            cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
            cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
            sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
            col.green = 0;
            col.blue = 65000;
            col.red = 0;
            cellStyle.color = col;
        }    
    
    }
    
    public class PC_FixedOrFloat : sophis.portfolio.CSMPortfolioColumn
    {
        //Get instance of the DataSource singleton
        private static DataSourceFixedOrFloat DataSource = DataSourceFixedOrFloat.GetInstance();

        public override void GetPositionCell(int activePortfolioCode, int portfolioCode,
                                            CSMExtraction extraction, int underlyingCode,
                                            int instrumentCode, sophis.instrument.eMPositionType positionType,
                                            int positionIdentifier, ref SSMCellValue Value,
                                            SSMCellStyle style, bool onlyTheValue)
        {
            //get the portfolio from its code and the extraction
            CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
            if (portfolio == null)
                return;

            //get the position
            CSMPosition position;
            if (positionIdentifier == 0) // We are in flat view
                position = portfolio.GetFlatViewPosition(instrumentCode, positionType);
            else // We are in hierchical view
                position = portfolio.GetTreeViewPosition(positionIdentifier);
            if (position == null)
                return;

            CSMInstrument instrument = CSMInstrument.GetInstance(instrumentCode);

            //Set Value
            Value.doubleValue = DataSource.GetFixedOrFloat(position, instrument);

            //Set style
            style.kind = NSREnums.eMDataType.M_dDouble;
            style.@decimal = 0;
            style.alignment = sophis.gui.eMAlignmentType.M_aRight;
            style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
        }

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                      sophis.portfolio.CSMExtraction extraction,
                                      ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                      bool onlyTheValue)
        {
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
                    SumAssetValue += position.GetAssetValue() * fxspot;//Somme de ttes les AV
                    cellValue.doubleValue += DataSource.GetFixedOrFloat(position, instrument) * position.GetAssetValue() * fxspot;//Somme des Av des float
                }
            }
            cellValue.doubleValue = cellValue.doubleValue * 100 / SumAssetValue;
            if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

            //Set style
            cellStyle.kind = NSREnums.eMDataType.M_dDouble;
            cellStyle.@decimal = 0;
            cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
            cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
            cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
        }

    }

}

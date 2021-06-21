using System.Text;
using sophis.portfolio;
using System.Collections;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using TkoPortfolioColumn.DataCache;
using sophis.static_data;
using TkoPortfolioColumn.DbRequester;
using System.ComponentModel;
using System;


namespace TkoPortfolioColumn
{
    public static class TkoGreeks
    {
        public static double TkoAvgPriceBase10Position(this CSMPosition position, InputProvider input)
        {
            input.TmpPortfolioColName = "Average price";
            var avgprice = Helper.TkoGetValuefromSophisDouble(input);

            return input.IndicatorValue;
        }

        public static double TkoGlobaDelta(this CSMPosition position, InputProvider input)
        {
            //SSMCellValue dummyCellValue = new SSMCellValue();
            //SSMCellStyle dummyCellStyle = new SSMCellStyle();

            //var underlyingNumber = input.PortFolio.GetUnderlyingCount();
            //CSMPortfolioColumn colDelta = CSMPortfolioColumn.GetCSRPortfolioColumn("Global Delta");
            //var sum = 0;
            //for (int index = 0; index < underlyingNumber; index++)
            //{
            //    CSMUnderlying underlying = input.PortFolio.GetNthUnderlying(index);
            //    input.UnderlyingCode = underlying.GetInstrumentCode();
            //    if (underlying != null && input.UnderlyingCode != 0)
            //    {
            //        colDelta.GetUnderlyingCell(
            //          input.ActivePortfolioCode,
            //          input.PortFolioCode,
            //          null,
            //          input.UnderlyingCode,
            //          ref dummyCellValue,
            //          dummyCellStyle,
            //          true);

            //        if (dummyCellValue.doubleValue != 0.0)
            //        {
            //            input.IndicatorValue += dummyCellValue.doubleValue;
            //        }
            //    }
            //}

            return input.IndicatorValue;
        }
    }
}
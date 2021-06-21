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
    public static class StressTest
    {
        public static double TkoStressPositionAssetValueByPrice(this CSMPosition position, InputProvider input)
        {

           double stressprice = 0.9 * CSMInstrument.GetLast(input.InstrumentCode);
           double val = sophis.value.CSMAmValuationTools.GetAssetValue(input.InstrumentCode, position.GetInstrumentCount(), stressprice);
            
           input.IndicatorValue = val;
           return val;
        }

    }

}
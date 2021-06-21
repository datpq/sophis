using sophis.instrument;
using System;
using sophis.utils;
namespace TkoPortfolioColumn
{
    namespace Ratings
    {
        public static class SophisStaticData
        {

            #region TKO RatingComp
            public static double TkoComputeRatingComp(this CSMInstrument instrument, InputProvider input)
            {
                double comprate = 0;

                try
                {
                    comprate = instrument.TkoNotationNum();
                    return comprate;
                }
                catch (Exception)
                {
                    CSMLog.Write("MarketIndicCompute", "ComputeRatingComp", CSMLog.eMVerbosity.M_warning, "Cannot compute RateComp for Instrument " + instrument.GetCode());
                    return -2;
                }
            }
            #endregion

            public static double TkoComputeRatingSecondComp(this CSMInstrument instrument, InputProvider input)
            {
                double comprate = 0;

                try
                {
                    comprate = instrument.TkoNotationSecondNum();
                    return comprate;
                }
                catch (Exception)
                {
                    CSMLog.Write("MarketIndicCompute", "ComputeRatingSecondComp", CSMLog.eMVerbosity.M_warning, "Cannot compute RateComp for Instrument " + instrument.GetCode());
                    return -2;
                }
            }



        }
    }
}
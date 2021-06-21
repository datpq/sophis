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

//@DPH
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class YTMExtentionMethod
    {
        #region YTM

        public static double CheckYTMValue(InputProvider input)
        {
            //CR 22052019 Floor 0 TKO YTM
            //if (input.IndicatorValue == -10000000.0 || input.IndicatorValue < 0)
            if (input.IndicatorValue == -10000000.0)
            {
                input.IndicatorValue = 0.0;
            }
            return input.IndicatorValue;
        }

        public static double TkoComputeTreeYTMValue(this CSMInstrument instrument, InputProvider input)
        {
            try
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0}, InstrumentType={1})",
                        input.ToString(), instrument.GetInstrumentType());
                }

                double val = 0;
                input.InstrumentType = instrument.GetInstrumentType().ToString();
                if (instrument.GetInstrumentType() == 'O' || instrument.GetInstrumentType() == 'D')
                {
                    int Tytm = 0;

                    input.TmpPortfolioColName = "Allotment";
                    var allotmentList = DbrTikehau_Config.GetTikehauConfigFromName("INTERNAL-SECURITIES-ALLOTMENTS");
                    bool ret = false;
                    foreach (var elt in allotmentList)
                    {
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "elt={0}", elt);
                        }

                        if (elt.VALUE == Helper.TkoGetValuefromSophisString(input))
                        {
                            ret = true;
                            break;
                        }
                    }

                    if (ret)
                    {
                        val = instrument.GetYTM() * 100;
                        input.IndicatorValue = val;
                        return CheckYTMValue(input);
                    }

                    //calcul du cas 
                    Tytm = instrument.TkoComputeTreeYTM(input);
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "Tytm={0}", Tytm);
                    }

                    if (Tytm == 1 || Tytm == 4)
                    {
                        //Correctif.
                        val = instrument.GetYTMMtoM() * 100;
                        //val = instrument.GetYTM_API() * 100;//IG (YTM) ou CMS ou First Call< Today
                        if (val < 15) input.IndicatorValue = val;
                        else input.IndicatorValue = 15;
                        return CheckYTMValue(input);
                    }
                    else if (Tytm == 2) // Sub not CMS et FirstCall>Today(YTC)
                    {
                        input.TmpPortfolioColName = "Yield to Call MtM";
                        val = Helper.TkoGetValuefromSophisDouble(input);
                        if (val < 15) input.IndicatorValue = val;
                        else input.IndicatorValue = 15;
                        return CheckYTMValue(input);
                    }
                    else if (Tytm == 3) //Hy (YTW)
                    {
                        input.TmpPortfolioColName = "Yield to Worst MtM";
                        val = Helper.TkoGetValuefromSophisDouble(input);
                        input.IndicatorValue = val;
                        return CheckYTMValue(input);
                    }
                    else
                    {
                        input.IndicatorValue = 0;
                        return CheckYTMValue(input);
                    }
                }
                else if (instrument.GetInstrumentType() == 'S') //CDS
                {
                    val = instrument.GetYTM() * 100;
                    input.IndicatorValue = val;
                    return CheckYTMValue(input);
                }
                else if (instrument.GetInstrumentType() == 'T') //Debt Instruments
                {
                    val = instrument.GetYTMMtoM() * 100;
                    input.IndicatorValue = val;
                    return CheckYTMValue(input);
                }
                else if (instrument.GetInstrumentType() == 'F') //Interest Rate Futures
                {
                    val = instrument.GetYTM() * 100;
                    input.IndicatorValue = val;
                    return CheckYTMValue(input);
                }
                else
                {
                    input.IndicatorValue = 0;
                    return CheckYTMValue(input);
                }
            }
            catch (Exception)
            {
                throw;
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
    }
}
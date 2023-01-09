using System.Reflection;
using System.Collections;
using sophis.tools;
using sophis.instrument;
using sophis.utils;
using sophis.finance;
using System;
using sophis.market_data;

namespace MEDIO.BackOffice.net
{
    public class CSxAbsPoolFactorCheck : CSMInstrumentAction
    {
        private void PrintRedemptionList(ArrayList arr)
        {
            using (CSMLog Log = new CSMLog())
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    var red = arr[i] as SSMRedemption;
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"On date={red.maturityDate}, flowType={red.flowType}, poolFactor = {red.poolFactor}");
                }
            }
        }

        public override void NotifyModified(CSMInstrument instrument, CSMEventVector message)
        {
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin(typeof(CSxAbsPoolFactorCheck).Name, MethodBase.GetCurrentMethod().Name);
                try
                {
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"BEGIN(Sicovam={instrument.GetCode()}, Reference={instrument.GetReference()})");
                    var oldInstrument = CSMMarketData.GetCurrentMarketData().GetCSRInstrument(instrument.GetCode());
                    CSMABSBond oldAbsBond = oldInstrument;
                    if (oldAbsBond == null)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_debug, "Old instrument is not an ABS Bond. Do nothing.");
                        return;
                    }
                    var oldRedemptionList = new ArrayList();
                    oldAbsBond.GetFixedRedemptions(oldRedemptionList);
                    CSMABSBond absBond = instrument;
                    if (absBond == null)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_debug, "Instrument is not an ABS Bond. Do nothing.");
                        return;
                    }

                    var newRedemptionList = new ArrayList();
                    absBond.GetFixedRedemptions(newRedemptionList);
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"oldRedemptionList.Count={oldRedemptionList.Count}, newRedemptionList.Count={newRedemptionList.Count}");
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"printing oldRedemptionList...Count={oldRedemptionList.Count}");
                    PrintRedemptionList(oldRedemptionList);
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"printing newRedemptionList...Count={newRedemptionList.Count}");
                    PrintRedemptionList(newRedemptionList);

                    var lastPoolFactor = (newRedemptionList[newRedemptionList.Count - 1] as SSMRedemption).poolFactor;
                    int newCount = newRedemptionList.Count;
                    for (int i = newCount; i < oldRedemptionList.Count; i++)
                    {
                        var oldRed = oldRedemptionList[i] as SSMRedemption;
                        oldRed.poolFactor = lastPoolFactor;
                        newRedemptionList.Add(oldRed);
                        Log.Write(CSMLog.eMVerbosity.M_debug, $"Add new poolFactor date={oldRed.maturityDate}, flowType={oldRed.flowType}, val = {oldRed.poolFactor}");
                    }
                    for (int i = 1; i < newRedemptionList.Count; i++)
                    {
                        var newRed = newRedemptionList[i] as SSMRedemption;
                        if (newRed.poolFactor == -10000000) // N/A
                        {
                            var prevRed = newRedemptionList[i-1] as SSMRedemption;
                            if (prevRed.poolFactor != -10000000)
                            {
                                Log.Write(CSMLog.eMVerbosity.M_debug, $"Update poolFactor when data is unavailable. date={newRed.maturityDate}, flowType={newRed.flowType}, old val = {newRed.poolFactor}, new val = {prevRed.poolFactor}");
                                newRed.poolFactor = prevRed.poolFactor;
                            }
                        }
                        if (oldRedemptionList.Count > i) // Check same PoolFactor but modified in the previous day
                        {
                            var oldRed = oldRedemptionList[i] as SSMRedemption;
                            //Log.Write(CSMLog.eMVerbosity.M_debug, $"i={i}, oldRed={oldRed.poolFactor}, newRed={newRed.poolFactor}. date={newRed.maturityDate}");
                            if (newRed.poolFactor == oldRed.poolFactor)
                            {
                                var prevNewRed = newRedemptionList[i - 1] as SSMRedemption;
                                var prevOldRed = oldRedemptionList[i - 1] as SSMRedemption;
                                if (prevNewRed.poolFactor != prevOldRed.poolFactor)
                                {
                                    Log.Write(CSMLog.eMVerbosity.M_debug, $"Update poolFactor when data has changed in the previous day. date={newRed.maturityDate}, flowType={newRed.flowType}, old val = {newRed.poolFactor}, new val = {prevNewRed.poolFactor}");
                                    newRed.poolFactor = prevNewRed.poolFactor;
                                }
                            }
                        }
                    }

                    //regenerate redemption to check whether the cash flow is full
                    absBond.RegenerateRedemption();
                    var afterGenRedemptionList = new ArrayList();
                    absBond.GetFixedRedemptions(afterGenRedemptionList);
                    newCount = newRedemptionList.Count;
                    for(int i=newCount; i<afterGenRedemptionList.Count; i++)
                    {
                        if (i == 0) continue;
                        var afterGenRed = afterGenRedemptionList[i] as SSMRedemption;
                        var prevRed = newRedemptionList[i - 1] as SSMRedemption;
                        if (prevRed.poolFactor != -10000000)
                        {
                            afterGenRed.poolFactor = prevRed.poolFactor;
                            newRedemptionList.Add(afterGenRed);
                            Log.Write(CSMLog.eMVerbosity.M_debug, $"Add new generation pool factor date={afterGenRed.maturityDate}, flowType={afterGenRed.flowType}, poolFactor={afterGenRed.poolFactor}");
                        }
                    }
                    absBond.SetFixedRedemptions(newRedemptionList);
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"Updating done.");

                }
                catch (Exception e)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, "Exception Occured: " + e.Message);
                }
                finally
                {
                    Log.Write(CSMLog.eMVerbosity.M_debug, "END");
                }
            }
        }
    }
}

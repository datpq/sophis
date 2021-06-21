using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using NLog;

using sophis.instrument;
using sophis.finance;
using sophis;
using sophis.misc;
using Sophis;
using sophis.market_data;
using sophis.utils;

namespace Eff.UpgradeUtilities
{
    public static class UpgradeExtensions
    {
        private static MemoryCache memoryCache = MemoryCache.Default;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static int cacheTimeout;
        private static bool IsFirstMsg = true;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethodName()
        {
            var stackTrace = new StackTrace();
            var result = stackTrace.GetFrame(1).GetMethod().Name;
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetCurrentLineNumber()
        {
            var stackTrace = new StackTrace();
            var result = stackTrace.GetFrame(1).GetFileLineNumber();
            return result;
        }

        public static bool IsDebugEnabled()
        {
            return Logger.IsDebugEnabled || CSMLog.IsLogWorthIt(CSMLog.eMVerbosity.M_debug);
        }

        public static void Log(CSMLog.eMVerbosity verb, string message, params object[] args)
        {
            string formatedMsg = string.Format(message, args);
            StackTrace stackTrace = new StackTrace();

            var methodBase = stackTrace.GetFrame(1).GetMethod();
            var assembly = Assembly.GetCallingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var assemblyName = assembly.GetName();
            var callSiteClass = string.Format("{0}({1}) {2}", assemblyName.Name, fileVersionInfo.FileVersion /* assemblyName.Version */, methodBase.DeclaringType);
            CSMLog.Write(callSiteClass, methodBase.Name, verb, formatedMsg);
            var loggerMsg = string.Format("{0}::{1}() : {2}", callSiteClass, methodBase.Name, formatedMsg); //same format as Sophis log
            if (IsFirstMsg)
            {
                IsFirstMsg = false;
                CSMConfigurationFile.getEntryValue("TKO", "CacheTimeout", ref cacheTimeout, 0);
                Logger.Info("NEW SOPHIS INSTANCE STARTING HERE... (cacheTimeout = {0})", cacheTimeout);
            }
            switch (verb)
            {
                case CSMLog.eMVerbosity.M_error:
                    Logger.Error(loggerMsg);
                    break;
                case CSMLog.eMVerbosity.M_warning:
                    Logger.Warn(loggerMsg);
                    break;
                case CSMLog.eMVerbosity.M_info:
                    Logger.Info(loggerMsg);
                    break;
                default:
                    Logger.Debug(loggerMsg);
                    break;
            }
        }

        private static void ComputeAll(CSMInstrument instrument, out CSMComputationResults computationResults, out ComputationResults computationResultsExt)
        {
            try
            {
                if (IsDebugEnabled())
                {
                    Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0})", instrument.GetCode());
                }

                //var cachePrefix = GetCurrentMethodName();
                //var cacheKey = string.Format("{0}.{1}", cachePrefix, instrument.GetCode());
                //var cacheTimeout = 3600;
                //var cacheValue = memoryCache.Get(cacheKey) as List<object>;
                //if (cacheValue == null)
                //{
                //    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Not found in cache. Do ComputeAll", cacheKey, cacheTimeout);
                    //using (var pricer = CSMPricer.GetPricerByPriority(instrument).model)
                    using (var pricer = instrument.GetDefaultPricer())
                    {
                        computationResults = new CSMComputationResults();
                        //pricer.ComputeAllCore(instrument, CSMMarketData.GetCurrentMarketData(), computationResults);
                        if (IsDebugEnabled())
                        {
                            Log(CSMLog.eMVerbosity.M_debug, "BEGIN ComputeAll");
                        }
                        pricer.ComputeAll(instrument, CSMMarketData.GetCurrentMarketData(), computationResults);
                        if (IsDebugEnabled())
                        {
                            Log(CSMLog.eMVerbosity.M_debug, "END ComputeAll");
                        }
                        computationResultsExt = computationResults.GetResults();
                    }
                //    cacheValue = new List<object> { computationResults, computationResultsExt };
                //    memoryCache.Set(cacheKey, cacheValue, DateTimeOffset.Now.AddSeconds(cacheTimeout));
                //}
                //else
                //{
                //    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Found in cache", cacheKey, cacheTimeout);
                //    computationResults = cacheValue[0] as CSMComputationResults;
                //    computationResultsExt = cacheValue[1] as ComputationResults;
                //}
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                if (IsDebugEnabled())
                {
                    Log(CSMLog.eMVerbosity.M_debug, "END");
                }
            }
        }

        public static double GetDuration(this CSMInstrument instrument)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0})", instrument.GetCode());
                //CSMPricer pricer = instrument.GetDefaultPricer();
                //var pricer = CSMPricer.GetPricerByPriority(instrument).model;
                using (var pricer = instrument.GetDefaultPricer())
                {
                    result = pricer.GetDuration(instrument, CSMMarketData.GetCurrentMarketData());
                }
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static int GetUnderlyingCount(this CSMInstrument instrument)
        {
            int result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0})", instrument.GetCode());
                var cachePrefix = GetCurrentMethodName();
                var cacheKey = string.Format("{0}.{1}", cachePrefix, instrument.GetCode());
                var cacheValue = memoryCache.Get(cacheKey) as int?;
                if (cacheValue == null)
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Not found in cache. Do ComputeAll", cacheKey, cacheTimeout);
                    CSMComputationResults computationResults;
                    ComputationResults computationResultsExt;
                    ComputeAll(instrument, out computationResults, out computationResultsExt);
                    result = computationResultsExt.GetDeltaCount();
                    cacheValue = result;
                    memoryCache.Set(cacheKey, cacheValue, cacheTimeout <= 0 ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.Now.AddSeconds(cacheTimeout));
                }
                else
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Found in cache", cacheKey, cacheTimeout);
                    result = cacheValue.Value;
                }
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static int GetUnderlying(this CSMInstrument instrument, int i)
        {
            int result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0}, i={1})", instrument.GetCode(), i);
                var cachePrefix = GetCurrentMethodName();
                var cacheKey = string.Format("{0}.{1}.{2}", cachePrefix, instrument.GetCode(), i);
                var cacheValue = memoryCache.Get(cacheKey) as int?;
                if (cacheValue == null)
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Not found in cache. Do ComputeAll", cacheKey, cacheTimeout);
                    CSMComputationResults computationResults;
                    ComputationResults computationResultsExt;
                    ComputeAll(instrument, out computationResults, out computationResultsExt);
                    result = computationResultsExt.GetDeltaKey(i).fSicovam;
                    cacheValue = result;
                    memoryCache.Set(cacheKey, cacheValue, cacheTimeout <= 0 ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.Now.AddSeconds(cacheTimeout));
                }
                else
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Found in cache", cacheKey, cacheTimeout);
                    result = cacheValue.Value;
                }
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static int GetUnderlying_API(this CSMInstrument instrument, int i)
        {
            int result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0}, i={1})", instrument.GetCode(), i);
                var cachePrefix = GetCurrentMethodName();
                var cacheKey = string.Format("{0}.{1}.{2}", cachePrefix, instrument.GetCode(), i);
                var cacheValue = memoryCache.Get(cacheKey) as int?;
                if (cacheValue == null)
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Not found in cache. Do ComputeAll", cacheKey, cacheTimeout);

                    CSMComputationResults computationResults;
                    ComputationResults computationResultsExt;
                    ComputeAll(instrument, out computationResults, out computationResultsExt);
                    result = computationResultsExt.GetDeltaKey(i).fSicovam;
                    cacheValue = result;
                    memoryCache.Set(cacheKey, cacheValue, cacheTimeout <= 0 ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.Now.AddSeconds(cacheTimeout));
                }
                else
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Found in cache", cacheKey, cacheTimeout);
                    result = cacheValue.Value;
                }
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static double GetDelta(this CSMInstrument instrument, ref int sicovam)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0}, sicovam={1})", instrument.GetCode(), sicovam);
                var cachePrefix = GetCurrentMethodName();
                var cacheKey = string.Format("{0}.{1}.{2}", cachePrefix, instrument.GetCode(), sicovam);
                var cacheValue = memoryCache.Get(cacheKey) as double?;
                if (cacheValue == null)
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Not found in cache. Do ComputeAll", cacheKey, cacheTimeout);
                    CSMComputationResults computationResults;
                    ComputationResults computationResultsExt;
                    ComputeAll(instrument, out computationResults, out computationResultsExt);
                    result = instrument.GetDelta(computationResults, ref sicovam);
                    cacheValue = result;
                    memoryCache.Set(cacheKey, cacheValue, cacheTimeout <= 0 ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.Now.AddSeconds(cacheTimeout));
                }
                else
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Found in cache", cacheKey, cacheTimeout);
                    result = cacheValue.Value;
                }
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(sicovam={0}, result={1})", sicovam, result);
            }
        }

        public static double GetYTM(this CSMInstrument instrument)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0})", instrument.GetCode());
                var cachePrefix = GetCurrentMethodName();
                var cacheKey = string.Format("{0}.{1}", cachePrefix, instrument.GetCode());
                var cacheValue = memoryCache.Get(cacheKey) as double?;
                if (cacheValue == null)
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Not found in cache. Do ComputeAll", cacheKey, cacheTimeout);
                    CSMComputationResults computationResults;
                    ComputationResults computationResultsExt;
                    ComputeAll(instrument, out computationResults, out computationResultsExt);
                    result = computationResultsExt.YTM.Value;
                    cacheValue = result;
                    memoryCache.Set(cacheKey, cacheValue, cacheTimeout <= 0 ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.Now.AddSeconds(cacheTimeout));
                }
                else
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Found in cache", cacheKey, cacheTimeout);
                    result = cacheValue.Value;
                }
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static double GetYTMMtoM(this CSMInstrument instrument)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0})", instrument.GetCode());
                var cachePrefix = GetCurrentMethodName();
                var cacheKey = string.Format("{0}.{1}", cachePrefix, instrument.GetCode());
                var cacheValue = memoryCache.Get(cacheKey) as double?;
                if (cacheValue == null)
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Not found in cache. Do ComputeAll", cacheKey, cacheTimeout);
                    CSMComputationResults computationResults;
                    ComputationResults computationResultsExt;
                    ComputeAll(instrument, out computationResults, out computationResultsExt);
                    result = computationResultsExt.YTMMtoM.Value;
                    cacheValue = result;
                    memoryCache.Set(cacheKey, cacheValue, cacheTimeout <= 0 ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.Now.AddSeconds(cacheTimeout));
                }
                else
                {
                    Log(CSMLog.eMVerbosity.M_debug, "cacheKey={0}, cacheTimeout={1}. Found in cache", cacheKey, cacheTimeout);
                    result = cacheValue.Value;
                }
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static double GetRho(this CSMInstrument instrument)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0})", instrument.GetCode());
                //var pricer = CSMPricer.GetPricerByPriority(instrument).model;
                using (var pricer = instrument.GetDefaultPricer())
                {
                    result = pricer.GetRho(instrument, CSMMarketData.GetCurrentMarketData(), instrument.GetCurrency(), eMRhoBumpType.M_BasisPoint);
                }

                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static double GetAccruedCoupon(this CSMInstrument instrument)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0})", instrument.GetCode());
                var evaluationDate = CSMMarketData.GetCurrentMarketData().GetDate();
                result = instrument.GetAccruedCoupon(evaluationDate);
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static double GetAccruedCoupon_API(this CSMSwap swap)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(swap={0})", swap.GetCode());
                var evaluationDate = CSMMarketData.GetCurrentMarketData().GetDate();
                result = swap.GetAccruedCoupon(evaluationDate);
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static double GetTheoreticalValue(this CSMInstrument instrument)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0})", instrument.GetCode());
                var computationResults = new CSMComputationResults();
                instrument.GetTheoreticalValueWithQuotationType(computationResults);
                var computationResultsExt = computationResults.GetResults();
                result = computationResultsExt.TheoreticalValue.Value;
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static double GetModDuration(this CSMBond bond, CSMMarketData context)
        {
            double result = 0;
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(bond={0})", bond.GetCode());
                //var pricer = CSMPricer.GetPricerByPriority(bond).model;
                using (var pricer = bond.GetDefaultPricer())
                {
                    result = pricer.GetModDuration(bond, context);
                }
                return result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(result={0})", result);
            }
        }

        public static void GetSwapInformation(this CSMSwap swap, CSMMarketData context, ArrayList receivedLeg, ArrayList paidLeg)
        {
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(swap={0})", swap.GetCode());
                var pricer = swap.GetDefaultPricer();
                var pricerSwap = (ISMPricerSwap)((CSmartPtr)pricer);
                pricerSwap.GetSwapInformation(swap, context, receivedLeg, paidLeg);
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(receivedLeg.Count={0}, paidLeg.Count={1})", receivedLeg.Count, paidLeg.Count);
            }
        }

        public static void GetName(this ISMInstrument instrument, CMString name)
        {
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(instrument={0}, name={1})", instrument.GetCode(), name);
                CMString result = instrument.GetName();
                name = result;
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END(name={0})", name);
            }
        }

        public static CSMCreditRisk GetCSRCreditRisk(this CSMMarketData marketData, int issuerCode, int currencyCode)
        {
            try
            {
                Log(CSMLog.eMVerbosity.M_debug, "BEGIN(issuerCode={0}, currencyCode={1})", issuerCode, currencyCode);
                return marketData.GetCreditRisk(issuerCode, currencyCode);
            }
            catch (Exception e)
            {
                Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                Log(CSMLog.eMVerbosity.M_debug, "END");
            }
        }
    }
}

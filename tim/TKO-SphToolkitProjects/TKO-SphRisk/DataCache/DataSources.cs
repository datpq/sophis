using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using sophis.portfolio;
using sophis.market_data;
using sophis.utils;

using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    namespace DataCache
    {
            public class DataSourcePorfolioIndicators
            {
                public static readonly Dictionary<string, InputProviderCache> DataCacheIndiactorValueByPosition = new Dictionary<string, InputProviderCache>();
                public static readonly Dictionary<int, Dictionary<string, IInputProvider>> DataCacheIndiactorValueByFolio = new Dictionary<int, Dictionary<string, IInputProvider>>();
                public static readonly Dictionary<string, Dictionary<string, IInputProvider>> DataCacheIndiactorValueByFolioStringKey = new Dictionary<string, Dictionary<string, IInputProvider>>();

                //Main Cache
                //_TODO_ Add a cache by compute date.
                public static readonly Dictionary<int, Dictionary<int, Dictionary<string, double>>> DataCacheIndiactorByComputeDate = new Dictionary<int, Dictionary<int, Dictionary<string, double>>>();

                //_TODO_
                private static readonly List<string> csvheader = new List<string> { "ReportingDate", "ActivePortfolioCode", "PortFolioCode", "PositionIdentifier", "InstrumentCode", "Indicator" };

                public static void FillPositionCache(InputProvider input)
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(Cache by InputProviderCache. DataCacheIndiactorValueByPosition.Count={0})", DataCacheIndiactorValueByPosition.Count());
                    }
                    int PositionId = input.PositionIdentifier;
                    string key = string.Format("{0}_{1}", input.PositionIdentifier, input.Column);
                    if (DataCacheIndiactorValueByPosition.ContainsKey(key))
                    {
                        DataCacheIndiactorValueByPosition[key] = input.GetInputProviderCache();
                    }
                    else
                    {
                        DataCacheIndiactorValueByPosition.Add(key, input.GetInputProviderCache());
                    }

                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(Cache by InputProviderCache. DataCacheIndiactorValueByPosition.Count={0})", DataCacheIndiactorValueByPosition.Count());
                    }
                }

                public static void FillFolioCache(InputProvider input)
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(DataCacheIndiactorValueByFolio.Count={0})", DataCacheIndiactorValueByFolio.Count());
                    }
                    int folioCode = input.PortFolioCode;
                    if (DataCacheIndiactorValueByFolio.ContainsKey(folioCode))
                    {
                        if (DataCacheIndiactorValueByFolio[folioCode] == null)
                            DataCacheIndiactorValueByFolio[folioCode] = new Dictionary<string, IInputProvider>();

                        if (DataCacheIndiactorValueByFolio[folioCode].ContainsKey(input.Column))
                            DataCacheIndiactorValueByFolio[folioCode][input.Column] = input;
                        else
                            DataCacheIndiactorValueByFolio[folioCode].Add(input.Column, input);
                    }
                    else
                    {
                        DataCacheIndiactorValueByFolio.Add(folioCode, new Dictionary<string, IInputProvider>() { { input.Column, input } });
                    }
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(DataCacheIndiactorValueByFolio.Count={0})", DataCacheIndiactorValueByFolio.Count());
                    }
                }

                public static void FillFolioCacheStringKey(InputProvider input)
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(DataCacheIndiactorValueByFolioStringKey.Count={0})", DataCacheIndiactorValueByFolioStringKey.Count());
                    }
                    string folioCodeKey = input.FolioCacheStringKey;
                    if (DataCacheIndiactorValueByFolioStringKey.ContainsKey(folioCodeKey))
                    {
                        if (DataCacheIndiactorValueByFolioStringKey[folioCodeKey] == null)
                            DataCacheIndiactorValueByFolioStringKey[folioCodeKey] = new Dictionary<string, IInputProvider>();

                        if (DataCacheIndiactorValueByFolioStringKey[folioCodeKey].ContainsKey(input.Column))
                            DataCacheIndiactorValueByFolioStringKey[folioCodeKey][input.Column] = input;
                        else
                            DataCacheIndiactorValueByFolioStringKey[folioCodeKey].Add(input.Column, input);
                    }
                    else
                    {
                        DataCacheIndiactorValueByFolioStringKey.Add(folioCodeKey, new Dictionary<string, IInputProvider>() { { input.Column, input } });
                    }
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(DataCacheIndiactorValueByFolioStringKey.Count={0})", DataCacheIndiactorValueByFolioStringKey.Count());
                    }
                }
            }

            public static class VersionClass
            {
                private static int      _RefreshVersion;//Numéro de version F8/F9
                private static int      _ReportingDate;//Date courante de Sophis

                public static int   Get_RefreshVersion() { return _RefreshVersion; }
                public static void  Set_RefreshVersion(int Version) { _RefreshVersion = Version; }
                public static int   Get_ReportingDate() { return _ReportingDate; }
                public static void  Set_ReportingDate(int Date) { _ReportingDate = Date; }

                public static bool CheckCacheVersion(InputProvider input)
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN");
                    bool ret = false;
                    //Si la date de Sophis change, vide tous les caches
                    if (VersionClass.Get_ReportingDate() != input.ReportingDate)
                    {
                        VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                        VersionClass.DeleteCache();
                        ret = true;

                    }

                    //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
                    if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
                    {
                        VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                        VersionClass.DeleteCache();
                        ret = true;
                    }
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                    return ret;
                }

                public static bool CheckComputeVersion(InputProvider input)
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN");
                    bool ret = false;
                    //Si la date de Sophis change, vide tous les caches
                    if (VersionClass.Get_ReportingDate() != input.ReportingDate)
                    {
                        ret = true;
                    }

                    //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
                    if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
                    {
                        VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                        ret = true;
                    }
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                    return ret;
                }

                public static void DeleteCache()
                {
                    if (null != DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition) DataSourcePorfolioIndicators.DataCacheIndiactorValueByPosition.Clear();
                    if (null != DataSourcePorfolioIndicators.DataCacheIndiactorValueByFolio) DataSourcePorfolioIndicators.DataCacheIndiactorValueByFolio.Clear();
                }
            }
    }
}

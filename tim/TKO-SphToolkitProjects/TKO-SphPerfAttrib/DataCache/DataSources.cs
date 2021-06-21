using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using sophis.portfolio;
using sophis.instrument;
using sophis.market_data;
using TkoPortfolioColumn.DbRequester;
using System.IO;

namespace TkoPortfolioColumn
{
    namespace DataCache
    {
            public class DataSourcePorfolioIndicators
            {
                public static readonly Dictionary<int, Dictionary<string, IInputProvider>> DataCacheIndiactorValueByPosition = new Dictionary<int, Dictionary<string, IInputProvider>>();
                public static readonly Dictionary<int, Dictionary<string, IInputProvider>> DataCacheIndiactorValueByFolio = new Dictionary<int, Dictionary<string, IInputProvider>>();

                //Main Cache
                //_TODO_ Add a cache by compute date.
                public static readonly Dictionary<int, Dictionary<int, Dictionary<string, double>>> DataCacheIndiactorByComputeDate = new Dictionary<int, Dictionary<int, Dictionary<string, double>>>();


                //_TODO_
                private static readonly List<string> csvheader = new List<string> { "ReportingDate", "ActivePortfolioCode", "PortFolioCode", "PositionIdentifier", "InstrumentCode", "Indicator" };

                public static void FillPositionCache(InputProvider input)
                {
                    int PositionId = input.Position.GetIdentifier();
                    if (DataCacheIndiactorValueByPosition.ContainsKey(PositionId))
                    {
                        if (DataCacheIndiactorValueByPosition[PositionId] == null)
                            DataCacheIndiactorValueByPosition[PositionId] = new Dictionary<string, IInputProvider>();

                        if (DataCacheIndiactorValueByPosition[PositionId].ContainsKey(input.Column))
                            DataCacheIndiactorValueByPosition[PositionId][input.Column] = input;
                        else
                            DataCacheIndiactorValueByPosition[PositionId].Add(input.Column, input);
                    }
                    else
                    {
                        DataCacheIndiactorValueByPosition.Add(PositionId, new Dictionary<string, IInputProvider>() { { input.Column, input} });
                    }
                }

                public static void FillFolioCache(InputProvider input)
                {
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
                    bool ret = false;
                    //Si la date de Sophis change, vide tous les caches
                    if (VersionClass.Get_ReportingDate() != input.ReportingDate)
                    {
                        VersionClass.Set_ReportingDate(input.ReportingDate);
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
                    return ret;
                }

                public static bool CheckComputeVersion(InputProvider input)
                {
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

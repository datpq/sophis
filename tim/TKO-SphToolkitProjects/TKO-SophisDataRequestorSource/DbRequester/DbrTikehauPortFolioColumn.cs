using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TkoPortfolioColumn.ConnectionProvider;


namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
        public static class DbrTikehauPortFolioColumn
        {
            public static List<TIKEHAU_PORTFOLIO_COLUMN> _config ;
            public class TIKEHAU_PORTFOLIO_COLUMN
            {
                public string NAME          { get; set;} 
                public string ACTIVATE      { get; set;} 
                public string PROVIDER      { get; set;} 
                public string DESCRIPTION   { get; set;}
                public string COLUMNGROUP   { get; set;}
                public string TOOLKIT       { get; set;}
            }

            public static List<TIKEHAU_PORTFOLIO_COLUMN> GetTikehauPortFolioColumn(string colname)
            {
                try
                {
                    var ret = GetTikehauColumnConfig();
                    if (ret != null)
                        return ret.Where(p => p.NAME.ToUpper() == colname.ToUpper()).ToList();
                    return new List<TIKEHAU_PORTFOLIO_COLUMN>();
                }
                catch (Exception ex)
                {
                    return new List<TIKEHAU_PORTFOLIO_COLUMN>();
                }
            }

            public static List<TIKEHAU_PORTFOLIO_COLUMN> GetTikehauPortFolioColumnToActivate()
            {
                try
                {
                    var ret = GetTikehauColumnConfig();
                    if (ret != null)
                        return ret.Where(p => p.ACTIVATE.ToUpper() == "Y").ToList();
                    return new List<TIKEHAU_PORTFOLIO_COLUMN>();
                }
                catch (Exception ex)
                {
                    return new List<TIKEHAU_PORTFOLIO_COLUMN>();
                }
            }

            public static List<TIKEHAU_PORTFOLIO_COLUMN> GetTikehauPortFolioColumnToDesactivate()
            {
                try
                {
                    var ret = GetTikehauColumnConfig();
                    if (ret != null)
                        return ret.Where(p => p.ACTIVATE.ToUpper() == "N").ToList();
                    return new List<TIKEHAU_PORTFOLIO_COLUMN>();
                }
                catch (Exception ex)
                {
                    return new List<TIKEHAU_PORTFOLIO_COLUMN>();
                }
            }

            public static List<TIKEHAU_PORTFOLIO_COLUMN> GetTikehauColumnConfig()
            {
                var query = "SELECT * FROM TIKEHAU_PORTFOLIO_COLUMN";
                if (_config == null)
                {
                    _config = ConnectionManager.OpenRisqueConnection().Query<TIKEHAU_PORTFOLIO_COLUMN>(query, null).ToList();
                }
                return _config;
            }

            public static TIKEHAU_PORTFOLIO_COLUMN GetTikehauColumnConfig(string colname)
            {
                try
                {
                    var ret = GetTikehauColumnConfig();
                    if (ret != null)
                        return ret.Where(p => p.NAME == colname).FirstOrDefault();
                    return new TIKEHAU_PORTFOLIO_COLUMN();
                }
                catch (Exception ex)
                {
                    return new TIKEHAU_PORTFOLIO_COLUMN();
                }
            }

            public static string GetTikehauPositionMethodFromColName(string colname)
            {
                try
                {
                    return DbrTikehauPortFolioColumn.GetTikehauColumnConfig(colname).PROVIDER.Split(',').ElementAt(0);
                }
                catch (Exception ex)
                {
                    return "";
                }
            }

            public static string GetTikehauPortFolioMethodFromColName(string colname)
            {
                try
                {
                    return DbrTikehauPortFolioColumn.GetTikehauColumnConfig(colname).PROVIDER.Split(',').ElementAt(1);
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
        }
    }

}

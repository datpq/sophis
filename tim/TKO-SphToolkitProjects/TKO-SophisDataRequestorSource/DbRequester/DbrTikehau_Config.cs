using System;
using System.Collections.Generic;
using System.Linq;
using TkoPortfolioColumn.ConnectionProvider;
using Dapper;


namespace TkoPortfolioColumn
{
    namespace DbRequester
    {
            public static class DbrTikehau_Config
            {
                         public class HISTO_MVTS
                    {
                        public int REFCON { get; set; }
                        public int OPCVM { get; set; }
                        public int SICOVAM { get; set; }
                        public int NBSECURITIES { get; set; }
                        public int MVTIDENT { get; set; }
                        public int SOPHIS_ORDER_ID { get; set; }
 
                    }

                    public static List<TIKEHAU_CONFIG> _config;
                    public class TIKEHAU_CONFIG
                    {
                        public string MODULE { get; set; }
                        public string NAME { get; set; }
                        public string VALUE { get; set; }
                        public string DESCRIPTION { get; set; }
                        public int LIST_POSITION { get; set; }
                    }

                    public static List<TIKEHAU_FOLIO_COLUMNS_CONFIG> _config1;
                    public class TIKEHAU_FOLIO_COLUMNS_CONFIG
                    {
                        public string MODULE        {get; set;} 
                        public string NAME	        {get; set;} 
                        public string VALUE         {get; set;}
                        public string DESCRIPTION   {get; set;} 
                    }

                    public static List<TIKEHAU_FOLIO_COLUMNS_CONFIG> GetTikehauColumnConfig1()
                    {
                        var query = "SELECT * FROM TIKEHAU_SOPHIS_COLUMNS_CONFIG";
                        if (_config1 == null)
                        {
                            _config1 = ConnectionManager.OpenRisqueConnection().Query<TIKEHAU_FOLIO_COLUMNS_CONFIG>(query, null).ToList();
                        }
                        return _config1;
                    }

                    public static List<TIKEHAU_FOLIO_COLUMNS_CONFIG> GetTikehauConfigFromName(string colname)
                    {
                        try
                        {
                            var ret = GetTikehauColumnConfig1();
                            if (ret != null)
                            {
                                var res = ret.Where(p => p.NAME == colname.ToUpper()).ToList();
                                return res;
                            }
                            return new List<TIKEHAU_FOLIO_COLUMNS_CONFIG>();
                        }
                        catch (Exception ex)
                        {
                            return new List<TIKEHAU_FOLIO_COLUMNS_CONFIG>();
                        }
                    }

                      public static List<HISTO_MVTS> RetrieveTradeFromSophisOrderID(int sophisOrderId)
                {
                    var query = "SELECT * FROM HISTOMVTS WHERE SOPHIS_ORDER_ID = :SOPHIS_ORDER_ID";
                    var listofposition = new List<HISTO_MVTS>();
                    //@DPH: avoid using(...) which will close the global connection
                    //using (var connection = ConnectionManager.OpenRisqueConnection())
                    {
                        var connection = ConnectionManager.OpenRisqueConnection();
                        listofposition = connection.Query<HISTO_MVTS>(query, new { SOPHIS_ORDER_ID = sophisOrderId }).ToList();
                    }
                    return listofposition ;
                }

       //public static List<ORDER_MVTS> RetrieveTradeFromSophisOrderID(int sophisOrderId)
                //{
                //    var query = "SELECT * FROM HISTOMVTS WHERE SOPHIS_ORDER_ID = :sophisorderid";
                //    var listofposition = new List<HISTO_MVTS>();
                //    using (var connection = OracleConnectionManager.OpenRisqueConnection())
                //    {
                //        listofposition = connection.Query<HISTO_MVTS>(query, new { sophis_order_id = sophisOrderId }).ToList();
                //    }
                //    return listofposition;
                //}


                public static List<TIKEHAU_CONFIG> GetTikehauColumnConfig()
                {


                    var query = "SELECT * FROM TIKEHAU_CONFIG";
                    if (_config == null)
                    {
                        //@DPH: avoid using(...) which will close the global connection
                        //using (var connection = ConnectionManager.OpenRisqueConnection())
                        {
                            var connection = ConnectionManager.OpenRisqueConnection();
                            _config = connection.Query<TIKEHAU_CONFIG>(query, null).ToList();
                        }
                    }
                    return _config;
                }


                public static List<HISTO_MVTS> GetPositionInfoFromFolioidSicovam(int folioid, int sicovam)
                {
                    var query = @"WITH 
                                    LISTTRADES 
                                    As
                                    (
                                    SELECT *
                                    FROM JOIN_POSITION_HISTOMVTS H 
                                    WHERE H.type 
                                    IN (SELECT ID  FROM BUSINESS_EVENTS S where compta = 1 ) 
                                    and H.opcvm in (SELECT ident FROM FOLIO START WITH ident = :ident connect by mgr = prior ident)
                                    and h.sicovam = :sico
                                    ) 
                                    Select opcvm,sicovam,mvtident,sum(quantite) as nbsecurities from LISTTRADES group by opcvm,sicovam,mvtident";
                    //@DPH: avoid using(...) which will close the global connection
                    //using (var connection = ConnectionManager.OpenRisqueConnection())
                    {
                        var connection = ConnectionManager.OpenRisqueConnection();
                        return connection.Query<HISTO_MVTS>(query, new { ident = folioid, sico = sicovam }).ToList();
                    }
                }


                public static List<TIKEHAU_CONFIG> GetTikehauConfigFromID(int id)
                {
                    try
                    {
                        var ret = GetTikehauColumnConfig();
                        if (ret != null)
                        {
                            var res = ret.Where(p => p.MODULE == "SphOrderActivity" && p.LIST_POSITION == id).ToList();
                            return res;
                        }
                        return new List<TIKEHAU_CONFIG>();
                    }
                    catch (Exception ex)
                    {
                        return new List<TIKEHAU_CONFIG>();
                }
            }

            }
        }  
}

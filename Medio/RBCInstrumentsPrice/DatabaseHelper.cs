
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Oracle.DataAccess.Client;
//using sophis.configuration;
//using sophis.oms;
using System.IO;
using System.Configuration;


namespace RBCInstrumentsPrice
{
    public class CSxDBHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static string _connection = "";
        public static void Initialize()
        {
            // System.Configuration.Configuration exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            // var dbDetails=(sophis.configuration.DatabaseConnectionConfiguration)exeConfiguration.GetSection("Common/RisqueDatabase");
            //var dbDetails = (sophis.configuration.SophisConfigurationSection)exeConfiguration.GetSection("Common/RisqueDatabase");
            //_connection = dbDetails.ConnectionString;

            _connection = "Data Source=" + RBCConfigurationSectionGroup.RBCSectionConfig.Server +
                ";Persist Security Info=True;User ID=" + RBCConfigurationSectionGroup.RBCSectionConfig.User + ";Password=" + RBCConfigurationSectionGroup.RBCSectionConfig.Password;
        }

        public static Dictionary<string, DBInfo> LoadInstrumentsFromDB()
        {


            string sqlQuery = null;
            string instrSicovam = "";
            string instrIsin = "";
            string instrRef = "";
            string instrCcy = "";
            string instrAllotment = "";
            string instrName = "";
            string instrBBGRef = "";

            Dictionary<string, DBInfo> results = new Dictionary<string, DBInfo>();

            using (Oracle.DataAccess.Client.OracleConnection connection = new Oracle.DataAccess.Client.OracleConnection(_connection))
            {
                try
                {
                    using (OracleCommand myCommand = new OracleCommand())
                    {
                        sqlQuery = RBCConfigurationSectionGroup.RBCSectionConfig.InScopeQuery;
                        myCommand.CommandText = sqlQuery;

                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                            myCommand.Connection = connection;
                        }

                        using (OracleDataReader reader = myCommand.ExecuteReader())
                        {

                            log.Info("Listing instruments loaded from database. ");
                            int nbOfRows = 0;
                            while (reader.Read())
                            {
                                nbOfRows++;
                                instrSicovam = Convert.ToString(reader[0]);
                                instrAllotment = Convert.ToString(reader[1]);
                                instrCcy = Convert.ToString(reader[2]);
                                instrName = Convert.ToString(reader[3]);
                                instrRef = Convert.ToString(reader[4]);
                                instrIsin = Convert.ToString(reader[5]);
                                instrBBGRef = Convert.ToString(reader[6]);

                                string keyIdent = "";
                                if (instrAllotment != null)
                                {

                                    DBInfo instrumentItem = new DBInfo();
                                    instrumentItem.InstrumentSicovam = instrSicovam;
                                    instrumentItem.Allotment = instrAllotment;
                                    instrumentItem.Ccy = instrCcy;
                                    instrumentItem.InstrumentTickerRef = instrRef;
                                    instrumentItem.InstrumentIsin = instrIsin;
                                    instrumentItem.InstrumentName = instrName;
                                    instrumentItem.InstrumentBBGRef = instrBBGRef;

                                    keyIdent = instrSicovam;

                                    if (DataModel.fileAllotments["CUVAL"].Contains(instrAllotment)) //for CUVAL isin should be used as identifier
                                    {
                                        keyIdent = instrumentItem.InstrumentIsin;
                                    }
                                    if (DataModel.fileAllotments["FUTURES"].Contains(instrAllotment)) //for FUTURES ticker ref should be used as identifier
                                    {
                                        keyIdent = instrumentItem.InstrumentTickerRef;
                                    }

                                    if (DataModel.fileAllotments["OPTIONS"].Contains(instrAllotment)) //for OPTIONS records, we use ID_BB_UNIQUE or SICOVAM depending on the allotment
                                    {
                                        if (instrAllotment == "LISTED OPTION" || instrAllotment == "IR DERIVATIVE")
                                        {
                                            keyIdent = instrumentItem.InstrumentBBGRef;
                                        }

                                    }

                                    if (keyIdent != "")
                                    {
                                        if (results.ContainsKey(keyIdent) == false)
                                        {

                                            log.Info("DB Instrument details Name: " + instrumentItem.InstrumentName + " Ccy: " + instrumentItem.Ccy);

                                            results.Add(keyIdent, instrumentItem);
                                        }
                                    }
                                }
                            }

                            log.Info("Number of instruments found: " + nbOfRows);

                        }

                        connection.Close();
                    }
                }
                finally
                {
                    if (connection.State.Equals(System.Data.ConnectionState.Open))
                    {
                        connection.Close();
                    }
                }

                return results;
            }
        }


        public static void InsertPricesInDB()
        {
            using (Oracle.DataAccess.Client.OracleConnection connection = new Oracle.DataAccess.Client.OracleConnection(_connection))
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        //????
                        //TO CONFIRM SYSTEM DATE FORMAT AS 06-DEC-21
                        //TO CONFIRM IF THE DATE IN EXCEL FILES COMES ALWAYS AS DD/MM/YYYY

                        string sqlQuery = "merge into HISTORIQUE using dual on ( SICOVAM= :instrIdent  and JOUR = MEDIO_OPT_MINUS_DAYS(to_char(to_date(:instrNavDate,'DD/MM/YYYY'), 'DD-MON-RR'),1,STR_TO_NUM(:instrCCY))) " +
                            " WHEN MATCHED THEN UPDATE SET D = :price " +
                            " WHEN NOT MATCHED THEN " +
                            "INSERT (SICOVAM, D, JOUR) VALUES (:instrIdent,:price,MEDIO_OPT_MINUS_DAYS(to_char(to_date(:instrNavDate,'DD/MM/YYYY'), 'DD-MON-RR'),1,STR_TO_NUM(:instrCCY)))";

                        cmd.CommandText = sqlQuery;
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                            cmd.Connection = connection;
                        }

                        foreach (var matchedInst in DataModel.matchedInstruments)
                        {
                            int sicovam = 0;
                            string sicoString = "";
                            double adjustedPrice = matchedInst.Value.Price;

                            if (matchedInst.Value.InstrumentIdentType.Equals("SICOVAM"))
                            {
                                sicoString = matchedInst.Value.InstrumentIdent;
                            }
                            else
                            {
                                sicoString = DataModel.instrInScope[matchedInst.Value.InstrumentIdent].InstrumentSicovam;
                            }


                            if (DataModel.fileAllotments["multiply"].Contains(DataModel.instrInScope[matchedInst.Key].Allotment))
                            {
                                double orgPrice = matchedInst.Value.Price;
                                adjustedPrice = orgPrice * 100;
                                log.Info("Adjusting price(multiply) for instrument: " + matchedInst.Key + " Received Price: " + orgPrice + " Adjusted Price(Received Price*100): " + adjustedPrice);

                            }
                            else if (DataModel.fileAllotments["divide"].Contains(DataModel.instrInScope[matchedInst.Key].Allotment))
                            {
                                double orgPrice = matchedInst.Value.Price;
                                adjustedPrice = orgPrice / 100;
                                log.Info("Adjusting price(divide) for instrument: " + matchedInst.Key + " Received Price: " + orgPrice + " Adjusted Price(Received Price/100): " + adjustedPrice);
                            }

                            if (DataModel.instrInScope[matchedInst.Key].Ccy == "GBp" || DataModel.instrInScope[matchedInst.Key].Ccy == "ZAr" || DataModel.instrInScope[matchedInst.Key].Ccy == "ILs")
                            {
                                double orgPrice = adjustedPrice;
                                adjustedPrice = adjustedPrice * 100;
                                log.Info("Currency adjustment price ccy: " + DataModel.instrInScope[matchedInst.Key].Ccy + " instrument: " + matchedInst.Key + " Received Price: " + orgPrice + " Adjusted Price(Received Price*100): " + adjustedPrice);
                            }

                            if (Int32.TryParse(sicoString, out sicovam))
                            {
                                OracleParameter paramIdent = new OracleParameter(":instrIdent", sicovam);
                                OracleParameter paramPrice = new OracleParameter(":price", adjustedPrice);
                                OracleParameter paramCCY = new OracleParameter(":instrCCY", matchedInst.Value.Ccy);
                                OracleParameter paramNavDate = new OracleParameter(":instrNavDate", matchedInst.Value.NavDate);

                                List<OracleParameter> parameters = new List<OracleParameter>() { paramIdent, paramPrice, paramCCY, paramNavDate };
                                
                                if(parameters.Count!=0)
                                //if (!parameters.IsNullOrEmpty())
                                {
                                    cmd.BindByName = true;
                                    cmd.Parameters.Clear();
                                    foreach (var param in parameters)
                                    {
                                        cmd.Parameters.Add(param);
                                    }
                                }

                                if (adjustedPrice != 0)
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        connection.Close();
                    }
                }
                finally
                {
                    if (connection.State.Equals(System.Data.ConnectionState.Open))
                    {
                        connection.Close();
                    }
                }

            }
        }


    }
}

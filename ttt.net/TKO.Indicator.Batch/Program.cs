using System;
using System.Configuration;
using NSREnums;
using sophis.instrument;
using sophis.utils;

namespace TKO.Indicator.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var api = new CSMApi())
            {
                using (CSMLog logger = new CSMLog())
                {
                    try
                    {
                        logger.Write(CSMLog.eMVerbosity.M_info, "Main.BEGIN");
                        logger.Write(CSMLog.eMVerbosity.M_info, "Initializing Sophis...");
                        api.Initialise();
                        using (var cmd = Sophis.DataAccess.DBContext.Connection.CreateCommand())
                        {
                            cmd.CommandText = "SELECT INDICATOR_NAME FROM TKO_INDICATOR_NAME";
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    logger.Write(CSMLog.eMVerbosity.M_info, string.Format("INDICATOR_NAME={0}", reader.GetString(0)));
                                }
                            }
                        }						
                        logger.Write(CSMLog.eMVerbosity.M_info, "Initializing OK. Loading portfolio...");
                        api.LoadFolio();
                        logger.Write(CSMLog.eMVerbosity.M_info, "Loading OK. creating instance of instrument...");
                        var instrument = CSMInstrument.GetInstance(int.Parse(ConfigurationManager.AppSettings["UpdatingSicovam"]));
                        logger.Write(CSMLog.eMVerbosity.M_info, "OK. Saving...");
                        instrument.Save(eMParameterModificationType.M_pmModification);
                        logger.Write(CSMLog.eMVerbosity.M_info, "Saving OK. Closing...");
                        api.doClose();
                        logger.Write(CSMLog.eMVerbosity.M_info, "Closing OK.");
                    }
                    catch (Exception e)
                    {
                        logger.Write(CSMLog.eMVerbosity.M_error, e.ToString());
                    }
                    finally
                    {
                        logger.Write(CSMLog.eMVerbosity.M_info, "Main.END");
                    }
                }
            }
        }
    }
}

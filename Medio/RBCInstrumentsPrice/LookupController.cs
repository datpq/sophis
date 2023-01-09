using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RBCInstrumentsPrice
{
    public class LookupController
    {
        private static readonly log4net.ILog log
    = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static void LoadInstrumentsInScopeFromFusion()
        {
            log.Info("Loading instruments in scope");
            DataModel.instrInScope = CSxDBHelper.LoadInstrumentsFromDB();
            log.Info("Loading instruments completed");
        }


        public static void MatchInstrumentsFromFiles(string fileName)
        {
            //compare instrument from the db with instruments from files

            Dictionary<string, string> results = new Dictionary<string, string>();

            List<string> notFoundInstr = new List<string>();

            log.Info("Matching instruments list for " + fileName);
            string[] nameSplit = fileName.Split('_');
            string fileType = nameSplit[1];
            if (fileType == "POSITIONS") fileType = "OPTIONS";

            foreach (var fusionInstrument in DataModel.instrInScope)
            {
                if (DataModel.instrFromFiles.ContainsKey(fusionInstrument.Key))
                {
                    if (DataModel.instrFromFiles[fusionInstrument.Key].Ccy.ToUpper() == fusionInstrument.Value.Ccy.ToUpper())//CCY Validation
                    {
                        log.Info("Matched instrument details Ident: " + fusionInstrument.Key + " Allotment: " + fusionInstrument.Value.Allotment + " Price: " + DataModel.instrFromFiles[fusionInstrument.Key].Price);

                        if (DataModel.matchedInstruments.ContainsKey(fusionInstrument.Key) == false)
                        {
                            DataModel.matchedInstruments.Add(fusionInstrument.Key, DataModel.instrFromFiles[fusionInstrument.Key]);
                        }
                    }
                }

            }

            foreach (var item in DataModel.instrInScope)
            {
                if (DataModel.fileAllotments[fileType].Contains(item.Value.Allotment)) //checking if allotment is in scope for this file
                {
                    if (DataModel.matchedInstruments.Count == 0 || DataModel.matchedInstruments.ContainsKey(item.Key) == false)
                    {
                        string itemDetails = "Ident: " + item.Key + " Allotment: " + item.Value.Allotment + " Name: " + item.Value.InstrumentName;
                        notFoundInstr.Add(itemDetails);
                    }
                }
            }

            if (DataModel.matchedInstruments.Count == 0)
            {
                log.Info("No matching instruments found!");
            }
            else
            {
                CSxDBHelper.InsertPricesInDB();
                DataModel.matchedInstruments.Clear();
            }

            WriteNotFoundInstruments(notFoundInstr, fileName);
        }


        public static void WriteNotFoundInstruments(List<string> notFoundInstr, string fileName)
        {

            string outputFile = RBCConfigurationSectionGroup.RBCSectionConfig.FileForNotFoundInstruments;
            string outputAdj = outputFile.Split('.')[0];
            outputAdj = outputAdj + "_" + DateTime.Now.ToString("yyyyMMdd");
            outputAdj = outputAdj + ".log";

            using (StreamWriter writer = new StreamWriter(outputAdj, true))
            {
                writer.Write("[File " + fileName + "] Instruments not found are:");
                writer.Write(writer.NewLine);
                string timeStamp = DateTime.Now.ToLongDateString() + "_" + DateTime.Now.ToLongTimeString();
                foreach (string item in notFoundInstr)
                {

                    writer.Write(timeStamp + ": " + item);
                    writer.Write(writer.NewLine);
                }
            }
            notFoundInstr.Clear();

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;

namespace RBCInstrumentsPrice
{
    class FileWatcher : FileSystemWatcher
    {
        //FileSystemWatcher watcher;
        private readonly object mutex = new object();
        //  private Thread fTread;
        private int fPollPeriod = 1000;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void watch()
        {

            Path = RBCConfigurationSectionGroup.RBCSectionConfig.InputFolder;

            NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            Filter = "";

            Changed += onFileSystemChanged;
            EnableRaisingEvents = false;

            // worker
            //  fTread = new Thread(run) { Name = GetType().Name };
            // fTread.Start();
            run();

        }

        private void run()
        {
            {
                log.Info("Start listening for RBC files at the following location " + Path);
                while (true)
                {
                    lock (mutex)
                    {
                        try
                        {
                            getFiles();
                            Monitor.Wait(mutex, fPollPeriod);
                        }
                        catch (ThreadInterruptedException)
                        {
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                    }
                }

            }
        }

        private void getFiles()
        {

            try
            {
                EnableRaisingEvents = false;

                List<string> files = Directory.GetFiles(Path, Filter, SearchOption.TopDirectoryOnly).ToList();
                List<string> sortedFiles = files.OrderBy(f => Directory.GetLastWriteTimeUtc(f)).ToList();
                if (sortedFiles.Count != 0)
                {

                    foreach (string file in sortedFiles.ToArray())
                    {
                        string fileExtension = System.IO.Path.GetExtension(file);
                       
                        if (fileExtension == ".CSV")
                        {
                            bool correctFormat = true;
                            string fileName = System.IO.Path.GetFileName(file);
                            log.Info("Starting to process CSV file received: " + fileName);
                            string[] nameSplit = fileName.Split('_');


                            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                            {

                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    string fileType = nameSplit[1];
                                    if (fileType == "POSITIONS") fileType = "OPTIONS";//ex: 20211022175610_POSITIONS_OPTIONS.CSV

                                    int identIndex = DataModel.fieldsMap[fileType].InstrumentIdentifier;
                                    int bbgIdentIndex = DataModel.fieldsMap[fileType].InstrumentIdentBBG;
                                    int priceIndex = DataModel.fieldsMap[fileType].PriceField;
                                    int ccyIndex = DataModel.fieldsMap[fileType].Currency;
                                    int navDateIndex = DataModel.fieldsMap[fileType].NavDate;

                                    bool headerLine = true;
                                    string line;
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (headerLine)
                                        {
                                            headerLine = false;
                                        }
                                        else
                                        {
                                            string[] split = line.Split(';');
                                            if (split.Length > 1)
                                            {
                                                InstrumentInfo info = new InstrumentInfo();
                                                info.InstrumentIdentType = DataModel.fieldsMap[fileType].InstrumentIdentType;
                                                double priceConvert = 0;
                                                Double.TryParse(split[priceIndex], out priceConvert);
                                                info.Price = priceConvert;
                                                info.Ccy = split[ccyIndex];
                                                info.NavDate = split[navDateIndex];

                                                info.InstrumentIdent = split[identIndex];

                                                if (fileType == "OPTIONS")
                                                {
                                                    if (split[bbgIdentIndex].Equals("") == false)
                                                    {
                                                        info.InstrumentIdent = split[bbgIdentIndex];
                                                    }

                                                }


                                                if (info.InstrumentIdent.Equals("") == false)
                                                {
                                                    if (DataModel.instrFromFiles.ContainsKey(info.InstrumentIdent) == false)
                                                    {
                                                        DataModel.instrFromFiles.Add(info.InstrumentIdent, info);

                                                        log.Info("File instrument details Ident: " + info.InstrumentIdent + " IdentType:" + info.InstrumentIdentType + " Ccy: " + info.Ccy + " NavDate: " + info.NavDate + " Price: " + info.Price);

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                log.Error("File format is not correct.Please check the content: " + fileName);
                                                correctFormat = false;
                                                break;
                                            }
                                        }
                                    }


                                }
                            }
                            if (correctFormat == false)
                            {
                                string newName = RBCConfigurationSectionGroup.RBCSectionConfig.ProcessedFolder + "\\" + fileName + ".wrongFormat";
                                if (File.Exists(newName)) File.Delete(newName);
                                File.Move(file, newName);
                            }
                            if (File.Exists(file))
                            {
                                log.Info("Successfully processed file: " + fileName);
                                LookupController.MatchInstrumentsFromFiles(fileName);
                                string newName = RBCConfigurationSectionGroup.RBCSectionConfig.ProcessedFolder + "\\" + fileName + ".processed";
                                if (File.Exists(newName)) File.Delete(newName);
                                File.Move(file, newName);
                            }
                        }
                        else
                        {
                            log.Info("File extension is not .csv. Skipping file...");

                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Error encountered while processing files: " + e.Message);
                //throw;
            }


        }

        private void onFileSystemChanged(object source, FileSystemEventArgs e)
        {

            lock (mutex)
            {
                Monitor.PulseAll(mutex);
            }
        }
    }
}

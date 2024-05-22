using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using sophis.instrument;
using System.Linq;
using SophisETL.ISEngine;
using System.Xml;
using System.Timers;

namespace ToolkitService
{
    class TickerLines
    {
        public string Ticker { get; set; }
        public int TaskId { get; set; }
        public string Status {get; set;}
        public List<string> Lines { get; set; }
    }

    public static class DimOTCService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        const char SEPARATOR = ',';
        private static string HeaderLine;
        private static readonly object _requestsLock = new object();
        private static List<TickerLines> CreateInstrumentRequests = new List<TickerLines>();
        public static Func<string, Action<string, string>, string> CreateInstrument;

        public static void ProcessBlotterFile(string filePath)
        {
            try
            {
                Logger.Info($"DOS.ProcessBlotterFile.BEGIN(filePath={filePath})");
                Utils.CopyToDestFile(filePath, ConfigurationManager.AppSettings["DIM_OTC_Blotter_Trade"]);//Store the original blotter file for the trade ingestion ulterior

                var lines = File.ReadAllLines(filePath);
                Utils.MoveToDestFile(filePath, $"{filePath}.done");

                HeaderLine = lines[0];
                var csvSrcHeaders = HeaderLine.Split(SEPARATOR);

                lock (_requestsLock)
                {
                    var outputFiles = new Dictionary<string, Tuple<string, int, StreamWriter>>();
                    foreach (string outputFileKey in ConfigurationManager.AppSettings.AllKeys.Where(x => x.StartsWith("DIM_OTC_OutputFile_")).AsEnumerable())
                    {
                        try
                        {
                            string outputCsvFileTemp = $"{Path.GetTempPath()}{outputFileKey}_{Guid.NewGuid()}.csv";
                            var swOut = new StreamWriter(outputCsvFileTemp);
                            Logger.Info($"creating temporary file: {outputCsvFileTemp}");
                            swOut.WriteLine(HeaderLine);
                            outputFiles.Add(outputFileKey, new Tuple<string, int, StreamWriter>(outputCsvFileTemp, 0, swOut));
                        } catch(Exception e)
                        {
                            Logger.Error(e, $"Error while creating file: {outputFileKey}");
                        }
                    }
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var inputLine = lines[i];
                        Logger.Info($"line {i}/{lines.Length - 1}: {inputLine}");
                        var csvVals = inputLine.Split(SEPARATOR);
                        try
                        {
                            var productSubType = Utils.GetCsvVal(csvSrcHeaders, csvVals, "Product Sub Type");
                            string outputFileKey = $"DIM_OTC_OutputFile_{productSubType}";
                            var outputLineCount = outputFiles[outputFileKey].Item2;
                            var swOut = outputFiles[outputFileKey].Item3;
                            if (productSubType == "INTRTSWP") //IRS
                            {
                                var underlyingTicker = Utils.GetCsvVal(csvSrcHeaders, csvVals, "Underlying Ticker");
                                var request = CreateInstrumentRequests.SingleOrDefault(x => x.Ticker == underlyingTicker);
                                if (request == null)
                                {
                                    Logger.Info($"processing underlying ticker: {underlyingTicker}");
                                    //var isinComplexRef = new SSMComplexReference
                                    //{
                                    //    type = "TICKER",
                                    //    value = underlyingTicker
                                    //};
                                    //var sicovam = CSMInstrument.GetCodeWithMultiReference(isinComplexRef);
                                    var sicovam = CSMInstrument.GetCodeWithReference($"{underlyingTicker} Index");
                                    if (sicovam == 0)
                                    {
                                        request = new TickerLines
                                        {
                                            Ticker = underlyingTicker,
                                            TaskId = 0,
                                            Lines = new List<string>(new string[] { inputLine })
                                        };
                                        CreateInstrumentRequests.Add(request);
                                        Logger.Info($"Missing ISIN: {underlyingTicker} --> passing to DS");

                                        var externalReferenceType = ConfigurationManager.AppSettings["DIM_OTC_DS_ExternalReference_Type"];
                                        var dataSource = ConfigurationManager.AppSettings["DIM_OTC_DS_DataSource"];
                                        var securityType = ConfigurationManager.AppSettings["DIM_OTC_DS_SecurityType"];
                                        var user = ConfigurationManager.AppSettings["DIM_OTC_DS_User"];
                                        var createInstrumentLine = $"CreateInstrument;{underlyingTicker} Index;{externalReferenceType};{dataSource};Definition;{securityType};Instrument;{user}";
                                        var taskIdStr = CreateInstrument?.Invoke(createInstrumentLine, CreateInstrumentCallBack);
                                        Logger.Info($"TaskId={taskIdStr}");
                                        request.TaskId = int.Parse(taskIdStr);
                                    }
                                    else
                                    {
                                        Logger.Info($"Instrument exist {underlyingTicker} --> pass to DTS");
                                        swOut.WriteLine(inputLine);
                                        outputLineCount++;
                                    }
                                }
                                else if (request.Status == null)
                                {
                                    Logger.Info($"Creation of underlying instrument ({underlyingTicker}) is on going");
                                    request.Lines.Add(inputLine);
                                }
                                else
                                {
                                    Logger.Info($"Ticker {underlyingTicker} has been already created -- > pass to DTS");
                                    swOut.WriteLine(inputLine);
                                    outputLineCount++;
                                }
                            } else
                            {
                                swOut.WriteLine(inputLine);
                                outputLineCount++;
                            }
                            outputFiles[outputFileKey] = new Tuple<string, int, StreamWriter>(outputFiles[outputFileKey].Item1, outputLineCount, swOut);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, $"Error while processing line: {inputLine}");
                        }
                    }

                    foreach(var kvp in outputFiles)
                    {
                        try
                        {
                            Logger.Info($"Saving temporary file: {kvp.Value.Item1}");
                            kvp.Value.Item3.Close();
                            kvp.Value.Item3.Dispose();
                            if (kvp.Value.Item2 == 0)
                            {
                                Logger.Info($"No line transformed. Delete the temporary file {kvp.Value.Item1}...");
                                File.Delete(kvp.Value.Item1);
                            }
                            else
                            {
                                Utils.RunCommandLineWithOutputFile(ConfigurationManager.AppSettings["DIM_OTC_PostTransCommandLine"],
                                    ConfigurationManager.AppSettings["DIM_OTC_PostTransCommandLineArgs"], kvp.Value.Item1, Path.GetFileName(ConfigurationManager.AppSettings[kvp.Key]));
                                Utils.MoveToDestFile(kvp.Value.Item1, ConfigurationManager.AppSettings[kvp.Key]);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, $"Error while saving file: {kvp.Value.Item1}");
                        }
                    }
                }

                Logger.Info($"DOS.ProcessBlotterFile.END");
            } catch(Exception e)
            {
                Logger.Error(e, "DOS. Error while processing file");
            }
        }

        private static Timer TimerDOS;
        private static readonly object ListLocker = new object();
        private static List<string> ErrorList = new List<string>();
        private static List<string> InstrumentRefList = new List<string>();
        private static void InitializeTimer()
        {
            if (TimerDOS == null)
            {
                var timerInterval = int.Parse(ConfigurationManager.AppSettings["DIM_OTC_TimerDOSInterval"]);
                TimerDOS = new Timer(timerInterval * 1000 * 60);
                TimerDOS.Elapsed += TimerDOS_Elapsed;
                TimerDOS.AutoReset = true;
                TimerDOS.Enabled = true;
            }
        }

        public static void Stop()
        {
            if (TimerDOS != null)
            {
                Logger.Info("Stopping timer DOS...");
                TimerDOS.Stop();
                TimerDOS.Dispose();
            }
        }

        private static void DeferReportingError(string message)
        {
            InitializeTimer();
            TimerDOS.Enabled = false; //reset timer
            lock(ListLocker)
            {
                ErrorList.Add(message);
            }
            TimerDOS.Enabled = true;
        }

        private static void DeferSavingInstrumentRef(string message)
        {
            InitializeTimer();
            TimerDOS.Enabled = false; //reset timer
            lock (ListLocker)
            {
                InstrumentRefList.Add(message);
            }
            TimerDOS.Enabled = true;
        }

        private static void TimerDOS_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (ErrorList.Any())
                {
                    string errorFile = Path.GetTempPath() + "DOS_Error.csv";
                    errorFile = errorFile.PutTimestamp();
                    Logger.Info($"saving errors {ErrorList.Count} lines to: {errorFile}");
                    using (var swOut = new StreamWriter(errorFile))
                    {
                        swOut.WriteLine("TradeReference;Error");
                        lock (ListLocker)
                        {
                            foreach (var message in ErrorList) swOut.WriteLine(message);
                        }
                    }
                    try
                    {
                        Utils.SendErrorEmail("DOS Error", errorFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error when sending error email");
                    }
                    ErrorList.Clear();
                }

                if (InstrumentRefList.Any())
                {
                    var outputFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["DIM_OTC_Lookup_InstrumentRef"]);
                    outputFile = outputFile.PutTimestamp();
                    Logger.Info($"saving InstrumentRef file {InstrumentRefList.Count} lines to: {outputFile}");
                    using (var swOut = new StreamWriter(outputFile))
                    {
                        swOut.WriteLine("TradeReference;InstrumentRef");
                        lock (ListLocker)
                        {
                            foreach (var message in InstrumentRefList) swOut.WriteLine(message);
                        }
                    }
                    InstrumentRefList.Clear();
                    Utils.RunCommandLineWithOutputFile(ConfigurationManager.AppSettings["DIM_OTC_PostTransCommandLine"],
                        ConfigurationManager.AppSettings["DIM_OTC_PostTransCommandLineArgs"], outputFile); // Lookup file for the trade ingestion

                    Utils.RunCommandLineWithOutputFile(ConfigurationManager.AppSettings["DIM_OTC_PostTransCommandLine"],
                        ConfigurationManager.AppSettings["DIM_OTC_PostTransCommandLineArgs"], ConfigurationManager.AppSettings["DIM_OTC_Blotter_Trade"]); //Trade ingestion
                }
            } catch(Exception ex)
            {
                Logger.Error(ex, "Error Timer");
            }
        }

        public static void ProcessFileConnectorFile(string filePath)
        {
            try
            {
                Logger.Info($"DOS.ProcessFileConnectorFile.BEGIN(filePath={filePath})");
                var xmlMessage = File.ReadAllText(filePath);
                Utils.MoveToDestFile(filePath, $"{filePath}.done");

                IntegrationServiceEngine _ISinstance = IntegrationServiceEngine.Instance;
                var response = _ISinstance.Import(xmlMessage);
                string sResponse = response.ToString();

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true; // to preserve the same formatting
                doc.LoadXml(sResponse);
                var messageType = doc.DocumentElement.SelectSingleNode("@*[local-name() = 'type']")?.InnerText;
                Logger.Info($"messageType={messageType}");
                messageType = messageType.Split(':')[1];
                string tradeReference = Path.GetFileName(filePath).Split('_')[1];
                if (messageType == "ImportMessagePartiallyAccepted" || messageType == "MessageRejected")
                {
                    var messageException = doc.SelectSingleNode("//*[local-name() = 'exception']")?.InnerText;
                    Logger.Error($"Import message error: messageType={messageType}, messageException={messageException}");
                    DeferReportingError($"{tradeReference};{messageException}");
                } else if (messageType == "ImportMessageAccepted")
                {
                    var sophisId = doc.SelectSingleNode("//*[local-name() = 'sophis']")?.InnerText;
                    Logger.Info($"messageType={messageType}, sophisId={sophisId}");
                    DeferSavingInstrumentRef($"{tradeReference};{sophisId}");
                }

                //Logger.Info($"sResponse={sResponse}");

                Logger.Info($"DOS.ProcessFileConnectorFile.END");
            }
            catch (Exception e)
            {
                Logger.Error(e, "DOS. Error while processing file");
            }
        }

        public static void CreateInstrumentCallBack(string taskIdStr, string status)
        {
            Logger.Info($"CreateInstrumentCallBack.BEGIN({taskIdStr}, {status})");
            try
            {
                int taskId = int.Parse(taskIdStr);
                lock(_requestsLock)
                {
                    var request = CreateInstrumentRequests.SingleOrDefault(x => x.TaskId == taskId);
                    if (request == null)
                    {
                        throw new Exception($"Task {taskIdStr} is not found");
                    }
                    request.Status = status;
                    Logger.Info($"ISIN={request}, status={status}");
                    if (CreateInstrumentRequests.All(x => x.Status != null))
                    {
                        // the last ISIN created.
                        string outputCsvFileTemp = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
                        using (var swOut = new StreamWriter(outputCsvFileTemp))
                        {
                            swOut.WriteLine(HeaderLine);
                            foreach(var isinRequest in CreateInstrumentRequests)
                            {
                                foreach(var line in isinRequest.Lines)
                                {
                                    swOut.WriteLine(line);
                                }
                            }
                        }
                        Utils.RunCommandLineWithOutputFile(ConfigurationManager.AppSettings["DIM_OTC_PostTransCommandLine"],
                            ConfigurationManager.AppSettings["DIM_OTC_PostTransCommandLineArgs"], outputCsvFileTemp, Path.GetFileName(ConfigurationManager.AppSettings["DIM_OTC_OutputFile_INTRTSWP"]));
                        Utils.MoveToDestFile(outputCsvFileTemp, ConfigurationManager.AppSettings["DIM_OTC_OutputFile_INTRTSWP"]);
                    }
                }
            } catch(Exception e)
            {
                Logger.Error(e, "DOS. Error while calling back from ToolkitService");
            }
            Logger.Info($"CreateInstrumentCallBack.END");
        }
    }
}

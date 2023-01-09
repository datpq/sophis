using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

using SophisETL.Common.Logger;
using SophisETL.Common.GlobalSettings;
using SophisETL.Common.Tools;
using SophisETL.Common.Reporting;
using SophisETL.Common.ErrorMgt;
using SophisETL.Runtime;
using SophisETL.Runtime.ChainBuilder;
using SophisETL.Runtime.ReportBuilder;

using SophisETL.Xml;


namespace SophisETL
{
    /// <summary>
    /// Main Program to run the ETL
    /// </summary>
    public class Program
    {
        public static void Main( string[] args )
        {
            try
            {
                // Read all the Global Settings (necessary for file names variabilization)
                // This include the command line variables and those defined in the variables file
                GlobalSettings.Initialize(args);
                GlobalSettings globalSettings = GlobalSettings.Instance;

                // Load the General Settings File
                string settingsContentWithVariables = File.ReadAllText(globalSettings.ETLSettingsFile);
                string settingsContentWithoutVariables = globalSettings.ReplaceSettings(settingsContentWithVariables);
                XmlSerializer serializer = new XmlSerializer(typeof(Xml.SophisETL));
                Xml.SophisETL etlSettings = serializer.Deserialize(new StringReader(settingsContentWithoutVariables)) as Xml.SophisETL;

                // Initialise the logging subsystem
                LogManager log = LogManager.Instance;
                log.LogFileName = etlSettings.log.logFileName;
                log.TimeStampFormat = etlSettings.log.timeStampFormat;
                log.DebugMode = etlSettings.log.debugMode;
                log.WriteToDebugConsole = etlSettings.log.alsoOnConsole;
                log.ClearFile = etlSettings.log.clearFile;

                // Official start (in the logs)
                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                log.Log(String.Format("[MAIN] Sophis ETL Framework v{0} starting", currentVersion));

                // Create the Chain
                ETLChainDefinition xmlChainDefintion = etlSettings.chain;
                ETLChainBuildDirector chainBuildDirector = new ETLChainBuildDirector();
                ETLChain chain = chainBuildDirector.BuildChain(xmlChainDefintion);
                if (chain == null)
                {
                    log.Log("[MAIN] ERROR - Chain could not be constructed, aborting ");
                    Environment.Exit(1);
                }

                // Create the Reports and attach them to the chain and to the Error Manager
                List<IReportingHandler> reportingHandlers = new List<IReportingHandler>();
                if (etlSettings.report != null)
                {
                    ReportingHandlerBuilder reportBuilder = new ReportingHandlerBuilder();
                    foreach (Report reportDefinition in etlSettings.report)
                    {
                        IReportingHandler reportingHandler = reportBuilder.BuildReportingHandler(reportDefinition);
                        reportingHandlers.Add(reportingHandler);
                        chain.AttachReportingHandler(reportingHandler);
                        ErrorHandler.Instance.AttachReportingHandler(reportingHandler);
                    }
                }


                log.Log(String.Format("[MAIN] Initializing Chain {0}", chain.Name));
                if (!chain.Init())
                {
                    log.Log(String.Format("[MAIN] Chain {0} could not be initialized, aborting", chain.Name));
                    Environment.Exit(1);
                }

                log.Log("[MAIN] Initializing Reporting Handlers");
                reportingHandlers.ForEach(r => r.Init());

                log.Log(String.Format("[MAIN] Starting Chain {0}", chain.Name));
                chain.Start();
                //chain.WaitForCompletion();

                log.Log(String.Format("[MAIN] Chain {0} steps completed, cleaning-up", chain.Name));
                chain.Dispose();
                reportingHandlers.ForEach(r => r.Dispose());

                // If all went fine, Code 0 will be returned, otherwise we issue a last warning
                if (chain.ChainInError)
                {
                    log.Log(String.Format("[MAIN] Chain {0} completed on General Error", chain.Name));
                    Environment.ExitCode = 1;
                }
                else
                    log.Log(String.Format("[MAIN] Chain {0} successfully completed", chain.Name));
            }
            catch (Exception ex)
            {
                // Log and on console as well (logging subsystem might be unavailable)
                string errorMessage = String.Format("[MAIN] Ended with Failure: {0}\n----------\n{1}", ex.Message, ex.ToString());
                LogManager.Instance.Log(errorMessage);
                System.Console.Out.WriteLine(errorMessage);
                Environment.Exit(1);
            }
        }

        public static bool RunGUIMode(string[] args, ref string errMessage)
        {
            bool res = false;
            try
            {
                // Read all the Global Settings (necessary for file names variabilization)
                // This include the command line variables and those defined in the variables file
                GlobalSettings.Initialize(args);
                GlobalSettings globalSettings = GlobalSettings.Instance;

                // Load the General Settings File
                string settingsContentWithVariables = File.ReadAllText(globalSettings.ETLSettingsFile);
                string settingsContentWithoutVariables = globalSettings.ReplaceSettings(settingsContentWithVariables);
                XmlSerializer serializer = new XmlSerializer(typeof(Xml.SophisETL));
                Xml.SophisETL etlSettings = serializer.Deserialize(new StringReader(settingsContentWithoutVariables)) as Xml.SophisETL;

                // Initialise the logging subsystem
                LogManager log = LogManager.Instance;
                log.LogFileName = etlSettings.log.logFileName;
                log.TimeStampFormat = etlSettings.log.timeStampFormat;
                log.DebugMode = etlSettings.log.debugMode;
                log.WriteToDebugConsole = etlSettings.log.alsoOnConsole;
                log.ClearFile = etlSettings.log.clearFile;

                // Official start (in the logs)
                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                log.Log(String.Format("[MAIN] Sophis ETL Framework v{0} starting", currentVersion));

                // Create the Chain
                ETLChainDefinition xmlChainDefintion = etlSettings.chain;
                ETLChainBuildDirector chainBuildDirector = new ETLChainBuildDirector();
                ETLChain chain = chainBuildDirector.BuildChain(xmlChainDefintion);
                if (chain == null)
                {
                    log.Log("[MAIN] ERROR - Chain could not be constructed, aborting ");
                    return res;
                }

                // Create the Reports and attach them to the chain and to the Error Manager
                List<IReportingHandler> reportingHandlers = new List<IReportingHandler>();
                if (etlSettings.report != null)
                {
                    ReportingHandlerBuilder reportBuilder = new ReportingHandlerBuilder();
                    foreach (Report reportDefinition in etlSettings.report)
                    {
                        IReportingHandler reportingHandler = reportBuilder.BuildReportingHandler(reportDefinition);
                        reportingHandlers.Add(reportingHandler);
                        chain.AttachReportingHandler(reportingHandler);
                        ErrorHandler.Instance.AttachReportingHandler(reportingHandler);
                    }
                }


                log.Log(String.Format("[MAIN] Initializing Chain {0}", chain.Name));
                if (!chain.Init())
                {
                    errMessage = String.Format("[MAIN] Chain {0} could not be initialized, aborting", chain.Name);
                    log.Log(String.Format("[MAIN] Chain {0} could not be initialized, aborting", chain.Name));
                    return res;
                }

                log.Log("[MAIN] Initializing Reporting Handlers");
                reportingHandlers.ForEach(r => r.Init());

                log.Log(String.Format("[MAIN] Starting Chain {0}", chain.Name));
                chain.Start();
                //chain.WaitForCompletion();

                log.Log(String.Format("[MAIN] Chain {0} steps completed, cleaning-up", chain.Name));
                chain.Dispose();
                reportingHandlers.ForEach(r => r.Dispose());

                // If all went fine, Code 0 will be returned, otherwise we issue a last warning
                if (chain.ChainInError)
                {
                    errMessage = String.Format("[MAIN] Chain {0} completed on General Error", chain.Name);
                    log.Log(String.Format("[MAIN] Chain {0} completed on General Error", chain.Name));
                    return res;
                }
                else
                    log.Log(String.Format("[MAIN] Chain {0} successfully completed", chain.Name));

                res = true;
            }
            catch (Exception ex)
            {
                // Log and on console as well (logging subsystem might be unavailable)
                errMessage = String.Format("[MAIN] Ended with Failure: {0}\n----------\n{1}", ex.Message, ex.ToString());
                LogManager.Instance.Log(errMessage);
                return res;
            }
            return res;
        }
    }
}

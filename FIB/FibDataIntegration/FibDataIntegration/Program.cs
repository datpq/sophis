using FibDataIntegration.Services;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace FibDataIntegration
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        //@SBO : BEGIN
        //private static readonly string XmlTemplate = ConfigurationManager.AppSettings["XmlTemplate"];
        //private static readonly string RepeatingNode = ConfigurationManager.AppSettings["RepeatingNode"];
        //private static readonly string OutputFile = ConfigurationManager.AppSettings["OutputFile"];
        // ---> DAT
        //SBO : END

        private static readonly string OutputDir = ConfigurationManager.AppSettings["OutputDir"];
        private static readonly string BackupDir = ConfigurationManager.AppSettings["BackupDir"];
        private static readonly DateTime FirstExecTime = DateTime.ParseExact(
            ConfigurationManager.AppSettings["FirstExecTime"], "HH:mm", CultureInfo.InvariantCulture);
        private static readonly int TimerInterval = int.Parse(ConfigurationManager.AppSettings["Interval"]);
        private static readonly bool ImmediateProcessing = bool.Parse(ConfigurationManager.AppSettings["ImmediateProcessing"]);
        private static bool _isWaitingForFirstExec = true;
        private static System.Timers.Timer _aTimer;
        private static IEnumerable<DateTime> _holidays;

        //@SBO : BEGIN
            //XMl Templates
        private static readonly string YCXmlTemplate = ConfigurationManager.AppSettings["YCXmlTemplate"];
        private static readonly string IRSPXmlTemplate = ConfigurationManager.AppSettings["IRSPXmlTemplate"];
        private static readonly string VLXmlTemplate = ConfigurationManager.AppSettings["VLXmlTemplate"];
        private static readonly string CDSXmlTemplate = ConfigurationManager.AppSettings["CDSXmlTemplate"];
        private static readonly string FXXmlTemplate = ConfigurationManager.AppSettings["FXXmlTemplate"];

            //Excel Templates
        private static readonly string YCExcelTemplate = ConfigurationManager.AppSettings["YCExcelTemplate"];
        private static readonly string IRExcelTemplate = ConfigurationManager.AppSettings["IRExcelTemplate"];
        private static readonly string SPExcelTemplate = ConfigurationManager.AppSettings["SPExcelTemplate"];
        private static readonly string VLExcelTemplate = ConfigurationManager.AppSettings["VLExcelTemplate"];
        private static readonly string CDSExcelTemplate = ConfigurationManager.AppSettings["CDSExcelTemplate"];
        private static readonly string FXExcelTemplate = ConfigurationManager.AppSettings["FXExcelTemplate"];

            //OutPut Files
        private static readonly string YCOutputFile = ConfigurationManager.AppSettings["YCOutputFile"];
        private static readonly string IROutputFile = ConfigurationManager.AppSettings["IROutputFile"];
        private static readonly string SPOutputFile = ConfigurationManager.AppSettings["SPOutputFile"];
        private static readonly string VLOutputFile = ConfigurationManager.AppSettings["VLOutputFile"];
        private static readonly string CDSOutputFile = ConfigurationManager.AppSettings["CDSOutputFile"];
        private static readonly string FXOutputFile = ConfigurationManager.AppSettings["FXOutputFile"];

            //Yield Curves
        private static readonly string YCRepeatingNodeByYieldCurve = ConfigurationManager.AppSettings["YCRepeatingNodeByYieldCurve"];
        private static readonly string YCRepeatingNodeByPoint = ConfigurationManager.AppSettings["YCRepeatingNodeByPoint"];
        private static readonly string YCCurrencyNode = ConfigurationManager.AppSettings["YCCurrencyNode"];
        private static readonly string YCFamilyNode = ConfigurationManager.AppSettings["YCFamilyNode"];
        private static readonly string YCNameNode = ConfigurationManager.AppSettings["YCNameNode"];
        private static readonly string YCPeriodMultiplierNode = ConfigurationManager.AppSettings["YCPeriodMultiplierNode"];
        private static readonly string YCPeriodEnumNode = ConfigurationManager.AppSettings["YCPeriodEnumNode"];
        private static readonly string YCRateNode = ConfigurationManager.AppSettings["YCRateNode"];

            //Interest Rates & Stock Prices
        private static readonly string IRSPRepeatingNodeBySICOVAM = ConfigurationManager.AppSettings["IRSPRepeatingNodeBySICOVAM"];
        private static readonly string IRSPRepeatingNodeByLast = ConfigurationManager.AppSettings["IRSPRepeatingNodeByLast"];
        private static readonly string IRSPSICOVAMNode = ConfigurationManager.AppSettings["IRSPSICOVAMNode"];
        private static readonly string IRSPDateNode = ConfigurationManager.AppSettings["IRSPDateNode"];
        private static readonly string IRSPLastNode = ConfigurationManager.AppSettings["IRSPLastNode"];
        private static readonly string IRSPIdentifierNode = ConfigurationManager.AppSettings["IRSPIdentifierNode"];

            //Volatility
        private static readonly string VLRepeatingNodeByYieldCurve = ConfigurationManager.AppSettings["VLRepeatingNodeByYieldCurve"];
        private static readonly string VLRepeatingNodeByPoint = ConfigurationManager.AppSettings["VLRepeatingNodeByPoint"];
        private static readonly string VLSicovamNode = ConfigurationManager.AppSettings["VLSicovamNode"];
        private static readonly string VLPeriodMultiplierNode = ConfigurationManager.AppSettings["VLPeriodMultiplierNode"];
        private static readonly string VLPeriodEnumNode = ConfigurationManager.AppSettings["VLPeriodEnumNode"];
        private static readonly string VLRateNode = ConfigurationManager.AppSettings["VLRateNode"];

            //CDS
        private static readonly string CDSRepeatingNode = ConfigurationManager.AppSettings["CDSRepeatingNode"];
        private static readonly string CDSSICOVAMNode = ConfigurationManager.AppSettings["CDSSICOVAMNode"];
        private static readonly string CDScurrencyNode = ConfigurationManager.AppSettings["CDScurrencyNode"];
        private static readonly string CDSseniorityNode = ConfigurationManager.AppSettings["CDSseniorityNode"];
        private static readonly string CDSdefaultEventNode = ConfigurationManager.AppSettings["CDSdefaultEventNode"];
        private static readonly string CDSmaturiryNode = ConfigurationManager.AppSettings["CDSmaturiryNode"];
        private static readonly string CDSrateNode = ConfigurationManager.AppSettings["CDSrateNode"];
        private static readonly string CDSbidRateNode = ConfigurationManager.AppSettings["CDSbidRateNode"];
        private static readonly string CDSaskRateNode = ConfigurationManager.AppSettings["CDSaskRateNode"];
        private static readonly string CDSdayCountBasisNode = ConfigurationManager.AppSettings["CDSdayCountBasisNode"];
        private static readonly string CDSyieldCalculationNode = ConfigurationManager.AppSettings["CDSyieldCalculationNode"];
        private static readonly string CDSperiodicityTypeNode = ConfigurationManager.AppSettings["CDSperiodicityTypeNode"];
        private static readonly string CDSconfidenceNode = ConfigurationManager.AppSettings["CDSconfidenceNode"];
        private static readonly string CDSisUsedNode = ConfigurationManager.AppSettings["CDSisUsedNode"];
        private static readonly string CDSreferenceNode = ConfigurationManager.AppSettings["CDSreferenceNode"];

            //Forex Rates
        private static readonly string FXRepeatingNode = ConfigurationManager.AppSettings["FXRepeatingNode"];
        private static readonly string FXCurrency1 = ConfigurationManager.AppSettings["FXCurrency1"];
        private static readonly string FXCurrency2 = ConfigurationManager.AppSettings["FXCurrency2"];
        private static readonly string FXCurrency = ConfigurationManager.AppSettings["FXCurrency"];
        private static readonly string FXQuoteBasis = ConfigurationManager.AppSettings["FXQuoteBasis"];
        private static readonly string FXValue = ConfigurationManager.AppSettings["FXValue"];

            //End Of Day
        private static readonly string BatchFile = ConfigurationManager.AppSettings["BatchFile"];

        //SBO : END

        static void Main(string[] args)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            NinjectManager.Wire(new ApplicationModule());
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Backup");
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Output");
            if (args.Length > 1)
            {
                PrintHelp();
                return;
            }
            if (args.Length == 1)
            {
                DateTime execDate;
                if (!DateTime.TryParseExact(args[0], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out execDate))
                {
                    PrintHelp();
                    return;
                }

                Logger.Info("Initializing...");
                Logger.Info("FirstExecTime={0:HH:mm}, Interval={1}", FirstExecTime, TimerInterval);

                Logger.Info("Syntax OK. Execution on {0:dd/MM/yyyy}", execDate);
                ProcessOnDate(execDate);
            }
            else
            {
                Logger.Info("Initializing...");
                Logger.Info("FirstExecTime={0:HH:mm}, Interval={1}", FirstExecTime, TimerInterval);

                _holidays = GetHolidays();
                _aTimer = new System.Timers.Timer(1000 * 60); // a minute
                _aTimer.Elapsed += OnTimedEvent;
                _aTimer.AutoReset = true;
                _aTimer.Enabled = true;
                if (ImmediateProcessing)
                {
                    OnTimedEvent(null, null);
                }
            }
            //ProcessOnDate();
            Console.ReadLine();
            if (_aTimer != null)
            {
                _aTimer.Stop();
                _aTimer.Dispose();
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: {0} [DD/MM/YYYY]", AppDomain.CurrentDomain.FriendlyName);
            Console.WriteLine("When the option [DD/MM/YYYY] is specified, the program will be executed once on the specified date.");
            Console.WriteLine("When there's no option, the program will be executed in timer mode on the date of D-1 which is not a holiday.");
            Console.WriteLine("The interval and the first execution time are specified in App.config.");
        }

        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            Logger.Debug("OnTimedEvent.BEGIN");
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var signalTime = DateTime.Now;
            //signalTime = e.SignalTime;
            if (!_isWaitingForFirstExec || (signalTime >= FirstExecTime && FirstExecTime > signalTime.AddMilliseconds(-_aTimer.Interval)) || e == null)
            {
                var execDate = GetLastWorkingDay();
                Logger.Info("Start processing at {0:HH:mm} on {1:dd/MM/yyyy}", signalTime, execDate);
                if (_isWaitingForFirstExec && (signalTime >= FirstExecTime && FirstExecTime > signalTime.AddMilliseconds(-_aTimer.Interval)) && e != null)
                {
                    Logger.Debug("First time execution. Setting new timer interval to {0}", TimerInterval);
                    _aTimer.Interval = TimerInterval * 60 * 1000;
                    _isWaitingForFirstExec = false;
                }
                ProcessOnDate(execDate);
            }
            Logger.Debug("OnTimedEvent.END");
        }

        private static DateTime GetLastWorkingDay()
        {
            var result = DateTime.Today.AddDays(-1);
            while (result.DayOfWeek == DayOfWeek.Saturday
                   || result.DayOfWeek == DayOfWeek.Sunday
                   || _holidays.Any(x => x == result))
            {
                result = result.AddDays(-1);
            }
            return result;
        }

        private static List<DateTime> GetHolidays()
        {
            var result = new List<DateTime>();
            try
            {
                using (var con = new OracleConnection(ConfigurationManager.AppSettings["OracleConnectionString"]))
                {
                    con.Open();
                    using (var command = new OracleCommand(ConfigurationManager.AppSettings["HolidaySqlQuery"], con))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(reader.GetDateTime(0));
                            }
                        }
                    }
                }
                result.Sort();
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return result;
            }
            finally
            {
                Logger.Debug("List of holidays:");
                foreach (var dateTime in result)
                {
                    Logger.Debug("{0:dd/MM/yyyy}", dateTime);
                }
            }
        }

        //@SBO : BEGIN

        /*
        private static string GetOutputFile()
        {
            var outputFileExt = Path.GetExtension(OutputFile) ?? string.Empty;
            var result = OutputFile.Substring(0, OutputFile.Length - outputFileExt.Length);
            var idx = result.IndexOf('_');
            if (idx >= 0)
            {
                result = result.Substring(0, idx + 1) + DateTime.Now.ToString(result.Substring(idx + 1));
            }
            else
            {
                result = DateTime.Now.ToString(result);
            }
            result = result + outputFileExt;
            return result;
        }
        */
            // ---> DAT
        private static string GetOutputFile(string OutputFile)
        {
            var outputFileExt = Path.GetExtension(OutputFile) ?? string.Empty;
            var result = OutputFile.Substring(0, OutputFile.Length - outputFileExt.Length);
            var idx = result.IndexOf('_');
            if (idx >= 0)
            {
                result = result.Substring(0, idx + 1) + DateTime.Now.ToString(result.Substring(idx + 1));
            }
            else
            {
                result = DateTime.Now.ToString(result);
            }
            result = result + outputFileExt;
            return result;
        }

        //SBO : END

        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void ProcessOnDate(DateTime execDate)
        {
            try
            {
                Logger.Info("Processing on date {0:dd/MM/yyyy}", execDate);
                var dataStore = NinjectManager.Resolve<IDataStore>();
                dataStore.GetRateCurve(execDate).ContinueWith(task =>
                {
                    try
                    {

                        Logger.Debug("Processing result...");
                        if (task.Result == null)
                        {
                            Logger.Error("Error getting data. Result = null");
                            return;
                        }

                        if (!task.Result.Any())
                        {
                            Logger.Warn("No data found on {0:dd/MM/yyyy}", execDate);
                            //return;
                        }

                        //@SBO : BEGIN

                        var excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };

                        //I - YieldCurve
                        if (File.Exists(YCExcelTemplate))
                        {
                            Logger.Info("Yield Curve Market Data Start Integration");

                            var xDoc = XDocument.Load(YCXmlTemplate);
                            
                            var wb = excel.Workbooks.Open(YCExcelTemplate);
                            // Update of Excel's Macros
                            excel.Run("Module1.EUR_OIS");
                            Logger.Info("Macro : Module1.EUR_OIS updated");

                            excel.Run("EUR_SWAP");
                            Logger.Info("Macro : EUR_SWAP updated");

                            excel.Run("Module1.EUR_USD");
                            Logger.Info("Macro : Module1.EUR_USD updated");

                            excel.Run("Module1.GOLD");
                            Logger.Info("Macro : Module1.GOLD updated");

                            excel.Run("Module1.JPY_OIS");
                            Logger.Info("Macro : Module1.JPY_OIS updated");

                            excel.Run("JPY_SWAP");
                            Logger.Info("Macro : JPY_SWAP updated");

                            excel.Run("Module1.JPY_USD");
                            Logger.Info("Macro : Module1.JPY_USD updated");

                            excel.Run("Module1.SOFR");
                            Logger.Info("Macro : Module1.SOFR updated");

                            excel.Run("USD_OIS");
                            Logger.Info("Macro : USD_OIS updated");

                            excel.Run("USD_SWAP");
                            Logger.Info("Macro : USD_SWAP updated");

                            wb.Save();
                            wb.Close();
                            //excel.Quit();

                            Logger.Info("Yield Curves Macros updated");

                            //excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            wb = excel.Workbooks.Open(YCExcelTemplate);
                            

                            //I.1 - The Two worksheets that we work with
                            var ws1 = wb.Worksheets[1];// ---> List of YieldCurves
                            var ws2 = wb.Worksheets[2];// ---> Inputs : (Date, Rate)

                            //I.2 - Searching the RepeatingNodeByYieldCurve, RepeatingNodebyPoint & parentYieldCurveNode
                            var repeatingNodeByYieldCurve = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(YCRepeatingNodeByYieldCurve));
                            if (repeatingNodeByYieldCurve == null)
                            {
                                throw new Exception("Repeating node market not found");
                            }
                            var repeatingNodeByPoint = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(YCRepeatingNodeByPoint));
                            if (repeatingNodeByPoint == null)
                            {
                                throw new Exception("Repeating node point not found");
                            }
                            var parentYieldCurveNode = repeatingNodeByYieldCurve.Parent;
                            if (parentYieldCurveNode == null)
                            {
                                throw new Exception("Parent node market not found");
                            }
                            //parentYieldCurveNode.RemoveNodes();
                            repeatingNodeByYieldCurve.Remove();
                            var parentNodeByPoint = repeatingNodeByPoint.Parent;
                            if (parentNodeByPoint == null)
                            {
                                throw new Exception("Points node not found");
                            }
                            //I.3 - Error from FileConnector: Key is not unique and couldn't be inserted into map
                            var distinctResult = task.Result.GroupBy(x => x.Date).Select(x => x.First()).OrderBy(x => x.Date);
                            //I.4 - Loop for every YieldCurve
                            var cell1 = ws1.Cells[2, 1].Value2;
                            int i = 2;
                            while (cell1 != null)
                            {
                                //Treatement for every YieldCurve
                                //I.4.1 - Get the value of ID, Currency, Family and the Name
                                string id1 = ws1.Cells[i, 1].Value2.ToString();
                                string currency = ws1.Cells[i, 3].Value2.ToString();
                                string family = ws1.Cells[i, 4].Value2.ToString();
                                string name = ws1.Cells[i, 2].Value2.ToString();
                                //I.4.2 - Creation of new Tag market ---> for this YieldCurve
                                var newYieldCurveNode = new XElement(repeatingNodeByYieldCurve);
                                //I.4.3 - Put the value of Currency, Family and Name inside the Node market
                                newYieldCurveNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(YCCurrencyNode)).Value = currency;
                                newYieldCurveNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(YCFamilyNode)).Value = family;
                                newYieldCurveNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(YCNameNode)).Value = name;
                                //I.4.4 - Search the node Points (Parent of node Point) and remove all childs of this node
                                var nodePoint = newYieldCurveNode.DescendantNodes().OfType<XElement>()
                                        .FirstOrDefault(x => x.Name.LocalName.Equals(YCRepeatingNodeByPoint));
                                var parentPointNode = nodePoint.Parent;
                                parentPointNode.RemoveNodes();
                                //I.4.5 - Loop for the Inputs for this YieldCurve
                                var cell2 = ws2.Cells[2, 1].Value2;
                                int j = 2;
                                while (cell2 != null)
                                {
                                    //Treatement for every line in Worksheet2
                                    //I.4.5.1 - Get the value of ID, Date, & Rate from this line
                                    string id2 = ws2.Cells[j, 1].Value2.ToString();
                                    string date = ws2.Cells[j, 2].Value2.ToString();
                                    string rate = ws2.Cells[j, 3].Value2.ToString();
                                    //I.4.5.2 - if this input is for the current YieldCurve
                                    if (id1.Equals(id2))
                                    {
                                        //I.4.5.2.1 - Create a new Node (point) containt the data (Date,Rate)
                                        var newPoint = new XElement(repeatingNodeByPoint);
                                        //I.4.5.2.2 - Set the Value of Date
                                        string pm = date.Substring(0, date.Length - 1).ToString();
                                        string pe = "";
                                        switch (date.Substring(date.Length - 1).ToLower())
                                        {
                                            case "d":
                                                pe = "Day";
                                                break;

                                            case "m":
                                                pe = "Month";
                                                break;

                                            case "y":
                                                pe = "Year";
                                                break;
                                        }
                                        newPoint.DescendantNodes().OfType<XElement>()
                                            .Single(x => x.Name.LocalName.Equals(YCPeriodMultiplierNode)).Value = pm;
                                        newPoint.DescendantNodes().OfType<XElement>()
                                            .Single(x => x.Name.LocalName.Equals(YCPeriodEnumNode)).Value = pe;
                                        //I.4.5.2.3 - Set the Value of Rate
                                        newPoint.DescendantNodes().OfType<XElement>()
                                            .Single(x => x.Name.LocalName.Equals(YCRateNode)).Value = rate;
                                        //I.4.5.2.4 - Add this node Point to the ParentNode Points ---> parentPointNode
                                        parentPointNode.Add(newPoint);
                                        //I.4.5.2.5 - END of 5.2
                                    }
                                    //I.4.5.3 - Move the Index j to the next Line of this second worksheet
                                    j++;
                                    cell2 = ws2.Cells[j, 1].Value2;
                                    //I.4.5.4 - END of I.4.5
                                }
                                //I.4.6 - Add this node newYieldCurveNode to the parentYieldCurveNode
                                parentYieldCurveNode.Add(newYieldCurveNode);
                                //I.4.7 - Move the Index i to the next Line of this first worksheet
                                i++;
                                cell1 = ws1.Cells[i, 1].Value2;
                                //I.4.8 - END
                            }
                            //I.5 - Save the xDoc
                            var outputFile = GetOutputFile(YCOutputFile);
                            //var outputFile = GetOutputFileSicovam(ws1.Cells[i, 3].Value2.ToString());
                            xDoc.Save(BackupDir + "\\" + outputFile);
                            File.Copy(BackupDir + "\\" + outputFile, OutputDir + "\\" + outputFile);
                            Logger.Info("New file {0} has been generated.", OutputDir + "\\" + outputFile);

                            //I.6 - Close of Excel File

                            wb.Close();
                            //excel.Quit();

                            //I.7 - END
                            Logger.Info("Yield Curve Market Data End Integration");


                        }
                        else Logger.Info("Excel Template for Yield Curve doesn\'t exist");

                        //II - Interset Rate
                        if (File.Exists(IRExcelTemplate))
                        {
                            Logger.Info("Interest Rates Market Data Start Integration");

                            var xDoc = XDocument.Load(IRSPXmlTemplate);

                            //var excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            var wb = excel.Workbooks.Open(IRExcelTemplate);
                            excel.Run("IR_Refresh_Date");
                            Logger.Info("Macro IR_Refresh_Date updated");

                            wb.Save();
                            wb.Close();
                            //excel.Quit();

                            Logger.Info("Interest Rates Macros updated");

                            //excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            wb = excel.Workbooks.Open(IRExcelTemplate);

                            //II.1 - The Two worksheets that we work with (ws1 for list of SICOVAM and ws2 for the Input Data (Date,Last))
                            var ws1 = wb.Worksheets[1];
                            var ws2 = wb.Worksheets[2];
                            //II.2 - Searching the RepeatingNodeBySICOVAM, RepeatingNodeByLast, identifierNode & parentNode
                            var repeatingNodeBySICOVAM = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(IRSPRepeatingNodeBySICOVAM));
                            if (repeatingNodeBySICOVAM == null)
                            {
                                throw new Exception("Repeating node quotationsByInstrument not found");
                            }
                            var repeatingNodeByLast = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(IRSPRepeatingNodeByLast));
                            if (repeatingNodeByLast == null)
                            {
                                throw new Exception("Repeating node quotesByDate not found");
                            }
                            var identifierNode = xDoc.DescendantNodes().OfType<XElement>()
                                    .FirstOrDefault(x => x.Name.LocalName.Equals(IRSPIdentifierNode));
                            if (identifierNode == null)
                            {
                                throw new Exception("Identifier node not found");
                            }
                            var parentNode = repeatingNodeBySICOVAM.Parent;
                            if (parentNode == null)
                            {
                                throw new Exception("Parent node not found");
                            }
                            parentNode.RemoveNodes();

                            //II.3 - Error from FileConnector: Key is not unique and couldn't be inserted into map
                            var distinctResult = task.Result.GroupBy(x => x.Date).Select(x => x.First()).OrderBy(x => x.Date);
                            //II.4 - Loop for every SICOVAM
                            var cell1 = ws1.Cells[2, 1].Value2;
                            int i = 2;
                            while (cell1 != null)
                            {
                                //Treatement for every SICOVAM
                                //II.4.1 - Value of Test_ID and the SICOVAM
                                string test_ID1 = ws1.Cells[i, 1].Value2.ToString();
                                var sicovamCell = ws1.Cells[i, 3].Value2;
                                var test_ID2Cell = ws2.Cells[i, 1].Value2;
                                var sDateCell = ws2.Cells[i, 2].Value2;
                                var lastCell = ws2.Cells[i, 3].Value2;



                                if ((sicovamCell != null) &&
                                    (test_ID2Cell != null) &&
                                    (sDateCell != null) &&
                                    (lastCell != null) &&
                                    (test_ID1.Equals(test_ID2Cell.ToString())))
                                {
                                    string sicovam = ws1.Cells[i, 3].Value2.ToString();
                                    string sDate = ws2.Cells[i, 2].Value2.ToString();
                                    //string sDate = (xlRange.Cells[4, 3] as Excel.Range).Value2.ToString();
                                    double dDate = double.Parse(sDate);
                                    string date = DateTime.FromOADate(dDate).ToString("yyyy-MM-dd");

                                    string last = ws2.Cells[i, 3].Value2.ToString();

                                    //II.4.2 - Creation of new Tag quotationsByInstrument and new Tag identifier (for this Sicovam)
                                    var newNodeSicovam = new XElement(repeatingNodeBySICOVAM);
                                    newNodeSicovam.RemoveNodes();
                                    var newIdentifier = new XElement(identifierNode);
                                    newIdentifier.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(IRSPSICOVAMNode)).Value = sicovam;
                                    newNodeSicovam.Add(newIdentifier);
                                    

                                    //II.4.3.2.1 - We must create a new Node containt Date and Last
                                    var newNode = new XElement(repeatingNodeByLast);
                                    //II.4.3.2.2 - Add Value of Date to OutputXML in Tag 'quotationDate'
                                    newNode.DescendantNodes().OfType<XElement>()
                                    .Single(x => x.Name.LocalName.Equals(IRSPDateNode)).Value = date;
                                    //II.4.3.2.3 - Add Value of Last to OutputXML in Tag 'value'
                                    newNode.DescendantNodes().OfType<XElement>()
                                            .Single(x => x.Name.LocalName.Equals(IRSPLastNode)).Value = last;
                                    //II.4.3.2.4 - We add this newNode to the Tag 'quotationsByInstrument'
                                    newNodeSicovam.Add(newNode);

                                    //II.4.4 - Add the newNodeSicovam to parentNode
                                    parentNode.Add(newNodeSicovam);
                                }

                                
                                //II.4.5 - Move the Index i to the next Line of this first worksheet
                                i++;
                                cell1 = ws1.Cells[i, 1].Value2;
                                //II.4.6 - END
                            }
                            //II.5 - Save the xDoc
                            var outputFile = GetOutputFile(IROutputFile);
                            //var outputFile = GetOutputFileSicovam(ws1.Cells[i, 3].Value2.ToString());
                            xDoc.Save(BackupDir + "\\" + outputFile);
                            File.Copy(BackupDir + "\\" + outputFile, OutputDir + "\\" + outputFile);
                            Logger.Info("New file {0} has been generated.", OutputDir + "\\" + outputFile);

                            //II.6 - Close of Excel File
                            wb.Close();
                            //excel.Quit();

                            //II.7 - END
                            Logger.Info("Interest Rates Market Data End Integration");

                        }
                        else Logger.Info("Excel Template for Interest Rates doesn\'t exist");

                        //III - Stock prices
                        if (File.Exists(SPExcelTemplate))
                        {
                            Logger.Info("Stock Prices Market Data Start Integration");

                            var xDoc = XDocument.Load(IRSPXmlTemplate);

                            //var excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            var wb = excel.Workbooks.Open(SPExcelTemplate);
                            
                            excel.Run("Macro1");
                            Logger.Info("Macro Macro1 updated");
                            wb.Save();
                            wb.Close();
                            //excel.Quit();

                            Logger.Info("Stock Prices Macros updated");

                            //excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            wb = excel.Workbooks.Open(SPExcelTemplate);
                            

                            //III.1 - The Two worksheets that we work with (ws1 for list of SICOVAM and ws2 for the Input Data (Date,Last))
                            var ws1 = wb.Worksheets[1];
                            var ws2 = wb.Worksheets[2];
                            //III.2 - Searching the RepeatingNodeBySICOVAM, RepeatingNodeByLast, identifierNode & parentNode
                            var repeatingNodeBySICOVAM = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(IRSPRepeatingNodeBySICOVAM));
                            if (repeatingNodeBySICOVAM == null)
                            {
                                throw new Exception("Repeating node quotationsByInstrument not found");
                            }
                            var repeatingNodeByLast = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(IRSPRepeatingNodeByLast));
                            if (repeatingNodeByLast == null)
                            {
                                throw new Exception("Repeating node quotesByDate not found");
                            }
                            var identifierNode = xDoc.DescendantNodes().OfType<XElement>()
                                    .FirstOrDefault(x => x.Name.LocalName.Equals(IRSPIdentifierNode));
                            if (identifierNode == null)
                            {
                                throw new Exception("Identifier node not found");
                            }
                            var parentNode = repeatingNodeBySICOVAM.Parent;
                            if (parentNode == null)
                            {
                                throw new Exception("Parent node not found");
                            }
                            parentNode.RemoveNodes();

                            //III.3 - Error from FileConnector: Key is not unique and couldn't be inserted into map
                            var distinctResult = task.Result.GroupBy(x => x.Date).Select(x => x.First()).OrderBy(x => x.Date);
                            //III.4 - Loop for every SICOVAM
                            var cell1 = ws1.Cells[2, 1].Value2;
                            int i = 2;
                            while (cell1 != null)
                            {

                                //Treatement for every SICOVAM
                                //II.4.1 - Value of Test_ID and the SICOVAM
                                string test_ID1 = ws1.Cells[i, 1].Value2.ToString();
                                var sicovamCell = ws1.Cells[i, 3].Value2;
                                var test_ID2Cell = ws2.Cells[i, 1].Value2;
                                var sDateCell = ws2.Cells[i, 2].Value2;
                                var lastCell = ws2.Cells[i, 3].Value2;



                                if ((sicovamCell != null) &&
                                    (test_ID2Cell != null) &&
                                    (sDateCell != null) &&
                                    (lastCell != null) &&
                                    (test_ID1.Equals(test_ID2Cell.ToString())))
                                {
                                    string sicovam = ws1.Cells[i, 3].Value2.ToString();
                                    string sDate = ws2.Cells[i, 2].Value2.ToString();
                                    //string sDate = (xlRange.Cells[4, 3] as Excel.Range).Value2.ToString();
                                    double dDate = double.Parse(sDate);
                                    string date = DateTime.FromOADate(dDate).ToString("yyyy-MM-dd");

                                    string last = ws2.Cells[i, 3].Value2.ToString();

                                    //II.4.2 - Creation of new Tag quotationsByInstrument and new Tag identifier (for this Sicovam)
                                    var newNodeSicovam = new XElement(repeatingNodeBySICOVAM);
                                    newNodeSicovam.RemoveNodes();
                                    var newIdentifier = new XElement(identifierNode);
                                    newIdentifier.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(IRSPSICOVAMNode)).Value = sicovam;
                                    newNodeSicovam.Add(newIdentifier);


                                    //II.4.3.2.1 - We must create a new Node containt Date and Last
                                    var newNode = new XElement(repeatingNodeByLast);
                                    //II.4.3.2.2 - Add Value of Date to OutputXML in Tag 'quotationDate'
                                    newNode.DescendantNodes().OfType<XElement>()
                                    .Single(x => x.Name.LocalName.Equals(IRSPDateNode)).Value = date;
                                    //II.4.3.2.3 - Add Value of Last to OutputXML in Tag 'value'
                                    newNode.DescendantNodes().OfType<XElement>()
                                            .Single(x => x.Name.LocalName.Equals(IRSPLastNode)).Value = last;
                                    //II.4.3.2.4 - We add this newNode to the Tag 'quotationsByInstrument'
                                    newNodeSicovam.Add(newNode);

                                    //II.4.4 - Add the newNodeSicovam to parentNode
                                    parentNode.Add(newNodeSicovam);
                                }


                                //II.4.5 - Move the Index i to the next Line of this first worksheet
                                i++;
                                cell1 = ws1.Cells[i, 1].Value2;
                                //II.4.6 - END

                                
                            }
                            //III.5 - Save the xDoc
                            var outputFile = GetOutputFile(SPOutputFile);
                            //var outputFile = GetOutputFileSicovam(ws1.Cells[i, 3].Value2.ToString());
                            xDoc.Save(BackupDir + "\\" + outputFile);
                            File.Copy(BackupDir + "\\" + outputFile, OutputDir + "\\" + outputFile);
                            Logger.Info("New file {0} has been generated.", OutputDir + "\\" + outputFile);

                            //III.6 - Close of Excel File
                            wb.Close();
                            //excel.Quit();

                            //III.7 - END
                            Logger.Info("Stock Prices Market Data End Integration");
                        }
                        else Logger.Info("Excel Template for Stock Prices doesn\'t exist");

                        //IV - Volatility
                        if (File.Exists(VLExcelTemplate))
                        {
                            Logger.Info("Volatility Market Data Start Integration");

                            var xDoc = XDocument.Load(VLXmlTemplate);
                            
                            //var excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            var wb = excel.Workbooks.Open(VLExcelTemplate);


                            excel.Run("Macro2");
                            Logger.Info("Macro Macro2 updated");

                            wb.Save();
                            
                            //wb.SheetTableUpdate
                            wb.Close();
                            //excel.Quit();

                            Logger.Info("Volatility Macros updated");

                            //excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            wb = excel.Workbooks.Open(VLExcelTemplate);


                            //IV.1 - The Two worksheets that we work with
                            var ws1 = wb.Worksheets[1];// ---> List of YieldCurves
                            var ws2 = wb.Worksheets[2];// ---> Inputs : (Date, Rate)

                            //IV.2 - Searching the RepeatingNodeByYieldCurve, RepeatingNodebyPoint & parentYieldCurveNode
                            var repeatingNodeByYieldCurve = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(VLRepeatingNodeByYieldCurve));
                            if (repeatingNodeByYieldCurve == null)
                            {
                                throw new Exception("Repeating node market not found");
                            }
                            var repeatingNodeByPoint = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(VLRepeatingNodeByPoint));
                            if (repeatingNodeByPoint == null)
                            {
                                throw new Exception("Repeating node point not found");
                            }
                            var parentYieldCurveNode = repeatingNodeByYieldCurve.Parent;
                            if (parentYieldCurveNode == null)
                            {
                                throw new Exception("Parent node market not found");
                            }
                            //parentYieldCurveNode.RemoveNodes();
                            repeatingNodeByYieldCurve.Remove();
                            var parentNodeByPoint = repeatingNodeByPoint.Parent;
                            if (parentNodeByPoint == null)
                            {
                                throw new Exception("Points node not found");
                            }
                            //IV.3 - Error from FileConnector: Key is not unique and couldn't be inserted into map
                            var distinctResult = task.Result.GroupBy(x => x.Date).Select(x => x.First()).OrderBy(x => x.Date);
                            //IV.4 - Loop for every YieldCurve
                            var cell1 = ws1.Cells[2, 1].Value2;
                            int i = 2;
                            while (cell1 != null)
                            {
                                //Treatement for every YieldCurve
                                //IV.4.1 - Get the value of ID, Currency, Family and the Name
                                string id1 = ws1.Cells[i, 1].Value2.ToString();
                                //string currency = ws1.Cells[i, 3].Value2.ToString();
                                //string family = ws1.Cells[i, 4].Value2.ToString();
                                //string name = ws1.Cells[i, 2].Value2.ToString();
                                string sicovam = ws1.Cells[i, 3].Value2.ToString();
                                //IV.4.2 - Creation of new Tag market ---> for this YieldCurve
                                var newYieldCurveNode = new XElement(repeatingNodeByYieldCurve);
                                //IV.4.3 - Put the value of Currency, Family and Name inside the Node market
                                newYieldCurveNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(VLSicovamNode)).Value = sicovam;

                                //IV.4.4 - Search the node Points (Parent of node Point) and remove all childs of this node
                                var nodePoint = newYieldCurveNode.DescendantNodes().OfType<XElement>()
                                        .FirstOrDefault(x => x.Name.LocalName.Equals(VLRepeatingNodeByPoint));
                                var parentPointNode = nodePoint.Parent;
                                nodePoint.Remove();
                                //IV.4.5 - Loop for the Inputs for this YieldCurve
                                var cell2 = ws2.Cells[2, 1].Value2;
                                int j = 2;
                                while (cell2 != null)
                                {
                                    //Treatement for every line in Worksheet2
                                    //IV.4.5.1 - Get the value of ID, Date, & Rate from this line
                                    string id2 = ws2.Cells[j, 1].Value2.ToString();
                                    string date = ws2.Cells[j, 2].Value2.ToString();
                                    string rate = ws2.Cells[j, 3].Value2.ToString();
                                    //IV.4.5.2 - if this input is for the current YieldCurve
                                    if (id1.Equals(id2))
                                    {
                                        //IV.4.5.2.1 - Create a new Node (point) containt the data (Date,Rate)
                                        var newPoint = new XElement(repeatingNodeByPoint);
                                        //IV.4.5.2.2 - Set the Value of Date
                                        string pm = date.Substring(0, date.Length - 1).ToString();
                                        string pe = "";
                                        switch (date.Substring(date.Length - 1).ToLower())
                                        {
                                            case "d":
                                                pe = "Day";
                                                break;

                                            case "m":
                                                pe = "Month";
                                                break;

                                            case "y":
                                                pe = "Year";
                                                break;
                                        }
                                        newPoint.DescendantNodes().OfType<XElement>()
                                            .Single(x => x.Name.LocalName.Equals(VLPeriodMultiplierNode)).Value = pm;
                                        newPoint.DescendantNodes().OfType<XElement>()
                                            .Single(x => x.Name.LocalName.Equals(VLPeriodEnumNode)).Value = pe;
                                        //IV.4.5.2.3 - Set the Value of Rate
                                        newPoint.DescendantNodes().OfType<XElement>()
                                            .Single(x => x.Name.LocalName.Equals(VLRateNode)).Value = rate;
                                        //IV.4.5.2.4 - Add this node Point to the ParentNode Points ---> parentPointNode
                                        parentPointNode.Add(newPoint);
                                        //IV.4.5.2.5 - END of 5.2
                                    }
                                    //IV.4.5.3 - Move the Index j to the next Line of this second worksheet
                                    j++;
                                    cell2 = ws2.Cells[j, 1].Value2;
                                    //IV.4.5.4 - END of 5
                                }
                                //IV.4.6 - Add this node newYieldCurveNode to the parentYieldCurveNode
                                parentYieldCurveNode.Add(newYieldCurveNode);
                                //IV.4.7 - Move the Index i to the next Line of this first worksheet
                                i++;
                                cell1 = ws1.Cells[i, 1].Value2;
                                //IV.4.8 - END of IV.4
                            }
                            //IV.5 - Save the xDoc
                            var outputFile = GetOutputFile(VLOutputFile);
                            //var outputFile = GetOutputFileSicovam(ws1.Cells[i, 3].Value2.ToString());
                            xDoc.Save(BackupDir + "\\" + outputFile);
                            File.Copy(BackupDir + "\\" + outputFile, OutputDir + "\\" + outputFile);
                            Logger.Info("New file {0} has been generated.", OutputDir + "\\" + outputFile);

                            //IV.6 - Close of Excel File

                            wb.Close();
                            //excel.Quit();

                            //IV.7 - END

                            Logger.Info("Volatility Market Data End Integration");
                        }
                        else Logger.Info("Excel Template for Volatility doesn\'t exist");

                        //V - CDS
                        if (File.Exists(CDSExcelTemplate))
                        {
                            Logger.Info("CDS Market Data Start Integration");

                            var xDoc = XDocument.Load(CDSXmlTemplate);

                            //var excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            var wb = excel.Workbooks.Open(CDSExcelTemplate);

                            excel.Run("Bumps");
                            Logger.Info("Macro Bumps updated");
                            wb.Save();
                            wb.Close();
                            //excel.Quit();

                            Logger.Info("CDS Macros updated");

                            //excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            wb = excel.Workbooks.Open(CDSExcelTemplate);


                            //V.1 - The Two worksheets that we work with (ws1 for list of SICOVAM and ws2 for the Input Data (Date,Last))
                            var ws1 = wb.Worksheets[1];
                            //var ws2 = wb.Worksheets[2];
                            
                            //V.2 - Searching the RepeatingNodeBySICOVAM, RepeatingNodeByLast, identifierNode & parentNode
                            var repeatingNode = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(CDSRepeatingNode));
                            if (repeatingNode == null)
                            {
                                throw new Exception("Repeating node currency not found");
                            }
                            var parentNode = repeatingNode.Parent;
                            if (parentNode == null)
                            {
                                throw new Exception("Parent node not found");
                            }
                            parentNode.RemoveNodes();
                            //repeatingNode.Remove();

                            //V.3 - Error from FileConnector: Key is not unique and couldn't be inserted into map
                            var distinctResult = task.Result.GroupBy(x => x.Date).Select(x => x.First()).OrderBy(x => x.Date);
                            //V.4 - Loop for every SICOVAM
                            var cell1 = ws1.Cells[2, 1].Value2;
                            int i = 2;
                            while (cell1 != null)
                            {
                                //Treatement for every SICOVAM
                                //V.4.1 - Value of Test_ID and the SICOVAM
                                //string test_ID1 = ws1.Cells[i, 1].Value2.ToString();
                                //string sicovam = ws1.Cells[i, 3].Value2.ToString();
                                var currencycell = ws1.Cells[i, 2].Value2;
                                var senioritycell = ws1.Cells[i, 3].Value2;
                                var defaultcell = ws1.Cells[i, 4].Value2;
                                var maturiycell = ws1.Cells[i, 5].Value2;
                                var cdsRatecell = ws1.Cells[i, 6].Value2;
                                var bidRatececll = ws1.Cells[i, 7].Value2;
                                var askRatecell = ws1.Cells[i, 8].Value2;
                                var dayCountcell = ws1.Cells[i, 9].Value2;
                                var yieldCalculationcell = ws1.Cells[i, 10].Value2;
                                var frequencycell = ws1.Cells[i, 11].Value2;
                                var confidencecell = ws1.Cells[i, 12].Value2;
                                var isUsedcell = ws1.Cells[i, 13].Value2;
                                //V.4.2 - Creation of new Tag quotationsByInstrument and new Tag identifier (for this Sicovam)
                                if (currencycell != null && senioritycell != null && defaultcell != null && maturiycell != null)
                                {
                                    var newNode = new XElement(repeatingNode);
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDScurrencyNode)).Value = currencycell.ToString();
                                    //all nodes
                                    var nodetempsen = newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSseniorityNode));
                                    nodetempsen.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSreferenceNode)).Value = senioritycell.ToString();
                                    //newNode.DescendantNodes().OfType<XElement>()
                                    //  .Single(x => x.Name.LocalName.Equals(seniorityNode)).Value = senioritycell.ToString();
                                    //newNode.DescendantNodes().OfType<XElement>()
                                    // .Single(x => x.Name.LocalName.Equals(currencyNode)).DescendantNodes().OfType<XElement>()
                                    // .Single(x => x.Name.LocalName.Equals(referenceNode)).Value =;
                                    var nodetempdef = newNode.DescendantNodes().OfType<XElement>()
                                      .Single(x => x.Name.LocalName.Equals(CDSdefaultEventNode));
                                    nodetempdef.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSreferenceNode)).Value = defaultcell.ToString();
                                    //newNode.DescendantNodes().OfType<XElement>()
                                    //  .Single(x => x.Name.LocalName.Equals(defaultEventNode)).Value = defaultcell.ToString(); 
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSmaturiryNode)).Value = maturiycell.ToString();
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSrateNode)).Value = cdsRatecell.ToString();
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSbidRateNode)).Value = bidRatececll.ToString();
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSaskRateNode)).Value = askRatecell.ToString();
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSdayCountBasisNode)).Value = dayCountcell.ToString();
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSyieldCalculationNode)).Value = yieldCalculationcell.ToString();
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSperiodicityTypeNode)).Value = frequencycell.ToString();
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSconfidenceNode)).Value = confidencecell.ToString();
                                    newNode.DescendantNodes().OfType<XElement>()
                                        .Single(x => x.Name.LocalName.Equals(CDSisUsedNode)).Value = isUsedcell.ToString();
                                    //
                                    parentNode.Add(newNode);
                                }

                                //V.4.3 - Loop for the Inputs for this SICOVAM                                          
                                //V.4.5 - Move the Index i to the next Line of this first worksheet
                                i++;
                                cell1 = ws1.Cells[i, 1].Value2;
                                //V.4.6 - END of V.4
                            }
                            //V.5 - Save the xDoc
                            var outputFile = GetOutputFile(CDSOutputFile);
                            //var outputFile = GetOutputFileSicovam(ws1.Cells[i, 3].Value2.ToString());
                            xDoc.Save(BackupDir + "\\" + outputFile);
                            File.Copy(BackupDir + "\\" + outputFile, OutputDir + "\\" + outputFile);
                            Logger.Info("New file {0} has been generated.", OutputDir + "\\" + outputFile);

                            //V.6 - Close of Excel File
                            wb.Close();
                            //excel.Quit();

                            //V.7 - END

                            Logger.Info("CDS Market Data End Integration");
                        }
                        else Logger.Info("Excel Template for CDS doesn\'t exist");

                        //VI - Forex
                        if (File.Exists(FXExcelTemplate))
                        {
                            Logger.Info("Forex Market Data Start Integration");

                            var xDoc = XDocument.Load(FXXmlTemplate);

                            //var excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            var wb = excel.Workbooks.Open(FXExcelTemplate);

                            excel.Run("FX_update");
                            Logger.Info("Macro FX_update updated");
                            wb.Save();
                            wb.Close();
                            //excel.Quit();

                            Logger.Info("Forex Macros updated");

                            //excel = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                            wb = excel.Workbooks.Open(FXExcelTemplate);

                            //VI.1 - The Two worksheets that we work with (ws1 for list of FX Rates Definitions and ws2 for FX Rates Values)
                            var ws1 = wb.Worksheets[1];
                            var ws2 = wb.Worksheets[2];

                            //VI.2 - Searching the RepeatingNode & parentNode
                            var repeatingNode = xDoc.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(FXRepeatingNode));
                            if (repeatingNode == null)
                            {
                                throw new Exception("Repeating node not found");
                            }
                            var parentNode = repeatingNode.Parent;
                            if (parentNode == null)
                            {
                                throw new Exception("Parent node not found");
                            }
                            repeatingNode.Remove();
                            //parentNode.RemoveNodes();

                            //VI.3 - Error from FileConnector: Key is not unique and couldn't be inserted into map
                            var distinctResult = task.Result.GroupBy(x => x.Date).Select(x => x.First()).OrderBy(x => x.Date);

                            //VI.4 - Loop for every FX Rates (From the first Worksheet)
                            var cell1 = ws1.Cells[2, 1].Value2;
                            int i = 2;
                            while (cell1 != null)
                            {
                                //Treatement for every FX Rate Definition
                                //VI.4.1 - Value of the ID and cells of Currency1, Currency2, QuoteBasis, Date and Last
                                string id1 = ws1.Cells[i, 1].Value2.ToString();
                                var currency1Cell = ws1.Cells[i, 2].Value2;
                                var currency2Cell = ws1.Cells[i, 3].Value2;
                                var quoteBasisCell = ws1.Cells[i, 4].Value2;
                                var id2Cell = ws2.Cells[i, 1].Value2;
                                var dateCell = ws2.Cells[i, 2].Value2;
                                var lastCell = ws2.Cells[i, 3].Value2;


                                //VI.4.2 - If id1 = id2 ===> Get the values of all Datas and add it to the XML File
                                if ((id2Cell != null) &&
                                    (id1.Equals(id2Cell.ToString())) &&
                                    (currency1Cell != null) &&
                                    (currency2Cell != null) &&
                                    (quoteBasisCell != null) &&
                                    (dateCell != null) &&
                                    (lastCell != null))
                                {
                                    //VI.4.2.1 - Creation of new Tag RepreatingNode (for this FX Rate) ---> 'market'
                                    var newNode = new XElement(repeatingNode);
                                    //VI.4.2.2 - Value of the Currency1, Currency2, QuoteBasis, Date and Last
                                    string currency1 = currency1Cell.ToString();
                                    string currency2 = currency2Cell.ToString();
                                    string quoteBasis = quoteBasisCell.ToString();
                                    string sDate = dateCell.ToString();
                                    double dDate = double.Parse(sDate);
                                    string date = DateTime.FromOADate(dDate).ToString("yyyy-MM-dd");
                                    string last = lastCell.ToString();
                                    //VI.4.2.3 - Add Value of Currency1,Currency2, QuoteBasis, Value and Currency to OutputXML
                                    newNode.DescendantNodes().OfType<XElement>()
                                    .Single(x => x.Name.LocalName.Equals(FXCurrency1)).Value = currency1;

                                    newNode.DescendantNodes().OfType<XElement>()
                                    .Single(x => x.Name.LocalName.Equals(FXCurrency2)).Value = currency2;

                                    newNode.DescendantNodes().OfType<XElement>()
                                    .Single(x => x.Name.LocalName.Equals(FXQuoteBasis)).Value = quoteBasis;

                                    newNode.DescendantNodes().OfType<XElement>()
                                    .Single(x => x.Name.LocalName.Equals(FXCurrency)).Value = currency1;

                                    newNode.DescendantNodes().OfType<XElement>()
                                    .Single(x => x.Name.LocalName.Equals(FXValue)).Value = last;

                                    //VI.4.2.4 - We add this newNode to parentNode
                                    parentNode.Add(newNode);
                                }

                                //VI.4.3 - Move the Index i to the next Line of this first worksheet
                                i++;
                                cell1 = ws1.Cells[i, 1].Value2;

                                //VI.4.4 - END of VI.4
                            }

                            //VI.5 - Save the xDoc
                            var outputFile = GetOutputFile(FXOutputFile);
                            //var outputFile = GetOutputFileSicovam(ws1.Cells[i, 3].Value2.ToString());
                            xDoc.Save(BackupDir + "\\" + outputFile);
                            File.Copy(BackupDir + "\\" + outputFile, OutputDir + "\\" + outputFile);
                            Logger.Info("New file {0} has been generated.", OutputDir + "\\" + outputFile);

                            //VI.6 - Close of Excel File
                            wb.Close();

                            //VI.7 - END
                            Logger.Info("Forex Market Data End Integration");
                        }
                        else Logger.Info("Excel Template for Forex doesn\'t exist");

                        excel.Quit();

                        Logger.Info("Market Data Integration is Done");

                        //VII - EOD
                        Logger.Info("EOD start");
                        System.Diagnostics.Process Proc = new System.Diagnostics.Process();
                        if (File.Exists(BatchFile))
                        {
                            Proc.StartInfo.FileName = BatchFile;
                            Proc.Start();
                            Proc.Close();
                        }
                        else Logger.Info("Batch file for End Of Day doesn\'t exist");

                        
                        
                        //System.Diagnostics.Process.Start(@"C: \Users\Salah\Documents\MyProjects\MDT\MDT_Final_Tool_EOD\batchEOD_1.bat");

                        //SBO : END

                    }
                    catch (IOException)
                    {
                        Logger.Error("Error when accessing the XML template file.");
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Unknown error when processing the returning data");
                    }
                },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error when running {0}", e.Message);
            }
        }

    }
}

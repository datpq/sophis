using sophis.scenario;
using sophis.log;
using System;
using System.Collections.Generic;
using System.IO;
using Sophis.DataAccess;
using MEDIO.CORE.Tools;
using sophis.static_data;
using System.Linq;

namespace MEDIO.BackOffice.net.src.Scenario
{
    class CSxSSBOTCMatchingScenario : sophis.scenario.CSMScenario
    {
        private readonly string[] IngestionTypes = new string[] { "CSA.", "CC23", "MIOTC23" };
        private readonly Dictionary<string, string> IngestionTypesDict = new Dictionary<string, string>();
        private static Dictionary<string, bool> IngestionTypesAreActive = new Dictionary<string, bool>();
        private static string _ClassName = typeof(CSxSSBOTCMatchingScenario).Name;
        private static string InputFilter = "SSB_DAILY_*.csv";
        private static string InputFolder = "";
        private static string OutputFolder = "";
        private static string SSBFundCodesTableName = "MEDIO_SSB_OTC_FUND_CODES";
        private static string SSBDataPointsTableName = "MEDIO_SSB_OTC_DATA_POINTS";
        private static string cashCCYSicovam = "67943599";//Cash for CCY EUR

        private static string SleeveFundCodes = "";
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pUserPreference;
        }


        public override void Initialise()
        {
            Logger logger = new Logger(_ClassName, nameof(Run));
            logger.log(Severity.info, "Initializing the Ingestion Flows Dictionary");
            logger.log($"fParam = {fParam}");
            IngestionTypesDict["CSA."] = "COLOUT";
            IngestionTypesDict["CC23"] = "COLOUTCC";
            IngestionTypesDict["MIOTC23"] = "COLOUTIMCC";
            InitializeIngestionTypesAreActives();
            if (!string.IsNullOrEmpty(fParam.StringValue)) {
                InputFolder = fParam.StringValue.Split(';')[0];
                OutputFolder = fParam.StringValue.Split(';')[1];
                logger.log(Severity.info, $"InputFolder = {InputFolder} ; OutputFolder = {OutputFolder}");
            } else
            {
                logger.log(Severity.error, $"The Input Folder and the Output should be provided as scenario's parameters");
            }
            GetSleeveFundCodes();
            logger.Dispose();
        }
        private static void GetSleeveFundCodes()
        {
            Logger logger = new Logger(_ClassName, nameof(GetSleeveFundCodes));
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = "select custody_fund_code from " + SSBFundCodesTableName + " where custody_fund_code is not null";
                logger.log(Severity.info, $"Executing the query : {cmd.CommandText}");
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0)) SleeveFundCodes += reader.GetString(0) + ",";
                }
                logger.log(Severity.info, $"Result of Query {SleeveFundCodes}");
            }
            logger.Dispose();
        }
        private static void InitializeIngestionTypesAreActives()
        {
            Logger logger = new Logger(_ClassName, nameof(InitializeIngestionTypesAreActives));
            if(DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using(var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = "select cusip_prefix, active from "+ SSBDataPointsTableName;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if(!reader.IsDBNull(0) && !reader.IsDBNull(1))
                    {
                        string IngesType = reader.GetString(0);
                        bool isActive = reader.GetString(1) == "1" ? true : false ;
                        IngestionTypesAreActive[IngesType] = isActive;
                    }
                }
            }
        }

        /// <summary>To run your scenario. this method is mandatory otherwise RISQUE will not do anything.</summary>
        public override void Run()
        {
            Logger logger = new Logger(_ClassName, nameof(Run));
            logger.log(Severity.info, "Medio SSB Matching Process Scenario starting");
            if(!Directory.Exists(InputFolder) || !Directory.Exists(OutputFolder))
            {
                logger.log(Severity.error, $"The input or the output path {InputFolder} / {OutputFolder} does not exist");
                return;
            }
            foreach (var filePath in Directory.GetFiles(InputFolder, InputFilter))
            {
                string outputContent = "Fund Code;CUSIP Prefix;Date;Net Amount\n";
                var ExistingAccounts = new List<string>();
                string outputFilePath = OutputFolder + $"MatchingFileSSB_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv";
                using (StreamReader reader = new StreamReader(filePath))
                {
                    //Skip tHE FIRST LINE
                    reader.ReadLine();
                    string line;
                    string correctDate = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] fields = line.Split(';');
                        if (string.IsNullOrEmpty(correctDate)) correctDate = fields[2]; 
                        //Her We should do The replacement if it's required-------------------------------------------------------------------------------
                        ExistingAccounts.Add(fields[0] + ";" + fields[1]);
                        //Giving Here That Replacement of Fa_Fund_Codes by Custody_Fund_Codes is already done in the first transformation ("Raw To Csv")
                        //Getting this FusionNav by Executing a Query
                        //this line may throw an Exception that needs to be Catched or we can made it return null and having if block
                        if (IngestionTypesAreActive[fields[1]])
                        {
                            try
                            {
                                //In case of CSA. the sleeve fund codes should not be concatenated with "COLOUT"
                                double FusionNav = GetFusionNavByAccountName(correctDate,(fields[1] == "CSA." && SleeveFundCodes.Contains(fields[0])) ? fields[0] : fields[0] + IngestionTypesDict[fields[1]]);
                                outputContent += $"{fields[0]};{fields[1]};{fields[2]};{(double.Parse(fields[3]) - FusionNav).ToString("0.00")}\n";
                            }
                            catch (ArgumentNullException ex)
                            {
                                logger.log(Severity.error, ex.Message);
                                outputContent += $"{fields[0]};{fields[1]};{fields[2]};0.00\n";
                            }
                        }
                    }
                    //Now we should Target The Fund Codes that do not exist in the input File
                    if (string.IsNullOrEmpty(correctDate)) correctDate = GetCorrectDate();
                    List<string> UnexistingAccounts = GetUnexistingAccounts(ExistingAccounts);
                    foreach (var account in UnexistingAccounts)
                    {
                        var fields = account.Split(';');
                        if (IngestionTypesAreActive[fields[1]])
                        {
                            try
                            {
                                double FusionNav = GetFusionNavByAccountName(correctDate, fields[0] + IngestionTypesDict[fields[1]]);
                                outputContent += $"{fields[0]};{fields[1]};{correctDate};{(FusionNav * -1).ToString("0.00")}\n";
                            }
                            catch (ArgumentNullException ex)
                            {
                                logger.log(Severity.error, ex.Message);
                                outputContent += $"{fields[0]};{fields[1]};{correctDate};0.00\n";
                            }
                        }
                    }
                }
                try
                {
                    using (StreamWriter sw = new StreamWriter(outputFilePath))
                    {
                        sw.Write(outputContent);
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, ex.Message);
                }

            }
            logger.log(Severity.info, "Medio SSB Matching Process Scenario ending");
            logger.Dispose();
        }

        private string GetCorrectDate()
        {
            int DaysToBeAdded = -2;
            var date = DateTime.Now;
            // All Transactions in EUR
            CSMCurrency ccy = CSMCurrency.GetCSRCurrency(CSMCurrency.StringToCurrency("EUR"));
            int sophisDate = Convert.ToInt32((date - new DateTime(1904, 01, 01, 0, 0, 0)).TotalDays);
            while (DaysToBeAdded < 0)
            {
                date = date.AddDays(-1);
                sophisDate--;
                if (!(date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || ccy.IsABankHolidayDay(sophisDate)))
                {
                    DaysToBeAdded++;
                }
            }
            return date.ToString("dd/MM/yyyy");
        }

        private List<string> GetUnexistingAccounts(List<string> existingAccounts)
        {
            Logger logger = new Logger(_ClassName, nameof(GetUnexistingAccounts));
            List<string> UnexistingAccounts = new List<string>();
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = "select FA_FUND_CODE, custody_fund_code from "+ SSBFundCodesTableName;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        foreach (var item in IngestionTypes)
                        {
                            string account = reader.GetString(0) + ";" + item;
                            // To avoid adding the Accounts that they are already replaced
                            if (!existingAccounts.Any() || (!existingAccounts.Contains(account) && (reader.IsDBNull(1) || !existingAccounts.Contains(reader.GetString(1) + ";" + item))))
                            {
                                UnexistingAccounts.Add(account);
                            }
                        }
                    }
                }
            }
            return UnexistingAccounts;
        }

        private double GetFusionNavByAccountName(string date, string AccountName)
        {
            Logger logger = new Logger(_ClassName, nameof(GetFusionNavByAccountName));
            double FusionNav = 0;
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = $"SELECT ACCOUNT_LEVEL_FOLIO FROM BO_TREASURY_ACCOUNT WHERE ID = (SELECT ID FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN ='{AccountName}')";
                logger.log(Severity.info, $"Starting Executing Query {cmd.CommandText}");
                var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    logger.log(Severity.error, $"Could Not Find the ACCOUNT LEVEL FOLIO Id For This Account {AccountName}");
                    throw new ArgumentNullException($"Could Not Find the ACCOUNT LEVEL FOLIO Id For This Account {AccountName}");
                }
                else
                {
                    int? FolioId = reader.GetInt32(0);
                    if (FolioId == null)
                    {
                        logger.log(Severity.error, $"Could Not Find the ACCOUNT LEVEL FOLIO Id For This Account {AccountName}");
                        throw new ArgumentNullException($"Could Not Find the ACCOUNT LEVEL FOLIO Id For This Account {AccountName}");
                    }
                    else
                    {
                        using (var cmd1 = DBContext.Connection.CreateCommand())
                        {
                            // having backoffice as default parameter
                            cmd1.CommandText = $"SELECT sum(MONTANT) * -1 from join_position_histomvts where opcvm = {FolioId} and sicovam = {cashCCYSicovam} and backoffice != 860 and dateneg <= to_date('{date}', 'DD/MM/YYYY')";
                            var reader1 = cmd1.ExecuteReader();
                            while (reader1.Read())
                            {
                                FusionNav = reader1.IsDBNull(0) ? 0 : reader1.GetDouble(0);
                            }
                        }
                    }

                }
            }
            return FusionNav;
            //What to do with the input file after processing it

        }

        /// <summary>Free initiliased memory after scenario is processed.</summary>
        public override void Done()
        {

        }
    }

}
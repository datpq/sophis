using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using System.Threading.Tasks;
using sophis;
using sophis.scenario;
using sophis.utils;
using Sophis.DataAccess;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using sophis.misc;
using System.Text.RegularExpressions;

namespace MEDIO.BackOffice.net.src.Scenario
{
    /// <summary>
    /// This class derived from <c>sophis.portfolio.CSMScenario</c> can be overloaded to create a new scenario
	/// </summary>
	public class CSxStrategyFilter : CSMScenario
    {
        /// <summary>This method specifies the context in which it can be launched.</summary>
        /// <returns> Returns the type of the scenario. It returns <c>sophis.scenario.eMProcessingType.M_pScenario</c> by default.</returns>
        /// <remarks>
        /// <para>The various types of scenarios are: </para>
        /// <para>M_pScenario: Will be available through the Analysis menu when the porfolio is launched or when an instrument is opened.</para>
        /// <para>M_pManagerPreference: Will be available through the Manager menu without any condition.</para>
        /// <para>M_pUserPreference: Will be available through the User menu (or the Manager menu if available) without any condition.</para>
        /// <para>M_pInstrument: Will be available through the Data menu when an instrument is opened or selected.</para>
        /// <para>M_pCounterparty: Will be available through the Data menu when a third party is opened or selected.</para>
        /// <para>M_pPortfolio: Will be available through the Data menu when a folio is selected.</para>
        /// <para>M_pBeforeEndOfDayProcedure: Will be launched automatically before the end of day procedure.</para>
        /// <para>M_pAfterEndOfDayProcedure: Will be launched automatically after the end of day procedure.</para>
        /// <para>M_pNightBatch: Will be launched automatically in the night batch.</para>
        /// <para>M_pBeforeReporting: Will be launched automatically before every reporting.</para>
        /// <para>M_pAfterReporting: Will be launched automatically after every reporting.</para>
        /// <para>M_pMarketData: </para>
        /// <para>M_pData: </para>
        /// <para>M_pEndOfDayConditionnal: Add to the end of day in a conditional form.</para>
        /// <para>M_pAfterAllInitialisation: Will be executed after all initialiation.</para>
        /// <para>M_pOther: Will be added in the prototype but never used. May be used for a scenario on Calculation server.</para>
        /// <para>M_pMultiSiteEODBeforePortfolioLoading: Will be executed before the portfolio loading during a MultiSite End Of Day</para>
        /// <para>M_pAccounting: Will be available through the Accounting menu without any condition.</para>
        /// <para>M_pBalanceEngineBeforePnL: </para>
        /// <para>M_pBalanceEngineAfterPnL: </para>
        /// <para>M_pPNLEngine: </para>
        /// <para>M_pAuxiliaryLedger: </para>
        /// <para>M_pSendToGL: </para>
        /// </remarks>
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pUserPreference;
        }

        /// <summary>To do all your initialisation. Typically, it may open a GUI to get data from the user.</summary>
        public override void Initialise()
        {
            /// Add your code here
        }

        /// <summary>To run your scenario. this method is mandatory otherwise RISQUE will not do anything.</summary>
        public override void Run()
        {
            //Update Closed Folios
            UpdateClosedFolio();
            //Get StrategiesList
            string strategiesIdList = GetClosedStrategiesList();
            //Load Users
            string usersIdConfig ="";
            CSMConfigurationFile.getEntryValue("MediolanumStrategyFilter", "UsersId", ref usersIdConfig, "4970;2212;6191;2730;3609");
            UsersConfigId = ConvertStringToList(usersIdConfig);
            //Update strategies list in filter for users.
            UpdateStrategyFilter(strategiesIdList);
            //Update strategies list in workbook for users
            UpdateWorkbookFilter(strategiesIdList);
        }

        /// <summary>Free initiliased memory after scenario is processed.</summary>
        public override void Done()
        {
            /// Add your code here
        }
        public static string GetClosedStrategiesList()
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxStrategyFilter", "Get closed Strategies Begin.");
                try
                {
                    List<string> strategiesIdList = new List<string>();
                    string sqlQuery = "SELECT strategy from PFR_MODEL_LINK where folio IN (select ident from medio_strat_visibility)";
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = sqlQuery;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string strategyId = reader.GetInt32(0).ToString();
                                strategiesIdList.Add(strategyId);
                            }
                        }
                        string strategiesId = string.Join(", ", strategiesIdList);
                        log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Strategies Id: = {0}", strategiesId));
                        return strategiesId;
                    }
                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("Execute exception! {0}", ex.Message));
                    return null;
                }
            }
        }
        public static bool UpdateClosedFolio()
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxStrategyFilter", "Update closed Folios Begin.");
                try
                {
                    string sqlQuery = "update folio set marked_as_closed=1 where ident in ( select ident from medio_strat_visibility) and marked_as_closed=0";
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = sqlQuery;
                        cmd.ExecuteNonQuery();
                        log.Write(CSMLog.eMVerbosity.M_info, "Closed Folios: List updated successfully.");
                    }
                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("Execute exception! {0}", ex.Message));
                    return false;
                }
                return true;
            }
        }
        protected static List<string> UsersConfigId;
        public static bool UpdateStrategyFilter(string strategiesId)
        {
            using (var log = new CSMLog())
            {
                string targetTagName = "Filter";
                string targetAttributeName = "name";
                string targetAttributeValue = "";
                CSMConfigurationFile.getEntryValue("MediolanumStrategyFilter", "FilterName", ref targetAttributeValue, "MEDIO_VISIBILITY");

                log.Begin("CSxStrategyFilter", "Update Strategy Filter");
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        string updatedCriteria = string.Format("Not [Strategy (level 1)] In ({0})", strategiesId);
                        string pattern = @"Not \[Strategy \(level 1\)\] In \([^)]*\)";
                        //get BLOB content rom db
                        foreach (string userId in UsersConfigId)
                        {
                            if (string.IsNullOrWhiteSpace(userId))
                            {
                                continue;
                            }
                            string sqlBlobVal = "select value from userprefslr where userid = " + userId + " and name ='FiltersList'";
                            //string sqlBlobVal = "select value from userprefslr where userid =6809 and name ='FiltersList'";
                            string xmlContent = "";
                            cmd.CommandText = sqlBlobVal;
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    OracleBlob blob = reader.GetOracleBlob(0);
                                    using (StreamReader readerStream = new StreamReader(blob, Encoding.UTF8))
                                    {
                                        xmlContent = readerStream.ReadToEnd();
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(xmlContent))
                            {
                                log.Write(CSMLog.eMVerbosity.M_error, "The user with ID: " + userId + ", have an Empty XML in Blob.");
                                continue;
                            }
                            //XML Operation
                            log.Write(CSMLog.eMVerbosity.M_debug, String.Format("XML content from BLOB type: = {0}", xmlContent));
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(xmlContent);

                            XmlNodeList targetElements = xmlDoc.GetElementsByTagName(targetTagName);
                            bool findFilter = false;
                            foreach (XmlNode element in targetElements)
                            {
                                XmlAttribute attribute = element.Attributes[targetAttributeName];
                                if (attribute != null && attribute.Value == targetAttributeValue)
                                {
                                    findFilter = true;
                                    string originalCriteria = element.Attributes["criteria"].Value;
                                    Match match = Regex.Match(originalCriteria, pattern);
                                    if (match.Success)
                                    {
                                        // Replace the matched part with the updatedCriteria
                                        log.Write(CSMLog.eMVerbosity.M_info, String.Format("Regex match successfully."));
                                        string newCriteria = Regex.Replace(originalCriteria, pattern, updatedCriteria);
                                        element.Attributes["criteria"].Value = newCriteria;
                                    }
                                    else
                                    {
                                        log.Write(CSMLog.eMVerbosity.M_error, String.Format("Error while matching Regex."));
                                        continue;
                                    }
                                    string updateQuery = "UPDATE userprefslr SET value = :xmlBlob " +
                                                                           "WHERE userid = " + userId + " AND name = 'FiltersList'";
                                    log.Write(CSMLog.eMVerbosity.M_debug, String.Format("CSxQuery Update User filterList: {0}", updateQuery));
                                    OracleParameter blobParameter = new OracleParameter("xmlBlob", OracleDbType.Blob);
                                    byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlDoc.OuterXml);
                                    blobParameter.Value = xmlBytes;
                                    blobParameter.Direction = System.Data.ParameterDirection.Input;
                                    cmd.Parameters.Add(blobParameter);
                                    cmd.CommandText = updateQuery;
                                    cmd.ExecuteNonQuery();
                                    log.Write(CSMLog.eMVerbosity.M_debug, "CSxQuery Update User filterList: Query executed successfully.");
                                    log.Write(CSMLog.eMVerbosity.M_info, "The user with ID: " + userId + " has successfully updated the filter: " + targetAttributeValue + ".");
                                    blobParameter.Dispose();
                                    cmd.Parameters.Clear();
                                    xmlDoc = null;
                                    xmlBytes = null;
                                }
                            }
                            if (!findFilter)
                            {
                                log.Write(CSMLog.eMVerbosity.M_error, "The user with ID: " + userId + ", does not have " + targetAttributeValue + " filter.");
                                continue;
                            }
                        }   
                    }
       
                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("Execute exception! {0}", ex.Message));
                    return false;
                }
                
                return true;
            } 
        }
        public static bool UpdateWorkbookFilter(string strategiesId)
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxStrategyFilter", "Update WorkBook strategy Filter");
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        string criteriaBaseName = "[Strategy (level 1)] In";
                        string updatedCriteria = string.Format("{0} ({1})", criteriaBaseName, strategiesId);
                        string pattern = @"\[Strategy \(level 1\)\] In \([^)]*\)";
                        string wokrbookNameCondition = "portfolio.workbook.%_Filter";
                        //get BLOB content rom db
                        foreach (string userId in UsersConfigId)
                        {
                            if (string.IsNullOrWhiteSpace(userId))
                            {
                                continue;
                            }
                            string sqlBlobVal = string.Format("select value, name from USERPREFSLR where USERID = {0} and name like '{1}'", userId, wokrbookNameCondition);
                            string xmlContent = "";
                            cmd.CommandText = sqlBlobVal;
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        string filterWBName = reader.GetValue(1).ToString();
                                        OracleBlob blob = reader.GetOracleBlob(0);
                                        using (StreamReader readerStream = new StreamReader(blob, Encoding.UTF8))
                                        {
                                            xmlContent = readerStream.ReadToEnd();
                                        }
                                        if (string.IsNullOrEmpty(xmlContent))
                                        {
                                            log.Write(CSMLog.eMVerbosity.M_error, "The user with ID: " + userId + ", have an Empty XML in Blob.");
                                            continue;
                                        }
                                        //XML Operation
                                        log.Write(CSMLog.eMVerbosity.M_debug, String.Format("XML content from BLOB type: = {0}", xmlContent));
                                        bool findFilter = xmlContent.Contains(criteriaBaseName);
                                        if (!findFilter)
                                        {
                                            log.Write(CSMLog.eMVerbosity.M_error, "The user with ID: " + userId + ", does not have " + criteriaBaseName + " keyword in "+ filterWBName + " filter.");
                                            continue;
                                        }

                                        string updateQuery = "UPDATE userprefslr SET value = :xmlBlob " +
                                                                               "WHERE userid = " + userId + " AND name = '" + filterWBName + "'";
                                        string modifiedXmlContent = "";
                                        Match match = Regex.Match(xmlContent, pattern);
                                        if (match.Success)
                                        {
                                            // Replace the matched part with the updatedCriteria
                                            log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Regex match successfully."));
                                            modifiedXmlContent = Regex.Replace(xmlContent, pattern, updatedCriteria);
                                        }
                                        else
                                        {
                                            log.Write(CSMLog.eMVerbosity.M_error, String.Format("Error while matching Regex."));
                                            continue;
                                        }
                                        log.Write(CSMLog.eMVerbosity.M_debug, String.Format("CSxQuery Update User Workbook filterList: {0}", updateQuery));
                                        OracleParameter blobParameter = new OracleParameter("xmlBlob", OracleDbType.Blob);
                                        byte[] xmlBytes = Encoding.UTF8.GetBytes(modifiedXmlContent);
                                        blobParameter.Value = xmlBytes;
                                        blobParameter.Direction = System.Data.ParameterDirection.Input;
                                        cmd.Parameters.Add(blobParameter);
                                        cmd.CommandText = updateQuery;
                                        cmd.ExecuteNonQuery();
                                        log.Write(CSMLog.eMVerbosity.M_debug, "CSxQuery Update User Workbook filterList: Query executed successfully.");
                                        log.Write(CSMLog.eMVerbosity.M_info, "The user with ID: " + userId + " has successfully updated the Workbook filter: " + filterWBName + ".");
                                        blobParameter.Dispose();
                                        cmd.Parameters.Clear();
                                        xmlBytes = null;

                                    }


                                }
                                else
                                {
                                    log.Write(CSMLog.eMVerbosity.M_error, "The user with ID: " + userId + ", does not have " + wokrbookNameCondition + " workbook filters.");
                                    continue;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, String.Format("Execute exception! {0}", ex.Message));
                    return false;
                }

                return true;
            }
        }

        public static List<string> ConvertStringToList(string input)
        {
            List<string> resultList = new List<string>();
            string[] substrings = input.Split(';');

            foreach (string substring in substrings)
            {
                resultList.Add(substring);
            }

            return resultList;
        }
    }
}

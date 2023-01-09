using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SophisETL.Common;
using SophisETL.Common.ErrorMgt;
using SophisETL.Common.Logger;
using SophisETL.Transform.GroupByAndSum.Xml;

//added for XML requests
using SophisETL.ISEngine;
using System.Xml.Linq;
using System.Xml;


namespace SophisETL.Transform.GroupByAndSum
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class GroupByAndSum : ITransformStep
    {
        #region private members
        private Settings _Settings { get; set; }
        private List<IETLQueue> m_sourceQueues = new List<IETLQueue>();
        private List<IETLQueue> m_targetQueues = new List<IETLQueue>();
        #endregion

        #region properties
        public List<IETLQueue> SourceQueues { get { return m_sourceQueues; } }
        public List<IETLQueue> TargetQueues { get { return m_targetQueues; } }
        public string Name { get; set; }

        #endregion


        public void Init()
        {
            // Make sure we have only 1 Source / Target Queue
            if (m_sourceQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have one source queue", Name, GetType().Name));
            }
            if (m_targetQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have only one target Queue", Name, GetType().Name));
            }
        }


        public void Start()
        {
            List<Record> theRecordSet = new List<Record>();
            LogManager.Instance.Log("Starting GroupByAndSum");
            // get all the records and add them to the list (only use the first Queue as there is only one)
            while (m_sourceQueues[0].HasMore)
            {
                
                Record record = m_sourceQueues[0].Dequeue();
                LogManager.Instance.Log("Dequeuing Record " + record.Fields.ToString());
                if (record != null)
                    theRecordSet.Add(record);
            }

            List<Record> merged = GroupByAndSumBy(_Settings, theRecordSet);
            foreach (Record rec in merged)
            {
                LogManager.Instance.Log("Enqueuing Record " + rec.Fields.ToString());
                m_targetQueues[0].Enqueue(rec);
            }
        }


        public List<Record> GroupByAndSumBy(Settings m_Settings, List<Record> recordList)
        {

                List<Record> result = new List<Record>();
                try
                {
                foreach (GroupByElement elementGroupBy in m_Settings.groupByList)
                {
                    // at each step, a new record will be created
                    double amountFromGUI = 0d;
                    while (recordList.Count > 0)
                    {
                        Record firstRecord = recordList[0];
                        Dictionary<string, object> theObjectToCompare = new Dictionary<string, object>();
                        LogManager.Instance.Log("Adding Object with fields " + elementGroupBy.groupByField + " , " + firstRecord.Fields[elementGroupBy.groupByField]);
                        theObjectToCompare.Add(elementGroupBy.groupByField, firstRecord.Fields[elementGroupBy.groupByField]);
                        LogManager.Instance.Log("Summing Record ");
                        List<Record> newListRecord = recordList.FindAll(rec => ObjectMatchGroupBy(elementGroupBy.groupByField, theObjectToCompare, rec));

#if (DEBUG)  // Need this as prod version may not be hosted in process with sufficient permissions for event logging !!
                    WriteEventLogEntry("sum n filter fields ''" + elementGroupBy.sumField + "_" + elementGroupBy.filterField, "SophisETL.Transform.GroupByAndSum.GroupByAndSum.GroupByAndSum", this.Name.ToString() + "_filter_sum_names");
                    WriteEventLogEntry("The Key for summing is " + elementGroupBy.sumField, "SophisETL.Transform.GroupByAndSum.GroupByAndSum.GroupByAndSum", this.Name.ToString() + "_b4sumField");
#endif
                        double sum = SumField(newListRecord, elementGroupBy.sumField, elementGroupBy.filterField);
#if (DEBUG)
                    WriteEventLogEntry("Computed sum over summed field is " + sum.ToString(), "SophisETL.Transform.GroupByAndSum.GroupByAndSum.GroupByAndSum", this.Name.ToString() + "_afta-sumField");
#endif
                        string realFundName = GetRealFundName(firstRecord.Fields[elementGroupBy.groupByField].ToString());
                        if (realFundName.Contains("&"))
                            realFundName = realFundName.Replace("&amp;", "&");
                        LogManager.Instance.Log("New Fund Name is : " + realFundName);
                        if (sum != 0d)
                        {
                            //#if (REPLACE_SUM)
                            //sum = sum - GetAmountFromGUI(firstRecord.Fields[elementGroupBy.groupByField].ToString());
                            LogManager.Instance.Log("''Amount from file is: " + sum.ToString() + "''");
                            LogManager.Instance.Log("Getting Balance from GUI");
                            amountFromGUI = GetAmountFromGUI(realFundName);
                            sum = (amountFromGUI != 0d) ? amountFromGUI - sum : -sum - 0d;
                            LogManager.Instance.Log("''Amount From Gui (-Balance) = " + amountFromGUI.ToString() + "''");
                            LogManager.Instance.Log("Sum  from GUI - sum from file = " + sum.ToString());

                            //#endif
#if (DEBUG)
                        WriteEventLogEntry("Final offset NAV sum is " + sum.ToString(), "SophisETL.Transform.GroupByAndSum.GroupByAndSum.GroupByAndSum", this.Name.ToString() + "_afta_offsetting");
#endif
                        }

                        string folioPath = GetFolioPath(firstRecord.Fields[elementGroupBy.groupByField].ToString());
                        if (folioPath.Contains("&"))
                            folioPath = folioPath.Replace("&amp;", "&");
                        LogManager.Instance.Log("New FolioPath Name is : " + folioPath);
                        string depositaryName = GetDepositaryName(firstRecord.Fields[elementGroupBy.groupByField].ToString());

                        
                        // get amended new sum, if null then no records to save.
                        if (Math.Abs(sum) > 10E-3)
                        {
                            LogManager.Instance.Log("replacing sum");
                            firstRecord.SafeAdd(elementGroupBy.sumField, sum, eExistsAction.replace);
                            LogManager.Instance.Log("replacing foliopath");
                            firstRecord.SafeAdd(elementGroupBy.folioPathField, folioPath, eExistsAction.replace);
                            LogManager.Instance.Log("replacing depositary");
                            firstRecord.SafeAdd(elementGroupBy.depositaryField, depositaryName, eExistsAction.replace);

                            // need to remove those records from the list
                            recordList.RemoveAll(rec => ObjectMatchGroupBy(elementGroupBy.groupByField, theObjectToCompare, rec));
                            LogManager.Instance.Log("Adding to Result");
                            LogManager.Instance.Log("replacing fundname");
                            firstRecord.SafeAdd(elementGroupBy.groupByField, realFundName, eExistsAction.replace);
                            result.Add(firstRecord);
                        }
                        else
                        {
                            recordList.RemoveAll(rec => ObjectMatchGroupBy(elementGroupBy.groupByField, theObjectToCompare, rec));
                            LogManager.Instance.Log(" Record discarded for Fund : " + realFundName + " , Depositary : " + depositaryName + ", Folio : " + folioPath + " , Amount = 0");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Log("ERROR : Exception occured -> " + ex.Message);
            }
            return result;
        }

        private bool ObjectMatchGroupBy(string key, Dictionary<string, object> objectToCompare, Record theRecord)
        {
            try
            {
                LogManager.Instance.Log("Object to match...begin");
                // try to adjust types
                LogManager.Instance.Log("Looking at object Record Key: " + key);
                LogManager.Instance.Log("With Value = " + theRecord.Fields[key].ToString());
                object fromRecord = theRecord.Fields[key];
                object fromDictionn = objectToCompare[key];
                fromDictionn = Convert.ChangeType(fromDictionn, fromRecord.GetType());
                if (fromRecord is IComparable)
                {
                    LogManager.Instance.Log("fromRecord is IComparable");
                    if ((fromRecord as IComparable).CompareTo(fromDictionn) != 0)
                        return false;
                }
                // Not IComparable, use Equals
                else if (!theRecord.Fields[key].Equals(objectToCompare[key]))
                    return false;
            }
            catch (System.Exception ex)
            {
                ErrorHandler.Instance.HandleError(new ETLError
                {
                    Step = this,
                    Record = theRecord,
                    Exception = ex,
                    Message = "Can not match the Group By criteria because keys are missing or of invalid type"
                });
                return false;
            }
            return true;
        }

        private double SumField(List<Record> recordList, string field, string filter)
        {
            double res = 0;
            foreach (Record record in recordList)
            {
                double fieldValue = 0, filterValue = 0;
#if (DEBUG) // Need this as prod version may not be hosted in process with sufficient permissions for event logging !!
                WriteEventLogEntry("Value to sum is: " + record.Fields[field].ToString(), "SophisETL.Transform.GroupByAndSum.GroupByAndSum.SumField", this.Name.ToString() + ".sumField");
#endif
                LogManager.Instance.Log("Checking for key = " + field); LogManager.Instance.Log("Checking for filter = " + filter);
                LogManager.Instance.Log("Filter column value is " + record.Fields[filter].ToString());
                if ((IsNumeric(record.Fields[field], ref fieldValue)) && (IsNumeric(record.Fields[filter], ref filterValue)))
                {
                    if (filterValue == 0) { res += fieldValue; }
                }
            }
            return res;
        }

        // check if an object is a numeric type and store the value in dValue
        private bool IsNumeric(object Expression, ref double dValue)
        {
            bool bIsNum = false;
            double dRetNum = .0;
            bIsNum = Double.TryParse(Convert.ToString(Expression),
                System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out dRetNum);

            dValue = dRetNum;
            return bIsNum;
        }

        private bool IsNumb(string expr)
        {
            bool res = false;
            try
            {
                if ((expr.Length>0) && (expr.All(char.IsDigit)))
                {
                    res = true;
                }

                return res;
            }
            catch
            {
                LogManager.Instance.Log("IsNumb failed ");
                return res; 
            }

        }
        private double GetAmountFromGUI (string fund_name)
        {
            LogManager.Instance.Log("Called GetAmountFromGUI with Param : " + fund_name);
        // extract and deduce required value(s) from GUI
            //  Call the SOA Method Request in XML
            string soaRequest = GetSOAMethodRequest();
            string soaResponse = GetXML(soaRequest,true);

            LogManager.Instance.Log("SOA RESPONSE = " +soaResponse);
            //PARSE the soaResponse for Fund_name to get the fees Amount per name fund
            List<string> res = new List<string>();
            XmlDocument doc = new XmlDocument();

            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("ns2", "http://www.sophis.net/reporting");
            ns.AddNamespace("ns0", "http://sophis.net/sophis/gxml/dataExchange");
            LogManager.Instance.Log("Loading SOA Response...");
            doc.LoadXml(soaResponse);
            
            System.IO.StringWriter sw = new System.IO.StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            doc.WriteTo(xw);
            string xml = sw.ToString();
            LogManager.Instance.Log("checking doc "+xml);
            if(doc != null)
            {
#if (DEBUGGER)
                XmlNode reportingWindow = doc.SelectSingleNode("ns0:message//ns2:soaMethodResult//ns2:default0//ns2:window", ns);
                //LogManager.Instance.Log("Node Loaded: " + reportingWindow.InnerXml);
                {
                    if ((reportingWindow != null) )
                    {
                        LogManager.Instance.Log("Before child nodes");
                        try
                        {
                            XmlNodeList check = reportingWindow.ChildNodes;
                            LogManager.Instance.Log("after child nodes ini");
                            {
                                //LogManager.Instance.Log("Looping in Window");
                                LogManager.Instance.Log("Looping in Window over each child of ''''" + reportingWindow.ChildNodes.Count.ToString() +
                                                        "'''' ChildNodes.");
                                LogManager.Instance.Log("passed s1");
                                if (!(reportingWindow.ChildNodes.Count > 0)) { return 0d;}
                                foreach (XmlNode xyz in check)
                                {
                                    LogManager.Instance.Log("Checking xyz");
                                    if (xyz.Name == "ns2:folio")
                                    {
                                        LogManager.Instance.Log("name == ''ns2:folio''");
                                        if (xyz.Attributes.Count > 0)
                                        {
                                            if (xyz.Attributes.GetNamedItem("ns2:name").Value == fund_name)
                                            {
                                                LogManager.Instance.Log("Got Item for fund :" + fund_name);
                                                foreach (XmlNode zyx in xyz.ChildNodes)
                                                {
                                                    LogManager.Instance.Log("childnodes : " + xyz.ChildNodes.Count);
                                                    if (zyx.Name == "ns2:unsettledBalance")
                                                    {
                                                        LogManager.Instance.Log(
                                                            "Adding data : " + zyx.FirstChild.Value);
                                                        res.Add(zyx.FirstChild.Value);
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                        catch (NullReferenceException e)
                        {
                            LogManager.Instance.Log("method call ''reportingWindow.HasChildNodes'' failed with error: " +
                                                    e.Message);
                            return 0d;
                        }
                        catch
                        {
                            LogManager.Instance.Log("Attempt to walk the ChildNodes collection blew up!");
                            return 0d;
                        }
                    }
                }
#else
                XmlNode reportingWindow = doc.SelectSingleNode("ns0:message//ns2:soaMethodResult//ns2:default0//ns2:window", ns);
                LogManager.Instance.Log("Node Loaded: " + reportingWindow.InnerXml);
                {
                    XmlNodeList check = reportingWindow.ChildNodes;
                    LogManager.Instance.Log("Looping in Window");
                    LogManager.Instance.Log("Looping over each child of ''''" + reportingWindow.ChildNodes.Count.ToString() + "'''' ChildNodes.");
                    if (!(reportingWindow.ChildNodes.Count > 0)) { return 0d; }
                    foreach (XmlNode xyz in reportingWindow.ChildNodes)
                    {
                        LogManager.Instance.Log("Checking xyz");
                        if (xyz.Name == "ns2:folio")
                        {
                            LogManager.Instance.Log("name == ''ns2:folio''");
                            if (xyz.Attributes != null && xyz.Attributes.GetNamedItem("ns2:name") != null)
                            {
                                LogManager.Instance.Log("Node 'xyz' has Attributes");
                                if (xyz.Attributes.GetNamedItem("ns2:name").Value == fund_name)
                                {
                                    LogManager.Instance.Log("Got Item for fund :" + fund_name);
                                    if (xyz.ChildNodes.Count > 0)
                                    {
                                        foreach (XmlNode zyx in xyz.ChildNodes)
                                        {
                                            LogManager.Instance.Log("childnodes : " + xyz.ChildNodes.Count);
                                            if (zyx.Name == "ns2:unsettledBalance")
                                            {
                                                LogManager.Instance.Log("Adding data : " + zyx.FirstChild.Value);
                                                res.Add(zyx.FirstChild.Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
#endif
            }
            else
            {
                LogManager.Instance.Log("Doc is null");
            }
            double ans=0d;

            if (res.Count > 0 && IsNumeric(res[0].ToString(), ref ans))
            {
                LogManager.Instance.Log("Sum frm GUI = " + ans.ToString());
                return ans;
            }
            else
            {
                return 0d;
            } // found value for fund_name...
            //TODO optimize to avoid maybe one call per step in the loop...
        }

        private static void WriteEventLogEntry(string message, string appName, string source="")
        {
            // Create an instance of EventLog
            System.Diagnostics.EventLog eventLog = new System.Diagnostics.EventLog();

            // Check if the event source exists. If not create it.
            if (!System.Diagnostics.EventLog.SourceExists(appName))
            {
                System.Diagnostics.EventLog.CreateEventSource(appName, "Application");
            }

            // Set the source name for writing log entries.
            eventLog.Source = appName;

            // Create an event ID to add to the event log
            int eventID = 9999;

            // Write an entry to the event log.
            if (source.Length == 0)
            {
                eventLog.WriteEntry(message, System.Diagnostics.EventLogEntryType.Information, eventID);
            }
            else 
            {
                System.Diagnostics.EventLog.WriteEntry(source, message, System.Diagnostics.EventLogEntryType.Information, eventID);
            }

            // Close the Event Log
            eventLog.Close();
        }


        private string GetRealFundName(string fun_ref)
        {
            string retval = "FundName";

            string soaRequest = GetFundExport(fun_ref);
            LogManager.Instance.Log("SOA REquest 'fund name' = ||" + soaRequest + "||");

            string soaResponse = GetXML(soaRequest, false);
            LogManager.Instance.Log("SOA RESPONSE 'fund name' = ||" + soaResponse + "||");

            XmlDocument doc = new XmlDocument();

            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("ns0", "http://sophis.net/sophis/gxml/dataExchange");
            ns.AddNamespace("ns2", "http://www.sophis.net/fund");
            ns.AddNamespace("ns5", "http://www.sophis.net/party");
            ns.AddNamespace("ns7", "http://www.sophis.net/folio");
            ns.AddNamespace("ns3", "http://www.sophis.net/Instrument");
            LogManager.Instance.Log("Loading SOA Response for fund name extraction...");
            doc.LoadXml(soaResponse);
            System.IO.StringWriter swNew = new System.IO.StringWriter();
            XmlTextWriter xwNew = new XmlTextWriter(swNew);
            doc.WriteTo(xwNew);
            string xmlNew = swNew.ToString();
            LogManager.Instance.Log("checking folio doc " + xmlNew);

            if (doc != null)
            {
                XmlNodeList fundNameNode = doc.SelectNodes("//ns3:name", ns);
                if (fundNameNode != null)
                {
                    LogManager.Instance.Log("Got Nb nodes: " + fundNameNode.Count);
                    retval = fundNameNode.Item(0).InnerXml;
                }
                else
                {
                    LogManager.Instance.Log("failed to find target fund name");
                }
            }

            else
            {
                LogManager.Instance.Log("Doc is null");

            }
            //#endif         
            LogManager.Instance.Log("Retreived fund name is ''" + retval + "''");


            return retval;
        }
        private string GetFolioPath(string fund_name) 
        {
            string result = "FolioPath";
            LogManager.Instance.Log("Called GetFolioPath with Param ''" + fund_name + "''");
            string soaRequest = GetFundExport(fund_name);
            LogManager.Instance.Log("SOA REquest 'folio path' = ||" + soaRequest + "||");

            string soaResponse = GetXML(soaRequest, false);
            LogManager.Instance.Log("SOA RESPONSE 'folio path' = ||" + soaResponse + "||");

            XmlDocument doc = new XmlDocument();

            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("ns0", "http://sophis.net/sophis/gxml/dataExchange");
            ns.AddNamespace("ns2", "http://www.sophis.net/fund");
            ns.AddNamespace("ns5", "http://www.sophis.net/party");
            ns.AddNamespace("ns7","http://www.sophis.net/folio");
            LogManager.Instance.Log("Loading SOA Response for folio path extraction...");
            doc.LoadXml(soaResponse);
            System.IO.StringWriter swNew = new System.IO.StringWriter();
            XmlTextWriter xwNew = new XmlTextWriter(swNew);
            doc.WriteTo(xwNew);
            string xmlNew = swNew.ToString();
            LogManager.Instance.Log("checking folio doc " + xmlNew);
            
            if (doc != null)
            {
                XmlNodeList portfolioNameNode = doc.SelectNodes("//ns2:rootPortfolio/ns7:portfolioName[@ns7:portfolioNameScheme='http://www.sophis.net/folio/portfolioName/fullName']", ns);
                if (portfolioNameNode != null)
                {
                    LogManager.Instance.Log("Got Nb nodes: " + portfolioNameNode.Count);
                    result=portfolioNameNode.Item(0).InnerXml+":Fee Accruals";
                }
                else 
                {
                    LogManager.Instance.Log("failed to find target folio");
                }
            }
                  
            else
            {
                LogManager.Instance.Log("Doc is null");
            
            }
//#endif         
            LogManager.Instance.Log("Retreived folio path is ''" + result + "''");

            return result;
        }

        private string GetFundExport(string refForDepositary)
        {
            string retval = "";
            retval="<?xml version=\"1.0\"?>"
                    +"<exch:export version=\"4-2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\""
                    +" xmlns:exch=\"http://sophis.net/sophis/gxml/dataExchange\" xmlns:fpml=\"http://www.fpml.org/2005/FpML-4-2\""
                    +" xmlns:dsig=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:common=\"http://sophis.net/sophis/common\""
                    +" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:party=\"http://www.sophis.net/party\""
                    +" xmlns:trade=\"http://www.sophis.net/trade\" xmlns:instrument=\"http://www.sophis.net/Instrument\""
                    +" xmlns:sph=\"http://www.sophis.net/Instrument\" xmlns:folio=\"http://www.sophis.net/folio\""
                    +" xmlns:user=\"http://www.sophis.net/user\">"
	                +"<fpml:header>"
		            +"<fpml:messageId messageIdScheme=\"http://www.sophis.net/gxml/exchange/messageIdScheme/simple\">001</fpml:messageId>"
		            +"<fpml:sentBy partyIdScheme=\"http://www.sophis.net/party/partyId/name\">CLIENT</fpml:sentBy>"
		            +"<fpml:sendTo partyIdScheme=\"http://www.sophis.net/party/partyId/name\">SOPHIS</fpml:sendTo>"
		            +"<fpml:creationTimestamp>2005-09-19T13:52:00</fpml:creationTimestamp>"
	                +"</fpml:header>"
		            +"<sph:instrumentIdentifier>"
			        +"<sph:reference sph:modifiable=\"UniqueNotPrioritary\" sph:name=\"Dbcode\">"+refForDepositary+"</sph:reference>"
		            +"</sph:instrumentIdentifier>"
                    +"</exch:export>";
            return retval;
        }

         private string GetDepositaryName(string refFordepositary) 
        {
            string result = "NA";
            LogManager.Instance.Log("Called GetDepositaryName with Param ''" + refFordepositary + "''");
            string soaRequest = GetFundExport(refFordepositary);
            LogManager.Instance.Log("SOA REquest 'Depositary Name' = ||" + soaRequest + "||");

            string soaResponse = GetXML(soaRequest, false);
            LogManager.Instance.Log("SOA RESPONSE 'Depositary Name' = ||" + soaResponse + "||");

            XmlDocument doc = new XmlDocument();

            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("ns0", "http://sophis.net/sophis/gxml/dataExchange");
            ns.AddNamespace("ns2", "http://www.sophis.net/fund");
            ns.AddNamespace("ns5", "http://www.sophis.net/party");
            LogManager.Instance.Log("Loading SOA Response for Depositary Name extraction...");
            doc.LoadXml(soaResponse);
            System.IO.StringWriter swNew = new System.IO.StringWriter();
            XmlTextWriter xwNew = new XmlTextWriter(swNew);
            doc.WriteTo(xwNew);
            string xmlNew = swNew.ToString();
            LogManager.Instance.Log("checking folio doc " + xmlNew);
            
            if (doc != null)
            {
                ///ns5:partyId[@ns5:partyIdScheme='http://www.sophis.net/party/partyId/name]'
                ///
                try
                {
                    XmlNodeList depositaryNameNode = doc.SelectNodes("//ns2:primeBroker/ns5:partyId[@ns5:partyIdScheme='http://www.sophis.net/party/partyId/name']", ns);
                    if (depositaryNameNode != null)
                    {
                        LogManager.Instance.Log("Got Nb nodes: " + depositaryNameNode.Count);
                        result = depositaryNameNode.Item(0).InnerXml;
                    }
                    else
                    {
                        LogManager.Instance.Log("No Depositary available");
                    }
                }
                catch (Exception ex)
                {
								  
                    LogManager.Instance.Log("No Depositary available");
                }
            }
                  
            else
            {
                LogManager.Instance.Log("Doc is null");
            
            }
           
//#endif         
            LogManager.Instance.Log("Retrieved Depositary Name is ''" + result + "''");

            return result;
        }

        private string GetXML(string xmlExportMessage,bool isMEthodDesigner)
        {

            string retval ="";
             try
                {
                    IntegrationServiceEngine _ISinstance = IntegrationServiceEngine.Instance;

                    // string message = GetTermDepositMessage(reference, ccy, startDate, maturityDate, interestRate);
                 XDocument response= new XDocument();
                 if(isMEthodDesigner==true)  
                 response = _ISinstance.SOAImport(xmlExportMessage);
                 else
                     response = _ISinstance.Import(xmlExportMessage);

                 string sResponse = response.ToString();

                //    if (sResponse.Contains("Accepted"))
                    {
                       // logger.log(Severity.debug, "Import Message Accepted.");
                        retval = sResponse;
                    }
                 //   else
                    {
                        //logger.log(Severity.error, "Import Message Rejected, Check IS logs");
                    }
                }
                catch (Exception ex)
                {
                   LogManager.Instance.Log("Exception Occured : " + ex.Message);
                }

            return retval;
        }

        private string GetSOAMethodRequest()
        {
            string retval = "";

            retval ="<?xml version=\"1.0\"?>"
                    +"<p1:soaMethodsRequest version=\"4-2\" p1:batchType=\"NoSession\" xmlns:p1=\"http://sophis.net/sophis/gxml/dataExchange\"  xmlns:p4=\"http://www.sophis.net/reporting\">"
                    +"<p4:soaMethod p4:name=\"Medio_Fee_Accruals\">"
                    +"</p4:soaMethod>"
                    +"</p1:soaMethodsRequest>";   

            return retval;
        }

        private string GetFolioExport(string fund_Name)
        {
            string retval = "";

            retval ="<?xml version=\"1.0\"?>"
                    +" <exch:export version=\"4-2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\""
                    +" xmlns:exch=\"http://sophis.net/sophis/gxml/dataExchange\" xmlns:fpml=\"http://www.fpml.org/2005/FpML-4-2\""
                    +" xmlns:dsig=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:common=\"http://sophis.net/sophis/common\""
                    +" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:party=\"http://www.sophis.net/party\""
                    +" xmlns:trade=\"http://www.sophis.net/trade\" xmlns:instrument=\"http://www.sophis.net/Instrument\""
                    +" xmlns:folio=\"http://www.sophis.net/folio\" xmlns:user=\"http://www.sophis.net/user\""
                    +" exch:batchType=\"AllRegardlessOfErrors\">"
	                +"<fpml:header>"
	                +"<fpml:conversationId conversationIdScheme=\"\"/>"
	                +"<fpml:messageId messageIdScheme=\"http://www.sophis.net/gxml/exchange/messageIdScheme/simple\">001</fpml:messageId>"
	                +"<fpml:sentBy partyIdScheme=\"http://www.sophis.net/party/partyId/name\">CLIENT</fpml:sentBy>"
	                +"<fpml:sendTo partyIdScheme=\"http://www.sophis.net/party/partyId/name\">SOPHIS</fpml:sendTo>"
	                +"<fpml:creationTimestamp>2005-09-19T13:52:00</fpml:creationTimestamp>"
	                +"</fpml:header>"
                    +"<folio:folioReference>"
                    +"   <folio:portfolioName folio:portfolioNameScheme=\"http://www.sophis.net/folio/portfolioName/name\">"+fund_Name+"</folio:portfolioName>"
                    +"</folio:folioReference>"
                    +"</exch:export>";

            return retval;
        }
      }
}


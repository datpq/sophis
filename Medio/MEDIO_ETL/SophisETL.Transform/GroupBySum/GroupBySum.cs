using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SophisETL.Common;
using SophisETL.Common.ErrorMgt;
using SophisETL.Common.Logger;
using SophisETL.Transform.GroupBySum.Xml;

namespace SophisETL.Transform.GroupBySum
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class GroupBySum : ITransformStep
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
            LogManager.Instance.Log("Starting GroupBySum");
            // get all the records and add them to the list (only use the first Queue as there is only one)
            while (m_sourceQueues[0].HasMore)
            {
                
                Record record = m_sourceQueues[0].Dequeue();
                LogManager.Instance.Log("Dequeuing Record " + record.Fields.ToString());
                if (record != null)
                    theRecordSet.Add(record);
            }

            List<Record> merged = GroupByAndSum(_Settings, theRecordSet);
            foreach (Record rec in merged)
            {
                LogManager.Instance.Log("Enqueuing Record " + rec.Fields.ToString());
                m_targetQueues[0].Enqueue(rec);
            }
        }


        public List<Record> GroupByAndSum(Settings m_Settings, List<Record> recordList)
        {
            List<Record> result = new List<Record>();
            foreach (GroupByElement elementGroupBy in m_Settings.groupByList)
            {
                // at each step, a new record will be created
                while (recordList.Count > 0)
                {
                    Record firstRecord = recordList[0];
                    Dictionary<string, object> theObjectToCompare = new Dictionary<string, object>();
                    LogManager.Instance.Log("Adding Object with fields " + elementGroupBy.groupByField + " , " + firstRecord.Fields[elementGroupBy.groupByField]);
                    theObjectToCompare.Add(elementGroupBy.groupByField, firstRecord.Fields[elementGroupBy.groupByField]);
                    LogManager.Instance.Log("Summing Record ");
                    List<Record> newListRecord = recordList.FindAll(rec => ObjectMatchGroupBy(elementGroupBy.groupByField, theObjectToCompare, rec));
                    WriteEventLogEntry("The Key for summing is " + elementGroupBy.sumField, "SophisETL.Transform.GroupBySum.GroupBySum.GroupBySum");
                    double sum = SumFiled(newListRecord, elementGroupBy.sumField);
                    firstRecord.SafeAdd(elementGroupBy.sumField, sum, eExistsAction.replace);
                    // need to remove those records from the list
                    recordList.RemoveAll(rec => ObjectMatchGroupBy(elementGroupBy.groupByField, theObjectToCompare, rec));
                    LogManager.Instance.Log("Adding to Result");
                    result.Add(firstRecord);
                }
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

        private double SumFiled(List<Record> recordList, string field)
        {
            double res = 0;
            foreach (Record record in recordList)
            {
                double fieldValue = 0;
                WriteEventLogEntry("Value to sum is: " + record.Fields[field].ToString(), "SophisETL.Transform.GroupBySum.GroupBySum.SumFiled");
                LogManager.Instance.Log("Checking for key = " + field);
                if (IsNumeric(record.Fields[field], ref fieldValue))
                {
                    res += fieldValue;
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

        private static void WriteEventLogEntry(string message, string appName)
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
            eventLog.WriteEntry(message,
                System.Diagnostics.EventLogEntryType.Information,
                eventID);

            // Close the Event Log
            eventLog.Close();
        }
    }
}

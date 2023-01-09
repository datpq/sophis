using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.ErrorMgt;
using SophisETL.Queue;
using SophisETL.Transform;

using SophisETL.Transform.GroupBy.Xml;


namespace SophisETL.Transform.GroupBy
{
    [SettingsType(typeof(Settings), "m_Settings")]
    public class GroupBy : ITransformStep
    {
        // private Settings _Settings;
        #region private members
        private List<IETLQueue> m_sourceQueues = new List<IETLQueue>();
        private List<IETLQueue> m_targetQueues = new List<IETLQueue>();
        private Settings m_Settings { get; set; }
        #endregion

        #region properties
        public List<IETLQueue> SourceQueues { get { return m_sourceQueues; } }
        public List<IETLQueue> TargetQueues { get { return m_targetQueues; } }
        public string Name { get; set; }
 
        #endregion
        

        public void Init()
        {
            LogManager log = LogManager.Instance;

            // Make sure we have only 1 Source / Target Queue
            if (m_sourceQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have only one source queue", Name, GetType().Name));
            }
            if (m_targetQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have only one target Queue", Name, GetType().Name));
            }
            
            foreach (Xml.GroupByElement elementGroupBy in m_Settings.groupByList)
            {
                log.LogDebug("value the Key field for the group by: " + elementGroupBy.keyField);
            }
        }

        private bool ObjectMatchGroupBy(List<string> keyList, Dictionary<string, object> objectToCompare, Record theRecord)
        {
            try
            {
                foreach(string key in keyList)
                {
                    // try to adjust types
                    object fromRecord   = theRecord.Fields[key];
                    object fromDictionn = objectToCompare[key];
                    fromDictionn = Convert.ChangeType(fromDictionn, fromRecord.GetType());
                    if (fromRecord is IComparable)
                    {
                        if ((fromRecord as IComparable).CompareTo(fromDictionn) != 0)
                            return false;
                    }
                    // Not IComparable, use Equals
                    else if (!theRecord.Fields[key].Equals(objectToCompare[key]))
                         return false;
                }
            }
            catch (System.Exception ex)
            {
                ErrorHandler.Instance.HandleError( new ETLError
                {
                    Step = this,
                    Record = theRecord,
                    Exception = ex,
                    Message = "Can not match the Group By criteria because keys are missing or of invalid type"
                } );
                return false;
            }
            return true;
        }

        public List<Record> GroupByKey(Settings m_Settings, List<Record> recordList)
        {
            List<Record> result = new List<Record>();
            List<string> keyList = new List<String>(); ;
            foreach (Xml.GroupByElement elementGroupBy in m_Settings.groupByList)
            {
                // add all the keyFields in the list
                keyList.Add(elementGroupBy.keyField);
            }
          
            // at each step, a new record will be created
            while (recordList.Count > 0)
            {
                Record firstRecord = recordList[0];
                
                Dictionary<string, object> theObjectToCompare = new Dictionary<string, object>();
                foreach (string key in keyList)
                {
                    theObjectToCompare.Add(key, firstRecord.Fields[key]);
                }
                List<Record> newListRecord = recordList.FindAll(rec => ObjectMatchGroupBy(keyList, theObjectToCompare, rec));

                // need to remove those records from the list
                recordList.RemoveAll(rec => ObjectMatchGroupBy(keyList, theObjectToCompare, rec));

                // need to construct the Key
                string keyNewElement = "";
                bool firstOne = true;
                foreach (Record rec in newListRecord)
                {
                    if (firstOne)
                    {
                        keyNewElement = rec.Key.ToString();
                        firstOne = false;
                    }
                    else
                    {
                        keyNewElement = keyNewElement + "_" + rec.Key.ToString();
                    }                    
                }

                // now construct and insert the record
                Record newRecord = Record.NewRecord(keyNewElement);
                newRecord.Fields.Add("Count", newListRecord.Count);
                
                // insert the several keys used for the "group by"
                foreach (string key in keyList)
                {
                    newRecord.Fields.Add(key, theObjectToCompare[key]);
                }
                
                // and now the list of records freshly retrieved
                newRecord.Fields.Add("Records", newListRecord);
                
                result.Add(newRecord);
            }
            
            return result;
        }

        public void Start()
        {
            List<Record> theRecordSet = new List<Record>();

            //get all the records and add them to the list (only use the first Queue as there is only one)
            while (m_sourceQueues[0].HasMore)
            {
                Record record = m_sourceQueues[0].Dequeue();
                if (record != null)
                {
                    theRecordSet.Add(record);
                }
            }
            
            // construct the final list with elements grouped by keys
            List<Record> merged = GroupByKey(m_Settings, theRecordSet);
            foreach (Record rec in merged)
            {
                m_targetQueues[0].Enqueue(rec);
            }
        }
    }
}

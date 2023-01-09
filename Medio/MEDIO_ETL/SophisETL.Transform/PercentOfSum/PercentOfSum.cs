using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.ErrorMgt;
using SophisETL.Queue;
using SophisETL.Transform;

using SophisETL.Transform.PercentOfSum.Xml;


namespace SophisETL.Transform.PercentOfSum
{
    [SettingsType(typeof(Settings), "m_Settings")]
    public class PercentOfSum : ITransformStep
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
        }

        public List<Record> ComputeAverage(Settings m_Settings, List<Record> recordList)
        {
            bool isRecordList   = m_Settings.percentOfSum.recordList;
            string fieldAverage = m_Settings.percentOfSum.fieldAverage;
            string fieldList    = m_Settings.percentOfSum.fieldList;
            string fieldTarget  = m_Settings.percentOfSum.fieldTarget;

            // if each record contains in itself a list of records (after a group by for example...)
            if (isRecordList)
            {
                foreach (Record theRecord in recordList)
                {
                    if (theRecord.Fields[fieldList] is List<Record>)
                    {
                        // first loop to get the sum
                        double sum = 0;
                        foreach (Record recordInList in (List<Record>)theRecord.Fields[fieldList])
                        {
                            double fieldValue = 0;
                            IsNumeric(recordInList.Fields[fieldAverage], ref fieldValue);
                            sum = sum + fieldValue;
                        }

                        // second loop to compute the average...
                        foreach (Record recordInList in (List<Record>)theRecord.Fields[fieldList])
                        {
                            double fieldValue = 0;
                            IsNumeric(recordInList.Fields[fieldAverage], ref fieldValue);
                            double theAverage = sum != 0 ? fieldValue / sum * 100 : 0;
                            if (recordInList.Fields.ContainsKey(fieldTarget))
                                recordInList.Fields[fieldTarget] = theAverage;
                            else
                                recordInList.Fields.Add(fieldTarget, theAverage);
                        }

                    }
                    else
                    {
                        // other cases to take into account ??
                    }
                }
            } 
            // otherwise it's the percentage of one of the fields
            else
            {
                // first loop to get the sum
                double sum = .0;
                foreach (Record theRecord in recordList)
                {
                    double fieldValue = 0;
                    IsNumeric(theRecord.Fields[fieldAverage], ref fieldValue);
                    sum = sum + fieldValue;
                }

                // now from the sum, the average can be computed
                foreach (Record theRecord in recordList) 
                {
                    double fieldValue = 0;
                    IsNumeric(theRecord.Fields[fieldAverage], ref fieldValue);
                    double theAverage = sum != 0 ? fieldValue / sum * 100 : 0;
                    if (theRecord.Fields.ContainsKey(fieldTarget))
                        theRecord.Fields[fieldTarget] = theAverage;
                    else
                        theRecord.Fields.Add(fieldTarget, theAverage);
                }
            }
            return recordList;
        }

        public void Start()
        {
            // store all the records in a list
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
            List<Record> merged = ComputeAverage(m_Settings, theRecordSet);
            foreach (Record rec in merged)
            {
                m_targetQueues[0].Enqueue(rec);
            }
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
    }
}

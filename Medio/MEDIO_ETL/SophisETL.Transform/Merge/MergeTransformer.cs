using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;
using SophisETL.Queue;

using SophisETL.Transform.Merge.Xml;



namespace SophisETL.Transform.Merge
{
    [SettingsType(typeof(Settings), "m_Settings")]
    public class MergeTransformer : ITransformStep
    {
        #region private members
        private Settings m_Settings {get; set;}
        private List<IETLQueue> m_sourceQueues = new List<IETLQueue>();
        private List<IETLQueue> m_targetQueues = new List<IETLQueue>();
        #endregion

        #region properties
        public List<IETLQueue> SourceQueues { get { return m_sourceQueues; } }
        public List<IETLQueue> TargetQueues { get { return m_targetQueues; } }
        public string Name         { get; set; }
       
        #endregion


        public void Init()
        {
            // Make sure we have only 1 Source / Target Queue
            if (m_sourceQueues.Count != 2)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have two source queues", Name, GetType().Name));
            }
            if (m_targetQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have only one target Queue", Name, GetType().Name));
            }
        }

        private static bool KeyEqualsPredictor(JointKey[] in_keys, Record in_left, Record in_right)
        {
            try
            {
                foreach (JointKey key in in_keys)
                {
                    // try to adjust types
                    object leftValue  = in_left.Fields[key.LeftKeyName];
                    object rightValue = in_right.Fields[key.RightKeyName];
                    rightValue = Convert.ChangeType( rightValue, leftValue.GetType() );

                    if ( leftValue is IComparable )
                    {
                        if ( ( leftValue as IComparable ).CompareTo( rightValue ) != 0 )
                            return false;
                    }
                    // Not IComparable, use Equals
                    else if ( !in_left.Fields[key.LeftKeyName].Equals( in_right.Fields[key.RightKeyName] ) )
                        return false;
                }
            }
            catch (System.Exception)
            {
                //key not found or types not compatibles
                return false;
            }
            return true;
        }

        public static List<Record> Merge(Settings in_setting, List<Record> in_left, List<Record> in_right)
        {
            List<Record> rtrn = new List<Record>();
            long index = 0;

            // ---- INNER JOIN ----
            if (in_setting.MergeType == SettingsMergeType.InnerJoin)
            {
                var result = from lrec in in_left
                             from rrec in in_right.Where(v => KeyEqualsPredictor(in_setting.KeySetList, lrec, v))
                             select new { lrec, rrec };
                foreach (var entry in result)
                    rtrn.Add( NewMergedRecord( index++, entry.lrec, entry.rrec ) );
            }
            // ---- LEFT JOIN ----
            else if ( in_setting.MergeType == SettingsMergeType.LeftJoin )
            {
                // left join is directly handled by LinQ
                var result = ( from lrec in in_left
                               from rrec in in_right.Where( v => KeyEqualsPredictor( in_setting.KeySetList, lrec, v ) ).DefaultIfEmpty()
                               select new { lrec, rrec }
                              );
                foreach ( var entry in result )
                    rtrn.Add( NewMergedRecord( index++, entry.lrec, entry.rrec ) );

            }
            // ---- (FULL) OUTER JOIN ----
            else if ( in_setting.MergeType == SettingsMergeType.OuterJoin )
            {
                //no full outer join in linq, union of two left join
                var result = (from lrec in in_left
                              from rrec in in_right.Where(v => KeyEqualsPredictor(in_setting.KeySetList, lrec, v)).DefaultIfEmpty()
                              select new { lrec, rrec }
                              ).Union(
                              from rrec in in_right
                              from lrec in in_left.Where(v => KeyEqualsPredictor(in_setting.KeySetList, v, rrec)).DefaultIfEmpty()
                              select new { lrec, rrec }
                              );
                foreach (var entry in result)
                    rtrn.Add( NewMergedRecord( index++, entry.lrec, entry.rrec ) );
                //rtrn.elem
            }
            else
            {
                LogManager.Instance.Log(String.Format("Transform/Merger: unknown merger type {0}", in_setting.MergeType));
            }
            return rtrn;
        }

        // Create a new Record (with index recordIndex) by merging the left record lrec and the right record rrec
        private static Record NewMergedRecord( long recordIndex, Record lrec, Record rrec )
        {
            bool leftIsNull  = ( lrec == null );
            bool rightIsNull = ( rrec == null );

            // New Standard Record
            Record res = Record.NewRecord( recordIndex );
            // Records are null headers
            res.SafeAdd( "LEFT_IS_NULL",  leftIsNull,  "LEFT_"  );
            res.SafeAdd( "RIGHT_IS_NULL", rightIsNull, "RIGHT_" );
            // Add the records content
            MergeInto( res, lrec, "L_" );
            MergeInto( res, rrec, "R_" );

            return res;
        }

        // Merge the fields of the source record into the target record
        // If the source field name already exist, we prefix it with the supplied prefix until
        // there is no more name collision
        // Null source can be supplied, in which case nothing is done
        private static void MergeInto( Record target, Record source, string prefix )
        {
            // Protect against null source case (authorized but ignored)
            if ( source == null )
                return;

            foreach ( KeyValuePair<string, object> kvp in source.Fields )
            {
                if ( kvp.Key.Equals( Record.ETL_PARAMS_FIELDNAME ) )
                    continue; // skip the ETL Parameters Field which is always contained in a Record
                target.SafeAdd(kvp.Key, kvp.Value, prefix);
            }
        }



        public void Start()
        {
            List<Record> lRecordSet = new List<Record>();
            List<Record> rRecordSet = new List<Record>();

            //get records
            while (m_sourceQueues[0].HasMore)
            {
                Record record = m_sourceQueues[0].Dequeue();
                if (record != null)
                {
                    lRecordSet.Add(record);
                }
            }
            while (m_sourceQueues[1].HasMore)
            {
                Record record = m_sourceQueues[1].Dequeue();
                if (record != null)
                {
                    rRecordSet.Add(record);
                }
            }
            // We can have valid cases where no data is available on one side
            //if (lRecordSet.Count == 0 || rRecordSet.Count == 0)
            //{
            //    LogManager.Instance.Log(String.Format("Transform/Merger/{0}: Not enough data", Name));
            //    return;
            //}

            List<Record> merged = Merge(m_Settings, lRecordSet, rRecordSet);
            foreach (Record rec in merged)
            {
                m_targetQueues[0].Enqueue(rec);
            }
         }
    }
}

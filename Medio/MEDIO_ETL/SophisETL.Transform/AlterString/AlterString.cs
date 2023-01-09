using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SophisETL.Transform.AlterString
{
    using SophisETL.Common;
    using SophisETL.Common.Logger;
    using SophisETL.Transform.AlterString.Xml;

    [SettingsType(typeof(Settings), "_Settings")]
    class AlterString : AbstractBasicTransformTemplate
    {
        //// Internal Members
        private Settings _Settings { get; set; }

        #region Overrides of AbstractBasicTransformTemplate

        protected override Record Transform(Record record)
        {
            foreach (Xml.AlterString stringOp in _Settings.alterString)
            {
                if (stringOp.type.ToString() == "Trim")
                    ApplyTrimOperation(stringOp, record);
                else if (stringOp.type.ToString() == "Join")
                    ApplyJoinOperation(stringOp, record);
                else
                    ApplyReplaceOperation(stringOp, record);
            }
            return record;
        }

        // Sub string 
        private void ApplyReplaceOperation(Xml.AlterString subStringOp, Record record)
        {
            string source = (string) record.Fields[subStringOp.sourceField].ToString();
            int startIndex = subStringOp.startIndex;
            int length = subStringOp.length;

            if (string.IsNullOrEmpty(source))
            {
                LogManager.Instance.LogDebug(string.Format("No source field {0} in record {1}: skipping replacement", subStringOp.sourceField, record.Key ));
            }
            else
            {
                string target = source.Substring(startIndex, Math.Min(startIndex + length, source.Length));
                record.SafeAdd(subStringOp.targetField,target,eExistsAction.replace);
            }
        }

        // Trim
        private void ApplyTrimOperation(Xml.AlterString stringOp, Record record)
        {
            string source = (string)record.Fields[stringOp.sourceField].ToString();
            if (string.IsNullOrEmpty(source))
            {
                LogManager.Instance.LogDebug(string.Format("No source field {0} in record {1}: skipping trimming", stringOp.sourceField, record.Key));
            }
            else
            {
                string target = source.Trim();
                record.SafeAdd(stringOp.targetField, target, eExistsAction.replace);
            }
        }

        // Join
        private void ApplyJoinOperation(Xml.AlterString stringOp, Record record)
        {
            string source1 = (string)record.Fields[stringOp.sourceField].ToString();
            string source2 = (string)record.Fields[stringOp.sourceField2].ToString();

            if (string.IsNullOrEmpty(source1) || string.IsNullOrEmpty(source2))
            {
                LogManager.Instance.LogDebug(string.Format("No source field {0} or field {1} in record {2}: skipping joining", stringOp.sourceField, stringOp.sourceField2, record.Key));
            }
            else
            {
                var joinedStrList = new List<string>()
                {
                    source1, source2
                };
                string target = String.Join(stringOp.separatorField, joinedStrList);
                record.SafeAdd(stringOp.targetField, target, eExistsAction.replace);
            }
        }
        #endregion
    }
}

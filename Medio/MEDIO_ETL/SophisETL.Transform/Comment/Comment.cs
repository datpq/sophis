using System;
using System.Collections.Generic;
using System.Text;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;

using SophisETL.Transform.Comment.Xml;

namespace SophisETL.Transform.Comment
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class Comment : AbstractBasicTransformTemplate
    {
        //// Internal Members
        private Settings _Settings { get; set; }

        private const string BEGIN_COMMENT = "<!--";
        private const string END_COMMENT   = "-->";

        public override void Init()
        {
            base.Init();
            // No specific checks can be done at this point
        }

        protected override Record Transform(Record record)
        {
            // Apply each Comment operation in order
            if (_Settings.comment != null)
            {
                foreach (Xml.Comment replaceOp in _Settings.comment)
                    ApplyCommentOperation(replaceOp, record);
            }         

            // Apply each MultiComment operation in order
            if (_Settings.complexComment != null)
            {
                foreach (Xml.MultiComment replaceOp in _Settings.complexComment)
                    ApplyComplexCommentOperation(replaceOp, record);
            }            

            return record;
        }

        private void ApplyComplexCommentOperation(Xml.MultiComment replaceOp, Record record)
        {
            if (!record.Fields.ContainsKey(replaceOp.targetField))
            {
                LogManager.Instance.LogDebug(String.Format("No target field {0} in record {1}: skipping replacement", replaceOp.targetField, record.Key));
            }
            else
            {
                System.Collections.IEnumerator ien = replaceOp.MultiMatch.GetEnumerator();

                bool hasAllMatch = true;
                string content = "";
                while (ien != null)
                {
                    MultiMatch multiMatch1 = ien.Current as MultiMatch;
                    bool hasNext = ien.MoveNext();
                    if (!hasNext)
                    {


                    }
                                            
                    object obj;
                    record.Fields.TryGetValue(multiMatch1.sourceField, out obj);                    
                    if (obj != null)
                    {
                        content = obj.ToString();
                    }
                    else
                    {
                        continue;
                    }

                    hasAllMatch &= hasMatched(content, multiMatch1);
                    
                }
                if (hasAllMatch)
                {
                    content = PerformMultiCommentAction(replaceOp, content);
                    record.SafeAdd(replaceOp.targetField, content, eExistsAction.replace);
                }

            }
        }

        private void ApplyCommentOperation(Xml.Comment replaceOp, Record record)
        {
            if (!record.Fields.ContainsKey(replaceOp.sourceField))
            {
                LogManager.Instance.LogDebug(String.Format("No source field {0} in record {1}: skipping replacement", replaceOp.sourceField, record.Key));
            }
            else
            {                
                object obj;
                record.Fields.TryGetValue(replaceOp.sourceField, out obj);
                string content = "";
                if (obj != null)
                {
                    content = obj.ToString();
                }
                foreach (SimpleMatch simpleMatch in replaceOp.simpleMatch)
                {
                    if (string.IsNullOrEmpty(simpleMatch.match) && string.IsNullOrEmpty(content))
                    {
                        content = PerformEmptyCommentMatch(simpleMatch, content);
                    }
                    else if (string.IsNullOrEmpty(simpleMatch.match) == false)
                    {
                        if (hasMatched(content, simpleMatch))
                        {
                            content = PerformCommentAction(simpleMatch, content);
                        }
                        else
                        {
                            content = "";
                        }                                                
                    } else
                    {
                        content = content.Replace(content, string.Empty);
                    }
                }                
                record.SafeAdd(replaceOp.targetField, content, eExistsAction.replace);                                                     
            }
        }

        private bool PerformMatch(MultiMatch match1, MultiMatch match2)
        {
            //if (match1.operand.Equals(Operand.And))
            //{
            //    return hasMatched(
            //}
            return true;
        }

        private bool hasMatched(object fieldValue, MultiMatch filter)
        {
            bool hasMatched = true;

            // see if we can get the filterValue in the same type
            object filterValue = null;
            try
            {
                filterValue = Convert.ChangeType(filter.match, fieldValue.GetType());
            }
            catch (Exception ex)
            {
                ex.GetType(); // avoid warning
            }

            if (fieldValue is IComparable)
            {
                int compare = ((IComparable)fieldValue).CompareTo(filterValue);
                if (
                     (filter.comparator == Comparator.equal && compare != 0)
                  || (filter.comparator == Comparator.different && compare == 0)
                  || (filter.comparator == Comparator.greater && compare < 0)
                  || (filter.comparator == Comparator.smaller && compare > 0)
                    )
                {
                    hasMatched = false;       
                }
            }
            else
            {
                bool fieldEquals = filterValue.Equals(fieldValue);
                if (
                     (filter.comparator == Comparator.equal && !fieldEquals)
                  || (filter.comparator == Comparator.different && fieldEquals)
                  || (filter.comparator == Comparator.greater)
                  || (filter.comparator == Comparator.smaller)
                    )
                {
                    hasMatched = false;             
                }
            }

            return hasMatched;
        }

        private bool hasMatched(object fieldValue, SimpleMatch filter)
        {
            bool hasMatched = true;

            // see if we can get the filterValue in the same type
            object filterValue = null;
            try
            {
                filterValue = Convert.ChangeType(filter.match, fieldValue.GetType());
            }
            catch (Exception ex)
            {
                ex.GetType(); // avoid warning
            }




            if (fieldValue is IComparable)
            {
                int compare = ((IComparable)fieldValue).CompareTo(filterValue);
                if (
                     (filter.comparator == Comparator.equal && compare != 0)
                  || (filter.comparator == Comparator.different && compare == 0)
                  || (filter.comparator == Comparator.greater && compare < 0)
                  || (filter.comparator == Comparator.smaller && compare > 0)
                    )
                {
                    hasMatched = false;
                }
            }
            else
            {
                bool fieldEquals = filterValue.Equals(fieldValue);
                if (
                     (filter.comparator == Comparator.equal && !fieldEquals)
                  || (filter.comparator == Comparator.different && fieldEquals)
                  || (filter.comparator == Comparator.greater)
                  || (filter.comparator == Comparator.smaller)
                    )
                {
                    hasMatched = false;
                }
            }

            return hasMatched;
        }


        //private string PerformCommentAction(SimpleMatch simpleMatch, string content)
        //{
        //    string result = content;
        //    switch (simpleMatch.action)
        //    {
        //        case Quote.beginComment:
        //            result = content.Replace(simpleMatch.match, BEGIN_COMMENT);
        //            break;
        //        case Quote.endComment:
        //            result = content.Replace(simpleMatch.match, END_COMMENT);
        //            break;
        //    }
        //    return result;
        //}

        private string PerformCommentAction(SimpleMatch simpleMatch, string content)
        {
            string result = content;
            switch (simpleMatch.action)
            {
                case Quote.beginComment:
                    result = BEGIN_COMMENT;
                    break;
                case Quote.endComment:
                    result = END_COMMENT;
                    break;
            }
            return result;
        }

        private string PerformEmptyCommentMatch(SimpleMatch simpleMatch, string content)
        {
            string result = content;
            switch (simpleMatch.action)
            {
                case Quote.beginComment:
                    result = content.Insert(0, BEGIN_COMMENT);
                    break;
                case Quote.endComment:
                    result = content.Insert(0, END_COMMENT);
                    break;
            }
            return result;
        }


        private string PerformMultiCommentAction(MultiComment simpleMatch, string content)
        {
            string result = content;
            switch (simpleMatch.action)
            {
                case Quote.beginComment:
                    result = content.Replace(simpleMatch.targetField, BEGIN_COMMENT);
                    break;
                case Quote.endComment:
                    result = content.Replace(simpleMatch.targetField, END_COMMENT);
                    break;
            }
            return result;
        }
    }
}

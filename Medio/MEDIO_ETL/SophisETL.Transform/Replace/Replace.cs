using System;
using System.Collections.Generic;
using System.Text;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;

using SophisETL.Transform.Replace.Xml;
using System.Text.RegularExpressions;



namespace SophisETL.Transform.Replace
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class Replace : AbstractBasicTransformTemplate
    {
        //// Internal Members
        private Settings _Settings { get; set; }

        private const string AND_ANMP = "&amp;";
        private const string AND = "&";

        public override void Init()
        {
            base.Init();
            // No specific checks can be done at this point
        }

        protected override Record Transform( Record record )
        {
            // Apply each Replace operation in order
            foreach ( Xml.Replace replaceOp in _Settings.replace )
                ApplyReplaceOperation( replaceOp, record );

            return record;
        }

        private void ApplyReplaceOperation( Xml.Replace replaceOp, Record record )
        {
            if ( !record.Fields.ContainsKey( replaceOp.sourceField ) )
            {
                LogManager.Instance.LogDebug( String.Format( "No source field {0} in record {1}: skipping replacement", replaceOp.sourceField, record.Key ) );
            }
            else
            {
                string content = record.Fields[ replaceOp.sourceField ].ToString();
                if (replaceOp.simpleMatch != null)
                {
                    foreach (SimpleMatch simpleMatch in replaceOp.simpleMatch)
                    {
                        if (string.IsNullOrEmpty(simpleMatch.match))
                        {
                            content = PerformEmptyMatch(simpleMatch, content);
                        }
                        else if (AND.Equals(simpleMatch.match))
                        {
                            content = PerformSpecialMatch(simpleMatch, content);
                        }
                        else
                        {
                            content = PerformSimpleMatch(simpleMatch, content);
                        }
                    }
                }
                if (replaceOp.regexMatch != null)
                {
                    foreach (RegexMatch regexMatch in replaceOp.regexMatch)
                    {
                        if (string.IsNullOrEmpty(regexMatch.match))
                        {
                            content = PerformEmptyMatch(regexMatch, content);
                        }
                        else if (AND.Equals(regexMatch.match))
                        {
                            content = PerformSpecialMatch(regexMatch, content);
                        }
                        else
                        {
                            content = PerformSimpleMatch(regexMatch, content);
                        }
                    }
                }

                record.SafeAdd( replaceOp.targetField, content, eExistsAction.replace );
            }
        }

        // We apply a Simple Match rule using the String.Replace method
        private string PerformSimpleMatch( SimpleMatch simpleMatch, string content )
        {
            return content.Replace( simpleMatch.match, simpleMatch.replace );
        }
        private string PerformSimpleMatch(RegexMatch regexMatch, string content)
        {
            Regex rgx = new Regex(regexMatch.match);
            return rgx.Replace(content, regexMatch.replace);
        }

        
        private string PerformEmptyMatch(SimpleMatch simpleMatch, string content)
        {
            return content.Insert(0, simpleMatch.replace);
        }
        private string PerformEmptyMatch(RegexMatch regexMatch, string content)
        {
            return content.Insert(0, regexMatch.replace);
        }
        private string PerformSpecialMatch(SimpleMatch simpleMatch, string content)
        {
            return content.Replace(simpleMatch.match, AND_ANMP);
        }
        private string PerformSpecialMatch(RegexMatch regexMatch, string content)
        {
            return content.Replace(regexMatch.match, AND_ANMP);
        }
    }
}

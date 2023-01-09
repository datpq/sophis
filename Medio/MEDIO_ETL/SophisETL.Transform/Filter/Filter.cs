using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

using SophisETL.Common;
using SophisETL.Common.Tools;
using SophisETL.Common.ErrorMgt;
using SophisETL.Queue;
using SophisETL.Transform;

using SophisETL.Transform.Filter.Xml;


namespace SophisETL.Transform.Filter
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class Filter : AbstractBasicTransformTemplate
    {
        private Settings _Settings { get; set; }


        public override void Init()
        {
            base.Init();
        }

        protected override Record Transform(Record record)
        {
            bool recordToConsider = true;

            // check the syntax of the incoming XML, if it makes sense or not
            foreach ( Xml.Filter filter in _Settings.filters.filter )
            {
                // make sure filter field is known
                if ( !record.Fields.ContainsKey( filter.field ) )
                {
                    recordToConsider = false;
                    break;
                }

                // Extract the values to compare
                object fieldValue = record.Fields[filter.field];
                string filterValueStr = filter.value;

                // Handle the NULL case
                if ( fieldValue == null && !( filterValueStr == null || filterValueStr.Length == 0 ) )
                {
                    // null versus non-empty, can not compare
                    recordToConsider = false;
                    break;
                }

                // see if we can get the filterValue in the same type
                object filterValue = null;
                try
                {
                    filterValue = Convert.ChangeType( filterValueStr, fieldValue.GetType() );
                }
                catch ( Exception ex )
                {
                    ex.GetType(); // avoid warning
                    recordToConsider = false; // types are not compatible
                    break;
                }

                // compare them
                if ( fieldValue is IComparable )
                {
                    int compare = ( (IComparable) fieldValue ).CompareTo( filterValue );
                    if (
                         ( filter.comparator == Comparator.equal && compare != 0 )
                      || ( filter.comparator == Comparator.different && compare == 0 )
                      || ( filter.comparator == Comparator.greater && compare < 0 )
                      || ( filter.comparator == Comparator.smaller && compare > 0 )
                        )
                    {
                        recordToConsider = false;
                        break;
                    }
                }
                else
                {
                    bool fieldEquals = filterValue.Equals( fieldValue );
                    if (
                         ( filter.comparator == Comparator.equal && !fieldEquals )
                      || ( filter.comparator == Comparator.different && fieldEquals )
                      || ( filter.comparator == Comparator.greater )
                      || ( filter.comparator == Comparator.smaller )
                        )
                    {
                        recordToConsider = false;
                        break;
                    }
                }
            }

            if ( !recordToConsider )
            {
                // Record does not match the filters, what do we do with it ?
                if ( _Settings.filters.orElse == OrElseEnum.error )
                    ReportErrorOnRecord( record );

                record = null; // filter it out
            }

            return record;
        }


        // The Record is in Error
        private void ReportErrorOnRecord( Record record )
        {
            // Build the list of objects used to make the custom error report
            string[] fields = _Settings.filters.errorFields.Split(new char[] { ',' });
            List<object> fieldValues = new List<object>();
            foreach( string field in fields )
            {
                string fieldName = field.Trim();
                if ( record.Fields.ContainsKey( fieldName ) )
                    fieldValues.Add( ToDisplayableValue(record.Fields[ fieldName ]) );
                else
                    fieldValues.Add( "unknown:" + fieldName );
            }

            object[] paramObjects = fieldValues.ToArray();

            ErrorHandler.Instance.HandleError( new ETLError {
                Step    = this,
                Message = String.Format( _Settings.filters.errorMessage, paramObjects ),
                Record  = record
            } );
        }

        // For now, just a specific management of collections
        private object ToDisplayableValue( object p )
        {
            if ( p is System.Collections.ICollection )
            {
                string pStr = "";
                foreach ( object o in ( p as System.Collections.ICollection ) )
                    pStr += ToDisplayableValue( o ) + ", ";
                return pStr.Length > 2 ? pStr.Substring( 0, pStr.Length - 2 ) : pStr;
            }
            else
                return p;
        }
    }
}

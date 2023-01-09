using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Commons.Collections;
using SophisETL.Common;
using SophisETL.Common.ErrorMgt;
using SophisETL.Common.Logger;
using SophisETL.Transform.Velocity.Medio;
using SophisETL.Transform.Velocity.Medio.Xml;

namespace SophisETL.Transform.Velocity.Medio
{
    /// <summary>
    /// The Velocity transformer uses the NVelocity Library to merge
    /// The current Record into a template, the resulting String is saved into the Record
    /// </summary>

    [SettingsType(typeof(Settings), "_Settings")]
    public class MedioBenchmarkVelocityTransformer : ITransformStep
    {
        //// Internal Members
        private NVelocity.Template _VelocityTemplate;
        private Settings _Settings { get; set; }

        // Internal members
        private int _RecordsTransformedCount = 0;

        #region ITransformStep Members

        //---- Forces One Source / One Target - available through SourceQueue and Target Queue ----
        public List<IETLQueue> SourceQueues { get { return _SourceQueues; } }
        private List<IETLQueue> _SourceQueues = new List<IETLQueue>();

        public List<IETLQueue> TargetQueues { get { return _TargetQueues; } }
        private List<IETLQueue> _TargetQueues = new List<IETLQueue>();

        // We have an Input and a Target Queue aliased to the List when we have only one
        public IETLQueue SourceQueue { get { return SourceQueues[0]; } set { SourceQueues.Clear(); SourceQueues.Add(value); } }
        public IETLQueue TargetQueue { get { return TargetQueues[0]; } set { TargetQueues.Clear(); TargetQueues.Add(value); } }
        #endregion

        #region IETLStep Members
        private string _Name;
        public string Name { get { return _Name; } set { _Name = value; } }
        #endregion

        public virtual void Init()
        {
            // Make sure we have only 1 Source / Target Queue
            if (_SourceQueues.Count != 1)
                throw new Exception("Transform step [" + Name + "/" + this.GetType().Name + "] must have one and only one Source Queue");
            if (_TargetQueues.Count != 1)
                throw new Exception("Transform step [" + Name + "/" + this.GetType().Name + "] must have one and only one Source Queue");

            // Open the Velocity Engine
            NVelocity.App.Velocity.Init();
            try
            {
                // Initialize the template already if it is a file, otherwise we will have to use record info for the template
                if (_Settings.template.kind == TemplateKindEnum.file)
                {
                    if (Path.IsPathRooted(_Settings.template.Value))
                    {
                        string relativePath = GetRelativePath(_Settings.template.Value, Directory.GetCurrentDirectory());
                        _VelocityTemplate = NVelocity.App.Velocity.GetTemplate(relativePath);
                    }
                    else
                        _VelocityTemplate = NVelocity.App.Velocity.GetTemplate(_Settings.template.Value);
                }
            }
            catch ( Exception ex )
            {
                // TODO: replace by proper error reporting framework    
                throw new Exception( "Error: can not find Velocity Template", ex );
            }
        }

        string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public void Start()
        {
            while (SourceQueue.HasMore)
            {
                try
                {
                    TargetQueue.Enqueue(Transform(GetRecords()));
                    _RecordsTransformedCount++;
                }
                catch (Exception ex)
                {
                    // Enrich the exception and pass it to the Error Handler
                    ErrorHandler.Instance.HandleError(new ETLError()
                    {
                        Step = this,
                        Exception = ex,
                        Message = ex.Message,
                    });
                }
            }

            LogManager.Instance.Log("Transform/Basic/" + Name + ": step finished - "
                                    + _RecordsTransformedCount + " record(s) transformed");
        }

        protected virtual Record Transform(List<Record> records)
        {
            var record = records[0];

            NVelocity.VelocityContext context = new NVelocity.VelocityContext();
            record.SafeAdd("creationTimestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"), eExistsAction.replace);
            record.SafeAdd("today", DateTime.Now.ToString("yyyy-MM-dd"), eExistsAction.replace);
            record.SafeAdd("time", DateTime.Now.ToString("HH:mm:ss"), eExistsAction.replace);
            context.Put( "record", record );

            // Template Merging depends on the settings
            StringWriter stringWriter = new StringWriter();
            if (_Settings.template.kind == TemplateKindEnum.file)
            {
                _VelocityTemplate.Merge(context, stringWriter);
            }
            // benchmark component xml
            string benchmarkXML = Between(stringWriter.ToString(), "<!-- copy - do not remove -->", "<!-- end - do not remove -->");
            foreach (string fieldKey in record.Fields.Keys)
            {
                if( record.Fields[fieldKey] is string )
                {
                    context.Put(fieldKey, WebUtility.HtmlEncode((string)record.Fields[fieldKey]));
                }
                else
                {
                    context.Put(fieldKey, record.Fields[fieldKey]);
                }
            }
            context.Put("templateHelper", new TemplateHelper()); // class with useful methods
            context.Put("regexHelper", new Regex(""));

            // Template Merging depends on the settings
            stringWriter = new StringWriter();
            bool evaluationSuccess = true;

            if ( _Settings.template.kind == TemplateKindEnum.file )
            {
                // We use the template instanciated at run-time
                _VelocityTemplate.Merge( context, stringWriter );
            }
            else if ( _Settings.template.kind == TemplateKindEnum.field )
            {
                // Override the template with a record field
                string fieldName = _Settings.template.Value;
                if ( ! record.Fields.ContainsKey( fieldName ) )
                    throw new Exception( String.Format( "Missing Velocity Template Field ({0}) in Record {1}", fieldName, record.Key ) );

                evaluationSuccess = NVelocity.App.Velocity.Evaluate(context, stringWriter, "Field[" + fieldName + "]", WebUtility.HtmlEncode(record.Fields[fieldName].ToString()));
            }
            else if ( _Settings.template.kind == TemplateKindEnum.inline )
            {
                // We use directly the field value as a Template (useful for quick token replacement)
                evaluationSuccess = NVelocity.App.Velocity.Evaluate( context, stringWriter, "inline", _Settings.template.Value );
            }

            if ( !evaluationSuccess )
                throw new Exception( "Velocity Transformation failed (check Velocity logs for details)" );

            string result = stringWriter.ToString();

            // start from the second record, as the first one has already been merged
            for (int i = 1; i < records.Count; i++)
            {
                var oneRecord = records[i];
                string newComponent = ReplaceFields(oneRecord, benchmarkXML);
                string toAppend = Environment.NewLine + "<!-- copy - do not remove -->" + newComponent + "<!-- end - do not remove -->" + Environment.NewLine;
                result = result.Insert(result.LastIndexOf("<!-- end - do not remove -->"), toAppend);
            }

            // Store it into the Record
            record.SafeAdd( _Settings.targetField, result, eExistsAction.replace );
            return record;
        }

        public List<Record> GetRecords()
        {
            List<Record> theRecordSet = new List<Record>();

            // get all the records and add them to the list (only use the first Queue as there is only one)
            while (_SourceQueues[0].HasMore)
            {
                Record record = _SourceQueues[0].Dequeue();
                if (record != null)
                    theRecordSet.Add(record);
            }
            return theRecordSet;
        }

        private string Between(string Text, string StartString, string EndString)
        {
            string FinalString = "";
            int Pos1 = Text.IndexOf(StartString) + StartString.Length;
            int Pos2 = Text.IndexOf(EndString);
            FinalString = Text.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }

        private string ReplaceFields(Record record, string xmlTemplateStr)
        {
            foreach (string fieldKey in record.Fields.Keys)
            {
                if( record.Fields[fieldKey] is string )
                {
                    xmlTemplateStr = xmlTemplateStr.Replace("$" + fieldKey, WebUtility.HtmlEncode((string)record.Fields[fieldKey]));
                }
                else
                {
                    xmlTemplateStr = xmlTemplateStr.Replace("$" + fieldKey, record.Fields[fieldKey].ToString());
                }
            }
            return xmlTemplateStr;
        }


    }
}

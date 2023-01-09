using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using NVelocity;

using SophisETL.Common;
using SophisETL.Common.Tools;
using SophisETL.Queue;
using SophisETL.Transform;

using SophisETL.Transform.Velocity.Xml;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;



namespace SophisETL.Transform.Velocity
{
    /// <summary>
    /// The Velocity transformer uses the NVelocity Library to merge
    /// The current Record into a template, the resulting String is saved into the Record
    /// </summary>
    [SettingsType(typeof(Settings), "_Settings")]
    public class VelocityTransformer : AbstractBasicTransformTemplate
    {
        //// Internal Members
        private NVelocity.Template _VelocityTemplate;
        private Settings _Settings { get; set; }


        public override void Init()
        {
            base.Init();

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

        protected override Record Transform( Record record )
        {
//#if DEBUG
//            Debugger.Launch();
//#endif
            // Prepare the Velocity Context Object (Add the record itself but also each field it contains)
            NVelocity.VelocityContext context = new NVelocity.VelocityContext();
            record.SafeAdd("creationTimestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"), eExistsAction.replace);
            record.SafeAdd("today", DateTime.Now.ToString("yyyy-MM-dd"), eExistsAction.replace);
            record.SafeAdd("time", DateTime.Now.ToString("HH:mm:ss"), eExistsAction.replace);
            context.Put( "record", record );
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
            StringWriter stringWriter = new StringWriter();
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

            // Store it into the Record
            record.SafeAdd( _Settings.targetField, result, eExistsAction.replace );

            return record;
        }
    }
}

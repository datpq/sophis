using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using SophisETL.Xml;
using SophisETL.Common;
using SophisETL.Common.Reporting;



namespace SophisETL.Runtime.ReportBuilder
{
    internal class ReportingHandlerBuilder
    {
        internal IReportingHandler BuildReportingHandler( Report reportDefinition )
        {
            try
            {
                Type candidateType = Type.GetType( reportDefinition.@class );
                if ( candidateType == null )
                    throw new Exception( "Can not resolve Type Name: " + reportDefinition.@class );

                // Instanciate and make sure it is a IReportingHandler
                object candidate = Activator.CreateInstance( candidateType );
                if ( !( candidate is IReportingHandler ) )
                    throw new Exception( "ERROR: Type " + candidateType.ToString() + " is not a valid IReportingHandler" );

                // Assign the basic properties
                IReportingHandler reportingHandler = candidate as IReportingHandler;
                reportingHandler.Name = reportDefinition.name;
                AssignSettings( reportingHandler, reportDefinition.settings );

                return reportingHandler;
            }
            catch ( Exception ex )
            {
                throw new Exception( String.Format("Failed to build Reporting Handler {0}: {1}", reportDefinition.name, ex.Message), ex );
            }
        }


        /// <summary>
        /// Assign the Settings to the Reporting Handler
        /// An injection method is used to give a specially instanciated object depending on the
        /// field setup requirements
        /// </summary>
        /// <param name="step">The Step Instance</param>
        /// <param name="settings">Settings coming from the XML file</param>
        /// 
        // TODO - Factorize this generic method between ChainBuilder and ReportBuilder
        private void AssignSettings( object targetClass, AnySettings stepSettings )
        {
            // Dynamic settings: has the Step a SettingsType attribute?
            SettingsTypeAttribute settingsTypeAttribute = Attribute.GetCustomAttribute( targetClass.GetType(), typeof( SettingsTypeAttribute ) ) as SettingsTypeAttribute;
            bool hasSettingsTypeAttribute = ( settingsTypeAttribute != null && settingsTypeAttribute.SettingsType != null );
            bool hasSettingsInXml         = ( stepSettings != null && stepSettings.Any != null && stepSettings.Any.Length > 0 );

            if ( hasSettingsInXml && hasSettingsTypeAttribute )
            {
                // All the best: we have settings both sides, let's inject them
                // But for that we need to re(de)serialize our "Any" settings element into a proper
                // Settings class expected by the Step - so we serialize and deserialize it again into
                // the proper type
                // For this, we reuse the "XmlRootAttribute" of the target settings class (we assume it
                // is XML (de)-serializable otherwise this does not work!)
                XmlRootAttribute customSettingsRootAttribute = Attribute.GetCustomAttribute(
                        settingsTypeAttribute.SettingsType, typeof( XmlRootAttribute ) ) as XmlRootAttribute;
                XmlSerializer genericSettingsSerializer = new XmlSerializer( typeof( AnySettings ), customSettingsRootAttribute );
                StringWriter genericSettings = new StringWriter();
                genericSettingsSerializer.Serialize( genericSettings, stepSettings );
                // Replace the Variables
                string genericSettingsWoVariables = genericSettings.ToString();
                // FIXME: now all done at top level
                //GlobalSettings.Instance.ReplaceSettings( genericSettings.ToString() );

                XmlSerializer settingsSerializer = new XmlSerializer( settingsTypeAttribute.SettingsType );
                object result = settingsSerializer.Deserialize( new StringReader( genericSettingsWoVariables ) );

                // We now have our settings instance, inject it in the class
                PropertyInfo settingsPropertyInfo = targetClass.GetType().GetProperty( settingsTypeAttribute.SettingsPropertyName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
                settingsPropertyInfo.SetValue( targetClass, result, null );
            }
            else if ( hasSettingsInXml && !hasSettingsTypeAttribute )
            {
                if ( targetClass is IReportingHandler )
                    throw new Exception( String.Format(
                            "ERROR: For Report {0}, this reporting handler does not accept settings, remove them from the definition",
                            ( (IReportingHandler) targetClass ).Name ) );
                else
                    throw new Exception( String.Format(
                            "ERROR: For Object {0}, this object does not accept settings, remove them from the definition",
                            targetClass.GetType().Name ) );

            }
            else if ( !hasSettingsInXml && hasSettingsTypeAttribute )
            {
                if ( targetClass is IReportingHandler )
                    throw new Exception( String.Format(
                            "ERROR: For Report {0}, this reporting handler requires specific settings, add them to the definition",
                            ( (IReportingHandler) targetClass ).Name ) );
                else
                    throw new Exception( String.Format(
                            "ERROR: For Object {0}, this object requires specific settings, add them to the definition",
                           targetClass.GetType().Name ) );
            }
            // else we have no settings in XML and no step settings required, which is all right :-)
        }
    }
}

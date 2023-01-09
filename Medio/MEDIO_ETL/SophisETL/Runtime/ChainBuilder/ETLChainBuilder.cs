using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

using SophisETL.Common;
using SophisETL.Common.GlobalSettings;
using SophisETL.Queue;

using SophisETL.Xml;



namespace SophisETL.Runtime.ChainBuilder
{
    internal class ETLChainBuilder
    {
        // Internal Members
        private ETLChain       _ETLChain;

        // private property to initialize on 1st access
        private ETLQueueManager _QueueManager;
        private ETLQueueManager QueueManager { get { if ( _QueueManager == null ) _QueueManager = ETLQueueManager.Instance; return _QueueManager; } }


        internal ETLChainBuilder()
        {
            _ETLChain = new ETLChain();
        }

        /// <summary>
        /// Assign the Chain Name
        /// </summary>
        /// <param name="chainName"></param>
        internal void SetChainName( string chainName )
        {
            _ETLChain.Name = chainName;
        }

        private IETLStep AddStepCommon( string @class, string name, AnySettings settings )
        {
            // Create the Extract Step object
            try
            {
                Type candidateType = Type.GetType( @class );
                if ( candidateType == null )
                    throw new Exception( "Can not resolve Type Name: " + @class );


                object candidate = Activator.CreateInstance( candidateType );
                if ( !( candidate is IETLStep ) )
                {
                    throw new Exception( "ERROR: Type " + candidateType.ToString() + " is not a valid IETLStep" );
                }

                // Assign the basic properties
                IETLStep step = (IETLStep) candidate;
                step.Name = name;
                AssignSettings( step, settings );

                return step;
            }
            catch ( Exception ex )
            {
                throw new Exception( "AddStepCommon failed (" + ex.Message + ")", ex );
            }
        }

        internal void AddExtractStep( ETLChainDefinitionExtract xmlExtract )
        {
            IETLStep stepCandidate = AddStepCommon( xmlExtract.@class, xmlExtract.name, xmlExtract.settings );
            if ( !( stepCandidate is IExtractStep ) )
            {
                throw new Exception( "ERROR: Type " + stepCandidate.GetType().ToString() + " is not a valid IExtractStep" );
            }
            IExtractStep extractStep = (IExtractStep) stepCandidate;

            // Connect the target queue
            IETLQueue targetQueue = QueueManager.GetQueue( xmlExtract.target );
            extractStep.TargetQueue = targetQueue;
            //targetQueue.RegisterProducer( extractStep );

            _ETLChain.Steps.Add( extractStep );
        }


        internal void AddTransformStep( ETLChainDefinitionTransform xmlTransform )
        {
            IETLStep stepCandidate = AddStepCommon( xmlTransform.@class, xmlTransform.name, xmlTransform.settings );
            if ( !( stepCandidate is ITransformStep ) )
            {
                throw new Exception( "ERROR: Type " + stepCandidate.GetType().ToString() + " is not a valid ITransformStep" );
            }
            ITransformStep transformStep = (ITransformStep) stepCandidate;

            // Connect the source queues
            foreach ( string sourceQueueName in xmlTransform.source )
            {
                IETLQueue sourceQueue = QueueManager.GetQueue( sourceQueueName );
                transformStep.SourceQueues.Add( sourceQueue );
                //sourceQueue.RegisterConsumer( transformStep );
            }


            // Connect the target queues
            foreach ( string targetQueueName in xmlTransform.target )
            {
                IETLQueue targetQueue = QueueManager.GetQueue( targetQueueName );
                transformStep.TargetQueues.Add( targetQueue );
                //targetQueue.RegisterProducer( transformStep );
            }

            _ETLChain.Steps.Add( transformStep );
        }


        internal void AddLoadStep( ETLChainDefinitionLoad xmlLoad )
        {
            IETLStep stepCandidate = AddStepCommon( xmlLoad.@class, xmlLoad.name, xmlLoad.settings );
            if ( !( stepCandidate is ILoadStep ) )
            {
                throw new Exception( "ERROR: Type " + stepCandidate.GetType().ToString() + " is not a valid ILoadStep" );
            }
            ILoadStep loadStep = (ILoadStep) stepCandidate;
            // Connect the target queue
            loadStep.SourceQueue = QueueManager.GetQueue( xmlLoad.source );
            //loadStep.SourceQueue.RegisterConsumer( loadStep );

            _ETLChain.Steps.Add( loadStep );
        }


        /// <summary>
        /// Assign the Settings to the Step
        /// An injection method is used to give a specially instanciated object depending on the
        /// field setup requirements
        /// </summary>
        /// <param name="step">The Step Instance</param>
        /// <param name="settings">Settings coming from the XML file</param>
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
                if ( targetClass is IETLStep )
                    throw new Exception( String.Format(
                            "ERROR: In Chain {0}, Step {1}, this step does not accept settings, remove them from the definition",
                            _ETLChain.Name, ((IETLStep) targetClass).Name ) );
                else
                    throw new Exception( String.Format(
                            "ERROR: In Chain {0}, Object {1}, this object does not accept settings, remove them from the definition",
                            _ETLChain.Name, targetClass.GetType().Name ) );

            }
            else if ( !hasSettingsInXml && hasSettingsTypeAttribute )
            {
                if ( targetClass is IETLStep )
                    throw new Exception( String.Format(
                            "ERROR: In Chain {0}, Step {1}, this step requires specific settings, add them to the definition",
                            _ETLChain.Name, ( (IETLStep) targetClass ).Name ) );
                else
                    throw new Exception( String.Format(
                            "ERROR: In Chain {0}, Object {1}, this object requires specific settings, add them to the definition",
                            _ETLChain.Name, targetClass.GetType().Name ) );
            }
            // else we have no settings in XML and no step settings required, which is all right :-)
        }


        /// <summary>
        /// Instanciate an Engine class and assigns its settings
        /// (Engine are singleton with an Instance parameter)
        /// </summary>
        /// <param name="xmlEngine"></param>
        internal IETLEngine AddEngine(ETLChainDefinitionEngine xmlEngine)
        {
            // Try to load the Engine
            // Create the Extract Step object
            try
            {
                Type candidateType = Type.GetType(xmlEngine.@class);
                if (candidateType == null)
                    throw new Exception("Can not resolve Type Name: " + xmlEngine.@class);

                // Make sure it is an IETLEngine
                if (candidateType.IsSubclassOf(typeof(IETLEngine)))
                    throw new Exception("ERROR: Type " + candidateType.ToString() + " is not a valid IETLEngine");

                // Get the "Instance" singleton static property
                PropertyInfo instanceAccessProperty = candidateType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceAccessProperty == null)
                    throw new Exception("An ETL Engine must have a public static Instance property returning the single Engine instance, fix the code of " + xmlEngine.@class);

                // Create the Instance through the property
                IETLEngine engineInstance = instanceAccessProperty.GetValue(null, null) as IETLEngine;
                AssignSettings(engineInstance, xmlEngine.settings);

                _ETLChain.Engines.Add(engineInstance);

                return engineInstance;
            }
            catch (Exception ex)
            {
                throw new Exception("AddEngine failed (" + ex.Message + ")", ex);
            }
        }

        /// <summary>
        /// Return the built chain, ready to use!
        /// </summary>
        /// <returns></returns>
        internal ETLChain GetChain()
        {
            return _ETLChain;
        }


       
    }
}

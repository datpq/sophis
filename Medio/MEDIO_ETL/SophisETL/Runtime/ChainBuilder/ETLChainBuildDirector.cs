using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SophisETL.Runtime;
using SophisETL.Queue;
using SophisETL.Xml;



namespace SophisETL.Runtime.ChainBuilder
{
    public class ETLChainBuildDirector
    {
        public ETLChain BuildChain( ETLChainDefinition xmlChain )
        {
            // Prepare the Queue Factory and Inject it to the Queue Manager
            ETLQueueManager queueManager = ETLQueueManager.Instance;
            queueManager.Init( /** we will put the settings here **/ );

            ETLChainBuilder etlChainBuilder = new ETLChainBuilder();

            etlChainBuilder.SetChainName( xmlChain.name == null ? "Chain1" : xmlChain.name );

            foreach ( ETLChainDefinitionExtract xmlExtract in xmlChain.extract )
            {
                etlChainBuilder.AddExtractStep( xmlExtract );
            }

            if ( xmlChain.transform != null )
            {
                foreach ( ETLChainDefinitionTransform xmlTransform in xmlChain.transform )
                {
                    etlChainBuilder.AddTransformStep( xmlTransform );
                }
            }

            foreach ( ETLChainDefinitionLoad xmlLoad in xmlChain.load )
            {
                etlChainBuilder.AddLoadStep( xmlLoad );
            }

            // Parse Engine Setup and Initialize them
            if (xmlChain.engine != null)
                Array.ForEach<ETLChainDefinitionEngine>(xmlChain.engine, e => etlChainBuilder.AddEngine(e));

            // clean queues ...
            ETLQueueManager.Instance.CleanQueues();
            return etlChainBuilder.GetChain();
        }
    }
}

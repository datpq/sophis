using System;
using System.Collections.Generic;
using System.Threading;


using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.ErrorMgt;
using SophisETL.Common.Reporting;

namespace SophisETL.Runtime
{
    public class ETLChain : IETLChain
    {
        // Internal Members
        private List<IETLStep> _ETLSteps = new List<IETLStep>();
        public List<IETLStep>   Steps { get { return _ETLSteps; } }
        private List<IETLEngine> _ETLEngines = new List<IETLEngine>();
        public List<IETLEngine> Engines { get { return _ETLEngines; } }
    
        /// <summary>Chain Name</summary>
        public string Name { get; set; }

        /// <summary>Set to true if the Chain experienced a General Error and killed itself</summary>
        public  bool  ChainInError { get { return _ChainInError; }  }
        private bool _ChainInError = false; // will be set to true if the chain must be stopped

        // Will link the Reporting Handler Events to the Load blocks
        public void AttachReportingHandler( IReportingHandler reportingHandler )
        {
            _ETLSteps.FindAll( s => ( s is ILoadStep ) ).ForEach( l =>
                {
                    ( (ILoadStep) l ).RecordLoaded    += new RecordLoadedEventHandler(reportingHandler.HandleRecordLoadedEvent);
                    ( (ILoadStep) l ).RecordNotLoaded += new RecordNotLoadedEventHandler( reportingHandler.HandleRecordNotLoadedEvent );
                }
            );
        }


        // Delegates the Init methods to the Steps
        // returns true if initialization went fine, false if an error occurred
        public bool Init()
        {
//#if DEBUG
//            System.Diagnostics.Debugger.Launch();
//#endif
            LogManager log = LogManager.Instance;
            foreach ( IETLStep step in _ETLSteps )
            {
                log.Log( "Initializing Step " + step.Name + "..." );
                try
                {
                    step.Init();
                }
                catch ( Exception ex )
                {
                    log.Log( "Exception during initialization of step " + step.Name + ": " + ex.Message + "\n" + ex.ToString() );
                    return false;
                }
            }

            return true;
        }

        // Delegates the Start methods to the Steps
        public void Start()
        {
            LogManager.Instance.Log( "Chain " + Name + " is starting..." );

            foreach ( IETLStep step in _ETLSteps )
            {
                //The step are correctly sorted, we start them one after the other
                step.Start();

                //Remove the asynchronous part, not usefull.

                //if ( _ChainInError )
                //{
                //    LogManager.Instance.Log( "Chain " + Name + " has received a General Error: aborting Start" );
                //    break;
                //}

                //// Decorate the step with Top Level Error Management and listen to it
                //StepDecorator decoratedStep = new StepDecorator( step );
                //decoratedStep.GeneralErrorOnStep += new EventHandler<GeneralErrorEventArgs>( OnStepGeneralError );

                //// Start the step in its own thread
                //ThreadStart job = new ThreadStart( decoratedStep.Start );
                //Thread thread = new Thread( job );
                //thread.Name = "[C:" + Name + "]-Step " + step.Name;
                //thread.Start();
                //_WorkerThreads.Add( thread );
            }
        }

        public void Dispose()
        {
            // For the moment dispose only Engines, but could be a nice idea for steps as well!
            _ETLEngines.ForEach(e => e.DisposeEngine());
        }

        void OnStepGeneralError( object sender, GeneralErrorEventArgs e )
        {
            _ChainInError = true;
            LogManager.Instance.Log( String.Format(
                    "Chain {0}: General Error received on step {1}, asking for chain abort", Name, e.FailedStepName ) );
        }
    }
}

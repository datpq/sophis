using Oracle.DataAccess.Client;
using sophis.backoffice_kernel;
using sophis.oms;
using Sophis.Logging;
using Sophis.WF.Core;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sophis.OMS.Activities;
using sophis;
using sophis.portfolio;
using sophis.tools;
using System.Configuration;

namespace MEDIO.OMS.WF4Activities.Activity
{
    public sealed class ProcessTrade : CodeActivity
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();

        //To access creator userId in the workflow use CreationInfo.UserID
        //To access to the event raiser userUd create a variable Sophis.OMS.WFEvents.OrderEventArgs OrderEventArgs.
        //Map it with the Result field of the activity WaitForUserAction before this activity
        [Category("Medio order routing")]
        [Description(@"In Event Name")]
        public InArgument<string> In_EventName
        {
            get;
            set;
        }

        [Category("Medio order routing")]
        [Description(@"In Status Name")]
        public InArgument<string> In_StatusName
        {
            get;
            set;
        }

                 
            
        static string _req = "SELECT H.REFCON FROM HISTOMVTS H, BO_KERNEL_STATUS S WHERE H.SOPHIS_ORDER_ID= :orderId"
        + " AND H.BACKOFFICE=S.ID AND S.NAME = :boStatus ";

        static internal int GetWfEvent(string eventName)
        {
            int wfId = CSMKernelEvent.GetIdByName(eventName);
            if( wfId > 0)
            {
                return wfId;
            }
            else
            {
                throw new ConfigurationErrorsException("Cannot find KernelEvent with name = " + eventName);
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            _logger.LogInformation("ProcessTrade::Execute Start Activity");
            try
            {
                int orderId = context.GetOrder().ID;

                SynchronizationContext apiContext = sophis.SophisWcfConfig.SynchronizationContext;
                apiContext.Send(delegate(object state)
                {
                    SelectAndProcessTrades(orderId, In_StatusName.Get(context), In_EventName.Get(context));
                }
                , null);
            }
            catch (Exception e)
            {
                _logger.LogDebug("IorderAdapter error 2 : " + e);
            }
            _logger.LogInformation("ProcessTrade::Execute End Activity");
                
            
        }

        public void SelectAndProcessTrades(int orderId, string statusName, string eventName)
        {
            try
            {
                _logger.LogDebug("ProcessTrade::SelectAndProcessTrades Before Oracle Command");
                    
                List<object> refCons;
                try
                {
                    var parameter1 = new OracleParameter("orderId", orderId);
                    var parameter2 = new OracleParameter("boStatus", statusName);
                    refCons = MEDIO.CORE.Tools.CSxDBHelper.GetMultiRecords(_req, new List<OracleParameter>() { parameter1, parameter2 });
                    _logger.LogDebug("ProcessTrade::SelectAndProcessTrades After Oracle Command");
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("ProcessTrade::SelectAndProcessTrades error during SQL request : " + ex);
                    throw;
                }

                if( refCons == null || refCons.Count == 0)
                {
                    _logger.LogDebug("ProcessTrade::SelectAndProcessTrades. No trade.");
                    return;
                }

                _logger.LogDebug("ProcessTrade::SelectAndProcessTrades " +refCons.Count+" trades extracted.");
                try
                {
                    CSMEventVector eventVector = new CSMEventVector();
                    eventVector.ActivateNativeLifeCycle();
                    CSMTransaction.StartMultiInsertion();
                    int eventId = GetWfEvent(eventName);
                    foreach (object oneRefconObj in refCons)
                    {
                        long oneRefcon = Convert.ToInt64(oneRefconObj);
                        _logger.LogDebug("ProcessTrade::SelectAndProcessTrades Before processing trade " + oneRefcon );
                        CSMTransaction trade = CSMTransaction.newCSRTransaction(oneRefcon);
                        trade.DoAction(eventId, eventVector, true);
                    }
                    CSMTransaction.EndMultiInsertionOK(eventVector);
                }
                catch(Exception ex)
                {
                    CSMTransaction.EndMultiInsertionBad();
                    _logger.LogDebug("ProcessTrade::SelectAndProcessTrades error during trade processing : " + ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("ProcessTrade::SelectAndProcessTrades error 1 : " + ex);
            }
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Oracle.DataAccess.Client;
using sophis.oms;
using sophis.orderadapter;
using QuickFix;
using sophis.instrument;
using sophis.log;
using sophis.utils;
using Sophis.DataAccess;
using QuickFix.Fields;


namespace Medio.FIXPlugin.net
{
    public class CSxTag7055Service : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTag7055Plugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    public class CSxTag7055Plugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        // TODO Get as param...
        public string FX_SPOT = "SPOT";
        public string FX_FORWARD = "HEDGE";


        public void cancel(Message message, IOrder order)
        {
            processTag7055(message, order);
        }

        public void create(Message message, IOrder order)//, SystemDescriptor systemDesc)
        {
            processTag7055(message, order);
        }

        public void replace(Message message, int oldIdent, IOrder order)
        {
            processTag7055(message, order);
        }

        private void processTag7055(Message message, IOrder order)
        {
            try
            {
                sophis.misc.CSMConfigurationFile.getEntryValue("MEDIO_FIX_7055", "FX_SPOT", ref FX_SPOT, "SPOT");
                sophis.misc.CSMConfigurationFile.getEntryValue("MEDIO_FIX_7055", "FX_FORWARD", ref FX_FORWARD, "HEDGE");

                using (Logger log = new Logger(this, this.GetType().Name))
                {
                    log.log(Severity.debug, "Start checking tag 7055 for order #" + order.ID + "...");
                    if (isFXSpot(order))
                    {
                        message.SetField(new StringField(7055, FX_SPOT));
                        log.log(Severity.debug, "Tag 7055 is set to " + FX_SPOT);

                    }
                    else if (isFXForward(order))
                    {
                        message.SetField(new StringField(7055, FX_FORWARD));
                        log.log(Severity.debug, "Tag 7055 is set to " + FX_FORWARD);
                    }
                    log.log(Severity.debug, "End of processing tag 7055");
                    log.end();
                }
            }
            catch (Exception ex)
            {
                sophis.utils.CSMLog.Write("CSxTag7055Plugin", "ProcessTag7055", CSMLog.eMVerbosity.M_error, "Exception While Processing Tag 7055 : " + ex.Message);
            }

           
        }

        private bool isFXSpot(IOrder order)
        {
            Logger log = new Logger(this, MethodBase.GetCurrentMethod().Name);
            try
            {
                // Extract order target
                WellKnownTarget orderTarget = null;
                if (order is SingleOrder)
                {
                    SingleOrder sOrder = (SingleOrder)order;
                    orderTarget = sOrder.Target;
                }
                if (orderTarget == null) return false;

                // Extract underlying target
                if (orderTarget.SecurityType == ESecurityType.Forex)
                {
                    log.log(Severity.debug, "Order #" + order.ID + " target is a Forex");
                    return true;
                }
            }
            catch (Exception e)
            {
                log.log(Severity.error, "Failed to extract underlying information = " + e.Message);
            }

            log.log(Severity.debug, "Order #" + order.ID + " is not a forex");
            log.end();
            return false;
        }

        private bool isFXForward(IOrder order)
        {
            Logger log = new Logger(this, MethodBase.GetCurrentMethod().Name);
            try
            {
                // Extract order target
                WellKnownTarget orderTarget = null;
                if (order is SingleOrder)
                {
                    SingleOrder sOrder = (SingleOrder)order;
                    orderTarget = sOrder.Target;
                }
                if (orderTarget == null) return false;

                if (orderTarget.SecurityType == ESecurityType.ForexForward || orderTarget.SecurityType==ESecurityType.ForexNDF)
                {
                    log.log(Severity.debug, "Order #" + order.ID + " target is a Forex Forward");
                    return true;
                }
            }
            catch (Exception e)
            {
                log.log(Severity.error, "Failed to extract underlying information = " + e.Message);
            }

            log.log(Severity.debug, "Order #" + order.ID + " is not a forex forward");
            log.end();
            return false;
        }
    }
}

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
    public class CSxTag310Service : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTag310Plugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    public class CSxTag310Plugin : sophis.orderadapter.fix.IFixOrderPluginOutput// IFixOrderPlugin
    {
        public void cancel(Message message, IOrder order)
        {
            processTag310(message, order);
        }

        public void create(Message message, IOrder order) //, SystemDescriptor systemDesc)
        {
            processTag310(message, order);
        }

        public void replace(Message message, int oldIdent, IOrder order)
        {
            processTag310(message, order);
        }

        private void processTag310(Message message, IOrder order)
        {
            using (Logger log = new Logger(this, this.GetType().Name))
            {
                log.log(Severity.debug, "Start checking tag 310 for order #" + order.ID + "...");
                if(isFutureOption(order))
                {
                    message.SetField(new StringField(310, "FUT"));
                    log.log(Severity.debug,"Tag 310 is set to FUT");
                }
                log.log(Severity.debug, "End of processing tag 310");
                log.end();
            }
        }

        private bool isFutureOption(IOrder order)
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
                if (orderTarget is OptionTarget)
                {
                    log.log(Severity.debug, "Order #" + order.ID + " target is an option");
                    OptionTarget optionTarget = (OptionTarget)orderTarget;

                    if (optionTarget.Underlying is FutureTarget)
                    {
                        log.log(Severity.debug, "Order #" + order.ID + " target is an future option");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                log.log(Severity.error, "Failed to extract underlying information = " + e.Message);
            }

            log.log(Severity.debug, "Order #" + order.ID + " is not a future option");
            log.end();
            return false;
        }

    }
}

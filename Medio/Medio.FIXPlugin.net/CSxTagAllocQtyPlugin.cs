using QuickFix.Fields;
using sophis.instrument;
using sophis.log;
using sophis.oms;
using System.Linq;
using sophis.portfolio;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using MEDIO.CORE.Tools;
using System;
using System.Collections.Generic;

namespace Medio.FIXPlugin.net
{

    public class CSxTagAllocQtyService : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTagAllocQtyPlugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class CSxTagAllocQtyPlugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public void process(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process CSxTagAllocQtyPlugin"))
            {
                log.log(Severity.debug, " CSxTagAllocQtyPlugin - Process");

                if (order is SingleOrder)
                {
                    SingleOrder sOrder = (SingleOrder)order;
                    string tagValue = "";
                   
                    foreach (var alloc in sOrder.AllocationRulesSet.Allocations)
                    {
                        double qty = alloc.Quantity;
                        log.log(Severity.debug, " Alloc quantity is " + qty);

                        tagValue += qty.ToString();
                        tagValue += ',';
                    }

                    tagValue= tagValue.TrimEnd(',');
                    log.log(Severity.debug, "List of allocation qty values: " + tagValue);

                    message.SetField(new StringField(10003, tagValue));
                    log.log(Severity.debug, "Message after setting the order creator ref on tag 10003: " + message.ToString());


                }

                log.end();
            }
        }

        public void create(QuickFix.Message message, IOrder order)
        {
            process(message, order);
        }

        public void replace(QuickFix.Message message, int oldIdent, IOrder order)
        {
            process(message, order);
        }

        public void cancel(QuickFix.Message message, IOrder order)
        {
            process(message, order);
        }
    }

}

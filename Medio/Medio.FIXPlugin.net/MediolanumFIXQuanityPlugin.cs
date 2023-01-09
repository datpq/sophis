using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.oms;
using sophis.misc;
using sophis.utils;
using sophis.log;
using QuickFix;
using System.Globalization;
using QuickFix.Fields;

namespace Medio.FIXPlugin.net
{
    public class Tag80Service : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new Tag80Plugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class Tag80Plugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public void process(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process FIXQuantityPlugin"))
            {
                log.log(Severity.debug, "Mediolanum FIXQuantityPlugin - Process");
                //    log.log(Severity.debug, message.ToString());

                if (message.IsSetField(Tags.NoAllocs))
                {
                    //log.log(Severity.debug, "Number of allocations field set");
                    int allocsCount = message.GetInt(Tags.NoAllocs);
                    log.log(Severity.debug, "Alloc count is " + allocsCount);
                    for (int i = 0; i < allocsCount; i++)
                    {
                        QuickFix.Group group = new QuickFix.Group(Tags.NoAllocs, Tags.AllocAccount);
                        message.GetGroup((i + 1), group);

                        if (group.IsSetField(Tags.AllocQty) == true)
                        {
                            log.log(Severity.debug, "Retrieving Allocated Qty field ");
                            decimal qty = Convert.ToDecimal(group.GetField(Tags.AllocQty), CultureInfo.InvariantCulture);
                            log.log(Severity.debug, "Alloc Qty is " + qty);

                            if (qty < 0)
                            {
                                qty = Math.Abs(qty);
                                group.SetField(new AllocQty(qty));
                                double qty2 = Convert.ToDouble(group.GetField(Tags.AllocQty), CultureInfo.InvariantCulture);
                                //log.log(Severity.debug, "Alloc Qty after tkt " + qty2);
                                message.ReplaceGroup((i + 1), group.Field ,group);
                                log.log(Severity.debug, "Message after setting allocated quantity to absolute value: " + message.ToString());
                            }
                        }
                    }
                }

                log.end();
            }
        }

        public void create(QuickFix.Message message, IOrder order)//, sophis.orderadapter.SystemDescriptor systemDesc)
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

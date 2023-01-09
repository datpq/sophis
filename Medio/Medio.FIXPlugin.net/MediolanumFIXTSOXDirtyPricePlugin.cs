using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix;
using QuickFix.Fields;
using sophis.oms;
using sophis.orderadapter;
using sophis.log;

namespace Medio.FIXPlugin.net
{
    //class MediolanumFIXTSOXDirtyPricePlugin : sophis.orderadapter.fix.IFixOrderPlugin
    //{
    //    public void cancel(Message message, IOrder order)
    //    {
    //    }

    //    public void create(Message message, IOrder order, SystemDescriptor systemDesc)
    //    {
    //        processDirtyPrice(message, order);
    //    }

    //    public void replace(Message message, int oldIdent, IOrder order)
    //    {
    //    }

    //    public void processDirtyPrice(Message message, IOrder order)
    //    {
    //        using (Logger log = new Logger(this, this.GetType().Name))
    //        {
    //            log.log(Severity.debug, "Fix message before modifications: " + message.ToString());
    //            try
    //            {
    //                if (message.IsSetField(22251))
    //                {
    //                    int priceQualifier = message.GetInt(22251);
    //                    log.log(Severity.debug, "Field 22251 found, value = " + priceQualifier);
    //                    if (priceQualifier == 1 && message.IsSetField(Tags.AccruedInterestAmt))
    //                    {
    //                        log.log(Severity.debug, "Set tag AccruedInterestAmt (159) to 0 ");
    //                        message.SetField(new AccruedInterestAmt(decimal.Zero), true);
    //                    }
    //                    else
    //                    {
    //                        log.log(Severity.debug, "Tag 159 not found");
    //                    }
    //                }
    //                else
    //                {
    //                    log.log(Severity.debug, "Field 22251 not found");
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                log.log(Severity.error, "Exception: " + e.Message);
    //            }

    //            log.log(Severity.debug, "Fix message after modifications: " + message.ToString());
    //        }
    //    }

    //}
}

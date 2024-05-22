
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

    public class CSxTagStratNameService : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTagStratNamePlugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class CSxTagStratNamePlugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public void process(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process CSxTagStratNamePlugin"))
            {
                log.log(Severity.debug, " CSxTagStratNamePlugin - Process");

                if (order is SingleOrder)
                {
                    SingleOrder sOrder = (SingleOrder)order;
                    string tagValue = "";

                    if (DBContext.Connection == null)
                        CSxDBHelper.InitDBConnection();
                    foreach (var alloc in sOrder.AllocationRulesSet.Allocations)
                    {
                        int folId = alloc.PortfolioID;
                        log.log(Severity.debug, " allocation folio is " + folId);


                        string sql = "select name from am_strategy where id in (select strategy from pfr_model_link where folio=:Folio)";

                        OracleParameter parameterList = new OracleParameter(":Folio", folId);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameterList };
                        string stratName = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                        if (stratName != "")
                        {
                            tagValue += stratName;
                            tagValue += ',';
                        }

                    }

                    tagValue = tagValue.TrimEnd(',');
                    message.SetField(new StringField(10002, tagValue));
                    log.log(Severity.debug, "Message after setting the order creator ref on tag 10002: " + message.ToString());


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

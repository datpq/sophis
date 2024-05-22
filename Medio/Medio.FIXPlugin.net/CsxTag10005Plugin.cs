
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
using sophis.utils;

namespace Medio.FIXPlugin.net
{

    public class CSxTag10005Service : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTag10005Plugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class CSxTag10005Plugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public void process(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process CSxTag10005Plugin"))
            {
                log.log(Severity.debug, " CSxTag10005Plugin - Process");

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
                        CMString fullName = "";
                        string sql = "select sys_connect_by_path (name,':') Path from folio where ident = :folioId start with name in (select name from folio where mgr = 12690) connect by nocycle prior ident = mgr";
                        OracleParameter parameterList = new OracleParameter(":folioId", folId);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameterList };
                        string fullPath = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                        log.log(Severity.debug, "tag 10005: FUll path SQL Result" + fullPath);
                        if (fullPath != "")
                        {
                            string ValuetoGet = (fullPath.Substring(1)).Replace("Investments:", "");
                            log.log(Severity.debug, "tag 10005: tag fUll path: " + ValuetoGet);
                            
                            tagValue += ValuetoGet;
                            tagValue += ',';
                        }
                    }

                    tagValue = tagValue.TrimEnd(',');
                    message.SetField(new StringField(10005, tagValue));
                    log.log(Severity.debug, "Message after setting the order creator ref on tag 10005: " + message.ToString());


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

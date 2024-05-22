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

    public class CSxTagFundNameService : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTagFundNamePlugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class CSxTagFundNamePlugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public void process(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process CSxTagFundNamePlugin"))
            {
                log.log(Severity.debug, " CSxTagFundNamePlugin - Process");

                if (order is SingleOrder)
                {
                    SingleOrder sOrder = (SingleOrder)order;
                    string tagValue = "";
                   
                    if (DBContext.Connection == null)
                        CSxDBHelper.InitDBConnection();
                    foreach (var alloc in sOrder.AllocationRulesSet.Allocations)
                    {
                       int entId= alloc.EntityID;
                       log.log(Severity.debug, " allocation entity is "+ entId);

                        
                        string sql = "select name from funds where entity = :allocEntity";

                        OracleParameter parameterList = new OracleParameter(":allocEntity", entId);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameterList };
                        string fundName = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                        if (fundName != "")
                        {
                            tagValue += fundName;
                            tagValue += ',';
                        }

                      }
                  //  entitiesList=entitiesList.TrimEnd(',');
                   
                   // log.log(Severity.debug, "List of allocation entity values: " + entitiesList);

                   /* string sql = "select name from funds where entity in (:allocEntityList)";

                    OracleParameter parameterList = new OracleParameter(":allocEntityList", entitiesList);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameterList };
                    if (DBContext.Connection == null)
                        CSxDBHelper.InitDBConnection();
                    var orderNames =CSxDBHelper.GetMultiRecords(sql, parameters);
                    */
                   
                    tagValue=tagValue.TrimEnd(',');
                    message.SetField(new StringField(10001, tagValue));
                    log.log(Severity.debug, "Message after setting the order creator ref on tag 10001: " + message.ToString());


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

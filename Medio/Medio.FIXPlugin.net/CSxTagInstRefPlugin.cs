using MEDIO.CORE.Tools;
using Oracle.DataAccess.Client;
using QuickFix.Fields;
using sophis.instrument;
using sophis.log;
using sophis.oms;
using Sophis.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medio.FIXPlugin.net
{
  
    public class CSxTagInstRefService : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new TagInstRefPlugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class TagInstRefPlugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public void process(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process TagInstRefPlugin"))
            {
                log.log(Severity.debug, " TagInstRefPlugin - Process");


                log.log(Severity.debug, "Checking for target owners...");
                IOrderTargetOwner owner = order.GetOrderTargetOwners().FirstOrDefault<IOrderTargetOwner>();
                log.log(Severity.debug, "Found owner Id  " + owner.ID);
           
                log.log(Severity.debug, "checking instrument  " + owner.Target.SecurityID);

                int instrIdent = owner.Target.SecurityID;

                string sql = "select reference from titres where sicovam = :sicovam";

                OracleParameter parameterSicovam = new OracleParameter(":sicovam", instrIdent);
                List<OracleParameter> parameters = new List<OracleParameter>() { parameterSicovam };
                if (DBContext.Connection == null)
                    CSxDBHelper.InitDBConnection();
                string reference = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                if (reference != "")
                {
                    log.log(Severity.debug, " DB ticker ref : " + reference);

                        message.SetField(new StringField(Tags.Symbol, reference));
                        log.log(Severity.debug, "Message after setting the ticker ref on tag 55: " + message.ToString());
                    
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

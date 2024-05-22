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

    public class CSxTagISINService : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTagISINPlugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class CSxTagISINPlugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public void process(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process CSxTagISINPlugin"))
            {
                log.log(Severity.debug, " CSxTagISINPlugin - Process");


                log.log(Severity.debug, "Checking for target owners...");
                IOrderTargetOwner owner = order.GetOrderTargetOwners().FirstOrDefault<IOrderTargetOwner>();
                log.log(Severity.debug, "Found owner Id  " + owner.ID);

                log.log(Severity.debug, "checking instrument  " + owner.Target.SecurityID);

                int instrIdent = owner.Target.SecurityID;

                string sql = "select ei.value from EXTRNL_REFERENCES_INSTRUMENTS ei "
                    + " left join EXTRNL_REFERENCES_DEFINITION ed on ei.ref_ident = ed.ref_ident "
                    + " left join TITRES t on t.sicovam = ei.sophis_ident where t.sicovam = :sicovam and ed.ref_name = :refName ";

                string refName = "ISIN";
                OracleParameter parameterRef = new OracleParameter(":refName", refName);
                OracleParameter parameterSicovam = new OracleParameter(":sicovam", instrIdent);
                List<OracleParameter> parameters = new List<OracleParameter>() { parameterRef, parameterSicovam };
                if (DBContext.Connection == null)
                    CSxDBHelper.InitDBConnection();
                string reference = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                if (reference != "")
                {
                    log.log(Severity.debug, "ISIN value found : " + reference);
                    message.SetField(new StringField(10048, reference));
                    log.log(Severity.debug, "Message after setting the ISIN ref on tag 10048: " + message.ToString());
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

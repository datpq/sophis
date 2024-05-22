
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

    public class CSxTagOrderCreatorService : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTagOrderCreatorPlugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class CSxTagOrderCreatorPlugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public void process(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process CSxTagOrderCreatorPlugin"))
            {
                log.log(Severity.debug, " CSxTagOrderCreatorPlugin - Process");


                log.log(Severity.debug, "Checking for target owners...");
                IOrderTargetOwner owner = order.GetOrderTargetOwners().FirstOrDefault<IOrderTargetOwner>();
                log.log(Severity.debug, "Found owner Id  " + owner.ID);
                int userID= order.CreationInfo.UserID;
                log.log(Severity.debug, "User Id is  " + userID);
                if (userID != 0)
                {
                   string sql = "select name from riskusers where ident = :userID";

                    OracleParameter parameterSicovam = new OracleParameter(":userID", userID);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameterSicovam };
                    if (DBContext.Connection == null)
                        CSxDBHelper.InitDBConnection();
                    string orderCreator = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                    if (orderCreator != "")
                    {
                        message.SetField(new StringField(10000, orderCreator));
                        log.log(Severity.debug, "Message after setting the order creator ref on tag 10000: " + message.ToString());
                    }

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

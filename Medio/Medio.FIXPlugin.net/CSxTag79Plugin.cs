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
using MEDIO.CORE.Tools;

namespace Medio.FIXPlugin.net
{
    public class CSxTag79Service : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTag79Plugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }

    public class CSxTag79Plugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
       // private bool isCacheLoaded = false;

        private Dictionary<int, string> folioAccount = new Dictionary<int, string>();

        public string GetAccountfromFolioId(int folioId)
        {
 /*           GetL2Folios << "SELECT " << CSROut("TMR.OBJECT_ID", folioId) << "," << CSROut("TMRBD.S_VALUE", folioTag, 256) << " FROM TAGMETADATA_RESULT TMR, TAGMETADATA_CATEGORY TMC, TAGMETADATA_RES_VAL_BY_DATE TMRBD WHERE "
    << " TMR.RESULT_ID = TMRBD.RESULT_ID AND TMR.ELEMENT_ID = TMC.ELEMENT_ID AND (UPPER(TMC.ELEMENT_NAME)='LEVEL 2' OR UPPER(TMC.ELEMENT_NAME)='LEVEL2')";
*/
            string retval="";

            if (DBContext.Connection == null)
                CSxDBHelper.InitDBConnection();
            using (Logger log = new Logger(this, this.GetType().Name))
            {
                try
                {

                    folioAccount.Clear();

                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TMR.OBJECT_ID,TMRBD.S_VALUE FROM TAGMETADATA_RESULT TMR, TAGMETADATA_CATEGORY TMC, TAGMETADATA_RES_VAL_BY_DATE TMRBD WHERE "
                            + " TMR.RESULT_ID = TMRBD.RESULT_ID AND TMR.ELEMENT_ID = TMC.ELEMENT_ID AND UPPER(TMC.ELEMENT_NAME)='FXALL' AND TMR.OBJECT_ID=" + folioId;

                        log.log(Severity.debug, "Querying DB :" + cmd.CommandText.ToString());

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                log.log(Severity.debug, "Getting values : " + reader[0] + " - " + reader[1]);
                                if (string.IsNullOrEmpty(reader[1].ToString()) == false)
                                {

                                    retval = reader[1].ToString();
                                }

                            }
                        }

                    }
                }

                catch (Exception ex)
                {
                    log.log(Severity.error, "Exception Occured: " + ex.Message);
                }
            }
            return retval;;
        }
        public void cancel(Message message, IOrder order)
        {
            processTag79(message, order);
        }

        public void create(Message message, IOrder order)//, SystemDescriptor systemDesc)
        {
            processTag79(message, order);
        }

        public void replace(Message message, int oldIdent, IOrder order)
        {
            processTag79(message, order);
        }

        private void processTag79(Message message, IOrder order)
        {

            using (Logger log = new Logger(this, this.GetType().Name))
            {
                log.log(Severity.debug, "Start checking tag 79 for order #" + order.ID + "...");
                // if hasvalueInCache(order)
                log.log(Severity.debug, "Checking for target owners...");
                IOrderTargetOwner owner = order.GetOrderTargetOwners().FirstOrDefault<IOrderTargetOwner>();
                log.log(Severity.debug, "Found owner Id  " + owner.ID);
                log.log(Severity.debug, "Checking alloc number of rule(s)" + owner.AllocationRulesSet.Allocations.Count);
                int i = 0;
                foreach (AllocationRule allocRule in owner.AllocationRulesSet.Allocations)
               // for(int i=1;i<owner.AllocationRulesSet.Allocations.Count;i++)
                {
                    ++i; // make sur the group is incrmented no matter what the account is (instead of getting the incrementation done on getgoup(++i,group) in the scope below.
                    log.log(Severity.debug, "Checking group : " + i);
                    log.log(Severity.debug, "Got alloc folio Id: " + allocRule.PortfolioID);
                    log.log(Severity.debug, "Got Message: "+message.ToString());
                    string Account = GetAccountfromFolioId(allocRule.PortfolioID);
                    if (string.IsNullOrEmpty(Account) == false)
                    {
                        
                        Group group = new Group(Tags.NoAllocs, Tags.AllocAccount);
                        message.GetGroup(i, group);
                        if (group.IsSetField(Tags.AllocAccount))

                        {
                            log.log(Severity.debug, "Original Tag =" + group.GetField(Tags.AllocAccount).ToString());
                            log.log(Severity.debug, "Replacing with Field: " + Account);
                            
                            StringField allocAcc = new StringField(Tags.AllocAccount, Account);
                            log.log(Severity.debug, "Removing Group to message ");
                            group.RemoveField(Tags.AllocAccount);
                            group.SetField(allocAcc);
                            log.log(Severity.debug, "new group : " + group.ToString());
                            log.log(Severity.debug, "Adding Group to message :" + group.ToString());
                            message.ReplaceGroup(i,Tags.NoAllocs, group);
                            log.log(Severity.debug, "Got New Message: " + message.ToString());
                        }
                        else
                        {
                            log.log(Severity.debug, "No Field 79...setting to : " + Account);
                            StringField allocAcc = new StringField(Tags.AllocAccount, Account);
                            group.SetField(allocAcc);
                            message.AddGroup(group);
                        }
                    }
                    else
                    {
                        log.log(Severity.debug, "No Tag Found, no changes made.");
                    }
                }

                log.log(Severity.debug, "End of processing tag 79");
                log.end();
            }
        }

       

    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using QuickFix;
using sophis.log;
using sophis.oms;
using sophis.orderadapter;
using Sophis.DataAccess;
using QuickFix.Fields;

namespace Medio.FIXPlugin.net
{
    public class CSxTradingAccountService : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new CSxTradingAccountPlugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }

    class CSxTradingAccountPlugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        private static Dictionary<int, string> TradingAccDic = new Dictionary<int, string>();
        private readonly string _DefaultTraingAccounnt = "GENERIC ACCOUNT";

        public void cancel(Message message, IOrder order)
        {
        }

        public void create(Message message, IOrder order)//, SystemDescriptor systemDesc)
        {
            processTradingAccountTags(message, order);
        }

        public void replace(Message message, int oldIdent, IOrder order)
        {
        }

        public void processTradingAccountTags(Message message, IOrder order)
        {
            using (Logger log = new Logger(this, this.GetType().Name))
            {
                try
                {
                    if (order is SingleOrder)
                    {
                        SingleOrder sOrder = (SingleOrder)order;
                        int i = 0;
                        foreach (var alloc in sOrder.AllocationRulesSet.Allocations)
                        {
                            Group group = new Group(Tags.NoAllocs, Tags.AllocAccount);
                            message.GetGroup(++i, group);
                            var tradingAcc = getTradingAccount(alloc.EntityID);
                            StringField allocAcc = new StringField(Tags.AllocAccount, tradingAcc);
                            if (group.IsSetField(Tags.AllocAccount))
                            {
                                group.SetField(allocAcc);
                            }
                            else
                            {
                                log.log(Severity.debug,  Tags.AllocAccount + " is not set");
                            }
                            StringField individualAcc = new StringField(Tags.IndividualAllocID, sOrder.LastPlacementId + "-" + tradingAcc);
                            if (group.IsSetField(Tags.IndividualAllocID))
                            {
                                group.SetField(individualAcc);
                            }
                            else
                            {
                                log.log(Severity.debug, Tags.IndividualAllocID + " is not set");
                            }
                            message.ReplaceGroup(i, group.Field, group);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.log(Severity.error, "Exception = " + e.Message );
                }
                log.log(Severity.debug, "Fix message after modifications: " + message.ToString());
            }
        }

        private string getTradingAccount(int entity)
        {
            using (Logger logger = new Logger(this, this.GetType().Name))
            {
                logger.log(Severity.debug, "getTradingAccount start");
                string res = "";
                try
                {
                    if (TradingAccDic.ContainsKey(entity))
                    {
                        res = TradingAccDic[entity];
                        logger.log(Severity.debug, String.Format("Found trading acc for entity #{0} in the cache! Trading acc = {1}", entity, res));
                        return res;
                    }

                    string sql = "select trading_account from order_defparam_selector where entity = :entity";
                    OracleParameter parameter = new OracleParameter(":entity", entity);
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Clear();
                        cmd.CommandText = sql;
                        cmd.Parameters.Add(parameter);
                        res = cmd.ExecuteScalar() == DBNull.Value ? _DefaultTraingAccounnt : cmd.ExecuteScalar().ToString();
                        res = String.IsNullOrEmpty(res) ? _DefaultTraingAccounnt : res;
                        if (!TradingAccDic.ContainsKey(entity)) TradingAccDic.Add(entity, res);
                    }
                    logger.log(Severity.debug, String.Format("trading account retrived for entity {0} = {1}", entity, res));
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error getting trading account : " + ex.Message);
                }
                return res;
            }
        }

    }
}

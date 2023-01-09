using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.log;
using sophis.oms;
using QuickFix;
using System.Globalization;
using sophis.value;
using sophis.backoffice_kernel;
using sophis.utils;

using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using QuickFix.Fields;

namespace Medio.FIXPlugin.net
{
    public class Tag670Service : sophis.orderadapter.fix.IFixOrderPluginService
    {
        public sophis.orderadapter.fix.IFixOrderPluginOutput GetOutput()
        {
            return new Tag670Plugin();
        }

        public sophis.orderadapter.fix.IFixOrderPluginInput GetInput()
        {
            return new sophis.orderadapter.fix.NullFixOrderPluginInput();
        }
    }
    class Tag670Plugin : sophis.orderadapter.fix.IFixOrderPluginOutput
    {
        public static bool ApiInit = false;

        public Tag670Plugin()
        {
            CSMLog.Write("Tag670Plugin", "Tag670Plugin",CSMLog.eMVerbosity.M_debug,"Trying to initialize API");
            initApi();
            CSMLog.Write("Tag670Plugin", "Tag670Plugin", CSMLog.eMVerbosity.M_debug, "ApiInit=" + ApiInit);
        }

        public void processTag670(QuickFix.Message message, IOrder order)
        {
            using (Logger log = new Logger(this, "Process FXSwapPlugin"))
            {
                log.log(Severity.debug, "Mediolanum FXSwapPlugin - Process");

                log.log(Severity.debug, string.Format("Order Type: {0}",order.GetType().ToString()));

                if (message == null || order == null)
                {
                    log.log(Severity.error, "Invalid argument provided: message or order cannot be null");
                }

                MultiLegsOrder fxSwapOrder = order as MultiLegsOrder;
                bool isSwap = (fxSwapOrder != null);

                #region Side should be 'B' instead of 1 (buy)

                if (message.IsSetField(Tags.Side) && isSwap)
                {
                    char side = Convert.ToChar(message.GetField(Tags.Side), CultureInfo.InvariantCulture);
                    if (side == '1')
                    {
                        log.log(Severity.debug, string.Format("Setting side = {0} ", side));
                        message.SetField(new Side('B'));
                    }
                }
                else
                {
                    log.log(Severity.debug, string.Format("Tag 54 not set"));
                }

                #endregion

                if (message.IsSetField(Tags.NoLegs) && isSwap) //Only for fx swaps
                {
                    int legCount = message.GetInt(Tags.NoLegs);
                    log.log(Severity.debug, string.Format("Found {0} legs", legCount));

                    for (int j = 0; j < legCount; j++)
                    {

                        QuickFix.Group groupLeg = new QuickFix.Group(Tags.NoLegs, Tags.NoLegAllocs); //might be possible to initialize with null.
                        message.GetGroup((j + 1), groupLeg);

                        if (groupLeg.IsSetField(Tags.NoLegAllocs))
                        {
                            #region Get group 670

                            string legAllocsCountStr = groupLeg.GetField(Tags.NoLegAllocs);
                            int legAllocsCount = 0;
                            Int32.TryParse(legAllocsCountStr, out legAllocsCount);
                            //.getInt(NoLegAllocs.FIELD);
                            //uint groupId = (uint)NoLegAllocs.FIELD;
                            //message.getGroup(groupId, group);

                            log.log(Severity.debug, string.Format("Found {0} leg allocs", legAllocsCount));

                            #endregion

                            for (int i = 0; i < legAllocsCount; i++)
                            {
                                QuickFix.Group group = new QuickFix.Group(Tags.NoLegAllocs, Tags.LegAllocAccount); //might be possible to initialize with null.
                                groupLeg.GetGroup((i + 1), group);

                                log.log(Severity.debug, string.Format("Trying to set account"));

                                #region Set account

                                IOrderTargetOwner owner = order.GetOrderTargetOwners().FirstOrDefault<IOrderTargetOwner>();
                                if ((owner == null) || (owner.Target == null))
                                {
                                    log.log(Severity.debug, string.Format("[LegAllocID={0}] Failed to retrieve order {1} owner or target.", i, order.ID));
                                    return;
                                }

                                string account = "";
                                log.log(Severity.debug, string.Format("owner.AllocationRulesSet.Allocations.Count = {0}", owner.AllocationRulesSet.Allocations.Count));
                                if (owner.AllocationRulesSet.Allocations.Count > 0)
                                {
                                    string account_name = "";
                                    int k = 0;
                                    foreach (AllocationRule allocRule in owner.AllocationRulesSet.Allocations)
                                    {
                                        log.log(Severity.debug, string.Format("AllocationRule {0} Trading account = {1} entity id = {2} alloc qty = {3} folio id = {4} prime broker = {5} qty = {6}", k, allocRule.TradingAccount, allocRule.EntityID, allocRule.AllocatedQuantity, allocRule.PortfolioID, allocRule.PrimeBrokerID, allocRule.Quantity));
                                        /*
                                        int fund_code = 0;
                                        int entity_code = 0;
                                        GetEntityIDFromFundOfFolio(allocRule.PortfolioID, out fund_code, out entity_code);
                                        CSMAmAccount AMAccount = new CSMAmAccount(fund_code, allocRule.PrimeBrokerID);
                                        if (AMAccount == null)
                                        {
                                            log.log(Severity.debug, "AMAccount is null");
                                        }
                                        account_name = AMAccount.GetAccountName();
                                        log.log(Severity.debug, string.Format("Account_name{0} = {1}",k,  account_name));
                                        CMString account_at_agent = AMAccount.GetAccountAtAgent();
                                        log.log(Severity.debug, string.Format("account_at_agent{0} = {1}", k, account_at_agent.ToString()));
                                        CMString account_at_custodian = AMAccount.GetAccountAtCustodian();
                                        log.log(Severity.debug, string.Format("account_at_custodian{0} = {1}", k, account_at_custodian.ToString()));
                                        */
                                        CSMThirdParty thirdParty = CSMThirdParty.GetCSRThirdParty(allocRule.EntityID);
                                        if (thirdParty != null)
                                        {
                                            account_name = thirdParty.GetName().ToString();
                                            log.log(Severity.debug, string.Format("Entity (ID={0}) name={1}", allocRule.EntityID, account_name));
                                        }
                                        else
                                        {
                                            log.log(Severity.debug, string.Format("thirdParty was null"));
                                        }
                                        k++;
                                    }
                                    account = account_name;//owner.AllocationRulesSet.Allocations.FirstOrDefault().TradingAccount;
                                    log.log(Severity.debug, string.Format("Account is: {0}", account));
                                }
                                else
                                {
                                    log.log(Severity.debug, string.Format("[LegAllocID={0}] Tag 671 already set", i));
                                }
                                log.log(Severity.debug, string.Format("[LegAllocID={0}] owner.AllocationRulesSet.Allocations.Count == 0 ", i));

                                if (group.IsSetField(Tags.LegAllocAccount) == false)
                                {
                                    log.log(Severity.debug, "Trying to set account field");
                                    if (!String.IsNullOrEmpty(account))
                                    {
                                        group.SetField(new StringField(Tags.LegAllocAccount,account));
                                        log.log(Severity.debug, string.Format("Setting account: {0}", account));
                                    }
                                }
                                else
                                {
                                    log.log(Severity.debug, string.Format("[LegAllocID={0}] Tag 671 already set", i));
                                }

                                #endregion

                                #region Absolute value of quantity

                                if (group.IsSetField(Tags.LegAllocQty) == true)
                                {
                                    double quantity = Convert.ToDouble(group.GetField(Tags.LegAllocQty), CultureInfo.InvariantCulture);
                                    log.log(Severity.debug, string.Format("[LegAllocID={0}] Setting quantity = {1} ", i, Math.Abs(quantity)));
                                    group.SetField(new DecimalField(Tags.LegAllocQty,Convert.ToDecimal(Math.Abs(quantity))));
                                }
                                else
                                {
                                    log.log(Severity.debug, string.Format("[LegAllocID={0}] Tag 673 not set", i));
                                }

                                #endregion

                                groupLeg.ReplaceGroup((i + 1), group.Field, group);
                                log.log(Severity.debug, "Replacing groupLeg with group");

                            }
                        }
                        else
                        {
                            log.log(Severity.debug, string.Format("Tag 670 not set"));
                        }

                        log.log(Severity.debug, "Replacing message with groupLeg");
                        message.ReplaceGroup((j + 1), groupLeg.Field, groupLeg);
                    }
                }
                else
                {
                    log.log(Severity.debug, string.Format("Tag 555 not set or order is not FX Swap"));
                }
            }
        }

        public void cancel(QuickFix.Message message, sophis.oms.IOrder order)
        {
            try
            {
                processTag670(message, order);
            }
            catch (Exception ex)
            {
                sophis.utils.CSMLog.Write("Tag670Plugin", "cancel", sophis.utils.CSMLog.eMVerbosity.M_error, "Exception occurred while trying to process Tag670 message: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        public void create(QuickFix.Message message, sophis.oms.IOrder order)//, sophis.orderadapter.SystemDescriptor systemDesc)
        {
            try
            {
                processTag670(message, order);
            }
            catch (Exception ex)
            {
                sophis.utils.CSMLog.Write("Tag670Plugin", "create", sophis.utils.CSMLog.eMVerbosity.M_error, "Exception occurred while trying to process Tag670 message: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        public void replace(QuickFix.Message message, int oldIdent, sophis.oms.IOrder order)
        {
            try
            {
                processTag670(message, order);
            }
            catch (Exception ex)
            {
                sophis.utils.CSMLog.Write("Tag670Plugin", "replace", sophis.utils.CSMLog.eMVerbosity.M_error, "Exception occurred while trying to process Tag670 message: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
        }

        public int GetEntityIDFromFundOfFolio(int folio_id, out int fund_id, out int entity_id)
        {
            fund_id = 0;
            entity_id = 0;
            string squery_str = "SELECT A.ID, F.ENTITY FROM ("
                    + " SELECT ID FROM ("
                    + " SELECT folio_parents.ident id, folio_parents.name name , (folio_parents.L) L FROM ("
                    + " SELECT ident, name, prior ident pi, prior name, level L"
                    + " FROM folio"
                    + " start with ident = " + folio_id
                    + " connect by prior mgr = ident"
                    + " ) folio_parents"
                    + " join FUNDS on folio_parents.ident = funds.tradingfolio"
                    + " ORDER BY L"
                    + " )"
                    + " where rownum=1"
                    + " ) A, FUNDS F WHERE A.ID = F.TRADINGFOLIO";
            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = squery_str;
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fund_id = Convert.ToInt32(reader["ID"]);
                            entity_id = Convert.ToInt32(reader["ENTITY"]);
                            CSMLog.Write("Tag670Plugin", "GetEntityIDFromFundOfFolio", CSMLog.eMVerbosity.M_debug, "Query returned: (FUND FOLIO) ID=" + fund_id + " ENTITY=" + entity_id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("Tag670Plugin", "GetEntityIDFromFundOfFolio", CSMLog.eMVerbosity.M_error, "Exception occured while trying to get fund entity id: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return entity_id;
        }

        public static bool initApi()
        {
            if (!ApiInit)
            {
                CSMApi api = null;
                try
                {
                    api = new CSMApi();
                }
                catch (Exception ex)
                {
                    CSMLog.Write("Tag670Plugin", "initApi", CSMLog.eMVerbosity.M_error, "Failed to create a new CSMApi object: " + ex.Message);
                    ApiInit = false;
                    return false;
                }
                if (api != null)
                {
                    try
                    {
                        api.Initialise();
                    }
                    catch (Exception ex)
                    {
                        CSMLog.Write("Tag670Plugin", "initApi", CSMLog.eMVerbosity.M_error, "Failed to initialize API: " + ex.Message);
                        ApiInit = false;
                        return false;
                    }
                }
                ApiInit = true;
                return true;
            }
            else
            {
                return true;
            }
        }

        /*
        public CSMAmFund GetFundFromFolioOrParents(int folio_id)
        {
            const int maxIter = 10;
            CSMAmFund retval = null;
            sophis.utils.CSMLog.Write("Tag670Plugin", "GetFundFromFolioOrParents", sophis.utils.CSMLog.eMVerbosity.M_debug, "Trying to get fund by folio ID = " + folio_id);
            CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folio_id);
            int iter = 0;
            do{
                iter++;
                if (folio != null)
                {
                    sophis.utils.CSMLog.Write("Tag670Plugin", "GetFundFromFolioOrParents", sophis.utils.CSMLog.eMVerbosity.M_debug, "Folio is not null: FolioID = " + folio.GetCode() + " and ParentID = " + folio.GetParentCode());
                    retval = CSMAmFund.GetFundFromFolio(folio);
                    if (retval != null)
                    {
                        sophis.utils.CSMLog.Write("Tag670Plugin", "GetFundFromFolioOrParents", sophis.utils.CSMLog.eMVerbosity.M_debug, "Found fund");
                        break;
                    }
                    if (folio.GetCode() == 1 || folio.GetParentCode() == 1)
                    {
                        break;
                    }
                    folio = CSMAmPortfolio.GetCSRPortfolio(folio.GetParentCode());
                }
                else
                {
                    sophis.utils.CSMLog.Write("Tag670Plugin", "GetFundFromFolioOrParents", sophis.utils.CSMLog.eMVerbosity.M_debug, "Folio was null");
                    break;
                }
            } while (retval == null && folio != null && iter < maxIter);
            return retval;
        }
         */ 

    }
}

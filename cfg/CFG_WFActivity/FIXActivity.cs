using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using Sophis.Logging;
using sophis.oms;
using sophis.instrument;
using System.Collections.Generic;
using sophis.value;
using sophis.backoffice_kernel;
using sophis.utils;
using sophis.static_data;
using sophis.portfolio;
using sophis;
using Sophis.OMS;


namespace CFG_WFActivity
{
    public partial class FIXActivity : Activity//SequenceActivity
    {
        public FIXActivity()
        {
            InitializeComponent();
        }

        #region Properties
        public static DependencyProperty OrderIDProperty = DependencyProperty.Register("OrderID", typeof(Int32), typeof(FIXActivity));
        [Description("Identifies the order")]
        [Category("Order")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int OrderID
        {
            get
            {
                return ((Int32)(base.GetValue(FIXActivity.OrderIDProperty)));
            }
            set
            {
                base.SetValue(FIXActivity.OrderIDProperty, value);
            }
        }

        public static DependencyProperty CashAccountProperty = DependencyProperty.Register("CashAccount", typeof(String), typeof(FIXActivity));
        [Description("Cash Account property name")]
        [Category("Order")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public String CashAccount
        {
            get
            {
                return ((String)(base.GetValue(FIXActivity.CashAccountProperty)));
            }
            set
            {
                base.SetValue(FIXActivity.CashAccountProperty, value);
            }
        }

        public static DependencyProperty FundCodeProperty = DependencyProperty.Register("FundCode", typeof(String), typeof(FIXActivity));
        [Description("Fund Code property name")]
        [Category("Order")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public String FundCode
        {
            get
            {
                return ((String)(base.GetValue(FIXActivity.FundCodeProperty)));
            }
            set
            {
                base.SetValue(FIXActivity.FundCodeProperty, value);
            }
        }

        public static DependencyProperty CFGGestionFundCodeProperty = DependencyProperty.Register("CFGGestionFundCode", typeof(String), typeof(FIXActivity));
        [Description("CFG Gestion Fund Code")]
        [Category("Order")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public String CFGGestionFundCode
        {
            get
            {
                return ((String)(base.GetValue(FIXActivity.CFGGestionFundCodeProperty)));
            }
            set
            {
                base.SetValue(FIXActivity.CFGGestionFundCodeProperty, value);
            }
        }

        public static DependencyProperty CurrencyCodeProperty = DependencyProperty.Register("CurrencyCode", typeof(String), typeof(FIXActivity));
        [Description("Currency Code")]
        [Category("Order")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public String CurrencyCode
        {
            get
            {
                return ((String)(base.GetValue(FIXActivity.CurrencyCodeProperty)));
            }
            set
            {
                base.SetValue(FIXActivity.CurrencyCodeProperty, value);
            }
        }
        #endregion Properties

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            //DPH
            //using (SophisLogger oLogger = LogManager.GetDefaultLogger(this, "Execute"))
            ILogger oLogger = LogManager.Instance.GetDefaultLogger();
            {

                try
                {
                    //Retrieve the Order by its Order Id, only for single order
                    SingleOrder order = OrderManager.Instance.GetOrderById(OrderID) as SingleOrder;

                    if (order == null)
                    {
                        oLogger.LogError(String.Format("Target Order(id:{0} cannot be found or is not a single order, no allocation can be retrieved"));
                        return ActivityExecutionStatus.Closed;
                    }

                    //DPH
                    //IDictionary<int, AllocationRule> rules = order.AllocationRulesSet.PortfolioAllocations;
                    ICollection<AllocationRule> rules = order.AllocationRulesSet.Allocations;
                    if (rules.Count == 0) //no allocation so that no fund can be identified. in this case, no change needed.
                    {
                        oLogger.LogError(String.Format("No allocation associated to Order(id:{0})", OrderID));
                    }
                    else if (rules.Count == 1) //only one allocation
                    {

                        oLogger.LogDebug(String.Format("Found single allocation on Order(id:{0})", OrderID));
                        // Load the main extraction

                        //CSMPreference.SetLoadAllPortfolio(true);
                        //CSMExtraction.Load();

                        oLogger.LogDebug(String.Format("Main extraction successfully loaded"));

                        //DPH
                        //CSMAmPortfolio portfolio = CSMAmPortfolio.GetCSRPortfolio(rules.First().Value.PortfolioID);
                        CSMAmPortfolio portfolio = CSMAmPortfolio.GetCSRPortfolio(rules.First().PortfolioID);
                        CSMAmFund fund = null;

                        if (portfolio != null)
                        {
                            // The corresponding fund and its NAV
                            int fundCode = portfolio.GetHedgeFund();
                            oLogger.LogDebug(String.Format("Get FundCode {0}", fundCode));
                            fund = CSMAmFund.GetFund(fundCode);
                        }

                        if (fund == null)
                        {
                            //DPH
                            //oLogger.LogError(String.Format("Cannot find fund from portfolio id:{0}", rules.First().Value.PortfolioID));
                            oLogger.LogError(String.Format("Cannot find fund from portfolio id:{0}", rules.First().PortfolioID));
                            return ActivityExecutionStatus.Closed;
                        }
                        else
                        {
                            //cash account of fund
                            try
                            {
                                ArrayList accountIds = new ArrayList();
                                int sicoFund = fund.GetCode();
                                ArrayList primaryBrokers = fund.GetPrimeBrokers();
                                oLogger.LogDebug(String.Format("find {0} fund brokers", primaryBrokers.Count));
                                if (primaryBrokers.Count > 0)
                                {
                                    int firstPrimaryBroker = (int)primaryBrokers[0];
                                    CSMAmCashAccountSet accountSet = CSMAmCashAccountSet.GetInstance();
                                    accountSet.SetRefCountValue(-1000);
                                    accountSet.GetAccountsFiltered(accountIds);

                                    bool found = false;
                                    foreach (int id in accountIds)
                                    {
                                        CSMAmAccount account = accountSet.GetAccountPtr(id);
                                        using (CMString buffer = new CMString())
                                        {
                                            CSMCurrency.CurrencyToString(account.GetCurrency(), buffer);
                                            oLogger.LogDebug(String.Format("Cash account fund id = {0}, pb = {1}, ccy = {2} ", account.getFund(), account.getPb(), buffer));
                                            if (account.getFund() == sicoFund && account.getPb() == firstPrimaryBroker && buffer.ToString().CompareTo(CurrencyCode) == 0)
                                            {
                                                String cashAccountValue = account.getName().ToString();
                                                oLogger.LogDebug(String.Format("Found matched cash account {0}", cashAccountValue));
                                                FindOrCreatePropertyWithValue(order, CashAccount, cashAccountValue, cashAccountValue);
                                                oLogger.LogDebug(String.Format("Set OrderProperty[{0}] = {1}", CashAccount, cashAccountValue));
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!found)
                                    {
                                        oLogger.LogError(String.Format("Cash Account not found for fund[{0}], Primary Broker[{1}], Currency[{2}]", sicoFund, firstPrimaryBroker, CurrencyCode));
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                oLogger.LogError(String.Format("Cannot fill order property CashAccount, reason: {0}", ex.Message));
                            }

                            //property of entity
                            try
                            {
                                int entityCode = fund.GetEntity();
                                CSMThirdParty entity = CSMThirdParty.GetCSRThirdParty(entityCode);
                                using (CMString buffer = new CMString())
                                {
                                    entity.GetProperty("OMSCODCLI", buffer, 128);
                                    String fundCodeValue = buffer.ToString();
                                    FindOrCreatePropertyWithValue(order, FundCode, fundCodeValue, fundCodeValue);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                oLogger.LogError(String.Format("Cannot fill order property FundCode, reason: {0}", ex.Message));
                            }
                        }
                        OrderManager.Instance.UpdateOrder(order, order.CreationInfo);
                    }
                    else //more than one allocation
                    {
                        FindOrCreatePropertyWithValue(order, FundCode, CFGGestionFundCode, CFGGestionFundCode);
                        OrderManager.Instance.UpdateOrder(order, order.CreationInfo);
                    }
                }
                catch (System.Exception ex)
                {
                    oLogger.LogError(String.Format("Cannot fill order property, reason: {0}", ex.Message));
                    return ActivityExecutionStatus.Closed;
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        private static void FindOrCreatePropertyWithValue(IOrder in_Order, String in_PropertyName, String in_Value, String in_Text)
        {
            if (in_Order.Properties.Count(v => v.Definition.Name == in_PropertyName) == 0)
            {
                //DPH
                //IList<OrderPropertyDefinition> definitions = OrderManager.Instance.GetOrderPropertyDefinitions();
                //OrderPropertyDefinition prop_definition = definitions.Single(v => v.Name == in_PropertyName); //throw when not found
                //in_Order.Properties.Add(prop_definition.CreateProperty<String>(in_Value));
                in_Order.Properties.Add(new OrderProperty { Definition = new OrderPropertyDefinition { Name = in_PropertyName, DefaultValue = in_Value } });
            }
            //DPH
            //in_Order.Properties.Single(v => v.Definition.Name == in_PropertyName).SetValue<String>(in_Value, in_Text);
            in_Order.Properties.Single(v => v.Definition.Name == in_PropertyName).SetValue(in_Text);
        }
    }
}

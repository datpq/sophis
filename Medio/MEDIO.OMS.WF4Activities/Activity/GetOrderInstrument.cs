using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Globalization;
using sophis;
using sophis.instrument;
using sophis.oms;
using Sophis.Activities;
using Sophis.Logging;
using Sophis.OMS.Activities;
using Sophis.WF.Core;
using sophis.static_data;
using sophis.utils;
using MEDIO.CORE.Tools;
using sophis.backoffice_kernel;
using sophis.listed_market;
using sophis.market_data;

namespace MEDIO.OMS.WF4Activities.Activity
{
    public class OrderInstrument
    {
        private string _instrumentName;
        private string _instrumentRef;
        private string _InstrumentCurrency;
        private string _instrumentType;
        private string _allotment;

        public OrderInstrument()
        {
            _instrumentName = "";
            _instrumentRef = "";
            _InstrumentCurrency = "";
            _instrumentType = "";
            _allotment = "";
        }

        // Public
        public string Name
        {
            get { return this._instrumentName; }
            set { this._instrumentName = value; }
        }

        public string Ref
        {
            get { return this._instrumentRef; }
            set { this._instrumentRef = value; }
        }

        public string Currency
        {
            get { return this._InstrumentCurrency; }
            set { this._InstrumentCurrency = value; }
        }

        public string Type
        {
            get { return this._instrumentType; }
            set { this._instrumentType = value; }
        }

        public string Allotment
        {
            get { return this._allotment; }
            set { this._allotment = value; }
        }
    }

    [Category("Medio Order Runtime")]
    [Description("Get the instrument of current workflow context")]
    public sealed class GetOrderInstrument : CodeActivity<OrderInstrument>
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        protected override OrderInstrument Execute(CodeActivityContext context)
        {
            _logger.LogDebug("OrderInstrument::Execute=BEGIN");
            IOrder order = context.GetOrder();
            SingleOrder singleOrder = (SingleOrder)order;
            if (singleOrder == null)
            {
                _logger.LogError(string.Format(
                    "Only single orders are supported! Order ID: {0}", order.ID));
            }
            IOrderTargetOwner owner = singleOrder.GetOrderTargetOwners().FirstOrDefault<IOrderTargetOwner>();
            if ((owner == null) || (owner.Target == null))
            {
                _logger.LogError(string.Format("Cannot retrieve target (qty, side ...) for order {0} ",
                    singleOrder.ID));
            }

            OrderInstrument OrderInst = new OrderInstrument();
            SophisWcfConfig.SynchronizationContext.Send(delegate(object _)
            {
                CSMInstrument inst = CSMInstrument.GetInstance(owner.Target.SecurityID);
                if (inst != null)
                {
                    CMString name = inst.GetName();
                    OrderInst.Name = name;
                    OrderInst.Ref = inst.GetReference();
                    OrderInst.Type = GetTypeName(owner.Target.SecurityType);
                    var currency = CSMCurrency.GetCSRCurrency(owner.Target.Currency);
                    if (currency != null)
                    {
                        CMString currencyName = currency.GetName();
                        OrderInst.Currency = currencyName;
                    }
                    OrderInst.Allotment = SSMAllotment.GetName(inst.GetAllotment());
                }
            }, null);
            return OrderInst;
            
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument arg1 = new RuntimeArgument("Result", typeof(OrderInstrument), ArgumentDirection.Out);
            metadata.Bind(this.Result, arg1);
            metadata.AddArgument(arg1);

            if (this.Result == null)
            {
                metadata.AddValidationError("Result cannot be null");
            }
        }

        private string GetTypeName(int type)
        {
            string res = "";
            switch (type)
            { 
                case ESecurityType.AsianSwaption:
                    res ="Asian Swaption"; break;
                case ESecurityType.AsianSwaptionInLots:
                    res = "AsianSwaptionInLots"; break;
                case ESecurityType.AssetSwap:
                    res = "Asset Swap"; break;
                case ESecurityType.Bond:
                    res = "Bond"; break;
                case ESecurityType.CreditDefaultEvent:
                    res = "Credit Default Event"; break;
                case ESecurityType.CreditDefaultSwap:
                    res = "Credit Default Swap"; break;
                case ESecurityType.CrossCurrencySwap:
                    res = "Cross Currency Swap"; break;
                case ESecurityType.Debt:
                    res = "Debt"; break;
                case ESecurityType.Equity:
                    res = "Equity"; break;
                case ESecurityType.ETF:
                    res = "ETF"; break;
                case ESecurityType.FixedSwap:
                    res = "FixedSwap"; break;
                case ESecurityType.FloatSwap:
                    res = "FloatSwap"; break;
                case ESecurityType.Forex:
                    res = "Forex"; break;
                case ESecurityType.ForexForward:
                    res = "ForexForward"; break;
                case ESecurityType.ForexNDF:
                    res = "ForexNDF"; break;
                //TODO test and check
                case ESecurityType.InternalFund:
                    res = "InternalFund"; break;
                case ESecurityType.Future:
                    res = "Future"; break;
                case ESecurityType.Index:
                    res = "Index"; break;
                case ESecurityType.InflationCapFloor:
                    res = "InflationCapFloor"; break;
                case ESecurityType.InflationSwap:
                    res = "InflationSwap"; break;
                case ESecurityType.InterestRateSwap:
                    res = "Interest Rate Swap"; break;
                case ESecurityType.Option:
                    res = "Option"; break;
                case ESecurityType.OTCProduct:
                    res = "OTCProduct"; break;
                case ESecurityType.Package:
                    res = "Package"; break;
                case ESecurityType.Repo:
                    res = "Repo"; break;
                case ESecurityType.RepoPool:
                    res = "RepoPool"; break;
                case ESecurityType.Swaption:
                    res = "Swaption"; break;
                case ESecurityType.TotalReturnSwap:
                    res = "TotalReturnSwap"; break;
                case ESecurityType.TenorBasisSwap:
                    res = "TenorBasisSwap"; break;
                case ESecurityType.VanillaFXOption:
                    res = "VanillaFXOption"; break;
                case ESecurityType.VarianceSwap:
                    res = "VarianceSwap"; break;
                default:
                    res = "Undefined"; break;
            }
            return res;
        }
    }

}

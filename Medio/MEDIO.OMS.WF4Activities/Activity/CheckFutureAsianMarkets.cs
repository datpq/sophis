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
using MEDIO.TransactionAction.Source;
using sophis.listed_market;
using sophis.market_data;

namespace MEDIO.OMS.WF4Activities.Activity
{
    /// <summary>
    /// Activity returning whether the order (1) IsFuture and (2) IsAsianMarket
    /// </summary>
    public sealed class CheckAsianFutureMarkets : CodeActivity
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        private const string ORDER_PROPERTY_OutsidePit = MedioConstants.MEDIO_ORDER_PROPERTY_NAME_OUTSIDEPIT;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            metadata.RequireExtension<BookmarkResumptionHelper>();
            metadata.AddDefaultExtensionProvider<BookmarkResumptionHelper>(() => new BookmarkResumptionHelper());
        }

        protected override void Execute(CodeActivityContext context)
        {
            
            IOrder order = context.GetOrder();
            try
            {
                _logger.LogDebug("CheckAsianFutureMarkets::Execute=BEGIN");
                SingleOrder singleOrder = (SingleOrder) order;
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

                if (IsFuture(owner.Target.SecurityType))
                {
                    _logger.LogDebug(string.Format("Order {0} is a Future", singleOrder.ID));

                    if (CSxAsianMarketCheck.IsTradingAfterPit(owner.Target.SecurityID, singleOrder.CreationInfo.TimeStamp))
                    {
                        CSxOrderHelper.SetOrderProperty(order.Properties, true, ORDER_PROPERTY_OutsidePit);
                        _logger.LogDebug(string.Format("Order {0} is traded after pit session, flag property '{1}' to true", order.ID, ORDER_PROPERTY_OutsidePit));
                    }
                    else
                        _logger.LogDebug(string.Format("Order {0} is not traded after pit session", singleOrder.ID));
                }
                else
                    _logger.LogDebug(string.Format("Order {0} is not a Future", singleOrder.ID));
                _logger.LogDebug("CheckAsianFutureMarkets::Execute=Test");
            }
            catch (Exception exception)
            {
                string message = string.Format("Cannot get property for order {0}! {1}", order.ID, exception);
                _logger.LogError(message);
            }
        }
        

        private bool IsFuture(int SecurityType)
        {
            return SecurityType == ESecurityType.Future ? true : false;
        }
    }
}

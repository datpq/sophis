using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.oms;
using Sophis.Logging;
using Sophis.OMS.Activities;
using Sophis.WF.Core;

namespace MEDIO.OMS.WF4Activities.Activity
{
    public sealed class ChangeTimeInForceTime : CodeActivity
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        protected override void Execute(CodeActivityContext context)
        {
            IOrder order = context.GetOrder();
            try
            {
                _logger.LogDebug("ChangeTimeInForceDate::Execute=BEGIN");
                SingleOrder singleOrder = (SingleOrder) order;
                if (singleOrder == null)
                {
                    _logger.LogError(string.Format(
                        "Only single orders are supported! Order ID: {0}", order.ID));
                }

                if (singleOrder.TimeInForce.Type == ETimeInForce.GoodTillDate)
                {
                    if (singleOrder.TimeInForce.ExpiryDate.HasValue)
                    {
                        TimeSpan ts = new TimeSpan(23, 59, 59);
                        singleOrder.TimeInForce.ExpiryDate = singleOrder.TimeInForce.ExpiryDate.Value.Date + ts;
                        _logger.LogDebug(String.Format("Order #{0} time in force changed to {1}", singleOrder.ID, singleOrder.TimeInForce.ExpiryDate));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Exception = " + e);
            }
        }
        

    }
}

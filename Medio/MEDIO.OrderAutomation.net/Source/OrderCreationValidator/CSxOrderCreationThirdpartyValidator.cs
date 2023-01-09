using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using sophis.oms;
using sophis.utils;

namespace MEDIO.OrderAutomation.NET.Source.OrderCreationValidator
{
    public class CSxOrderCreationThirdpartyValidator : IOrderCreationValidator
    {
        public ValidationResult Validate(IOrder order, bool creating)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                if (order.ThirdParties == null)
                {
                    return new ValidationResult() { IsValid = true };
                }

                foreach (var third in order.ThirdParties)
                {
                    if (third.ThirdPartyID == -1)
                    {
                        order.ThirdParties.Remove(third);
                        break;
                    }
                }

                return new ValidationResult() { IsValid = true };
            }
        }
    }
}

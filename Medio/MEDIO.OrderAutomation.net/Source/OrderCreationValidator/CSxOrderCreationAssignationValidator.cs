using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.MEDIO_CUSTOM_PARAM;
using sophis.oms;
using sophis.utils;

namespace MEDIO.OrderAutomation.NET.Source.OrderCreationValidator
{
    public class CSxOrderCreationAssignationValidator  : IOrderCreationValidator
    {
        private static readonly string FXExceptionGroupName = CSxToolkitCustomParameter.Instance.USERGROUP_FXEXCEPTION_NAME;
        private static readonly string FXDefaultGroupName = CSxToolkitCustomParameter.Instance.USERGROUP_FXDEFAULT_NAME;

        private static readonly uint FXExceptionGroupId = CSMUserRights.ConvNameToIdent(FXExceptionGroupName);
        private static readonly uint FXDefaultsGroupId = CSMUserRights.ConvNameToIdent(FXDefaultGroupName);

        public ValidationResult Validate(IOrder order, bool creating)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                if (order is SingleOrder)
                {
                    SingleOrder sOrder = (SingleOrder)order;
                    if (sOrder.Target is ForexTarget)
                    {
                        LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Order #{0} is an FX order", order.ID));
                        int uident = 0;
                        int gident = 0;
                        CSMPreference.GetUserID(ref uident, ref gident);

                        if (gident != FXExceptionGroupId)
                        {
                            LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("User {0} is not in the Treasury group. Assign the order to Derivatives group.", uident));
                            if (order.Assignation == null) order.Assignation = new Assignation();
                            order.Assignation.To = (int)FXDefaultsGroupId;
                        }
                        else
                        {
                            LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("User {0} is in the Treasury group. Do nothing", uident));
                        }
                    }
                }
                // Always return true, we just want to change the assignation
                return new ValidationResult() { IsValid = true };
            }
        }

    }
}

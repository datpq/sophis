using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using sophis.oms;
using sophis.oms.execution;
using sophis.oms.execution.entity;
using sophis.utils;

namespace MEDIO.CORE.Tools
{
    public class CSxOrderHelper
    {
        public static void UpdateOrders(List<IOrder> Orders)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxOrderHelper", "UpdateOrders");
                try
                {
                    OrderManagerConnector.Instance.GetOrderManager().UpdateOrders(Orders);
                }
                catch (Exception ex)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_error, ex.Message);
                    MessageBox.Show(ex.Message);
                }
                LOG.End();
            }
        }

        public static void SetOrderProperty<T>(IList<OrderProperty> singleOrderProperties, T orderValue, String orderPropertyName)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxOrderHelper", "SetOrderProperty");
                OrderProperty prop = GetOrderProperty(singleOrderProperties, orderPropertyName);
                if (prop!=null) prop.SetValue(orderValue);
                LOG.End();
            }
        }

        private static OrderProperty GetOrderProperty(IList<OrderProperty> singleOrderProperties, String orderPropertyName)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxOrderHelper", "GetOrderProperty");
                List<OrderProperty> found = new List<OrderProperty>();// beware runtime type of singleOrderProperties is not List<OrderProperty>. So use IEnumerator within foreach
                foreach (OrderProperty oneProp in singleOrderProperties)
                {
                    if (oneProp.Definition.Name == orderPropertyName)
                    {
                        found.Add(oneProp);
                        continue;
                    }
                }
                if (found.Count == 0)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_warning, string.Format("Order property '{0}' is not defined", orderPropertyName));
                    return null;
                }
                else if (found.Count > 1)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_warning, string.Format("{0} order properties '{1}' defined!", found.Count, orderPropertyName));
                    return null;
                }
                LOG.End();
                return found[0];
            }
        }

        public static void SetExecProperty<T>(IList<ExecutionProperty> execProperties, T orderValue, String orderPropertyName)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxOrderHelper", "SetOrderProperty");
                ExecutionProperty prop = GetExecProperty(execProperties, orderPropertyName);
                if (prop != null) prop.SetValue(orderValue);
                LOG.End();
            }
        }

        public static ExecutionProperty GetExecProperty(IList<ExecutionProperty> execProperties, String execPropertyName)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin("CSxOrderHelper", "GetExecProperty");
                List<ExecutionProperty> found = new List<ExecutionProperty>();// beware runtime type of singleOrderProperties is not List<OrderProperty>. So use IEnumerator within foreach
                foreach (ExecutionProperty oneProp in execProperties)
                {
                    if (oneProp.Definition.Name == execPropertyName)
                    {
                        found.Add(oneProp);
                        continue;
                    }
                    var list = new List<Int32>(){1,2};
                    Math.Min(1, 2);
                }
                if (found.Count == 0)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_warning, string.Format("Exec property '{0}' is not defined", execPropertyName));
                    return null;
                }
                else if (found.Count > 1)
                {
                    LOG.Write(CSMLog.eMVerbosity.M_warning, string.Format("{0} exec properties '{1}' defined!", found.Count, execPropertyName));
                    return null;
                }
                LOG.End();
                return found[0];
            }
        }
    }
}

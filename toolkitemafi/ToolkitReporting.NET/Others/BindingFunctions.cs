using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.reporting;
using Sophis.Reporting.Controls;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public abstract class BindingFunctionImpl : CSMBindingFunction
        {
            public BindingFunctionImpl(string functionName, params object[] args)
            {
                SetFunctionName(functionName);
                var paramNames = new ArrayList();
                paramNames.AddRange(args);
                SetParametersNames(paramNames);
            }

            public static void InitializeAllBidingFunctions()
            {
                CSMBindingFunctionManager.GetInstance().AddBindingFunction(new BfLastDayOfMonth());
            }

            public override void InitializeFunctionName() {}
            public override void InitializeParametersNames(){}
        }

        public class BfLastDayOfMonth : BindingFunctionImpl
        {
            public BfLastDayOfMonth() : base("LastDayOfMonth", "date") {}

            public override string Execute(ArrayList paramValues)
            {
                return "30/04/2018";
            }
        }
    }
}

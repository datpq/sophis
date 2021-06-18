using System;

using sophis;
using sophis.utils;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace FCI_CSharp
{
    /// <summary>
    /// Definition of DLL entry point, registrations, and closing point
    /// </summary>
    public class MainClass : IMain
    {
        public void EntryPoint()
        {
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            // TO DO; Perform registrations

            sophis.portfolio.CSMPortfolioColumn.Register("Column Toolkit", new FCIColumnToolkit());
            sophis.backoffice_kernel.CSMKernelCondition.Register("Check newquantity compliance", new FCIKernelConditionNewQuantity());
			//}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}
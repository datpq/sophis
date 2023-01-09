using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using sophis.backoffice_kernel;
using sophis.portfolio;
using sophis.tools;
using sophis.utils;

namespace MEDIO.BackOffice.net.src.KernelEngine
{
    class CSxRBCABSKernelEngine : sophis.backoffice_kernel.CSMKernelEngine
    {

        private string fClassName = "CSxExecutionAveragePriceKernelEngine";

        public override void Run(CSMTransaction original, CSMTransaction final, ArrayList recipientType, eMGenerationType generationType, CSMEventVector mess, int event_id)
        {

            try
            {
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Begin.");

                long transactionId = 0;

                // Avoid issue when transaction Id is null ( "original" at creation or "final" at deletion )
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_verbose, "Retrieving the trade id...");

                if (final != null)
                {
                    transactionId = final.getInternalCode();
                }
                else if (original != null)
                {
                    transactionId = original.getInternalCode();
                }
                else
                {
                    throw new Exception("The Trade is NULL !");
                }

                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_verbose, "Adding the CSxEventExecAveragePrice to Event Vector with creation param: " + transactionId);

                CSxRBCABSEvent RBCABSEvent = new CSxRBCABSEvent(transactionId);
                RBCABSEvent.ActivateNativeLifeCycle();
                mess.Add(RBCABSEvent);

            }
            catch (System.Exception ex)
            {
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "An Exception Occurred: " + ex.Message);
            }
        }
    }
}

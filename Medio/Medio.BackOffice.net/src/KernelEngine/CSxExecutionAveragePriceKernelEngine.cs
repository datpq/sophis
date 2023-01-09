using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection;
using MEDIO.BackOffice.net.src.Utils.Email;
using MEDIO.CORE.ConfigElements;
using sophis.backoffice_kernel;
using sophis.portfolio;
using sophis.tools;
using sophis.utils;

namespace MEDIO.BackOffice.net.src.KernelEngine
{
    public class CSxExecutionAveragePriceKernelEngine : sophis.backoffice_kernel.CSMKernelEngine
    {
        private string fClassName = "CSxExecutionAveragePriceKernelEngine";

        public static int fKernelEventId = 780;

        public CSxExecutionAveragePriceKernelEngine()
        {
            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "In Constructor");
            string fKernelEvent = "Check Av Price";
            sophis.misc.CSMConfigurationFile.getEntryValue("MEDIO_BO_DEALACTION_CUSTOM_SECTION", "MEDIO_BO_DEALACTION_CUSTOM_SECTION_CHECKAVPRICE",ref fKernelEvent);
            fKernelEventId = sophis.backoffice_kernel.CSMKernelEvent.GetIdByName(fKernelEvent);
            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Got fKernelEventId = "+fKernelEventId);
        }


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

                CSxEventExecAveragePrice avgPriceEvent = new CSxEventExecAveragePrice(transactionId, fKernelEventId);
                avgPriceEvent.ActivateNativeLifeCycle();
                mess.Add(avgPriceEvent);
                
            }
            catch (System.Exception ex)
            {
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "An Exception Occurred: " + ex.Message);
            }
            
        }
    }
}

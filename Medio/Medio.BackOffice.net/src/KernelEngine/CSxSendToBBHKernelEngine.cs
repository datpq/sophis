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
    public class CSxSendToBBHKernelEngine : sophis.backoffice_kernel.CSMKernelEngine
    {
        private string fClassName = "CSxSendToBBHKernelEngine";

        public static int fKernelEventId = 1662;//Send to BBH

        public CSxSendToBBHKernelEngine()
        {
            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "In Constructor");          
            string fKernelEventName = "Send to BBH";
            sophis.misc.CSMConfigurationFile.getEntryValue("MEDIO_BO_DEALACTION_CUSTOM_SECTION", "MEDIO_BO_DEALACTION_CUSTOM_SECTION_SENDTOBBH", ref fKernelEventName, "Send to BBH");
            fKernelEventId = sophis.backoffice_kernel.CSMKernelEvent.GetIdByName(fKernelEventName);
            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Got fKernelEventId = " + fKernelEventId);
        }


        public override void Run(CSMTransaction original, CSMTransaction final, ArrayList recipientType, eMGenerationType generationType, CSMEventVector mess, int event_id)
        {

            try
            {
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Begin.");

                long transactionId = 0;

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

                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_verbose, "Adding the CSxSendToBBHKernelEngine to Event Vector with creation param: trade id=" + transactionId +" evtId="+ fKernelEventId);
             
                CSxEventExecAveragePrice avPriceAndSendEvent = new CSxEventExecAveragePrice(transactionId, fKernelEventId);
                avPriceAndSendEvent.ActivateNativeLifeCycle();
                mess.Add(avPriceAndSendEvent);

            }
            catch (System.Exception ex)
            {
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "An Exception Occurred: " + ex.Message);
            }

        }
    }
}

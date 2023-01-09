using sophis.backoffice_kernel;
using sophis.portfolio;
using sophis.tools;
using sophis.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MEDIO.BackOffice.net.src.KernelEngine
{

    public class CSxRefreshFeesKernelEngine : sophis.backoffice_kernel.CSMKernelEngine
    {
        private string fClassName = "CSxRefreshFeesKernelEngine";


        public CSxRefreshFeesKernelEngine()
        {
            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "In Constructor");
        }


        public override void Run(CSMTransaction original, CSMTransaction final, ArrayList recipientType, eMGenerationType generationType, CSMEventVector mess, int event_id)
        {

        }


        public override void RunVote(CSMTransaction original, CSMTransaction final, ArrayList recipientType, eMGenerationType generationType, CSMEventVector mess, int event_id)
        {
            try
            {
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Begin.");

                long transactionId = 0;

                if (final != null)
                {
                    transactionId = final.getInternalCode();

                    CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Recomputing fees and gross amount for trade id: " + transactionId);
                    final.RecomputeGrossAmount(true);
                }
            }
            catch (System.Exception ex)
            {
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "An Exception Occurred: " + ex.Message);
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using sophis.backoffice_kernel;
using sophis.portfolio;
using sophis.utils;
using sophis.tools;

using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Net.Mail;
using System.Reflection;

namespace MEDIO.BackOffice.net.src.KernelEngine
{
    class CSxRBCABSEvent : sophis.tools.CSMAbstractEvent
    {
        private long fRefcon = 0;

        private string fClassName = "CSxRBCABS";

        public CSxRBCABSEvent(long refcon)
            : base()
        {

            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Begin for transaction id: " + refcon);
            fRefcon = refcon;
            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "End.");
        }
        public override void Send()
        {
            CSMTransaction trans = CSMTransaction.newCSRTransaction(fRefcon);

            if (trans != null)
            {
                trans.DoAction(782);
            }
        }

    }
}

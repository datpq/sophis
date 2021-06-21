using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TKOAllotment
{
    static class AllotmentData
    {
        public static int GetIRFuturesCTAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("IR Futures CT");
        }

        public static int GetIRFuturesAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("Futures");
        }

        public static int GetIROptionsCTAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("IR Options CT");
        }

        public static int GetIROptionsFuturesAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("IR Options Futures");
        }

        public static int GetOptionsCDSAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("Options CDS");
        }

        public static int GetIRSwapsAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("IR Swaps");
        }

        public static int GetSwaptionsAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("Swaptions");
        }

        public static int GetListedOptionsAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("Listed Options");
        }

        public static int GetCDSAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("CDS");
        }

        public static int GetOTCStockDerivativesAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("OTC Credit Derivatives");
        }

        public static int GetOTCIRDerivativesAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("OTC IR Derivatives");
        }

        public static int GetTRSAllotmentID()
        {
            return sophis.backoffice_kernel.SSMAllotment.GetByName("TRS");
        }

    }
}

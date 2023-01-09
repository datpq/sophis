using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.portfolio;
using sophis.instrument;
using sophis.utils;
using sophis.static_data;

namespace MEDIO.FXCompliance.net
{
    class MedioCSxCCYCriterium : CSMCriterium
    {
        public override void GetName(int code, sophis.utils.CMString name, long size)
        {
            CSMCurrency.CurrencyToString(code, name);
            size = name.ToString().Length;
        }

        public override CSMCriterium.MCriterionCaps GetCaps()
        {
            return new MCriterionCaps(true, true, false);
        }

        //public override void GetCode(sophis.portfolio.CSMPosition position, System.Collections.ArrayList list)
        //{
        //    list.Clear();
        //    CSMCriterium.SSMOneValue value = new SSMOneValue();

        //    //check if is FX Forward allotment
        //    CSMInstrument instr = CSMInstrument.GetInstance(position.GetInstrumentCode());
        //    //if (instr.GetAllotment() == allo)
        //    string strType = instr.GetInstrumentType().ToString();

        //    if ((strType == "E") || (strType == "X"))
        //    {
        //        CSMForexFuture forexFuture = CSMForexSpot.GetInstance(instr.GetCode());
        //        CSMForexSpot forexSpot = CSMForexSpot.GetInstance(instr.GetCode());
        //        SSMFxPair fxPair = new SSMFxPair();

        //        if (forexFuture != null)
        //        {
        //            fxPair.fDev1 = forexFuture.GetCurrency();
        //            fxPair.fDev2 = forexFuture.GetExpiryCurrency();
        //        }
        //        else
        //        {
        //            if (forexSpot != null)
        //            {
        //                fxPair = forexSpot.GetFxPair();
        //            }
        //            else
        //            {
        //                return;
        //            }
        //        }

        //        int nCCY1 = fxPair.fDev1;
        //        int nCCY2 = fxPair.fDev2;
        //        CMString CCY1 = new CMString();
        //        CSMCurrency.CurrencyToString(nCCY1, CCY1);
        //        CMString CCY2 = new CMString();
        //        CSMCurrency.CurrencyToString(nCCY2, CCY2);

        //        if ((CCY1 != "EUR") && (CCY2 != "EUR"))
        //        {
        //            value.fCode = 0;
        //        }
        //        if (CCY1 != "EUR")
        //        {
        //            value.fCode = nCCY1;
        //        }
        //        if (CCY2 != "EUR")
        //        {
        //            value.fCode = nCCY2;
        //        }
        //    }
        //    else
        //    {
        //        List<int> ids = new List<int> { 1, 12, 13, 1159 };

        //        if (ids.IndexOf(instr.GetAllotment()) != -1)
        //        {
        //            int nCCY = instr.GetCurrency();
        //            CMString CCY = new CMString();
        //            CSMCurrency.CurrencyToString(nCCY, CCY);

        //            if (CCY == "EUR")
        //            {
        //                value.fCode = 0;
        //            }
        //            else
        //            {
        //                value.fCode = nCCY;
        //            }
        //        }
        //        else
        //        {
        //            value.fCode = 0;
        //        }
        //    }

        //    list.Add(value);
        //}

        public override void GetCode(SSMReportingTrade mvt, System.Collections.ArrayList list)
        {
            list.Clear();
            CSMCriterium.SSMOneValue value = new SSMOneValue();

            //check if is FX Forward allotment
            CSMInstrument instr = CSMInstrument.GetInstance(mvt.sicovam);
            //if (instr.GetAllotment() == allo)
            string strType = instr.GetInstrumentType().ToString();

            if ((strType == "E") || (strType == "X"))
            {
                CSMForexFuture forexFuture = CSMForexSpot.GetInstance(instr.GetCode());
                CSMForexSpot forexSpot = CSMForexSpot.GetInstance(instr.GetCode());
                SSMFxPair fxPair = new SSMFxPair();

                if (forexFuture != null)
                {
                    fxPair.fDev1 = forexFuture.GetCurrency();
                    fxPair.fDev2 = forexFuture.GetExpiryCurrency();
                }
                else
                {
                    if (forexSpot != null)
                    {
                        fxPair = forexSpot.GetFxPair();
                    }
                    else
                    {
                        return;
                    }
                }

                int nCCY1 = fxPair.fDev1;
                int nCCY2 = fxPair.fDev2;
                CMString CCY1 = new CMString();
                CSMCurrency.CurrencyToString(nCCY1, CCY1);
                CMString CCY2 = new CMString();
                CSMCurrency.CurrencyToString(nCCY2, CCY2);

                if ((CCY1 != "EUR") && (CCY2 != "EUR"))
                {
                    value.fCode = 0;
                }
                if (CCY1 != "EUR")
                {
                    value.fCode = nCCY1;
                }
                if (CCY2 != "EUR")
                {
                    value.fCode = nCCY2;
                }
            }
            else
            {
                List<int> ids = new List<int> { 1, 12, 13, 1159 };

                if (ids.IndexOf(instr.GetAllotment()) != -1)
                {
                    int nCCY = instr.GetCurrency();
                    CMString CCY = new CMString();
                    CSMCurrency.CurrencyToString(nCCY, CCY);

                    if (CCY == "EUR")
                    {
                        value.fCode = 0;
                    }
                    else
                    {
                        value.fCode = nCCY;
                    }
                }
                else
                {
                    value.fCode = 0;
                }
            }

            list.Add(value);
        }

   }
}

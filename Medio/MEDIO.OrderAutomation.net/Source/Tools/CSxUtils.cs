using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using Oracle.DataAccess.Client;
using sophis.instrument;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
using sophis.value;

namespace MEDIO.OrderAutomation.net.Source.Tools
{
    public static class CSxUtils
    {
        public static int ToInt32(this string str)
        {
            int res = 0;
            res = Int32.TryParse(str, out res) ? res : 0;
            return res;
        }

        public static string GetFundCurrency(int folioId, out int ccy)
        {
            CMString res = "";
            ccy = 0;
            CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                CSMCurrency currency = CSMCurrency.GetCSRCurrency(folio.GetCurrency());
                if (currency != null)
                {
                    ccy = folio.GetCurrency();
                    res = GetCurrencyName(ccy);
                }
            }
            return res;
        }

        public static string GetFundName(int folioId)
        {
            CMString res = "";
            CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                CSMAmPortfolio fund = folio.GetFundRootPortfolio();
                if (fund != null)
                {
                    fund.GetName(res);
                }
            }
            return res;
        }

        //?? CSMCurrency.StringToCurrency
        //public static int StringToCurrency(string name)
        //{
        //    int res = 0;
        //    try
        //    {
        //        string sql = "select STR_TO_DEVISE(:name) from dual";
        //        OracleParameter parameter = new OracleParameter(":name", name);
        //        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
        //        res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
        //    }
        //    catch (Exception ex)
        //    {
        //        return res;
        //    }
        //    return res;
        //}

        public static double GetFundNAV(int folioId)
        {
            double res = 0;
            CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                CSMAmPortfolio fund = folio.GetFundRootPortfolio();
                if (fund != null)
                {                    
                    //if (!fund.IsLoaded())
                    //    fund.Load();
                    //fund.Compute();
                    res = fund.GetNetAssetValue();
                }
            }
            return res;
        }

        public static string GetCurrencyName(int? ccy)
        {
            int id = 0;
            CMString res = "";
            if (ccy == null || ccy <= 0)
                return res;
            id = ccy.Value;
            CSMCurrency.CurrencyToString(id, res);
            return res;
        }

        public static string GetFullFolioPath(int folioId)
        {
            CMString fullName = "";
            CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                folio.GetFullName(fullName);
            }
            return fullName;
        }

        public static string GetFolioName(int folioId)
        {
            CMString name = "";
            CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                folio.GetName(name);
            }
            return name;
        }

        public static string GetInstrumentName(int sicovam)
        {
            string res = "";
            CSMInstrument targetInstrument = CSMInstrument.GetInstance(sicovam);
            //Instrument name
            using (CMString instrumentName = targetInstrument.GetName())
            {
                res = instrumentName;
            }
            return res;
        }

        /// <summary>
        /// Should be used only in an Extraction 
        /// </summary>
        /// <returns></returns>
        public static bool IsVirtualPortfolioFXForward(CSMPortfolio portfolio, out CSMForexFuture fxFwd)
        {
            int sicovam = CSMInstrument.GetCodeWithName(portfolio.GetName());
            sicovam = sicovam == 0 ? CSMInstrument.GetCodeWithReference(portfolio.GetName()) : sicovam;
            fxFwd = CSMInstrument.GetInstance(sicovam);
            return fxFwd != null;
        }

    }
}

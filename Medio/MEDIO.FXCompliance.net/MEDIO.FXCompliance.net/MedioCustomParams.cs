using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using sophis.misc;

namespace MEDIO.FXCompliance.net
{
    class MedioCustomParams
    {
        public static string PortfolioFXAmountName = "Commitment - FX Forwards";
        public static string PortfolioOtherAmountName = "Commitment - Underlyings";
        public static string FXAmount1 = "Nominal 1st CCY";
        public static string FXAmount2 = "Nominal 2nd CCY";
        public static string FXColumn = "Commitment - FX Forwards ";
        public static string OtherColumn = "Commitment - Underlyings ";
        //public static string HedgeColumn = "Hedge Amount ";
        public static string HedgeColumn = "Commitment - FX Forwards ";
        public static string RankingColumnName = "Classification Commitment type";
        public static string RankingFXValue = "Commitment - FX Forwards";
        public static string RankingOtherValue = "Commitment - Underlyings";
        public static string CachingFXExposureCCYColumn = "N";
        public static int gRefreshVersion = -1;

        public static bool InitCustomParams()
        {
            bool bRet = true;

            try
            {
                CSMConfigurationFile.getEntryValue("MEDIO", "PortfolioFXAmountName", ref PortfolioFXAmountName, "Commitment - FX Forwards");
                CSMConfigurationFile.getEntryValue("MEDIO", "PortfolioOtherAmountName", ref PortfolioOtherAmountName, "Commitment - Underlyings");
                CSMConfigurationFile.getEntryValue("MEDIO", "FXAmountName1", ref FXAmount1, "Nominal 1st CCY");
                CSMConfigurationFile.getEntryValue("MEDIO", "FXAmountName2", ref FXAmount2, "Nominal 2nd CCY");
                CSMConfigurationFile.getEntryValue("MEDIO", "FXColumn", ref FXColumn, "Commitment - FX Forwards ");
                CSMConfigurationFile.getEntryValue("MEDIO", "OtherColumn", ref OtherColumn, "Commitment - Underlyings ");
                //CSMConfigurationFile.getEntryValue("MEDIO", "HedgeColumn", ref HedgeColumn, "Hedge Amount ");
                CSMConfigurationFile.getEntryValue("MEDIO", "HedgeColumn", ref HedgeColumn, "Commitment - FX Forwards ");
                CSMConfigurationFile.getEntryValue("MEDIO", "RankingColumnName", ref RankingColumnName, "Ranking Commitment type");
                CSMConfigurationFile.getEntryValue("MEDIO", "RankingFXValue", ref RankingFXValue, "Commitment - FX Forwards");
                CSMConfigurationFile.getEntryValue("MEDIO", "RankingOtherValue", ref RankingOtherValue, "Commitment - Underlyings");
               // CSMConfigurationFile.getEntryValue("MEDIO", "CachingFXExposureCCYColumn", ref CachingFXExposureCCYColumn, "N");
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception occurred while loading custom parameters: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                bRet = false;
            }

            return bRet;
        }

    }
}

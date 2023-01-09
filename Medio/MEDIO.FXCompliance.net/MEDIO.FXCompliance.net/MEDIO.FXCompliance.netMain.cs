using System;

using sophis;
using sophis.utils;
using MEDIO.FXCompliance.net;
using System.Reflection;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace MEDIO.FXCompliance.net
{
    /// <summary>
    /// Definition of DLL entry point, registrations, and closing point
    /// </summary>
    public class MainClass : IMain
    {
        public void EntryPoint()
        {

            CSMLog.Write("Main", "EntryPoint", CSMLog.eMVerbosity.M_error, "In Entry Point...");
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            // TO DO; Perform registrations
            try
            {
                MedioCustomParams.InitCustomParams();


                string sCCY = "";
                string sSQL = "SELECT DEVISE_TO_STR(CODE) CCY, CODE FROM DEVISEV2 WHERE DEVISE_TO_STR(CODE) != 'EUR'";
                using (OracleCommand cmd = Sophis.DataAccess.DBContext.Connection.CreateCommand())
                {
                    cmd.CommandText = sSQL;
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sCCY = reader[0].ToString();
                            var newAmountCol = new MedioCommitmentFXExposureCCYColumn(sCCY, MedioCustomParams.CachingFXExposureCCYColumn);
                            sophis.portfolio.CSMCachedPortfolioColumn.Register(newAmountCol.GetColumnName(), newAmountCol);
                            var newCommitmentCol = new MedioCommitmentAssetExposureCCYColumn(sCCY);
                            sophis.portfolio.CSMCachedPortfolioColumn.Register(newCommitmentCol.GetColumnName(), newCommitmentCol);
                            var newHedgeCol = new MedioCommimentNettingCCYColumn(sCCY);
                            sophis.portfolio.CSMCachedPortfolioColumn.Register(newHedgeCol.GetColumnName(), newHedgeCol);
                        }
                        reader.Close();
                    }
                }

                CSMLog.Write("Main", "EntryPoint", CSMLog.eMVerbosity.M_info, "Registration Last 3 Columns");

                var newGeneralFXAmountCol = new MedioGeneralCommitmentFXExposureCCYColumn();
                sophis.portfolio.CSMCachedPortfolioColumn.Register(newGeneralFXAmountCol.GetColumnName(), newGeneralFXAmountCol);
                var newGeneralCommitmentAmountCol = new MedioGeneralCommitmentAssetExposureCCYColumn();
                sophis.portfolio.CSMCachedPortfolioColumn.Register(newGeneralCommitmentAmountCol.GetColumnName(), newGeneralCommitmentAmountCol);
                var newGeneralHedgeCol = new MedioGeneralCommimentNettingCCYColumn();
                sophis.portfolio.CSMCachedPortfolioColumn.Register(newGeneralHedgeCol.GetColumnName(), newGeneralHedgeCol);

                //sophis.portfolio.CSMCriterium.Register("FX Netting Criterium", new MedioCSxCCYCriterium());
            }
            catch (Exception ex)
            {
                CSMLog.Write("Main", "EntryPoint", CSMLog.eMVerbosity.M_error, "Exception Occured at Registrations..." + ex.Message);
                //System.Windows.Forms.MessageBox.Show(ex.Message);
                //throw;
            }
            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }

   }

}

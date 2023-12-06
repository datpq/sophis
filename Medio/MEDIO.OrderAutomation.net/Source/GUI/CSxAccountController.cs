using Oracle.DataAccess.Client;
using sophis.utils;
using Sophis.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public class CSxAccountController
    {
       public static Dictionary<int, int> _thresholdAccMap = new Dictionary<int, int>();

        public static IList<CSxAccountDataModel> GetAccountsList()
        {
            List<CSxAccountDataModel> retval = new List<CSxAccountDataModel>();
            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = "SELECT A.ACCOUNT_NAME,R.ACC_ID, R.VALUE AS THRESHOLD FROM BO_TREASURY_ACCOUNT A, BO_TREASURY_EXT_REF R, BO_TREASURY_EXT_REF_DEF D  WHERE A.ACCOUNT_AT_CUSTODIAN IS NOT NULL AND A.ID = R.ACC_ID AND R.REF_ID = D.REF_ID AND D.REF_NAME = 'SubsRedsThreshold'";

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CSxAccountDataModel itm = new CSxAccountDataModel();
                            
                            string account = (string)reader["ACCOUNT_NAME"];
                            int threshold = Convert.ToInt32(reader["THRESHOLD"]);
                            int accId = Convert.ToInt32(reader["ACC_ID"]);
                            itm.AccountName = account;
                            itm.Threshold = threshold;
                            retval.Add(itm);

                            if (_thresholdAccMap.ContainsKey(accId) == false)
                            {
                                _thresholdAccMap.Add(accId, threshold);
                            }
                          
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("CSxAccountController", "GetAccountsList", CSMLog.eMVerbosity.M_error, "Error occurred while trying to read accounts from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            return retval;
        }
    }
}

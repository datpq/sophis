using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using Sophis.Windows.Integration;
using sophis.utils;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using sophis.xaml;

namespace CFG_RetrocessionFeesGUI
{
    class CSxRetrocessionFeesResultsHandler
    {
        protected static CSxRetrocessionFeesGUI _RetrocessionFeesGUI;

        public static void DisplayResultsTable()
        {
            //If the Gui has not been created Or is initialized
            if (_RetrocessionFeesGUI == null || _RetrocessionFeesGUI.IsInitialized == false)
            {
                //Get audit dataTable
                DataTable resultsDataTable = GetResultsDataTable();

                _RetrocessionFeesGUI = new CSxRetrocessionFeesGUI();
                _RetrocessionFeesGUI.Initialize(resultsDataTable);
            }

            //Open The Gui In a MDI Container                
            sophis.xaml.XSRWinFormsAdapter<CSxRetrocessionFeesGUI> adapter = sophis.xaml.XSRWinFormsAdapter<CSxRetrocessionFeesGUI>.GetUniqueDialog(new WindowKey(14081985, 0, 0), "Retrocession fees results", () => _RetrocessionFeesGUI, true);
            adapter.ShowWindow();
        }

        private static DataTable GetResultsDataTable()
        {
            DataTable resultsTable = new DataTable();

            string SQLQuery = "select ROWNUM as \"Row\", TMP.* " +
                                    "from (select A.FUND_ID \"Fund id\", T.LIBELLE as \"Fund name\", A.BUSINESS_PARTNER_ID \"Business partner id\", " +
                                                "B.NAME as \"Business partner\", BUSINESS_PARTNER_TYPE \"Business partner type id\", " +
                                                "DECODE(A.BUSINESS_PARTNER_TYPE,0,'Promoteur de fonds',1,'Apporteur d affaires','error') as \"Business partner type\", " +
                                                "A.COMPUTATION_METHOD \"Computation method id\", DECODE(A.COMPUTATION_METHOD,0,'Prorata',1,'Moyenne pondérée AM',2,'Moyenne pondérée CGR',3,'XXX','error') \"Computation method\", A.START_DATE \"Start date\",A.END_DATE \"End date\", " +
                                                "A.AVERAGE_ASSET \"Average asset\", A.NB_DAYS \"Nb days\", A.RETRO_RATE \"Retro rate\", A.COMMISSION_RATE \"Commission rate\", A.AMOUNT_HT \"Amount HT\", A.AMOUNT_TTC \"Amount TTC\" " +
                                          "from CFG_RETRO_FEES_RESULTS A, TITRES T, TIERS B " +
                                          "where A.FUND_ID = T.SICOVAM and A.BUSINESS_PARTNER_ID = B.IDENT " +
                                          "order by FUND_ID,BUSINESS_PARTNER_ID,BUSINESS_PARTNER_TYPE,COMPUTATION_METHOD,START_DATE,END_DATE) TMP";


            using (OracleCommand myCommand = new OracleCommand(SQLQuery, DBContext.Connection))
            {
                using (OracleDataAdapter dataAdapter = new OracleDataAdapter(myCommand))
                {
                    DataSet ds = new DataSet();
                    dataAdapter.Fill(ds, "CFG_RETRO_FEES_RESULTS");
                    resultsTable = ds.Tables["CFG_RETRO_FEES_RESULTS"];
                }
            }

            foreach (DataColumn oneColumn in resultsTable.Columns)
            {
                oneColumn.ReadOnly = true;
            }

            return resultsTable;
        }

        public static void DisplayRetroFeesDetailedResultsGUI(int rowId, int fundId, int businessPartnerId, int businessPartnerType, int computationMethod, int startDate, int endDate)
        {            
            //Open The Gui In a MDI Container   
            int key = 14081986 + rowId;
            WindowKey winKey = new WindowKey(key, 0, 0);

            IntPtr ptr = XSRWinFormsAdapter<CSxRetroFeesDetailedResultsGUI>.GetMDIDialog(winKey, true);

            if (ptr.ToInt32() == 0)
            {
                //Get detailed results dataTable
                DataTable detailedResultsTable = GetDetailedResultsDataTable(fundId, businessPartnerId, businessPartnerType, computationMethod, startDate, endDate);
                CSxRetroFeesDetailedResultsGUI retroFeesDetailedResultsGUI = new CSxRetroFeesDetailedResultsGUI();
                retroFeesDetailedResultsGUI.Initialize(computationMethod, detailedResultsTable);
                
                //DPH
                //XSRWinFormsAdapter<CSxRetroFeesDetailedResultsGUI> adapter = XSRWinFormsAdapter<CSxRetroFeesDetailedResultsGUI>.OpenMDIDialog(retroFeesDetailedResultsGUI, "Retrocession fees details for row " + rowId.ToString(), winKey, 0, null, null,null);
                XSRWinFormsAdapter<CSxRetroFeesDetailedResultsGUI> adapter = XSRWinFormsAdapter<CSxRetroFeesDetailedResultsGUI>.OpenMDIDialog(retroFeesDetailedResultsGUI, "Retrocession fees details for row " + rowId.ToString(), winKey, 0, null, null,System.Drawing.Rectangle.Empty);
                adapter.ShowWindow();
            }
        }

        private static DataTable GetDetailedResultsDataTable(int fundId, int businessPartnerId, int businessPartnerType, int computationMethod, int startDate, int endDate)
        {            
            DataTable detailedResultsDataTable = new DataTable();

            string SQLQuery = "select COMPUTATION_METHOD, NAV_DATE,NB_SHARES_BUSINESS_PARTNER,NB_SHARES_TOTAL, NB_DAYS, NAV, FDG, CDVM, DDG, MCL, FUND_PROMOTER_RETROCESSION, PNB  from CFG_RETRO_FEES_DETAILS where FUND_ID = " + fundId.ToString() + " and BUSINESS_PARTNER_ID = " + businessPartnerId.ToString() +
                                " and BUSINESS_PARTNER_TYPE = " + businessPartnerType.ToString() + " and COMPUTATION_METHOD = " + computationMethod.ToString() +
                                " and date_to_num(START_DATE) = " + startDate.ToString() + " and date_to_num(END_DATE) = " + endDate.ToString() + " order by NAV_DATE";

            using (OracleCommand myCommand = new OracleCommand(SQLQuery, DBContext.Connection))
            {
                using (OracleDataAdapter dataAdapter = new OracleDataAdapter(myCommand))
                {
                    DataSet ds = new DataSet();
                    dataAdapter.Fill(ds, "CFG_RETRO_FEES_DETAILS");
                    detailedResultsDataTable = ds.Tables["CFG_RETRO_FEES_DETAILS"];
                }                
            }

            foreach (DataColumn oneColumn in detailedResultsDataTable.Columns)
            {
                oneColumn.ReadOnly = true;
            }

            return detailedResultsDataTable;
        }
    }
}

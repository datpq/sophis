using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using sophis;
using sophis.utils;
using sophis.portfolio;
using System.ComponentModel;
using System.Globalization;
using sophis.tools;
using sophisTools;
using sophis.market_data;
using MEDIO.CORE.Tools;
using sophis.backoffice_kernel;
using sophis.instrument;
using Oracle.DataAccess.Client;


namespace MEDIO.BackOffice.net.src.DealAction
{
    public class CSxBondNotionalCheck : sophis.instrument.CSMInstrumentAction
    {
        static bool CheckNotionalWarning = false;

        public override void NotifyModified(CSMInstrument instrument, CSMEventVector message)
        {
            
            //base.NotifyModified(instrument, message);
            using (CSMLog Log = new CSMLog())
            {
                try
                {
                    int sicovam = instrument.GetCode();
                    CSMInstrument oldInstrument = CSMMarketData.GetCurrentMarketData().GetCSRInstrument(sicovam);

                    if (oldInstrument != null)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_debug, "Checking if ABS Bond or just bond...");
                        sophis.finance.CSMABSBond abs = oldInstrument;
                        CSMBond bond = oldInstrument;
                        // we apply the change only to bonds, and avoid touching the ABS trades...
                        if(bond != null && abs == null)
                        {
                        // detecting if change of notional (which impact existing trades like on Fixed Income instrument )
                        double oldNotional = oldInstrument.GetNotional();
                        double newNotional = instrument.GetNotional();
                        Log.Write(CSMLog.eMVerbosity.M_info, " Check Notional Change for instrument: " + instrument.GetReference().ToString() + "from :" + oldNotional.ToString() + " to: " + newNotional.ToString());
                        if (oldNotional != newNotional && newNotional != 0  && IsTraded(sicovam))
                            {
                                if (oldNotional == 0)
                                {
                                    sophis.misc.CSMConfigurationFile.getEntryValue("MediolanumGUI", "CheckNotionalChange", ref CheckNotionalWarning, false);
                                    if (CheckNotionalWarning)
                                    {
                                        // should be enables in the gui via config file
                                        string mess = "WARNING! INSTRUMENT NOTIONAL HAS BEEN UPDATED.\n THIS WILL CHANGE NOMINAL OF ANY LINKED TRADES, LEAVING THE QUANTITY UNCHANGED.";
                                        string title = "Notional Update";
                                        MessageBoxButtons buttons = MessageBoxButtons.OK;
                                        DialogResult result = MessageBox.Show(mess, title, buttons, MessageBoxIcon.Warning);

                                    }
                                }
                                else
                                {
                                    Log.Write(CSMLog.eMVerbosity.M_info, "Found Notional Change for instrument: " + instrument.GetReference().ToString() + "from :" + oldNotional.ToString() + " to: " + newNotional.ToString());
                                    double ratio = oldNotional / newNotional;
                                    ApplyRatio(sicovam, ratio);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, "Exception Occured: "+e.Message);
                }
            }
        }

        private bool IsTraded(int sicovam)
        {
            bool retval = false;

            string query = "SELECT COUNT(*) FROM JOIN_POSITION_HISTOMVTS WHERE SICOVAM = :reference";
            OracleParameter parameter = new OracleParameter(":reference", sicovam);
            List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
            
            int nb = Convert.ToInt32(CSxDBHelper.GetOneRecord(query,parameters));

            retval = nb != 0;

            return retval;

        }

        private void updateTrades(int sicovam, double ratio)
        {
            using (CSMLog Log = new CSMLog())
            {
                try
                {

                    // firest query
                    {
                        string query = "UPDATE JOIN_POSITION_HISTOMVTS SET QUANTITE = :factor*QUANTITE WHERE SICOVAM = :reference AND TYPECOURS != 0";
                        Log.Write(CSMLog.eMVerbosity.M_info, "Modifying all trades with query: " + query + " with factor = " + ratio.ToString() + " , Sicovam= " + sicovam.ToString());
                        OracleParameter param1 = new OracleParameter(":factor", ratio);
                        OracleParameter param2 = new OracleParameter(":reference", sicovam);
                        List<OracleParameter> parameters = new List<OracleParameter>() { param1, param2 };
                        CSxDBHelper.Execute(query, parameters);
                    }

                    //Query for coupons
                    {


                        string query2 = "UPDATE JOIN_POSITION_HISTOMVTS SET QUANTITE = :factor1*QUANTITE, COURS = COURS/:factor1 WHERE SICOVAM = :reference1 AND TYPECOURS = 0";
                        Log.Write(CSMLog.eMVerbosity.M_info, "Modifying all trades with query: " + query2 + " with factor = " + ratio.ToString() + " , Sicovam= " + sicovam.ToString());
                        OracleParameter param3 = new OracleParameter(":factor1", ratio);
                        OracleParameter param4 = new OracleParameter(":reference1", sicovam);
                        List<OracleParameter> parameters2 = new List<OracleParameter>() { param3, param4 };
                        CSxDBHelper.Execute(query2, parameters2);
                    }
                }
                catch (Exception e)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, "Exception Occured: "+e.Message);
                }
            }
        }

        private void ApplyRatio(int sicovam, double ratio)
        {
            using (CSMLog Log = new CSMLog())
            {


                try
                {
                    sophis.misc.CSMConfigurationFile.getEntryValue("MediolanumGUI", "CheckNotionalChange", ref CheckNotionalWarning, false);
                    if (CheckNotionalWarning)
                    {
                        // should be enables in the gui via config file         
                        string message = "WARNING! INSTRUMENT NOTIONAL HAS BEEN UPDATED. \n\n IF ‘OK’ IS SELECTED THIS WILL CHANGE THE QUANTITY OF ANY LINKED TRADES, LEAVING THE NOMINAL UNCHANGED.\n\n IF ‘CANCEL’ IS SELECTED THIS WILL CHANGE THE NOMINAL OF ANY LINKED TRADES, LEAVING THE QUANTITY UNCHANGED";
                        string title = "Notional Update";
                        MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
                        DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);

                        if (result == DialogResult.OK)
                        {

                            updateTrades(sicovam, ratio);

                        }
                        else
                        {
                            //Do Nothing...
                        }
                    }
                    else
                    {
                        // by default we update trades without asking so when update comes from servers this is done without GUI popup.
                        updateTrades(sicovam, ratio);
                    }
                }
                catch (Exception e)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, "Issue Occured processing the trades: " + e.Message);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.instrument;
using sophis.utils;
using sophis.market_data;
using sophis.tools;
using System.Collections;
using System.Windows.Forms;
using sophis.portfolio;
using System.Reflection;



namespace MEDIO.BackOffice.net
{

    public class CSxRussianBondRedemption : sophis.instrument.CSMInstrumentAction
    {
        static bool AdjustRedemptionWarning = false;

        public override void NotifyCreated(CSMInstrument instrument, CSMEventVector message)
        {
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin(typeof(CSxRussianBondRedemption).Name, MethodBase.GetCurrentMethod().Name);
                try
                {
                    Log.Write(CSMLog.eMVerbosity.M_debug, "Begin");
                    int sicovam = instrument.GetCode();
                    CSMBond bond = instrument;
                    Log.Write(CSMLog.eMVerbosity.M_debug, "Instrument code = " + sicovam);
                    if (bond != null)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_debug, "Instrument is a bond");

                        SSMComplexReference complexRef = new SSMComplexReference();
                        complexRef.type = "CALC_TYP_CODE";
                        complexRef.value = "";
                        CSMInstrument.GetClientReference(sicovam, complexRef);
                        Log.Write(CSMLog.eMVerbosity.M_debug, "Instrument ext reference CALC_TYP_CODE= " + complexRef.value);
                        CMString refISIN = bond.GetReference();
                        Log.Write(CSMLog.eMVerbosity.M_debug, "Instrument  reference " + refISIN);

                        if (complexRef.value == "1155" || refISIN.StringValue.StartsWith("RU"))
                        {
                            Log.Write(CSMLog.eMVerbosity.M_debug, "Russian bond. Adjusting redemption coupons.");
                            ArrayList newRedemption = new ArrayList();
                            bond.GetFixedRedemptions(newRedemption);
                           
                            ArrayList redemptionList = new ArrayList();
                            foreach (SSMRedemption item in newRedemption)
                            {
                                item.coupon = Math.Round(item.coupon, 2, MidpointRounding.ToEven);
                                redemptionList.Add(item);
                            }

                            bond.SetFixedRedemptions(redemptionList);
                           // bond.SetFixedRedemptionCustomizedIndex(redemptionList.Count);
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, "Exception Occured: " + e.Message);
                }
            }
        }


        public override void VoteForModification(CSMInstrument instrument)
        {
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin(typeof(CSxRussianBondRedemption).Name, MethodBase.GetCurrentMethod().Name);
                try
                {
                    Log.Write(CSMLog.eMVerbosity.M_debug, "Begin");
                    int sicovam = instrument.GetCode();
                    CSMBond bond = instrument;
                    CSMInstrument oldInstrument = CSMMarketData.GetCurrentMarketData().GetCSRInstrument(sicovam);
                    CSMBond oldBond = oldInstrument;

                    if (bond != null && oldBond != null)
                    {

                        SSMComplexReference complexRef = new SSMComplexReference();
                        complexRef.type = "CALC_TYP_CODE";
                        complexRef.value = "";
                        CSMInstrument.GetClientReference(sicovam, complexRef);

                        if (complexRef.value == "1155")
                        {
                            bool redemptionCouponsModified = false;

                            ArrayList oldRedemption = new ArrayList();
                            oldBond.GetFixedRedemptions(oldRedemption);

                            ArrayList newRedemption = new ArrayList();
                            bond.GetFixedRedemptions(newRedemption);

                            if (oldRedemption.Count != newRedemption.Count)
                            {
                                redemptionCouponsModified = true;
                            }
                            else
                            {
                                for (int i = 0; i < newRedemption.Count; i++)
                                {
                                    SSMRedemption newItem = (SSMRedemption)newRedemption[i];
                                    SSMRedemption oldItem = (SSMRedemption)oldRedemption[i];

                                    if (newItem.coupon != oldItem.coupon)
                                    {
                                        redemptionCouponsModified = true;
                                        break;
                                    }

                                }
                            }

                            if (redemptionCouponsModified == true)
                            {
                                //  use the entry already defined in the config file for CSxBondNotionalCheck action, this is set as TRUE in the client config and FALSE for the services
                                sophis.misc.CSMConfigurationFile.getEntryValue("MediolanumGUI", "CheckNotionalChange", ref AdjustRedemptionWarning, false);
                                if (AdjustRedemptionWarning)
                                {

                                    string message = "WARNING!\nRedemption coupon values will be rounded.\nPlease reopen the instrument to see the updated values!\nDo you want to proceed with the coupon adjustments?";
                                    string title = "Redemption Adjustment";
                                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;

                                    DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Question);

                                    if (result == DialogResult.Yes)
                                    {

                                        ArrayList redemptionList = new ArrayList();
                                        foreach (SSMRedemption item in newRedemption)
                                        {
                                            item.coupon = Math.Round(item.coupon, 2, MidpointRounding.ToEven);
                                            redemptionList.Add(item);
                                        }

                                        bond.SetFixedRedemptions(redemptionList);
                                       // bond.SetFixedRedemptionCustomizedIndex(redemptionList.Count);
                                    }


                                }
                                else //services side, no warning dialog to display, adjusting redemption
                                { 

                                    ArrayList redemptionList = new ArrayList();
                                    foreach (SSMRedemption item in newRedemption)
                                    {
                                        item.coupon = Math.Round(item.coupon, 2, MidpointRounding.ToEven);
                                        redemptionList.Add(item);
                                    }

                                    bond.SetFixedRedemptions(redemptionList);
                                    //bond.SetFixedRedemptionCustomizedIndex(redemptionList.Count);
                                }
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, "Exception Occured: " + e.Message);
                }
            }
        }

    }

}

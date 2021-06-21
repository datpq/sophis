using sophis.utils;
using System;
using System.Globalization;
using System.Windows.Forms;

using sophis.value;
using System.Linq;
using System.Threading;

namespace CFG_Utilities
{
    public class TaskFixingSrDialog
    {
        public const string SrTitle = "Subscriptions / Redemptions";
        private static int CTL_ID_IDENT = int.Parse("67", NumberStyles.HexNumber);
        private static int CTL_ID_GROSSAMOUNT = int.Parse("73", NumberStyles.HexNumber);
        private static int CTL_ID_UNITS = int.Parse("74", NumberStyles.HexNumber);
        private static int CTL_ID_SHARES = int.Parse("75", NumberStyles.HexNumber);
        private static int CTL_ID_INTERNAL_FEES = int.Parse("85", NumberStyles.HexNumber);

        private static CSMLog logger = new CSMLog();
        private static IntPtr CurrentDialogHwnd = IntPtr.Zero;
        private static long CurrentSrId = 0;

        public static void Run()
        {
            try
            {
                logger.Begin("TaskFixingSrDialog", "Run");
                var newSrDlg = WindowApi.FindWindow(default(string), SrTitle);
                if (!newSrDlg.Equals(IntPtr.Zero)) // if dialog is open
                {
                    Thread.Sleep(200); //wait for loading of dialog to finish
                    var srIdStrVal = WindowApi.GetTextByControlId(newSrDlg, CTL_ID_IDENT);
                    srIdStrVal = string.Concat(srIdStrVal.Where(c => !char.IsWhiteSpace(c) && c != ',' && c != '.'));
                    if (srIdStrVal != string.Empty)
                    {
                        var newSrId = long.Parse(srIdStrVal);
                        if (!newSrDlg.Equals(CurrentDialogHwnd)) // if action of opening dialog
                        {
                            CurrentDialogHwnd = newSrDlg;
                            logger.Write(CSMLog.eMVerbosity.M_debug, $"CurrentDialogHwnd = {CurrentDialogHwnd}");

                            CurrentSrId = newSrId;
                            logger.Write(CSMLog.eMVerbosity.M_debug, $"CurrentSrId = {CurrentSrId}");
                            Initialize();
                        }
                        else if (CurrentSrId != newSrId) // if dialog stay open, but clicking on button Next, Previous
                        {
                            CurrentSrId = newSrId;
                            logger.Write(CSMLog.eMVerbosity.M_debug, $"CurrentSrId = {CurrentSrId}");
                            Initialize();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Write(CSMLog.eMVerbosity.M_error, "error: " + e.Message);
                logger.Write(CSMLog.eMVerbosity.M_error, e.StackTrace);
            }
            finally
            {
                logger.End();
            }
        }

        private static void Initialize()
        {
            CSMAmFundSR fundSr = new CSMAmFundSR(CurrentSrId);
            var fund = CSMAmFund.CreateInstance(fundSr.GetFundCode());
            logger.Write(CSMLog.eMVerbosity.M_debug, $"GetSRRoundingMode = {fund.GetSRRoundingMode()}, GetSRType = {fundSr.GetSRType()}, GetAmount = {fundSr.GetAmount()}");
            if (fund.GetSRRoundingMode() == eMRoundingModeType.M_rmLower && fundSr.GetSRType() == eMSRType.M_srtNetAmount && fundSr.GetAmount() < 0)
            {
                var unitGuiStr = WindowApi.GetTextByControlId(CurrentDialogHwnd, CTL_ID_UNITS);
                unitGuiStr = string.Concat(unitGuiStr.Where(c => !char.IsWhiteSpace(c) && c != ',' && c != '.'));
                var unitsGui = double.Parse(unitGuiStr);

                logger.Write(CSMLog.eMVerbosity.M_debug, $"GetNAV = {fundSr.GetNAV()}, GetFeesInt = {fundSr.GetFeesInt()}");
                double dunits = fundSr.GetAmount() * (100 + fundSr.GetFeesInt()) / 100 / fundSr.GetNAV();
                int units = (int)Math.Floor(dunits);
                //double grossAmount = units * fundSr.GetNAV() * 100 / (100 - fundSr.GetFeesInt());
                double grossAmount = units * fundSr.GetNAV();
                double feeAmountInt = Math.Abs(grossAmount) * fundSr.GetFeesInt() / 100;

                logger.Write(CSMLog.eMVerbosity.M_debug, $"unitsGui={unitsGui}, dunits={dunits}, units={units}, grossAmount={grossAmount}, feeAmountInt={feeAmountInt}");
                //double units = fundSr.GetNbShares();
                //double grossAmount = fundSr.GetGrossAmount();
                //logger.Write(CSMLog.eMVerbosity.M_debug, $"unitsGui={unitsGui}, units={units}, grossAmount={grossAmount}");
                if (units != unitsGui)
                {
                    //MessageBox.Show(new WindowWrapper(CurrentDialogHwnd),
                    //    $"Attention: Les valeurs a considerer sont les suivantes:\nNet Amount={fundSr.GetAmount()}\nNbre parts={units}\nGross Amount={grossAmount}\nNB: Les valeurs en GUI sont erronées par 1 part de moins",
                    //    "CFG", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //fundSr.SetNbShares(units);
                    //fundSr.SetNbUnits(units);
                    //fundSr.SetGrossAmount(grossAmount);
                    //fundSr.SetFeesAmountInt(feeAmountInt);
                    WindowApi.SetTextByControlId(CurrentDialogHwnd, CTL_ID_UNITS, units.ToString());
                    WindowApi.SetTextByControlId(CurrentDialogHwnd, CTL_ID_SHARES, units.ToString());
                    WindowApi.SetTextByControlId(CurrentDialogHwnd, CTL_ID_GROSSAMOUNT, string.Format("{0:0.00}", grossAmount));
                    WindowApi.SetTextByControlId(CurrentDialogHwnd, CTL_ID_INTERNAL_FEES, string.Format("{0:0.00}", feeAmountInt));
                }
            }
        }
    }
}

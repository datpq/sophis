using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using sophis.utils;
using Sophis.Event;

namespace MEDIO.OrderAutomation.NET.Source.EvenListener
{
    /// <summary>
    /// Workaround to fix a bug on DOB. 
    /// When one clicks on 'Round Orders' it is not updated in database.
    /// </summary>
    public class CSxDOBEventListener
    {
        //When one clicks on 'Round Orders' an internal event is sent.
        //In debug we found out that the event id is 1095124559.
        private static int eWhat_DOBRoundingCheckBox = 1095124559;

        public static void HandleEvent(Sophis.Event.IEvent myEvent, ref bool deleteEvent)
        {
            if (myEvent.What == eWhat_DOBRoundingCheckBox)
            {
                CSMUserRights user = new CSMUserRights();
                SetRoundingDOBFlag(1, user.GetIdent());
            }
        }

        public static void Install()
        {
            SophisEventManager.Instance.AddHandler(new SophisEventHandler(HandleEvent), Layer.Model);
        }

        private static void SetRoundingDOBFlag(int flag, int userId)
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDOBEventListener", MethodBase.GetCurrentMethod().Name);
                string query = String.Format("update DOB_GUI_PREFS set ROUND_ORDERS= {0} WHERE USER_ID = {1}", flag, userId);
                if (CSxDBHelper.Execute(query))
                    log.Write(CSMLog.eMVerbosity.M_debug, "DOB rounding flag set to " + flag + " for user " + userId);
                else
                    log.Write(CSMLog.eMVerbosity.M_debug, "DOB rounding flag not set");
            }
        }
    }
}

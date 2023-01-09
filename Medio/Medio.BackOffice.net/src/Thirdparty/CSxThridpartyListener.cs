using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MEDIO.BackOffice.net.src.DealCondition;
using sophis.backoffice_kernel;
using Sophis.Event;

namespace MEDIO.BackOffice.net.src.Thirdparty
{
    [Obsolete] // CSxThirdpartyAction is used instead
    class CSxThridpartyListener
    {
        private static int eWhat_ThirpartySave = 1179796303;
        private static Dictionary<string, List<CSxThirdpartyAllotment>> _thirdpartyAllotments = new Dictionary<string, List<CSxThirdpartyAllotment>>();

        public static void HandleEvent(Sophis.Event.IEvent myEvent, ref bool deleteEvent)
        {
            if (myEvent.What == eWhat_ThirpartySave)
            {
                var allPrototypes = CSMKernelCondition.GetAllPrototypes();
                var allAgreementNames = CSxThirdpartyCondition.GetAgreementNames();
                // Refresh
                foreach (var one in allAgreementNames)
                {
                    UpdatethirdpartyAllotment(one);
                    string name = String.Format("Valid {0}", one);
                    if (!allPrototypes.ContainsKey(name))
                    {
                        CSMKernelCondition.Register(name, new CSxThirdpartyCondition(one));
                    }
                }
                // Unregister  
                foreach (var one in allPrototypes)
                {
                    string toBeSearched = "Valid ";
                    if (one.Key.StartsWith(toBeSearched))
                    {
                        var name = one.Key.Substring(one.Key.IndexOf(toBeSearched) + toBeSearched.Length);
                        if (!allAgreementNames.Contains(name))
                        {
                            one.Value.UnRegister();
                            UpdatethirdpartyAllotment(name, true);
                        }
                    }
                }
            }
        }

        public static void Install()
        {
            SophisEventManager.Instance.AddHandler(new SophisEventHandler(HandleEvent), Layer.Model);
        }

        public static List<CSxThirdpartyAllotment> GetThirdpartyAllotment(string key)
        {
            if (!_thirdpartyAllotments.ContainsKey(key))
            {
                _thirdpartyAllotments[key] = CSxThirdpartyCondition.GetThirdpartyAllotment(key);
            }
            return _thirdpartyAllotments[key];
        }

        public static void UpdatethirdpartyAllotment(string key, bool deleteElement = false)
        {
            if (deleteElement)
            {
                if (_thirdpartyAllotments.ContainsKey(key))
                {
                    _thirdpartyAllotments.Remove(key);
                }
            }
            else
            {
                _thirdpartyAllotments[key] = CSxThirdpartyCondition.GetThirdpartyAllotment(key);
            }
        }

    }
}

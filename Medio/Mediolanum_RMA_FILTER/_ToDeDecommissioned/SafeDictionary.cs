using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.utils;

namespace Mediolanum_RMA_FILTER
{
    public class SafeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new TValue this[TKey key]
        {
            get
            {
                TValue value = default(TValue);
                try
                {
                    value = this[key];
                }
                catch (Exception ex)
                {
                    CSMLog.Write("SafeDictionary", "TValue this[TKey] get", CSMLog.eMVerbosity.M_error, "Failed to access SafeDictionary<" + typeof(TKey).Name + "," + typeof(TValue).Name + "> value by key = " + key + ". Returning default value = " + value + ". Exception message: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    //value = default(TValue);
                }
                return value;
            }
        }
    }
}

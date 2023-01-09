using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using Mediolanum_RMA_FILTER.Filters;
using sophis.log;
using sophis.utils;

namespace Mediolanum_RMA_FILTER.Data
{
    public class CSxCachingDataManager
    {
        private static string _ClassName = typeof(CSxCachingDataManager).Name; 

        #region Singleton
        private static CSxCachingDataManager gInstance;
        public static CSxCachingDataManager Instance
        {
            get
            {
                if (gInstance == null)
                    gInstance = new CSxCachingDataManager();
                return gInstance;
            }
        }
        #endregion

        Dictionary<string, object> cache = new Dictionary<string, object>();
        static readonly object padlock = new object();
        public virtual void AddItem(string key, object value)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (string.IsNullOrEmpty(key))
                {
                    logger.log(Severity.warning, "key is null");
                }
                if (cache.ContainsKey(key))
                {
                    cache[key] = value;
                    logger.log(Severity.warning, "An element with the same key already exists");
                }
                else
                {
                    cache.Add(key, value);
                    logger.log(Severity.debug, "Add key " + key + " to cache");
                }
            }
        }
        public virtual object GetItem(string key, bool remove = false)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                lock (padlock)
                {
                    var res = cache[key];
                    if (res != null)
                    {
                        logger.log(Severity.debug, "Value found by key " + key);
                        if (remove == true)
                            cache.Remove(key);
                    }
                    else
                    {
                        logger.log(Severity.warning, "Does not contain the key!");
                    }
                    return res;
                }
            }
        }

    }
}

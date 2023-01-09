// -----------------------------------------------------------------------
//  <copyright file="CacheManager.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2011/09/23</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Utils.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    ///<summary>
    /// A class to help manage cached items
    ///</summary>
    public static class CacheManager
    {
        #region Fields

        private static int _timeToLive = 300;

        private static readonly IDictionary<string, CachedItem> CacheDictionary;

        #endregion

        #region Properties

        ///<summary>
        /// Gets or sets the max amount of time in seconds a object can live in cache
        ///</summary>
        /// <remarks>
        /// The default value is 5 minutes (300 seconds)
        /// </remarks>
        public static int TimeToLive
        {
            get { return _timeToLive; }
            set { _timeToLive = value; }
        }

        /// <summary>
        /// The number of cached items
        /// </summary>
        public static int CachedItems
        {
            get { return CacheDictionary.Count; }
        }

        /// <summary>
        /// Number of cache misses
        /// </summary>
        public static int Misses { get; private set; }

        /// <summary>
        /// Number of cache hits
        /// </summary>
        public static int Hits { get; private set; }

        /// <summary>
        /// Kind of algorythm to use when caching items
        /// </summary>
        public static CacheMode CacheMode { get; set; }

        #endregion

        #region Constructors

        static CacheManager()
        {
            CacheDictionary = new Dictionary<string, CachedItem>();
        } 

        #endregion

        #region Methods

        ///<summary>
        /// Sets or replaces an item in cache
        ///</summary>
        ///<param name="cacheKey">the unique key to identify the item in the cache</param>
        ///<param name="item">the item to cache</param>
        public static void SetItem(string cacheKey, object item)
        {
            var cacheItem = new CachedItem(item, TimeToLive);
            lock (CacheDictionary)
            {
                CacheDictionary[cacheKey] = cacheItem;
            }
        }

        ///<summary>
        /// Removes an item from cache
        ///</summary>
        ///<param name="cacheKey">The unique key to identify the item in the cache</param>
        public static void ExpireItem(string cacheKey)
        {
            if (!CacheDictionary.ContainsKey(cacheKey)) return;

            lock (CacheDictionary)
            {
                CacheDictionary.Remove(cacheKey);
            }
        }

        ///<summary>
        /// Sets or replaces an item in cache
        ///</summary>
        ///<param name="cacheKey">the unique key to identify the item in the cache</param>
        ///<param name="item">the item to </param>
        ///<param name="timeToLive">The time for the item to live in cache (in seconds)</param>
        public static void SetItem(string cacheKey, object item, int timeToLive)
        {
            var cacheItem = new CachedItem(item, timeToLive);
            lock (CacheDictionary)
            {
                CacheDictionary[cacheKey] = cacheItem;
            }
        }

        ///<summary>
        /// Gets an item from cache
        ///</summary>
        ///<param name="cacheKey">the cached key</param>
        ///<returns>the item or null if item doesn't exists on cache or it's TimeToLive was exceed</returns>
        public static object GetItem(string cacheKey)
        {
            lock (CacheDictionary)
            {
                CachedItem cacheItem;
                if (CacheDictionary.TryGetValue(cacheKey, out cacheItem))
                {
                    //if item has expired it's removed from memory
                    if (cacheItem.Expired)
                    {
                        CacheDictionary.Remove(cacheKey);
                        Misses++;
                        return null;
                    }
                    Hits++;

                    if (CacheMode == CacheMode.Optimistic) cacheItem.KeepAlive();
                    
                    return cacheItem.Item;
                }
            }
            Misses++;
            return null;
        }

        ///<summary>
        /// Gets an item from cache
        ///</summary>
        ///<param name="cacheKey">the cached key</param>
        ///<returns>the item or null if item doesn't exists on cache or it's TimeToLive was exceed</returns>
        /// <exception cref="InvalidCastException"/>
        public static T GetItem<T>(string cacheKey)
        {
            return (T) GetItem(cacheKey);
        }

        /// <summary>
        /// Realeases expired cached items
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void Invalidate()
        {
            lock (CacheDictionary)
            {
                IList<string> expiredItems = (from p in CacheDictionary
                                              where p.Value.Expired
                                              select p.Key).ToList();

                foreach (var expiredItem in expiredItems)
                {
                    CacheDictionary.Remove(expiredItem);
                }
            }
        }

        /// <summary>
        /// Realeases all cached items
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void Clear()
        {
            lock (CacheDictionary)
            {
                CacheDictionary.Clear();
            }
        }

        #endregion
    }

    /// <summary>
    /// Kind of algorythm to use when caching items
    /// </summary>
    public enum CacheMode
    {
        /// <summary>
        /// Items are kept in cache for a period of time counting from when they were added to cache
        /// </summary>
        Normal,
        /// <summary>
        /// Items are kept in cache for a period of time counting from when the last hit ocurred or when added to cache
        /// </summary>
        Optimistic
    }
}
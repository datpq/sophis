// -----------------------------------------------------------------------
//  <copyright file="CachedItem.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2011/09/23</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Utils.Cache
{
    using System;

    internal class CachedItem
    {
        #region Properties

        private int _timeToLive;

        protected int TimeToLive
        {
            get { return _timeToLive / 1000; }
            set { _timeToLive = value * 1000; }
        }

        public object Item { get; private set; }
        public DateTime Registered { get; private set; }
        public DateTime ValidUntil { get { return Registered.AddMilliseconds(_timeToLive); } }
        public bool Expired
        {
            get { return _timeToLive != 0 && DateTime.UtcNow.CompareTo(ValidUntil) > 0; }
        }

        #endregion

        #region Constructors

        public CachedItem(object item, int timeToLive)
        {
            Item = item;
            TimeToLive = timeToLive;
            Registered = DateTime.UtcNow;
        }

        #endregion

        #region Methods

        internal void KeepAlive()
        {
            Registered = DateTime.Now;
        }

        #endregion
    }
}
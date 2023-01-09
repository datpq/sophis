// -----------------------------------------------------------------------
//  <copyright file="AbtractVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments.Impl
{
    public abstract class AbtractVisitor<T>
    {
        private T _host;

        protected AbtractVisitor(T instrument)
        {
            SetHost(instrument);
        }

        public T Host
        {
            get { return _host; }
        }

        protected void SetHost(T instrument)
        {
            _host = instrument;
        }
    }
}
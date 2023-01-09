// -----------------------------------------------------------------------
//  <copyright file="CurrencyVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData.Impl
{
    using System;
    using Extensions;
    using Instruments.Impl;
    using sophis.static_data;
    using sophis.utils;
    using StaticData;

    public class CurrencyVisitor : AbtractVisitor<CSMCurrency>, ICurrencyVisitor
    {
        #region Constructors

        public CurrencyVisitor(CSMCurrency currency) : base(currency)
        {
        }

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                var str = new CMString();
                Currency.GetName(str);
                return str;
            }
        }

        public string Code
        {
            get
            {
                var str = new CMString();
                CSMCurrency.CurrencyToString(Id, str);
                return str;
            }
        }

        public int Id
        {
            get { return Currency.GetIdent(); }
        }

        public int Version
        {
            get { return Currency.GetVersion(); }
        }

        public short DecimalsForRoundingMethod
        {
            get { return Currency.GetNbOfDecimalsForRoundingMethod(); }
        }

        public short Quotity
        {
            get { return Currency.GetQuotity(); }
        }

        public double Spot
        {
            get { return Currency.GetSpot(); }
        }

        protected CSMCurrency Currency { get { return Host; } }

        #endregion

        #region Methods

        public double GetRoundedAmount(double amount)
        {
            return Currency.GetRoundedAmount(amount);
        }

        public double GetHistoryLast(DateTime date)
        {
            return Currency.GetHistoryLast(date.ToSophisDate());
        }

        public double GetForexHistory(DateTime date)
        {
            return Currency.GetForexHistory(date.ToSophisDate());
        }

        public DateTime MatchingBusinessDay(DateTime date, eMHolidayAdjustmentType holidayAdjustmentType)
        {
            return Currency.MatchingBusinessDay(date.ToSophisDate(), holidayAdjustmentType).ToDateTime();
        }

        #endregion
    }
}
// -----------------------------------------------------------------------
//  <copyright file="InstrumentVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments.Impl
{
    using System;
    using Enums;
    using Extensions;
    using NSREnums;
    using sophis.instrument;
    using sophis.market_data;
    using sophis.utils;

    public abstract class InstrumentVisitor<T> : AbtractVisitor<T>, IBaseInstrumentVisitor where T : CSMInstrument
    {
        #region Fields

        private CSMMarketData _marketData;

        #endregion

        #region Constructors

        protected InstrumentVisitor(CSMInstrument instrument):base(null)
        {
            SetInstrument(instrument);
        }

        #endregion

        #region Properties

        public int Sicovam
        {
            get { return Host.GetCode(); }
        }

        public string Name
        {
            get
            {
                var str = new CMString();
                Host.GetName(str);
                return str;
            }

            set { Host.SetName(value); }
        }

        public CSMMarketData MarketData
        {
            get
            {
                return _marketData ?? CSMMarketData.GetCurrentMarketData();
            }
            set { _marketData = value; }
        }

        public double LastSpot
        {
            get
            {
                return MarketData.GetSpot(Sicovam);
            }
        }

        public double Theoretical {
            get { return Host.GetTheoreticalValue(); }
            set { Host.SetTheoreticalValue(value); }
        }

        public double ManagementTheoretical
        {
            get { return Host.GetManagementTheoreticalValue(); }
            set { Host.SetManagementTheoreticalValue(value); }
        }

        public double Theta
        {
            get { return Host.GetTheta(); }
            set { Host.SetTheta(value); }
        }

        public double ValueSecondLeg
        {
            get { return Host.GetValueSecondLeg(); }
            set { Host.SetValueSecondLeg(value); }
        }

        public double AccruedCoupon
        {
            get { return Host.GetAccruedCoupon(); }
            set { Host.SetAccruedCoupon(value); }
        }

        public double FloatingNotionalFactor
        {
            get { return Host.GetFloatingNotionalFactor(); }
            set { Host.SetFloatingNotionalFactor(value); }
        }

        public double Ytm
        {
            get { return Host.GetYTM(); }
            set { Host.SetYTM(value); }
        }

        public double YtmMtoM
        {
            get { return Host.GetYTMMtoM(); }
            set { Host.SetYTMMtoM(value); }
        }

        public double Duration
        {
            get { return Host.GetDuration(); }
            set { Host.SetDuration(value); }
        }

        public double ModDuration
        {
            get { return Host.GetModDuration(); }
            set { Host.SetModDuration(value); }
        }

        public double CleanPrice
        {
            get { return Host.GetCleanPrice(); }
            set { Host.SetCleanPrice(value); }
        }

        public double CreditRiskSensitivity
        {
            get { return Host.GetCreditRiskSensitivity(); }
            set { Host.SetCreditRiskSensitivity(value); }
        }

        public double CreditRiskConvexity
        {
            get { return Host.GetCreditRiskConvexity(); }
            set { Host.SetCreditRiskConvexity(value); }
        }

        public double RecoveryRateSensitivity
        {
            get { return Host.GetRecoveryRateSensitivity(); }
            set { Host.SetRecoveryRateSensitivity(value); }
        }

        public double YtmSensitivity
        {
            get { return Host.GetYTMSensitivity(); }
            set { Host.SetYTMSensitivity(value); }
        }

        public double BasketDelta
        {
            get { return Host.GetBasketDelta(); }
            set { Host.SetBasketDelta(value); }
        }

        public double BasketGamma
        {
            get { return Host.GetBasketGamma(); }
            set { Host.SetBasketGamma(value); }
        }

        public double BasketVega
        {
            get { return Host.GetBasketVega(); }
            set { Host.SetBasketVega(value); }
        }

        public double BasketTotalVega
        {
            get { return Host.GetBasketTotalVega(); }
            set { Host.SetBasketTotalVega(value); }
        }

        public double EquityDelta
        {
            get { return Host.GetEquityDelta(); }
            set { Host.SetEquityDelta(value); }
        }

        public double EquityThetaSpot
        {
            get { return Host.GetEquityThetaSpot(); }
            set { Host.SetEquityThetaSpot(value); }
        }

        public bool ShortPosition
        {
            get { return Host.GetShortPosition(); }
            set { Host.SetShortPosition(value); }
        }

        public int Seniority
        {
            get { return Host.GetSeniority(); }
            set { Host.SetSeniority(value); }
        }

        public int Issuer
        {
            get { return Host.GetIssuerCode(); }
            set { Host.SetIssuerCode(value); }
        }

        public int Currency
        {
            get { return Host.GetCurrency(); }
            set { Host.SetCurrency(value); }
        }

        public string Reference
        {
            get { return Host.GetReference(); }
            set { Host.SetReference(value); }
        }

        public DateTime IssueDate
        {
            get { return Host.GetIssueDate().ToDateTime(); }
        }

        #endregion

        #region Methods

        public void SetFirstDerivative(int which, double value)
        {
            Host.SetFirstDerivative(which, value);
        }

        public void SetFirstDerivativeUndiscounted(int which, double value)
        {
            Host.SetFirstDerivativeUndiscounted(which, value);
        }

        public void SetEpsilon(int which, double value)
        {
            Host.SetEpsilon(which, value);
        }

        public void SetVega(int which, double value)
        {
            Host.SetVega(which, value);
        }

        public void SetCrossedVega(int which1, int which2, double value)
        {
            Host.SetCrossedVega(which1, which2, value);
        }

        public void SetRho(int which, double value)
        {
            Host.SetRho(which, value);
        }

        public void SetConvexity(int which, double value)
        {
            Host.SetConvexity(which, value);
        }

        public void SetCrossedDeltaRho(int whichUnderlying, int whichCurrency, double value)
        {
            Host.SetCrossedDeltaRho(whichUnderlying, whichCurrency, value);
        }

        public void SetSeniority(string seniority)
        {
            Host.SetSeniority(seniority);
        }

        public void Save(InstrumentParameterModificationType type)
        {
            Host.Save((eMParameterModificationType)type);
        }

        protected abstract T GetInstance(CSMInstrument instrument);

        private void SetInstrument(CSMInstrument instrument)
        {
            SetHost(GetInstance(instrument));
        }

        #endregion
    }
}
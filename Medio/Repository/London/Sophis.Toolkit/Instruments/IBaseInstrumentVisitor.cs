// -----------------------------------------------------------------------
//  <copyright file="IBaseInstrumentVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments
{
    using Enums;

    public interface IBaseInstrumentVisitor : IInstrumentVisitor
    {
        #region Properties

        double LastSpot { get; }
        double Theoretical { get; set; }
        double ManagementTheoretical { get; set; }
        double Theta { get; set; }
        double ValueSecondLeg { get; set; }
        double AccruedCoupon { get; set; }
        double FloatingNotionalFactor { get; set; }
        double Ytm { get; set; }
        double YtmMtoM { get; set; }
        double Duration { get; set; }
        double ModDuration { get; set; }
        double CleanPrice { get; set; }
        double CreditRiskSensitivity { get; set; }
        double CreditRiskConvexity { get; set; }
        double RecoveryRateSensitivity { get; set; }
        double YtmSensitivity { get; set; }
        //Properties specific to some children
        double BasketDelta { get; set; }
        double BasketGamma { get; set; }
        double BasketVega { get; set; }
        double BasketTotalVega { get; set; }
        double EquityDelta { get; set; }
        double EquityThetaSpot { get; set; }
        bool ShortPosition { get; set; }
        int Seniority { get; set; }
        int Issuer { get; set; }

        #endregion

        #region Methods

        //Set Methods for Greeks arrays
        void SetFirstDerivative(int which, double value);
        void SetFirstDerivativeUndiscounted(int which, double value);
        void SetEpsilon(int which, double value);
        void SetVega(int which, double value);
        void SetCrossedVega(int which1, int which2, double value);
        void SetRho(int which, double value);
        void SetConvexity(int which, double value);
        void SetCrossedDeltaRho(int whichUnderlying, int whichCurrency, double value);
        void SetSeniority(string seniority);

        void Save(InstrumentParameterModificationType type);

        #endregion
    }
}
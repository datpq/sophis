// -----------------------------------------------------------------------
//  <copyright file="BondVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments.Impl
{
    using sophis.instrument;

    public class BondVisitor : InstrumentVisitor<CSMBond>, IBondVisitor
    {
        #region Constructors

        public BondVisitor(CSMInstrument instrument)
            : base(instrument)
        {
        }

        #endregion

        #region Properties

        public eMSpreadType SpreadType
        {
            get { return Host.GetSpreadType(); }
            set { Host.SetSpreadType(value); }
        }

        public double Spread
        {
            get { return Host.GetSpread(); }
            set { Host.SetSpread(value); }
        }

        public bool UseMetaModel
        {
            get { return Host.UseMetaModel(); }
        }

        #endregion

        #region Methods

        public void RecalculateRedemptions()
        {
            Host.RegenerateRedemption();
        }

        protected override CSMBond GetInstance(CSMInstrument instrument)
        {
            CSMBond bond = instrument;
            return bond;
        }

        #endregion
    }
}
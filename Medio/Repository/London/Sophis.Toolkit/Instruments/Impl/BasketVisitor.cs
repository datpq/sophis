// -----------------------------------------------------------------------
//  <copyright file="BasketVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/02/07</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments.Impl
{
    using System.Collections.Generic;
    using System.IO;
    using StaticData;
    using StaticData.Impl;
    using sophis.instrument;

    public class BasketVisitor : InstrumentVisitor<CSMEquity>, IBasketVisitor
    {
        private readonly ISophisFactory _sophisFactory;

        public BasketVisitor(CSMInstrument instrument) : base(instrument)
        {
            _sophisFactory = SophisContainer.Resolve<ISophisFactory>();
        }

        public int ComponentCount
        {
            get { return Host.GetBasketComponentCount(); }
        }

        public IEnumerable<IEquityVisitor> Components
        {
            get
            {
                for (var i = 0; i < ComponentCount; i++)
                {
                    var basket = new SSMBasket();
                    if (!Host.GetNthBasketComponent(i, basket)) continue;
                    var v = _sophisFactory.GetInstrument(basket.code) as IEquityVisitor;
                    if (v == null) continue;

                    yield return v;
                }
            }
        }

        protected override CSMEquity GetInstance(CSMInstrument instrument)
        {
            CSMEquity equity = instrument;

            if (!equity.IsABasket())
                throw new InvalidDataException(string.Format("Instrument '{0}' is not a Basket/Index", equity.GetReference()));

            return equity;
        }
    }
}
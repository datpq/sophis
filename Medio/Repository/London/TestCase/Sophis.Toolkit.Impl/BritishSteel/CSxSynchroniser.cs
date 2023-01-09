// -----------------------------------------------------------------------
//  <copyright file="CSxSynchroniser.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Oscar Tercan</author>
//  <created>2013/01/21</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.BritishSteel
{
    using System;
    using System.Reflection;
    using Data;
    using Instruments;
    using Model;
    using StaticData;
    using Utils.Log;

    public class CSxSynchroniser
    {
        private readonly int _globalCurreny;
        private readonly ISophisFactory _sophisFactory;
        private readonly IDataProvider _dataProvider;

        public CSxSynchroniser(ISophisFactory sophisFactory, IDataProvider dataProvider)
        {
            //_sophisFactory = SophisContainer.Resolve<ISophisFactory>();
            //_dataProvider = SophisContainer.Resolve<IDataProvider>();
            _sophisFactory = sophisFactory;
            _dataProvider = dataProvider;
            _globalCurreny = _sophisFactory.StringToCurrency("GBP");
        }

        public void UpdateBenchmarkWeight(DateTime date)
        {
            using (var log = LogHelper.GetLogger(GetType().Name, MethodBase.GetCurrentMethod().Name))
            {
                var listIndexToBench = _dataProvider.LoadIndexToBenchmarkMappings();

                foreach (var pair in listIndexToBench)
                {
                    var benchmarkInst = _sophisFactory.GetInstrument(pair.Sicovam) as IBenchmarkVisitor;
                    if (benchmarkInst == null)
                    {
                        log.WriteWarning("Benchmark not found : {0}", pair.Sicovam);
                        continue;
                    }
                    var indexInst = _sophisFactory.GetInstrument(pair.Index) as IBasketVisitor;
                    if (indexInst == null)
                    {
                        log.WriteWarning("Index not found : {0}", pair.Sicovam);
                        continue;
                    }

                    var activeDate = benchmarkInst.ActiveRecordDate;
                    var bmComp = benchmarkInst.GetComposition(activeDate);
                    bmComp.Clear();

                    foreach (var component in indexInst.Components)
                    {
                        var weight = CalculateWeight(component.Sicovam, date, pair.IsFloat);
                        bmComp.Rows.Add(new Object[] { component.Reference, component.Name, weight, 0, 0, "Instrument", component.Sicovam, "0.29992604742769" });
                    }

                    benchmarkInst.SetComposition(bmComp, date);
                    benchmarkInst.Save();
                }
            }
        }

        private double CalculateWeight(int sicovam, DateTime date, bool isFloat)
        {
            var equity = _sophisFactory.GetInstrument(sicovam);
            var equityHistoric = _dataProvider.LoadEquityHistoric(sicovam, date);
            var currency = equity.Currency;
            var fx = 1.0;

            if (currency != _globalCurreny)
            {
                fx = _sophisFactory.GetCurrentMarketData().GetForex(currency, _globalCurreny);
            }

            if (isFloat)
                return CalculateWeightWithFloat(equityHistoric) * fx;

            return CalculateWeightWithoutFloat(equityHistoric) * fx;
        }

        private double CalculateWeightWithoutFloat(EquityHistoric historic)
        {
            decimal weight;

            if (historic.Ftsnosh > 0)
                weight = historic.Ftsnosh * historic.Last; //*Fx(currency, date);
            else
                weight = historic.Ftnosh * 1000000 * historic.Last; // *Fx(currency, date);

            return (double)weight;
        }

        private double CalculateWeightWithFloat(EquityHistoric historic)
        {
            decimal weight;

            if (historic.Ftsnosh > 0)
                weight = historic.Ftsnosh * historic.Last * historic.Ftsmcf; //*Fx(currency, date);
            else
                weight = historic.Ftnosh * 1000000 * historic.Last * historic.Ftsmcf; // *Fx(currency, date);

            return (double)weight;
        }
    }
}
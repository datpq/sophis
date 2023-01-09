// -----------------------------------------------------------------------
//  <copyright file="CSxSyncScenario.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/29</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.BritishSteel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Forms;
    using StaticData.Impl;
    using UI;
    using Utils.Log;
    using sophis.scenario;

    /// <summary>
    /// This class derived from <c>sophis.portfolio.CSMScenario</c> can be overloaded to create a new scenario
    /// </summary>
    public class CSxSyncScenario : CSMScenario
    {
        private DateTime _date;
        private bool _userCancelled;

        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pManagerPreference;
        }

        /// <summary>To do all your initialisation. Typically, it may open a GUI to get data from the user.</summary>
        public override void Initialise()
        {
            if (IsBatch())
            {
                _date = DateTime.ParseExact(fParam, "ddMMyyyy", CultureInfo.CurrentCulture);
            }
            else
            {
                var dialog = new CSxSyncParamDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _date = dialog.BenchmarkDate;
                }
                else
                {
                    _userCancelled = true;
                }
            }
#if DEBUG
            Debugger.Launch();
#endif
        }

        /// <summary>To run your scenario. this method is mandatory otherwise RISQUE will not do anything.</summary>
        public override void Run()
        {
            if (_userCancelled) return;

            try
            {
                var synchroniser = SophisContainer.Resolve<CSxSynchroniser>();
                synchroniser.UpdateBenchmarkWeight(_date);

                MessageBox.Show("Benchmark weights updated");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An error occurred while updating Benchmark weights.\n{0}", ex.Message));
            }
        }

        /// <summary>Free initiliased memory after scenario is processed.</summary>
        public override void Done()
        {
            //Add your code here
        }
    }
}
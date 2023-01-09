// -----------------------------------------------------------------------
//  <copyright file="CSxSyncParamDialog.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/21</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.BritishSteel.UI
{
    using System;
    using System.Windows.Forms;

    public partial class CSxSyncParamDialog : Form
    {
        public CSxSyncParamDialog()
        {
            InitializeComponent();

            BenchmarkDate = DateTime.Today.AddDays(-1);
        }

        public DateTime BenchmarkDate
        {
            get { return dateTimePicker1.Value; }
            set { dateTimePicker1.Value = value; }
        }
    }
}
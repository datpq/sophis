// -----------------------------------------------------------------------
//  <copyright file="IInstrumentVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments
{
    public interface IInstrumentVisitor
    {
        #region Properties

        int Sicovam { get; }
        string Name { get; set; }
        int Currency { get; set; }
        string Reference { get; set; }

        #endregion

        #region Methods


        #endregion
    }
}
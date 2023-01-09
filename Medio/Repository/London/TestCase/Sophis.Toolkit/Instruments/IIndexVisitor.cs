namespace Sophis.Toolkit.Instruments
{
    using System.Collections.Generic;

    public interface IBasketVisitor : IInstrumentVisitor
    {     
        #region Properties

        int ComponentCount { get; }

        IEnumerable<IEquityVisitor> Components { get; }

        #endregion

        #region Methods

    
        #endregion
    }
}
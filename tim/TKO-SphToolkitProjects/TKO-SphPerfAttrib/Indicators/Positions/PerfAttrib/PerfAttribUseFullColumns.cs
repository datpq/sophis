using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.portfolio;


namespace TkoPortfolioColumn
{
    public static class PerfAttribUseFullColumns
    {
        public static double TkoPerfAttribFindPositionFolio(this CSMPosition position, InputProvider input)
        {
            input.StringIndicatorValue = input.PortFolioName;
            return 0;
        }
    }
}

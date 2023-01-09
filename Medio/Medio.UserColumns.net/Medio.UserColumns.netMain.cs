using System;

using sophis;
using sophis.utils;
using Medio.UserColumns;
using MEDIO.UserColumns.NET.Source.Column;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace Medio.UserColumns
{
    /// <summary>
    /// Definition of DLL entry point: to register new functionality and closing point
    /// </summary>
    public class MainClass : IMain
    {
        public void EntryPoint()
        {
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            // TO DO; Perform registrations
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Nominal 1st CCY", new CSxNominal1stCcy());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Nominal 2nd CCY", new CSxNominal2ndCcy());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk CCY TYPE", new CSxCcyType());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk RBC Fund Nav", new CSxRBCFundNav());       
            sophis.portfolio.CSMPortfolioColumn.Register("Tk FolioId", new CSxFolioId());               // duplicate of CSxOpcvm
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Opcvm", new CSxOpcvm());                   // duplicate of CSxFolioId
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Delta In Percent", new CSxDeltaInPercent());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Gamma In Percent", new CSxGammaInPercent());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Delta Cash", new CSxDeltaCash());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Gamma Cash", new CSxGammaCash());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Last Date", new CSxLastDate());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Last Minus 1", new CSxLastMinus1());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Last Minus 2", new CSxLastMinus2());
            sophis.portfolio.CSMPortfolioColumn.Register("Tk Is Market Way", new IsMarketWay());

            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {

        }
    }

}

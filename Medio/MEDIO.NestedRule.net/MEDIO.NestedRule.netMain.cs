using System;

using sophis;
using sophis.utils;
using MEDIO.NestedRule.net;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace MEDIO.NestedRule.net
{
    /// <summary>
    /// Definition of DLL entry point, registrations, and closing point
    /// </summary>
    public class MainClass : IMain
    {
        public void EntryPoint()
        {
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            // TO DO; Perform registrations
            sophis.portfolio.CSMPortfolioColumn.Register("Compliance - Biggest Issuer Name", new CSxNestedRuleColumn(1));
            sophis.portfolio.CSMPortfolioColumn.Register("Compliance - Biggest Issuer  Weight", new CSxNestedRuleColumn(2));
            sophis.portfolio.CSMPortfolioColumn.Register("Compliance - Asset Count", new CSxNestedRuleColumn(3));
            sophis.portfolio.CSMPortfolioColumn.Register("Compliance - Nested Rule", new CSxNestedRuleColumn(4));
            sophis.portfolio.CSMPortfolioColumn.Register("Compliance - Nested Rule Instrument by Issuer", new CSxNestedRuleColumn(5));
            sophis.portfolio.CSMPortfolioColumn.Register("Medio - AggregFirst ", new CSxLevlFirstAgregg());
			sophis.portfolio.CSMPortfolioColumn.Register("MEDIO_AggregFirst_Target ", new CSxAggregFirstTarget());
            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}
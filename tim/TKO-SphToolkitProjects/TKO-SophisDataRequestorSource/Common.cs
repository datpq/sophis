using System;
using System.Collections.Generic;
using System.Linq;

namespace TkoPortfolioColumn
{

    public interface IInputProvider
    {
        string SophisReportingDate    { get; set; }

        string MarketDataDate         { get; set; }

        string Column                 { get; set; }

        string PortFolioName          { get; set; }

        int PositionReference         { get; set; }

        string InstrumentReference    { get; set; }

        string InstrumentType         { get; set; }

        double IndicatorValue         { get; set; }

        double Rho                    { get; set; }

        double Yield                  { get; set; }

        double Delta                  { get; set; }

        double Volatility             { get; set; }

        double NumberOfSecurities     { get; set; }

        double ContractSize           { get; set; }

        double UnderLyingLast         { get; set; }

        double ConvertionRatio        { get; set; }

        double Notional               { get; set; }

        double Strike                 { get; set; }

        double Nominal                { get; set; }

        string StringIndicatorValue   { get; set; }

        Dictionary<string,string> AllOtherFieldInfos { get;}

        Dictionary<int, TkoPortfolioColumn.DbRequester.DbrPerfAttribMapping.TIKEHAU_PERFATTRIB_MAPPING> PerfAttribMappingConfigDic { get;set ;}
     }
}


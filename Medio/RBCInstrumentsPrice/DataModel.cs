using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RBCInstrumentsPrice
{
    public class DataModel
    {
        public static Dictionary<string, ColumnsIndex> fieldsMap;
        public static Dictionary<string, InstrumentInfo> instrFromFiles = new Dictionary<string, InstrumentInfo>();
        public static Dictionary<string, InstrumentInfo> matchedInstruments = new Dictionary<string, InstrumentInfo>();

        public static Dictionary<string, DBInfo> instrInScope = new Dictionary<string, DBInfo>();


        public static Dictionary<string, List<string>> fileAllotments;


        public static void LoadFileAllotments()//loading allotments in scope per file
        {
            fileAllotments = new Dictionary<string, List<string>>();

            List<string> cuvalAllotments = new List<string>(RBCConfigurationSectionGroup.AllotSectionConfig.AllotmentsCUVAL.Split(';'));
            List<string> swapsAllotments = new List<string>(RBCConfigurationSectionGroup.AllotSectionConfig.AllotmentsSWAPS.Split(';'));
            List<string> optionsAllotments = new List<string>(RBCConfigurationSectionGroup.AllotSectionConfig.AllotmentsOPTIONS.Split(';'));
            List<string> futuresAllotments = new List<string>(RBCConfigurationSectionGroup.AllotSectionConfig.AllotmentsFUTURES.Split(';'));

            List<string> allotmentsMultiply = new List<string>(RBCConfigurationSectionGroup.AllotSectionConfig.AllotmentsPriceMultiply.Split(';'));
            List<string> allotmentsDivide = new List<string>(RBCConfigurationSectionGroup.AllotSectionConfig.AllotmentsPriceDivide.Split(';'));

            fileAllotments.Add("CUVAL", cuvalAllotments);
            fileAllotments.Add("SWAPS", swapsAllotments);
            fileAllotments.Add("OPTIONS", optionsAllotments);
            fileAllotments.Add("FUTURES", futuresAllotments);

            fileAllotments.Add("multiply", allotmentsMultiply);
            fileAllotments.Add("divide", allotmentsDivide);


        }

        public static void LoadFileMappings()//loading column indexes per file
        {

            fieldsMap = new Dictionary<string, ColumnsIndex>();

            //POSITIONS_CUVAL

            ColumnsIndex cuval_Indexes = new ColumnsIndex();
            cuval_Indexes.InstrumentIdentType = "ISIN";
            cuval_Indexes.InstrumentIdentifier = 24;
            cuval_Indexes.PriceField = 43;
            cuval_Indexes.Currency = 13;
            cuval_Indexes.NavDate = 0;

            fieldsMap.Add("CUVAL", cuval_Indexes);

            //POSITIONS_FUTURES

            ColumnsIndex futures_Indexes = new ColumnsIndex();
            futures_Indexes.InstrumentIdentType = "TICKER";
            futures_Indexes.InstrumentIdentifier = 39;
            futures_Indexes.PriceField = 8;
            futures_Indexes.Currency = 7;
            futures_Indexes.NavDate = 4;

            fieldsMap.Add("FUTURES", futures_Indexes);


            //POSITIONS_SWAPS

            ColumnsIndex swaps_Indexes = new ColumnsIndex();
            swaps_Indexes.InstrumentIdentType = "SICOVAM";
            swaps_Indexes.InstrumentIdentifier = 32;
            swaps_Indexes.PriceField = 17;
            swaps_Indexes.Currency = 12;
            swaps_Indexes.NavDate = 3;

            fieldsMap.Add("SWAPS", swaps_Indexes);


            //POSITIONS_OPTIONS 
            ColumnsIndex option_Indexes = new ColumnsIndex();
            option_Indexes.InstrumentIdentType = "SICOVAM/BBG";
            option_Indexes.InstrumentIdentifier = 28;
            option_Indexes.InstrumentIdentBBG = 27;
            option_Indexes.PriceField = 13;
            option_Indexes.Currency = 8;
            option_Indexes.NavDate = 5;

            fieldsMap.Add("OPTIONS", option_Indexes);
        }
    }

    public class KeyTkt
    {
        public int NavDate;
        public string InstrumentIdentType;
        public int IntrumentIdentifier;
        public int PriceField;
        public int Currency;
    }

    public class ColumnsIndex
    {
        public int NavDate;
        public string InstrumentIdentType;
        public int InstrumentIdentifier;
        public int InstrumentIdentBBG;
        public int PriceField;
        public int Currency;
    }

    public class InstrumentInfo
    {
        public string NavDate;
        public string InstrumentIdentType;
        public string InstrumentIdent;
        public double Price;
        public string Ccy;
    }

    public class DBInfo
    {
        public string InstrumentSicovam;
        public string InstrumentIsin;
        public string Ccy;
        public string Allotment;
        public string InstrumentName;
        public string InstrumentBBGRef;
        public string InstrumentTickerRef;
    }
}

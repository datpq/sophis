using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.portfolio;
using System.Collections;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using TkoPortfolioColumn.DataCache;
using System.Collections.Generic;
using sophis.static_data;
using TkoPortfolioColumn.DbRequester;
using System.ComponentModel;
using System.Linq;

namespace TkoPortfolioColumn
{

    #region Enum
    public enum TkoEnumCreditInstrumentType { OTHERS = 0, FIXED, FLOAT, FIXEDTOFLOAT, CDS, AllotmentsNulIRDuration };
    #endregion

    #region IInputProvider

    public class InputProvider : IInputProvider
    {
        #region Inner Class Attribute

        //private CSMPortfolio _portFolio;
        public CSMPortfolio PortFolio
        {
            //get { return _portFolio; }
            //set { _portFolio = value; }
            //get
            //{
            //    return CSMPortfolio.GetCSRPortfolio(PortFolioCode, Extraction) ?? CSMPortfolio.GetCSRPortfolio(PortFolioCode);
            //}
            set
            {
                if (value != null) PortFolioCode = value.GetCode();
            }
        }

        private CSMPosition _position;
        public CSMPosition Position
        {
            get { return _position; }
            set { _position = value; }
        }

        //private CSMInstrument _instrument;
        public CSMInstrument Instrument
        {
            //get { return _instrument; }
            //set { _instrument = value; }
            get
            {
                return CSMInstrument.GetInstance(InstrumentCode);
            }
            set
            {
                if (value != null) InstrumentCode = value.GetCode();
            }
        }

        //private CSMExtraction _extraction;
        private int extractionId;
        public CSMExtraction Extraction
        {
            //get { return _extraction; }
            //set { _extraction = value; }
            get
            {
                return CSMExtraction.CreateInstance(extractionId);
            }
            set
            {
                if (value != null) extractionId = value.GetID();
            }
        }

        private string _column;
        public string Column
        {
            get { return _column; }
            set { _column = value; }
        }

        private int _reportingDate;
        public int ReportingDate
        {
            get { return _reportingDate; }
            set { _reportingDate = value; }
        }

        private string _sophisReportingDate;
        public string SophisReportingDate
        {
            get { return _sophisReportingDate; }
            set { _sophisReportingDate = value; }
        }

        private sophis.instrument.eMPositionType _positionType;
        public sophis.instrument.eMPositionType PositionType
        {
            get { return _positionType; }
            set { _positionType = value; }
        }

        private bool _onlyTheValue;
        public bool OnlyTheValue
        {
            get { return _onlyTheValue; }
            set { _onlyTheValue = value; }
        }

        private double _indicatorValue;
        public double IndicatorValue
        {
            get { return _indicatorValue; }
            set { _indicatorValue = value; }
        }

        private string _stringindicatorValue;
        public string StringIndicatorValue
        {
            get { return _stringindicatorValue; }
            set { _stringindicatorValue = value; }
        }

        private double _yield;
        public double Yield
        {
            get { return _yield; }
            set { _yield = value; }
        }

        private int _workoutDate;
        public int WorkoutDate
        {
            get { return _workoutDate; }
            set { _workoutDate = value; }
        }

        private string _tmpPortfolioColName;
        public string TmpPortfolioColName
        {
            get { return _tmpPortfolioColName; }
            set { _tmpPortfolioColName = value; }
        }

        private string _methods;
        public string Methods
        {
            get { return _methods; }
            set { _methods = value; }
        }

        public double _rho;
        public double Rho
        {
            get { return _rho; }
            set { _rho = value; }
        }

        private double _volatility;
        public double Volatility
        {
            get { return _volatility; }
            set { _volatility = value; }
        }

        private string _portfolioName;
        public string PortFolioName
        {
            get { return _portfolioName; }
            set { _portfolioName = value; }
        }

        private int _activePortFolioCode;
        public int ActivePortfolioCode
        {
            get { return _activePortFolioCode; }
            set { _activePortFolioCode = value; }
        }

        private int _portFolioCode;
        public int PortFolioCode
        {
            get { return _portFolioCode; }
            set { _portFolioCode = value; }
        }

        private int _underlyingCode;
        public int UnderlyingCode
        {
            get { return _underlyingCode; }
            set { _underlyingCode = value; }
        }

        private int _instrumentCode;
        public int InstrumentCode
        {
            get { return _instrumentCode; }
            set { _instrumentCode = value; }
        }

        private int _positionIdentifier;
        public int PositionIdentifier
        {
            get { return _positionIdentifier; }
            set { _positionIdentifier = value; }
        }

        public Dictionary<int, TkoPortfolioColumn.DbRequester.DbrPerfAttribMapping.TIKEHAU_PERFATTRIB_MAPPING> PerfAttribMappingConfigDic { get; set; }

        public TkoPortfolioColumn.CallBack.PortFolioColumnCallbacker.SophisPortfolioConsolidation delegateFolioConsolidation;

        #endregion

        public InputProvider(int activePortfolioCode, int portfolioCode,
                              CSMExtraction extraction, int underlyingCode,
                              int instrumentCode, sophis.instrument.eMPositionType positionType,
                              int positionIdentifier, bool onlyTheValue, string folioName, double rho, string column)
        {
            PortFolioName = folioName;
            PortFolioCode = portfolioCode;
            UnderlyingCode = underlyingCode;
            InstrumentCode = instrumentCode;
            PositionIdentifier = positionIdentifier;
            OnlyTheValue = onlyTheValue;
            Rho = rho;
            Extraction = extraction;
            PositionType = positionType;
            Column = column;
            //DbrPerfAttribMapping.SetColumnConfig(this);
        }


        public InputProvider(int activePortfolioCode, int portfolioCode, CSMExtraction extraction,
                               bool onlyTheValue, CSMPortfolio portfolio, string folioName, double rho, string column)
        {

            ActivePortfolioCode = activePortfolioCode;
            OnlyTheValue = onlyTheValue;
            PortFolioName = folioName;
            PortFolioCode = portfolioCode;
            Rho = rho;
            this.Extraction = extraction;
            this.PortFolio = portfolio;
            Column = column;
            //DbrPerfAttribMapping.SetColumnConfig(this);
        }

        private int _positionReference;
        public int PositionReference
        {
            get { return _positionReference; }
            set { _positionReference = value; }
        }

        private string _instrumentReference;
        public string InstrumentReference
        {
            get { return _instrumentReference; }
            set { _instrumentReference = value; }
        }

        private double _delta;
        public double Delta
        {
            get { return _delta; }
            set { _delta = value; }
        }

        internal void Reset()
        {
            ActivePortfolioCode = 0;
            PortFolioCode = 0;
            UnderlyingCode = 0;
            InstrumentCode = 0;
            PositionIdentifier = 0;
            Rho = 0;
            Delta = 0;
            PortFolioName = "";
            extractionId = 0;
        }

        private double _numberOfSecurities;
        public double NumberOfSecurities
        {
            get { return _numberOfSecurities; }
            set { _numberOfSecurities = value; }
        }

        private double _contractSize;
        public double ContractSize
        {
            get { return _contractSize; }
            set { _contractSize = value; }
        }

        private double _underlyingLast;
        public double UnderLyingLast
        {
            get { return _underlyingLast; }
            set { _underlyingLast = value; }
        }

        private double _convertionRatio;
        public double ConvertionRatio
        {
            get { return _convertionRatio; }
            set { _convertionRatio = value; }
        }

        private string _instrumentType;
        public string InstrumentType
        {
            get { return _instrumentType; }
            set { _instrumentType = value; }
        }

        private string _marketDataDate;
        public string MarketDataDate
        {
            get { return _marketDataDate; }
            set { _marketDataDate = value; }
        }

        public double _notional;
        public double Notional
        {
            get { return _notional; }
            set { _notional = value; }
        }

        public double _strike;
        public double Strike
        {
            get { return _strike; }
            set { _strike = value; }
        }

        public double _nominal;
        public double Nominal
        {
            get { return _nominal; }
            set { _nominal = value; }
        }

        Dictionary<string, string> _AllOtherFieldInfos;
        public Dictionary<string, string> AllOtherFieldInfos
        {
            get
            {
                if (_AllOtherFieldInfos == null)
                    _AllOtherFieldInfos = new Dictionary<string, string>();
                return _AllOtherFieldInfos;
            }
            set { _AllOtherFieldInfos = value; }
        }

        public string FolioCacheStringKey
        {
            get { return PortFolioCode + "-" + PortFolioName; }
        }

        public InputProvider(InputProvider input)
        {
            SophisReportingDate = input.SophisReportingDate;
            MarketDataDate = input.MarketDataDate;
            Column = input.Column;
            PortFolioName = input.PortFolioName;
            PositionReference = input.PositionReference;
            InstrumentReference = input.InstrumentReference;
            InstrumentType = input.InstrumentType;
            IndicatorValue = input.IndicatorValue;
            PortFolioCode = input.PortFolioCode;
            Rho = input.Rho;
            Yield = input.Yield;
            Delta = input.Delta;
            Volatility = input.Volatility;
            NumberOfSecurities = input.NumberOfSecurities;
            ContractSize = input.ContractSize;
            UnderLyingLast = input.UnderLyingLast;
            ConvertionRatio = input.ConvertionRatio;
            Notional = input.Notional;
            Strike = input.Strike;
            Nominal = input.Nominal;
            StringIndicatorValue = input.StringIndicatorValue;
            Position = input.Position;
            Instrument = input.Instrument;
            Extraction = input.Extraction;

            foreach (var key in input.AllOtherFieldInfos.Keys)
            {
                AllOtherFieldInfos.Add(key, input.AllOtherFieldInfos[key]);
            }
        }

        public InputProviderCache GetInputProviderCache()
        {
            return new InputProviderCache
                {IndicatorValue = IndicatorValue, StringIndicatorValue = StringIndicatorValue};
        }

        public override string ToString()
        {
            return string.Format("PortFolioCode={0}, PositionIdentifier={1}, InstrumentCode={2}, UnderlyingCode={3}, Methods={4}, Extraction={5}",
                PortFolioCode, PositionIdentifier, InstrumentCode, UnderlyingCode, Methods, extractionId);
        }
    }
    #endregion

    public class InputProviderCache
    {
        public double IndicatorValue { get; set; }
        public string StringIndicatorValue { get; set; }
    }

    public static class CommonExtentionMethods
    {
        #region Tikehau YTM Tree Rules

        //méthode qui va déterimner le YTM/YTC de l'instrument suivant ses caractéristiques
        // 0 si aucune possibilité de calcul
        public static int TkoComputeTreeYTM(this CSMInstrument instrument, InputProvider input)
        {
            //Variables de  calcul
            int algosenior = input.Instrument.TkoTestSeniority(input);

            //début de l'algo de selection 
            if (algosenior == -1)//Si l'instrument est dans le cas Sub
            {
                //On récupère la date du jour et celle du premier call
                int txft = instrument.TkoComputeFixedOrFloat(input);
                int today = VersionClass.Get_ReportingDate();
                int date1stcall = instrument.TkoGet1stCallDate(input);
                //Code CMS : 67883897 EIISDA 10 // tec10 index 67905657
                if (instrument.GetInstrumentType() == 'O')
                {
                    CSMBond bond = CSMBond.GetInstance(instrument.GetCode());

                    //bool Cms = (txft == 1 && (bond.GetFloatingRate() == 67883897 || bond.GetFloatingRate() == 67905657 || bond.GetFloatingRate() == 67739540));

                    bool checkFloatingRate = false;
                    var config = DbrTikehau_Config.GetTikehauConfigFromName("CMS-FLOATINGRATE");
                    foreach (var elt in config)
                    {
                        if (elt.VALUE == bond.GetFloatingRate().ToString())
                            checkFloatingRate = true;
                    }

                    bool Cms = (txft == 1 && checkFloatingRate);
                    if (!Cms && (today < date1stcall)) return 2;//Si le bond est tx float alors on retourne 2
                    else return 1;
                }
                else return 1;
            }
            //Si l'instrument est dans le cas Senior
            else if (algosenior == 1)
            {
                int testratevalue = Helper.TestRatingNb(instrument.TkoNotationNum());
                if (testratevalue == 1)
                {
                    int mat = instrument.GetExpiry();

                    if (mat > 52961) return 3;//HY (perpétuelle)
                    else return 4; //IG
                }
                else return 3;//HY
            }
            else return 0; //cas où il y a erreur
        }

        //Renvoie 1 si le prochain coupon est flottant
        public static int TkoComputeFixedOrFloat(this CSMInstrument instrument, InputProvider input)
        {
            int NextCouponIsFloat = 0;
            try
            {

                CSMBond Bond;
                System.Collections.ArrayList RedemptionArray;//Table de CF
                switch (instrument.GetType_API())
                {
                    case 'O':   //Obligation
                        Bond = CSMBond.GetInstance(instrument.GetCode());
                        RedemptionArray = new System.Collections.ArrayList();//Table des CF de l'obligation de la repotingdate à sa maturité
                        SSMRedemption IthRedemption;// ième flux à venir
                        RedemptionArray = instrument.TkoGetBondExplanationArray(input);

                        if (RedemptionArray.Count >= 2)
                        {
                            IthRedemption = (sophis.instrument.SSMRedemption)RedemptionArray[1];
                            if (IthRedemption.flowType == sophis.instrument.eMFlowType.M_ftFloating)
                            {
                                NextCouponIsFloat = 1;
                            }
                        }
                        break;
                    default:
                        break;
                }
                return NextCouponIsFloat;
            }
            catch (Exception)
            {
                CSMLog.Write("ExtentionMethods", "TkocComputeFixedOrFloat", CSMLog.eMVerbosity.M_warning, "NextCouponIsFloat : " + NextCouponIsFloat + " ; Date : " + instrument.GetReference().StringValue + "");
                return NextCouponIsFloat;
            }

        }
        #endregion

        #region Others Tikehau instrument method

        public static int TkoTestSeniority(this CSMInstrument instrument, InputProvider input)
        {
            string sector_research;
            Boolean fsub;
            input.Instrument = instrument;
            input.TmpPortfolioColName = "Sector Tikehau Research Sectors";
            sector_research = Helper.TkoGetValuefromSophisString(input);
            fsub = sector_research.Contains("Financials");

            if (instrument.GetSeniority() == 102 && fsub) return -1;
            else if ((instrument.GetSeniority() == 102 && !fsub) || instrument.GetSeniority() == 121 ||
                      instrument.GetSeniority() == 101 || (instrument.GetSeniority() > 102 && instrument.GetSeniority() <= 109)) return 1;
            else return 0;

        }



        public static int TkoGet1stCallDate(this CSMInstrument instrument, InputProvider input)
        {
            ArrayList Clauses = new ArrayList();
            instrument.GetClauseInformation(Clauses);
            int i = 0;
            if (Clauses.Count > 0)
            {
                SSMClause IthClause = new SSMClause();
                IthClause = (SSMClause)Clauses[i];
                while (IthClause.type != 2)
                {
                    i++;
                    if (i >= Clauses.Count) return 0;
                    IthClause = (SSMClause)Clauses[i];

                }
                return IthClause.start_date;
            }
            else
            {
                return 0;
            }
        }

        public static System.Collections.ArrayList TkoGetBondExplanationArray(this CSMInstrument instrument, InputProvider input)
        {
            System.Collections.ArrayList explicationArray;
            try
            {
                CSMBond Bond = CSMBond.GetInstance(instrument.GetCode());//Création de l'obligation à partir de son sicovam
                CSMMarketData Context = CSMMarketData.GetCurrentMarketData();

                //Informations sur l'obligation
                int IssueDate = Bond.GetIssueDate();
                int MaturityDate = Bond.GetMaturity();
                int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                int PariPassudate = Bond.GetPariPassuDate(input.ReportingDate);

                //Table des CF restants de l'obligation (table "Explanation" de Sophis)
                explicationArray = new System.Collections.ArrayList();
                SSMExplication explication = new SSMExplication();
                explication.transactionDate = input.ReportingDate;
                explication.settlementDate = input.ReportingDate + SettlementShift;
                explication.pariPassuDate = PariPassudate;
                explication.endDate = MaturityDate;

                explication.withCredit = false;
                explication.discounted = false;
                Bond.GetRedemptionExplication(Context, explicationArray, explication);

                return explicationArray;
            }
            catch (Exception)
            {
                CSMLog.Write("ExtensionMethods", "TkocGetBondExplanationArray", CSMLog.eMVerbosity.M_warning, "Cannot load explanation array for Instrument " + input.InstrumentCode);
                return null;
            }
        }

        public static TkoEnumCreditInstrumentType TkoGetCreditInstrumentType(this CSMInstrument instrument, InputProvider input)
        {
            TkoEnumCreditInstrumentType InstrumentType;
            try
            {
                InstrumentType = 0;
                int IsFixed = 0;
                int isFloat = 0;
                int isRedemption = 0;
                var ttttt = Helper.TkoGetValuefromSophisString(input);

                input.TmpPortfolioColName = "Allotment";
                var allotmentList = DbrTikehau_Config.GetTikehauConfigFromName("ALLOTMENTS-NULL-IR-DURATION");
                foreach (var elt in allotmentList)
                {
                    if (elt.VALUE == Helper.TkoGetValuefromSophisString(input))
                        return TkoEnumCreditInstrumentType.AllotmentsNulIRDuration;
                }

                switch (instrument.GetInstrumentType())
                {
                    case 'O'://Obligation
                        //Table des CF de l'obligation de son émission à sa maturité
                        int IssueDate = instrument.GetIssueDate();
                        int RedemptionDate = instrument.GetExpiry();//Date de remboursement
                        System.Collections.ArrayList RedemptionArray = new System.Collections.ArrayList();
                        instrument.GetRedemption(RedemptionArray, IssueDate, RedemptionDate);
                        int nbCF = RedemptionArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                        SSMRedemption IthRedemption;// ième flux

                        for (int j = 0; j < nbCF; j++)
                        {
                            IthRedemption = (sophis.instrument.SSMRedemption)RedemptionArray[j];
                            eMFlowType FlowType = IthRedemption.flowType;
                            switch (FlowType)
                            {
                                case sophis.instrument.eMFlowType.M_ftFixed://fixed
                                    IsFixed = 1;
                                    break;
                                case sophis.instrument.eMFlowType.M_ftFloating://floating
                                    isFloat = 1;
                                    break;

                                //Que fait-on ds ces cas????????
                                case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                                    if (j == 0)
                                    {
                                        isRedemption = 1;
                                    }
                                    break;
                                case sophis.instrument.eMFlowType.M_ftFloatingSwap://FloatingSwap
                                    break;
                                case sophis.instrument.eMFlowType.M_ftCapitalFlow://CapitalFlow
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (IsFixed == 1 & isFloat == 0) { InstrumentType = TkoEnumCreditInstrumentType.FIXED; }//Fixed
                        if (IsFixed == 0 & isFloat == 1) { InstrumentType = TkoEnumCreditInstrumentType.FLOAT; }//Float pur
                        if (IsFixed == 1 & isFloat == 1) { InstrumentType = TkoEnumCreditInstrumentType.FIXEDTOFLOAT; }//FixedToFloat
                        break;
                    //Convertibles
                    case 'D':
                        input.TmpPortfolioColName = "Allotment";
                        var config = DbrTikehau_Config.GetTikehauConfigFromName("CONVERTIBLE-BOND-ALLOTMENTS");
                        bool ret = false;
                        foreach (var elt in config)
                        {
                            if (elt.VALUE == Helper.TkoGetValuefromSophisString(input))
                                ret = true;
                        }
                        if (ret)
                        {
                            int IssueDate_d = instrument.GetIssueDate();

                            int RedemptionDate_d = instrument.GetExpiry();//Date de remboursement
                            System.Collections.ArrayList RedemptionArray_d = new System.Collections.ArrayList();
                            instrument.GetRedemption(RedemptionArray_d, IssueDate_d, RedemptionDate_d);
                            int nbCF_d = RedemptionArray_d.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                            SSMRedemption IthRedemption_d;// ième flux

                            for (int j = 0; j < nbCF_d; j++)
                            {
                                IthRedemption_d = (sophis.instrument.SSMRedemption)RedemptionArray_d[j];
                                eMFlowType FlowType_d = IthRedemption_d.flowType;
                                switch (FlowType_d)
                                {
                                    case sophis.instrument.eMFlowType.M_ftFixed://fixed
                                        IsFixed = 1;
                                        break;
                                    case sophis.instrument.eMFlowType.M_ftFloating://floating
                                        isFloat = 1;
                                        break;

                                    //Que fait-on ds ces cas????????
                                    case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                                        break;
                                    case sophis.instrument.eMFlowType.M_ftFloatingSwap://FloatingSwap
                                        break;
                                    case sophis.instrument.eMFlowType.M_ftCapitalFlow://CapitalFlow
                                        break;
                                    default:
                                        break;
                                }
                            }
                            if (IsFixed == 1 & isFloat == 0) { InstrumentType = TkoEnumCreditInstrumentType.FIXED; }//Fixed
                            if (IsFixed == 0 & isFloat == 1) { InstrumentType = TkoEnumCreditInstrumentType.FLOAT; }//Float pur
                            if (IsFixed == 1 & isFloat == 1) { InstrumentType = TkoEnumCreditInstrumentType.FIXEDTOFLOAT; }//FixedToFloat
                        }
                        else InstrumentType = TkoEnumCreditInstrumentType.OTHERS;
                        break;
                    case 'S'://CDS
                        InstrumentType = TkoEnumCreditInstrumentType.CDS;
                        break;
                    default://Ni CDS, ni bond fixed/float/fixedToFloat
                        InstrumentType = TkoEnumCreditInstrumentType.OTHERS;
                        break;
                }
                return InstrumentType;
            }
            catch (Exception)
            {
                CSMLog.Write("ExtensionMethods", "TkocGetCreditInstrumentType", CSMLog.eMVerbosity.M_warning, "InstrumentType cannot be computed for Instrument " + input.InstrumentCode);
                return TkoEnumCreditInstrumentType.OTHERS;
            }
        }

        //Trouve la meilleure notation parmi l'ensemble des notes d'un instrument
        //( code agence supposé 41 SP/100 Moodys/182 Fitch)
        public static int TkoNotationNum(this CSMInstrument instrument)
        {
            //Var
            int rating = -1;
            int[] tabrating = new int[3] { 0, 0, 0 };
            string[] notation = new string[3] { "", "", "" };

            try
            {
                //récupération des notations
                if (instrument.GetRatingScale(41, 0) != null) notation[0] = instrument.GetRatingScale(41, 0).GetName();
                if (instrument.GetRatingScale(100, 0) != null) notation[1] = instrument.GetRatingScale(100, 0).GetName();
                if (instrument.GetRatingScale(182, 0) != null) notation[2] = instrument.GetRatingScale(182, 0).GetName();


                //Si aucune notation des agences on attribue la notation Tikehau
                if (notation[0] == notation[1] && notation[1] == notation[2] && notation[2] == "")
                {
                    rating = -2;
                }
                else //Calcul de la meilleure notation
                {
                    //implémentation du tabelau de notation 
                    for (int j = 0; j < 3; j++)
                        //tabrating[j] = DbrHelperFunctions.DefineNumerating(notation[j]);
                        tabrating[j] = Helper.DefineNumerating(notation[j]);

                    rating = Helper.FindBestRatingNum(tabrating);
                }

                return rating;
            }
            catch (Exception)
            {
                CSMLog.Write("ExtentionMethods", "TkocNotationNum", CSMLog.eMVerbosity.M_warning, "Cannot compute NotationNum for Instrument " + instrument.GetCode());
                return -3;
            }
        }


        public static int TkoNotationSecondNum(this CSMInstrument instrument)
        {
            //Var
            int rating = -1;
            int[] tabrating = new int[3] { 0, 0, 0 };
            string[] notation = new string[3] { "", "", "" };

            try
            {
                //récupération des notations
                if (instrument.GetRatingScale(41, 0) != null) notation[0] = instrument.GetRatingScale(41, 0).GetName();
                if (instrument.GetRatingScale(100, 0) != null) notation[1] = instrument.GetRatingScale(100, 0).GetName();
                if (instrument.GetRatingScale(182, 0) != null) notation[2] = instrument.GetRatingScale(182, 0).GetName();


                //Si aucune notation des agences on attribue la notation Tikehau
                if (notation[0] == notation[1] && notation[1] == notation[2] && notation[2] == "")
                {
                    rating = -2;
                }
                else //Calcul de la meilleure notation
                {
                    //implémentation du tabelau de notation 
                    for (int j = 0; j < 3; j++)
                        tabrating[j] = Helper.DefineNumerating(notation[j]);

                    rating = Helper.FindSecondRatingNum(tabrating);
                }

                return rating;
            }

            catch (Exception)
            {
                CSMLog.Write("MarketIndicCompute", "NotationSecondNum", CSMLog.eMVerbosity.M_warning, "Cannot compute NotationNum for Instrument " + instrument.GetCode());

                return -3;
            }
        }
        #endregion
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sophis;
using Sophis.ClauseBuilder;

namespace MEDIO.ClauseBuilder.net.Data
{
    [DBTable("GB_MASK_DATA_AUTOCALL")]
    [DBPrimaryKey]
    [XMLNamespaceBinding("www.sophis.net/ToolkitExoticMask")] //namespace needed, corresponds to the one defined in the new xsd file
    [System.Reflection.ObfuscationAttribute(ApplyToMembers = true)]
    public class CSxClauseBuilderAutocallData : Sophis.ClauseBuilder.ClauseBuilderExoticMaskDataWithScheduler 
    {

        public CSxClauseBuilderAutocallData()
            : base()
        {
            FactorKI = 1;
            PerfCapKI = 1;
            PerfFloorKI = 0;
            FactorNoKI = 1;
            PerfCapNoKI = 1;
            PerfFloorNoKI = 0;

            _GeneratedDateList.ListChanged += new ListChangedEventHandler(List_ListChanged);
        }

        public CSxClauseBuilderAutocallData(CSxClauseBuilderAutocallData toCopy)
            : base(toCopy)
        {
            foreach (var generatedDate in toCopy._GeneratedDateList)
            {
                _GeneratedDateList.Add((GeneratedDates) generatedDate.Clone());
            }
            // standard 
            _KIFrequency = toCopy._KIFrequency;
            _MemoryFrequency = toCopy._MemoryFrequency;
            _CouponType = toCopy._CouponType;
            _MaxCouponIfKO = toCopy._MaxCouponIfKO;
            _AutocallLevel = toCopy._AutocallLevel;
            _KOFactor = toCopy._KOFactor;
            _KOPerfCap = toCopy._KOPerfCap;
            _KOPerfFloor = toCopy._KOPerfFloor;
            _KiIsChecked = toCopy._KiIsChecked;
            _KiIntraday = toCopy._KiIntraday;
            _KIBarrierLevel = toCopy._KIBarrierLevel;
            _PayoffTypeKI = toCopy._PayoffTypeKI;
            _PerfCapKI = toCopy._PerfCapKI;
            _PerfFloorKI = toCopy._PerfFloorKI;
            _FactorKI = toCopy._FactorKI;
            _PayoffTypeNoKI = toCopy._PayoffTypeNoKI;
            _PerfCapNoKI = toCopy._PerfCapNoKI;
            _PerfFloorNoKI = toCopy._PerfFloorNoKI;
            _FactorNoKI = toCopy._FactorNoKI;
            _GenerateLastKO = toCopy._GenerateLastKO;

            // toolkit 
            _couponFrequency = toCopy.CouponFrequency;
            _couponChecked = toCopy.CouponChecked;
            _conditionalCoupon = toCopy.ConditionalCoupon;
            _conditionalCouponLevel = toCopy.ConditionalCouponLevel;
            _GeneratedDateList.ListChanged += new ListChangedEventHandler(List_ListChanged);
        }

        public override object Clone()
        {
            return new CSxClauseBuilderAutocallData(this);
        }

        public override string MaskExplanation
        {
            get
            {
                String explanation =
                    "Autocallable products (‘autocalls’) are structured products linked to an underlying asset "
                    + "(typically single underlying or worst of), which can automatically mature (early redemption or kick out) prior "
                    + "to their scheduled maturity date if certain pre-determined market conditions have been met with regard to the underlying asset.";
                return explanation;
            }
        }

        #region Properties

        [DBColumn("BASKET_TYPE", Storage = "fBasketType")]
        //For BasketType, XML is managed in base class
        public override String BasketType
        {
            get { return base.BasketType; }
            set { base.BasketType = value; }
        }

        private int _KIFrequency = -1;
        [DBColumn(Name = "KI_FREQ", Storage = "_KIFrequency")]
        [XMLBinding(XMLIndex = 3, Tag = "KIFrequency", XMLType = eXMLBindingType.xmlDateFpml)]
        public int KIFrequency
        {
            get { return _KIFrequency; }
            set { var old = _KIFrequency; _KIFrequency = value; OnPropertyChanged("KIFrequency", old != value); }
        }

        private int _MemoryFrequency = 0;
        [DBColumn(Name = "MEMORY_FREQ", Storage = "_MemoryFrequency")]
        [XMLBinding(XMLIndex = 4, Tag = "memoryFrequency", XMLType = eXMLBindingType.xmlDateFpml)]
        public int MemoryFrequency
        {
            get { return _MemoryFrequency; }
            set { var old = _MemoryFrequency; _MemoryFrequency = value; OnPropertyChanged("MemoryFrequency", old != value); }
        }

        public const string FixedCouponStr = "Fixed Coupon";
        public const string AccumulatedCouponStr = "Accumulated Coupon";
        public const string MemoryCouponStr = "Memory Coupon";

        static List<string> _CouponTypes = new List<string> { FixedCouponStr, AccumulatedCouponStr, MemoryCouponStr };
        public List<string> CouponTypes
        {
            get { return _CouponTypes; }
        }

        private string _CouponType = FixedCouponStr;
        [DBColumn("COUPON_TYPE", Storage = "_CouponType")]
        [XMLBinding(XMLIndex = 5, Tag = "couponType", XMLType = eXMLBindingType.xmlString)]
        public string CouponType
        {
            get { return _CouponType; }
            set
            {
                if (_CouponType != value)
                {
                    string old = _CouponType;
                    _CouponType = value;
                    OnCouponTypeChanged(old);
                }
            }
        }

        public void OnCouponTypeChanged(string old)
        {
            OnPropertyChanged("CouponType");
            OnPropertyChanged("IsFixedCoupon");
            OnPropertyChanged("IsAccumulatedCoupon");
            OnPropertyChanged("IsMemoryCoupon");
            if (_CouponType == AccumulatedCouponStr)
            {
                KiIsChecked = false;
                FactorKI = 1;
                PerfCapKI = 1;
                PerfFloorKI = 0;
                FactorNoKI = 1;
                PerfCapNoKI = 1;
                PerfFloorNoKI = 0;
                PayoffTypeKI = PayoffTypePerfIndexedStr;
                PayoffTypeNoKI = PayoffTypePerfIndexedStr;
            }
            else
            {
                KiIsChecked = true;
                FactorKI = 1;
                PerfCapKI = 1;
                PerfFloorKI = 0;
                FactorNoKI = 1;
                PerfCapNoKI = 1;
                PerfFloorNoKI = 0;
                if (_CouponType == FixedCouponStr)
                {
                    PayoffTypeKI = PayoffTypePerfIndexedStr;
                    PayoffTypeNoKI = PayoffTypePerfIndexedStr;
                }
                else
                {
                    PayoffTypeKI = PayoffTypePerfIndexedStr;
                    PayoffTypeNoKI = PayoffTypeAbsPerfStr;
                }
            }
        }

        public bool IsFixedCoupon
        {
            get { return CouponType == FixedCouponStr; }
        }

        public bool IsAccumulatedCoupon
        {
            get { return CouponType == AccumulatedCouponStr; }
        }

        public bool IsMemoryCoupon
        {
            get { return CouponType == MemoryCouponStr; }
        }

        private double _MaxCouponIfKO = 0.0;
        [DBColumn(Name = "COUPON_KO", Storage = "_MaxCouponIfKO")]
        [XMLBinding(XMLIndex = 6, Tag = "KOCoupon", XMLType = eXMLBindingType.xmlDouble)]
        public double MaxCouponIfKO
        {
            get { return _MaxCouponIfKO; }
            set { var old = _MaxCouponIfKO; _MaxCouponIfKO = value; OnPropertyChanged("MaxCouponIfKO", old != value); }
        }

        private double _AutocallLevel = 0.0;
        [DBColumn(Name = "AUTOCALL_LEVEL", Storage = "_AutocallLevel")]
        [XMLBinding(XMLIndex = 7, Tag = "KOAutocallLevel", XMLType = eXMLBindingType.xmlDouble)]
        public double AutocallLevel
        {
            get { return _AutocallLevel; }
            set { var old = _AutocallLevel; _AutocallLevel = value; OnPropertyChanged("AutocallLevel", old != value); }
        }

        private double _KOFactor;
        [DBColumn("FACTOR_KO", Storage = "_KOFactor")]
        [XMLBinding(XMLIndex = 8, Tag = "KOFactor", XMLType = eXMLBindingType.xmlDouble)]
        public double KOFactor
        {
            get { return _KOFactor; }
            set
            {
                if (_KOFactor != value)
                {
                    _KOFactor = value;
                    OnPropertyChanged("KOFactor");
                }
            }
        }

        private double _KOPerfCap;
        [DBColumn("PERF_CAP_KO", Storage = "_KOPerfCap")]
        [XMLBinding(XMLIndex = 9, Tag = "KOPerfCap", XMLType = eXMLBindingType.xmlDouble)]
        public double KOPerfCap
        {
            get { return _KOPerfCap; }
            set
            {
                if (_KOPerfCap != value)
                {
                    _KOPerfCap = value;
                    OnPropertyChanged("KOPerfCap");
                }
            }
        }

        private double _KOPerfFloor;
        [DBColumn("PERF_FLOOR_KO", Storage = "_KOPerfFloor")]
        [XMLBinding(XMLIndex = 10, Tag = "KOPerfFloor", XMLType = eXMLBindingType.xmlDouble)]
        public double KOPerfFloor
        {
            get { return _KOPerfFloor; }
            set
            {
                if (_KOPerfFloor != value)
                {
                    _KOPerfFloor = value;
                    OnPropertyChanged("KOPerfFloor");
                }
            }
        }

        private bool _KiIsChecked = false;
        [DBColumn("HAS_KI", DbType = eDbType.Int16, Storage = "_KiIsChecked")]
        [XMLBinding(XMLIndex = 11, Tag = "hasKI", XMLType = eXMLBindingType.xmlBoolean)]
        public bool KiIsChecked
        {
            get { return _KiIsChecked; }
            set
            {
                if (_KiIsChecked != value)
                {
                    _KiIsChecked = value;
                    OnPropertyChanged("KiIsChecked");
                }
            }
        }

        private bool _KiIntraday = false;
        [DBColumn("KI_INTRADAY", Storage = "_KiIntraday")]
        [XMLBinding(XMLIndex = 12, Tag = "KiIntraday", XMLType = eXMLBindingType.xmlBoolean)]
        public bool KiIntraday
        {
            get { return _KiIntraday; }
            set
            {
                if (_KiIntraday != value)
                {
                    _KiIntraday = value;
                    OnPropertyChanged("KiIntraday");
                }
            }
        }

        private double _KIBarrierLevel = 0.0;
        [DBColumn(Name = "KI_BARRIER_LEVEL", Storage = "_KIBarrierLevel")]
        [XMLBinding(XMLIndex = 13, Tag = "KIBarrierLevel", XMLType = eXMLBindingType.xmlDouble)]
        public double KIBarrierLevel
        {
            get { return _KIBarrierLevel; }
            set { var old = _KIBarrierLevel; _KIBarrierLevel = value; OnPropertyChanged("KIBarrierLevel", old != value); }
        }

        public const string PayoffTypeFixedStr = "Fixed Coupon";
        public const string PayoffTypePerfIndexedStr = "Perf-indexed";
        public const string PayoffTypeAbsPerfStr = "Absolute Performance";
        public const string PayoffTypeAveragePerfStr = "Average Performance";

        static List<string> _PayoffTypes = new List<string> { PayoffTypeFixedStr, 
            PayoffTypePerfIndexedStr, PayoffTypeAbsPerfStr, PayoffTypeAveragePerfStr };
        public List<string> PayoffTypes
        {
            get { return _PayoffTypes; }
        }

        private string _PayoffTypeKI = PayoffTypePerfIndexedStr;
        [DBColumn("PAYOFF_TYPE_KI", Storage = "_PayoffTypeKI")]
        [XMLBinding(XMLIndex = 14, Tag = "payoffTypeKI", XMLType = eXMLBindingType.xmlString)]
        public string PayoffTypeKI
        {
            get { return _PayoffTypeKI; }
            set
            {
                if (_PayoffTypeKI != value)
                {
                    _PayoffTypeKI = value;
                    OnPropertyChanged("PayoffTypeKI");
                    OnPropertyChanged("IsPayoffKIFixedCoupon");
                    //UpdateElements();
                }
            }
        }

        public bool IsPayoffKIFixedCoupon
        {
            get { return PayoffTypeKI == PayoffTypeFixedStr; }
        }

        private double _PerfCapKI;
        [DBColumn("PERF_CAP_KI", Storage = "_PerfCapKI")]
        [XMLBinding(XMLIndex = 15, Tag = "perfCapKI", XMLType = eXMLBindingType.xmlDouble)]
        public double PerfCapKI
        {
            get { return _PerfCapKI; }
            set
            {
                if (_PerfCapKI != value)
                {
                    _PerfCapKI = value;
                    OnPropertyChanged("PerfCapKI");
                }
            }
        }

        private double _PerfFloorKI;
        [DBColumn("PERF_FLOOR_KI", Storage = "_PerfFloorKI")]
        [XMLBinding(XMLIndex = 16, Tag = "perfFloorKI", XMLType = eXMLBindingType.xmlDouble)]
        public double PerfFloorKI
        {
            get { return _PerfFloorKI; }
            set
            {
                if (_PerfFloorKI != value)
                {
                    _PerfFloorKI = value;
                    OnPropertyChanged("PerfFloorKI");
                }
            }
        }

        private double _FactorKI;
        [DBColumn("FACTOR_KI", Storage = "_FactorKI")]
        [XMLBinding(XMLIndex = 17, Tag = "factorKI", XMLType = eXMLBindingType.xmlDouble)]
        public double FactorKI
        {
            get { return _FactorKI; }
            set
            {
                if (_FactorKI != value)
                {
                    _FactorKI = value;
                    OnPropertyChanged("FactorKI");
                }
            }
        }

        private string _PayoffTypeNoKI = PayoffTypeFixedStr;
        [DBColumn("PAYOFF_TYPE_NO_KI", Storage = "_PayoffTypeNoKI")]
        [XMLBinding(XMLIndex = 18, Tag = "payoffTypeNoKI", XMLType = eXMLBindingType.xmlString)]
        public string PayoffTypeNoKI
        {
            get { return _PayoffTypeNoKI; }
            set
            {
                if (_PayoffTypeNoKI != value)
                {
                    _PayoffTypeNoKI = value;
                    OnPropertyChanged("PayoffTypeNoKI");
                    OnPropertyChanged("IsPayoffNoKIFixedCoupon");
                    //UpdateElements();
                }
            }
        }

        public bool IsPayoffNoKIFixedCoupon
        {
            get { return PayoffTypeNoKI == PayoffTypeFixedStr; }
        }

        private double _PerfCapNoKI;
        [DBColumn("PERF_CAP_NO_KI", Storage = "_PerfCapNoKI")]
        [XMLBinding(XMLIndex = 19, Tag = "perfCapNoKI", XMLType = eXMLBindingType.xmlDouble)]
        public double PerfCapNoKI
        {
            get { return _PerfCapNoKI; }
            set
            {
                if (_PerfCapNoKI != value)
                {
                    _PerfCapNoKI = value;
                    OnPropertyChanged("PerfCapNoKI");
                }
            }
        }

        private double _PerfFloorNoKI;
        [DBColumn("PERF_FLOOR_NO_KI", Storage = "_PerfFloorNoKI")]
        [XMLBinding(XMLIndex = 20, Tag = "perfFloorNoKI", XMLType = eXMLBindingType.xmlDouble)]
        public double PerfFloorNoKI
        {
            get { return _PerfFloorNoKI; }
            set
            {
                if (_PerfFloorNoKI != value)
                {
                    _PerfFloorNoKI = value;
                    OnPropertyChanged("PerfFloorNoKI");
                }
            }
        }

        private double _FactorNoKI;
        [DBColumn("FACTOR_NO_KI", Storage = "_FactorNoKI")]
        [XMLBinding(XMLIndex = 21, Tag = "factorNoKI", XMLType = eXMLBindingType.xmlDouble)]
        public double FactorNoKI
        {
            get { return _FactorNoKI; }
            set
            {
                if (_FactorNoKI != value)
                {
                    _FactorNoKI = value;
                    OnPropertyChanged("FactorNoKI");
                }
            }
        }

        private int _fixingKIId = -1;
        public int fixingKIId
        {
            get { return _fixingKIId; }
            set
            {
                if (_fixingKIId != value)
                {
                    _fixingKIId = value;
                }
            }
        }

        private Dictionary<int, int> _MemoryFixingsMap = null;
        public Dictionary<int, int> MemoryFixingsMap
        {
            get { return _MemoryFixingsMap; }
            set
            {
                if (_MemoryFixingsMap != value)
                {
                    _MemoryFixingsMap = value;
                }
            }
        }

        private int _LastPeriodEndfixingId = -1;
        public int LastPeriodEndfixingId
        {
            get { return _LastPeriodEndfixingId; }
            set
            {
                if (_LastPeriodEndfixingId != value)
                {
                    _LastPeriodEndfixingId = value;
                }
            }
        }

        private bool _GenerateLastKO = true;
        [DBColumn("Generate_Last_KO", Storage = "_GenerateLastKO")]
        [XMLBinding(XMLIndex = 23, Tag = "generateLastKO", XMLType = eXMLBindingType.xmlBoolean)]
        public bool GenerateLastKO
        {
            get { return _GenerateLastKO; }
            set
            {
                if (_GenerateLastKO != value)
                {
                    _GenerateLastKO = value;
                    OnPropertyChanged("GenerateLastKO");
                }
            }
        }


        //XMLBinding : start with XMLIndex = 1000, and then increment for each additional property that must be exported in XML (1001, 1002...)

        private int _couponFrequency = -1;

        [DBColumn(Name = "COUPON_FREQ", Storage = "_couponFrequency")]
        [XMLBinding(XMLIndex = 1000, Tag = "CouponFrequency", XMLType = eXMLBindingType.xmlDateFpml)]
        public int CouponFrequency
        {
            get { return _couponFrequency; }
            set
            {
                var old = _couponFrequency;
                _couponFrequency = value;
                OnPropertyChanged("CouponFrequency", old != value);
            }
        }

        private bool _couponChecked = false;

        [DBColumn("HAS_CON_COUPON", DbType = eDbType.Int16, Storage = "_couponChecked")]
        [XMLBinding(XMLIndex = 1001, Tag = "CouponChecked", XMLType = eXMLBindingType.xmlBoolean)]
        public bool CouponChecked
        {
            get { return _couponChecked; }
            set
            {
                if (_couponChecked != value)
                {
                    _couponChecked = value;
                    OnPropertyChanged("CouponChecked");
                }
            }
        }

        private double _conditionalCoupon = 0.0;

        [DBColumn(Name = "CON_COUPON", Storage = "_conditionalCoupon")]
        [XMLBinding(XMLIndex = 1002, Tag = "ConditionalCoupon", XMLType = eXMLBindingType.xmlDouble)]
        public double ConditionalCoupon
        {
            get { return _conditionalCoupon; }
            set
            {
                var old = _conditionalCoupon;
                _conditionalCoupon = value;
                OnPropertyChanged("ConditionalCoupon", old != value);
            }
        }

        private double _conditionalCouponLevel = 0.0;

        [DBColumn(Name = "CON_COUPON_LEVEL", Storage = "_conditionalCouponLevel")]
        [XMLBinding(XMLIndex = 1003, Tag = "ConditionalCouponLevel", XMLType = eXMLBindingType.xmlDouble)]
        public double ConditionalCouponLevel
        {
            get { return _conditionalCouponLevel; }
            set
            {
                var old = _conditionalCouponLevel;
                _conditionalCouponLevel = value;
                OnPropertyChanged("ConditionalCouponLevel", old != value);
            }
        }

        #endregion

        [DBIndex("NUMERO")]
        [DBPrimaryKey("CODE")]
        [DBTable("GB_MASK_GENERATED_DATES")]
        public class GeneratedDates : INotifyPropertyChanged, ICloneable
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propName)
            {
                if (null != this.PropertyChanged)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }

            public GeneratedDates()
            {
            }

            public object Clone()
            {
                GeneratedDates clone = new GeneratedDates
                {
                    _CouponLastDate = this.CouponLastDate,
                    _CouponDate = this.CouponDate,
                    _AutocallLevel = this.AutocallLevel,
                    _CouponPayDate = this.CouponPayDate,
                    _AutocallCoupon = this.AutocallCoupon,
                    _ConCoupon = this.ConCoupon,
                    _CouponLevel = this.CouponLevel,
                    _FixingClauseId = this.FixingClauseId
                };
                return clone;
            }

            private int _CouponDate = 0;
            [DBColumn("COUPON_DATE", Storage = "_CouponDate", DbType = eDbType.Date)]
            [XMLBinding(XMLIndex = 0, Tag = "couponDate", XMLType = eXMLBindingType.xmlDate)]
            public int CouponDate
            {
                get { return _CouponDate; }
                set
                {
                    if (_CouponDate != value)
                    {
                        _CouponDate = value;
                        OnPropertyChanged("CouponDate");
                    }
                }
            }

            private int _CouponPayDate = 0;
            [DBColumn("COUPON_PAYDATE", Storage = "_CouponPayDate", DbType = eDbType.Date)]
            [XMLBinding(XMLIndex = 1, Tag = "couponPayDate", XMLType = eXMLBindingType.xmlDate)]
            public int CouponPayDate
            {
                get { return _CouponPayDate; }
                set
                {
                    if (_CouponPayDate != value)
                    {
                        _CouponPayDate = value;
                        OnPropertyChanged("CouponPayDate");
                    }
                }
            }

            private double _AutocallLevel = 0;
            [DBColumn("BARRIER_LEVEL", Storage = "_AutocallLevel")]
            [XMLBinding(XMLIndex = 2, Tag = "autocallLevel", XMLType = eXMLBindingType.xmlDouble)]
            public double AutocallLevel
            {
                get { return _AutocallLevel; }
                set
                {
                    if (_AutocallLevel != value)
                    {
                        _AutocallLevel = value;
                        OnPropertyChanged("AutocallLevel");
                    }
                }
            }

            private double _AutocallCoupon = 0;
            [DBColumn("COUPON", Storage = "_AutocallCoupon")]
            [XMLBinding(XMLIndex = 3, Tag = "autocallCoupon", XMLType = eXMLBindingType.xmlDouble)]
            public double AutocallCoupon
            {
                get { return _AutocallCoupon; }
                set
                {
                    if (_AutocallCoupon != value)
                    {
                        _AutocallCoupon = value;
                        OnPropertyChanged("AutocallCoupon");
                    }
                }
            }

            private int _FixingClauseId = 0;
            public int FixingClauseId
            {
                get { return _FixingClauseId; }
                set { _FixingClauseId = value; }
            }

            private int _CouponLastDate = 0;
            [XMLBinding(XMLIndex = 1005, Tag = "CouponLastDate", XMLType = eXMLBindingType.xmlDate)]
            [DBColumn("COUPON_LASTDATE", Storage = "_CouponLastDate", DbType = eDbType.Date)]
            public int CouponLastDate
            {
                get { return _CouponLastDate; }
                set
                {
                    if (_CouponLastDate != value)
                    {
                        _CouponLastDate = value;
                        OnPropertyChanged("CouponLastDate");
                    }
                }
            }

            private double _CouponLevel = 0;
            [XMLBinding(XMLIndex = 1006, Tag = "CouponLevel", XMLType = eXMLBindingType.xmlDouble)]
            [DBColumn("COUPON_LEVEL", Storage = "_CouponLevel", DbType = eDbType.Double)]
            public double CouponLevel
            {
                get { return _CouponLevel; }
                set
                {
                    if (_CouponLevel != value)
                    {
                        _CouponLevel = value;
                        OnPropertyChanged("CouponLevel");
                    }
                }
            }

            private double _ConCoupon = 0;
            [DBColumn("CONDITIONAL_COUPON", Storage = "_ConCoupon")]
            [XMLBinding(XMLIndex = 1007, Tag = "ConCoupon", XMLType = eXMLBindingType.xmlDouble)]
            public double ConCoupon
            {
                get { return _ConCoupon; }
                set
                {
                    if (_ConCoupon != value)
                    {
                        _ConCoupon = value;
                        OnPropertyChanged("conCoupon");
                    }
                }
            }

        }

        public override string GetXMLName()
        {
            return "CSxClauseBuilderAutocallData";
        }

        private readonly BindingList<GeneratedDates> _GeneratedDateList = new BindingList<GeneratedDates>()
            ; //CustomBindingList

        [DBNested]
        [ListXMLBindingAttribute(XMLIndex = 1007, DataType = typeof(GeneratedDates), Tag = "GeneratedDateList", Section = "generatedDates")]
        public BindingList<GeneratedDates> GeneratedDateList
        {
            get { return _GeneratedDateList; }
        }

}

}

using System;
using System.Data;
using System.Collections;
using System.Threading;
using System.IO;
using System.Text;

using sophis;
using sophis.portfolio;
using sophis.misc;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;
using sophis.utils;

//@DPH
using Eff.UpgradeUtilities;

namespace dnPortfolioColumn
{
    //Fonctions de calcul
    public class CFCompute
    {
        // Holds the instance of the singleton class
        private static CFCompute Instance = null;
        //private ORCLFactory.ORCLFactory CS_ORCL;

        //Constructeur
        private CFCompute()
        {
            //CS_ORCL = new ORCLFactory.ORCLFactory();
            ORCLFactory.ORCLFactory.Initialize();      
        }
        public void Close()
        {
            //CloseAll();
        }
        
        public static CFCompute GetInstance()
        {
            Instance = new CFCompute();
            return Instance;
        }

        //Fonctions renvoyant le contenu des colonnes CF
        
        public double ComputeCoupon(CSMInstrument InstrumentPtr)
        {
            double coupon = 0;//coupon en %
            CSMBond Bond;
            double Length;//Taille de la 1ere période de coupon
            System.Collections.ArrayList RedemptionArray;//Table de CF
            SSMRedemption FirstRedemption;//er flux

            try
            {
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        Bond = CSMBond.GetInstance(InstrumentPtr.GetCode());
                        RedemptionArray = new System.Collections.ArrayList();//Table des CF de l'obligation de son émission à sa maturité
                        RedemptionArray = GetBondExplanationArray(InstrumentPtr.GetCode(), reportingdate);
                        FirstRedemption = (sophis.instrument.SSMRedemption)RedemptionArray[0];
                        if (FirstRedemption.startDate <= reportingdate)
                        {
                            Length = CSMDayCountBasis.GetCSRDayCountBasis(Bond.GetMarketCSDayCountBasisType()).GetEquivalentYearCount(FirstRedemption.startDate, FirstRedemption.endDate);
                            coupon = FirstRedemption.coupon / Length;
                            coupon = coupon / InstrumentPtr.GetNotional();
                        }
                        else//Si le 1er coupon est postérieur à la date de reporting
                        {
                            coupon = 0;
                        }
                        if (coupon == 0)
                        {
                            coupon = GetCouponOldStyle(InstrumentPtr,reportingdate);
                        }
                        break;

                    case "A"://Action - pas implémenté mettre peut être un jour le dividend yield (rl)
                        coupon = 0;
                        break;


                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            RedemptionArray = new System.Collections.ArrayList();//Table des CF de l'obligation de son émission à sa maturité
                            InstrumentPtr.GetRedemption(RedemptionArray, InstrumentPtr.GetIssueDate(), InstrumentPtr.GetExpiry());
                            FirstRedemption = (sophis.instrument.SSMRedemption)RedemptionArray[0];
                            int i = 1;
                            while (FirstRedemption.endDate < reportingdate)
                            {
                                FirstRedemption = (sophis.instrument.SSMRedemption)RedemptionArray[i];
                                i++;
                            }
                            if (FirstRedemption.startDate <= reportingdate)
                            {
                                coupon = FirstRedemption.coupon;
                            }
                            else//Si le 1er coupon est postérieur à la date de reporting
                            {
                                coupon = 0;
                            }
                            if (coupon == 0)
                            {
                                coupon = GetCouponOldStyle(InstrumentPtr, reportingdate);
                            }
                        }
                        else//option
                        {
                            coupon = 0;
                        }
                        break;
                    default:
                        coupon = 0;
                        break;
                }
                return coupon;
            }
            catch (Exception)
            {
                Console.WriteLine("coupon cannot be computed for instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }
        
        public double ComputeInvestedCash(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double invcash = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                int PositionMvtident = PositionPtr.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        invcash = ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
                        break;

                    case "A"://Actions
                        invcash = ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            invcash = ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
                        }
                        else//option
                        {
                            invcash = 0;
                        }
                        break;

                    case "T"://Billets de treso
                        invcash = ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
                        break;

                    default:
                        break;
                }
                invcash = invcash * fxspot;
                return invcash;
            }
            catch (Exception)
            {
                Console.WriteLine("invcash cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
        
        public double ComputeReceivedCoupons(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double receivedcoupons = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                int PositionMvtident = PositionPtr.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        receivedcoupons = - ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                        //deferred coupons Mezzanine à rajouter une fois la date passée
                        receivedcoupons = receivedcoupons - ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
                        break;

                    case "A"://Actions
                        receivedcoupons = - ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                        //deferred coupons Mezzanine à rajouter une fois la date passée
                        receivedcoupons = receivedcoupons - ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            receivedcoupons = -ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                            //deferred coupons Mezzanine à rajouter une fois la date passée
                            receivedcoupons = receivedcoupons - ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
                        }
                        else//option
                        {
                            receivedcoupons = 0;
                        }
                        break;

                    default:
                        break;
                }
                receivedcoupons = receivedcoupons * fxspot;

                return receivedcoupons;
            }
            catch (Exception)
            {
                Console.WriteLine("receivedcoupons cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }

        public double ComputeReceivedCouponsLocalCCY(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double receivedcoupons = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                int PositionMvtident = PositionPtr.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        receivedcoupons = -ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                        //deferred coupons Mezzanine à rajouter une fois la date passée
                        receivedcoupons = receivedcoupons - ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
                        break;

                    case "A"://Actions
                        receivedcoupons = -ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                        //deferred coupons Mezzanine à rajouter une fois la date passée
                        receivedcoupons = receivedcoupons - ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            receivedcoupons = -ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                            //deferred coupons Mezzanine à rajouter une fois la date passée
                            receivedcoupons = receivedcoupons - ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
                        }
                        else//option
                        {
                            receivedcoupons = 0;
                        }
                        break;

                    default:
                        break;
                }

                return receivedcoupons;
            }
            catch (Exception)
            {
                Console.WriteLine("receivedcoupons cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
        
        
        public double ComputeTMZAccrued(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double TMZAccrued = 0;
            try
            {
                int PositionMvtident = PositionPtr.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                double nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                double unscash;
                double accruedamount;
                double defcoupon;

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        defcoupon = GetDefferedCoupon(PositionPtr, InstrumentPtr);
                        accruedamount = InstrumentPtr.GetAccruedCoupon() * 0.01 * nominal * fxspot + defcoupon;
                        unscash = PositionPtr.GetUnsettledBalance() * 1000 * fxspot;
                        unscash = unscash - defcoupon; //Deferred coupon non encore reconnu en tant que tel
                        if (Math.Abs(unscash) < 0.01) { unscash = 0; }
                        TMZAccrued = unscash + accruedamount;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            defcoupon = GetDefferedCoupon(PositionPtr, InstrumentPtr);
                            accruedamount = InstrumentPtr.GetAccruedCoupon() * 0.01 * nominal * fxspot + defcoupon;
                            unscash = PositionPtr.GetUnsettledBalance() * 1000 * fxspot;
                            unscash = unscash - defcoupon; //Deferred coupon non encore reconnu en tant que tel
                            if (Math.Abs(unscash) < 0.01) { unscash = 0; }
                            TMZAccrued = unscash + accruedamount;
                        }
                        else//option
                        {
                            TMZAccrued = 0;
                        }
                        break;

                    default:
                        break;
                }
                return TMZAccrued;
            }
            catch (Exception)
            {
                Console.WriteLine("TMZAccrued cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
        //Renvoie 1 si le prochain coupon est flottant
        public int ComputeFixedOrFloat(CSMInstrument InstrumentPtr)
        {
            int NextCouponIsFloat = 0;
            try
            {
                
                CSMBond Bond;
                System.Collections.ArrayList RedemptionArray;//Table de CF

                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        Bond = CSMBond.GetInstance(InstrumentPtr.GetCode());
                        RedemptionArray = new System.Collections.ArrayList();//Table des CF de l'obligation de la repotingdate à sa maturité
                        SSMRedemption IthRedemption;// ième flux à venir
                        RedemptionArray = GetBondExplanationArray(InstrumentPtr.GetCode(), reportingdate);

                        if (RedemptionArray.Count >= 2)
                        {
                            IthRedemption = (sophis.instrument.SSMRedemption)RedemptionArray[1];
                            if (IthRedemption.flowType.ToString().Equals("M_ftFloating"))
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
                Console.WriteLine("FixedOrFloat cannot be computed for instrument " + InstrumentPtr.GetCode());
                return NextCouponIsFloat;
            }

        }

        //Fonctions annexes
        public double GetDefferedCoupon(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double defcoupon = 0;
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            defcoupon = -ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionPtr.GetIdentifier() + " and TYPE=360 and DATEVAL > num_to_date(" + reportingdate + ") and DATENEG <= num_to_date(" + reportingdate + ")");
            return defcoupon;
        }
        public System.Collections.ArrayList GetBondExplanationArray(int InstrumentCode, int reportingdate)
        {
            System.Collections.ArrayList explicationArray;
            try
            {
                CSMBond Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam
                CSMMarketData Context = CSMMarketData.GetCurrentMarketData();

                //Informations sur l'obligation
                int IssueDate = Bond.GetIssueDate();
                int MaturityDate = Bond.GetMaturity();
                int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                int PariPassudate = Bond.GetPariPassuDate(reportingdate);

                //Table des CF restants de l'obligation (table "Explanation" de Sophis)
                explicationArray = new System.Collections.ArrayList();
                SSMExplication explication = new SSMExplication();
                explication.transactionDate = reportingdate;
                explication.settlementDate = reportingdate + SettlementShift;
                explication.pariPassuDate = PariPassudate;
                explication.endDate = MaturityDate;
                explication.withCredit = false;
                explication.discounted = false;
                Bond.GetRedemptionExplication(Context, explicationArray, explication);

                return explicationArray;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot load explanation array for Instrument " + InstrumentCode);
                return null;
            }
        }
        public double GetCouponOldStyle(CSMInstrument InstrumentPtr, int reportingdate)
        {
            double coupon = InstrumentPtr.GetNotionalRate() * 0.01;
            //On essaye d'abord de récupérer le coupon fixe si c'est un fixtofloat encore en mode fixe
            if (coupon == 0)
            {
                int ftofdate = ORCLFactory.ORCLFactory.getResultI("select date_to_num(DATEFINAL) from TITRES where SICOVAM=" + InstrumentPtr.GetCode() + "");
                if (ftofdate >= reportingdate)
                {
                    coupon = ORCLFactory.ORCLFactory.getResultD("select CROIDIV from TITRES where SICOVAM=" + InstrumentPtr.GetCode() + "") * 0.01;
                }
            }

            //Si le coupon est toujours égal à 0, alors c'est un floater pur ou la période fixe est terminée

            if (coupon == 0)
            {
                coupon = InstrumentPtr.GetSpread() * 0.01;
                coupon = coupon + CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentPtr.GetFloatingRate()) * 0.01;

            }
            return coupon;
        }
    }
    
    //Fonctions de gestion de l'affichage des colonnes

    public class DataSourceCoupon
    {
        // Holds the instance of the singleton class
        private static DataSourceCoupon Instance = null;
        
        public static Hashtable DataCacheCoupon;//Cache pour les valeurs de coupon par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version des intruments
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceCoupon()
        {
            DataCacheCoupon = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceCoupon GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceCoupon();
            return Instance;
        }

        public double GetCoupon(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //La valeur est (re)calculée:
                //si le cache est vide 
                //si la version de l'instrument change
                //si le spot change
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                if (!DataCacheCoupon.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheCoupon.ContainsKey(Instrumentcode))
                {
                    return (double)DataCacheCoupon[Instrumentcode];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        /// <summary>
        /// Executes the SQL query and stores the results in the DataCache
        /// </summary>
        /// <param name="PortfolioCode">Code of the portfolio in question</param>
        private void FillCache(CSMInstrument Instrument)
        {
            //mise à jour du Coupon
            CFCompute ICF = CFCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            double Coupon = ICF.ComputeCoupon(Instrument) * 100;
            if (DataCacheCoupon.ContainsKey(InstrumentCode))
                DataCacheCoupon[InstrumentCode] = Coupon;
            else
                DataCacheCoupon.Add(InstrumentCode, Coupon);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode);
            if (DataCacheSpot.ContainsKey(InstrumentCode))
                DataCacheSpot[InstrumentCode] = Spot;
            else
                DataCacheSpot.Add(InstrumentCode, Spot);

            ICF.Close();
        }
    }
    
    public class DataSourceInvestedCash
    {
        // Holds the instance of the singleton class
        private static DataSourceInvestedCash Instance = null;

        public static Hashtable DataCacheInvestedCash;//Cache pour la valeur de invcash par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceInvestedCash()
        {
            DataCacheInvestedCash = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceInvestedCash GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceInvestedCash();
            return Instance;
        }

        public double GetInvestedCash(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
            if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
            {
                VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0 | Instrument.GetInstrumentType().ToString().Equals("A"))
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheInvestedCash.ContainsKey(PositionIdentifier))
                        DataCacheInvestedCash[PositionIdentifier] = 0.0;
                    else
                        DataCacheInvestedCash.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide
                if (!DataCacheInvestedCash.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheInvestedCash.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheInvestedCash[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            //mise à jour du Coupon
            CFCompute ICF = CFCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            double InvestedCash = ICF.ComputeInvestedCash(Position, Instrument);
            if (DataCacheInvestedCash.ContainsKey(PositionId))
                DataCacheInvestedCash[PositionId] = InvestedCash;
            else
                DataCacheInvestedCash.Add(PositionId, InvestedCash);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            ICF.Close();
        }
    }
    
    public class DataSourceReceivedCoupons
    {
        // Holds the instance of the singleton class
        private static DataSourceReceivedCoupons Instance = null;

        public static Hashtable DataCacheReceivedCoupons;//Cache pour les coupons reçus par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceReceivedCoupons()
        {
            DataCacheReceivedCoupons = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceReceivedCoupons GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceReceivedCoupons();
            return Instance;
        }

        public double GetReceivedCoupons(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
            if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
            {
                VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            //Si la version de l'instr change, on met 0 ds le cache
            if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
            {
                if (DataCacheReceivedCoupons.ContainsKey(PositionIdentifier))
                    DataCacheReceivedCoupons[PositionIdentifier] = 0.0;
                else
                    DataCacheReceivedCoupons.Add(PositionIdentifier, 0.0);

                //mise à jour de la version de l'instrument
                DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
            }

            //La valeur est (re)calculée:
            //si le cache est vide
            if (!DataCacheReceivedCoupons.ContainsKey(PositionIdentifier))
            {
                FillCache(Position, Instrument);
            }


            // At this point, the value that this method should return must be available in the cache
            if (DataCacheReceivedCoupons.ContainsKey(PositionIdentifier))
            {
                return (double)DataCacheReceivedCoupons[PositionIdentifier];
            }
            else { return 0; }//on ne devrait jamais passer par là ms au cas où

        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            //mise à jour de Received Coupons
            CFCompute ICF = CFCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            double ReceivedCoupons = ICF.ComputeReceivedCoupons(Position, Instrument);
            if (DataCacheReceivedCoupons.ContainsKey(PositionId))
                DataCacheReceivedCoupons[PositionId] = ReceivedCoupons;
            else
                DataCacheReceivedCoupons.Add(PositionId, ReceivedCoupons);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            ICF.Close();
        }
    }

    public class DataSourceReceivedCouponsLocalCCY
    {
        // Holds the instance of the singleton class
        private static DataSourceReceivedCouponsLocalCCY Instance = null;

        public static Hashtable DataCacheReceivedCouponsLocalCCY;//Cache pour les coupons reçus par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceReceivedCouponsLocalCCY()
        {
            DataCacheReceivedCouponsLocalCCY = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceReceivedCouponsLocalCCY GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceReceivedCouponsLocalCCY();
            return Instance;
        }

        public double GetReceivedCouponsLocalCCY(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
            if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
            {
                VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            //Si la version de l'instr change, on met 0 ds le cache
            if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
            {
                if (DataCacheReceivedCouponsLocalCCY.ContainsKey(PositionIdentifier))
                    DataCacheReceivedCouponsLocalCCY[PositionIdentifier] = 0.0;
                else
                    DataCacheReceivedCouponsLocalCCY.Add(PositionIdentifier, 0.0);

                //mise à jour de la version de l'instrument
                DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
            }

            //La valeur est (re)calculée:
            //si le cache est vide
            if (!DataCacheReceivedCouponsLocalCCY.ContainsKey(PositionIdentifier))
            {
                FillCache(Position, Instrument);
            }


            // At this point, the value that this method should return must be available in the cache
            if (DataCacheReceivedCouponsLocalCCY.ContainsKey(PositionIdentifier))
            {
                return (double)DataCacheReceivedCouponsLocalCCY[PositionIdentifier];
            }
            else { return 0; }//on ne devrait jamais passer par là ms au cas où

        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            //mise à jour de Received Coupons
            CFCompute ICF = CFCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            double ReceivedCoupons = ICF.ComputeReceivedCouponsLocalCCY(Position, Instrument);
            if (DataCacheReceivedCouponsLocalCCY.ContainsKey(PositionId))
                DataCacheReceivedCouponsLocalCCY[PositionId] = ReceivedCoupons;
            else
                DataCacheReceivedCouponsLocalCCY.Add(PositionId, ReceivedCoupons);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            ICF.Close();
        }
    }

    public class DataSourceTMZAccrued
    {
        // Holds the instance of the singleton class
        private static DataSourceTMZAccrued Instance = null;

        public static Hashtable DataCacheTMZAccrued;//Cache pour l'accrued TMZ par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceTMZAccrued()
        {
            DataCacheTMZAccrued = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceTMZAccrued GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceTMZAccrued();
            return Instance;
        }

        public double GetTMZAccrued(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
            if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
            {
                VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Instrument.GetCode();
            CSMMarketData context = CSMMarketData.GetCurrentMarketData();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheTMZAccrued.ContainsKey(PositionIdentifier))
                        DataCacheTMZAccrued[PositionIdentifier] = 0.0;
                    else
                        DataCacheTMZAccrued.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide
                if (!DataCacheTMZAccrued.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheTMZAccrued.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheTMZAccrued[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            //mise à jour du TMZAccrued
            CFCompute ICF = CFCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            double TMZAccrued = ICF.ComputeTMZAccrued(Position, Instrument);
            if (DataCacheTMZAccrued.ContainsKey(PositionId))
                DataCacheTMZAccrued[PositionId] = TMZAccrued;
            else
                DataCacheTMZAccrued.Add(PositionId, TMZAccrued);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            ICF.Close();
        }
    }

    public class DataSourceFixedOrFloat
    {
        // Holds the instance of the singleton class
        private static DataSourceFixedOrFloat Instance = null;

        public static Hashtable DataCacheFixedOrFloat;//Cache pour les valeurs de coupon par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version des intruments
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceFixedOrFloat()
        {
            DataCacheFixedOrFloat = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceFixedOrFloat GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceFixedOrFloat();
            return Instance;
        }

        public double GetFixedOrFloat(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //La valeur est (re)calculée:
                //si le cache est vide 
                //si la version de l'instrument change
                //si le spot change
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                if (!DataCacheFixedOrFloat.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheFixedOrFloat.ContainsKey(Instrumentcode))
                {
                    return (double)DataCacheFixedOrFloat[Instrumentcode];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        /// <summary>
        /// Executes the SQL query and stores the results in the DataCache
        /// </summary>
        /// <param name="PortfolioCode">Code of the portfolio in question</param>
        private void FillCache(CSMInstrument Instrument)
        {
            //mise à jour du Coupon
            CFCompute ICF = CFCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            double FixedOrFloat = ICF.ComputeFixedOrFloat(Instrument);
            if (DataCacheFixedOrFloat.ContainsKey(InstrumentCode))
                DataCacheFixedOrFloat[InstrumentCode] = FixedOrFloat;
            else
                DataCacheFixedOrFloat.Add(InstrumentCode, FixedOrFloat);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode);
            if (DataCacheSpot.ContainsKey(InstrumentCode))
                DataCacheSpot[InstrumentCode] = Spot;
            else
                DataCacheSpot.Add(InstrumentCode, Spot);

            ICF.Close();
        }
    }
}
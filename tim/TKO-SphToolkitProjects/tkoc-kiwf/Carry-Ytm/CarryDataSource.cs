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
using sophis.finance;

//@DPH
using Eff.UpgradeUtilities;

namespace dnPortfolioColumn
{
    //Fonctions de Calcul
    public class CarryYtmCompute
    {
        // Holds the instance of the singleton class
        private static CarryYtmCompute Instance = null;
        //private ORCLFactory.ORCLFactory CS_ORCL;

        //Constructeur
        private CarryYtmCompute()
        {
            ORCLFactory.ORCLFactory.Initialize();
        }

        public void Close()
        {
            //CS_ORCL.CloseAll();
        }
        
        /// <summary>
        /// Returns an instance of CarryYtmCompute
        /// </summary>
        public static CarryYtmCompute GetInstance()
        {
            Instance = new CarryYtmCompute();
            return Instance;
        }

        //Calcul des éléments à renvoyer dans les colonnes Carry-Ytm
        public double ComputeInvestedYTM(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double investedytm = 0;
            try
            {
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
                int PositionMvtident = PositionPtr.GetIdentifier();
                double dirtyprice;
                double couponontrade;
                int ydate;

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        //on récupère la transaction date qui sera la settlement et la ownership date
                        ydate = ORCLFactory.ORCLFactory.getResultI("select MAX(date_to_num(DATENEG)) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and date_to_num(DATENEG) <= " + reportingdate + "");
                        //couponontrade = ORCLFactory.ORCLFactory.getResultD("select COUPON from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and date_to_num(DATENEG) in (select MAX(date_to_num(DATENEG)) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and date_to_num(DATENEG) <= " + reportingdate + ")");
                        dirtyprice = ORCLFactory.ORCLFactory.getResultD("select COUPON+COURS as PRIX from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and date_to_num(DATENEG) in (select MAX(date_to_num(DATENEG)) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and date_to_num(DATENEG) <= " + reportingdate + ")");

                        //si on est après la date de reporting ca renvoie 0 et donc on ne fait rien

                        investedytm = 0;
                        if (ydate != 0)
                        {
                            //dirtyprice = PositionPtr.GetAveragePrice() + couponontrade;
                            investedytm = InstrumentPtr.GetYTMByDirtyPrice(ydate, ydate, ydate, dirtyprice);
                            CSMLog.Write("", "", CSMLog.eMVerbosity.M_warning, "Invested YTM is " + investedytm + " for Instrument " + InstrumentPtr.GetCode() + "");
                            CSMLog.Write("", "", CSMLog.eMVerbosity.M_warning, "Params are Spot : " + dirtyprice + " ; Date : " + ydate + "");
                        }
                        break;
                    
                    case "A"://Action
                        //pas implémenté, on pourrait mettre le dividend yield
                        investedytm = 0;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            //on récupère la transaction date qui sera la settlement et la ownership date
                            ydate = ORCLFactory.ORCLFactory.getResultI("select MAX(date_to_num(DATENEG)) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and date_to_num(DATENEG) <= " + reportingdate + "");
                            couponontrade = ORCLFactory.ORCLFactory.getResultD("select COUPON from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and date_to_num(DATENEG) in (select MAX(date_to_num(DATENEG)) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and date_to_num(DATENEG) <= " + reportingdate + ")");

                            //si on est après la date de reporting ca renvoie 0 et donc on ne fait rien
                            if (ydate != 0)
                            {
                                dirtyprice = PositionPtr.GetAveragePrice() + couponontrade;
                                investedytm = InstrumentPtr.GetYTMByDirtyPrice(ydate, ydate, ydate, dirtyprice);

                            }
                        }
                        else//option
                        {
                            investedytm = 0;
                        }
                        break;
                    default:
                        break;
                }
                return investedytm;
            }
            catch (Exception)
            {
                Console.WriteLine("investedytm cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
        
        public double ComputeCarryAtInvDate(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double dcarryinv = 0;
            try
            {
                double investedytm;
                double invcash;
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        investedytm = ComputeInvestedYTM(PositionPtr, InstrumentPtr);
                        invcash = ComputeInvestedCash(PositionPtr, InstrumentPtr);
                        dcarryinv = -invcash * investedytm / 365;
                        break;
                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            investedytm = ComputeInvestedYTM(PositionPtr, InstrumentPtr);
                            invcash = ComputeInvestedCash(PositionPtr, InstrumentPtr);
                            dcarryinv = -invcash * investedytm / 365;
                        }
                        else//option
                        {
                            dcarryinv = 0;
                        }
                        break;
                    default:
                        break;
                }
                return dcarryinv * fxspot;
            }
            catch(Exception)
            {
                Console.WriteLine("dcarryinv cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
        
        public double ComputeDailyCarryAct(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double dcarryact = 0;
            try
            {
                double nominal;
                double notional;
                double YTM;
                double dirtyprice;
                CSMBond Bond;
                System.Collections.ArrayList explicationArray;
                SSMRedemption IthRedemption;
                int nbCF;
                int SettlementShift;
                int PariPassudate;

                CSMMarketData Context = CSMMarketData.GetCurrentMarketData();
                sophis.static_data.eMDayCountBasisType DayCountBasisType;
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());

                int InstrumentCode = InstrumentPtr.GetCode();
                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam
                        DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                        SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                        PariPassudate = Bond.GetPariPassuDate(reportingdate);
                        dirtyprice = Bond.GetDirtyPriceByZeroCoupon(Context, reportingdate, Bond.GetAdjustedDate(reportingdate + SettlementShift), PariPassudate);
                        dirtyprice = dirtyprice / 100; //DirtyPrice en %
                        explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);
                        nbCF = explicationArray.Count;
                        notional = InstrumentPtr.GetNotional();
                        nominal = PositionPtr.GetInstrumentCount() * notional;

                        //Variables relatives au ième flux
                        dcarryact = -dirtyprice * nominal;
                        for (int j = 0; j < nbCF; j++)
                        {
                            IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                            dcarryact += (IthRedemption.coupon + IthRedemption.redemption) * PositionPtr.GetInstrumentCount();
                        }
                        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[nbCF - 1];
                        dcarryact = dcarryact / (CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(reportingdate, IthRedemption.endDate));
                        break;

                    case "S":
                        CSMIssuer Issuer = CSMIssuer.GetInstance(InstrumentPtr.GetIssuerCode());
                        if (Issuer.GetFirstDefaultDate(0, 0) <= reportingdate && Issuer.GetFirstDefaultDate(0, 0) > 0)
                        {
                            dcarryact = 0;
                        }
                        else
                        {
                            double SP;//Probabilité de survie
                            nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                            MarketIndicCompute MarketIndic = MarketIndicCompute.GetInstance();
                            SP = 1 - MarketIndic.ComputeDefaultProbability(InstrumentPtr, PositionPtr);//Je ne suis pas du tout sure que cette proba de survie soit bonne. C'est ce qui était fait dans l'ancien reporting
                            dcarryact = ComputeDailyCarryCoupon(PositionPtr, InstrumentPtr) / fxspot + SP * (-PositionPtr.GetAssetValue() * 1000 / (InstrumentPtr.GetExpiry() - reportingdate));
                        }
                        break;

                    case "P"://Repos
                        double dcarrycash = 0;

                        nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        //Si on fait un repo sur autre chose que du fixed income
                        if (nominal == 0) { nominal = PositionPtr.GetInstrumentCount(); }

                        int PositionMvtident = PositionPtr.GetIdentifier();
                        int reratenumber = ORCLFactory.ORCLFactory.getResultI("select count(*) from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ")");
                        CSMLoanAndRepo CS_Loan = CSMLoanAndRepo.GetInstance(InstrumentPtr.GetCode());
                        double taux_initial = CS_Loan.GetMarginOnCollateral();
                        //On teste si c'est un open repo
                        double openrepo = InstrumentPtr.GetExpiry();
                        if (reratenumber.Equals(0) && openrepo.Equals(0))
                        { dcarrycash = taux_initial * nominal / 360; }
                        if (reratenumber.Equals(0) && openrepo != 0)
                        { dcarrycash = taux_initial * nominal / 360; }
                        if (reratenumber != 0)
                        {
                            //On récupère la start date du repo
                            int segment_date_start = InstrumentPtr.GetStartDate();
                            int initial_date = segment_date_start;

                            //On prend la value date du 1er deal rerate et le rerate mais on ne s'en sert pas pour l'instant
                            int segment_date_end = ORCLFactory.ORCLFactory.getResultI("select * from (select date_to_num(dateval) from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ") and DATEVAL> num_to_date(" + initial_date + ") order by DATEVAL ASC) where ROWNUM=1");
                            double segment_taux = ORCLFactory.ORCLFactory.getResultD("select * from (select cours from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ") and DATEVAL> num_to_date(" + initial_date + ") order by DATEVAL ASC) where ROWNUM=1") * 0.01;

                            //on entre dans une boucle qui démarre sur le second rerate et qui s'arrête à l'avant dernier
                            for (int j = 2; j < reratenumber + 1; j++)
                            {
                                segment_taux = ORCLFactory.ORCLFactory.getResultD("select * from (select cours from (select cours,RANK() OVER (order by h.DATEVAL ASC) r from histomvts h where h.TYPE=28 and h.MVTIDENT=" + PositionMvtident + " and h.DATEVAL < num_to_date (" + reportingdate + ") and h.DATEVAL> num_to_date(" + initial_date + ")) where r=" + j + " )") * 0.01;
                            }
                            //on calcule le daily carry cash
                            dcarrycash = segment_taux * nominal / 360;
                        }
                        dcarryact = dcarrycash;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam
                            DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                            SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                            PariPassudate = Bond.GetPariPassuDate(reportingdate);
                            dirtyprice = Bond.GetDirtyPriceByZeroCoupon(Context, reportingdate, Bond.GetAdjustedDate(reportingdate + SettlementShift), PariPassudate);
                            dirtyprice = dirtyprice / 100; //DirtyPrice en %
                            explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);
                            nbCF = explicationArray.Count;
                            notional = InstrumentPtr.GetNotional();
                            nominal = PositionPtr.GetInstrumentCount() * notional;

                            //Variables relatives au ième flux
                            dcarryact = -dirtyprice * nominal;
                            for (int j = 0; j < nbCF; j++)
                            {
                                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                                dcarryact += (IthRedemption.coupon + IthRedemption.redemption) * PositionPtr.GetInstrumentCount();
                            }
                            IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[nbCF - 1];
                            dcarryact = dcarryact / (CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(reportingdate, IthRedemption.endDate));
                        }
                        else//option
                        {
                            dcarryact = 0;
                        }
                        break;

                    case "T"://Billets de tréso
                        DayCountBasisType = InstrumentPtr.GetMarketAIDayCountBasisType();
                        YTM = InstrumentPtr.GetYTMMtoM();
                        if (YTM.Equals(0)) { YTM = InstrumentPtr.GetYTM(); } //si on a pas de prix, on prend le yield théorique
                        SettlementShift = InstrumentPtr.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                        PariPassudate = InstrumentPtr.GetPariPassuDate(reportingdate);
                        dirtyprice = InstrumentPtr.GetDirtyPriceByYTM(reportingdate, reportingdate + SettlementShift, PariPassudate, YTM);
                        notional = InstrumentPtr.GetNotional();
                        nominal = PositionPtr.GetInstrumentCount() * notional;
                        dcarryact = -dirtyprice + nominal;
                        dcarryact = dcarryact / (CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(reportingdate, InstrumentPtr.GetExpiry()));
                        break;

                    default:
                        break;
                }

                return dcarryact * fxspot;
            }
            catch (Exception)
            {
                Console.WriteLine("dcarryeco cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
        
        public double ComputeDailyCarryCoupon(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double dcarrycoupon = 0;
            try
            {
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                double nominal = 0;

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        dcarrycoupon = nominal * ComputeFirstCoupon(InstrumentPtr) / 365;
                        break;

                    case "A"://Actions
                        dcarrycoupon = 0;
                        break;

                    case "S"://Swap
                        CSMIssuer Issuer = CSMIssuer.GetInstance(InstrumentPtr.GetIssuerCode());
                        if (Issuer.GetFirstDefaultDate(0,0) <= reportingdate && Issuer.GetFirstDefaultDate(0,0) > 0)
                        {
                            dcarrycoupon = 0;
                        }
                        else
                        {
                            nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                            double avp = ORCLFactory.ORCLFactory.getResultD("select fixe2 from TITRES where SICOVAM=" + InstrumentPtr.GetCode() + "") * 100;
                            dcarrycoupon = ((Math.Pow(1 + (avp / 10000) / 4, 4) - 1) * -nominal) / 365;
                        }
                        break;

                    case "P"://Repo
                        nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        //Si on fait un repo sur autre chose que du fixed income
                        if (nominal == 0) { nominal = PositionPtr.GetInstrumentCount(); }

                        int PositionMvtident = PositionPtr.GetIdentifier();
                        int reratenumber = ORCLFactory.ORCLFactory.getResultI("select count(*) from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ")");
                        CSMLoanAndRepo CS_Loan = CSMLoanAndRepo.GetInstance(InstrumentPtr.GetCode());
                        double taux_initial = CS_Loan.GetMarginOnCollateral();
                        //On teste si c'est un open repo
                        double openrepo = InstrumentPtr.GetExpiry();
                        if (reratenumber.Equals(0) && openrepo.Equals(0))
                        { dcarrycoupon = taux_initial * nominal / 365; }
                        if (reratenumber.Equals(0) && openrepo != 0)
                        { dcarrycoupon = taux_initial * nominal / 365; }
                        if (reratenumber != 0)
                        {
                            //On récupère la start date du repo
                            int segment_date_start = InstrumentPtr.GetStartDate();
                            int initial_date = segment_date_start;

                            //On prend la value date du 1er deal rerate et le rerate mais on ne s'en sert pas pour l'instant
                            int segment_date_end = ORCLFactory.ORCLFactory.getResultI("select * from (select date_to_num(dateval) from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ") and DATEVAL> num_to_date(" + initial_date + ") order by DATEVAL ASC) where ROWNUM=1");
                            double segment_taux = ORCLFactory.ORCLFactory.getResultD("select * from (select cours from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ") and DATEVAL> num_to_date(" + initial_date + ") order by DATEVAL ASC) where ROWNUM=1") * 0.01;

                            //on entre dans une boucle qui démarre sur le second rerate et qui s'arrête à l'avant dernier
                            for (int j = 2; j < reratenumber + 1; j++)
                            {
                                segment_taux = ORCLFactory.ORCLFactory.getResultD("select * from (select cours from (select cours,RANK() OVER (order by h.DATEVAL ASC) r from histomvts h where h.TYPE=28 and h.MVTIDENT=" + PositionMvtident + " and h.DATEVAL < num_to_date (" + reportingdate + ") and h.DATEVAL> num_to_date(" + initial_date + ")) where r=" + j + " )") * 0.01;
                            }
                            //on calcule le daily carry cash
                            dcarrycoupon = segment_taux * nominal / 365;
                        }
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                            dcarrycoupon = nominal * ComputeFirstCoupon(InstrumentPtr) / 365;
                        }
                        else//option
                        {
                            dcarrycoupon = 0;
                        }
                        break;

                    case "T"://billets de trésorerie
                        nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        dcarrycoupon = nominal * ComputeFirstCoupon(InstrumentPtr) / 365;
                        break;

                    default:
                        break;
                }
                return dcarrycoupon * fxspot;
            }
            catch (Exception)
            {
                Console.WriteLine("dcarrycoupon cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }

        public string TKORatingComposite(CSMInstrument InstrumentPtr)
        {
            string rating="NR";
            int InstrumentCode = InstrumentPtr.GetCode();
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            CMString name = new CMString();
                InstrumentPtr.GetName(name); //just FYI 
                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                    CSMBond Bond = CSMBond.GetInstance(InstrumentCode);
                    ArrayList agencies = new ArrayList();
                    Bond.GetAvailableRatingAgencies(agencies);
                    
                    break;

                    default:
                    break;
                }

            return rating;
        }

       
        public double TestComputeYtm(CSMInstrument InstrumentPtr)
        {
            double tkoYtm = 0;
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            int InstrumentCode = InstrumentPtr.GetCode();
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché

            try
            {
                CMString name = new CMString();
                InstrumentPtr.GetName(name); //just FYI 
                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        InstrumentPtr.GetModelName(name);
                        double poolFactor = 1;
                        double pikFactor = 1;
                        if (name == "ABS Bond") //on distingue les Bonds types PIK (implémentés façon Ecobat)
                        {
                            CSMABSBond pikBond = CSMABSBond.GetInstance(InstrumentCode);
                            poolFactor = pikBond.GetValidPoolFactorAt(reportingdate);
                            pikBond.GetPiKElligibility(reportingdate);
                            
                        }
                       
                        CSMBond Bond = CSMBond.GetInstance(InstrumentCode);

                        int Maturity = Bond.GetExpiry();
                        double Nominal = Bond.GetNotional()*poolFactor;
                        Bond.GetInstrumentSpread();
                        Bond.GetPaymentMethod();
                        
                        Bond.GetFloatingRate();
                        
                        if (Bond.IsPiKElligible())
                        {
                            double cpnRate = Bond.GetNotionalRate();
                            System.Collections.ArrayList explicationArrayForPik = GetBondExplanationArray(InstrumentCode, reportingdate);
                            int numbCF = explicationArrayForPik.Count;
                           
                            sophis.static_data.eMDayCountBasisType DayCountBasisTypePik = Bond.GetMarketAIDayCountBasisType();
                            SSMRedemption IthRedemptionPik;
                            double IthDatePik;
                            int prevDate = reportingdate - Bond.GetAccrualPeriod(reportingdate,false)+1; //lastCouponDate;

                            for (int k = 0; k < numbCF; k++)
                            {

                                IthRedemptionPik = (sophis.instrument.SSMRedemption)explicationArrayForPik[k];
                                IthDatePik = (double)CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisTypePik).GetEquivalentDayCount(prevDate, IthRedemptionPik.endDate) / 360;
                                
                                if (IthRedemptionPik.coupon == 0) // pas de coupon versé, seulement du Pik.
                                {
                                    pikFactor = pikFactor * (1 + cpnRate / 100 * IthDatePik);
                                }

                                prevDate = IthRedemptionPik.endDate;
                                
                            }
                           
                        }

                        ///// Test
                        /* ArrayList agencies = new ArrayList();
                        Bond.GetAvailableRatingAgencies(agencies);
                        ArrayList scaleList = new ArrayList();
                        Bond.GetAvailableRatingAgencies(scaleList);

                        CSRRatingSource source = new CSRRatingSource();
                        CSMRatingScale ratingScale = Bond.GetRatingScale(100, 102);
                        ratingScale.GetName();
                         */
                        /////

                        //////////////
                        //Méthode de Newton
                        //////////////
                        double x = 0;//point de départ de l'algorithme 
                        double eps = 1000; //erreur
                        double f_x = 0; //fonction dont on recherche le zéro
                        double fp_x = 0; // dérivée de la fonction dont on recherche le zéro

                        //Travail sur les flux restants de l'obligation (table "Explanation" de Sophis)

                        double DirtyPrice = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode) + InstrumentPtr.GetAccruedCoupon();
                        sophis.static_data.eMDayCountBasisType DayCountBasisType = Bond.GetMarketAIDayCountBasisType();
                        System.Collections.ArrayList explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);
                        
                        int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                        SSMRedemption IthRedemption;
                        double IthDate;//Date avec laquelle on actualise le ième CF
                        double IthCoupon;//ième flux
                        double IthDiscountFactor;
                        int nbiter = 0;
                        while (Math.Abs(eps) > 0.000001 && nbiter < 1000)//précision
                        {
                            f_x = 0;
                            fp_x = 0;
                            for (int j = 0; j < nbCF; j++)
                            {
                                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                                IthCoupon = IthRedemption.coupon + IthRedemption.redemption * pikFactor;
                                IthDate =(double) CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(reportingdate, IthRedemption.endDate)/360; //GetEquivalentYearCount //GetDoubleEquivalentYearCount
                                //IthDiscountFactor = Math.Exp(-c * IthDate);
                                IthDiscountFactor = 1 / Math.Pow(1 + x, IthDate);
                                f_x += IthCoupon * IthDiscountFactor;
                                fp_x += -IthDate * IthCoupon * 1 / Math.Pow(1 + x, IthDate + 1);
                            }
                            f_x = f_x / Nominal - DirtyPrice * 0.01;
                            fp_x = fp_x / Nominal;

                            x = x - f_x / fp_x; // construction par récurrence
                            eps = f_x;
                            nbiter++;
                        }
                        tkoYtm = x;


                        //Si le YTM déconne trop, on essaye de renvoyer celui de Sophis
                        //if (tkoYtm >= 0.95 | tkoYtm <= 0)
                        // {
                        //  tkoYtm = CSMBond.GetInstance(InstrumentCode).GetYTMMtoM();
                        // }

                        //Si le YTM est trop éloigné de celui de Sophis, on renvoie celui de Sophis - ECOBATstyle
                        /*
                        if ((tkoYtm - CSMBond.GetInstance(InstrumentCode).GetYTMMtoM()) >= 0.1)
                        {
                           
                            tkoYtm = CSMBond.GetInstance(InstrumentCode).GetYTMMtoM();
                        }*/

                        break;
                    case "S":
                        break;

                    case "T": //TCN 
                        //CSMBond TCN = CSMBond.GetInstance(InstrumentCode);
                        CSMDebtInstrument TCN = CSMDebtInstrument.GetInstance(InstrumentCode);
                        
                        /*
                        int mat = TCN.GetExpiry();
                        double nomi = TCN.GetNotional();
                        double DirtyPriceTCN = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode) + InstrumentPtr.GetAccruedCoupon();
                        */
                        
                        tkoYtm = TCN.GetRate()*0.01;
                        break;
                    
                    default:
                        tkoYtm = 0;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            Maturity = InstrumentPtr.GetExpiry();
                            //Quand on est a 1mois de la maturité, on considère que le prix ne doit 
                            //plus avoir plus avoir d'influence sur le ytm: le rendement annuel ercu est alors limité au coupon
                            if (Maturity - reportingdate < 30)
                            {
                                tkoYtm = ComputeFirstCoupon(InstrumentPtr);
                            }
                            else
                            {
                                tkoYtm = InstrumentPtr.GetYTMMtoM();
                            }
                        }
                        break;
                }
                return tkoYtm;

            }
            catch (Exception)
            {
                CSMLog.Write("", "TKO YTM", CSMLog.eMVerbosity.M_warning, "Test Tko Ytm cannot be computed for bond Instrument " + InstrumentCode);
                return 0;
            }
            
        }

        public double[] getNewtonValues(CSMInstrument InstrumentPtr, double x)
        {
            double[] results = new double[2];
           
            double f_x = 0; //fonction dont on recherche le zéro : les cashFlows du bond moins le dirty price
            double fp_x = 0; // dérivée de la fonction dont on recherche le zéro

            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            int InstrumentCode = InstrumentPtr.GetCode();
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            
            /*CMString name = new CMString();
            InstrumentPtr.GetName(name);*/
            if (InstrumentPtr.GetInstrumentType().ToString() == "O") //on se limite aux obligations classiques 
            {
                string name = "";
                InstrumentPtr.GetModelName(name);
                double poolFactor = 1;
                double pikFactor = 1;
                if (name == "ABS Bond") //on distingue les Bonds types type old PIK (Ecobat)
                {
                    CSMABSBond pikBond = CSMABSBond.GetInstance(InstrumentCode);
                    poolFactor = pikBond.GetValidPoolFactorAt(reportingdate); //pik
                }

                CSMBond Bond = CSMBond.GetInstance(InstrumentCode);
                int Maturity = Bond.GetExpiry();
                double Nominal = Bond.GetNotional()*poolFactor;

                //Travail sur les flux restants de l'obligation (table "Explanation" de Sophis)

                double DirtyPrice = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode) + InstrumentPtr.GetAccruedCoupon();
                sophis.static_data.eMDayCountBasisType DayCountBasisType = Bond.GetMarketAIDayCountBasisType();
                System.Collections.ArrayList explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);

                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthDiscountFactor;

                if (Bond.IsPiKElligible())
                {
                    double cpnRate = Bond.GetNotionalRate();
                    System.Collections.ArrayList explicationArrayForPik = GetBondExplanationArray(InstrumentCode, reportingdate);
                    int numbCF = explicationArrayForPik.Count;

                    sophis.static_data.eMDayCountBasisType DayCountBasisTypePik = Bond.GetMarketAIDayCountBasisType();
                    SSMRedemption IthRedemptionPik;
                    double IthDatePik;
                    int prevDate = reportingdate - Bond.GetAccrualPeriod(reportingdate, false) + 1; //lastCouponDate;

                    for (int k = 0; k < numbCF; k++)
                    {

                        IthRedemptionPik = (sophis.instrument.SSMRedemption)explicationArrayForPik[k];
                        IthDatePik = (double)CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisTypePik).GetEquivalentDayCount(prevDate, IthRedemptionPik.endDate) / 360;

                        if (IthRedemptionPik.coupon == 0) // pas de coupon versé, seulement du Pik.
                        {
                            pikFactor = pikFactor * (1 + cpnRate / 100 * IthDatePik);
                        }

                        prevDate = IthRedemptionPik.endDate;

                    }

                }
           
                f_x = 0;
                fp_x = 0;
           
                for (int j = 0; j < nbCF; j++)
                {
                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                    IthCoupon = IthRedemption.coupon + IthRedemption.redemption * pikFactor;
                    IthDate = (double)CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(reportingdate, IthRedemption.endDate)/360; //.GetEquivalentYearCount
                    //IthDiscountFactor = Math.Exp(-c * IthDate);
                    IthDiscountFactor = 1 / Math.Pow(1 + x, IthDate);
                    f_x += IthCoupon * IthDiscountFactor;
                    fp_x += -IthDate * IthCoupon * 1 / Math.Pow(1 + x, IthDate + 1);
                }
                f_x = f_x / Nominal - DirtyPrice * 0.01;
                fp_x = fp_x / Nominal;
            }
            results[0] = f_x;
            results[1] = fp_x;

            /*if (InstrumentPtr.GetReference() == "XS0288291833 Corp") //Ecobat
            {
                CMString name = new CMString();
                InstrumentPtr.GetName(name);
                results[0] = 0;
                results[1] = 0; 
            }*/

            return results;
            //Si le YTM déconne trop, on essaye de renvoyer celui de Sophis
            //if (tkoYtm >= 0.95 | tkoYtm <= 0)
            // {
            //  tkoYtm = CSMBond.GetInstance(InstrumentCode).GetYTMMtoM();
            // }

            //Si le YTM est trop éloigné de celui de Sophis, on renvoie celui de Sophis - ECOBATstyle
           /* if ((tkoYtm - CSMBond.GetInstance(InstrumentCode).GetYTMMtoM()) >= 0.1)
            {
                tkoYtm = CSMBond.GetInstance(InstrumentCode).GetYTMMtoM();
            }*/

        }

        public double ComputeYtm(CSMInstrument InstrumentPtr)
        {
            double tkoYtm = 0;
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            int InstrumentCode = InstrumentPtr.GetCode();
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            try
            {
                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        CSMBond Bond = CSMBond.GetInstance(InstrumentCode);
                        int Maturity = Bond.GetExpiry();
                        double Nominal = Bond.GetNotional();

                        //rl pour les ABS Bonds à implémenter...
                        //CSMABSBond Bond2 = CSMABSBond.GetInstance(InstrumentCode);
                        //int Maturity = Bond2.GetExpiry();
                        //double Nominal = Bond2.GetNotionalInProduct();


                        //Quand on est a 1mois de la maturité, on considère que le prix ne doit 
                        //plus avoir plus avoir d'influence sur le ytm: le rendement annuel ercu est alors limité au coupon
                        if (Maturity - reportingdate < 30)
                        {
                            tkoYtm = ComputeFirstCoupon(InstrumentPtr);
                        }

                        else
                        {
                            //Méthode de calcul dichotomique
                            double a, b, c;
                            a = -0.5; b = 1;//Limites fixées: le ytm est fixé entre -50% et 100%
                            double DirtyPriceTest = 0;

                            //Travail sur les flux restants de l'obligation (table "Explanation" de Sophis)
                            int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                            int PariPassudate = Bond.GetPariPassuDate(reportingdate);
                            //double DirtyPrice = Bond.GetDirtyPriceByZeroCoupon(Context, reportingdate, Bond.GetAdjustedDate(reportingdate + SettlementShift), PariPassudate); // mauvaise methode
                            double DirtyPrice = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode) + InstrumentPtr.GetAccruedCoupon();
                            sophis.static_data.eMDayCountBasisType DayCountBasisType = Bond.GetMarketAIDayCountBasisType();
                            if (DayCountBasisType.ToString().Equals("M_dcbActual_Actual_ISMA_ISDA06")) { DayCountBasisType = (eMDayCountBasisType)4; }//Correction d'un bug quand la base Actual_Actual_ISMA_ISDA06 est utilisée (on la remplace par la base Actual/360)
                            //rl bug corrigé en 4.1 donc désactivé
                            System.Collections.ArrayList explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);
                            int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                            SSMRedemption IthRedemption;
                            double IthDate;//Date avec laquelle on actualise le ième CF
                            double IthCoupon;//ième flux
                            double IthDiscountFactor;
                            int nbiter = 0;
                            while ((b - a) > Math.Pow(10, -5) && nbiter < 1000)//précision
                            {
                                c = (a + b) * 0.5;
                                DirtyPriceTest = 0;
                                for (int j = 0; j < nbCF; j++)
                                {
                                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                                    IthCoupon = IthRedemption.coupon + IthRedemption.redemption;
                                    IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                                    //IthDiscountFactor = Math.Exp(-c * IthDate);
                                    IthDiscountFactor = 1 / Math.Pow(1 + c, IthDate);
                                    DirtyPriceTest += IthCoupon * IthDiscountFactor;
                                }
                                DirtyPriceTest = DirtyPriceTest / Nominal;
                                if (DirtyPriceTest > DirtyPrice * 0.01)
                                {
                                    a = c;
                                }
                                else
                                {
                                    b = c;
                                }
                                nbiter++;
                            }
                            tkoYtm = ((a + b) * 0.5);

                            //Si le YTM déconne trop, on essaye de renvoyer celui de Sophis
                            if (tkoYtm >= 0.95 | tkoYtm <= 0)
                            {
                                tkoYtm = CSMBond.GetInstance(InstrumentCode).GetYTMMtoM();
                            }

                            //Si le YTM est trop éloigné de celui de Sophis, on renvoie celui de Sophis - ECOBATstyle
                            if ((tkoYtm - CSMBond.GetInstance(InstrumentCode).GetYTMMtoM()) >= 0.1)
                            {
                                tkoYtm = CSMBond.GetInstance(InstrumentCode).GetYTMMtoM();
                            }
                        }
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            Maturity = InstrumentPtr.GetExpiry();
                            //Quand on est a 1mois de la maturité, on considère que le prix ne doit 
                            //plus avoir plus avoir d'influence sur le ytm: le rendement annuel ercu est alors limité au coupon
                            if (Maturity - reportingdate < 30)
                            {
                                tkoYtm = ComputeFirstCoupon(InstrumentPtr);
                            }
                            else
                            {
                                tkoYtm = InstrumentPtr.GetYTMMtoM();
                                ////RQ: Attention. Pour les convertibles, la table des flux est différente de celle des bonds
                                ////Elle prend en compte tous les flux depuis l'emission
                                ////Redemption.coupon renvoie le coupon en % et non le montant du coupon
                                ////IL FAUT SAVOIR RECUPERER LE QUOTATION TYPE --TRAITEMENT DIFFERENT SELON LE TYPE
                                ////JE NE TROUVE PAS POUR L'INSTANT
                                ////Méthode de calcul dichotomique
                                //double a, b, c;
                                //a = -0.5; b = 1;//Limites fixées: le ytm est fixé entre -50% et 100%
                                //double DirtyPriceTest = 0;

                                ////Travail sur les flux restants de l'obligation (table "Explanation" de Sophis)
                                //double Notional = InstrumentPtr.GetNotional();
                                //double DirtyPrice = InstrumentPtr.GetValueInPrice() / 100;
                                //sophis.static_data.eMDayCountBasisType DayCountBasisType = InstrumentPtr.GetMarketAIDayCountBasisType();
                                //if (DayCountBasisType.ToString().Equals("M_dcbActual_Actual_ISMA_ISDA06")) { DayCountBasisType = (eMDayCountBasisType)4; }//Correction d'un bug quand la base Actual_Actual_ISMA_ISDA06 est utilisée (on la remplace par la base Actual/360)
                                //System.Collections.ArrayList explicationArray = new ArrayList();
                                //InstrumentPtr.GetRedemption(explicationArray, InstrumentPtr.GetIssueDate(), InstrumentPtr.GetExpiry());
                                //int nbCF = explicationArray.Count;
                                //SSMRedemption IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[0];
                                //int i = 0;
                                //while (i + 1 < nbCF && IthRedemption.endDate < reportingdate)
                                //{
                                //    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[i + 1];
                                //    i++;
                                //}
                                //double IthDate;//Date avec laquelle on actualise le ième CF
                                //double IthCoupon;//ième flux
                                //double IthDiscountFactor;
                                //int nbiter = 0;
                                //while ((b - a) > Math.Pow(10, -5) && nbiter < 1000)//précision
                                //{
                                //    c = (a + b) * 0.5;
                                //    DirtyPriceTest = 0;
                                //    for (int j = i; j < nbCF; j++)
                                //    {
                                //        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                                //        IthCoupon = IthRedemption.coupon * Notional + IthRedemption.redemption;
                                //        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                                //        //IthDiscountFactor = Math.Exp(-c * IthDate);
                                //        IthDiscountFactor = 1 / Math.Pow(1 + c, IthDate);
                                //        DirtyPriceTest += IthCoupon * IthDiscountFactor;
                                //        CSMApi.Log(" c" + c, true);
                                //        CSMApi.Log(" IthDate" + IthDate, true);
                                //        CSMApi.Log("IthRedemption.coupon * Notional" + IthRedemption.coupon * Notional, true);
                                //        CSMApi.Log(" IthRedemption.redemption" + IthRedemption.redemption, true);
                                //    }
                                //    DirtyPriceTest = DirtyPriceTest / Notional;
                                //    CSMApi.Log(" DirtyPriceTest" + DirtyPriceTest, true);
                                //    CSMApi.Log(" DirtyPrice" + DirtyPrice, true);
                                //    if (DirtyPriceTest > DirtyPrice)
                                //    {
                                //        a = c;
                                //    }
                                //    else
                                //    {
                                //        b = c;
                                //    }
                                //    nbiter++;
                                //}
                                //tkoYtm = ((a + b) * 0.5);
                            }
                        }
                        break;

                   case "S":
                        tkoYtm = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode) * 0.01;
                        break;

                   default:
                        tkoYtm = 0;
                        break;
                }
                return tkoYtm;
            }

            //Méthode Sophis
            //CSMBond Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam
            //CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            //int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
            //int PariPassudate = Bond.GetPariPassuDate(reportingdate);
            //int OwnershipShift = Bond.GetSettlementShift();
            //double Dirty = Bond.GetDirtyPriceByZeroCoupon(Context, reportingdate, reportingdate + SettlementShift, PariPassudate);
            //tkoYtm = Bond.GetYTMByDirtyPrice(reportingdate, reportingdate + SettlementShift, reportingdate + OwnershipShift, Dirty);
            //return tkoYtm;
                            
            catch (Exception)
            {
                CSMLog.Write("", "TKO YTM", CSMLog.eMVerbosity.M_warning, "Tko Ytm cannot be computed for bond Instrument " + InstrumentCode);
                return 0;
            }
        }

        public double ComputeAYtmTRI(CSMPortfolio PortfolioPtr)
        {
            double tkoYtm = 0;

            //Déclarations du contexte
            double strategyAV = 0;//asset value des instruments pris en compte dans le calcul du rendement du fond
            CSMPosition positionPtr;
            CSMInstrument instrumentPtr;
            int instrumentCode;
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

            //Déclarations pour les bonds
            CSMBond Bond;
            int Maturity;
            double Notional;
            double InstrumentCount;
            int SettlementShift;
            int PariPassudate;
            double DirtyPrice;
            sophis.static_data.eMDayCountBasisType DayCountBasisType;
            System.Collections.ArrayList explicationArray;
            int nbCF;
            SSMRedemption IthRedemption;
            double IthDate;//Date avec laquelle on actualise le ième CF
            double IthCoupon;//ième flux
            double IthDiscountFactor;

            //Méthode de calcul dichotomique
            double a, b, c;
            a = -0.5; b = 1;//Limites fixées: le ytm est fixé entre -50% et 100%
            double DirtyPriceTest = 0;
            int nbiter = 0;
            int OpenPositions = 0;
            int positionNumber = PortfolioPtr.GetTreeViewPositionCount();
            if (positionNumber == 0) { tkoYtm = 0; }
            else
            {
                while ((b - a) > Math.Pow(10, -5) && nbiter < 100)//précision
                {
                    c = (a + b) * 0.5;
                    
                    //CSMPortfolio Fund = CSMPortfolio.GetCSRPortfolio(CSMPortfolio.GetCSRPortfolio(PortfolioPtr.GetParentCode()).GetParentCode());
                    //DirtyPriceTest = -(Fund.GetBalance() + Fund.GetUnsettledBalance()) * 1000;
                    //CSMApi.Log("cash du fonds " + DirtyPriceTest,true);
                    //DirtyPriceTest = -(PortfolioPtr.GetBalance() + PortfolioPtr.GetUnsettledBalance()) * 1000;
                    //CSMApi.Log("cash du fonds " + DirtyPriceTest,true);

                    DirtyPriceTest = 0;
                    strategyAV = 0;
                    OpenPositions = 0;
                    for (int index = 0; index < positionNumber; index++)
                    {
                        positionPtr = PortfolioPtr.GetNthTreeViewPosition(index);
                        if (positionPtr.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                        {
                            OpenPositions++;
                            instrumentCode = positionPtr.GetInstrumentCode();
                            instrumentPtr = CSMInstrument.GetInstance(instrumentCode);
                            switch (instrumentPtr.GetInstrumentType().ToString())
                            {
                                case "O"://Obligation
                                    strategyAV += positionPtr.GetAssetValue();

                                    Bond = CSMBond.GetInstance(instrumentCode);
                                    Maturity = Bond.GetExpiry();
                                    Notional = Bond.GetNotional();
                                    InstrumentCount = positionPtr.GetInstrumentCount();

                                    //Travail sur les flux restants de l'obligation (table "Explanation" de Sophis)
                                    SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                                    PariPassudate = Bond.GetPariPassuDate(reportingdate);
                                    DirtyPrice = Bond.GetDirtyPriceByZeroCoupon(Context, reportingdate, Bond.GetAdjustedDate(reportingdate + SettlementShift), PariPassudate);
                                    DayCountBasisType = Bond.GetMarketAIDayCountBasisType();
                                    if (DayCountBasisType.ToString().Equals("M_dcbActual_Actual_ISMA_ISDA06")) { DayCountBasisType = (eMDayCountBasisType)4; }//Correction d'un bug quand la base Actual_Actual_ISMA_ISDA06 est utilisée (on la remplace par la base Actual/360)
                                    explicationArray = GetBondExplanationArray(instrumentCode, reportingdate);
                                    nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                                    DirtyPriceTest -= DirtyPrice / 100 * Notional * InstrumentCount;
                                    for (int j = 0; j < nbCF; j++)
                                    {
                                        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                                        IthCoupon = IthRedemption.coupon + IthRedemption.redemption;
                                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                                        IthDiscountFactor = 1 / Math.Pow(1 + c, IthDate);
                                        DirtyPriceTest += IthCoupon * IthDiscountFactor * InstrumentCount;
                                    }
                                    break;

                                case "D"://Options et convertibles
                                    //RQ: Attention. Pour les convertibles, la table des flux est différente de celle des bonds
                                    //Elle prend en compte tous les flux depuis l'emission
                                    //Redemption.coupon renvoie le coupon en % et non le montant du coupon
                                    //IL FAUT SAVOIR RECUPERER LE QUOTATION TYPE --TRAITEMENT DIFFERENT SELON LE TYPE
                                    //JE NE TROUVE PAS POUR L'INSTANT
                                    CMString otype = "";
                                    instrumentPtr.GetModelName(otype);
                                    string otypes = otype.GetString();
                                    if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                                    {
                                        //Notional = instrumentPtr.GetNotional();
                                        //InstrumentCount = positionPtr.GetInstrumentCount();
                                        //DirtyPrice = instrumentPtr.GetValueInPrice();
                                        //DirtyPriceTest -= DirtyPrice / 100 * Notional * InstrumentCount;
                                        //DayCountBasisType = instrumentPtr.GetMarketAIDayCountBasisType();
                                        //if (DayCountBasisType.ToString().Equals("M_dcbActual_Actual_ISMA_ISDA06")) { DayCountBasisType = (eMDayCountBasisType)4; }//Correction d'un bug quand la base Actual_Actual_ISMA_ISDA06 est utilisée (on la remplace par la base Actual/360)
                                        //explicationArray = new ArrayList();
                                        //instrumentPtr.GetRedemption(explicationArray, instrumentPtr.GetIssueDate(), instrumentPtr.GetExpiry());
                                        //nbCF = explicationArray.Count;
                                        //IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[0];
                                        //int i = 0;
                                        //while (i+1 < nbCF && IthRedemption.endDate < reportingdate)
                                        //{
                                        //    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[i+1];
                                        //    i++;
                                        //}
                                        //for (int j = i; j < nbCF; j++)
                                        //{
                                        //    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                                        //    IthCoupon = (IthRedemption.coupon * Notional + IthRedemption.redemption);
                                        //    IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                                        //    IthDiscountFactor = 1 / Math.Pow(1 + c, IthDate);
                                        //    DirtyPriceTest += IthCoupon * IthDiscountFactor * InstrumentCount;
                                        //}
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    if (DirtyPriceTest > 0)
                    {
                        a = c;
                    }
                    else
                    {
                        b = c;
                    }
                    nbiter++;
                }
                tkoYtm = ((a + b) * 0.5);
            }
            if (OpenPositions == 0) { tkoYtm = 0; }
            CSMLog.Write("", "", CSMLog.eMVerbosity.M_verbose, "PortfolioPtr.GetAssetValue()" + PortfolioPtr.GetAssetValue());
            CSMLog.Write("", "", CSMLog.eMVerbosity.M_verbose, "strategyAV" + strategyAV);
            CSMLog.Write("", "", CSMLog.eMVerbosity.M_verbose, "PortfolioPtr.GetNetAssetValue()" + PortfolioPtr.GetNetAssetValue());
            return tkoYtm * strategyAV / PortfolioPtr.GetNetAssetValue();// (PortfolioPtr.GetBalance() + PortfolioPtr.GetUnsettledBalance());
        }
        
        public double ComputeAYtmApprox(CSMPortfolio PortfolioPtr, double ytmportfolioEstimate)
        {
            double tkoAYtm = 0;//YTM du portefeuille

            //Coefficients du trinôme ax^2 + bx +c = 0 avec x = tkoAYtm
            //La racine solution est x = [-b + racine(delta)] / 2a  
            double a, b, c, delta;
            a = 0; b = 0; c = 0;

            //Déclarations du contexte
            CSMPosition positionPtr;
            CSMInstrument instrumentPtr;
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

            //Caractéristiques d'un instrument
            double Ytm;
            double Maturity;
            double Nominal;
            double Coupon;
            double DirtyPrice;
            double Alpha;
            sophis.static_data.eMDayCountBasisType DayCountBasisType;

            //Calcul approché du rendement actuariel (TRI) d'un portefeuille
            int positionNumber = PortfolioPtr.GetTreeViewPositionCount();
            if (positionNumber == 0) { tkoAYtm = 0; }
            else
            {
                for (int index = 0; index < positionNumber; index++)
                {
                    positionPtr = PortfolioPtr.GetNthTreeViewPosition(index);
                    if (positionPtr.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrumentPtr = CSMInstrument.GetInstance(positionPtr.GetInstrumentCode());

                        //Nominal de la position
                        Nominal = positionPtr.GetInstrumentCount() * instrumentPtr.GetNotional();

                        //Nombre d'années entre reporting date et la maturité de l'instrument
                        DayCountBasisType = instrumentPtr.GetMarketAIDayCountBasisType();
                        if (DayCountBasisType.ToString().Equals("M_dcbActual_Actual_ISMA_ISDA06")) { DayCountBasisType = (eMDayCountBasisType)4; }//Correction d'un bug quand la base Actual_Actual_ISMA_ISDA06 est utilisée (on la remplace par la base Actual/360)
                        Maturity = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, instrumentPtr.GetExpiry());

                        Ytm = DataSourceYtm.GetInstance().GetYtm(positionPtr,instrumentPtr)/100;
                        //Les obligations sont triées en 2 catégories: YM petit devant 1 et YM grand
                        //if (Maturity * Ytm < 1)//obligations à maturité finie et rendement petit
                        //{
                        //    //Coupon de l'instrument: coupon de la période courante. S'il est nul, coupon de la première période de coupon non nulle
                        //    Coupon = DataSourceCoupon.GetInstance().GetCoupon(positionPtr, instrumentPtr) / 100;

                        //    //Coefficient Alpha
                        //    Alpha = 1 + Coupon * 0.5 * (Maturity + 1);//1 + C*(M+1)/2

                        //    //Calcul des coefficients du trinome solution
                        //    a += Nominal * Maturity * Alpha;
                        //    b -= Nominal * Maturity * Alpha * Ytm;
                        //}
                        //else//Obligations perpétuelles
                        //{
                        //    //Prix dirty de l'instrument
                        //    DirtyPrice = instrumentPtr.GetDirtyPriceByYTM(reportingdate, instrumentPtr.GetSettlementDate(reportingdate), reportingdate, Ytm) / 100;

                        //    //Calcul des coefficients du trinome solution
                        //    b += Nominal * DirtyPrice;
                        //    c -= Nominal * DirtyPrice * Ytm;
                        //}

                        if (Maturity * ytmportfolioEstimate < 1)//obligations à maturité finie et rendement petit
                        {
                            //Prix dirty de l'instrument
                            DirtyPrice = instrumentPtr.GetDirtyPriceByYTM(reportingdate, instrumentPtr.GetSettlementDate(reportingdate), reportingdate, Ytm) / 100;

                            //Coupon de l'instrument: coupon de la période courante. S'il est nul, coupon de la première période de coupon non nulle
                            Coupon = DataSourceCoupon.GetInstance().GetCoupon(positionPtr, instrumentPtr) / 100;

                            //Coefficient Alpha
                            Alpha = 1 + Coupon * 0.5 * (Maturity + 1);//1 + C*(M+1)/2

                            if (Maturity * Ytm < 1)
                            {
                                //Calcul des coefficients du trinome solution
                                a += Nominal * Maturity * Alpha;
                                b -= Nominal * Maturity * Alpha * Ytm;
                            }
                            else
                            {
                                //Calcul des coefficients du trinome solution
                                //a += Nominal * Maturity * Alpha;
                                //b -= Nominal * Maturity * ((1 - DirtyPrice) / Maturity + Coupon);
                                b += Nominal * DirtyPrice;
                                c -= Nominal * DirtyPrice * Ytm;
                            }
                        }
                        else
                        {
                            //Prix dirty de l'instrument
                            DirtyPrice = instrumentPtr.GetDirtyPriceByYTM(reportingdate, instrumentPtr.GetSettlementDate(reportingdate), reportingdate, Ytm) / 100;

                            //Coupon de l'instrument: coupon de la période courante. S'il est nul, coupon de la première période de coupon non nulle
                            Coupon = DataSourceCoupon.GetInstance().GetCoupon(positionPtr, instrumentPtr) / 100;

                            //if (Maturity * Ytm < 1)
                            //{
                            //    //Calcul des coefficients du trinome solution
                            //b += Nominal * DirtyPrice;
                            //c -= Nominal * Coupon;
                            //}
                            //else
                            //{
                                //Calcul des coefficients du trinome solution
                                b += Nominal * DirtyPrice;
                                c -= Nominal * DirtyPrice * Ytm;
                            //}
                        }
                    }
                }
                delta = Math.Pow(b, 2) - 4 * a * c;//b^2 - 4ac
                tkoAYtm = (-b + Math.Sqrt(delta)) / (2 * a);//[-b + racine(delta)] / 2a  
            }
            return tkoAYtm;
        }
        
        //public double ComputePortfolioYTM(CSMPortfolio PortfolioPtr)
        //{
        //    double tkoYtm = 0;
        //    double portfolioAssetValue = PortfolioPtr.GetAssetValue();
        //    sophis.static_data.eMDayCountBasisType DayCountBasisType = (eMDayCountBasisType)7;//Actual/365
        //    sophis.static_data.eMYieldCalculationType YieldCalculationType = (eMYieldCalculationType)1;//Actuarial

        //    //Reste a trouver comment lier au portefeuille et à la date
        //    CSMCashFlowDiagram CFDiagram = new CSMCashFlowDiagram();

        //    tkoYtm = CFDiagram.GetYTMByPrice(portfolioAssetValue, CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType), CSMYieldCalculation.GetCSRYieldCalculation(YieldCalculationType));

        //    return tkoYtm;
        //}

        //Fonctions annexes
        
        public double ComputeInvestedCash(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            int PositionMvtident = PositionPtr.GetIdentifier();
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            double invcash = -ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
            return invcash;
        }
        
        public double ComputeDefferedCoupon(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double defcoupon = 0;
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            defcoupon = -ORCLFactory.ORCLFactory.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionPtr.GetIdentifier() + " and TYPE=360 and DATEVAL > num_to_date(" + reportingdate + ") and DATENEG <= num_to_date(" + reportingdate + ")");
            return defcoupon;
        }

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
                            coupon = GetCouponOldStyle(InstrumentPtr, reportingdate);
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
        public double ComputeFirstCoupon(CSMInstrument InstrumentPtr)
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
        
        public System.Collections.ArrayList GetBondExplanationArray(int InstrumentCode, int reportingdate)
        {
            System.Collections.ArrayList explicationArray;
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();
            try
            {
                CSMBond Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam

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

        //Fonction de calcul pour l'arbre de décisions YTM/YTC/YTW
        //fonction qui retourne 1 si la notation est haute, -1 si la notation est basse, 0 en cas d'erreur
        public int TestRatingNb(double notationNum)
        {
            if (notationNum >= 12 && notationNum < 22) return 1;
            else if ((notationNum >= 0 && notationNum < 12) || notationNum == -1) return -1;
            else return 0;
        }
        //Fonction traitant la seniorité  (101 = Senior // 102 = Subordinated)
        public int TestSenioritYtm(CSMInstrument instrument)
        {
            if (instrument.GetSeniority() == 102) return -1; //Cas Subordinated
            else if (instrument.GetSeniority() == 101 || instrument.GetSeniority() == 121 || (instrument.GetSeniority() > 102 && instrument.GetSeniority() <= 109) ) return 1;//Cas Senior ou autre
            else return 0;
        }
        //Fonction traitant la seniorité  (101 = Senior // 102 = Subordinated)
        public int TestSeniority(CSMInstrument instrument, CSMPosition position)
        {
            string sector_research;
            Boolean fsub;
            sector_research = FonctionAdd.GetValuefromSophisString(position, instrument, "Sector Tikehau Research Sectors");
            fsub = sector_research.Contains("Financials");

            if (instrument.GetSeniority() == 102 && fsub) return -1; //Cas Subordinated
            else if ((instrument.GetSeniority() == 102 && !fsub) || instrument.GetSeniority() == 121 || instrument.GetSeniority() == 101 || (instrument.GetSeniority() > 102 && instrument.GetSeniority() <= 109)) return 1;//Cas Senior ou autre
            else return 0;
        }

        //méthode qui va déterimner le YTM/YTC de l'instrument suivant ses caractéristiques
        // 0 si aucune possibilité de calcul
        public int ComputeTreeYTM(CSMPosition Position, CSMInstrument Instrument)
        {
            //Classe 
         
            MarketIndicCompute IMarket = MarketIndicCompute.GetInstance();

            //Variables de  calcul
            int algosenior = TestSeniority(Instrument, Position);


            //début de l'algo de selection 
            if (algosenior == -1)//Si l'instrument est dans le cas Sub
            {
                CFCompute ft = CFCompute.GetInstance();
                DataSource1stCallDate datafirst = DataSource1stCallDate.GetInstance();
                //On récupère la date du jour et celle du premier call
                int txft = ft.ComputeFixedOrFloat(Instrument);
                ft.Close();
                int today = VersionClass.Get_ReportingDate();
                int date1stcall = datafirst.Get1stCallDate(Position, Instrument);
                //Code CMS : 67883897 EIISDA 10 // tec10 index 67905657
                if (Instrument.GetInstrumentType() == 'O')
                {
                    CSMBond bond = CSMBond.GetInstance(Instrument.GetCode());
                    bool Cms = (txft == 1 && (bond.GetFloatingRate() == 67883897 || bond.GetFloatingRate() == 67905657 || bond.GetFloatingRate() == 67739540));
                    if (!Cms && (today < date1stcall)) return 2;//Si le bond est tx float alors on retourne 2
                    else return 1;
                }
                else return 1;

            }
            //Si l'instrument est dans le cas Senior
            else if (algosenior == 1)
            {
                int testratevalue = TestRatingNb(IMarket.NotationNum(Instrument));
                IMarket.Close();
                if (testratevalue == 1)
                {
                   int mat = Instrument.GetExpiry();

                    if(mat>52961)return 3;//HY (perpétuelle)
                    else return 4; //IG
                }
                else return 3;//HY
            }
            else return 0; //cas où il y a erreur
        }

        public double ComputeTreeYTMValue(CSMPosition Position, CSMInstrument Instrument)
        {

            double val = 0;

            if (Instrument.GetInstrumentType() == 'O' || Instrument.GetInstrumentType() == 'D')
            {
               
                int Tytm = 0;

                if (FonctionAdd.GetValuefromSophisString(Position, Instrument, "Allotment") == "Internal Securities")
                {
                    val = Instrument.GetYTM() * 100;
                    return val;
                }

                //calcul du cas 
                Tytm = ComputeTreeYTM(Position, Instrument);

                if (Tytm == 1 || Tytm == 4)
                {
                    val = Instrument.GetYTMMtoM() * 100;//IG (YTM) ou CMS ou First Call< Today
                    if (val < 15) return val;
                    else return 15;
                }
                else if (Tytm == 2)// Sub not CMS et FirstCall>Today(YTC)
                {

                    val = FonctionAdd.GetValuefromSophisDouble(Position, Instrument, "Yield to Call MtM");
                    if (val < 15) return val;
                    else return 15;
                }
                else if (Tytm == 3)//Hy (YTW)
                {
                    val = FonctionAdd.GetValuefromSophisDouble(Position, Instrument, "Yield to Worst MtM");
                    return val;
                }
                else return 0;
            }
            else if (Instrument.GetInstrumentType() == 'S')//CDS
            {
                val = Instrument.GetYTM() * 100;
                return val;
            }
            else if (Instrument.GetInstrumentType() == 'T')//Debt Instruments
            {
                val = Instrument.GetYTMMtoM() * 100;
                return val;
            }
            else if (Instrument.GetInstrumentType() == 'F')//Interest Rate Futures
            {
                val = Instrument.GetYTM() * 100;
                return val;
            }
            else return 0;

        }

        public double ComputeDateWorkOut(CSMPosition Position, CSMInstrument Instrument)
        {
            int Tdate = 0;

            //On récupère une valeur en int représentant le nombre de jour depuis 1 janvier 1904
            // il est donc nécesaire de la comparer à la date du jour afin d'en déduire le nombre de jour entre aujourd'hui et la date annoncée

            //calcul du cas 
            if (Instrument.GetInstrumentType().ToString() == "O" || Instrument.GetInstrumentType() == 'D')
            {
                Tdate = ComputeTreeYTM(Position, Instrument);
                if (Tdate == 1 || Tdate == 4)//IG (YTM) ou CMS ou First Call< Today
                {
                    if (Instrument.GetInstrumentType().ToString() == "O" || FonctionAdd.GetValuefromSophisString(Position, Instrument, "Allotment") == "Convertibles")
                    {
                        CSMBond bond = CSMBond.GetInstance(Instrument.GetCode());
                        return (bond.GetMaturity() - VersionClass.Get_ReportingDate());
                    }
                  
                    else return 0;

                }
                else if (Tdate == 2) return (FonctionAdd.GetValuefromSophisDate(Position, Instrument, "Date to Call MtM") - VersionClass.Get_ReportingDate());// Sub not CMS et FirstCall>Today(YTC)
                else if (Tdate == 3) return (FonctionAdd.GetValuefromSophisDate(Position, Instrument, "Date to Worst MtM") - VersionClass.Get_ReportingDate());//HY (YTW)
                else return 0;
            }
            else return 0;

        }

        public string ComputeTreeYTMName(int valuetree)
        {
            string name = "";

            //détermination de la valeur YTM/YTC
            if (valuetree == 1) name = " Subordinated YTM";
            else if (valuetree == 2) name = " Sub not CMS et FirstCall>Today YTC";
            else if (valuetree == 3) name = " HY";
            else if (valuetree == 4) name = " IG";
            else return name;

            return name;
        }
                
    }

    //Fonctions de gestion de l'affichage des colonnes

    public class DataSourceCarryAtInvDate
    {
        // Holds the instance of the singleton class
        private static DataSourceCarryAtInvDate Instance = null;

        public static Hashtable DataCacheCarryAtInvDate;//Cache pour le Carry@inv date par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la verison de l'instrument

        /// <summary>
		/// Constructor
		/// </summary>
        private DataSourceCarryAtInvDate()
		{
            DataCacheCarryAtInvDate = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
		}

		/// <summary>
		/// Returns an instance of the DataSource singleton
		/// </summary>
        public static DataSourceCarryAtInvDate GetInstance()
		{
			if(null == Instance)
                Instance = new DataSourceCarryAtInvDate();
			return Instance;
		}

		/// <summary>
		/// Queries the database for the maximum quote for a certain position. When this method is called
		/// for the first time, i.e. for the first position of a portfolio, it gets maximum quotes for all
		/// the positions in the portfolio. The results are then stored in the cache. When the method is
		/// called for the other positions in the portfolio, the result is taken directly from the cache.
		/// The SQL query is therefore executed for the first position of the portfolio.
		/// </summary>
		/// <param name="PortfolioCode">Code of the opened portfolio</param>
		/// <param name="PositionIdentifier">Identifier of the current position</param>
		/// <returns>Value to display in the position cell</returns>
        public double GetCarryAtInvDate(CSMPosition Position, CSMInstrument Instrument)
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
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheCarryAtInvDate.ContainsKey(PositionIdentifier))
                        DataCacheCarryAtInvDate[PositionIdentifier] = 0.0;
                    else
                        DataCacheCarryAtInvDate.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheCarryAtInvDate.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position,Instrument);
                }
                
                // At this point, the value that this method should return must be available in the cache
                if (DataCacheCarryAtInvDate.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheCarryAtInvDate[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

		}

        private void FillCache(CSMPosition Position,CSMInstrument Instrument)
        {
            //mise à jour du Carry@Inv_Date
            CarryYtmCompute ICarryYtm = CarryYtmCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            double CarryAtInvDate = ICarryYtm.ComputeCarryAtInvDate(Position, Instrument);
            if (DataCacheCarryAtInvDate.ContainsKey(PositionId))
                DataCacheCarryAtInvDate[PositionId] = CarryAtInvDate;
            else
                DataCacheCarryAtInvDate.Add(PositionId, CarryAtInvDate);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            ICarryYtm.Close();
        }


    }

    public class DataSourceYtmAtInvDate
    {
        // Holds the instance of the singleton class
        private static DataSourceYtmAtInvDate Instance = null;

        public static Hashtable DataCacheInvYtm;//Cache pour le ytm@inv date par positon
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
		/// Constructor
		/// </summary>
        private DataSourceYtmAtInvDate()
		{
            DataCacheInvYtm = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }

		/// <summary>
		/// Returns an instance of the DataSource singleton
		/// </summary>
        public static DataSourceYtmAtInvDate GetInstance()
		{
			if(null == Instance)
                Instance = new DataSourceYtmAtInvDate();
			return Instance;
		}

		public double GetInvYtm(CSMPosition Position,CSMInstrument Instrument)
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

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheInvYtm.ContainsKey(PositionIdentifier))
                        DataCacheInvYtm[PositionIdentifier] = 0.0;
                    else
                        DataCacheInvYtm.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheInvYtm.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position,Instrument);
                }
                
                // At this point, the value that this method should return must be available in the cache
                if (DataCacheInvYtm.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheInvYtm[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

		}

        private void FillCache(CSMPosition Position,CSMInstrument Instrument)
        {
            //mise à jour de Ytm@Inv_Date
            CarryYtmCompute ICarryYtm = CarryYtmCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            double YtmAtInvdate = ICarryYtm.ComputeInvestedYTM(Position, Instrument) * 100;//en %
            if (DataCacheInvYtm.ContainsKey(PositionId))
                DataCacheInvYtm[PositionId] = YtmAtInvdate;
            else
                DataCacheInvYtm.Add(PositionId, YtmAtInvdate);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            ICarryYtm.Close();
        }
    }
    
    public class DataSourceDCarryCoupon
    {
        // Holds the instance of the singleton class
        private static DataSourceDCarryCoupon Instance = null;

        public static Hashtable DataCacheDCarryCoupon;//Cache pour le DCarryCoupon par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
		/// Constructor
		/// </summary>
        private DataSourceDCarryCoupon()
		{
            DataCacheDCarryCoupon = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
		}

		/// <summary>
		/// Returns an instance of the DataSource singleton
		/// </summary>
        public static DataSourceDCarryCoupon GetInstance()
		{
			if(null == Instance)
                Instance = new DataSourceDCarryCoupon();
			return Instance;
		}

        public double GetDCarryCoupon(CSMPosition Position, CSMInstrument Instrument)
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
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheDCarryCoupon.ContainsKey(PositionIdentifier))
                        DataCacheDCarryCoupon[PositionIdentifier] = 0.0;
                    else
                        DataCacheDCarryCoupon.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide
                if (!DataCacheDCarryCoupon.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position,Instrument);
                }
                
                // At this point, the value that this method should return must be available in the cache
                if (DataCacheDCarryCoupon.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheDCarryCoupon[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

		}

        private void FillCache(CSMPosition Position,CSMInstrument Instrument)
        {
            CarryYtmCompute IDCarryCoupon = CarryYtmCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            int PositionCode = Position.GetIdentifier();

            //mise à jour du DCarryCoupon
            double DCarryCoupon = IDCarryCoupon.ComputeDailyCarryCoupon(Position, Instrument);
            if (DataCacheDCarryCoupon.ContainsKey(PositionCode))
                DataCacheDCarryCoupon[PositionCode] = DCarryCoupon;
            else
                DataCacheDCarryCoupon.Add(PositionCode, DCarryCoupon);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(InstrumentCode))
                DataCacheInstrVersion[InstrumentCode] = InstrVersion;
            else
                DataCacheInstrVersion.Add(InstrumentCode, InstrVersion);

            IDCarryCoupon.Close();
        }
    }
    
    public class DataSourceDCarryActuarial
    {
        // Holds the instance of the singleton class
        private static DataSourceDCarryActuarial Instance = null;

        public static Hashtable DataCacheDCarryActuarial;//Cache pour le DCarryActuarial par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
		/// Constructor
		/// </summary>
        private DataSourceDCarryActuarial()
		{
            DataCacheDCarryActuarial = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
		}

		/// <summary>
		/// Returns an instance of the DataSource singleton
		/// </summary>
        public static DataSourceDCarryActuarial GetInstance()
		{
			if(null == Instance)
                Instance = new DataSourceDCarryActuarial();
			return Instance;
		}

        public double GetDCarryActuarial(CSMPosition Position, CSMInstrument Instrument)
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
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheDCarryActuarial.ContainsKey(PositionIdentifier))
                        DataCacheDCarryActuarial[PositionIdentifier] = 0.0;
                    else
                        DataCacheDCarryActuarial.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheDCarryActuarial.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position,Instrument);
                }
                
                // At this point, the value that this method should return must be available in the cache
                if (DataCacheDCarryActuarial.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheDCarryActuarial[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

		}

        private void FillCache(CSMPosition Position,CSMInstrument Instrument)
        {
            CarryYtmCompute IDCarryActuarial = CarryYtmCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            //mise à jour du DCarryActuarial
            double DCarryActuarial = IDCarryActuarial.ComputeDailyCarryAct(Position, Instrument);
            if (DataCacheDCarryActuarial.ContainsKey(PositionId))
                DataCacheDCarryActuarial[PositionId] = DCarryActuarial;
            else
                DataCacheDCarryActuarial.Add(PositionId, DCarryActuarial);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            IDCarryActuarial.Close();
        }
    }
    
    public class DataSourceYtm
    {
        // Holds the instance of the singleton class
        private static DataSourceYtm Instance = null;

        public static Hashtable DataCacheYtm;//Cache pour la valeur du CDS impl par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceYtm()
        {
            DataCacheYtm = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();

        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceYtm GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceYtm();
            return Instance;
        }

        /// <summary>
        /// Queries the database for the maximum quote for a certain position. When this method is called
        /// for the first time, i.e. for the first position of a portfolio, it gets maximum quotes for all
        /// the positions in the portfolio. The results are then stored in the cache. When the method is
        /// called for the other positions in the portfolio, the result is taken directly from the cache.
        /// The SQL query is therefore executed for the first position of the portfolio.
        /// </summary>
        /// <param name="PortfolioCode">Code of the opened portfolio</param>
        /// <param name="PositionIdentifier">Identifier of the current position</param>
        /// <returns>Value to display in the position cell</returns>
        public double GetYtm(CSMPosition Position, CSMInstrument Instrument)
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
                if (!DataCacheYtm.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheYtm.ContainsKey(Instrumentcode))
                {
                    return (double)DataCacheYtm[Instrumentcode];
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
            //mise à jour du Ytm
            CarryYtmCompute ICarryYtm = CarryYtmCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            double Ytm = ICarryYtm.ComputeYtm(Instrument) * 100;
            if (DataCacheYtm.ContainsKey(InstrumentCode))
                DataCacheYtm[InstrumentCode] = Ytm;
            else
                DataCacheYtm.Add(InstrumentCode, Ytm);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(InstrumentCode))
                DataCacheInstrVersion[InstrumentCode] = InstrVersion;
            else
                DataCacheInstrVersion.Add(InstrumentCode, InstrVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode);
            if (DataCacheSpot.ContainsKey(InstrumentCode))
                DataCacheSpot[InstrumentCode] = Spot;
            else
                DataCacheSpot.Add(InstrumentCode, Spot);

            ICarryYtm.Close();
        }
    }

    public class DataSourceYtmTest
    {
        // Holds the instance of the singleton class
        private static DataSourceYtmTest Instance = null;

        public static Hashtable DataCacheYtm;//Cache pour la valeur du CDS impl par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument
        public static Hashtable DataCacheNewton;//Cache pour Newton

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceYtmTest()
        {
            DataCacheYtm = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
            DataCacheNewton = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceYtmTest GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceYtmTest();
            return Instance;
        }

        /// <summary>
        /// Queries the database for the maximum quote for a certain position. When this method is called
        /// for the first time, i.e. for the first position of a portfolio, it gets maximum quotes for all
        /// the positions in the portfolio. The results are then stored in the cache. When the method is
        /// called for the other positions in the portfolio, the result is taken directly from the cache.
        /// The SQL query is therefore executed for the first position of the portfolio.
        /// </summary>
        /// <param name="PortfolioCode">Code of the opened portfolio</param>
        /// <param name="PositionIdentifier">Identifier of the current position</param>
        /// <returns>Value to display in the position cell</returns>

        public double[] getNewtonValues(CSMPosition Position, CSMInstrument Instrument, double x)
        {
            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            string key = Convert.ToString(Instrumentcode) + "_" + Convert.ToString(x);

            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            double[] res = new double[2];
            if (Nominal != 0)
            {
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                if (!DataCacheNewton.ContainsKey(key))
                {
                    FillCache(Instrument,x);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheNewton.ContainsKey(key))
                {
                    res = (double[])DataCacheNewton[key];
                    return (double[])DataCacheNewton[key];
                }
                else { return new double[2]; }//on ne devrait jamais passer par là ms au cas où

                /*
                CarryYtmCompute ICarryYtm = CarryYtmCompute.GetInstance();
                
                res[0] = ICarryYtm.getNewtonValues(Instrument, x)[0];//* 100
                res[1] = ICarryYtm.getNewtonValues(Instrument, x)[1];//* 100
                ICarryYtm.Close();*/
            }
            return res;
        }
        public double GetYtm(CSMPosition Position, CSMInstrument Instrument)
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
                if (!DataCacheYtm.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument,0);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheYtm.ContainsKey(Instrumentcode))
                {
                    return (double)DataCacheYtm[Instrumentcode];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }
        }

        /// <summary>
        /// Executes the SQL query and stores the results in the DataCache
        /// </summary>
        /// <param name="PortfolioCode">Code of the portfolio in question</param>
        private void FillCache(CSMInstrument Instrument,double x)
        {
            //mise à jour du Ytm
            CarryYtmCompute ICarryYtm = CarryYtmCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            double Ytm = ICarryYtm.TestComputeYtm(Instrument) * 100;
            if (DataCacheYtm.ContainsKey(InstrumentCode))
                DataCacheYtm[InstrumentCode] = Ytm;
            else
                DataCacheYtm.Add(InstrumentCode, Ytm);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(InstrumentCode))
                DataCacheInstrVersion[InstrumentCode] = InstrVersion;
            else
                DataCacheInstrVersion.Add(InstrumentCode, InstrVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode);
            if (DataCacheSpot.ContainsKey(InstrumentCode))
                DataCacheSpot[InstrumentCode] = Spot;
            else
                DataCacheSpot.Add(InstrumentCode, Spot);

            //mise à jour du Newton de l'instrument
            string key = Convert.ToString(InstrumentCode) +"_"+ Convert.ToString(x);
            double[] newton = new double[2];
            newton[0] = ICarryYtm.getNewtonValues(Instrument, x)[0];
            newton[1] = ICarryYtm.getNewtonValues(Instrument, x)[1];
            if (DataCacheNewton.ContainsKey(key))
                DataCacheNewton[key] = newton;
            else
                DataCacheNewton.Add(key, newton);

            ICarryYtm.Close();
        }

      
    }

    public class DataSourceAYtmTRI
    {
        // Holds the instance of the singleton class
        private static DataSourceAYtmTRI Instance = null;

        public static Hashtable DataCacheYtmAgregate;//Cache pour la valeur du CDS impl par instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceAYtmTRI()
        {
            DataCacheYtmAgregate = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceAYtmTRI GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceAYtmTRI();
            return Instance;
        }

        /// <summary>
        /// Queries the database for the maximum quote for a certain position. When this method is called
        /// for the first time, i.e. for the first position of a portfolio, it gets maximum quotes for all
        /// the positions in the portfolio. The results are then stored in the cache. When the method is
        /// called for the other positions in the portfolio, the result is taken directly from the cache.
        /// The SQL query is therefore executed for the first position of the portfolio.
        /// </summary>
        /// <param name="PortfolioCode">Code of the opened portfolio</param>
        /// <param name="PositionIdentifier">Identifier of the current position</param>
        /// <returns>Value to display in the position cell</returns>
        public double GetYtmAgregate(CSMPortfolio Portfolio)
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

            int Portfoliocode = Portfolio.GetCode();
            //La valeur est (re)calculée:
            //si le cache est vide 
            //si la version de l'instrument change
            //si le spot change
            CSMMarketData context = CSMMarketData.GetCurrentMarketData();
            if (!DataCacheYtmAgregate.ContainsKey(Portfoliocode))
            {
                FillCache(Portfolio);
            }

            // At this point, the value that this method should return must be available in the cache
            if (DataCacheYtmAgregate.ContainsKey(Portfoliocode))
            {
                return (double)DataCacheYtmAgregate[Portfoliocode];
            }
            else { return 0; }//on ne devrait jamais passer par là ms au cas où

        }

        /// <summary>
        /// Executes the SQL query and stores the results in the DataCache
        /// </summary>
        /// <param name="PortfolioCode">Code of the portfolio in question</param>
        private void FillCache(CSMPortfolio Portfolio)
        {
            //mise à jour du Ytm
            CarryYtmCompute ICarryYtm = CarryYtmCompute.GetInstance();
            int PortfolioCode = Portfolio.GetCode();
            double YtmAgregate = ICarryYtm.ComputeAYtmTRI(Portfolio) * 100;
            if (DataCacheYtmAgregate.ContainsKey(PortfolioCode))
            { DataCacheYtmAgregate[PortfolioCode] = YtmAgregate; }
            else
            { DataCacheYtmAgregate.Add(PortfolioCode, YtmAgregate); }
            ICarryYtm.Close();
        }
    }
    
    public class DataSourceAYtmApprox
    {
        // Holds the instance of the singleton class
        private static DataSourceAYtmApprox Instance = null;

        public static Hashtable DataCacheAYtmApprox;//Cache pour la valeur du CDS impl par instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceAYtmApprox()
        {
            DataCacheAYtmApprox = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceAYtmApprox GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceAYtmApprox();
            return Instance;
        }

        /// <summary>
        /// Queries the database for the maximum quote for a certain position. When this method is called
        /// for the first time, i.e. for the first position of a portfolio, it gets maximum quotes for all
        /// the positions in the portfolio. The results are then stored in the cache. When the method is
        /// called for the other positions in the portfolio, the result is taken directly from the cache.
        /// The SQL query is therefore executed for the first position of the portfolio.
        /// </summary>
        /// <param name="PortfolioCode">Code of the opened portfolio</param>
        /// <param name="PositionIdentifier">Identifier of the current position</param>
        /// <returns>Value to display in the position cell</returns>
        public double GetYtmAYtmApprox(CSMPortfolio Portfolio, double ytmportfolioEstimate)
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

            int Portfoliocode = Portfolio.GetCode();
            //La valeur est (re)calculée:
            //si le cache est vide 
            //si la version de l'instrument change
            //si le spot change
            CSMMarketData context = CSMMarketData.GetCurrentMarketData();
            if (!DataCacheAYtmApprox.ContainsKey(Portfoliocode))
            {
                FillCache(Portfolio, ytmportfolioEstimate);
            }

            // At this point, the value that this method should return must be available in the cache
            if (DataCacheAYtmApprox.ContainsKey(Portfoliocode))
            {
                return (double)DataCacheAYtmApprox[Portfoliocode];
            }
            else { return 0; }//on ne devrait jamais passer par là ms au cas où

        }

        /// <summary>
        /// Executes the SQL query and stores the results in the DataCache
        /// </summary>
        /// <param name="PortfolioCode">Code of the portfolio in question</param>
        private void FillCache(CSMPortfolio Portfolio, double ytmportfolioEstimate)
        {
            //mise à jour du Ytm
            CarryYtmCompute ICarryYtm = CarryYtmCompute.GetInstance();
            int PortfolioCode = Portfolio.GetCode();
            double AYtmApprox = ICarryYtm.ComputeAYtmApprox(Portfolio, ytmportfolioEstimate) * 100;
            if (DataCacheAYtmApprox.ContainsKey(PortfolioCode))
            { DataCacheAYtmApprox[PortfolioCode] = AYtmApprox; }
            else
            { DataCacheAYtmApprox.Add(PortfolioCode, AYtmApprox); }
            ICarryYtm.Close();
        }
    }

    public class DataSourceTreeFindYT
    {
        // Holds the instance of the singleton class
        private static DataSourceTreeFindYT Instance = null;

        public static Hashtable DataCacheTreeYtm;//Cache pour le treeYTM date par positon
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public CarryYtmCompute ICarryYtm;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceTreeFindYT()
        {
            DataCacheTreeYtm = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            ICarryYtm = CarryYtmCompute.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceTreeFindYT GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceTreeFindYT();
            return Instance;
        }

        public string GetTreeYtm(CSMPosition Position, CSMInstrument Instrument)
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

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheTreeYtm.ContainsKey(PositionIdentifier))
                        DataCacheTreeYtm[PositionIdentifier] = 0.0;
                    else
                        DataCacheTreeYtm.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheTreeYtm.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheTreeYtm.ContainsKey(PositionIdentifier))
                {
                    return (string)DataCacheTreeYtm[PositionIdentifier];
                }
                else { return ""; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return ""; }

        }



        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            //mise à jour de Ytm
            
            int PositionId = Position.GetIdentifier();
            string TreeYtm = (string)ICarryYtm.ComputeTreeYTMName(ICarryYtm.ComputeTreeYTM(Position, Instrument));
            if (DataCacheTreeYtm.ContainsKey(PositionId))
                DataCacheTreeYtm[PositionId] = TreeYtm;
            else
                DataCacheTreeYtm.Add(PositionId, TreeYtm);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);
        }
    }

    public class DataSourceTreeFindYTValue
    {
        // Holds the instance of the singleton class
        private static DataSourceTreeFindYTValue Instance = null;

        public static Hashtable DataCacheTreeYtmValue;//Cahce pour leTreeYTMValue par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public CarryYtmCompute ICarryYtm;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceTreeFindYTValue()
        {
            DataCacheTreeYtmValue = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            ICarryYtm = CarryYtmCompute.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceTreeFindYTValue GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceTreeFindYTValue();
            return Instance;
        }

        public double GetTreeYtmValue(CSMPosition Position, CSMInstrument Instrument)
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

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheTreeYtmValue.ContainsKey(PositionIdentifier))
                        DataCacheTreeYtmValue[PositionIdentifier] = 0.0;
                    else
                        DataCacheTreeYtmValue.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheTreeYtmValue.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheTreeYtmValue.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheTreeYtmValue[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }
   
        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
    
            int PositionId = Position.GetIdentifier();
            double TreeYtmval = ICarryYtm.ComputeTreeYTMValue(Position, Instrument);
            if (DataCacheTreeYtmValue.ContainsKey(PositionId))
                DataCacheTreeYtmValue[PositionId] = TreeYtmval;
            else
                DataCacheTreeYtmValue.Add(PositionId, TreeYtmval);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

        }

    }

    public class DataSourceTreeFindYTValueContrib
    {
        // Holds the instance of the singleton class
        private static DataSourceTreeFindYTValueContrib Instance = null;

        public static Hashtable DataCacheTreeYtmValue;//Cahce pour leTreeYTMValue par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public CarryYtmCompute ICarryYtm;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceTreeFindYTValueContrib()
        {
            DataCacheTreeYtmValue = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            ICarryYtm = CarryYtmCompute.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceTreeFindYTValueContrib GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceTreeFindYTValueContrib();
            return Instance;
        }

        public double GetTreeYtmValue(CSMPosition Position, CSMInstrument Instrument)
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

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheTreeYtmValue.ContainsKey(PositionIdentifier))
                        DataCacheTreeYtmValue[PositionIdentifier] = 0.0;
                    else
                        DataCacheTreeYtmValue.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheTreeYtmValue.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheTreeYtmValue.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheTreeYtmValue[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {

            int PositionId = Position.GetIdentifier();
            double TreeYtmval = ICarryYtm.ComputeTreeYTMValue(Position, Instrument);
            if (DataCacheTreeYtmValue.ContainsKey(PositionId))
                DataCacheTreeYtmValue[PositionId] = TreeYtmval;
            else
                DataCacheTreeYtmValue.Add(PositionId, TreeYtmval);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

        }

    }
   
    public class DataSourceDateWorkOut
    {
        // Holds the instance of the singleton class
        private static DataSourceDateWorkOut Instance = null;

        public static Hashtable DataCacheDateWorkOut;//Cahce pour leTreeYTMValue par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public CarryYtmCompute ICarryYtm;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceDateWorkOut()
        {
            DataCacheDateWorkOut = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            ICarryYtm = CarryYtmCompute.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceDateWorkOut GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceDateWorkOut();
            return Instance;
        }


        public double GetDateWorkOut(CSMPosition Position, CSMInstrument Instrument)
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

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheDateWorkOut.ContainsKey(PositionIdentifier))
                        DataCacheDateWorkOut[PositionIdentifier] = 0.0;
                    else
                        DataCacheDateWorkOut.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheDateWorkOut.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheDateWorkOut.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheDateWorkOut[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }
        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            //mise à jour de 


            int PositionId = Position.GetIdentifier();
            double stdate = ICarryYtm.ComputeDateWorkOut(Position, Instrument);
            if (DataCacheDateWorkOut.ContainsKey(PositionId))
                DataCacheDateWorkOut[PositionId] = stdate;
            else
                DataCacheDateWorkOut.Add(PositionId, stdate);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);


        }
    }
    //Fonctions Critere colonne 
    class HVBCriteria : CSMCriterium
    {
        //DPH
        //public override void GetName(int code, sophis.utils.CMString name)
        public override void GetName(int code, sophis.utils.CMString name, long size)
        {
            if (code == 1)
            {
                name.SetString("IG");
            }
            else
            {
                name.SetString("HY");
            }
        }

        public override void GetCode(SSMReportingTrade mvt, System.Collections.ArrayList list)
        {
            list.Clear();
            SSMOneValue st = new SSMOneValue();
            //@DPH
            st.fCode = (int)mvt.refcon;

            //Apply HVB Formula on Instruments
            CSMInstrument inst = CSMInstrument.GetInstance(mvt.sicovam);

            /*if (CarryYtmCompute(inst))
            {
                st.fCode = 1;
            }
            else
            {
                st.fCode = 2;
            }
            */
            list.Add(st);
        }
    } 

}

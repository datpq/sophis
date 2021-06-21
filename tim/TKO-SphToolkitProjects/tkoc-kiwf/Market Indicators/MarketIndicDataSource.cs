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
    public class MarketIndicCompute
    {
        // Holds the instance of the singleton class
        private static MarketIndicCompute Instance = null;
        //private ORCLFactory.ORCLFactory CS_ORCL;

        //Constructeur
        private MarketIndicCompute()
        {
            //CS_ORCL = new ORCLFactory.ORCLFactory();
            ORCLFactory.ORCLFactory.Initialize();
        }
        public void Close()
        {
            //CS_ORCL.CloseAll();
        }

        /// <summary>
        /// Returns an instance of CFCompute
        /// </summary>
        public static MarketIndicCompute GetInstance()
        {
            Instance = new MarketIndicCompute();
            return Instance;
        }

        //Fonctions renvoyant le contenu des colonnes CF
        public double ComputeInternalGearing(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double gearing = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                double assetvalue;
                double nominal;
                int PositionMvtident = PositionPtr.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "S"://Swap
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        if (nominal > 0)//acheteur de protection
                        {
                            sophis.static_data.eMDayCountBasisType DayCountBasisType = InstrumentPtr.GetMarketAIDayCountBasisType();
                            double RemainingTime = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, InstrumentPtr.GetExpiry());
                            CSMSwap Swap = CSMSwap.GetInstance(InstrumentPtr.GetCode());
                            CSMFixedLeg FixedLeg;
                            if (Swap.GetLegFlowType(0).ToString() == "M_lfCredit")//jmbe crédit
                            {
                                FixedLeg = Swap.GetLeg(1);
                            }
                            else//Jambe fixe
                            {
                                FixedLeg = Swap.GetLeg(0);
                            }
                            gearing = nominal * FixedLeg.GetFixedRate() * RemainingTime;

                        }
                        else { gearing = Math.Abs(nominal) + assetvalue; }//vendeur de protection

                        break;

                    case "P"://Repo
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "A"://Actions
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "Z"://Fund
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "F"://Futures
                        assetvalue = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();//PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;//un peu trop simple peut être pour les shorts
                        break;

                    case "C"://Cash,fees
                        gearing = 0; //aucune exposition cash, on pourrait faire le CDS de la banque !!!
                        break;

                    case "E"://Forex
                        gearing = 0; //à revoir
                        break;

                    case "T"://Billets de treso
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        
                        //Différencier convertible et Stock derivatives    
                        string otypes = otype.GetString();

                        if (FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Allotment") == "OTC Stock Derivatives")
                        {
                            gearing = Math.Abs(PositionPtr.GetDeltaCash())*fxspot;
                        }
                        else
                        {
                            if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                            {
                                assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                            else//option
                            {
                                assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                        }
                        break;

                    default:
                        break;
                }
                return gearing;
            }
            catch (Exception)
            {
                CSMLog.Write("", "", CSMLog.eMVerbosity.M_warning, "gearing cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
        public double ComputeGearing(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double gearing = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(PositionPtr.GetCurrency());
                double assetvalue;
                double nominal;
                int PositionMvtident = PositionPtr.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "S"://Swap
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        nominal = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        gearing = Math.Abs(nominal) + assetvalue; //vendeur de protection
                        
                        break;

                    case "P"://Repo
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "A"://Actions
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "Z"://Fund
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "F"://Futures
                        assetvalue = PositionPtr.GetInstrumentCount() * InstrumentPtr.GetNotional();//PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;//un peu trop simple peut être pour les shorts
                        break;

                    case "C"://Cash,fees
                        gearing = 0; //aucune exposition cash, on pourrait faire le CDS de la banque !!!
                        break;

                    case "E"://Forex
                        gearing = 0; //à revoir
                        break;

                    case "T"://Billets de treso
                        assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (FonctionAdd.GetValuefromSophisString(PositionPtr, InstrumentPtr, "Allotment") == "OTC Stock Derivatives")
                        {
                            gearing = Math.Abs(PositionPtr.GetDeltaCash()) * fxspot;
                        }
                        else
                        {
                            if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                            {
                                assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                            else//option
                            {
                                assetvalue = PositionPtr.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                        }
                        break;

                    default:
                        break;
                }
                return gearing;
            }
            catch (Exception)
            {
                CSMLog.Write("", "", CSMLog.eMVerbosity.M_warning, "gearing cannot be computed for position " + PositionPtr.GetIdentifier());
                return 0;
            }
        }
        public double ComputeImplCDS(CSMInstrument InstrumentPtr)
        {
            double implcds = 0;
            try
            {
                double ytm;
                double dirtyPrice;
                int settlementShift;
                int pariPassudate;
                CSMBond bond;
                double zcrate;
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        bond = CSMBond.GetInstance(InstrumentPtr.GetCode());
                        settlementShift = bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                        pariPassudate = bond.GetPariPassuDate(reportingdate);
                        dirtyPrice = bond.GetDirtyPriceByZeroCoupon(context, reportingdate, reportingdate + settlementShift, pariPassudate);
                        ytm = ComputeBondYTMByDirtyPrice(InstrumentPtr.GetCode(), dirtyPrice, reportingdate);
                        zcrate = ComputeZCRate(InstrumentPtr);
                        implcds = (ytm - zcrate);
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            ytm = InstrumentPtr.GetYTMMtoM();
                            zcrate = ComputeZCRate(InstrumentPtr);
                            implcds = (ytm - zcrate);
                            zcrate = ComputeZCRate(InstrumentPtr);
                            implcds = (ytm - zcrate);
                        }
                        else//option
                        {
                            implcds = 0;
                        }
                        break;

                    default:
                        break;
                }
                return implcds;
            }
            catch (Exception)
            {
                Console.WriteLine("implcds cannot be computed for instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }
        public double ComputeBasis(CSMInstrument InstrumentPtr)
        {
            double basis = 0;
            try
            {
                double ytm;
                eMSpreadType spreadType;
                double dirtyPrice;
                int settlementShift;
                int pariPassudate;
                CSMBond bond;
                double zcrate;
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        spreadType = InstrumentPtr.GetSpreadType();
                        if (spreadType == 0) { basis = 0; }//Spead Type Undefined
                        else
                        {
                            bond = CSMBond.GetInstance(InstrumentPtr.GetCode());
                            settlementShift = bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                            pariPassudate = bond.GetPariPassuDate(reportingdate);
                            dirtyPrice = bond.GetDirtyPriceByZeroCoupon(context, reportingdate, reportingdate + settlementShift, pariPassudate);
                            ytm = ComputeBondYTMByDirtyPrice(InstrumentPtr.GetCode(), dirtyPrice, reportingdate);
                            zcrate = ComputeZCRate(InstrumentPtr);
                            basis = ComputeCDSSpread(InstrumentPtr) - (ytm - zcrate);
                        }
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            spreadType = InstrumentPtr.GetSpreadType();//Spead Type Undefined
                            if (spreadType == 0) { basis = 0; }
                            else
                            {
                                ytm = InstrumentPtr.GetYTMMtoM();
                                zcrate = ComputeZCRate(InstrumentPtr);
                                basis = ComputeCDSSpread(InstrumentPtr) - (ytm - zcrate);
                            }
                        }
                        else//option
                        {
                            basis = 0;
                        }
                        break;

                    default:
                        break;
                }
                return basis;
            }
            catch (Exception)
            {
                Console.WriteLine("basis cannot be computed for instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }
        public double ComputeDefaultProbability(CSMInstrument InstrumentPtr, CSMPosition PositionPtr)
        {//JE N'AI PAS DE JUSTIFICATION POUR CETTE METHODE DE CALCUL. C'EST CE QUI ETAIT FAIT DANS L'ANCIEN REPORTING ET TIM SOUHAITE LE GARDER.
            double defaultprobability = 0;
            try
            {
                sophis.market_data.CSMMarketData context = new CSMMarketData();
                double maturity;
                int issuer;
                int currency;
                int seniority;
                int defevent;
                double recoveryrate;
                double mdays;
                double dF;
                double cdsrate;
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        maturity = InstrumentPtr.GetExpiry();
                        issuer = InstrumentPtr.GetIssuerCode();
                        currency = InstrumentPtr.GetCurrency();
                        seniority = InstrumentPtr.GetSeniority();
                        defevent = InstrumentPtr.GetDefaultEvent();
                        if (defevent == 0) { defevent = 61; }//On force la valeur à MMR
                        recoveryrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetRecoveryRate(seniority, defevent);
                        if (recoveryrate == 0)//on force la recovery car il ne reconnait sophis ne reconnait pas tjours la seniorité
                        {
                            if (seniority == 101) { recoveryrate = 0.4; }//Senior
                            if (seniority == 102) { recoveryrate = 0.05; }//Sub
                        }
                        mdays = maturity - reportingdate;
                        dF = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(currency, reportingdate, maturity);
                        //cdsrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetCreditDefaultSwapRate(recoveryrate, reportingdate, maturity, seniority, midm, defevent, context);
                        //Si on l'a on le set pour le choper par derrière et éviter de refaire un bootstrap
                        cdsrate = ComputeImplCDS(InstrumentPtr);//CDS implicite
                        defaultprobability = 1 - Math.Pow((1 / (1 + (cdsrate * (1 + recoveryrate) / 4 * dF))), (mdays) / 360 * 4);
                        CSMLog.Write("", "", CSMLog.eMVerbosity.M_verbose, "cds proba de défaut pour instrument " + InstrumentPtr.GetCode() + " =  " + defaultprobability);
                        
                        break;

                    case "S":
                        maturity = InstrumentPtr.GetExpiry();
                        issuer = InstrumentPtr.GetIssuerCode();
                        currency = InstrumentPtr.GetCurrency();
                        seniority = InstrumentPtr.GetSeniority();
                        defevent = InstrumentPtr.GetDefaultEvent();
                        if (defevent == 0) { defevent = 61; }//On force la valeur à MMR
                        recoveryrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetRecoveryRate(seniority, defevent);
                        if (recoveryrate == 0)//on force la recovery car il ne reconnait sophis ne reconnait pas tjours la seniorité
                        {
                            if (seniority == 101) { recoveryrate = 0.4; }//Senior
                            if (seniority == 102) { recoveryrate = 0.05; }//Sub
                        }
                        mdays = maturity - reportingdate;
                        dF = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(currency, reportingdate, maturity);
                        
                        //cdsrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetCreditDefaultSwapRate(recoveryrate, reportingdate, maturity, seniority, midm, defevent, context);
                        //Si on l'a on le set pour le choper par derrière et éviter de refaire un bootstrap
                        int valtype = 0;
                        valtype = (int)PositionPtr.GetValorisationType();
                        // On teste si la valo est faite avec le MtoM. Dans ce cas, on récupère le spot 
                        // qui est le CDS rate bootstrapé. On divise par 100 pour l'avoir en absolu
                        cdsrate = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentPtr.GetCode());
                        if (valtype.Equals(2))
                        { cdsrate = cdsrate / 100; }
                        // Dans le cas des loans valorisés uniquement par MS, ou si la valo est faite en PFPC/MS
                        // On va chercher dans la base le spread MS le plus proche of course inférieur à la date du reporting
                        // Qui sera stocké dans l'historique, colonne PB_CDS_SPREAD
                        if (valtype.Equals(1))
                        { cdsrate = ORCLFactory.ORCLFactory.getResultD("select * from (select PB_CDS_SPREAD from historique where SICOVAM=" + InstrumentPtr.GetCode() + " and JOUR < num_to_date(" + reportingdate + ") and PB_CDS_SPREAD is not null order by JOUR DESC) where ROWNUM=1") / 100; }
                        defaultprobability = 1 - Math.Pow((1 / (1 + (cdsrate * (1 + recoveryrate) / 4 * dF))), (mdays) / 360 * 4);
                        //LE SPOT N'A PAS L'AIR D'ETRE LE BON POUR CALCULER LA PROBA DE DEFAUT: UPFRONT? RUNNING ?  >>> JE NE COMPRENDS PAS!
                        CSMLog.Write("", "", CSMLog.eMVerbosity.M_verbose, "cds proba de défaut pour instrument " + InstrumentPtr.GetCode() + " =  " + defaultprobability);
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            maturity = InstrumentPtr.GetExpiry();
                            issuer = InstrumentPtr.GetIssuerCode();
                            currency = InstrumentPtr.GetCurrency();
                            seniority = InstrumentPtr.GetSeniority();
                            //seniority = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetDefaultSeniority();
                            defevent = InstrumentPtr.GetDefaultEvent();
                            if (defevent == 0) { defevent = 61; }//On force la valeur à MMR
                            recoveryrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetRecoveryRate(seniority, defevent);
                            if (recoveryrate == 0)//on force la recovery car il ne reconnait sophis ne reconnait pas tjours la seniorité
                            {
                                if (seniority == 101) { recoveryrate = 0.4; }//Senior
                                if (seniority == 102) { recoveryrate = 0.05; }//Sub
                            }
                            mdays = maturity - reportingdate;
                            dF = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(currency, reportingdate, maturity);
                            //cdsrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetCreditDefaultSwapRate(recoveryrate, reportingdate, maturity, seniority, midm, defevent, context);
                            //Si on l'a on le set pour le choper par derrière et éviter de refaire un bootstrap
                            cdsrate = ComputeImplCDS(InstrumentPtr);//CDS implicite
                            defaultprobability = 1 - Math.Pow((1 / (1 + (cdsrate * (1 + recoveryrate) / 4 * dF))), (mdays) / 360 * 4);                   
                        }
                        else//option
                        {
                            defaultprobability = 0;
                        }
                        break;

                    default:
                        break;
                }
                return defaultprobability;
            }
            catch (Exception)
            {
                Console.WriteLine("defaultprobability cannot be computed for instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }

        //Fonctions Annexes
        public double ComputeZCRate(CSMInstrument InstrumentPtr)
        {
            double zcrate = 0;
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            int currency = InstrumentPtr.GetCurrency();

            try
            {
                double maturity = InstrumentPtr.GetExpiry();
                zcrate = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(currency, reportingdate, maturity);
                eMDayCountBasisType dcbt = CSMPreference.GetDayCountBasisType();
                double eqyc = CSMDayCountBasis.GetCSRDayCountBasis(dcbt).GetEquivalentYearCount(Convert.ToInt32(reportingdate), Convert.ToInt32(maturity));
                zcrate = Math.Pow((zcrate), 1 / (eqyc)) - 1;
                return zcrate;
            }

            catch (Exception)
            {
                Console.WriteLine("Cannot compute ZC Rate for Instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }
        public double ComputeCDSSpread(CSMInstrument InstrumentPtr)
        {
            sophis.market_data.CSMMarketData context = new CSMMarketData();
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            int currency = InstrumentPtr.GetCurrency();
            int issuer = InstrumentPtr.GetIssuerCode();
            double maturity = InstrumentPtr.GetExpiry();
            //int seniority = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetDefaultSeniority();
            int seniority = InstrumentPtr.GetSeniority();
            int defevent = InstrumentPtr.GetDefaultEvent();
            if (defevent == 0) { defevent = 61; }//On force la valeur à MMR
            double recoveryrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetRecoveryRate(seniority, defevent);
            if (recoveryrate == 0)//on force la recovery car il ne reconnait sophis ne reconnait pas tjours la seniorité
            {
                if (seniority == 101) { recoveryrate = 0.4; }//Senior
                if (seniority == 102) { recoveryrate = 0.05; }//Sub
            }
            double mdays = maturity - reportingdate;
            eMCreditRiskCurve midm = eMCreditRiskCurve.M_mid_market;
            double dF = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(currency, reportingdate, maturity);
            double cdsrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(issuer, currency).GetCreditDefaultSwapRate(recoveryrate, reportingdate, maturity, seniority, midm, defevent, context);
            return cdsrate;
        }
        /// <summary>
        /// Calcul le rendement actuariel continu de l'obligation à partir de son dirty price.
        /// On utilise la méthode de la bi-section 
        /// </summary>
        /// <param name="InstrumentCode"></param>
        /// <param name="DirtyPrice"></param>
        /// <returns></returns>
        public double ComputeBondYTMByDirtyPrice(int InstrumentCode, double DirtyPrice, int reportingdate)
        {
            double tkoYtm;
            try
            {
                //Méthode de calcul dichotomique
                double a, b, c;
                a = -0.5; b = 1.3;
                CSMBond Bond = CSMBond.GetInstance(InstrumentCode);
                int Maturity = Bond.GetExpiry();
                double Nominal = Bond.GetNotional();
                double DirtyPriceTest = 0;

                //Travail sur les flux restants de l'obligation (table "Explanation" de Sophis)
                System.Collections.ArrayList explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                sophis.static_data.eMDayCountBasisType DayCountBasisType=Bond.GetMarketYTMDayCountBasisType();
                double IthDiscountFactor;
                int nbiter = 0;

                while ((b - a) > Math.Pow(10, -5) && nbiter < 1000)
                {
                    c = (a + b) * 0.5;
                    DirtyPriceTest = 0;
                    for (int j = 0; j < nbCF; j++)
                    {
                        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                        IthCoupon = IthRedemption.coupon + IthRedemption.redemption;
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                        IthDiscountFactor = Math.Exp(-c * IthDate);
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
                return tkoYtm;
            }
            catch (Exception)
            {
                Console.WriteLine("Tko Ytm cannot be computed for bond Instrument " + InstrumentCode);
                return 0;
            }
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
   

        //Rating Comp
     
        
        public double ComputeRatingComp(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double comprate = 0;

            try
            {
                comprate = NotationNum(InstrumentPtr);
                return comprate;
            }
            catch (Exception)
            {
                CSMLog.Write("MarketIndicCompute", "ComputeRatingComp", CSMLog.eMVerbosity.M_warning, "Cannot compute RateComp for Instrument " + InstrumentPtr.GetCode());
                return -2;
            }
        }
        public double ComputeRatingSecondComp(CSMPosition PositionPtr, CSMInstrument InstrumentPtr)
        {
            double comprate = 0;

            try
            {
                comprate = NotationSecondNum(InstrumentPtr);
                return comprate;
            }
            catch (Exception)
            {
                CSMLog.Write("MarketIndicCompute", "ComputeRatingSecondComp", CSMLog.eMVerbosity.M_warning, "Cannot compute RateComp for Instrument " + InstrumentPtr.GetCode());
                return -2;
            }
        }
        //Definir la notation numérique suivant la notation de l'agence 
        //quelque soit sa provenance à partir d'une chaine de caractère
        public int DefineNumerating(String notation)
        {
            try
            {
                string tmp = "    ";
                notation += tmp; //augmentation de la taille de la chaine pour passer les tests
                //liés à la fonction Substring()

                if (notation.Substring(0, 3) == "AAA" || notation.Substring(0, 3) == "Aaa" || notation.Substring(0, 5) == "(P)Aaa") return 21;
                else if (notation.Substring(0, 3) == "AA+" || notation.Substring(0, 3) == "Aa1") return 20;
                else if (notation.Trim() == "AA    " || notation == "Aa2    ") return 19;
                else if (notation == "AA-    " || notation == "Aa3    ") return 18;
                else if (notation == "A+    " || notation == "A1    ") return 17;
                else if (notation == "A    " || notation.Substring(0, 2) == "A2") return 16;
                else if (notation == "A-    " || notation == "A3    ") return 15;
                else if (notation == "BBB+    " || notation == "Baa1    ") return 14;
                else if (notation == "BBB    " || notation.Substring(0, 4) == "Baa2") return 13;
                else if (notation.Substring(0, 4) == "BBB-" || notation.Substring(0, 4) == "Baa3") return 12;
                else if (notation.Substring(0, 3) == "BB+" || notation.Substring(0, 3) == "Ba1") return 11;
                else if (notation == "BB    " || notation.Substring(0, 3) == "Ba2") return 10;
                else if (notation.Substring(0, 3) == "BB-" || notation.Substring(0, 3) == "Ba3") return 9;
                else if (notation.Substring(0, 2) == "B+" || notation.Substring(0, 2) == "B1") return 8;
                else if (notation == "B    " || notation.Substring(0, 2) == "B2") return 7;
                else if (notation.Substring(0, 2) == "B-" || notation.Substring(0, 2) == "B3") return 6;
                else if (notation.Substring(0, 4) == "CCC+" || notation.Substring(0, 4) == "Caa1") return 5;
                else if (notation == "CCC    " || notation.Substring(0, 4) == "Caa2") return 4;
                else if (notation.Substring(0, 4) == "CCC-" || notation.Substring(0, 4) == "Caa3") return 3;
                else if (notation.Substring(0, 2) == "CC" || notation.Substring(0, 3) == "Ca1" || notation.Substring(0, 3) == "Ca2") return 2;
                else if (notation == "C    " || notation == "C1    " || notation == "C2    " || notation == "C3    ") return 1;
                else if (notation == "R    " || notation == "D    " || notation == "SD    ") return 0;
                else if (notation == "    ") return -1;
                else return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        //méthode permettant de définir une notation Lettre suivant un chiffre
        //La référence sera la notation S&P
        public string DefineRating(double notationnum)
        {
            string strrating = "";

            if (notationnum == 21) strrating = "AAA";
            else if (notationnum < 21 && notationnum >= 20) strrating = "AA+";
            else if (notationnum < 20 && notationnum >= 19) strrating = "AA";
            else if (notationnum < 19 && notationnum >= 18) strrating = "AA-";
            else if (notationnum < 18 && notationnum >= 17) strrating = "A+";
            else if (notationnum < 17 && notationnum >= 16) strrating = "A";
            else if (notationnum < 16 && notationnum >= 15) strrating = "A-";
            else if (notationnum < 15 && notationnum >= 14) strrating = "BBB+";
            else if (notationnum < 14 && notationnum >= 13) strrating = "BBB";
            else if (notationnum < 13 && notationnum >= 12) strrating = "BBB-";
            else if (notationnum < 12 && notationnum >= 11) strrating = "BB+";
            else if (notationnum < 11 && notationnum >= 10) strrating = "BB";
            else if (notationnum < 10 && notationnum >= 9) strrating = "BB-";
            else if (notationnum < 9 && notationnum >= 8) strrating = "B+";
            else if (notationnum < 8 && notationnum >= 7) strrating = "B";
            else if (notationnum < 7 && notationnum >= 6) strrating = "B-";
            else if (notationnum < 6 && notationnum >= 5) strrating = "CCC+";
            else if (notationnum < 5 && notationnum >= 4) strrating = "CCC";
            else if (notationnum < 4 && notationnum >= 3) strrating = "CCC-";
            else if (notationnum < 3 && notationnum >= 2) strrating = "CC";
            else if (notationnum < 2 && notationnum >= 1) strrating = "C";
            else if (notationnum < 1 && notationnum > 0) strrating = "D";
            else if (notationnum == -1) strrating = "NR";
            return strrating;
        }

        //Méthode permettant de noter un instrument en absence de notation
        //Fonction à compléter
        public int TKRating(CSMInstrument instrument)
        {
            int rating = -1;
            //développer la méthode
            return rating;
        }

        //Trouve la meilleure notation parmi l'ensemble des notes d'un instrument
        //( code agence supposé 41 SP/100 Moodys/182 Fitch)
        public int NotationNum(CSMInstrument instrument)
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
                        tabrating[j] = DefineNumerating(notation[j]);

                    rating = FindBestRatingNum(tabrating);
                }

                return rating;
            }

            catch (Exception)
            {
                CSMLog.Write("MarketIndicCompute", "NotationNum", CSMLog.eMVerbosity.M_warning, "Cannot compute NotationNum for Instrument " + instrument.GetCode());
                return -3;
            }
        }
        public int NotationSecondNum(CSMInstrument instrument)
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
                        tabrating[j] = DefineNumerating(notation[j]);

                    rating = FindSecondRatingNum(tabrating);
                }

                return rating;
            }

            catch (Exception)
            {
                CSMLog.Write("MarketIndicCompute", "NotationSecondNum", CSMLog.eMVerbosity.M_warning, "Cannot compute NotationNum for Instrument " + instrument.GetCode());

                return -3;
            }
        }
        //Trouve la plus grande notation
        public int FindBestRatingNum(int[] tabrating)
        {
            int bestrating = -1;

            //mise à jour
            if (tabrating[0] > -1) bestrating = tabrating[0];

            // Parcourir l'ensemble des notations d'un instrument puis conserver la valeur la plus élévée
            for (int i = 0; i < 2; i++)
                if (tabrating[i + 1] > bestrating) bestrating = tabrating[i + 1];

            return bestrating;
        }

        //trouve la plus petite notation
        public int FindWorstRatingNum(int[] tabrating)
        {
            int worstrating = -1;
            //mise à jour
            if (tabrating[0] > -1) worstrating = tabrating[0];

            for (int i = 0; i < 2; i++)
                if (tabrating[i + 1] < worstrating && tabrating[i + 1] != -1) worstrating = tabrating[i + 1];

            return worstrating;
        }

        //Trouve la seconde best notation
        public int FindSecondRatingNum(int[] tabrating)
        {

            int temp=-1;
            int cpt=0;
            //Si une seule notation on sort direct
            for (int i = 0; i < 3; i++)
            {
                if (tabrating[i] < 0) cpt++;
                else temp = tabrating[i];
            }


            if(cpt==1) return temp;

            int bestrating = -1;

            //mise à jour
            if (tabrating[0] > -1) bestrating = tabrating[0];

            // Parcourir l'ensemble des notations d'un instrument puis conserver la valeur la plus élévée
            for (int i = 0; i < 2; i++)
                if (tabrating[i + 1] > bestrating) bestrating = tabrating[i + 1];

            for (int i = 0; i < 2; i++)
            {
                if (tabrating[i + 1] == bestrating)
                {
                    tabrating[i + 1] = -1;
                    i = 2;
                }
            }

            bestrating = -1;

            if (tabrating[0] > -1) bestrating = tabrating[0];

            //deuxième boucle
            for (int i = 0; i < 2; i++)
                if (tabrating[i + 1] > bestrating) bestrating = tabrating[i + 1];

            return bestrating;
        }
    }

    //Fonctions de gestions de l'affichage des colonnes

    public class DataSourceInternalGearing
    {
        // Holds the instance of the singleton class
        private static DataSourceInternalGearing Instance = null;

        public static Hashtable DataCacheGearing;//Cache pour le Gearing par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceInternalGearing()
        {
            DataCacheGearing = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceInternalGearing GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceInternalGearing();
            return Instance;
        }

        public double GetInternalGearing(CSMPosition Position, CSMInstrument Instrument)
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
            if (Nominal != 0 | Instrument.GetInstrumentType().ToString().Equals("A"))
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheGearing.ContainsKey(PositionIdentifier))
                        DataCacheGearing[PositionIdentifier] = 0.0;
                    else
                        DataCacheGearing.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide
                if (!DataCacheGearing.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheGearing.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheGearing[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            int InstrumentId = Instrument.GetCode();

            //mise à jour du Gearing
            double Gearing = IMarketIndic.ComputeGearing(Position, Instrument);
            if (DataCacheGearing.ContainsKey(PositionId))
                DataCacheGearing[PositionId] = Gearing;
            else
                DataCacheGearing.Add(PositionId, Gearing);

            //mise à jour de la version de l'instrument
            if (DataCacheInstrVersion.ContainsKey(InstrumentId))
                DataCacheInstrVersion[InstrumentId] = Instrument.GetVersion();
            else
                DataCacheInstrVersion.Add(InstrumentId, Instrument.GetVersion());

            IMarketIndic.Close();
        }
    }

    public class DataSourceGearing
    {
        // Holds the instance of the singleton class
        private static DataSourceGearing Instance = null;

        public static Hashtable DataCacheGearing;//Cache pour le Gearing par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceGearing()
        {
            DataCacheGearing = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceGearing GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceGearing();
            return Instance;
        }

        public double GetGearing(CSMPosition Position, CSMInstrument Instrument)
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
            if (Nominal != 0 | Instrument.GetInstrumentType().ToString().Equals("A"))
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheGearing.ContainsKey(PositionIdentifier))
                        DataCacheGearing[PositionIdentifier] = 0.0;
                    else
                        DataCacheGearing.Add(PositionIdentifier, 0.0);
                    
                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide
                if (!DataCacheGearing.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheGearing.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheGearing[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }
        
        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
            int PositionId = Position.GetIdentifier();
            int InstrumentId = Instrument.GetCode();

            //mise à jour du Gearing
            double Gearing = IMarketIndic.ComputeGearing(Position, Instrument);
            if (DataCacheGearing.ContainsKey(PositionId))
                DataCacheGearing[PositionId] = Gearing;
            else
                DataCacheGearing.Add(PositionId, Gearing);

            //mise à jour de la version de l'instrument
            if (DataCacheInstrVersion.ContainsKey(InstrumentId))
                DataCacheInstrVersion[InstrumentId] = Instrument.GetVersion();
            else
                DataCacheInstrVersion.Add(InstrumentId, Instrument.GetVersion());

            IMarketIndic.Close();
        }
    }

    public class DataSourceImplCDS
    {
        // Holds the instance of the singleton class
        private static DataSourceImplCDS Instance = null;

        public static Hashtable DataCacheImplCDS;//Cache pour la valeur du CDS impl par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceImplCDS()
        {
            DataCacheImplCDS = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();

        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceImplCDS GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceImplCDS();
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
        public double GetImplCDS(CSMPosition Position, CSMInstrument Instrument)
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
                if (!DataCacheImplCDS.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheImplCDS.ContainsKey(Instrumentcode))
                {
                    return (double)DataCacheImplCDS[Instrumentcode];
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
            //mise à jour du cds implicite
            MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            double ImplCDS = IMarketIndic.ComputeImplCDS(Instrument)*100;
            if (DataCacheImplCDS.ContainsKey(InstrumentCode))
                DataCacheImplCDS[InstrumentCode] = ImplCDS;
            else
                DataCacheImplCDS.Add(InstrumentCode, ImplCDS);

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

            IMarketIndic.Close();
        }
    }
    
    public class DataSourceBasis
    {
        // Holds the instance of the singleton class
        private static DataSourceBasis Instance = null;
 
        public static Hashtable DataCacheBasis;//Cace pour la base par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceBasis()
        {
            DataCacheBasis = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceBasis GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceBasis();
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
        public double GetBasis(CSMPosition Position, CSMInstrument Instrument)
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
                if (!DataCacheBasis.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheBasis.ContainsKey(Instrumentcode))
                {
                    return (double)DataCacheBasis[Instrumentcode];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        private void FillCache(CSMInstrument Instrument)
        {
            //mise à jour de la base
            MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            double Basis = IMarketIndic.ComputeBasis(Instrument)*100;
            if (DataCacheBasis.ContainsKey(InstrumentCode))
                DataCacheBasis[InstrumentCode] = Basis;
            else
                DataCacheBasis.Add(InstrumentCode, Basis);

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

            IMarketIndic.Close();
        }
    }
    
    public class DataSourceDefaultProb
    {
        // Holds the instance of the singleton class
        private static DataSourceDefaultProb Instance = null;

        public static Hashtable DataCacheDefaultProb;//Cache pour la proba de défaut par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceDefaultProb()
        {
            DataCacheDefaultProb = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceDefaultProb GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceDefaultProb();
            return Instance;
        }

        public double GetDefaultProb(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Instrument.GetCode();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0)
            {
                //La valeur est (re)calculée:
                //si le cache est vide 
                //si la version de l'instrument change
                //si le spot change
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                if (!DataCacheDefaultProb.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument, Position);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheDefaultProb.ContainsKey(Instrumentcode))
                {
                    return (double)DataCacheDefaultProb[Instrumentcode];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }
        }

        private void FillCache(CSMInstrument Instrument, CSMPosition Position)
        {
            //mise à jour de la proba de défaut
            MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            double DefaultProb = IMarketIndic.ComputeDefaultProbability(Instrument, Position) * 100;
            if (DataCacheDefaultProb.ContainsKey(InstrumentCode))
                DataCacheDefaultProb[InstrumentCode] = DefaultProb;
            else
                DataCacheDefaultProb.Add(InstrumentCode, DefaultProb);

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

            IMarketIndic.Close();
        }
    }

    public class DataSourceRatingComp
    {
        // Holds the instance of the singleton class
        private static DataSourceRatingComp Instance = null;

        public static Hashtable DataCacheRatingComp;//Cache pour le Rateing par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public static MarketIndicCompute IMarketIndic;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceRatingComp()
        {
            DataCacheRatingComp = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            IMarketIndic = MarketIndicCompute.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceRatingComp GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceRatingComp();
            return Instance;
        }

        public double GetRatingComp(CSMPosition Position, CSMInstrument Instrument)
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
                    if (DataCacheRatingComp.ContainsKey(PositionIdentifier))
                        DataCacheRatingComp[PositionIdentifier] = 0.0;
                    else
                        DataCacheRatingComp.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide
                if (!DataCacheRatingComp.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheRatingComp.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheRatingComp[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {


            int PositionId = Position.GetIdentifier();
            int InstrumentId = Instrument.GetCode();

            //mise à jour du Rating
            double RatingComp = IMarketIndic.ComputeRatingComp(Position, Instrument);
            if (DataCacheRatingComp.ContainsKey(PositionId))
                DataCacheRatingComp[PositionId] = RatingComp;
            else
                DataCacheRatingComp.Add(PositionId, RatingComp);

            //mise à jour de la version de l'instrument
            if (DataCacheInstrVersion.ContainsKey(InstrumentId))
                DataCacheInstrVersion[InstrumentId] = Instrument.GetVersion();
            else
                DataCacheInstrVersion.Add(InstrumentId, Instrument.GetVersion());

        }
    }

    public class DataSourceRatingSecondComp
    {
        // Holds the instance of the singleton class
        private static DataSourceRatingSecondComp Instance = null;

        public static Hashtable DataCacheRatingComp;//Cache pour le Rateing par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public static MarketIndicCompute IMarketIndic;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceRatingSecondComp()
        {
            DataCacheRatingComp = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            IMarketIndic = MarketIndicCompute.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceRatingSecondComp GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceRatingSecondComp();
            return Instance;
        }

        public double GetRatingComp(CSMPosition Position, CSMInstrument Instrument)
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
                    if (DataCacheRatingComp.ContainsKey(PositionIdentifier))
                        DataCacheRatingComp[PositionIdentifier] = 0.0;
                    else
                        DataCacheRatingComp.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide
                if (!DataCacheRatingComp.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheRatingComp.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheRatingComp[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {


            int PositionId = Position.GetIdentifier();
            int InstrumentId = Instrument.GetCode();

            //mise à jour du Rating
            double RatingComp = IMarketIndic.ComputeRatingSecondComp(Position, Instrument);
            if (DataCacheRatingComp.ContainsKey(PositionId))
                DataCacheRatingComp[PositionId] = RatingComp;
            else
                DataCacheRatingComp.Add(PositionId, RatingComp);

            //mise à jour de la version de l'instrument
            if (DataCacheInstrVersion.ContainsKey(InstrumentId))
                DataCacheInstrVersion[InstrumentId] = Instrument.GetVersion();
            else
                DataCacheInstrVersion.Add(InstrumentId, Instrument.GetVersion());

        }
    }
}
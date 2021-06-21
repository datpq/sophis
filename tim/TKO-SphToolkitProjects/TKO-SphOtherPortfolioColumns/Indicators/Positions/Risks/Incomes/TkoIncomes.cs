using sophis.market_data;
using sophis.portfolio;
using sophis.instrument;
using System;
using sophis.utils;
using TkoPortfolioColumn.DbRequester;
using sophis.static_data;
using System.Collections;

//@DPH
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class TikehauIncomes
    {
        #region TKO Received Coupons CCY
        public static  double TkoComputeReceivedCouponsLocalCCY(this CSMInstrument instrument, InputProvider input)
        {

            CSMTransactionVector transactionvector = new CSMTransactionVector();
            input.Position.GetTransactions(transactionvector);
            double montantCoupon = 0;
            double montantDeferredCoupon = 0;
            if (transactionvector.Count > 0)
            {
                foreach (CSMTransaction deal in transactionvector)
                {
                    eMTransactionType transactionType = deal.GetTransactionType();
                    int dateneg = deal.GetTransactionDate();
                    int dateval = deal.GetSettlementDate();
                    if (transactionType != eMTransactionType.M_tCoupon &&
                        dateneg <= input.ReportingDate)
                    {
                        montantCoupon += deal.GetNetAmount();
                    }

                    //Deferred Coupon.
                    if ((int)transactionType != 360 &&
                        dateval <= input.ReportingDate)
                    {
                        montantDeferredCoupon += deal.GetNetAmount();
                    }
                }
            }

            double receivedcoupons = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                switch (input.Instrument.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        receivedcoupons = receivedcoupons - montantDeferredCoupon;
                        break;
                    case "A"://Actions
                        //deferred coupons Mezzanine à rajouter une fois la date passée
                        receivedcoupons = receivedcoupons - montantDeferredCoupon;;
                        break;
                    case "D"://Options et convertibles
                        CMString otype = "";
                        input.Instrument.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                            receivedcoupons = receivedcoupons - montantDeferredCoupon;
                        else//option
                            receivedcoupons = 0;
                        break;
                    default:
                        break;
                }
                return receivedcoupons;
            }
            catch (Exception)
            {
                CSMLog.Write("TikehauCoupon", "TKOComputeReceivedCouponsLocalCCY", CSMLog.eMVerbosity.M_warning, "receivedcoupons cannot be computed for position " + input.PositionIdentifier);
                return 0;
            }
        }
        #endregion 

        #region TKO Received Coupons
        public static double TkoComputeReceivedCoupons(this CSMInstrument InstrumentPtr, InputProvider input)
        {
            double receivedcoupons = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                CSMTransactionVector transactionvector = new CSMTransactionVector();
                input.Position.GetTransactions(transactionvector);
                double montantCoupon = 0;
                double montantDeferredCoupon = 0;
                if (transactionvector.Count > 0)
                {
                    foreach (CSMTransaction deal in transactionvector)
                    {
                        eMTransactionType transactionType = deal.GetTransactionType();
                        int dateneg = deal.GetTransactionDate();
                        int dateval = deal.GetSettlementDate();
                        if (transactionType != eMTransactionType.M_tCoupon &&
                            dateneg <= input.ReportingDate)
                        {
                            montantCoupon += deal.GetNetAmount();
                        }

                        //Deferred Coupon.
                        if ((int)transactionType != 360 &&
                            dateval <= input.ReportingDate)
                        {
                            montantDeferredCoupon += deal.GetNetAmount();
                        }
                    }
                }

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        receivedcoupons = receivedcoupons - montantDeferredCoupon;
                        break;

                    case "A"://Actions
                        receivedcoupons = receivedcoupons - montantDeferredCoupon;
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            receivedcoupons = receivedcoupons - montantDeferredCoupon;
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
                CSMLog.Write("TikehauCoupon", "TkoComputeReceivedCoupons", CSMLog.eMVerbosity.M_error, "receivedcoupons cannot be computed for position " + input.PositionIdentifier);
                return 0;
            }
        }
        #endregion

        #region TKO Daily Carry Coupon

        public static double TkoComputeDailyCarryCoupon(this CSMInstrument InstrumentPtr, InputProvider input)
        {
            double dcarrycoupon = 0;
            try
            {
                int reportingdate = input.ReportingDate;
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                double nominal = 0;

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        nominal = input.Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        dcarrycoupon = nominal * InstrumentPtr.TkoComputeFirstCoupon(input) / 365;
                        break;

                    case "A"://Actions
                        dcarrycoupon = 0;
                        break;

                    case "S"://Swap
                        CSMIssuer Issuer = CSMIssuer.GetInstance(InstrumentPtr.GetIssuerCode());
                        if (Issuer.GetFirstDefaultDate(0, 0) <= reportingdate && Issuer.GetFirstDefaultDate(0, 0) > 0)
                        {
                            dcarrycoupon = 0;
                        }
                        else
                        {
                            nominal = input.Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
                            double avp = DbrStringQuery.getResultD("select fixe2 from TITRES where SICOVAM=" + InstrumentPtr.GetCode() + "") * 100;
                            dcarrycoupon = ((Math.Pow(1 + (avp / 10000) / 4, 4) - 1) * -nominal) / 365;
                        }
                        break;

                    case "P"://Repo
                        nominal = input.Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        //Si on fait un repo sur autre chose que du fixed income
                        if (nominal == 0) { nominal = input.Position.GetInstrumentCount(); }

                        int PositionMvtident = input.Position.GetIdentifier();
                        int reratenumber = DbrStringQuery.getResultI("select count(*) from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ")");
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
                            int segment_date_end = DbrStringQuery.getResultI("select * from (select date_to_num(dateval) from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ") and DATEVAL> num_to_date(" + initial_date + ") order by DATEVAL ASC) where ROWNUM=1");
                            double segment_taux = DbrStringQuery.getResultD("select * from (select cours from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ") and DATEVAL> num_to_date(" + initial_date + ") order by DATEVAL ASC) where ROWNUM=1") * 0.01;

                            //on entre dans une boucle qui démarre sur le second rerate et qui s'arrête à l'avant dernier
                            for (int j = 2; j < reratenumber + 1; j++)
                            {
                                segment_taux = DbrStringQuery.getResultD("select * from (select cours from (select cours,RANK() OVER (order by h.DATEVAL ASC) r from histomvts h where h.TYPE=28 and h.MVTIDENT=" + PositionMvtident + " and h.DATEVAL < num_to_date (" + reportingdate + ") and h.DATEVAL> num_to_date(" + initial_date + ")) where r=" + j + " )") * 0.01;
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
                            nominal = input.Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
                            dcarrycoupon = nominal * InstrumentPtr.TkoComputeFirstCoupon(input) / 365;
                        }
                        else//option
                        {
                            dcarrycoupon = 0;
                        }
                        break;

                    case "T"://billets de trésorerie
                        nominal = input.Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        dcarrycoupon = nominal * InstrumentPtr.TkoComputeFirstCoupon(input) / 365;
                        break;

                    default:
                        break;
                }
                return dcarrycoupon * fxspot;
            }
            catch (Exception ex)
            {
                CSMLog.Write("DurationExtentionMethods", "ComputeDailyCarryCoupon", CSMLog.eMVerbosity.M_warning, "dcarrycoupon cannot be computed for position " + input.Position.GetIdentifier());
                return 0;
            }
        }

        public static double TkoComputeFirstCoupon(this CSMInstrument InstrumentPtr,InputProvider input)
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
                        RedemptionArray = InstrumentPtr.TkoGetBondExplanationArray(input);
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
                CSMLog.Write("TikehauIncome", "TkoComputeFirstCoupon", CSMLog.eMVerbosity.M_warning, "coupon cannot be computed for instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }
        #endregion

        #region TkoComputeDailyCarryAct
        public static double TkoComputeDailyCarryAct(this CSMInstrument InstrumentPtr, InputProvider input)
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
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());

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
                        explicationArray = InstrumentPtr.TkoGetBondExplanationArray(input);
                        nbCF = explicationArray.Count;
                        notional = InstrumentPtr.GetNotional();
                        nominal = InstrumentPtr.GetInstrumentCount() * notional;

                        //Variables relatives au ième flux
                        dcarryact = -dirtyprice * nominal;
                        for (int j = 0; j < nbCF; j++)
                        {
                            IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                            dcarryact += (IthRedemption.coupon + IthRedemption.redemption) * input.Position.GetInstrumentCount();
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
                            nominal = input.Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
                            SP = 1 - InstrumentPtr.TkoComputeDefaultProbability(input);//Je ne suis pas du tout sure que cette proba de survie soit bonne. C'est ce qui était fait dans l'ancien reporting
                            dcarryact = InstrumentPtr.TkoComputeDailyCarryCoupon(input) / fxspot + SP * (-input.Position.GetAssetValue() * 1000 / (InstrumentPtr.GetExpiry() - reportingdate));
                        }
                        break;

                    case "P"://Repos
                        double dcarrycash = 0;

                        nominal = input.Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
                        //Si on fait un repo sur autre chose que du fixed income
                        if (nominal == 0) { nominal = input.Position.GetInstrumentCount(); }

                        int PositionMvtident =  input.Position.GetIdentifier();
                        int reratenumber = DbrStringQuery.getResultI("select count(*) from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ")");
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
                            int segment_date_end = DbrStringQuery.getResultI("select * from (select date_to_num(dateval) from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ") and DATEVAL> num_to_date(" + initial_date + ") order by DATEVAL ASC) where ROWNUM=1");
                            double segment_taux = DbrStringQuery.getResultD("select * from (select cours from HISTOMVTS where TYPE=28 and MVTIDENT=" + PositionMvtident + " and DATEVAL < num_to_date (" + reportingdate + ") and DATEVAL> num_to_date(" + initial_date + ") order by DATEVAL ASC) where ROWNUM=1") * 0.01;

                            //on entre dans une boucle qui démarre sur le second rerate et qui s'arrête à l'avant dernier
                            for (int j = 2; j < reratenumber + 1; j++)
                            {
                                segment_taux = DbrStringQuery.getResultD("select * from (select cours from (select cours,RANK() OVER (order by h.DATEVAL ASC) r from histomvts h where h.TYPE=28 and h.MVTIDENT=" + PositionMvtident + " and h.DATEVAL < num_to_date (" + reportingdate + ") and h.DATEVAL> num_to_date(" + initial_date + ")) where r=" + j + " )") * 0.01;
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
                            explicationArray = InstrumentPtr.TkoGetBondExplanationArray(input);
                            nbCF = explicationArray.Count;
                            notional = InstrumentPtr.GetNotional();
                            nominal = input.Position.GetInstrumentCount() * notional;

                            //Variables relatives au ième flux
                            dcarryact = -dirtyprice * nominal;
                            for (int j = 0; j < nbCF; j++)
                            {
                                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                                dcarryact += (IthRedemption.coupon + IthRedemption.redemption) *   input.Position.GetInstrumentCount();
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
                        nominal = input.Position.GetInstrumentCount() * notional;
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
                CSMLog.Write("TikehauCoupon", "TkoComputeDailyCarryAct", CSMLog.eMVerbosity.M_warning, "dcarryeco cannot be computed for position " + input.Position.GetIdentifier());
                return 0;
            }
        }

        public static double TkoComputeDefaultProbability(this CSMInstrument InstrumentPtr, InputProvider input)
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
                        cdsrate = InstrumentPtr.TkoComputeImplCDS(input);//CDS implicite
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
                        valtype = (int) input.Position.GetValorisationType();
                        // On teste si la valo est faite avec le MtoM. Dans ce cas, on récupère le spot 
                        // qui est le CDS rate bootstrapé. On divise par 100 pour l'avoir en absolu
                        cdsrate = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentPtr.GetCode());
                        if (valtype.Equals(2))
                        { cdsrate = cdsrate / 100; }
                        // Dans le cas des loans valorisés uniquement par MS, ou si la valo est faite en PFPC/MS
                        // On va chercher dans la base le spread MS le plus proche of course inférieur à la date du reporting
                        // Qui sera stocké dans l'historique, colonne PB_CDS_SPREAD
                        if (valtype.Equals(1))
                        { cdsrate = DbrStringQuery.getResultD("select * from (select PB_CDS_SPREAD from historique where SICOVAM=" + InstrumentPtr.GetCode() + " and JOUR < num_to_date(" + reportingdate + ") and PB_CDS_SPREAD is not null order by JOUR DESC) where ROWNUM=1") / 100; }
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
                            cdsrate = InstrumentPtr.TkoComputeImplCDS(input);//CDS implicite
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
                CSMLog.Write("TikehauCoupon", "TkoComputeDefaultProbability", CSMLog.eMVerbosity.M_warning, "defaultprobability cannot be computed for instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }

        public static double TkoComputeImplCDS(this CSMInstrument InstrumentPtr, InputProvider input)
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
                        ytm = InstrumentPtr.TkoComputeBondYTMByDirtyPrice(input,dirtyPrice);
                        zcrate = InstrumentPtr.TkoComputeZCRate(input);
                        implcds = (ytm - zcrate);
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            ytm = InstrumentPtr.GetYTMMtoM();
                            zcrate = InstrumentPtr.TkoComputeZCRate(input);
                            implcds = (ytm - zcrate);
                            zcrate = InstrumentPtr.TkoComputeZCRate(input);
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
                CSMLog.Write("TikehauCoupon", "TkoComputeImplCDS", CSMLog.eMVerbosity.M_warning, "implcds cannot be computed for instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }

        public static double TkoComputeBondYTMByDirtyPrice(this CSMInstrument InstrumentPtr,InputProvider input,double DirtyPrice)
        {
            double tkoYtm;
            try
            {
                //Méthode de calcul dichotomique
                double a, b, c;
                a = -0.5; b = 1.3;
                CSMBond Bond = CSMBond.GetInstance(input.InstrumentCode);
                int Maturity = Bond.GetExpiry();
                double Nominal = Bond.GetNotional();
                double DirtyPriceTest = 0;

                //Travail sur les flux restants de l'obligation (table "Explanation" de Sophis)
                System.Collections.ArrayList explicationArray = InstrumentPtr.TkoGetBondExplanationArray(input);
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                sophis.static_data.eMDayCountBasisType DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
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
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(input.ReportingDate, IthRedemption.endDate);
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
                CSMLog.Write("TikehauCoupon", "TkoComputeBondYTMByDirtyPrice", CSMLog.eMVerbosity.M_warning, "Tko Ytm cannot be computed for bond Instrument " + InstrumentPtr.GetCode());
                return 0;
            }
        }

        public static double TkoComputeZCRate(this CSMInstrument InstrumentPtr, InputProvider input)
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
                CSMLog.Write("TikehauCoupon", "TkoComputeZCRate", CSMLog.eMVerbosity.M_warning, "Cannot compute ZC Rate for Instrument[" + InstrumentPtr.GetCode() + "]");
                return 0;
            }
        }

        #endregion 

        #region TkoGetDefferedCoupon

        public static double TkoGetDefferedCoupon(this CSMInstrument InstrumentPtr, InputProvider input)
        {
            double defcoupon = 0;
            int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            defcoupon = -   DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + input.PositionIdentifier + " and TYPE=360 and DATEVAL > num_to_date(" + reportingdate + ") and DATENEG <= num_to_date(" + reportingdate + ")");
            input.IndicatorValue =  defcoupon;
            return  input.IndicatorValue ;
        }

        #endregion

        #region TkoAvMonthNetReturn

        public static double TkoAvMonthNetReturn(this CSMInstrument instrument,InputProvider input, CSMPortfolio portfolio)
        {
             // Set Value
            int Count;
            int folioSicovam;
            folioSicovam = DbrStringQuery.getResultI("select SICOVAM from titres where type='Z'and REFERENCE = '" + portfolio.GetName() + "'");
            if (folioSicovam > 0)
            {
                double[,] tab;//Tableau pour stoker les résultats de la requete i.e. 
                Count = DbrStringQuery.getResultI("select count(*) from fund_navforeod where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')");
                tab = new double[Count, 2];//Count lignes, 2 colonnes
                tab = DbrStringQuery.getResultTab2("select cast(to_char(to_date(NAV_DATE),'MM') as int), NAV from fund_navforeod where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')", Count, 2);

                int i = 0;
                double MonthPerf;//Perf net mensuelle
                int MonthLast;//Indice de la dernière valeur du mois précédent
                int nbMonthPerf = 0;//Nombre de performance mensuelles mesurées

                //On se place sur le 1er indice du mois suivant le mois de 
                //création pour éviter d'avoir une 1ère perf fausse si pas un mois complet
                while (tab[i, 0] == tab[i + 1, 0]) { i++; }
                MonthLast = i;
                i++;//A ce stade, i est l'indice de la 1ere valeur du mois suivant le mois de création
                double doubleValue = 0;
                while (i < (Count - 1))
                {
                    while (i < (Count - 1) && tab[i, 0] == tab[i + 1, 0]) { i++; }
                    if (i < (Count - 1))
                    {
                        //A ce stade, i est la dernière valeur du mois courant
                        MonthPerf = tab[i, 1] / tab[MonthLast, 1] - 1;
                        doubleValue += MonthPerf;
                        nbMonthPerf++;
                        MonthLast = i;//Indice de la 1ère valeur du mois suivant
                        i++;
                    }
                }
                doubleValue = doubleValue / nbMonthPerf;
                doubleValue = doubleValue * 100;

                input.IndicatorValue = doubleValue;
                return input.IndicatorValue;
                //Le dernier mois n'est pas compté dans cette boucle 
            }
            return input.IndicatorValue;
        }
        #endregion 

        #region TKO Inv Cash

        public static double TkoComputeInvestedCash(this CSMInstrument InstrumentPtr, InputProvider input)
        {
            double invcash = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                int PositionMvtident = input.Position.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        invcash = DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
                        break;

                    case "A"://Actions
                        invcash = DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            invcash = DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
                        }
                        else//option
                        {
                            invcash = 0;
                        }
                        break;

                    case "T"://Billets de treso
                        invcash = DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=1 and DATENEG <=num_to_date(" + reportingdate + ")");
                        break;

                    default:
                        break;
                }
                invcash = invcash * fxspot;
                return invcash;
            }
            catch (Exception)
            {
                CSMLog.Write("TikehauCoupon", "TkoComputeZCRate", CSMLog.eMVerbosity.M_warning, "invcash cannot be computed for position [" + input.Position.GetIdentifier()  + "]");
                return 0;
            }
        }

        #endregion    

        #region TKR Net Return Since Incep
        public static double TkoNetReturnSinceInception(this CSMInstrument InstrumentPtr, InputProvider input)
        {
            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            if (instrType != "A")
            {
                input.IndicatorValue = 0;
                return 0;
            }
            double minNAV;
            double lastNAV;
            double folioSicovam;
            DbrStringQuery.Initialize();
            folioSicovam = DbrStringQuery.getResultI("select SICOVAM from titres where type='Z'and REFERENCE = '" + input.PortFolioName + "'");
            if (folioSicovam > 0)
            {
                minNAV = DbrStringQuery.getResultD("select NAV from FUND_NAVFOREOD where SICOVAM = '" + folioSicovam + "'and NAV_DATE in (select Min(NAV_DATE) from FUND_NAVFOREOD where sicovam='" + folioSicovam + "')");
                lastNAV = DbrStringQuery.getResultD("select NAV from FUND_NAVFOREOD where SICOVAM = '" + folioSicovam + "' and NAV_DATE in (select Max(NAV_DATE) from FUND_NAVFOREOD where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "'))");
                input.IndicatorValue = lastNAV / minNAV - 1;
            }
            input.IndicatorValue  = input.IndicatorValue * 100;
            return input.IndicatorValue;
        }
        #endregion 

        #region TKR % of Positive Months
        public static double TkoPositiveMonths(this CSMInstrument InstrumentPtr, InputProvider input)
        {
            double[,] tab;//Tableau pour stoker les résultats de la requete i.e. les dates et les NAV correspondantes
            int Count;//Nombre de dates pour lesquels le fond concerné affiche une NAV

            DbrStringQuery.Initialize();

            int i;//Compteur sur le nombre de lignes de tab
            int nbPerf;//Nombre de perf mensuelles calculées
            double MonthPerf;//Perf net mensuelle
            int MonthLast;//Indice de la dernière valeur du mois précédent
            int folioSicovam;
            folioSicovam = DbrStringQuery.getResultI("select SICOVAM from titres where type='Z'and REFERENCE = '" + input.PortFolioName + "'");
            if (folioSicovam > 0)
            {
                Count = DbrStringQuery.getResultI("select count(*) from fund_navforeod where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')");
                tab = new double[Count, 2];//'Count' lignes, 2 colonnes
                tab = DbrStringQuery.getResultTab2("select cast(to_char(to_date(NAV_DATE),'MM') as int), NAV from fund_navforeod where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')", Count, 2);

                //On se place sur le 1er indice du mois suivant le mois de 
                //création pour éviter d'avoir une 1ère perf fausse si pas un mois complet
                i = 0;
                nbPerf = 0;
                while (tab[i, 0] == tab[i + 1, 0]) { i++; }
                MonthLast = i;
                i++;//A ce stade, i est l'indice de la 1ere valeur du mois suivant le mois de création
                input.IndicatorValue = 0;
                while (i < (Count - 1))
                {
                    while (i < (Count - 1) && tab[i, 0] == tab[i + 1, 0]) { i++; }
                    if (i < (Count - 1))
                    {
                        //A ce stade, i est la dernière valeur du mois courant
                        MonthPerf = tab[i, 1] / tab[MonthLast, 1] - 1;
                        if (MonthPerf >= 0) { input.IndicatorValue++; }
                        nbPerf++;
                        MonthLast = i;//Indice de la 1ère valeur du mois suivant
                        i++;
                    }
                }
                input.IndicatorValue = 100 * input.IndicatorValue / nbPerf;
                //Le dernier mois n'est pas compté dans cette boucle 
            }
            return input.IndicatorValue;
        }
        #endregion

        #region TKO Received Coupons CCY

        public static double TkoComputeReceivedCouponsCCY(this CSMInstrument InstrumentPtr, InputProvider input)
        {
            double receivedcoupons = 0;
            try
            {
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                int PositionMvtident = input.PositionIdentifier;
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (InstrumentPtr.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        receivedcoupons = -DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                        //deferred coupons Mezzanine à rajouter une fois la date passée
                        receivedcoupons = receivedcoupons - DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
                        break;

                    case "A"://Actions
                        receivedcoupons = -DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                        //deferred coupons Mezzanine à rajouter une fois la date passée
                        receivedcoupons = receivedcoupons - DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
                        break;

                    case "D"://Options et convertibles
                        CMString otype = "";
                        InstrumentPtr.GetModelName(otype);
                        string otypes = otype.GetString();
                        if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                        {
                            receivedcoupons = -DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
                            //deferred coupons Mezzanine à rajouter une fois la date passée
                            receivedcoupons = receivedcoupons - DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
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
                CSMLog.Write("TikehauIncomes", "TkoComputeReceivedCouponsLocalCCY", CSMLog.eMVerbosity.M_warning, "receivedcoupons cannot be computed for position " + input.PositionIdentifier);
                return 0;
            }
        }

        #endregion

        //#region TKO Received Coupons

        //public static double TkoComputeReceivedCoupons(this CSMInstrument InstrumentPtr, InputProvider input)
        //{
        //    double receivedcoupons = 0;
        //    try
        //    {
        //        double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
        //        int PositionMvtident = input.Position.GetIdentifier();
        //        int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

        //        switch (InstrumentPtr.GetInstrumentType().ToString())
        //        {
        //            case "O"://Obligation
        //                receivedcoupons = -DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
        //                //deferred coupons Mezzanine à rajouter une fois la date passée
        //                receivedcoupons = receivedcoupons - DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
        //                break;

        //            case "A"://Actions
        //                receivedcoupons = -DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
        //                //deferred coupons Mezzanine à rajouter une fois la date passée
        //                receivedcoupons = receivedcoupons - DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
        //                break;

        //            case "D"://Options et convertibles
        //                CMString otype = "";
        //                InstrumentPtr.GetModelName(otype);
        //                string otypes = otype.GetString();
        //                if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
        //                {
        //                    receivedcoupons = -DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=2 and DATENEG <=num_to_date(" + reportingdate + ")");
        //                    //deferred coupons Mezzanine à rajouter une fois la date passée
        //                    receivedcoupons = receivedcoupons - DbrStringQuery.getResultD("select SUM(MONTANT) from HISTOMVTS where MVTIDENT=" + PositionMvtident + " and TYPE=360 and DATEVAL <=num_to_date(" + reportingdate + ")");
        //                }
        //                else//option
        //                {
        //                    receivedcoupons = 0;
        //                }
        //                break;

        //            default:
        //                break;
        //        }
        //        receivedcoupons = receivedcoupons * fxspot;

        //        return receivedcoupons;
        //    }
        //    catch (Exception)
        //    {
        //        CSMLog.Write("TikehauIncomes", "TkoComputeReceivedCoupons", CSMLog.eMVerbosity.M_warning, "receivedcoupons cannot be computed for position " + input.PositionIdentifier);
        //        return 0;
        //    }
        //}
        //#endregion
    }
}
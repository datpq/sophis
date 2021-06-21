using System.Text;
using sophis.portfolio;
using System.Collections;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using TkoPortfolioColumn.DataCache;
using sophis.static_data;
using TkoPortfolioColumn.DbRequester;
using System.ComponentModel;
using System;

//@DPH
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class DurationExtentionMethods
    {
        #region Duration CR

        public static double TkoComputeDurationValue(this CSMInstrument instrument, InputProvider input)
        {
            double duration = 0;
            input.Instrument = instrument;
            //SI futures
            
            //@SB
            input.TmpPortfolioColName = "Allotment";
            string allotFlag = Helper.TkoGetValuefromSophisString(input);
            if (allotFlag.Equals("IR Futures CT", StringComparison.InvariantCultureIgnoreCase))
            {
                duration = 0;
            }
            else if (allotFlag.Equals("IR Option CT", StringComparison.InvariantCultureIgnoreCase))
            {
                duration = 0;
            }
            else
            {

                if (instrument.GetType_API() == 'F')
                {
                    duration = instrument.GetDuration();
                }
                else
                {
                    int Tytm = instrument.TkoComputeTreeYTM(input);
                    //calcul du cas 
                    if (Tytm == 1 || Tytm == 4) { duration = instrument.GetDuration(); }//FonctionAdd.GetValuefromSophisDouble(Position, Instrument, "Duration");//IG (YTM) ou CMS ou First Call< Today

                    else if (Tytm == 2)
                    {
                        input.TmpPortfolioColName = "Duration to Call MtM";
                        duration = Helper.TkoGetValuefromSophisDouble(input); // Sub not CMS et FirstCall>Today(YTC)
                    }
                    else if (Tytm == 3)
                    {
                        input.TmpPortfolioColName = "Duration to Worst MtM";
                        duration = Helper.TkoGetValuefromSophisDouble(input);//HY (YTW)
                    }
                }
            }
            input.IndicatorValue = duration;
            return duration;

            //@SB

        }
        #endregion

        #region Duration IR

        public static double TkoComputeTKODurationIr(this CSMInstrument instrument, InputProvider input)
        {
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "input={0}, InstrumentType={1}", input.ToString(), instrument.GetInstrumentType());
            }
            int instrumentCode = instrument.GetCode();
            double durIr = 0;//duration
            try
            {

                //@SB
                if (instrument.GetInstrumentType() == 'F') //Cas futures
                {
                    sophis.finance.CSMNotionalFuture fut = instrument;
                    if (fut != null)
                    {
                        var sicovam = fut.GetCheapest();
                        SSMCalcul concordanceFactor = new SSMCalcul();
                        fut.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), sicovam, concordanceFactor);
                        CSMBond bond = CSMInstrument.GetInstance(sicovam);

                        if (concordanceFactor.fConcordanceFactor != 0)
                        {
                            const double percent = 100;
                            double factor = concordanceFactor.fConcordanceFactor / percent;
                            durIr = bond.GetModDuration(CSMMarketData.GetCurrentMarketData()) / factor;
                            input.IndicatorValue = durIr;
                            return durIr;
                        }
                        else
                        {
                            durIr = instrument.GetDuration();
                            input.IndicatorValue = durIr;
                            return durIr;
                        }
                    }
                    input.IndicatorValue = durIr;
                    return instrument.GetDuration();
                }
                //Cas Obligation
                else if (instrument.GetInstrumentType() == 'O'
                        || instrument.GetInstrumentType() == 'D')
                {
                    //Call/Worst/Maturity
                    int Tytm = 0;
                    int maturitydate = 0;
                    double yield = 0;
                    //calcul du cas 
                    Tytm = instrument.TkoComputeTreeYTM(input);

                    if (Tytm == 1 || Tytm == 4)
                    {
                        maturitydate = input.Instrument.GetExpiry();
                        yield = input.Instrument.GetYTMMtoM();
                    }
                    else if (Tytm == 2)
                    {
                        input.TmpPortfolioColName = "Date to Call MtM";
                        maturitydate = Helper.TkoGetValuefromSophisDate(input);// Sub not CMS et FirstCall>Today(YTC)
                        input.TmpPortfolioColName = "Yield to Call MtM";
                        yield = Helper.TkoGetValuefromSophisDouble(input) / 100;
                    }
                    else if (Tytm == 3)
                    {
                        input.TmpPortfolioColName = "Date to Worst MtM";
                        maturitydate = Helper.TkoGetValuefromSophisDate(input);//Hy (YTW)

                        input.TmpPortfolioColName = "Yield to Worst MtM";
                        yield = Helper.TkoGetValuefromSophisDouble(input) / 100;
                    }
                    else maturitydate = 0;

                    //Type d'instrument (Fixed / Float / FixedOrFloat / CDS)

                    //@SB
                    //TkoEnumCreditInstrumentType Instrumenttype = instrument.TkoGetCreditInstrumentType(input);
                    //@SB

                    input.Yield = yield;
                    input.WorkoutDate = maturitydate;

                    //@SB

                    //Type d'instrument (Fixed / Float / FixedOrFloat / CDS)


                    input.TmpPortfolioColName = "Allotment";
                    string allotFlag = Helper.TkoGetValuefromSophisString(input);
                    if (allotFlag.Equals("IR Futures CT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        input.TmpPortfolioColName = "PV01";
                        double pv = Helper.TkoGetValuefromSophisDouble(input);

                        input.TmpPortfolioColName = "Nominal";
                        double nominal = Helper.TkoGetValuefromSophisDouble(input);
                        //double nominal = input.NumberOfSecurities * input.Instrument.GetNotional();


                        durIr = Math.Abs(pv * (-1) * input.NumberOfSecurities * 100 / nominal);
                        input.IndicatorValue = durIr;
                        return durIr;
                    }
                    else if (allotFlag.Equals("IR Option CT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        input.IndicatorValue = 0;
                        return 0;
                    }
                    else
                    {

                        TkoEnumCreditInstrumentType Instrumenttype = instrument.TkoGetCreditInstrumentType(input);


                        switch (Instrumenttype)
                        {
                            case TkoEnumCreditInstrumentType.FIXED://Fixed
                                //durIr = calc.ComputeDurationValue(positionPtr, Instrument);
                                durIr = instrument.TkoComputeDurationIRFix(input);
                                break;
                            case TkoEnumCreditInstrumentType.FLOAT://Float pur
                                durIr = instrument.TkoComputeDurationIRFloat(input);
                                break;
                            case TkoEnumCreditInstrumentType.FIXEDTOFLOAT://FixedToFloat
                                durIr = instrument.TkoComputeDurationIRFixToFloat(input);
                                break;
                            case TkoEnumCreditInstrumentType.CDS://CDS
                                CSMSwap CDS = CSMSwap.GetInstance(instrumentCode);
                                double spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
                                double nominal = input.Instrument.GetInstrumentCount() * CSMInstrument.GetInstance(instrumentCode).GetNotional();
                                durIr = -CDS.GetRho() / (input.Position.GetAssetValue() * 1000 / nominal);
                                break;
                            case TkoEnumCreditInstrumentType.AllotmentsNulIRDuration://ALLOTMENTS-NULL-IR-DURATION
                                durIr = 0;
                                break;
                            default://Autre
                                break;
                        }


                        input.IndicatorValue = durIr;
                        return durIr;
                    }

                    //@SB

                }
                else return 0;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        #endregion

        #region Duration IR Fix

        //Duration IR Fixed
        public static double TkoComputeDurationIRFix(this CSMInstrument instrument, InputProvider input)
        {
            double Duration = 0;
            sophis.static_data.eMDayCountBasisType DayCountBasisType;
            //Définition de la liste des Flux
            System.Collections.ArrayList explicationArray = instrument.TkoGetBondExplanationArray(input);
            try
            {
                //Définition de la base de l'instrument
                if (instrument.GetInstrumentType() == 'O')
                {
                    CSMBond Bond = CSMBond.GetInstance(instrument.GetCode());
                    DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                }
                else if (instrument.GetInstrumentType() == 'S')
                {
                    CSMSwap Swap = CSMSwap.GetInstance(instrument.GetCode());
                    DayCountBasisType = Swap.GetMarketYTMDayCountBasisType();
                }
                else DayCountBasisType = instrument.GetMarketAIDayCountBasisType();


                //Variable de calcul de la Duration Cas Fixed
                //Informations à calculer
                double PresentValue = 0;//Somme des pV de tous les flux
                Duration = 0;//Duration Macaulay
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                double ActCpn = 1;
                //Variables relatives au ième flux
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthDiscountFactor;//exp(-ytm*temps du cf): actualisation au Ytm pr le calcul de la duration. Exp car duration Macaulay

                //Calcul de la duration
                for (int j = 0; j < nbCF; j++)
                {

                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                    eMFlowType FlowType = IthRedemption.flowType;
                    switch (FlowType)
                    {
                        case sophis.instrument.eMFlowType.M_ftFixed://fixed
                            if (input.WorkoutDate <= IthRedemption.endDate)
                            {
                                ActCpn = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(input.WorkoutDate, IthRedemption.endDate) / CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(IthRedemption.startDate, IthRedemption.endDate);
                            }
                            IthCoupon = IthRedemption.coupon * ActCpn;
                            break;
                        case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                            IthCoupon = IthRedemption.redemption;
                            break;
                        default:
                            IthCoupon = 0;
                            break;
                    }
                    if (input.WorkoutDate <= IthRedemption.endDate)
                    {
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(input.ReportingDate, input.WorkoutDate);
                    }
                    else
                    {
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(input.ReportingDate, IthRedemption.endDate);
                    }

                    IthDiscountFactor = Math.Exp(-input.Yield * IthDate);
                    //Calcul de la duration
                    PresentValue += IthCoupon * IthDiscountFactor;
                    Duration += IthCoupon * IthDiscountFactor * IthDate;
                    //Si on atteint la date de work out on ajoute la redemtion finale, et on sort de la boucle
                    if (input.WorkoutDate <= IthRedemption.endDate)
                    {
                        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[nbCF - 1];

                        IthCoupon = IthRedemption.redemption;//pondéré par le nombre de jours le coupon 
                        PresentValue += IthCoupon * IthDiscountFactor;
                        Duration += IthCoupon * IthDiscountFactor * IthDate;
                        j = nbCF;
                    }


                }
                if (PresentValue == 0) { Duration = 0; }//Si il n'y a aucun flux, la duration est nulle
                else { Duration = Duration / PresentValue; }

                return Duration;


            }
            catch (Exception)
            {
                CSMLog.Write("ExtensionMethods", "TkocComputeDurationIRFix", CSMLog.eMVerbosity.M_warning, "Macaulay Duration cannot be computed for Instrument " + input.InstrumentCode);
                return 0;
            }
        }
        #endregion

        #region Duration IR Float
        //Duration IR Float
        public static double TkoComputeDurationIRFloat(this CSMInstrument instrument, InputProvider input)
        {
            double Duration = 0;
            int InstrumentCode = instrument.GetCode();
            sophis.static_data.eMDayCountBasisType DayCountBasisType;
            //Définition de la liste des Flux
            System.Collections.ArrayList explicationArray = instrument.TkoGetBondExplanationArray(input);
            try
            {
                Duration = 0;
                //Définition de la base de l'instrument
                if (instrument.GetInstrumentType() == 'O')
                {
                    CSMBond Bond = CSMBond.GetInstance(instrument.GetCode());
                    DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                }
                else if (instrument.GetInstrumentType() == 'S')
                {
                    CSMSwap Swap = CSMSwap.GetInstance(instrument.GetCode());
                    DayCountBasisType = Swap.GetMarketYTMDayCountBasisType();
                }
                else DayCountBasisType = instrument.GetMarketAIDayCountBasisType();

                //Variable de calcul de la Duration Cas Float
                SSMRedemption IthRedemption;

                //Calcul de la duration taux. Temps jusqu'au prochain fixing
                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[0];

                Duration = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(input.ReportingDate, IthRedemption.endDate);
                input.IndicatorValue = Duration;

                return Duration;
            }
            catch (Exception)
            {
                CSMLog.Write("ExtensionMethods", "TkocComputeDurationIRFloat", CSMLog.eMVerbosity.M_warning, "Macaulay Duration cannot be computed for Instrument " + input.InstrumentCode);
                return 0;
            }
        }
        #endregion

        #region Duration IR FixToFloat
        //Duration IR FixToFloat
        public static double TkoComputeDurationIRFixToFloat(this CSMInstrument instrument, InputProvider input)
        {
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "input={0}, InstrumentType={1}", input.ToString(), instrument.GetInstrumentType());
            }
            double Duration = 0;
            int InstrumentCode = instrument.GetCode();
            sophis.static_data.eMDayCountBasisType DayCountBasisType;

            //Définition de la liste des Flux
            System.Collections.ArrayList explicationArray = instrument.TkoGetBondExplanationArray(input);

            try
            {
                //Définition de la base de l'instrument
                if (instrument.GetInstrumentType() == 'O')
                {
                    CSMBond Bond = CSMBond.GetInstance(instrument.GetCode());
                    DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                }
                else if (instrument.GetInstrumentType() == 'S')
                {
                    CSMSwap Swap = CSMSwap.GetInstance(instrument.GetCode());
                    DayCountBasisType = Swap.GetMarketYTMDayCountBasisType();
                }
                else DayCountBasisType = instrument.GetMarketAIDayCountBasisType();

                //Variable de calcul de la Duration Cas Fixed
                //Informations à calculer
                double PresentValue = 0;//Somme des pV de tous les flux
                Duration = 0;//Duration Macaulay
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)

                //Variables relatives au ième flux
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthDiscountFactor;//exp(-ytm*temps du cf): actualisation au Ytm pr le calcul de la duration. Exp car duration Macaulay

                //Calcul de la duration
                for (int j = 0; j < nbCF; j++)
                {
                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                    eMFlowType FlowType = IthRedemption.flowType;
                    switch (FlowType)
                    {
                        case sophis.instrument.eMFlowType.M_ftFixed://fixed
                            IthCoupon = IthRedemption.coupon;

                            IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(input.ReportingDate, IthRedemption.endDate);
                            IthDiscountFactor = Math.Exp(-instrument.GetYTMMtoM() * IthDate);
                            PresentValue += IthCoupon * IthDiscountFactor;
                            Duration += IthCoupon * IthDiscountFactor * IthDate;

                            break;
                        case sophis.instrument.eMFlowType.M_ftFloating: //float (on sort de la boucle et on ajoute la redemption)
                            if (j != 0)
                            {
                                j = nbCF;
                                IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(input.ReportingDate, IthRedemption.startDate);
                                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[nbCF - 1];
                                IthCoupon = IthRedemption.redemption;
                                IthDiscountFactor = Math.Exp(-instrument.GetYTMMtoM() * IthDate);
                                PresentValue += IthCoupon * IthDiscountFactor;
                                Duration += IthCoupon * IthDiscountFactor * IthDate;
                            }
                            break;
                        case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                            IthCoupon = IthRedemption.redemption;
                            IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(input.ReportingDate, IthRedemption.endDate);
                            IthDiscountFactor = Math.Exp(-instrument.GetYTMMtoM() * IthDate);
                            PresentValue += IthCoupon * IthDiscountFactor;
                            Duration += IthCoupon * IthDiscountFactor * IthDate;
                            break;
                        default:
                            IthCoupon = 0;
                            break;
                    }
                    //Si on atteint la date de work out on ajoute la redemtion finale, et on sort de la boucle
                    if (input.WorkoutDate == IthRedemption.endDate && j != nbCF)
                    {
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(input.ReportingDate, IthRedemption.endDate);
                        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[nbCF - 1];
                        IthDiscountFactor = Math.Exp(-input.Yield * IthDate);
                        IthCoupon = IthRedemption.redemption;
                        PresentValue += IthCoupon * IthDiscountFactor;
                        Duration += IthCoupon * IthDiscountFactor * IthDate;
                        j = nbCF;
                    }

                }
                if (PresentValue == 0) { Duration = 0; }//Si il n'y a aucun flux, la duration est nulle
                else { Duration = Duration / PresentValue; }

                input.IndicatorValue = Duration;
                return Duration;
            }
            catch (Exception)
            {
                CSMLog.Write("ExtensionMethods", "TkocComputeDurationIRFixToFloat", CSMLog.eMVerbosity.M_warning, "Macaulay Duration cannot be computed for Instrument " + input.InstrumentCode);
                return 0;
            }
        }
        #endregion
    }

}
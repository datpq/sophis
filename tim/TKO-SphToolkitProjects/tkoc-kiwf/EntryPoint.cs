using System;
using sophis;
using sophis.portfolio;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;

namespace dnPortfolioColumn
{
	/// <summary>
	/// Used to specify the entry point of the DLL and register the derived classes
	/// </summary>
	public class MainClass : sophis.IMain
	{

     	public void EntryPoint()
		{

            //Durations
            sophis.portfolio.CSMColumnConsolidate.Register("TKO IR Duration Maturity", new PC_tko_duration_taux()); //Maturity 
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Cr Duration Maturity", new PC_tko_duration_credit()); //Maturity 
            sophis.portfolio.CSMColumnConsolidate.Register("TKO IR rDuration", new PC_tko_Riskduration_taux());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Cr rDuration", new PC_tko_Riskduration_credit());

            //New Durations
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Cr Duration", new PC_DurationValue());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO IR Duration", new PC_DurationValueir());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Cr Duration Contrib", new PC_DurationValueContrib());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO IR Duration Contrib", new PC_DurationValueirContrib());

            //Carry-Ytm
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Carry@Inv_Date", new PC_CarryAtInvDate());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO YTM at Inv_Date", new PC_YtmAtInvDate());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Daily Carry Coupon", new PC_DCarryCoupon());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Daily Carry Act", new PC_DCarryAct());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO YTM Maturity", new PC_Ytm());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO YTM Test", new PC_Ytm_test());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO YTM Algo", new PC_Ytm_Algo());
           // sophis.portfolio.CSMColumnConsolidate.Register("TKO YTM", new PC_Ytm_AlgoValue());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO YTM Contrib", new PC_Ytm_AlgoValueContrib());

            //Cash Flows
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Coupon", new PC_Coupon());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Inv Cash", new PC_InvCash());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Received Coupons", new PC_ReceivedCoupons());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Received Coupons CCY", new PC_ReceivedCouponsLocalCCY());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO TMZ Accrued", new PC_TMZAccrued());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Floating Rates", new PC_FixedOrFloat());

            //Market indicators
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Internal Gearing", new PC_Internal_Gearing());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Gearing", new PC_Gearing());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Implied CDS", new PC_ImpliedCDS());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Basis", new PC_Basis());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Default Probability", new PC_DefaultProba());

            //Indicateurs
            sophis.portfolio.CSMColumnConsolidate.Register("TKO RatingComp", new PC_RatingComp());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO RatingCompLetter", new PC_RatingCompLetter());


      
            //Date Workout
            sophis.portfolio.CSMColumnConsolidate.Register("TKO Date WorkOut", new PC_DateWorkOut());

            //Reporting
            sophis.portfolio.CSMColumnConsolidate.Register("TKR Top3 Positions", new PC_Top3Positions());
            sophis.portfolio.CSMColumnConsolidate.Register("TKR Top5 Positions", new PC_Top5Positions());
            sophis.portfolio.CSMColumnConsolidate.Register("TKR Net Return Since Incep", new PC_NetReturnSinceInception());
            sophis.portfolio.CSMColumnConsolidate.Register("TKR Ann Return Since Incep", new PC_AnnualizedReturnSinceInception());
            sophis.portfolio.CSMColumnConsolidate.Register("TKR Av Monthly net Return", new PC_AvMonthNetReturn());
            sophis.portfolio.CSMColumnConsolidate.Register("TKR % of Positive Months", new PC_PositiveMonths());
            
            //Attention, cette colonne ne marche qu'avec un type d'extract très précis. Elle ne doit jamais être utilisée en lecture directe dans Sophis
            sophis.portfolio.CSMColumnConsolidate.Register("X TKR YTM (%of NAV)", new PC_XXYTMPercentNAV());

            //Credit Spreads: pret mais l faut avoir des valeurs correctes des calls dans Sophis
            //sophis.portfolio.CSMColumnConsolidate.Register("TKO OAS", new PC_OAS());
            //sophis.portfolio.CSMColumnConsolidate.Register("TKO ZSpread", new PC_ZSpread());
            sophis.portfolio.CSMColumnConsolidate.Register("TKO 1stCall StartDate", new PC_1stCallDate());

            sophis.portfolio.CSMColumnConsolidate.Register("TKOtest AYtmTRItousFlux", new PC_AYtmTRI());
            sophis.portfolio.CSMColumnConsolidate.Register("TKOtest AYtmApprox", new PC_AYtmApprox());
        }
        public void Close()
        {
            GC.Collect();
        }
    }

    public class VersionClass
    {
        private static int _RefreshVersion;//Numéro de version F8/F9
        private static int _ReportingDate;//Date courante de Sophis

        public static int Get_RefreshVersion() { return _RefreshVersion; }
        public static void Set_RefreshVersion(int Version) { _RefreshVersion = Version; }
        public static int Get_ReportingDate() { return _ReportingDate; }
        public static void Set_ReportingDate(int Date) { _ReportingDate = Date; }


        public static void DeleteCache()
        {

            ////CARRY-YTM
            //Ytm
            if (null != DataSourceYtm.DataCacheYtm) DataSourceYtm.DataCacheYtm.Clear();
            if (null != DataSourceYtm.DataCacheInstrVersion) DataSourceYtm.DataCacheInstrVersion.Clear();
            if (null != DataSourceYtm.DataCacheSpot) DataSourceYtm.DataCacheSpot.Clear();

            ////DURATIONS
            //Duration taux pour les CDS
            if (null != DataSourceDurationTaux.DataCacheDurationTauxCDS) DataSourceDurationTaux.DataCacheDurationTauxCDS.Clear();
            if (null != DataSourceDurationTaux.DataCacheInstrVersionCDS) DataSourceDurationTaux.DataCacheInstrVersionCDS.Clear();

            //Rduration taux des CDS
            if (null != DataSourceRiskDurationTaux.DataCacheRDurationTauxCDS) DataSourceRiskDurationTaux.DataCacheRDurationTauxCDS.Clear();
            if (null != DataSourceRiskDurationTaux.DataCacheInstrVersionCDS) DataSourceRiskDurationTaux.DataCacheInstrVersionCDS.Clear();

            ////CARRY-YTM
            //Carry@Inv_Date
            if (null != DataSourceCarryAtInvDate.DataCacheCarryAtInvDate) DataSourceCarryAtInvDate.DataCacheCarryAtInvDate.Clear();
            if (null != DataSourceCarryAtInvDate.DataCacheInstrVersion) DataSourceCarryAtInvDate.DataCacheInstrVersion.Clear();
            //YTM@Inv_Date
            if (null != DataSourceYtmAtInvDate.DataCacheInstrVersion) DataSourceYtmAtInvDate.DataCacheInstrVersion.Clear();
            if (null != DataSourceYtmAtInvDate.DataCacheInvYtm) DataSourceYtmAtInvDate.DataCacheInvYtm.Clear();
            //DCarryActuarial
            if (null != DataSourceDCarryActuarial.DataCacheDCarryActuarial) DataSourceDCarryActuarial.DataCacheDCarryActuarial.Clear();
            if (null != DataSourceDCarryActuarial.DataCacheInstrVersion) DataSourceDCarryActuarial.DataCacheInstrVersion.Clear();
            //DCarryCoupon
            if (null != DataSourceDCarryCoupon.DataCacheDCarryCoupon) DataSourceDCarryCoupon.DataCacheDCarryCoupon.Clear();
            if (null != DataSourceDCarryCoupon.DataCacheInstrVersion) DataSourceDCarryCoupon.DataCacheInstrVersion.Clear();
            //YTM Agregate
            if (null != DataSourceAYtmTRI.DataCacheYtmAgregate) DataSourceAYtmTRI.DataCacheYtmAgregate.Clear();
            if (null != DataSourceAYtmApprox.DataCacheAYtmApprox) DataSourceAYtmApprox.DataCacheAYtmApprox.Clear();

            //Date
            if (null != DataSourceDateWorkOut.DataCacheDateWorkOut) DataSourceDateWorkOut.DataCacheDateWorkOut.Clear();
            if (null != DataSourceDateWorkOut.DataCacheInstrVersion) DataSourceDateWorkOut.DataCacheInstrVersion.Clear();
            //Algo
            if (null != DataSourceTreeFindYT.DataCacheInstrVersion) DataSourceTreeFindYT.DataCacheInstrVersion.Clear();
            if (null != DataSourceTreeFindYT.DataCacheTreeYtm) DataSourceTreeFindYT.DataCacheTreeYtm.Clear();

            //YTM
            if (null != DataSourceTreeFindYTValue.DataCacheInstrVersion) DataSourceTreeFindYTValue.DataCacheInstrVersion.Clear();
            if (null != DataSourceTreeFindYTValue.DataCacheTreeYtmValue) DataSourceTreeFindYTValue.DataCacheTreeYtmValue.Clear();

            //YTM Contrib
            if (null != DataSourceTreeFindYTValueContrib.DataCacheInstrVersion) DataSourceTreeFindYTValueContrib.DataCacheInstrVersion.Clear();
            if (null != DataSourceDurationValueContrib.DataCacheDurationValue) DataSourceDurationValueContrib.DataCacheDurationValue.Clear();

            ////DURATIONS
            //Duration taux des Bonds
            if (null != DataSourceDurationTaux.DataCacheDurationTauxBOND) DataSourceDurationTaux.DataCacheDurationTauxBOND.Clear();
            if (null != DataSourceDurationTaux.DataCacheInstrVersionBOND) DataSourceDurationTaux.DataCacheInstrVersionBOND.Clear();
            if (null != DataSourceDurationTaux.DataCacheSpotBOND) DataSourceDurationTaux.DataCacheSpotBOND.Clear();
            //Duration credit
            if (null != DataSourceDurationCredit.DataCacheDurationCred) DataSourceDurationCredit.DataCacheDurationCred.Clear();
            if (null != DataSourceDurationCredit.DataCacheInstrVersion) DataSourceDurationCredit.DataCacheInstrVersion.Clear();
            if (null != DataSourceDurationCredit.DataCacheSpot) DataSourceDurationCredit.DataCacheSpot.Clear();
            //Rduration taux des Bonds
            if (null != DataSourceRiskDurationTaux.DataCacheRDurationTauxBOND) DataSourceRiskDurationTaux.DataCacheRDurationTauxBOND.Clear();
            if (null != DataSourceRiskDurationTaux.DataCacheInstrVersionBOND) DataSourceRiskDurationTaux.DataCacheInstrVersionBOND.Clear();
            if (null != DataSourceRiskDurationTaux.DataCacheSpotBOND) DataSourceRiskDurationTaux.DataCacheSpotBOND.Clear();
            //Rduration credit
            if (null != DataSourceRiskDurationCredit.DataCacheRDurationCred) DataSourceRiskDurationCredit.DataCacheRDurationCred.Clear();
            if (null != DataSourceRiskDurationCredit.DataCacheInstrVersion) DataSourceRiskDurationCredit.DataCacheInstrVersion.Clear();
            if (null != DataSourceRiskDurationCredit.DataCacheSpot) DataSourceRiskDurationCredit.DataCacheSpot.Clear();
            //Duration Cr
            if (null != DataSourceDurationValue.DataCacheDurationValue) DataSourceDurationValue.DataCacheDurationValue.Clear();
            if (null != DataSourceDurationValue.DataCacheInstrVersion) DataSourceDurationValue.DataCacheInstrVersion.Clear();

            //Duration Ir
            if (null != DataSourceDurationValueir.DataCacheDurationValueir) DataSourceDurationValueir.DataCacheDurationValueir.Clear();
            if (null != DataSourceDurationValueir.DataCacheInstrVersion) DataSourceDurationValueir.DataCacheInstrVersion.Clear();

            //Duration Contribution
            if (null != DataSourceDurationValueContrib.DataCacheDurationValue) DataSourceDurationValueContrib.DataCacheDurationValue.Clear();
            if (null != DataSourceDurationValueContrib.DataCacheInstrVersion) DataSourceDurationValueContrib.DataCacheInstrVersion.Clear();

            //Duration Ir Contribution


            ///CashFlows
            //Coupons
            if (null != DataSourceCoupon.DataCacheCoupon) DataSourceCoupon.DataCacheCoupon.Clear();
            if (null != DataSourceCoupon.DataCacheInstrVersion) DataSourceCoupon.DataCacheInstrVersion.Clear();
            if (null != DataSourceCoupon.DataCacheSpot) DataSourceCoupon.DataCacheSpot.Clear();

            //Inv Cash
            if (null != DataSourceInvestedCash.DataCacheInvestedCash) DataSourceInvestedCash.DataCacheInvestedCash.Clear();
            if (null != DataSourceInvestedCash.DataCacheInstrVersion) DataSourceInvestedCash.DataCacheInstrVersion.Clear();

            //Received Coupons
            if (null != DataSourceReceivedCoupons.DataCacheReceivedCoupons) DataSourceReceivedCoupons.DataCacheReceivedCoupons.Clear();
            if (null != DataSourceReceivedCoupons.DataCacheInstrVersion) DataSourceReceivedCoupons.DataCacheInstrVersion.Clear();

            //Received Coupons Local CCY
            if (null != DataSourceReceivedCouponsLocalCCY.DataCacheReceivedCouponsLocalCCY) DataSourceReceivedCouponsLocalCCY.DataCacheReceivedCouponsLocalCCY.Clear();
            if (null != DataSourceReceivedCouponsLocalCCY.DataCacheInstrVersion) DataSourceReceivedCouponsLocalCCY.DataCacheInstrVersion.Clear();

            //FixedOrFloat
            if (null != DataSourceFixedOrFloat.DataCacheFixedOrFloat) DataSourceFixedOrFloat.DataCacheFixedOrFloat.Clear();
            if (null != DataSourceFixedOrFloat.DataCacheInstrVersion) DataSourceFixedOrFloat.DataCacheInstrVersion.Clear();
            if (null != DataSourceFixedOrFloat.DataCacheSpot) DataSourceFixedOrFloat.DataCacheSpot.Clear();

            //TMZ Accrued
            if (null != DataSourceTMZAccrued.DataCacheTMZAccrued) DataSourceTMZAccrued.DataCacheTMZAccrued.Clear();
            if (null != DataSourceTMZAccrued.DataCacheInstrVersion) DataSourceTMZAccrued.DataCacheInstrVersion.Clear();


            ////////MARKETINDIC//////////////////
            
            //Gearing
            if (null != DataSourceGearing.DataCacheGearing) DataSourceGearing.DataCacheGearing.Clear();
            if (null != DataSourceGearing.DataCacheInstrVersion) DataSourceGearing.DataCacheInstrVersion.Clear();

            //Internal Gearing
            if (null != DataSourceInternalGearing.DataCacheGearing) DataSourceInternalGearing.DataCacheGearing.Clear();
            if (null != DataSourceInternalGearing.DataCacheInstrVersion) DataSourceInternalGearing.DataCacheInstrVersion.Clear();

            //ImplCDS
            if (null != DataSourceImplCDS.DataCacheImplCDS) DataSourceImplCDS.DataCacheImplCDS.Clear();
            if (null != DataSourceImplCDS.DataCacheInstrVersion) DataSourceImplCDS.DataCacheInstrVersion.Clear();
            if (null != DataSourceImplCDS.DataCacheSpot) DataSourceImplCDS.DataCacheSpot.Clear();

            //Basis
            if (null != DataSourceBasis.DataCacheBasis) DataSourceBasis.DataCacheBasis.Clear();
            if (null != DataSourceBasis.DataCacheInstrVersion) DataSourceBasis.DataCacheInstrVersion.Clear();
            if (null != DataSourceBasis.DataCacheSpot) DataSourceBasis.DataCacheSpot.Clear();

            //Defautprob
            if (null != DataSourceDefaultProb.DataCacheDefaultProb) DataSourceDefaultProb.DataCacheDefaultProb.Clear();
            if (null != DataSourceDefaultProb.DataCacheInstrVersion) DataSourceDefaultProb.DataCacheInstrVersion.Clear();
            if (null != DataSourceDefaultProb.DataCacheSpot) DataSourceDefaultProb.DataCacheSpot.Clear();

            //Rating
            if (null != DataSourceRatingComp.DataCacheInstrVersion) DataSourceRatingComp.DataCacheInstrVersion.Clear();
            if (null != DataSourceRatingComp.DataCacheRatingComp) DataSourceRatingComp.DataCacheRatingComp.Clear();
            
            //Rating Second Best 
			if (null != DataSourceRatingSecondComp.DataCacheInstrVersion) DataSourceRatingSecondComp.DataCacheInstrVersion.Clear();
			if (null != DataSourceRatingSecondComp.DataCacheRatingComp) DataSourceRatingSecondComp.DataCacheRatingComp.Clear();


            ////CREDIT SPREADS
            //OAS
            if (null != DataSourceOAS.DataCacheOAS) DataSourceOAS.DataCacheOAS.Clear();
            if (null != DataSourceOAS.DataCacheInstrVersion) DataSourceOAS.DataCacheInstrVersion.Clear();
            if (null != DataSourceOAS.DataCacheSpot) DataSourceOAS.DataCacheSpot.Clear();
            //ZSpread
            if (null != DataSourceZSpread.DataCacheZSpread) DataSourceZSpread.DataCacheZSpread.Clear();
            if (null != DataSourceZSpread.DataCacheInstrVersion) DataSourceZSpread.DataCacheInstrVersion.Clear();
            if (null != DataSourceZSpread.DataCacheSpot) DataSourceZSpread.DataCacheSpot.Clear();
            //1st Call Date
            if (null != DataSource1stCallDate.DataCache1stCallDate) DataSource1stCallDate.DataCache1stCallDate.Clear();
            if (null != DataSource1stCallDate.DataCacheInstrVersion) DataSource1stCallDate.DataCacheInstrVersion.Clear();


        }


        /// <summary>
        /// Vide les caches des colonnes dont la valeur ne dépend que de l'instrument
        /// </summary>
        public static void ClearInstrumentCaches()
        {
            /*
                 
           ////CARRY-YTM
           //Ytm
           DataSourceYtm.DataCacheYtm.Clear();
           DataSourceYtm.DataCacheInstrVersion.Clear();
           DataSourceYtm.DataCacheSpot.Clear();

           ////DURATIONS
           //Duration taux pour les CDS
           DataSourceDurationTaux.DataCacheDurationTauxCDS.Clear();
           DataSourceDurationTaux.DataCacheInstrVersionCDS.Clear();

           //Rduration taux des CDS
           DataSourceRiskDurationTaux.DataCacheRDurationTauxCDS.Clear();
           DataSourceRiskDurationTaux.DataCacheInstrVersionCDS.Clear();

           ////CARRY-YTM
           //Carry@Inv_Date
           DataSourceCarryAtInvDate.DataCacheCarryAtInvDate.Clear();
           DataSourceCarryAtInvDate.DataCacheInstrVersion.Clear();
           //YTM@Inv_Date
           DataSourceYtmAtInvDate.DataCacheInstrVersion.Clear();
           DataSourceYtmAtInvDate.DataCacheInvYtm.Clear();
           //DCarryActuarial
           DataSourceDCarryActuarial.DataCacheDCarryActuarial.Clear();
           DataSourceDCarryActuarial.DataCacheInstrVersion.Clear();
           //DCarryCoupon
           DataSourceDCarryCoupon.DataCacheDCarryCoupon.Clear();
           DataSourceDCarryCoupon.DataCacheInstrVersion.Clear();
           //YTM Agregate
           DataSourceAYtmTRI.DataCacheYtmAgregate.Clear();
           DataSourceAYtmApprox.DataCacheAYtmApprox.Clear();

           //Date
           DataSourceDateWorkOut.DataCacheDateWorkOut.Clear();
           DataSourceDateWorkOut.DataCacheInstrVersion.Clear();
            
           */

       }

     


    }
   
}

#pragma warning(disable:4251)
#include "CFG_ConstantAmortizedBond.h"
#include "CFG_BondMarketData.h"
#include "Resource/resource.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/finance/SphABSBond.h"
#include "SphInc/static_data/SphYieldCurveFamily.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/static_data/SphYieldCalculation.h"
#include "SphInc/scenario/SphMarketDataOverloader.h"
//DPH
//#include "SphInc/inflation/SphInflationCurve.h"
//DPH
#include "SphLLInc/finance_v2/SSModel.h"
#include "CFG_Maths.h"
#include __STL_INCLUDE_PATH(vector)

using _STL::vector;

#ifndef	__MIN
#define __MIN(a,b) (((a)>(b)) ? (b) : (a))
#endif

inline bool is_relative_or_null_date(long date)
{
	return date < 2000;
};

//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_BOND(CFG_ConstantAmortizedBond, CSRBond)

//DPH
const sophis::finance::CSRPricer * CFG_ConstantAmortizedBond::GetDefaultPricer() const {
	sophis::finance::CSRPricer* pricer = const_cast<sophis::finance::CSRPricer*>(sophis::finance::CSRPricer::GetPrototype().GetData("CFG_ConstantAmortizedBond_MetaModel"));
	return pricer;
}

//DPH
///*virtual*/ double	CFG_ConstantAmortizedBond::ComputeNonFlatCurvePrice(	long	transactionDate,
/*virtual*/ double	CFG_ConstantAmortizedBond::EmcComputeNonFlatCurvePrice(long	transactionDate,
											 long	settlementDate,		
											 long	ownershipDate,
											 const market_data::CSRMarketData		&param,
											 double									*derivee,
											 const static_data::CSRDayCountBasis		*base,
											 short									adjustedDates,
											 short									valueDates,
											 const static_data::CSRDayCountBasis		*dayCountBasis,
											 const static_data::CSRYieldCalculation	*yieldCalculation,
											 _STL::vector<SSRedemption>				*redemptionArray,
											 _STL::vector<SSBondExplication>			*explicationArray,
											 bool									withSpreadMgt,
											 bool									throwException) const
{		
	const CSRDayCountBasis * TKT_dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis("Act/Act CFG Coupon");
	double CFGRiskSpread = 0.;
	LoadGeneralElement(IDC_RISK_SPREAD-ID_ITEM_SHIFT,&CFGRiskSpread);
	CFGRiskSpread *= 0.01;

	const CSRBond *bondPtr = dynamic_cast<const CSRBond*>(this);

	//DPH
	//_STL::auto_ptr<sophis::instrument::CSRMarketCategoryHandle> marketCategoryHandle;
	std::unique_ptr<sophis::instrument::CSRMarketCategoryHandle> marketCategoryHandle;
	if (bondPtr) 
		bondPtr->ActivateMarketCategory(marketCategoryHandle);

	int 						i;
	const CSRYieldCurve			*ZeroCoupon;
	double						tx,signature;
	double						coef;
	double						overRate;
	double						nt_titres, tranche;
	long						datefinbase;
	CSRInterestRate				*taux = 0;
	const	CSRCurrency			*devisePtr;
	bool						enLivraison;
	double						facteur, fact2;
	long						jusquaDate;
	bool oldFRN = IsFloatingRateOldWay();
	const CSRDayCountBasis		*rateBasis = 0, *fixedRateBasis = 0;

	tranche = 1.0;

	if (!redemptionArray && explicationArray)
		explicationArray = 0;

	SSRedemption				redemption,localFlow;
	SSBondExplication			cashFlowExplication;

	if (withSpreadMgt)
		InitDefaultValue(adjustedDates, valueDates, &dayCountBasis, &yieldCalculation);
	fixedRateBasis = CSRDayCountBasis::GetCSRDayCountBasis( GetMarketCSDayCountBasisType() );

	CSRMarketData*			newContext		= 0;
	const CSRMarketData*	contextCoupon	= &param;
	double		dirtyPrice=0;
	double fixedNominal = 0;
	int index_start=0;
	int index;

	long currency = this->GetFamily() ? this->GetFamily() : this->GetCurrency();

	long family						= currency;

	devisePtr						= CSRCurrency::GetCSRCurrency(currency);
	if (!devisePtr)
	{
		currency	=  GetCurrency();
		devisePtr	= CSRCurrency::GetCSRCurrency(currency);
	}

	long taux_var					= GetFloatingRate();
	long today_0					= gApplicationContext->GetDate();
	int	 redemptionCount			= GetRedemptionCount();
	int	 nb							= redemptionCount;
	double nominal					= GetNotionalInProduct();
	const CSRCalendar* calendarRate = 0;
		
	CSRFixingDateComputation* fixingComp= 0;
	SSCouponCalculation ccData = GetRules();
	SSDayCountCalculation dccData = GetDayCountCalculation();	
	SSDayCountCalculation dccDataForBrokenFlows = dccData;
	dccDataForBrokenFlows.fBackWards = false;
	dccDataForBrokenFlows.fCashFlowType = SSDayCountCalculation::eLast;

	fixingComp = new_FixingDateComputation(0);		

	const CSRMarketData*	contextUsed		= newContext ? newContext : &param;		

	eCapitalizedType capitalizedType  = bondPtr->GetCapitalization();
	bool capitalize = capitalizedType != ctNone;
	_STL::vector<int> capitalisedCoupon;
	_STL::vector<size_t> capitalisedCouponExplanation;

	double percent, minimum, maximum;

	if(!ownershipDate)
		ownershipDate = settlementDate;

	if(ownershipDate<transactionDate)
		ownershipDate = transactionDate;

	SScoupon_amount	st;
	long sico = GetCode();
	//if(!param.GetUserParameter(spAmountCoupon, &sico, &st))
	{
		st.start_date	= ownershipDate;
		st.end_date		= kInfiniteDate;
		st.discount		= true;
	}

	double remainingNotional = nominal; // for floating sinkable bond
	long expiryDate = GetRevisedExpiry(transactionDate);

	if(devisePtr && transactionDate)
	{
		overRate = contextUsed->GetOverRate(currency);
		ZeroCoupon=contextUsed->GetCSRYieldCurve(family);
		if(ZeroCoupon)
		{
			bool inAvanceMethodPayment = ccData.fPaymentType != pmInArrears && taux_var!=0 && !oldFRN;

			taux = (CSRInterestRate*)dynamic_cast<const CSRInterestRate*>(CSRInstrument::GetInstance(taux_var));

			if (taux)
			{
				//DPH
				//calendarRate = taux->NewCSRCalendar();
				calendarRate = taux->GetCSRCalendar(eictDefault);
				rateBasis	 = CSRDayCountBasis::GetCSRDayCountBasis(taux->GetDayCountBasisType());
			}

			if(transactionDate<today_0)
				datefinbase=transactionDate;
			else
				datefinbase=today_0;

			signature=overRate;

			index_start = 0;
			coef=1;

			enLivraison = GetBondOwnershipType()==ioAtDelivery;

			long maturity = 0;

			for ( ; nb>1; index_start++,nb--) // remove the first coupons that are before st.start_date=ownership date
			{
				if (GetNthRedemption(index_start,&localFlow))
				{
					if (capitalize && localFlow.flowType == ftFloating && localFlow.maturityDate > maturity)
					{
						SSRedemption endFlow;
						for (int indexLoc=0;indexLoc<nb;++indexLoc)
						{
							if (!GetNthRedemption(index_start+indexLoc, &endFlow) || endFlow.flowType != ftFloating)
								continue;
							if (endFlow.paymentDate != localFlow.paymentDate)
								break;
							maturity = endFlow.maturityDate;
						}
					}
					else 
						maturity = localFlow.maturityDate;

					long minPayDate = is_relative_or_null_date(localFlow.paymentDate) ? sophisTools::kInfiniteDate : localFlow.paymentDate;
					if(__MIN(maturity,minPayDate)>st.start_date && (!inAvanceMethodPayment || localFlow.flowType!=ftFloating)
						|| __MIN(localFlow.startDate,minPayDate)>st.start_date && localFlow.endDate)
						break;

					if (localFlow.redemption && localFlow.securityCount == 0)
					{
						remainingNotional -= localFlow.redemption;
					}
				}
			}
			for(index=index_start,i=nb,nt_titres=0;--i>=0;index++)
			{
				if(GetNthRedemption(index,&localFlow))
				{
					if(localFlow.securityCount>localFlow.convertedSecurityCount)
						nt_titres+=localFlow.securityCount-localFlow.convertedSecurityCount;
				}
			}
			jusquaDate = 0;
			facteur = 1;

			SSCapitalizedCouponCalculation couponDataLoc(*taux);
			if (capitalize)
			{
				couponDataLoc.fCapitalizationType	= capitalizedType;	
				couponDataLoc.fPaymentCurrency		= GetCurrency();
			}
			CSRCapitalizedCouponCalculation capitalizedCouponCalculation(couponDataLoc,fixingComp,CSRCapitalizedCouponCalculationParameters(this));
			_STL::vector<double> coefVector;

			GetNthRedemption(index_start, &localFlow);
			SSFlow localSSFlow;

			for(index=index_start,i=nb;--i>=0;index++)
			{
				memset(&cashFlowExplication,0,sizeof(SSBondExplication));
				memset(&redemption,0,sizeof(SSRedemption));
				dccData.SetCashFlowType(index , redemptionCount);
				double ytmExplication	  = 1.;

				if (!GetNthRedemption(index,&localFlow))
					continue;

				if(localFlow.maturityDate > st.end_date || localFlow.maturityDate > expiryDate)
					break;

				long minPayDate = is_relative_or_null_date(localFlow.paymentDate) ? sophisTools::kInfiniteDate : localFlow.paymentDate;

				tx = __MIN(localFlow.maturityDate,minPayDate)>st.start_date && (oldFRN || localFlow.flowType!=ftFloating) 
					? localFlow.coupon 
					: 0;

				if(capitalize && localFlow.flowType == ftFloating && capitalisedCoupon.empty()) 
				{
					capitalisedCoupon.clear();
					capitalisedCoupon.reserve( redemptionCount -index);
					capitalisedCoupon.push_back(index);

					for(int j=index+1; j<redemptionCount;j++)
					{
						SSRedemption otherFlow;
						if(GetNthRedemption(j,&otherFlow) && otherFlow.flowType == ftFloating)								
						{
							long minPayDateLoc = is_relative_or_null_date(otherFlow.paymentDate) ? sophisTools::kInfiniteDate : otherFlow.paymentDate;
							if(__MIN(otherFlow.endDate,minPayDateLoc)>localFlow.paymentDate)	
								break;

							capitalisedCoupon.push_back(j);
						}
					}
				}

				double	notionalFactor				 = 1.;
				double	notionalFactorForRedemption	 = 1.;					

				localSSFlow.fStartDate = localFlow.startDate;
				localSSFlow.fEndDate = localFlow.endDate;
				localSSFlow.fSettlementDate = localFlow.maturityDate;
				double accrualratio = 1.0;

				if (redemptionArray && tx && nominal && (oldFRN || localFlow.flowType!=ftFloating))
				{
					redemption.securityCount			= 0;
					redemption.convertedSecurityCount	= 0;
					redemption.redemption				= 0.;
					redemption.percentage				= localFlow.percentage;
					redemption.instrumentCode			= 0;
					redemption.coupon					= tx/nominal;
					redemption.flowType					= ftFixed;
					redemption.startDate				= localFlow.startDate;
					redemption.endDate					= localFlow.endDate;
					redemption.maturityDate				= localFlow.paymentDate;
					redemption.nonAdjustedMaturityDate	= localFlow.maturityDate;						
					redemption.poolFactor				= localFlow.poolFactor;
					redemption.absFlow					= localFlow.absFlow;
					redemption.isBrokenFlow				= localFlow.isBrokenFlow;

					(*redemptionArray).push_back(redemption);

					if (explicationArray)
					{
						cashFlowExplication					 = redemption;
						cashFlowExplication.instrumentCode	 = index;
						cashFlowExplication.floatingRate	 = 0.;
						cashFlowExplication.presentValue	 = redemption.coupon* nominal*notionalFactor * accrualratio * coef;
						cashFlowExplication.netCoupon		 = redemption.coupon* nominal*notionalFactor * accrualratio;
						cashFlowExplication.coupon			 = cashFlowExplication.netCoupon;
						cashFlowExplication.inflationRate	 = notionalFactor-1.;
						cashFlowExplication.dayCount		 = fixedRateBasis ? fixedRateBasis->GetEquivalentDayCount(localFlow.startDate,localFlow.endDate, dccData)
							: localFlow.endDate-localFlow.startDate;
						cashFlowExplication.rateForwardStartDate = 0;
						cashFlowExplication.rateForwardEndDate	 = 0;
						cashFlowExplication.pikFactor			= 1.;
						cashFlowExplication.accrual_ratio		= accrualratio;
						explicationArray->push_back(cashFlowExplication);
					}
				}
				if(taux && localFlow.flowType==ftFloating)
				{
					double unitCoupon = 0.;
					double margin = 0. ;
					CSRCashFlowInformation rateExplanation;

					static_data::eDayCountBasisType couponBasisType = ccData.fDayCountBasisTypeOverloaded != dcbUndefined ? ccData.fDayCountBasisTypeOverloaded : taux->GetDayCountBasisType();
					const CSRDayCountBasis		*basisLoc	= CSRDayCountBasis::GetCSRDayCountBasis(couponBasisType);
					const CSRYieldCalculation	*modeLoc	= CSRYieldCalculation::GetCSRYieldCalculation(taux->GetYieldCalculationType());
					double period = basisLoc->GetEquivalentYearCount(localFlow.startDate,localFlow.endDate, dccData);

					double amount=0.;

					if(capitalize)
					{
						SSCouponCalculationForCapitalizing ccDataForCapitalizing;
						ccDataForCapitalizing.SSCouponCalculation::operator=(ccData);
						ccDataForCapitalizing = localFlow;

						ccDataForCapitalizing.fNominal *= coef*remainingNotional*notionalFactor;

						capitalizedCouponCalculation.AddFlowToCapitalize(ccDataForCapitalizing);
						coefVector.push_back(coef);
						if (explicationArray)
							capitalisedCouponExplanation.push_back(explicationArray->size());
						percent = localFlow.percentage*.01;
						margin = ccDataForCapitalizing.fMargin;
					}
					else
					{							
						//coupon parameters

						ccData.fFixingDate = localFlow.fixingDate;
						ccData.fDccData		= &dccData;
						ccData.fPaymentDate = localFlow.paymentDate;
						if (taux && !localFlow.fixingDate)
							taux->GetFixingDate(localFlow.startDate, localFlow.endDate, ccData.fFixingDate, fixingComp);
						else
							ccData.fFixingDate = localFlow.fixingDate;

						GetFloatingRateComponent(localFlow,ccData,minimum, maximum, percent);
						margin = ccData.fMargin;

						unitCoupon	= taux->GetUniversalCouponRate(	 *contextCoupon,
							localFlow.startDate,
							localFlow.endDate,
							ccData,
							&rateExplanation,
							true,
							false, // No quanto on bond
							GetCurrency(),
							minimum,
							maximum);

						amount = percent*remainingNotional*unitCoupon;
					}

					tx+=amount;

					//Asset Swaps: Fill redemptionArray (floating coupon)
					if (redemptionArray)
					{
						redemption.securityCount		  = 0;
						redemption.convertedSecurityCount = 0;
						redemption.redemption			  = 0.;
						redemption.percentage			  = 100.*percent;
						redemption.instrumentCode		  = 0;
						redemption.coupon				  = margin;
						redemption.flowType				  = ftFloating;
						redemption.variable_rate		  = taux_var;
						redemption.startDate			  = localFlow.startDate;
						redemption.endDate				  = localFlow.endDate;
						redemption.paymentDate			  = localFlow.paymentDate;
						redemption.maturityDate			  = localFlow.paymentDate;
						redemption.nonAdjustedMaturityDate = localFlow.maturityDate;
						redemption.poolFactor				= localFlow.poolFactor;
						redemption.absFlow					= localFlow.absFlow;
						redemption.isBrokenFlow			  = localFlow.isBrokenFlow;

						(*redemptionArray).push_back(redemption);
					}
					if (explicationArray)
					{
						cashFlowExplication					 = redemption;
						cashFlowExplication.instrumentCode	 = index; 
						cashFlowExplication.flowType		= rateExplanation.cash_flow_type == cfFixed ? ftFixed : ftFloating;	// fixing is done

						cashFlowExplication.presentValue	 = amount*notionalFactor*accrualratio*coef;
						cashFlowExplication.netCoupon		 = amount*notionalFactor*accrualratio;
						cashFlowExplication.coupon			 = cashFlowExplication.netCoupon;
						cashFlowExplication.inflationRate	 = notionalFactor-1.;
						cashFlowExplication.dayCount		 = rateBasis? rateBasis->GetEquivalentDayCount(localFlow.startDate,localFlow.endDate,dccData)
							: localFlow.endDate-localFlow.startDate;

						cashFlowExplication.floatingRate = modeLoc->GetRate(unitCoupon,period);
						cashFlowExplication.fixingWeight1	 = rateExplanation.fFixingWeight1;
						cashFlowExplication.fixingWeight2	 = rateExplanation.fFixingWeight2;
						cashFlowExplication.rateForwardStartDate = rateExplanation.start_date_floating_rate;
						cashFlowExplication.rateForwardEndDate	 = rateExplanation.end_date_floating_rate;
						cashFlowExplication.pikFactor		 = 1.;
						cashFlowExplication.accrual_ratio	 = accrualratio;

						explicationArray->push_back(cashFlowExplication);
					}
				}
				if(jusquaDate<localFlow.paymentDate)
					facteur = 1 - contextUsed->GetCouponTaxes(currency, GetMarketCode(), localFlow.maturityDate, &jusquaDate);
				tx *= facteur;
				fact2 = 1.;
				if(base)
				{
					fact2 *= base->GetEquivalentYearCount(settlementDate,localFlow.paymentDate, dccDataForBrokenFlows);
				}

				if(st.discount)
				{
					fact2 /=ZeroCoupon->GetForwardCompoundFactor(transactionDate,localFlow.maturityDate,signature+CFGRiskSpread);
}
				//added to round zc = fact2
				//double period = basisLoc->GetEquivalentYearCount(localFlow.startDate,localFlow.endDate, dccData);
				//const CSRDayCountBasis * TKT_dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis("Act/Act CFG Coupon");
				//double yearFraction = TKT_dayCountBasis->GetEquivalentYearCount(transactionDate,localFlow.endDate, dccData);
				
				
				const CSRDayCountBasis * basisLoc = CSRDayCountBasis::GetCSRDayCountBasis(dcb_Actual_Actual_AFB);
				double yearFr1 = basisLoc->GetEquivalentYearCount(transactionDate,localFlow.endDate, dccData);

				//double yearFraction = localFlow.endDate - localFlow.startDate;
				double a = 1/fact2;
				double b = 1/yearFr1;
				double zc = pow(a,b);
				zc = zc - 1.;
				
				//if (st.discount)
					//zc += (CFGRiskSpread * 0.00001);

				//double zc = pow((1/fact2),(1/period)) - 1;
				zc = CFG_Maths::Round(zc,5);
				double yearFr2 = TKT_dayCountBasis->GetEquivalentYearCount(transactionDate,localFlow.endDate, dccData);

				fact2 = 1/(pow(zc + 1, yearFr2));

				dirtyPrice += tx*fact2*coef*notionalFactor*accrualratio;

				if(capitalize && localFlow.flowType == ftFloating && capitalisedCoupon.back()==index)
				{						
					CSRCashFlowInformationList listeLocale;
					double capitalizedCoupon = capitalizedCouponCalculation.ComputeCapitalizedCouponRate(*contextCoupon,dccData,&listeLocale);
					capitalizedCoupon *= facteur*fact2;

					dirtyPrice += capitalizedCoupon;

					if(explicationArray)
					{
						CSRCashFlowInformationList::iterator iteLocalList = listeLocale.begin();
						_STL::vector<double>::iterator iteCoef = coefVector.begin();

						for(size_t localIndex = 0; localIndex<capitalisedCouponExplanation.size(); localIndex++)
						{
							SSBondExplication& localCashFlowExplication= (*explicationArray)[capitalisedCouponExplanation[localIndex]];

							localCashFlowExplication.instrumentCode	= -1;// in order not to multiply by the tax factor
							localCashFlowExplication.coupon			= iteLocalList->amount / *iteCoef *.0001;
							localCashFlowExplication.netCoupon		= localCashFlowExplication.coupon*facteur;
							localCashFlowExplication.presentValue	= localCashFlowExplication.coupon*facteur;

							localCashFlowExplication.floatingRate	= iteLocalList->variable_rate *.0001;
							localCashFlowExplication.fixingWeight1	= iteLocalList->fFixingWeight1;
							localCashFlowExplication.fixingWeight2	= iteLocalList->fFixingWeight2;
							localCashFlowExplication.YTM			= fact2;
							localCashFlowExplication.rateForwardStartDate= iteLocalList->start_date_floating_rate;
							localCashFlowExplication.rateForwardEndDate= iteLocalList->end_date_floating_rate;

							++iteLocalList;
							++iteCoef;
						}
					}
					coefVector.clear();

					capitalisedCoupon.clear();
					capitalisedCouponExplanation.clear();
					capitalizedCouponCalculation.ResetFlowList();
				}

				if (explicationArray)
				{
					ytmExplication = fact2;

					for (size_t j=0 ; j<explicationArray->size() ; j++)
					{
						if((*explicationArray)[j].instrumentCode==index && (*explicationArray)[j].flowType!=ftRedemption)
						{
							(*explicationArray)[j].YTM			 = ytmExplication;
							(*explicationArray)[j].netCoupon	*= facteur;
						}
					}
				}

				double factRedemption	= fact2;
				if(localFlow.redemption)
				{						
					fixedNominal += localFlow.redemption;

					bool amortized = (nt_titres == 0 || localFlow.securityCount == 0);
					if(amortized)
					{
						remainingNotional -= localFlow.redemption;
						dirtyPrice += localFlow.redemption*factRedemption*notionalFactorForRedemption;
					}
					else
					{
						tranche = (localFlow.securityCount-localFlow.convertedSecurityCount)/nt_titres;
						dirtyPrice += localFlow.redemption*factRedemption*tranche*notionalFactorForRedemption;
						coef-=tranche;
					}

					//Fill redemptionArray (redemption)
					if (redemptionArray)
					{
						redemption.securityCount			= localFlow.securityCount;
						redemption.convertedSecurityCount	= localFlow.convertedSecurityCount;
						redemption.redemption				= localFlow.redemption*notionalFactorForRedemption;
						redemption.percentage				= 100.;
						redemption.instrumentCode			= 0;
						redemption.coupon					= 0.;
						cashFlowExplication.netCoupon		= 0.;
						redemption.flowType					= ftRedemption;
						if (taux && !oldFRN && i==0)
						{
							redemption.startDate				= localFlow.startDate;
							redemption.endDate					= localFlow.startDate;
						}
						else
						{
							redemption.startDate				= localFlow.maturityDate;
							redemption.endDate					= localFlow.maturityDate;
						}

						redemption.maturityDate				= localFlow.paymentDate;
						redemption.nonAdjustedMaturityDate	= localFlow.maturityDate;
						redemption.isBrokenFlow				= localFlow.isBrokenFlow;																					
						(*redemptionArray).push_back(redemption);

						if (explicationArray)
						{
							cashFlowExplication.netCoupon		= 0.;
							cashFlowExplication					 = redemption;
							cashFlowExplication.instrumentCode	 = index;
							cashFlowExplication.floatingRate	 = 0.;
							cashFlowExplication.presentValue	 = amortized ? redemption.redemption : redemption.redemption*tranche;
							cashFlowExplication.rateForwardStartDate = 0;
							cashFlowExplication.rateForwardEndDate	 = 0;

							cashFlowExplication.YTM				 = factRedemption;
							cashFlowExplication.inflationRate	 = notionalFactorForRedemption-1.;

							cashFlowExplication.dayCount		 = 0;

							cashFlowExplication.fixingWeight1	 = 0;
							cashFlowExplication.fixingWeight2	 = 0;
							cashFlowExplication.pikFactor		 = 1.;
							cashFlowExplication.accrual_ratio	 = 1.;

							explicationArray->push_back(cashFlowExplication);
						}
					}
				}
			}				
			
			if(IsRevisable() && remainingNotional > 0) // full redemption of the rest on the revised expiry date
			{
				double cf = ZeroCoupon->GetForwardCompoundFactor(transactionDate, expiryDate, overRate + CFGRiskSpread);
				double timeFactor = 1.0 / cf;

				if(base)
					timeFactor *= base->GetEquivalentYearCount(transactionDate,expiryDate, dccDataForBrokenFlows);

				//adjust timeFactor to match the rounding of ZC rate
				const CSRDayCountBasis * basisLoc = CSRDayCountBasis::GetCSRDayCountBasis(dcb_Actual_Actual_AFB);
				double yearFr1 = basisLoc->GetEquivalentYearCount(transactionDate,expiryDate, dccDataForBrokenFlows);
				double a = 1/timeFactor;
				double b = 1/yearFr1;
				double zc = pow(a,b);
				zc = zc - 1.;
				zc = CFG_Maths::Round(zc,5);
				double yearFr2 = TKT_dayCountBasis->GetEquivalentYearCount(transactionDate,expiryDate, dccDataForBrokenFlows);
				timeFactor = 1/(pow(zc + 1, yearFr2));

				dirtyPrice += remainingNotional * timeFactor;

				if (redemptionArray)
				{
					redemption.securityCount			= localFlow.securityCount;
					redemption.convertedSecurityCount	= localFlow.convertedSecurityCount;
					redemption.redemption				= remainingNotional;
					redemption.percentage				= 100.0;
					redemption.instrumentCode			= 0;
					redemption.coupon					= 0.0;
					redemption.flowType					= ftRedemption;
					redemption.startDate				= expiryDate;
					redemption.endDate					= expiryDate;
					redemption.maturityDate				= expiryDate;
					redemption.nonAdjustedMaturityDate	= expiryDate;
					redemption.isBrokenFlow				= localFlow.isBrokenFlow;

					(*redemptionArray).push_back(redemption);
				}
				if (explicationArray)
				{
					cashFlowExplication							= redemption;
					cashFlowExplication.netCoupon				= 0.0;
					cashFlowExplication.instrumentCode			= 0L;
					cashFlowExplication.floatingRate			= 0.0;
					cashFlowExplication.presentValue			= redemption.redemption;
					cashFlowExplication.rateForwardStartDate	= 0;
					cashFlowExplication.rateForwardEndDate		= 0;
					cashFlowExplication.YTM						= timeFactor;
					cashFlowExplication.inflationRate			= 0.0;
					cashFlowExplication.dayCount				= 0;
					cashFlowExplication.fixingWeight1			= 0;
					cashFlowExplication.fixingWeight2			= 0;

					explicationArray->push_back(cashFlowExplication);
				}
			}

			double MtMSpread = 0;
			MtMSpread = param.GetUserParameter(spMTMZCSpread, &sico, &MtMSpread) ? MtMSpread : 0.;
			
			double compoundFactor;
			const CSRYieldCurveStaticSpread *ycSS = dynamic_cast<const CSRYieldCurveStaticSpread*>(ZeroCoupon);
			if (ycSS)
			{
				CSRYieldCurveStaticSpread *ycSSNonConst = const_cast<CSRYieldCurveStaticSpread*>(ycSS);
				double frozenRateBackUp = ycSSNonConst->GetFrozenRate();
				ycSSNonConst->SetFrozenRate(-NOTDEFINED);
				compoundFactor	 = ycSSNonConst->GetForwardCompoundFactor(transactionDate,settlementDate,overRate-MtMSpread+CFGRiskSpread);
				ycSSNonConst->SetFrozenRate(frozenRateBackUp);
			}
			else
			{					
				double MtMSpread = 0;
				MtMSpread = param.GetUserParameter(spMTMZCSpread, &sico, &MtMSpread) ? MtMSpread : 0.;
				compoundFactor	 = ZeroCoupon->GetForwardCompoundFactor(transactionDate,settlementDate,overRate-MtMSpread+CFGRiskSpread);
			}

			dirtyPrice *= compoundFactor;

			if (explicationArray)
			{
				int nb = SIZE_T_TO_LONG(explicationArray->size());
				const CSRDayCountBasis* baseLoc		= CSRDayCountBasis::GetCSRDayCountBasis(dcb_Actual_Actual_AFB);
				const CSRYieldCalculation* calcul	= CSRYieldCalculation::GetCSRYieldCalculation(ycActuarial);

				for (int i=0 ; i<nb ; i++)
				{						
					(*explicationArray)[i].YTM *= compoundFactor;
					CFG_Maths::Round((*explicationArray)[i].YTM,3+2);
					(*explicationArray)[i].presentValue *= (*explicationArray)[i].YTM;

					long maturityCashFlow = (*explicationArray)[i].nonAdjustedMaturityDate;

					if(maturityCashFlow>settlementDate && fabs((*explicationArray)[i].YTM)>1e-8)
					{
						double dtLoc = TKT_dayCountBasis->GetEquivalentYearCount(settlementDate, maturityCashFlow, dccDataForBrokenFlows);
						(*explicationArray)[i].YTM = dtLoc>1e-8?CFG_Maths::Round(calcul->GetRate(1/(*explicationArray)[i].YTM-1, dtLoc),3+2):0.;
					}
					else
						(*explicationArray)[i].YTM = 0.;
				}
			}
		}
	}

	if (explicationArray)
	{
		long code = this->GetCode();

		for (unsigned int i=0 ; i<explicationArray->size() ; i++)
			(*explicationArray)[i].instrumentCode = code;
	}

	if (newContext)
	{
		delete newContext;
		newContext = 0;
	}	
	if (fixingComp)
	{
		delete fixingComp;
		fixingComp = 0;
	}

	return dirtyPrice;	
};

double CFG_ConstantAmortizedBond::GetDirtyPriceByYTM(	long 						transactionDate, 
												long 						settlementDate, 
												long						ownershipDate,
												double 						yieldToMaturity,
												short						adjustedDates,
												short						valueDates,
												const CSRDayCountBasis*		dayCountBasis, 
												const CSRYieldCalculation*	yieldCalculation,
												const CSRMarketData&		context) const
{
	if(!transactionDate)
		return 0.0;

	InitDefaultValue(adjustedDates, valueDates, &dayCountBasis, &yieldCalculation);

	//DPH
	//tools::CSRAssignement<const SSModelMarkt*> toto(CSRMarketData::GetModelMarket(), CSRMarketData::GetModelMarket(*this));
	const SSModelMarkt* dest = const_cast<const SSModelMarkt*>(&(CSRMarketData::GetModelMarket()));
	const SSModelMarkt* src = const_cast<const SSModelMarkt*>(&(CSRMarketData::GetCurrentMarketData()->GetModelMarket(*this)));
	tools::CSRAssignement<const SSModelMarkt*> toto(dest, src);

	if(settlementDate == 0)
		settlementDate = GetSettlementDate(transactionDate);
	if(ownershipDate == 0)
		ownershipDate = GetPariPassuDate(transactionDate, settlementDate);

	CSRBond* pRevisedClone = GetRevisedClone(transactionDate);
	if(!pRevisedClone)
		return 0.0;

	double val = pRevisedClone->ComputePriceByYTM(	
		transactionDate,
		settlementDate,
		ownershipDate,
		yieldToMaturity,
		0,
		false,
		context,
		adjustedDates? true : false,
		valueDates? true : false,
		dayCountBasis,
		yieldCalculation,
		NULL);

	delete pRevisedClone;

	if (GetAskQuotationType() == aqInPrice || GetAskQuotationType() == aqInPriceWithoutAccrued)
		return val;
	else
		return GetNotionalInProduct()? val / GetNotionalInProduct() * 100.0 : 0.0;
};

double CFG_ConstantAmortizedBond::GetYTMByDirtyPrice(	long 						transactionDate, 
												long 						settlementDate, 
												long						ownershipDate,
												double 						dirtyPrice,
												short						adjustedDates,
												short						valueDates,
												const CSRDayCountBasis*		dayCountBasis, 
												const CSRYieldCalculation*	yieldCalculation,
												const CSRMarketData&		context,
												//DPH
												double						startPoint) const
{
	if(!transactionDate)
		return 0.0;

	InitDefaultValue(adjustedDates, valueDates, &dayCountBasis, &yieldCalculation);

	//DPH
	//tools::CSRAssignement<const SSModelMarkt*> toto(CSRMarketData::GetModelMarket(), CSRMarketData::GetModelMarket(*this));
	const SSModelMarkt* dest = const_cast<const SSModelMarkt*>(&(CSRMarketData::GetModelMarket()));
	const SSModelMarkt* src = const_cast<const SSModelMarkt*>(&(CSRMarketData::GetCurrentMarketData()->GetModelMarket(*this)));
	tools::CSRAssignement<const SSModelMarkt*> toto(dest, src);

	if(FairValueInPercent(GetAskQuotationType()))
		dirtyPrice *= GetNotionalInProduct() / 100.0;

	CSRBond* pRevisedClone = GetRevisedClone(transactionDate);
	if(!pRevisedClone)
		return 0.0;

	double val = pRevisedClone->ComputeYTMByPrice(	
		transactionDate,
		settlementDate, 
		ownershipDate,
		dirtyPrice,
		adjustedDates,
		valueDates,
		dayCountBasis, 
		yieldCalculation,
		context);

	delete pRevisedClone;
	val = CFG_Maths::Round(val , 3+2);
	return val;
};

/*virtual*/ void	CFG_ConstantAmortizedBond::InitDefaultValue(	short						&adjustedDatesCalc,
								 short						&valueDatesCalc,
								 const static_data::CSRDayCountBasis		**dayCountBasis, 
								 const static_data::CSRYieldCalculation	**yieldCalculation) const
{
	adjustedDatesCalc = 0;
	valueDatesCalc = 0;
	*dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis(dcb_Actual_Actual_AFB);
	*yieldCalculation = CSRYieldCalculation::GetCSRYieldCalculation(ycActuarial);
};


bool CFG_ConstantAmortizedBond::IsRevisable() const
{
	bool b = (GetClauseCountOfThisType("Revision") > 0)? true : false;
	return b;
};

long CFG_ConstantAmortizedBond::GetRevisedExpiry(long transactionDate) const
{
	long date = GetExpiry();
	int nbClauses = GetClauseCountOfThisType("Revision");
	SSClause c;
	for (int i = 0; i < nbClauses; i++)
		if(GetNthClauseOfThisType("Revision", i, &c))
			if( c.start_date <= date && c.start_date > transactionDate)
				date = c.start_date;

	return date;
};

SSAlert* CFG_ConstantAmortizedBond::NewAlertList(long forecastDate, int* nb) const
{
	*nb = 0;
	if(!IsRevisable())
		return NULL;

	int cutoffDate = forecastDate + 2;
	vector<long> vAlerts;

	SSClause c;
	int nbClauses = GetClauseCountOfThisType("Revision");
	for(int i = 0; i < nbClauses; i++)
		if(GetNthClauseOfThisType("Revision", i, &c))
			if(c.start_date <= cutoffDate && c.value.value <= 0.0)
				vAlerts.push_back(c.start_date);

	*nb = (int)vAlerts.size();
	if(*nb == 0)
		return NULL;

	SSAlert* pAlerts = new SSAlert[*nb];
	for(int i = 0; i < *nb; i++)
	{
		pAlerts[i].date = vAlerts[i];
		strcpy_s(pAlerts[i].infos, "Revision clause needs fixing");
	}

	return pAlerts;
};

CSRBond* CFG_ConstantAmortizedBond::GetRevisedClone(long transactionDate) const
{
	CSRBond* pClone = dynamic_cast<CSRBond*>(Clone_API());
	if(!pClone)
		return NULL;

	if(!IsRevisable())
		return pClone;

	long shortMaturity = GetRevisedExpiry(transactionDate);

	double remainingNotional = GetNotionalInProduct();
	SSRedemption r;
	vector<SSRedemption> vRedemptions;
	int nbRedemptions = pClone->GetRedemptionCount();
	for(int i = 0; i < nbRedemptions; i++)
		if(pClone->GetNthRedemption(i, &r) && r.startDate < shortMaturity)
		{
			if(r.endDate > shortMaturity)
			{
				r.endDate = shortMaturity;
				r.paymentDate = shortMaturity;
				r.maturityDate = shortMaturity;
				r.nonAdjustedMaturityDate = shortMaturity;
			}

			remainingNotional -= r.redemption;

			vRedemptions.push_back(r);
		}

		if(remainingNotional > 0.0)
		{
			r = SSRedemption();
			r.redemption = remainingNotional;
			r.startDate = shortMaturity;
			r.endDate = shortMaturity;
			r.paymentDate = shortMaturity;
			r.maturityDate = shortMaturity;
			r.nonAdjustedMaturityDate = shortMaturity;
			r.flowType = ftRedemption;
			r.percentage = 100.0;
			vRedemptions.push_back(r);
		}

		pClone->CleanRedemption();
		pClone->AddRedemption(vRedemptions);

		return pClone;
};

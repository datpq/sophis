#pragma warning(disable:4251)
#include "CFG_StaticSpreadBond.h"
#include "CFG_BondMarketData.h"
#include "CFG_RevisionClause.h"
#include "Resource/resource.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/inflation/SphInflationRule.h"
#include "SphInc/market_data/SphYieldCurve.h"
#include "SphInc/market_data/SphCreditRisk.h"
#include "SphInc/instrument/SphHandleError.h"
//DPH
//#include "SphInc/inflation/SphInflationCurve.h"
//DPH
#include "SphLLInc/finance_v2/SSModel.h"
#include "CFG_Maths.h"
#include "SphInc/static_data/SphDayCountBasis.h"
#include __STL_INCLUDE_PATH(vector)

using _STL::vector;

long CFG_StaticSpreadBond::yieldDecimals(3L);

CONSTRUCTOR_BOND(CFG_StaticSpreadBond, CSRBond);

Boolean CFG_StaticSpreadBond::ValidInstrument() const
{
	return CSRBond::ValidInstrument();
};

//DPH
const sophis::finance::CSRPricer * CFG_StaticSpreadBond::GetDefaultPricer() const {
	sophis::finance::CSRPricer* pricer = const_cast<sophis::finance::CSRPricer*>(sophis::finance::CSRPricer::GetPrototype().GetData("CFG_StaticSpreadBond_MetaModel"));
	return pricer;
}

double	CFG_StaticSpreadBond::GetDirtyPriceByZeroCoupon(	const CSRMarketData			&context,
															long 						transactionDate, 
															long						settlementDate,
															long						ownershipDate,
															short						adjustedDates,
															short						valueDates,
															const CSRDayCountBasis		*dayCountBasis, 
															const CSRYieldCalculation	*yieldCalculation,
															const finance::CSRPricer *	model) const
{
	if(!ValidInstrument() || !transactionDate)
		return 0.0;

	const CSRDefaultPricerBond* bondPricer = dynamic_cast<const CSRDefaultPricerBond*>(GetDefaultPricer());
	//DPH
	//double fairValue = ComputeNonFlatCurvePrice( 
	double fairValue = bondPricer->ComputeNonFlatCurvePrice(*this, 
		transactionDate,
		settlementDate,		
		ownershipDate,
		context,
		NULL,//derivee,
		NULL,//base,
		adjustedDates,
		valueDates,
		dayCountBasis,
		yieldCalculation,
		NULL,//redemptionArray,
		NULL,//explicationArray,
		true,
		false);

	if (GetAskQuotationType() == aqInPrice || GetAskQuotationType() == aqInPriceWithoutAccrued)
		return fairValue;
	else
		return GetNotionalInProduct()? fairValue / GetNotionalInProduct() * 100.0 : 0.0;
};

double CFG_StaticSpreadBond::GetDirtyPriceByYTM( long transactionDate, long settlementDate, long ownershipDate, double yieldToMaturity,
	//DPH
	const bool enableRounding) const
{
	return GetDirtyPriceByYTM(transactionDate,settlementDate,ownershipDate,yieldToMaturity,0,0);
};

double CFG_StaticSpreadBond::GetDirtyPriceByYTM(	long 						transactionDate, 
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

	bool isLongTerm;
	if(!GetCalculationParams(transactionDate, &dayCountBasis, &yieldCalculation, isLongTerm))
		return 0.0;

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

	double fairValue = pRevisedClone->ComputePriceByYTM(	
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

//	fairValue = CFG_Maths::Round(fairValue, 2); 

	if (GetAskQuotationType() == aqInPrice || GetAskQuotationType() == aqInPriceWithoutAccrued)
		return fairValue;
	else
		return GetNotionalInProduct()? fairValue / GetNotionalInProduct() * 100.0 : 0.0;
};

double CFG_StaticSpreadBond::GetYTMByDirtyPrice(long transactionDate, long settlementDate, long ownershipDate, double dirtyPrice,
	//DPH
	double startPoint) const
{
	return GetYTMByDirtyPrice(transactionDate,settlementDate,ownershipDate,dirtyPrice,0,0);
};

double CFG_StaticSpreadBond::GetYTMByDirtyPrice(	long 						transactionDate, 
													long 						settlementDate, 
													long						ownershipDate,
													double 						dirtyPrice,
													short						adjustedDates,
													short						valueDates,
													const CSRDayCountBasis*		dayCountBasis, 
													const CSRYieldCalculation*	yieldCalculation,
													const CSRMarketData&		context,
													//DPH
													double startPoint) const
{
	if(!transactionDate)
		return 0.0;

	bool isLongTerm;
	if(!GetCalculationParams(transactionDate, &dayCountBasis, &yieldCalculation, isLongTerm))
		return 0.0;

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

	val = CFG_Maths::Round(val, yieldDecimals + 2);
	return val;
};

//DPH
//double CFG_StaticSpreadBond::ComputeNonFlatCurvePrice(	long							transactionDate,
double CFG_StaticSpreadBond::EmcComputeNonFlatCurvePrice(long							transactionDate,
														long							settlementDate,		
														long							ownershipDate,
														const CSRMarketData				&param,
														double							*derivee,
														const CSRDayCountBasis			*base,
														short							adjustedDates,
														short							valueDates,
														const CSRDayCountBasis			*dayCountBasis,
														const CSRYieldCalculation		*yieldCalculation,
														_STL::vector<SSRedemption>		*redemptionArray,
														_STL::vector<SSBondExplication>	*explicationArray,
														bool							withSpreadMgt,
														bool							throwException) const
{
	if(!transactionDate)
		return 0.0;

	bool isLongTerm;
	if(!GetCalculationParams(transactionDate, &dayCountBasis, &yieldCalculation, isLongTerm))
		return 0.0;

	long currency = GetCurrency();
	//DPH
	//long family = GetFamily();
	long family = GetFamilyOrCurrency();
	const CSRYieldCurve* pZCYieldCurve = param.GetCSRYieldCurve(family);
	if(!pZCYieldCurve)
		return 0.0;

	double fairValue = 0.0;
	long expiryDate = GetRevisedExpiry(transactionDate);

	//DPH
	//tools::CSRAssignement<const SSModelMarkt*> toto(CSRMarketData::GetModelMarket(), CSRMarketData::GetModelMarket(*this));
	const SSModelMarkt* dest = const_cast<const SSModelMarkt*>(&(CSRMarketData::GetModelMarket()));
	const SSModelMarkt* src = const_cast<const SSModelMarkt*>(&(CSRMarketData::GetCurrentMarketData()->GetModelMarket(*this)));
	tools::CSRAssignement<const SSModelMarkt*> toto(dest, src);

	if (!redemptionArray && explicationArray)
		explicationArray = NULL;

	long taux_var = GetFloatingRate();
	const CSRInterestRate* pIR = CSRInstrument::GetInstance<CSRInterestRate>(taux_var);

	int	 redemptionCount	= GetRedemptionCount();
	double notional			= GetNotionalInProduct();

	SSCouponCalculation ccData = GetRules();
	bool inAvanceMethodPayment = ccData.fPaymentType != pmInArrears && taux_var!=0;
	SSDayCountCalculation dccData = GetDayCountCalculation();
	SSDayCountCalculation dccDataForBrokenFlows = dccData;
	dccDataForBrokenFlows.fBackWards = false;
	dccDataForBrokenFlows.fCashFlowType = SSDayCountCalculation::eLast;

	if(!ownershipDate)
		ownershipDate = settlementDate;
	if(ownershipDate < transactionDate)
		ownershipDate = transactionDate;

	SScoupon_amount	st;
	long sico = GetCode();
	if(!param.GetUserParameter(spAmountCoupon, &sico, &st))
	{
		st.start_date	= ownershipDate;
		st.end_date		= kInfiniteDate;
		st.discount		= true;
	}

	double remainingNotional = notional;
	double riskSpread = 0.0;
	LoadGeneralElement(IDC_RISK_SPREAD - ID_ITEM_SHIFT, &riskSpread);
	double overRate = param.GetOverRate(currency) + riskSpread / 100.0;

	double cf = pZCYieldCurve->GetForwardCompoundFactor(transactionDate, expiryDate, overRate);
	double dt;
	if(isLongTerm)
	{
		const CSRDayCountBasis * basisAFB = CSRDayCountBasis::GetCSRDayCountBasis(dcb_Actual_Actual_AFB);
		dt = basisAFB->GetEquivalentYearCount(transactionDate, expiryDate, dccData);
	}
	else //short term
	{
		dt = dayCountBasis->GetEquivalentYearCount(transactionDate, expiryDate, dccData);
	}
	double discountRate = yieldCalculation->GetRate(cf - 1.0, dt);
	discountRate=CFG_Maths::Round(discountRate,yieldDecimals+2);

	int nbExpl = 0;
	SSRedemption redemption, localFlow;
	SSBondExplication cashFlowExplication;

	int nb = redemptionCount;
	int index_start = 0;
	for( ; nb>1; index_start++, nb--)
	{
		if (GetNthRedemption(index_start, &localFlow))
		{
			if(localFlow.maturityDate > st.start_date && (!inAvanceMethodPayment || localFlow.flowType!=ftFloating)
				|| localFlow.startDate > st.start_date && localFlow.endDate)
				break;
			remainingNotional -= localFlow.redemption;
		}
	}

	int i = 0, index = 0;
	for(index=index_start, i=nb; --i>=0; index++)
	{
		if (!GetNthRedemption(index, &localFlow))
			continue;

		if(localFlow.maturityDate > st.end_date || localFlow.maturityDate > expiryDate) //coupon flow past expiry -> ignore
			break;

		memset(&cashFlowExplication, 0, sizeof(SSBondExplication));
		memset(&redemption, 0, sizeof(SSRedemption));
		dccData.SetCashFlowType(index , redemptionCount);

		double couponAmount = 0.0;

		if(localFlow.flowType != ftFloating)
		{
			couponAmount = localFlow.maturityDate > st.start_date? localFlow.coupon : 0.0;

			if(couponAmount && notional)
			{
				if (redemptionArray)
				{
					redemption.securityCount			= 0;
					redemption.convertedSecurityCount	= 0;
					redemption.redemption				= 0.0;
					redemption.percentage				= 100.0;
					redemption.instrumentCode			= 0;
					redemption.coupon					= couponAmount/notional;
					redemption.flowType					= ftFixed;
					redemption.startDate				= localFlow.startDate;
					redemption.endDate					= localFlow.endDate;
					redemption.paymentDate				= localFlow.paymentDate;
					redemption.maturityDate				= localFlow.maturityDate;
					redemption.nonAdjustedMaturityDate	= localFlow.maturityDate;
					redemption.poolFactor				= localFlow.poolFactor;
					redemption.absFlow					= localFlow.absFlow;
					redemption.isBrokenFlow				= localFlow.isBrokenFlow;

					(*redemptionArray).push_back(redemption);
				}
				if (explicationArray)
				{
					cashFlowExplication					 = redemption;
					cashFlowExplication.instrumentCode	 = index;
					cashFlowExplication.floatingRate	 = 0.0;
					cashFlowExplication.presentValue	 = redemption.coupon * notional;
					cashFlowExplication.netCoupon		 = cashFlowExplication.presentValue;
					cashFlowExplication.coupon			 = cashFlowExplication.presentValue;
					cashFlowExplication.inflationRate	 = 0.0;
					cashFlowExplication.dayCount		 = dayCountBasis->GetEquivalentDayCount(localFlow.startDate, localFlow.endDate, dccData);
					cashFlowExplication.rateForwardStartDate = 0;
					cashFlowExplication.rateForwardEndDate	 = 0;

					explicationArray->push_back(cashFlowExplication);
				}
			}
		}
		else if(pIR && localFlow.flowType==ftFloating)
		{
			ccData.fFixingDate	= localFlow.fixingDate;
			ccData.fDccData		= &dccData;
			ccData.fPaymentDate = localFlow.paymentDate;
			ccData.fFixingDate	= localFlow.fixingDate;

			double percent, minimum, maximum;
			GetFloatingRateComponent(localFlow, ccData, minimum, maximum, percent);
			CSRCashFlowInformation rateExplanation;
			CSRYieldCurveMarketData contextCurve(param, param.GetCSRYieldCurve(family), currency);

			double couponRate = pIR->GetUniversalCouponRate(
				contextCurve,
				localFlow.startDate,
				localFlow.endDate,
				ccData,
				&rateExplanation,
				true,
				false,
				GetCurrency(),
				minimum,
				maximum);

			couponAmount = percent * remainingNotional * couponRate;

			if (redemptionArray)
			{
				redemption.securityCount			= 0;
				redemption.convertedSecurityCount	= 0;
				redemption.redemption				= 0.;
				redemption.percentage				= 100.*percent;
				redemption.instrumentCode			= 0;
				redemption.coupon					= ccData.fMargin;
				redemption.flowType					= ftFloating;
				redemption.variable_rate			= taux_var;
				redemption.startDate				= localFlow.startDate;
				redemption.endDate					= localFlow.endDate;
				redemption.paymentDate				= localFlow.paymentDate;
				redemption.maturityDate				= localFlow.maturityDate;
				redemption.nonAdjustedMaturityDate	= localFlow.maturityDate;
				redemption.poolFactor				= localFlow.poolFactor;
				redemption.absFlow					= localFlow.absFlow;
				redemption.isBrokenFlow				= localFlow.isBrokenFlow;

				redemptionArray->push_back(redemption);
			}
			if (explicationArray)
			{
				cashFlowExplication						= redemption;
				cashFlowExplication.instrumentCode		= index; 
				cashFlowExplication.flowType			= rateExplanation.cash_flow_type == cfFixed ? ftFixed : ftFloating;
				cashFlowExplication.presentValue		= couponAmount;
				cashFlowExplication.netCoupon			= cashFlowExplication.presentValue;
				cashFlowExplication.coupon				= cashFlowExplication.presentValue;
				cashFlowExplication.inflationRate		= 0.0;
				cashFlowExplication.dayCount			= dayCountBasis->GetEquivalentDayCount(localFlow.startDate, localFlow.endDate, dccData);
				cashFlowExplication.floatingRate		= CFG_Maths::Round(yieldCalculation->GetRate(couponRate, cashFlowExplication.dayCount),yieldDecimals+2);
				cashFlowExplication.fixingWeight1		= rateExplanation.fFixingWeight1;
				cashFlowExplication.fixingWeight2		= rateExplanation.fFixingWeight2;
				cashFlowExplication.rateForwardStartDate = rateExplanation.start_date_floating_rate;
				cashFlowExplication.rateForwardEndDate	= rateExplanation.end_date_floating_rate;

				explicationArray->push_back(cashFlowExplication);
			}
		}

		double flow_dt = dayCountBasis->GetEquivalentYearCount(transactionDate, localFlow.maturityDate);
		double flowRate = yieldCalculation->GetCouponRate(discountRate, flow_dt);
		double timeFactor = (flow_dt == 0.0)? 1.0 : 1.0 / (1.0 + flowRate);
		
		if(base)
			timeFactor *= base->GetEquivalentYearCount(settlementDate,localFlow.paymentDate, dccDataForBrokenFlows);

		fairValue += couponAmount * timeFactor;

		if (explicationArray)
		{
			nbExpl = (int)explicationArray->size(); 
			for (int j = 0; j < nbExpl; j++)
			{
				if((*explicationArray)[j].instrumentCode==index && (*explicationArray)[j].flowType!=ftRedemption)
				{
					(*explicationArray)[j].YTM			 = timeFactor;
					(*explicationArray)[j].netCoupon	*= 1.0;
				}
			}
		}

		if(localFlow.redemption)
		{
			remainingNotional -= localFlow.redemption;
			fairValue += localFlow.redemption * timeFactor;

			if (redemptionArray)
			{
				redemption.securityCount			= localFlow.securityCount;
				redemption.convertedSecurityCount	= localFlow.convertedSecurityCount;
				redemption.redemption				= localFlow.redemption;
				redemption.percentage				= 100.0;
				redemption.instrumentCode			= 0;
				redemption.coupon					= 0.0;
				redemption.flowType					= ftRedemption;
				redemption.startDate				= localFlow.maturityDate;
				redemption.endDate					= localFlow.maturityDate;
				redemption.maturityDate				= localFlow.maturityDate;
				redemption.nonAdjustedMaturityDate	= localFlow.maturityDate;
				redemption.isBrokenFlow				= localFlow.isBrokenFlow;

				(*redemptionArray).push_back(redemption);
			}
			if (explicationArray)
			{
				cashFlowExplication							= redemption;
				cashFlowExplication.netCoupon				= 0.0;
				cashFlowExplication.instrumentCode			= index;
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
	}

	if(IsRevisable() && remainingNotional > 0) // full redemption of the rest on the revised expiry date
	{
		double flow_dt = dayCountBasis->GetEquivalentYearCount(transactionDate, expiryDate);
		double flowRate = yieldCalculation->GetCouponRate(discountRate, flow_dt);
		double timeFactor = (flow_dt == 0.0)? 1.0 : 1.0 / (1.0 + flowRate);

		if(base)
			timeFactor *= base->GetEquivalentYearCount(transactionDate,expiryDate, dccDataForBrokenFlows);
		
		fairValue += remainingNotional * timeFactor;

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

	double settlementFactor = pZCYieldCurve->GetForwardCompoundFactor(transactionDate, settlementDate, overRate);
	fairValue *= settlementFactor;

	if (explicationArray)
	{
		long code = GetCode();
		nbExpl = (int)explicationArray->size();
		for (int i = 0; i < nbExpl; i++)
		{
			(*explicationArray)[i].instrumentCode = code;
			(*explicationArray)[i].YTM *= settlementFactor;
			if ((*explicationArray)[i].flowType!=ftRedemption)
				(*explicationArray)[i].presentValue = (*explicationArray)[i].netCoupon*(*explicationArray)[i].YTM;
			else
				(*explicationArray)[i].presentValue *= (*explicationArray)[i].YTM;

			long maturityCashFlow = (*explicationArray)[i].maturityDate;
			if(maturityCashFlow>settlementDate && fabs((*explicationArray)[i].YTM) > 1e-8)
			{
				double dtLoc = dayCountBasis->GetEquivalentYearCount(settlementDate, maturityCashFlow, dccDataForBrokenFlows);
				//double dtLoc = TKT_dayCountBasis->GetEquivalentYearCount(settlementDate, maturityCashFlow, dccDataForBrokenFlows);
				(*explicationArray)[i].YTM = (dtLoc > 1e-8)? CFG_Maths::Round(yieldCalculation->GetRate(1.0/(*explicationArray)[i].YTM - 1.0, dtLoc), yieldDecimals + 2) : 0.0;

			}
			else
				(*explicationArray)[i].YTM = 0.0;
		}
	}

	return fairValue;
};

bool CFG_StaticSpreadBond::GetCalculationParams(long transactionDate, const CSRDayCountBasis** dayCountBasis, const CSRYieldCalculation** yieldCalculation, bool& isLongTerm) const
{
	long currency = GetCurrency();
	const CSRCurrency* pCcy = CSRCurrency::GetCSRCurrency(currency);
	if(!pCcy) return false;

	long expiryDate = GetRevisedExpiry(transactionDate);
	SSMaturity mat_1y;
	mat_1y.fMaturity = 1;
	mat_1y.fType = 'y';
	long timeToMaturity_1y = SSMaturity::GetDayCount(mat_1y, transactionDate, pCcy);

	if(expiryDate - transactionDate <= timeToMaturity_1y)
	{
		*dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis(dcbActual_360);
		*yieldCalculation = CSRYieldCalculation::GetCSRYieldCalculation(ycLinear);
		isLongTerm = false;
	}
	else
	{
		//*dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis(dcb_Actual_Actual_AFB);
		*dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis("Act/Act CFG Coupon");
		*yieldCalculation = CSRYieldCalculation::GetCSRYieldCalculation(ycActuarial);
		isLongTerm = true;
	}

	return true;
};

bool CFG_StaticSpreadBond::IsRevisable() const
{
	bool b = (GetClauseCountOfThisType("Revision") > 0)? true : false;
	return b;
};

long CFG_StaticSpreadBond::GetRevisedExpiry(long transactionDate) const
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

SSAlert* CFG_StaticSpreadBond::NewAlertList(long forecastDate, int* nb) const
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

CSRBond* CFG_StaticSpreadBond::GetRevisedClone(long transactionDate) const
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

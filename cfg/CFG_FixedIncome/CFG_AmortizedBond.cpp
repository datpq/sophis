#include "CFG_AmortizedBond.h"
#include "CFG_BondMarketData.h"
#include "Resource/resource.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/inflation/SphInflationRule.h"
#include "SphInc/market_data/SphYieldCurve.h"
#include "SphInc/market_data/SphCreditRisk.h"
#include "SphInc/instrument/SphHandleError.h"
#include "SphInc/inflation/SphInflationCurve.h"

CONSTRUCTOR_BOND(CFG_AmortizedBond, CSRBond);

Boolean CFG_AmortizedBond::ValidInstrument() const
{
	return CSRBond::ValidInstrument();
};

double	CFG_AmortizedBond::GetDirtyPriceByZeroCoupon(	const CSRMarketData			&context,
															long 						transactionDate, 
															long						settlementDate,
															long						ownershipDate,
															short						adjustedDates,
															short						valueDates,
															const CSRDayCountBasis		*dayCountBasis, 
															const CSRYieldCalculation	*yieldCalculation) const
{
	if(!ValidInstrument() || !transactionDate)
		return 0.0;

	double fairValue = ComputeNonFlatCurvePrice( 
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

double CFG_AmortizedBond::ComputeNonFlatCurvePrice(	long							transactionDate,
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

	long currency = GetCurrency();
	long family = GetFamily();
	if(!currency || !family)
		return 0.0;
	const CSRYieldCurve* pZCYieldCurve = param.GetCSRYieldCurve(family);
	if(!pZCYieldCurve)
		return 0.0;

	double fairValue = 0.0;

	tools::CSRAssignement<const SSModelMarkt*> toto(CSRMarketData::GetModelMarket(), CSRMarketData::GetModelMarket(*this));
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
		}
	}

	int i = 0, index = 0;
	for(index=index_start, i=nb; --i>=0; index++)
	{
		if (!GetNthRedemption(index, &localFlow))
			continue;

		if(localFlow.maturityDate > st.end_date) //flow past expiry -> ignore
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

			double couponRate	= pIR->GetUniversalCouponRate(
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
				cashFlowExplication.floatingRate		= yieldCalculation->GetRate(couponRate, cashFlowExplication.dayCount);
				cashFlowExplication.fixingWeight1		= rateExplanation.fFixingWeight1;
				cashFlowExplication.fixingWeight2		= rateExplanation.fFixingWeight2;
				cashFlowExplication.rateForwardStartDate = rateExplanation.start_date_floating_rate;
				cashFlowExplication.rateForwardEndDate	= rateExplanation.end_date_floating_rate;

				explicationArray->push_back(cashFlowExplication);
			}
		}

		double cf = pZCYieldCurve->GetForwardCompoundFactor(transactionDate, localFlow.maturityDate, overRate);
		double timeFactor = 1.0 / cf;

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
				(*explicationArray)[i].YTM = (dtLoc > 1e-8)? yieldCalculation->GetRate(1.0/(*explicationArray)[i].YTM - 1.0, dtLoc) : 0.0;
			}
			else
				(*explicationArray)[i].YTM = 0.0;
		}
	}

	return fairValue;
};

void CFG_AmortizedBond::InitDefaultValue(	short									&adjustedDatesCalc,
											short									&valueDatesCalc,
											const static_data::CSRDayCountBasis		**dayCountBasis, 
											const static_data::CSRYieldCalculation	**yieldCalculation) const
{
	adjustedDatesCalc = 0;
	valueDatesCalc = 0;
	*dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis(dcb_Actual_Actual_AFB);
	*yieldCalculation = CSRYieldCalculation::GetCSRYieldCalculation(ycActuarial);
};

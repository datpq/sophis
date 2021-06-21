#pragma warning(disable:4251)
#include "CFG_RevisableBondAction.h"
#include "CFG_RevisionClause.h"
#include "SphInc/instrument/SphClause.h"
#include __STL_INCLUDE_PATH(vector)

using _STL::vector;

CONSTRUCTOR_INSTRUMENT_ACTION(CFG_RevisableBondAction);

void CFG_RevisableBondAction::VoteForCreation(CSRInstrument& instrument)
throw (VoteException , ExceptionBase)
{
	if(instrument.GetType_API() != iBond)
		return;
	CSRBond* pBond = dynamic_cast<CSRBond*>(&instrument);
	ModifyRedemptions(pBond);
};

void CFG_RevisableBondAction::VoteForModification(CSRInstrument& instrument, NSREnums::eParameterModificationType type)
throw (VoteException, ExceptionBase)
{
	if(instrument.GetType_API() != iBond)
		return;
	CSRBond* pBond = dynamic_cast<CSRBond*>(&instrument);
	ModifyRedemptions(pBond);
};

void CFG_RevisableBondAction::ModifyRedemptions(CSRBond* pBond)
{
	if(!pBond) return;

	//Amortized bond, constant annuities
	long nbAmort = pBond->GetNumberOfPartialRedemptions();
	double notional = pBond->GetNotionalInProduct();
	if(pBond->GetFloatingRate() == 0L && nbAmort > 1L && pBond->GetPartialRedemptionType() == ePRTSea)
	{
		pBond->SetPartialRedemptionType(ePRTFloatingNotional);
		int nbRedemptions = pBond->GetRedemptionCount();
		nbAmort = nbRedemptions; // 1 coupon = 1 redemption (constant annuity)
		double rate = pBond->GetNotionalRate() / 100.0;
		double annuity = notional * rate / (1.0 - pow(1.0 + rate, -nbAmort));
		double remainingNotional = notional;
		vector<SSRedemption> vRedemptions;
		SSRedemption r;
		for (int i = 0; i < nbRedemptions; i++)
			if(pBond->GetNthRedemption(i, &r))
			{
				r.coupon = GetUnitCouponValue(pBond, r) * remainingNotional;
				r.redemption = annuity - r.coupon;
				r.securityCount = 0;

				vRedemptions.push_back(r);
				remainingNotional -= r.redemption;
			}

		pBond->CleanRedemption();
		pBond->SetRedemption(vRedemptions);
		pBond->SetModified(true);
	}
	
	if(pBond->GetFloatingRate() == 0L && nbAmort > 1L && pBond->GetPartialRedemptionType() == ePRTNotional)
	{
		pBond->SetPartialRedemptionType(ePRTFloatingNotional);
		pBond->SetModified(true);
	}

	// Revisable bond
	if(pBond->GetFloatingRate() == 0L && pBond->GetClauseCountOfThisType("Revision") > 0)
	{
		double remainingNotional = notional;
		int nbRedemptions = pBond->GetRedemptionCount();
		vector<SSRedemption> vRedemptions;
		SSRedemption r;
		for (int i = 0; i < nbRedemptions; i++)
			if(pBond->GetNthRedemption(i, &r))
			{
				if(r.coupon > 0.0)
					r.coupon = GetUnitCouponValue(pBond, r) * remainingNotional;

				vRedemptions.push_back(r);
				remainingNotional -= r.redemption;
			}

		pBond->CleanRedemption();
		pBond->SetRedemption(vRedemptions);
		pBond->SetModified(true);
	}
};

double CFG_RevisableBondAction::GetUnitCouponValue(const CSRBond* pBond, const SSRedemption& r) const
{
	if(!pBond) return 0.0;
	const CSRDayCountBasis* pDCB = CSRDayCountBasis::GetCSRDayCountBasis(pBond->GetMarketCSDayCountBasisType());
	if(!pDCB) return 0.0;
	const CSRYieldCalculation* pYC = CSRYieldCalculation::GetCSRYieldCalculation(pBond->GetYieldCalculationType());
	if(!pYC) return 0.0;
	double rate = CFG_RevisionClause::GetCorrespondingRate(*pBond, r);
	double dt = pDCB->GetEquivalentYearCount(r.startDate, r.endDate, pBond->GetDayCountCalculation());
	double coupon = pYC->GetCouponRate(rate, dt);
	return coupon;
};

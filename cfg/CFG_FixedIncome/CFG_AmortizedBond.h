#ifndef __CFG_AmortizedBond_H__
#define __CFG_AmortizedBond_H__

#include "SphInc/instrument/SphBond.h"

class CFG_AmortizedBond : public sophis::instrument::CSRBond
{
//------------------------------------ PUBLIC ---------------------------------
public:
	DECLARATION_BOND(CFG_AmortizedBond);

	virtual double	GetDirtyPriceByZeroCoupon(
		const CSRMarketData&		context,
		long						transactionDate, 
		long 						settlementDate,
		long						ownershipDate,
		short						adjustedDates,
		short						valueDates = kUseDefaultValue,
		const CSRDayCountBasis*		dayCountBasis = 0, 
		const CSRYieldCalculation*	yieldCalculation = 0) const;

	virtual double	ComputeNonFlatCurvePrice(  
		long							transactionDate,
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
		bool							throwException) const;

	virtual void	InitDefaultValue(	
		short									&adjustedDatesCalc,
		short									&valueDatesCalc,
		const static_data::CSRDayCountBasis		**dayCountBasis, 
		const static_data::CSRYieldCalculation	**yieldCalculation) const;

	virtual Boolean ValidInstrument() const;
};

#endif // __CFG_AmortizedBond_H__

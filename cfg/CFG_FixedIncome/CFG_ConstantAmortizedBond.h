#ifndef __CSxConstantAmortizedBond_H__
#define __CSxConstantAmortizedBond_H__

/*
** Includes
*/
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/instrument/SphBond.h"
#include "SphInc/market_data/SphMarketData.h"

//DPH
#include "UpgradeExtension.h"

/*
** Class definition for the bond model.
*/
//DPH
//class CFG_ConstantAmortizedBond : public sophis::instrument::CSRBond
class CFG_ConstantAmortizedBond : public EmcBond
{
//------------------------------------ PUBLIC ---------------------------------
public:
	DECLARATION_BOND(CFG_ConstantAmortizedBond);
	
	//DPH
	virtual const sophis::finance::CSRPricer * GetDefaultPricer() const override;

	//DPH
	//virtual double	ComputeNonFlatCurvePrice(	long	transactionDate,
	virtual double	EmcComputeNonFlatCurvePrice(long	transactionDate,
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
												bool									throwException) const;
	
	virtual double	GetDirtyPriceByYTM(			
		long 						transactionDate, 
		long 						settlementDate,
		long						pariPassuDate,
		double 						yieldToMaturity,
		short						adjustedDates,
		short						valueDates=kUseDefaultValue,
		const CSRDayCountBasis		*dayCountBasis = 0, 
		const CSRYieldCalculation	*yieldCalculation = 0,
		const CSRMarketData			&context = *gApplicationContext) const;

	virtual double	GetYTMByDirtyPrice(			
		long 						transactionDate, 
		long 						settlementDate,
		long						pariPassuDate,
		double 						dirtyPrice,
		short						adjustedDates,
		short						valueDates=kUseDefaultValue,
		const CSRDayCountBasis		*dayCountBasis = 0, 
		const CSRYieldCalculation	*yieldCalculation = 0,
		const CSRMarketData			&context = *gApplicationContext,
		//DPH
		double						startPoint = NOTDEFINED) const;

	virtual void	InitDefaultValue(	short						&adjustedDatesCalc,
										short						&valueDatesCalc,
										const static_data::CSRDayCountBasis		**dayCountBasis, 
										const static_data::CSRYieldCalculation	**yieldCalculation) const;

	virtual SSAlert* NewAlertList(long forecastDate, int* nb) const;
	
	virtual bool IsRevisable() const;
	virtual long GetRevisedExpiry(long transactionDate) const;
	virtual CSRBond* GetRevisedClone(long transactionDate) const;
};

#endif // __CSxConstantAmortizedBond_H__

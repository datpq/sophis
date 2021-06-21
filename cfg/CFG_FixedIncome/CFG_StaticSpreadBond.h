#ifndef __CFG_StaticSpreadBond_H__
#define __CFG_StaticSpreadBond_H__

#include "SphInc/instrument/SphBond.h"
#include "SphInc/market_data/SphMarketData.h"

//DPH
#include "UpgradeExtension.h"

//DPH
//class CFG_StaticSpreadBond : public sophis::instrument::CSRBond
class CFG_StaticSpreadBond : public EmcBond
{
//------------------------------------ PUBLIC ---------------------------------
public:
	DECLARATION_BOND(CFG_StaticSpreadBond);

	//DPH
	virtual const sophis::finance::CSRPricer * GetDefaultPricer() const override;

	virtual double	GetDirtyPriceByZeroCoupon(
		const CSRMarketData&		context,
		long						transactionDate, 
		long 						settlementDate,
		long						ownershipDate,
		short						adjustedDates,
		short						valueDates = kUseDefaultValue,
		const CSRDayCountBasis*		dayCountBasis = 0, 
		const CSRYieldCalculation*	yieldCalculation = 0,
		//DPH
		const finance::CSRPricer *	model = NULL) const;

	virtual double	GetDirtyPriceByYTM( long transactionDate, 
		long 						settlementDate,
		long						pariPassuDate,
		double						yieldToMaturity,
		//DPH
		const bool enableRounding) const;

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

	virtual double	GetYTMByDirtyPrice(	long transactionDate, 
		long 		 settlementDate,
		long		 ownershipDate,
		double 		 dirtyPrice,
		//DPH
		double		 startPoint = NOTDEFINED) const;

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

	//DPH
	//virtual double	ComputeNonFlatCurvePrice(
	virtual double	EmcComputeNonFlatCurvePrice(  
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

	virtual Boolean ValidInstrument() const;
	virtual SSAlert* NewAlertList(long forecastDate, int* nb) const;

	virtual bool GetCalculationParams(long transactionDate, const CSRDayCountBasis** dayCountBasis, const CSRYieldCalculation** yieldCalculation, bool& isLongTerm) const;
	
	virtual bool IsRevisable() const;
	virtual long GetRevisedExpiry(long transactionDate) const;
	virtual CSRBond* GetRevisedClone(long transactionDate) const;
	static long yieldDecimals;
};

#endif // __CFG_StaticSpreadBond_H__

#ifndef __CFG_YieldCurveFixedPoints_H__
#define __CFG_YieldCurveFixedPoints_H__

#include "SphInc/market_data/SphYieldCurve.h"
#include "SphInc/SphEnums.h"
#include "SphTools/SphExceptions.h"

//DPH
//TODO to choose between CSRYieldCurveLinear and CSRYieldCurveNOTLinear
//class CFG_YieldCurveFixedPoints : public sophis::market_data::CSRYieldCurve
class CFG_YieldCurveFixedPoints : public sophis::market_data::CSRYieldCurveLinear
{
//------------------------------------ PUBLIC ---------------------------------
public:
	DECLARATION_YIELD_CURVE(CFG_YieldCurveFixedPoints);
	virtual void Initialize(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);
	virtual void InitialiseConstructor(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);
	virtual void InitialiseConstructorWithSpreadMgr(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);

	virtual double GetInterpolatedZeroCouponRate( double ratePrevious, double rateNext, 
		double timeToMaturityPrevious, double timeToMaturityNext, double timeToMaturity, const CSRCalendar* calendar) const;
	//DPH
	virtual double GetCompoundFactor(double timeToMaturity, double overRate = 0) const;
	//virtual double CompoundFactor(double timeToMaturity, double overRate=0) const;

	mutable int fShift;

	//DPH
	bool takeDateCurveForStartPoint;
};

#endif //!__CFG_YieldCurveFixedPoints_H__
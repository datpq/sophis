#ifndef __CSxZCYieldCurveSimulation_H__
#define __CSxZCYieldCurveSimulation_H__


#include "SphInc/market_data/SphYieldCurve.h"

#define ZCYieldCurveSimulationModelName "CFG ZC Simulation"

//DPH CSRYieldCurveNOTLinear.IsCompoundFactorOverloaded = true, CSRYieldCurveLinear.IsCompoundFactorOverloaded = false
class CSxZCYieldCurveSimulation : public sophis::market_data::CSRYieldCurveLinear
{
//------------------------------------ PUBLIC ---------------------------------
public:
	DECLARATION_YIELD_CURVE(CSxZCYieldCurveSimulation);

	virtual void Initialize(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);
	virtual void InitialiseConstructor(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);
	virtual void InitialiseConstructorWithSpreadMgr(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);

	virtual double GetInterpolatedZeroCouponRate(	double ratePrevious, 
													double rateNext, 
													double timeToMaturityPrevious, 
													double timeToMaturityNext, 
													double timeToMaturity) const;
	//DPH
	//virtual double CompoundFactor(	double timeToMaturity, 
	virtual double GetCompoundFactor(double timeToMaturity,
									double overRate = 0) const;

	mutable int fShift;

//------------------------------------ PROTECTED ---------------------------------
protected:
	long GetTimeToMaturity1y(const CSRCalendar * calendar) const;

	double GetCouponRate(double rate, double timeToMaturity, int shift, bool forceShort = false) const;
	double ConvertShortToLong(double rateShort, double timeToMaturity) const;
	double ConvertLongToShort(double rateLong, double timeToMaturity) const;

//------------------------------------ PRIVATE ---------------------------------
private:
	/*
	** Logger data
	*/
	static const char * __CLASS__;

	//DPH
	bool takeDateCurveForStartPoint;
};

#endif //__CSxZCYieldCurveSimulation_H__
#ifndef __CFG_YieldCurveZCMAD_H__
#define __CFG_YieldCurveZCMAD_H__

#include "CFG_Maths.h"
#include "SphInc/market_data/SphYieldCurve.h"
#include "SphInc/SphEnums.h"
#include "SphTools/SphExceptions.h"

//DPH
//TODO to choose between CSRYieldCurveLinear and CSRYieldCurveNOTLinear
//class CFG_YieldCurveZCMAD : public sophis::market_data::CSRYieldCurve
class CFG_YieldCurveZCMAD : public sophis::market_data::CSRYieldCurveLinear
{
//------------------------------------ PUBLIC ---------------------------------
public:
	DECLARATION_YIELD_CURVE(CFG_YieldCurveZCMAD);
	virtual void Initialize(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);
	virtual void InitialiseConstructor(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);
	virtual void InitialiseConstructorWithSpreadMgr(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve);

	virtual double GetInterpolatedZeroCouponRate( double ratePrevious, double rateNext, 
		double timeToMaturityPrevious, double timeToMaturityNext, double timeToMaturity) const;
	//DPH
	virtual double GetCompoundFactor(double timeToMaturity, double overRate = 0) const;
	//virtual double CompoundFactor(double timeToMaturity, double overRate=0) const;
	virtual double GetCalculatedDF(int y, double zc_y, double* dfArray) const;

	mutable int fShift;
	//DPH
	bool takeDateCurveForStartPoint;
};

class BootstrapFunction : public CalcFunction
{
	//------------------------------------ PUBLIC ---------------------------------
public:
	BootstrapFunction(int which, double zc_which, double* zcArray);
	virtual double f(double x);

	int nMat;
	double zc_n;
	double* zcCalculated;
};

#endif //!__CFG_YieldCurveZCMAD_H__
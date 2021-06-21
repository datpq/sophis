#include "CFG_YieldCurveFixedPoints.h"
#include "CFG_Maths.h"
#include "SphInc\market_data\SphMarketData.h"
#include "SphInc\static_data\SphCurrency.h"
#include "SphInc\static_data\SphYieldCurveFamily.h"

//DPH
#include "UpgradeExtension.h"

WITHOUT_CONSTRUCTOR_YIELD_CURVE(CFG_YieldCurveFixedPoints);

CFG_YieldCurveFixedPoints::CFG_YieldCurveFixedPoints(const SSYieldCurve& curve)
: CSRYieldCurveLinear()
{	
	Initialize(curve, true, true);
	//DPH
	takeDateCurveForStartPoint = UpgradeExtension::TakeDateCurveForStartPoint();
};

void CFG_YieldCurveFixedPoints::Initialize(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
{
	InitialiseConstructorWithSpreadMgr(curve, computeZeroCouponYieldCurve, validationYieldCurve);
};

void CFG_YieldCurveFixedPoints::InitialiseConstructorWithSpreadMgr(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
{
	SetCalibrationState(eNotCalibrated);

	if (fZeroCouponArray)
		delete [] fZeroCouponArray;
	if (fZeroCouponMaturity)
		delete [] fZeroCouponMaturity;

	fZeroCouponArray		= 0;
	fZeroCouponMaturity		= 0;
	fZeroCouponPointCount	= 0;
	fShock					= 0;
	fCurrencyCode			= 0;
	fShift					= 0;
	fStartPointDate			= 0;
	fYieldCurve				= curve;

	CSRSpreadCurveMgr* mgr = (CSRSpreadCurveMgr*) GetCSRSpreadCurveMgr();
	if (mgr && !mgr->InitCurve())
		return;
	
	if (mgr)
		mgr->BeginConstructor(this, fYieldCurve, computeZeroCouponYieldCurve, validationYieldCurve);
	
	InitialiseConstructor(fYieldCurve, computeZeroCouponYieldCurve, validationYieldCurve);
	
	if (mgr)
		mgr->EndConstructor(this, fYieldCurve, computeZeroCouponYieldCurve, validationYieldCurve);

	SetCalibrationState(eCalibrated);
};

void CFG_YieldCurveFixedPoints::InitialiseConstructor(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
{
	//DPH
	//fZeroCouponArray		= new double [fYieldCurve.fPointCount];
	fZeroCouponArray = new double[fYieldCurve.fPoints.fPointCount];
	//fZeroCouponMaturity		= new long [fYieldCurve.fPointCount];
	fZeroCouponMaturity = new long[fYieldCurve.fPoints.fPointCount];
	fZeroCouponPointCount	= 0;
	fStartPointDate			= 0;
	fShift					= 0;
	fYieldCurve				= curve;
	fCurrencyCode			= fYieldCurve.fCode;
	fShock					= 0;
	const CSRCalendar* pCcy = CSRCurrency::GetCSRCurrency(fCurrencyCode);
	if(!pCcy) return;

	//DPH
	//long family = CSRYieldCurveFamily::GetYieldCurveFamilyCode(fCurrencyCode, fYieldCurve.fSpreadFamily);
	long family = CSRYieldCurveFamily::GetYieldCurveFamilyCode(fCurrencyCode, fYieldCurve.fSpreadFamily.c_str());
	const CSRYieldCurve* pBaseYC = CSRYieldCurve::GetInstanceByYieldCurveFamily(family);
	if(!pBaseYC) return;
	
	long oldAppDate = gApplicationContext->GetDate();
	//DPH
	//if (CSRPreference::TakeDateCurveForStartPoint() && fYieldCurve.fCurveDate < oldAppDate)
	if (UpgradeExtension::TakeDateCurveForStartPoint() && fYieldCurve.fCurveDate < oldAppDate)
		gApplicationContext->SetGlobalDate(fYieldCurve.fCurveDate);
	
	long today = gApplicationContext->GetDate();
	//DPH
	//for(int i = 0; i < fYieldCurve.fPointCount; i++)
	for (int i = 0; i < fYieldCurve.fPoints.fPointCount; i++)
	{
		//DPH
		//if(!(fYieldCurve.fPointList[i]).fInfoPtr->fIsUsed)
		if (!(fYieldCurve.fPoints.fPointList[i]).fInfoPtr->fIsUsed)
			continue;
		
		SSMaturity mat;
		//DPH
		//mat.fMaturity = (fYieldCurve.fPointList[i]).fMaturity;
		mat.fMaturity = (fYieldCurve.fPoints.fPointList[i]).fMaturity;
		//mat.fType = (fYieldCurve.fPointList[i]).fType;
		mat.fType = (fYieldCurve.fPoints.fPointList[i]).fType;
		long maturity = SSMaturity::GetDayCount(mat, today, NULL);
		
		long timeToMaturity_1y = CFG_Maths::GetTimeToMaturity1y(pCcy);
		eDayCountBasisType dcb = dcb_Actual_Actual_AFB;
		eYieldCalculationType yc = ycActuarial;
		if(maturity <= timeToMaturity_1y)
		{
			dcb = dcbActual_360;
			yc = ycLinear;
		}

		double cf = pBaseYC->CompoundFactor((double)maturity);
		double zc = CFG_Maths::GetZCFromCompoundFactor(cf, maturity, dcb, yc);

		fZeroCouponPointCount++;
		SetZeroCouponPoint(zc, maturity);
	}

	gApplicationContext->SetGlobalDate(oldAppDate);
};

double CFG_YieldCurveFixedPoints::GetInterpolatedZeroCouponRate( double ratePrevious, double rateNext, 
																double timeToMaturityPrevious, double timeToMaturityNext, 
																double timeToMaturity, const CSRCalendar* calendar) const
{
	double interpolatedRate = 0.0;

	long timeToMaturity_1y = CFG_Maths::GetTimeToMaturity1y(calendar);
	if(timeToMaturityPrevious <= timeToMaturity_1y && timeToMaturityNext > timeToMaturity_1y) // each basis is different, convert lower base from Act/360 Linear to Act/Act Actuarial
	{
		// OPTION 1: ALWAYS INTERPOLATE IN ACT/ACT ACTUARIAL
		//double weight = (timeToMaturityNext - timeToMaturity) / (timeToMaturityNext - timeToMaturityPrevious);
		//ratePrevious = CFG_Maths::ConvertZCRate(ratePrevious, (long) timeToMaturityPrevious, dcbActual_360, ycLinear, dcb_Actual_Actual_AFB, ycActuarial, calendar);
		//interpolatedRate = rateNext - weight * (rateNext - ratePrevious); // in Act/Act Actuarial
		//if(timeToMaturity <= timeToMaturity_1y) // re-convert from Act/Act Actuarial to Act/360 Linear
		//	interpolatedRate = CFG_Maths::ConvertZCRate(interpolatedRate, (long) timeToMaturity, dcb_Actual_Actual_AFB, ycActuarial, dcbActual_360, ycLinear, calendar);
		// END OPTION 1

		// OPTION 2: INTERPOLATION DEPENDING ON MATURITY
		//long today = gApplicationContext->GetDate();
		if(timeToMaturity <= timeToMaturity_1y)
		{
			double weight = (timeToMaturity - timeToMaturityPrevious) / (timeToMaturityNext - timeToMaturityPrevious);
			rateNext = CFG_Maths::ConvertZCRate(rateNext, (long) timeToMaturityNext, dcb_Actual_Actual_AFB, ycActuarial, dcbActual_360, ycLinear, calendar);
			interpolatedRate = ratePrevious + weight * (rateNext - ratePrevious); // in Act/360 Linear
		}
		else
		{
			double weight = (timeToMaturityNext - timeToMaturity) / (timeToMaturityNext - timeToMaturityPrevious);
			ratePrevious = CFG_Maths::ConvertZCRate(ratePrevious, (long) timeToMaturityPrevious, dcbActual_360, ycLinear, dcb_Actual_Actual_AFB, ycActuarial, calendar);
			interpolatedRate = rateNext - weight * (rateNext - ratePrevious); // in Act/Act Actuarial
		}
		// END OPTION 2
	}
	else
	{
		double weight = (timeToMaturity - timeToMaturityPrevious) / (timeToMaturityNext - timeToMaturityPrevious);
		interpolatedRate = ratePrevious + weight * (rateNext - ratePrevious);
	}

	return interpolatedRate;
};

//DPH
//double CFG_YieldCurveFixedPoints::CompoundFactor(double timeToMaturity, double overRate) const
double CFG_YieldCurveFixedPoints::GetCompoundFactor(double timeToMaturity, double overRate) const
{
	const CSRSpreadCurveMgr* mgr = GetCSRSpreadCurveMgr();
	if (mgr)
		return mgr->CompoundFactor(this, timeToMaturity, overRate);

	long appDate = gApplicationContext->GetDate();
	const CSRCalendar* calendar = CSRCurrency::GetCSRCurrency(fCurrencyCode);

	const long* mat;
	int i, shift;
	double rate, shiftzc;
	long date = fStartPointDate ? fStartPointDate : appDate;

	if (GetSpreadType() == ycstZeroCoupon)
		overRate += GetSpread();

	if(timeToMaturity <= 0) // negative maturity
		return 1.0;

	if(fZeroCouponPointCount == 0) // empty curve
		return pow(1.0 + overRate, timeToMaturity / 365.25);

	//date shift, if any
	//DPH
	//bool takeDateCurveForStartPoint = CSRPreference::TakeDateCurveForStartPoint();
	if(takeDateCurveForStartPoint && ( shift = date - fYieldCurve.fCurveDate) > 0)
	{
		if(fShift != shift)
		{
			//DPH
			//CSRPreference::SetTakeDateCurveForStartPoint(false);
			CFG_YieldCurveFixedPoints * changeMemberInConstMethodTrick = const_cast<CFG_YieldCurveFixedPoints*> (this);
			changeMemberInConstMethodTrick->takeDateCurveForStartPoint = false;
			//recalculating shift 
			double spreadToAdd = (GetSpreadType()==ycstZeroCoupon) ? -GetSpread() : 0;
			shiftzc = CompoundFactor(shift, spreadToAdd);
			
			//resetting variables
			//DPH
			//CSRPreference::SetTakeDateCurveForStartPoint(true);
			changeMemberInConstMethodTrick->takeDateCurveForStartPoint = true;
			fShift = shift;
		}

		timeToMaturity += shift; 
	}
	else
	{
		shift = 0;
		shiftzc = 1;
	}

	if(fZeroCouponPointCount == 1) // only one point
	{
		rate = fZeroCouponArray[0];
		i = 1;
	}
	else
	{
		for(mat = fZeroCouponMaturity, i = 0; i < fZeroCouponPointCount; i++, mat++)
			if(timeToMaturity < *mat)
				break;

		if(i == 0) // before first point
		{
			rate = fZeroCouponArray[0];
		}
		else if(i == fZeroCouponPointCount) // after last point
		{
			rate = GetInterpolatedZeroCouponRate(
				fZeroCouponArray[fZeroCouponPointCount-2], 
				fZeroCouponArray[fZeroCouponPointCount-1], 
				mat[-2], 
				mat[-1], 
				timeToMaturity, 
				calendar);
		}
		else
		{
			rate = GetInterpolatedZeroCouponRate(
				fZeroCouponArray[i-1], 
				fZeroCouponArray[i], 
				mat[-1], 
				mat[0], 
				timeToMaturity, 
				calendar);
		}
	}

	if (fShock)
		overRate += fShock->GetShock(timeToMaturity);

	long timeToMaturity_1y = CFG_Maths::GetTimeToMaturity1y(calendar);
	long t = (long)timeToMaturity - shift;
	eDayCountBasisType dcb = (t <= timeToMaturity_1y)? dcbActual_360 : dcb_Actual_Actual_AFB;
	eYieldCalculationType yc = (t <= timeToMaturity_1y)? ycLinear : ycActuarial;
	double cf = CFG_Maths::GetCompoundFactorFromZC(rate + overRate, t, dcb, yc, calendar);
	return cf;
};

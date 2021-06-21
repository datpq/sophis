#include "CFG_YieldCurveLinebyLine.h"
#include "CFG_YieldCurveData.h"
#include "CFG_Maths.h"
#include "SphInc\market_data\SphMarketData.h"
#include "SphInc\static_data\SphCurrency.h"
#include "SphInc\instrument\SphHandleError.h"
#include "SphInc\market_data\SphDataIntegrityManager.h"
#include "SphTools\SphLoggerUtil.h"
#include "SphSDBCInc\queries\SphQueryBuffered.h"
#include "CFG_YCDataTable.h"

#pragma warning(push)
#pragma warning(disable:4103) //  '...' : alignment changed after including header, may be due to missing #pragma pack(pop)
#include __STL_INCLUDE_PATH(stdio.h)
#pragma warning(pop)

//DPH
#include "UpgradeExtension.h"

using _STL::map;

WITHOUT_CONSTRUCTOR_YIELD_CURVE(CFG_YieldCurveLinebyLine);

static double sPrecision = 1e-10;

CFG_YieldCurveLinebyLine::CFG_YieldCurveLinebyLine(const SSYieldCurve& curve)
: CSRYieldCurve()
{	
	Initialize(curve, true, true);
	//DPH
	takeDateCurveForStartPoint = UpgradeExtension::TakeDateCurveForStartPoint();
};

void CFG_YieldCurveLinebyLine::Initialize(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
{
	InitialiseConstructorWithSpreadMgr(curve, computeZeroCouponYieldCurve, validationYieldCurve);
};

void CFG_YieldCurveLinebyLine::InitialiseConstructorWithSpreadMgr(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
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

void CFG_YieldCurveLinebyLine::InitialiseConstructor(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
{
	long oldDateFinBase;

	InitialiseArrayZeroCoupon();
	fStartPointDate			= 0;
	fShift					= 0;
	fYieldCurve				= curve;
	fCurrencyCode			= fYieldCurve.fCode;
	fShock					= 0;
	const CSRCalendar* pCalendar = CSRCurrency::GetCSRCurrency(fCurrencyCode);
	if(!pCalendar) return;

	//DPH
	//fZeroCouponArray		= new double [fYieldCurve.fPointCount];
	fZeroCouponArray = new double[fYieldCurve.fPoints.fPointCount];
	//fZeroCouponMaturity		= new long [fYieldCurve.fPointCount];
	fZeroCouponMaturity = new long[fYieldCurve.fPoints.fPointCount];
	
	long curveId = curve.fYieldCurveCode;
	std::map<long, ycPointsInfos> CFGYieldPointsMap = CFG_YCDataTable::LoadCFGYieldPoints(curveId);
	// save the spread type
	eYieldCurveSpreadType oldSpreadType = GetSpreadType();
	SetSpreadType(ycstUndefined);

	oldDateFinBase = gApplicationContext->GetDate();
	//DPH
	//if (CSRPreference::TakeDateCurveForStartPoint()
	if (UpgradeExtension::TakeDateCurveForStartPoint()
		&& fYieldCurve.fCurveDate < oldDateFinBase)
		gApplicationContext->SetGlobalDate(fYieldCurve.fCurveDate);
	
	map<long, double> mAllPoints;
	//DPH
	//for(int i = 0; i < fYieldCurve.fPointCount; i++)
	for (int i = 0; i < fYieldCurve.fPoints.fPointCount; i++)
	{
		//DPH
		//CFG_YieldCurveData* pYCData = dynamic_cast<CFG_YieldCurveData*>((fYieldCurve.fPointList[i]).fInfoPtr);
		/*CFG_YieldCurveData* pYCData = dynamic_cast<CFG_YieldCurveData*>((fYieldCurve.fPoints.fPointList[i]).fInfoPtr);
		if(!pYCData || !pYCData->fIsUsed)
			continue;
			*/
		auto iter = CFGYieldPointsMap.find((fYieldCurve.fPoints.fPointList[i]).fMaturity);
		if (iter != CFGYieldPointsMap.end())
		{
			if ((fYieldCurve.fPoints.fPointList[i]).fMaturity < gApplicationContext->GetDate())//yc point is disabled
			{
				continue;
			}

		//DPH
		//double yield = (fYieldCurve.fPointList[i]).fYield * 0.01;
		double yield = (fYieldCurve.fPoints.fPointList[i]).fYield * 0.01;
		//long maturity = (fYieldCurve.fPointList[i]).fMaturity - pYCData->fValueDate;
		//long maturity = (fYieldCurve.fPoints.fPointList[i]).fMaturity - pYCData->fValueDate;

		long maturity = (fYieldCurve.fPoints.fPointList[i]).fMaturity - CFGYieldPointsMap[(fYieldCurve.fPoints.fPointList[i]).fMaturity].valueDate;

		mAllPoints[maturity] = yield;
		}
	}

	fZeroCouponPointCount = 0;
	for(map<long, double>::iterator it = mAllPoints.begin(); it != mAllPoints.end(); it++)
	{
		fZeroCouponPointCount++;
		SetZeroCouponPoint(it->second, it->first);
	}

	gApplicationContext->SetGlobalDate(oldDateFinBase);

	// reload the spread type
	SetSpreadType(oldSpreadType);
};

double CFG_YieldCurveLinebyLine::GetInterpolatedZeroCouponRate( double ratePrevious, double rateNext, 
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
//double CFG_YieldCurveLinebyLine::CompoundFactor(double timeToMaturity, double overRate) const
double CFG_YieldCurveLinebyLine::GetCompoundFactor(double timeToMaturity, double overRate) const
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
		return pow(1.0 + overRate, timeToMaturity/365.25);

	//shift initialization
	//DPH
	//bool takeDateCurveForStartPoint = CSRPreference::TakeDateCurveForStartPoint();
	if(takeDateCurveForStartPoint && ( shift = date - fYieldCurve.fCurveDate) > 0)
	{
		if(fShift != shift)
		{
			//DPH
			//CSRPreference::SetTakeDateCurveForStartPoint(false);
			CFG_YieldCurveLinebyLine * changeMemberInConstMethodTrick = const_cast<CFG_YieldCurveLinebyLine*> (this);
			changeMemberInConstMethodTrick->takeDateCurveForStartPoint = false;
			//recalculating shift 
			double spreadToAdd = (GetSpreadType()==ycstZeroCoupon) ? -GetSpread() : 0;
			shiftzc = CompoundFactor(shift, spreadToAdd);
			
			//resetting variables
			//DPH
			//CSRPreference::SetTakeDateCurveForStartPoint(true);
			changeMemberInConstMethodTrick->takeDateCurveForStartPoint = true;
			fShift = shift;
			((CFG_YieldCurveLinebyLine*)(this))->InitialiseArrayZeroCoupon();
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

	long t = (long) timeToMaturity - shift;
	long timeToMaturity_1y = CFG_Maths::GetTimeToMaturity1y(calendar);
	eDayCountBasisType dcb = (t <= timeToMaturity_1y)? dcbActual_360 : dcb_Actual_Actual_AFB;
	eYieldCalculationType yc = (t <= timeToMaturity_1y)? ycLinear : ycActuarial;
	double cf = CFG_Maths::GetCompoundFactorFromZC(rate + overRate, t, dcb, yc, calendar);
	return cf;
};

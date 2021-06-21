#include "CFG_YieldCurveZCMAD.h"
#include "CFG_YieldCurveData.h"
#include "SphInc\market_data\SphMarketData.h"
#include "SphInc\static_data\SphCurrency.h"
#include "SphInc\static_data\SphYieldCurveFamily.h"
#include "SphInc\instrument\SphHandleError.h"
#include "SphInc\market_data\SphDataIntegrityManager.h"
#include "SphTools\SphLoggerUtil.h"
#pragma warning(push)
#pragma warning(disable:4103) //  '...' : alignment changed after including header, may be due to missing #pragma pack(pop)
#include __STL_INCLUDE_PATH(stdio.h)
#pragma warning(pop)

#include "UpgradeExtension.h"

WITHOUT_CONSTRUCTOR_YIELD_CURVE(CFG_YieldCurveZCMAD);

CFG_YieldCurveZCMAD::CFG_YieldCurveZCMAD(const SSYieldCurve& curve)
: CSRYieldCurveLinear()
{	
	Initialize(curve, true, true);
	//DPH
	takeDateCurveForStartPoint = UpgradeExtension::TakeDateCurveForStartPoint();
};

void CFG_YieldCurveZCMAD::Initialize(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
{
	InitialiseConstructorWithSpreadMgr(curve, computeZeroCouponYieldCurve, validationYieldCurve);
};

void CFG_YieldCurveZCMAD::InitialiseConstructorWithSpreadMgr(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
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
}

void CFG_YieldCurveZCMAD::InitialiseConstructor(const SSYieldCurve& curve, bool computeZeroCouponYieldCurve, bool validationYieldCurve)
{
	SSYieldPoint			*pYieldPoint;
	long					oldDateFinBase;

	fStartPointDate			= 0;
	fShift					= 0;
	fYieldCurve				= curve;
	fCurrencyCode			= fYieldCurve.fCode;
	fShock					= 0;
	//DPH
	//fZeroCouponArray		= new double [fYieldCurve.fPointCount];
	fZeroCouponArray = new double[fYieldCurve.fPoints.fPointCount];
	//fZeroCouponMaturity		= new long [fYieldCurve.fPointCount];
	fZeroCouponMaturity = new long[fYieldCurve.fPoints.fPointCount];

	// save the spread type
	eYieldCurveSpreadType oldSpreadType = GetSpreadType();
	SetSpreadType(ycstUndefined);

	oldDateFinBase = gApplicationContext->GetDate();
	//DPH
	//if(CSRPreference::TakeDateCurveForStartPoint()
	if (UpgradeExtension::TakeDateCurveForStartPoint()
		&& fYieldCurve.fCurveDate < oldDateFinBase)
		gApplicationContext->SetGlobalDate(fYieldCurve.fCurveDate);
	
	fZeroCouponPointCount = 0;
	const CSRCalendar* pCalendar = CSRCurrency::GetCSRCurrency(fCurrencyCode);
	//DPH
	//long baseFamily = CSRYieldCurveFamily::GetYieldCurveFamilyCode(fCurrencyCode, curve.fSpreadFamily);
	long baseFamily = CSRYieldCurveFamily::GetYieldCurveFamilyCode(fCurrencyCode, curve.fSpreadFamily.c_str());
	const CSRYieldCurve* pYCBase = CSRYieldCurve::GetInstanceByYieldCurveFamily(baseFamily);
	if(!pCalendar || !pYCBase) return;
	
	long startDate = gApplicationContext->GetDate();
	SSMaturity relMaturity;
	long timeToMaturity = 0L;
	long timeToMaturity_1y = CFG_Maths::GetTimeToMaturity1y(pCalendar);	

	// First, bootstrap from 1y to last maturity
	relMaturity.fType = 'y';
	//DPH
	//int lastMat = (int)fYieldCurve.fPointList[fYieldCurve.fPointCount - 1].fMaturity;
	int lastMat = (int)fYieldCurve.fPoints.fPointList[fYieldCurve.fPoints.fPointCount - 1].fMaturity;
	double* zcCalculated = new double[lastMat];
	double* dfCalculated = new double[lastMat];
	for(int j = 1; j <= lastMat; j++)
	{
		relMaturity.fMaturity = (long)j;
		timeToMaturity = SSMaturity::GetDayCount(relMaturity, startDate, NULL);

		double cf = pYCBase->CompoundFactor(timeToMaturity);
		double zc_pt_fixes_y = CFG_Maths::GetZCFromCompoundFactor(cf, timeToMaturity, dcb_Actual_Actual_AFB, ycActuarial, pCalendar); // in Act/Act Actuarial
		if(j == 1) // no calculation needed
		{
			zcCalculated[0] = zc_pt_fixes_y;
			dfCalculated[0] = 1.0 / cf;
		}
		else // j >=2
		{
			dfCalculated[j-1] = GetCalculatedDF(j, zc_pt_fixes_y, dfCalculated);
			zcCalculated[j-1] = CFG_Maths::GetZCFromCompoundFactor((1.0 / dfCalculated[j-1]), timeToMaturity, dcb_Actual_Actual_AFB, ycActuarial, pCalendar);
			
			//DEPRECATED: use closed formula instead
			//BootstrapFunction fToCompute(j, zc_pt_fixes_y, zcCalculated);
			//double zc_calc = 0.0;
			//CFG_Maths::BrentRootFinder(fToCompute, 0.0, 0.001, 0.1, 1e-15, zc_calc, 100, false, true);
			//zcCalculated[j-1] = zc_calc;
		}
	}

	int i = 0;
	//DPH
	//for(pYieldPoint=fYieldCurve.fPointList; i < fYieldCurve.fPointCount; pYieldPoint++, i++)
	for (pYieldPoint = fYieldCurve.fPoints.fPointList; i < fYieldCurve.fPoints.fPointCount; pYieldPoint++, i++)
	{
		if(!pYieldPoint->fInfoPtr->fIsUsed)
			continue;

		relMaturity.fMaturity = pYieldPoint->fMaturity;
		relMaturity.fType = pYieldPoint->fType;
		timeToMaturity = SSMaturity::GetDayCount(relMaturity, startDate, NULL);
		if(timeToMaturity < timeToMaturity_1y)
		{
			// Spread 0 over the base family, but zc rate in Act/Act Actuarial
			double cf = pYCBase->CompoundFactor(timeToMaturity);
			double zc = CFG_Maths::GetZCFromCompoundFactor(cf, timeToMaturity, dcb_Actual_Actual_AFB, ycActuarial, pCalendar);
			
			fZeroCouponPointCount++;
			SetZeroCouponPoint(zc, timeToMaturity);
		}
		else
		{
			int y = (int) pYieldPoint->fMaturity;
			if(relMaturity.fMaturity == 12L && relMaturity.fType == 'm')
				y = 1;

			fZeroCouponPointCount++;
			SetZeroCouponPoint(zcCalculated[y-1], timeToMaturity);			
		}
	}

	// reset original params
	gApplicationContext->SetGlobalDate(oldDateFinBase);
	SetSpreadType(oldSpreadType);
};

double CFG_YieldCurveZCMAD::GetInterpolatedZeroCouponRate( double ratePrevious, double rateNext, 
	double timeToMaturityPrevious, double timeToMaturityNext, double timeToMaturity) const
{
	double weight = (timeToMaturity - timeToMaturityPrevious) / (timeToMaturityNext - timeToMaturityPrevious);
	double interpolatedRate = ratePrevious + weight * (rateNext - ratePrevious);
	return interpolatedRate;
};

//DPH
//double CFG_YieldCurveZCMAD::CompoundFactor(double timeToMaturity, double overRate) const
double CFG_YieldCurveZCMAD::GetCompoundFactor(double timeToMaturity, double overRate) const
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

	//shift initialization
	//DPH
	//bool takeDateCurveForStartPoint = CSRPreference::TakeDateCurveForStartPoint();
	if(takeDateCurveForStartPoint && ( shift = date - fYieldCurve.fCurveDate) > 0)
	{
		if(fShift != shift)
		{
			//recalculating shift 
			//DPH
			//CSRPreference::SetTakeDateCurveForStartPoint(false);
			CFG_YieldCurveZCMAD * changeMemberInConstMethodTrick = const_cast<CFG_YieldCurveZCMAD*> (this);
			changeMemberInConstMethodTrick->takeDateCurveForStartPoint = false;
			double spreadToAdd = (GetSpreadType() == ycstZeroCoupon) ? -GetSpread() : 0;
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
	else // 2 points or more, need interpolation
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
				timeToMaturity);
		}
		else
		{
			rate = GetInterpolatedZeroCouponRate(
				fZeroCouponArray[i-1], 
				fZeroCouponArray[i], 
				mat[-1], 
				mat[0], 
				timeToMaturity);
		}
	}

	if (fShock)
		overRate += fShock->GetShock(timeToMaturity);

	double cf = CFG_Maths::GetCompoundFactorFromZC(rate + overRate, (long)timeToMaturity - shift, dcb_Actual_Actual_AFB, ycActuarial, calendar);
	return cf;
};

double CFG_YieldCurveZCMAD::GetCalculatedDF(int y, double zc_y, double* dfArray) const
{
	double temp = 0.0;
	for(int i = 1; i < y; i++)
		temp += dfArray[i-1];

	double df = 1.0 - zc_y * temp;
	df /= (1.0 + zc_y);
	return df;
};

BootstrapFunction::BootstrapFunction(int which, double zc_which, double* zcArray)
: CalcFunction()
{
	nMat = which;
	zc_n = zc_which;
	zcCalculated = zcArray;
};

double BootstrapFunction::f(double x)
{
	double temp = 0.0;
	for(int i = 1; i < nMat; i++)
	{
		temp += (zc_n / pow(1.0+zcCalculated[i-1], i));
	}
	temp += ((1.0 + zc_n) / pow(1.0+x, nMat));
	return temp - 1.0;
};

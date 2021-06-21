#pragma warning(disable:4251)

/*
** Includes
*/
#include "SphInc/SphMacros.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphTools/SphLoggerUtil.h"

#include "CSxZCYieldCurveSimulation.h"

//DPH
#include "UpgradeExtension.h"

/*
** Static
*/
const char * CSxZCYieldCurveSimulation::__CLASS__ = "CSxZCYieldCurveSimulation";


WITHOUT_CONSTRUCTOR_YIELD_CURVE(CSxZCYieldCurveSimulation);

//------------------------------------------------------------------------------------------------------------------------------------
CSxZCYieldCurveSimulation::CSxZCYieldCurveSimulation(const SSYieldCurve& curve)
	: CSRYieldCurveLinear()
{	
	Initialize(curve, true, true);
	//DPH
	takeDateCurveForStartPoint = UpgradeExtension::TakeDateCurveForStartPoint();
}

//------------------------------------------------------------------------------------------------------------------------------------
void CSxZCYieldCurveSimulation::Initialize(	const SSYieldCurve& curve, 
											bool computeZeroCouponYieldCurve, 
											bool validationYieldCurve)
{
	InitialiseConstructorWithSpreadMgr(curve, computeZeroCouponYieldCurve, validationYieldCurve);
}

//------------------------------------------------------------------------------------------------------------------------------------
void CSxZCYieldCurveSimulation::InitialiseConstructorWithSpreadMgr(	const SSYieldCurve& curve, 
																	bool computeZeroCouponYieldCurve, 
																	bool validationYieldCurve)
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

//------------------------------------------------------------------------------------------------------------------------------------
void CSxZCYieldCurveSimulation::InitialiseConstructor(	const SSYieldCurve& curve, 
														bool computeZeroCouponYieldCurve, 
														bool validationYieldCurve)
{
	//DPH
	//fZeroCouponArray		= new double [fYieldCurve.fPointCount];
	//fZeroCouponMaturity		= new long [fYieldCurve.fPointCount];
	fZeroCouponArray = new double[fYieldCurve.fPoints.fPointCount];
	fZeroCouponMaturity = new long[fYieldCurve.fPoints.fPointCount];
	fZeroCouponPointCount	= 0;
	fStartPointDate			= 0;
	fShift					= 0;
	fYieldCurve				= curve;
	fCurrencyCode			= fYieldCurve.fCode;
	fShock					= 0;
	const CSRCalendar* pCcy = CSRCurrency::GetCSRCurrency(fCurrencyCode);
	if(!pCcy) return;	

	long oldAppDate = gApplicationContext->GetDate();
	//DPH
	//if(CSRPreference::TakeDateCurveForStartPoint() && fYieldCurve.fCurveDate < oldAppDate)
	if (takeDateCurveForStartPoint && fYieldCurve.fCurveDate < oldAppDate)
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
		//mat.fType = (fYieldCurve.fPointList[i]).fType;
		mat.fMaturity = (fYieldCurve.fPoints.fPointList[i]).fMaturity;
		mat.fType = (fYieldCurve.fPoints.fPointList[i]).fType;
		long maturity = SSMaturity::GetDayCount(mat, today, NULL);		

		fZeroCouponPointCount++;

		if (mat.fType != 'f')
		{
			//DPH
			//SetZeroCouponPoint((fYieldCurve.fPointList[i]).fYield, maturity);
			SetZeroCouponPoint((fYieldCurve.fPoints.fPointList[i]).fYield, maturity);
		}
		else
		{
			//DPH
			//SetZeroCouponPoint((fYieldCurve.fPointList[i]).fYield*.01, maturity);
			SetZeroCouponPoint((fYieldCurve.fPoints.fPointList[i]).fYield*.01, maturity);
		}
	}

	gApplicationContext->SetGlobalDate(oldAppDate);
}

//------------------------------------------------------------------------------------------------------------------------------------
double CSxZCYieldCurveSimulation::GetInterpolatedZeroCouponRate(	double ratePrevious, 
																	double rateNext, 
																	double timeToMaturityPrevious, 
																	double timeToMaturityNext, 
																	double timeToMaturity) const
{
	double weight = (timeToMaturity - timeToMaturityPrevious) / (timeToMaturityNext - timeToMaturityPrevious);
	double interpolatedRate = ratePrevious + weight * (rateNext - ratePrevious);
	return interpolatedRate;
}

//------------------------------------------------------------------------------------------------------------------------------------
//DPH
//double CSxZCYieldCurveSimulation::CompoundFactor(double timeToMaturity, double overRate) const
double CSxZCYieldCurveSimulation::GetCompoundFactor(double timeToMaturity, double overRate) const
{
	BEGIN_LOG("CompoundFactor");

	const CSRSpreadCurveMgr* mgr = GetCSRSpreadCurveMgr();
	if (mgr)
	{
		END_LOG();
		return mgr->CompoundFactor(this, timeToMaturity, overRate);
	}

	long appDate = gApplicationContext->GetDate();
	const long * mat;
	int i, shift;
	double rate, shiftzc;
	long date = fStartPointDate ? fStartPointDate : appDate;

	if (GetSpreadType() == ycstZeroCoupon)
		overRate += GetSpread();

	if(timeToMaturity <= 0) // negative maturity
	{
		END_LOG();
		return 1.0;
	}

	if(fZeroCouponPointCount == 0) // empty curve
	{
		END_LOG();
		return pow(1.0 + overRate, timeToMaturity / 365.25);
	}

	//shift initialization
	//DPH
	//bool takeDateCurveForStartPoint = CSRPreference::TakeDateCurveForStartPoint();
	if (takeDateCurveForStartPoint && (shift = date - fYieldCurve.fCurveDate) > 0)
	{
		if(fShift != shift)
		{
			//recalculating shift 
			//DPH
			//CSRPreference::SetTakeDateCurveForStartPoint(false);
			CSxZCYieldCurveSimulation * changeMemberInConstMethodTrick = const_cast<CSxZCYieldCurveSimulation*> (this);
			changeMemberInConstMethodTrick->takeDateCurveForStartPoint = false;
			double spreadToAdd = (GetSpreadType()==ycstZeroCoupon) ? -GetSpread() : 0;
			//DPH
			shiftzc = CompoundFactor(shift, spreadToAdd);
			//shiftzc = GetCompoundFactor(shift, spreadToAdd);

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

	if (fShock)
	{
		overRate += fShock->GetShock(timeToMaturity);	
	}

	// Bool to check if we must force the short term convention
	bool forceShort = false;

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
		else 
		{
			const CSRCalendar * calendar = CSRCurrency::GetCSRCurrency(fCurrencyCode);
			long maturity1y = GetTimeToMaturity1y(calendar);
			long mat1 = 0, mat2 = 0;
			double rate1 = 0.0, rate2 = 0.0;

			if(i == fZeroCouponPointCount) // after last point
			{
				rate1 = fZeroCouponArray[fZeroCouponPointCount-2];
				mat1 = mat[-2];
				rate2 = fZeroCouponArray[fZeroCouponPointCount-1];
				mat2 = mat[-1];
			}
			else
			{
				rate1 = fZeroCouponArray[i-1];
				mat1 = mat[-1];
				rate2 = fZeroCouponArray[i];
				mat2 = mat[0];
			}

			if (mat1 <= maturity1y && mat2 > maturity1y && timeToMaturity <= maturity1y)
			{
				// First point before 1y, second point after 1y and maturity before 1y => Force rate2 to short term
				// Convert the long term rate to short term rate
				MESS(Log::debug, "Force Short Term Parameter for Rate 2");
				rate2 = ConvertLongToShort(rate2, mat2);
				forceShort = true;
			}
			else if (mat1 <= maturity1y && mat2 > maturity1y && timeToMaturity > maturity1y)
			{
				// First point before 1y, second point after 1y and maturity after 1y => Force rate1 to long term
				// Convert the long term rate to short term rate
				MESS(Log::debug, "Force Long Term Parameter for Rate 1");
				rate1 = ConvertShortToLong(rate1, mat1);
				forceShort = false;
			}
			else if (mat2 <= maturity1y && mat1 > maturity1y && timeToMaturity <= maturity1y)
			{
				// Second point before 1y, first point after 1y and maturity before 1y => Force rate1 to short term
				// Convert the long term rate to short term rate
				MESS(Log::debug, "Weird : Force Short Term Parameter for Rate 1");
				rate1 = ConvertLongToShort(rate1, mat1);
				forceShort = false;
			}
			else if (mat2 <= maturity1y && mat1 > maturity1y && timeToMaturity > maturity1y)
			{
				// Second point before 1y, first point after 1y and maturity after 1y => Force rate2 to long term
				// Convert the short term rate to long term rate
				MESS(Log::debug, "Weird : Force Long Term Parameter for Rate 2");
				rate2 = ConvertShortToLong(rate2, mat2);
				forceShort = false;
			}
	
			rate = GetInterpolatedZeroCouponRate(	rate1,
													rate2, 
													mat1, 
													mat2, 
													timeToMaturity);
		}
	}

	//rate += overRate;

	MESS(Log::debug, "Interpolated Rate " << rate << " (OverRate " << overRate << ", Shift " << shift << ")");
	double cf = GetCouponRate(rate + overRate, timeToMaturity, shift, forceShort);

	END_LOG();
	return cf;
}

//------------------------------------------------------------------------------------------------------------------------------------
long CSxZCYieldCurveSimulation::GetTimeToMaturity1y(const CSRCalendar * calendar) const
{
	long today = gApplicationContext->GetDate();
	SSMaturity mat_1y;
	mat_1y.fMaturity = 1;
	mat_1y.fType = 'y';
	long timeToMaturity_1y = SSMaturity::GetDayCount(mat_1y, today, calendar);
	return timeToMaturity_1y;
};

//------------------------------------------------------------------------------------------------------------------------------------
double CSxZCYieldCurveSimulation::GetCouponRate(double rate, double timeToMaturity, int shift, bool forceShort /* = false */) const
{
	BEGIN_LOG("GetCouponRate");

	long today = gApplicationContext->GetDate();

	const CSRCalendar * calendar = CSRCurrency::GetCSRCurrency(fCurrencyCode);
	long maturity1 = GetTimeToMaturity1y(calendar);
	
	eDayCountBasisType dayCountBasisType = dcb_Actual_Actual_AFB;
	eYieldCalculationType yieldCalculationType = ycActuarial;
	
	long baseRateId = 0;
	
	if (timeToMaturity <= maturity1 || forceShort)
	{
		baseRateId = fYieldCurve.fShortTermRate;
		MESS(Log::debug, "Short Term: " << baseRateId << " (" << timeToMaturity << "<=" << maturity1 << ")");
	}
	else
	{
		baseRateId = fYieldCurve.fLongTermRate;
		MESS(Log::debug, "Long Term: " << baseRateId << " (" << timeToMaturity << ">"  << maturity1 << ")");
	}

	const CSRInterestRate * currentInterestRate = dynamic_cast<const CSRInterestRate *>(CSRInstrument::GetInstance(baseRateId));
	if (currentInterestRate)
	{
		dayCountBasisType = currentInterestRate->GetDayCountBasisType();
		yieldCalculationType = currentInterestRate->GetYieldCalculationType();
		//DPH
		//MESS(Log::debug, "Day Count Basis " << dayCountBasisType << ", Yield Calculation " << yieldCalculationType << ", Instrument Date " << gApplicationContext->GetInstrumentDate());
		MESS(Log::debug, "Day Count Basis " << dayCountBasisType << ", Yield Calculation " << yieldCalculationType << ", Instrument Date " << gApplicationContext->GetInstrumentDate(GetCurveCode()));
	}
	else
	{
		MESS(Log::warning, "Failed to find rate " << baseRateId);
	}

	const CSRDayCountBasis * currentDayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis(dayCountBasisType);
	const CSRYieldCalculation * currentYieldCalculation = CSRYieldCalculation::GetCSRYieldCalculation(yieldCalculationType);


	double dt = currentDayCountBasis->GetEquivalentYearCount(today, today + (long)timeToMaturity - shift, SSDayCountCalculation(calendar));
	double cf = 1.0 + currentYieldCalculation->GetCouponRate(rate, dt);
	MESS(Log::debug, "CF " << cf << ", Rate " << rate);

	END_LOG();
	return cf;
}

//------------------------------------------------------------------------------------------------------------------------------
double CSxZCYieldCurveSimulation::ConvertShortToLong(double rateShort, double timeToMaturity) const
{
	BEGIN_LOG("ConvertShortToLong");
	long today = gApplicationContext->GetDate();

	const CSRCalendar * calendar = CSRCurrency::GetCSRCurrency(fCurrencyCode);
	long maturity1y = GetTimeToMaturity1y(calendar);

	eDayCountBasisType dayCountBasisTypeShort = dcb_Actual_Actual_AFB;
	eDayCountBasisType dayCountBasisTypeLong = dcb_Actual_Actual_AFB;
	eYieldCalculationType yieldCalculationTypeShort = ycActuarial;
	eYieldCalculationType yieldCalculationTypeLong = ycActuarial;

	const CSRInterestRate * currentInterestRate = dynamic_cast<const CSRInterestRate *>(CSRInstrument::GetInstance(fYieldCurve.fShortTermRate));
	if (currentInterestRate)
	{
		dayCountBasisTypeShort = currentInterestRate->GetDayCountBasisType();
		yieldCalculationTypeShort = currentInterestRate->GetYieldCalculationType();
		//DPH
		//MESS(Log::debug, "Short - Day Count Basis " << dayCountBasisTypeShort << ", Yield Calculation " << yieldCalculationTypeShort << ", Instrument Date " << gApplicationContext->GetInstrumentDate());
		MESS(Log::debug, "Short - Day Count Basis " << dayCountBasisTypeShort << ", Yield Calculation " << yieldCalculationTypeShort << ", Instrument Date " << gApplicationContext->GetInstrumentDate(GetCurveCode()));
	}

	currentInterestRate = dynamic_cast<const CSRInterestRate *>(CSRInstrument::GetInstance(fYieldCurve.fLongTermRate));
	if (currentInterestRate)
	{
		dayCountBasisTypeLong = currentInterestRate->GetDayCountBasisType();
		yieldCalculationTypeLong = currentInterestRate->GetYieldCalculationType();
		//DPH
		//MESS(Log::debug, "Long - Day Count Basis " << dayCountBasisTypeLong << ", Yield Calculation " << yieldCalculationTypeLong << ", Instrument Date " << gApplicationContext->GetInstrumentDate());
		MESS(Log::debug, "Long - Day Count Basis " << dayCountBasisTypeLong << ", Yield Calculation " << yieldCalculationTypeLong << ", Instrument Date " << gApplicationContext->GetInstrumentDate(GetCurveCode()));
	}

	const CSRDayCountBasis * currentDayCountBasisShort = CSRDayCountBasis::GetCSRDayCountBasis(dayCountBasisTypeShort);
	const CSRDayCountBasis * currentDayCountBasisLong = CSRDayCountBasis::GetCSRDayCountBasis(dayCountBasisTypeLong);
	const CSRYieldCalculation * currentYieldCalculationShort = CSRYieldCalculation::GetCSRYieldCalculation(yieldCalculationTypeShort);
	const CSRYieldCalculation * currentYieldCalculationLong = CSRYieldCalculation::GetCSRYieldCalculation(yieldCalculationTypeLong);

	double dtShort  = currentDayCountBasisShort->GetEquivalentYearCount(today, today + (long)timeToMaturity, SSDayCountCalculation(calendar));
	double dtLong   = currentDayCountBasisLong->GetEquivalentYearCount (today, today + (long)timeToMaturity, SSDayCountCalculation(calendar));
	double cfShort  = 1.0 + currentYieldCalculationShort->GetCouponRate(rateShort, dtShort);
	double rateLong =       currentYieldCalculationLong->GetRate(cfShort - 1.0, dtLong);

	MESS(Log::debug, "CF Short " << cfShort << ", Rate Short " << rateShort << ", Rate Long " << rateLong << ", Date " << timeToMaturity);
	END_LOG();
	return rateLong;
}

//----------------------------------------------------------------------------------------------------------
double CSxZCYieldCurveSimulation::ConvertLongToShort(double rateLong, double timeToMaturity) const
{
	BEGIN_LOG("ConvertLongToShort");
	long today = gApplicationContext->GetDate();

	const CSRCalendar * calendar = CSRCurrency::GetCSRCurrency(fCurrencyCode);
	long maturity1y = GetTimeToMaturity1y(calendar);

	eDayCountBasisType dayCountBasisTypeShort = dcb_Actual_Actual_AFB;
	eDayCountBasisType dayCountBasisTypeLong = dcb_Actual_Actual_AFB;
	eYieldCalculationType yieldCalculationTypeShort = ycActuarial;
	eYieldCalculationType yieldCalculationTypeLong = ycActuarial;

	const CSRInterestRate * currentInterestRate = dynamic_cast<const CSRInterestRate *>(CSRInstrument::GetInstance(fYieldCurve.fShortTermRate));
	if (currentInterestRate)
	{
		dayCountBasisTypeShort = currentInterestRate->GetDayCountBasisType();
		yieldCalculationTypeShort = currentInterestRate->GetYieldCalculationType();
		//DPH
		//MESS(Log::debug, "Short - Day Count Basis " << dayCountBasisTypeShort << ", Yield Calculation " << yieldCalculationTypeShort << ", Instrument Date " << gApplicationContext->GetInstrumentDate());
		MESS(Log::debug, "Short - Day Count Basis " << dayCountBasisTypeShort << ", Yield Calculation " << yieldCalculationTypeShort << ", Instrument Date " << gApplicationContext->GetInstrumentDate(GetCurveCode()));
	}

	currentInterestRate = dynamic_cast<const CSRInterestRate *>(CSRInstrument::GetInstance(fYieldCurve.fLongTermRate));
	if (currentInterestRate)
	{
		dayCountBasisTypeLong = currentInterestRate->GetDayCountBasisType();
		yieldCalculationTypeLong = currentInterestRate->GetYieldCalculationType();
		//DPH
		//MESS(Log::debug, "Long - Day Count Basis " << dayCountBasisTypeLong << ", Yield Calculation " << yieldCalculationTypeLong << ", Instrument Date " << gApplicationContext->GetInstrumentDate());
		MESS(Log::debug, "Long - Day Count Basis " << dayCountBasisTypeLong << ", Yield Calculation " << yieldCalculationTypeLong << ", Instrument Date " << gApplicationContext->GetInstrumentDate(GetCurveCode()));
	}

	const CSRDayCountBasis * currentDayCountBasisShort = CSRDayCountBasis::GetCSRDayCountBasis(dayCountBasisTypeShort);
	const CSRDayCountBasis * currentDayCountBasisLong = CSRDayCountBasis::GetCSRDayCountBasis(dayCountBasisTypeLong);
	const CSRYieldCalculation * currentYieldCalculationShort = CSRYieldCalculation::GetCSRYieldCalculation(yieldCalculationTypeShort);
	const CSRYieldCalculation * currentYieldCalculationLong = CSRYieldCalculation::GetCSRYieldCalculation(yieldCalculationTypeLong);

	double dtShort  = currentDayCountBasisShort->GetEquivalentYearCount(today, today + (long)timeToMaturity, SSDayCountCalculation(calendar));
	double dtLong   = currentDayCountBasisLong->GetEquivalentYearCount (today, today + (long)timeToMaturity, SSDayCountCalculation(calendar));
	double cfLong  = 1.0 + currentYieldCalculationLong->GetCouponRate(rateLong, dtLong);
	double rateShort =       currentYieldCalculationShort->GetRate(cfLong - 1.0, dtShort);

	MESS(Log::debug, "CF Long " << cfLong << ", Rate Long " << rateLong << ", Rate Short " << rateShort << ", Date " << timeToMaturity);
	END_LOG();
	return rateShort;
}
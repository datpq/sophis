#pragma warning(disable:4251)
/*
** Includes
*/
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/scenario/SphMarketDataOverloader.h"
#include "SphInc/market_data/SphYieldCurve.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/misc/SphGlobalFunctions.h"
#include "SphInc/static_data/SphInterestRate.h"

#include "CSxTools.h"
#include "CSxZCYieldCurveSimulation.h"
#include "CSxMarketDataOverloader.h"

/*
** Namespace
*/
using namespace sophis::market_data;
using namespace sophis::static_data;
using namespace sophis::scenario;
using namespace sophis::misc;

/*
** Static
*/
/*static*/ const char* CSxMarketDataOverloader::__CLASS__ = "CSxMarketDataOverloader";

/*
** Methods
*/
CSxMarketDataOverloader::CSxMarketDataOverloader(	const market_data::CSRMarketData& context, 
													_STL::vector<_STL::string>& maturitiesList, 
													_STL::vector<double>& ratesList,
													long shortTermRate,
													long longTermRate) 
	: CSRMarketDataOverloader(context)
{
	// Initialise yield curve
	fMaturitiesList = maturitiesList;
	fRatesList = ratesList;
	fShortTermRate = shortTermRate;
	fLongTermRate = longTermRate;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ CSxMarketDataOverloader::~CSxMarketDataOverloader()
{
	for (unsigned int i = 0; i < fYieldCurveToDestroyVector.size(); i++)
	{
		delete fYieldCurveToDestroyVector[i];
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	const market_data::CSRYieldCurve *	CSxMarketDataOverloader::GetCSRYieldCurve(long currency) const
{
	BEGIN_LOG("GetCSRYieldCurve");

	const CSRYieldCurve* yc = CSRMarketDataOverloader::GetCSRYieldCurve(currency);

	int nbPoints = (int)fMaturitiesList.size();

	if (yc && nbPoints > 0) //overload ZC yield curve
	{													
		//Use a map to sort the list of maturity points
		_STL::map<long,SSYieldPoint> yieldPointsMap;
		_STL::map<long,SSYieldPoint>::iterator yieldPointsMapIter;
		long today = gApplicationContext->GetDate();
		
		for (int i = 0; i < nbPoints; i++)
		{
			SSYieldPoint yieldPoint;
			
			char maturityType = fMaturitiesList[i][fMaturitiesList[i].length()-1];			

			bool isAValidMaturity = false;

			if (maturityType == 'd' || maturityType == 'm' || maturityType == 'y')
			{
				yieldPoint.fType = maturityType;
				_STL::string maturityStr = fMaturitiesList[i].substr(0,fMaturitiesList[i].length()-1);
				yieldPoint.fMaturity = atol(maturityStr.c_str());
				yieldPoint.fYield = fRatesList[i]*.01;

				if (yieldPoint.fMaturity)
				{
					isAValidMaturity = true;
				}
			}
			else //Test if the maturity is an absolute date (at format DD/MM/YYYY)
			{
				long maturityDate = CSxTools::DDMMYYYYDateToNum(fMaturitiesList[i]);

				if (maturityDate > 0)
				{
					yieldPoint.fType = 'f';
					yieldPoint.fMaturity = maturityDate;
					yieldPoint.fYield = fRatesList[i];
					isAValidMaturity = true;
				}				
			}

			if (!isAValidMaturity)
			{
				_STL::string mess = FROM_STREAM("Maturity (" << fMaturitiesList[i] << ") is invalid");
				RunTimeFailure runTimeFailureException(mess.c_str());			
				throw runTimeFailureException;
			}			

			SSMaturity mat;
			mat.fMaturity = yieldPoint.fMaturity;
			mat.fType = yieldPoint.fType;
			long maturity = SSMaturity::GetDayCount(mat, today, NULL);
			yieldPointsMapIter = yieldPointsMap.find(maturity);
			
			if (yieldPointsMapIter == yieldPointsMap.end())
			{
				yieldPointsMap[maturity] = yieldPoint;
			}
			else
			{
				_STL::string mess = FROM_STREAM("Maturity (" << fMaturitiesList[i] << ") is defined twice in the list of maturities");
				RunTimeFailure runTimeFailureException(mess.c_str());			
				throw runTimeFailureException;
			}			
		}
		
		//Build the SSYield curve
		SSYieldCurve ssYieldCurve;

		ssYieldCurve.fCode = (yc->GetSSYieldCurve())->fCode;
		ssYieldCurve.SetModelName(ZCYieldCurveSimulationModelName);
		ssYieldCurve.InitPointList((int)yieldPointsMap.size());
		ssYieldCurve.fShortTermRate = fShortTermRate;
		ssYieldCurve.fLongTermRate = fLongTermRate;

		//Maps elements are sorted by their key		
		int i = 0;
		for (yieldPointsMapIter = yieldPointsMap.begin(); yieldPointsMapIter != yieldPointsMap.end(); yieldPointsMapIter++)
		{
			//DPH
			//ssYieldCurve.fPointList[i] = yieldPointsMapIter->second;
			//ssYieldCurve.fPointList[i].SetInfo(gGlobalFunctions->new_YCRateInfoSup(),false);
			ssYieldCurve.fPoints.fPointList[i] = yieldPointsMapIter->second;
			ssYieldCurve.fPoints.fPointList[i].SetInfo(gGlobalFunctions->new_YCRateInfoSup(), false);
			i++;
		}
		
		//Build the CSRYieldCurve
		CSRYieldCurve* overloadedYieldCurve = CSRYieldCurve::new_CSRYieldCurveFromAPI(ssYieldCurve, ZCYieldCurveSimulationModelName);
		if (!overloadedYieldCurve)
		{			
			RunTimeFailure runTimeFailureException("Unable to create a new yield curve with model \"CFG ZC Simulation\"");			
			throw runTimeFailureException;
		}

		fYieldCurveToDestroyVector.push_back(overloadedYieldCurve); //To avoid memory leaks				

		yc = overloadedYieldCurve;
		MESS(Log::debug, "Use Toolkit Yield Curve for currency " << currency);
	}
	else
	{
		MESS(Log::debug, "Use Standard Yield Curve for currency " << currency);
	}

	END_LOG();
	return yc;
}

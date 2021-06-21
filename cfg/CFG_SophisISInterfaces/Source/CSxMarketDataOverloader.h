#ifndef __CSxMarketDataOverloader_H__
#define __CSxMarketDataOverloader_H__

#include "SphInc/SphMacros.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)
#include "SphInc/scenario/SphMarketDataOverloader.h"
#include "SphInc/market_data/SphYieldCurve.h"

class CSxMarketDataOverloader : public sophis::scenario::CSRMarketDataOverloader
{	
public :			

	CSxMarketDataOverloader(const market_data::CSRMarketData& context, 
							_STL::vector<_STL::string>& maturitiesList, 
							_STL::vector<double>& ratesList,
							long shortTermRate,
							long longTermRate);

	virtual ~CSxMarketDataOverloader();

	virtual	const market_data::CSRYieldCurve * GetCSRYieldCurve(long currency) const;


private:
	static const char* __CLASS__;

	_STL::vector<_STL::string> fMaturitiesList;
	_STL::vector<double> fRatesList;
	mutable _STL::vector<market_data::CSRYieldCurve *> fYieldCurveToDestroyVector;
	long fShortTermRate;
	long fLongTermRate;
};

#endif //__CSxMarketDataOverloader_H__

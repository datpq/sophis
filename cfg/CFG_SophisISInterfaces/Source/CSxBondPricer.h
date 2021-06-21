#ifndef __CSxBondPricer_H__
#define __CSxBondPricer_H__

#include "SphInc/SphMacros.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)
#include "SphInc/instrument/SphBond.h"

#define  SIZE_MESSAGE	1024

class CSxBondPricer
{	
public :			

	static CSxBondPricer* GetInstance();	
	
	void ComputeTheoretical(const char* instrRef, long date, const char* curveFamilyName, int pricingModel, int quotationType, double spread,
							_STL::vector<_STL::string>& maturitiesList, _STL::vector<double>& ratesList, double& price, double& ytm, double& accrued, 
							double& duration, double& sensitivity);
	
	double GetPriceFromYTM(const char* instrRef, long date, int pricingModel, int quotationType, double ytm);
	double GetYTMFromPrice(const char* instrRef, long date, int pricingModel, int quotationType, double price);

	double GetSensitivity(long instrumentCode, const char* sensitivityUserColumnName);
	//DPH
	//double GetSensitivity(const sophis::instrument::CSRInstrument* instr, double dirtyPrice, const sophis::market_data::CSRMarketData& context);
	double GetSensitivity(const sophis::instrument::ISRInstrument* instr, double dirtyPrice, const sophis::market_data::CSRMarketData& context);
	void SetPricesDate(long date);


private:
	static const char* __CLASS__;
	static CSxBondPricer* fInstance;

	CSxBondPricer();
};

#endif //__CSxBondPricer_H__

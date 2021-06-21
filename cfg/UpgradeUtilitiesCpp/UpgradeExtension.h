#pragma once
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/instrument/SphBond.h"
#include "SphInc/finance/SphDefaultPricerBond.h"
#include "SphInc/market_data/SphMarketData.h"

using namespace sophis;
using namespace sophis::instrument;

class UpgradeExtension
{
public:
	static void SetYTM(const sophis::instrument::ISRInstrument* instr, double ytm);
	static int GetRhoCount(const sophis::instrument::ISRInstrument* instr);
	static double GetRho(const sophis::instrument::ISRInstrument* instr, const sophis::market_data::CSRMarketData& context, long whichUnderlying);
	//static long GetUnderlying(const sophis::instrument::ISRInstrument* instr, int whichUnderlying);
	static double GetFloatingNotionalFactor(const sophis::instrument::ISRInstrument* instr);
	static double GetTheoreticalValue(const sophis::instrument::CSRInstrument* instr);
	static double GetTheoreticalValue(const sophis::instrument::CSRInstrument* instr, const market_data::CSRMarketData& context);
	static bool TakeDateCurveForStartPoint();
private:
	static const char* __CLASS__;
};

#define SEARCH_MAP_VALUE(keyType, valType) \
std::map<keyType, valType>::iterator searchMapValue(std::map<keyType, valType> & mapObj, valType val) \
{	std::map<keyType, valType>::iterator it = mapObj.begin(); \
	while (it != mapObj.end()) { if (it->second == val) return it; it++; } \
}

class EmcPricerBond : public virtual sophis::finance::CSRDefaultPricerBond
{
public:
	DECLARATION_BOND_META_MODEL(EmcPricerBond);

	virtual double	ComputeNonFlatCurvePrice(const instrument::CSRInstrument					&instr,
		long											transactionDate,
		long											settlementDate,
		long											ownershipDate,
		const market_data::CSRMarketData				&param,
		double											*derivative,
		const static_data::CSRDayCountBasis				*base,
		short											adjustedDates,
		short											valueDates,
		const static_data::CSRDayCountBasis				*dayCountBasis,
		const static_data::CSRYieldCalculation			*yieldCalculation,
		std::vector<instrument::SSRedemption>			*redemptionArray,
		std::vector<instrument::SSBondExplication>		*explicationArray,
		bool											withSpreadMgt,
		bool											throwException) const;
private:
	static const char* __CLASS__;
};

class EmcBond : public virtual sophis::instrument::CSRBond
{
public:
	DECLARATION_BOND(EmcBond);

	virtual double	EmcComputeNonFlatCurvePrice(long	transactionDate,
		long	settlementDate,
		long	ownershipDate,
		const market_data::CSRMarketData		&param,
		double									*derivee,
		const static_data::CSRDayCountBasis		*base,
		short									adjustedDates,
		short									valueDates,
		const static_data::CSRDayCountBasis		*dayCountBasis,
		const static_data::CSRYieldCalculation	*yieldCalculation,
		_STL::vector<SSRedemption>				*redemptionArray,
		_STL::vector<SSBondExplication>			*explicationArray,
		bool									withSpreadMgt,
		bool									throwException) const = 0;
private:
	static const char* __CLASS__;
};

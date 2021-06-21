#include "UpgradeExtension.h"
#include "SphInc\SphComputationResults.h"
#include "SphInc\SphComputationResults_Global.h"
#include "SphInc\instrument\SphInstrument.h"
#include "SphInc\finance\SphPricer.h"
#include "SphTools/SphLoggerUtil.h"

using namespace sophis;
using namespace sophisTools::base;
using namespace sophis::market_data;
using namespace sophis::instrument;
using namespace finance;

const char*  UpgradeExtension::__CLASS__ = "UpgradeExtension";
const char*  EmcPricerBond::__CLASS__ = "EmcPricerBond";
const char*  EmcBond::__CLASS__ = "EmcBond";

SEARCH_MAP_VALUE(CSRRiskSourceDelta, double)
SEARCH_MAP_VALUE(CSRRiskSourceRho, double)

void UpgradeExtension::SetYTM(const ISRInstrument* instr, double ytm)
{
	BEGIN_LOG("SetYTM");
	try
	{
		MESS(Log::debug, FROM_STREAM("SetYTM.BEGIN(instr=" << instr->GetCode() << ", ytm=" << ytm << ")"));

		//CSRPricer * pricer;
		//*pricer = CSRPricer::GetPricerByPriority(*instr).model;
		//CSRMarketData * marketData = CSRMarketData::GetCurrentMarketData();
		//CSRComputationResults *computationResults;

		//MESS(Log::debug, FROM_STREAM("ComputeAll.BEGIN"));
		//pricer->ComputeAll(*instr, *marketData, *computationResults);
		//MESS(Log::debug, FROM_STREAM("ComputeAll.END"));

		//Case number 02180325
		sophis::CSRGlobalComputationKey key(instr->GetCode());
		sophis::CSRComputationResults &computationResults = CSRComputationResults_Global::GetInstance().GetOrCreate(key);
		computationResults.YTMSensitivity() = ytm;

		//sophis::CSRComputationResults fResultss;
		//instr.GetDefaultPricer()->GetRiskSources(instr, fResultss);
		//fResultss.YTM = ytm;

		MESS(Log::debug, FROM_STREAM("SetYTM.END"));
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to SetYTM: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
}

int UpgradeExtension::GetRhoCount(const ISRInstrument* instr)
{
	BEGIN_LOG("GetRhoCount");
	long result = 0;
	try
	{
		MESS(Log::debug, FROM_STREAM("BEGIN(instr=" << instr->GetCode() << ")"));

		CSRMarketData * marketData = CSRMarketData::GetCurrentMarketData();
		const CSRPricer * pricer = instr->GetDefaultPricer();
		CSRComputationResults computationResults;

		MESS(Log::debug, FROM_STREAM("ComputeAll.BEGIN"));
		pricer->ComputeAll(*instr, *marketData, computationResults);
		MESS(Log::debug, FROM_STREAM("ComputeAll.END"));

		result = computationResults.Rho().size();
		//sophis::CSRComputationResults fResultss;
		//instr->GetDefaultPricer()->GetRiskSources(*instr, fResultss);
		//return fResultss.Rho().size();

		MESS(Log::debug, FROM_STREAM("END(result=" << result << ")"));
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to GetRhoCount: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}

double UpgradeExtension::GetRho(const ISRInstrument* instr, const sophis::market_data::CSRMarketData& context, long whichUnderlying)
{
	BEGIN_LOG("GetRho");
	double result = 0;
	try
	{
		MESS(Log::debug, FROM_STREAM("GetRho.BEGIN(instr=" << instr->GetCode() << ", whichUnderlying=" << whichUnderlying << ")"));

		//CSRPricer * pricer;
		//*pricer = CSRPricer::GetPricerByPriority(*instr).model;
		//CSRComputationResults *computationResults;

		//MESS(Log::debug, FROM_STREAM("ComputeAll.BEGIN"));
		//pricer->ComputeAll(*instr, context, *computationResults);
		//MESS(Log::debug, FROM_STREAM("ComputeAll.END"));

		//std::map<CSRRiskSourceRho, double> *rhoMap;
		//*rhoMap = computationResults->Rho();
		//std::map<CSRRiskSourceRho, double>::iterator it = searchMapValue(*rhoMap, whichUnderlying);
		//it->first.
		
		result = instr->GetDefaultPricer()->GetRho(*instr, context, instr->GetCurrency(), sophis::instrument::eRhoBumpType::BasisPoint);

		MESS(Log::debug, FROM_STREAM("GetRho.END(result=" << result << ")"));
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to GetRho: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}

/*
long UpgradeExtension::GetUnderlying(const ISRInstrument* instr, int whichUnderlying)
{
	BEGIN_LOG("GetUnderlying");
	long result = 0;
	try
	{
		MESS(Log::debug, FROM_STREAM("GetUnderlying.BEGIN(instr=" << instr->GetCode() << ", whichUnderlying=" << whichUnderlying << ")"));

		CSRPricer * pricer;
		*pricer = CSRPricer::GetPricerByPriority(*instr).model;
		CSRMarketData * marketData = CSRMarketData::GetCurrentMarketData();
		CSRComputationResults *computationResults;

		MESS(Log::debug, FROM_STREAM("ComputeAll.BEGIN"));
		pricer->ComputeAll(*instr, *marketData, *computationResults);
		MESS(Log::debug, FROM_STREAM("ComputeAll.END"));

		std::map<CSRRiskSourceDelta, double> *deltaMap;
		*deltaMap = computationResults->Delta();
		std::map<CSRRiskSourceDelta, double>::iterator it = searchMapValue(*deltaMap, whichUnderlying);
		result = it->first.fSicovam;

		MESS(Log::debug, FROM_STREAM("GetUnderlying.END(result=" << result << ")"));
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to GetUnderlying: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}
*/

double UpgradeExtension::GetFloatingNotionalFactor(const ISRInstrument* instr)
{
	BEGIN_LOG("GetFloatingNotionalFactor");
	double result = 0;
	try
	{
		MESS(Log::debug, FROM_STREAM("GetFloatingNotionalFactor.BEGIN(instr=" << instr->GetCode() << ")"));

		CSRMarketData * marketData = CSRMarketData::GetCurrentMarketData();
		const CSRPricer * pricer = instr->GetDefaultPricer();
		CSRComputationResults computationResults;

		MESS(Log::debug, FROM_STREAM("ComputeAll.BEGIN"));
		pricer->ComputeAll(*instr, *marketData, computationResults);
		MESS(Log::debug, FROM_STREAM("ComputeAll.END"));

		//Case number 02180325
		//sophis::CSRGlobalComputationKey key(instr->GetCode());
		//sophis::CSRComputationResults &computationResults = CSRComputationResults_Global::GetInstance().GetOrCreate(key);
		result = computationResults.FloatingNotionalFactor();

		MESS(Log::debug, FROM_STREAM("GetFloatingNotionalFactor.END(result=" << result << ")"));
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to GetFloatingNotionalFactor: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}

double UpgradeExtension::GetTheoreticalValue(const CSRInstrument* instr)
{
	BEGIN_LOG("GetTheoreticalValue");
	double result = 0;
	try
	{
		MESS(Log::debug, FROM_STREAM("GetTheoreticalValue.BEGIN(instr=" << instr->GetCode() << ")"));

		CSRMarketData * marketData = CSRMarketData::GetCurrentMarketData();
		const CSRPricer * pricer = instr->GetDefaultPricer();
		CSRComputationResults computationResults;

		MESS(Log::debug, FROM_STREAM("ComputeAll.BEGIN"));
		pricer->ComputeAll(*instr, *marketData, computationResults);
		MESS(Log::debug, FROM_STREAM("ComputeAll.END"));

		//Case number 02180325
		//sophis::CSRGlobalComputationKey key(instr->GetCode());
		//sophis::CSRComputationResults &computationResults = CSRComputationResults_Global::GetInstance().GetOrCreate(key);
		result = computationResults.TheoreticalValue();

		MESS(Log::debug, FROM_STREAM("GetTheoreticalValue.END(result=" << result << ")"));
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to GetTheoreticalValue: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}

double UpgradeExtension::GetTheoreticalValue(const CSRInstrument* instr, const market_data::CSRMarketData& context)
{
	BEGIN_LOG("GetTheoreticalValueWithContext");
	MESS(Log::debug, FROM_STREAM("GetTheoreticalValueWithContext.BEGIN(instr=" << instr->GetCode() << ")"));

	double result = GetTheoreticalValue(instr);

	MESS(Log::debug, FROM_STREAM("GetTheoreticalValueWithContext.END(result=" << result << ")"));
	END_LOG();
	return result;
}

bool UpgradeExtension::TakeDateCurveForStartPoint()
{
	//In 7.2, the preference flag 'Use Curve Date for Zero Coupon' is removed
	//CSRPreference::TakeDateCurveForStartPoint()
	return false;
}

EmcBond::EmcBond() {}

CONSTRUCTOR_BOND_META_MODEL(EmcPricerBond)

double EmcPricerBond::ComputeNonFlatCurvePrice(const instrument::CSRInstrument					&instr,
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
	bool											throwException) const
{
	const EmcBond * emcBond = dynamic_cast<const EmcBond *>(&instr);
	return emcBond->EmcComputeNonFlatCurvePrice(transactionDate, settlementDate, ownershipDate, param, derivative, base,
		adjustedDates, valueDates, dayCountBasis, yieldCalculation, redemptionArray, explicationArray, withSpreadMgt, throwException);
}
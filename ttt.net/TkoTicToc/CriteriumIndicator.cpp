#include "CriteriumIndicator.h"

#include "SphInc/SphMacros.h"
#include "SphSDBCInc/queries/SphQuery.h"
#include "SphSDBCInc/queries/SphQueryBuffered.h"
#include "SphTools/SphLoggerUtil.h"

using namespace std;

const char* CriteriumIndicator::__CLASS__ = "CriteriumIndicator";

void CriteriumIndicator::Register() {
	BEGIN_LOG("Initialize");
	struct TKO_INDICATOR_NAME
	{
		char INDICATOR_NAME[50];
		char INDICATOR_DISPLAY[100];
		char DATA_TYPE;
	};
	CSRQueryBuffered<TKO_INDICATOR_NAME> query;
	_STL::vector<TKO_INDICATOR_NAME> allIndicatorNames;

	query << "SELECT"
		<< OutOffset("INDICATOR_NAME", &TKO_INDICATOR_NAME::INDICATOR_NAME)
		<< OutOffset("NVL(INDICATOR_DISPLAY, INDICATOR_NAME) INDICATOR_DISPLAY", &TKO_INDICATOR_NAME::INDICATOR_DISPLAY)
		<< OutOffset("DATA_TYPE", &TKO_INDICATOR_NAME::DATA_TYPE) << "FROM TKO_INDICATOR_NAME WHERE IS_ENABLED = 1 AND DATA_TYPE IN ('D', 'F', 'T')";
	MESS(Log::info, "Retrieving all TKO_INDICATOR_NAME : " << query.GetSQL());
	query.FetchAll(allIndicatorNames);

	for each(auto it in allIndicatorNames) {
		MESS(Log::info, "Registering criterium: " << it.INDICATOR_NAME);
		INITIALISE_CRITERIUM_WITH_GROUPNAME(CriteriumIndicator, string(it.INDICATOR_DISPLAY).c_str(), "TIM Indicators");
		sophis::portfolio::CSRCriterium* crt = const_cast<sophis::portfolio::CSRCriterium*>(sophis::portfolio::CSRCriterium::GetPrototype().GetData(it.INDICATOR_DISPLAY));
		CriteriumIndicator* crtIndicator = dynamic_cast<CriteriumIndicator*>(crt);
		crtIndicator->setDataType(it.DATA_TYPE);
		crtIndicator->setIndicatorName(string(it.INDICATOR_NAME));
	}
	END_LOG();
}

void CriteriumIndicator::GetCode(SSReportingTrade* mvt, TCodeList &list)  const
{
	BEGIN_LOG("GetCode");
	MESS(Log::debug, "refcon=" << mvt->refcon << ", mvtident=" << mvt->mvtident << ", sicovam=" << mvt->sicovam);
	const CSRInstrument * instrument = CSRInstrument::GetInstance(mvt->sicovam);
	MESS(Log::debug, "instrument=" << instrument);

	GetCode(*instrument, NULL, list);
	END_LOG();
}

void CriteriumIndicator::GetCode(const CSRPosition& position, TCodeList& list) const
{
	BEGIN_LOG("GetCode");
	MESS(Log::debug, "position.GetIdentifier=" << position.GetIdentifier() << ", position.GetInstrumentCode=" << position.GetInstrumentCode());
	const CSRInstrument * instrument = CSRInstrument::GetInstance(position.GetInstrumentCode());
	MESS(Log::debug, "instrument=" << instrument);

	GetCode(*instrument, NULL, list);
	END_LOG();
}

void CriteriumIndicator::GetCode(const sophis::instrument::ISRInstrument& instr,
	const sophis::CSRComputationResults* results, TCodeList& list) const
{
	BEGIN_LOG("GetCode");
	MESS(Log::debug, "GetCode for Instrument: IndicatorName=" << indicatorName.c_str() << ", sicovam = " << instr.GetCode());

	try
	{
		CSRQuery query;
		double val;
		if (dataType == 'D' || dataType == 'F') {
			query << "SELECT" << CSROut("VALUE_NUMBER", val) << "FROM TKO_INDICATOR WHERE INDICATOR_NAME='"
				<< indicatorName.c_str() << "', AND INSTRUMENT_ID=" << std::to_string(instr.GetCode()).c_str();
		}
		else if (dataType == 'T') {
			query << "SELECT" << CSROut("ASCII(SUBSTR(VALUE_TEXT, 1, 1))", val) << "FROM TKO_INDICATOR WHERE INDICATOR_NAME='"
				<< indicatorName.c_str() << "' AND INSTRUMENT_ID=" << std::to_string(instr.GetCode()).c_str();
		}
		MESS(Log::debug, "GetCode SQL : " << query.GetSQL());
		if (query.Fetch()) {
			MESS(Log::debug, "val : " << val);
			list[0].fType = 0;
			list[0].fValue = val;
			list[0].fValueType = kDouble;
		}
	}
	catch (ExceptionBase & ex)
	{
		MESS(Log::error, "GetCode failed: (" << (const char *)ex << ")");
	}
	catch (...)
	{
		MESS(Log::error, "GetCode unknown error");
	}

	END_LOG();
}

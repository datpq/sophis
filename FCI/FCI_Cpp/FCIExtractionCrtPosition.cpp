#include "FCIExtractionCrtPosition.h"

#include "SphTools/SphLoggerUtil.h"

const char* FCIExtractionCrtPosition::__CLASS__ = "FCIExtractionCrtPosition";

void FCIExtractionCrtPosition::GetCode(const sophis::instrument::ISRInstrument& instr,
	const sophis::CSRComputationResults* results,
	TCodeList& list) const
{
	BEGIN_LOG("GetCode");

	try
	{
		std::string ref = instr.GetReference();
		long code = ref.at(0);
		MESS(Log::debug, "sicovam = " << instr.GetCode() << ", reference = " << ref.c_str() << ", code = " << code);
		list.push_back(SSOneValue(code));
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

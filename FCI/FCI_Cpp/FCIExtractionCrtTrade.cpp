#include "FCIExtractionCrtTrade.h"

#include "SphTools/SphLoggerUtil.h"

const char* FCIExtractionCrtTrade::__CLASS__ = "FCIExtractionCrtTrade";

void FCIExtractionCrtTrade::GetCode(SSReportingTrade* mvt, TCodeList &list)  const
{
	BEGIN_LOG("GetCode");

	try
	{
		long code = (long)mvt->quantity;
		list.push_back(SSOneValue(code));
		MESS(Log::debug, "refcon = " << mvt->refcon << ", quantity = " << mvt->quantity << ", code = " << code);
	}
	catch (ExceptionBase & ex)
	{
		MESS(Log::error, "GetCode failed with exception(" << (const char *)ex << ")");
	}
	catch (...)
	{
		MESS(Log::error, "GetCode unknown error");
	}

	END_LOG();
}

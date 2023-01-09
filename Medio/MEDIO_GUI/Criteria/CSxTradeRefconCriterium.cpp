#include "CSxTradeRefconCriterium.h"
#include <sstream>

//-------------------------------------------------------------------------
const char* CSxTradeRefconCriterium::__CLASS__ = "CSxTradeRefconCriterium";

//-------------------------------------------------------------------------
void CSxTradeRefconCriterium::GetName(long code, char* name, size_t size) const
{
	std::ostringstream oss;
	oss << code;
	strcpy_s(name, size, oss.str().c_str());
}

//-------------------------------------------------------------------------
void CSxTradeRefconCriterium::GetCode(SSReportingTrade* mvt, TCodeList &list) const
{
	list[0].fType = 0;
	list[0].fCode = (long)mvt->refcon;
}


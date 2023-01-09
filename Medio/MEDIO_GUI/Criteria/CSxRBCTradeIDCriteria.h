#pragma once

//#include "SphInc/SphIncludes.h"
#include "SphTools/SphCommon.h"
#include "SphInc/portfolio/SphCriteria.h"

using namespace sophis::gui;

class CSxRBCTradeIDCriteria : public CSRCriterium
{
	DECLARATION_CRITERIUM_WITH_CAPS(CSxRBCTradeIDCriteria, true, false, false);

public:
	virtual void GetName(long code, char* name, size_t size) const override;
	virtual void GetCode(SSReportingTrade* mvt, TCodeList &list) const override;

private:
	static const char* __CLASS__;
	static _STL::map<long,_STL::string> RBCTradeIDs;
};


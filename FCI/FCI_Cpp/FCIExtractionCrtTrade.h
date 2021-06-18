#pragma once

#ifndef __FCIExtractionCrtTrade_H__
#define __FCIExtractionCrtTrade_H__

/*
** Includes
*/

#include "SphInc/portfolio/SphCriteria.h"

class FCIExtractionCrtTrade : public sophis::portfolio::CSRCriterium
{
public:
	DECLARATION_CRITERIUM_WITH_CAPS(FCIExtractionCrtTrade, true, false, true)

	virtual void GetCode(SSReportingTrade* mvt, TCodeList &list)  const;

private:
	static const char * __CLASS__;
};

#endif // !__FCIExtractionCrtTrade_H__

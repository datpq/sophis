#pragma once

#ifndef __FCIExtractionCrtPosition_H__
#define __FCIExtractionCrtPosition_H__

/*
** Includes
*/

#include "SphInc/portfolio/SphCriteria.h"

class FCIExtractionCrtPosition : public sophis::portfolio::CSRCriterium
{
public:
	DECLARATION_CRITERIUM_WITH_CAPS(FCIExtractionCrtPosition, false, false, true)

	virtual void GetCode(const sophis::instrument::ISRInstrument& instr,
		const sophis::CSRComputationResults* results,
		TCodeList& list) const;

private:
	static const char * __CLASS__;
};

#endif // !__FCIExtractionCrtPosition_H__

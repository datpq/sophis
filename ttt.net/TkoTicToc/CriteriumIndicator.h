#pragma once

#ifndef __CriteriumIndicator_H__
#define __CriteriumIndicator_H__

/*
** Includes
*/

#include "SphInc/portfolio/SphCriteria.h"

class CriteriumIndicator : public sophis::portfolio::CSRCriterium
{
public:
	DECLARATION_CRITERIUM_WITH_CAPS(CriteriumIndicator, true, true, true)

	virtual void GetCode(const sophis::instrument::ISRInstrument& instr,
		const sophis::CSRComputationResults* results, TCodeList& list) const;
	virtual void GetCode(const CSRPosition& position, TCodeList& list) const;
	virtual void GetCode(SSReportingTrade* mvt, TCodeList &list)  const;

	char getDataType() { return dataType; }
	void setDataType(const char dataType) { this->dataType = dataType; }
	std::string& getIndicatorName() { return indicatorName; }
	void setIndicatorName(const std::string indicatorName) { this->indicatorName = indicatorName; }

	static void Register();
private:
	static const char * __CLASS__;
	std::string indicatorName;
	char dataType;
};

#endif // !__CriteriumIndicator_H__

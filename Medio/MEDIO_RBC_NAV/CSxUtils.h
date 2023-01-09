#ifndef __CSxUtils_H__
	#define __CSxUtils_H__

#define RBC_Strategy_NAV_Column_Name "RBC Strategy NAV"
#define RBC_Weight_In_Strategy_Column_Name "Weight in Strategy (Market Value/RBC NAV)"

#include "SphInc\portfolio\SphPortfolio.h"
#include "SphInc\portfolio\SphPosition.h"

class CSxUtils
{
public:
	static long GetStrategyOfPosition(const CSRPosition& position, PSRExtraction extraction);
};

#endif //!__CSxUtils_H__
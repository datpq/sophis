#include "CFG_GlobalFunctions.h"
#include "CFG_YieldCurveData.h"

CONSTRUCTOR_GLOBAL_FUNCTIONS(CFG_GlobalFunctions);

CSRInfoSup*	CFG_GlobalFunctions::new_YCRateInfoSup() const
{
	return new CFG_YieldCurveData();
};

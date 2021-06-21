#ifndef __CFG_GlobalFunctions_H__
#define __CFG_GlobalFunctions_H__

#include "SphInc/value/kernel/SphAMGlobalFunctions.h"

class CFG_GlobalFunctions : public sophis::value::CSAMGlobalFunctions
{
//------------------------------------ PUBLIC ------------------------------------
public:
	DECLARATION_GLOBAL_FUNCTIONS(CFG_GlobalFunctions);
	virtual	market_data::CSRInfoSup* new_YCRateInfoSup() const;
};

#endif //!__CFG_GlobalFunctions_H__
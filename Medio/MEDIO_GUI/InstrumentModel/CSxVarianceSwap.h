#ifndef __CSxVarianceSwap_H__
	#define __CSxVarianceSwap_H__

/*
** Includes
*/
#include "SphInc/instrument/SphSwap.h"
#include "SphInc/instrument/SphInstrument.h"
//#include "../cc_data/VarianceSwap.h"
//#include "../cc_data/VolatilitySwap.h"
#include "SphInc\instrument\SphVarianceSwap.h"

class CSxVarianceSwap : public CSRVarianceSwap
{
	DECLARATION_SWAP(CSxVarianceSwap)
};


class CSxVolatilitySwap : public CSRVolatilitySwap
{
	DECLARATION_SWAP(CSxVolatilitySwap)
};

#endif // __CSxVarianceSwap_H__



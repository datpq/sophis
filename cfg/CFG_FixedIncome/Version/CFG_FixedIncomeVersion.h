#ifndef _CFG_FixedIncome_Version_H_
#define _CFG_FixedIncome_Version_H_

#include "SphInc/SphBuildVersion.h"

#define CFG_FixedIncome_VERSION 			7, 3, 3, 1000
#define CFG_FixedIncome_VERSION_STR			"7.3.3.1000\0"
#ifdef WIN64
	#define CFG_FixedIncome_TOOLKIT_DESCRIPTION	"CFG_MoroccanFixedIncome (x64)"
#else
	#define CFG_FixedIncome_TOOLKIT_DESCRIPTION	"CFG_MoroccanFixedIncome (x86)"
#endif

#endif // _CFG_FixedIncome_Version_H_

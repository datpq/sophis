#ifndef _CFG_SRTreasury_Version_H_
#define _CFG_SRTreasury_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"

#define CFG_SRTreasury_VERSION 				7, 3, 3, 1000
#define CFG_SRTreasury_VERSION_STR			"7.3.3.1000\0"
#ifdef WIN64
	#define CFG_SRTreasury_TOOLKIT_DESCRIPTION	"CFG_SRTreasury (x64)"
#else
	#define CFG_SRTreasury_TOOLKIT_DESCRIPTION	"CFG_SRTreasury (x86)"
#endif

#endif // _CFG_SRTreasury_Version_H_

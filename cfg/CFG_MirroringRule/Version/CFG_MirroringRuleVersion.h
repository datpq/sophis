#ifndef _CFG_MirroringRule_Version_H_
#define _CFG_MirroringRule_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"

#define CFG_MirroringRule_VERSION 				7, 3, 3, 1000
#define CFG_MirroringRule_VERSION_STR			"7.3.3.1000\0"
#ifdef WIN64
	#define CFG_MirroringRule_TOOLKIT_DESCRIPTION	"CFG_MirroringRule (x64)"
#else
	#define CFG_MirroringRule_TOOLKIT_DESCRIPTION	"CFG_MirroringRule (x86)"
#endif

#endif // _CFG_MirroringRule_Version_H_

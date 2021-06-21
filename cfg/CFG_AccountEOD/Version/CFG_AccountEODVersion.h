#ifndef _CFG_AccountEOD_Version_H_
#define _CFG_AccountEOD_Version_H_

#include "SphInc/SphBuildVersion.h"

#define CFG_AccountEOD_VERSION 				7, 3, 3, 1000
#define CFG_AccountEOD_VERSION_STR			"7.3.3.1000\0"
#ifdef WIN64
	#define CFG_AccountEOD_TOOLKIT_DESCRIPTION	"CFG_AccountEOD (x64)"
#else
	#define CFG_AccountEOD_TOOLKIT_DESCRIPTION	"CFG_AccountEOD (x86)"
#endif

#endif // _CFG_AccountEOD_Version_H_

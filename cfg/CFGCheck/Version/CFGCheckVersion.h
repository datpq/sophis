#ifndef _CFGCheck_Version_H_
#define _CFGCheck_Version_H_

#include "SphInc/SphBuildVersion.h"

#define CFGCheck_VERSION 				7, 3, 3, 1000
#define CFGCheck_VERSION_STR			"7.3.3.1000\0"
#ifdef WIN64
	#define CFGCheck_TOOLKIT_DESCRIPTION	"CFGCheck (x64)"
#else
	#define CFGCheck_TOOLKIT_DESCRIPTION	"CFGCheck (x86)"
#endif

#endif // _CFGCheck_Version_H_

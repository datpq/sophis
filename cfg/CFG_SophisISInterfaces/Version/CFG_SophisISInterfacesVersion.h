#ifndef _CFG_SophisISInterfaces_Version_H_
#define _CFG_SophisISInterfaces_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"

#define CFG_SophisISInterfaces_VERSION 				7, 3, 3, 1000
#define CFG_SophisISInterfaces_VERSION_STR			"7.3.3.1000\0"
#ifdef WIN64
	#define CFG_SophisISInterfaces_TOOLKIT_DESCRIPTION	"CFG_SophisISInterfaces (x64)"
#else
	#define CFG_SophisISInterfaces_TOOLKIT_DESCRIPTION	"CFG_SophisISInterfaces (x86)"
#endif

#endif // _CFG_SophisISInterfaces_Version_H_

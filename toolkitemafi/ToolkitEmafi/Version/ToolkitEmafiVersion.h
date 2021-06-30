#ifndef _ToolkitEmafi_Version_H_
#define _ToolkitEmafi_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"

#define ToolkitEmafi_VERSION 				1, 0, 0, 0
#define ToolkitEmafi_VERSION_STR			"1.0.0.0\0"
#ifdef WIN32
	#define ToolkitEmafi_TOOLKIT_DESCRIPTION	"ToolkitEmafi (x86)"
#else
	#define ToolkitEmafi_TOOLKIT_DESCRIPTION	"ToolkitEmafi (x64)"
#endif

#endif // _ToolkitEmafi_Version_H_

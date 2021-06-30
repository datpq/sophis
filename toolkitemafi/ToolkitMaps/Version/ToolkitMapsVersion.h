#ifndef _ToolkitMaps_Version_H_
#define _ToolkitMaps_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"

#define ToolkitMaps_VERSION 				1, 0, 0, 0
#define ToolkitMaps_VERSION_STR			"1.0.0.0\0"
#ifdef WIN32
	#define ToolkitMaps_TOOLKIT_DESCRIPTION	"ToolkitMaps (x86)"
#else
	#define ToolkitMaps_TOOLKIT_DESCRIPTION	"ToolkitMaps (x64)"
#endif

#endif // _ToolkitMaps_Version_H_

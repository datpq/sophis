#ifndef _MEDIO_DOB_INDICATOR_Version_H_
#define _MEDIO_DOB_INDICATOR_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"
#include "..\..\MediolanumVersion.h"

#define MEDIO_DOB_INDICATOR_VERSION 			MEDIOLANUM_VERSION
#define MEDIO_DOB_INDICATOR_VERSION_STR			MEDIOLANUM_VERSION_STR
#ifdef WIN32
	#define MEDIO_DOB_INDICATOR_TOOLKIT_DESCRIPTION	"MEDIO_DOB_INDICATOR (x86)"
#else
	#define MEDIO_DOB_INDICATOR_TOOLKIT_DESCRIPTION	"MEDIO_DOB_INDICATOR (x64)"
#endif

#endif // _MEDIO_DOB_INDICATOR_Version_H_
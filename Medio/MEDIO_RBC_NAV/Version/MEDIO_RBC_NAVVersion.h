#ifndef _MEDIO_RBC_NAV_Version_H_
#define _MEDIO_RBC_NAV_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"
#include "..\..\MediolanumVersion.h"

#define MEDIO_RBC_NAV_VERSION 				MEDIOLANUM_VERSION
#define MEDIO_RBC_NAV_VERSION_STR			MEDIOLANUM_VERSION_STR
#ifdef WIN32
	#define MEDIO_RBC_NAV_TOOLKIT_DESCRIPTION	"MEDIO_RBC_NAV (x86)"
#else
	#define MEDIO_RBC_NAV_TOOLKIT_DESCRIPTION	"MEDIO_RBC_NAV (x64)"
#endif

#endif // _MEDIO_RBC_NAV_Version_H_

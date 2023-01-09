#ifndef _MEDIO_GUI_Version_H_
#define _MEDIO_GUI_Version_H_

#include "SphInc/Value/kernel/SphBuildVersion.h"
#include "../../MediolanumVersion.h"

#define MEDIO_GUI_VERSION 				MEDIOLANUM_VERSION
#define MEDIO_GUI_VERSION_STR			MEDIOLANUM_VERSION_STR
#ifdef WIN32
	#define MEDIO_GUI_TOOLKIT_DESCRIPTION	"MEDIO_GUI (x86)"
#else
	#define MEDIO_GUI_TOOLKIT_DESCRIPTION	"MEDIO_GUI (x64)"
#endif

#endif // _MEDIO_GUI_Version_H_

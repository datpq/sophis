#ifndef _ToolkitRapprochement_Version_H_
#define _ToolkitRapprochement_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"

#define ToolkitRapprochement_VERSION 				1, 0, 0, 0
#define ToolkitRapprochement_VERSION_STR			"1.0.0.0\0"
#ifdef WIN32
	#define ToolkitRapprochement_TOOLKIT_DESCRIPTION	"ToolkitRapprochement (x86)"
#else
	#define ToolkitRapprochement_TOOLKIT_DESCRIPTION	"ToolkitRapprochement (x64)"
#endif

#endif // _ToolkitRapprochement_Version_H_

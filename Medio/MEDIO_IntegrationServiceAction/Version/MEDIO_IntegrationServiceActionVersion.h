#ifndef _MEDIO_IntegrationServiceAction_Version_H_
#define _MEDIO_IntegrationServiceAction_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"
#include "..\MediolanumVersion.h"

#define MEDIO_IntegrationServiceAction_VERSION 				MEDIOLANUM_VERSION
#define MEDIO_IntegrationServiceAction_VERSION_STR			MEDIOLANUM_VERSION_STR
#ifdef WIN32
	#define MEDIO_IntegrationServiceAction_TOOLKIT_DESCRIPTION	"MEDIO_IntegrationServiceAction (x86)"
#else
	#define MEDIO_IntegrationServiceAction_TOOLKIT_DESCRIPTION	"MEDIO_IntegrationServiceAction (x64)"
#endif

#endif // _MEDIO_IntegrationServiceAction_Version_H_

#ifndef _Mediolanum_RMA_FILTER_CLI_Version_H_
#define _Mediolanum_RMA_FILTER_CLI_Version_H_

#include "SphInc\Value\kernel\SphBuildVersion.h"
#include "..\MediolanumVersion.h"

#define Mediolanum_RMA_FILTER_CLI_VERSION 				MEDIOLANUM_VERSION
#define Mediolanum_RMA_FILTER_CLI_VERSION_STR			MEDIOLANUM_VERSION_STR
#ifdef WIN32
	#define Mediolanum_RMA_FILTER_CLI_TOOLKIT_DESCRIPTION	"Mediolanum_RMA_FILTER_CLI (x86)"
#else
	#define Mediolanum_RMA_FILTER_CLI_TOOLKIT_DESCRIPTION	"Mediolanum_RMA_FILTER_CLI (x64)"
#endif

#endif // _Mediolanum_RMA_FILTER_CLI_Version_H_

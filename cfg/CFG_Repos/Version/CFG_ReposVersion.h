#ifndef _CFG_Repos_Version_H_
#define _CFG_Repos_Version_H_

#include "SphInc/SphBuildVersion.h"

#define CFG_Repos_VERSION 				7, 3, 3, 1000
#define CFG_Repos_VERSION_STR			"7.3.3.1000\0"
#ifdef WIN64
#define CFG_Repos_TOOLKIT_DESCRIPTION	"CFG_Repos (x64)"
#else
#define CFG_Repos_TOOLKIT_DESCRIPTION	"CFG_Repos (x86)"
#endif

#endif // _CFG_Repos_Version_H_

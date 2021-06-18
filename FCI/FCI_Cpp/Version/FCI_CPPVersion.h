#ifndef _FCI_CPP_Version_H_
#define _FCI_CPP_Version_H_

#include "SphInc/SphBuildVersion.h"

#define FCI_CPP_VERSION 			2, 0, 0, 8
#define FCI_CPP_VERSION_STR			"2.0.0.8\0"
#ifdef WIN64
	#define FCI_CPP_TOOLKIT_DESCRIPTION	"FCI_CPP (x64)"
#else
	#define FCI_CPP_TOOLKIT_DESCRIPTION	"FCI_CPP (x86)"
#endif

#endif // _FCI_CPP_Version_H_

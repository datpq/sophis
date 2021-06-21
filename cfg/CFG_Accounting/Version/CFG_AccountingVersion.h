#ifndef _CFG_Accounting_Version_H_
#define _CFG_Accounting_Version_H_

#include "SphInc/SphBuildVersion.h"
#include "SphInc/value/kernel/SphBuildVersion.h"

#define CFG_Accounting_VERSION 				7, 3, 3, 1000
#define CFG_Accounting_VERSION_STR			"7.3.3.1000\0"
#ifdef WIN64
#define CFG_Accounting_TOOLKIT_DESCRIPTION	"CFG_Accounting (x64)"
#else
#define CFG_Accounting_TOOLKIT_DESCRIPTION	"CFG_Accounting (x86)"
#endif


#endif // _CFG_Accounting_Version_H_


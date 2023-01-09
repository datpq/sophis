#pragma once

#define MAJOR_VERSION	21
#define MINOR_VERSION	1
#define PATCH_VERSION	0
#define BUILD_NUMBER 12115
#define ZERO 0  //required to avoid link warnings on file version string

//double macro evaluation trick
#define MAKESTRING_(s) #s
#define MAKESTRING(s) MAKESTRING_(s)

// product version strings
#define PRODUCT_VERSION_INT	MAJOR_VERSION, MINOR_VERSION, PATCH_VERSION, BUILD_NUMBER
#define FILE_VERSION_INT MAJOR_VERSION, MINOR_VERSION, PATCH_VERSION, BUILD_NUMBER
#define PRODUCT_VERSION_STRING 	MAKESTRING(MAJOR_VERSION.MINOR_VERSION.PATCH_VERSION.BUILD_NUMBER)
#define FILE_VERSION_STRING 	MAKESTRING(MAJOR_VERSION.MINOR_VERSION.PATCH_VERSION.BUILD_NUMBER)

// assembly version string
#define ASSEMBLY_VERSION MAKESTRING(MAJOR_VERSION.MINOR_VERSION.0.0)

// COPYRIGHT used in MFC resource and needs final \0.
#define COPYRIGHT "Copyright (c) Finastra 1993-2021\0"
// COPYRIGHT used in .NET
#define COPYRIGHT_DOTNET "Copyright (c) Finastra 1993-2021"

// Company name
#define MISYS_COMPANY_NAME "Finastra"

#define FUSION_SOPHIS_PRODUCT_NAME "Fusion Sophis"
#define FUSION_INVEST_PRODUCT_NAME "Fusion Invest"

// Target platform
#ifndef _WIN64
#define TARGETPLATFORM "x86"
#else
#define TARGETPLATFORM "x64"
#endif

#if (defined(WIN32)||defined(_WIN64))
#	pragma once // speed up VC++ compilation
#endif

/**
* ----------------------------------------------------------------------------
* Sophis Technology 
* Copyright (c) 2007
* File : SphSystemExport.h
* Creation : Nov 12 2007 by silviu Marele
* Description : Symbol import management for WINDOWS DLL clients.
* ----------------------------------------------------------------------------
*/

#ifndef __SPHSYSTEMEXPORTS_H__
#define __SPHSYSTEMEXPORTS_H__

#if (defined(WIN32)||defined(_WIN64))
#	ifdef SPHSYSTEM_EXPORTS
#		define SPHSYSTEM_API __declspec(dllexport)
#		define SPHSYSTEM_API_TEMPLATE
#		define SPHSYSTEM SPHSYSTEM_API
#	else
#		define SPHSYSTEM_API __declspec(dllimport)
#		define SPHSYSTEM_API_TEMPLATE extern
#		define SPHSYSTEM SPHSYSTEM_API
#	endif
#	define SPHSYSTEM_API_TMPL_DLL(tmpl) SPHSYSTEM_API_TEMPLATE template class SPHSYSTEM_API tmpl
#else
#	define SPHSYSTEM_API
#	define SPHSYSTEM_API_TEMPLATE
#	define SPHSYSTEM_API_TMPL_DLL(tmpl) template class tmpl
#	define SPHSYSTEM SPHSYSTEM_API
#endif

#endif // __SPHSYSTEMEXPORTS_H__

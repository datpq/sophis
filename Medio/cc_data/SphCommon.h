#if (defined(WIN32)||defined(_WIN64))
#	pragma once // speed up VC++ compilation
#endif

/**
* Common definitions for all Sophis Tools classes
*
* @author : OP
* @date : 2003/01/08
*/

#ifndef __SPHCOMMON_H__
#define __SPHCOMMON_H__

#if (defined(WIN32)||defined(_WIN64))
	// Windows platform definitions
#pragma warning(disable:4514)
#pragma warning(disable:4710)
#pragma warning(disable:4711)
#pragma warning(disable:4820)

#pragma warning(push)
// LONG_PTR definition
#	pragma warning(disable:4668)
#	include <BaseTsd.h>
#pragma warning(pop)

#	pragma warning(error:4103)

#	pragma warning(disable:4503) // Template name too long
#	pragma warning(disable:4786) // identifier name too long (truncated for debug)
#	pragma warning(disable:4231) // 'extern' before template explicit instantiation
#	pragma warning(disable:4290) // C++ Exception Specification ignored
#	pragma warning(disable:4482) // nonstandard extension used: enum '...' used in qualified name
#else

#   define INT_PTR long
#   define LONG_PTR long

#endif

#define _STL std
#define __STL_INCLUDE_PATH(header_file) <header_file>

#ifdef _WIN64
#	define BOOST_PACK_VALUE 16
#else
#	define BOOST_PACK_VALUE 8
#endif

#if (defined(WIN32)||defined(_WIN64))

#	define BOOST_INCLUDE_BEGIN \
	__pragma(managed(push,off)) \
	__pragma(pack(push,BOOST_PACK_VALUE)) \
	__pragma(warning(push)) \
	__pragma(warning(disable:4365)) \
	__pragma(warning(disable:4619)) \
	__pragma(warning(disable:4371)) \
	__pragma(warning(disable:4626))	\
	__pragma(warning(disable:4625)) \
	__pragma(warning(disable:4127)) \
	__pragma(warning(disable:4265))

#	define BOOST_INCLUDE_END \
	__pragma(warning(pop)) \
	__pragma(pack(pop)) \
	__pragma(managed(pop))

#else
#	define BOOST_INCLUDE_BEGIN
#	define BOOST_INCLUDE_END
#endif

#define BOOST_INCLUDE_PATH(header) <boost/header>

//VC8 64bits >> Defined Casts

// #define use to avoid the warnings on size_t and time_t
#define SIZE_T_TO_LONG(arg) ((long)(arg))				
#define SIZE_T_TO_ULONG(arg) ((unsigned long)(arg))		
#define SIZE_T_TO_UCHAR(arg) ((unsigned char)(arg))		
#define SIZE_T_TO_SHORT(arg) ((short)(arg))				
#define TIME_T_TO_LONG(arg) ((long)(arg))				
#define TIME_T_TO_ULONG(arg) ((unsigned long)(arg))		
#define TIME_T_TO_SIZE_T(arg) ((size_t)(arg))			

// #define use to cast arguments of virtual function defined as LONG_PTR when the contain is long or short														 
#define LONG_PTR_TO_LONG(arg) ((long)(arg))				
#define LONG_PTR_TO_SHORT(arg) ((short)(arg))

// transformation of Windows types when these types contains long
#define SOCKET_TO_LONG(arg) ((long)(arg))
#define INT_PTR_TO_LONG(arg) ((long)(arg))

/*
 *	Other definitions common to all OS's
 */
//VC8 64bits (LONG_PTR) cast needed to disable warning 
#define OFFSET(a,b)	((long)(LONG_PTR) &((*(a*)0L).b))

//#define needed for the argument of a window functions when the signature is different between WIN32 and WIN64
//typically used when the name of the function is a windows #define
#ifdef _WIN64
#	define LONG_PTR_OR_LONG(arg) ((LONG_PTR)(arg))
#else
#	define LONG_PTR_OR_LONG(arg) ((long)(arg))
#endif

// transformation between void* and long when void* is a generic container for multiple usage
#define VOID_E_TO_LONG(arg) ((long)(LONG_PTR)(arg))		
#define LONG_TO_VOID_E(arg) ((void*)(LONG_PTR)(arg))


// Definitions common to all platforms


#endif // __SPHCOMMON_H__

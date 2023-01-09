#ifndef _SOPHIS_EVENT_EVENTCOREEXPORT_H
#define _SOPHIS_EVENT_EVENTCOREEXPORT_H

#if(defined(WIN32)||defined(_WIN64))
#	ifdef SOPHIS_EVENT_CORE_EXPORT
#		define SOPHIS_EVENT_CORE __declspec(dllexport)
#	else
#		define SOPHIS_EVENT_CORE __declspec(dllimport)
#	endif
#else
#	define SOPHIS_EVENT_CORE
#endif


#if(defined(WIN32)||defined(_WIN64))
#	ifdef _WIN64
#		define EVENT_PACK_VALUE 16
#	else
#		define EVENT_PACK_VALUE 8
#	endif

#define EVENT_PROLOG \
	__pragma(managed(push,off)) \
	__pragma(pack(push,EVENT_PACK_VALUE))

#define EVENT_EPILOG \
	__pragma(pack(pop)) \
	__pragma(managed(pop))

#else


#endif

#endif // _SOPHIS_EVENT_EVENTCOREEXPORT_H

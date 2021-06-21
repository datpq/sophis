#pragma once

#ifndef __CFG_REPOSEXPORT_H__
#define __CFG_REPOSEXPORT_H__

#if (defined(WIN32)||defined(_WIN64))
#	ifdef CFG_REPOS_EXPORTS
#		define CFG_REPOS __declspec(dllexport)
#	else
#		define CFG_REPOS __declspec(dllimport)
#	endif
#else
#	define CFG_REPOS
#endif

#endif //__CFG_REPOSEXPORT_H__
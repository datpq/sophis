#ifndef __CFG_MANAGEMENT_FEES_EXPORTS_H__
#define __CFG_MANAGEMENT_FEES_EXPORTS_H__
#pragma once

#if (defined(WIN32)||defined(_WIN64))
#	ifdef CFG_MANAGEMENT_FEES_EXPORTS
#		define CFG_MANAGEMENT_FEES __declspec(dllexport)
#	else
#		define CFG_MANAGEMENT_FEES __declspec(dllimport)
#	endif
#endif

#endif
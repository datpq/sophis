#ifndef _CSxTools_H_
#define _CSxTools_H_

#include "SphTools/SphCommon.h"
#include "SphTools/SphLoggerUtil.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)


class CSxTools
{
	//------------------------------------ PUBLIC ---------------------------------
public:

	static _STL::string DuplicateChar(_STL::string str, const char c );
	static _STL::string NumToDateYYYYMMDD(long sphDate);	

protected:


	/** For log purpose
	@version 1.0.0.0
	*/
	static const char * __CLASS__;


};


#endif //!_CSxTools_H_
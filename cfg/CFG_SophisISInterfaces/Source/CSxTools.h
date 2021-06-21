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
	static _STL::string NumToDateDDMMYYYY(long sphDate);
	static _STL::string GetDateYYYYMMDDHHmmss();		
	static long DDMMYYYYDateToNum(_STL::string date);
	static _STL::string NumToDateDDMMYY(long sphDate);
	static void AddFormatedString(_STL::string& dest, unsigned long nbChar, _STL::string str, _STL::string elem = " ");
	static _STL::string FormatDouble(double val, int nbIntegers, int nbDecimals);

protected:


	/** For log purpose
	@version 1.0.0.0
	*/
	static const char * __CLASS__;


};


#endif //!_CSxTools_H_
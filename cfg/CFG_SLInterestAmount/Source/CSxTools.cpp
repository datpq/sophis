/*
** Includes
*/

#include "SphInc/SphMacros.h"
#include "SphTools/SphDay.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphTools/base/StringUtil.h"

#include <time.h>
#include <math.h>

#include "CSxTools.h"


/*
** Used Namespace
*/

using namespace _STL;
using namespace sophis::misc;
using namespace sophis::math;

/*
** defines
*/

/*
** Globals
*/
const char * CSxTools::__CLASS__ = "CSxTools";


_STL::string CSxTools::DuplicateChar(_STL::string str, const char c)
{
	_STL::string result("");	

	for(size_t i = 0; i < str.size(); i++)
	{		
		if (str[i] == c)
			result += c;

		result += str[i];
	}
	return result;
}

_STL::string CSxTools::NumToDateYYYYMMDD(long sphDate)
{
	CSRDay Date(sphDate);	

	char year[5]; 
	sprintf_s(year, 5, "%04d", long(Date.fYear));

	char month[3];  
	sprintf_s(month, 3,  "%02d", long(Date.fMonth));

	char day[3];  
	sprintf_s(day, 3, "%02d", long(Date.fDay));

	return FROM_STREAM(year<< month << day);
}


#pragma warning(disable:4251)
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

_STL::string CSxTools::NumToDateDDMMYYYY(long sphDate)
{
	CSRDay Date(sphDate);	

	char year[5]; 
	sprintf_s(year, 5, "%04d", long(Date.fYear));

	char month[3];  
	sprintf_s(month, 3,  "%02d", long(Date.fMonth));

	char day[3];  
	sprintf_s(day, 3, "%02d", long(Date.fDay));

	return FROM_STREAM(day << "/" << month << "/" << year);
}

_STL::string CSxTools::NumToDateDDMMYY(long sphDate)
{
	CSRDay Date(sphDate);	
	
	char year[5]; 
	sprintf_s(year, 5, "%04d", long(Date.fYear));
	_STL::string yearStr = year;
	_STL::string YearSubStr = yearStr.substr(2,2);


	char month[3];  
	sprintf_s(month, 3,  "%02d", long(Date.fMonth));

	char day[3];  
	sprintf_s(day, 3, "%02d", long(Date.fDay));

	_STL::string ret = FROM_STREAM(day << month << YearSubStr);
	
	return ret;
}

_STL::string CSxTools::GetDateYYYYMMDDHHmmss()
{
	//creation date
	_STL::string dateTime;
	CSRDay Date(gApplicationContext->GetDate());	

	char year[5]; 
	sprintf_s(year, 5, "%04d", long(Date.fYear));

	char month[3];  
	sprintf_s(month, 3, "%02d", long(Date.fMonth));

	char day[3];  
	sprintf_s(day, 3, "%02d", long(Date.fDay));	 

	//creation time
	time_t systemtime;
	struct tm timeinfo ;

	time (&systemtime);
	localtime_s(&timeinfo, &systemtime);

	char hour[3];
	sprintf_s(hour, 3, "%02d", long(timeinfo.tm_hour));

	char minutes[3];
	sprintf_s(minutes, 3, "%02d", long(timeinfo.tm_min));

	char seconds[3];
	sprintf_s(seconds, 3, "%02d", long(timeinfo.tm_sec));
	dateTime = FROM_STREAM(year << month << day << hour << minutes << seconds);

	return dateTime;
}

long CSxTools::DDMMYYYYDateToNum(_STL::string date)
{
	_STL::string strDay = date.substr(0,2);
	_STL::string strMonth = date.substr(3,2);
	_STL::string strYear = date.substr(6,4);
	_STL::string strDateyyyymmdd = strYear + strMonth + strDay;
	CSRDay dayObject((char*)(strDateyyyymmdd.c_str()));

	return dayObject.toLong();
}

void CSxTools::AddFormatedString(_STL::string& dest, unsigned long nbChar, _STL::string str, _STL::string elem)
{
	BEGIN_LOG("AddFormatedString");

	size_t strLenght = str.size();

	if (strLenght > nbChar)
	{
		_STL::string mess = FROM_STREAM("String too long (" << str << ") Size max : (" << nbChar << ")");
		MESS(Log::error, mess.c_str());
		throw GeneralException(mess.c_str());
	}

	dest += str;

	for (unsigned long i = 0; i < nbChar-strLenght; i++)
	{
		dest += elem;
	}

	END_LOG();
}

_STL::string CSxTools::FormatDouble(double val, int nbIntegers, int nbDecimals)
{	
	// Adjust decimal part
	double adjust = 0.5/pow(10., nbDecimals+1);
	if (val < 0)
	{
		val -= adjust;
	}
	else
	{
		val += adjust;
	}

	static char output[80] = "";
	static char formatChar[80] = "";
	sprintf_s(formatChar, 80, "%%0%d.%d%%lf", nbIntegers+nbDecimals+1, nbDecimals); // %0'nbIntegers+NbDecimals+1'.'nbDecimals'
	sprintf_s(output, 80, formatChar, val);

	return output;
}


#include <ctime>
#include "CSxUtils.h"

using namespace std;

string CSxUtils::GetCurDateTime(const char * format)
{
	time_t rawtime;
	struct tm timeinfo;
	char buffer[80];

	time(&rawtime);
	localtime_s(&timeinfo,&rawtime);

	strftime(buffer, sizeof(buffer), format, &timeinfo);
	string str(buffer);
	return str;
}


string CSxUtils::GetCurDateTimeYYMMDDHHMMSS()
{
	return GetCurDateTime("%y%m%d%H%M%S");
}

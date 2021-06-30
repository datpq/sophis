#include "Inc/StringUtils.h"
#include "Inc/Log.h"
#include <stdarg.h>
#include <atlstr.h>

using namespace std;

string eff::utils::StrReplace(string& str, const char * oldValue, const char * newValue)
{
	string::size_type pos = 0u;
	while((pos = str.find(oldValue, pos)) != string::npos){
		str.replace(pos, strlen(oldValue), newValue);
		pos += strlen(newValue);
	}
	return str;
}

string eff::utils::StrReplace(string& str, const string& oldValue, const string& newValue)
{
	string::size_type pos = 0u;
	while((pos = str.find(oldValue, pos)) != string::npos){
		str.replace(pos, oldValue.length(), newValue);
		pos += newValue.length();
	}
	return str;
}

string eff::utils::StrReplace(string& str, map<string, string> mapOldNewValues)
{
	map<string, string>::iterator it;
	for (it = mapOldNewValues.begin(); it != mapOldNewValues.end(); it++)
	{
		StrReplace(str, it->first, it->second);
	}
	return str;
}

string eff::utils::LoadResourceString(UINT uID, ...)
{
	CString cStr;
	cStr.LoadStringA(uID);
	va_list args;
	va_start(args, uID);
	char (formattedMsg)[MSG_LEN] = {'\0'};
	_vsnprintf_s((formattedMsg), sizeof((formattedMsg)), (cStr), args);
	va_end(args);
	return string(formattedMsg);
}

string eff::utils::StrFormat(const char * msg, ...)
{
	FORMAT_MESSAGE(msg, formattedMsg);
	return string(formattedMsg);
}

vector<string> eff::utils::StrSplit(const char * str, const char * delimiter)
{
	vector<string> result;
	string s(str);
	string delimiterStr(delimiter);
	size_t pos = 0;
	string token;
	while((pos = s.find(delimiterStr)) != string::npos)
	{
		token = s.substr(0, pos);
		s.erase(0, pos + delimiterStr.length());
		result.push_back(token);
	}
	result.push_back(s);
	return result;
}
#include "SphTools/SphLoggerUtil.h"

#include "BindingFunctions.h"

//#include "SqlUtils.h"
//#include "StringUtils.h"
//#include "Config.h";

//using namespace eff::utils;
using namespace std;
using namespace eff::ToolkitReporting;

const char*  BfLastDayOfMonth::__CLASS__ = "BfLastDayOfMonth";
const char*  BfFirstDayOfMonth::__CLASS__ = "BfFirstDayOfMonth";
const char*  BfLastDayOfPreviousMonth::__CLASS__ = "BfLastDayOfPreviousMonth";
const char*  BfConcatenate::__CLASS__ = "BfConcatenate";

BindingFunctionImpl::BindingFunctionImpl(std::string functionName, int numOfParams, ...) {
	//DATE_FORMAT = Config::getSettingStr(TOOLKIT_SECTION, "DATE_FORMAT", "DD/MM/YYYY");
	DATE_FORMAT = "DD/MM/YYYY";
	SetFunctionName(functionName);
	va_list args;
	va_start(args, numOfParams);
	for (int i = 0; i < numOfParams; i++)
	{
		const char * paramName = va_arg(args, const char *);
		_parametersNames.push_back(string(paramName));
	}
	va_end(args);
}

string BfLastDayOfMonth::Execute(std::vector<std::string>& paramValues)
{
	BEGIN_LOG("Execute");
	string result;
	try {
		//string paramDate = paramValues[0];
		//result = SqlUtils::QueryReturning1StringException("SELECT ADD_MONTHS(TRUNC(TO_DATE('%s', '%s'), 'MM'), 1) - 1 FROM DUAL", paramDate.c_str(), DATE_FORMAT.c_str());
		paramValues[0] = "15/04/2018";
		result = "30/04/2018";
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to Execute function: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}

string BfFirstDayOfMonth::Execute(std::vector<std::string>& paramValues)
{
	BEGIN_LOG("Execute");
	string result;
	try {
		//string paramDate = paramValues[0];
		//result = SqlUtils::QueryReturning1StringException("SELECT TRUNC(TO_DATE('%s', '%s'), 'MM') FROM DUAL", paramDate.c_str(), DATE_FORMAT.c_str());
		paramValues[0] = "15/04/2018";
		result = "01/04/2018";
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to Execute function: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}

string BfLastDayOfPreviousMonth::Execute(std::vector<std::string>& paramValues)
{
	BEGIN_LOG("Execute");
	string result;
	try {
		//string paramDate = paramValues[0];
		//result = SqlUtils::QueryReturning1StringException("SELECT TRUNC(TO_DATE('%s', '%s'), 'MM') - 1 FROM DUAL", paramDate.c_str(), DATE_FORMAT.c_str());
		paramValues[0] = "15/04/2018";
		result = "31/03/2018";
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to Execute function: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}

string BfConcatenate::Execute(std::vector<std::string>& paramValues)
{
	BEGIN_LOG("Execute");
	string result;
	try {
		string str1 = paramValues[0];
		string str2 = paramValues[1];
		result = str1 + str2;
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to Execute function: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return result;
}
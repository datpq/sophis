#include "afx.h"
#include "Inc/Log.h"
#include "Inc/Config.h"
#include "Shlwapi.h";
#include "Inc/systemutils.h"
#include <algorithm>
#include <stdarg.h>

using namespace eff::utils;
using namespace std;

//#define CALL_LOG(logLevel, msg) \
//{ \
//	va_list args; \
//	va_start(args, msg); \
//	LogMsg((logLevel), (msg), args); \
//	va_end(args); \
//} \

Log * Log::m_instance = NULL;

string getLogLevelStr(int logLevel)
{
	switch(logLevel) {
	case LOG_LEVEL_COMM: return "COMM ";
	case LOG_LEVEL_DEBUG: return "DEBUG";
	case LOG_LEVEL_INFO: return "INFO ";
	case LOG_LEVEL_WARN: return "WARN ";
	case LOG_LEVEL_ERROR: return "ERROR";
	default: return "INFO";
	}
}

int getLogLevelInt()
{
	string sLogLevel = Config::getSettingStr(TOOLKIT_SECTION, "LogLevel", "INFO");
	transform(sLogLevel.begin(), sLogLevel.end(), sLogLevel.begin(), ::toupper);
	if (sLogLevel == "COMM") return LOG_LEVEL_COMM;
	if (sLogLevel == "DEBUG") return LOG_LEVEL_DEBUG;
	if (sLogLevel == "INFO") return LOG_LEVEL_INFO;
	if (sLogLevel == "WARN") return LOG_LEVEL_WARN;
	if (sLogLevel == "ERROR") return LOG_LEVEL_ERROR;
	return LOG_LEVEL_INFO;
}

Log::Log()
{
	m_LogLevel = getLogLevelInt();
	init();
}

Log * Log::getInstance()
{
	if (m_instance == NULL) {
		m_instance = new Log();
	}
	return m_instance;
}

//void Log::Comm(const char * msg, ...) CALL_LOG(LOG_LEVEL_COMM, msg)
//void Log::Debug(const char * msg, ...) CALL_LOG(LOG_LEVEL_DEBUG, msg)
//void Log::Info(const char * msg, ...) CALL_LOG(LOG_LEVEL_INFO, msg)
//void Log::Warn(const char * msg, ...) CALL_LOG(LOG_LEVEL_WARN, msg)
//void Log::Error(const char * msg, ...) CALL_LOG(LOG_LEVEL_ERROR, msg)

void Log::LogMsg(const char * caller, int level, const char * msg, ...)
{
	if (level >= m_LogLevel) {
		FORMAT_MESSAGE(msg, formattedMsg);
		string dateTime = GetCurDateTime("%Y-%m-%d %H:%M:%S");
		string sCaller(caller);
		m_LogFile << dateTime.c_str() << " " << getLogLevelStr(level) << " " << sCaller.substr(sCaller.find_last_of("::") + 1) << " " << formattedMsg << "\n";
		Flush();
	}
}

void Log::Flush()
{
	m_LogFile.flush();
	string curDateTime = GetCurDateTimeYYMMDD();
	if (m_FileNameDate != curDateTime) {
		m_LogFile.close();
		init();
	}
}

void Log::init()
{
	char fileName[MAX_PATH] = {'\0'};
	strcpy(fileName, Config::getInstance()->getDllFullPath().c_str());
	PathRemoveExtension(fileName);
	m_FileNameDate = GetCurDateTimeYYMMDD();
	string sFileName = string(fileName) + "_" + m_FileNameDate + ".log";
	m_LogFile.open(sFileName, ios_base::out | ios_base::app);
}

Log::~Log()
{
	if (m_LogFile.is_open()) {
		m_LogFile.flush();
		m_LogFile.close();
	}
}

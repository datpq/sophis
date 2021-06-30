#pragma once
#include <fstream>

#define LOG_LEVEL_COMM 0
#define LOG_LEVEL_DEBUG 1
#define LOG_LEVEL_INFO 2
#define LOG_LEVEL_WARN 3
#define LOG_LEVEL_ERROR 4

#define MSG_LEN 6000

#define FORMAT_MESSAGE(msg, formattedMsg) \
	va_list args; \
	va_start(args, msg); \
	char (formattedMsg)[MSG_LEN] = {'\0'}; \
	_vsnprintf_s((formattedMsg), sizeof((formattedMsg)), (msg), args); \
	va_end(args); \

#define COMM(msg, ...) eff::utils::Log::getInstance()->LogMsg(__FUNCTION__, LOG_LEVEL_COMM, msg, ##__VA_ARGS__)
#define DEBUG(msg, ...) eff::utils::Log::getInstance()->LogMsg(__FUNCTION__, LOG_LEVEL_DEBUG, msg, ##__VA_ARGS__)
#define INFO(msg, ...) eff::utils::Log::getInstance()->LogMsg(__FUNCTION__, LOG_LEVEL_INFO, msg, ##__VA_ARGS__)
#define WARN(msg, ...) eff::utils::Log::getInstance()->LogMsg(__FUNCTION__, LOG_LEVEL_WARN, msg, ##__VA_ARGS__)
#define ERROR(msg, ...) eff::utils::Log::getInstance()->LogMsg(__FUNCTION__, LOG_LEVEL_ERROR, msg, ##__VA_ARGS__)

namespace eff
{
	namespace utils
	{
		class Log
		{
		private:
			static Log * m_instance;
			Log();
			std::ofstream m_LogFile;
			int m_LogLevel;
			std::string m_FileNameDate;
			void init();
		public:
			//Singleton
			static Log * getInstance();
			void LogMsg(const char * caller, int level, const char * msg, ...);

			//void Comm(const char * msg, ...);
			//void Debug(const char * msg, ...);
			//void Info(const char * msg, ...);
			//void Warn(const char * msg, ...);
			//void Error(const char * msg, ...);
			void Flush();

			~Log();
		};
	}
}
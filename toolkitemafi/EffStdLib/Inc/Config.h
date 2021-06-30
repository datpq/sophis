#pragma once
#include <string>

#define TOOLKIT_SECTION "TOOLKIT"

namespace eff
{
	namespace utils
	{
		class Config
		{
		private:
			Config();
			static Config * m_instance;
			std::string m_DllFullPath;
		public:
			//Singleton
			static Config * getInstance();
			std::string getDllFullPath() { return m_DllFullPath; }
			void setDllFullPath(std::string dllFullPath) { m_DllFullPath = dllFullPath.c_str(); }
			std::string getDllDirectory();

			static std::string getSettingStr(const char * sectionName, const char * settingName, const char * defaultValue = NULL);
			static long getSettingLong(const char * sectionName, const char * settingName, long defaultValue = 0L);
			static bool getSettingBool(const char * sectionName, const char * settingName, bool defaultValue = false);
		};
	}
}

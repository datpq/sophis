#include "Inc/Config.h"
#include "afx.h"
#include "Shlwapi.h";

#if (SOPHISVER < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif

using namespace eff::utils;
using namespace std;
using namespace sophis::misc;

Config * Config::m_instance = NULL;

Config::Config() {}

Config * Config::getInstance()
{
	if (m_instance == NULL) {
		m_instance = new Config();
	}
	return m_instance;
}

string Config::getDllDirectory()
{
	char dllDirectory[MAX_PATH] = {'\0'};
	strcpy_s(dllDirectory, m_DllFullPath.c_str());
	PathRemoveFileSpec(dllDirectory);

	return string(dllDirectory);
}

string Config::getSettingStr(const char * sectionName, const char * settingName, const char * defaultValue)
{
	string result = "";
	ConfigurationFileWrapper::getEntryValue(sectionName, settingName, result, defaultValue);
	return result;
}

long Config::getSettingLong(const char * sectionName, const char * settingName, long defaultValue)
{
	long result = 0L;
	ConfigurationFileWrapper::getEntryValue(sectionName, settingName, result, defaultValue);
	return result;
}

bool Config::getSettingBool(const char * sectionName, const char * settingName, bool defaultValue)
{
	bool result = false;
	ConfigurationFileWrapper::getEntryValue(sectionName, settingName, result, defaultValue);
	return result;
}


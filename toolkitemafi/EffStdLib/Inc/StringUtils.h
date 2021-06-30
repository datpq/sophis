#include <string>
#include <map>
#include <vector>
#include <Windows.h>

namespace eff {
	namespace utils {
		std::string StrReplace(std::string& str, const char * oldValue, const char * newValue);
		std::string StrReplace(std::string& str, const std::string& oldValue, const std::string& newValue);
		std::string StrReplace(std::string& str, std::map<std::string, std::string> mapOldNewValues);
		std::string StrFormat(const char * msg, ...);
		std::vector<std::string> StrSplit(const char * str, const char * delimiter); 
		std::string LoadResourceString(UINT uID, ...);
	}
}

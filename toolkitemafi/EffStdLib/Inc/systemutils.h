#include <comdef.h>
#include <string>
#include <vector>

struct picture
{
	std::string fileName;
	int rowIndex;
	int cellIndex;
};
int OpenExcel(const char * fileName, const char* pdfFileName = 0, const char* excelFileName = 0, picture* pictures = 0);

std::string GetWindowsUserName();
std::string GetCurDateTime(const char * format);
std::string GetCurDateTimeYYMMDD();
std::string GetCurDateTimeYYMMDDHHMMSS();
std::string GetOpenFileDlg(const char * title, const char * filter, HWND owner = NULL);
std::vector<std::string> GetOpenFileDlgMulti(const char * title, const char * filter, HWND owner = NULL);
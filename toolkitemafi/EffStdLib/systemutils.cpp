#include "Inc/systemutils.h"
#include <windows.h>
#include <stdio.h>
#include <ole2.h> // OLE2 Definitions
#include <ctime>
#include <Lmcons.h>
#include "Inc\Config.h"
using namespace std;

// AutoWrap() - Automation helper function...
HRESULT AutoWrap(int autoType, VARIANT *pvResult, IDispatch *pDisp, LPOLESTR ptName, int cArgs...) {
    // Begin variable-argument list...
    va_list marker;
    va_start(marker, cArgs);

    if(!pDisp) {
        MessageBox(NULL, "NULL IDispatch passed to AutoWrap()", "Error", 0x10010);
        _exit(0);
    }

    // Variables used...
    DISPPARAMS dp = { NULL, NULL, 0, 0 };
    DISPID dispidNamed = DISPID_PROPERTYPUT;
    DISPID dispID;
    HRESULT hr;
    char buf[200];
    char szName[200];

    
    // Convert down to ANSI
    WideCharToMultiByte(CP_ACP, 0, ptName, -1, szName, 256, NULL, NULL);
    
    // Get DISPID for name passed...
    hr = pDisp->GetIDsOfNames(IID_NULL, &ptName, 1, LOCALE_USER_DEFAULT, &dispID);
    if(FAILED(hr)) {
        sprintf(buf, "IDispatch::GetIDsOfNames(\"%s\") failed w/err 0x%08lx", szName, hr);
        MessageBox(NULL, buf, "AutoWrap()", 0x10010);
        _exit(0);
        return hr;
    }
    
    // Allocate memory for arguments...
    VARIANT *pArgs = new VARIANT[cArgs+1];
    // Extract arguments...
    for(int i=0; i<cArgs; i++) {
        pArgs[i] = va_arg(marker, VARIANT);
    }
    
    // Build DISPPARAMS
    dp.cArgs = cArgs;
    dp.rgvarg = pArgs;
    
    // Handle special-case for property-puts!
    if(autoType & DISPATCH_PROPERTYPUT) {
        dp.cNamedArgs = 1;
        dp.rgdispidNamedArgs = &dispidNamed;
    }
    EXCEPINFO pExcep; //to read exception details in case of an exception thrown
    // Make the call!
    hr = pDisp->Invoke(dispID, IID_NULL, LOCALE_SYSTEM_DEFAULT, autoType, &dp, pvResult, &pExcep, NULL);
    if(FAILED(hr)) {
        sprintf(buf, "IDispatch::Invoke(\"%s\"=%08lx) failed w/err 0x%08lx", szName, dispID, hr);
        MessageBox(NULL, buf, "AutoWrap()", 0x10010);
        _exit(0);
        return hr;
    }
    // End variable-argument section...
    va_end(marker);
    
    delete [] pArgs;
    
    return hr;
}

void InsertImage(IDispatch *  pXlSheet, const char * strImagePath, int iRow, int iCol)//Image will set within the Range given
{
	VARIANT result;
	VariantInit(&result);

	/*VARIANT range;
	range.vt = VT_BSTR;
	range.bstrVal = ::SysAllocString(_com_util::ConvertStringToBSTR(strRange));*/

	VARIANT nRow;
	nRow.intVal = iRow;
	nRow.vt = VT_INT;

	VARIANT nCol;
	nCol.intVal = iCol;
	nCol.vt = VT_INT;
	AutoWrap(DISPATCH_PROPERTYGET, &result, pXlSheet, L"Cells", 2,nCol, nRow);
	IDispatch * pXlRange = result.pdispVal;

	double dTop,dLeft,dWidth,dHeight;
	VariantInit(&result);
	AutoWrap(DISPATCH_PROPERTYGET,&result,pXlRange,L"Top",0);
	dTop = result.dblVal;
	VariantInit(&result);
	AutoWrap(DISPATCH_PROPERTYGET,&result,pXlRange,L"Left",0);
	dLeft = result.dblVal;
	VariantInit(&result);

	IDispatch *pXlShapes;
	VariantInit(&result);
	AutoWrap(DISPATCH_PROPERTYGET, &result, pXlSheet, L"Shapes", 0);
	pXlShapes = result.pdispVal;

	VARIANT xprop;
	xprop.vt=VT_BOOL;
	xprop.boolVal=FALSE;
	VARIANT sratio;
	sratio.vt=VT_R4;
	sratio.fltVal=1.0;
	VariantInit(&result);

	VARIANT fname;
	fname.vt = VT_BSTR;
	fname.bstrVal=::SysAllocString(_com_util::ConvertStringToBSTR(strImagePath));
	VARIANT xpropf;
	xpropf.vt=VT_BOOL;
	xpropf.boolVal=TRUE;
	VARIANT xpropt;
	xpropt.vt=VT_BOOL;
	xpropt.boolVal=TRUE;
	VARIANT xtop;
	xtop.vt=VT_R8;
	xtop.dblVal= dTop;
	VARIANT xleft;
	xleft.vt=VT_R8;
	xleft.dblVal=dLeft;
	VARIANT xwidth;
	xwidth.vt=VT_R8;
	xwidth.dblVal=142.;
	VARIANT xheight;
	xheight.vt=VT_R8;
	xheight.dblVal=50.;

	AutoWrap(DISPATCH_METHOD, &result, pXlShapes, L"AddPicture", 7,xheight,xwidth,xtop,xleft,xpropt,xpropf,fname);
	IDispatch * pXlShape = result.pdispVal;
}

int OpenExcel(BSTR fileName, BSTR pdfFileName = 0, BSTR excelFileName = 0,picture* pictures = 0)
{
	// Initialize COM for this thread...
	CoInitialize(NULL);

	// Get CLSID for our server...
	CLSID clsid;
	HRESULT hr = CLSIDFromProgID(L"Excel.Application", &clsid);

	if(FAILED(hr)) {
		//::MessageBox(NULL, "CLSIDFromProgID() failed", "Error", 0x10010); // Excel not installed --> no error
		return -1;
	}

	// Start server and get IDispatch...
	IDispatch *pXlApp;
	hr = CoCreateInstance(clsid, NULL, CLSCTX_LOCAL_SERVER, IID_IDispatch, (void **)&pXlApp);
	if(FAILED(hr)) {
		::MessageBox(NULL, "Excel not registered properly", "Error", 0x10010);
		return -2;
	}

   // Make it visible (i.e. app.visible = 1) only if we want to generate only excel file
   
     /* VARIANT x;
      x.vt = VT_I4;
      x.lVal = 1;
      AutoWrap(DISPATCH_PROPERTYPUT, NULL, pXlApp, L"Visible", 1, x);*/

	// Get Workbooks collection
	IDispatch *pXlBooks;
	{
		VARIANT result;
		VariantInit(&result);
		AutoWrap(DISPATCH_PROPERTYGET, &result, pXlApp, L"Workbooks", 0);
		pXlBooks = result.pdispVal;
	}
	VARIANT fname;
	VariantInit(&fname);
	// Tell Excel to quit (i.e. App.Quit)
	//AutoWrap(DISPATCH_METHOD, NULL, pXlApp, L"Quit", 0);
	IDispatch * pXlBook;
	{
		VARIANT result;
		VariantInit(&result);

		fname.vt = VT_BSTR;
		fname.bstrVal=::SysAllocString(fileName);
		AutoWrap(DISPATCH_METHOD, &result, pXlBooks, L"Open", 1, fname);
		pXlBook = result.pdispVal;
		// insert LOGO efficiency 
		if(pictures)
		{
			VARIANT result;
			VariantInit(&result);
			AutoWrap(DISPATCH_PROPERTYGET, &result, pXlBook, L"Sheets",0);
			IDispatch * pXlSheets = result.pdispVal;

			VariantInit(&result);
			AutoWrap(DISPATCH_PROPERTYGET, &result, pXlSheets, L"Count",0);
			int nCount = result.intVal;

			//string fileName = pictures[i].fileName//eff::utils::Config::getInstance()->getDllDirectory() + "\\eff.png";
			IDispatch *pXlSheet;
			{
				VARIANT result;
				VariantInit(&result);
				VARIANT item_sheet;
				item_sheet.vt = VT_I4;
				for(int i = 0;i<nCount;i++)
				{
					if (pictures[i].rowIndex == -1 || pictures[i].cellIndex == -1) continue;
					item_sheet.lVal = i+1;
					AutoWrap(DISPATCH_PROPERTYGET, &result, pXlSheets, L"Item", 1, item_sheet);
					pXlSheet = result.pdispVal;
					InsertImage(pXlSheet,pictures[i].fileName.c_str(), pictures[i].rowIndex,pictures[i].cellIndex);
				}
			}
		}
	}

	if(excelFileName != 0)
	{
		VARIANT outFile;
		outFile.vt = VT_BSTR;
		outFile.bstrVal = excelFileName;
		VARIANT missingType = vtMissing;

		VARIANT fileType;
		fileType.vt = VT_INT;
		fileType.intVal = 51;
		AutoWrap(DISPATCH_METHOD, NULL, pXlBook, L"SaveAs", 7,missingType,missingType,missingType,missingType,missingType,fileType,outFile);
		AutoWrap(DISPATCH_METHOD, NULL, pXlBooks, L"Close", 0);
		AutoWrap(DISPATCH_METHOD, NULL, pXlApp, L"Quit", 0);

		hr = CoCreateInstance(clsid, NULL, CLSCTX_LOCAL_SERVER, IID_IDispatch, (void **)&pXlApp);
		if(FAILED(hr)) {
			::MessageBox(NULL, "Excel not registered properly", "Error", 0x10010);
			return -2;
		}
		VARIANT result;
		VariantInit(&result);
		AutoWrap(DISPATCH_PROPERTYGET, &result, pXlApp, L"Workbooks", 0);
		pXlBooks = result.pdispVal;
		VARIANT x;
		x.vt = VT_I4;
		x.lVal = 1;
		AutoWrap(DISPATCH_PROPERTYPUT, NULL, pXlApp, L"Visible", 1, x);
		AutoWrap(DISPATCH_METHOD, &result, pXlBooks, L"Open", 1, outFile);
		pXlBook = result.pdispVal;

		if(pdfFileName != 0)
		{
			VARIANT exportType;
			exportType.vt = VT_INT;
			exportType.intVal = 0;

			VARIANT outFile;
			VariantInit(&outFile);
			outFile.vt = VT_BSTR;

			outFile.bstrVal = pdfFileName;

			VARIANT missingType = vtMissing;

			VARIANT openAfterPublish;
			openAfterPublish.vt = VT_BOOL;
			openAfterPublish.boolVal = true;

			AutoWrap(DISPATCH_METHOD, NULL, pXlBook, L"ExportAsFixedFormat", 8,openAfterPublish,missingType,missingType,missingType,missingType,missingType,outFile,exportType);
			AutoWrap(DISPATCH_METHOD, NULL, pXlBooks, L"Close", 0);
			const char* fileNameStr = _com_util::ConvertBSTRToString(fileName);
			remove(fileNameStr);
			// Tell Excel to quit (i.e. App.Quit)
			AutoWrap(DISPATCH_METHOD, NULL, pXlApp, L"Quit", 0);
		}
	}

	pXlBooks->Release();
	pXlApp->Release();

	// Uninitialize COM for this thread...
	CoUninitialize();

	return 1;
}
	

int OpenExcel(const char * fileName, const char* pdfFileName, const char* excelFileName, picture* pictures)
{
	BSTR bstrFileName = _com_util::ConvertStringToBSTR(fileName);
	
	BSTR bstrPdfFileName = pdfFileName != 0 ? _com_util::ConvertStringToBSTR(pdfFileName) : 0;

	BSTR bstrExcelFileName = excelFileName != 0 ? _com_util::ConvertStringToBSTR(excelFileName) : 0;
	return OpenExcel(bstrFileName,bstrPdfFileName,bstrExcelFileName,pictures);
}

string GetWindowsUserName()
{
	char username[UNLEN+1];
	//wchar_t username[UNLEN + 1];
	DWORD username_len = UNLEN+1;
	GetUserName(username, &username_len);
	//std::wstring ws(username);
	//return string(ws.begin(), ws.end());
	return string(username);
}

string GetCurDateTime(const char * format)
{
	time_t rawtime;
	struct tm * timeinfo;
	char buffer[80];

	time(&rawtime);
	timeinfo = localtime(&rawtime);

	strftime(buffer,sizeof(buffer), format, timeinfo);
	string str(buffer);
	return str;
}

string GetCurDateTimeYYMMDD()
{
	return GetCurDateTime("%y%m%d");
}

string GetCurDateTimeYYMMDDHHMMSS()
{
	return GetCurDateTime("%y%m%d%H%M%S");
}

string GetOpenFileDlg(const char * filter, const char * title, HWND owner)
{
	char filename[1000] = {'\0'};
	OPENFILENAME ofn;
	ZeroMemory(&filename, sizeof(filename));
	ZeroMemory(&ofn, sizeof(ofn));
	ofn.lStructSize = sizeof(ofn);
	ofn.hwndOwner = owner;  // If you have a window to center over, put its HANDLE here
	ofn.lpstrFilter  = filter;
	ofn.lpstrFile    = filename;
	ofn.nMaxFile     = MAX_PATH;
	ofn.lpstrTitle   = title;
	ofn.Flags        = OFN_DONTADDTORECENT | OFN_FILEMUSTEXIST | OFN_EXPLORER;

	if (GetOpenFileNameA(&ofn))
	{
		return string(filename);
	}
	return string();
}

vector<string> GetOpenFileDlgMulti(const char * filter, const char * title, HWND owner)
{
	char filename[1000] = {'\0'};
	OPENFILENAME ofn;
	ZeroMemory(&filename, sizeof(filename));
	ZeroMemory(&ofn, sizeof(ofn));
	ofn.lStructSize  = sizeof(ofn);
	ofn.hwndOwner    = owner;  // If you have a window to center over, put its HANDLE here
	ofn.lpstrFilter  = filter;
	ofn.lpstrFile    = filename;
	ofn.nMaxFile     = MAX_PATH;
	ofn.lpstrTitle   = title;
	ofn.Flags        = OFN_DONTADDTORECENT | OFN_FILEMUSTEXIST | OFN_EXPLORER | OFN_ALLOWMULTISELECT;

	vector<string> result;
	if (GetOpenFileNameA(&ofn))
	{
		char* str = filename;
		while (*str) {
			string elem = string(str);
			str += (elem.length() + 1);
			result.push_back(elem);
		}
	}
	return result;
}
#include "RapportGeneral.h"
#include "Log.h"
#include "msxmlutils.h"
#include "SqlUtils.h"
#include "SqlWrapper.h"
#include "StringUtils.h"
#include <string>
#include "Config.h"
#define BUFF_LEN 255

using namespace eff::emafi::reports;
using namespace eff::utils;
using namespace std;

RapportGeneral::RapportGeneral(const char * xmlTemplateFile, char etat_type) : etat_type(etat_type)
{
	pXMLDom = NULL;
	sXmlTemplateFile = _com_util::ConvertStringToBSTR(xmlTemplateFile);
	
	loadTemplateFile();
}

IXMLDOMNode* RapportGeneral::getWorkSheetNodes(int i)
{
	return pWSNodes[i];
}

void RapportGeneral::creatWorksheet(const char * worksheetName, const char * newWorksheetName)
{
	if (initialWS.find(worksheetName) != initialWS.end()) {
		IXMLDOMNode *newWorkSheet=NULL;
		initialWS[worksheetName]->cloneNode(VARIANT_TRUE, &newWorkSheet);//append new worksheet
		pWorkbook->appendChild(newWorkSheet, &newWorkSheet);
		pWSNodes.push_back(newWorkSheet);

		setNodeAttribute(pXMLDom, newWorkSheet, L"ss:Name", _com_util::ConvertStringToBSTR(newWorksheetName));
		SAFE_RELEASE(newWorkSheet);
	}
}

void RapportGeneral::LoadParamValues(map<string, string> arrParamValues)
{
	XmlReplace(pXMLDom, arrParamValues);
	map<string, IXMLDOMNode*>::iterator it;
	for (it = initialWS.begin(); it != initialWS.end(); it++)
	{
		XmlReplace(it->second, arrParamValues);
	}
}

void RapportGeneral::loadTemplateFile()
{
	DEBUG("BEGIN");

	HRESULT hr = S_OK;
	VARIANT_BOOL varStatus;
	VARIANT varFileNameSrc;
	VariantInit(&varFileNameSrc);

	CHK_HR(CreateAndInitDOM(&pXMLDom));
	// XML file name to load
	CHK_HR(VariantFromString(sXmlTemplateFile, varFileNameSrc));
	CHK_HR(pXMLDom->load(varFileNameSrc, &varStatus));

	if (varStatus != VARIANT_TRUE)
	{
		char sMsg[BUFF_LEN] = {'\0'};
		_snprintf_s(sMsg, BUFF_LEN, "Failed to load DOM from %S.", sXmlTemplateFile);
		CHK_HR(ReportParseError(pXMLDom, sMsg));
	}

	pXMLDom->get_lastChild(&pWorkbook);
		
	//7.1.3
	//try {
		SqlWrapper* sqlWrapper = new SqlWrapper("c50", StrFormat("SELECT DISTINCT WORKSHEET FROM EMAFI_REPORT WHERE ETAT_TYPE = '%c' AND ENABLED = 1", etat_type).c_str());
		for(int i=0; i<sqlWrapper->GetRowCount(); i++) {
			string xPath = StrFormat("//*[local-name()='Worksheet'][@*[local-name()='Name' and .='%s']]", (*sqlWrapper)[i][0].value<string>().c_str());
			IXMLDOMNode * pNode = NULL;
			GetFirstNodeByXPath(pWorkbook, xPath.c_str(), &pNode);
			pWorkbook->removeChild(pNode, NULL);
			initialWS[(*sqlWrapper)[i][0].value<string>()] = pNode;
		}

	//} catch (const sophis::sql::OracleException &ex) {
	//	throw;
	//}
	DEBUG("END");
CleanUp:
	VariantClear(&varFileNameSrc);
}

void RapportGeneral::save(const char * xmlFileDest)
{
	//before saving we must remove initial worksheets
	IXMLDOMNode* pWorkBook = NULL;
	pXMLDom->get_lastChild(&pWorkBook);
	map<string,IXMLDOMNode*>::iterator it;
	for(it = initialWS.begin(); it != initialWS.end(); it++)
	{
		pWorkBook->removeChild(it->second, NULL);
	}

	BSTR sXmlFileDest = _com_util::ConvertStringToBSTR(xmlFileDest);
	HRESULT hr = S_OK;
	VARIANT varFileNameDest;
	VariantInit(&varFileNameDest);
	VariantFromString(sXmlFileDest, varFileNameDest);
	HRESULT h = pXMLDom->save(varFileNameDest);
CleanUp:
	//VariantClear(&varFileNameDest);
	SAFE_RELEASE(pWorkBook);
}

void RapportGeneral::getLogoPosition(int sheetIndex, int& rowIndex, int& colIndex)
{
	IXMLDOMNodeList* pNodes = 0;
	HRESULT hr = S_OK;
	IXMLDOMNode* pLogo = 0;
	CHK_HR(pWSNodes[sheetIndex]->selectNodes(L".//*[text()='{LOGO}']", &pNodes));
	if (pNodes) 
	{
		CHK_HR(pNodes->get_item(0, &pLogo));//<Data />

		if (pLogo == NULL) {
			rowIndex = colIndex = -1;
			return;
		}
		pLogo->get_parentNode(&pLogo);
		SAFE_RELEASE(pNodes);
	}
	
	getCellCoordinates(pLogo,rowIndex,colIndex);

	IXMLDOMNode* pLogoRow = 0;
	pLogo->get_parentNode(&pLogoRow);
	if(pLogoRow)
		pLogoRow->removeChild(pLogo,NULL);
	SAFE_RELEASE(pLogo);
CleanUp:
	SAFE_RELEASE(pNodes);
	
}
RapportGeneral::~RapportGeneral(void)
{
CleanUp:
	SAFE_RELEASE(pXMLDom);
}

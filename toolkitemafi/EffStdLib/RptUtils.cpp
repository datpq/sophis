#ifndef REPORT_UTILS_CPP
#define REPORTS_UTILS_CPP
#include "Inc\RptUtils.h"
#include "Inc\StringUtils.h"
#include<sstream>

using namespace std;
using namespace eff::utils;

void eff::utils::report::setParameterValue(IXMLDOMNode* pWorksheet, const char * paramName, const char * paramValue)
{
	HRESULT hr = S_OK;
	IXMLDOMNodeList *pNodes = NULL;
	IXMLDOMNode *pNode = NULL;
	BSTR bstr;
	string xPath = string(".//*[contains(text(),'{{") + paramName + "}}')]";
	BSTR bstrXPath = _com_util::ConvertStringToBSTR(xPath.c_str());
	pWorksheet->selectNodes(bstrXPath, &pNodes);
	if (pNodes != NULL) {
		long length;
		CHK_HR(pNodes->get_length(&length));
		bstr = _com_util::ConvertStringToBSTR(paramValue);
		for(long i=0; i<length; i++) {
			CHK_HR(pNodes->get_item(i, &pNode));
			pNode->get_text(&bstr);
			string sNodeTxt = _com_util::ConvertBSTRToString(bstr);
			StrReplace(sNodeTxt, string("{{") + paramName + "}}", paramValue);
			bstr = _com_util::ConvertStringToBSTR(sNodeTxt.c_str());
			pNode->put_text(bstr);
			SAFE_RELEASE(pNode);
		}
		SysFreeString(bstr);
	}
	SAFE_RELEASE(pNodes);
CleanUp:
	SAFE_RELEASE(pNodes);
	SAFE_RELEASE(pNode);
	SysFreeString(bstr);
}

void eff::utils::report::loadOneParameterValue(IXMLDOMNode* pWorksheet, const char * oneParamValue)
{
	string sOneParamValue = oneParamValue;
	size_t pos = sOneParamValue.find_first_of("=");
	if (pos != string::npos) {
		string param = sOneParamValue.substr(0, pos);
		string value = sOneParamValue.substr(pos + 1);
		setParameterValue(pWorksheet,param.c_str(), value.c_str());
	}
}

void eff::utils::report::loadParameterValues(IXMLDOMNode* pWorksheet, const char * paramValues)
{
	vector<string> pv = StrSplit(paramValues, ";");
	for (vector<string>::iterator it = pv.begin() ; it != pv.end(); ++it)
	{
		loadOneParameterValue(pWorksheet, it->c_str());
	}
}

void eff::utils::report::setCellFormula(IXMLDOMDocument* pXMLNode, IXMLDOMNode *pCellNode, const char * formula)
{
	BSTR bstr = _com_util::ConvertStringToBSTR(formula);
	setNodeAttribute(pXMLNode,pCellNode, L"ss:Formula", bstr);
CleanUp:
	SysFreeString(bstr);
}

void eff::utils::report::setRowValue(IXMLDOMDocument* pXMLNode, IXMLDOMNode *pNodeRow, CComVariant nativeValue, int nodeIdx, int mergeRow)
{
	IXMLDOMNode *pNode = NULL;
	if (nodeIdx < 0) {
		pNodeRow->get_lastChild(&pNode);
		for (int i=nodeIdx+1; i < 0; i++) {
			pNode->get_previousSibling(&pNode);
		}
	} else if (nodeIdx > 0) {
		pNodeRow->get_firstChild(&pNode);
		for (int i=nodeIdx-1; i>0; i--) {
			pNode->get_nextSibling(&pNode);
		}
	}

	stringstream ss;
	ss << mergeRow;
	BSTR bstr = _com_util::ConvertStringToBSTR(ss.str().c_str());
	setNodeAttribute(pXMLNode,pNode,L"ss:MergeDown",bstr);
	
	pNode->get_firstChild(&pNode);
	pNode->put_nodeTypedValue(nativeValue);
CleanUp:
	SAFE_RELEASE(pNode);
}

void eff::utils::report::setCellValue(IXMLDOMNode* pNodeCell, CComVariant nativeValue)
{
	IXMLDOMNode *pNode = NULL;
    pNodeCell->get_firstChild(&pNode);
	pNode->put_nodeTypedValue(nativeValue);
CleanUp:
	SAFE_RELEASE(pNode);
}

void eff::utils::report::loadTemplateNode(IXMLDOMNode *pWorksheet, const char * nodeName, IXMLDOMNode **pTemplateNodeRow, IXMLDOMNode **pTemplateNodeCell)
{
	HRESULT hr = S_OK;
	IXMLDOMNodeList *pNodes = NULL;
	IXMLDOMNode *pNode = NULL;
	pWorksheet->selectNodes(_com_util::ConvertStringToBSTR((string(".//*[text()='") + nodeName + "']").c_str()), &pNodes);
	if (pNodes) {
		CHK_HR(pNodes->get_item(0, &pNode));//<Data />
		if (pNode) {
			pNode->get_parentNode(pTemplateNodeCell); //<Cell />
			(*pTemplateNodeCell)->get_parentNode(pTemplateNodeRow); //<Row />
			(*pTemplateNodeRow)->removeChild(*pTemplateNodeCell, NULL);
		}
	}
CleanUp:
	SAFE_RELEASE(pNode);
	SAFE_RELEASE(pNodes);
}

#endif
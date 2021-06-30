#include <stdio.h>
#include<string>
#include "Inc/msxmlutils.h"
#include "Inc/StringUtils.h"

using namespace eff::utils;
using namespace std;

// Helper function to create a DOM instance. 
HRESULT CreateAndInitDOM(IXMLDOMDocument **ppDoc)
{
    HRESULT hr = CoCreateInstance(__uuidof(DOMDocument60), NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(ppDoc));
    if (SUCCEEDED(hr))
    {
        // these methods should not fail so don't inspect result
        (*ppDoc)->put_async(VARIANT_FALSE);  
        (*ppDoc)->put_validateOnParse(VARIANT_FALSE);
        (*ppDoc)->put_resolveExternals(VARIANT_FALSE);
    }
    return hr;
}

// Helper function to create a VT_BSTR variant from a null terminated string. 
HRESULT VariantFromString(PCWSTR wszValue, VARIANT &Variant)
{
    HRESULT hr = S_OK;
    BSTR bstrString = SysAllocString(wszValue);
    CHK_ALLOC(bstrString);

    V_VT(&Variant)   = VT_BSTR;
    V_BSTR(&Variant) = bstrString;

CleanUp:
    return hr;
}

// Helper function to load xml from file. 
HRESULT LoadXMLFile(IXMLDOMDocument *pXMLDom, LPCWSTR lpszXMLFile)
{
    HRESULT hr = S_OK;
    VARIANT_BOOL varStatus;
    VARIANT varFileName;
    IXMLDOMParseError *pXMLErr=NULL;
    BSTR bstrErr = NULL;

    VariantInit(&varFileName);
    CHK_HR(VariantFromString(lpszXMLFile, varFileName));
    CHK_HR(pXMLDom->load(varFileName, &varStatus));

    //load xml failed
    if(varStatus != VARIANT_TRUE)
    {
        hr = E_FAIL;
        CHK_HR(pXMLDom->get_parseError(&pXMLErr));
        CHK_HR(pXMLErr->get_reason(&bstrErr));
        printf("Failed to load %S:\n%S\n", lpszXMLFile, bstrErr);
    }

CleanUp:
    SAFE_RELEASE(pXMLErr);
    SysFreeString(bstrErr);
    VariantClear(&varFileName);
    return hr;
}

// Helper that allocates the BSTR param for the caller.
HRESULT CreateElement(IXMLDOMDocument *pXMLDom, PCWSTR wszName, IXMLDOMElement **ppElement)
{
    HRESULT hr = S_OK;
    *ppElement = NULL;

    BSTR bstrName = SysAllocString(wszName);
    CHK_ALLOC(bstrName);
    CHK_HR(pXMLDom->createElement(bstrName, ppElement));

CleanUp:
    SysFreeString(bstrName);
    return hr;
}

// Helper function to append a child to a parent node.
HRESULT AppendChildToParent(IXMLDOMNode *pChild, IXMLDOMNode *pParent)
{
    HRESULT hr = S_OK;
    IXMLDOMNode *pChildOut = NULL;
    CHK_HR(pParent->appendChild(pChild, &pChildOut));

CleanUp:
    SAFE_RELEASE(pChildOut);
    return hr;
}

// Helper function to create and add a processing instruction to a document node.
HRESULT CreateAndAddPINode(IXMLDOMDocument *pDom, PCWSTR wszTarget, PCWSTR wszData)
{
    HRESULT hr = S_OK;
    IXMLDOMProcessingInstruction *pPI = NULL;

    BSTR bstrTarget = SysAllocString(wszTarget);
    BSTR bstrData = SysAllocString(wszData);
    CHK_ALLOC(bstrTarget && bstrData);
    
    CHK_HR(pDom->createProcessingInstruction(bstrTarget, bstrData, &pPI));
    CHK_HR(AppendChildToParent(pPI, pDom));

CleanUp:
    SAFE_RELEASE(pPI);
    SysFreeString(bstrTarget);
    SysFreeString(bstrData);
    return hr;
}

// Helper function to create and add a comment to a document node.
HRESULT CreateAndAddCommentNode(IXMLDOMDocument *pDom, PCWSTR wszComment)
{
    HRESULT hr = S_OK;
    IXMLDOMComment *pComment = NULL;

    BSTR bstrComment = SysAllocString(wszComment);
    CHK_ALLOC(bstrComment);
    
    CHK_HR(pDom->createComment(bstrComment, &pComment));
    CHK_HR(AppendChildToParent(pComment, pDom));

CleanUp:
    SAFE_RELEASE(pComment);
    SysFreeString(bstrComment);
    return hr;
}

// Helper function to create and add an attribute to a parent node.
HRESULT CreateAndAddAttributeNode(IXMLDOMDocument *pDom, PCWSTR wszName, PCWSTR wszValue, IXMLDOMElement *pParent)
{
    HRESULT hr = S_OK;
    IXMLDOMAttribute *pAttribute = NULL;
    IXMLDOMAttribute *pAttributeOut = NULL; // Out param that is not used

    BSTR bstrName = NULL;
    VARIANT varValue;
    VariantInit(&varValue);

    bstrName = SysAllocString(wszName);
    CHK_ALLOC(bstrName);
    CHK_HR(VariantFromString(wszValue, varValue));

    CHK_HR(pDom->createAttribute(bstrName, &pAttribute));
    CHK_HR(pAttribute->put_value(varValue));
    CHK_HR(pParent->setAttributeNode(pAttribute, &pAttributeOut));

CleanUp:
    SAFE_RELEASE(pAttribute);
    SAFE_RELEASE(pAttributeOut);
    SysFreeString(bstrName);
    VariantClear(&varValue);
    return hr;
}

// Helper function to create and append a text node to a parent node.
HRESULT CreateAndAddTextNode(IXMLDOMDocument *pDom, PCWSTR wszText, IXMLDOMNode *pParent)
{
    HRESULT hr = S_OK;    
    IXMLDOMText *pText = NULL;

    BSTR bstrText = SysAllocString(wszText);
    CHK_ALLOC(bstrText);

    CHK_HR(pDom->createTextNode(bstrText, &pText));
    CHK_HR(AppendChildToParent(pText, pParent));

CleanUp:
    SAFE_RELEASE(pText);
    SysFreeString(bstrText);
    return hr;
}

// Helper function to create and append a CDATA node to a parent node.
HRESULT CreateAndAddCDATANode(IXMLDOMDocument *pDom, PCWSTR wszCDATA, IXMLDOMNode *pParent)
{
    HRESULT hr = S_OK;
    IXMLDOMCDATASection *pCDATA = NULL;

    BSTR bstrCDATA = SysAllocString(wszCDATA);
    CHK_ALLOC(bstrCDATA);

    CHK_HR(pDom->createCDATASection(bstrCDATA, &pCDATA));
    CHK_HR(AppendChildToParent(pCDATA, pParent));

CleanUp:
    SAFE_RELEASE(pCDATA);
    SysFreeString(bstrCDATA);
    return hr;
}

// Helper function to create and append an element node to a parent node, and pass the newly created
// element node to caller if it wants.
HRESULT CreateAndAddElementNode(IXMLDOMDocument *pDom, PCWSTR wszName, PCWSTR wszNewline, IXMLDOMNode *pParent, IXMLDOMElement **ppElement)
{
    HRESULT hr = S_OK;
    IXMLDOMElement* pElement = NULL;

    CHK_HR(CreateElement(pDom, wszName, &pElement));
    // Add NEWLINE+TAB for identation before this element.
    CHK_HR(CreateAndAddTextNode(pDom, wszNewline, pParent));
    // Append this element to parent.
    CHK_HR(AppendChildToParent(pElement, pParent));

CleanUp:
    if (ppElement)
        *ppElement = pElement;  // Caller is repsonsible to release this element.
    else
        SAFE_RELEASE(pElement); // Caller is not interested on this element, so release it.

    return hr;
}

// Helper function to display parse error.
// It returns error code of the parse error.
HRESULT ReportParseError(IXMLDOMDocument *pDoc, char *szDesc)
{
    HRESULT hr = S_OK;
    HRESULT hrRet = E_FAIL; // Default error code if failed to get from parse error.
    IXMLDOMParseError *pXMLErr = NULL;
    BSTR bstrReason = NULL;

    CHK_HR(pDoc->get_parseError(&pXMLErr));
    CHK_HR(pXMLErr->get_errorCode(&hrRet));
    CHK_HR(pXMLErr->get_reason(&bstrReason));
    printf("%s\n%S\n", szDesc, bstrReason);

CleanUp:
    SAFE_RELEASE(pXMLErr);
    SysFreeString(bstrReason);
    return hrRet;
}

void GetParentNode(IXMLDOMNode *pNode, IXMLDOMNode **parent, int step)
{
	*parent = pNode;
	for(int i=0; i<step; i++) {
		(*parent)->get_parentNode(parent);
	}
}

void getNodeAttribute(IXMLDOMNode *pNode, BSTR attribute, BSTR *value)
{
	IXMLDOMNamedNodeMap *pXMLNamedNodeMap = NULL;
	IXMLDOMNode *pXMLAttr = NULL;
	pNode->get_attributes(&pXMLNamedNodeMap);
	pXMLNamedNodeMap->getNamedItem(attribute, &pXMLAttr);
	if(pXMLAttr) {
		pXMLAttr->get_text(value);
	} else {
		value = NULL;
	}
CleanUp:
	SAFE_RELEASE(pXMLAttr);
	SAFE_RELEASE(pXMLNamedNodeMap);
}

void setNodeAttribute(IXMLDOMDocument* pXMLNode, IXMLDOMNode *pNode, BSTR attribute, BSTR value)
{
	IXMLDOMNamedNodeMap *pXMLNamedNodeMap = NULL;
	IXMLDOMNode *pXMLAttr = NULL;
	pNode->get_attributes(&pXMLNamedNodeMap);
	pXMLNamedNodeMap->getNamedItem(attribute, &pXMLAttr);

	if(pXMLAttr == NULL) /*adding the attribute in case it doesn't exist*/
	{
		IXMLDOMAttribute* pAttr = NULL;
		pXMLNode->createAttribute(attribute,&pAttr);
		pAttr->put_text(value);
		pXMLNamedNodeMap->setNamedItem(pAttr,NULL);

		SAFE_RELEASE(pAttr);
	}
	else
		pXMLAttr->put_text(value);

CleanUp:
	SAFE_RELEASE(pXMLAttr);
	SAFE_RELEASE(pXMLNamedNodeMap);
}

void getRelativePosition(IXMLDOMNode* pGeneralNode, int& pos,const char* nodeName_)
{
	IXMLDOMNode* pNode = pGeneralNode;
	pos = 0;
	BSTR index = NULL;
	BSTR nodeName = NULL;
	while(pNode != NULL)
	{
		if(pNode)
			getNodeAttribute(pNode,L"ss:Index",&index);
		pNode->get_nodeName(&nodeName);

		if(index != NULL || _com_util::ConvertBSTRToString(nodeName) != string(nodeName_))
			break;
		pNode->get_previousSibling(&pNode);
		pos++;
	}

	if(index)
		pos+=_wtoi(index);
}

void getCellCoordinates(IXMLDOMNode* pCellNode, int& iRow, int& iCol)
{
	getRelativePosition(pCellNode,iCol,"Cell");

	IXMLDOMNode* pRowNode = 0;
	pCellNode->get_parentNode(&pRowNode);
	if(pRowNode)
		getRelativePosition(pRowNode,iRow,"Row");

	SAFE_RELEASE(pRowNode);
}

void GetFirstNodeByXPath(IXMLDOMNode* pXMLNode, const char * xPath, IXMLDOMNode **pNode)
{
	IXMLDOMNodeList * pNodeList = NULL;
	pXMLNode->selectNodes(_com_util::ConvertStringToBSTR(xPath), &pNodeList);
	if (pNodeList) {
		pNodeList->get_item(0, pNode);
	}
	SAFE_RELEASE(pNodeList);
}

void XmlReplace(IXMLDOMNode* pXMLNode, const char * oldValue, const char * newValue)
{
	IXMLDOMNodeList * pNodeList = NULL;
	pXMLNode->selectNodes(_com_util::ConvertStringToBSTR(StrFormat("//*[contains(text(),'%s')]", oldValue).c_str()), &pNodeList);
	IXMLDOMNode * pNode = NULL;
	pNodeList->nextNode(&pNode);
	while (pNode) {
		BSTR bstrTxt;
		pNode->get_text(&bstrTxt);
		string sTxt = _com_util::ConvertBSTRToString(bstrTxt);
		StrReplace(sTxt, oldValue, newValue);
		pNode->put_text(_com_util::ConvertStringToBSTR(sTxt.c_str()));
		pNodeList->nextNode(&pNode);
	}
	SAFE_RELEASE(pNode);
	SAFE_RELEASE(pNodeList);
}

void XmlReplace(IXMLDOMNode* pXMLNode, std::map<std::string, std::string> mapOldNewValues)
{
	map<string, string>::iterator it;
	for (it = mapOldNewValues.begin(); it != mapOldNewValues.end(); it++)
	{
		XmlReplace(pXMLNode, it->first.c_str(), it->second.c_str());
	}
}

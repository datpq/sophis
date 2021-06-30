#ifndef MSXML_UTILS
#define MSXML_UTILS
#include <msxml6.h>
#include <comdef.h>
#include <atlbase.h> // Includes CComVariant and CComBSTR.
#include <map>

// Macro that calls a COM method returning HRESULT value.
#define CHK_HR(stmt)        do{ hr=(stmt); if (FAILED(hr)) goto CleanUp; } while(0)

// Macro to verify memory allcation.
#define CHK_ALLOC(p)        do { if (!(p)) { hr = E_OUTOFMEMORY; goto CleanUp; } } while(0)

// Macro that releases a COM object if not NULL.
#define SAFE_RELEASE(p)     do { if ((p)) { (p)->Release(); (p) = NULL; } } while(0)

HRESULT VariantFromString(PCWSTR wszValue, VARIANT &Variant);
HRESULT CreateAndInitDOM(IXMLDOMDocument **ppDoc);
HRESULT LoadXMLFile(IXMLDOMDocument *pXMLDom, LPCWSTR lpszXMLFile);
//HRESULT TransformDOM2Str(IXMLDOMDocument *pXMLDom, IXMLDOMDocument *pXSLDoc);
//HRESULT TransformDOM2Str(IXMLDOMDocument *pXMLDom, IXMLDOMDocument *pXSLDoc, BSTR *bstrResult);
//HRESULT TransformDOM2Obj(IXMLDOMDocument *pXMLDom, IXMLDOMDocument *pXSLDoc);

HRESULT CreateElement(IXMLDOMDocument *pXMLDom, PCWSTR wszName, IXMLDOMElement **ppElement);
HRESULT AppendChildToParent(IXMLDOMNode *pChild, IXMLDOMNode *pParent);
HRESULT CreateAndAddPINode(IXMLDOMDocument *pDom, PCWSTR wszTarget, PCWSTR wszData);
HRESULT CreateAndAddCommentNode(IXMLDOMDocument *pDom, PCWSTR wszComment);
HRESULT CreateAndAddAttributeNode(IXMLDOMDocument *pDom, PCWSTR wszName, PCWSTR wszValue, IXMLDOMElement *pParent);
HRESULT CreateAndAddTextNode(IXMLDOMDocument *pDom, PCWSTR wszText, IXMLDOMNode *pParent);
HRESULT CreateAndAddCDATANode(IXMLDOMDocument *pDom, PCWSTR wszCDATA, IXMLDOMNode *pParent);
HRESULT CreateAndAddElementNode(IXMLDOMDocument *pDom, PCWSTR wszName, PCWSTR wszNewline, IXMLDOMNode *pParent, IXMLDOMElement **ppElement = NULL);

HRESULT ReportParseError(IXMLDOMDocument *pDoc, char *szDesc);

//@DPH
void GetParentNode(IXMLDOMNode *pNode, IXMLDOMNode **parent, int step = 1);
void getNodeAttribute(IXMLDOMNode *pNode, BSTR attribute, BSTR *value);
void setNodeAttribute(IXMLDOMDocument* pXMLNode, IXMLDOMNode *pNode, BSTR attribute, BSTR value); // we need the pXMLNode to create the attribute in case it doesn't exist
void getCellCoordinates(IXMLDOMNode* pCellNode, int& iRow, int& iCol); //returns the index of row and column of the IXMLDOMNode cell
void GetFirstNodeByXPath(IXMLDOMNode* pXMLNode, const char * xPath, IXMLDOMNode **pNode);
void XmlReplace(IXMLDOMNode* pXMLNode, const char * oldValue, const char * newValue);
void XmlReplace(IXMLDOMNode* pXMLNode, std::map<std::string, std::string> mapOldNewValues);

#endif
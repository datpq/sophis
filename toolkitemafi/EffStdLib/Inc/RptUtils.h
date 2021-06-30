#ifndef REPORTS_UTILS_H
#define REPORTS_UTILS_H
#include<string>
#include"msxmlutils.h"

namespace eff {
	namespace utils {
		namespace report
		{
			void setParameterValue(IXMLDOMNode* pWorksheet, const char * paramName, const char * paramValue);
			void loadOneParameterValue(IXMLDOMNode* pWorksheet, const char * oneParamValue);
			void loadParameterValues(IXMLDOMNode* pWorksheet, const char * paramValues);
			void setCellFormula(IXMLDOMDocument* pXMLNode, IXMLDOMNode *pCellNode, const char * formula);
			void setRowValue(IXMLDOMDocument* pXMLNode, IXMLDOMNode *pNodeRow, CComVariant nativeValue, int nodeIdx = -1, int mergeRow = 0);
			void setCellValue(IXMLDOMNode* pNodeCell, CComVariant nativeValue);
			void loadTemplateNode(IXMLDOMNode *pWorksheet, const char * nodeName, IXMLDOMNode **pTemplateNodeRow, IXMLDOMNode **pTemplateNodeCell);
		}
	}
}

#endif
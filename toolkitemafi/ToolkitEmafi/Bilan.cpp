#include "StdAfx.h"
#include "Bilan.h"
#include "Log.h"
#include "Constants.h"
//#include <comutil.h>
#include <sstream>
#include <vector>
#include <regex>

#include "RptUtils.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SqlUtils.h"
#define BUFF_LEN 255

using namespace eff::emafi::reports;
using namespace std;
using namespace eff::utils::report;
using namespace eff::utils;

struct StER{
	char opcvm[50];
	char code_opcvm[50];
};

void checkTaggedNode(const char * nodeText, IXMLDOMNode* pNode, std::map<string, IXMLDOMNode *> &arrTaggedNodes)
{
	string s = StrSplit(nodeText, "=")[0];
	regex tagReg("\\([A-Z]\\)");
	smatch matchTag;
	if (regex_search(s, matchTag, tagReg))
	{
		string tagName = matchTag[0];
		arrTaggedNodes[tagName] = pNode;
	}
}

void checkFormulaNode(const char * nodeText, vector<IXMLDOMNode*> &formula_nodes, string &formula_operators, std::map<string, IXMLDOMNode *> &arrTaggedNodes)
{
	regex formulaReg("[-=+\\/](?:[-+\\/\\s](\\([A-Z]\\)))+");
	smatch matchFormula;
	string s = nodeText;
	while (regex_search(s, matchFormula, formulaReg))
	{
		string tagWithOper = matchFormula[0];
		if (string("+-/").find(tagWithOper.at(0)) == string::npos) {
			formula_operators.append(1, '+');
		} else {
			formula_operators.append(1, tagWithOper.at(0));
		}
		string tagName = matchFormula[1];
		//for(auto iter = matchFormula.begin(); iter != matchFormula.end(); ++iter)
		//{
		//	string temp = (*iter).str();
		//}
		formula_nodes.push_back(arrTaggedNodes[tagName]);
		s = s.substr(matchFormula.position() + matchFormula.length());
	}
}

void calculateFormula(IXMLDOMNode* pCurrentCell, vector<IXMLDOMNode*> formula_datas, const char * formula_operators, string &sFormula, int &currentRow)
{
	int currentCol, dataRow, dataCol;
	getCellCoordinates(pCurrentCell, currentRow, currentCol);

	if (!formula_datas.empty()) {
		int firstRow, firstCol, lastRow, lastCol;
		getCellCoordinates(formula_datas[0], firstRow, firstCol);
		getCellCoordinates(formula_datas[formula_datas.size() - 1], lastRow, lastCol);
		if ((lastRow - firstRow == formula_datas.size() - 1) && (formula_operators == NULL || string(formula_datas.size(), '+') == formula_operators)) {
			sFormula = StrFormat("=SUM(R[%d]C:R[%d]C)", firstRow - currentRow, lastRow - currentRow);
		} else {
			sFormula = "";
			for(int i=0; i<formula_datas.size(); i++) {
				getCellCoordinates(formula_datas[i], dataRow, dataCol);
				char oper = formula_operators == NULL ? '+' : formula_operators[i];
				if (string("/*").find(oper) == string::npos) {
					sFormula = StrFormat("%s%cR[%d]C", sFormula.c_str(), oper, dataRow - currentRow);
				} else { // if * or / create a bracket
					sFormula = StrFormat("(%s)%cR[%d]C", sFormula.c_str(), oper, dataRow - currentRow);
				}
			}
			sFormula = "=" + sFormula;
		}
	}
}

/*getters and setters*/
Bilan_::Bilan_(IXMLDOMDocument* pXMLDom, IXMLDOMNode *pNodeWorkSheet, string worksheet_name, string report_name) : pXMLNode(pXMLDom), pWorksheet(pNodeWorkSheet), worksheet_name(worksheet_name)
{
	pTable = pSubTitleRow = pRubricRow = pLabelRow = pValRow = pCategRow = pTotalRow = NULL;
	pSubTitleCell = pRubricCell = pLabelCell = pValCell = pCategCell = pTotalCell = NULL;

	numOfCols = 0;
	nb_labels = 0;

	BSTR bstr = _com_util::ConvertStringToBSTR(report_name.c_str());
	setNodeAttribute(pXMLNode, pWorksheet, L"ss:Name", bstr);
	loadInitialNodes();
}

int Bilan_::getNumOfCols()
{
	long result = 0;
	if (arrColumnDataTypes.size() > 0) {
		// First 4 cols: ID_LABEL, LABEL, RUBRIC_TYPE, NIV --last 2 cols: RUBRIC_ORDER, LABEL_ORDER
		result = arrColumnDataTypes.size() - 4;
	} else if (getTemplateNode("{TITLE}") != NULL) {
		IXMLDOMNodeList * pNodeList = NULL;
		getTemplateNode("{TITLE}")->get_childNodes(&pNodeList);
		pNodeList->get_length(&result);
		SAFE_RELEASE(pNodeList);
	}
	numOfCols = max(numOfCols, result);
	return numOfCols;
}

void Bilan_::appendCell(CComVariant dataVal, const char* dataType, IXMLDOMNode* pRow, IXMLDOMNode* pCloneCell, IXMLDOMNode** pOutCell, int mergeDown, int mergeAcross)
{
	pCloneCell->cloneNode(VARIANT_TRUE, pOutCell);
	pRow->appendChild(*pOutCell, pOutCell);

	stringstream ss;
	BSTR bstr;
	if(mergeDown > 0)
	{
		ss << mergeDown;
		bstr = _com_util::ConvertStringToBSTR(ss.str().c_str());
		setNodeAttribute(pXMLNode,*pOutCell,L"ss:MergeAcross",bstr);
	}
	if(mergeAcross > 0)
	{
		ss << mergeAcross;
		bstr = _com_util::ConvertStringToBSTR(ss.str().c_str());
		setNodeAttribute(pXMLNode,*pOutCell,L"ss:MergeAcross",bstr);
	}

	IXMLDOMNode *pData = NULL;
	(*pOutCell)->get_firstChild(&pData);
	if (string("String") == dataType) {
		string sVal = _com_util::ConvertBSTRToString(dataVal.bstrVal);
		if (!sVal.empty() && sVal.at(0) == '=') {
			setNodeAttribute(pXMLNode, *pOutCell, L"ss:Formula", dataVal.bstrVal);
		} else {
			setCellValue(*pOutCell, dataVal);
			setNodeAttribute(pXMLNode, pData, L"ss:Type", _com_util::ConvertStringToBSTR(dataType));
		}
	} else {
	//	string test = _com_util::ConvertBSTRToString(dataVal.bstrVal);
		setCellValue(*pOutCell, dataVal);
		setNodeAttribute(pXMLNode, pData, L"ss:Type", _com_util::ConvertStringToBSTR(dataType));
	}
}


void Bilan_::updateFormula(IXMLDOMNode* pRow, string rubric_type, vector<IXMLDOMNode*> *formula_datas, const char * formula_operators)
{
	if (pRow == NULL) return;
	IXMLDOMNode* pOutCell = NULL;
	vector<IXMLDOMNode*> data_to_sum;
	
	if (formula_datas != NULL)
		data_to_sum = *formula_datas;
	else if(rubric_type == RUB_R || rubric_type == RUB_RB ||rubric_type == RUB_TB)
		data_to_sum = labels_to_sum;
	else if(rubric_type == RUB_T || rubric_type == RUB_TC)
		data_to_sum = rubrics_to_sum;
	else if(rubric_type == RUB_TG)
		data_to_sum = totals_to_sum;

	IXMLDOMNode* pCurrRubricCell = NULL;
	pRow->get_firstChild(&pCurrRubricCell);
	pCurrRubricCell->get_nextSibling(&pCurrRubricCell);
	string sFormula;
	int currentRow;
	calculateFormula(pCurrRubricCell, data_to_sum, formula_operators, sFormula, currentRow);
	if(rubric_type == RUB_TC && !totals_to_sum.empty())
	{
		int dataRow, dataCol;
		getCellCoordinates(totals_to_sum[totals_to_sum.size() - 1], dataRow, dataCol);
		sFormula = StrFormat("%s+R[%d]C", sFormula.c_str(), dataRow - currentRow);
	}

	//find number of non-indexed sibling of current RubricCell
	int byPassRubricCount = -1;
	while(pCurrRubricCell != NULL) {
		BSTR sIdx = NULL;
		getNodeAttribute(pCurrRubricCell, L"ss:Index", &sIdx);
		if (sIdx == NULL) {
			byPassRubricCount++;
			IXMLDOMNode *pData = NULL;
			pCurrRubricCell->get_firstChild(&pData);
			VARIANT dataVal;
			pData->get_nodeTypedValue(&dataVal);
			string sVal = _com_util::ConvertBSTRToString(dataVal.bstrVal);
			if (sVal.empty() && !sFormula.empty()) {
				setNodeAttribute(pXMLNode, pCurrRubricCell, L"ss:Formula", _com_util::ConvertStringToBSTR(sFormula.c_str()));
			}
		}
		pCurrRubricCell->get_nextSibling(&pCurrRubricCell);
	}


	//first 5 cols are ID_LABEL, LABEL, RUBRIC_TYPE, RUBRIC, NIV
	for(int i = 5+byPassRubricCount; i < arrColumnDataTypes.size(); i++)
	{
		bool isNumber = arrColumnDataTypes[i] != SqlWrapper::eChar;
		if (rubric_type == RUB_T || rubric_type == RUB_TB || rubric_type == RUB_TC || rubric_type == RUB_TG || rubric_type == RUB_RB)
			appendCell(isNumber ? 0 : "", isNumber ? "Number" : "String", pRow, pValTotalCell, &pOutCell);
		else
			appendCell(isNumber ? 0 : "", isNumber ? "Number" : "String", pRow, pValCell, &pOutCell);
		if (isNumber && !sFormula.empty()) {
			setNodeAttribute(pXMLNode, pOutCell, L"ss:Formula", _com_util::ConvertStringToBSTR(sFormula.c_str()));
		}
	}
	//}

	//reorder the cells (predefined cell will have theire correct position)
	IXMLDOMNode *pNode = NULL;
	pRow->get_firstChild(&pNode);
	vector<IXMLDOMNode *> arrNodeWithIndexes;
	int index = 0;
	while(pNode) {
		IXMLDOMNode *pNextNode = NULL;
		pNode->get_nextSibling(&pNextNode);
		BSTR sIdx = NULL;
		getNodeAttribute(pNode, L"ss:Index", &sIdx);
		if (sIdx) {
			int idx = _wtoi(sIdx);
			if (index == 0) {
				index = idx; // first cell with index. Keep it
			} else { // from sencond cell with index --> remove it and keep in a vectors
				pRow->removeChild(pNode, NULL);
				arrNodeWithIndexes.push_back(pNode);
			}
		} else {
			index++;
			if (arrNodeWithIndexes.size() > 0) {
				getNodeAttribute(arrNodeWithIndexes[0], L"ss:Index", &sIdx);
				if (index == _wtoi(sIdx)) {
					pRow->insertBefore(arrNodeWithIndexes[0], (CComVariant)pNode, NULL);
					pRow->removeChild(pNode, NULL);
					arrNodeWithIndexes.erase(arrNodeWithIndexes.begin());
				}
			}
		}
		pNode = pNextNode;
	}
}

void Bilan_::appendTemplateNode(const char * templateName, const char * templateNodeValue)
{
	IXMLDOMNode * pTemplateRowNode = getTemplateNode(templateName);
	IXMLDOMNode * pTemplateCellNode = getTemplateNode((string(templateName) + "_").c_str());
	if (pTemplateRowNode) {
		IXMLDOMNode * pNode = NULL;
		IXMLDOMNode * pCellNode = NULL;
		if (string(templateName) == "{TableFooter}") {
			pTemplateRowNode->cloneNode(VARIANT_TRUE, &pNode);
			pTable->appendChild(pNode, &pNode);
			pTemplateCellNode->cloneNode(VARIANT_TRUE, &pCellNode);
			pNode->appendChild(pCellNode, &pCellNode);
			setCellValue(pCellNode, "");
		}

		pTemplateRowNode->cloneNode(VARIANT_TRUE, &pNode);
		pTable->appendChild(pNode, &pNode);
		pTemplateCellNode->cloneNode(VARIANT_TRUE, &pCellNode);
		pNode->appendChild(pCellNode, &pCellNode);
		setCellValue(pCellNode, templateNodeValue);

		pTemplateRowNode->cloneNode(VARIANT_TRUE, &pNode);
		pTable->appendChild(pNode, &pNode);
		pTemplateCellNode->cloneNode(VARIANT_TRUE, &pCellNode);
		pNode->appendChild(pCellNode, &pCellNode);
		setCellValue(pCellNode, "");

		SAFE_RELEASE(pCellNode);
		SAFE_RELEASE(pNode);
	}
}

void Bilan_::appendTitle(const char* title, bool &firstTitle, const char* subtitle, bool &firstSubTitle, int numOfSubTitles, int totalSubTitles)
{
	IXMLDOMNode *pOutCell = 0;

	if(firstTitle)
	{
		getTemplateNode("{TITLE}")->cloneNode(VARIANT_TRUE, &pCurrentTitleRow);
		pTable->appendChild(pCurrentTitleRow, &pCurrentTitleRow);
		firstTitle = false;
	}

	if(string(title) != current_title)
	{
		appendCell(title, "String", pCurrentTitleRow, arrTemplateNodes["{TITLE}_"], &pCurrTitleCell, 0, numOfSubTitles > 1 ? numOfSubTitles-1 : 0);	
		current_title = string(title);
	}

	if (totalSubTitles > 0) {
		if(firstSubTitle)
		{
			pSubTitleRow->cloneNode(VARIANT_TRUE, &pCurrentSubTitleRow);
			pTable->appendChild(pCurrentSubTitleRow, &pCurrentSubTitleRow);
			firstSubTitle = false;
		}
		appendCell(subtitle, "String", pCurrentSubTitleRow, pSubTitleCell, &pOutCell);
	}
}

void Bilan_::appendRubric(string rubric, string rubric_type, bool insert, bool update)
{
	if(rubric.empty()) return;
	IXMLDOMNode *pNode = NULL, *pOutCell = NULL;
	vector<IXMLDOMNode*> formula_nodes;//list of nodes to be used in a formula
	string formula_operators;

	/*insert a new rubric */
	if(insert)
	{
		IXMLDOMNode * pRow = (rubric_type == RUB_TB || rubric_type == RUB_RB) ? pTotalRow : pRubricRow;
		IXMLDOMNode * pCell = (rubric_type == RUB_TB || rubric_type == RUB_RB) ? pTotalCell : pRubricCell;
		pRow->cloneNode(VARIANT_TRUE,&pNode);
		pTable->appendChild(pNode,&pCurrRow);

		vector<string> arrRubricCells = StrSplit(rubric.c_str(), "^");
	
		appendCell(arrRubricCells[0].c_str(), "String", pCurrRow, pCell, &pOutCell);
		rubrics_to_sum.push_back(pOutCell);

		checkTaggedNode(rubric.c_str(), pOutCell, arrTaggedNodes);
		checkFormulaNode(rubric.c_str(), formula_nodes, formula_operators, arrTaggedNodes);

		for(int i=1; i<arrRubricCells.size(); i++)
		{
			appendCell(arrRubricCells[i].c_str(), "String", pCurrRow, pCell, &pOutCell);
		}
	}
	
	/* update previous rubric or the same rubric (in case of Balance)*/
	if(update)
	{
		IXMLDOMNode * pRowToUpdate = (rubric_type == RUB_R || rubric_type == RUB_RB) ? pPrevRubricRow : pCurrRow;
		updateFormula( pRowToUpdate, rubric_type);
		nb_labels = 0;
	}

	pPrevRubricRow = pCurrRow;
	current_rubric_type = rubric_type;
	labels_to_sum.clear();

	if (!formula_nodes.empty()) {
		pPrevRubricRow = NULL;
		updateFormula(pCurrRow, rubric_type, &formula_nodes, formula_operators.c_str());
	}
}

void Bilan_::appendRow(vector<Variant> data,bool isLastRow)
{
	IXMLDOMNode* pNode = NULL;
	IXMLDOMNode* pOutCell = NULL;

	string rubric = data[1].value<string>();
	string rubric_type = data[2].value<string>();
	string label = data[3].value<string>();
	//int level = data[4].value<long>();
	int level = int(data[4].value<double>());
	if (level == 0) level = 1;

	if(rubric_type == RUB_C)
	{
		pCategRow->cloneNode(VARIANT_TRUE,&pNode);
		pTable->appendChild(pNode,&pCurrRow);

		appendCell(rubric.c_str(),"String",pCurrRow,pCategCell,&pOutCell);
		return;
	}
	
	if(rubric_type == RUB_T || rubric_type == RUB_TG || rubric_type == RUB_TC)
	{
		appendRubric(current_rubric, current_rubric_type, current_rubric_type == RUB_TB, nb_labels != 0);

		pTotalRow->cloneNode(VARIANT_TRUE,&pNode);
		pTable->appendChild(pNode,&pCurrRow);

		appendCell(rubric.c_str(),"String",pCurrRow,pTotalCell,&pOutCell);

		updateFormula(pCurrRow, rubric_type.c_str());
		totals_to_sum.push_back(pOutCell);
		rubrics_to_sum.clear();
		
		return;
	}

	if(rubric != current_rubric && rubric_type != RUB_L)
	{
		appendRubric((rubric_type == RUB_R || rubric_type == RUB_RB) ? rubric : current_rubric, rubric_type, true, nb_labels != 0);
		current_rubric = rubric;
	}

	IXMLDOMNode * pRow = isLastRow ? (rubric_type == RUB_RB ? pTotalRow : getTemplateNode("{LASTLABEL}")) : pLabelRow;
	IXMLDOMNode * pCell = isLastRow ? (rubric_type == RUB_RB ? pTotalCell : getTemplateNode("{LASTLABEL}_")) : pLabelCell;
	IXMLDOMNode * pVal = isLastRow ? (rubric_type == RUB_RB ? pValTotalCell : getTemplateNode("{LASTVAL}_")) : pValCell;
	pRow->cloneNode(VARIANT_TRUE,&pNode);
	pTable->appendChild(pNode,&pRow);

	appendCell(string((level-1) * 8, ' ').append(label).c_str(),"String", pRow, pCell, &pOutCell);
	
	vector<IXMLDOMNode*> formula_nodes;//list of nodes to be used in a formula
	string formula_operators;
	checkTaggedNode(label.c_str(), pOutCell, arrTaggedNodes);
	checkFormulaNode(label.c_str(), formula_nodes, formula_operators, arrTaggedNodes);

	string sFormula;
	int currentRow;
	calculateFormula(pOutCell, formula_nodes, formula_operators.c_str(), sFormula, currentRow);

	if (level == 1) {
		labels_to_sum.push_back(pOutCell);
	}

	for (int i = 5; i < data.size(); i++) {
		//string typeName = data[i].type().name();
		if (data[i].type() == typeid(string))
		appendCell(data[i].value<string>().c_str(), "String", pRow, pVal, &pOutCell);
	else if (data[i].type() == typeid(long))
		appendCell(data[i].value<long>(), "Number", pRow, pVal, &pOutCell);
		else if (data[i].type() == typeid(double))
			appendCell(data[i].value<double>(), "Number", pRow, pVal, &pOutCell);
		if ((data[i].type() == typeid(long) || data[i].type() == typeid(double)) && !sFormula.empty()) {
			setNodeAttribute(pXMLNode, pOutCell, L"ss:Formula", _com_util::ConvertStringToBSTR(sFormula.c_str()));
		}
	}
	nb_labels++;

	if (!formula_nodes.empty()) {
		updateFormula(pRow, rubric_type, &formula_nodes, formula_operators.c_str());
	}

	if (isLastRow) {
		if (rubric_type == RUB_TB) {
			appendRubric(rubric, rubric_type, true, nb_labels != 0);
		} else if (rubric_type != RUB_L) {
			updateFormula(pCurrRow, rubric_type);
		}
		pPrevRubricRow = NULL;
		pCurrRow = NULL;
		getNumOfCols();//refresh numOfCols
		rubrics_to_sum.clear();
		labels_to_sum.clear();
		nb_labels = 0;
		current_rubric.clear();
		current_rubric_type.clear();
		current_title.clear();
	}
}

void Bilan_::updateExpandedRowCol()
{
	BSTR sNodeTxt = NULL;
	getNodeAttribute(pTable, L"ss:ExpandedRowCount", &sNodeTxt);
	int expandedRowCount = _wtoi(sNodeTxt);
	IXMLDOMNodeList * pNodeList = NULL;
	pTable->get_childNodes(&pNodeList);
	long numOfRows = 0;
	pNodeList->get_length(&numOfRows);
	expandedRowCount += numOfRows;
	wchar_t temp_str[5];
	_itow(++expandedRowCount, temp_str, 10);
	setNodeAttribute(pXMLNode,pTable, L"ss:ExpandedRowCount", SysAllocString(temp_str));
	getNodeAttribute(pTable, L"ss:ExpandedColumnCount", &sNodeTxt);
	int expandedColCount = _wtoi(sNodeTxt);
	int ColCount = _wtoi(sNodeTxt);


	int expandedColCountTest = getNumOfCols();
	expandedColCount += expandedColCountTest;
	_itow(++expandedColCount, temp_str, 10);
	setNodeAttribute(pXMLNode,pTable, L"ss:ExpandedColumnCount", SysAllocString(temp_str));
	
	// Bourama :  Add all the column node after setting expandedColumnCount
				
	//Get the number of column in the initial template file
	IXMLDOMNode * pColumn = NULL;
	int nbColumn=0;
	BSTR spNodeName = NULL;
	for (int ii = 0; ii < ColCount; ii++){
		pNodeList->get_item(ii, &pColumn);
		pColumn->get_nodeName(&spNodeName);
		string spnodename = _com_util::ConvertBSTRToString(spNodeName);
		if (spnodename == "Column")
			nbColumn += 1;
	}

	pColumn = NULL;
	pNodeList->get_item(nbColumn - 1, &pColumn); //get the last column to clone
	IXMLDOMElement *pNewColumn = NULL;


	//Start Bourama	
	BSTR sWorksheetName = NULL;
	getNodeAttribute(pWorksheet, L"ss:Name", &sWorksheetName);
	string name = _com_util::ConvertBSTRToString(sWorksheetName);
	if (nbColumn >1 && name != "Balance"){ //for  Balance report do not add column
		pColumn = NULL;
		pNodeList->get_item(nbColumn-1, &pColumn); //get the last column to clone
		IXMLDOMNode * pChildColumnNode;
		for (int i = 0; i < expandedColCountTest; i++){
			pColumn->cloneNode(VARIANT_FALSE, &pChildColumnNode);
			pTable->insertBefore(pChildColumnNode, (CComVariant)pColumn, &pChildColumnNode);
		}
	}//End bourama
	
CleanUp:
	SAFE_RELEASE(pNodeList);
	SysFreeString(sNodeTxt);
}

void Bilan_::loadInitialNodes()
{
	DEBUG("BEGIN");
	
	pWorksheet->get_firstChild(&pTable);
	const char * lstTemplateNodes[] = {"{TITLE}", "{SUBTITLE}", "{RUBRIQUE}", "{VAL}", "{LASTVAL}", "{VALTOTAL}", "{LABEL}", "{LASTLABEL}", "{CATEGORY}", "{TOTAL}", "{TOTAL GENERAL}", "{TableHeader}", "{TableFooter}"};
	const int lstSize = 13;
	for(int i=0; i<lstSize; i++) {
		IXMLDOMNode * pRow = NULL;
		IXMLDOMNode * pCell = NULL;
		loadTemplateNode(pWorksheet, lstTemplateNodes[i], &pRow, &pCell);
		if (pRow) {
			arrTemplateNodes[lstTemplateNodes[i]] = pRow;
			arrTemplateNodes[string(lstTemplateNodes[i]) + "_"] = pCell;
		}
	}
	for(int i=0; i<lstSize; i++) {
		pTable->removeChild(arrTemplateNodes[lstTemplateNodes[i]], NULL);
	}
	pSubTitleRow = arrTemplateNodes["{SUBTITLE}"];
	pSubTitleCell = arrTemplateNodes["{SUBTITLE}_"];
	pRubricRow = arrTemplateNodes["{RUBRIQUE}"];
	pRubricCell = arrTemplateNodes["{RUBRIQUE}_"];
	pValRow = arrTemplateNodes["{VAL}"];
	pValCell = arrTemplateNodes["{VAL}_"];
	pValTotalRow = arrTemplateNodes["{VALTOTAL}"];
	pValTotalCell = arrTemplateNodes["{VALTOTAL}_"];
	pLabelRow = arrTemplateNodes["{LABEL}"];
	pLabelCell = arrTemplateNodes["{LABEL}_"];
	pCategRow = arrTemplateNodes["{CATEGORY}"];
	pCategCell = arrTemplateNodes["{CATEGORY}_"];
	pTotalRow = arrTemplateNodes["{TOTAL}"];
	pTotalCell = arrTemplateNodes["{TOTAL}_"];

	nb_removed_cols = 2;
	nb_removed_rows = 6;
}

IXMLDOMNode * Bilan_::getTemplateNode(const char * nodeName)
{
	if (arrTemplateNodes.find(nodeName) == arrTemplateNodes.end())
		return NULL;
	else
		return arrTemplateNodes[nodeName];
}

//bourama
void Bilan_::setColumnLenght(const char* title, const char* subTitle, const char* code, int numColumn)
{
	IXMLDOMNodeList * pNodeList = NULL;
	pTable->get_childNodes(&pNodeList);

	string lenghtQuery = "";
	string null = " ";
	string reportTaille = "ReportTaille%";
	string reportSubTaille = "ReportSubTaille%";
	if (strcmp(subTitle, "") != 0){// SubTitle
		lenghtQuery = StrFormat("SELECT S.DESCRIPTION LENGHT FROM EMAFI_PARAMETRAGE S, EMAFI_PARAMETRAGE T WHERE S.CATEGORIE LIKE '%s' AND T.DESCRIPTION = '%s' AND T.CODE=S.CODE AND T.CATEGORIE='%s'", reportSubTaille.c_str(), subTitle,code);
	}else if (strcmp(title, null.c_str()) == 0)
		lenghtQuery = StrFormat("SELECT S.DESCRIPTION LENGHT FROM EMAFI_PARAMETRAGE S, EMAFI_PARAMETRAGE T WHERE S.CATEGORIE LIKE '%s' AND T.DESCRIPTION IS NULL AND T.CODE=S.CODE AND T.CODE='%s'", reportTaille.c_str(), code);
	else
		lenghtQuery = StrFormat("SELECT S.DESCRIPTION LENGHT FROM EMAFI_PARAMETRAGE S, EMAFI_PARAMETRAGE T WHERE S.CATEGORIE LIKE '%s' AND T.DESCRIPTION = '%s' AND T.CODE=S.CODE AND T.CODE='%s'", reportTaille.c_str(), title,code);

	DEBUG("LENGHT TITLE QUERY = %s", lenghtQuery.c_str());
		SqlWrapper * lenghtQueryWrapper = new SqlWrapper("c100", lenghtQuery.c_str());
		int lenghtCount = lenghtQueryWrapper->GetRowCount();
		string lenght = "0";
		if (lenghtCount > 0){
			lenght = (*lenghtQueryWrapper)[0][0].value<string>();

			BSTR sNodeName = NULL;
			IXMLDOMNode *pNode = NULL;
			int u = numColumn + 2; //do not process the two first column
			pNodeList->get_item(numColumn + 2, &pNode);
			pNode->get_nodeName(&sNodeName);
			string snodename = _com_util::ConvertBSTRToString(sNodeName);
			if (snodename == "Column"){
				BSTR bstr = _com_util::ConvertStringToBSTR(lenght.c_str());
				setNodeAttribute(pXMLNode, pNode, L"ss:Width", bstr);
			}
		}
}

Bilan_::~Bilan_()
{
	SAFE_RELEASE(pTable);
	SAFE_RELEASE(pSubTitleRow);
	SAFE_RELEASE(pRubricRow);
	SAFE_RELEASE(pLabelRow);
	SAFE_RELEASE(pValRow);
	SAFE_RELEASE(pCategRow);
	SAFE_RELEASE(pTotalRow);
	SAFE_RELEASE(pSubTitleCell);
	SAFE_RELEASE(pRubricCell);
	SAFE_RELEASE(pLabelCell);
	SAFE_RELEASE(pCategCell);
	SAFE_RELEASE(pCategCell);
	SAFE_RELEASE(pTotalCell);
}
IXMLDOMNode* Bilan::getWorksheetNode()
{
	return pNodeWorkSheet;
}

string Bilan::getWorksheetName()
{
	return worksheet_name;
}

Bilan::Bilan(IXMLDOMDocument* pXMLDom, IXMLDOMNode *pNodeWorkSheet, string worksheet_name) : pXMLNode(pXMLDom),worksheet_name (worksheet_name)
{
	this->pNodeWorkSheet = pNodeWorkSheet;
	this->pXMLNode = pXMLNode;
	pNodeRubric = pNodeTotal = pNodeTotalGeneral = pNodeTable = pNodeLabel = NULL;
	m_currentRubricId = 0;
	m_currentRubricLabelCount = 0;
	m_currentRubricVal = 0;
	m_currentRubricValPrec = 0;
	m_firstRubricIdxToComputeTotal = 0;
	m_rowCount = 0;
	isTotalExceeded = false;
	m_currentRubricNode = NULL;
	m_rubricNodes.clear();
	m_rubricLabelCounts.clear();
	m_totalNodeIdxs.clear();
}

void Bilan::init()
{
	if(worksheet_name != "EtatDerogations") //kb
		loadTemplateFile();
}

void Bilan::loadTemplateFile()
{
	DEBUG("BEGIN");
	HRESULT hr = S_OK;
	
	IXMLDOMNodeList *pNodes = NULL;
	//BSTR sNodeTxt = NULL;
	pNodeWorkSheet->selectNodes(L".//*[text()='{RUBRIQUE}']", &pNodes);
	if (pNodes) {
		CHK_HR(pNodes->get_item(0, &pNodeRubric));//<Data />
		pNodeRubric->get_parentNode(&pNodeRubric);//<Cell />
		pNodeRubric->get_parentNode(&pNodeRubric);//<Row />

		pNodeRubric->get_parentNode(&pNodeTable);
		pNodeRubric->get_nextSibling(&pNodeLabel);

		SAFE_RELEASE(pNodes);
	}
	pNodeWorkSheet->selectNodes(L".//*[text()='{TOTAL}']", &pNodes);
	if (pNodes) {
		CHK_HR(pNodes->get_item(0, &pNodeTotal));//<Data />
		pNodeTotal->get_parentNode(&pNodeTotal);//<Cell />
		pNodeTotal->get_parentNode(&pNodeTotal);//<Row />

		SAFE_RELEASE(pNodes);
	}
	pNodeWorkSheet->selectNodes(L".//*[text()='{TOTAL GENERAL}']", &pNodes);
	if (pNodes) {
		CHK_HR(pNodes->get_item(0, &pNodeTotalGeneral));//<Data />
		pNodeTotalGeneral->get_parentNode(&pNodeTotalGeneral);//<Cell />
		pNodeTotalGeneral->get_parentNode(&pNodeTotalGeneral);//<Row />

		SAFE_RELEASE(pNodes);
	}

	pNodeWorkSheet->selectNodes(L".//*[text()='{CATEGORIE}']", &pNodes);
	if (pNodes) {
		CHK_HR(pNodes->get_item(0, &pCategory));
		if(pCategory)
		{
			pCategory->get_parentNode(&pCategory);
			pCategory->get_parentNode(&pCategory);
		}
		SAFE_RELEASE(pNodes);
	}
	
	if (pNodeRubric == NULL || pNodeTotal == NULL || pNodeTotalGeneral == NULL || pNodeTable == NULL || pNodeLabel == NULL) {
        ERROR("Error reading template. One of required node is not found.");
	}

	pNodeTable->removeChild(pNodeRubric, NULL);
	pNodeTable->removeChild(pNodeLabel, NULL);
	pNodeTable->removeChild(pNodeTotal, NULL);
	pNodeTable->removeChild(pNodeTotalGeneral, NULL);
	pNodeTable->removeChild(pCategory,NULL);

	nRemovedRows = 4;

	DEBUG("END");
CleanUp:
	SAFE_RELEASE(pNodes);
}

void IA::loadTemplateFile()
{
	DEBUG("BEGIN");

	HRESULT hr = S_OK;
	
	IXMLDOMNodeList *pNodes = NULL;
	//BSTR sNodeTxt = NULL;

	pNodeWorkSheet->selectNodes(L".//*[text()='{CODE_ISIN}']", &pNodes);
	if (pNodes) 
	{
		CHK_HR(pNodes->get_item(0, &pCodeIsin));
		pCodeIsin->get_parentNode(&pNodeIsin);//cell
		pNodeIsin->get_parentNode(&pCodeIsin);//row
	}

	pNodeWorkSheet->selectNodes(L".//*[text()='{EMETTEUR}']", &pNodes);
	if (pNodes) {
		CHK_HR(pNodes->get_item(0, &pNodeEmetteur));//<Data /> emetteur	
		
		pNodeEmetteur->get_parentNode(&pNodeEmetteur);//<Cell />
		pNodeEmetteur->get_nextSibling(&pNodeQuantity); // cell quantity
		pNodeEmetteur->get_parentNode(&pNodeEmetteurRow);//<Row />
		pNodeEmetteurRow->get_parentNode(&pNodeTable);
		pNodeEmetteurRow->get_nextSibling(&pNodeEmetteurSibling);
		SAFE_RELEASE(pNodes);
	}
	
	pNodeWorkSheet->selectNodes(L".//*[text()='{DEPOT}']", &pNodes);
	if (pNodes) {
		CHK_HR(pNodes->get_item(0, &pNodeDepot));//<Data /> depot	

		pNodeDepot->get_parentNode(&pNodeDepot);//<Cell />
		pNodeDepot->get_nextSibling(&pNodePourcentage); // cell poucentage
		SAFE_RELEASE(pNodes);
	}

	pNodeWorkSheet->selectNodes(L".//*[text()='{TableFooter}']", &pNodes); //footer
	if (pNodes)
	{
		CHK_HR(pNodes->get_item(0, &pNodeFooter));
		pNodeFooter->get_parentNode(&pNodeFooter);//cell
		pNodeFooter->get_parentNode(&pNodeFooterRow);//row
		SAFE_RELEASE(pNodes);
	}

	/*if (pNodeRubric == NULL || pNodeTotal == NULL || pNodeTotalGeneral == NULL || pNodeTable == NULL || pNodeLabel == NULL) {
        ERROR("Error reading template. One of required node is not found.");
	}*/

	pNodeEmetteurRow->removeChild(pNodeEmetteur, NULL); //remove the node model for text
	pNodeEmetteurRow->removeChild(pNodeQuantity, NULL); //remove the node model for number

	pNodeEmetteurRow->removeChild(pNodeDepot, NULL); //remove the node model for text
	pNodeEmetteurRow->removeChild(pNodePourcentage, NULL); //remove the node model for number

	pNodeTable->removeChild(pNodeEmetteurRow, NULL);

	pNodeFooterRow->removeChild(pNodeFooter, NULL); // footer
	pNodeTable->removeChild(pNodeFooterRow, NULL); // footer

	pCodeIsin->removeChild(pNodeIsin, NULL);
	pNodeTable->removeChild(pCodeIsin, NULL);

	nRemovedRows = 3; // 2 + footer 

	DEBUG("END");
CleanUp:
	SAFE_RELEASE(pNodes);
}


void Bilan::updateExpandedRow()
{
	BSTR sNodeTxt = NULL;
	getNodeAttribute(pNodeTable, L"ss:ExpandedRowCount", &sNodeTxt);
	int expandedRowCount = _wtoi(sNodeTxt);
	expandedRowCount += (m_rowCount - nRemovedRows); // 4 deleted tempalte row
	wchar_t temp_str[5];
	_itow(++expandedRowCount, temp_str, 10);
	setNodeAttribute(pXMLNode,pNodeTable, L"ss:ExpandedRowCount", SysAllocString(temp_str));
CleanUp:
	SysFreeString(sNodeTxt);
}

void Bilan::updateFormula()
{
	IXMLDOMNode *pNode = NULL, *pNodePrec = NULL;
	if (m_currentRubricNode != NULL) {
		m_currentRubricNode->get_lastChild(&pNode);
		pNode->get_previousSibling(&pNodePrec);
		char sMsg[BUFF_LEN] = {'\0'};
		_snprintf_s(sMsg, BUFF_LEN, "=SUM(R[1]C:R[%d]C)", m_currentRubricLabelCount > 1 ? m_currentRubricLabelCount : 1);
		BSTR sNodeTxt = _com_util::ConvertStringToBSTR(sMsg);
		setNodeAttribute(pXMLNode,pNode, L"ss:Formula", sNodeTxt);
		setRowValue(pXMLNode,m_currentRubricNode, m_currentRubricValPrec);
		setNodeAttribute(pXMLNode,pNodePrec, L"ss:Formula", sNodeTxt);
		setRowValue(pXMLNode,m_currentRubricNode, m_currentRubricVal, -2);
		m_rubricLabelCounts.push_back(m_currentRubricLabelCount);
		m_currentRubricLabelCount = 0;
		m_currentRubricVal = 0;
		m_currentRubricValPrec = 0;
		SAFE_RELEASE(m_currentRubricNode);
	}
CleanUp:
	SAFE_RELEASE(pNode);
	SAFE_RELEASE(pNodePrec);
}

void Bilan::appendRow(long id_rubric, const char * rubric, const char * rubric_type, const char * label, double amount, double amountPrec, double amount_sec, double amountPrec_sec)
{
		// change to new section, update the formula of current Rubric
	if (((string(rubric_type) != RUB_R && string(rubric_type) != RUB_RB) || id_rubric != m_currentRubricId) && m_currentRubricNode != NULL)
	{
		updateFormula();
	}

	IXMLDOMNode *pNode = NULL, *pFirstCell = NULL;
	if(string(rubric_type) == RUB_C)
	{
		pCategory->cloneNode(VARIANT_TRUE,&pNode);
		pNodeTable->appendChild(pNode,NULL);
		pNode->get_firstChild(&pFirstCell);
		setCellValue(pFirstCell,rubric);
		//m_rowCount++;
	} else if (string(rubric_type) == RUB_T || string(rubric_type) == RUB_TC) {
		pNodeTotal->cloneNode(VARIANT_TRUE, &pNode);//append new total
		pNodeTable->appendChild(pNode, NULL);
		pNode->get_firstChild(&pFirstCell);
		setCellValue(pFirstCell, rubric);
		std::stringstream ss;
		int rowCount = 0;
		ss << "=SUM(";
		for(int i = m_rubricNodes.size() - 1; i >= m_firstRubricIdxToComputeTotal; i--) 
		{
			rowCount += (m_rubricLabelCounts[i] + 1);
			ss << "R[" << -rowCount << "]C";
			if (i != m_firstRubricIdxToComputeTotal) {
				ss << ",";
			}
		}
		if (string(rubric_type) == RUB_TC && m_totalNodeIdxs.size() > 0) {
			rowCount++;
			if(worksheet_name == "AnalyseRevenus") /*to take into account the line between the rubric and total in case of AR*/
				rowCount++;
			ss << ",R[" << -rowCount << "]C";
		}
		ss << ")";

		setRowFormula(pNode, ss.str().c_str());
	
		m_totalNodeIdxs.push_back(m_rowCount);
		m_firstRubricIdxToComputeTotal = m_rubricNodes.size();

		if(worksheet_name == "AnalyseRevenus")
			isTotalExceeded = true; //indicates the the total is inserted

	} else if (string(rubric_type) == RUB_TG) {
		pNodeTotalGeneral->cloneNode(VARIANT_TRUE, &pNode);//append new total general
		pNodeTable->appendChild(pNode, NULL);
		pNode->get_firstChild(&pFirstCell);
		setCellValue(pFirstCell, rubric);
		std::stringstream ss;
		ss << "=SUM(";
		for(int i = 0; i < m_totalNodeIdxs.size(); i++) 
		{
			ss << "R[" << m_totalNodeIdxs[i] - m_rowCount << "]C";
			if (i != m_totalNodeIdxs.size() - 1) {
				ss << ",";
			}
		}
		ss << ")";
		
		/** setRowFormula is called 1 time for the first table and then for the second one ****/
		setRowFormula(pNode, ss.str().c_str());

	} else if (string(rubric_type) == RUB_R || string(rubric_type) == RUB_RB) {
		if (id_rubric != m_currentRubricId) {
			pNodeRubric->cloneNode(VARIANT_TRUE, &m_currentRubricNode);//append new rubric
			pNodeTable->appendChild(m_currentRubricNode, NULL);
			m_currentRubricNode->get_firstChild(&pFirstCell);
			setCellValue(pFirstCell, rubric);
			m_rubricNodes.push_back(m_currentRubricNode);
			if(isTotalExceeded)
				m_totalNodeIdxs.push_back(m_rowCount); /* if the total is already inserted insert m_rowCount into m_totalNodeIdxs to be taken into account when computing total general (just in case of AR) */
			m_rowCount++;
		}
		pNodeLabel->cloneNode(VARIANT_TRUE, &pNode);//append new label
		pNodeTable->appendChild(pNode, NULL);
		pNode->get_firstChild(&pFirstCell);
		setCellValue(pFirstCell, label);// kb

		// setRowFormula is called 2 times for the first table and then for the second one
		setRowValue(pXMLNode,pNode, amountPrec,-1);
		setRowValue(pXMLNode,pNode, amount, -2);
		if(worksheet_name=="AnalyseRevenus")
		{
			setRowValue(pXMLNode,pNode, amountPrec_sec,-3);
			setRowValue(pXMLNode,pNode, amount_sec,-4);
		}

		SAFE_RELEASE(pNode);
		m_currentRubricLabelCount++;
		m_currentRubricVal += amount;
		m_currentRubricValPrec += amountPrec;
	}
	m_rowCount++;
	m_currentRubricId = id_rubric;
CleanUp:
	SAFE_RELEASE(pNode);
}

void IA::appendRow(vector<Variant> data, double prix)
{
	string emetteur_name = data[3].value<string>();
	string code_isin = data[4].value<string>();
	string designation = data[5].value<string>();
	double quantite = data[6].value<double>();
	double val_boursiere = data[7].value<double>();
	long number_of_assets = data[9].value<long>();
	double actif_pourcentage = data[10].value<double>();

	bool emetteurChange = false;

	int nb = 2;


	pNodeEmetteurRow->cloneNode(VARIANT_TRUE, &m_currentEmetteurNode);
	pNodeTable->appendChild(m_currentEmetteurNode, NULL);//append new asset

	IXMLDOMNode* pNode;

	//first column


	pNodeEmetteur->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, emetteur_name.c_str(), nb++);

	pNodeEmetteur->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, code_isin.c_str(), nb++);

	pNodeEmetteur->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, designation.c_str(), nb++);



	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, quantite, nb++);

	double adjustedPrice = prix*quantite;
	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, adjustedPrice, nb++); //prix

	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, val_boursiere, nb++);

	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, val_boursiere - adjustedPrice, nb++); //val_latente


	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, actif_pourcentage, nb++);

	m_rowCount++;
CleanUp:
	SAFE_RELEASE(pNode);
}




void IA::appendRowOPC4(vector<Variant> data, double prix, double total)
{

	string emetteur_name = data[3].value<string>();
	string code_isin = data[4].value<string>();
	string designation = data[5].value<string>();
	double quantite = data[6].value<double>();
	double PrimeDeRisque = data[7].value<double>();
	double ValorisationParTitre = data[8].value<double>();
	double ValorisationGlobale = data[9].value<double>();
	double CouponCouru = data[10].value<double>();


	int nb = 2;

	pNodeEmetteurRow->cloneNode(VARIANT_TRUE, &m_currentEmetteurNode);
	pNodeTable->appendChild(m_currentEmetteurNode, NULL);

	IXMLDOMNode* pNode;


	pNodeEmetteur->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, emetteur_name.c_str(), nb++);

	pNodeEmetteur->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, code_isin.c_str(), nb++);

	pNodeEmetteur->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, designation.c_str(), nb++);



	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, quantite, nb++);

	double adjustedPrice = prix*quantite;
	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, adjustedPrice, nb++);

	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, PrimeDeRisque, nb++);

	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, ValorisationParTitre, nb++);


	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, ValorisationGlobale, nb++);

	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, CouponCouru, nb++);

	double ValPrixPied = ValorisationGlobale - CouponCouru;
	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, ValPrixPied, nb++);

	double plusMoinsValue = ValPrixPied - adjustedPrice;
	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, plusMoinsValue, nb++);

	double pourcentage = ValorisationGlobale / total * 100;
	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, pourcentage, nb++);
	m_rowCount++;

CleanUp:
	SAFE_RELEASE(pNode);
}



void IA::appendRowOPC4_tableau2(const char* colonne, double val1, double val2, double val3)
{

	int nb = 2;

	IXMLDOMNode *pNode;

	pNodeEmetteurRow->cloneNode(VARIANT_TRUE, &m_currentEmetteurNode);
	pNodeTable->appendChild(m_currentEmetteurNode, NULL);


	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, colonne, nb++);

	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, "", nb++);

	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, "", nb++);

	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, "", nb++);

	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, "", nb++);


	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, "", nb++);

	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, "", nb++);

	pNodePourcentage->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, val1, nb++);


	pNodePourcentage->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, val2, nb++);

	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, "", nb++);


	pNodeDepot->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, "", nb++);


	double pourcentage = 0;
	if (val3 != 0)
		pourcentage = ((val1 + val2) / val3) * 100;
	pNodePourcentage->cloneNode(VARIANT_TRUE, &pNode);
	m_currentEmetteurNode->appendChild(pNode, NULL);
	setRowValue(pXMLNode, m_currentEmetteurNode, pourcentage, nb++);

	m_rowCount++;

CleanUp:
	SAFE_RELEASE(pNode);
}

void IA::insertFooter()
{
	IXMLDOMNode* pNodeRow, *pNodeCell;

	pNodeFooterRow->cloneNode(VARIANT_TRUE, &pNodeRow);
	pNodeTable->appendChild(pNodeRow, NULL);

	pNodeFooter->cloneNode(VARIANT_TRUE, &pNodeCell);
	pNodeFooterRow->appendChild(pNodeCell, NULL);
	setRowValue(pXMLNode, pNodeCell, "");


	m_rowCount++;

CleanUp:
	SAFE_RELEASE(pNodeRow);
	SAFE_RELEASE(pNodeCell);
}



void IA::appendRowOPC5(double valeur, bool endOfFile, bool sautDeLigne)
{
	IXMLDOMNode* pNode;	
	
	pNodeQuantity->cloneNode(VARIANT_TRUE, &pNode);
	pNodeEmetteurSibling->appendChild(pNode, NULL);
	setRowValue(pXMLNode, pNodeEmetteurSibling, valeur);

	if (!endOfFile) 
		pNodeEmetteurSibling->get_nextSibling(&pNodeEmetteurSibling);

	if (sautDeLigne)
		pNodeEmetteurSibling->get_nextSibling(&pNodeEmetteurSibling);
	
CleanUp:
	SAFE_RELEASE(pNode);
}




void Bilan::setRowFormula(IXMLDOMNode *pNodeRow, const char * sValue)
{
	IXMLDOMNode *pNode = NULL, *pNodePrec = NULL;
	pNodeRow->get_lastChild(&pNode);
	setCellFormula(pXMLNode,pNode,sValue);
	pNode->get_previousSibling(&pNodePrec);
	setCellFormula(pXMLNode,pNodePrec,sValue);

CleanUp:
	SAFE_RELEASE(pNode);
	SAFE_RELEASE(pNodePrec);
}



Bilan::~Bilan(void)
{
CleanUp:
	SAFE_RELEASE(m_currentRubricNode);
	SAFE_RELEASE(pNodeTable);
	SAFE_RELEASE(pNodeRubric);
	SAFE_RELEASE(pNodeTotal);
	SAFE_RELEASE(pNodeTotalGeneral);
	SAFE_RELEASE(pNodeLabel);
}

IA::~IA(void)
{
	SAFE_RELEASE(pNodeEmetteur);
	SAFE_RELEASE(pNodeQuantity);
	SAFE_RELEASE(pNodeEmetteurRow);
	SAFE_RELEASE(m_currentEmetteurNode);
	SAFE_RELEASE(pNodeDepot);
	SAFE_RELEASE(pNodePourcentage);
	SAFE_RELEASE(pNodeFooter);
	SAFE_RELEASE(pNodeFooterRow);
}

void OPCH3::loadInitialNodes()
{
	pNodeRubric = pNodeTotal = pParam = pParamVal = NULL;
	IXMLDOMNodeList* pNodes = NULL;
	HRESULT hr = S_OK;
	//loadTemplateNode(pNodeWorkSheet, "{RUBRIC}", &pLabelRow, &pNodeRubric);
	//loadTemplateNode(pNodeWorkSheet, "{TOTAL}", &pLabelRow, &pNodeTotal);
	//loadTemplateNode(pNodeWorkSheet, "0", &pValRow, &pRubricVal);
	pNodeWorkSheet->selectNodes(L".//*[text()='{RUBRIC}']", &pNodes);
	if (pNodes)
	{
		CHK_HR(pNodes->get_item(0, &pNodeRubric));//<Data />

		pNodeRubric->get_parentNode(&pNodeRubric);
		SAFE_RELEASE(pNodes);
	}
	pNodes = NULL;
	pNodeWorkSheet->selectNodes(L".//*[text()='{TOTAL}']", &pNodes);
	if (pNodes)
	{
		CHK_HR(pNodes->get_item(0, &pNodeTotal));//<Data />

		pNodeTotal->get_parentNode(&pNodeTotal);
		SAFE_RELEASE(pNodes);
	}

	pNodes = NULL;
	pNodeWorkSheet->selectNodes(L".//*[text()='0']", &pNodes);
	if (pNodes)
	{
		CHK_HR(pNodes->get_item(0, &pRubricVal));//<Data />

		pRubricVal->get_parentNode(&pRubricVal);
		SAFE_RELEASE(pNodes);
	}

	pNodeRubric->get_parentNode(&pLabelRow);
	pRubricVal->get_parentNode(&pValRow);

	pLabelRow->removeChild(pNodeRubric,NULL);
	pLabelRow->removeChild(pNodeTotal,NULL);

	pLabelRow->get_parentNode(&pNodeTable);
	pNodeTable->removeChild(pValRow,0);

	pRubricVal->get_nextSibling(&pTotalVal);

	pValRow->removeChild(pTotalVal,NULL);
	pValRow->removeChild(pRubricVal,NULL);

	nFirstIndex = 0;
	m_ColCount = 0;
	n_RemovedCol = 2;
	n_RemovedRow = 1;
CleanUp:
	SAFE_RELEASE(pNodes);
}

OPCH3::OPCH3(IXMLDOMDocument* pXMLNode, IXMLDOMNode* pNodeWorkSheet, std::string worksheet_name,std::string report_type, int fundCount, int colCount) : fundCount(fundCount), colCount(colCount),
pNodeWorkSheet(pNodeWorkSheet), worksheet_name(worksheet_name), report_type(report_type), pXMLNode(pXMLNode)
{
	pNodeRubric = pNodeTotal = pParam = pParamVal = pRubricVal = pLabelRow = pValRow = pCurrLabelCell = pCurrValCell = pCurrRubricVal = NULL;
	toUpdate = false;
	loadInitialNodes();
}

void OPCH3::updateFormula()
{
	stringstream ss;
	ss << "=SUM(";
		for(auto iter = mLabelToSum.begin();iter != mLabelToSum.end();++iter)
		{
			ss << "RC[" << *iter << "]";
			if (iter != mLabelToSum.end()-1) {
				ss << ",";
			}
		}

		ss<< ")";
		BSTR bstr = _com_util::ConvertStringToBSTR(ss.str().c_str());
	setNodeAttribute(pXMLNode,pCurrRubricVal,L"ss:Formula",bstr);
		mLabelToSum.clear();
}

void OPCH3::appendRow(StBilan* oneRow, long fund_id,string sConsultDate, bool isFirstRow)
{
	IXMLDOMNode* pNode = 0;
	pCurrValRow = 0;
	pValRow->cloneNode(VARIANT_TRUE,&pNode);
	pNodeTable->appendChild(pNode,&pCurrValRow);
	for(int i = 0;i<colCount;i++)
		appendColumn(oneRow[i].rubric,oneRow[i].label,oneRow[i].query,oneRow[i].rubric_type,oneRow[i].info_type,oneRow[i].sum,fund_id,sConsultDate,isFirstRow);
}

void OPCH3::makeTitles(StBilan* oneRow)
{
	IXMLDOMNode* pNode = 0;
	for(int i = 0;i<colCount;i++)
	{
		if(currentRubric != string(oneRow[i].rubric))
		{
			pNodeRubric->cloneNode(VARIANT_TRUE,&pNode);

			pLabelRow->appendChild(pNode,&pCurrLabelCell);
			setCellValue(pCurrLabelCell,oneRow[i].rubric);

			currentRubric = string(oneRow[i].rubric);
		}
		if(string(oneRow[i].rubric_type) != RUB_L && string(oneRow[i].label) != "")
		{
			pNodeRubric->cloneNode(VARIANT_TRUE,&pNode);
			pLabelRow->appendChild(pNode,&pCurrLabelCell);
			setCellValue(pCurrLabelCell,oneRow[i].label);
		}
	}
}
void OPCH3::appendColumn(const char* rubric, const char* label, const char* query,const char* rubric_type, const char* info_type, const char* sum, long fund_id, string consultDate, bool isFirstRow)
{
	/************************* CASE : TOTAL **************************/
	if(string(rubric_type) == RUB_T || string(rubric_type) == RUB_TB || string(rubric_type) == RUB_TC)
	{
		/*last rubric before total*/
		if(m_ColCount>0) 
				m_rubricLabelCounts.push_back(m_LabelCount);

		stringstream ss;
		int rowCount = 0;
		ss << "=SUM(";
		for(int i = m_rubricLabelCounts.size()-1;i>=0;i--)
		{
			rowCount += (m_rubricLabelCounts[i] + 1);
			ss << "RC[" << -rowCount << "]";
			if (i != 0) {
				ss << ",";
			}
		}

		if(string(rubric_type) == RUB_TC)
		{
			rowCount++;
			ss << ",RC[" << -rowCount << "]";
		}
		ss<< ")";

		IXMLDOMNode *pNode = NULL, *pOutNode = NULL;
		pTotalVal->cloneNode(VARIANT_TRUE,&pNode);
		
		pCurrValRow->appendChild(pNode,&pOutNode);
		BSTR bstr = _com_util::ConvertStringToBSTR(ss.str().c_str());
		setNodeAttribute(pXMLNode,pOutNode,L"ss:Formula",bstr);
		m_ColCount++;

		nFirstIndex = 0; //the index is set to 0 every time we pass a total rubic
		m_rubricLabelCounts.clear();
		SAFE_RELEASE(pNode);
		return;
	}
	/*******************************************************************************************************************/

	/************************ CASE : RUBRIC + LABEL **********************************************/
	IXMLDOMNode* pNode = NULL;
	pRubricVal->cloneNode(VARIANT_TRUE,&pNode);
	pCurrValRow->appendChild(pNode,&pCurrValCell);

	if(currentRubric != string(rubric))
	{
		if(toUpdate)
		{
			updateFormula();
		}
		pCurrRubricVal = pCurrValCell; //pCurrRubricVal is used to update the formula (pCurrRubricVal must point to the last rubric node so we can update its formula)

		if(string(rubric_type) != RUB_L) //meaning if the rubric type is not a label type
		{
			currentRubric = string(rubric);
			pRubricVal->cloneNode(VARIANT_TRUE,&pNode);
			pCurrValRow->appendChild(pNode,&pCurrValCell);	
		}

		if(nFirstIndex>0 && string(sum) != "N") //not to add the rubric in case 1 : first rubric, case 2 : the label of the rubric specifies that it shouldn't be added
			m_rubricLabelCounts.push_back(m_LabelCount);

		m_ColCount++;
		m_LabelCount = 0; // set the label counter to 0 for the new rubric
	}

	/********************** Data feed in label node **************************/
	if(string(info_type)=="F") // formula info type
	{
		BSTR bstr = _com_util::ConvertStringToBSTR(query);
		setNodeAttribute(pXMLNode,pCurrValCell,L"ss:Formula",bstr);
	}
	else /*by default : query*/
	{
		string sQuery(query);
		StrReplace(sQuery, "{FUND_ID}", to_string((long long)fund_id));
		StrReplace(sQuery, "{VL_DATE}", consultDate);
		if (string(info_type) == "QN")
		{
			double result = SqlUtils::QueryReturning1Double(sQuery.c_str());
			setCellValue(pCurrValCell, result);
		}

		if (string(info_type) == "QS")
		{
			string result = SqlUtils::QueryReturning1StringException(sQuery.c_str());
			IXMLDOMNode* pNodeData = 0;
			pCurrValCell->get_firstChild(&pNodeData);
			setNodeAttribute(pXMLNode, pNodeData, L"ss:Type", L"String");
			setCellValue(pCurrValCell, result.c_str());
		}
	}
	m_ColCount++;
	
	if(string(sum) != "N") 
		nFirstIndex++;
	if(string(rubric_type) != RUB_L) //this shouldn't incerment label_count in case of a label-type rubric
		m_LabelCount++;

	/* this is used for updateFormula */
	if(string(sum) != "N" && m_LabelCount != 0)
		mLabelToSum.push_back(m_LabelCount);
	/*********************************************/

	toUpdate = string(rubric_type) != RUB_L;
	SAFE_RELEASE(pNode);
}

void OPCH3::updateExpandedColumn()
{
	BSTR sNodeTxt = NULL;

	getNodeAttribute(pNodeTable, L"ss:ExpandedColumnCount", &sNodeTxt);
	int expandedRowCount = _wtoi(sNodeTxt);
	expandedRowCount += (colCount - n_RemovedCol); // 4 deleted tempalte row
	wchar_t temp_str[5];
	_itow_s(++expandedRowCount, temp_str, 10);
	setNodeAttribute(pXMLNode,pNodeTable, L"ss:ExpandedColumnCount", SysAllocString(temp_str));
CleanUp:
	SysFreeString(sNodeTxt);
}

void OPCH3::updateExpandedRow()
{
	BSTR sNodeTxt = NULL;
	
	getNodeAttribute(pNodeTable, L"ss:ExpandedRowCount", &sNodeTxt);
	int expandedRowCount = _wtoi(sNodeTxt);
	expandedRowCount += fundCount - n_RemovedRow; 
	wchar_t temp_str[5];
	_itow_s(++expandedRowCount, temp_str, 10);
	setNodeAttribute(pXMLNode,pNodeTable, L"ss:ExpandedRowCount", SysAllocString(temp_str));
}

OPCH3::~OPCH3()
{
	SAFE_RELEASE(pNodeRubric);
	SAFE_RELEASE(pNodeTotal);
	SAFE_RELEASE(pParam);
	SAFE_RELEASE(pParamVal);
	SAFE_RELEASE(pRubricVal);
	SAFE_RELEASE(pLabelRow);
	SAFE_RELEASE(pValRow);
	SAFE_RELEASE(pCurrLabelCell);
	SAFE_RELEASE(pCurrValCell);
	pCurrRubricVal = NULL; //this can't be deleted because it's shared with pCurrValCell and pCurrValCell is already deleted
}
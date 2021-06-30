#include "Inc/CtxEditList.h"

using namespace eff::gui;
using namespace std;

CtxEditList::CtxEditList(CSRFitDialog *dialog, int ERId_List,
	void (*selectedIndexChangedHandler)(CtxEditList *, int lineNumber),
	void (*doubleClickHandler)(CtxEditList *, int lineNumber))
	: CSREditList(dialog, ERId_List), SelectedIndexChangedHandler(selectedIndexChangedHandler), DoubleClickHandler(doubleClickHandler)
{
	fDynamicSize = true;
	fResizeInDialog = true;
	//Initialize(); // This calls the version of base class (not overriden version)
};

void CtxEditList::Selection(int lineNumber)
{
	if (SelectedIndexChangedHandler != NULL) {
		SelectedIndexChangedHandler(this, lineNumber);
	}
}

void CtxEditList::DoubleClick(int lineNumber)
{
	if (DoubleClickHandler != NULL) {
		DoubleClickHandler(this, lineNumber);
	}
}

void CtxEditList::ReloadData()
{
	if (!m_Query.empty()) {
		LoadData(m_Query.c_str());
	}
}

void CtxEditList::Selection(int colNumber, const char * cellValue)
{
	int lineCount = this->GetLineCount();
	for(int i=0; i<lineCount; i++) {
		char val[2048] = {'\0'};
		this->LoadElement(i, colNumber, val);
		if (std::string(val) == cellValue) {
			this->Selection(i);
			break;
		}
	}
}

void CtxEditList::GetSelectedValue(int colNumber, void * address)
{
	long lineNumber, col;
	GetSelectedCell(lineNumber, col);
	LoadElement(lineNumber, colNumber, address);
}

void CtxEditList::GetLineState(int lineIndex, eTextStyleType &style, eTextColorType &color, Boolean	&isSelected, Boolean &canEdit, Boolean &isStrikedThrough) const
{
	GetLineColor(lineIndex, color);
}

vector<void *> CtxEditList::GetMultiSelectedValues(int colNumber)
{
	vector<void *> result;
	vector<long> selectedLines = this->GetSelectedLines();
	for(int i=0; i<selectedLines.size(); i++) {
		void * elem = new void *;
		this->LoadElement(selectedLines[i], colNumber, elem);
		result.push_back(elem);
	}
	return result;
}

string CtxEditList::GetCombineSelectedString(int colNumber)
{
	string result;
	vector<long> selectedLines = this->GetSelectedLines();
	for(int i=0; i<selectedLines.size(); i++) {
		void * elem = new void *;
		this->LoadElement(selectedLines[i], colNumber, elem);
		result = result + ",'" + (char *)elem + "'";
	}
	if (selectedLines.size() > 0) {
		result = result.substr(1);
	}
	return result;
}
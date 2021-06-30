#include "Inc/CtxComboBox.h"
#include "Inc/Log.h"
#include "Inc/SqlUtils.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include <string>
#include <algorithm>

using namespace sophis::sql;
using namespace eff::gui;
using namespace std;

#define MSG_LEN 255

CtxComboBox::CtxComboBox(CSRFitDialog *dialog, int ERId_Menu, void (*f)(CtxComboBox *))
	: CSRCustomMenu(dialog, ERId_Menu), m_Enabled(true), SelectedIndexChangedHandler(f) {}

CtxComboBox::CtxComboBox(CSRFitDialog *dialog, int ERId_Menu, const char * query, bool enabled, long selectedValue, void (*f)(CtxComboBox *), bool fireEvent)
	: CSRCustomMenu(dialog, ERId_Menu), m_Enabled(enabled), SelectedIndexChangedHandler(f)
{
	if (query != NULL) {
		LoadData(query, selectedValue, fireEvent);
	}
}

CtxComboBox::CtxComboBox(CSREditList *list, int ERId_Menu, void (*f)(CtxComboBox *))
	: CSRCustomMenu(list, ERId_Menu), m_Enabled(true), SelectedIndexChangedHandler(f) {}

CtxComboBox::CtxComboBox(CSREditList *list, int ERId_Menu, const char * query, bool enabled, long selectedValue, void (*f)(CtxComboBox *), bool fireEvent)
	: CSRCustomMenu(list, ERId_Menu), m_Enabled(enabled), SelectedIndexChangedHandler(f)
{
	if (query != NULL) {
		LoadData(query, selectedValue, fireEvent);
	}
}

void CtxComboBox::LoadData(const char * query, long selectedValue, bool fireEvent)
{
	ResetMenu();
	m_Items.clear();

	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(2, sizeof(CtxComboBoxItem));
	ADD(gabarit, CtxComboBoxItem, value, rdfInteger);
	ADD(gabarit, CtxComboBoxItem, label, rdfString);

	CtxComboBoxItem * arrItems;
	int count = 0;
	//7.1.3
	//try {
		errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrItems, &count);
		if (err)
		{
			char msg[MSG_LEN] = {'\0'};
			_snprintf_s(msg, sizeof(msg), "CtxComboBox initialization. Database error %d with query : %s", err, query);
			GetDialog()->Message(msg);
		}

		for(int i=0; i<count; i++) {
			AddElement(arrItems[i].label);
			m_Items.push_back(arrItems[i]);
			if (arrItems[i].value == selectedValue) {
				SetListValue(i+1);
				if (fireEvent) {
					Action();
				}
			}
			
		}
	//} catch (const sophis::sql::OracleException &ex) {
	//	GetDialog()->Message(ex.getError().c_str());
	//}
	delete gabarit;
}

void CtxComboBox::LoadItems(const char * arrItems[], unsigned size, long selectedIndex)
{
	for(int i=0; i<size; i++) {
		AddElement(arrItems[i]);
		if (i == selectedIndex) {
			SetListValue(i+1);
			Action();
		}
		CtxComboBoxItem newItem = {i};
		strncpy_s(newItem.label, arrItems[i], 256); 
		m_Items.push_back(newItem);
	}
}

void CtxComboBox::Open()
{
	if (!m_Enabled) {
		Disable();
	}
}

void CtxComboBox::Action()
{
	if (SelectedIndexChangedHandler != NULL) {
		SelectedIndexChangedHandler(this);
	}
}

//void CtxComboBox::AddElement(const char* element)
//{
	//vector<CtxComboBoxItem>::iterator it = find_if(m_Items.begin(), m_Items.end(), [&](const CtxComboBoxItem &item){ return (string(item.label) == element); });
	//if (it == m_Items.end()) {
		//CSRCustomMenu::AddElement(element);
	//} else {
	//	WARN("Duplicate element in ComboBox %s", element);
	//}
//}

int CtxComboBox::SelectedIndex() const
{
	return GetListValue() - 1;
}

void CtxComboBox::SelectedIndex(const int selectedIndex, bool fireEvent)
{
	int oldSelectedIndex = SelectedIndex();
	if (oldSelectedIndex != selectedIndex) {
		SetListValue(selectedIndex + 1);
		if (fireEvent) {
			Action();
		}
	}
}

long CtxComboBox::SelectedValue() const
{
	CtxComboBoxItem * selectedItem = SelectedItem();
	if (selectedItem == NULL) return 0;
	return selectedItem->value;
}

void CtxComboBox::SelectedValue(const long selectedValue, bool fireEvent)
{
	for(int i=0; i<m_Items.size(); i++)
	{
		if (selectedValue == m_Items[i].value) {
			SelectedIndex(i, fireEvent);
			break;
		}
	}
}

CtxComboBoxItem * CtxComboBox::SelectedItem() const
{
	short selectedIndex = SelectedIndex();

	//short selectedIndex = 0;
	//GetValue(&selectedIndex);
	if (selectedIndex < 0) return NULL;
	CtxComboBoxItem selectedItem = m_Items.at(selectedIndex);
	return &selectedItem;
}

string CtxComboBox::SelectedText() const
{
	CtxComboBoxItem * selectedItem = SelectedItem();
	if (selectedItem == NULL) return textValue;
	char duplicateLabel[255] = {'\0'};
	strcpy_s(duplicateLabel, selectedItem->label);
	return string(duplicateLabel);
}

void CtxComboBox::SelectedText(const string& selectedText, bool fireEvent)
{
	for(int i=0; i<m_Items.size(); i++)
	{
		if (selectedText == m_Items[i].label) {
			SelectedIndex(i, fireEvent);
			return;
		}
	}
	SetListValue(0);
	SetText(selectedText.c_str());
	textValue = selectedText;
}

bool CtxComboBox::ContainsItemText(const char * text)
{
	for(int i=0; i<m_Items.size(); i++)
	{
		if (string(text) == m_Items[i].label) {
			return true;
		}
	}
	return false;
}

bool CtxComboBox::ContainsItemValue(long value)
{
	for(int i=0; i<m_Items.size(); i++)
	{
		if (value == m_Items[i].value) {
			return true;
		}
	}
	return false;
}

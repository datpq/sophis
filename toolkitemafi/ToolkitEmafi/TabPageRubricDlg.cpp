#include "TabPageRubricDlg.h"
#include "Resource\resource.h"
#include "CtxComboBox.h"
#include "CtxElements.h"
#include "EdlRubric.h"
#include "SqlUtils.h"
#include "StringUtils.h"
#include <string>
#include "Log.h"
#include "Constants.h"
//#include "SphInc/gui/SphDialogUI.h"
//#include <Windows.h>

using namespace eff::emafi::gui;
using namespace eff::gui;
using namespace eff::utils;

string getSelectedCodeBilanR(CtxComboBox * cbo) {
	string selectedTxt = cbo->SelectedText();
	return selectedTxt;
}

void TabPageRubricDlg_CboBilanRubric_SelectedIndexChanged(CtxComboBox * sender)
{
	string codeBilan = getSelectedCodeBilanR(sender);
	string query = StrFormat("\n\
SELECT ID, RUBRIC, \n\
	CASE RUBRIC_TYPE \n\
	WHEN '%s' THEN 'Rubrique' \n\
	WHEN '%s' THEN 'Rubrique Bordure' \n\
	WHEN '%s' THEN 'Total Haut' \n\
	WHEN '%s' THEN 'Total Bas' \n\
	WHEN '%s' THEN 'Total Cumulé' \n\
	WHEN '%s' THEN 'Total Général' \n\
	WHEN '%s' THEN 'Catégorie' \n\
	WHEN '%s' THEN 'Label' END \n\
FROM EMAFI_RUBRIC WHERE REPORT_TYPE = '%s' ORDER BY REPORT_ORDER", RUB_R, RUB_RB, RUB_T, RUB_TB, RUB_TC, RUB_TG, RUB_C, RUB_L, codeBilan);
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstRubric = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_RUBRIC - ID_ITEM_SHIFT);
	lstRubric->LoadData(query.c_str());
	lstRubric->Selection(0);
}

void TabPageRubricDlg_LstRubric_SelectedIndexChanged(CtxEditList * sender, int lineNumber)
{
	EdlRubricItem selectedItem = EdlRubricItem();
	int col = 1;
	sender->LoadElement(lineNumber, col++, selectedItem.rubric);
	sender->LoadElement(lineNumber, col++, selectedItem.rubric_type);
	TabPageRubricDlg *dialog = (TabPageRubricDlg *)sender->GetDialog();
	CtxComboBox * cboRubricType = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_RubricType - ID_ITEM_SHIFT);
	cboRubricType->SelectedText(selectedItem.rubric_type);
	CSRElement *txtElement = dialog->GetElementByAbsoluteId(IDC_TXT_Rubric - ID_ITEM_SHIFT);
	txtElement->SetValue(selectedItem.rubric);
	dialog->UpdateElements(false);
}

bool TabPageRubricDlg::IsAddMode()
{
	CSRElement * cmdAdd = this->GetElementByAbsoluteId(IDC_CMD_Add - ID_ITEM_SHIFT);
	return !cmdAdd->IsEnabled();
}

void TabPageRubricDlg::UpdateElements(bool isAddMode)
{
	CSRElement * cmdUp = this->GetElementByAbsoluteId(IDC_CMD_Up - ID_ITEM_SHIFT);
	CSRElement * cmdDown = this->GetElementByAbsoluteId(IDC_CMD_Down - ID_ITEM_SHIFT);
	CSRElement * cmdAdd = this->GetElementByAbsoluteId(IDC_CMD_Add - ID_ITEM_SHIFT);
	CSRElement * cmdDelete = this->GetElementByAbsoluteId(IDC_CMD_Delete - ID_ITEM_SHIFT);
	CSRElement * cboRubricType = this->GetElementByAbsoluteId(IDC_CBO_RubricType - ID_ITEM_SHIFT);
	CSRElement * txtRubric = this->GetElementByAbsoluteId(IDC_TXT_Rubric - ID_ITEM_SHIFT);

	CtxGuiUtils::Enabled(cmdUp, !isAddMode);
	CtxGuiUtils::Enabled(cmdDown, !isAddMode);
	CtxGuiUtils::Enabled(cmdAdd, !isAddMode);
	CtxGuiUtils::Enabled(cmdDelete, !isAddMode);
	if (isAddMode) {
		txtRubric->SetValue("");
		txtRubric->SetFocus();
	}
}

void TabPageRubricDlg_CmdAdd_OnClick(CtxButton * sender)
{
	TabPageRubricDlg *dialog = (TabPageRubricDlg *)sender->GetDialog();
	dialog->UpdateElements(true);
}

void TabPageRubricDlg_CmdUpdate_OnClick(CtxButton * sender)
{
	TabPageRubricDlg *dialog = (TabPageRubricDlg *)sender->GetDialog();
	try {
		CtxComboBox * cboBilan = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Bilan - ID_ITEM_SHIFT);
		CtxComboBox * cboRubricType = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_RubricType - ID_ITEM_SHIFT);
		CSRElement * txtRubric = dialog->GetElementByAbsoluteId(IDC_TXT_Rubric - ID_ITEM_SHIFT);
		EdlRubricItem selectedItem = EdlRubricItem();
		txtRubric->GetValue(&selectedItem.rubric);
		string rubricType = cboRubricType->SelectedText();
		rubricType = rubricType == "Rubrique" ? RUB_R :
			(rubricType == "Rubrique Bordure" ? RUB_RB :
			(rubricType == "Total Haut" ? RUB_T :
			(rubricType == "Total Bas" ? RUB_TB :
			(rubricType == "Total Cumulé" ? RUB_TC :
			(rubricType == "Label" ? RUB_L :
			(rubricType == "Total Général" ? RUB_TG : RUB_C))))));

		CtxEditList * lstRubric = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_RUBRIC - ID_ITEM_SHIFT);
		string codeBilan = getSelectedCodeBilanR(cboBilan);
		string sRubric = StrReplace(string(selectedItem.rubric), "'", "''");
		if (dialog->IsAddMode()) {
			string insertQuery = StrFormat("\
INSERT INTO EMAFI_RUBRIC(ID, RUBRIC, REPORT_TYPE, REPORT_ORDER, RUBRIC_TYPE) \n\
VALUES (( \n\
	SELECT MIN(ID) FROM( \n\
		SELECT MAX(ID) + 1 ID FROM EMAFI_RUBRIC WHERE REPORT_TYPE = '%s' \n\
		UNION \n\
		SELECT NVL(MAX(ID), 0) + 1 ID FROM EMAFI_RUBRIC) \n\
	WHERE ID NOT IN (SELECT ID FROM EMAFI_RUBRIC)), '%s', '%s', \n\
	(SELECT NVL(MAX(REPORT_ORDER), 0) + 1 FROM EMAFI_RUBRIC WHERE REPORT_TYPE = '%s'), '%s')",
	codeBilan, sRubric.c_str(), codeBilan, codeBilan, rubricType.c_str());
			//DEBUG(insertQuery.c_str());
			SqlUtils::QueryWithoutResultException(insertQuery.c_str());
		} else {
			lstRubric->GetSelectedValue(0, &selectedItem.id);

			SqlUtils::QueryWithoutResultException("UPDATE EMAFI_RUBRIC SET RUBRIC = '%s', RUBRIC_TYPE = '%s' WHERE ID = '%ld'",
				sRubric.c_str(), rubricType.c_str(), selectedItem.id);
		}
		CSRSqlQuery::Commit();
		lstRubric->ReloadData();
		lstRubric->Selection(0, selectedItem.rubric);
	} catch(const CSROracleException &e) {
		dialog->Message(LoadResourceString(MSG_DB_ERROR, e.getError().c_str()).c_str());
	}
}

void TabPageRubricDlg_CmdDelete_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstRubric = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_RUBRIC - ID_ITEM_SHIFT);
	EdlRubricItem selectedItem = EdlRubricItem();
	lstRubric->GetSelectedValue(0, &selectedItem.id);
	long sqlCount = SqlUtils::QueryReturning1LongException("SELECT COUNT(*) FROM EMAFI_LABEL WHERE ID_RUBRIC = %ld", selectedItem.id);
	if (sqlCount > 0) {
		dialog->Message(LoadResourceString(MSG_RURIC_LABEL).c_str());
		return;
	}
	//if (IDYES == MessageBox(CSRFitDialogUI::GetHWND(*dialog), "Voulez vous vraiment supprimer la rubrique ?", dialog->GetTitle(), MB_YESNO | MB_ICONQUESTION)) {
	if (dialog->ConfirmDialog(LoadResourceString(MSG_DELETE_RUBRIC).c_str()) == CSRFitDialog::CONFIRM_DIALOG_YES) {
		SqlUtils::QueryWithoutResultException("BEGIN EMAFI.RUBRIC_DELETE(%d); END;", selectedItem.id);
		CSRSqlQuery::Commit();
		sqlCount = SqlUtils::QueryReturning1LongException("SELECT EMAFI.GET_OUTPUT_VALUE('SqlRowCount') FROM DUAL");
		if (sqlCount == 1) {
			lstRubric->ReloadData();
			lstRubric->Selection(0);
			dialog->Message(LoadResourceString(MSG_DELETED_RUBRIC).c_str());
		} else {
			dialog->Message(LoadResourceString(MSG_NO_SUCCESS).c_str());
		}
	};
}

void TabPageRubricDlg_CmdUp_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstRubric = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_RUBRIC - ID_ITEM_SHIFT);
	long lineNumber;
	long column;
	lstRubric->GetSelectedCell(lineNumber, column);
	EdlRubricItem selectedItem;
	lstRubric->LoadElement(lineNumber, 0, &selectedItem.id);
	SqlUtils::QueryWithoutResultException("BEGIN EMAFI.RUBRIC_UP(%d); END;", selectedItem.id);
	CSRSqlQuery::Commit();
	long rowCount = SqlUtils::QueryReturning1LongException("SELECT EMAFI.GET_OUTPUT_VALUE('SqlRowCount') FROM DUAL");

	if (rowCount > 0) {
		lstRubric->ReloadData();
		lstRubric->Select(lineNumber - 1);
		lstRubric->Update();
	}
}

void TabPageRubricDlg_CmdDown_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstRubric = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_RUBRIC - ID_ITEM_SHIFT);
	long lineNumber;
	long column;
	lstRubric->GetSelectedCell(lineNumber, column);
	EdlRubricItem selectedItem;
	lstRubric->LoadElement(lineNumber, 0, &selectedItem.id);
	SqlUtils::QueryWithoutResultException("BEGIN EMAFI.RUBRIC_DOWN(%d); END;", selectedItem.id);
	CSRSqlQuery::Commit();
	long rowCount = SqlUtils::QueryReturning1LongException("SELECT EMAFI.GET_OUTPUT_VALUE('SqlRowCount') FROM DUAL");

	if (rowCount > 0) {
		lstRubric->ReloadData();
		lstRubric->Select(lineNumber + 1);
		lstRubric->Update();
	}
}

TabPageRubricDlg::TabPageRubricDlg() : CSRFitDialog()
{
	DEBUG("BEGIN");

	fResourceId	= IDD_TAB_Rubric - ID_DIALOG_SHIFT;
	NewElementList(9);

	int nb = 0;
	if (fElementList)
	{

		
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Up - ID_ITEM_SHIFT, &TabPageRubricDlg_CmdUp_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Down - ID_ITEM_SHIFT, &TabPageRubricDlg_CmdDown_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Add - ID_ITEM_SHIFT, &TabPageRubricDlg_CmdAdd_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Update - ID_ITEM_SHIFT, &TabPageRubricDlg_CmdUpdate_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Delete - ID_ITEM_SHIFT, &TabPageRubricDlg_CmdDelete_OnClick);
		fElementList[nb++]	= new CSREditText(this, IDC_TXT_Rubric - ID_ITEM_SHIFT, 100);
		CtxComboBox * cboRubricType = new CtxComboBox(this, IDC_CBO_RubricType - ID_ITEM_SHIFT, (void (*)(CtxComboBox *))NULL);
		const char* arrRubricTypes[] = {"Rubrique", "Rubrique Bordure", "Total Haut", "Total Bas", "Total Cumulé", "Total Général", "Catégorie", "Label"};
		cboRubricType->LoadItems(arrRubricTypes, 8);
		fElementList[nb++] = cboRubricType;

		fElementList[nb++] = new EdlRubric(this, IDC_LST_RUBRIC - ID_ITEM_SHIFT, &TabPageRubricDlg_LstRubric_SelectedIndexChanged);
		CtxComboBox * cboBilan = new CtxComboBox(this, IDC_CBO_Bilan - ID_ITEM_SHIFT,
			"SELECT ROWNUM, REPORT_TYPE FROM (SELECT REPORT_TYPE FROM EMAFI_REPORT WHERE ENABLED = 1 ORDER BY ETAT_TYPE, REPORT_ORDER)",
			true, 1, &TabPageRubricDlg_CboBilanRubric_SelectedIndexChanged);
		fElementList[nb++] = cboBilan;

	
		
	//	const char* arrBilans[] = {"Actif", "Passif"};
		//cboBilan->LoadItems(arrBilans, 2, 0);
	}
	UpdateElements(false);

	DEBUG("END");
}

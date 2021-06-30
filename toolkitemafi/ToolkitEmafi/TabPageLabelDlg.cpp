#include "TabPageLabelDlg.h"
#include "Resource\resource.h"
#include "Log.h"
#include "CtxComboBox.h"
#include "CtxElements.h"
#include "EdlLabel.h"
#include "SqlUtils.h"
#include "StringUtils.h"
#include "SphInc/SphUserRights.h"
#include "StringUtils.h"
#include "Resource\resource.h"

using namespace eff::emafi::gui;
using namespace eff::gui;
using namespace eff::utils;

string getSelectedCodeBilanL(CtxComboBox * cbo) {
	string selectedTxt = cbo->SelectedText();
	return selectedTxt;
}

void TabPageLabelDlg_CboBilanLabel_SelectedIndexChanged(CtxComboBox * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();

	string codeBilan = getSelectedCodeBilanL(sender);
	CtxComboBox * cboRubric =(CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Rubric - ID_ITEM_SHIFT);
	string queryCode = StrFormat("SELECT ID, RUBRIC FROM EMAFI_RUBRIC WHERE REPORT_TYPE = '%s' ORDER BY REPORT_ORDER", codeBilan);
	cboRubric->LoadData(queryCode.c_str());
	  
	string query = StrFormat("\n\
SELECT EL.ID_RUBRIC, ER.RUBRIC, EL.ID, EL.LABEL, EA.ACCOUNT_NUMBER, EA.ACCOUNT_DESC,  EL.ID_PARENT, EMAFI.BOOL_TO_STR_YN(EL.ENABLED) \n\
FROM EMAFI_LABEL EL \n\
  JOIN EMAFI_RUBRIC ER ON ER.ID = EL.ID_RUBRIC \n\
  LEFT JOIN EMAFI_ACCOUNT EA ON EA.ID_LABEL = EL.ID \n\
WHERE ER.REPORT_TYPE = '%s' \n\
ORDER BY ER.REPORT_TYPE, ER.REPORT_ORDER, EL.REPORT_ORDER", codeBilan);
	CtxEditList * lstLabel = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_LABEL - ID_ITEM_SHIFT);
	lstLabel->LoadData(query.c_str());
	lstLabel->Selection(0);
}

void TabPageLabelDlg_LstLabel_SelectedIndexChanged(CtxEditList * sender, int lineNumber)
{
	EdlLabelItem selectedItem = EdlLabelItem();
	int col = 0;
	sender->LoadElement(lineNumber, col++, &selectedItem.id_rubric);
	sender->LoadElement(lineNumber, col++, selectedItem.rubric);
	sender->LoadElement(lineNumber, col++, &selectedItem.id);
	sender->LoadElement(lineNumber, col++, selectedItem.label);	
	sender->LoadElement(lineNumber, col++, selectedItem.account_number);
	col++; //ACCOUNT_DESC
	sender->LoadElement(lineNumber, col++, &selectedItem.id_parent);
	sender->LoadElement(lineNumber, col++, selectedItem.active);
	TabPageLabelDlg *dialog = (TabPageLabelDlg *)sender->GetDialog();
	CtxComboBox * cboRubric = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Rubric - ID_ITEM_SHIFT);
	cboRubric->SelectedValue((long)selectedItem.id_rubric);
	CSRElement * txtElement = dialog->GetElementByAbsoluteId(IDC_TXT_Label - ID_ITEM_SHIFT);
	txtElement->SetValue(selectedItem.label);
	CtxComboBox * cboParent = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_PARENT - ID_ITEM_SHIFT);
	cboParent->SelectedValue((long)selectedItem.id_parent);
	dialog->UpdateElements(false);

	CSRElement * cmdDeleteLabel = dialog->GetElementByAbsoluteId(IDC_CMD_Delete - ID_ITEM_SHIFT);
	if (string(selectedItem.account_number).empty()) {
		cmdDeleteLabel->Enable();
	} else {
		cmdDeleteLabel->Disable();
	}
}

bool TabPageLabelDlg::IsAddMode()
{
	CSRElement * cmdAdd = this->GetElementByAbsoluteId(IDC_CMD_Add - ID_ITEM_SHIFT);
	return !cmdAdd->IsEnabled();
}

void TabPageLabelDlg::UpdateElements(bool isAddMode)
{
	CSRElement * cmdUp = this->GetElementByAbsoluteId(IDC_CMD_Up - ID_ITEM_SHIFT);
	CSRElement * cmdDown = this->GetElementByAbsoluteId(IDC_CMD_Down - ID_ITEM_SHIFT);
	CSRElement * cmdAdd = this->GetElementByAbsoluteId(IDC_CMD_Add - ID_ITEM_SHIFT);
	CSRElement * cmdDelete = this->GetElementByAbsoluteId(IDC_CMD_Delete - ID_ITEM_SHIFT);
	CSRElement * cboRubric = this->GetElementByAbsoluteId(IDC_CBO_Rubric - ID_ITEM_SHIFT);
	CSRElement * txtLabel = this->GetElementByAbsoluteId(IDC_TXT_Label - ID_ITEM_SHIFT);

	CtxGuiUtils::Enabled(cmdUp, !isAddMode);
	CtxGuiUtils::Enabled(cmdDown, !isAddMode);
	CtxGuiUtils::Enabled(cmdAdd, !isAddMode);
	CtxGuiUtils::Enabled(cmdDelete, !isAddMode);
	CtxGuiUtils::Enabled(cboRubric, isAddMode);
	if (isAddMode) {
		txtLabel->SetValue("");
		cboRubric->SetFocus();
	}
}

void TabPageLabelDlg_CmdAdd_OnClick(CtxButton * sender)
{
	TabPageLabelDlg *dialog = (TabPageLabelDlg *)sender->GetDialog();
	dialog->UpdateElements(true);
}

void TabPageLabelDlg_CmdUpdate_OnClick(CtxButton * sender)
{
	TabPageLabelDlg *dialog = (TabPageLabelDlg *)sender->GetDialog();
	try {
		CtxComboBox * cboRubric = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Rubric - ID_ITEM_SHIFT);
		CtxComboBox * cboParent = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_PARENT - ID_ITEM_SHIFT);
		CSRElement * txtLabel = dialog->GetElementByAbsoluteId(IDC_TXT_Label - ID_ITEM_SHIFT);
		CSRElement * chkEnabled = dialog->GetElementByAbsoluteId(IDC_CHK_Enabled - ID_ITEM_SHIFT);
		CtxEditList * lstLabel = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_LABEL - ID_ITEM_SHIFT);
		EdlLabelItem selectedItem = EdlLabelItem();
		selectedItem.id_parent = cboParent->SelectedValue();
		txtLabel->GetValue(&selectedItem.label);
		bool itemEnabled;
		chkEnabled->GetValue(&itemEnabled);
		string sLabel = StrReplace(string(selectedItem.label), "'", "''");
		if (dialog->IsAddMode()) {
			selectedItem.id_rubric = cboRubric->SelectedValue();
			string insertQuery = StrFormat("\
INSERT INTO EMAFI_LABEL(ID, ID_RUBRIC, LABEL, REPORT_ORDER, ENABLED, ID_PARENT) \n\
VALUES (( \n\
	SELECT MIN(ID) FROM( \n\
		SELECT MAX(ID) + 1 ID FROM EMAFI_LABEL WHERE ID_RUBRIC = %d \n\
		UNION \n\
		SELECT MAX(ID) + 1 ID FROM EMAFI_LABEL) \n\
	WHERE ID NOT IN (SELECT ID FROM EMAFI_LABEL)), %d, '%s', \n\
	(SELECT NVL(MAX(REPORT_ORDER), 0) + 1 FROM EMAFI_LABEL WHERE ID_RUBRIC = %d), %d, %s)",
	(long)selectedItem.id_rubric, (long)selectedItem.id_rubric, sLabel.c_str(), (long)selectedItem.id_rubric, itemEnabled, selectedItem.id_parent == 0 ? "NULL" : to_string((long long)selectedItem.id_parent).c_str());
			//DEBUG(insertQuery.c_str());
			SqlUtils::QueryWithoutResultException(insertQuery.c_str());
		} else {
			lstLabel->GetSelectedValue(2, &selectedItem.id) ;
			string updateQuery = StrFormat("UPDATE EMAFI_LABEL SET LABEL = '%s', ENABLED = %d, ID_PARENT = %s WHERE ID = %d",
				sLabel.c_str(), itemEnabled, selectedItem.id_parent == 0 ? "NULL" : to_string((long long)selectedItem.id_parent).c_str(), (long)selectedItem.id);
			SqlUtils::QueryWithoutResultException(updateQuery.c_str());
		}
		CSRSqlQuery::Commit();
		lstLabel->ReloadData();
		lstLabel->Selection(3, selectedItem.label);
	} catch(const CSROracleException &e) {
		dialog->Message(LoadResourceString(MSG_DB_ERROR, e.getError().c_str()).c_str());
	}
}

void TabPageLabelDlg_CmdDelete_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstLabel = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_LABEL - ID_ITEM_SHIFT);
	EdlLabelItem selectedItem = EdlLabelItem();
	lstLabel->GetSelectedValue(2, &selectedItem.id);
	if (dialog->ConfirmDialog(LoadResourceString(MSG_LABEL_ASK).c_str()) == CSRFitDialog::CONFIRM_DIALOG_YES) {
		SqlUtils::QueryWithoutResultException("BEGIN EMAFI.LABEL_DELETE(%d); END;", (long)selectedItem.id);
		long sqlCount = SqlUtils::QueryReturning1LongException("SELECT EMAFI.GET_OUTPUT_VALUE('SqlRowCount') FROM DUAL");
		CSRSqlQuery::Commit();

		if (sqlCount == 1) {
			lstLabel->ReloadData();
			lstLabel->Selection(0);
			dialog->Message(LoadResourceString(MSG_LABEL_DELETED).c_str());
		} else {
			dialog->Message(LoadResourceString(MSG_NO_SUCCESS).c_str());
		}
	}
}

void TabPageLabelDlg_CmdUp_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstLabel = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_LABEL - ID_ITEM_SHIFT);
	long lineNumber;
	long column;
	lstLabel->GetSelectedCell(lineNumber, column);
	EdlLabelItem selectedItem = EdlLabelItem();
	int col = 2;
	lstLabel->LoadElement(lineNumber, col++, &selectedItem.id);
	SqlUtils::QueryWithoutResultException("BEGIN EMAFI.LABEL_UP(%d); END;", (long)selectedItem.id);
	long rowCount = SqlUtils::QueryReturning1LongException("SELECT EMAFI.GET_OUTPUT_VALUE('SqlRowCount') FROM DUAL");
	CSRSqlQuery::Commit();

	if (rowCount > 0) {
		lstLabel->ReloadData();
		lstLabel->Select(lineNumber - 1);
		//lstLabel->Selection(lineNumber - 1);
		lstLabel->Update();
	}
}

void TabPageLabelDlg_CmdDown_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstLabel = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_LABEL - ID_ITEM_SHIFT);
	long lineNumber;
	long column;
	lstLabel->GetSelectedCell(lineNumber, column);
	EdlLabelItem selectedItem = EdlLabelItem();
	int col = 2;
	lstLabel->LoadElement(lineNumber, col++, &selectedItem.id);
	SqlUtils::QueryWithoutResultException("BEGIN EMAFI.LABEL_DOWN(%d); END;", (long)selectedItem.id);
	long rowCount = SqlUtils::QueryReturning1LongException("SELECT EMAFI.GET_OUTPUT_VALUE('SqlRowCount') FROM DUAL");
	CSRSqlQuery::Commit();

	if (rowCount > 0) {
		lstLabel->ReloadData();
		lstLabel->Select(lineNumber + 1);
		//lstLabel->Selection(lineNumber + 1);
		lstLabel->Update();
	}
}

TabPageLabelDlg::TabPageLabelDlg() : CSRFitDialog()
{
	fResourceId	= IDD_TAB_Label - ID_DIALOG_SHIFT;

	NewElementList(11);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new CtxComboBox(this, IDC_CBO_Rubric - ID_ITEM_SHIFT, (void (*)(CtxComboBox *))NULL);
		fElementList[nb++]	= new CSREditText(this, IDC_TXT_Label - ID_ITEM_SHIFT, 100);
		fElementList[nb++]	= new CSRCheckBox(this, IDC_CHK_Enabled - ID_ITEM_SHIFT, true);
		fElementList[nb++] = new CtxComboBox(this, IDC_CBO_PARENT - ID_ITEM_SHIFT,
			"SELECT ID, LABEL || '    ' || ID LABEL FROM EMAFI_LABEL UNION SELECT 0, ' ' FROM DUAL ORDER BY LABEL");
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Up - ID_ITEM_SHIFT, &TabPageLabelDlg_CmdUp_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Down - ID_ITEM_SHIFT, &TabPageLabelDlg_CmdDown_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Add - ID_ITEM_SHIFT, &TabPageLabelDlg_CmdAdd_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Update - ID_ITEM_SHIFT, &TabPageLabelDlg_CmdUpdate_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Delete - ID_ITEM_SHIFT, &TabPageLabelDlg_CmdDelete_OnClick);
		fElementList[nb++] = new EdlLabel(this, IDC_LST_LABEL - ID_ITEM_SHIFT, &TabPageLabelDlg_LstLabel_SelectedIndexChanged);
		fElementList[nb++] = new CtxComboBox(this, IDC_CBO_Bilan - ID_ITEM_SHIFT,
			"SELECT ROWNUM, REPORT_TYPE FROM (SELECT REPORT_TYPE FROM EMAFI_REPORT WHERE ENABLED = 1 ORDER BY ETAT_TYPE, REPORT_ORDER)",
			true, 1, &TabPageLabelDlg_CboBilanLabel_SelectedIndexChanged);
	}
}

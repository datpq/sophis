#include "TabPageAccountDlg.h"
#include "Resource\resource.h"
#include "CtxElements.h"
#include "CtxTemplates.h"
#include "CtxComboBox.h"
#include "EdlAccount.h"
#include "StringUtils.h"
#include "SqlUtils.h"
#include "StringUtils.h"
#include "Resource\resource.h"


using namespace eff::emafi::gui;
using namespace eff::gui;
using namespace eff::utils;

string GetAccountNumber(CSRFitDialog * dialog)
{
	CtxComboBox * cboAccountNumber = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_AccountNumber - ID_ITEM_SHIFT);
	string sAccountNumber = cboAccountNumber->SelectedText();
	if (sAccountNumber.empty()) {
		char sAccountPattern[100] = {'\0'};
		CSRElement * txtAccountPattern = dialog->GetElementByAbsoluteId(IDC_TXT_ACCOUNT_PATTERN - ID_ITEM_SHIFT);
		txtAccountPattern->GetValue(&sAccountPattern);
		sAccountNumber = sAccountPattern;
	}
	return sAccountNumber;
}

void TabPageAccountDlg_AccountNumber_Changed(TabPageAccountDlg * dialog, const char * accountNumber)
{
	if (string(accountNumber).empty()) return;
	CtxComboBox * cboLabel = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Label - ID_ITEM_SHIFT);
	CtxComboBox * cboAccountType = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Type - ID_ITEM_SHIFT);
	CSRElement * txtAccountDesc = dialog->GetElementByAbsoluteId(IDC_TXT_AccountDesc - ID_ITEM_SHIFT);
	try {
		long count = SqlUtils::QueryReturning1LongException("SELECT COUNT(*) FROM EMAFI_ACCOUNT WHERE ACCOUNT_NUMBER = '%s'", accountNumber);
		long idLabel = cboLabel->SelectedValue();
		if (count == 1) {
			idLabel = SqlUtils::QueryReturning1LongException("SELECT ID_LABEL FROM EMAFI_ACCOUNT WHERE ACCOUNT_NUMBER = '%s'", accountNumber);
			cboLabel->SelectedValue(idLabel, false);
		} else if (count > 1) {
			count = SqlUtils::QueryReturning1LongException("SELECT COUNT(*) FROM EMAFI_ACCOUNT WHERE ACCOUNT_NUMBER = '%s' AND ID_LABEL = %d", accountNumber, idLabel);
		}
		dialog->UpdateElements(count == 0);

		string acccount_desc = SqlUtils::QueryReturning1StringException("SELECT ACCOUNT_DESC FROM EMAFI_ACCOUNT WHERE ACCOUNT_NUMBER = '%s'", accountNumber);
		txtAccountDesc->SetValue(acccount_desc.c_str());

		string account_type = SqlUtils::QueryReturning1StringException("SELECT UNIQUE ACCOUNT_TYPE from EMAFI_ACCOUNT WHERE ACCOUNT_NUMBER= '%s'", accountNumber);
		cboAccountType->SetText(account_type.c_str());
	} catch (const CSRNoRowException) {
		txtAccountDesc->SetValue("");
	}
}

void TabPageAccountDlg_AccountPattern_Validating(CtxEditText * sender)
{
	TabPageAccountDlg *dialog = (TabPageAccountDlg *)sender->GetDialog();
	char sAccountPattern[100] = {'\0'};
	sender->GetValue(&sAccountPattern);

	//empty the AccountPattern text box if found in the list
	CtxComboBox * cboAccountNumber = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_AccountNumber - ID_ITEM_SHIFT);
	if (cboAccountNumber->ContainsItemText(sAccountPattern)) {
		sender->SetValue("");
		cboAccountNumber->SelectedText(sAccountPattern, false);
	} else {
		cboAccountNumber->SelectedText("", false);
	}

	TabPageAccountDlg_AccountNumber_Changed(dialog, sAccountPattern);
}

void TabPageAccountDlg_LstAccount_SelectedIndexChanged(CtxEditList * sender, int lineNumber)
{
	EdlAccountItem selectedItem = EdlAccountItem();
	int col = 0;
	sender->LoadElement(lineNumber, col++, selectedItem.account_number);
	sender->LoadElement(lineNumber, col++, selectedItem.account_desc);
	sender->LoadElement(lineNumber, col++, &selectedItem.id_label);
	sender->LoadElement(lineNumber, col++, selectedItem.label);
	sender->LoadElement(lineNumber, col++, selectedItem.account_type);

	TabPageAccountDlg *dialog = (TabPageAccountDlg *)sender->GetDialog();
	CtxComboBox * cboAccountNumber = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_AccountNumber - ID_ITEM_SHIFT);
	CSRElement * txtAccountPattern = dialog->GetElementByAbsoluteId(IDC_TXT_ACCOUNT_PATTERN - ID_ITEM_SHIFT);
	if (cboAccountNumber->ContainsItemText(selectedItem.account_number)) {
		txtAccountPattern->SetValue("");
		cboAccountNumber->SelectedText(selectedItem.account_number, false);
	} else {
		txtAccountPattern->SetValue(selectedItem.account_number);
		cboAccountNumber->SelectedText("", false);
	}
	CtxComboBox * cboLabel = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Label - ID_ITEM_SHIFT);
	cboLabel->SelectedValue(selectedItem.id_label, false);
	CtxComboBox * cboType= (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Type - ID_ITEM_SHIFT);
	cboType->SelectedText(selectedItem.account_type, false);
	CSRElement * txtAccountDesc = dialog->GetElementByAbsoluteId(IDC_TXT_AccountDesc - ID_ITEM_SHIFT);
	txtAccountDesc->SetValue(selectedItem.account_desc);
	dialog->UpdateElements(false);
}

void TabPageAccountDlg_ComboAccountNumber_SelectedIndexChanged(CtxComboBox * sender)
{
	TabPageAccountDlg *dialog = (TabPageAccountDlg *)sender->GetDialog();
	CtxComboBox * cboAccountNumber = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_AccountNumber - ID_ITEM_SHIFT);
	string sAccountNumber = cboAccountNumber->SelectedText();

	//found in the list --> empty the AccountPattern text box
	if (!sAccountNumber.empty()) {
		CSRElement * txtAccountPattern = dialog->GetElementByAbsoluteId(IDC_TXT_ACCOUNT_PATTERN - ID_ITEM_SHIFT);
		txtAccountPattern->SetValue("");
	}

	TabPageAccountDlg_AccountNumber_Changed(dialog, sAccountNumber.c_str());
}

void TabPageAccountDlg_ComboLabel_SelectedIndexChanged(CtxComboBox * sender)
{
	TabPageAccountDlg *dialog = (TabPageAccountDlg *)sender->GetDialog();
	CtxComboBox * cboLabel = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Label - ID_ITEM_SHIFT);

	string sAccountNumber = GetAccountNumber(dialog);
	long idLabel = cboLabel->SelectedValue();
	long count = SqlUtils::QueryReturning1LongException("SELECT COUNT(*) FROM EMAFI_ACCOUNT WHERE ACCOUNT_NUMBER = '%s' AND ID_LABEL = %d", sAccountNumber.c_str(), idLabel);
	dialog->UpdateElements(count == 0);
}

void TabPageAccountDlg_ComboAccountType_SelectedIndexChanged(CtxComboBox * sender)
{
	TabPageAccountDlg *dialog = (TabPageAccountDlg *)sender->GetDialog();
	sender->AddElement((sender->SelectedText()).c_str());
}

void TabPageAccountDlg_CmdAdd_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxComboBox * cboLabel = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Label - ID_ITEM_SHIFT);
	CtxComboBox * cboType = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Type - ID_ITEM_SHIFT);
	CSRElement * txtAccountDesc = dialog->GetElementByAbsoluteId(IDC_TXT_AccountDesc - ID_ITEM_SHIFT);
	char sAccountDesc[100] = {'\0'};
	txtAccountDesc->GetValue(&sAccountDesc);
	string strAccountDesc(sAccountDesc);
	StrReplace(strAccountDesc, "'", "''");
	string sAccountNumber = GetAccountNumber(dialog);
	
	SqlUtils::QueryWithoutResultException("INSERT INTO EMAFI_ACCOUNT(ACCOUNT_NUMBER, ACCOUNT_DESC, ID_LABEL, ACCOUNT_TYPE) VALUES('%s', '%s', %d, '%s')",
		sAccountNumber.c_str(), strAccountDesc.c_str(), cboLabel->SelectedValue(), cboType->SelectedText());
	CSRSqlQuery::Commit();
	CtxEditList * lstAccount = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_ACCOUNT - ID_ITEM_SHIFT);
	lstAccount->ReloadData();
	lstAccount->Selection(0, sAccountNumber.c_str());
}

void TabPageAccountDlg_CmdUpdate_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxComboBox * cboLabel = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Label - ID_ITEM_SHIFT);
	CtxComboBox * cboType = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Type - ID_ITEM_SHIFT);
	CSRElement * txtAccountDesc = dialog->GetElementByAbsoluteId(IDC_TXT_AccountDesc - ID_ITEM_SHIFT);
	CtxComboBox * cboAccountType = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Type - ID_ITEM_SHIFT);
	char sAccountDesc[100] = {'\0'};
	txtAccountDesc->GetValue(&sAccountDesc);
	string strAccountDesc(sAccountDesc);
	StrReplace(strAccountDesc, "'", "''");

	string sAccountNumber = GetAccountNumber(dialog);

	string sAccountType = cboType->SelectedText();
	SqlUtils::QueryWithoutResultException("UPDATE EMAFI_ACCOUNT SET ACCOUNT_DESC = '%s', ACCOUNT_TYPE= '%s' WHERE ACCOUNT_NUMBER = '%s' AND ID_LABEL = %d",
		strAccountDesc.c_str(), sAccountType.c_str(), sAccountNumber.c_str(), cboLabel->SelectedValue());
	CSRSqlQuery::Commit();

	CtxEditList * lstAccount = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_ACCOUNT - ID_ITEM_SHIFT);
	lstAccount->ReloadData();
	lstAccount->Selection(0, sAccountNumber.c_str());
}

void TabPageAccountDlg_CmdDelete_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	if (dialog->ConfirmDialog("Voulez vous vraiment enlever la liaison ?") == CSRFitDialog::CONFIRM_DIALOG_YES) {
		CtxComboBox * cboLabel = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_Label - ID_ITEM_SHIFT);
		string sAccountNumber = GetAccountNumber(dialog);

		SqlUtils::QueryWithoutResultException("DELETE EMAFI_ACCOUNT WHERE ACCOUNT_NUMBER = '%s' AND ID_LABEL = %d",
			sAccountNumber.c_str(), cboLabel->SelectedValue());
		CSRSqlQuery::Commit();
		long sqlCount = SqlUtils::QueryReturning1LongException("SELECT COUNT(*) FROM EMAFI_ACCOUNT WHERE ACCOUNT_NUMBER = '%s' AND ID_LABEL = %d",
			sAccountNumber.c_str(), cboLabel->SelectedValue());
		if (sqlCount == 0) {
			CtxEditList * lstAccount = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_ACCOUNT - ID_ITEM_SHIFT);
			lstAccount->ReloadData();
			lstAccount->Selection(0);
			dialog->Message(LoadResourceString(MSG_LINK_REMOVED).c_str());
		} else {
			dialog->Message(LoadResourceString(MSG_NO_SUCCESS).c_str());
		}
	}
}

void TabPageAccountDlg::UpdateElements(bool isAddMode)
{
	CSRElement * cmdAdd = this->GetElementByAbsoluteId(IDC_CMD_Add - ID_ITEM_SHIFT);
	CSRElement * cmdUpdate = this->GetElementByAbsoluteId(IDC_CMD_Update - ID_ITEM_SHIFT);
	CSRElement * cmdDelete = this->GetElementByAbsoluteId(IDC_CMD_Delete - ID_ITEM_SHIFT);

	CtxGuiUtils::Enabled(cmdAdd, isAddMode);
	CtxGuiUtils::Enabled(cmdUpdate, !isAddMode);
	CtxGuiUtils::Enabled(cmdDelete, !isAddMode);
}

TabPageAccountDlg::TabPageAccountDlg() : CSRFitDialog()
{
	fResourceId	= IDD_TAB_Account - ID_DIALOG_SHIFT;

	NewElementList(9);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++] = new ComboBoxAccountNumber(this, IDC_CBO_AccountNumber - ID_ITEM_SHIFT, &TabPageAccountDlg_ComboAccountNumber_SelectedIndexChanged);
		fElementList[nb++]	= new CtxEditText(this, IDC_TXT_ACCOUNT_PATTERN - ID_ITEM_SHIFT, 100, &TabPageAccountDlg_AccountPattern_Validating);
		fElementList[nb++]	= new CSREditText(this, IDC_TXT_AccountDesc - ID_ITEM_SHIFT, 100);
		fElementList[nb++]	= new CtxComboBox(this, IDC_CBO_Label - ID_ITEM_SHIFT, "SELECT ID, LABEL FROM EMAFI_LABEL ORDER BY 2",
			true, -1, &TabPageAccountDlg_ComboLabel_SelectedIndexChanged, false);
		fElementList[nb++]	= new CtxComboBox(this, IDC_CBO_Type - ID_ITEM_SHIFT, "SELECT ROWNUM, ACCOUNT_TYPE FROM (SELECT DISTINCT ACCOUNT_TYPE FROM EMAFI_ACCOUNT) ORDER BY 2");
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Add - ID_ITEM_SHIFT, &TabPageAccountDlg_CmdAdd_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Update - ID_ITEM_SHIFT, &TabPageAccountDlg_CmdUpdate_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Delete - ID_ITEM_SHIFT, &TabPageAccountDlg_CmdDelete_OnClick);
		CtxEditList * lstAccount = new EdlAccount(this, IDC_LST_ACCOUNT - ID_ITEM_SHIFT, &TabPageAccountDlg_LstAccount_SelectedIndexChanged);
		fElementList[nb++] = lstAccount;
		lstAccount->LoadData("\n\
							 SELECT ACCOUNT_NUMBER, ACCOUNT_DESC, ID_LABEL, LABEL, ACCOUNT_TYPE, \n\
							 EMAFI.GET_ACCOUNT_INFORMATIONS(ACCOUNT_NUMBER,'CATEGORY'), EMAFI.GET_ACCOUNT_INFORMATIONS(ACCOUNT_NUMBER,'CLASSIFICATION') \n\
							 FROM EMAFI_ACCOUNT EA \n\
							 JOIN EMAFI_LABEL EL ON EL.ID = EA.ID_LABEL \n\
							 ORDER BY 1");
		lstAccount->Selection(0);
	}
}

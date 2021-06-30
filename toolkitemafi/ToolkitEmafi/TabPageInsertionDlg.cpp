#include "TabPageInsertionDlg.h"
#include "Resource\resource.h"
#include "CtxElements.h"
#include "CtxComboBox.h"
#include "CtxTemplates.h"
#include "SqlUtils.h"
#include "SphUtils.h"
#include "SphInc\gui\SphEditList.h" 
#include "EdlOd.h"
#include "GestionOdDlg.h"
#include "StringUtils.h"
#include "Resource\resource.h"



using namespace eff::gui;
using namespace eff::emafi::gui;
using namespace eff::utils;
using namespace std;


void EdlOdUpdate::Initialize()
{
	fColumnCount = 14;
	fColumns = new SSColumn[fColumnCount];

	int i=0;
	fColumns[i].fColumnName = "FolioId";
	fColumns[i].fColumnWidth = 0;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CtxStaticLong(this, i);

	i++;
	fColumns[i].fColumnName = "Portefeuille";
	fColumns[i].fColumnWidth = 120;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 40);

	i++;
	fColumns[i].fColumnName = "Compte";
	fColumns[i].fColumnWidth = 80;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

	i++;
	fColumns[i].fColumnName = "Sens";
	fColumns[i].fColumnWidth = 40;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSRStaticText(this, i, 1);

	i++;
	fColumns[i].fColumnName = "Montant";
	fColumns[i].fColumnWidth = 90;
	fColumns[i].fAlignmentType = aRight;
	fColumns[i].fElement = new CtxStaticDouble(this, i);

	i++;
	fColumns[i].fColumnName = "TiersId";
	fColumns[i].fColumnWidth = 0;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CtxStaticLong(this, i);

	i++;
	fColumns[i].fColumnName = "Tiers";
	fColumns[i].fColumnWidth = 200;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);
	
	i++;
	long dateValeur = 0;
	fColumns[i].fColumnName = "Date valeur";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSRStaticDate(this, i);

	i++;
	fColumns[i].fColumnName = "Devise";
	fColumns[i].fColumnWidth = 120;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 100);
	
	i++;
	fColumns[i].fColumnName = "DeviseCode";
	fColumns[i].fColumnWidth = 0;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CtxStaticLong(this, i);
	i++;
	fColumns[i].fColumnName = "Journal";
	fColumns[i].fColumnWidth = 120;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 100);

	i++;
	fColumns[i].fColumnName = "Piece";
	fColumns[i].fColumnWidth = 120;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 100);

	i++;
	fColumns[i].fColumnName = "Comentaire";
	fColumns[i].fColumnWidth = 120;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 100);

	i++;
	fColumns[i].fColumnName = "Code commentaire";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 100);
	
	SetLineCount(0);
	//SaveLineCount(GetLineCount() + 1);
}

void TabPageInsertionDlg_CmdAdd_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstOdUpdate = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_ODUPDATE - ID_ITEM_SHIFT);

	CtxComboBox * cboFolio = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_FOLIO - ID_ITEM_SHIFT);
	long folioId = cboFolio->SelectedValue();
	CtxComboBox * cboAccountNumber = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_AccountNumber - ID_ITEM_SHIFT);
	CtxComboBox * cboSens = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_SENS - ID_ITEM_SHIFT);
	CtxEditDouble * txtAmount = (CtxEditDouble *)dialog->GetElementByAbsoluteId(IDC_TXT_MONTANT - ID_ITEM_SHIFT);
	CtxComboBox * cboTiers = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_TIERS - ID_ITEM_SHIFT);
	long tiersId = cboTiers->SelectedValue();
	CSREditDate * txtDate = (CSREditDate *)dialog->GetElementByAbsoluteId(IDC_DATEVALEUR - ID_ITEM_SHIFT);
	long dateValue;
	txtDate->GetValue(&dateValue);
	CSREditText * txtJournal = (CSREditText *)dialog->GetElementByAbsoluteId(IDC_TXT_JOURNAL - ID_ITEM_SHIFT);
	CSREditText * txtPiece = (CSREditText *)dialog->GetElementByAbsoluteId(IDC_TXT_PIECE - ID_ITEM_SHIFT);
	CtxComboBox * cboComment = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_COMMENT - ID_ITEM_SHIFT);
	CtxComboBox * cboDevise = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_DEVISE - ID_ITEM_SHIFT);
	long codeDevise = cboDevise->SelectedValue();
	if (cboFolio->SelectedIndex() < 0) {
		CSRFitDialog::Message(LoadResourceString(MSG_FOLIO).c_str());
		cboFolio->SetFocus();
		return;
	}
	if (cboAccountNumber->SelectedIndex() < 0) {
		CSRFitDialog::Message(LoadResourceString(MSG_ACCOUNT).c_str());
		cboAccountNumber->SetFocus();
		return;
	}
	if (cboSens->SelectedIndex() < 0) {
		CSRFitDialog::Message(LoadResourceString(MSG_DEBIT_CREDIT).c_str());
		cboSens->SetFocus();
		return;
	}
	if (cboTiers->SelectedIndex() < 0) {
		CSRFitDialog::Message(LoadResourceString(MSG_THIRD_PARTY).c_str());
		cboTiers->SetFocus();
		return;
	}
	double * amount = txtAmount->GetValue();
	if (*amount == 0) {
		CSRFitDialog::Message(LoadResourceString(MSG_AMOUNT).c_str());
		txtAmount->SetFocus();
		return;
	}
	if (dateValue == 0) {
		CSRFitDialog::Message(LoadResourceString(MSG_VALUE_DATE).c_str());
		txtDate->SetFocus();
		return;
	}
	if (cboDevise->SelectedIndex() < 0) {
		CSRFitDialog::Message(LoadResourceString(MSG_CURRENCY).c_str());
		txtDate->SetFocus();
		return;
	}

	lstOdUpdate->SaveLineCount(lstOdUpdate->GetLineCount() + 1);
	int lineIdx = lstOdUpdate->GetLineCount() - 1;
	int col = 0;
	lstOdUpdate->SaveElement(lineIdx, col++, &folioId);
	lstOdUpdate->SaveElement(lineIdx, col++, (void *)cboFolio->SelectedText().c_str());
	lstOdUpdate->SaveElement(lineIdx, col++, (void *)cboAccountNumber->SelectedText().c_str());
	lstOdUpdate->SaveElement(lineIdx, col++, (void *)cboSens->SelectedText().c_str());
	lstOdUpdate->SaveElement(lineIdx, col++, amount);
	lstOdUpdate->SaveElement(lineIdx, col++, &tiersId);
	lstOdUpdate->SaveElement(lineIdx, col++, (void *)cboTiers->SelectedText().c_str());
	lstOdUpdate->SaveElement(lineIdx, col++, &dateValue);
	lstOdUpdate->SaveElement(lineIdx, col++, (void *)cboDevise->SelectedText().c_str());
	lstOdUpdate->SaveElement(lineIdx, col++, &codeDevise);
	lstOdUpdate->SaveElement(lineIdx, col++, txtJournal->GetValue());
	lstOdUpdate->SaveElement(lineIdx, col++, txtPiece->GetValue());

	cboFolio->Enable(lstOdUpdate->GetLineCount() == 0);
	cboDevise->Enable(lstOdUpdate->GetLineCount() == 0);

	
	std::string commentaire = cboComment->SelectedText();
    std::string commentaire_code= cboComment->SelectedText();

	
	int pos = cboComment->SelectedText().find("    ");
	commentaire_code.erase(pos,commentaire_code.length());
	commentaire.erase(0,pos+4);

    lstOdUpdate->SaveElement(lineIdx, col++,(void*)commentaire.c_str());
    lstOdUpdate->SaveElement(lineIdx, col++,(void*)commentaire_code.c_str());	


	txtDate->Enable(lstOdUpdate->GetLineCount() == 0);
	cboComment->Enable(lstOdUpdate->GetLineCount() == 0);
}

void TabPageInsertionDlg_CmdDelete_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstOdUpdate = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_ODUPDATE - ID_ITEM_SHIFT);
	long line, column;
	lstOdUpdate->GetSelectedCell(line, column);
	if (line >= 0) {
		lstOdUpdate->RemoveLine(line);
		if (line >=1) {
			lstOdUpdate->Select(line - 1);
		}
	}

	CtxComboBox * cboFolio = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_FOLIO - ID_ITEM_SHIFT);
	cboFolio->Enable(lstOdUpdate->GetLineCount() == 0);
	CtxComboBox * cboDevise = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_DEVISE - ID_ITEM_SHIFT);
	cboDevise->Enable(lstOdUpdate->GetLineCount() == 0);
	CSREditDate * txtDate = (CSREditDate *)dialog->GetElementByAbsoluteId(IDC_DATEVALEUR - ID_ITEM_SHIFT);
	txtDate->Enable(lstOdUpdate->GetLineCount() == 0);

	CtxComboBox * cboComment = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_COMMENT - ID_ITEM_SHIFT);
	cboComment->Enable(lstOdUpdate->GetLineCount() == 0);
}

void TabPageInsertionDlg_CmdSave_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstOdUpdate = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_ODUPDATE - ID_ITEM_SHIFT);
	
	GestionOdDlg * tabDialog = (GestionOdDlg*)dialog->GetParent();
	TabGestionOD * tabButton = (TabGestionOD*)tabDialog->GetElementByAbsoluteId(IDC_TAB_GestionOD - ID_ITEM_SHIFT);
	CSRFitDialog * consult_dialog = tabButton->GetPageDlg(1);
	CtxEditList * lstConsult = (CtxEditList *)consult_dialog->GetElementByAbsoluteId(IDC_LST_OD - ID_ITEM_SHIFT);

	double sumC = 0, sumD = 0;
	for(int i=0; i<lstOdUpdate->GetLineCount(); i++) {
		char sens[2] = {'\0'};
		double amount = 0;
		lstOdUpdate->LoadElement(i, 3, sens);
		lstOdUpdate->LoadElement(i, 4, &amount);
		if (string(sens) == "D") {
			sumD += amount;
		} else {
			sumC += amount;
		}
	}
	if ( sumC != sumD) {
		dialog->Message(LoadResourceString(MSG_ERROR_OD).c_str());
		return;
	}

	/* control : accounts of OD belong to the same classification */

	char query[SQL_LEN] = {'\0'};
	long control_indicator;
	int i;
	for(i=0; i<lstOdUpdate->GetLineCount(); i++) 
	{
		char account[50];

		lstOdUpdate->LoadElement(i,2,account);
		_snprintf_s(query, sizeof(query),
			"SELECT EMAFI.IS_BILAN('%s') FROM DUAL",account);

		long is_bilan = 0; // 1 if it is , 0 if it is not
		errorCode err = CSRSqlQuery::QueryReturning1Long(query,&is_bilan);

		control_indicator+=is_bilan; 
	}

	if(control_indicator < lstOdUpdate->GetLineCount() && control_indicator > 0)
	{
		dialog->Message(LoadResourceString(MSG_CLASSIFICATION).c_str());
		return;
	}

	char tmp[255] = { '\0' };
	for(int i=0; i<lstOdUpdate->GetLineCount(); i++) 
	{
		long idFolio = 0;
		long codeDevise = 0;
		long dateValue = 0;
		long  idTiers= 0;
		double amount = 0;
		int col = 0;
		lstOdUpdate->LoadElement(i, col++, &idFolio);
		col++; //Folio Name
		lstOdUpdate->LoadElement(i, col++, tmp);
		string compte(tmp);
		lstOdUpdate->LoadElement(i, col++, tmp);
		string sens(tmp);
		lstOdUpdate->LoadElement(i, col++, &amount);
		lstOdUpdate->LoadElement(i, col++, &idTiers);
		col++; //Tiers Name
		lstOdUpdate->LoadElement(i, col++, &dateValue);
		lstOdUpdate->LoadElement(i, col++, tmp); //varchar
		string devise(tmp);
		lstOdUpdate->LoadElement(i, col++, &codeDevise);//number
		lstOdUpdate->LoadElement(i, col++, tmp);
		string journal(tmp);
		lstOdUpdate->LoadElement(i, col++, tmp);
		string piece(tmp);
		lstOdUpdate->LoadElement(i, col++, tmp);
		string comment(tmp);
		lstOdUpdate->LoadElement(i, col++, tmp);
		string commentaire_code(tmp);
		
		SqlUtils::QueryWithoutResultException("BEGIN EMAFI.INSERT_ONE_ROW(%d, '%s', '%s', %f,%d, '%s', %d,'%s', '%s','%s','%s', %d,%d); END;",
			idFolio, compte.c_str(), sens.c_str(), amount, dateValue, devise.c_str(), codeDevise, journal.c_str(), piece.c_str(),
			comment.c_str(), commentaire_code.c_str(), SphUtils::GetRiskUserId(), idTiers);
	}
	SqlUtils::QueryWithoutResultException("BEGIN EMAFI.ADD_OD; END;");
	CSRSqlQuery::Commit();

	if(lstOdUpdate->GetLineCount() != 0)
	{
		dialog->Message(LoadResourceString(MSG_SAVING_DONE).c_str());
		lstConsult->ReloadData();
	}	
	else
		dialog->Message(LoadResourceString(MSG_OD_ACCOUNTING).c_str());

	int lineCount = lstOdUpdate->GetLineCount(); //We can't use directly the GetLineCount() method cause the size decreases with the RemoveLine() method
	for(int i=0; i<lineCount; i++) 
		lstOdUpdate->RemoveLine(0); //We use always the same line cause the list gets updated at every call of th RemoveLine method

	CtxComboBox * cboFolio = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_FOLIO - ID_ITEM_SHIFT);
	cboFolio->Enable(lstOdUpdate->GetLineCount() == 0);
	CtxComboBox * cboComment = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_COMMENT - ID_ITEM_SHIFT);

	CtxComboBox * cboDevise = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_DEVISE - ID_ITEM_SHIFT);
	cboDevise->Enable(lstOdUpdate->GetLineCount() == 0);
	cboComment->Enable(lstOdUpdate->GetLineCount() == 0);
	CSREditDate * txtDate = (CSREditDate *)dialog->GetElementByAbsoluteId(IDC_DATEVALEUR - ID_ITEM_SHIFT);
	txtDate->Enable(lstOdUpdate->GetLineCount() == 0);
}

TabPageInsertionDlg::TabPageInsertionDlg() : CSRFitDialog()
{
	fResourceId	= IDD_TAB_INSERTION - ID_DIALOG_SHIFT;

	NewElementList(14);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++] = new ComboBoxFolio(this, IDC_CBO_FOLIO - ID_ITEM_SHIFT);
		fElementList[nb++] = new ComboBoxAccountNumber(this, IDC_CBO_AccountNumber - ID_ITEM_SHIFT);
		CtxComboBox * cboSens= new CtxComboBox(this, IDC_CBO_SENS - ID_ITEM_SHIFT, (void (*)(CtxComboBox *))NULL);
		fElementList[nb++] = cboSens;
		const char* arrSens[] = {"C", "D"};
		cboSens->LoadItems(arrSens, 2, 0);
		fElementList[nb++] = new CtxEditDouble(this, IDC_TXT_MONTANT - ID_ITEM_SHIFT);
		fElementList[nb++] =  new CtxComboBox(this, IDC_CBO_TIERS - ID_ITEM_SHIFT, "SELECT IDENT, NAME FROM TIERS ORDER BY NAME");
		fElementList[nb++] =  new CSREditDate(this, IDC_DATEVALEUR - ID_ITEM_SHIFT, CSRDay::GetSystemDate());
		fElementList[nb++] =  new CtxComboBox(this, IDC_CBO_DEVISE - ID_ITEM_SHIFT, "SELECT CODE, DEVISE_TO_STR (A.CODE) FROM DEVISEV2 A ORDER BY DEVISE_TO_STR (A.CODE)");
		fElementList[nb++] = new CSREditText(this, IDC_TXT_JOURNAL - ID_ITEM_SHIFT, 100);
		fElementList[nb++] = new CSREditText(this, IDC_TXT_PIECE - ID_ITEM_SHIFT, 100);
				fElementList[nb++] =  new CtxComboBox(this, IDC_CBO_COMMENT - ID_ITEM_SHIFT, 
			"SELECT ORDRE, CODE|| '    ' || DESCRIPTION FROM EMAFI_PARAMETRAGE WHERE CATEGORIE ='OD_Commentaire' ORDER BY ORDRE");
			//"SELECT ORDRE, LPAD(CODE, 5, '0') || '    ' || DESCRIPTION FROM EMAFI_PARAMETRAGE WHERE CATEGORIE ='OD_Commentaire' ORDER BY ORDRE");
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Add - ID_ITEM_SHIFT, &TabPageInsertionDlg_CmdAdd_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Delete - ID_ITEM_SHIFT, &TabPageInsertionDlg_CmdDelete_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Update - ID_ITEM_SHIFT, &TabPageInsertionDlg_CmdSave_OnClick);
		fElementList[nb++] = new EdlOdUpdate(this, IDC_LST_ODUPDATE - ID_ITEM_SHIFT);
	}
}
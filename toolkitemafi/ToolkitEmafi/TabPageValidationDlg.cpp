#include "TabPageValidationDlg.h"
#include "Resource\resource.h"
#include "CtxComboBox.h"
#include "CtxTemplates.h"
#include "CtxElements.h"
#include "EdlOd.h"
#include "SqlUtils.h"
#include "Log.h"
#include "TabPageHistoriqueDlg.h"
#include "GestionOdDlg.h"
#include "StringUtils.h"
#include "Resource\resource.h"

using namespace eff::emafi::gui;
using namespace eff::gui;
using namespace eff::utils;

void TabPageValidationDlg_CmdConsult_OnClick(CtxButton * sender)
{
	TabPageValidationDlg *dialog = (TabPageValidationDlg*)sender->GetDialog();
	
	CSREditDate *startDate = (CSREditDate *)dialog->GetElementByAbsoluteId(IDC_TXT_START_DATE - ID_ITEM_SHIFT);
	long lStartDate = 0;
	startDate->GetValue(&lStartDate);
	string sStartDate = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'YYYYMMDD') FROM DUAL", lStartDate);


	CSREditDate *endDate = (CSREditDate *)dialog->GetElementByAbsoluteId(IDC_TXT_END_DATE - ID_ITEM_SHIFT);
	long lEndDate = 0;
	endDate->GetValue(&lEndDate);
	string sEndDate = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'YYYYMMDD') FROM DUAL", lEndDate);

	string journal;
	string piece;
	CSRElement * txtJournal = dialog->GetElementByAbsoluteId(IDC_TXT_JOURNAL - ID_ITEM_SHIFT);
	txtJournal->GetValue(&journal);
	
	CSRElement * txtPiece = dialog->GetElementByAbsoluteId(IDC_TXT_PIECE - ID_ITEM_SHIFT);
	txtPiece->GetValue(&piece);

	CtxComboBox * cboDevise = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_DEVISE - ID_ITEM_SHIFT);
	
	CtxComboBoxItem selectedFolio = dialog->GetSelectedFolio();

	EdlOd * lstOd = (EdlOd *)dialog->GetElementByAbsoluteId(IDC_LST_OD - ID_ITEM_SHIFT);
	
	string queryStr = lstOd->Getquery(true,selectedFolio.value,sStartDate.c_str(),sEndDate.c_str(),cboDevise->SelectedText().c_str(),journal.c_str(),piece.c_str(), dialog->status-1);
	lstOd->LoadData(queryStr.c_str());
	lstOd->Selection(0);

}

void TabPageValidationDlg_CmdValid_OnClick(CtxButton * sender)
{
	TabPageValidationDlg *dialog = (TabPageValidationDlg*)sender->GetDialog();
	CtxEditList * lstOd = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_OD - ID_ITEM_SHIFT);
	EdlOdItem selectedItem = EdlOdItem();
	lstOd->GetSelectedValue(0, &selectedItem.num_od);
	
	GestionOdDlg * tabDialog = (GestionOdDlg*)dialog->GetParent();
	TabGestionOD * tabButton = (TabGestionOD*)tabDialog->GetElementByAbsoluteId(IDC_TAB_GestionOD - ID_ITEM_SHIFT);
	CSRFitDialog * consult_dialog = tabButton->GetPageDlg(1);
	CtxEditList * lstConsult = (CtxEditList *)consult_dialog->GetElementByAbsoluteId(IDC_LST_OD - ID_ITEM_SHIFT);
	long status_od = SqlUtils::QueryReturning1LongException("SELECT OD_STATUS from EMAFI_CRE where OD_NUM= %d",selectedItem.num_od);

	if(status_od < dialog->status)
	{
		if (dialog->ConfirmDialog(LoadResourceString(MSG_OPERATION).c_str()) == CSRFitDialog::CONFIRM_DIALOG_YES){
			long statut_cre = SqlUtils::QueryReturning1LongException("SELECT CRE_STATUS FROM EMAFI_ODSTATUS WHERE ID_STATUS= %d",dialog->status);
			SqlUtils::QueryWithoutResultException("update EMAFI_CRE SET OD_STATUS= %d ,STATUS= %d  where OD_NUM =%d", dialog->status,statut_cre, selectedItem.num_od);
			SqlUtils::QueryWithoutResultException("update ACCOUNT_POSTING SET STATUS= %d where ID IN (SELECT ID_POSTING FROM EMAFI_CRE WHERE OD_NUM =%d)", statut_cre,selectedItem.num_od);
			CSRSqlQuery::Commit();
	
			dialog->Message(LoadResourceString(MSG_VALIDATION_SUCCESS).c_str());
			lstConsult->ReloadData();
			lstOd->ReloadData();
			lstOd->Selection(0);
			}
	}
	else
		dialog->Message(LoadResourceString(MSG_NO_VALIDATION).c_str());
		
}


TabPageValidationDlg::TabPageValidationDlg(long initial_status) : CSRFitDialog()
{
	
	fResourceId	= IDD_TAB_VALIDATION - ID_DIALOG_SHIFT;
	status = initial_status;

	NewElementList(9);

	int nb = 0;
	if (fElementList)
	{
		fElementList[nb++]	= new ComboBoxFolio(this, IDC_CBO_FOLIO - ID_ITEM_SHIFT);
		long startDate = 0;
		CSRSqlQuery::QueryReturning1Long("SELECT DATE_TO_NUM(TRUNC(SYSDATE, 'Y')) FROM DUAL", &startDate);
		fElementList[nb++]	= new CSREditDate(this, IDC_TXT_START_DATE - ID_ITEM_SHIFT, startDate);
		
		
		long endDate = 0;
		CSRSqlQuery::QueryReturning1Long("SELECT DATE_TO_NUM(ADD_MONTHS(TRUNC(SYSDATE, 'Y'), 12)) - 1 FROM DUAL", &endDate);
		fElementList[nb++]	= new CSREditDate(this, IDC_TXT_END_DATE - ID_ITEM_SHIFT, endDate);



		fElementList[nb++]	= new CSREditText(this, IDC_TXT_JOURNAL - ID_ITEM_SHIFT, 100);
		fElementList[nb++]	= new CSREditText(this, IDC_TXT_PIECE - ID_ITEM_SHIFT, 100);
	    fElementList[nb++]	= new CtxButton(this, IDC_CMD_Consult2 - ID_ITEM_SHIFT, &TabPageValidationDlg_CmdConsult_OnClick);
	
		fElementList[nb++] = new CtxButton(this, IDC_CMD_Validate_OD - ID_ITEM_SHIFT, &TabPageValidationDlg_CmdValid_OnClick);
		

		fElementList[nb++]  =  new CtxComboBox(this, IDC_CBO_DEVISE - ID_ITEM_SHIFT, "SELECT CODE, DEVISE_TO_STR (A.CODE) FROM DEVISEV2 A ORDER BY DEVISE_TO_STR (A.CODE)");
		fElementList[nb++] = new EdlOd(this, IDC_LST_OD - ID_ITEM_SHIFT);
	}
}
CtxComboBoxItem TabPageValidationDlg::GetSelectedFolio()
{
	CtxComboBox *cboFolio = (CtxComboBox *)this->GetElementByAbsoluteId(IDC_CBO_FOLIO - ID_ITEM_SHIFT);
	CtxComboBoxItem * selectedFolio = cboFolio->SelectedItem();
	return *selectedFolio;
}


#include "TabPageConsultationDlg.h"
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

using namespace eff::utils;
using namespace eff::emafi::gui;
using namespace eff::gui;


struct StPosting{
	long postingID;
};
void TabPageODConsultation_LstOD_DoubleClick(CtxEditList * sender, int lineNumber)
{
	TabPageConsultationDlg *dialog = (TabPageConsultationDlg*)sender->GetDialog();
	GestionOdDlg* tabDialog = (GestionOdDlg*)dialog->GetParent();

	TabGestionOD* tabButton = (TabGestionOD*)tabDialog->GetElementByAbsoluteId(IDC_TAB_GestionOD - ID_ITEM_SHIFT);
	
	
	long num_od = 0;
	sender->LoadElement(lineNumber,0,&num_od);

	int nbPages = tabButton->GetNbPages();
	string histoDialog("HISTORIQUE OD " + to_string((long long)num_od));
	int i;
	for(i = nbPages - 1; i>=0;i--)
	{
		
		CSRFitDialog* dialog = tabButton->GetPageDlg(i);
		string dialogTitle(dialog->GetTitle());
		if(dialogTitle == histoDialog)
			break;
	}

	if(i>0)
	{
		tabButton->SetActivePage(i);
		return;
	}

	TabPageHistoDlg* HistoDialog = new TabPageHistoDlg(num_od,tabButton->GetActivePage());

	HistoDialog->SetTitle(histoDialog.c_str());

	tabButton->InsertPage(nbPages,HistoDialog);
}

void TabPageConsultationDlg_CmdConsult_OnClick(CtxButton * sender)
{
	TabPageConsultationDlg *dialog = (TabPageConsultationDlg*)sender->GetDialog();
		//
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
	
	string queryStr = lstOd->Getquery(false,selectedFolio.value,sStartDate.c_str(),sEndDate.c_str(),cboDevise->SelectedText().c_str(),journal.c_str(),piece.c_str(), dialog->id_statut-1);
	lstOd->LoadData(queryStr.c_str());
	lstOd->Selection(0);
}


void TabPageConsultationDlg_CmdDelete_OnClick(CtxButton * sender)
{
	TabPageConsultationDlg *dialog = (TabPageConsultationDlg*)sender->GetDialog();
	CtxEditList * lstOd = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_OD - ID_ITEM_SHIFT);
	EdlOdItem selectedItem = EdlOdItem();
	lstOd->GetSelectedValue(0, &selectedItem.num_od);
	long status_od = SqlUtils::QueryReturning1LongException("SELECT OD_STATUS from EMAFI_CRE where OD_NUM= %d",selectedItem.num_od);
	long count_od = SqlUtils::QueryReturning1LongException("SELECT COUNT(OD_NUM) from EMAFI_CRE where COMMENTAIRE_DESC like 'ANNUL-OD%%' and OD_NUM= %d",selectedItem.num_od);
	if(status_od <= dialog->id_statut)
	{
		if (count_od == 0)
		{

			if (dialog->ConfirmDialog("Voulez vous vraiment supprimer cette opération?") == CSRFitDialog::CONFIRM_DIALOG_YES) 
			{

				char status[255]=" ";
				SqlUtils::QueryWithoutResultException("BEGIN EMAFI.DELETE_OD(%d); END;",selectedItem.num_od);
				CSRSqlQuery::Commit();
				long sqlCount = SqlUtils::QueryReturning1LongException("SELECT EMAFI.GET_OUTPUT_VALUE('SqlRowCount') FROM DUAL");

				if (sqlCount >= 1) {
						lstOd->ReloadData();
						lstOd->Selection(0);
						dialog->Message(LoadResourceString(MSG_OD_DELETED).c_str());
					}
				else {
					dialog->Message(LoadResourceString(MSG_NO_CHANGES).c_str());
					}
			}
		}
		else
			dialog->Message(LoadResourceString(MSG_ALREADY_DELETED).c_str());
	}
	else
		dialog->Message(LoadResourceString(MSG_DELETION_DENIED).c_str());

}

TabPageConsultationDlg::TabPageConsultationDlg(long statut) : CSRFitDialog()
{
	id_statut = statut;
	fResourceId	= IDD_TAB_CONSULTATION - ID_DIALOG_SHIFT;
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
	    fElementList[nb++]	= new CtxButton(this, IDC_CMD_Consult - ID_ITEM_SHIFT, &TabPageConsultationDlg_CmdConsult_OnClick);
			 
		fElementList[nb++] = new CtxButton(this, IDC_CMD_Delete_OD - ID_ITEM_SHIFT, &TabPageConsultationDlg_CmdDelete_OnClick);

		fElementList[nb++]  =  new CtxComboBox(this, IDC_CBO_DEVISE - ID_ITEM_SHIFT, "SELECT CODE, DEVISE_TO_STR (A.CODE) FROM DEVISEV2 A ORDER BY DEVISE_TO_STR (A.CODE)");

		fElementList[nb++] = new EdlOd(this, IDC_LST_OD - ID_ITEM_SHIFT, NULL, &TabPageODConsultation_LstOD_DoubleClick);
	}
}

CtxComboBoxItem TabPageConsultationDlg::GetSelectedFolio()
{
	CtxComboBox *cboFolio = (CtxComboBox *)this->GetElementByAbsoluteId(IDC_CBO_FOLIO - ID_ITEM_SHIFT);
	CtxComboBoxItem * selectedFolio = cboFolio->SelectedItem();
	return *selectedFolio;
}
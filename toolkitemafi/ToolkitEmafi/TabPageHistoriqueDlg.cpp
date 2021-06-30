#include "TabPageHistoriqueDlg.h"
#include "Resource\resource.h"
#include "CtxComboBox.h"
#include "CtxTemplates.h"
#include "CtxElements.h"
#include "EdlOdHisto.h"
#include "SqlUtils.h"
#include "Log.h"
#include"GestionOdDlg.h"

using namespace eff::emafi::gui;
using namespace eff::gui;
using namespace eff::utils;

void Cancel_Tab_Histo(CtxButton* cancelButton)
{
	TabPageHistoDlg* histoDialog = (TabPageHistoDlg*)cancelButton->GetDialog();
	GestionOdDlg* tabDialog = (GestionOdDlg*)histoDialog->GetParent();

	TabGestionOD* tabButton = (TabGestionOD*)tabDialog->GetElementByAbsoluteId(IDC_TAB_GestionOD - ID_ITEM_SHIFT);
	//int pageIndex = tabButton->GetActivePage();
	tabButton->SetActivePage(histoDialog->generatingPageIndex);
	tabButton->RemovePage(histoDialog);
	histoDialog->Close();
}

TabPageHistoDlg::TabPageHistoDlg(int num_od, int generatingPageIndex) : CSRFitDialog(), generatingPageIndex(generatingPageIndex)
{
	fResourceId	= IDD_TAB_HISTO - ID_DIALOG_SHIFT;
	
	NewElementList(2);

	int nb = 0;
	if (fElementList)
	{
		EdlOdHisto* listOdHisto = new EdlOdHisto(this, IDC_LST_ODHISTO - ID_ITEM_SHIFT);

		char query[SQL_LEN] = {'\0'};
		_snprintf_s(query, sizeof(query),"SELECT ID, (SELECT NAME FROM RISKUSERS WHERE IDENT = USERID), VERSION, DATEMODIF,OD_NUM, ID_POSTING, ENTITY_NAME, PTF_NAME,TIERS_NAME,ACCOUNT_NUMBER,AMOUNT,SENS,POSTING_DATE, \n\
		GENERATION_DATE,JOURNAL,PIECE,COMMENTAIRE_DESC,STATUS,OD_STATUS,(SELECT NAME FROM RISKUSERS WHERE IDENT = OPERATEUR) FROM EMAFI_AUDIT_CRE WHERE OD_NUM = '%d' ORDER BY ID_POSTING, VERSION",num_od);
		listOdHisto->LoadData(query);
		fElementList[nb++] = listOdHisto;
		fElementList[nb++] = new CtxButton(this,IDC_CMD_CmdCancel - ID_ITEM_SHIFT,&Cancel_Tab_Histo);
	}
	
}


#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphEditElement.h"
#include "EdlReport.h"
#include "EdlFund.h"
#include "SphInc/SphUserRights.h"
#include "CmdGenerate.h"
#include "CtxTemplates.h"
#include "ConfigDlg.h"
#include "Log.h"
#include "EtatsReglementaireDlg.h"
#include "Resource\resource.h"
#include "SphSDBCInc/SphSQLQuery.h"

using namespace sophis::gui;
using namespace eff::gui;
using namespace eff::emafi::gui;

EtatReglementaireDlg::EtatReglementaireDlg()
{
	DEBUG("BEGIN");
	fResourceId	= IDD_DIALOG_ETATREGLEMENTAIRE - ID_DIALOG_SHIFT;

	NewElementList(6);

	int nb = 0;

	if (fElementList)
	{
	//	fElementList[nb++] = new ComboBoxFund(this, IDC_COMBO_FOND - ID_ITEM_SHIFT);
		long consultationDate = 0;
		CSRSqlQuery::QueryReturning1Long("SELECT DATE_TO_NUM(TRUNC(SYSDATE, 'Y')) - 1 FROM DUAL", &consultationDate);

		fElementList[nb++]	= new CSREditDate(this, IDC_DATE_CONSULTATION - ID_ITEM_SHIFT, consultationDate);

		fElementList[nb++] = new CmdGenerate(this,IDC_CMD_GENERATEREG - ID_ITEM_SHIFT,etatType::ETAT_REGLEMENTAIRE);
		
		EdlReport* reportList = new EdlReport(this,IDC_LST_REPORT_REG - ID_ITEM_SHIFT, 0, 30);
		reportList->LoadData("SELECT NAME, REPORT_TYPE FROM EMAFI_REPORT WHERE ETAT_TYPE  = 'R' ORDER BY REPORT_ORDER"); 
		reportList->Selection(0);
		fElementList[nb++] = reportList;

		EdlFund* fundList = new EdlFund(this,IDC_LST_FUND - ID_ITEM_SHIFT, 0, 30);
		fundList->LoadData("SELECT T.SICOVAM, T.LIBELLE FROM TITRES T WHERE T.TYPE = 'Z' AND T.MNEMO IS NOT NULL ORDER BY T.LIBELLE"); 
		fundList->Selection(0);
		fElementList[nb++] = fundList;
		
		CtxComboBox * cboFileType = new CtxComboBox(this, IDC_COMBO_FILETYPE_REG - ID_ITEM_SHIFT, (void (*)(CtxComboBox *))NULL);
		const char* arrFileTypes[] = {"EXCEL", "PDF"};
		cboFileType->LoadItems(arrFileTypes, 2, 0);

		fElementList[nb++] = cboFileType;
		fElementList[nb++]	= new CSRCancelButton(this);
	}
}

EtatReglementaireDlg::~EtatReglementaireDlg() {}
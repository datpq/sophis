/*
** Includes
*/
#include "GenerateBilanDlg.h"
#include "Resource\resource.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/gui/SphEditElement.h"
#include "EdlReport.h"
#include "SphInc/SphUserRights.h"
#include "CmdGenerate.h"
#include "CtxTemplates.h"
#include "CtxElements.h"
#include "ConfigDlg.h"
#include "Log.h"
#include "SphInc\gui\SphCheckBox.h"
/*
** Namespace
*/
using namespace sophis::gui;
using namespace eff::gui;
using namespace eff::emafi::gui;

#define MSG_LEN 255

void RadioFolioFund_OnCheckedChanged(CtxRadioButton * sender, short value)
{
	CSRFitDialog * dialog = sender->GetDialog();
	CtxComboBox *cboFolio = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_FOLIO - ID_ITEM_SHIFT);
	CtxComboBox *cboFund = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_FUND - ID_ITEM_SHIFT);
	CtxGuiUtils::Enabled(cboFolio, value == 1);
	CtxGuiUtils::Enabled(cboFund, value == 2);
	switch (value)
	{
	case 1:
		cboFolio->SetFocus();
		break;
	case 2:
		cboFund->SetFocus();
		break;
	}
}

GenerateBilanDlg::GenerateBilanDlg() : CSRFitDialog()
{
	DEBUG("BEGIN");

	fResourceId	= IDD_DLG_GenerateBilan - ID_DIALOG_SHIFT;
	NewElementList(11);

	int nb = 0;
	if (fElementList)
	{
		fElementList[nb++] = new ComboBoxFolio(this, IDC_CBO_FOLIO - ID_ITEM_SHIFT);
		fElementList[nb++] = new ComboBoxFundFolio(this, IDC_CBO_FUND - ID_ITEM_SHIFT);
		fElementList[nb++] = new CtxRadioButton(this, IDC_RADIO_FOLIO - ID_ITEM_SHIFT, IDC_RADIO_FUND - ID_ITEM_SHIFT, 1, RadioFolioFund_OnCheckedChanged);
		//StartDate = 31 december last year
		long startDate = 0;
		CSRSqlQuery::QueryReturning1Long("SELECT DATE_TO_NUM(TRUNC(SYSDATE, 'Y')) - 1 FROM DUAL", &startDate);
		//StartDate = 31 december this year
		long endDate = 0;
		CSRSqlQuery::QueryReturning1Long("SELECT DATE_TO_NUM(ADD_MONTHS(TRUNC(SYSDATE, 'Y'), 12)) - 1 FROM DUAL", &endDate);
		fElementList[nb++]	= new CSREditDate(this, IDC_TXT_START_DATE - ID_ITEM_SHIFT, startDate);
		fElementList[nb++]	= new CSREditDate(this, IDC_TXT_END_DATE - ID_ITEM_SHIFT, endDate);

		CtxComboBox * cboFileType = new CtxComboBox(this, IDC_COMBO_FILETYPE - ID_ITEM_SHIFT, (void (*)(CtxComboBox *))NULL);
		const char* arrFileTypes[] = {"EXCEL", "PDF"};
		cboFileType->LoadItems(arrFileTypes, 2, 0);
		fElementList[nb++] = cboFileType;
		
		CtxComboBox* typeDate = new CtxComboBox(this, IDC_COMBO_TYPEDATE - ID_ITEM_SHIFT,(void (*)(CtxComboBox *))NULL);
		const char* arrTypeDates[] = {"Posting Date","Generation Date"};
		typeDate->LoadItems(arrTypeDates,2);
		fElementList[nb++] = new CSRCheckBox(this, IDC_CHECK_SIMULATION - ID_ITEM_SHIFT);
		fElementList[nb++] = typeDate;

		CtxEditList* reportList = new EdlReport(this, IDC_LIST_REPORT - ID_ITEM_SHIFT, 0, 30);
		reportList->LoadData("SELECT NAME, REPORT_TYPE FROM EMAFI_REPORT WHERE ETAT_TYPE = 'C' ORDER BY REPORT_ORDER");
		reportList->Selection(0);
		fElementList[nb++] = reportList;

		fElementList[nb++] = new CmdGenerate(this, IDC_CMD_CmdGenerate - ID_ITEM_SHIFT);
		fElementList[nb++] = new CSRCancelButton(this);
	}
	
	DEBUG("END");
}

CtxComboBoxItem GenerateBilanDlg::GetSelectedFolio()
{
	CtxComboBox *cboFolio = (CtxComboBox *)this->GetElementByAbsoluteId(IDC_CBO_FOLIO - ID_ITEM_SHIFT);
	CtxComboBoxItem selectedFolio = *(cboFolio->SelectedItem());
	return selectedFolio;
}

CtxComboBoxItem GenerateBilanDlg::GetSelectedFileType()
{
	CtxComboBox* cboFileType = (CtxComboBox*)this->GetElementByAbsoluteId(IDC_COMBO_FILETYPE - ID_ITEM_SHIFT);
	CtxComboBoxItem selectedFileType = *(cboFileType->SelectedItem());
	return selectedFileType;
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ GenerateBilanDlg::~GenerateBilanDlg()
{
}
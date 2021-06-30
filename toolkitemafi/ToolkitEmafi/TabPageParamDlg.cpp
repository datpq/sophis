#include "TabPageParamDlg.h"
#include "Resource\resource.h"
#include "CtxElements.h"
#include "CtxComboBox.h"
#include "CtxTemplates.h"
#include "SqlUtils.h"
#include "SphUtils.h"
#include "SphInc\gui\SphEditList.h" 
#include "StringUtils.h"
#include "Resource\resource.h"

using namespace eff::gui;
using namespace eff::emafi::gui;
using namespace eff::utils;
using namespace std;



void EdlOdParam::Initialize()
{
	fColumnCount = 3;
	fColumns = new SSColumn[fColumnCount];

	int i=0;
	fColumns[i].fColumnName = "Code Paramètre";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 40);

	i++;
	fColumns[i].fColumnName = "Description";
	fColumns[i].fColumnWidth = 400;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);


	i++;
	fColumns[i].fColumnName = "Ordre";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CtxEditShort(this, i);
	
	SetLineCount(0);
	//SaveLineCount(GetLineCount() + 1);
}



void EdlOdParam::LoadData(const char * query)
{
	CtxEditList::LoadData(query);

	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(2, sizeof(EdlParamItem));
	ADD(gabarit, EdlParamItem, code, rdfString);
	ADD(gabarit, EdlParamItem, description, rdfString);
	ADD(gabarit, EdlParamItem, ordre, rdfInteger);

	EdlParamItem * arrItems;
	int count = 0;
	//7.1.3
	//try {
		errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrItems, &count);
		SetLineCount(count);
		for(int i=0; i<count; i++) {
			int col = 0;
			SaveElement(i, col++, arrItems[i].code);
			SaveElement(i, col++, arrItems[i].description);
			SaveElement(i, col++, &arrItems[i].ordre);
		}
		Update();
	//} catch (const sophis::sql::OracleException &ex) {
	//	//GetDialog()->Message(ex.getError().c_str());
	//	throw;
	//}
	delete gabarit;
}



void TabPageParamDlg_LstParam_SelectedIndexChanged(CtxEditList * sender, int lineNumber)
{

	EdlParamItem selectedItem = EdlParamItem();
	int col = 0;
	sender->LoadElement(lineNumber, col++, selectedItem.code);
	sender->LoadElement(lineNumber, col++, selectedItem.description);
	TabPageParamDlg *dialog = (TabPageParamDlg *)sender->GetDialog();

	CSRElement * txtCode = dialog->GetElementByAbsoluteId(IDC_TXT_CODE_COMMENT - ID_ITEM_SHIFT);
    CSRElement * txtDesc = dialog->GetElementByAbsoluteId(IDC_TXT_DESC_COMMENT - ID_ITEM_SHIFT);

	txtCode->SetValue(selectedItem.code);
	txtDesc->SetValue(selectedItem.description);
	
}


void TabPageParamDlg_CmdAdd_OnClick(CtxButton * sender) {
    TabPageParamDlg * dialog = (TabPageParamDlg * ) sender->GetDialog();
    CSRElement * txtCode = dialog->GetElementByAbsoluteId(IDC_TXT_CODE_COMMENT - ID_ITEM_SHIFT);
    CSRElement * txtDesc = dialog->GetElementByAbsoluteId(IDC_TXT_DESC_COMMENT - ID_ITEM_SHIFT);
    CtxEditList * lstParam = (CtxEditList * ) dialog->GetElementByAbsoluteId(IDC_LST_PARAM - ID_ITEM_SHIFT);
    EdlParamItem selectedItem = EdlParamItem();
    txtCode->GetValue( &selectedItem.code);
    txtDesc->GetValue( &selectedItem.description);

    bool check = false;
	//7.1.3
    //try {

        CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(1, sizeof(EdlParamItem));
        ADD(gabarit, EdlParamItem, code, rdfString);

		//7.1.3
        //try {
            char query[SQL_LEN] = {'\0'};
            _snprintf_s(query, sizeof(query), "SELECT CODE FROM EMAFI_PARAMETRAGE WHERE CATEGORIE='%s' ", dialog->categorie);

            EdlParamItem * arrParam;
            int count = 0;

            errorCode err1 = QueryWithNResultsArray(query, gabarit, (void * * ) & arrParam, & count);

            for (int i = 0; i < count; i++) 
			{
                if (strcmp(arrParam[i].code, selectedItem.code) == 0)
                    check = true;
            }

        //} catch (const sophis::sql::OracleException & ex) {
        //    throw;
        //}
        delete gabarit;

        if (selectedItem.code[0] == '\0' || selectedItem.description[0] == '\0') {
			CSRFitDialog::Message(LoadResourceString(MSG_FIELD).c_str());
            txtCode->SetFocus();
            return;
        } else {
            if (check) { // if the code already exists
				CSRFitDialog::Message(LoadResourceString(MSG_CODE).c_str());
                txtCode->SetFocus();
            } else

            {
                SqlUtils::QueryWithoutResultException("\
				insert into EMAFI_PARAMETRAGE (CATEGORIE,CODE,DESCRIPTION,ORDRE) VALUES ('%s','%s','%s',\n\
				(select max(ORDRE) from EMAFI_PARAMETRAGE where CATEGORIE ='%s')+1)",dialog->categorie, selectedItem.code, selectedItem.description,
				dialog->categorie);
				CSRSqlQuery::Commit();
                lstParam->ReloadData();
                lstParam->Selection(0, selectedItem.code);

            }
        }
    //} catch (const CSROracleException & e) {

    //    dialog->Message((std::string("Database error: ") + e.getError()).c_str());
    //}
}

void TabPageParamDlg_CmdUpdate_OnClick(CtxButton * sender)
{
	TabPageParamDlg * dialog = (TabPageParamDlg * ) sender->GetDialog();
	CtxEditList * lstParam = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_PARAM - ID_ITEM_SHIFT);
	CSRElement * txtCode = dialog->GetElementByAbsoluteId(IDC_TXT_CODE_COMMENT - ID_ITEM_SHIFT);
    CSRElement * txtDesc = dialog->GetElementByAbsoluteId(IDC_TXT_DESC_COMMENT - ID_ITEM_SHIFT);
	EdlParamItem selectedItem = EdlParamItem();
	txtCode->GetValue( &selectedItem.code);
    txtDesc->GetValue( &selectedItem.description);

	char query[SQL_LEN] = {'\0'};
	_snprintf_s(query, sizeof(query), "UPDATE EMAFI_PARAMETRAGE SET DESCRIPTION  = '%s' WHERE CODE='%s' AND CATEGORIE ='%s'", selectedItem.description,selectedItem.code,dialog->categorie );

	SqlUtils::QueryWithoutResultException(query);
	CSRSqlQuery::Commit();

	lstParam->ReloadData();
	lstParam->Selection(0);

}

void TabPageParamDlg_CmdDelete_OnClick(CtxButton * sender)
{
	CSRFitDialog *dialog = sender->GetDialog();
	CtxEditList * lstParam = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_PARAM - ID_ITEM_SHIFT);
	EdlParamItem selectedItem = EdlParamItem();
	int nmbreLinesBefore = lstParam->GetLineCount();
	lstParam->GetSelectedValue(0, &selectedItem.code);
	if (dialog->ConfirmDialog(LoadResourceString(MSG_DELETE_ASK).c_str()) == CSRFitDialog::CONFIRM_DIALOG_YES) {
		SqlUtils::QueryWithoutResultException("DELETE FROM EMAFI_PARAMETRAGE WHERE CODE='%s'", selectedItem.code);
		CSRSqlQuery::Commit();

		lstParam->ReloadData();
		lstParam->Selection(0);
		int nmbreLinesAfter = lstParam->GetLineCount();
		if(nmbreLinesAfter<nmbreLinesBefore)
			dialog->Message(LoadResourceString(MSG_SUCCESS_DELETE).c_str());
		else
			dialog->Message(LoadResourceString(MSG_NO_SUCCESS).c_str());
	}
}

void TabPageParamDlg_CmdUp_OnClick(CtxButton * sender)
{
	TabPageParamDlg * dialog = (TabPageParamDlg * ) sender->GetDialog();
	CtxEditList * lstParam = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_PARAM - ID_ITEM_SHIFT);
	long lineNumber;
	long column;
	lstParam->GetSelectedCell(lineNumber, column);
	if(lineNumber != 0)
	{
	
		EdlParamItem selectedItem = EdlParamItem();
		EdlParamItem previousItem = EdlParamItem();
		int col = 0;
	
		lstParam->LoadElement(lineNumber, col, &selectedItem.code);
		lstParam->LoadElement(lineNumber-1,col++, &previousItem.code);
		col++;
		lstParam->LoadElement(lineNumber, col, &selectedItem.ordre);
		lstParam->LoadElement(lineNumber-1,col, &previousItem.ordre);

		char query[SQL_LEN] = {'\0'};
		_snprintf_s(query, sizeof(query), "UPDATE EMAFI_PARAMETRAGE SET ORDRE  = %d WHERE CODE='%s' AND CATEGORIE ='%s'",previousItem.ordre,selectedItem.code,dialog->categorie );
		SqlUtils::QueryWithoutResultException(query);


		_snprintf_s(query, sizeof(query), "UPDATE EMAFI_PARAMETRAGE SET ORDRE  = %d WHERE CODE='%s' AND CATEGORIE ='%s'",selectedItem.ordre,previousItem.code,dialog->categorie );
		SqlUtils::QueryWithoutResultException(query);

		CSRSqlQuery::Commit();
		lstParam->ReloadData();
		lstParam->Select(lineNumber-1);

	}
}

void TabPageParamDlg_CmdDown_OnClick(CtxButton * sender)
{
	TabPageParamDlg * dialog = (TabPageParamDlg * ) sender->GetDialog();
	CtxEditList * lstParam = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_PARAM - ID_ITEM_SHIFT);
	long lineNumber;
	long column;
	lstParam->GetSelectedCell(lineNumber, column);

	if(lineNumber != lstParam->GetLineCount()-1)
	{
	
		EdlParamItem selectedItem = EdlParamItem();
		EdlParamItem nextItem = EdlParamItem();
		int col = 0;
	
		lstParam->LoadElement(lineNumber, col, &selectedItem.code);
		lstParam->LoadElement(lineNumber+1,col++, &nextItem.code);
		col++;
		lstParam->LoadElement(lineNumber, col, &selectedItem.ordre);
		lstParam->LoadElement(lineNumber+1,col, &nextItem.ordre);

		char query[SQL_LEN] = {'\0'};
		_snprintf_s(query, sizeof(query), "UPDATE EMAFI_PARAMETRAGE SET ORDRE  = %d WHERE CODE='%s' AND CATEGORIE ='%s'",nextItem.ordre,selectedItem.code,dialog->categorie );
		SqlUtils::QueryWithoutResultException(query);


		_snprintf_s(query, sizeof(query), "UPDATE EMAFI_PARAMETRAGE SET ORDRE  = %d WHERE CODE='%s' AND CATEGORIE ='%s'",selectedItem.ordre,nextItem.code,dialog->categorie );
		SqlUtils::QueryWithoutResultException(query);

		CSRSqlQuery::Commit();
		lstParam->ReloadData();
		lstParam->Select(lineNumber+1);

	}
}


TabPageParamDlg::TabPageParamDlg(const char* categorie_) : CSRFitDialog()
{
	fResourceId	= IDD_TAB_PARAMETRAGE_OD - ID_DIALOG_SHIFT;
	categorie = categorie_;
	NewElementList(8);

	int nb = 0;

	if (fElementList)
	{
		
		fElementList[nb++] = new CSREditText(this, IDC_TXT_CODE_COMMENT - ID_ITEM_SHIFT, 100);
		fElementList[nb++] = new CSREditText(this, IDC_TXT_DESC_COMMENT - ID_ITEM_SHIFT, 100);			
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Add - ID_ITEM_SHIFT, &TabPageParamDlg_CmdAdd_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Delete - ID_ITEM_SHIFT, &TabPageParamDlg_CmdDelete_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Update - ID_ITEM_SHIFT, &TabPageParamDlg_CmdUpdate_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Down - ID_ITEM_SHIFT, &TabPageParamDlg_CmdDown_OnClick);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_Up - ID_ITEM_SHIFT, &TabPageParamDlg_CmdUp_OnClick);


		CtxEditList * lstParam = new EdlOdParam(this, IDC_LST_PARAM - ID_ITEM_SHIFT,&TabPageParamDlg_LstParam_SelectedIndexChanged);
		fElementList[nb++] = lstParam;
		char query[SQL_LEN] = {'\0'};
		_snprintf_s(query, sizeof(query), "select CODE, DESCRIPTION,ORDRE from EMAFI_PARAMETRAGE WHERE CATEGORIE='%s' ORDER BY ORDRE",categorie);
		lstParam->LoadData(query);
		lstParam->Selection(0);
	}
}
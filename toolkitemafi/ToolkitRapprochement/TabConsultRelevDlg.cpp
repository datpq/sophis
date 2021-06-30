#include "TabConsultRelevDlg.h"
#include "Resource\resource.h"
#include "CtxElements.h"
#include "SqlUtils.h"
#include "CtxComboBox.h"
#include"SqlWrapper.h"
#include "StringUtils.h"

using namespace eff::emafi::gui;
using namespace eff::gui;
using namespace eff::utils;

//**********************



void EdlRelev::Initialize()
{
	fColumnCount = 18;
	fColumns = new SSColumn[fColumnCount];
	int i=0;
	fColumns[i].fColumnName = "Type Enregistrement";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;

	fColumns[i].fColumnName = "Code banque";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Code Operation";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Code guichet";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement =new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Code Devise";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

		i++;

	fColumns[i].fColumnName = "Nbr Decimal";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxStaticLong(this, i); 

		i++;

	fColumns[i].fColumnName = "Numéro de compte";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

		i++;

	fColumns[i].fColumnName = "Code operation interbancaire";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Date operation";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

	i++;
	fColumns[i].fColumnName = "Date valeur";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

	
		i++;
	fColumns[i].fColumnName = "Libelle";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

		i++;

	fColumns[i].fColumnName = "Numero ecrit";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Montant";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSREditDouble(this, i, 2, 0, 10000000000, 0);

		i++;
	fColumns[i].fColumnName = "Refer_1";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);


	i++;
	fColumns[i].fColumnName = "Nom Fichier";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255); 

			i++;
	fColumns[i].fColumnName = "Refer FI";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxStaticLong(this, i);

			i++;
	fColumns[i].fColumnName = "Statut";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

			i++;
	fColumns[i].fColumnName = "Motif Rejet";
	fColumns[i].fColumnWidth = 120;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
}



string EdlRelev::Getquery(const char* compte, long codeDevise, const char * dateD, const char * dateF)
{
		
		const char * SQL_CONSULT = "select R.Type_Enreg,R.Code_Banq, R.Code_ope,  R.Code_Guichet, R.Code_Dev, \n\
			R.Nbr_Decim, R.Num_compte, Code_Oper_interb, R.Date_Operation, R.Date_valeur, libelle, R.Numero_Ecrit, \n\
			R.MONTANT_NUMBER, R.Refer_1, R.Nom_fichier, R.Refer_FI, R.Statut, DECODE(R.STATUT, 'OK', P.DESCRIPTION, NVL(P.DESCRIPTION, 'Untreated')) DESCRITPION \n\
			from EMAFI_RECO_RELEV_BANC R left join  EMAFI_PARAMETRAGE P \n\
			on P.CODE = R.Motif_Rejet and P.CATEGORIE = 'Recon_Motif' where R.code_banq||R.code_guichet||R.num_compte = '%s' \n\
			And Code_dev = devise_to_str(%d) \n\
			And to_date(R.date_operation, 'DD/MM/YY') >= to_date('%s', 'YYYYMMDD') \n\
			And to_date(R.date_operation, 'DD/MM/YY') <= to_date('%s', 'YYYYMMDD') \n\
			order by Type_Enreg ";
		
		string newSql = StrFormat(SQL_CONSULT,compte, codeDevise, dateD,dateF);
		return newSql;
}


void EdlRelev::LoadData(const char * query)
{

	CtxEditList::LoadData(query);
	
	SqlWrapper* report_wrapper = new SqlWrapper("c50,c50,c50,c50,c50,l,c50,c50,c50,c50,c50,c50,d,c50,c50,l,c50,c50",query);
	
	int count = 0;
	count = report_wrapper->GetRowCount();
		
	SetLineCount(count);
		
	for(int i=0; i<count; i++) {

			int col = 0;
			
			SaveElement(i, col++, (void*)(*report_wrapper)[i][0].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][1].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][2].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][3].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][4].value<string>().c_str());
			SaveElement(i, col++, &(*report_wrapper)[i][5].value<long>());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][6].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][7].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][8].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][9].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][10].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][11].value<string>().c_str());
			SaveElement(i, col++, &(*report_wrapper)[i][12].value<double>());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][13].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][14].value<string>().c_str());
			SaveElement(i, col++, &(*report_wrapper)[i][15].value<long>());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][16].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][17].value<string>().c_str());
		}
	Update();
}


//**********************
void TabConsultRelevDlg_CmdConsult_OnClick(CtxButton *e)
{

	TabConsultRelevDlg *dialog = (TabConsultRelevDlg*)e->GetDialog();
		//
	CSREditDate *date = (CSREditDate *)dialog->GetElementByAbsoluteId(IDC_Txt_Date_Debut - ID_ITEM_SHIFT);
	long lDate = 0;
	date->GetValue(&lDate);
	string sDateDebut = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'YYYYMMDD') FROM DUAL", lDate);


	date = (CSREditDate *)dialog->GetElementByAbsoluteId(IDC_Txt_Date_Fin - ID_ITEM_SHIFT);
	lDate = 0;
	date->GetValue(&lDate);
	string sDateFin = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'YYYYMMDD') FROM DUAL", lDate);


	CtxComboBox * cboDevise = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_DEVISE - ID_ITEM_SHIFT);
	
	
	CtxComboBox * cboComptes = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_COMPTES - ID_ITEM_SHIFT);
	
	
	EdlRelev * lstRelev = (EdlRelev *)dialog->GetElementByAbsoluteId(IDC_LST_MVT - ID_ITEM_SHIFT);
	

	string queryStr = lstRelev->Getquery(cboComptes->SelectedText().c_str(), cboDevise->SelectedValue(), sDateDebut.c_str(), sDateFin.c_str());
	lstRelev->LoadData(queryStr.c_str());
	lstRelev->Selection(0);


}

TabConsultRelevDlg::TabConsultRelevDlg() : CSRFitDialog()
{
	fResourceId	=  IDD_TAB_CONSULT_RELEVES- ID_DIALOG_SHIFT;

	NewElementList(6);

	int nb = 0;

	if (fElementList)
	{
	
		fElementList[nb++] =  new CtxComboBox(this, IDC_CBO_COMPTES - ID_ITEM_SHIFT,
			"select ROWNUM, compte from (select distinct CODE_BANQ||CODE_GUICHET||NUM_COMPTE compte from EMAFI_RECO_RELEV_BANC) \n\
			order by compte");


		fElementList[nb++] =  new CtxComboBox(this, IDC_CBO_DEVISE - ID_ITEM_SHIFT,
			"select De.code, devise_to_str (De.code) from devisev2 De order by devise_to_str (De.code)");
		fElementList[nb++] =  new CSREditDate(this, IDC_Txt_Date_Debut - ID_ITEM_SHIFT, CSRDay::GetSystemDate());
		fElementList[nb++] = new CSREditDate(this, IDC_Txt_Date_Fin - ID_ITEM_SHIFT, CSRDay::GetSystemDate());
		
		fElementList[nb++] = new CtxButton(this, IDC_CMD_Consult - ID_ITEM_SHIFT, &TabConsultRelevDlg_CmdConsult_OnClick);


		const char * SQL_CONSULT ="select R.Type_Enreg,R.Code_Banq, R.Code_ope,  R.Code_Guichet, R.Code_Dev, \n\
			R.Nbr_Decim, R.Num_compte, Code_Oper_interb, R.Date_Operation, R.Date_valeur, libelle, R.Numero_Ecrit, \n\
			R.MONTANT_NUMBER, R.Refer_1, R.Nom_fichier, R.Refer_FI, R.Statut, DECODE(R.STATUT, 'OK', P.DESCRIPTION, NVL(P.DESCRIPTION, 'Untreated')) DESCRITPION \n\
			from EMAFI_RECO_RELEV_BANC R left join  EMAFI_PARAMETRAGE P \n\
			on P.CODE = R.Motif_Rejet and P.CATEGORIE = 'Recon_Motif' order by \n\
			Nom_fichier, CODE_BANQ||CODE_GUICHET||NUM_COMPTE  ,Code_Dev, Type_Enreg,Date_Operation ";
		
		EdlRelev * lstMvt = new EdlRelev(this, IDC_LST_MVT - ID_ITEM_SHIFT);
		lstMvt->LoadData(SQL_CONSULT);
		lstMvt->Selection(0);

		fElementList[nb++] = lstMvt;

	}
}

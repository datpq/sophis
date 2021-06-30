#include "TabConsultMouvementDlg.h"
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



void EdlMvt::Initialize()
{
	fColumnCount = 13;
	fColumns = new SSColumn[fColumnCount];
	int i=0;
	fColumns[i].fColumnName = "Type Mouvement";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;

	fColumns[i].fColumnName = "ID";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxStaticLong(this, i);

	i++;
	fColumns[i].fColumnName = "Compte";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "External_account";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement =new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Devise";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Date opération";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

	i++;
	fColumns[i].fColumnName = "Date valeur";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

	i++;
	fColumns[i].fColumnName = "Montant";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSREditDouble(this,i,2,0,10000000000,0); 

		i++;
	fColumns[i].fColumnName = "Code opération";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

		i++;
	fColumns[i].fColumnName = "Code interbancaire";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

		i++;
	fColumns[i].fColumnName = "Libellé";
	fColumns[i].fColumnWidth = 120;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Ref externe";
	fColumns[i].fColumnWidth = 80;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSRStaticText(this, i, 255); 

	i++;
	fColumns[i].fColumnName = "Nom Fichier";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSRStaticText(this, i, 255); // double
}



string EdlMvt::Getquery(const char* compte, long codeDevise, const char * date)
{
		const char * SQL_CONSULT = 
			"select  type_en, id,account,Tkt_External_account,cur, dateOp,date_val, montant, Code_ope,Code_Oper_interb, \n\
			Libelle, Refer_1, Nom_fichier from( \n\
			select temp.TYPE_ENREG, Decode(temp.TYPE_ENREG, '01', 'Solde début', '04', 'Mouvements', '07', 'Solde fin', 'Autres') type_en,\n\
			B.id, B.Account, B.Tkt_External_account, devise_to_str(B.Currency) cur,\n\
			(select to_date(temp.DATE_OPERATION,'DD-MM-YY') from dual) dateOp, (select to_date(temp.DATE_VALEUR,'DD-MM-YY') from dual)date_val, \n\
			Decode(temp.type_enreg, '01', B.trade_opening_balance,\n\
			'07', B.trade_closing_balance, 0) montant, temp.Code_ope, temp.Code_Oper_interb,\n\
			temp.Libelle, temp.Refer_1, temp.Nom_fichier \n\
			FROM  EMAFI_RECO_RELEV_BANC temp, RECON_EXTERNAL_BALANCES B \n\
		  where B.ID = temp.Refer_FI \n\
		And B.tkt_external_account = '%s' \n\
		And B.currency = %d  And B.balances_date =  TO_DATE('%s', 'YYYYMMDD') \n\
		UNION ALL \n\
			select temp.TYPE_ENREG, Decode(temp.TYPE_ENREG, '01', 'Solde début', '04', 'Mouvements', '07', 'Solde fin', 'Autres') type_en,\n\
			Tr.id, Tr.Account, Tr.Tkt_External_account, devise_to_str(Tr.Currency), \n\
			(select to_date(temp.DATE_OPERATION,'DD-MM-YY') from dual)dateOp, (select to_date(temp.DATE_VALEUR,'DD-MM-YY') from dual)date_val, \n\
			Tr.AMOUNT * -1, temp.Code_ope, temp.Code_Oper_interb,\n\
			temp.Libelle, temp.Refer_1, temp.Nom_fichier \n\
			FROM  EMAFI_RECO_RELEV_BANC temp, RECON_EXTERNAL_TRADES Tr \n\
		where Tr.ID = temp.Refer_FI \n\
		And Tr.tkt_external_account = '%s' \n\
		And Tr.currency = %d And Tr.trade_date = TO_DATE('%s', 'YYYYMMDD') )\n\
			order by TYPE_ENREG ";
		
		string newSql = StrFormat(SQL_CONSULT, compte, codeDevise, date, compte, codeDevise, date);
		return newSql;
}


void EdlMvt::LoadData(const char * query)
{
	CtxEditList::LoadData(query);
	
	SqlWrapper* report_wrapper = new SqlWrapper("c13,l,c20,c50,c10,c10,c10,d,c6,c20,c50,c20,c50",query);
	
	int count = 0;
	count = report_wrapper->GetRowCount();
		
	SetLineCount(count);
		
	for(int i=0; i<count; i++) {

			int col = 0;
			
			SaveElement(i, col++, (void*)(*report_wrapper)[i][0].value<string>().c_str());
			SaveElement(i, col++, &(*report_wrapper)[i][1].value<long>());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][2].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][3].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][4].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][5].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][6].value<string>().c_str());
			SaveElement(i, col++, &(*report_wrapper)[i][7].value<double>());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][8].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][9].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][10].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][11].value<string>().c_str());
			SaveElement(i, col++, (void*)(*report_wrapper)[i][12].value<string>().c_str());


		}
	Update();

}



//**********************
void TabConsultMouvementDlg_CmdConsult_OnClick(CtxButton *e)
{

	TabConsultMouvementDlg *dialog = (TabConsultMouvementDlg*)e->GetDialog();
		//
	CSREditDate *dateConsultation = (CSREditDate *)dialog->GetElementByAbsoluteId(IDC_Txt_Date - ID_ITEM_SHIFT);
	long lDate = 0;
	dateConsultation->GetValue(&lDate);
	string sDate = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'YYYYMMDD') FROM DUAL", lDate);

	CtxComboBox * cboDevise = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_DEVISE - ID_ITEM_SHIFT);
	
	
	CtxComboBox * cboComptes = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_COMPTES - ID_ITEM_SHIFT);
	
	
	EdlMvt * lstMvt = (EdlMvt *)dialog->GetElementByAbsoluteId(IDC_LST_MVT - ID_ITEM_SHIFT);
	

	string queryStr = lstMvt->Getquery(cboComptes->SelectedText().c_str(),cboDevise->SelectedValue(),sDate.c_str());
	lstMvt->LoadData(queryStr.c_str());
	lstMvt->Selection(0);


}

TabConsultMouvementDlg::TabConsultMouvementDlg() : CSRFitDialog()
{
	fResourceId	=  IDD_TAB_CONSULT_MOUVEMENT - ID_DIALOG_SHIFT;

	NewElementList(5);

	int nb = 0;

	if (fElementList)
	{
	
		fElementList[nb++] =  new CtxComboBox(this, IDC_CBO_COMPTES - ID_ITEM_SHIFT,
			" select ROWNUM, tkt_external_account from (select distinct  tkt_external_account \n\
			from RECON_EXTERNAL_BALANCES B  WHERE tkt_external_account is not null UNION select distinct  \n\
			tkt_external_account from RECON_EXTERNAL_trades Tr  WHERE tkt_external_account is not null) order by tkt_external_account");
			
		fElementList[nb++] =  new CtxComboBox(this, IDC_CBO_DEVISE - ID_ITEM_SHIFT,"select De.code, devise_to_str (De.code) from devisev2 De order by devise_to_str (De.code)"/*, "SELECT IDENT, NAME FROM TIERS ORDER BY NAME"*/);
		fElementList[nb++] =  new CSREditDate(this, IDC_Txt_Date - ID_ITEM_SHIFT, CSRDay::GetSystemDate());
		
		fElementList[nb++] = new CtxButton(this, IDC_CMD_Consult - ID_ITEM_SHIFT, &TabConsultMouvementDlg_CmdConsult_OnClick);


		const char * SQL_CONSULT = 
			"select  type_en, id,account,Tkt_External_account,cur, dateOp,date_val, montant, Code_ope,Code_Oper_interb,  \n\
			Libelle, Refer_1, Nom_fichier from( \n\
			select temp.code_dev, temp.TYPE_ENREG, Decode(temp.TYPE_ENREG, '01', 'Solde début', '04', 'Mouvements', '07', 'Solde fin', 'Autres') type_en, \n\
			B.id, B.Account, B.Tkt_External_account, devise_to_str(B.Currency) cur, \n\
			(select to_date(temp.DATE_OPERATION, 'DD-MM-YY') from dual) dateOp, (select to_date(temp.DATE_VALEUR, 'DD-MM-YY') from dual)date_val, \n\
			Decode(temp.type_enreg, '01', B.trade_opening_balance, \n\
			'07', B.trade_closing_balance, 0) montant, temp.Code_ope, temp.Code_Oper_interb, \n\
			temp.Libelle, temp.Refer_1, temp.Nom_fichier \n\
			FROM  EMAFI_RECO_RELEV_BANC temp, RECON_EXTERNAL_BALANCES B \n\
		where B.ID = temp.Refer_FI  \n\
		UNION ALL  \n\
			select temp.code_dev, temp.TYPE_ENREG, Decode(temp.TYPE_ENREG, '01', 'Solde début', '04', 'Mouvements', '07', 'Solde fin', 'Autres') type_en,  \n\
			Tr.id, Tr.Account, Tr.Tkt_External_account, devise_to_str(Tr.Currency),  \n\
			(select to_date(temp.DATE_OPERATION, 'DD-MM-YY') from dual)dateOp, (select to_date(temp.DATE_VALEUR, 'DD-MM-YY') from dual)date_val,  \n\
			Tr.AMOUNT * -1, temp.Code_ope, temp.Code_Oper_interb,  \n\
			temp.Libelle, temp.Refer_1, temp.Nom_fichier  \n\
			FROM  EMAFI_RECO_RELEV_BANC temp, RECON_EXTERNAL_TRADES Tr  \n\
		where Tr.ID = temp.Refer_FI)  \n\
			order by Nom_fichier, TKT_EXTERNAL_ACCOUNT, code_dev, TYPE_ENREG, dateOp";
	

		EdlMvt * lstMvt = new EdlMvt(this, IDC_LST_MVT - ID_ITEM_SHIFT);
		lstMvt->LoadData(SQL_CONSULT);
		lstMvt->Selection(0);

		fElementList[nb++] = lstMvt;
	}
}

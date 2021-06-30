#include "TabImportDlg.h"
#include "Resource\resource.h"
#include "CtxElements.h"
#include "SqlUtils.h"
#include "SphInc/gui/SphButton.h"
//***
#include <windows.h> 
#include <iostream>
#include <fstream>
#include <string>
#include "Config.h"
#include "SqlWrapper.h"
#include "SphInc/gui/SphElement.h"
#include "StringUtils.h"
#include "CtxComboBox.h"



//***
using namespace eff::emafi::gui;
using namespace eff::gui;
using namespace eff::utils;



vector<string> decouper(string chaine, char c){
	int size = chaine.size();
	int r = 0;
	vector<string> v;
	for (int i = 0; i<size; i++){
		if (chaine[i] == c){
			v.push_back(chaine.substr(r, i - r));
			r = i + 1;
		}
	}
	v.push_back(chaine.substr(r, size - r));
	return v;
}



void TabImportDlg_CmdImport_OnClick(CtxButton *sender)
{
	TabImportDlg *dialog = (TabImportDlg*)sender->GetDialog();
	vector<string> selectedFiles = CtxGuiUtils::GetOpenFileMulti("Select files to import", "Text files (*.txt)\0*.txt\0All files (*.*)\0*.*\0", sender);

	bool check_nom_fichier = true;
	for(int i=0; i<selectedFiles.size();i++)
	{
		if(selectedFiles.size() == 1 || i>0)
		{
				vector<string> words = decouper(selectedFiles[i], '\\');
				vector<string> fileName = decouper(words[words.size() - 1], '.');

				const char * SQL_CONSULT = "select distinct NOM_FICHIER FROM EMAFI_RECO_RELEV_BANC where NOM_FICHIER= '%s'";
					
				string newSql = StrFormat(SQL_CONSULT,words[words.size() - 1].c_str());
				
				SqlWrapper* report_wrapper = new SqlWrapper("c50", newSql.c_str());

				int count = 0;
				count = report_wrapper->GetRowCount();

				if (count==0)
				{
					string backupDir = Config::getSettingStr(TOOLKIT_SECTION, "RapprochementBackupDirectory", "RapprochementBackup");
					string fullPath = Config::getInstance()->getDllDirectory()+"\\" + backupDir + "\\" + fileName[fileName.size() - 2] + ".bkp";
					
					CopyFile(selectedFiles[i].c_str(), fullPath.c_str(), TRUE); //#include <windows.h> 

					ifstream fichier(selectedFiles[i].c_str(), ios::in);

					if (fichier)  // si l'ouverture a réussi
					{
						string ligne;
						int count = 0;
						while (getline(fichier, ligne))
						{
							count++;
							try {
								SqlUtils::QueryWithoutResultException("INSERT INTO EMAFI_RECO_RELEV_BANC(NOM_FICHIER,ORIGINAL_DATA,LINE_NUMBER) VALUES('%s','%s',%d)", words[words.size() - 1].c_str(), ligne.c_str(), count);
								CSRSqlQuery::Commit();
							}
							catch (const CSROracleException &e) {
								ERROR("Database error code = %d, reason = %s", e.GetErrorCode(), e.GetReason().c_str());
							}
							
						}
						fichier.close();  // on ferme le fichier
					}
					else  // sinon
					{
						dialog->Message("Impossible d'ouvrir le fichier !");
						check_nom_fichier = false;
					}	
				}
				else
				{
					check_nom_fichier = false;
					dialog->Message("Fichier Déjà intégré");
				}	
		}
	}

	if (check_nom_fichier == true && selectedFiles.size()!=0)
	{
	
		try {
			SqlUtils::QueryWithoutResultException("BEGIN EMAFI_RAPPR_KB.Rapprochement_Procedures; END;"); 
			CSRSqlQuery::Commit();
		
			CtxComboBox * cboTab2 = (CtxComboBox *)dialog->tab2->GetElementByAbsoluteId(IDC_CBO_COMPTES - ID_ITEM_SHIFT);

			cboTab2->LoadData(" select ROWNUM, tkt_external_account from (select distinct  tkt_external_account \n\
			from RECON_EXTERNAL_BALANCES B  WHERE tkt_external_account is not null UNION select distinct  \n\
			tkt_external_account from RECON_EXTERNAL_trades Tr  WHERE tkt_external_account is not null) order by tkt_external_account",
			1, false);


			EdlMvt * lstMvt = (EdlMvt *)dialog->tab2->GetElementByAbsoluteId(IDC_LST_MVT - ID_ITEM_SHIFT);
			lstMvt->LoadData("select  type_en, id,account,Tkt_External_account,cur, dateOp,date_val, montant, Code_ope,Code_Oper_interb,  \n\
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
				order by Nom_fichier, TKT_EXTERNAL_ACCOUNT, code_dev, TYPE_ENREG, dateOp");
			lstMvt->Selection(0);


			CtxComboBox * cboTab3 = (CtxComboBox *)dialog->tab3->GetElementByAbsoluteId(IDC_CBO_COMPTES - ID_ITEM_SHIFT);

			cboTab3->LoadData("select ROWNUM, compte from (select distinct CODE_BANQ||CODE_GUICHET||NUM_COMPTE compte from EMAFI_RECO_RELEV_BANC) \n\
				order by compte",1, false);

			EdlRelev * lstRlv = (EdlRelev *)dialog->tab3->GetElementByAbsoluteId(IDC_LST_MVT - ID_ITEM_SHIFT);
			lstRlv->LoadData("select R.Type_Enreg,R.Code_Banq, R.Code_ope,  R.Code_Guichet, R.Code_Dev, \n\
				R.Nbr_Decim, R.Num_compte, Code_Oper_interb, R.Date_Operation, R.Date_valeur, libelle, R.Numero_Ecrit, \n\
				R.MONTANT_NUMBER, R.Refer_1, R.Nom_fichier, R.Refer_FI, R.Statut, DECODE(R.STATUT, 'OK', P.DESCRIPTION, NVL(P.DESCRIPTION, 'Untreated')) DESCRITPION \n\
				from EMAFI_RECO_RELEV_BANC R left join  EMAFI_PARAMETRAGE P \n\
				on P.CODE = R.Motif_Rejet and P.CATEGORIE = 'Recon_Motif' order by \n\
				Nom_fichier, CODE_BANQ||CODE_GUICHET||NUM_COMPTE  ,Code_Dev, Type_Enreg,Date_Operation ");

			lstRlv->Selection(0);

			dialog->Message("Données Intégrées avec succès !");

		}
		catch (const CSROracleException &e) {
			ERROR("Database error code = %d, reason = %s", e.GetErrorCode(), e.GetReason().c_str());
		}
	}
}



TabImportDlg::TabImportDlg(TabConsultMouvementDlg* tab_2, TabConsultRelevDlg* tab_3) : CSRFitDialog()
{
	fResourceId	=  IDD_TAB_IMPORT - ID_DIALOG_SHIFT;
	this->tab2 = tab_2;
	this->tab3 = tab_3;
	NewElementList(1);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]= new CtxButton(this, IDC_CMD_IMPORT - ID_ITEM_SHIFT, &TabImportDlg_CmdImport_OnClick);

	}
}

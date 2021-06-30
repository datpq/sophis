#include "EdlOd.h"
#include "CtxElements.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/gui/SphDialog.h"
#include "SqlUtils.h"

using namespace eff::emafi::gui;

void EdlOd::GetLineColor(int lineNumber, eTextColorType &color) const
{
	short odStatus = 0;
	LoadElement(lineNumber, 15, &odStatus);
	if (odStatus == -1) {
		color = tcRed;
	}
}
string EdlOd::Getquery(bool valid, long ptf,const char * startDate,const char * endDate,const char * devise,const char * journal, const char * piece, long statutId )
{
		if(!valid)
			statutId = 0;
		
		if(!strcmp(journal,""))
			journal = "null";
		
		if(!strcmp(piece,""))
			piece = "null";
		
		char  query[SQL_LEN] = {'\0'};
		const char * SQL_CONSULT = "SELECT OD_NUM, ID_POSTING, ENTITY_NAME, PTF_NAME,TIERS_NAME,ACCOUNT_NUMBER,AMOUNT,SENS,POSTING_DATE, \n\
		GENERATION_DATE,CURRENCY,JOURNAL,PIECE,COMMENTAIRE_CODE, COMMENTAIRE_DESC, OD_STATUS,(SELECT NAME FROM EMAFI_ODSTATUS A WHERE A.ID_STATUS= B.OD_STATUS ),STATUS,(select NAME FROM RISKUSERS WHERE IDENT = OPERATEUR) FROM EMAFI_CRE B \n\
		WHERE PTF_ID=%ld AND (POSTING_DATE BETWEEN TO_DATE('%s', 'YYYYMMDD') AND TO_DATE('%s', 'YYYYMMDD')) AND CURRENCY = '%s'\n\
		AND POSTING_TYPE=(select ID FROM ACCOUNT_POSTING_TYPES WHERE NAME='OD' ) AND(JOURNAL = '%s' or '%s' = 'null' ) AND (PIECE = '%s' or '%s' = 'null') and (OD_STATUS  = %d or %d = 0)  ORDER BY OD_NUM,ID_POSTING ";
		
		_snprintf_s(query, sizeof(query), SQL_CONSULT,ptf,startDate,endDate,devise,journal,journal,piece, piece,statutId,statutId);

		
		return string(query);
}


void EdlOd::Initialize()
{
	fColumnCount = 19;
	fColumns = new SSColumn[fColumnCount];
	int i = 0;
	fColumns[i].fColumnName = "NUM OD";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CtxEditShort(this, i);

	i++;
	fColumns[i].fColumnName = "ID Posting";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement =new CSREditDouble(this,i,0,0,10000000000,0); 

	i++;
	fColumns[i].fColumnName = "Entity";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Portefeuille";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
	i++;
	fColumns[i].fColumnName = "Tiers";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Compte";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Montant";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSREditDouble(this,i,2,0,10000000000,0.00); 

	i++;
		fColumns[i].fColumnName = "Sens";
	fColumns[i].fColumnWidth = 30;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Posting Date";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Generation Date";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
	
	i++;
	fColumns[i].fColumnName = "Devise";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Journal";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
	i++;
		fColumns[i].fColumnName = "Piece";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
	
	i++;
	 fColumns[i].fColumnName = "Code Commentaire";
    fColumns[i].fColumnWidth = 50;
    fColumns[i].fAlignmentType = aLeft;
    fColumns[i].fElement = new CSRStaticText(this, i, 255);
 
        
	i++;
	fColumns[i].fColumnName = "Commentaire";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "OD_Status";
	fColumns[i].fColumnWidth = 0;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxEditShort(this, i);
	
	i++;
	fColumns[i].fColumnName = "Status";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Status_ID";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxEditShort(this, i);
	
		i++;
	fColumns[i].fColumnName = "Operateur";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
}

void EdlOd::LoadData(const char * query)
{
	CtxEditList::LoadData(query);
	  
				
	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(19, sizeof(EdlOdItem));
	ADD(gabarit, EdlOdItem, num_od, rdfInteger);
	ADD(gabarit, EdlOdItem, id_posting, rdfFloat);
	ADD(gabarit, EdlOdItem, entity_name, rdfString);
	ADD(gabarit, EdlOdItem, portfolio, rdfString);

	ADD(gabarit, EdlOdItem, tiers_name, rdfString);
	ADD(gabarit, EdlOdItem, account_number, rdfString);
	ADD(gabarit, EdlOdItem, amount, rdfFloat);

		ADD(gabarit, EdlOdItem, sens, rdfString);
	ADD(gabarit, EdlOdItem, posting_date, rdfString);
	ADD(gabarit, EdlOdItem, generation_date, rdfString);
	ADD(gabarit, EdlOdItem, devise, rdfString);
	ADD(gabarit, EdlOdItem, journal, rdfString);

		ADD(gabarit, EdlOdItem, piece, rdfString);
		ADD(gabarit, EdlOdItem, code_commentaire, rdfString);
	ADD(gabarit, EdlOdItem, commentaire, rdfString);
	ADD(gabarit, EdlOdItem, od_status, rdfInteger);
	ADD(gabarit, EdlOdItem, status, rdfString);
	ADD(gabarit, EdlOdItem, status_id, rdfInteger);
	ADD(gabarit, EdlOdItem, operateur, rdfString);

	EdlOdItem * arrItems;
	int count = 0;
	//7.1.3
	//try {
		errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrItems, &count);
		SetLineCount(count);
		for(int i=0; i<count; i++) {
			int col = 0;
			SaveElement(i, col++, &arrItems[i].num_od);
			SaveElement(i, col++, &arrItems[i].id_posting);
			SaveElement(i, col++, arrItems[i].entity_name);
			SaveElement(i, col++, arrItems[i].portfolio);

			SaveElement(i, col++, arrItems[i].tiers_name);
			SaveElement(i, col++, arrItems[i].account_number);
			SaveElement(i, col++, &arrItems[i].amount);

			SaveElement(i, col++, arrItems[i].sens);
			SaveElement(i, col++, arrItems[i].posting_date);
			SaveElement(i, col++, arrItems[i].generation_date);
			SaveElement(i, col++, arrItems[i].devise);
			SaveElement(i, col++, arrItems[i].journal);

			SaveElement(i, col++, arrItems[i].piece);
			 SaveElement(i, col++, arrItems[i].code_commentaire);
			SaveElement(i, col++, arrItems[i].commentaire);
			SaveElement(i, col++, &arrItems[i].od_status);
			SaveElement(i, col++, &arrItems[i].status);
			SaveElement(i, col++, &arrItems[i].status_id);
			SaveElement(i, col++, arrItems[i].operateur);
		}
		Update();
	//} catch (const sophis::sql::OracleException &ex) {
	//	//GetDialog()->Message(ex.getError().c_str());
	//	throw;
	//}
	delete gabarit;
}

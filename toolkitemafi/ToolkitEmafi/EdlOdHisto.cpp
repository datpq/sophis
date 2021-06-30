#include "EdlOdHisto.h"
#include "CtxElements.h"
#include "SqlUtils.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/gui/SphDialog.h"

using namespace eff::emafi::gui;
void EdlOdHisto::GetLineColor(int lineNumber, eTextColorType &color) const
{
	short odStatus = 0;
	LoadElement(lineNumber, 18, &odStatus);
	if (odStatus == -1) {
		color = tcRed;
	}
}
void EdlOdHisto::Initialize()
{
	fColumnCount = 20;
	fColumns = new SSColumn[fColumnCount];
	int i = 0;
	fColumns[i].fColumnName = "ID";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CtxEditShort(this, i);

	i++;

	fColumns[i].fColumnName = "User Name";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i,255);
	i++;

	fColumns[i].fColumnName = "Version";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CtxEditShort(this, i);
	i++;
	fColumns[i].fColumnName = "Date de modification";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i,255);
	i++;


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
	fColumns[i].fColumnName = "Commentaire";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Status";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxEditShort(this, i);
	
		i++;
	fColumns[i].fColumnName = "OD Status";
	fColumns[i].fColumnWidth = 0;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxEditShort(this, i);

		i++;
	fColumns[i].fColumnName = "Operateur";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
}

void EdlOdHisto::LoadData(const char * query)
{
	CtxEditList::LoadData(query);

	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(20, sizeof(EdlOdHistoItem));

	ADD(gabarit, EdlOdHistoItem, id, rdfInteger);
	ADD(gabarit, EdlOdHistoItem, user_name, rdfString);
	ADD(gabarit, EdlOdHistoItem, version, rdfInteger);
	ADD(gabarit, EdlOdHistoItem, date_modif, rdfString);


	ADD(gabarit, EdlOdHistoItem, num_od, rdfInteger);
	ADD(gabarit, EdlOdHistoItem, id_posting, rdfFloat);
	ADD(gabarit, EdlOdHistoItem, entity_name, rdfString);
	ADD(gabarit, EdlOdHistoItem, portfolio, rdfString);

		ADD(gabarit, EdlOdHistoItem, tiers_name, rdfString);
	ADD(gabarit, EdlOdHistoItem, account_number, rdfString);
	ADD(gabarit, EdlOdHistoItem, amount, rdfFloat);

		ADD(gabarit, EdlOdHistoItem, sens, rdfString);
	ADD(gabarit, EdlOdHistoItem, posting_date, rdfString);
	ADD(gabarit, EdlOdHistoItem, generation_date, rdfString);
	ADD(gabarit, EdlOdHistoItem, journal, rdfString);

		ADD(gabarit, EdlOdHistoItem, piece, rdfString);
	ADD(gabarit, EdlOdHistoItem, commentaire, rdfString);
	ADD(gabarit, EdlOdHistoItem, status, rdfInteger);
	ADD(gabarit, EdlOdHistoItem, od_status, rdfInteger);
	ADD(gabarit, EdlOdHistoItem, operateur, rdfString);

	EdlOdHistoItem * arrItems;
	int count = 0;
	//7.1.3
	//try {
		errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrItems, &count);
		SetLineCount(count);
		for(int i=0; i<count; i++) {
			int col = 0;

			SaveElement(i, col++, &arrItems[i].id);
			SaveElement(i, col++, &arrItems[i].user_name);
			SaveElement(i, col++, &arrItems[i].version);
			SaveElement(i, col++, arrItems[i].date_modif);


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
			SaveElement(i, col++, arrItems[i].journal);

			SaveElement(i, col++, arrItems[i].piece);
			SaveElement(i, col++, arrItems[i].commentaire);
			SaveElement(i, col++, &arrItems[i].status);
			SaveElement(i, col++, &arrItems[i].od_status);
			SaveElement(i, col++, arrItems[i].operateur);
		}
		Update();
	//} catch (const sophis::sql::OracleException &ex) {
	//	//GetDialog()->Message(ex.getError().c_str());
	//	throw;
	//}
	delete gabarit;
}
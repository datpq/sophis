#include "EdlFund.h"
#include "CtxElements.h"
#include "SqlUtils.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/gui/SphDialog.h"

using namespace eff::emafi::gui;

void EdlFund::Initialize()
{
	fColumnCount = 2;
	fColumns = new SSColumn[fColumnCount];
	int i=0;
	fColumns[i].fColumnName = "Sicovam";
	fColumns[i].fColumnWidth = 150;
	fColumns[i].fAlignmentType = aLeft;
	//fColumns[i].fElement =  new CtxEditShort(this, i);//new CtxStaticDouble(this, i, 0);
	fColumns[i].fElement =  new CSREditDouble(this,i,0,0,10000000000,0); //new CtxStaticDouble(this, i, 0);

	i++;
	fColumns[i].fColumnName = "Fonds";
	fColumns[i].fColumnWidth = 390;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i,50);
}

void EdlFund::LoadData(const char * query)
{
	CtxEditList::LoadData(query);

	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(2, sizeof(EdlFundItem));
	ADD(gabarit, EdlFundItem, sicovam, rdfFloat);
	ADD(gabarit,EdlFundItem,libelle,rdfString);

	EdlFundItem * arrItems;
	int count = 0;
	//7.1.3
	//try {
		errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrItems, &count);
		SetLineCount(count);
		for(int i=0; i<count; i++) {
			int col = 0;
			SaveElement(i, col++, &arrItems[i].sicovam);
			SaveElement(i,col++,arrItems[i].libelle);
		}
		Update();
	//} catch (const sophis::sql::OracleException &ex) {
	//	//GetDialog()->Message(ex.getError().c_str());
	//	throw;
	//}
	delete gabarit;
}

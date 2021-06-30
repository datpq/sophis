#include "EdlReport.h"
#include "CtxElements.h"
#include "SqlUtils.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/gui/SphDialog.h"

using namespace eff::emafi::gui;

void EdlReport::Initialize()
{
	fColumnCount = 2;
	fColumns = new SSColumn[fColumnCount];
	int i=0;
	fColumns[i].fColumnName = "Nom du rapport";
	fColumns[i].fColumnWidth = 450;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);
	i++;

	fColumns[i].fColumnWidth = 0;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i,5);
}

void EdlReport::LoadData(const char * query)
{
	CtxEditList::LoadData(query);

	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(2, sizeof(EdlReportItem));
	ADD(gabarit, EdlReportItem, report_name, rdfString);
	ADD(gabarit,EdlReportItem,report_type,rdfString);

	EdlReportItem * arrItems;
	int count = 0;
	//7.1.3
	//try {
		errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrItems, &count);
		SetLineCount(count);
		for(int i=0; i<count; i++) {
			int col = 0;
			SaveElement(i, col++, arrItems[i].report_name);
			SaveElement(i,col++,arrItems[i].report_type);
		}
		Update();
	//} catch (const sophis::sql::OracleException &ex) {
	//	//GetDialog()->Message(ex.getError().c_str());
	//	throw;
	//}
	delete gabarit;
}

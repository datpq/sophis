#include "EdlRubric.h"
#include "CtxElements.h"
#include "SqlUtils.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/gui/SphDialog.h"

using namespace eff::emafi::gui;

void EdlRubric::Initialize()
{
	fColumnCount = 3;
	fColumns = new SSColumn[fColumnCount];
	int i = 0;

	fColumns[i].fColumnName = "Id";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement  = new CtxStaticLong(this, i);

	i++;
	fColumns[i].fColumnName = "Rubrique";
	fColumns[i].fColumnWidth = 450;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Type";
	fColumns[i].fColumnWidth = 180;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

}

void EdlRubric::LoadData(const char * query)
{
	CtxEditList::LoadData(query);

	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(3, sizeof(EdlRubricItem));
	ADD(gabarit, EdlRubricItem, id, rdfInteger);
	ADD(gabarit, EdlRubricItem, rubric, rdfString);
	ADD(gabarit, EdlRubricItem, rubric_type, rdfString);

	EdlRubricItem * arrItems;
	int count = 0;
	//7.1.3
	//try {
		errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrItems, &count);
		SetLineCount(count);
		for(int i=0; i<count; i++) {
			int col = 0;
			SaveElement(i, col++, &arrItems[i].id);
			SaveElement(i, col++, arrItems[i].rubric);
			SaveElement(i, col++, arrItems[i].rubric_type);
		}
		Update();
	//} catch (const sophis::sql::OracleException &ex) {
	//	//GetDialog()->Message(ex.getError().c_str());
	//	throw;
	//}
	delete gabarit;
}
#include "EdlLabel.h"
#include "CtxElements.h"
#include "SqlUtils.h"
#include "StringUtils.h"
#include "SqlWrapper.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/gui/SphDialog.h"

using namespace eff::emafi::gui;
using namespace eff::utils;

void EdlLabel::Initialize()
{
	fColumnCount = 8;
	fColumns = new SSColumn[fColumnCount];
	int i=0;
	fColumns[i].fColumnName = "IdRubric";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxStaticDouble(this, i, 0);

	i++;
	fColumns[i].fColumnName = "Rubrique";
	fColumns[i].fColumnWidth = 280;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "IdLabel";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxStaticDouble(this, i, 0); 

	i++;
	fColumns[i].fColumnName = "Libellés";
	fColumns[i].fColumnWidth = 270;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "No compte";
	fColumns[i].fColumnWidth = 70;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

	i++;
	fColumns[i].fColumnName = "Description de compte";
	fColumns[i].fColumnWidth = 150;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

	i++;
	fColumns[i].fColumnName = "IdParent";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CtxStaticDouble(this, i, 0); 

	i++;
	fColumns[i].fColumnName = "Active";
	fColumns[i].fColumnWidth = 50;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement = new CSRStaticText(this, i, 2);
}

void EdlLabel::LoadData(const char * query)
{
	CtxEditList::LoadData(query);

	try {
		SqlWrapper * sqlWrapper = new SqlWrapper("d,c255,d,c255,c50,c255,d,c1", query);
		int count = sqlWrapper->GetRowCount();

		SetLineCount(count);
		for(int i=0; i<count; i++) {
			int col = 0;
			SaveElement(i, col++, &(*sqlWrapper)[i][0].value<double>());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][1].value<string>().c_str());
			SaveElement(i, col++, &(*sqlWrapper)[i][2].value<double>());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][3].value<string>().c_str());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][4].value<string>().c_str());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][5].value<string>().c_str());
			SaveElement(i, col++, &(*sqlWrapper)[i][6].value<double>());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][7].value<string>().c_str());
		}
		Update();
	} catch (const sophis::sql::CSROracleException &ex) {
		GetDialog()->Message(ex.getError().c_str());
		throw;
	}
}
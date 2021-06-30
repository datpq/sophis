#include "EdlAccount.h"
#include "CtxElements.h"
#include "SqlUtils.h"
#include "SqlWrapper.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/gui/SphDialog.h"

using namespace eff::emafi::gui;
using namespace eff::utils;

void EdlAccount::Initialize()
{
	fColumnCount = 7;
	fColumns = new SSColumn[fColumnCount];
	int i=0;
	fColumns[i].fColumnName = "Numéro de compte";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 50);

	i++;
	fColumns[i].fColumnName = "Description de compte";
	fColumns[i].fColumnWidth = 200;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "IdLabel";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aCenter;
	fColumns[i].fElement =new CtxStaticDouble(this, i, 0); 

	i++;
	fColumns[i].fColumnName = "Libellé";
	fColumns[i].fColumnWidth = 400;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Type";
	fColumns[i].fColumnWidth = 80;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Category Description";
	fColumns[i].fColumnWidth = 200;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);

	i++;
	fColumns[i].fColumnName = "Classification";
	fColumns[i].fColumnWidth = 80;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
}

void EdlAccount::LoadData(const char * query)
{
	CtxEditList::LoadData(query);

	try {
		SqlWrapper * sqlWrapper = new SqlWrapper("c50,c255,d,c255,c50,c100,c100", query);
		int count = sqlWrapper->GetRowCount();

		SetLineCount(count);
		for(int i=0; i<count; i++) {
			int col = 0;
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][0].value<string>().c_str());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][1].value<string>().c_str());
			SaveElement(i, col++, &(*sqlWrapper)[i][2].value<double>());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][3].value<string>().c_str());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][4].value<string>().c_str());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][5].value<string>().c_str());
			SaveElement(i, col++, (void*)(*sqlWrapper)[i][6].value<string>().c_str());
		}
		Update();
	} catch (const sophis::sql::CSROracleException &ex) {
		GetDialog()->Message(ex.getError().c_str());
		throw;
	}
}

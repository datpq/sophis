/*
** Includes
*/
#include <fstream>
#include "ExportScenarioDlg.h"
#include "Resource\resource.h"
#include "CtxComboBox.h"
#include "SqlUtils.h"
#include "Config.h"
#include "systemutils.h"
#include "CtxElements.h"
#include "SqlWrapper.h"

/*
** Namespace
*/
using namespace sophis::gui;
using namespace eff::gui;
using namespace eff::utils;
using namespace eff::maps::gui;

struct EdlExportRowsItem {
	double id;
	char name[255];
};

void EdlExportRows::Initialize()
{
	fColumnCount = 2;
	fColumns = new SSColumn[fColumnCount];
	int i=0;
	fColumns[i].fColumnName = "ID";
	fColumns[i].fColumnWidth = 100;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CtxStaticDouble(this, i, 0); 

	i++;
	fColumns[i].fColumnName = "Name";
	fColumns[i].fColumnWidth = 500;
	fColumns[i].fAlignmentType = aLeft;
	fColumns[i].fElement = new CSRStaticText(this, i, 255);
}

void EdlExportRows::LoadData(const char * query)
{
	CtxEditList::LoadData(query);
	SqlWrapper* sqlWrapper = new SqlWrapper("d,c255", query);
	SetLineCount(sqlWrapper->GetRowCount());
	for(int i=0; i<sqlWrapper->GetRowCount(); i++) {
		int col = 0;
		SaveElement(i, col++, &(*sqlWrapper)[i][0].value<double>());
		SaveElement(i, col++, (void *)(*sqlWrapper)[i][1].value<string>().c_str());
	}
	Update();
}

void CboTables_SelectedIndexChanged(CtxComboBox * sender)
{
	string tableName = sender->SelectedText();
	string query = SqlUtils::QueryReturning1StringException("SELECT SELECTCLAUSE FROM MAPS_TKT_TABLES WHERE TABLE_NAME = '%s'", tableName.c_str());
	CSRFitDialog * dialog = sender->GetDialog();
	CtxEditList * lstExportRows = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_ROWS - ID_ITEM_SHIFT);
	lstExportRows->LoadData(query.c_str());
	//dialog->Message(tableName.c_str());
}

void CmdExport_OnClick(CtxButton * sender)
{
	CSRFitDialog * dialog = sender->GetDialog();
	CtxComboBox * cboTables = (CtxComboBox *)dialog->GetElementByAbsoluteId(IDC_CBO_TABLES - ID_ITEM_SHIFT);
	CtxEditList * lstExportRows = (CtxEditList *)dialog->GetElementByAbsoluteId(IDC_LST_ROWS - ID_ITEM_SHIFT);
	EdlExportRowsItem selectedRow = EdlExportRowsItem();
	lstExportRows->GetSelectedValue(1, &selectedRow.name);
	//dialog->Message(selectedRow.name);

	std::string exportedFileName = Config::getInstance()->getDllDirectory() + "\\MAPS_" + cboTables->SelectedText() + GetWindowsUserName() + "_" + GetCurDateTimeYYMMDDHHMMSS() + ".SQL";
	std::ofstream ofsExportedFile (exportedFileName, ofstream::out);
	SqlWrapper* sqlWrapper = new SqlWrapper("c4000", "SELECT MSG FROM EF_LOGS ORDER BY ID");
	for(int i=0; i<sqlWrapper->GetRowCount(); i++) {
		ofsExportedFile << (*sqlWrapper)[i][0].value<string>() << "\n";
	}
	ofsExportedFile.flush();
	ofsExportedFile.close();
}

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
ExportScenarioDlg::ExportScenarioDlg() : CSRFitDialog()
{

	fResourceId	= IDD_ExportScenario_DLG - ID_DIALOG_SHIFT;

	NewElementList(4);

	int nb = 0;

	if (fElementList)
	{
		CtxComboBox * cboTables = new CtxComboBox(this, IDC_CBO_TABLES - ID_ITEM_SHIFT, "SELECT ROWNUM, TABLE_NAME FROM MAPS_TKT_TABLES WHERE ENABLED = 1",
			true, -1, &CboTables_SelectedIndexChanged);
		fElementList[nb++] = cboTables;
		fElementList[nb++]	= new EdlExportRows(this, IDC_LST_ROWS - ID_ITEM_SHIFT);
		fElementList[nb++]	= new CtxButton(this, IDC_CMD_EXPORT - ID_ITEM_SHIFT, &CmdExport_OnClick);
		fElementList[nb++]	= new CSRCancelButton(this);
		CboTables_SelectedIndexChanged(cboTables);
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void ExportScenarioDlg::OnOK()
{

}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ ExportScenarioDlg::~ExportScenarioDlg()
{
}
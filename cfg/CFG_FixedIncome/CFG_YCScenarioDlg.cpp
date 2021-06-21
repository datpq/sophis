/*
** Includes
*/
#include "CFG_YCScenarioDlg.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphEditElement.h"
#include "SphInc/gui/SphEditList.h"
#include "CFG_YCDataTable.h"

/*
** Namespace
*/
using namespace sophis::gui;


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------

 long CFG_YCScenarioDlg::yieldCurveCode = 0;

CFG_YCScenarioDlg::CFG_YCScenarioDlg(long curveId) : CSRFitDialog()
{

	fResourceId	= IDD_DB_DIALOG - ID_DIALOG_SHIFT;

	NewElementList(eNbFields);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new CSROKButton(this);
		fElementList[nb++]	= new CSRCancelButton(this);
		fElementList[nb++] = new CFG_YCDataTable(this,eYCPointsTable);
		
	}
	yieldCurveCode = curveId;
}

 void CFG_YCScenarioDlg::OpenAfterInit()
{
	 CSRFitDialog::OpenAfterInit();

	 CFG_YCDataTable* ycPointsList = dynamic_cast<CFG_YCDataTable*>(GetElementByRelativeId(eYCPointsTable));
	 if (ycPointsList != nullptr)
	 {
		 ycPointsList->SetLineCount(0);
		 ycPointsList->Update();

		 if (yieldCurveCode != 0)
		 {
			 ycPointsList->fCurveId = yieldCurveCode;
			 ycPointsList->LoadList(yieldCurveCode);
		 }
	 }
	 

}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CFG_YCScenarioDlg::OnOK()
{
	CFG_YCDataTable* ycPointsList = dynamic_cast<CFG_YCDataTable*>(GetElementByRelativeId(eYCPointsTable));
	if (ycPointsList != nullptr)
	{		
		ycPointsList->CommandSave();

	}


}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ CFG_YCScenarioDlg::~CFG_YCScenarioDlg()
{
}

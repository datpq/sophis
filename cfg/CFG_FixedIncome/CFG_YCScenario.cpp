
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "CFG_YCScenario.h"
#include "CFG_YCScenarioDlg.h"


/*static*/ const char* CFG_YCScenario::__CLASS__ = "CFG_YCScenario";
int CFG_YCScenario::yCurveIdent = 0;
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CFG_YCScenario)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	CFG_YCScenario::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void CFG_YCScenario::Run()
{
	BEGIN_LOG("Run");

	long curveId = GetCode();
	// Create the dialog instance
	CFG_YCScenarioDlg *dialog = new CFG_YCScenarioDlg(curveId);

	// Display a modal dialog
	dialog->DoDialog();

	END_LOG();
}

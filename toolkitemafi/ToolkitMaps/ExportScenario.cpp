
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "ExportScenario.h"
#include "ExportScenarioDlg.h"


/*static*/ const char* ExportScenario::__CLASS__ = "ExportScenario";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(ExportScenario)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	ExportScenario::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void ExportScenario::Run()
{
	BEGIN_LOG("Run");
	

	// Create the dialog instance
	CSRFitDialog *dialog = new eff::maps::gui::ExportScenarioDlg();

	// Display a modal dialog
	dialog->DoDialog();
	
	END_LOG();
}

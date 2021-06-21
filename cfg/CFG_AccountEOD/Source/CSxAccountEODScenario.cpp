
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "CSxAccountEODScenario.h"
#include "CSxAccountEODScenarioDlg.h"


/*static*/ const char* CSxAccountEODScenario::__CLASS__ = "CSxAccountEODScenario";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CSxAccountEODScenario)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	CSxAccountEODScenario::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void CSxAccountEODScenario::Run()
{
	BEGIN_LOG("Run");
	
	try
	{
		// Create the dialog instance
		CSxAccountEODScenarioDlg *dialog = new CSxAccountEODScenarioDlg();

		// Display a modal dialog

		dialog->DoDialog();

		delete dialog;
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		CSRFitDialog::Message(FROM_STREAM("ERROR ("<<ex<<") while running the account EOD scenario"));		
	}
	catch(...)
	{		
		CSRFitDialog::Message(FROM_STREAM("Unhandled exception occured while running the account EOD scenario"));		
	}	
	
	END_LOG();
}

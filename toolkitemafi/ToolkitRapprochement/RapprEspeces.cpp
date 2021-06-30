
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "RapprEspeces.h"
#include "RapprEspecesDlg.h"


/*static*/ const char* RapprEspeces::__CLASS__ = "RapprEspeces";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(RapprEspeces)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	RapprEspeces::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void RapprEspeces::Run()
{
	BEGIN_LOG("Run");
	

	// Create the dialog instance
	CSRFitDialog *dialog = new  eff::emafi::gui::RapprEspecesDlg();

	// Display a modal dialog
	dialog->DoDialog();
	
	END_LOG();
}

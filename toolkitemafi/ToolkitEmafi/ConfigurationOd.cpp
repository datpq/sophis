
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "ConfigurationOd.h"
#include "ConfigurationOdDlg.h"
#include "SphInc/SphUserRights.h"
#include "Resource\resource.h"
#include "StringUtils.h"


using namespace eff::utils;

/*static*/ const char* ConfigurationOD::__CLASS__ = "ConfigurationOD";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(ConfigurationOD)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	ConfigurationOD::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void ConfigurationOD::Run()
{
	BEGIN_LOG("Run");
	

	CSRUserRights user;
	bool right = user.HasAccess("Emafi Admin");
	// Create the dialog instance
	CSRFitDialog *dialog= new eff::emafi::gui::ConfigurationOdDlg();
	if(right)
		// Display a modal dialog
		dialog->DoDialog();
	else
		dialog->Message(LoadResourceString(MSG_ACCESS_DENIED).c_str());


	
	END_LOG();
}

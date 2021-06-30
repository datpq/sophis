
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "ConfigButton.h"
#include "ConfigDlg.h"
#include "SphInc/SphUserRights.h"
#include "StringUtils.h"
#include "Resource\resource.h"


using namespace eff::utils;
/*static*/ const char* ConfigButton::__CLASS__ = "ConfigButton";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(ConfigButton)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	ConfigButton::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void ConfigButton::Run()
{
	BEGIN_LOG("Run");

	CSRUserRights user;
	bool right = user.HasAccess("Emafi Admin");
	// Create the dialog instance
	CSRFitDialog *dialog = new eff::emafi::gui::ConfigDlg();
	if(right)
		// Display a modal dialog
		dialog->DoDialog();
	else
		dialog->Message(LoadResourceString(MSG_ACCESS_DENIED).c_str());

	END_LOG();
}


/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "EtatReglementaire.h"
#include "EtatsReglementaireDlg.h"
#include "SphInc/SphUserRights.h"
#include "StringUtils.h"
#include "Resource\resource.h"


using namespace eff::utils;
/*static*/ const char* EtatReglementaire::__CLASS__ = "EtatReglementaire";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(EtatReglementaire)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	EtatReglementaire::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void EtatReglementaire::Run()
{
	BEGIN_LOG("Run");

	CSRUserRights user;
	bool right = user.HasAccess("Emafi Admin");
	// Create the dialog instance
	CSRFitDialog *dialog = new eff::emafi::gui::EtatReglementaireDlg();
	if(right)
		// Display a modal dialog
		dialog->DoDialog();
	else
		dialog->Message(LoadResourceString(MSG_ACCESS_DENIED).c_str());

	END_LOG();
}

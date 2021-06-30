
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "GestionOd.h"
#include "GestionOdDlg.h"


/*static*/ const char* GestionOd::__CLASS__ = "GestionOd";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(GestionOd)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	GestionOd::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void GestionOd::Run()
{
	BEGIN_LOG("Run");
	

	// Create the dialog instance
	eff::emafi::gui::GestionOdDlg *dialog = new eff::emafi::gui::GestionOdDlg();

	// Display a modal dialog
	dialog->DoDialog();
	
	END_LOG();
}

#ifndef __ConfigurationOd__H__RIBBON__
	#define __ConfigurationOd__H__RIBBON__

/*
** Includes
*/
// standard
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/gui/SphRibbon.h"
#include "ConfigurationOdDlg.h"
#include "SphInc/SphUserRights.h"
#include "StringUtils.h"
#include "Resource\resource.h"


using namespace eff::utils;

class ConfigurationOdRibbon : public sophis::gui::CSRRibbonCommand
{
	//------------------------------------ PUBLIC ------------------------------------
public:
	DECLARATION_RIBBON_COMMAND(ConfigurationOdRibbon);
	void Do() const
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

private:
	static const char* __CLASS__;
	
};

 const char* ConfigurationOdRibbon::__CLASS__ = "GestionOdRibbon";
#endif // !__ConfigurationOd__H__RIBBON__

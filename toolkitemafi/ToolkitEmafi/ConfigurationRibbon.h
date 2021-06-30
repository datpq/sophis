#ifndef __Configuration__H__RIBBON__
	#define __Configuration__H__RIBBON__

/*
** Includes
*/
// standard
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/gui/SphRibbon.h"
#include "ConfigDlg.h"
#include "StringUtils.h"
#include "Resource\resource.h"


using namespace eff::utils;


class ConfigurationRibbon : public sophis::gui::CSRRibbonCommand
{
	//------------------------------------ PUBLIC ------------------------------------
public:
	DECLARATION_RIBBON_COMMAND(ConfigurationRibbon);
	void Do() const
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

private:
	static const char* __CLASS__;
	
};

 const char* ConfigurationRibbon::__CLASS__ = "GestionOdRibbon";
#endif // !__Configuration__H__RIBBON__

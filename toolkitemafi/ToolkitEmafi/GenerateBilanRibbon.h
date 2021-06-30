#ifndef __GenerateBilan__H__RIBBON__
	#define __GenerateBilan__H__RIBBON__

/*
** Includes
*/
// standard
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/gui/SphRibbon.h"
#include "GenerateBilanDlg.h"



class GenerateBilanRibbon : public sophis::gui::CSRRibbonCommand
{
	//------------------------------------ PUBLIC ------------------------------------
public:
	DECLARATION_RIBBON_COMMAND(GenerateBilanRibbon);
	void Do() const
	{
	
	BEGIN_LOG("Run");
	

	// Create the ribbon instance
	eff::emafi::gui::GenerateBilanDlg *dialog = new eff::emafi::gui::GenerateBilanDlg();

	// Display a modal dialog
	dialog->DoDialog();
	
	END_LOG();
	}

private:
	static const char* __CLASS__;
	
};

 const char* GenerateBilanRibbon::__CLASS__ = "GestionOdRibbon";
#endif // !__GenerateBilan__H__RIBBON__

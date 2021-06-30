#ifndef __GestionOd__H__RIBBON__
	#define __GestionOd__H__RIBBON__

/*
** Includes
*/
// standard
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/gui/SphRibbon.h"
#include "GestionOdDlg.h"



class GestionOdRibbon : public sophis::gui::CSRRibbonCommand
{
	//------------------------------------ PUBLIC ------------------------------------
public:
	DECLARATION_RIBBON_COMMAND(GestionOdRibbon);
	void Do() const
	{
	
	BEGIN_LOG("Run");
	

	// Create the ribbon instance
	eff::emafi::gui::GestionOdDlg *dialog = new eff::emafi::gui::GestionOdDlg();

	// Display a modal dialog
	dialog->DoDialog();
	
	END_LOG();
	}

private:
	static const char* __CLASS__;
	
};

 const char* GestionOdRibbon::__CLASS__ = "GestionOdRibbon";
#endif // !__GestionOd__H__RIBBON__

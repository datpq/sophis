#ifndef __RapprEspeces_H__
#define __RapprEspeces_H__

/*
** Includes
*/
// standard
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/gui/SphRibbon.h"
#include "RapprEspecesDlg.h"



class RapprEspecesRibbon : public sophis::gui::CSRRibbonCommand
{
	//------------------------------------ PUBLIC ------------------------------------
public:
	DECLARATION_RIBBON_COMMAND(RapprEspecesRibbon);
	void Do() const
	{

		BEGIN_LOG("Run");


		// Create the ribbon instance
		eff::emafi::gui::RapprEspecesDlg *dialog = new eff::emafi::gui::RapprEspecesDlg();

		// Display a modal dialog
		dialog->DoDialog();

		END_LOG();
	}

private:
	static const char* __CLASS__;

};

const char* RapprEspecesRibbon::__CLASS__ = "RapprEspecesRibbon";
#endif // !__EtatsReglementaire_H__

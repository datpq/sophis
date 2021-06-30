#ifndef __EtatsReglementaire_H__
	#define __EtatsReglementaire_H__

/*
** Includes
*/
// standard
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/gui/SphRibbon.h"
#include "EtatsReglementaireDlg.h"



class EtatReglementaireRibbon : public sophis::gui::CSRRibbonCommand
{
	//------------------------------------ PUBLIC ------------------------------------
public:
	DECLARATION_RIBBON_COMMAND(EtatReglementaireRibbon);
	void Do() const
	{
	
	BEGIN_LOG("Run");
	

	// Create the ribbon instance
	eff::emafi::gui::EtatReglementaireDlg *dialog = new eff::emafi::gui::EtatReglementaireDlg();

	// Display a modal dialog
	dialog->DoDialog();
	
	END_LOG();
	}

private:
	static const char* __CLASS__;
	
};

 const char* EtatReglementaireRibbon::__CLASS__ = "EtatReglementaireRibbon";
#endif // !__EtatsReglementaire_H__

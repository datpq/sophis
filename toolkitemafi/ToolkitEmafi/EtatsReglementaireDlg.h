#ifndef __EtatReglementaireDlg__H__
	#define __EtatReglementaireDlg__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"
#include "CtxComboBox.h"


namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class EtatReglementaireDlg : public sophis::gui::CSRFitDialog
			{
				//------------------------------------ PUBLIC ------------------------------------
			public:

				/**
				* Constructor
				By default, it is the dialog resource ID is 6030
				*/
				EtatReglementaireDlg();

				/**
				* Destructor
				*/
				virtual ~EtatReglementaireDlg();


				//------------------------------------ PROTECTED ----------------------------------
			protected:

				//------------------------------------ PRIVATE ------------------------------------
			private:

			};
		}
	}
}

#endif // !__EtatReglementaireDlg__H__
#ifndef __GestionOdDLG__H__
	#define __GestionOdDLG__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"
#include "SphInc/gui/SphTabButton.h"

/*
** Class
*/
namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class GestionOdDlg : public sophis::gui::CSRFitDialog
			{
				//------------------------------------ PUBLIC ------------------------------------
			public:

				/**
				* Constructor
				By default, it is the dialog resource ID is 6030
				*/
				GestionOdDlg();

				/**
				* Destructor
				*/
				virtual ~GestionOdDlg();
				//------------------------------------ PROTECTED ----------------------------------
			protected:

				//------------------------------------ PRIVATE ------------------------------------
			private:

			};

			class TabGestionOD : public sophis::gui::CSRTabButton
			{
			public:
				TabGestionOD(CSRFitDialog *dialog, int ERId_Element);
				virtual void Open();
			};
		}
	}
}

#endif // !__GestionOd__H__
#ifndef __ConfigurationODDLG__H__
	#define __ConfigurationODDLG__H__
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
			class ConfigurationOdDlg : public sophis::gui::CSRFitDialog
			{
				//------------------------------------ PUBLIC ------------------------------------
			public:

				/**
				* Constructor
				By default, it is the dialog resource ID is 6030
				*/
				ConfigurationOdDlg();

				/**
				* Destructor
				*/
				virtual ~ConfigurationOdDlg();
				//------------------------------------ PROTECTED ----------------------------------
			protected:

				//------------------------------------ PRIVATE ------------------------------------
			private:

			};

			class TabConfigOD : public sophis::gui::CSRTabButton
			{
			public:
				TabConfigOD(CSRFitDialog *dialog, int ERId_Element);
				virtual void Open();
			};
		}
	}
}

#endif // !__ConfigurationOD__H__
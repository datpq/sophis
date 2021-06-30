#ifndef __GenerateBilanDLG__H__
	#define __GenerateBilanDLG__H__
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
			class GenerateBilanDlg : public sophis::gui::CSRFitDialog
			{
				//------------------------------------ PUBLIC ------------------------------------
			public:

				/**
				* Constructor
				By default, it is the dialog resource ID is 6030
				*/
				GenerateBilanDlg();

				/**
				* Destructor
				*/
				virtual ~GenerateBilanDlg();

				CtxComboBoxItem GenerateBilanDlg::GetSelectedFolio();
				CtxComboBoxItem GenerateBilanDlg::GetSelectedFileType();

				//------------------------------------ PROTECTED ----------------------------------
			protected:

				//------------------------------------ PRIVATE ------------------------------------
			private:

			};
		}
	}
}

#endif // !__GenerateBilan__H__
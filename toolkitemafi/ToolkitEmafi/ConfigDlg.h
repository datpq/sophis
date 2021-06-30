#pragma once
#ifndef __ConfigDlg__H__
	#define __ConfigDlg__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"

/*
** Class
*/


namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class ConfigDlg : public sophis::gui::CSRFitDialog
			{
				//------------------------------------ PUBLIC ------------------------------------
			public:

				/**
				* Constructor
				By default, it is the dialog resource ID is 6030
				*/
				ConfigDlg();

				//------------------------------------ PROTECTED ----------------------------------
			protected:

				//------------------------------------ PRIVATE ------------------------------------
			private:

			};
		}
	}
}

#endif // !__ConfigDlg__H__
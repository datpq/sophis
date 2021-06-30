#pragma once
#include "SphInc/gui/SphDialog.h"


namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabPageAccountDlg : public sophis::gui::CSRFitDialog
			{
			public:
				TabPageAccountDlg(void);
				void UpdateElements(bool isAddMode);
			};
		}
	}
}


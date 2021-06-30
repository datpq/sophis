#pragma once
#include "SphInc/gui/SphDialog.h"


namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabPageLabelDlg : public sophis::gui::CSRFitDialog
			{
			public:
				TabPageLabelDlg(void);
				bool IsAddMode();
				void UpdateElements(bool isAddMode);
			};
		}
	}
}

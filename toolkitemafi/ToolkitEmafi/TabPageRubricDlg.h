#pragma once
#include "SphInc/gui/SphDialog.h"


namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabPageRubricDlg : public sophis::gui::CSRFitDialog
			{
			public:
				TabPageRubricDlg(void);
				bool IsAddMode();
				void UpdateElements(bool isAddMode);
			};
		}
	}
}

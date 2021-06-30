#pragma once
#include "SphInc/gui/SphDialog.h"


namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabPageHistoDlg : public sophis::gui::CSRFitDialog
			{
			public:

				int generatingPageIndex;
				TabPageHistoDlg(int num_od, int generatingPageIndex);
				void UpdateElements(bool isAddMode);
			};
		}
	}
}

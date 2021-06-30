#pragma once
#include "SphInc/gui/SphDialog.h"
#include "CtxComboBox.h"

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabPageValidationDlg : public sophis::gui::CSRFitDialog
			{
			public:
				long status;
			public:
				TabPageValidationDlg(long initial_status);
				CtxComboBoxItem TabPageValidationDlg::GetSelectedFolio();
			};
		}
	}
}
#pragma once
#include "SphInc/gui/SphDialog.h"
#include "CtxComboBox.h"

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabPageConsultationDlg : public sophis::gui::CSRFitDialog
			{
			public:
				long id_statut;
			public:
				TabPageConsultationDlg(long id_statut);
				CtxComboBoxItem TabPageConsultationDlg::GetSelectedFolio();
			};
		}
	}
}
	
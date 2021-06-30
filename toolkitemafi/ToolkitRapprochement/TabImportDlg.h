#pragma once
#include "SphInc/gui/SphDialog.h"
#include "TabConsultMouvementDlg.h"
#include "TabConsultRelevDlg.h"

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabImportDlg : public sophis::gui::CSRFitDialog
			{
			public:
				TabImportDlg(TabConsultMouvementDlg* tab2, TabConsultRelevDlg* tab3);
				TabImportDlg(void);
				TabConsultMouvementDlg* tab2;
				TabConsultRelevDlg* tab3;
			};
		}
	}
}

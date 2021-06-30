#pragma once
#include "SphInc/gui/SphDialog.h"
#include "CtxEditList.h"

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabPageInsertionDlg : public sophis::gui::CSRFitDialog
			{
			public:
				TabPageInsertionDlg();
			};

			class EdlOdUpdate : public eff::gui::CtxEditList
			{
			public:
				EdlOdUpdate(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
					: CtxEditList(dialog, ERId_List, f)
				{
					Initialize();
				};
				void Initialize();
			};
		}
	}
}
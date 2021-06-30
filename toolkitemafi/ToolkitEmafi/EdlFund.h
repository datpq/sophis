#pragma once
#include "CtxEditList.h"

using namespace eff::gui;

struct EdlFundItem {
	double sicovam;
	char libelle[50];
};

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class EdlFund : public CtxEditList
			{
			public:
				EdlFund(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL,int maxSelection = 1)
					: CtxEditList(dialog, ERId_List, f)
				{
					Initialize();
					SetMaxSelection(maxSelection);
				};
				virtual void Initialize();
				virtual void LoadData(const char * query);
			};
		}
	}
}

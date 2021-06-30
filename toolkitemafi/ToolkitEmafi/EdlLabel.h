#pragma once
#include "CtxEditList.h"

using namespace eff::gui;

struct EdlLabelItem {
	double id;
	double id_rubric;
	double id_parent;
	char rubric[255];
	char label[255];
	char active[2];
	char account_number[50];
	char account_desc[50];
};

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class EdlLabel : public CtxEditList
			{
			public:
				EdlLabel(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
					: CtxEditList(dialog, ERId_List, f)
				{
					Initialize();
				};
				virtual void Initialize();
				virtual void LoadData(const char * query);
			};
		}
	}
}

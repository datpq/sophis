#pragma once
#include "CtxEditList.h"

using namespace eff::gui;

struct EdlAccountItem {
	char account_number[50];
	char account_desc[255];
	char account_type[50];
	char label[255];
	double id_label;
	char categ_desc[100];
	char classification[100];
};

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class EdlAccount : public CtxEditList
			{
			public:
				EdlAccount(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
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

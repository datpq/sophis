#pragma once
#include "CtxEditList.h"

using namespace eff::gui;

struct EdlRubricItem {
	char rubric[255];
	char rubric_type[50];
	long id;
};

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class EdlRubric : public CtxEditList
			{
			public:
				EdlRubric(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
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

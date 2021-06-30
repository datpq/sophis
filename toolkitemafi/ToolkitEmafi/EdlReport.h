#pragma once
#include "CtxEditList.h"

using namespace eff::gui;

struct EdlReportItem {
	char report_name[50];
	char report_type[4];
};

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class EdlReport : public CtxEditList
			{
			public:
				EdlReport(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL,int maxSelection = 1)
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

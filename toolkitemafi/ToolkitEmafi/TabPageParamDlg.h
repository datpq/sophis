#pragma once
#include "SphInc/gui/SphDialog.h"
#include "CtxEditList.h"


struct EdlParamItem {
	char code[255];
	char description[50];
	long ordre;
	
};



namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabPageParamDlg : public sophis::gui::CSRFitDialog
			{
			public:
				TabPageParamDlg(const char* categorie_);
				const char* categorie;
			};

			class EdlOdParam : public eff::gui::CtxEditList
			{
			public:
				EdlOdParam(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
					: CtxEditList(dialog, ERId_List, f)
				{
					Initialize();

				};
				void Initialize();
				void LoadData(const char * query);
			};
		}
	}
}
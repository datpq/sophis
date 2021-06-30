#pragma once
#include "CtxEditList.h"

using namespace eff::gui;

struct EdlOdHistoItem {
	long id;
	char user_name[255];
	long version;
	char date_modif[255];
	long num_od;
	double id_posting;
	char entity_name[255];
	char portfolio[255];

	char tiers_name[255];
	char account_number[255];
	double amount;

	char sens[255];
	char posting_date[255];
	char generation_date[255];
	char journal[255];

	char piece[255];
	char commentaire[255];
	long status;
	long od_status;
	char operateur[255];
};

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class EdlOdHisto : public CtxEditList
			{
			public:
				EdlOdHisto(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
					: CtxEditList(dialog, ERId_List, f)
				{
					Initialize();
				};
				virtual void Initialize();
				virtual void LoadData(const char * query);
				virtual void GetLineColor(int lineIndex, eTextColorType &color) const; 
			};
		}
	}
}

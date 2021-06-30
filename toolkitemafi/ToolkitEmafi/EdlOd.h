#pragma once
#include "CtxEditList.h"

using namespace eff::gui;

struct EdlOdItem {
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
	char devise[255];
	char journal[255];

	char piece[255];
	char code_commentaire[255];
	char commentaire[255];
	long od_status;
	char status[255];
	long status_id;
	char operateur[255];
};

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class EdlOd : public CtxEditList
			{
			public:
				EdlOd(CSRFitDialog *dialog, int ERId_List,
					void (*selectedIndexChangedHandler)(CtxEditList *, int lineNumber) = NULL,
					void (*doubleClickHandler)(CtxEditList*, int lineNumber) = NULL)
					: CtxEditList(dialog, ERId_List, selectedIndexChangedHandler, doubleClickHandler)
				{
					Initialize();
				};
				virtual void Initialize();
				virtual void LoadData(const char * query);
				virtual void GetLineColor(int lineIndex, eTextColorType &color) const; 
				virtual std::string Getquery(bool valid, long ptf,const char * startDate,const char * endDate,const char * devise,const char * journal, const char * piece, long statutId);
			};
		}
	}
}

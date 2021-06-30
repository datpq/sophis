#pragma once
#include "SphInc/gui/SphDialog.h"
#include "CtxEditList.h"

using namespace eff::gui;
//***************
struct EdlMvtItem {
	 char type_mvt[255];
	 long id;
	 char compte[255];
	 char external_account[255];
	 char devise[10];
	 char dateOp[20];
	 char dateVal[20];
	 double montant;
	 char codeOp[100];
	 char codeInterbanc[100];
	 char libelle[255];
	 char refExt[100];
	 char nomFichier[30];
};



//****************
namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabConsultMouvementDlg : public sophis::gui::CSRFitDialog
			{
			public:
				TabConsultMouvementDlg(void);
			};

			class EdlMvt : public CtxEditList
			{
			public:
				EdlMvt(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
					: CtxEditList(dialog, ERId_List, f)
				{
					Initialize();
				};
				virtual void Initialize();
				virtual void LoadData(const char * query);
				virtual std::string Getquery(const char* compte, long codeDevise, const char * date);
				
			};
		}
	}
}
			
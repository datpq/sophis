#pragma once
#include "SphInc/gui/SphDialog.h"
#include "CtxEditList.h"

using namespace eff::gui;
//***************
struct EdlRelevItem {
		char Type_Enreg[13];
		char Code_banq[20];
		char Code_Op[20];
		char Code_guichet[20];
		char Code_Devise[20];
		long Nbr_Decimal;
		char Num_compte[20];
		char Code_Oper_interb[20];
		char Date_op[20];
		char Date_val[20];
		char Libelle[20];
		char Num_ecrit[20];
		double Montant;
		char Refer_1[20];
		char Nom_Fichier[20];
		long Refer_FI;
		char Statut[20];
		char Motif_Rejet[20];
};



//****************
namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class TabConsultRelevDlg : public sophis::gui::CSRFitDialog
			{
			public:
				TabConsultRelevDlg(void);
			};

			class EdlRelev : public CtxEditList
			{
			public:
				EdlRelev(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
					: CtxEditList(dialog, ERId_List, f)
				{
					Initialize();
				};
				virtual void Initialize();
				virtual void LoadData(const char * query);
				virtual std::string Getquery(const char* compte, long codeDevise, const char * dateD, const char * dateF);
			};
		}
	}
}
			
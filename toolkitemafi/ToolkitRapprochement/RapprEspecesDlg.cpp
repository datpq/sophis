/*
** Includes
*/
#include "RapprEspecesDlg.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphTabButton.h"
#include "SphInc/gui/SphEditElement.h"
#include "TabImportDlg.h"
#include "TabConsultMouvementDlg.h"
#include "TabConsultRelevDlg.h"
/*
** Namespace
*/
using namespace eff::emafi::gui;

class TabRappr : public CSRTabButton
{
public:
	TabRappr(CSRFitDialog *dialog, int ERId_Element) : CSRTabButton(dialog, ERId_Element)
	{
		fNbPages = 3;
		fPages = new SSPageTab[fNbPages];

		TabConsultMouvementDlg * tab2 = new TabConsultMouvementDlg();
		TabConsultRelevDlg * tab3 = new TabConsultRelevDlg();


		fPages[0].dialog = new TabImportDlg(tab2,tab3);
		fPages[0].resIdTitle = STR_TAB_IMPORT;

		fPages[1].dialog = tab2;
		fPages[1].resIdTitle = STR_TAB_MOUVEMENT;

		fPages[2].dialog = tab3;
		fPages[2].resIdTitle = STR_TAB_RELEVES;
	}
};


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
RapprEspecesDlg::RapprEspecesDlg() : CSRFitDialog()
{

	fResourceId	= IDD_RapprEspeces_DLG - ID_DIALOG_SHIFT;

	NewElementList(2);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new TabRappr(this, IDC_TAB_IMPORTATION - ID_ITEM_SHIFT);
		fElementList[nb++]	= new CSRCancelButton(this);
	}
	
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ RapprEspecesDlg::~RapprEspecesDlg()
{
}
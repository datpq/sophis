#include "ConfigDlg.h"
#include "Resource\resource.h"
#include "SphInc/gui/SphTabButton.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphEditElement.h"

#include "TabPageRubricDlg.h"
#include "TabPageLabelDlg.h"
#include "TabPageAccountDlg.h"
#include "GenerateBilanDlg.h"
#include "Log.h"

using namespace eff::emafi::gui;

class TabConfig : public CSRTabButton
{
public:
	TabConfig(CSRFitDialog *dialog, int ERId_Element) : CSRTabButton(dialog, ERId_Element)
	{
		fNbPages = 3;
		fPages = new SSPageTab[fNbPages];
		fPages[0].dialog = new TabPageRubricDlg();
		fPages[0].resIdTitle = STR_TAB_RUBRIC;
		fPages[1].dialog = new TabPageLabelDlg();
		fPages[1].resIdTitle = STR_TAB_LABEL;
		fPages[2].dialog = new TabPageAccountDlg();
		fPages[2].resIdTitle = STR_TAB_ACCOUNT;
	}
};

ConfigDlg::ConfigDlg() : CSRFitDialog()
{
	fResourceId	= IDD_DLG_Config - ID_DIALOG_SHIFT;

	NewElementList(2);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new TabConfig(this, IDC_TAB_Config - ID_ITEM_SHIFT);
		fElementList[nb++]	= new CSRCancelButton(this);
	}
}

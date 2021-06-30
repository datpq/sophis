#include "ConfigurationOdDlg.h"
#include "Resource\resource.h"
#include "SphInc/gui/SphButton.h"
#include "TabPageParamDlg.h"
#include "StringUtils.h"
#include "Config.h"

using namespace eff::emafi::gui;
using namespace eff::utils;

TabConfigOD::TabConfigOD(CSRFitDialog *dialog, int ERId_Element) : CSRTabButton(dialog, ERId_Element)
{
}

void TabConfigOD::Open()
{
	string lstCategories = Config::getSettingStr(TOOLKIT_SECTION, "ConfigurableCategories");
	vector<string> arrCategories = StrSplit(lstCategories.c_str(), ",");
	for (int i = 0; i<arrCategories.size(); i++)
	{
		CSRFitDialog * dialog = new TabPageParamDlg(arrCategories[i].c_str());
		dialog->SetTitle(arrCategories[i].c_str());
		AppendPage(dialog);
	}
}
	
ConfigurationOdDlg::ConfigurationOdDlg() : CSRFitDialog()
{
	fResourceId	= IDD_CONFIGURATIONOD_DLG - ID_DIALOG_SHIFT;
	NewElementList(2);
	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new TabConfigOD(this,IDC_TAB_Config_OD - ID_ITEM_SHIFT);
		fElementList[nb++]	= new CSRCancelButton(this);
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ ConfigurationOdDlg::~ConfigurationOdDlg()
{
}
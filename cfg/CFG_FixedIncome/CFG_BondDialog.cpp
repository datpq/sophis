#include "CFG_BondDialog.h"
#include "Resource\resource.h"
#include "SphInc\instrument\SphBond.h"
#include "SphInc\gui\SphEditElement.h"

WITHOUT_CONSTRUCTOR_INSTRUMENT_DIALOG(CFG_BondDialog);

CFG_BondDialog::CFG_BondDialog() : CSRInstrumentDialogWithTabs()
{
	fResourceId = IDD_BOND_DIALOG - ID_DIALOG_SHIFT;
	fElementCount = 0;

	OverloadExistingDialog(new CFG_BondCalculationTab(), kBondCalculationTab);
};


CFG_BondCalculationTab::CFG_BondCalculationTab() : CSRInstrumentTabPage()
{
	fResourceId  = IDD_BOND_CALCULATION_TAB - ID_DIALOG_SHIFT;
	NewElementList(1);

	if(fElementList)
	{
		fElementList[0] = new CSREditDouble(this, IDC_RISK_SPREAD - ID_ITEM_SHIFT, CSRPreference::GetNumberOfDecimalsForPrice(), -100.0, 100.0, 0.0, "CFG_RISK_SPREAD");
	}
};


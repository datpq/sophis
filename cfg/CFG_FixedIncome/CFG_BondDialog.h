#ifndef __CFG_BondDialog__H__
#define __CFG_BondDialog__H__

#include "SphInc\gui\SphInstrumentDialogWithTabs.h"

class CFG_BondDialog : public sophis::gui::CSRInstrumentDialogWithTabs
{
	DECLARATION_INSTRUMENT_DIALOG(CFG_BondDialog);
};


class CFG_BondCalculationTab : public sophis::gui::CSRInstrumentTabPage
{
public:
	CFG_BondCalculationTab();
};

#endif //!__CFG_BondDialog__H__

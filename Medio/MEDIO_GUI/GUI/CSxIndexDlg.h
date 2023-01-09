#ifndef __CSxIndexDlg__H__
	#define __CSxIndexDlg__H__


/*
** Includes
*/

#include "SphInc/gui/SphInstrumentDialog.h"
#include "..\Resource\resource.h"

/*
** Class
*/

class CSxIndexDlg : public sophis::gui::CSRInstrumentDialog 
{
	DECLARATION_INSTRUMENT_DIALOG(CSxIndexDlg)

//------------------------------------ PUBLIC ---------------------------------
public:

	// Fields enumeration
	// for every new item in dialog, add its enumeration here...
	enum // already without ID_ITEM_SHIFT
	{
		ePitClosingTime			= IDC_PITCOSEING - ID_ITEM_SHIFT,
		ePitOpeningTime			= IDC_PITOPENING - ID_ITEM_SHIFT,
		eMarketTimeZone			= IDC_MARKET_TOMEZONE - ID_ITEM_SHIFT,
		eNbFields = 3
	};
};

#endif //!__CSxIndexDlg__H__

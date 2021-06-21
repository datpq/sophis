#ifndef __ShareDialog__H__
	#define __ShareDialog__H__


/*
** Includes
*/

#include "SphInc/gui/SphInstrumentDialog.h"
#include "Resource\resource.h"

/*
** Class
*/

class ShareDialog : public sophis::gui::CSRInstrumentDialog 
{
	DECLARATION_INSTRUMENT_DIALOG(ShareDialog)

//------------------------------------ PUBLIC ---------------------------------
public:

	// Fields enumeration
	// for every new item in dialog, add its enumeration here...
	enum // already without ID_ITEM_SHIFT
	{
		ePriceGap	= IDC_RATE_GAP - ID_ITEM_SHIFT,
		eNbFields	= 1
	};

	virtual void   ReInitialise();
};

#endif //!__ShareDialog__H__

/*
** Includes
*/
#include "SphInc/gui/SphEditElement.h"
// specific
#include "CSxIndexDlg.h"
#include "../MediolanumConstants.h"

/*
** Namespace
*/
using namespace sophis::gui;

const char* pitClosingTime = MEDIO_GUI_FIELDNAME_PIT_CLOSING_TIME;
const char* marketTimeZone = MEDIO_GUI_FIELDNAME_MARKET_TIMEZONE;
const char* pitOpeningTime = MEDIO_GUI_FIELDNAME_PIT_OPENING_TIME;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_INSTRUMENT_DIALOG(CSxIndexDlg)

//-------------------------------------------------------------------------------------------------------------
CSxIndexDlg::CSxIndexDlg() : CSRInstrumentDialog()
{
	fResourceId  = IDD_INDEXBASKET_DIALOG - ID_DIALOG_SHIFT;

	NewElementList(eNbFields);

	int nb = 0;
	if(fElementList)
	{
		fElementList[nb++] = new CSREditText(this, ePitClosingTime, 40, 0, pitClosingTime);
		fElementList[nb++] = new CSREditText(this, ePitOpeningTime, 40, 0, pitOpeningTime);
		fElementList[nb++] = new CSREditText(this, eMarketTimeZone, 40, 0, marketTimeZone);
		// to do
	}
}



/*
** Includes
*/
#include "SphInc/gui/SphEditElement.h"
// specific
#include "ShareDialog.h"

/*
** Namespace
*/
using namespace sophis::gui;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_INSTRUMENT_DIALOG(ShareDialog)

//-------------------------------------------------------------------------------------------------------------
ShareDialog::ShareDialog() : CSRInstrumentDialog()
{
	fResourceId  = IDD_SHARE_DIALOG - ID_DIALOG_SHIFT;

	NewElementList(eNbFields);
	
	int nb = 0;

	if(fElementList)
	{
		fElementList[nb++]	= new CSREditDouble(this,ePriceGap,4,0,1000000, 6, "PRICE_GAP");
	}
}

void   ShareDialog::ReInitialise()
{
	double sharePriceGap = 0.0;
	GetElementByRelativeId(ShareDialog::ePriceGap)->GetValue(&sharePriceGap);			
	UpdateElement(ePriceGap);
}
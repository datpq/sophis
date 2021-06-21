#pragma once

/*
** Includes
*/

#include "SphInc/gui/SphInstrumentDialog.h"
#include "SphInc/gui/SphButton.h"
#include "Resource\resource.h"

/*
** Class
*/

class CSxYieldCurveDlg : public sophis::gui::CSRInstrumentDialog
{
	DECLARATION_INSTRUMENT_DIALOG(CSxYieldCurveDlg)

		//------------------------------------ PUBLIC ---------------------------------
public:

	static long yieldCurveId;
	// Fields enumeration
	// for every new item in dialog, add its enumeration here...
	enum // already without ID_ITEM_SHIFT
	{
	//	eXMLData = ID_XML_ID - ID_ITEM_SHIFT,
		eDisplayButton = IDC_TKT_BUTTON - ID_ITEM_SHIFT,
		eNbFields = 1
	};
	virtual void OpenAfterInit() override;
	virtual void ElementValidation(int EAId_Modified) override;
};

class EnhancedDataButton : public sophis::gui::CSRButton
{
public:
	long fcurveId;
	/** Constructor.
	The constructor EnhancedDataButton::EnhancedDataButton() calls the constructor CSRElement::CSRElement() by handing over
	the parameters dialog and ERId_Element.
	The ERId_Element parameter is by default 1.
	@param dialog points to the dialog to which the button belongs.
	@param ERId_Element is the relative ID of the button in the dialog.
	*/
	EnhancedDataButton(CSRFitDialog *dialog, int ERId_Element = 1);
	void SetCurveId(long id)
	{
		fcurveId = id;
	}

	/** Action associated with the button.
	Default actions associated with the OK button causes the closure
	of the dialog and the return to the method CSRFitDialog::DoDialog() with the exiting parameter.
	@see CSRFitDialog::OnOK()
	This method will in turn call CSRFitDialog::OnOK() of the containing dialog.
	@see CSRFitDialog::DoDialog()
	*/
	virtual	void	Action();
};

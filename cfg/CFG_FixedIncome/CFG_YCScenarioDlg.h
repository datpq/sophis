#pragma once

#ifndef __CFG_YCScenarioDLG__H__
	#define __CFG_YCScenarioDLG__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"
#include "Resource\resource.h"

/*
** Class
*/

class CFG_YCScenarioDlg : public sophis::gui::CSRFitDialog
{
//------------------------------------ PUBLIC ------------------------------------
public:

	/**
	* Constructor
	By default, it is the dialog resource ID is 6030
	*/
	CFG_YCScenarioDlg(long curveId);

	/**
	* Destructor
	*/
	virtual ~CFG_YCScenarioDlg();

	/**
	* Performs actions in response to pressing the OK button.
	This method is invoked if the dialog contains an element of type CSRElement-derived CSROKButton.
	Upon pressing the OK button, CSRFitDialog::OnOK() is subsequently invoked from CSROKBouton::Action().
	@version 4.5.2
	*/
	virtual	void	OnOK() override;

	virtual void OpenAfterInit() override;
	
	// Fields enumeration
	// for every new item in dialog, add its enumeration here...
	enum // already without ID_ITEM_SHIFT
	{
		eYCPointsTable= IDC_YC_TABLE-ID_ITEM_SHIFT,
		eNbFields = 3
	};

	static  long yieldCurveCode;
//------------------------------------ PROTECTED ----------------------------------
protected:

//------------------------------------ PRIVATE ------------------------------------
private:

};

#endif // !__CFG_YCScenario__H__

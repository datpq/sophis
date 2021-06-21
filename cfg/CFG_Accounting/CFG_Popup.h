/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// DialogTest.h : Interface for CSxDialogTest class
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __DIALOGTEST_H__
#define __DIALOGTEST_H__

/*
** Include
*/
#include "SphInc/gui/SphDialog.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "Resource/resource.h"

/*
** Class
*/
class CSxScenarioPopup : public sophis::gui::CSRFitDialog
{
//----------------------------------- PUBLIC -----------------------------------
public:
	CSxScenarioPopup();
	virtual ~CSxScenarioPopup();
	virtual	void ElementValidation(int EAId_Modified);

	// Fields enumeration
	enum // already without ID_ITEM_SHIFT
	{
		editOk=IDC_BUTTON_OK - ID_ITEM_SHIFT,
		editText=IDC_STATIC_MESSAGE -ID_ITEM_SHIFT,
		
		eNbFields=2,
	};

	
	virtual	void	OnOK();			
	

//----------------------------------- PRIVATE -----------------------------------
private:
	/*
	** Logger data
	*/
	static const char * __CLASS__;
};


#endif

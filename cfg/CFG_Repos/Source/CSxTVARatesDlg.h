#ifndef __CSxTVARatesDlg__H__
#define __CSxTVARatesDlg__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"
#include "SphInc/gui/SphEditList.h"
#include "SphInc/gui/SphButton.h"

#include "../Resource/resource.h"
#include "CSxTVARates.h"


/*
** Class
*/

class CSxTVARatesDlg : public sophis::gui::CSRFitDialog
{
	//------------------------------------ PUBLIC ------------------------------------
public:

	/**
	* Constructor
	By default, it is the dialog resource ID is 6030
	*/
	CSxTVARatesDlg();	

	virtual	void Open(void);

	/**
	* Performs actions in response to pressing the OK button.
	This method is invoked if the dialog contains an element of type CSRElement-derived CSROKButton.
	Upon pressing the OK button, CSRFitDialog::OnOK() is subsequently invoked from CSROKBouton::Action().
	@version 4.5.2
	*/
	virtual	void	OnOK();

	static void Display();

	void CloseDlg();

	void LoadFromTVARates();
	void SaveToTVARates() const;

	// Fields enumeration
	// for every new item in dialog, add its enumeration here...
	enum // already without ID_ITEM_SHIFT
	{
		eOK = 1,
		eCancel,
		eTVARatesList,		
		eNbFields = 3
	};

	//------------------------------------ PROTECTED ----------------------------------
protected:	

	//------------------------------------ PRIVATE ------------------------------------
private:

};

class CSxTVARatesEditList : public CSREditList
{
public:

	CSxTVARatesEditList(CSRFitDialog* dialog, int nre, const char* tableName = kUndefinedTable);	

	enum Controls// already without ID_ITEM_SHIFT
	{		
		eID,
		eRateType,
		eRateName,
		eRate,
		eColumnCount
	};
};

class CSxOKButton : public sophis::gui::CSROKButton
{
public:	

	CSxOKButton(CSRFitDialog *dialog, int ERId_Element = 1);

	/** Action associated with the button.
	Default actions associated with the OK button causes the closure
	of the dialog and the return to the method CSRFitDialog::DoDialog() with the exiting parameter.
	@see CSRFitDialog::OnOK()
	This method will in turn call CSRFitDialog::OnOK() of the containing dialog.
	@see CSRFitDialog::DoDialog()
	@version 4.5.2
	*/
	virtual	void	Action();

};



#endif // !__CSxTVARatesDlg__H__
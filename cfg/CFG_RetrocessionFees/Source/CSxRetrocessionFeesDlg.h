#ifndef __CSxRetrocessionFeesDlg__H__
#define __CSxRetrocessionFeesDlg__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"
#include "SphInc/gui/SphEditList.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/SphUserRightsEnums.h"
#include "..\Resource\resource.h"

#include "CSxRetrocessionFees.h"

/*
** Class
*/

class CSxRetrocessionFeesDlg : public sophis::gui::CSRFitDialog
{
//------------------------------------ PUBLIC ------------------------------------
public:

	/**
	* Constructor
	By default, it is the dialog resource ID is 6030
	*/
	CSxRetrocessionFeesDlg(eRightStatusType userRight);

	/**
	* Destructor
	*/
	virtual ~CSxRetrocessionFeesDlg();

	virtual	void Open(void);
	
	/**
	* Performs actions in response to pressing the OK button.
	This method is invoked if the dialog contains an element of type CSRElement-derived CSROKButton.
	Upon pressing the OK button, CSRFitDialog::OnOK() is subsequently invoked from CSROKBouton::Action().
	@version 4.5.2
	*/
	virtual	void	OnOK();

	static void Display(eRightStatusType userRight);

	void CloseDlg();

	void LoadFromFees(const CSxRetrocessionFees* fees);
	void SaveToFees(CSxRetrocessionFees* fees) const;

	// Fields enumeration
	// for every new item in dialog, add its enumeration here...
	enum // already without ID_ITEM_SHIFT
	{
		eOK = 1,
		eCancel,
		eRetrocessionFeesList,
		eAddButton,
		eRemoveButton,
		eNbFields = 5
	};

//------------------------------------ PROTECTED ----------------------------------
protected:

	eRightStatusType fUserRights;

//------------------------------------ PRIVATE ------------------------------------
private:

};

class CSxRetrocessionFeesEditList : public CSREditList
{
public:

	CSxRetrocessionFeesEditList(CSRFitDialog* dialog, int nre, const char* tableName = kUndefinedTable);
	~CSxRetrocessionFeesEditList();	

	void OnAddFees();
	void OnRemoveFees();

	//Add Button for fees List
	class FeesAddButton : public sophis::gui::CSRButton
	{
	public:
		FeesAddButton(sophis::gui::CSRFitDialog* dialog, int nre, CSxRetrocessionFeesEditList* feesList);
		void Action(); // override
	protected:
		CSxRetrocessionFeesEditList* fFeesList;
	};

	//Remove button for fees list
	class FeesRemoveButton : public sophis::gui::CSRButton
	{
	public:
		FeesRemoveButton(sophis::gui::CSRFitDialog* dialog, int nre, CSxRetrocessionFeesEditList* feesList);
		void Action(); // override
	protected:
		CSxRetrocessionFeesEditList* fFeesList;
	};

	enum Controls// already without ID_ITEM_SHIFT
	{		
		eName,
		eThirdType, //Promoteur ou apporteur d'affaire
		eFundName,
		eComputationMethod,
		eStartDate,
		eEndDate,
		eCommissionRate,
		eLevel1,
		eRetrocessionRate1,
		eLevel2,
		eRetrocessionRate2,
		eLevel3,
		eRetrocessionRate3,
		eLevel4,
		eRetrocessionRate4,
		eTVA_Rate,		
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



#endif // !__CSxRetrocessionFeesDlg__H__
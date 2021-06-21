#ifndef __CSxAccountEODScenarioDLG__H__
#define __CSxAccountEODScenarioDLG__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"
#include "SphInc/gui/SphEditList.h"
#include "SphInc/gui/SphButton.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(map)
#include "SphInc/SphUserRights.h"
#include "..\Resource\resource.h"
#include "SphInc/instrument/SphBond.h"

/*
** Class
*/

class CSxAccountEODScenarioDlg : public sophis::gui::CSRFitDialog
{
	//------------------------------------ PUBLIC ------------------------------------
public:

	/**
	* Constructor
	By default, it is the dialog resource ID is 6030
	*/
	CSxAccountEODScenarioDlg();

	/**
	* Destructor
	*/
	virtual ~CSxAccountEODScenarioDlg();

	virtual void	OpenAfterInit(void);

	/**
	* Performs actions in response to pressing the OK button.
	This method is invoked if the dialog contains an element of type CSRElement-derived CSROKButton.
	Upon pressing the OK button, CSRFitDialog::OnOK() is subsequently invoked from CSROKBouton::Action().
	@version 4.5.2
	*/
	virtual	void	OnOK();	

	// Fields enumeration
	// for every new item in dialog, add its enumeration here...
	enum // already without ID_ITEM_SHIFT
	{
		eOK = 1,
		eCancel,
		eListOfAccountEntities,
		eNAVDate,
		eNbFields = 4
	};
	static _STL::string RemoveEscape(_STL::string initialName);

	//------------------------------------ PROTECTED ----------------------------------
protected:
	void GetLastEODDate(_STL::map<long,long> &entityLastEODDateMap);	
	long GetFundId(const char* fundName);
	bool CheckData(_STL::map<_STL::string,long> &listOfSelectedEntities, long EODDate, bool &rollBack);
	void ProcessData(_STL::map<_STL::string,long> &listOfSelectedEntities, long EODDate, bool rollBack);
	bool CheckDealBOStatus(_STL::string accountEntityName, long eodDate);
	bool CheckSRBOStatus(_STL::string accountEntityName, long eodDate);
	eRightStatusType GetEditAccountingEODUserRight();
	void RollBack(long EODDate, _STL::map<_STL::string,long> &listOfSelectedEntities);
	_STL::string GetListOfAccountEntityId(_STL::map<_STL::string,long> &listOfSelectedEntities);
	bool DisplayEODResults(_STL::string accountEntityName, long EODDate);
	void ApplyBOEvent(long fundId, long EODDate, long BOKernelEventId, _STL::string BOStatusGroup);
	void UpdateSR(_STL::map<_STL::string,long> &listOfSelectedEntities, long EODDate, long BOKernelEventId,_STL::string BOStatusGroup);
	void FindClosedPositions(const CSRPortfolio * parentFolio);
	_STL::string GetBOStatusCondition(const _STL::string& statusGroup);

	_STL::string NumToDateDDMMYYY(long sphDate);

	_STL::map<_STL::string,long> fAccountEntityNameMap;
	_STL::map<_STL::string,long> fFundNameToIdMap;

	_STL::list<int> fInstrumentIds;
	_STL::list<long> fFundIds;
	_STL::map<int,int> fInsturmentAmounts;

	//------------------------------------ PRIVATE ------------------------------------
private:

	static char* __CLASS__;

};

class CSxAccountEntitiesEditList : public CSREditList
{
public:

	CSxAccountEntitiesEditList(CSRFitDialog* dialog, int nre);
	~CSxAccountEntitiesEditList();	


	enum Controls// already without ID_ITEM_SHIFT
	{		
		eFund,
		eLastEODDate,
		eNavFrequency,
		eColumnCount
	};
};

class CSxOKButton : public sophis::gui::CSRButton
{
public:	

	CSxOKButton(CSRFitDialog *dialog, int ERId_Element, _STL::string tooltip = "");

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


#endif // !__CSxAccountEODScenario__H__
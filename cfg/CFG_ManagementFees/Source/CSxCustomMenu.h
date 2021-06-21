#ifndef __CSxCustomMenu__H__
#define __CSxCustomMenu__H__

#include "SphInc/gui/SphMenu.h"
#include "SphInc/gui/SphCustomMenu.h"
#include "SphInc/gui/SphEditList.h"
#include "SphInc/gui/SphButton.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)



/** Virtual class to handle popup menus based on a list of integers
*/
class CSxCustomMenuLong : public sophis::gui::CSRCustomMenu
{
public:

	CSxCustomMenuLong(sophis::gui::CSRFitDialog	*dialog, int ERId_Menu, bool editable=true,
		NSREnums::eDataType	valueType=NSREnums::dShort,
		unsigned int valueSize=0, short listValue=1, const char *columnName=kUndefinedField);

	~CSxCustomMenuLong();

	void SetValueFromList();
	void SetListFromValue();


protected:
	_STL::vector<long> fData;	
	void BuildMenu();
};

class CSxNAVTypeMenu : public CSxCustomMenuLong
{
public:	

	CSxNAVTypeMenu(sophis::gui::CSRFitDialog	*dialog, int ERId_Menu, const char *columnName);		

protected:
	void BuildMenu();

};

class CSxEditListFees : public sophis::gui::CSREditList
{
public:
	CSxEditListFees(sophis::gui::CSRFitDialog* dialog, int nre, const char* tableName = kUndefinedTable);

	void OnAddElement();
	void OnRemoveElement();

	//Add Button for Allotement List
	class FeesAddButton : public sophis::gui::CSRButton
	{
	public:
		FeesAddButton(sophis::gui::CSRFitDialog* dialog, int nre, CSxEditListFees* feesList);
		void Action(); // override
	protected:
		CSxEditListFees* fFeesList;
	};

	//Remove button for allotement list
	class FeesRemoveButton : public sophis::gui::CSRButton
	{
	public:
		FeesRemoveButton(sophis::gui::CSRFitDialog* dialog, int nre, CSxEditListFees* feesList);
		void Action(); // override
	protected:
		CSxEditListFees* fFeesList;
	};

};

#endif // __CSxCustomMenu__H__
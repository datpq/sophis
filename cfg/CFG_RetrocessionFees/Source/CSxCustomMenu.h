#ifndef __CSxCustomMenu__H__
#define __CSxCustomMenu__H__

#include "SphInc/gui/SphMenu.h"
#include "SphInc/gui/SphCustomMenu.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)
#include __STL_INCLUDE_PATH(map)


/** Virtual class to handle popup menus based on a DB request
DB field must be of type VARCHAR2(40)
*/
class CSxCustomMenuStringQuery : public sophis::gui::CSRCustomMenu 
{
public:

	CSxCustomMenuStringQuery(sophis::gui::CSREditList *list, int CNb_Menu, _STL::string sqlQuery);

	~CSxCustomMenuStringQuery();

	void SetValueFromList();
	void SetListFromValue();	

	long GetCode(_STL::string);
	_STL::string GetVal(long code);

protected:
	_STL::vector<_STL::string> fData;
	_STL::map<_STL::string,long> fStr2CodeMap;
	_STL::map<long,_STL::string> fCode2StrMap;
	void BuildMenu();	
};


/** Popup menu Item containing all third parties.
To be used in a dialog.
*/
class CSxThirdPartyMenu : public CSxCustomMenuStringQuery 
{
public:
	CSxThirdPartyMenu(sophis::gui::CSREditList *list, int CNb_Menu);

};

/** Popup menu Item containing all internal funds.
To be used in a dialog.
*/
class CSxInternalFundMenu : public CSxCustomMenuStringQuery 
{
public:
	CSxInternalFundMenu(sophis::gui::CSREditList *list, int CNb_Menu);

};

/** Virtual class to handle popup menus based on a list of integers
*/
class CSxCustomMenuLong : public sophis::gui::CSRCustomMenu
{
public:

	CSxCustomMenuLong(sophis::gui::CSREditList *list, int CNb_Menu, bool editable=true,
		NSREnums::eDataType	valueType=NSREnums::dShort,
		unsigned int valueSize=0, short listValue=1, const char *columnName=kUndefinedField);

	~CSxCustomMenuLong();

	void SetValueFromList();
	void SetListFromValue();


protected:
	_STL::vector<long> fData;	
	void BuildMenu();
};

class CSxThirdTypeMenu : public CSxCustomMenuLong 
{
public:

	CSxThirdTypeMenu(sophis::gui::CSREditList *list, int CNb_Menu, const char *columnName);	

	enum eThirdType
	{
		eFundPromoter,
		eApporteurAffaire		
	};

	static _STL::string GetThirdTypeName(long thirdType);

protected:
	void BuildMenu();
};

class CSxComputationMethodMenu : public CSxCustomMenuLong
{
public:	

	CSxComputationMethodMenu(sophis::gui::CSREditList *list, int CNb_Menu, const char *columnName);

	enum eComputationMethods
	{
		eProrata,
		eAverageAM,
		eAverageCGR,
		eNoComputationMethod
	};

protected:
	void BuildMenu();

};

#endif // __CSxCustomMenu__H__
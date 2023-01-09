#include "CSxThirdPartyCreditCriterium.h"

#include "SphInc\portfolio\SphPosition.h"
#include "SphInc\portfolio\SphExtraction.h"
#include "SphInc\portfolio\SphPortfolio.h"
#include "SphInc\gui\SphCustomMenu.h"
#include "SphInc\backoffice_kernel\SphThirdParty.h"

#include "SphLLInc\interface\CustomMenu.h"

#include "SphTools\SphLoggerUtil.h"
#include "boost/algorithm/string.hpp"
#include <shlwapi.h>

using namespace sophis::backoffice_kernel;
using namespace sophis::portfolio;

CONSTRUCTOR_CRITERIUM(CSxThirdPartyCreditCriterium)

//-------------------------------------------------------------------------
class  ThirdPartyPropertyCriteriaElement : public sophis::gui::CSRCustomMenu 
{
public:
	ThirdPartyPropertyCriteriaElement(	sophis::gui::CSREditList	*list, int CNb_Menu, bool editable=true, const char *columnName=kUndefinedField);
	ThirdPartyPropertyCriteriaElement(	sophis::gui::CSRFitDialog *dlg, int CNb_Menu, bool editable=true, const char *columnName=kUndefinedField);

protected:
	void BuildMenu();
};

//-------------------------------------------------------------------------
ThirdPartyPropertyCriteriaElement::ThirdPartyPropertyCriteriaElement(sophis::gui::CSREditList *liste, int NRC_Menu, bool editable, const char* columnName) :
	CSRCustomMenu(liste, NRC_Menu, editable, NSREnums::dShort, 0, 1, columnName)
{
	BuildMenu();
}

//-------------------------------------------------------------------------
ThirdPartyPropertyCriteriaElement::ThirdPartyPropertyCriteriaElement(	sophis::gui::CSRFitDialog *dialogue, int NRC_Menu, bool editable, const char *columnName) :
	CSRCustomMenu(dialogue, NRC_Menu, editable,NSREnums::dShort)
{
	BuildMenu();
}

//-------------------------------------------------------------------------
void ThirdPartyPropertyCriteriaElement::BuildMenu()
{
	_STL::vector<_STL::string> items;
	items.push_back("YES");
	items.push_back("NO");
	items.push_back("N/A");
	CreateOrUpdateMenu(items,fMenuHandle);
}

//-------------------------------------------------------------------------
/*static*/ const char* CSxThirdPartyCreditCriterium::__CLASS__="CSxThirdPartyCreditCriterium";
/*static*/ const char* CSxThirdPartyCreditCriterium::_ThirdPartyProperty = MEDIO_THIRDPARTY_PROP_CREDIT;


//-------------------------------------------------------------------------
/*virtual*/ void CSxThirdPartyCreditCriterium::GetCode(SSReportingTrade* mvt, TCodeList &list)  const
{
	list[0].fType = 0;
	char* value = GetCounterPartyProperty(mvt->counterparty, _ThirdPartyProperty);
	if (strcmp(value, "YES") == 0)
		list[0].fCode = 1;
	else if(strcmp(value, "NO") == 0)
		list[0].fCode = 2;
	else 
		list[0].fCode = 3;
}

//-------------------------------------------------------------------------
/*virtual*/ void CSxThirdPartyCreditCriterium::GetName(long code, char* name, size_t size) const
{
	switch (code)
	{
		case 1:
			sprintf_s(name,256, "Yes");break;
		case 2:
			sprintf_s(name,256, "No");break;
		default: 
			sprintf_s(name,256, "N/A");
	}
	size = strlen(name);
}

//-------------------------------------------------------------------------
char* CSxThirdPartyCreditCriterium::GetCounterPartyProperty(long counterparty, const char* thirdPartyProperty) const
{
	char* res = "N/A";
	char buff[10];
	const CSRThirdParty* party = CSRThirdParty::GetCSRThirdParty(counterparty);
	if( party != NULL)
	{
		party->GetProperty(thirdPartyProperty, buff, 10);
		if(strcmp(buff, "") == 0)
			return res;
		else
		{
			boost::to_upper(buff);
			return buff;
		}
	}
	return res;
}

//-------------------------------------------------------------------------
/*virtual*/ sophis::gui::CSRElement* CSxThirdPartyCreditCriterium::new_FieldElement(int i, sophis::gui::CSREditList* l, int nre, bool editable, const char* nameDB) const
{
	return new ThirdPartyPropertyCriteriaElement(l,nre);
}
//-------------------------------------------------------------------------



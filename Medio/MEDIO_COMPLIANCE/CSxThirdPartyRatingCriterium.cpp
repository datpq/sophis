#include "CSxThirdPartyRatingCriterium.h"

#include "SphInc\portfolio\SphPosition.h"
#include "SphInc\portfolio\SphExtraction.h"
#include "SphInc\portfolio\SphPortfolio.h"
#include "SphInc\gui\SphCustomMenu.h"
#include "SphInc\backoffice_kernel\SphThirdParty.h"
#include "SphLLInc\interface\CustomMenu.h"
#include "SphTools\SphLoggerUtil.h"
#include "SphSDBCInc\tools\SphWrapperForMap.h"

#include "..\Tools/CSxSQLHelper.h"

using namespace sophis::backoffice_kernel;
using namespace sophis::portfolio;

CONSTRUCTOR_CRITERIUM(CSxThirdPartyRatingCriterium)

//-------------------------------------------------------------------------
class  ThirdPartyRatingCriteriaElement : public sophis::gui::CSRCustomMenu 
{
public:
	ThirdPartyRatingCriteriaElement(	sophis::gui::CSREditList	*list, int CNb_Menu, bool editable=true, const char *columnName=kUndefinedField);
	ThirdPartyRatingCriteriaElement(	sophis::gui::CSRFitDialog *dlg, int CNb_Menu, bool editable=true, const char *columnName=kUndefinedField);
	static ResultMapStrings GetAllRatings();

protected:
	void BuildMenu();
};

//-------------------------------------------------------------------------
ThirdPartyRatingCriteriaElement::ThirdPartyRatingCriteriaElement(sophis::gui::CSREditList *liste, int NRC_Menu, bool editable, const char* columnName) :
	CSRCustomMenu(liste, NRC_Menu, editable, NSREnums::dShort, 0, 1, columnName)
{
	BuildMenu();
}

//-------------------------------------------------------------------------
ThirdPartyRatingCriteriaElement::ThirdPartyRatingCriteriaElement(	sophis::gui::CSRFitDialog *dialogue, int NRC_Menu, bool editable, const char *columnName) :
	CSRCustomMenu(dialogue, NRC_Menu, editable,NSREnums::dShort)
{
	BuildMenu();
}

//-------------------------------------------------------------------------
void ThirdPartyRatingCriteriaElement::BuildMenu()
{
	_STL::vector<_STL::string> items;
	ResultMapStrings ratingVec = GetAllRatings(); 
	for(ResultMapStrings::iterator it = ratingVec.begin(); it != ratingVec.end(); ++it)
	{
		items.push_back(it->second);
	}
	CreateOrUpdateMenu(items,fMenuHandle);
}

//-------------------------------------------------------------------------
/*static*/ResultMapStrings  ThirdPartyRatingCriteriaElement::GetAllRatings()
{
	//TODO migration test and check query
	typedef CSRWrapperForMap<ResultMapStrings> Wrapper;
	Wrapper wrapper;
	ResultMapStrings map;
	CSRQueryBuffered<Wrapper> select;
	select << "select "
		<< OutOffset("distinct(value)", wrapper, wrapper.second.fName)
		<< OutOffset("rownum", wrapper, wrapper.first)
		<< "from tiersproperties where value is not null and name = '" << MEDIO_THIRDPARTY_PROP_RATING << "'";
	select.FetchAll(map);
	return map;
}

//-------------------------------------------------------------------------
/*static*/ const char* CSxThirdPartyRatingCriterium::__CLASS__="CSxThirdPartyRatingCriterium";
/*static*/ const char* CSxThirdPartyRatingCriterium::_ThirdPartyProperty = MEDIO_THIRDPARTY_PROP_RATING;
ResultMapStrings CSxThirdPartyRatingCriterium::_RatingVec = ThirdPartyRatingCriteriaElement::GetAllRatings();

//-------------------------------------------------------------------------

//-------------------------------------------------------------------------
/*virtual*/ void CSxThirdPartyRatingCriterium::GetCode( SSReportingTrade* mvt, TCodeList &list)  const
{
	list[0].fType = 0;
	list[0].fCode = 0;
	char* res = GetCounterPartyProperty(mvt->counterparty, _ThirdPartyProperty);

	for(ResultMapStrings::iterator it = _RatingVec.begin(); it != _RatingVec.end(); ++it)
	{
		if(strcmp(it->second.fName, res) == 0)
		{
			list[0].fCode = static_cast<long>(it->first);
			break;
		}
	}
}

//-------------------------------------------------------------------------
/*virtual*/ void CSxThirdPartyRatingCriterium::GetName(long code, char* name, size_t size) const
{
	if(code == 0)
		sprintf_s(name,256, "N/A");
	else
		sprintf_s(name,256, _RatingVec[code].fName);
	size = strlen(name);
}

//-------------------------------------------------------------------------
/*virtual*/ sophis::gui::CSRElement* CSxThirdPartyRatingCriterium::new_FieldElement(int i, sophis::gui::CSREditList* l, int nre, bool editable, const char* nameDB) const
{
	return new ThirdPartyRatingCriteriaElement(l,nre);
}

//-------------------------------------------------------------------------
char* CSxThirdPartyRatingCriterium::GetCounterPartyProperty(long counterparty, const char* thirdPartyProperty) const
{
	BEGIN_LOG("GetCounterPartyProperty");
	char res[10];
	const CSRThirdParty* party = CSRThirdParty::GetCSRThirdParty(counterparty);
	if( party != NULL)
	{
		party->GetProperty(thirdPartyProperty, res, 10);
		MESS(Log::debug,FROM_STREAM("Counterparty ="<<counterparty<<" value = "<<_STL::string(res)));
		return res;
	}
	return "N/A";
	END_LOG();
}
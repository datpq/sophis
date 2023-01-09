#ifndef __CSxThirdPartyRatingCriterium_H__
	#define __CSxThirdPartyRatingCriterium_H__

#include "CSxThirdPartyCreditCriterium.h"  
#include __STL_INCLUDE_PATH(set)
#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(cstring)
#include "../Tools/CSxSQLHelper.h"

//-------------------------------------------------------------------------
class CSxThirdPartyRatingCriterium : public sophis::portfolio::CSRCriterium
{
public:
	DECLARATION_CRITERIUM_WITH_CAPS(CSxThirdPartyRatingCriterium, true, false, false);
public:
	virtual void GetCode(sophis::portfolio::SSReportingTrade* mvt, TCodeList &list)  const override;
    virtual void GetName(long code, char* name, size_t size) const override;  
	virtual sophis::gui::CSRElement* new_FieldElement(int i, sophis::gui::CSREditList* l, int nre, bool editable, const char* nameDB) const override;

private:	
	char* GetCounterPartyProperty(long counterparty, const char* thirdPartyProperty) const;
	static const char*	_ThirdPartyProperty;
	static ResultMapStrings _RatingVec;
	static const char*	__CLASS__;
};
//-------------------------------------------------------------------------


#endif

#ifndef __CSxCounterpartyGroupCriterium_H__
	#define __CSxCounterpartyGroupCriterium_H__

#include "SphInc/portfolio/SphCriteria.h"  
#include "..\MediolanumConstants.h"
#include __STL_INCLUDE_PATH(set)
#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(cstring)
//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
class CSxCounterpartyGroupCriterium : public sophis::portfolio::CSRCriterium
{
public:
	DECLARATION_CRITERIUM_WITH_CAPS(CSxCounterpartyGroupCriterium, true, false, false);
	virtual void GetCode(sophis::portfolio::SSReportingTrade* mvt, TCodeList &list)  const override;
    virtual void GetName(long code, char* name, size_t size) const override;
	long GetGroup(long counterpartyId) const;
	//virtual sophis::gui::CSRElement* new_FieldElement(int i, sophis::gui::CSREditList* l, int nre, bool editable, const char* nameDB) const override;
	void FillGroupCache() const;

private:	
	static const char*	__CLASS__;
	

};
//-------------------------------------------------------------------------


#endif
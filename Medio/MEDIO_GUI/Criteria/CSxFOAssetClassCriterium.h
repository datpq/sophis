#ifndef __CSxFOAssetClassCriterium_H__
	#define __CSxFOAssetClassCriterium_H__


#include "SphInc/portfolio/SphCriteria.h"  
#include "..\MediolanumConstants.h"
#include __STL_INCLUDE_PATH(set)
#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(cstring)


class CSxFOAssetClassCriterium : public sophis::portfolio::CSRCriterium
{
public:

	DECLARATION_CRITERIUM_WITH_CAPS(CSxFOAssetClassCriterium, true, false, false);

	virtual void GetCode( SSReportingTrade* mvt, TCodeList &list)  const override;
    virtual void GetName(long code, char* name, size_t size) const override;
	virtual void GetCode(const sophis::instrument::ISRInstrument& instr, 
			                     const sophis::CSRComputationResults* results, 
			                     TCodeList& list) const override;


private:	
	const CSRCriterium* GetRankingCriterium() const;
	const bool IsFXInstrument(const sophis::instrument::CSRInstrument& inst) const;
	const CSRInstrument* GetOneEquityinstrument() const;
	mutable const CSRCriterium* fRankingCriterium;
	mutable const CSRInstrument* fOneEquityInstrument;
	static const char*	_ThirdPartyProperty;
	static const char*	__CLASS__;
};



#endif

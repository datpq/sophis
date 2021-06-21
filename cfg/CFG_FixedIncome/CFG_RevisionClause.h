#ifndef __CFG_RevisionClause_H__
#define __CFG_RevisionClause_H__

#include "SphInc/instrument/SphClause.h"
#include "SphInc/instrument/SphBond.h"

class CFG_RevisionClause : public sophis::instrument::CSRClause
{
public:
	DECLARATION_CLAUSE(CFG_RevisionClause);
	//virtual void GetBriefExplanation(eClausePosition position, 
	//									const _STL::string &clauseName,
	//									_STL::string& briefComment) const;
	//virtual bool GetLongExplanation(eClausePosition position,
	//								const _STL::string &clauseName,
	//								_STL::string& longComment) const;
	
	static double GetCorrespondingRate(const CSRBond& bond, const SSRedemption& r);
};


#endif // __CFG_RevisionClause_H__

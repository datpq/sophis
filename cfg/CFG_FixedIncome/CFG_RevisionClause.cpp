#pragma warning(disable:4251)
#include "CFG_RevisionClause.h"

CONSTRUCTOR_CLAUSE(CFG_RevisionClause, CSRClause);


double CFG_RevisionClause::GetCorrespondingRate(const CSRBond& bond, const SSRedemption& r)
{
	double val = bond.GetNotionalRate();

	int nbClauses = bond.GetClauseCountOfThisType("Revision");
	SSClause c;
	for (int i = 0; i < nbClauses; i++)
		if(bond.GetNthClauseOfThisType("Revision", i, &c))
			if( c.start_date <= r.startDate && c.value.value > 0.0)
				val = c.value.value;

	return val / 100.0;
};

//void CFG_RevisionClause::GetBriefExplanation(eClausePosition position, const _STL::string& clauseName, _STL::string& briefComment) const
//{
//	switch (position)
//	{
//	case eClauseName: // Clause Type
//		briefComment = FROM_STREAM(clauseName << ": N/A");
//		break;
//	case eBeginDate:	// Begin of Clause
//		briefComment = "Begin of Clause: N/A";
//		break;
//	case eEndDate:		// End of Clause
//		briefComment = "End of Clause: N/A";
//		break;
//	case ePaymentDate:	// Pay Date
//		briefComment = "Pay Date: N/A";
//		break;
//	case eMinimum:		// Minimum
//		briefComment = "Minimum: N/A";
//		break;
//	case eMaximum:		// Maximum
//		briefComment = "Maximum: N/A";
//		break;
//	case eValue:		// Value
//		briefComment = "Value: N/A";
//		break;
//	case eComment:		// Information
//		briefComment = "Information: N/A";
//		break;
//	}
//};
//
//bool CFG_RevisionClause::GetLongExplanation(eClausePosition position, const _STL::string& clauseName, _STL::string& longComment) const
//{
//	switch (position)
//	{
//	case eClauseName:
//	case eBeginDate:
//	case eEndDate:
//	case ePaymentDate:
//	case eMinimum:
//	case eMaximum:
//	case eValue:
//	case eComment:
//		longComment = "just for trial";
//		return true;
//	}
//	return false;
//};

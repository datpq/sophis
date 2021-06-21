/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"

// specific
#include "MandatoryCounterpart.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;

/*
** Statics
*/
/*static*/  const char* MandatoryCounterpart::__CLASS__ = "MandatoryCounterpart";


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool MandatoryCounterpart::VoteForCreation(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/
{
	//we check the existence of a counterpart
	if (transaction.GetCounterparty() > 0)
		return true;
	return false;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool MandatoryCounterpart::VoteForModification(const CSRTransaction & original, const CSRTransaction &transaction) const /*= 0*/
{
	if (transaction.GetCounterparty() > 0)
		return true;
	return false;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool MandatoryCounterpart::VoteForDeletion(const CSRTransaction &transaction) const /*= 0*/
{
	if (transaction.GetCounterparty() > 0)
		return true;
	return false;
}
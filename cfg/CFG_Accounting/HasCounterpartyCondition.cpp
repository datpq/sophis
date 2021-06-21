#pragma warning(disable:4251)
/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"

// specific
#include "HasCounterpartyCondition.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;

/*
** Statics
*/
/*static*/  const char* CSxHasCounterpartyCondition::__CLASS__ = "CSxHasCounterpartyCondition";


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxHasCounterpartyCondition::VoteForCreation(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForCreation");

	if (transaction.GetCounterparty() != 0)
	{
		END_LOG();
		return true;
	}

	END_LOG();
	return false;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxHasCounterpartyCondition::VoteForModification(const CSRTransaction & original, const CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForModification");
	END_LOG();
	return VoteForCreation(transaction);
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxHasCounterpartyCondition::VoteForDeletion(const CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForDeletion");
	END_LOG();
	return VoteForCreation(transaction);
}
/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc\backoffice_kernel\SphThirdParty.h"

// specific
#include "CSxIsNotSSBCondition.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;

/*
** Statics
*/
/*static*/  const char* CSxIsNotSSBCondition::__CLASS__ = "CSxIsNotSSBCondition";

bool CSxIsNotSSBCondition::checkCondition(const sophis::portfolio::CSRTransaction &transaction) const {
	BEGIN_LOG("checkCondition");
	long depository = transaction.GetDepositary();
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ", depository=" << depository << ")");
	bool ans = false;
	const CSRThirdParty* party = CSRThirdParty::GetCSRThirdParty(depository);
	if( party != NULL)
	{
		MESS(Log::debug, "name=" << party->GetName());
		ans = (party->GetName() != "State Street Milan");
	} else {
		MESS(Log::warning, "depository is not retrievable");
	}
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsNotSSBCondition::VoteForCreation(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForCreation");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsNotSSBCondition::VoteForModification(const CSRTransaction & original, const CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForModification");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsNotSSBCondition::VoteForDeletion(const CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForDeletion");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}
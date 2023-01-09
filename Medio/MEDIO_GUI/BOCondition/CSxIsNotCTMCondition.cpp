/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc\backoffice_kernel\SphThirdParty.h"

// specific
#include "CSxIsNotCTMCondition.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;

/*
** Statics
*/
/*static*/  const char* CSxIsNotCTMCondition::__CLASS__ = "CSxIsNotCTMCondition";

bool CSxIsNotCTMCondition::checkCondition(const sophis::portfolio::CSRTransaction &transaction) const {
	BEGIN_LOG("checkCondition");
	long depository = transaction.GetDepositary();
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ", depository=" << depository << ")");
	bool ans = false;
	const CSRThirdParty* party = CSRThirdParty::GetCSRThirdParty(depository);
	if( party != NULL)
	{
		char buff[10];
		party->GetProperty("Is CTM", buff, 10);
		MESS(Log::debug, "name=" << party->GetName() << ", Is CTM=" << buff);
		ans = (std::string(buff) != "Yes");
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
/*virtual*/ bool CSxIsNotCTMCondition::VoteForCreation(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForCreation");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsNotCTMCondition::VoteForModification(const CSRTransaction & original, const CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForModification");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsNotCTMCondition::VoteForDeletion(const CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForDeletion");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}
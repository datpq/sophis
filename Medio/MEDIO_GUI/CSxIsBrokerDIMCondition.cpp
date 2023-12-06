/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc\backoffice_kernel\SphThirdParty.h"
#include <SphInc/misc/ConfigurationFileWrapper.h>

// specific
#include "CSxIsBrokerDIMCondition.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;

/*
** Statics
*/
/*static*/  const char* CSxIsBrokerDIMCondition::__CLASS__ = "CSxIsBrokerDIMCondition";

bool CSxIsBrokerDIMCondition::checkCondition(const sophis::portfolio::CSRTransaction &transaction) const {
	BEGIN_LOG("checkCondition");
	long brokerId = transaction.GetBroker();
	long DIMGroupIdent = 0;
	ConfigurationFileWrapper::getEntryValue("CashAutomation", "DIMGroupIdent", DIMGroupIdent, 10004362);

	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ", Broker Id= " << brokerId << ")");
	bool res = false;
	const char * comment = transaction.GetComment();
	const CSRThirdParty* party = CSRThirdParty::GetCSRThirdParty(brokerId);
	if (party != NULL)
	{				
		long groupId=party->GetHolding();
		if (groupId == DIMGroupIdent && (strcmp(comment, "") != 0))
		{
			res = true;
		}	
	}
	else if (brokerId == 0 && (strcmp(comment, "") != 0))
		{	
		sophis::portfolio::CSRTransaction* trans = NULL;	
		long ident = atol(comment);
		if (ident != 0)
		{
			try
			{
				trans = CSRTransaction::newCSRTransaction(ident);
				if (trans != NULL)
				{
					res = true;
				}
			}
			catch (...)
			{

			}
			delete trans; 
			trans = 0;
		}
		}
	
	MESS(Log::debug, "End(result=" << res << ")");
	END_LOG();
	return res;
}

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsBrokerDIMCondition::VoteForCreation(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForCreation");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsBrokerDIMCondition::VoteForModification(const CSRTransaction & original, const CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForModification");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsBrokerDIMCondition::VoteForDeletion(const CSRTransaction &transaction) const /*= 0*/
{
	BEGIN_LOG("VoteForDeletion");
	MESS(Log::debug, "Begin(GetTransactionCode=" << transaction.GetTransactionCode() << ")");
	bool ans = checkCondition(transaction);
	MESS(Log::debug, "End(ans=" << ans << ")");
	END_LOG();
	return ans;
}
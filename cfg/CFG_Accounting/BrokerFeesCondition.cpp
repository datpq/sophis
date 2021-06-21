///////////////////////////////////////////////////////////////////////////////////////////
// BrokerFeesCondition.cpp
///////////////////////////////////////////////////////////////////////////////////////////


/*
** Includes
*/
#include "BrokerFeesCondition.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/backoffice_kernel/SphThirdParty.h"

/*
** Namespace
*/
using namespace sophis::backoffice_kernel;

/*
** Static 
*/
const char * CSxBrokerFeesCondition::__CLASS__ = "CSxBrokerFeesCondition";

//---------------------------------------------------------------------------------------------------------
//DPH
//CONSTRUCTOR_BROKERFEES_RULES_CONDITION_TRANSACTION("CSxBrokerFeesCondition");

//---------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxBrokerFeesCondition::get_condition( const sophis::portfolio::CSRTransaction & trade ) const
{
	BEGIN_LOG("get_condition");

	long counterpartyId = trade.GetCounterparty();
	if (!counterpartyId)
	{
		END_LOG();
		return false;
	}

	//DPH
	//CSRThirdParty * currentThirdparty = CSRThirdParty::GetCSRThirdParty(counterpartyId);
	const CSRThirdParty * currentThirdparty = CSRThirdParty::GetCSRThirdParty(counterpartyId);
	if (!currentThirdparty)
	{
		MESS(Log::debug, "Failed to find thirdparty " << counterpartyId);
		END_LOG();
		return false;
	}

	//if (currentThirdparty->GetSite() == 0 /* Internal */)
	if (currentThirdparty->IsEntity())
	{
		END_LOG();
		return true;
	}
	else
	{
		END_LOG();
		return false;
	}

	END_LOG();
	return false;
}
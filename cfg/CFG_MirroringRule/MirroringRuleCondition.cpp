/*
** Includes
*/
#include "MirroringRuleCondition.h"
#include "SphInc/backoffice_kernel/SphThirdParty.h"
#include "SphInc/backoffice_kernel/SphThirdPartyEnums.h"
#include "SphTools/SphLoggerUtil.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;

/*
** Static
*/
const char * MirroringRuleCondition::__CLASS__ = "MirroringRuleCondition";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool MirroringRuleCondition::GetCondition(const CSRTransaction& tr, const SSMirrorInstrumentSelector &sel) const /*= 0*/
{
	BEGIN_LOG("GetCondition");
	long counterpartyID = tr.GetCounterparty();
	if (counterpartyID == 0)
	{
		MESS(Log::debug, "No counterparty defined");
		END_LOG();
		return false;
	}

	const CSRThirdParty * pThird = CSRThirdParty::GetCSRThirdParty(counterpartyID);
	if (!pThird)
	{
		MESS(Log::error, "Failed to find counterparty " << counterpartyID << "");
		END_LOG();
		return false;
	}
			
	//if (pThird->GetSite() == 0 /* Internal */)
	if (pThird->IsEntity())
	{
		MESS(Log::debug, "Counterparty " << counterpartyID << " site is internal");
		END_LOG();
		return true;
	}
	else
	{
		MESS(Log::debug, "Counterparty " << counterpartyID << " site is not internal");
		END_LOG();
		return false;
	}

	MESS(Log::debug, "Unsupported case");
	END_LOG();
	return false;

}
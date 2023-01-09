#include "CSxBrokerFeesConditionSEDOL.h"
#include <SphSDBCInc/SphSqlQuery.h>
#include <SphTools/SphOStrStream.h>
#include <SphTools/SphLoggerUtil.h>
#include "SphInc\portfolio\SphTransaction.h"
#include "SphInc\instrument\SphInstrument.h"

/*
** Namespace
*/
using namespace sophis::backoffice_kernel;

const char * CSxBrokerFeesConditionSEDOL/*<classkey>*/::__CLASS__ = "CSxBrokerFeesConditionSEDOL";

//CONSTRUCTOR_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesConditionSEDOL)

/*virtual*/ /*inline*/  bool CSxBrokerFeesConditionSEDOL::get_condition(const sophis::portfolio::CSRTransaction & trade) const
{
	BEGIN_LOG("get_condition");
	const CSRInstrument * inst = trade.GetInstrument();
	if (!inst)
	{
		LOG(Log::debug, FROM_STREAM("Canont get the instrument"));
		return false;
	}

	long sicovam = inst->GetCode();
	SSComplexReference complexRef;
	//sprintf_s(complexRef.type, "SEDOL");
	//sprintf_s(complexRef.value, "");

	complexRef.type = "SEDOL";
	complexRef.value = "";

	if (inst->GetClientReference(sicovam, &complexRef))
	{
		LOG(Log::debug, FROM_STREAM("Instrument #" <<sicovam<< " SEDOL = " << std::string(complexRef.value)));
		
		char firstChar = complexRef.value[0];
		if (strcmp(&firstChar, "0") == 0 || strcmp(&firstChar, "3"))
		{
			LOG(Log::debug, FROM_STREAM("Return true"));
			return true;
		}
	}
	LOG(Log::debug, FROM_STREAM("Return false"));
	return false;
}


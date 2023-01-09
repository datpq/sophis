#include "CSxBrokerFeesConditionSettlePlace.h"
#include <SphTools/SphLoggerUtil.h>
#include "SphInc\portfolio\SphTransaction.h"
#include "SphInc\instrument\SphInstrument.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc\static_data\SphMarket.h"

_STL::map<int, _STL::string> CSxMapping::MyDico;

const char * CSxBrokerFeesConditionSettlePlace/*<classkey>*/::__CLASS__ = "CSxBrokerFeesConditionSettlePlace";

/*virtual*/ /*inline*/  bool CSxBrokerFeesConditionSettlePlace/*<classkey>*/::get_condition(const sophis::portfolio::CSRTransaction & trade) const
{
	BEGIN_LOG("get_condition");

	//int test=classkey;

	const CSRInstrument * inst = trade.GetInstrument();
	if (!inst)
	{
		MESS(Log::warning, "Could not get instrument");
		return false;
	}

	int market_code = inst->GetMarketCode();
	int ccy_code = inst->GetCurrencyCode();

	int nbRecords = 0;
	// 1. Build descriptor
	SSxSEDOL * result = NULL;
	CSRStructureDescriptor * gabSelect = new CSRStructureDescriptor(2, sizeof(SSxSEDOL));
	ADD(gabSelect, SSxSEDOL, fLibelle, rdfString)
	// 2. Write query
	std::string query = FROM_STREAM("select DOMICILE from tiers "
	<<"where REFERENCE in "
	<<"(select value from EXTRNL_REF_MARKET_VALUE "
	<< "where ref_ident = 5 and CURRENCY =" << ccy_code << " and market =" << market_code<<")");
	// 3. Execute the query
	errorCode err = CSRSqlQuery::StaticQueryWithNResults(query.c_str(), gabSelect, (void**)&result, &nbRecords);

	_STL::string className = CSxMapping::MyDico[classKey];
	for (int i = 0; i < nbRecords; i++)
	{
		if (strcmp(result[i].fLibelle, className.c_str()) == 0)
		{
			END_LOG();
			return true;
		}
	}
	END_LOG();
	return false;
}



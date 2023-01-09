#include "CSxBrokerFeesIsMAML.h"
#include <SphTools/SphLoggerUtil.h>
#include "SphInc\portfolio\SphTransaction.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "..\..\Tools\CSxSQLHelper.h"
/*
** Namespace
*/
using namespace sophis::backoffice_kernel;
using namespace sophis::portfolio;

const char * CSxBrokerFeesIsMAML/*<classkey>*/::__CLASS__ = "CSxBrokerFeesIsMAML";

// CONSTRUCTOR_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesIsMAML)


/*virtual*/ /*inline*/  bool CSxBrokerFeesIsMAML::get_condition(const sophis::portfolio::CSRTransaction & trade) const
{
	BEGIN_LOG("get_condition");
	bool res = false;
	const CSRPortfolio * folio = CSRPortfolio::GetCSRPortfolio(trade.GetFolioCode());

	if (nullptr != folio)
	{
		char fullName[255];
		folio->GetFullName(fullName);
		std::size_t found = std::string(fullName).find(CSxSQLHelper::GetTargetPortfolioName());//"MAML"); //TOCHANGE
		res = found != std::string::npos;
		LOG(Log::debug, FROM_STREAM("Is folio " << fullName << "an Medio folio? " << res));
	}

	END_LOG();
	return res;
}

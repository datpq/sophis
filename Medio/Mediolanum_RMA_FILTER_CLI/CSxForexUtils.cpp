
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/instrument/SphForexSpot.h"
#include "cache/CSRFutureAndPositionSettlementCachingInterface.h"

#include "CSxForexUtils.h"

CSxForexUtils::CSxForexUtils(void)
{
}


CSxForexUtils::~CSxForexUtils(void)
{
}

bool CSxForexUtils::IsFXPairReversed(long leftCcy, long rightCcy)
{
	const CSRForexSpot* fxpair = CSRForexSpot::GetCSRForexSpot(leftCcy, rightCcy);
	bool inverse = false;
	CSRForexSpot::GetForexInverse(1, leftCcy, rightCcy, &inverse);
	return inverse;
}


long CSxForexUtils::GetFWDInstrCode(long leftCcy, long rightCcy, long date)
{
	long sicoFWD = 0;
	const CSRForexSpot* originalFxSpot = CSRForexSpot::GetCSRForexSpot(leftCcy, rightCcy);
	if (originalFxSpot != nullptr)
	{

		long baseSicoTest = originalFxSpot->GetCode();
		 sicoFWD = sophis::static_data_service::CSRFutureAndPositionSettlementCachingInterface::GetInstrumentId(
			baseSicoTest, date, NULL, NULL, sophis::instrument::eForwardContractType::efctStandard);
	}

	return sicoFWD;
}

long CSxForexUtils::GetNDFInstrCode(long leftCcy, long rightCcy, long date)
{
	long sicoNdf = 0;
	const CSRForexSpot* originalFxSpot = CSRForexSpot::GetCSRForexSpot(leftCcy, rightCcy);
	if (originalFxSpot != nullptr)
	{
		long baseSicoTest = originalFxSpot->GetCode();
		 sicoNdf = sophis::static_data_service::CSRFutureAndPositionSettlementCachingInterface::GetInstrumentId(
			baseSicoTest, date, NULL, NULL, sophis::instrument::eForwardContractType::efctNonDeliverableForward);
	}
	return sicoNdf;
}
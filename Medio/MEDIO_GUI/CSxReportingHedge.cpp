#include "CSxReportingHedge.h"
#include "SphInc\SphMacros.h"
#include "SphTools\SphOStrStream.h"
#include "SphLLInc\portfolio\SphFolioStructures.h"
#include "SphInc\market_data\SphMarketData.h"
#include "PositionVisibility\CSxPositionVisibilityHook.h"


using namespace sophis::market_data;

bool CSxReportingHedge::FilterDealHook(const SSReportingTrade * trade, const CSRAccountingPrinciple * principle, portfolio::PSRExtraction extraction) const
{
	bool hidePos = false;

	long folioCode = trade->folio;
	if (CSxPositionVisibilityHook::_seeHedgeUserRight == false)
	{
		const CSRPortfolio * parentFolio = CSRPortfolio::GetCSRPortfolio(folioCode);
		if (parentFolio != nullptr)
		{
			std::string folName = parentFolio->GetName();
			if (CSxPositionVisibilityHook::_HedgeSet.find(folName) != CSxPositionVisibilityHook::_HedgeSet.end())
			{
				char instType = trade->instrumenttype;
				if (instType == iForexSpot || instType == iCommission || instType == iForexFuture || instType == iForexNonDeliverable)
				{
					hidePos = true;

				}
			}
		}
	}

	return hidePos;
}

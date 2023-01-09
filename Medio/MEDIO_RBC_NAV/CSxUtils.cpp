#include "CSxUtils.h"
#include "SphInc\value\kernel\SphFundPortfolio.h"

using namespace sophis::value;

long CSxUtils::GetStrategyOfPosition(const CSRPosition& position, PSRExtraction extraction)
{
	const CSRPortfolio* positionFolio = position.GetPortfolio();
	const CSAMPortfolio * amPositionFolio = dynamic_cast<const CSAMPortfolio *>(positionFolio);
	if(!amPositionFolio)
		return 0;
	const CSAMPortfolio* amMainStrategyFolio = amPositionFolio->GetStrategyPortfolioInMain();
	if(!amMainStrategyFolio)
		return 0;
	return amMainStrategyFolio->GetCode();	
}
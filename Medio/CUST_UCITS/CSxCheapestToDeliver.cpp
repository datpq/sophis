
/*
** Includes
*/
// specific
#include "SphInc/finance/SphNotionalFuture.h"
#include "CSxCheapestToDeliver.h"

using namespace sophis::portfolio;
using namespace sophis::finance;
using namespace std;

const char* CSxCheapestToDeliver::__CLASS__ = "CSxCheapestToDeliver";
// int CSxCheapestToDeliver::previousRefreshVersion_;


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxCheapestToDeliver, "Uncataloged")

//-------------------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxCheapestToDeliver::GetPositionCell(	const CSRPosition&	position,
														long				activePortfolioCode,
														long				portfolioCode,
														PSRExtraction		extraction,
														long				underlyingCode,
														long				instrumentCode,
														SSCellValue			*cellValue,
														SSCellStyle			*cellStyle,
														bool				onlyTheValue) const
{
	const CSRInstrument* inst = gApplicationContext->GetCSRInstrument(instrumentCode);
	const CSRNotionalFuture* future = NULL;
	if (inst && (future = dynamic_cast<const CSRNotionalFuture*>(inst)))
	{
		long cheapestId = future->GetCheapest();
		cellStyle->kind = NSREnums::dDouble;
		cellStyle->alignment = aRight;
		cellStyle->decimal = 4;
		cellValue->floatValue = CSRInstrument::GetLast(cheapestId);
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CSxCheapestToDeliver::GetDefaultWidth() const
{
	// TO DO
	return 60;
}


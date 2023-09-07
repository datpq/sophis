/*
** Includes
*/
// specific
#include "SphInc/finance/SphNotionalFuture.h"
#include "../MediolanumConstants.h"
#include "CSxCTDSicovam.h"

using namespace sophis::portfolio;
using namespace sophis::finance;
using namespace std;

const char* CSxCTDSicovam::__CLASS__ = "CSxCTDSicovam";


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxCTDSicovam, MEDIO_COLUMNGROP_TKT)

//-------------------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxCTDSicovam::GetPositionCell(const CSRPosition&	position,
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
		cellStyle->kind = NSREnums::dLong;
		cellStyle->alignment = aRight;
		cellStyle->decimal = 0;
		cellValue->integerValue = cheapestId;
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CSxCTDSicovam::GetDefaultWidth() const
{
	// TO DO
	return 60;
}


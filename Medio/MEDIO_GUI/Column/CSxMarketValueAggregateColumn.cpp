
/*
** Includes
*/
// specific
#include "CSxMarketValueAggregateColumn.h"
#include "../../MediolanumConstants.h"
#include "SphInc/portfolio/SphPortfolio.h"

#define COLUMN_STD_MARKET_VALUE					"Market Value"
#define COLUMN_TKT_MARKET_VALUE_CUSTOM			"Market Value Custom"

using namespace NSREnums;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxMarketValueAggregateColumn, MEDIO_COLUMNGROP_TKT)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxMarketValueAggregateColumn::GetPortfolioCell(	long				activePortfolioCode,
														long				portfolioCode,
														PSRExtraction		extraction,
														SSCellValue			*cellValue,
														SSCellStyle			*cellStyle,
														bool				onlyTheValue) const
{
	if (cellStyle)
	{
		cellStyle->kind = dDouble;
		cellStyle->alignment = aRight;
		cellStyle->decimal = 2;
		cellStyle->null = nvUndefined;
		cellStyle->canEdit = false;
	}
	SSCellValue cellValueMarketValue;
	const CSRPortfolioColumn* marketValue =	CSRPortfolioColumn::GetCSRPortfolioColumn(COLUMN_TKT_MARKET_VALUE_CUSTOM);
	if (!marketValue) return;
	marketValue->GetPortfolioCell(activePortfolioCode, portfolioCode, extraction, &cellValueMarketValue, cellStyle, true);
	cellValue->floatValue = cellValueMarketValue.floatValue;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxMarketValueAggregateColumn::GetUnderlyingCell(long				activePortfolioCode,
														long				portfolioCode,
														PSRExtraction		extraction,
														long				underlyingCode,
														SSCellValue			*cellValue,
														SSCellStyle			*cellStyle,
														bool				onlyTheValue) const
{
	// TO DO
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxMarketValueAggregateColumn::GetPositionCell(	const CSRPosition&	position,
														long				activePortfolioCode,
														long				portfolioCode,
														PSRExtraction		extraction,
														long				underlyingCode,
														long				instrumentCode,
														SSCellValue			*cellValue,
														SSCellStyle			*cellStyle,
														bool				onlyTheValue) const
{
	if (cellStyle)
	{
		cellStyle->kind = dDouble;
		cellStyle->alignment = aRight;
		cellStyle->decimal = 2;
		cellStyle->null = nvUndefined;
		cellStyle->canEdit = false;
	}
	PositionIdent positionIdentifier = position.GetIdentifier();
	// Handle Flat View
	if(positionIdentifier == 0) return;
	
	const CSRPortfolioColumn* marketValue =	CSRPortfolioColumn::GetCSRPortfolioColumn(COLUMN_TKT_MARKET_VALUE_CUSTOM);
	if (!marketValue) return;

	// Handle Virtual Position
	if (positionIdentifier < 0) 
	{
		// Check if the instrument is Cash
		const CSRInstrument * instrument = position.GetCSRInstrument();
		if (!instrument)
			return;
		char instrType = instrument->GetType();
		if(instrType == 'C') // Cash Instrument
		{
			SSCellValue cellValueMarketValue;
			marketValue->GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, &cellValueMarketValue, cellStyle, true);
			cellValue->floatValue += cellValueMarketValue.floatValue;
			return;
		}
	}

	// Handle Hierarchy View
	const CSRPortfolio* lPortfolio = CSRPortfolio::GetCSRPortfolio(portfolioCode, extraction);
	if(!lPortfolio)
		return;
	double aggregatedMarketValue = 0.0;
	int nbPositions = lPortfolio->GetTreeViewPositionCount();
	for(int i=0; i<nbPositions; ++i) 
	{
		const CSRPosition* lPosition = lPortfolio->GetNthTreeViewPosition(i);
		if(!lPosition)
			continue;
		long lInstrumentCode = lPosition->GetInstrumentCode();
		if(lInstrumentCode != instrumentCode) 
			continue;
		SSCellValue cellValueMarketValue;
		marketValue->GetPositionCell(*lPosition, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, &cellValueMarketValue, cellStyle, true);
		aggregatedMarketValue += cellValueMarketValue.floatValue;
	}
	cellValue->floatValue = aggregatedMarketValue;
}

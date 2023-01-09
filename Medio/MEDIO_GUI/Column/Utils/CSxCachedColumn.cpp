#include "CSxCachedColumn.h"
#include "SphInc\portfolio\SphPortfolio.h"
#include "SphTools\SphLoggerUtil.h"
#include "SphInc\static_data\SphCurrency.h"

#define EXTRACTIONKEY(extr_smartptr)			\
		((long*)extr_smartptr.get())	

using namespace sophis::static_data;
using namespace sophis::portfolio;

//-------------------------------------------------------------------------------------------------------------
bool SSxFolioLevelBuffer::operator <(const SSxFolioLevelBuffer &buf) const
{
	if (extraction < buf.extraction)
		return true;
	if (extraction > buf.extraction)
		return false;
	return portfolioCode < buf.portfolioCode;
}

//-------------------------------------------------------------------------------------------------
/*static*/ const char* CSxCachedColumn::__CLASS__ = "CSxCachedColumn";
//-------------------------------------------------------------------------------------------------------------
CSxCachedColumn::CSxCachedColumn(void)
{
	fBufferConsolidationVersion = fBufferCalculationVersion = 0;
	fFolioBuffer.clear();
	
	Initialize(fName, true, true, true, false, true);

}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxCachedColumn::GetPortfolioCell(long				activePortfolioCode,
	long				portfolioCode,
	PSRExtraction		extraction,
	SSCellValue			*cellValue,
	SSCellStyle			*cellStyle,
	bool				onlyTheValue) const
{
	BEGIN_LOG(__FUNCTION__);

	cellValue->floatValue = 0;

	long currencyCode = 0;
	const CSRPortfolio* folio = CSRPortfolio::GetCSRPortfolio(portfolioCode, extraction);

	if (folio == NULL || !folio->IsLoaded()) return;

	if (!onlyTheValue){
		if (folio){
			currencyCode = folio->GetCurrency();
		}
	}
	SetPortfolioStyle(cellStyle, currencyCode);
	if (folio && !folio->IsLoaded())
	{
		MESS(Log::verbose, "Folio " << portfolioCode << " in extraction " << EXTRACTIONKEY(extraction) << " has not been loaded yet. ");
		END_LOG();
	}
	cellStyle->style = tsBold;

	SSxFolioLevelBuffer	buf;
	buf.extraction = EXTRACTIONKEY(extraction);
	buf.portfolioCode = portfolioCode;
	buf.value = 0;
	buf.decimal = cellStyle->decimal;

	if (fBufferConsolidationVersion != CSRPortfolioColumn::GetRefreshVersion())
	{
		fBufferConsolidationVersion = CSRPortfolioColumn::GetRefreshVersion();
		FlushAll();
	}

	// IF IN CACHE...
	_STL::set<SSxFolioLevelBuffer>::const_iterator it;
	if ((it = fFolioBuffer.find(buf)) != fFolioBuffer.end())
	{
		cellStyle->decimal = it->decimal;
		cellValue->floatValue = it->value;
		MESS(Log::verbose, "Cache hit for folio " << portfolioCode << " in extraction " << extraction.get());
		END_LOG(); return;
	}

	// ELSE
	MESS(Log::debug, "Compute Value");

	// ... COMPUTE
	ComputePortfolioCell(activePortfolioCode,
		portfolioCode,
		extraction,
		cellValue,
		cellStyle,
		onlyTheValue);
	// SET IN CACHE
	buf.value = cellValue->floatValue;
	fFolioBuffer.insert(buf);

	END_LOG();
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxCachedColumn::GetPositionCell(const CSRPosition&	position,
	long				activePortfolioCode,
	long				portfolioCode,
	PSRExtraction		extraction,
	long				underlyingCode,
	long				instrumentCode,
	SSCellValue			*cellValue,
	SSCellStyle			*cellStyle,
	bool				onlyTheValue) const
{
	BEGIN_LOG(__FUNCTION__);

	cellValue->floatValue = 0;
	long currencyCode = position.GetCurrency();
	SetPositionStyle(cellStyle, currencyCode);

	if (fBufferConsolidationVersion != CSRPortfolioColumn::GetRefreshVersion())
	{
		fBufferConsolidationVersion = CSRPortfolioColumn::GetRefreshVersion();
		FlushAll();
	}

	SSxPositionValue posValue(0, cellStyle->decimal);

	// CHECK CACHE
	TExtractionIt extractionIt = fExtractionValueMap.find(EXTRACTIONKEY(extraction));
	if (extractionIt != fExtractionValueMap.end())
	{
		TFolioIt folioIt = extractionIt->second.find(portfolioCode);
		if (folioIt != extractionIt->second.end())
		{
			TPositionIt positionIt = folioIt->second.find(position.GetIdentifier());
			if (positionIt != folioIt->second.end())
			{
				cellStyle->decimal = positionIt->second.decimal;
				cellValue->floatValue = positionIt->second.value;

				MESS(Log::verbose, "Cache hit for position " << position.GetIdentifier() << " in portfolio " << portfolioCode << " in extraction " << EXTRACTIONKEY(extraction));
				END_LOG(); return;
			}
		}
	}

	// ELSE
	ComputePositionCell(position,
		activePortfolioCode,
		portfolioCode,
		extraction,
		underlyingCode,
		instrumentCode,
		cellValue,
		cellStyle,
		onlyTheValue);

	// SET IN CACHE
	posValue.value = cellValue->floatValue;
	fExtractionValueMap[EXTRACTIONKEY(extraction)][portfolioCode][position.GetIdentifier()] = posValue;
	//fPositionSetPerSicovam[instrumentCode].emplace(_STL::make_tuple(EXTRACTIONKEY(extraction), portfolioCode, position.GetIdentifier()));

	MESS(Log::verbose, " Value for position " << position.GetIdentifier() << " in portfolio " << portfolioCode << " in extraction " << EXTRACTIONKEY(extraction) << " is " << cellValue->floatValue);
	END_LOG();
}

void CSxCachedColumn::ComputePortfolioCell(long activePortfolioCode, long portfolioCode, PSRExtraction extraction, SSCellValue* cellValue, SSCellStyle* cellStyle, bool onlyTheValue) const
{
	
}


void CSxCachedColumn::ComputePositionCell(const CSRPosition& position, long activePortfolioCode, long portfolioCode, PSRExtraction extraction, long underlyingCode, long instrumentCode, SSCellValue* cellValue, SSCellStyle* cellStyle, bool onlyTheValue) const
{
	
}


void CSxCachedColumn::ConsolidateUnder(bool indicatorInCurrency, long activePortfolioCode, long portfolioCode, PSRExtraction extraction, SSCellValue* cellValue, SSCellStyle* cellStyle, bool onlyTheValue) const
{
	BEGIN_LOG(__FUNCTION__);

	const CSRPortfolio* amFolio = CSRPortfolio::GetCSRPortfolio(portfolioCode, extraction);
	if (!amFolio)
	{
		MESS(Log::warning, "Could not retrieve portfolio  " << portfolioCode << " in extraction " << EXTRACTIONKEY(extraction));
		; END_LOG(); return;
	}

	SSCellValue tempCellValue; SSCellStyle	tempCellStyle; double fx = 1;
	// ITERATE on SIBLING folio (folio underlying does not matter. Si minimize the number of calls and leverage on folio level buffer)
	for (int i = 0; i<amFolio->GetSiblingCount(); i++)
	{
		tempCellValue.floatValue = 0;
		fx = 1;

		const CSRPortfolio* childFolio = amFolio->GetNthSibling(i);
		if (!childFolio){ MESS(Log::error, "Could not retrieve " << i << "th portfolio under " << portfolioCode << " in extraction " << EXTRACTIONKEY(extraction)); continue; }

		GetPortfolioCell(activePortfolioCode,
			childFolio->GetCode(),
			extraction,
			&tempCellValue,
			&tempCellStyle,
			onlyTheValue);

		double fx = 1;
		if (indicatorInCurrency && childFolio->GetCurrency() != amFolio->GetCurrency())
		{
			fx = gApplicationContext->GetForex(childFolio->GetCurrency(), amFolio->GetCurrency());
			MESS(Log::debug, "FX from folio " << childFolio->GetCode() << " (in " << childFolio->GetCurrency() << "), to folio " << amFolio->GetCode() << " (in " << amFolio->GetCurrency() << ") in extraction " << EXTRACTIONKEY(extraction) << " is " << fx);
		}
		cellValue->floatValue += fx * tempCellValue.floatValue;
	}
	// ITERATE on TREE view position under it
	for (int j = 0; j<amFolio->GetTreeViewPositionCount(); j++)
	{
		tempCellValue.floatValue = 0;
		fx = 1;

		const CSRPosition* childPosition = amFolio->GetNthTreeViewPosition(j);
		if (!childPosition){ MESS(Log::error, "Could not retrieve " << j << "th tree view position under " << portfolioCode << " in extraction " << EXTRACTIONKEY(extraction)); continue; }

		GetPositionCell(*childPosition,
			activePortfolioCode,
			portfolioCode,
			extraction,
			/*underlyingCode*/amFolio->GetUnderlyingCode(),
			childPosition->GetInstrumentCode(),
			&tempCellValue,
			&tempCellStyle,
			onlyTheValue);


		double fx = 1;
		if (indicatorInCurrency && childPosition->GetCurrency() != amFolio->GetCurrency())
		{
			fx = gApplicationContext->GetForex(childPosition->GetCurrency(), amFolio->GetCurrency());
			MESS(Log::debug, "FX from childPosition " << childPosition->GetIdentifier() << " (in " << childPosition->GetCurrency() << "), to folio " << amFolio->GetCode() << " (in " << amFolio->GetCurrency() << ") in extraction " << EXTRACTIONKEY(extraction) << " is " << fx);
		}
		cellValue->floatValue += fx * tempCellValue.floatValue;
	}

	END_LOG();
}


void CSxCachedColumn::FlushAll() const
{
	fFolioBuffer.clear();
	fExtractionValueMap.clear();
	//fPositionSetPerSicovam.clear();
}


#pragma once
#include "SphInc/portfolio/SphPortfolioColumn.h"
#include "SphInc/portfolio/SphExtraction.h"
#include <SphInc/portfolio/SphPortfolioColumnCache.h>


class CSxColumnHelper
{
public:

	static SSCellValue GetPositionColumn(const CSRPosition& position, long portfolioCode, PSRExtraction extraction, std::string columnName);

	static SSCellValue GetPortfolioColumn(long portfolioCode, PSRExtraction extraction, std::string columnName);

	static const CSRPortfolioColumn* GetColumn(std::string columnName);

	static void SetStyleForDouble(SSCellStyle& cellStyle, long currencyCode, bool isDoubleInCurrency, short digit, bool bold); 

private: 
	static const char*	__CLASS__;
};


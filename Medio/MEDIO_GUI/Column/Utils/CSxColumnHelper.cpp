#include "CSxColumnHelper.h"
#include <SphTools/SphLoggerUtil.h>
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc\static_data\SphCurrency.h"

/*static*/ const char* CSxColumnHelper::__CLASS__="CSxColumnHelper";

//-------------------------------------------------------------------------------------------------------------
const CSRPortfolioColumn* CSxColumnHelper::GetColumn(std::string columnName)
{
	return CSRPortfolioColumn::GetCSRPortfolioColumn(columnName.c_str());
}

//-------------------------------------------------------------------------------------------------------------
SSCellValue CSxColumnHelper::GetPortfolioColumn(long portfolioCode, PSRExtraction extraction, std::string columnName)
{
	BEGIN_LOG("GetPortfolioColumn");
	SSCellValue value;
	SSCellStyle style;
	const CSRPortfolioColumn* column = GetColumn(columnName);
	if(column)
	{
		column->GetPortfolioCell(portfolioCode, portfolioCode, extraction, &value, &style, true);
	}
	else
	{
		MESS(Log::warning, "Cannot get column by name : "<<columnName);
	}
	END_LOG();
	return value;
}


//-------------------------------------------------------------------------------------------------------------
SSCellValue CSxColumnHelper::GetPositionColumn(const CSRPosition& position, long portfolioCode, PSRExtraction extraction, std::string columnName)
{
	BEGIN_LOG("GetPositionColumn");
	SSCellValue value;
	SSCellStyle style;
	const CSRPortfolioColumn* column = GetColumn(columnName);
	if(column)
	{
		column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
	}
	else
	{
		MESS(Log::warning, "Cannot get column by name : "<<columnName);
	}

	END_LOG();
	return value;
}


//-------------------------------------------------------------------------------------------------------------
void CSxColumnHelper::SetStyleForDouble(SSCellStyle& cellStyle, long currencyCode, bool isDoubleInCurrency, short digit, bool bold)
{
	cellStyle.alignment = sophis::gui::aRight;
	cellStyle.kind = NSREnums::dDouble;
	cellStyle.null = sophis::gui::nvZeroAndUndefined;
	cellStyle.decimal = digit;
	cellStyle.style = bold ? tsBold : tsNormal;
	cellStyle.currency = isDoubleInCurrency ? (currencyCode ? currencyCode : CSRPreference::GetCurrency())//certain extraction crit return a null currency
		: 0;
	if (cellStyle.currency)
	{
		const CSRCurrency* underlyingCurrency = CSRCurrency::CreateInstance(currencyCode);
		if (underlyingCurrency){
			underlyingCurrency->GetRGBColor(&cellStyle.color);
		}
	}
}



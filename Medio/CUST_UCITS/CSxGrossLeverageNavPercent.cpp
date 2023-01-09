#include "CSxGrossLeverageNavPercent.h"
/*
** Includes
*/
// specific
#include "SphInc/finance/SphNotionalFuture.h"
#include "SphInc/instrument/SphFuture.h"
#include "CSxGrossLeverageNavPercent.h"
#include "SphInc/value/kernel/SphFundPortfolio.h"
#include "SphInc/instrument/SphSwap.h"
#include "SphInc/instrument/SphIndexLeg.h"
#include "SphInc/instrument/SphForexFuture.h"
#include "../MediolanumConstants.h"


using namespace sophis::portfolio;
using namespace sophis::finance;
using namespace std;
using namespace sophis::value;


const char* CSxGrossLeverageNavPercent::__CLASS__ = "CSxGrossLeverageNavPercent";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxGrossLeverageNavPercent, MEDIO_COLUMNGROP_TKT)



	void	CSxGrossLeverageNavPercent::GetPortfolioCell(long activePortfolioCode, long portfolioCode, PSRExtraction extraction, SSCellValue* cellValue, SSCellStyle* cellStyle, bool onlyTheValue) const
{
	CSxCachedColumn::GetPortfolioCell(activePortfolioCode,
		portfolioCode,
		extraction,
		cellValue,
		cellStyle,
		onlyTheValue);

	cellStyle->decimal = 2;
}
/*virtual*/	void			CSxGrossLeverageNavPercent::ComputePortfolioCell(long				activePortfolioCode,
	long				portfolioCode,
	PSRExtraction		extraction,
	SSCellValue			*cellValue,
	SSCellStyle			*cellStyle,
	bool				onlyTheValue) const
{
	cellStyle->decimal = 2;
	ConsolidateUnder(false, activePortfolioCode, portfolioCode, extraction, cellValue, cellStyle, onlyTheValue);
	cellStyle->decimal = 2;
	
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxGrossLeverageNavPercent::ComputePositionCell(const CSRPosition&	position,
	long				activePortfolioCode,
	long				portfolioCode,
	PSRExtraction		extraction,
	long				underlyingCode,
	long				instrumentCode,
	SSCellValue			*cellValue,
	SSCellStyle			*cellStyle,
	bool				onlyTheValue) const
{

	cellStyle->kind = NSREnums::dDouble;
	cellStyle->alignment = aRight;
	cellStyle->decimal = 2;
	cellStyle->null = eNullValueType::nvZeroAndUndefined;

	double result = 0;
	SSCellValue value;
	SSCellStyle style;
	style.kind = NSREnums::dDouble;
	style.alignment = aRight;
	style.decimal = 2;
	style.null = eNullValueType::nvZeroAndUndefined;

	double nav = 0;
	double grossLeverageVal = 0;
	
	string columnFundNav = "Fund NAV";
	const CSRPortfolioColumn*column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnFundNav.c_str());
	if (column)
	{		
		column->GetPortfolioCell(activePortfolioCode, portfolioCode, extraction, &value, &style, true);
		nav = value.floatValue;
	}


	string columnGrossLeverage = "Gross Leverage curr. fund";
	const CSRPortfolioColumn*columnGross = CSRPortfolioColumn::GetCSRPortfolioColumn(columnGrossLeverage.c_str());
	if (columnGross)
	{
		columnGross->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
		grossLeverageVal = value.floatValue;
	}

	if (nav != 0)
	{
		result = grossLeverageVal * 100 / abs(nav);
	}
	
	cellValue->floatValue = result;

}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CSxGrossLeverageNavPercent::GetDefaultWidth() const
{
	// TO DO
	return 60;
}


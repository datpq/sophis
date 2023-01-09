/*
** Includes
*/
// specific
#include "SphInc/finance/SphNotionalFuture.h"
#include "SphInc/instrument/SphFuture.h"
#include "CSxGrossLeverageFundCcy.h"
#include "SphInc/value/kernel/SphFundPortfolio.h"
#include "SphInc/instrument/SphSwap.h"
#include "SphInc/instrument/SphIndexLeg.h"
#include "SphInc/instrument/SphForexFuture.h"
#include "../MediolanumConstants.h"


using namespace sophis::portfolio;
using namespace sophis::finance;
using namespace std;
using namespace sophis::value;


const char* CSxGrossLeverageFundCcy::__CLASS__ = "CSxGrossLeverageFundCcy";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxGrossLeverageFundCcy, MEDIO_COLUMNGROP_TKT)


/*virtual*/	void			CSxGrossLeverageFundCcy::ComputePortfolioCell(long				activePortfolioCode,
	long				portfolioCode,
	PSRExtraction		extraction,
	SSCellValue			*cellValue,
	SSCellStyle			*cellStyle,
	bool				onlyTheValue) const
{
	ConsolidateUnder(false, activePortfolioCode, portfolioCode, extraction, cellValue, cellStyle, onlyTheValue);
}



//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxGrossLeverageFundCcy::ComputePositionCell(const CSRPosition&	position,
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
		cellStyle->decimal = 0;
		cellStyle->null = eNullValueType::nvZeroAndUndefined;

		double result = 0;
		SSCellValue value;
		SSCellStyle style;
		double grossLeverageVal = 0;
		string columnGrossLeverage = "Gross Leverage";
		const CSRPortfolioColumn*column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnGrossLeverage.c_str());
		if (column)
		{
			column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
			grossLeverageVal = value.floatValue;
		}
		const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position.GetPortfolioCode(), position.GetExtraction());
		const CSAMPortfolio* fundFolio = NULL;

		long folioCcy = '\3EUR';
		if (folio && (fundFolio = folio->GetFundRootPortfolio()))
			folioCcy = fundFolio->GetCurrency();

		long ccyCode = folioCcy;
		
		const CSRPortfolioColumn*columnCcy = CSRPortfolioColumn::GetCSRPortfolioColumn("Currency");
		if (columnCcy)
		{
			columnCcy->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
			string ccyName = value.nullTerminatedString;
			 ccyCode = CSRCurrency::StringToCurrency(ccyName.c_str());
		}
			
			const CSRForexSpot* pFx = CSRForexSpot::GetCSRForexSpot(ccyCode, folioCcy);
			double fxRate = 1;
			if (pFx)
			{
				 fxRate = pFx->GetLast();
			}
		
		SetPositionStyle(cellStyle, folioCcy);
		result = fxRate * grossLeverageVal;
		cellValue->floatValue = result;
	
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CSxGrossLeverageFundCcy::GetDefaultWidth() const
{
	// TO DO
	return 60;
}



/*
** Includes
*/
// specific
#include "CsxIMPortfolioColumn.h"
#include "CsxBusinessEventReportingCallBack.h"
#include "SphInc/market_data/SphMarketData.h"
#include "../MediolanumConstants.h"

#include "SphInc\static_data\SphCurrency.h"
#include "SphInc/portfolio/SphPortfolio.h"

using namespace sophis::portfolio;
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CsxIMPortfolioColumn, MEDIO_COLUMNGROP_TKT)


void CsxIMPortfolioColumn::AggregateFolio(double& aggregateValue, const CSRPortfolio* folio, PSRExtraction extraction) const
{
	for(int i = 0; i < folio->GetSiblingCount(); i++)
	{
		double siblingValue = 0.0;
		const CSRPortfolio* folioSibling = folio->GetNthSibling(i);
		SSCellValue value;
		SSCellStyle style;
		//This will force to fill the cache at each folio level
		GetPortfolioCell(folioSibling->GetCode(), folioSibling->GetCode(), extraction, &value, &style, true);  
		aggregateValue += value.floatValue * CSRMarketData::GetCurrentMarketData()->GetForex(folioSibling->GetCurrency(), folio->GetCurrency());
	}

	for(int i = 0; i < folio->GetTreeViewPositionCount(); i++)
	{
		const CSRPosition* pos = folio->GetNthTreeViewPosition(i);
		auto p = pos->GetExtraColumn<SxDataDouble>(sophis::portfolio::GetCurrentReportingContext());
		double val = p ? p->data : CsxBusinessEventReportingColumnComputer().GetDefault().data;
		aggregateValue += val * CSRMarketData::GetCurrentMarketData()->GetForex(pos->GetCurrency(), folio->GetCurrency());
	}
}
void CsxIMPortfolioColumn::ComputePortfolioCell(const SSCellKey& key, SSCellValue* value, SSCellStyle* style) const
{
	value->floatValue = 0;
	style->kind = NSREnums::dDouble;
	style->null = nvZeroAndUndefined;

	const CSRPortfolio* folio = 0;
	if ((folio = CSRPortfolio::GetCSRPortfolio(key.PortfolioCode(), key.Extraction())) != 0)
	{
		long ccyFolio = folio->GetCurrency();
		const CSRCurrency* ccyObj = 0;
		if ((ccyObj = CSRCurrency::GetCSRCurrency(ccyFolio)) != 0)
		{
			gui::SSRgbColor color;
			ccyObj->GetRGBColor(&color);
			style->color = color;
		}
		style->alignment = aRight;
		style->style = tsBold;
		
		double valInFolioCcy = 0.0;
		AggregateFolio(valInFolioCcy, folio, key.Extraction());
		value->floatValue = valInFolioCcy;
	}
}
		

void CsxIMPortfolioColumn::ComputeUnderlyingCell(const SSCellKey& key, 
	SSCellValue			*value, 
	SSCellStyle			*style
) const
{
}
			
void CsxIMPortfolioColumn::ComputePositionCell(const SSCellKey& key, SSCellValue *value, SSCellStyle *style) const
{
	style->decimal = CSRPreference::GetNumberOfDecimalsForPrice();
	style->kind = NSREnums::dDouble;
	style->alignment = aRight;
	const CSRCurrency* ccyObj = 0;
	if ((ccyObj = CSRCurrency::GetCSRCurrency(key.InstrumentCurrency())) != 0)
	{
		gui::SSRgbColor color;
		ccyObj->GetRGBColor(&color);
		style->color = color;
	}
	auto p = key.Position()->GetExtraColumn<SxDataDouble>(sophis::portfolio::GetCurrentReportingContext());
	double val = p ? p->data : CsxBusinessEventReportingColumnComputer().GetDefault().data;
	//double val = CsxBusinessEventReportingColumnCallback::GetValue(key.Extraction(), key.PositionId(), key.PortfolioCode(), key.InstrumentCode(), (short)key.PositionType());
	value->floatValue = val;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CsxIMPortfolioColumn::GetDefaultWidth() const
{
	// TO DO
	return 60;
}

/*
** Includes
*/
// specific
#include "CSxPortfolioColumnRBCWeight.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc\value\kernel\SphFundPortfolio.h"
#include "SphTools/SphLoggerUtil.h"
#include "CSxUtils.h"
using namespace sophis::value;
/*
** Methods
*/ 
//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_PORTFOLIO_COLUMN(CSxPortfolioColumnRBCWeight)
const char* CSxPortfolioColumnRBCWeight::__CLASS__ = "CSxPortfolioColumnRBCWeight";
//-------------------------------------------------------------------------------------------------------------

CSxPortfolioColumnRBCWeight::CSxPortfolioColumnRBCWeight()
{
	fGroup = "MEDIO_COLUMNGROP_TKT";
	fName = RBC_Strategy_NAV_Column_Name;
	Initialize(fName, true, true, true, false, true);
}
//-------------------------------------------------------------------------------------------------------------
void CSxPortfolioColumnRBCWeight::ComputePortfolioCell(const SSCellKey& key, SSCellValue *cellValue, SSCellStyle *cellStyle) const
{
	BEGIN_LOG("GetPortfolioCell");
	cellValue->floatValue = 0.;
	if (key.Portfolio() == nullptr || !key.Portfolio()->IsLoaded())
		return;
	if (cellStyle != nullptr)
    {
		cellStyle->kind = NSREnums::dDouble;
		cellStyle->null = eNullValueType::nvZeroAndUndefined;
		cellStyle->decimal = 2;
    }
	const CSRPortfolio * folio = key.Portfolio();
	if(folio == nullptr)
		return;
	for(int i = 0; i < folio->GetFlatViewPositionCount(); i++)
	{
		const CSRPosition* position = folio->GetNthFlatViewPosition(i);
		if(!position)
			continue;
		SSCellValue posValue;
		SSCellStyle posStyle;
		GetPositionCell(*position,key.ActivePortfolioCode(),key.PortfolioCode(),key.Extraction(),0,position->GetInstrumentCode(),&posValue,&posStyle,true);
		cellValue->floatValue += posValue.floatValue;
	}
	END_LOG();
}

void CSxPortfolioColumnRBCWeight::ComputeUnderlyingCell(const SSCellKey& key, SSCellValue *cellValue, SSCellStyle *cellStyle) const
{
	// TO DO
}

//-------------------------------------------------------------------------------------------------------------
void CSxPortfolioColumnRBCWeight::ComputePositionCell(const SSCellKey& key, SSCellValue *cellValue, SSCellStyle *cellStyle) const
{
	BEGIN_LOG("GetPositionCell");
	cellValue->floatValue = 0.;
	if (key.Portfolio() == nullptr || !key.Portfolio()->IsLoaded())
		return;
	if (cellStyle != nullptr)
    {
		cellStyle->kind = NSREnums::dDouble;
		cellStyle->null = eNullValueType::nvZeroAndUndefined;
		cellStyle->decimal = 2;
    }

	//get RBC NAV of strategy
	const CSRPortfolioColumn* rbcNavColumn = CSRPortfolioColumn::GetCSRPortfolioColumn(RBC_Strategy_NAV_Column_Name);
	SSCellValue cellValueNAV;
	SSCellStyle cellStyleNAV;
	rbcNavColumn->GetPositionCell(*(key.Position()),key.ActivePortfolioCode(),key.PortfolioCode(),key.Extraction(),key.UnderlyingCode(),key.InstrumentCode(),&cellValueNAV,&cellStyleNAV,true);

	if(cellValueNAV.floatValue != 0)
	{
		//get Market Value of current position
		const CSRPortfolioColumn* marketValueColumn = CSRPortfolioColumn::GetCSRPortfolioColumn("Market Value");
		SSCellValue cellValueMV;
		SSCellStyle cellStyleMV;
		marketValueColumn->GetPositionCell(*(key.Position()), key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), key.UnderlyingCode(), key.InstrumentCode(), &cellValueMV,&cellStyleMV,true);

		//computing weight
		double fx = gApplicationContext->GetForex(key.Position()->GetCurrency(),cellStyleNAV.currency);
		cellValue->floatValue = cellValueMV.floatValue*fx/cellValueNAV.floatValue*100;
	}
	else
	{
		MESS(Log::debug,FROM_STREAM("RBC Strategy NAV for position " << key.Position()->GetIdentifier() << " is 0. RBC Weight will not be calculated"));
	}
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CSxPortfolioColumnRBCWeight::GetDefaultWidth() const
{
	// TO DO
	return 60;
}
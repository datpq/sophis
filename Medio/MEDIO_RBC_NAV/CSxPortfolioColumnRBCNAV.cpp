
/*
** Includes
*/
// specific
#include "CSxPortfolioColumnRBCNAV.h"
#include "SphSDBCInc/queries/SphQuery.h"
#include "SphInc\portfolio\SphPortfolio.h"
#include "SphInc\value\kernel\SphFundPortfolio.h"
#include "SphInc\instrument\SphEquity.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/static_data/SphCurrency.h"
#include "CSxUtils.h"
using namespace sophis::value;
using namespace sophisTools::base;
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_PORTFOLIO_COLUMN(CSxPortfolioColumnRBCNAV)
const char* CSxPortfolioColumnRBCNAV::__CLASS__="CSxPortfolioColumnRBCNAV";
//-------------------------------------------------------------------------------------------------------------

CSxPortfolioColumnRBCNAV::CSxPortfolioColumnRBCNAV()
{
	fGroup = "MEDIO_COLUMNGROP_TKT";
	fName = RBC_Strategy_NAV_Column_Name;
	Initialize(fName, true, true, true, false, true);
}
void CSxPortfolioColumnRBCNAV::ComputePortfolioCell(const SSCellKey& key, SSCellValue *cellValue, SSCellStyle *cellStyle) const
{
	BEGIN_LOG("GetPortfolioCell");
	cellValue->floatValue = 0;
	long equityCcy = 0;
	if (key.Portfolio() == nullptr || !key.Portfolio()->IsLoaded())
		return;

	const CSAMPortfolio * amFolio = dynamic_cast <const CSAMPortfolio * >(key.Portfolio());


	if(amFolio != nullptr && amFolio->IsAStrategy())
	{
		std::ostringstream oss;
		if(key.Extraction() != nullptr && key.Extraction()->isMain())
		{
			oss << amFolio->GetCode();
		}
		else
		{
			const CSAMPortfolio* amMainStrategyFolio = amFolio->GetStrategyPortfolioInMain();
			if(amMainStrategyFolio)
				oss << amMainStrategyFolio->GetCode();	
		}
		long equityId = CSRInstrument::GetCodeWithExternalTypeAndRef("STRATEGY",oss.str().c_str());
		if(equityId!=0)
		{
			const CSREquity *eq = dynamic_cast <const CSREquity * >(CSRInstrument::GetInstance(equityId));
			if(eq)
			{
				cellValue->floatValue = CSRInstrument::GetLast(equityId);
				equityCcy = eq->GetCurrency();
			}
			else
			{
				MESS(Log::debug,FROM_STREAM("The sicovam " << equityId << " associated with folio " << key.PortfolioCode() << " is not a share"));
				
			}
		}
		else
		{
			MESS(Log::debug,FROM_STREAM("No share has folio " << key.PortfolioCode() << " defined as 'STRATEGY' external reference"));
		}
	}
	else
	{
		MESS(Log::debug,FROM_STREAM("Folio " << key.PortfolioCode() << " is not a strategy"));
	}
	if (cellStyle != NULL)
    {
		cellStyle->kind = NSREnums::dDouble;
		cellStyle->null = eNullValueType::nvZeroAndUndefined;
		cellStyle->currency = equityCcy;
        const CSRCurrency * curr = CSRCurrency::GetCSRCurrency(equityCcy);
        if (curr)
            curr->GetRGBColor(&cellStyle->color);
    }
	END_LOG();
}

void CSxPortfolioColumnRBCNAV::ComputeUnderlyingCell(const SSCellKey& key, SSCellValue *cellValue, SSCellStyle *cellStyle) const
{
	// TO DO
}

//-------------------------------------------------------------------------------------------------------------
void CSxPortfolioColumnRBCNAV::ComputePositionCell(const SSCellKey& key, SSCellValue *cellValue, SSCellStyle *cellStyle) const
{
	BEGIN_LOG("GetPositionCell");
	cellValue->floatValue = 0.;
	if (key.Portfolio() == nullptr || !key.Portfolio()->IsLoaded())
		return;	
	if (cellStyle != NULL)
    {
		cellStyle->kind = NSREnums::dDouble;
		cellStyle->null = eNullValueType::nvZeroAndUndefined;
		cellStyle->decimal = 2;
    }
	long strategyId = CSxUtils::GetStrategyOfPosition(*(key.Position()),key.Extraction());
	const CSRPortfolio* strategyFolio = CSRPortfolio::GetCSRPortfolio(strategyId);
	if(strategyFolio)
	{
		//get RBC NAV of strategy
		SSCellValue cellValueNAV;
		SSCellStyle cellStyleNAV;
		GetPortfolioCell(key.ActivePortfolioCode(),strategyFolio->GetCode(),key.Extraction(),&cellValueNAV,&cellStyleNAV,false);
		cellValue->floatValue = cellValueNAV.floatValue;
	}
	else
	{
		MESS(Log::debug,FROM_STREAM("Position " << key.Position()->GetIdentifier() << " is not booked inside a strategy. RBC NAV = 0."));
	}
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CSxPortfolioColumnRBCNAV::GetDefaultWidth() const
{
	// TO DO
	return 60;
}


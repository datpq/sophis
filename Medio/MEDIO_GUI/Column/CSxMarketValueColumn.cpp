
/*
** Includes
*/
// specific
#include "CSxMarketValueColumn.h"
#include "../../MediolanumConstants.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "Column/Utils/CSxColumnHelper.h"
#include "SphInc/instrument/SphFuture.h"
#include <SphTools/SphLoggerUtil.h>
#include "SphInc/instrument/SphSwap.h"
#include <SphInc/instrument/SphForexFuture.h>
#include <SphInc/instrument/SphOption.h>
#include <boost/algorithm/string/predicate.hpp>
#include <boost/lexical_cast.hpp>
#include <boost/algorithm/string.hpp>
#include <cmath>
using namespace sophis::portfolio;

const char* InstrumentTypeColumn = MEDIO_COLUMN_STD_InstrumentType;
const char* MarketValueColumn = MEDIO_COLUMN_STD_MARKETVALUE;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxMarketValueColumn, MEDIO_COLUMNGROP_TKT)
/*static*/ const char* CSxMarketValueColumn::__CLASS__ = "CSxMarketValueColumn";

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxMarketValueColumn::ComputePortfolioCell(long activePortfolioCode, long portfolioCode, PSRExtraction extraction, SSCellValue* cellValue, SSCellStyle* cellStyle, bool onlyTheValue) const
{
	ConsolidateUnder(true, activePortfolioCode, portfolioCode, extraction, cellValue, cellStyle, onlyTheValue);
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxMarketValueColumn::ComputePositionCell(const CSRPosition& position, long activePortfolioCode, long portfolioCode, PSRExtraction extraction, long underlyingCode, long instrumentCode, SSCellValue* cellValue, SSCellStyle* cellStyle, bool onlyTheValue) const
{
	auto inst = position.GetCSRInstrument();
	if (inst)
	{
		SSCellValue instrumentType = CSxColumnHelper::GetPositionColumn(position, portfolioCode, extraction, InstrumentTypeColumn);
		const CSRFuture* pFuture = dynamic_cast<const CSRFuture*>(inst);
		const CSRSwap* pSwap = dynamic_cast<const CSRSwap*>(inst);
		const CSROption* pOption = dynamic_cast<const CSROption*>(inst);
		if (std::strcmp(instrumentType.nullTerminatedString, "Index Futures") == 0 || std::strcmp(instrumentType.nullTerminatedString, "Exchange Rate Futures") == 0 && pFuture)
		{
			double last = CSRInstrument::GetLast(inst->GetCode());
			double pointValue = pFuture->GetQuotityInProduct();
			//JIRA 569
			//cellValue->floatValue = last / pointValue * position.GetQuantityToBeProvisioned();
			cellValue->floatValue = last * pointValue * position.GetQuantityToBeProvisioned();
		}
		else if (std::strcmp(instrumentType.nullTerminatedString, "Interest Rate Futures") == 0 && pFuture)
		{
			double last = CSRInstrument::GetLast(inst->GetCode());
			char modelName[40];
			pFuture->GetModelName(modelName);
			if (boost::starts_with(std::string(modelName), "SFE")) // AUS bond futures 
			{
				last = pFuture->GetDirtyPriceByYTM(0 /*unused*/, 0 /*unused*/, 0 /*unused*/, last / 100,false) / 100;
			}
			cellValue->floatValue = last * pFuture->GetNotional() * position.GetQuantityToBeProvisioned() / 100;
		}
		else if(std::strcmp(instrumentType.nullTerminatedString, "Swaps") == 0 && pSwap)
		{
			cellValue->floatValue = position.GetDeltaCash();
		}
		else if (std::strcmp(instrumentType.nullTerminatedString, "Credit Default Swaps") == 0 && pSwap)
		{
			cellValue->floatValue = pSwap->GetNotional() * position.GetQuantityToBeProvisioned();
		}
		else if (pOption)
		{
			double last = inst->GetLast(instrumentCode);

			if (std::abs(last - NOTDEFINED) <= std::numeric_limits<double>::epsilon())
			{
				//No last defined, try to get the theo if it was already calculated by a F9
				CSRGlobalComputationKey key(inst->GetCode(), false); // position.GetMetaModelId());
				const CSRComputationResults* results = CSRComputationResults_Global::GetInstance().Get(key);
				if (results)
				{
					last = results->TheoreticalValue();
				}
			}
			cellValue->floatValue = position.GetQuantityToBeProvisioned() * last * pOption->GetQuotity();
		}
		else if (inst->GetType_API() == 'E' || inst->GetType_API() == 'K' || inst->GetType_API() == 'X')
		{
			const CSRForexSpot* fxSpot = dynamic_cast<const CSRForexSpot*>(inst);
			const CSRNonDeliverableForexForward* fxNDF = dynamic_cast<const CSRNonDeliverableForexForward*>(inst);
			const CSRForexFuture* fxFwd = dynamic_cast<const CSRForexFuture*>(inst); 
			long fx1(0), fx2(0); double fx(0.0);
			if (fxSpot)
			{
				/*Medio JIRA P2FOT-530
				fx1 = fxSpot->GetForex1();
				fx2 = fxSpot->GetForex2();
				fx = gApplicationContext->GetDayForex(fx1, fx2);*/
				fx = 0.0;
			}
			else if (fxFwd)
			{
				fx1 = fxFwd->GetExpiryCurrency();
				fx2 = fxFwd->GetCurrency();
				fx = gApplicationContext->GetDayForex(fx1, fx2);
			}
			else if (fxNDF)
			{
				fx1 = fxNDF->GetExpiryCurrency();
				fx2 = fxNDF->GetCurrency();
				fx = gApplicationContext->GetDayForex(fx1, fx2);
			}
			cellValue->floatValue = fx *position.GetQuantityToBeProvisioned();
		}
		else
		{
			SSCellValue marketValue = CSxColumnHelper::GetPositionColumn(position, portfolioCode, extraction, MarketValueColumn);
			cellValue->floatValue = marketValue.floatValue;
		}
	}
}



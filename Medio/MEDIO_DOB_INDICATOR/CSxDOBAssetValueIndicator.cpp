/*
** Includes
*/
#include "CSxDOBAssetValueIndicator.h"

#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/value/kernel/SphAMValuationTools.h"
#include "SphTools\SphLoggerUtil.h"
#include "SphInc/Value/kernel/SphAMOverloadedDialogs.h"
#include "SphInc\value\kernel\SphFundPortfolio.h"
#include "SphInc\portfolio\SphPortfolio.h"
#include "SphInc/portfolio/SphExtraction.h"
#include "SphInc/instrument/SphFuture.h"
#include "SphInc\instrument\SphForexSpot.h"
#include "SphInc\instrument\SphForexFuture.h"
#include "SphInc/instrument/SphOption.h"
#include "SphInc\portfolio\SphPosition.h"
#include "SphLLInc\portfolio\SphFolioStructures.h"
#include <boost/algorithm/string/predicate.hpp>
#include <boost/lexical_cast.hpp>
#include <boost/algorithm/string.hpp>
#include "SphInc/value/modelPortfolio/SphMPIndicator.h"
#include "SphInc/value/modelPortfolio/SphDistributionNodeKey.h"

using namespace sophis::portfolio;
using namespace sophis::market_data;
using namespace sophis::instrument;
using namespace sophis::value::benchmark;
using namespace sophis::tools;
using namespace sophis::value;
using namespace std;

#define IS_EQUAL_INTERNAL(A, B)		( (fabs((A) - (B)) < 1e-8) ? true : false)
//=========================================
//==       CSxDOBAssetValueIndicator       ==
//=========================================

//-------------------------------------------------------------------------------------------------------------
const char * CSxDOBAssetValueIndicator::fIndicatorName = "Asset Value Medio";
const char * CSxDOBAssetValueIndicator::__CLASS__ = "CSxDOBAssetValueIndicator";

DEFINITION_MP_INDICATOR(CSxDOBAssetValueIndicator, fIndicatorName);
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxDOBAssetValueIndicator::IsPercent() const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ double CSxDOBAssetValueIndicator::RecomputeExposure(double oldExposure, double oldReferenceExposure, double newReferenceExposure) const
{
	return oldExposure * oldReferenceExposure / newReferenceExposure;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ double CSxDOBAssetValueIndicator::RecomputePercentExposure(double oldExposure, double oldReferenceExposure, double newReferenceExposure) const
{
	return 0;
}
double CSxDOBAssetValueIndicator::GetUnitPrice(long instrumentCode) const
{
	sophis::DAL::ReportingPreference preference = CSRPreference::GetReportingPreferences();
	return CSAmValuationTools::GetUnitPrice(preference, instrumentCode);	 
}
//-------------------------------------------------------------------------------------------------------------
double CSxDOBAssetValueIndicator::GetUnitExposure(long instrumentCode, long referenceCurrency, long strategyFolioId) const
{
	BEGIN_LOG(__FUNCTION__);
	
	double forex(1);
	double unitPriceTemp(0.);
	if ( referenceCurrency > 0 )
	{
		const CSRInstrument* inst = CSRInstrument::GetInstance(instrumentCode);
		if ( !inst ) return GetUnitPrice(instrumentCode);

		forex = gApplicationContext->GetAggregationForex(inst->GetCurrency(), referenceCurrency);
		if(inst->GetType() == 'F') //Futures 
		{
			const CSRFuture* pFuture = dynamic_cast<const CSRFuture*>(inst);
			if (!pFuture || pFuture->GetUnderlyingCode()<0)
				return GetUnitPrice(instrumentCode);

			double last = inst->GetLast(instrumentCode);
			char modelName[40];
			pFuture->GetModelName(modelName);
			if (boost::starts_with(std::string(modelName), "SFE")) // AUS bond futures 
			{
				last = pFuture->GetDirtyPriceByYTM(0 /*unused*/, 0 /*unused*/, 0 /*unused*/, last/100,false)/100;
			}
			unitPriceTemp = last * pFuture->GetQuotityInProduct();
		}
		else if (inst->GetType() == 'D') // Options in general
		{
			const CSROption* pOption = dynamic_cast<const CSROption*>(inst);
			if (!pOption || pOption->GetUnderlyingCode()<0)
				return GetUnitPrice(instrumentCode);

			// OTC options will return 0, as they don't have a last
			unitPriceTemp = inst->GetLast(instrumentCode) * pOption->GetQuotity();
		}
		else if (inst->GetType() == 'E' || inst->GetType() == 'X' || inst->GetType() == 'K') // FX spot, FX forward and NDF 
		{
			const CSRForexSpot* fxSpot = dynamic_cast<const CSRForexSpot*>(inst);
			const CSRNonDeliverableForexForward* fxNDF = dynamic_cast<const CSRNonDeliverableForexForward*>(inst);
			const CSRForexFuture* fxFwd = dynamic_cast<const CSRForexFuture*>(inst); // FX Forward

			long fx1(0), fx2(0);
			if(fxSpot)
			{
				fx1 = fxSpot->GetForex1();
				fx2 = fxSpot->GetForex2();
				unitPriceTemp = gApplicationContext->GetAggregationForex(fx1, fx2);
			}
			else if (fxFwd)
			{
				fx2 = fxFwd->GetCurrency();
				fx1 = fxFwd->GetExpiryCurrency();
				unitPriceTemp = gApplicationContext->GetAggregationForex(fx1, fx2);
			}
			else if (fxNDF)
			{
				fx2 = fxNDF->GetCurrency();
				fx1 = fxNDF->GetExpiryCurrency();
				unitPriceTemp = gApplicationContext->GetAggregationForex(fx1, fx2);
			}
		}
		else
		{
			unitPriceTemp = GetUnitPrice(instrumentCode);
		}
	}
	else
	{
		unitPriceTemp = GetUnitPrice(instrumentCode);
	}
	double res = unitPriceTemp * forex;// *penceFactor;

	
	LOG(Log::debug, FROM_STREAM("instrumentCode #" << instrumentCode << "; unitPrice = " << unitPriceTemp << "; fx = " << forex << "; unit exposure = " << res));
	END_LOG();
	return res;
}
//-------------------------------------------------------------------------------------------------------------
double CSxDOBAssetValueIndicator::GetExposureUnitVariation(long instrumentCode, double referenceExposure, long referenceCurrency, long strategyFolioId) const
{
	return GetUnitExposure(instrumentCode,referenceCurrency,strategyFolioId);
}

//-------------------------------------------------------------------------------------------------------------
double CSxDOBAssetValueIndicator::ComputePortfolioExposure( PSRExtraction  extraction, const sophis::portfolio::CSRPortfolio* folio, double referenceExposure, long referenceCurrency, double investmentRatio, int positionFilter /*= pfKeepAll*/ ) const
{
	double res = 0.0;
	if ( !folio )
		return res;

	if(positionFilter == pfKeepAll)
	{
		// Faster.
		res = folio->GetAssetValue();
	}
	// IsPercent -> true
	// IsCurrency -> true
	// FX needed in aggregation.
	res =  ComputePortfolioExposureFromPositionsExposure(extraction, folio, referenceExposure, referenceCurrency, investmentRatio, positionFilter, true);

	return res;
}

//-------------------------------------------------------------------------------------------------------------
double CSxDOBAssetValueIndicator::ComputePositionExposure( PSRExtraction  extraction, const sophis::portfolio::CSRPosition* position, double referenceExposure, long referenceCurrency, double investmentRatio, int positionFilter /*= pfKeepAll*/ ) const
{
	BEGIN_LOG(__FUNCTION__);

	if ( !position )
		return 0.;

	double result = position->GetAssetValue();

	long sicovam = position->GetInstrumentCode();
	//double penceFactor(1.);
	const CSRInstrument* inst = CSRInstrument::GetInstance(sicovam);
	if(inst)
	{
		/*if (true == IsInPence(inst->GetSettlementCurrency()))
		{
			penceFactor = 0.01;
		}*/

		if (inst->GetType() == 'E' || inst->GetType() == 'X' || inst->GetType() == 'K')
		{
			const CSRForexSpot* fxSpot = dynamic_cast<const CSRForexSpot*>(inst);
			const CSRNonDeliverableForexForward* fxNdf = dynamic_cast<const CSRNonDeliverableForexForward*>(inst);
			const CSRForexFuture* fxFwd = dynamic_cast<const CSRForexFuture*>(inst);

			long fx1(0), fx2(0); double fx(0);
			if(fxSpot)
			{
				//Medio JIRA P2FOT-530
				if (positionFilter == pfKeepAllNonSimulatedOrder)
				{
					return 0.0;
				}
				
				fx1 = fxSpot->GetForex1();
				fx2 = fxSpot->GetForex2();
				fx = gApplicationContext->GetAggregationForex(fx1, fx2);
			}
			else if (fxFwd)
			{
				fx1 = fxFwd->GetCurrency();
				fx2 = fxFwd->GetExpiryCurrency();
				fx = gApplicationContext->GetAggregationForex(fx1, fx2);
			}
			else if (fxNdf)
			{
				fx1 = fxNdf->GetCurrency();
				fx2 = fxNdf->GetExpiryCurrency();
				fx = gApplicationContext->GetAggregationForex(fx1, fx2);
			}
			result = position->GetQuantityToBeProvisioned() * fx;
		}
	}
	if(positionFilter == pfKeepAll)
		return result;//penceFactor;

	// It is safer to remove the simulated orders AV from the initial AV than recompute the AV from scratch from the corrected number of securities.
	double simulatedOrdersNumberOfSecurities = CSMPIndicator::GetPositionNumberOfSecurities(*position, pfKeepSimulatedOrder);
	// SRQ-54077: for performance sake, do not compute a unit exposure if the number of securities is zero.
	double simulatedOrderPortion = 0;
	if(!IS_EQUAL_INTERNAL(simulatedOrdersNumberOfSecurities,0))
	{
		long iFolioIdInMain = position->GetPosition().real_folio;
		if (iFolioIdInMain == 0)
			iFolioIdInMain = position->GetPortfolioCode();

		// The unit exposure is in monetary unit but we need a result in thousands.
		simulatedOrderPortion = 0.001 * simulatedOrdersNumberOfSecurities * GetUnitExposure(	position->GetInstrumentCode(),
																								position->GetCurrency(),		// we need a unit exposure expressed in the position currency
																								iFolioIdInMain);
	}

	if(positionFilter == pfKeepAllNonSimulatedOrder)
	{
		double numberOfSecurities = CSMPIndicator::GetPositionNumberOfSecurities(*position, pfKeepAllNonSimulatedOrder);
		if (IS_EQUAL_INTERNAL(numberOfSecurities, 0))
			result = 0;
		else
			result = numberOfSecurities * GetUnitExposure(	position->GetInstrumentCode(),
															position->GetCurrency(),		// we need an unit exposure expressed in the position currency
															0) * 0.001;
		//result -= simulatedOrderPortion;
	}
		
	else if(positionFilter == pfKeepSimulatedOrder)
		result = simulatedOrderPortion;


	return result;
}

////-------------------------------------------------------------------------------------------------------------
bool CSxDOBAssetValueIndicator::GetNbDecimalForExposureInAmount(long intrumentId, int& nbDecimal) const
{
	nbDecimal = 0;
	return true;
}


////-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxDOBAssetValueIndicator::IsCurrency() const
{
	return true;
}
////-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxDOBAssetValueIndicator::CanBeRescaled() const
{
	return true;
}

_STL::string CSxDOBAssetValueIndicator::GetBenchmarkColumnName(const CSAmReferenceLevelData& refLvlData) const
{
	eReferenceLevel level = refLvlData.GetReferenceLevel();
	switch (level)
	{
	case eReferenceLevel::refLvlFund:
		return "Fund Benchmark Weights";
	case eReferenceLevel::refLvlStrategy:
		return "Strategy Benchmark Weights";
	default:
		return "";
	}
}







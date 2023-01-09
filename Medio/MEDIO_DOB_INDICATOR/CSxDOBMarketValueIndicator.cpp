/*
** Includes
*/
#include "CSxDOBMarketValueIndicator.h"

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
//==       CSxDOBMarketValueIndicator       ==
//=========================================

//-------------------------------------------------------------------------------------------------------------
const char * CSxDOBMarketValueIndicator::fIndicatorName = "Market Value Medio";
const char * CSxDOBMarketValueIndicator::__CLASS__ = "CSxDOBMarketValueIndicator";

DEFINITION_MP_INDICATOR(CSxDOBMarketValueIndicator, fIndicatorName);
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxDOBMarketValueIndicator::IsPercent() const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ double CSxDOBMarketValueIndicator::RecomputeExposure(double oldExposure, double oldReferenceExposure, double newReferenceExposure) const
{
	return oldExposure * oldReferenceExposure / newReferenceExposure;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ double CSxDOBMarketValueIndicator::RecomputePercentExposure(double oldExposure, double oldReferenceExposure, double newReferenceExposure) const
{
	return 0;
}
double CSxDOBMarketValueIndicator::GetUnitPrice(long instrumentCode) const
{
	sophis::DAL::ReportingPreference preference = CSRPreference::GetReportingPreferences();
	return CSAmValuationTools::GetUnitPrice(preference, instrumentCode);
}
//-------------------------------------------------------------------------------------------------------------
double CSxDOBMarketValueIndicator::GetUnitExposure(long instrumentCode, long referenceCurrency, long strategyFolioId) const
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
double CSxDOBMarketValueIndicator::GetExposureUnitVariation(long instrumentCode, double referenceExposure, long referenceCurrency, long strategyFolioId) const
{
	return GetUnitExposure(instrumentCode,referenceCurrency,strategyFolioId);
}

//-------------------------------------------------------------------------------------------------------------
double CSxDOBMarketValueIndicator::ComputePortfolioExposure(PSRExtraction  extraction, const sophis::portfolio::CSRPortfolio* folio, double referenceExposure, long referenceCurrency, double investmentRatio, int positionFilter /*= pfKeepAll*/) const
{
	BEGIN_LOG(__FUNCTION__);
	double res = 0.0;
	if ( !folio )
		return res;

	// ITERATE on SIBLING folio (folio underlying does not matter. Si minimize the number of calls and leverage on folio level buffer)
	for (int i = 0; i < folio->GetSiblingCount(); i++)
	{
		const CSRPortfolio* childFolio = folio->GetNthSibling(i);
		if (!childFolio) { MESS(Log::error, "Could not retrieve " << i << "th portfolio under " << folio->GetCode()); continue; }

		double childFolioRes = ComputePortfolioExposure(extraction, childFolio, referenceExposure, referenceCurrency, investmentRatio, positionFilter);

		double fx = 1;
		if (childFolio->GetCurrency() != folio->GetCurrency())
		{
			fx = gApplicationContext->GetForex(childFolio->GetCurrency(), folio->GetCurrency());
			MESS(Log::debug, "FX from folio " << childFolio->GetCode() << " (in " << childFolio->GetCurrency() << "), to folio " << folio->GetCode() << " (in " << folio->GetCurrency() << ") is " << fx);
		}
		res += fx * childFolioRes;
	}

	// ITERATE on TREE view position under it
	for (int j = 0; j < folio->GetTreeViewPositionCount(); j++)
	{
		const CSRPosition* childPosition = folio->GetNthTreeViewPosition(j);
		if (!childPosition) { MESS(Log::error, "Could not retrieve " << j << "th tree view position under " << folio->GetCode()); continue; }

		double childPositionRes = ComputePositionExposure(extraction, childPosition, referenceExposure, referenceCurrency, investmentRatio, positionFilter);

		double fx = 1;
		if (childPosition->GetCurrency() != folio->GetCurrency())
		{
			fx = gApplicationContext->GetForex(childPosition->GetCurrency(), folio->GetCurrency());
			MESS(Log::debug, "FX from childPosition " << childPosition->GetIdentifier() << " (in " << childPosition->GetCurrency() << "), to folio " << folio->GetCode() << " (in " << folio->GetCurrency() << ") is " << fx);
		}
		res += fx * childPositionRes;
	}

	END_LOG();
	return res;
}

//-------------------------------------------------------------------------------------------------------------
double CSxDOBMarketValueIndicator::ComputePositionExposure(PSRExtraction  extraction, const sophis::portfolio::CSRPosition* position, double referenceExposure, long referenceCurrency, double investmentRatio, int positionFilter /*= pfKeepAll*/) const
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

		MESS(Log::info, "GetPositionCell.BEGIN(GetType=" << inst->GetType() << ")");
		if (inst->GetType() == 'C') {//Cash
			MESS(Log::info, "GetPositionCell.BEGIN(sicovam=" << sicovam << ",GetIdentifier=" << position->GetIdentifier() << ", result=" << result << ")");
			const CSRPortfolioColumn* marketValue = CSRPortfolioColumn::GetCSRPortfolioColumn("Market Value Custom");
			if (marketValue) {
				SSCellStyle cellStyle;
				SSCellValue cellValueMarketValue;
				marketValue->GetPositionCell(*position, position->GetPortfolioCode(), position->GetPortfolioCode(), extraction, 0, sicovam, &cellValueMarketValue, &cellStyle, true);
				result = cellValueMarketValue.floatValue / 1000;
				MESS(Log::info, "GetPositionCell.END(sicovam=" << sicovam << ",GetIdentifier=" << position->GetIdentifier() << ", result=" << result << ")");
				return result;
			}
		}

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


	END_LOG();
	return result;
}

////-------------------------------------------------------------------------------------------------------------
bool CSxDOBMarketValueIndicator::GetNbDecimalForExposureInAmount(long intrumentId, int& nbDecimal) const
{
	nbDecimal = 0;
	return true;
}


////-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxDOBMarketValueIndicator::IsCurrency() const
{
	return true;
}
////-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxDOBMarketValueIndicator::CanBeRescaled() const
{
	return true;
}

_STL::string CSxDOBMarketValueIndicator::GetBenchmarkColumnName(const CSAmReferenceLevelData& refLvlData) const
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







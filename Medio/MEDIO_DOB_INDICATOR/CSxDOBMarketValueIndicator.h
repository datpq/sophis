#ifndef __CSxDOBMarketValueIndicator_H__
#define __CSxDOBMarketValueIndicator_H__


/*
** Includes
*/
#include "SphInc/value/modelPortfolio/SphMPIndicator.h"
#include "sphtools\sphloggerutil.h"
#include "SphInc\portfolio\SphPosition.h"
#include "SphInc/value/modelPortfolio/SphAmReferenceDenominator.h"

using namespace sophis::modelPortfolio;


/**
Class CSxDOBMarketValueIndicator:
*/
class CSxDOBMarketValueIndicator : public CSMPIndicator
{
//------------------------------------ PUBLIC ---------------------------------
public:

	static const char * __CLASS__;
	static const char * fIndicatorName;
	virtual bool IsPercent() const;
	virtual bool CanBeRescaled() const;
	virtual double RecomputeExposure(double oldExposure, double oldReferenceExposure, double newReferenceExposure) const override;
	virtual double RecomputePercentExposure(double oldPercentExposure, double oldReferenceExposure, double newReferenceExposure) const override;
	virtual bool IsCurrency() const;

	DECLARATION_MP_INDICATOR(CSxDOBMarketValueIndicator);

	virtual double GetUnitExposure(long instrumentCode, long referenceCurrency, long strategyFolioId) const override;
	virtual double GetExposureUnitVariation(long instrumentCode, double referenceExposure, long referenceCurrency, long strategyFolioId) const override;
	double GetUnitPrice(long instrumentCode) const;
	//virtual bool CanBeRescaled() const;
	//virtual bool IsCashIncludedInTotalExposure() const;

	virtual bool GetNbDecimalForExposureInAmount(long intrumentId, int& nbDecimal) const override;
protected:
	virtual double ComputePortfolioExposure(PSRExtraction  extraction,
		const sophis::portfolio::CSRPortfolio* folio,
		double referenceExposure,
		long referenceCurrency,
		double investmentRatio,
		int positionFilter) const override;

	virtual double ComputePositionExposure(PSRExtraction  extraction,
		const sophis::portfolio::CSRPosition* position,
		double referenceExposure,
		long referenceCurrency,
		double investmentRatio,
		int positionFilter) const override;


	virtual _STL::string GetBenchmarkColumnName(const CSAmReferenceLevelData& refLvlData) const override;


//------------------------------------ PRIVATE --------------------------------
};


#endif
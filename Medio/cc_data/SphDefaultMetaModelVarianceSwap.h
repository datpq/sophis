#pragma once

#ifndef __SphDefaultMetaModelVarianceSwap_H__
#define __SphDefaultMetaModelVarianceSwap_H__

#include "SphInc/SphMacros.h"
#include "VarianceSwap.h"
#include "SphInc/finance/SphDefaultMetaModelSwap.h"


namespace sophis
{
	namespace finance
	{
		class CSRHestonModel;
		class SOPHIS_FINANCE CSRDefaultMetaModelVarianceSwap : public CSRDefaultMetaModelSwap
		{
		public:
			DECLARATION_META_MODEL(CSRDefaultMetaModelVarianceSwap)
			virtual ~CSRDefaultMetaModelVarianceSwap();

			virtual void GetRiskSources(const instrument::CSRInstrument& instr, /*out*/ sophis::CSRComputationResults& rs, unsigned long bitFlag) const OVERRIDE;

			virtual double	GetLegTheoreticalValue(const instrument::CSRInstrument & swap, const market_data::CSRMarketData& context, int which) const;
			UNMASK_FUNCTION(CSRDefaultMetaModelSwap, GetTheoreticalValue);
			virtual double	GetTheoreticalValue(const instrument::CSRInstrument & instrument, const market_data::CSRMarketData &context) const;
			//virtual bool new_CashFlowDiagram(const instrument::CSRInstrument& instrument, const market_data::CSRMarketData& context, sophis::instrument::CSRCashFlowDiagram*& diag) const;
			virtual double	GetVega(const instrument::CSRInstrument & instrument, const market_data::CSRMarketData &context,int whichUnderlying) const;
			//virtual bool	IsAVegaUnderlying(const instrument::CSRInstrument& instrument, int whichUnderlying) const;
			virtual	double	GetEquityGlobalVega(const instrument::CSRInstrument & instrument, const sophis::CSRComputationResults& results) const OVERRIDE;
			virtual	double	GetEquityGlobalVega(const instrument::CSRInstrument & instrument, const market_data::CSRMarketData& context) const;
			virtual void	GetPriceDeltaGamma(const instrument::CSRInstrument & instrument, const market_data::CSRMarketData& context, double *price, double *delta, double *gamma, int whichUnderlying) const;
			virtual double	GetFirstDerivative(const instrument::CSRInstrument & instrument, const market_data::CSRMarketData &context, int whichUnderlying) const;
			virtual double	GetSecondDerivative(const instrument::CSRInstrument & instrument, const market_data::CSRMarketData &context, int which1, int which2) const;
			virtual void	SetComputationResults(sophis::CSRComputationResults& results) const;

			// Variance swap specific methods
			virtual	double GetVarianceLegValue(const CSRVarianceSwap& varSwap, const sophis::instrument::CSRIndexedLeg &leg, const market_data::CSRMarketData &context) const;
			virtual double	GetVolatilityCoupon(const CSRVarianceSwap& varSwap, long equityCode, long startDate, long endDate, double& expectation, const market_data::CSRMarketData& context) const;

			double GetTheoVarSwapPayoffCorrection(const CSRVarianceSwap& varSwap, long equityCode, double startDate, double endDate) const;
			virtual double ComputeNumberOfDaysInsideCorridor(const CSRVarianceSwap& varSwap, long equityCode, long startDate, long endDate, const market_data::CSRMarketData &context) const;
			virtual double GetRealizedVariance(const CSRVarianceSwap& varSwap, const market_data::CSRMarketData &context, long startDate, long endDate, double& expectation, double & lastFixing, bool &lastFixingFixed, long &histoCount, long& triggerCount, long underlying) const;
			virtual double GetInstantaneousVariance(const CSRVarianceSwap& varSwap, double spotNum, double spotDen, double splitNum, double splitDen, bool &triggered) const;

			virtual double GetIntegralCall(const CSRVarianceSwap& varSwap, long equityCode, double strikeMin, double forward, double startDate, double maturity, const market_data::CSRMarketData& context, double strikeMax = NOTDEFINED) const;
			virtual double GetIntegralPut(const CSRVarianceSwap& varSwap, long equityCode, double strikeMax, double forward, double startDate, double maturity, const market_data::CSRMarketData& context, double strikeMin = NOTDEFINED) const;
			double TimeIntegral(const CSRVarianceSwap& varSwap, long equityCode, long startDate, long endDate, const market_data::CSRMarketData& context, double lowerBarrier, double upperBarrier) const;
			virtual double GetAdjustment(const CSRVarianceSwap& varSwap, long equityCode, double barrierMin, double barrierMax, double forward0, double forward, double startDate, double maturity, const market_data::CSRMarketData&context) const;
			virtual double	GetDividendJumps(const CSRVarianceSwap& varSwap, const market_data::CSRMarketData &context, long equityCode, long startDate, long endDate) const;
			virtual double GetWeight(const CSRVarianceSwap& varSwap, int i, double strike, double strikeMoins, double strikePlus, double forward, bool callIntegral) const;
			virtual double	GetCashDividendJumpSize(const CSRVarianceSwap& varSwap, double cashDividend, double forward, double locVol, double maturity) const;

		protected:
			virtual void ComputeAllCore(sophis::instrument::CSRInstrument& instr, const sophis::market_data::CSRMarketData & param, sophis::CSRComputationResults& results) const OVERRIDE;
			mutable sophis::CSRComputationResults* fOptimResults;
			friend class sophis::finance::CSRVarianceSwap;
			friend class sophis::finance::CSRHestonModel;
		};
	}
}



#endif

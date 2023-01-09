// ***********************************************************************************************
//
// File:		VolatilitySwap.h
// 
// ***********************************************************************************************
#pragma once

#include "VarianceSwap.h"

namespace sophis	{
	namespace instrument {}
	namespace finance {

		enum volSwapIntegrationModel
		{
			eIMNoConvexAdj = 0,
			eIMFrizGatheral,
			eIMLogNormalVol
		};

		enum VolSwapDialogElements
		{
			eVolSwapIntegrationModel = 1,
			eVolSwapVolofVol,
			eVolSwapDenom,
			eVolCap,
			eVolSwapUserDenom, // 5
			eVolSwapIntraDayDelta,
			eVolSwapWithDividends,
			eVolSwapSplitEditList,
			eVolSwapFixingColumn,
			eVolSwapSmileIntegration, // 10
			eVolSwapSmileIntegrationLabel,
			eVolSwapVolofVolLabel,
			eVolSwapResult_Realized,
			eVolSwapResult_Future,
			eVolSwapResult_OneDay, // 15
			eVolSwapResult_ConvexityAdj,
			eVolSwapResult_Total,
			eVolSwapResult_Factor,
			eVolSwapFixegLegTheo,
			eVolSwapMDE, // 20
			eVolSwapLast
		};

		class SOPHIS_FINANCE CSRVolatilitySwap	: public virtual sophis::finance::CSRVarianceSwap
		{
			DECLARATION_SWAP(CSRVolatilitySwap)

			// if reconstruct is true, variables are initialized from the dialog
			CSRVolatilitySwap(SW_Donne** sw, bool initialiseRecomputeAll, bool reconstruct);

		protected:
			void Initialize(SW_Donne** sw, bool initialiseRecomputeAll, bool reconstruct);

		public:
			virtual const sophis::finance::CSRMetaModel * GetDefaultMetaModel() const;

			volSwapIntegrationModel GetIntegrationModel() const;
			double GetVolofVol() const;

			virtual double	GetTicketCoupon(const flux_jambe &ptrFlux, int which) const;
			double			GetExpectedSquareRootOfFutureVariance() const;
			void			SetExpectedSquareRootOfFutureVariance(const double& val);
		
			//void			SetUseVolSwapDividendJumpSize(bool useVol);

			virtual	bool	SplitDone(long splitDate, long underlyingCode, double factor) const;

			//virtual void	GetCalculationData(sophis::tools::CSRArchive & archive) const;
			//virtual void	SetCalculationData (const sophis::tools::CSRArchive & archive);

			//virtual double	GetCashDividendJumpSize(double	cashDividend,
			//										double	forward,
			//										double  locVol,
			//										double	maturity) const;

		//protected:
			virtual void	AddMDEDate(long mdeDate);

			virtual void	AddMDEFixing(long mdeDate, double newFixing, long underlyingCode = 0);

			// loads the calculation data from the CSRVarianceSwapDialog popup
			void   Construct();

			//virtual	double GetVarianceLegValue(const sophis::instrument::CSRIndexedLeg &leg, const market_data::CSRMarketData &context) const;

			// returns E[sqrt[realizedVariance+futureVariance]]
			//virtual double	GetExpectedSquareRootOfVariance(long								equityCode,
			//												long								startDate,
			//												long								endDate,
			//												const market_data::CSRMarketData	&context,
			//												double								realizedVariance,
			//												double								&expectedFutureVariance) const;

			//double GetFrizGatheralDirac(long								equityCode,
			//							double								forward,
			//							double  							startDate,
			//							double								maturity,
			//							const market_data::CSRMarketData	&context,
			//							bool								is_put) const;

			//virtual double GetWeight(	int		i,
			//							double strike,
			//							double strikeMoins,
			//							double strikePlus,
			//							double forward,
			//							bool   callIntegral) const;

		//protected:

			volSwapIntegrationModel fIntegrationModel;
			double					fVolofVol;

			//mutable bool			fUseVolSwapJumps;
		private:
			//DEPRECATED_RESULTS double			GetExpectedSquareRootOfVariance() const;
			//DEPRECATED_RESULTS void			SetExpectedSquareRootOfVariance(const double& val) const;
			//
			//DEPRECATED_RESULTS mutable double			fExpectedSquareRootOfVariance;
			//DEPRECATED_RESULTS  mutable double			fExpectedSquareRootOfFutureVariance;
		};

		class SOPHIS_FINANCE CSRVolatilitySwapDialog: public sophis::gui::CSRInstrumentDialog
		{
		public:
			DECLARATION_INSTRUMENT_DIALOG(CSRVolatilitySwapDialog)

			virtual void	OpenAfterInit(void);
			virtual	void	ElementValidation(int EAId_Modified);

			virtual void	BeforeRecompute(const instrument::CSRInstrument &security);
			virtual void	AfterRecompute(const instrument::CSRInstrument &security, const sophis::CSRComputationResults& results);
		};

	}
}
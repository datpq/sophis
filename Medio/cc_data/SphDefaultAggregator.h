#pragma once

/**
* ----------------------------------------------------------------------------
* Sophis Technology 
* Copyright (c) 2009
* File : DefaultAggregator.h
* Creation : June 30 2009 by Silviu Marele
* 
* ----------------------------------------------------------------------------
*/
#ifndef __SPHDEFAULTAGGREGATOR_H__
#define __SPHDEFAULTAGGREGATOR_H__

/*
** System includes
*/
#include "SphTools/base/CommonOS.h"
#include "SphTools/base/UnsafeRefCount.h"
#include "SphInc/SphMacros.h"

/*
** Application includes
*/

#include "..\cc_data/SphExecutionAggregator.h"

/*
** defines
*/

/*
** typedef and classes
*/

SPH_PROLOG

#if (defined( WIN32 ) || defined( _WIN64 ))
#	pragma warning(push)
#	pragma warning(disable:4275) // Can not export a class derivated from a non exported one
#	pragma warning(disable:4251) // Can not export a class agregating a non exported one
#endif

namespace sophis
{
	namespace execution
	{
		class SOPHIS_EXECUTION CSRDefaultExecutionAggregator : public sophis::execution::CSRExecutionAggregator
		{
			DECLARATION_PROTOTYPE(CSRDefaultExecutionAggregator, sophis::execution::CSRExecutionAggregator)
		public:

			//---------------------------------------------------------------------
			void getTradesToAggregateOn(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList)
				throw (sophisTools::base::ExceptionBase);

			bool aggregate(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList, tools::CSREventVector& context)
				throw (sophisTools::base::ExceptionBase);

			void deAggregate(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList, tools::CSREventVector& context)
				throw (sophisTools::base::ExceptionBase);
			
			const char* GetName() const;
			//---------------------------------------------------------------------

		private:
			void getFXSwapTransactionsForAggregation(sophis::portfolio::CSRTransactionRefCount** transactions, long transactionsSize, 
				sophis::portfolio::CSRTransactionRefCount** transactionsFarLeg, long transactionsFarLegSize, sophis::portfolio::TradeList& tradeList)
				throw (sophisTools::base::ExceptionBase);

			long getFXSpotForwardTransactionForAggregation(const sophis::execution::CSRExecutionAllocation& allocation, 
				sophis::portfolio::CSRTransactionRefCount** transactions, long transactionsSize)
				throw (sophisTools::base::ExceptionBase);

			bool aggregateLeg(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::CSRTransactionRefCount& trade, 
				tools::CSREventVector& context, LegType leg)
				throw (sophisTools::base::ExceptionBase);

			void deAggregateLeg(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::CSRTransactionRefCount& trade, 
				tools::CSREventVector& context, LegType leg)
				throw (sophisTools::base::ExceptionBase);

			
		public:
			/**
			 * Key of the default aggregator.
			 */
			static const char* DefaultAggregatorKey;

		private:
			static const char* __CLASS__;
		};
	}
}

#if (defined( WIN32 ) || defined( _WIN64 ))
#	pragma warning(pop)
#endif


SPH_EPILOG

#endif //__SPHDEFAULTAGGREGATOR_H__

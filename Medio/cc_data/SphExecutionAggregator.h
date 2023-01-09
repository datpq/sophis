#pragma once

#ifndef _SphExecutionAggregator_h_
#define _SphExecutionAggregator_h_

#include "SphInc/SphMacros.h"
#include "SphInc/tools/SphAlgorithm.h"
#include "../cc_data/SphExecution.h"
#include "SphTools/SphExceptions.h"
#include "../cc_data/SphPrototype.h"

SPH_PROLOG
namespace sophis
{
	namespace tools
	{
		class CSREventVector;
	}

	namespace execution
	{
		class SOPHIS_EXECUTION CSRExecutionAggregator
		{
		public:
			/**
			 * Look for the trades to aggregate the given execution on
			 * Most of the times there will be only one trade found. The only case when there are 2 is the FXSwaps
			 */
			virtual void getTradesToAggregateOn(const CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList)
				throw (sophisTools::base::ExceptionBase) = 0;

			/**
			 * Aggregate an external execution on the given sophis trade
			 * @param externalExec (in) is the execution to aggregate
			 * @param trade (in/out) is the sophis trade to aggregate on
			 * returns true if succes
			 * An exception is thrown in case of severe error. In this case the Executions Manager abandons the aggregation of this execution.
			 */
			virtual bool aggregate(const CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList,
								   tools::CSREventVector& context)
				throw (sophisTools::base::ExceptionBase) = 0;

			/**
			 * Deaggregate an execution from the given sophis trade
			 * @param externalExec (in) is the execution to deaggregate
			 * @param trade (in/out) is the sophis trade to deaggregate from
			 */
			virtual void deAggregate(const CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList,
								   tools::CSREventVector& context)
				throw (sophisTools::base::ExceptionBase) = 0;

			/**
			 * Retrieve the display name of the execution aggregator.
			 * This name is displayed in the GUI and should not be stored in database.
			 */
			virtual const char* GetName() const = 0;

			//===========================================================================================

			// CSRExecutionAggregator is also a prototype for normal use
			virtual CSRExecutionAggregator* Clone() const = 0;
			static CSRExecutionAggregator* get_ExecutionAggregator(const char* name);
			typedef tools::CSRPrototype<CSRExecutionAggregator, const char*, tools::less_char_star> prototype;
			static prototype& GetPrototype();

		private:
			static const char* __CLASS__;
		};
	}
}
SPH_EPILOG
#endif //_SphExecutionAggregator_h_
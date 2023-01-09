#pragma once
#ifndef __CSxExecutionsAggregatorCounterparty_H__
#define __CSxExecutionsAggregatorCounterparty_H__

//#include "../cc_data/SphDefaultAggregator.h"
#include "SphLLInc\execution\SphDefaultAggregator.h"



class CSxExecutionsAggregatorCounterparty : public sophis::execution::CSRDefaultExecutionAggregator
{
	DECLARATION_PROTOTYPE(CSxExecutionsAggregatorCounterparty, CSxExecutionsAggregatorCounterparty)

	public:

		struct FxSwapLegs
		{
			long nearLeg;
			long farLeg;
		};
	//---------------------------------------------------------------------
	void getTradesToAggregateOn(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList)
			throw (sophisTools::base::ExceptionBase);

	bool aggregate(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList, tools::CSREventVector& context)
		throw (sophisTools::base::ExceptionBase);

	void deAggregate(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList, tools::CSREventVector& context)
		throw (sophisTools::base::ExceptionBase);

	const char* GetName() const;

	void getFxSwapList(const std::string& sRefconsToLookFor, std::list<FxSwapLegs>& foundFxSwapList);

	void getFxSwapLegs(const std::string& sRefconsToLookFor, std::list<long>& foundFxSwapLegs);
	//---------------------------------------------------------------------

	//---------------------------------------------------------------------

private:
	void getFXSwapTransactionsForAggregation(sophis::portfolio::CSRTransactionRefCount** transactions, long transactionsSize,
		sophis::portfolio::CSRTransactionRefCount** transactionsFarLeg, long transactionsFarLegSize, sophis::portfolio::TradeList& tradeList)
		throw (sophisTools::base::ExceptionBase);

	long getFXSpotForwardTransactionForAggregation(const sophis::execution::CSRExecutionAllocation& allocation,
		sophis::portfolio::CSRTransactionRefCount** transactions, long transactionsSize)
		throw (sophisTools::base::ExceptionBase);

public:
	/**
	* Key of the aggregator.
	*/
	static const char* DefaultAggregatorKey;

private:
	static const char* __CLASS__;
};

#endif //!__CSxExecutionsAggregatorCounterparty_H__
#include "CSxExecutionsAggregatorCounterparty.h"
// #include "..\cc_data/ExternalExecutionDBGetter.h"
#include "SphTools/logger/LoggerUtil.h"
#include "SphSDBCInc/queries/SphQueryBuffered.h"

const char * CSxExecutionsAggregatorCounterparty::__CLASS__ = "CSxExecutionsAggregatorCounterparty";
const char * CSxExecutionsAggregatorCounterparty::DefaultAggregatorKey = "DeAggregate by counterparty";

typedef _STL::map<sophis::portfolio::TransactionIdent, long> RefconToIdxTradesMap;

/*virtual*/  void CSxExecutionsAggregatorCounterparty::getTradesToAggregateOn(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& resultTradeList)
throw (sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("getTradesToAggregateOn");

	// Generate execution allocation
	execution::CSRExecutionAllocationHandle duplicate = new execution::CSRExecutionAllocation(allocation); // Duplicate because createCSRTransaction is not const
	TradeList tradeList;
	duplicate->createCSRTransactions(tradeList, false);

	if (tradeList.size() == 0)
	{
		_STL::string errMsg = "Failed to obtain trades from execution allocation";
		MESS(Log::error, errMsg);
		throw sophisTools::base::GeneralException(errMsg.c_str());
	}

	TradeList::const_iterator iterTrades = tradeList.begin();
	const CSRTransactionHandle allocatedTransac = *iterTrades;
	if (!allocatedTransac)
	{
		_STL::string errMsg = "Failed to retrieve execution allocation";
		MESS(Log::error, errMsg);
		throw sophisTools::base::GeneralException(errMsg.c_str());
	}
	CSRTransactionHandle farLegAllocatedTransac = NULL;
	iterTrades++;
	if (iterTrades != tradeList.end())
		farLegAllocatedTransac = *iterTrades;

	// Retrieve external order reference
	size_t orefLen = 0;
	const char* oref = allocatedTransac->GetOrderReference();
	if (oref != NULL)
		orefLen = strlen(oref);

	TMvtFull* internalStructure = allocatedTransac->GetTransaction();
	TMvtFull* internalStructureFarLeg = NULL;

	// Allocation side
	_STL::string sideClause = "";
	if (internalStructure->quantity > 0) sideClause = " AND (quantite > 0)";
	else if (internalStructure->quantity < 0) sideClause = " AND (quantite < 0)";

	bool isFXSwap = allocation.isFxSwap();
	if (isFXSwap)
	{
		if (!farLegAllocatedTransac)
		{
			_STL::string errMsg = FROM_STREAM("Far leg transaction can not be created for a FWSwap. Can not aggregate.");
			MESS(Log::error, errMsg);
			throw sophisTools::base::GeneralException(errMsg.c_str());
		}
		internalStructureFarLeg = farLegAllocatedTransac->GetTransaction();
	}

	const CSRInstrument* instrument = CSRInstrument::GetInstance(internalStructure->code_sico);
	if (instrument == NULL)
	{
		_STL::string errMsg = FROM_STREAM("Instrument with code " << internalStructure->code_sico << " not found in execution. Can not aggregate.");
		MESS(Log::error, errMsg);
		throw sophisTools::base::GeneralException(errMsg.c_str());
	}
	char type = instrument->GetType();
	bool isForex = false;
	switch (type)
	{
	case iForexFuture:
	case iForexNonDeliverable:
	case iForexSpot:
		isForex = true;
		break;
	}

	_STL::string sqlCondition;
	if (oref && orefLen > 0)
	{
		sqlCondition = FROM_STREAM(
			"(cmpt_ordre = '" << oref << "')"
			<< " AND (sicovam = " << internalStructure->code_sico << ")"
			<< " AND (opcvm = " << internalStructure->code_opcvm << ")"
			<< " AND (devisepay = " << internalStructure->devisePay << ")"
			<< " AND (contrepartie = " << internalStructure->contrepartie << ")"
			<< " AND (dateneg-TO_DATE('01/01/1904','DD/MM/YYYY') = " << internalStructure->date_neg << ")"
			<< sideClause);
	}
	else if (internalStructure->sophis_order_id)
	{
		sqlCondition = FROM_STREAM(
			"(sophis_order_id = " << internalStructure->sophis_order_id << ")"
			<< " AND (sicovam = " << internalStructure->code_sico << ")"
			<< " AND (opcvm = " << internalStructure->code_opcvm << ")"
			<< " AND (devisepay = " << internalStructure->devisePay << ")"
			<< " AND (contrepartie = " << internalStructure->contrepartie << ")"
			<< " AND (dateneg-TO_DATE('01/01/1904','DD/MM/YYYY') = " << internalStructure->date_neg << ")"
			<< sideClause);
	}
	else
	{
		sqlCondition = FROM_STREAM(
			"(sicovam = " << internalStructure->code_sico << ")"
			<< " AND (opcvm = " << internalStructure->code_opcvm << ")"
			<< " AND (devisepay = " << internalStructure->devisePay << ")"
			<< " AND (contrepartie = " << internalStructure->contrepartie << ")"
			<< " AND (sophis_order_id = 0)"
			<< " AND (cmpt_ordre is null)"
			<< " AND (dateneg-TO_DATE('01/01/1904','DD/MM/YYYY') = " << internalStructure->date_neg << ")"
			<< sideClause);
	}

	const char* table = NULL;
	char destinationTable[40];
	if (sophis::misc::eecNoError == allocation.fFields.Get(tagDestinationTable, destinationTable))
	{
		MESS(Log::debug, "DestinationTable='" << destinationTable << "'");
		table = destinationTable;
	}

	_STL::string sqlConditionFarLeg;
	if (isFXSwap)
	{
		sqlConditionFarLeg = sqlCondition + FROM_STREAM(" AND (dateval-TO_DATE('01/01/1904','DD/MM/YYYY') = " << internalStructureFarLeg->date_val << ")");
	}
	sqlCondition = sqlCondition + FROM_STREAM(" AND (dateval-TO_DATE('01/01/1904','DD/MM/YYYY') = " << internalStructure->date_val << ")");

	//SRQ-41631
	if (!isFXSwap && !isForex)
		sqlCondition = sqlCondition + " AND rownum < 2";

	long size = 0;
	CSRTransactionRefCount** transactions = CSRTransactionRefCount::newCSRTransactionArray(sqlCondition.c_str(), &size, table);

	// Normally, we should get at most one corresponding aggregated trade.
	// In case we get more, we aggregate with the first one of the list.
	CSRTransactionHandle ret = NULL;
	if (size > 0)
	{
		if (isFXSwap)
		{
			long sizeFarLegs = 0;
			CSRTransactionRefCount** transactionsFarLeg = CSRTransactionRefCount::newCSRTransactionArray(sqlConditionFarLeg.c_str(), &sizeFarLegs, table);

			getFXSwapTransactionsForAggregation(transactions, size, transactionsFarLeg, sizeFarLegs, resultTradeList);

			for (size_t i = 0; i<(size_t)sizeFarLegs; ++i)
				delete transactionsFarLeg[i];
			delete transactionsFarLeg;
		}
		else if (isForex)
		{
			//If FxSpot or FxForward, prevent the aggregation on a FxSwap leg
			long i = getFXSpotForwardTransactionForAggregation(allocation, transactions, size);
			if (i >= 0)
			{
				MESS(Log::debug, "Fx trade " << transactions[i]->GetTransactionCode() << " selected for aggregation.");
				ret = transactions[i];
				resultTradeList.push_back(ret);

				// don't delete it
				transactions[i] = NULL;
			}
		}
		else
		{
			// Found at least one : we retain only the first one and merge.
			MESS(Log::debug, "Trade " << transactions[0]->GetTransactionCode() << " selected for aggregation.");
			ret = transactions[0];
			resultTradeList.push_back(ret);

			// don't delete first one
			transactions[0] = NULL;
		}

		for (size_t i = 0; i<(size_t)size; ++i)
			delete transactions[i];
		delete transactions;
	} // End if.

	END_LOG();
}

//-----------------------------------------------------------------------------------------------------------
void CSxExecutionsAggregatorCounterparty::getFXSwapTransactionsForAggregation(CSRTransactionRefCount** transactions, long transactionsSize,
	CSRTransactionRefCount** transactionsFarLeg, long transactionsFarLegSize,
	sophis::portfolio::TradeList& tradeList)
	throw (sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("getFXSwapTransactionsForAggregation");

	RefconToIdxTradesMap refconFarLegsToIdxTradesMap;
	for (long j = 0; j<transactionsFarLegSize; j++)
		refconFarLegsToIdxTradesMap.insert(_STL::make_pair(transactionsFarLeg[j]->getInternalCode(), j));

	//Select all the found trades which are first leg in a FxSwap
	//work on blocks of size 100
	const long blockSize = 100;
	for (long first = 0; first < transactionsSize; first = first + blockSize)
	{
		long last = first + blockSize - 1;
		if (last >= transactionsSize)
			last = transactionsSize - 1;

		_STL::string sRefconsToLookFor;
		RefconToIdxTradesMap refconToIdxTradesMap;
		for (long i = first; i <= last; i++)
		{
			portfolio::TransactionIdent refcon = transactions[i]->getInternalCode();
			if (i == first)
			{
				sRefconsToLookFor = sRefconsToLookFor + FROM_STREAM(refcon);
			}
			else
			{
				sRefconsToLookFor = sRefconsToLookFor + FROM_STREAM("," << refcon);
			}
			refconToIdxTradesMap.insert(_STL::make_pair(refcon, i));
		}
		
		_STL::list<FxSwapLegs> foundFxSwapList;
		 getFxSwapList(sRefconsToLookFor, foundFxSwapList);
		for (_STL::list<FxSwapLegs>::const_iterator iterFoundFxSwap = foundFxSwapList.begin(); iterFoundFxSwap != foundFxSwapList.end(); iterFoundFxSwap++)
		{
			RefconToIdxTradesMap::const_iterator iterTradeFarLeg = refconFarLegsToIdxTradesMap.find(iterFoundFxSwap->farLeg);
			if (iterTradeFarLeg != refconFarLegsToIdxTradesMap.end())
			{
				//We found our FxSwap
				RefconToIdxTradesMap::const_iterator iterTradeNearLeg = refconToIdxTradesMap.find(iterFoundFxSwap->nearLeg);
				if (iterTradeNearLeg != refconToIdxTradesMap.end())
				{
					tradeList.push_back(transactions[iterTradeNearLeg->second]);
					tradeList.push_back(transactionsFarLeg[iterTradeFarLeg->second]);
					transactions[iterTradeNearLeg->second] = NULL;
					transactionsFarLeg[iterTradeFarLeg->second] = NULL;

					MESS(Log::verbose, "Found the swap to aggregate the execution on : (" << iterFoundFxSwap->nearLeg << "," << iterFoundFxSwap->farLeg << ")");
					END_LOG();
					return;
				}
			}
		}
	}

	END_LOG();
}

//-----------------------------------------------------------------------------------------------------------
long CSxExecutionsAggregatorCounterparty::getFXSpotForwardTransactionForAggregation(const execution::CSRExecutionAllocation& allocation,
	CSRTransactionRefCount** transactions, long transactionsSize)
	throw (sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("getFXSpotForwardTransactionForAggregation");

	//Select all the found trades which are not the first and not the second leg in a FxSwap
	//work on blocks of size 100
	const long blockSize = 100;
	for (long first = 0; first < transactionsSize; first = first + blockSize)
	{
		long last = first + blockSize - 1;
		if (last >= transactionsSize)
			last = transactionsSize - 1;

		MESS(Log::debug, "First=" << first << ",last=" << last);

		RefconToIdxTradesMap refconToIdxTradesMap;
		_STL::string sRefconsToLookFor;
		for (long i = first; i <= last; i++)
		{
			portfolio::TransactionIdent refcon = transactions[i]->getInternalCode();
			if (i == first)
			{
				sRefconsToLookFor = sRefconsToLookFor + FROM_STREAM(refcon);
			}
			else
			{
				sRefconsToLookFor = sRefconsToLookFor + FROM_STREAM("," << refcon);
			}
			refconToIdxTradesMap.insert(_STL::make_pair(refcon, i));
		}

		_STL::list<long> foundFxSwapLegs;
		getFxSwapLegs(sRefconsToLookFor, foundFxSwapLegs);
		for (_STL::list<long>::const_iterator iterFoundFxLeg = foundFxSwapLegs.begin(); iterFoundFxLeg != foundFxSwapLegs.end(); iterFoundFxLeg++)
		{
			refconToIdxTradesMap.erase(*iterFoundFxLeg);
		}

		if (refconToIdxTradesMap.size() > 0)
		{
			//at least one trade is not a FxSwap leg
			END_LOG();
			return refconToIdxTradesMap.begin()->second;
		}
	}

	END_LOG();
	return -1;
}

bool CSxExecutionsAggregatorCounterparty::aggregate(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList, tools::CSREventVector& context) throw(sophisTools::base::ExceptionBase)
{
	return CSRDefaultExecutionAggregator::aggregate(allocation, tradeList, context);
}

void CSxExecutionsAggregatorCounterparty::deAggregate(const sophis::execution::CSRExecutionAllocation& allocation, sophis::portfolio::TradeList& tradeList, tools::CSREventVector& context) throw(sophisTools::base::ExceptionBase)
{
	CSRDefaultExecutionAggregator::deAggregate(allocation, tradeList, context);
}

const char* CSxExecutionsAggregatorCounterparty::GetName() const
{
	return DefaultAggregatorKey;
}


void CSxExecutionsAggregatorCounterparty::getFxSwapList(const std::string& sRefconsToLookFor, std::list<FxSwapLegs>& foundFxSwapList)
{
	BEGIN_LOG("getFxSwapList");

	std::string strQuery = "";
	CSRQueryBuffered<FxSwapLegs> query;
	try
	{
		std::vector<FxSwapLegs> results;

		query << "SELECT "
			<< OutOffset("refcon", &FxSwapLegs::nearLeg)
			<< OutOffset("linkedrefcon", &FxSwapLegs::farLeg)
			<< " FROM FOREX_SWAP WHERE refcon IN (" << sRefconsToLookFor.c_str() << ") AND linkedrefcon <> 0";

		size_t iNbResults = query.FetchAll(results);

		// set ids found
		for (int i = 0; i < iNbResults; i++)
		{
			foundFxSwapList.push_back(results[i]);
		}
	}
	catch (const CSROracleException & ex)
	{
		std::string errMsg = FROM_STREAM("A CSROracleException exception occurred while trying to execute query '" << query.GetSQL() << "': (" << ex << ")");
		MESS(Log::error, errMsg);
		END_LOG();
		throw sophisTools::base::DatabaseException(errMsg.c_str(), strQuery.c_str());
	}
	catch (const CSRDatabaseException  & ex)
	{
		std::string errMsg = FROM_STREAM("A CSRDatabaseException exception occurred while trying to execute query '" << query.GetSQL() << "': (" << ex << ")");
		MESS(Log::error, errMsg);
		END_LOG();
		throw sophisTools::base::DatabaseException(errMsg.c_str(), strQuery.c_str());
	}
	catch (...)
	{
		std::string errMsg = FROM_STREAM("An exception occurred while trying to execute query '" << query.GetSQL() << "': (unexpected)");
		MESS(Log::error, errMsg);
		END_LOG();
		throw(sophisTools::base::GeneralException(errMsg.c_str()));
	}
	END_LOG();
}

void CSxExecutionsAggregatorCounterparty::getFxSwapLegs(const std::string& sRefconsToLookFor, std::list<long>& foundFxSwapLegs)
{
	BEGIN_LOG("getFxSwapLegs");

	std::string strQuery = "";
	struct FxSwapLeg
	{
		long leg;
	};
	CSRQueryBuffered<FxSwapLeg> query;
	try
	{
		//look in the first leg column
		std::vector<FxSwapLeg> results;
		query << "SELECT "
			<< OutOffset("refcon", &FxSwapLeg::leg)
			<< " FROM FOREX_SWAP WHERE refcon IN (" << sRefconsToLookFor.c_str() << ") AND linkedrefcon <> 0";
		size_t iNbResults = query.FetchAll(results);
		for (int i = 0; i < iNbResults; i++)
		{
			foundFxSwapLegs.push_back(results[i].leg);
		}
	}
	catch (const CSROracleException & ex)
	{
		std::string errMsg = FROM_STREAM("A CSROracleException exception occurred while trying to execute query '" << query.GetSQL() << "': (" << ex << ")");
		MESS(Log::error, errMsg);
		END_LOG();
		throw sophisTools::base::DatabaseException(errMsg.c_str(), strQuery.c_str());
	}
	catch (const CSRDatabaseException  & ex)
	{
		std::string errMsg = FROM_STREAM("A CSRDatabaseException exception occurred while trying to execute query '" << query.GetSQL() << "': (" << ex << ")");
		MESS(Log::error, errMsg);
		END_LOG();
		throw sophisTools::base::DatabaseException(errMsg.c_str(), strQuery.c_str());
	}
	catch (...)
	{
		std::string errMsg = FROM_STREAM("An exception occurred while trying to execute query '" << query.GetSQL() << "': (unexpected)");
		MESS(Log::error, errMsg);
		END_LOG();
		throw(sophisTools::base::GeneralException(errMsg.c_str()));
	}

	CSRQueryBuffered<FxSwapLeg> query1;
	try
	{
		//look in the second leg column
		std::vector<FxSwapLeg> results1;
		query1 << "SELECT "
			<< OutOffset("linkedrefcon", &FxSwapLeg::leg)
			<< " FROM FOREX_SWAP WHERE refcon <> 0 AND linkedrefcon IN (" << sRefconsToLookFor.c_str() << ")";

		size_t iNbResults = query1.FetchAll(results1);

		// set ids found
		for (int i = 0; i < iNbResults; i++)
		{
			foundFxSwapLegs.push_back(results1[i].leg);
		}
	}
	catch (const CSROracleException & ex)
	{
		std::string errMsg = FROM_STREAM("A CSROracleException exception occurred while trying to execute query '" << query1.GetSQL() << "': (" << ex << ")");
		MESS(Log::error, errMsg);
		END_LOG();
		throw sophisTools::base::DatabaseException(errMsg.c_str(), strQuery.c_str());
	}
	catch (const CSRDatabaseException  & ex)
	{
		std::string errMsg = FROM_STREAM("A CSRDatabaseException exception occurred while trying to execute query '" << query1.GetSQL() << "': (" << ex << ")");
		MESS(Log::error, errMsg);
		END_LOG();
		throw sophisTools::base::DatabaseException(errMsg.c_str(), strQuery.c_str());
	}
	catch (...)
	{
		std::string errMsg = FROM_STREAM("An exception occurred while trying to execute query '" << query1.GetSQL() << "': (unexpected)");
		MESS(Log::error, errMsg);
		END_LOG();
		throw(sophisTools::base::GeneralException(errMsg.c_str()));
	}
	END_LOG();
}
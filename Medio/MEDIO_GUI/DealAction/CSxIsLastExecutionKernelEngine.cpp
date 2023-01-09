/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/backoffice_kernel/SphKernelEvent.h"
#include "SphSDBCInc/queries/SphQuery.h"
#include "..\..\MediolanumConstants.h"
#include "math.h"
// specific
#include "SphTools/SphLoggerUtil.h"
#include "CSxIsLastExecutionKernelEngine.h"
#include <map>

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::tools;
using namespace sophis::backoffice_kernel;
using namespace std;

const char* CSxApplyEvent::__CLASS__ = "CSxApplyEvent";

/*static*/ _STL::set<TransactionIdent> CSxApplyEvent::fTradesAlreadyPlayed;

//-------------------------------------------------------------------------------------------------------------
/*static*/bool CSxApplyEvent::IsLast(TransactionIdent id)
{
	BEGIN_LOG("IsLast");
	sophis::sql::CSRQuery oneIntQuery;
	oneIntQuery.SetName("Performing one int query");
	int outVal = -1;

	oneIntQuery << "SELECT "
		<< sophis::sql::CSROut("ISLAST", outVal)
		<< " from EXTERNAL_EXECUTIONS where pk_sophisexecid in "
		<< " (select SOPHISEXECID from EXTERNAL_EXECUTIONS_TO_TRADES where "
		<< " tradeid = "
		<< CSRIn(id)
		<< ") and ISLAST = 1 ";


	if (oneIntQuery.Fetch())
	{
		LOG(Log::debug, FROM_STREAM("Int value fetched: " << outVal));
		END_LOG();
		return true;
	}
	else
	{
		LOG(Log::debug, "No value fectched.");
		END_LOG();
		return false;
	}
}


/* NOT WORKING : the column order_quantity is updated after the trade has been inserted - too late */
//-------------------------------------------------------------------------------------------------------------
/*static*/bool CSxApplyEvent::IsFullyExecuted(long orderId)
{
	BEGIN_LOG("IsLast");
	sophis::sql::CSRQuery oneIntQuery;
	oneIntQuery.SetName("Performing one int query");
	int outVal = -1;

	oneIntQuery << "SELECT "
		<< CSROut("ISTOTALLYEXECUTED", outVal)
		<< " from order_quantity where "
		<< " id = " << CSRIn(orderId);

	if (oneIntQuery.Fetch())
	{
		LOG(Log::debug, FROM_STREAM("Int value fetched: " << outVal));
		END_LOG();
		return outVal == 1;
	}
	else
	{
		LOG(Log::debug, "No value fectched.");
		END_LOG();
		return false;
	}
}

//-------------------------------------------------------------------------------------------------------------
/*static*/ double CSxApplyEvent::GetOrderQuantity(long orderId)
{
	BEGIN_LOG("GetOrderQuantity");

	sophis::sql::CSRQuery oneDoubleQuery;
	oneDoubleQuery.SetName("Order quantity");
	double outVal = 0;

	oneDoubleQuery << "SELECT "
		<< sophis::sql::CSROut("orderedqty", outVal)
		<< " from order_quantity where id = " << CSRIn(orderId);

	if (oneDoubleQuery.Fetch())
	{
		LOG(Log::debug, FROM_STREAM("Double value fetched: " << outVal));
		END_LOG();
	}
	else
	{
		LOG(Log::debug, "No value fectched.");
		END_LOG();
	}
	return outVal;
}

//-------------------------------------------------------------------------------------------------------------
/*static*/ double CSxApplyEvent::GetExecutedQuantity(long orderId)
{
	BEGIN_LOG("GetExecutedQuantity");

	sophis::sql::CSRQuery oneDoubleQuery;
	oneDoubleQuery.SetName("Executed quantity");
	double outVal = 0;

	oneDoubleQuery << "SELECT "
		<< sophis::sql::CSROut("sum(abs(e.lastquantity))", outVal)
		<< " from External_executions e "
		<< " inner join order_placement o "
		<< " on e.sophisorderid = o.id "
		<< " and o.orderid = " << CSRIn(orderId);

	if (oneDoubleQuery.Fetch())
	{
		LOG(Log::debug, FROM_STREAM("Double value fetched: " << outVal));
		END_LOG();
	}
	else
	{
		LOG(Log::debug, "No value fectched.");
		END_LOG();
	}
	return outVal;
}

std::vector<TransactionIdent> CSxApplyEvent::GetTradesFromOrder(long orderId)
{
	BEGIN_LOG("GetTradesFromOrder")

		sophis::sql::CSRQuery oneDoubleQuery;
	oneDoubleQuery.SetName("Trades from order");
	long outVal = 0;

	//Select DISTINCT T.TRADEID
	//	FROM ORDER_PLACEMENT O, EXTERNAL_EXECUTIONS E, EXTERNAL_EXECUTIONS_TO_TRADES T
	//	WHERE O.id = 18769
	//	

	oneDoubleQuery << "SELECT DISTINCT "
		<< sophis::sql::CSROut("T.TRADEID", outVal)
		<< " from ORDER_PLACEMENT O, EXTERNAL_EXECUTIONS E, EXTERNAL_EXECUTIONS_TO_TRADES T "
		<< " WHERE O.ORDERID = " << CSRIn(orderId)
		<< " AND O.ID = E.SOPHISORDERID "
		<< " AND E.PK_SOPHISEXECID = T.SOPHISEXECID ";

	std::vector<TransactionIdent> trades;
	while (oneDoubleQuery.Fetch())
	{
		LOG(Log::debug, FROM_STREAM("Double value fetched: " << outVal));
		trades.push_back(outVal);
	}
	END_LOG();
	return trades;
}

void CSxApplyEvent::Send()
{
	BEGIN_LOG("Send");
	MESS(Log::verbose, "In send message for refcon " << fRefCon);
	if (fRefCon>0)
	{
		if (fTradesAlreadyPlayed.find(fRefCon) != fTradesAlreadyPlayed.end())
		{
			MESS(Log::debug, "Trade " << fRefCon
				<< " was already processed as fully executed.");
			return;
		}
		sophis::portfolio::CSRTransaction* trans = NULL;
		try
		{
			trans = CSRTransaction::newCSRTransaction(fRefCon);
			if (trans != NULL)
			{
				bool res = false;
				bool isLast = IsLast(fRefCon);
				if (isLast == 1)
				{
					MESS(Log::debug, "Trade " << fRefCon << " is fully executed");
					res = true;
				}
				else
				{
					long orderid = trans->GetSophisOrderId();
					double execQty = GetExecutedQuantity(orderid);
					double orderQty = GetOrderQuantity(orderid);
					if (abs(orderQty - execQty) < 0.000000001)
					{
						MESS(Log::debug, "Trade " << fRefCon << " is fully executed");
						res = true;
					}
				}
				if (res == true)
				{
					MESS(Log::info, "DoAction requested for deal " << fRefCon
						<< " event used is " << CsxIsLastExecutionKernelEngine::fKernelEvent);
					//executions may be processed in the same message
					//we store the refcon to play the trade only once
					long orderid = trans->GetSophisOrderId();
					fTradesAlreadyPlayed.insert(fRefCon);
	
					trans->DoAction(CsxIsLastExecutionKernelEngine::fKernelEvent);
					MESS(Log::debug, "Main trade correctly inserted");
					try
					{
						std::vector<TransactionIdent> tradesImpacted = GetTradesFromOrder(orderid);
						if (tradesImpacted.size() > 1)
						{
							std::vector<TransactionIdent>::const_iterator ite = tradesImpacted.begin();
							while (ite != tradesImpacted.end())
							{
								if (*ite != fRefCon)
								{
									MESS(Log::info, "DoAction requested for deal " << *ite
										<< " event used is " << CsxIsLastExecutionKernelEngine::fKernelEvent);
									CSRTransaction* linkedTrade = NULL;
									try
									{
										linkedTrade = CSRTransaction::newCSRTransaction(*ite);
										if (linkedTrade != NULL)
										{
											linkedTrade->DoAction(CsxIsLastExecutionKernelEngine::fKernelEvent);
										}
										fTradesAlreadyPlayed.insert(*ite);
										delete linkedTrade; linkedTrade = 0;
									}
									catch (const ExceptionBase& ex)
									{
										MESS(Log::error, "Error during linked deal DoAction " << *ite
											<< " with event " << CsxIsLastExecutionKernelEngine::fKernelEvent << ": " << ex.getError());
										if (linkedTrade != NULL)
										{
											delete linkedTrade;
										}
									}
								}
								ite++;
							}
						}
					}
					catch (const ExceptionBase& ex)
					{
						MESS(Log::error, "Error while processing linked trades : " << ex.getError());
					}
				}
				delete trans;
			}
		}
		catch (const ExceptionBase& ex)
		{
			MESS(Log::error, "Error during deal DoAction " << fRefCon
				<< " with event " << CsxIsLastExecutionKernelEngine::fKernelEvent << ": " << ex.getError());
			if (trans != NULL)
			{
				delete trans;
			}
		}
	}
	END_LOG();
}

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_KERNEL_ENGINE(CsxIsLastExecutionKernelEngine);

const char* CsxIsLastExecutionKernelEngine::__CLASS__ = "CsxIsLastExecutionKernelEngine";

/*static*/ int CsxIsLastExecutionKernelEngine::fKernelEvent = -1;

CsxIsLastExecutionKernelEngine::CsxIsLastExecutionKernelEngine() : CSRKernelEngine()
{
	_STL::string eventName = "FullyExecuted";
	ConfigurationFileWrapper::getEntryValue(MEDIO_BO_DEALACTION_CUSTOM_SECTION,
		MEDIO_BO_DEALCHECK_CUSTOM_SECTION_AFTERFINALEXECEVENTNAME,
		eventName, eventName.c_str());
	fKernelEvent = CSRKernelEvent::GetIdByName(eventName);
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CsxIsLastExecutionKernelEngine::Run(const CSRTransaction* original,
	const CSRTransaction* final,
	const _STL::vector<long>& recipientType,
	eGenerationType generationType,
	CSREventVector& mess,
	long event_id) const
{
	BEGIN_LOG("Run");
	try
	{
		TransactionIdent ident;
		MESS(Log::info, "In KernelEngine IsLast");
		if (original != NULL)
		{
			ident = original->GetTransactionCode();
			CSxApplyEvent* applyEvent = new CSxApplyEvent(ident);
			mess.push_back(applyEvent);
		}
		else if (final != NULL)
		{
			ident = final->GetTransactionCode();
			CSxApplyEvent* applyEvent = new CSxApplyEvent(ident);
			mess.push_back(applyEvent);
		}
		MESS(Log::debug, "Message successfully added to eventVector for trade " << ident);
	}
	catch (ExceptionBase e)
	{
		MESS(Log::error, e);
		END_LOG();
	}

	END_LOG();

}

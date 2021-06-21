#pragma warning(disable:4251)

/////////////////////////////////////////////////////////////////////////////////////////
// CSxAccountingQuantity.cpp
/////////////////////////////////////////////////////////////////////////////////////////

/*
** Includes
*/
#include "CSxAccountingQuantity.h"
#include "SphTools/SphLoggerUtil.h"
#include "CFG_Conditions.h"
#include "SphInc/portfolio/SphPosition.h"
#include "SphInc/portfolio/SphTransactionVector.h"
#include "SphInc/backoffice_kernel/SphBusinessEvent.h"

/*
** Static
*/
const char * CSxAccountingQuantity::__CLASS__ = "CSxAccountingQuantity";

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::accounting;
using namespace sophis::backoffice_kernel;

/*
** Methods
*/
CONSTRUCTOR_ACCOUNTING_QUANTITY(CSxAccountingQuantity);

//----------------------------------------------------------------------------------------------------------
/*virtual*/ CSRAccountingQuantity::ID_to_Populate CSxAccountingQuantity::get_quantity( const CSRTransaction& trade, eFlow flow, double* quantity ) const
{
	BEGIN_LOG("get_quantity Trade");
	ID_to_Populate res = NotDefined;

	// Get bond informations
	bool isABond = false;
	bool isAmortissable = false;
	bool isFinal = false;
	CFG_AccountingConditionsMgr::GetBondCriteria(trade, isABond, isAmortissable, isFinal);

	long businessEvent = (long)trade.GetTransactionType();

	// Get Redemption and partial redemption business event id
	long redemptionBusinessEvent = CSRBusinessEvent::GetIdByName("Redemption");
	if (!redemptionBusinessEvent)
	{
		MESS(Log::error, "Failed to find business Event 'Redemption' id");
		END_LOG();
		return NotDefined;
	}

	long partialRedemptionBusinessEvent = CSRBusinessEvent::GetIdByName("Partial Redemption");
	if (!partialRedemptionBusinessEvent)
	{
		MESS(Log::error, "Failed to find business Event 'Partial Redemption' id");
		END_LOG();
		return NotDefined;
	}


	// Do the process
	if (isAmortissable && isABond)
	{
		MESS(Log::debug, "Amortized Bond & Bond");
		if (!isFinal && businessEvent == partialRedemptionBusinessEvent)
		{
			MESS(Log::debug, "Not Final & Partial Redemption => 0.0");
			*quantity = 0.0;
			res = PositionID;
		}
		else if (businessEvent == redemptionBusinessEvent && isFinal)
		{
			MESS(Log::debug, "Final & Redemption => -quantity");
			long mvtident = trade.GetPositionID();
			const CSRPosition * currentPosition = CSRPosition::GetCSRPosition(mvtident);
			if (!currentPosition)
			{
				MESS(Log::error, "Failed to find position " << mvtident);
				END_LOG();
				return res;
			}

			*quantity = 0.0;
			// Sum the quantity of purchase/sale only
			CSRTransactionVector transactionList;
			currentPosition->GetTransactions(transactionList);
			for (unsigned int i = 0; i < transactionList.size(); i++)
			{
				if (transactionList[i].GetTransactionType() != tPurchaseSale)
				{
					continue;
				}
				
				*quantity = transactionList[i].GetQuantity();
			}
			*quantity = -*quantity;

			res = PositionID;
		}
		else
		{
			MESS(Log::warning, "Unexpected case => Standard");
			*quantity = trade.GetQuantity();
			res = PositionID;
		}
	}
	else
	{
		MESS(Log::debug, "Standard");
		*quantity = trade.GetQuantity();
		res = PositionID;
	}

	MESS(Log::debug, "Retruned quantity = " << (double)*quantity);

	END_LOG();
	return res;
}

//----------------------------------------------------------------------------------------------------------
/*virtual*/ CSRAccountingQuantity::ID_to_Populate CSxAccountingQuantity::get_quantity( const sophis::portfolio::CSRPosition& position, eFlow flow, double* quantity ) const
{
	BEGIN_LOG("get_quantity PnL");
	*quantity = position.GetInstrumentCount();
	END_LOG();
	return PositionID ;
}

//----------------------------------------------------------------------------------------------------------
/*virtual*/ CSRAccountingQuantity::ID_to_Populate CSxAccountingQuantity::populate_in_balance_engine( long instrument_id ) const
{
	BEGIN_LOG("get_quantity Balance");
	END_LOG();
	return PositionID ;
}

/*
** Includes
*/

// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"

// specific
#include "CSxShortSellDealCheck.h"
#include "SphInc/misc/ConfigurationFileWrapper.h"
#include "../MediolanumConstants.h"
#include <SphInc/backoffice_kernel/SphBusinessEvent.h>
#include "../Tools/CSxSQLHelper.h"
#include "SphInc/portfolio/SphTransactionVector.h"
#include "SphTools/SphExceptions.h"

/*
** Namespace
*/
using namespace sophis::backoffice_kernel;
using namespace sophis::portfolio;
using namespace sophisTools::logger;

/*static*/ const char* CSxShortSellDealCheck::__CLASS__ = "CSxShortSellDealCheck";
/*static*/ std::vector<long> CSxShortSellDealCheck::_InclusiveBusEvents;
const char* DEALCHECK_SECTION = MEDIO_BO_DEALCHECK_CUSTOM_SECTION;
const char* BUSGROUPNAME = MEDIO_BO_DEALCHECK_CUSTOM_SECTION_SHORTSELLBUSGROUP;
const char* MODIFYID = MEDIO_BO_DEALCHECK_CUSTOM_SECTION_MODIFYEVENTID;
const char* SHORTSELLBUSEVENTID = MEDIO_BO_DEALCHECK_CUSTOM_SECTION_SHORTSELLBUSEVNT;
	
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_CHECK_DEAL(CSxShortSellDealCheck);

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxShortSellDealCheck::VoteForCreation(CSRTransaction& transaction) const
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForCreation");
	Check(transaction);
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxShortSellDealCheck::VoteForModification(const CSRTransaction& original,
									 CSRTransaction& transaction) const
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForModification");

	Check(transaction);

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxShortSellDealCheck::VoteForDeletion(const CSRTransaction& transaction) const
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForDeletion");
	END_LOG();
}


//-------------------------------------------------------------------------------------------------------------
void CSxShortSellDealCheck::Check(sophis::portfolio::CSRTransaction& transaction) const 
throw(sophis::tools::VoteException)
{
	BEGIN_LOG("Check");

	long busID = transaction.GetTransactionType();
	if (std::find(_InclusiveBusEvents.begin(), _InclusiveBusEvents.end(), busID) != _InclusiveBusEvents.end())
	{
		LOG(Log::debug, "About to Check the trade #" + transaction.GetTransactionCode());

		CSRTransactionVector* trades = GetProvisionTrades(transaction);
		if (IsShortSelling(trades))
		{
			LOG(Log::debug, FROM_STREAM("Trade #" << transaction.GetTransactionCode() << " is a short sell!"));

			long _temp = transaction.GetTransactionType();
			long busEvent = GetShortSellBusEventFromConfig();
			//long modifyID = GetModifyEventIDFromConfig();
			ModifyBusEvent(transaction, busEvent);

			LOG(Log::debug, FROM_STREAM("Business event has been changed from " << _temp << " to " << transaction.GetTransactionType()));
		}
	}
	END_LOG();
}

/* Get trades of the following characteristics:
 * 1) Same security 
 * 2) Same fund 
 * 3) Payment date of the trade <= the value date of the current trade
 * 4) Business event is ‘impacting number of securities’ 
 */
//-------------------------------------------------------------------------------------------------------------
CSRTransactionVector* CSxShortSellDealCheck::GetProvisionTrades(sophis::portfolio::CSRTransaction& transaction) const 
throw (sophis::tools::VoteException)
{
	bool isCurrentTradeIncluded = false;
	_STL::string whereClause = FROM_STREAM("DATE_TO_NUM(DATEVAL) <= " << transaction.GetTransactionDate()
		<< " AND SICOVAM = " << transaction.GetInstrumentCode()
		<< " AND ENTITE = " << transaction.GetEntity());
	
	CSRTransactionVector* transVector = new CSRTransactionVector(whereClause.c_str());
	CSRTransactionVector::const_iterator it = transVector->begin();
	for (; it != transVector->end(); it++)
	{
		// Add the current trade if not found 
		if (it->GetTransactionCode() == transaction.GetTransactionCode())
		{
			isCurrentTradeIncluded = true;
		}
		if (!CSRBusinessEvent::ModifyNumberOfSecurities(it->GetTransactionType()))
		{
			transVector->erase(it);
		}
	}
	if (!isCurrentTradeIncluded)
	{
		transVector->push_back(transaction);
	}
	return transVector;
}


//-------------------------------------------------------------------------------------------------
/*static*/ bool CSxShortSellDealCheck::IsShortSelling(CSRTransactionVector* transactionVec)
{
	double res = 0;
	CSRTransactionVector::const_iterator it = transactionVec->begin();
	for (; it != transactionVec->end(); it++)
	{
		res += it->GetQuantity();
	}
	return res < 0;
}


//-------------------------------------------------------------------------------------------------
/*static*/ void CSxShortSellDealCheck::ModifyBusEvent(CSRTransaction& transaction, long busEvent)
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("ModifyBusEvent");
	try
	{
		transaction.SetTransactionType((eTransactionType)busEvent);
		//transaction.SaveToDatabase(kernelModifyEvent);
	}
	catch (VoteException e)
	{
		_STL::string msg = FROM_STREAM("Failed to update the business event of trade #" << transaction.getInternalCode());
		LOG(Log::error, msg);
		throw VoteException(msg.c_str());
	}
	END_LOG();
}

//-------------------------------------------------------------------------------------------------
/*static*/ std::vector<long> CSxShortSellDealCheck::GetBusEventFromConfig()
{

	BEGIN_LOG(__FUNCTION__);

	std::vector<long> vec;
	_STL::string group = "";
	ConfigurationFileWrapper::getEntryValue(DEALCHECK_SECTION, BUSGROUPNAME, group);


	CSRQueryBuffered<long> getBEId;
	getBEId.SetName("GetBEId");


	getBEId << "SELECT " << BuildOut("BE_ID",getBEId )
			<< " from BO_BE_GROUPS_COMPONENTS be"
			<< " on bg.RECORD_TYPE = 1 AND bg.NAME = " << CSRIn(group)
			<< " and be.BE_GROUP_COMPONENT_ID = bg.id";

	MESS(Log::debug, "Retrieving all groups ID with query : " << getBEId.GetSQL());
	getBEId.FetchAll(vec);
	MESS(Log::debug, "Retrieved: " << vec.size());
	END_LOG();

	return vec;
}


////-------------------------------------------------------------------------------------------------
///*static*/ long CSxShortSellDealCheck::GetModifyEventIDFromConfig()
//{
//	static long res = -1;
//	if( res < 0 )
//	{
//		ConfigurationFileWrapper::getEntryValue(DEALCHECK_SECTION, MODIFYID, res, 0);
//	}
//	return res;
//}

//-------------------------------------------------------------------------------------------------
/*static*/ long CSxShortSellDealCheck::GetShortSellBusEventFromConfig()
{
	static long res = -1;
	if( res < 0 )
	{
		ConfigurationFileWrapper::getEntryValue(DEALCHECK_SECTION, SHORTSELLBUSEVENTID, res);
	}
	return res;
}


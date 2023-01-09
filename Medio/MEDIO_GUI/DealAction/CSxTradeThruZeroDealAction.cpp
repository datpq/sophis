/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/portfolio/SphSwapTransactionReferencesCache.h"
#include "SphLLInc\accounting\Processing\SphBOKernelEnvironment.h"
#include "SphInc/backoffice_kernel/SphKernelStatusGroup.h"
#include "SphTools/SphLogger.h"

// specific
#include "CSxTradeThruZeroDealAction.h"
#include <SphInc/portfolio/SphTransactionCtxMenu.h>
#include <SphInc/portfolio/SphTransactionAction.h>
#include "SphInc/misc/ConfigurationFileWrapper.h"
#include "../MediolanumConstants.h"
#include <SphInc/backoffice_kernel/SphBusinessEvent.h>

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::tools;
using namespace sophis::backoffice_kernel;

/*static*/ const char* CSxTradeThruZeroDealAction::__CLASS__ = "CSxTradeThruZeroDealAction";
/*static*/ long CSxTradeThruZeroDealAction::_KernStatusGroupID;
/*static*/ _STL::string CSxTradeThruZeroDealAction::_EXTREF_RBC_REPORTING_BE;
/*static*/ _STL::string CSxTradeThruZeroDealAction::_EXTREF_RBC_REPORTING_BE_EXCEPTION;

const char* _DEALACTION_SECTION = MEDIO_BO_DEALACTION_CUSTOM_SECTION;
const char* _KERNELGROUPNAME = MEDIO_BO_DEALACTION_CUSTOM_SECTION_TRADETHRUZEROKERNELGROUP;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_KERNEL_ENGINE(CSxTradeThruZeroDealAction);

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxTradeThruZeroDealAction::Run( const CSRTransaction* original,
										  const CSRTransaction* final,
										  const _STL::vector<long>& recipientType,
										  eGenerationType generationType,
										  CSREventVector& mess,
										  long event_id) const
{

	BEGIN_LOG("Run");

	if (!final)
	{
		LOG(Log::debug, FROM_STREAM("Trade #" + final->GetTransactionCode() << " is NULL. Do nothing ..."));
		return;
	}
	long kernelStatus = final->GetBackOfficeType();
	if (IsInStatusGroup(kernelStatus, _KernStatusGroupID))
	{
		LOG(Log::debug, FROM_STREAM("Trade #" + final->GetTransactionCode() << " is in status group" << _KernStatusGroupID));

		CSRTransactionVector* provTrades = GetProvisionTrades(final, false);
		double provQty = GetTotalQuantity(provTrades);
		
		// Set external ref
		auto type = GetTradeReportingType(final, provQty);
		LOG(Log::debug, FROM_STREAM("Trade #" << final->GetTransactionCode() << " type = " << ToString(type)));

		if (type != UnDefined)
			SetExternalReference(final->GetTransactionCode(), _EXTREF_RBC_REPORTING_BE, ToString(type));
			
		double allQty = provQty + final->GetQuantity();
		if (allQty * provQty < 0) // If Sign(Provision) <> Sign(Provision + Quanity)
			SetExternalReference(final->GetTransactionCode(), _EXTREF_RBC_REPORTING_BE_EXCEPTION, "Trading through zero");
	}
	END_LOG();
}


//-------------------------------------------------------------------------------------------------------------
/*Static*/ bool CSxTradeThruZeroDealAction::IsInStatusGroup(long statusID, long groupID)
{
	BEGIN_LOG("Run");
	CSRKernelGroupOfStatuses currentGroup(groupID);
	if (&currentGroup != nullptr)
	{
		_STL::vector<long> statusList;
		currentGroup.GetStatuses(statusList);
		if (std::find(statusList.begin(), statusList.end(), statusID) != statusList.end())
		{
			LOG(Log::debug, FROM_STREAM("Status " << statusID << " is in the group " <<groupID));
			return true;
		}
		else
		{
			LOG(Log::debug, FROM_STREAM("Status " << statusID << " does not belong to the group " << groupID));
			return false;
		}
	}
	else
	{
		LOG(Log::debug, FROM_STREAM("GroupID " << groupID << " is not valid to find a group"));
	}
	return false;
	END_LOG();
	//return CSRBOKernelEnvironment::Instance()->IsStatusInGroup(statusID, groupID); NOT WORKING SADLY
}


//-------------------------------------------------------------------------------------------------------------
/*Static*/ _STL::string CSxTradeThruZeroDealAction::GetExtRefNameBEFromConfig()
{
	_STL::string name = "";
	ConfigurationFileWrapper::getEntryValue(_DEALACTION_SECTION, MEDIO_BO_DEALCHECK_CUSTOM_SECTION_BENAME, name, MEDIO_TRADE_EXTREF_RBC_REPORTING_BE);
	return name;
}


//-------------------------------------------------------------------------------------------------------------
/*Static*/ _STL::string CSxTradeThruZeroDealAction::GetExtRefNameBEExceptionFromConfig()
{
	_STL::string name = "";
	ConfigurationFileWrapper::getEntryValue(_DEALACTION_SECTION, MEDIO_BO_DEALCHECK_CUSTOM_SECTION_BEEXCEPTIONNAME, name, MEDIO_TRADE_EXTREF_RBC_REPORTING_BE_EXCEPTION);
	return name;
}


//-------------------------------------------------------------------------------------------------------------
/*Static*/ long CSxTradeThruZeroDealAction::GetStatusGroupFromConfig()
{
	_STL::string group = "";
	ConfigurationFileWrapper::getEntryValue(_DEALACTION_SECTION, _KERNELGROUPNAME, group, group.c_str());
	CSRBOKernelEnvironment * env = CSRBOKernelEnvironment::Instance();
	CSRBOKernelEnvironment::KStatGroups list = env->GetAllKernStatGroups();
	CSRBOKernelEnvironment::KStatGroups::iterator it;
	for (it = list.begin(); it != list.end(); ++it)
	{
		if (it->second.name == group)
			return it->first;
	}
	return 0;
}


//-------------------------------------------------------------------------------------------------
/*static*/ bool CSxTradeThruZeroDealAction::IsShortSelling(double totalQty)
{
	return totalQty < 0;
}


//-------------------------------------------------------------------------------------------------
/*static*/ double CSxTradeThruZeroDealAction::GetTotalQuantity(CSRTransactionVector* transactionVec)
{
	BEGIN_LOG("GetTotalQuantity");
	double totalQty = 0;
	CSRTransactionVector::const_iterator it = transactionVec->begin();
	for (; it != transactionVec->end(); it++)
	{
		totalQty += it->GetQuantity();
		LOG(Log::debug, FROM_STREAM("Trade #" << it->GetTransactionCode() << " qty = " << it->GetQuantity() << ", total qty = " << totalQty));
	}
	return totalQty;
	END_LOG();
}


/* Get trades of the following characteristics:
* 1) Same security
* 2) Same fund
* 3) Payment date of the trade <= the value date of the current trade
* 4) Business event is ‘impacting number of securities’
*/
//-------------------------------------------------------------------------------------------------------------
CSRTransactionVector* CSxTradeThruZeroDealAction::GetProvisionTrades(const CSRTransaction* transaction, bool IncludeCurrentTrade) const
{
	BEGIN_LOG("GetProvisionTrades");
	bool isCurrentTradeIncluded = false;
	_STL::string whereClause = FROM_STREAM("DATE_TO_NUM(DATEVAL) <= " << transaction->GetSettlementDate()
		<< " AND SICOVAM = " << transaction->GetInstrumentCode()
		<< " AND ENTITE = " << transaction->GetEntity());

	CSRTransactionVector* toAdd = new CSRTransactionVector();
	CSRTransactionVector* transVector = new CSRTransactionVector(whereClause.c_str());
	CSRTransactionVector::const_iterator it = transVector->begin();
	for (; it != transVector->end(); ++it)
	{
		long status = it->GetBackOfficeType();
		if (!IsInStatusGroup(status, _KernStatusGroupID))
		{
			LOG(Log::debug, FROM_STREAM("Trade #" << it->GetTransactionCode() << " with status = " << status 
				<< " is not in the status group #" << _KernStatusGroupID << ". Do not add"));
			continue;
		}
		if (!CSRBusinessEvent::ModifyNumberOfSecurities(it->GetTransactionType()))
		{
			LOG(Log::debug, FROM_STREAM("Trade #" << it->GetTransactionCode() << " doesn't update the number of securities. Do not add"));
			continue;
		}
		if (it->GetTransactionCode() == transaction->GetTransactionCode())
		{
			if (IncludeCurrentTrade)
			{
				toAdd->push_back(*it);
				LOG(Log::debug, FROM_STREAM("Add current trade #" << it->GetTransactionCode() << " to the list"));
			}
		}
		else
		{
			toAdd->push_back(*it);
			LOG(Log::debug, FROM_STREAM("Add trade #" << it->GetTransactionCode() << " to the list"));
		}
	}
	/*if (!isCurrentTradeIncluded && IncludeCurrentTrade && IsInStatusGroup(transaction->GetBackOfficeType(), _KernStatusGroupID))
	{
		transVector->push_back(*transaction);
		LOG(Log::debug, FROM_STREAM("Add trade #" << it->GetTransactionCode() << " to the list"));
	}
*/
	END_LOG();
	return toAdd;
}


//-------------------------------------------------------------------------------------------------
/*static*/ void CSxTradeThruZeroDealAction::SetExternalReference(TransactionIdent code, _STL::string name, _STL::string value)
{
	BEGIN_LOG("SetExternalReference");

	try
	{
		if (code != 0 && !name.empty())
		{
			const auto TransactionReferences = CSRSwapTransactionReferencesCache::GetInstance();
			CSRMarketRegulationReferences references;
			references.SetRef(name, value);

			if (TransactionReferences != nullptr)
			{
				TransactionReferences->Save(code, &references, nullptr);
				LOG(Log::debug, FROM_STREAM("Trade #" << code << " has invalid id or externl ref is null"));
			}
		}
		else
		{
			LOG(Log::debug, FROM_STREAM("Trade #" << code << " has invalid id or externl ref is null"));
		}
	}
	catch(ExceptionBase e)
	{
		LOG(Log::debug, FROM_STREAM("Exception caught while saving external ref: " << e));
	}
	
	END_LOG();
}


//-------------------------------------------------------------------------------------------------
/*static*/ eTradeReportingType CSxTradeThruZeroDealAction::GetTradeReportingType(const CSRTransaction* transaction, double provQty) const
{
	if (provQty > 0)
	{
		if (transaction->GetQuantity() > 0) return BuyLong;
		else if (transaction->GetQuantity() < 0) return SellLong;
	}
	else if (provQty < 0)
	{
		if (transaction->GetQuantity() > 0) return BuyCover;
		else if (transaction->GetQuantity() < 0) return SellShort;
	}
	else //provQty == 0
	{
		if (transaction->GetQuantity() > 0) return Long;
		else if (transaction->GetQuantity() < 0) return Short;
	}
	return UnDefined;
}
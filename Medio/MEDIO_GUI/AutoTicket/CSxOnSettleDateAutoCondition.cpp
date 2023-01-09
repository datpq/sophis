#pragma warning(push)
#pragma warning(disable:4251)

/*
** Includes
*/
#include "CSxOnSettleDateAutoCondition.h"
#include "SphInc/market_data/SphMarketData.h"
#include <SphTools/SphLoggerUtil.h>
#pragma warning(pop)


/*
** Namespace
*/
using namespace sophis::portfolio;

/*static*/ const char* CSxOnSettleDateAutoCondition::__CLASS__ = "CSxOnSettleDateAutoCondition";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ const char* CSxOnSettleDateAutoCondition::GetName() const /*= 0*/
{
	return "On Settlement Date";
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnSettleDateAutoCondition::AppliedTo(const CSRTransaction &transaction) const
{
	BEGIN_LOG("AppliedTo");
	bool res = false;
	if (!&transaction) return res;

	res = gApplicationContext->GetDate() == transaction.GetSettlementDate();
	LOG(Log::debug, FROM_STREAM("Transaction #" << transaction.GetTransactionCode() << " settlement date = " << transaction.GetSettlementDate()));

	END_LOG();
	return res;

}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnSettleDateAutoCondition::AppliedTo(const CSRTransferTrade::CorporateAction &corpAction) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnSettleDateAutoCondition::AppliedTo(const CSRTransferFixings::Fixing &fixing, long fixingType) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnSettleDateAutoCondition::AppliedTo(const CSRTransferSplits::Split &ticket, long splitType) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnSettleDateAutoCondition::AppliedTo(const CSRTransferOptionBarrier::OptionBarrier &ticket) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnSettleDateAutoCondition::AppliedTo(const CSRTransferCustomTicket::CustomTicket &ticket) const
{
	return true;
}
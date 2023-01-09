#pragma warning(push)
#pragma warning(disable:4251)

/*
** Includes
*/
#include "CSxOnTradeDateAutoCondition.h"
#include "SphInc/market_data/SphMarketData.h"
#include <SphTools/SphLoggerUtil.h>
#pragma warning(pop)


/*
** Namespace
*/
using namespace sophis::portfolio;

/*static*/ const char* CSxOnTradeDateAutoCondition::__CLASS__ = "CSxOnTradeDateAutoCondition";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ const char* CSxOnTradeDateAutoCondition::GetName() const /*= 0*/
{
	return "On Trade Date";
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnTradeDateAutoCondition::AppliedTo(const CSRTransaction &transaction) const
{
	BEGIN_LOG("AppliedTo");

	bool res = false;
	if (!&transaction) return res;

	res = gApplicationContext->GetDate() == transaction.GetTransactionDate();
	LOG(Log::debug, FROM_STREAM("Transaction #" << transaction.GetTransactionCode() << " trade date = " << transaction.GetTransactionDate()));

	END_LOG();
	return res;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnTradeDateAutoCondition::AppliedTo(const CSRTransferTrade::CorporateAction &corpAction) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnTradeDateAutoCondition::AppliedTo(const CSRTransferFixings::Fixing &fixing, long fixingType) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnTradeDateAutoCondition::AppliedTo(const CSRTransferSplits::Split &ticket, long splitType) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnTradeDateAutoCondition::AppliedTo(const CSRTransferOptionBarrier::OptionBarrier &ticket) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxOnTradeDateAutoCondition::AppliedTo(const CSRTransferCustomTicket::CustomTicket &ticket) const
{
	return true;
}
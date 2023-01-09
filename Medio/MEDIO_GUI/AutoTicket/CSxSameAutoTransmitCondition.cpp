#pragma warning(push)
#pragma warning(disable:4251)

/*
** Includes
*/
#include "CSxSameAutoTransmitCondition.h"
#include <SphSDBCInc/params/ins/SphIn.h>
#include <SphSDBCInc/params/outs/SphOut.h>
#include "../../Tools/CSxSQLHelper.h"
#pragma warning(pop)


/*
** Namespace
*/
using namespace sophis::portfolio;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ const char* CSxSameAutoTransmitCondition::GetName() const /*= 0*/
{
	return "MEDIO : NEW AUTO TICKET";
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxSameAutoTransmitCondition::AppliedTo(const CSRTransaction &transaction) const
{
	auto positionId = transaction.GetPositionID();

	if (positionId != 0)
	{
		auto adjustmentId = transaction.GetAdjustment();
		auto entity = transaction.GetEntity();
		auto counterparty = transaction.GetCounterparty();
		auto tradeDate = transaction.GetTransactionDate();
		auto settlementDate = transaction.GetSettlementDate();
		auto businessEvent = static_cast<int>(transaction.GetTransactionType());

		auto resultCount = 0;
		CSRQuery query;
		query << "SELECT " << CSROut("COUNT(*)", resultCount) <<
			" FROM JOIN_POSITION_HISTOMVTS WHERE "
			" MVTIDENT=" << CSRIn(positionId) <<
			" AND DATE_TO_NUM(DATENEG)=" << CSRIn(tradeDate) <<
			" AND DATE_TO_NUM(DATEVAL)=" << CSRIn(settlementDate) <<
			" AND TYPE=" << CSRIn(businessEvent) <<
			" AND ENTITE=" << CSRIn(entity) <<
			" AND CONTREPARTIE=" << CSRIn(counterparty) <<
			" AND AJUSTEMENT=" << CSRIn(adjustmentId);
		query.Fetch();

		return resultCount == 0;
	}
	return false;
}

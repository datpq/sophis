#pragma warning(push)
#pragma warning(disable:4251)

/*
** Includes
*/
#include "CSxIsDelegateAutoCondition.h"
#include <SphTools/SphLoggerUtil.h>
#include "SphInc/portfolio/SphPortfolio.h"
#include "..\..\Tools\CSxSQLHelper.h"
#pragma warning(pop)


/*
** Namespace
*/
using namespace sophis::portfolio;

/*static*/ const char* CSxIsDelegateAutoCondition::__CLASS__ = "CSxIsDelegateAutoCondition";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ const char* CSxIsDelegateAutoCondition::GetName() const /*= 0*/
{
	return "Medio - Is Delegate";
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsDelegateAutoCondition::AppliedTo(const CSRTransaction &transaction) const
{
	BEGIN_LOG("AppliedTo");
	bool res = false;
	const CSRPortfolio * folio = CSRPortfolio::GetCSRPortfolio(transaction.GetFolioCode());

	if (nullptr != folio)
	{
		char fullName[255];
		folio->GetFullName(fullName);
		std::size_t found = std::string(fullName).find(CSxSQLHelper::GetTargetPortfolioName());//"MAML");//TOCHANGE
		res = found == std::string::npos;
		LOG(Log::debug, FROM_STREAM("Is folio " << fullName << "an Medio folio? " << res));
	}

	END_LOG();
	return res;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsDelegateAutoCondition::AppliedTo(const CSRTransferTrade::CorporateAction &corpAction) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsDelegateAutoCondition::AppliedTo(const CSRTransferFixings::Fixing &fixing, long fixingType) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsDelegateAutoCondition::AppliedTo(const CSRTransferSplits::Split &ticket, long splitType) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsDelegateAutoCondition::AppliedTo(const CSRTransferOptionBarrier::OptionBarrier &ticket) const
{
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsDelegateAutoCondition::AppliedTo(const CSRTransferCustomTicket::CustomTicket &ticket) const
{
	return true;
}
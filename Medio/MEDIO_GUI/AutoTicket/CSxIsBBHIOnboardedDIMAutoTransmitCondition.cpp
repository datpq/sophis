#pragma warning(push)
#pragma warning(disable:4251)

/*
** Includes
*/
#include "CSxIsBBHIOnboardedDIMAutoTransmitCondition.h"
#include <SphSDBCInc/params/ins/SphIn.h>
#include <SphSDBCInc/params/outs/SphOut.h>
#include "SphSDBCInc/queries/SphQuery.h"
#include "SphSDBCInc/queries/SphQueryBuffered.h"
#include <SphInc/backoffice_kernel/SphBusinessEvent.h>
#include <SphTools/SphLoggerUtil.h>
#include <SphInc/misc/ConfigurationFileWrapper.h>
#pragma warning(pop)


/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;

const char* CSxIsBBHIOnboardedDIMAutoTransmitCondition::__CLASS__ = "CSxIsBBHIOnboardedDIMAutoTransmitCondition";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ const char* CSxIsBBHIOnboardedDIMAutoTransmitCondition::GetName() const /*= 0*/
{
	return "Medio - BBHI onboarded DIM";
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsBBHIOnboardedDIMAutoTransmitCondition::AppliedTo(const CSRTransaction &transaction) const
{
	BEGIN_LOG("AppliedTo");
	LOG(Log::debug, FROM_STREAM("BEGIN: GetTransactionCode= " << transaction.GetTransactionCode() << ", GetTransactionType=" << transaction.GetTransactionType() << ", GetFolioCode=" << transaction.GetFolioCode()));
	_STL::string onboardedBE = "";
	ConfigurationFileWrapper::getEntryValue("MEDIO_TOOLKIT_PARAM", "BBHI_OnBoarded_BE", onboardedBE, "Variation Margin"); //change config file to change to Mark to Market
	LOG(Log::debug, FROM_STREAM("Check TransactionType = " << onboardedBE));
	if (transaction.GetTransactionType() != CSRBusinessEvent::GetIdByName(onboardedBE.c_str())) return false;

	_STL::string folioCode = _STL::to_string(transaction.GetFolioCode());
	auto resultCount = 0;
	CSRQuery query;
	query << "SELECT " << CSROut("COUNT(*)", resultCount) <<
		" FROM BO_TREASURY_ACCOUNT A" <<
		" JOIN MEDIO_BBH_FUNDFILTER BBH ON BBH.FUNDID = A.ACCOUNT_AT_CUSTODIAN" <<
		" JOIN BO_TREASURY_EXT_REF R ON R.ACC_ID = A.ID AND R.VALUE = " << CSRIn(folioCode) <<
		" JOIN BO_TREASURY_EXT_REF_DEF D ON D.REF_ID = R.REF_ID AND D.REF_NAME = 'RootPortfolio'";
	query.Fetch();

	LOG(Log::debug, FROM_STREAM("END: resultCount= " << resultCount));
	END_LOG();
	return resultCount != 0;
}

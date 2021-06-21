#pragma warning(disable:4251)
/*
** Includes
*/
// standard
#include "SphTools/base/CommonOS.h"
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/backoffice_kernel/SphKernelStatus.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphTools/SphLoggerUtil.h"

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_AccountingVersion.h"

#include "CFG_PostingAmounts.h"
#include "CFG_Conditions.h"
#include "CFG_Forex_Rule.h"
#include "CFG_Scenario.h"
#include "BrokerFeesCondition.h"
#include "CSxAccountingQuantity.h"
#include "CSxALSortByPostingDateAndTradeId.h"
#include "HasCounterpartyCondition.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	const char * __CLASS__ = "???";
	BEGIN_LOG("EntryPoint");

	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_Accounting_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	struct AllotmentList
	{
		char allotment_name[256];
	};

	AllotmentList* MyAllotmentList = NULL;
	int cnt = 0;
	_STL::string ConditionName = "CFG IS UNDERLYING ALLOTMENT ";

	CSRStructureDescriptor descriptor(1, sizeof(AllotmentList),true);
	ADD( &descriptor, AllotmentList, allotment_name	, rdfString );

	_STL::string query("select distinct LIBELLE from AFFECTATION");
	//DPH
	//errorCode err = CSRSqlQuery::QueryWithNResults(query.c_str(),&descriptor,(void**) &MyAllotmentList,&cnt);
	errorCode err = CSRSqlQuery::QueryWithNResultsWithoutParam(query.c_str(), &descriptor, (void**)&MyAllotmentList, &cnt);
	if (err) return;
	if ( cnt==0 ) return;

	for(int i=0;i<cnt;++i)
	{
		CFG_RulesConditionTradeWithAllot * myCondition = new CFG_RulesConditionTradeWithAllot();
		CFG_RulesConditionPnLWithAllot * myConditionPnL = new CFG_RulesConditionPnLWithAllot();
		myCondition->set_allotment(MyAllotmentList[i].allotment_name);
		myConditionPnL->set_allotment(MyAllotmentList[i].allotment_name);
		char * ConditionName = new char[256];
		strcpy_s(ConditionName, 256, "CFG IS UNDERLYING ALLOTMENT ");
		strcat_s(ConditionName, 256, MyAllotmentList[i].allotment_name);
		CFG_RulesConditionTradeWithAllot::GetPrototype().insert(ConditionName, myCondition);	
		CFG_RulesConditionPnLWithAllot::GetPrototype().insert(ConditionName, myConditionPnL);	
	}

	//free(MyAllotmentList);
	delete[] MyAllotmentList;

	INITIALISE_POSTING_AMOUNT_FOR_PNL(CFG_UnsettledBalanceAmount, "CFG Unsettled Balance")
	INITIALISE_POSTING_AMOUNT_FOR_PNL(CFG_PostingAmountPNL_AccruedAmount, "Accrued Amount") 
	INITIALISE_POSTING_AMOUNT_FOR_PNL(CFG_SF_Accrued_Total_Interest, "CFG SF Accrued Total Interest") //OK
	INITIALISE_POSTING_AMOUNT_FOR_PNL(CFG_RepoStock_Loan_Asset_Value, "CFG Repo/Stock Loan Asset Value") //OK
	INITIALISE_POSTING_AMOUNT_FOR_PNL(CFG_Revaluation_Asset_in_Stock, "CFG Revaluation Asset in Stock") //OK

	INITIALISE_POSTING_AMOUNT_FOR_TRADE(CSxInterestDebtInstrumentAmountForTrade, "CFG Interest Amount on DAT")

	INITIALISE_POSTING_AMOUNT_FOR_TRADE(CFG_Repo_Underlying_Instrument, "CFG Repo Underlying Instrument")
	INITIALISE_RULES_CONDITION_POSITION(CFGIsSLRepoLeg,"CFG Is SL Repo Leg") 
	INITIALISE_RULES_CONDITION_POSITION(CFGIsNotSLRepoLeg,"CFG Is Not SL Repo Leg")
	INITIALISE_RULES_CONDITION_POSITION(CFGIsDATOver2years, "CFG Is DAT > 2 years")
	INITIALISE_RULES_CONDITION_POSITION(CFGIsDATLess2years, "CFG Is DAT <= 2years")
	INITIALISE_RULES_CONDITION_TRANSACTION(CFGIsDATOver2yearsTrade, "CFG Is DAT > 2 years")
	INITIALISE_RULES_CONDITION_TRANSACTION(CFGIsDATLess2yearsTrade, "CFG Is DAT <= 2years")
	INITIALISE_RULES_CONDITION_TRANSACTION(CFG_RulesConditionTradeIsPartialRedemption, "CFG Is Partial Redemption")
	INITIALISE_RULES_CONDITION_TRANSACTION(CFG_RulesConditionTradeIsFinalRedemption, "CFG Is Final Redemption")

	INITIALISE_RULES_CONDITION_SR(CFGIsARedemption, "CFG Is A Redemption")

	INITIALISE_SR_AMOUNT_TYPE(CFGSRNet,"CFG S/R Net")
	INITIALISE_SR_AMOUNT_TYPE(CFGSRNominal,"CFG S/R Nominal")
	INITIALISE_SR_AMOUNT_TYPE(CFGSRRevenueRegulation,"CFG S/R Revenue Regulation")
	INITIALISE_SR_AMOUNT_TYPE(CFGSRIncomeRegulation,"CFG S/R Income Regulation")
	INITIALISE_SR_AMOUNT_TYPE(CFGSRExpenseRegulation,"CFG S/R Expense Regulation")
	INITIALISE_SR_AMOUNT_TYPE(CFGSRRANRegulation,"CFG S/R R.A.N. Regulation")
	INITIALISE_SR_AMOUNT_TYPE(CFGSRRANRoundingRegulation,"CFG S/R R.A.N. Rounding Regulation")
	INITIALISE_SR_AMOUNT_TYPE(CFGSRFees,"CFG S/R Fees")
	INITIALISE_SR_AMOUNT_TYPE(CFGSubscriptionBalance,"CFG Subscription Balance")
	INITIALISE_SR_AMOUNT_TYPE(CFGRedemptionBalance,"CFG Redemption Balance")

	INITIALISE_FOREX_RULE(CFG_Forex_Rule, "CFG Forex Rule")

	char propertyName[30] = "";
	try
	{
		_STL::string defaultBOStatus = "";

		strcpy_s(propertyName, "CFG_Status_Paid");
		ConfigurationFileWrapper::getEntryValue("ACCOUNTING", propertyName, defaultBOStatus, "Paid");
		static char ruleName[50] = "";
		strcpy_s(ruleName, FROM_STREAM("Bo Status '" << defaultBOStatus.c_str() << "'"));
		INITIALISE_RULES_CONDITION_TRANSACTION(CFGBOEventAccRule, (const char *)ruleName);
	}
	catch(ExceptionBase & ex)
	{
		MESS(Log::warning, "Failed to find property '" << propertyName << "' in section 'ACCOUNTING' (" << (const char *)ex << ")");
	}

	char propertyName2[30] = "";
	try
	{
		_STL::string defaultBOStatus2 = "";

		strcpy_s(propertyName2, "CFG_Status_Fees_Paid");
		ConfigurationFileWrapper::getEntryValue("ACCOUNTING", propertyName2, defaultBOStatus2, "Fees Paid");
		static char ruleName2[50] = "";
		strcpy_s(ruleName2, FROM_STREAM("Bo Status '" << defaultBOStatus2.c_str() << "'"));
		INITIALISE_RULES_CONDITION_TRANSACTION(CFGBOEventAccRule2, (const char *)ruleName2);
	}
	catch(ExceptionBase & ex)
	{
		MESS(Log::warning, "Failed to find property '" << propertyName2 << "' in section 'ACCOUNTING' (" << (const char *)ex << ")");
	}

	// [CR 2010/11/23 Since version 3.1.0.0]
	INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesCondition, "Is Internal Counterparty");

	// [CR 2011/02/11 Since version 3.10.0.0]
	INITIALISE_ACCOUNTING_QUANTITY(CSxAccountingQuantity, "AcountingQuantityBond");

	INITIALISE_AUX_LEDGER_RULES_SORT(CSxALSortByPostingDateAndTradeId, "Posting date/Trade id");

	INITIALISE_KERNEL_ACTION_CONDITION(CSxHasCounterpartyCondition , "Has a Counterparty in Ticket");

//}}SOPHIS_INITIALIZATION
}

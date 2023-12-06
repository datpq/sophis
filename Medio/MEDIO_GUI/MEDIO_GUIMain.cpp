/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/MEDIO_GUIVersion.h"
#include "SphInc/instrument/SphEquity.h"
#include <SphInc/instrument/SphFuture.h>
#include "GUI/CSxTransactionDlg.h"
#include "GUI/CSxIndexDlg.h"
#include <SphInc/portfolio/SphCriteria.h>
#include "Criteria/CSxTradeRefconCriterium.h"
#include "InstrumentModel/CSxOptionModel.h"
#include "DealAction/CSxIsLastExecutionKernelEngine.h"
#include "DealAction/CSxTradeThruZeroDealAction.h"
#include "BOCondition/CSxIsDelegateCondition.h"
#include "BOCondition/CSxIsHedgedCondition.h"
#include "AutoTicket/CSxIsBBHIOnboardedDIMAutoTransmitCondition.h"
#include "AutoTicket/CSxIsDelegateAutoCondition.h"
#include "AutoTicket/CSxOnSettleDateAutoCondition.h"
#include "AutoTicket/CSxOnTradeDateAutoCondition.h"
#include "AutoTicket/CSxSameAutoTransmitCondition.h"
#include "AutoTicket/CSxIsModifiedAutoTransmitCondition.h"
#include "FolioAction/CSxCreateShareStrategyAction.h"
#include "PositionCtxMenu/CSxShareStratCtxMenu.h"
#include "IMReporting/CsxBusinessEventReportingCallBack.h"
#include "IMReporting/CsxIMPortfolioColumn.h"
#include "BrokerFeeCondition/CSxBrokerFeesConditionSEDOL.h"
#include "DealCheck/CSxUnderlyingRICDealCheck.h"
#include "DealCheck/CSxFXCheckDeal.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "BrokerFeeCondition/CSxBrokerFeesConditionSettlePlace.h"
#include "DealAggregator/CSxExecutionsAggregatorCounterparty.h"
#include "BrokerFeeCondition/CSxBrokerFeesIsMAML.h"
//#include "InstrumentModel/CSxVarianceSwap.h"
#include "GUI/CSxVarianceSwapDlg.h"
#include "SphInc/instrument/SphSwap.h"
#include "Column/CSxVariationMarginColumn.h"
#include "Column/CSxMarketValueColumn.h"
#include "CSxBondTypeColumn.h"
#include  "..\Tools\CSxSQLHelper.h"
#include "SphTools\SphLoggerUtil.h"
#include "BrokerFeeCondition\CSxBrokerFeesCtryInc.h"

#include "SphSDBCInc\queries\SphQueryBuffered.h"
#include "Column/CSxMarketValueAggregateColumn.h"
#include "CSxGrossConsiderationAction.h"
#include "CSxCDSPriceAction.h"
#include "BOCondition/CSxIsNotCTMCondition.h"
#include "BOCondition/CSxIsNotSSBCondition.h"
#include "CSxThirdPartyActionCountryCode.h"
#include "BrokerFeeCondition\CSxBrokerFeesCtryCode.h"
#include "BrokerFeeCondition\CSxBrokerFeesException.h"
#include "PositionVisibility\CSxPositionVisibilityHook.h"
#include "CSxIsBrokerDIMCondition.h" 
#include "CSxNACKTemplateCondition.h"


//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
    CSRPreference::SetToolkitVersion(MEDIO_GUI_TOOLKIT_DESCRIPTION);

	//{{SOPHIS_INITIALIZATION (do not delete this line)
	INITIALISE_STANDARD_DIALOG(CSxTransactionDlg, kTransactionDialogId)
	INITIALISE_STANDARD_DIALOG(CSxIndexDlg, kIndexDialogId)
	INITIALISE_CRITERIUM(CSxTradeRefconCriterium, "Trade Ident")  
	INITIALISE_OPTION(CSxOptionModel, "Margin Call Model")
	//INITIALISE_SWAP(CSxVarianceSwap, "Medio Variance Swap")
	//INITIALISE_SPECIFIC_DIALOG(CSxVarianceSwapDlg, CSxVarianceSwap)
	//INITIALISE_SWAP(CSxVolatilitySwap, "Medio Volatility Swap")
	INITIALISE_STANDARD_DIALOG(CSxSwapGeneralDlg, kSwapDialogId)
	INITIALISE_KERNEL_ENGINE(CSxTradeThruZeroDealAction, "Trade Through Zero")
	INITIALISE_KERNEL_ENGINE(CsxIsLastExecutionKernelEngine, "Fully Executed Action")
	INITIALISE_WORKFLOW_DEF_CONDITION(CSxIsDelegateCondition, "Medio - Is Delegate")
	INITIALISE_WORKFLOW_DEF_CONDITION(CSxIsHedgedCondition, "Medio - Is Hedged")
	INITIALISE_AUTO_TRANSMIT_CONDITION(CSxIsDelegateAutoCondition)
	INITIALISE_AUTO_TRANSMIT_CONDITION(CSxOnSettleDateAutoCondition)
	INITIALISE_AUTO_TRANSMIT_CONDITION(CSxOnTradeDateAutoCondition)		
	INITIALISE_AUTO_TRANSMIT_CONDITION(CSxSameAutoTransmitCondition)
	INITIALISE_AUTO_TRANSMIT_CONDITION(CSxIsModifiedAutoTransmitCondition)
	INITIALISE_AUTO_TRANSMIT_CONDITION(CSxIsBBHIOnboardedDIMAutoTransmitCondition)
	INITIALISE_FOLIO_STRATEGY_ACTION(CreateShareStrategyAction,"MedioStratCreation")
	//Looks like this callback does not work.
	INITIALISE_STRATEGY_ALLOCATION_ACTION(CreateShareStrategyAllocationAction,"MedioStratFolioCreation")
	INITIALISE_POSITION_CTX_MENU(CSxShareStratCtxMenu, "Show NAV")
	//This object is called back during F8 to prepare a cache.
	//Note that in 713 it is not called-back when a new trade is created (by this client or another client connected to coherency)
	//It is corrected in 721.3
	INITIALISE_REPORTING_CALLBACK(CsxBusinessEventReportingColumnCallback, "Medio_BU_ReportingCallback")
	INITIALISE_PORTFOLIO_COLUMN(CsxIMPortfolioColumn, "Initial Margin")
	INITIALISE_CHECK_DEAL(CSxUnderlyingRICDealCheck, "Underlying RIC Check")
	INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesConditionSEDOL, "SEDOL Starts With 0 or 3")
	// Query to retrieve main portfolio trading and register properly

	int iMnbRecords = 0;
	// 1. Build descriptor
	SSxSEDOL * iMresult = NULL;
	CSRStructureDescriptor * iMgabSelect = new CSRStructureDescriptor(2, sizeof(SSxSEDOL));
	ADD(iMgabSelect, SSxSEDOL, fLibelle, rdfString)
	// 2. Write query
	std::string iMquery = FROM_STREAM("select 'Is '||CONFIG_VALUE from MEDIO_TKT_CONFIG WHERE CONFIG_NAME='Trading_Target_Folio_Name'");
	// 3. Execute the query
	errorCode iMerr = CSRSqlQuery::StaticQueryWithNResults(iMquery.c_str(), iMgabSelect, (void**)&iMresult, &iMnbRecords);


	INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesIsMAML, iMresult[0].fLibelle)  //MAML") //TOCHANGE
	INITIALISE_PROTOTYPE(CSxExecutionsAggregatorCounterparty, (char*)CSxExecutionsAggregatorCounterparty::DefaultAggregatorKey);

	//Condition 1 - In progress
	int nbRecords = 0;
	// 1. Build descriptor
	SSxSEDOL * result = NULL;
	CSRStructureDescriptor * gabSelect = new CSRStructureDescriptor(2, sizeof(SSxSEDOL));
	ADD(gabSelect, SSxSEDOL, fLibelle, rdfString)
	// 2. Write query
	std::string query = FROM_STREAM("select distinct(T.DOMICILE) as Country "
	<<"from EXTRNL_REF_MARKET_VALUE E "
	<<"inner join TIERS T ON E.REF_IDENT = 5 and E.VALUE is not null and T.REFERENCE = E.VALUE");
	// 3. Execute the query
	errorCode err = CSRSqlQuery::StaticQueryWithNResults(query.c_str(), gabSelect, (void**)&result, &nbRecords);

	for (int i = 0; i < nbRecords; i++)
	{
		My_INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION_SETTLEMENT_PLACE(i, result[i].fLibelle);
	}

	int nbCtryInc = 0;
	SSxCtryInc * resCtryInc = NULL;

	//Needs SphSDBCInc\SphStructureDescriptor.h
	CSRStructureDescriptor * ctryIncSelect = new CSRStructureDescriptor(2, sizeof(SSxCtryInc));
	ADD(ctryIncSelect, SSxCtryInc, fName, rdfString);

	//Use concatenation in string to avoid issues with the constructor
	std::string queryCtryInc = FROM_STREAM("select DISTINCT 'CtryInc '||NAME from SECTORS " <<
		"where PARENT in (select distinct ID from SECTORS where NAME='Country of Incorporation' " <<
		" and PARENT=0)");

	errorCode errCtryInc = CSRSqlQuery::StaticQueryWithNResults(queryCtryInc.c_str(), ctryIncSelect, (void**)&resCtryInc, &nbCtryInc);

	for (int idx = 0; idx < nbCtryInc; idx++)
	{
		My_INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION_CTRYOFINC(idx, resCtryInc[idx].fName);
	}

	int nbCtryCode = 0;
	SSxCtryInc * resCtryCode = NULL;

	 queryCtryInc = FROM_STREAM("select DISTINCT 'CtryBBG '||NAME from SECTORS " <<
		"where PARENT in (select distinct ID from SECTORS where NAME='SEDOL1_COUNTRY_ISO' " <<
		" and PARENT=0)");

	 errCtryInc = CSRSqlQuery::StaticQueryWithNResults(queryCtryInc.c_str(), ctryIncSelect, (void**)&resCtryCode, &nbCtryCode);
	for (int idx = 0; idx < nbCtryCode; idx++)
	{
		My_INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION_CTRYCODE(idx, resCtryCode[idx].fName);
	}

	INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesIsException, "Is Taxable")

	INITIALISE_PORTFOLIO_COLUMN(CSxVariationMarginColumn, "Variation Margin")
	INITIALISE_PORTFOLIO_COLUMN(CSxMarketValueColumn, "Market Value Custom")
	if (CSxFXCheckDeal::IsActivated() == true)
	{
		INITIALISE_TRANSACTION_ACTION(CSxFXCheckDeal, oBeforeDatabaseSaving, "ACheckDeal")
	}

	INITIALISE_PORTFOLIO_COLUMN(CSxBondTypeColumn, "Medio Bond Type")
		INITIALISE_PORTFOLIO_COLUMN(CSxMarketValueAggregateColumn, "Market Value Custom Aggregate")

		INITIALISE_TRANSACTION_ACTION(CSxGrossConsiderationAction, oBeforeDatabaseSaving, "CSxGrossConsiderationAction")
		INITIALISE_TRANSACTION_ACTION(CSxCDSPriceAction, oBeforeDatabaseSaving, "CSxCDSPriceAction")

		INITIALISE_KERNEL_ACTION_CONDITION(CSxIsNotCTMCondition, "Is Not CTM")
		INITIALISE_KERNEL_ACTION_CONDITION(CSxIsNotSSBCondition, "Is Not SSB")
		INITIALISE_KERNEL_ACTION_CONDITION(CSxIsBrokerDIMCondition, "Is Broker DIM")
		INITIALISE_THIRDPARTY_ACTION(CSxThirdPartyActionCountryCode, oBefore, "Country Code Validation")

		INITIALISE_TP_DOC_GEN_RULES_CONDITION(CSxNACKTemplateCondition, "Block Cash MGR NACK generation")

	
	CSxPositionVisibilityHook::_HedgeSet = CSxSQLHelper::GetHedgeFoliosFromConfig();
	CSRUserRights currentUser;
	currentUser.LoadDetails();
	
	if (currentUser.HasAccess("See Expired FX Frwd"))
	{
		CSxPositionVisibilityHook::_seeExpiredFXUserRight = true;
	}
	if (currentUser.HasAccess("See Hedge Positions"))
	{
		CSxPositionVisibilityHook::_seeHedgeUserRight = true;
	}
	
//}}SOPHIS_INITIALIZATION
} 

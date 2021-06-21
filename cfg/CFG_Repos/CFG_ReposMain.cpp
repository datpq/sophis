#pragma warning(disable:4251)
/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_ReposVersion.h"

#include "Source/CSxTVARatesScenario.h"
//}}SOPHIS_TOOLKIT_INCLUDE
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "Source/CSxStandardDealInput.h"
#include "Source/CSxLoanAndRepoDealInput.h"
#include "Source/CSxTransactionAction.h"
#include "Source/CSxPortfolioColumn.h"
#include "Source/CSxTransferTrade.h"

UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_Repos_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)
		
	INITIALISE_STANDARD_DIALOG(CSxStandardDealInput, kTransactionDialogId)
	INITIALISE_STANDARD_DIALOG(CSxLoanAndRepoDealInput, kAdvancedLoanAndRepoDialogId)	
	INITIALISE_TRANSACTION_ACTION(CSxTransactionAction,oAfterSophisValidation, "CSxTransactionAction")	
	INITIALISE_PORTFOLIO_COLUMN(CSxIncomeHTPortfolioColumn, "Income HT")	
	INITIALISE_SCENARIO(CSxTVARatesScenario, "TVA Rates")
	CSRTransferTrade::CreateInstance = CSxTransferTrade::CreateInstance;
	INITIALISE_SCENARIO(CSxExecute, "CFG Transfert trades");

//}}SOPHIS_INITIALIZATION
}

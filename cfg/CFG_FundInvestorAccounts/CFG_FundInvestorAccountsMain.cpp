/*
** Includes
*/
// standard
#pragma warning(disable:4251) // '...' : class '...' needs to have dll-interface to be used by clients of class '...'
#include "SphTools/base/CommonOS.h"
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_FundInvestorAccountsVersion.h"

#include "CFGFundInvestorAccountHandler.h"

#include "SphLLInc/misc/CSXML.h"
//}}SOPHIS_TOOLKIT_INCLUDE
using namespace sophis;

UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_FundInvestorAccounts_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	sophis::misc::dataModel::FpmlEntityHandler::installPrototype(CFG_FIA_NS, new CFGFundInvestorAccountHandler());
	loadGrammarFile("data\\schema","fundInvestorAccount.xsd");

//}}SOPHIS_INITIALIZATION
}

/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_MirroringRuleVersion.h"

#include "MirroringFeesBuilder.h"
#include "MirroringRuleCondition.h"
#include "TKTFieldAction.h"
#include "CFGObligationsMirroringRule.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_MirroringRule_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	INITIALISE_MIRRORING_BUILDER_METHOD(MirroringFeesBuilder, "Standard Fees");
	INITIALISE_MIRRORING_BUILDER_METHOD(CFGObligationsMirroringRule, "CFG Obligation");
	INITIALISE_MIRROR_SEL_CONDITION(MirroringRuleCondition, "IsInternalCounterparty");

	// [CR 2011-07-03 Not cleaned but working, must be done before DB Save]
	INITIALISE_TRANSACTION_ACTION(TKTFieldAction,oBeforeDatabaseSaving,"TKTFieldAction")


//}}SOPHIS_INITIALIZATION
}

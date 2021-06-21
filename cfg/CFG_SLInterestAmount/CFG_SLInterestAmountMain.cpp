/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_SLInterestAmountVersion.h"

#include "Source/CSxSLInterestAmountScenario.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_SLInterestAmount_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)


	INITIALISE_SCENARIO(CSxSLInterestAmountScenario, "SL Interest Amount")
//}}SOPHIS_INITIALIZATION
}

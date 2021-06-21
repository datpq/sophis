/*
** Includes
*/
// standard
#include "SphTools/base/CommonOS.h"
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_RetrocessionFeesVersion.h"

#include "Source/CSxRetrocessionFeesScenario.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_RetrocessionFees_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)


	INITIALISE_SCENARIO(CSxRetrocessionFeesConfigScenario, "Retrocession Fees Configuration")
	INITIALISE_SCENARIO(CSxRetrocessionFeesScenario, "Retrocession Fees Calculation")

//}}SOPHIS_INITIALIZATION
}

/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/MEDIO_COMPLIANCEVersion.h"

#include "CSxThirdPartyCreditCriterium.h"
#include "CSxThirdPartyRatingCriterium.h"
#include "CSxCounterpartyGroupCriterium.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(MEDIO_COMPLIANCE_TOOLKIT_DESCRIPTION);
	INITIALISE_CRITERIUM(CSxThirdPartyCreditCriterium, "Medio Thirdparty Credit");
	INITIALISE_CRITERIUM(CSxThirdPartyRatingCriterium, "Medio Thirdparty Rating");
	INITIALISE_CRITERIUM(CSxCounterpartyGroupCriterium,"Medio Counterparty Group");

//}}SOPHIS_INITIALIZATION
}

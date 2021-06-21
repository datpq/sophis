/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_AccountEODVersion.h"
#include __STL_INCLUDE_PATH(set)
#include "SphQSInc/SphDataDef.h"
#include "SphQSInc/SphSourceFwd.h"

#include "Source/CSxAccountEODScenario.h"

//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_AccountEOD_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)


	INITIALISE_SCENARIO(CSxAccountEODScenario, "Accounting EOD");	
//}}SOPHIS_INITIALIZATION
}

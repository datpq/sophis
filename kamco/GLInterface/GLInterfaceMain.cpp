/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/GLInterfaceVersion.h"

#include "CSxGLscenario.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(GLInterface_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	sophis::scenario::CSRScenario::GetPrototype().erase("Sen&d to GL");
	INITIALISE_SCENARIO(CSxGLscenario, "Sen&d to GL")
//}}SOPHIS_INITIALIZATION
}

/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/TkoTicTocVersion.h"
#include "CriteriumIndicator.h"

//}}SOPHIS_TOOLKIT_INCLUDE

using namespace sophis::sql;

UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(TkoTicToc_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)
	CriteriumIndicator::Register();

//}}SOPHIS_INITIALIZATION
}

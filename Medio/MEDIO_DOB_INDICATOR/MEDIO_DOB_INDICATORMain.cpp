/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/MEDIO_DOB_INDICATORVersion.h"
#include <SphInc/value/modelPortfolio/SphMPIndicator.h>
#include "CSxDOBAssetValueIndicator.h"
#include "CSxDOBMarketValueIndicator.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(MEDIO_DOB_INDICATOR_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	INITIALISE_MP_INDICATOR(CSxDOBAssetValueIndicator);
	//INITIALISE_MP_INDICATOR(CSxDOBMarketValueIndicator);

//}}SOPHIS_INITIALIZATION
}

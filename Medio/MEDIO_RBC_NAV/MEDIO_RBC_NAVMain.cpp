/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/MEDIO_RBC_NAVVersion.h"

#include "CSxPortfolioColumnRBCNAV.h"
#include "CSxPortfolioColumnRBCWeight.h"
#include "CSxUtils.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(MEDIO_RBC_NAV_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)


	INITIALISE_PORTFOLIO_COLUMN(CSxPortfolioColumnRBCNAV, RBC_Strategy_NAV_Column_Name)
	INITIALISE_PORTFOLIO_COLUMN(CSxPortfolioColumnRBCWeight, RBC_Weight_In_Strategy_Column_Name)
//}}SOPHIS_INITIALIZATION
}

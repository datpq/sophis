/*
** Includes
*/
// standard
#pragma warning(disable:4251) // '...' : struct '...' needs to have dll-interface to be used by clients of class '...')
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
#include "SphInc/gui/SphRadioButton.h"


///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_ManagementFeesVersion.h"
#include "Source/CSxManagementFees.h"
#include "Source/CSxManagementFeesGUI.h"
#include "SphInc/SphRiskApi.h"




UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_ManagementFees_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)
	INITIALISE_FUND_FEES(CSxManagementFees, (const char *)(CSxManagementFees::GetStaticTemplateName()));
	if (CSRApi::gAPI && CSRApi::gAPI->IsInGUIMode())
	{
		INITIALISE_FUND_FEES_PAGES(CSxManagementFees::Page, (const char *)(CSxManagementFees::GetStaticTemplateName()));
	}

//}}SOPHIS_INITIALIZATION
}

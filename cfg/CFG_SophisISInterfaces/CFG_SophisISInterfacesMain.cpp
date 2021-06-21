/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFG_SophisISInterfacesVersion.h"
#include "SphInc/SphRiskApi.h"

#include "Source/CSxISInterfacesInstrumentAction.h"
#include "Source/CSxZCYieldCurveSimulation.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_SophisISInterfaces_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)
	
	// by default, this instrument action is executed before any trigger

	if(!CSRApi::IsInBatchMode() && CSRApi::gAPI && CSRApi::gAPI->IsInGUIMode())
	{
		CSRApi::Log("GUI mode - Load ISInterfacesInstrumentAction toolkit");
		INITIALISE_INSTRUMENT_ACTION(CSxISInterfacesInstrumentAction, oUser3, "CSxISInterfacesInstrumentAction")		
	}
	else
	{
		CSRApi::Log("Batch mode - Do not load ISInterfacesInstrumentAction toolkit");
	}

	INITIALISE_YIELD_CURVE(CSxZCYieldCurveSimulation, ZCYieldCurveSimulationModelName);

	
//}}SOPHIS_INITIALIZATION
}

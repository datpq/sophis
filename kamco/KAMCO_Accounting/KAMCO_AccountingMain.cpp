/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/KAMCO_AccountingVersion.h"

#include "ToDoIfInstrumentQuotation.h"
//}}SOPHIS_TOOLKIT_INCLUDE

using namespace eff::kamco::accounting;

UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(ToolkitKAMCO_TOOLKIT_DESCRIPTION);

	//{{SOPHIS_INITIALIZATION (do not delete this line)

	INITIALISE_POSTING_TO_DO_IF(ToDoIfTradeInstrumentIsQuoted, "Instrument Is Quoted")
	INITIALISE_POSTING_TO_DO_IF(ToDoIfTradeInstrumentIsUnquoted, "Instrument Is Unquoted")
	INITIALISE_POSTING_TO_DO_IF_POSITION(ToDoIfPositionInstrumentIsQuoted, "Instrument Is Quoted")
	INITIALISE_POSTING_TO_DO_IF_POSITION(ToDoIfPositionInstrumentIsUnquoted, "Instrument Is Unquoted")

	//}}SOPHIS_INITIALIZATION
}

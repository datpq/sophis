///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
#include "version/FCI_CPPVersion.h"
//}}SOPHIS_TOOLKIT_INCLUDE

#include "FCIExtractionCrtPosition.h"
#include "FCIExtractionCrtTrade.h"
#include "FCIDealInput.h"

UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(FCI_CPP_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	INITIALISE_CRITERIUM(FCIExtractionCrtPosition, "TKT_POSITION_CRT")
	INITIALISE_CRITERIUM(FCIExtractionCrtTrade, "TKT_TRADE_CRT")
	INITIALISE_STANDARD_DIALOG(FCIDealInput, kTransactionDialogId)

//}}SOPHIS_INITIALIZATION

};

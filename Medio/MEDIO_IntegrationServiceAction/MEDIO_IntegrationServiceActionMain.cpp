/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/MEDIO_IntegrationServiceActionVersion.h"

#include "MediolanumTransactionAction.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(MEDIO_IntegrationServiceAction_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)


	// By default, this action is initialized with oAfterSophisValidation. It is called after all sophis triggers.
	// You can change this default value if needed.
	INITIALISE_TRANSACTION_ACTION(MediolanumTransactionAction,oAfterSophisValidation, "MediolanumTransactionAction")
//}}SOPHIS_INITIALIZATION
}

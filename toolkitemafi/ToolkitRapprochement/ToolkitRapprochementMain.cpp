/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/ToolkitRapprochementVersion.h"

#include "RapprEspeces.h"
#include "RapprEspecesRibbon.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(ToolkitRapprochement_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	INITIALISE_SCENARIO(RapprEspeces, "EMAFI Rapprochement Especes")
	
	INITIALISE_RIBBON_COMMAND_WITH_DATA(RapprEspecesRibbon, "RAPPROCHEMENT_ESPECS_RIBBON", "Rapprochement Especes", "data_disk", "Rapprochement Espèces");
//}}SOPHIS_INITIALIZATION
}

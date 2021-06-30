/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/ToolkitEmafiVersion.h"

#include "GenerateBilan.h"
#include "GestionOd.h"
#include "GestionOdRibbon.h"
#include "ConfigurationOdRibbon.h"
#include "GenerateBilanRibbon.h"
#include "ConfigurationRibbon.h"
#include "ConfigButton.h"
#include "ConfigurationOD.h"

#include "EtatReglementaire.h"
#include "EtatReglementairerRibbon.h"
#include "SphInc/SphUserRights.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SqlUtils.h"
//}}SOPHIS_TOOLKIT_INCLUDE

struct StUserRights {
	char profile[50];
};

UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(ToolkitEmafi_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)
	
	CSRUserRights user;
	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(1, sizeof(StUserRights));
	ADD(gabarit, StUserRights, profile, rdfString);

	char query[SQL_LEN] = {'\0'};
	_snprintf_s(query, sizeof(query), "SELECT PROFILE FROM EMAFI_ODSTATUS WHERE PROFILE IS NOT NULL ORDER BY 1");	
	StUserRights * arrRights;
	int count = 0, i = 0;
	errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrRights, &count);
	for(int i=0; i<count; i++)
	{
		if(user.HasAccess(arrRights[i].profile))
		{
			INITIALISE_SCENARIO(GestionOd, "EMAFI GESTION DES ODs")
			INITIALISE_RIBBON_COMMAND_WITH_DATA(GestionOdRibbon, "GESTION_OD_RIBBON","Gestion des OD","index","Pour la Gestion des Operations Diverses");
			break;
		}
	}
	if (user.HasAccess("Emafi Admin")) {
		INITIALISE_SCENARIO(ConfigButton, "EMAFI CONFIG")
	    INITIALISE_SCENARIO(ConfigurationOD, "EMAFI Config OD")
		INITIALISE_RIBBON_COMMAND_WITH_DATA(ConfigurationOdRibbon,"CONFIGURATION_OD_RIBBON","Configuration des Commentaires","text_align_justified","Configuration des commentaires pour Gestion des Operations Diverses");
		INITIALISE_RIBBON_COMMAND_WITH_DATA(ConfigurationRibbon,"CONFIGURATION_RIBBON","Configuration","gears","Configuration de génération des Etats Comptables");
	}


	INITIALISE_RIBBON_COMMAND_WITH_DATA(GenerateBilanRibbon, "GENERATE_REPORTS_RIBBON","Generation des rapports","scroll_run","Pour la génération des Etats Comptables");
	INITIALISE_RIBBON_COMMAND_WITH_DATA(EtatReglementaireRibbon, "GENERATE_REPORTS_REGLEMENTAIRE","Etats reglementaires","scroll_run","Pour la génération des Etats Réglementaire");

	INITIALISE_SCENARIO(GenerateBilan, "EMAFI ETATS COMPTABLES");
	INITIALISE_SCENARIO(EtatReglementaire,"EMAFI ETATS REGLEMENTAIRES");
	
//}}SOPHIS_INITIALIZATION
}

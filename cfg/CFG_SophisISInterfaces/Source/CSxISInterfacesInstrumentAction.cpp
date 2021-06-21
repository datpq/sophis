#pragma warning(disable:4251)
/*
** Includes
*/
#include <stdio.h>
#include "SphTools/SphLoggerUtil.h"
#include __STL_INCLUDE_PATH(string)
//DPH
#include "SphInc/SphMacros.h"
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/instrument/SphInstrument.h"
#include "SphSDBCInc/SphSQLQuery.h"
//DPH
#include "UpgradeExtension.h"

// specific
#include "CSxISInterfacesInstrumentAction.h"


using namespace sophis::misc;
using namespace sophis::sql;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_INSTRUMENT_ACTION(CSxISInterfacesInstrumentAction)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxISInterfacesInstrumentAction::VoteForCreation(CSRInstrument &instrument)
throw (VoteException , ExceptionBase)
{	
	UpdateISInterfacesExternalReferences(instrument,"1","KO");
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxISInterfacesInstrumentAction::VoteForModification(CSRInstrument &instrument, NSREnums::eParameterModificationType type)
throw (VoteException, ExceptionBase)
{
	char externalRefVal[100];
	
	GetCFGExternalReference(instrument,externalRefVal);
	
	if (strcmp(externalRefVal,"") != 0)
	{
		UpdateISInterfacesExternalReferences(instrument,"2","KO");
	}
	else
	{
		UpdateISInterfacesExternalReferences(instrument,"1","KO");
	}	
}

void CSxISInterfacesInstrumentAction::GetCFGExternalReference(CSRInstrument &instrument,char* val)
{
	strcpy_s(val,100,"");
	
	_STL::string CFGExternalRefName = "";	

	ConfigurationFileWrapper::getEntryValue("CFG_SOPHIS_IS_INTERFACES", "CFGExternalRefName",CFGExternalRefName,"CFGExternalRef");	

	char SQLQuery[1024];
	sprintf_s(SQLQuery, 1024,"select value from extrnl_references_instruments I, extrnl_references_definition D where I.REF_IDENT = D.REF_IDENT and" 
		" D.ref_name = '%s' and I.sophis_ident = %ld", CFGExternalRefName.c_str(),instrument.GetCode());

	struct SSxResults
	{
		char fRefValue[100];
	};

	SSxResults *resultBuffer = NULL;
	int		 nbResults = 0;

	CSRStructureDescriptor	desc(1, sizeof(SSxResults));

	ADD(&desc, SSxResults, fRefValue, rdfString);

	//DPH
	//CSRSqlQuery::QueryWithNResults(SQLQuery,&desc,(void **) &resultBuffer,&nbResults);
	CSRSqlQuery::QueryWithNResultsWithoutParam(SQLQuery, &desc, (void **)&resultBuffer, &nbResults);

	if (nbResults > 0)
	{
		strcpy_s(val,100,resultBuffer[0].fRefValue);
	}

	free((char*)resultBuffer);
}

void CSxISInterfacesInstrumentAction::UpdateISInterfacesExternalReferences(CSRInstrument &instrument, const char* actionTypeValue, const char* integrStatusValue)
{
	_STL::string actionTypeExternalRefName = "";
	_STL::string integrStatusExternalRefName = "";

	ConfigurationFileWrapper::getEntryValue("CFG_SOPHIS_IS_INTERFACES", "ActionTypeExternalRefName",actionTypeExternalRefName,"CFGActionType");
	ConfigurationFileWrapper::getEntryValue("CFG_SOPHIS_IS_INTERFACES", "IntegrStatusExternalRefName",integrStatusExternalRefName,"CFGIntegrStatus");

	//DPH
	//SSComplexReferenceP initReferences;
	std::vector<SSComplexReference> initReferences;

	initReferences = instrument.GetClientReference();

	bool foundActionTypeRef = false;
	bool foundIntegrStatusRef = false;

	//DPH
	for(int i = 0; i < initReferences.size(); i++)
	{
		//DPH
		//if( strcmp(initReferences.ref[i].type,actionTypeExternalRefName.c_str()) == 0 )
		if (initReferences.at(i).type == actionTypeExternalRefName)
		{
			//DPH
			//strcpy_s(initReferences.ref[i].value,100,actionTypeValue);
			initReferences.at(i).value = actionTypeValue;
			foundActionTypeRef = true;			
		}

		//DPH
		//if( strcmp(initReferences.ref[i].type,integrStatusExternalRefName.c_str()) == 0 )
		if (initReferences.at(i).type == integrStatusExternalRefName)
		{
			//strcpy_s(initReferences.ref[i].value,100,integrStatusValue);
			initReferences.at(i).value = integrStatusValue;
			foundIntegrStatusRef = true;
		}

		if (foundActionTypeRef == true && foundIntegrStatusRef == true)
			break;
	}

	//DPH
	//SSComplexReferenceP newReferences;
	std::vector<SSComplexReference> newReferences;
	//DPH
	/*
	if(foundActionTypeRef == false && foundIntegrStatusRef == false)
	{
		newReferences.refNum = initReferences.refNum + 2;
	}
	else if (foundActionTypeRef == true && foundIntegrStatusRef == true)
	{
		newReferences.refNum = initReferences.refNum;
	}
	else
	{
		newReferences.refNum = initReferences.refNum + 1;
	}

	newReferences.ref = new SSComplexReference[newReferences.refNum];
	*/
	//DPH
	//for(int i = 0; i < initReferences.refNum; i++)
	for (int i = 0; i < initReferences.size(); i++)
	{
		//strcpy_s(newReferences.ref[i].type,100, initReferences.ref[i].type);
		//strcpy_s(newReferences.ref[i].value,100, initReferences.ref[i].value);
		SSComplexReference newRef;
		newRef.type = initReferences.at(i).type;
		newRef.value = initReferences.at(i).value;
		newReferences.push_back(newRef);
	}

	//DPH
	//int lastIndex = initReferences.refNum;

	if(foundActionTypeRef == false)
	{
		SSComplexReference CFGActionTypeRef;
		//DPH
		//strcpy_s(CFGActionTypeRef.type,100,actionTypeExternalRefName.c_str());
		//strcpy_s(CFGActionTypeRef.value,100, actionTypeValue);
		//newReferences.ref[lastIndex] = CFGActionTypeRef;
		//lastIndex++;
		CFGActionTypeRef.type = actionTypeExternalRefName;
		CFGActionTypeRef.value = actionTypeValue;
		newReferences.push_back(CFGActionTypeRef);
	}

	if(foundIntegrStatusRef == false)
	{
		SSComplexReference CFGIntegrStatusRef;
		//DPH
		//strcpy_s(CFGIntegrStatusRef.type,100,integrStatusExternalRefName.c_str());
		//strcpy_s(CFGIntegrStatusRef.value,100,integrStatusValue);
		//newReferences.ref[lastIndex] = CFGIntegrStatusRef;
		CFGIntegrStatusRef.type = integrStatusExternalRefName;
		CFGIntegrStatusRef.value = integrStatusValue;
		newReferences.push_back(CFGIntegrStatusRef);
	}

	//DPH
	//instrument.SetClientReference(newReferences);
	instrument.SetClientReference(std::move(newReferences));
}

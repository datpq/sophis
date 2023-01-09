#include "CSxCounterpartyGroupCriterium.h"

//#include "CSxThirdPartyCreditCriterium.h"

#include "SphInc\portfolio\SphPosition.h"
#include "SphInc\portfolio\SphExtraction.h"
//#include "SphInc\gui\SphCustomMenu.h"
#include "SphInc\backoffice_kernel\SphThirdParty.h"
#include "SphSDBCInc/queries/SphQuery.h"

//#include "SphLLInc\interface\CustomMenu.h"

#include "SphTools\SphLoggerUtil.h"
//#include "boost/algorithm/string.hpp"
//#include <shlwapi.h>

using namespace sophis::backoffice_kernel;
using namespace sophis::sql;
using namespace sophis::portfolio;

CONSTRUCTOR_CRITERIUM(CSxCounterpartyGroupCriterium)

//-------------------------------------------------------------------------
/*static*/ const char* CSxCounterpartyGroupCriterium::__CLASS__="CSxCounterpartyGroupCriterium";

bool IsCacheFilled = false;

_STL::vector<int> counterpartyGroups;

void CSxCounterpartyGroupCriterium::FillGroupCache() const
{

	BEGIN_LOG(__FUNCTION__);
	int groupId = -1;
	CSRQueryBuffered<int> query;
	query.SetName("Fill Ctpy Group Cache");
	query << "SELECT " <<BuildOut("IDENT", query) <<" FROM TIERS WHERE MGR in (SELECT IDENT FROM TIERS WHERE MGR=-1)";
	MESS(Log::debug,"Retrieving all groups ID with query : "<<query.GetSQL());
	query.FetchAll(counterpartyGroups);
	MESS(Log::debug,"Retrieved "<<counterpartyGroups.size());

	IsCacheFilled = true;

	END_LOG();

}


//-------------------------------------------------------------------------
/*virtual*/ void CSxCounterpartyGroupCriterium::GetCode( SSReportingTrade* mvt, TCodeList &list)  const
{

	// TODO :
	// Get Counterparty from trade
	// Check if in a group and check the group level
	// Return the group code
	BEGIN_LOG(__FUNCTION__);

	try
	{
		if(!IsCacheFilled)
		{
			FillGroupCache();
		}

		long ctpyId = mvt->counterparty;
		long id = GetGroup(ctpyId);
		MESS(Log::debug," Checking CTPTY: "<<ctpyId <<" with Group: "<<id);
		list[0].fType = 0;
		list[0].fCode = id;
	}
		catch(...)
	{
		MESS(Log::error,"Uknown exception caught...");
	}


	END_LOG();
}

//-------------------------------------------------------------------------
/*virtual*/ void CSxCounterpartyGroupCriterium::GetName(long code, char* name, size_t size) const
{
	BEGIN_LOG(__FUNCTION__);

	try
	{
	const CSRThirdParty* ctpy = CSRThirdParty::GetCSRThirdParty(code);
	
	if(ctpy)
	{
		MESS(Log::debug,"Getting Name for CTPTY: "<<code);
		_STL::string cName = ctpy->GetName();
		sprintf_s(name,256,cName.c_str());
		size = strlen(name);
	}
	else
	{
			MESS(Log::debug,"Couldn't find CTPTY with code: "<<code);
			sprintf_s(name,256, "N/A");
	}
	}
	catch(...)
	{
		MESS(Log::error,"Uknown exception caught...");
	}

	END_LOG();
}

long CSxCounterpartyGroupCriterium::GetGroup(long counterpartyId) const
{

	BEGIN_LOG(__FUNCTION__);

	long retval = counterpartyId;

	bool ctpty_found = false;

	try
	{
		while (!ctpty_found)
		{
			if(_STL::find(counterpartyGroups.begin(),counterpartyGroups.end(),retval)!=counterpartyGroups.end())
			{
				MESS(Log::debug,"Found Counterparty Group = "<< retval <<" for counterparty : "<< counterpartyId);
				ctpty_found=true;
			}
			else
			{
					const CSRThirdParty* ctpty = CSRThirdParty::GetCSRThirdParty(retval);

					if(ctpty)
					{
						MESS(Log::debug,"Looking fro parent of CTPTY : "<<retval);
						CSRThirdParty group = ctpty->GetParent();
					
						if(group != NULL)
						{
							retval=group.GetIdent();
							MESS(Log::debug,"Found Parent : "<<retval);
						}
						else
						{
							MESS(Log::debug,"No Parent Found for : "<<retval);
							ctpty_found=true; //no parent...
						}
					}
					else
					{
						ctpty_found=true;
					}

			}
		}
	}
	catch(...)
	{
		MESS(Log::error,"Unknown Exception Occured !");
	}

	END_LOG();

	return retval;


}


//-------------------------------------------------------------------------



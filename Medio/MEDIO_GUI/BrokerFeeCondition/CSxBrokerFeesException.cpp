#include "CSxBrokerFeesException.h"
#include <SphTools/SphLoggerUtil.h>
#include "SphInc\portfolio\SphTransaction.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc\static_data\SphMarket.h"
#include "SphInc\instrument\SphInstrument.h"
#include "SphInc\static_data\SphSector.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc\DataAccessLayer\Structures\SphSectorData.h"

/*
** Namespace
*/
using namespace sophis::instrument;
using namespace sophis::static_data;
using namespace sophis::sql;
using namespace sophis::DAL;

const char * CSxBrokerFeesIsException/*<classkey>*/::__CLASS__ = "CSxBrokerFeesIsException";


/*virtual*/ /*inline*/  bool CSxBrokerFeesIsException::get_condition(const sophis::portfolio::CSRTransaction & trade) const
{
	BEGIN_LOG("get_condition");
	bool res = false;

	static long exceptionSectorID = 0;
	if (exceptionSectorID == 0)
	{
		try
		{
			exceptionSectorID = GetExceptionSectorID();
		}
		catch (...)
		{
			MESS(Log::error, "Unable to retrieve the ID of Is Taxable sector definition");
			exceptionSectorID = -1;
			return false;
		}
	}
	else if (exceptionSectorID == -1)
	{
		MESS(Log::error, "Please fix the issues raised with respect to the retrieval " <<
			"of the ID of Is Taxable sector definition and then re-start the processes");
		return false;
	}
	const CSRInstrument * inst = trade.GetInstrument();
	if (!inst)
	{
		MESS(Log::warning, "Could not get instrument");
		return false;
	}
	
	const CSRSectorData * instSectors = inst->GetSector(exceptionSectorID);
	if (instSectors != nullptr)
	{
		std::string name = instSectors->GetName();
		
		if (name == "Y")
		{		
			return true;
		}
	}
	else
	{
		MESS(Log::error, "Sector data is null");
	}

	END_LOG();
	return false;

}

long GetExceptionSectorID()
{
	long sectorId = 0;
	int count = 0;
	
	struct SSectorID_Struct { long fSectorID; } *sSectorId = NULL;
	CSRStructureDescriptor desc(1, sizeof(SSectorID_Struct));
	ADD(&desc, SSectorID_Struct, fSectorID, rdfInteger);

	std::string queryCtryInc = FROM_STREAM("select DISTINCT ID from SECTORS" <<
		" where NAME='IsTaxable' and PARENT=0");
	errorCode err = CSRSqlQuery::StaticQueryWithNResults(queryCtryInc.c_str(), &desc, (void **)&sSectorId, &count);
	if ((!err) && (count > 0))
	{
		sectorId = sSectorId->fSectorID;
	}

	return sectorId; 
}

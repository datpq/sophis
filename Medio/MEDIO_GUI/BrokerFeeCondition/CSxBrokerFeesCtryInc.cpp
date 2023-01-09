#include "CSxBrokerFeesCtryInc.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc\portfolio\SphTransaction.h"
#include "SphInc\instrument\SphInstrument.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc\static_data\SphMarket.h"
#include "SphInc\static_data\SphSector.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc\DataAccessLayer\Structures\SphSectorData.h"


using namespace sophis::instrument;
using namespace sophis::static_data;
using namespace sophis::sql;
using namespace sophis::DAL;

_STL::map<int, _STL::string> CSxBFCtryIncMapping::MyDict;

const char * CSxBrokerFeesCtryInc::__CLASS__ = "CSxBrokerFeesCtryInc";

bool CSxBrokerFeesCtryInc::get_condition(const sophis::portfolio::CSRTransaction & trade) const
{
	BEGIN_LOG("get_condition");

	static long cntryIncSectorID = 0;
	if (cntryIncSectorID == 0)
	{
		try
		{
			cntryIncSectorID = GetCntryIncorpSectorID();
		}
		catch (...)
		{
			MESS(Log::error, "Unable to retrieve the ID of Country of Incorporation sector definition");
			cntryIncSectorID = -1;
			return false;
		}
	}
	else if (cntryIncSectorID == -1)
	{
		MESS(Log::error, "Please fix the issues raised with respect to the retrieval " <<
			"of the ID of Country of Incorporation sector definition and then re-start the processes");
		return false;
	}

	const CSRInstrument * inst = trade.GetInstrument();
	if (!inst)
	{
		MESS(Log::warning, "Could not get instrument");
		return false;
	}

	_STL::string className = CSxBFCtryIncMapping::MyDict[classKey];
	/*MESS(Log::debug, "[CMSDBG] Found class name " << className.c_str());*/

	int classNameLen = className.size();
	_STL::string shortClassName = className.substr(8, classNameLen - 8);

	const CSRSectorData * instSectors = inst->GetSector(cntryIncSectorID);
	if (instSectors != NULL)
	{
		std::string name = instSectors->GetName();
		long ident = instSectors->GetIdent();

		if (shortClassName.compare(name) == 0)
		{
			/*MESS(Log::debug, "[CMSDBG] Found sector data having IDENT " << ident <<
				" and NAME " << name.c_str() << ", short class name is " << shortClassName.c_str() <<
				", returning TRUE");*/
			return true;
		}

		/*MESS(Log::debug, "[CMSDBG] Found sector data having IDENT " << ident <<
			" and NAME " << name.c_str() << ", short class name is " << shortClassName.c_str() <<
			", returning FALSE");*/
	}
	else
	{
		MESS(Log::error, "Sector data is null");
	}

	END_LOG();
	return false;
}

long GetCntryIncorpSectorID()
{
	long sectorId = 0;
	int count = 0;
	/* The output of the query:
		select DISTINCT ID from SECTORS where NAME='Country of Incorporation' and PARENT=0
	*/

	struct SSectorID_Struct { long fSectorID; } * sSectorId = NULL;
	CSRStructureDescriptor desc(1, sizeof(SSectorID_Struct));
	ADD(&desc, SSectorID_Struct, fSectorID, rdfInteger);

	std::string queryCtryInc = FROM_STREAM("select DISTINCT ID from SECTORS" <<
		" where NAME='Country of Incorporation' and PARENT=0");
	errorCode err = CSRSqlQuery::StaticQueryWithNResults(queryCtryInc.c_str(), &desc, (void **)&sSectorId, &count);
	if ((!err) && (count > 0))
	{
		//sectorId = sSectorId.fSectorID;
		sectorId = sSectorId->fSectorID;
	}

	return sectorId; /*17667*/
}

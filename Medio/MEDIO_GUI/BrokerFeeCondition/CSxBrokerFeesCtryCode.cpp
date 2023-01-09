#include "CSxBrokerFeesCtryCode.h"
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

_STL::map<int, _STL::string> CSxBFCtryCodeMapping::MyDict;

const char * CSxBrokerFeesCtryCode::__CLASS__ = "CSxBrokerFeesCtryCode";

bool CSxBrokerFeesCtryCode::get_condition(const sophis::portfolio::CSRTransaction & trade) const
{
	BEGIN_LOG("get_condition");

	static long cntryCodeSectorID = 0;
	if (cntryCodeSectorID == 0)
	{
		try
		{
			cntryCodeSectorID = GetCntryCodeSectorID();
		}
		catch (...)
		{
			MESS(Log::error, "Unable to retrieve the ID of BBG Country Code sector definition");
			cntryCodeSectorID = -1;
			return false;
		}
	}
	else if (cntryCodeSectorID == -1)
	{
		MESS(Log::error, "Please fix the issues raised with respect to the retrieval " <<
			"of the ID of BBG Country Code sector definition and then re-start the processes");
		return false;
	}

	const CSRInstrument * inst = trade.GetInstrument();
	if (!inst)
	{
		MESS(Log::warning, "Could not get instrument");
		return false;
	}

	_STL::string className = CSxBFCtryCodeMapping::MyDict[classKey];
	
	int classNameLen = className.size();
	_STL::string shortClassName = className.substr(8, classNameLen - 8);

	const CSRSectorData * instSectors = inst->GetSector(cntryCodeSectorID);
	if (instSectors != nullptr)
	{
		std::string name = instSectors->GetName();
		long ident = instSectors->GetIdent();

		if (shortClassName.compare(name) == 0)
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

long GetCntryCodeSectorID()
{
	long sectorId = 0;
	int count = 0;
	
	struct SSectorID_Struct { long fSectorID; } *sSectorId = NULL;
	CSRStructureDescriptor desc(1, sizeof(SSectorID_Struct));
	ADD(&desc, SSectorID_Struct, fSectorID, rdfInteger);

	std::string queryCtryCode = FROM_STREAM("select DISTINCT ID from SECTORS" <<
		" where NAME='SEDOL1_COUNTRY_ISO' and PARENT=0");
	errorCode err = CSRSqlQuery::StaticQueryWithNResults(queryCtryCode.c_str(), &desc, (void **)&sSectorId, &count);
	if ((!err) && (count > 0))
	{
		sectorId = sSectorId->fSectorID;
	}

	return sectorId;
}

#include "CSxSQLHelper.h"
#include <SphTools/SphLoggerUtil.h>
#include <SphInc/misc/ConfigurationFileWrapper.h>

using namespace std;

CSxSQLHelper::CSxSQLHelper(void)
{

}

CSxSQLHelper::~CSxSQLHelper(void)
{
}

_STL::string CSxSQLHelper::GetTargetPortfolioName()
{	
	_STL::string retval ="";
	CSRQuery query;
	char confValue[40] = {'\0'};
	query.SetName("getTargetPortfolio");
	query << "select "<< CSROut::FromStr("CONFIG_VALUE",confValue)<<" from MEDIO_TKT_CONFIG WHERE CONFIG_NAME='Trading_Target_Folio_Name'";
	query.Fetch();
	retval = confValue;
	return retval;
}

std::vector<long> CSxSQLHelper::GetAllotmentsFromConfig()
{
	std::vector<long> AllotmentVec;
	_STL::string allotmentList = "";
	ConfigurationFileWrapper::getEntryValue("MEDIO_CDS_IMPORTPRICE", "CDS Allotments", allotmentList, "CDS;CDX");
	std::string delimiter = ";";
	vector<string> SplitVec;
	boost::split(SplitVec, allotmentList, boost::is_any_of(delimiter));
	for (vector<string>::iterator it = SplitVec.begin(); it != SplitVec.end(); ++it)
	{
		long allotmentId = LoadAllotmentIdFromDB(*it);
		AllotmentVec.push_back(allotmentId);
	}
	return AllotmentVec;
}

/*static*/long CSxSQLHelper::LoadAllotmentIdFromDB(std::string allotmentName)
{
	long id = 0;
	CSRQuery getIdent;
	getIdent << " SELECT " << CSROut("ident", id)
		<< " from AFFECTATION where libelle like " << CSRIn(allotmentName);
	getIdent.Fetch();
	return id;
	
}


/*
 _STL::string CSxSQLHelper::QueryReturning1String(const _STL::string query)
{
	struct SResult
	{
		char value[200];
	} result;
	char value[200];


	CSRStructureDescriptor structureDescriptor(1, sizeof(SResult));
	ADD(&structureDescriptor, SResult, value, rdfString);

	errorCode error = CSRSqlQuery::QueryWith1Result(query.c_str(), &structureDescriptor, &result);
	if (error != 0) return "";

	return _STL::string(result.value);
}

long CSxSQLHelper::QueryReturning1Long(const _STL::string query)
{
    long result = -1;
	CSRSqlQuery::QueryReturning1Long(query.c_str(), &result);
	return result;
}

double CSxSQLHelper::QueryReturning1Double(const _STL::string& query)
{
	double result = 0.0;
	CSRSqlQuery::QueryReturning1Double(query.c_str(), &result);
	return result;
}

_STL::vector<long> CSxSQLHelper::QueryReturningNLongs(const _STL::string query)
{
	try
	{
		_STL::vector<long> Result;
		CSRStructureDescriptor longDesc(1, sizeof(long));
		longDesc.Add(0, sizeof(long), rdfInteger);
		CSRSqlQuery::ResultWithSequence(Result, query.c_str(), &longDesc);
		return Result;
	}
	catch (OracleException exception)
	{
		return _STL::vector<long>();
	}
}

ResultMapStrings CSxSQLHelper::QueryReturningMapStrings(const _STL::string query)
{
	ResultMapStrings resultMap;

	CSRStructureDescriptor	descriptor(2, sizeof(ResultMapStrings::value_type));
	ADD(&descriptor, ResultMapStrings::value_type, second.fName, rdfString);
	ADD(&descriptor, ResultMapStrings::value_type, first, rdfInteger);

	CSRSqlQuery::ResultWithMap(resultMap, query.c_str(), &descriptor);

	return resultMap;
}
*/

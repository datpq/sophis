#include "Inc/SqlUtils.h"
#include "Inc/Log.h"
#include "Inc/SqlWrapper.h"

using namespace eff::utils;
using namespace std;
using namespace sophis::sql;

string SqlUtils::QueryReturning1StringException(const char* query, ...)
{
	FORMAT_MESSAGE(query, formattedQuery);
	COMM(formattedQuery);

	SqlWrapper * sqlWrapper = new SqlWrapper("c1024", formattedQuery);
	if (sqlWrapper->GetRowCount() > 0) {
		string result = (*sqlWrapper)[0][0].value<string>();
		delete sqlWrapper;
		return result;
	} else {
		delete sqlWrapper;
		throw CSRNoRowException(formattedQuery);
	}
}

void SqlUtils::QueryWithoutResultException(const char* query, ...)
{
	FORMAT_MESSAGE(query, formattedQuery);
	COMM(formattedQuery);

	try {
		QueryWithoutResult(formattedQuery);
		//long sqlRowCount = 0;
		//CSRSqlQuery::QueryReturning1Long("SELECT SQL%ROWCOUNT FROM DUAL", &sqlRowCount);
		//CSRSqlQuery::Commit();
	} catch(const CSROracleException &e) {
		ERROR("QueryWithoutResultException error code = %d, reason = %s", e.GetErrorCode(), e.GetReason().c_str());
		//CSRSqlQuery::RollBack();
		throw;
	//} catch(const CSRNoRowException &e) {
	//	CSRSqlQuery::RollBack();
	//	throw;
	}
}

long SqlUtils::QueryReturning1LongException(const char* query, ...)
{
	FORMAT_MESSAGE(query, formattedQuery);
	COMM(formattedQuery);

	long result = 0;
	try {
		CSRSqlQuery::QueryReturning1LongException(formattedQuery, &result);
	} catch(const CSROracleException &e) {
		ERROR("QueryReturning1LongException error code = %d, reason = %s", e.GetErrorCode(), e.GetReason().c_str());
		//CSRSqlQuery::RollBack();
		throw;
	}
	return result;
}

double SqlUtils::QueryReturning1Double(const char* query, ...)
{
	FORMAT_MESSAGE(query, formattedQuery);
	COMM(formattedQuery);

	double result = 0;
	CSRSqlQuery::QueryReturning1Double(formattedQuery, &result);
	return result;
}
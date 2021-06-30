#pragma once

#include <string>
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphSDBCInc/exceptions/SphNoRowException.h"
#include "SphSDBCInc/exceptions/SphOracleException.h"

using namespace sophis::sql;
using namespace std;

#if (SOPHISVER < 720)
#define QueryWithNResultsArray(queryWithoutParam, resultDescriptor, results, resultCount) CSRSqlQuery::QueryWithNResults(queryWithoutParam, resultDescriptor, results, resultCount)
#define QueryWithNResultsVector(queryWithoutParam, resultDescriptor, result) CSRSqlQuery::QueryWithNResults(queryWithoutParam, resultDescriptor, result)
#define QueryWith1Result(queryWithoutParam, resultDescriptor, result) CSRSqlQuery::QueryWith1ResultException(queryWithoutParam, resultDescriptor, result)
#define QueryWithoutResult(queryWithoutParam) CSRSqlQuery::QueryWithoutResultException(queryWithoutParam)
#else
#define QueryWithNResultsArray(queryWithoutParam, resultDescriptor, results, resultCount) CSRSqlQuery::QueryWithNResultsWithoutParam(queryWithoutParam, resultDescriptor, results, resultCount)
#define QueryWithNResultsVector(queryWithoutParam, resultDescriptor, result) CSRSqlQuery::QueryWithNResultsWithoutParam(queryWithoutParam, resultDescriptor, result)
#define QueryWith1Result(queryWithoutParam, resultDescriptor, result) CSRSqlQuery::QueryWith1ResultWithoutParamException(queryWithoutParam, resultDescriptor, result)
#define QueryWithoutResult(queryWithoutParam) CSRSqlQuery::QueryWithoutResultAndParamException(queryWithoutParam)
#endif

#define SQL_LEN 4001

namespace eff {
	namespace utils {
		class SqlUtils
		{
		public:
			static string QueryReturning1StringException(const char* query, ...)
				throw (CSROracleException, CSRNoRowException);
			static void QueryWithoutResultException(const char* query, ...)
				throw (CSROracleException, CSRNoRowException);
			static long QueryReturning1LongException(const char* query, ...)
				throw (CSROracleException);
			static double QueryReturning1Double(const char * query, ...);
		};
	}
}


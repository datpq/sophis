#ifndef __CSxSQLHelper__H__
	#define __CSxSQLHelper__H__

#include "SphSDBCInc/SphSQLQuery.h"
#include "SphTools/SphOStrStream.h"
#include "SphSDBCInc/queries/SphQuery.h"
#include "SphSDBCInc/queries/SphQueryBuffered.h"
#include "..\MediolanumConstants.h"
#include <boost/algorithm/string/split.hpp>
#include <boost/algorithm/string/classification.hpp>


#include <map>

using namespace sophis::sql;

using namespace std;


struct SSxNameStr
{
	char fName[CATEGORY_NAME_LENGTH];
	SSxNameStr()
	{
		memset(fName, 0, CATEGORY_NAME_LENGTH);
	};

	SSxNameStr::SSxNameStr(const char * name)
	{
		strncpy_s(fName, CATEGORY_NAME_LENGTH, name, CATEGORY_NAME_LENGTH - 1);
		fName[CATEGORY_NAME_LENGTH - 1] = 0;
	}

	SSxNameStr::SSxNameStr(const SSxNameStr& name)
	{
		strncpy_s(fName, CATEGORY_NAME_LENGTH, name.fName, CATEGORY_NAME_LENGTH - 1);
		fName[CATEGORY_NAME_LENGTH - 1] = 0;
	}

	bool SSxNameStr::operator < ( const SSxNameStr & right) const
	{
		return (strcmp(fName, right.fName) < 0);
	}

	operator _STL::string() { return fName; }
};

typedef _STL::map<long, SSxNameStr> ResultMapStrings;

class CSxSQLHelper
{
public:

	CSxSQLHelper(void);
	~CSxSQLHelper(void);

	//deprecated, see details on samples on how to replace the methods with DBC10
	/*return: returns a long result of the query or -1 if error*/
	/*static _STL::string QueryReturning1String(const _STL::string query);
	static _STL::vector<long> QueryReturningNLongs(const _STL::string query);
	static long QueryReturning1Long(const _STL::string query);
	static double QueryReturning1Double(const _STL::string& queryTrsBasket);
	static ResultMapStrings QueryReturningMapStrings(const _STL::string query); //see MEDIO_COMPLIANCE\CSxThirdPartyRatingCriterium.cpp for a sample on SophisDBC
	static int QueryNoResults(const _STL::string query)
	{
		return CSRSqlQuery::QueryWithoutResult(query.c_str());
	}*/
	static _STL::string GetTargetPortfolioName();
	static std::vector<long> GetAllotmentsFromConfig();
	static std::set<string> GetHedgeFoliosFromConfig();
	static long LoadAllotmentIdFromDB(std::string allotmentName);
};

#endif // !__CSxSQLHelper__H__
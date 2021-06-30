#pragma once

#include "SphSDBCInc/SphSQLQuery.h"
#include <vector>
#include "Variant.h"

using namespace std;
using namespace sophis::sql;

namespace eff {
	namespace utils {
		class SqlWrapper
		{
		public:
			enum eSqlDataType
			{
				eLong = 'l',
				eChar = 'c',
				eDouble = 'd'
			};

		private:
			vector<vector<Variant>> m_arrResults;
			vector<eSqlDataType> m_arrDataTypes;
			int m_fieldCount;

			int GetFieldSize(string field, vector<int> &arrFieldSizes, vector<eSqlDataType> &arrDataTypes, vector<eRelationalDatabaseFieldType> &arrDatabaseFieldTypes);
		public:
			SqlWrapper(const char * fieldTypes, const char * sqlQuery);
			~SqlWrapper(void);

			const vector<Variant> operator[](int i) const;
			vector<Variant> operator[](int i);

			vector<eSqlDataType> GetDataTypes();
			int GetRowCount();
			int GetFieldCount();
		};
	}
}


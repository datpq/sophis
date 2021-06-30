#include "Inc/SqlWrapper.h"
#include "Inc/SqlUtils.h"
#include "Inc/Log.h"

using namespace eff::utils;
using namespace std;
using namespace sophis::sql;

int SqlWrapper::GetFieldSize(string field, vector<int> &arrFieldSizes, vector<eSqlDataType> &arrDataTypes, vector<eRelationalDatabaseFieldType> &arrDatabaseFieldTypes)
{
	if (field.empty()) return 0;
	int fieldSize = 0;
	eSqlDataType sqlDataType = (eSqlDataType)field.at(0);
	eRelationalDatabaseFieldType databaseFieldType;
	switch(sqlDataType) {
	case eLong:
		fieldSize = sizeof(long);
		databaseFieldType = rdfInteger;
		break;
	case eDouble:
		fieldSize = sizeof(double);
		databaseFieldType = rdfFloat;
		break;
	case eChar:
		fieldSize = stoi(field.substr(1)) + 1;
		databaseFieldType = rdfString;
		break;
	}
	arrFieldSizes.push_back(fieldSize);
	arrDataTypes.push_back(sqlDataType);
	arrDatabaseFieldTypes.push_back(databaseFieldType);
	return fieldSize;
}

SqlWrapper::SqlWrapper(const char * fieldTypes, const char * sqlQuery)
{
	m_fieldCount = 0;
	int structureSize = 0;
	vector<int> arrFieldSizes;
	vector<eRelationalDatabaseFieldType> arrDatabaseFieldTypes;

	string s(fieldTypes);
	string delimiter = ",";
	size_t pos = 0;
	while((pos = s.find(delimiter)) != string::npos)
	{
		structureSize += GetFieldSize(s.substr(0, pos), arrFieldSizes, m_arrDataTypes, arrDatabaseFieldTypes);
		m_fieldCount++;
		s.erase(0, pos + delimiter.length());
	}
	structureSize += GetFieldSize(s.substr(0, pos), arrFieldSizes, m_arrDataTypes, arrDatabaseFieldTypes);
	m_fieldCount++;

	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(m_fieldCount, structureSize);
	long offset = 0;
	for(int i = 0; i<m_fieldCount; i++) {
		gabarit->Add(offset, arrFieldSizes.at(i), arrDatabaseFieldTypes.at(i));
		offset += arrFieldSizes.at(i);
		//gabarit->AddTail(arrFieldSizes.at(i), arrDatabaseFieldTypes.at(i));
	}
	sophisTools::csrvector csrVector;
	COMM(sqlQuery);
	QueryWithNResultsVector(sqlQuery, gabarit, csrVector);
	delete gabarit;
	for(int idx=0; idx<csrVector.nb_elem(); idx++) {
		vector<Variant> variantRow;
		const char * oneRow = (const char *)csrVector[idx];
		offset = 0;
		for(int i=0; i<m_fieldCount; i++) {
			switch(m_arrDataTypes.at(i)) {
			case eLong:
				{
					long lValue;
					memcpy(&lValue, oneRow + offset, arrFieldSizes.at(i));
					variantRow.push_back(Variant(lValue));
				}
				break;
			case eChar:
				{
					char * strValue = new char[arrFieldSizes.at(i) + 1];
					strValue[arrFieldSizes.at(i)] = '\0';
					memcpy(strValue, oneRow + offset, arrFieldSizes.at(i));
					variantRow.push_back(Variant(string(strValue)));
					delete [] strValue;
				}
				break;
			case eDouble:
				{
					double dValue;
					memcpy(&dValue, oneRow + offset, arrFieldSizes.at(i));
					variantRow.push_back(Variant(dValue));
				}
				break;
			}
			offset += arrFieldSizes.at(i);
		}
		m_arrResults.push_back(variantRow);
	}
}

vector<SqlWrapper::eSqlDataType> SqlWrapper::GetDataTypes()
{
	return m_arrDataTypes;
}

int SqlWrapper::GetRowCount()
{
	return m_arrResults.size();
}

int SqlWrapper::GetFieldCount()
{
	return m_fieldCount;
}

const vector<Variant> SqlWrapper::operator[](int i) const
{
	return m_arrResults.at(i);
}

vector<Variant> SqlWrapper::operator[](int i)
{
	return m_arrResults.at(i);
}

SqlWrapper::~SqlWrapper(void)
{
	//for(int i=0; i<m_arrResults.size(); i++) {
	//	vector<Variant> oneRow = m_arrResults.at(i);
	//	for(int j=0; j<oneRow.size(); j++) {
	//		Variant v = oneRow.at(j);
	//		if (v.type() == typeid(string)) {
	//		}
	//	}
	//}
}

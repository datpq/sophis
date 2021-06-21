/*
** Includes
*/
// specific
#include "CSxALSortByPostingDateAndTradeId.h"
#include "SphSDBCInc/SphSQLQuery.h"

/*
** Namespace
*/
using namespace sophis::accounting;


/*
** Statics
*/
const char* CSxALSortByPostingDateAndTradeId::__CLASS__ = "CSxALSortByPostingDateAndTradeId";
const bool CSxALSortByPostingDateAndTradeId::isOldSortAL = CSxALSortByPostingDateAndTradeId::OldSortAL();
/*
** Methods
*/

//-------------------------------------------------------------------------------------------------------------
int CSxALSortByPostingDateAndTradeId::FirstToSort(const SSFinalPostingAuxID &a, const SSFinalPostingAuxID &b, int splitPostingType) const
{
	if(isOldSortAL)
	{
		if (a.fAmount == 0 && a.fQuantity != 0 && (a.fPostingType == splitPostingType) && b.fPostingType != splitPostingType)
		{
			if (a.fPostingDate <b.fPostingDate ) return -1;
			else return 1;
		}
		if (b.fAmount == 0 && b.fQuantity != 0 && 
			(b.fPostingType == splitPostingType))
		{ 
			if (a.fPostingDate >= b.fPostingDate) return 1;
			else return -1;
		}
	}
	else
	{
		if(a.fPostingDate < b.fPostingDate)
			return -1;
		if(a.fPostingDate > b.fPostingDate)
			return 1;
		if(a.fPostingType == splitPostingType)
		{
			if(b.fPostingType == splitPostingType)
				return 0;
			else
				return -1;
		}
		else if( b.fPostingType == splitPostingType)
			return 1;
	}
	return 0;
}

//-------------------------------------------------------------------------------------------------------------
bool CSxALSortByPostingDateAndTradeId::OldSortAL()
{
	struct SPref
	{
		int value;
	} pref;

	CSRStructureDescriptor structureDescriptor(1, sizeof(SPref));
	ADD(&structureDescriptor, SPref, value, rdfInteger);

	_STL::string query("SELECT PREFVALEUR FROM RISKPREF WHERE PREFNOM = 'OldSortAL'");

	//DPH
	//short err = CSRSqlQuery::QueryWith1Result(query.c_str(),&structureDescriptor, &pref);
	errorCode err = CSRSqlQuery::QueryWith1ResultWithoutParam(query.c_str(), &structureDescriptor, &pref);
	return (err == 0) ? (pref.value != 0) : false;
}

/*virtual*/ bool CSxALSortByPostingDateAndTradeId::IsLower(const SSFinalPostingAuxID &f1 , const SSFinalPostingAuxID &f2) const
{
	int res = FirstToSort(f1,f2,fSplitPostingType);
	if(res == -1)
		return true;
	if(res == 1)
		return false;

	// We would like Trade id by growing : first filter
	if (f1.fPostingDate != f2.fPostingDate)
		return (f1.fPostingDate < f2.fPostingDate);		

	// We would like Generation date by growing : second filter
	if (f1.fTradeID != f2.fTradeID)
		return f1.fTradeID < f2.fTradeID;

	// We would like Posting id by growing : third filter
	if (f1.fID != f2.fID)
		return f1.fID < f2.fID;

	return DefaultIsLower(f1,f2);
}

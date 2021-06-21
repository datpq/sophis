#include <stdio.h>

#include "SphInc/SphMacros.h"
#include "SphInc/gui/SphDialog.h"
#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(string)
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/backoffice_kernel/SphBusinessEvent.h"

#include "Constants.h"
#include "CSxTVARates.h"

using namespace sophis::gui;
using namespace sophis::backoffice_kernel;
using namespace sophis::sql;

/*static*/ const char* CSxTVARates::__CLASS__ = "CSxTVARates";
/*static*/ _STL::map<long, CSxTVARates::SSxTVARatesData> CSxTVARates::fTVARatesList;
/*static*/ _STL::map<long, long> CSxTVARates::fBusinessEventTVARateMap;


CSxTVARates::CSxTVARates()
{
}

/*static*/ void CSxTVARates::GetData()
{
	fTVARatesList.clear();	

	SSxTVARatesData *resultBuffer = NULL;
	int		 nbResults = 0;

	CSRStructureDescriptor	desc(4, sizeof(SSxTVARatesData));

	ADD(&desc, SSxTVARatesData, fRateId, rdfInteger);
	ADD(&desc, SSxTVARatesData, fRateType, rdfString);
	ADD(&desc, SSxTVARatesData, fRateName, rdfString);
	ADD(&desc, SSxTVARatesData, fRate, rdfFloat);	


	_STL::string sqlQuery = "select R.ID,T.NAME,R.NAME,R.RATE from CFG_TVA_RATES R,CFG_TVA_RATE_TYPE T where R.TYPE_ID = T.ID";			

	//DPH
	//CSRSqlQuery::QueryWithNResults(sqlQuery.c_str(), &desc, (void **) &resultBuffer, &nbResults);
	CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery.c_str(), &desc, (void **)&resultBuffer, &nbResults);


	for (int i = 0; i < nbResults ; i++)
	{
		fTVARatesList[resultBuffer[i].fRateId] = resultBuffer[i];
	}

	free((char*)resultBuffer);
}

void CSxTVARates::SaveData(_STL::map<long, CSxTVARates::SSxTVARatesData>& listOfTVARates)
{	
	if (listOfTVARates.size() > 0)
	{
		_STL::map<long, CSxTVARates::SSxTVARatesData>::iterator iter;

		for (iter = listOfTVARates.begin(); iter != listOfTVARates.end();iter++)
		{
			char queryBuffer[QUERY_BUFFER_SIZE];
			sprintf_s(queryBuffer,QUERY_BUFFER_SIZE,"update CFG_TVA_RATES set RATE = %lf where ID = %ld",(iter->second).fRate,iter->first);
			//DPH
			//CSRSqlQuery::QueryWithoutResult(queryBuffer);
			CSRSqlQuery::QueryWithoutResultAndParam(queryBuffer);
		}

		CSRSqlQuery::Commit();
	}	
}

int CSxTVARates::GetTVARatesListSize()
{
	return (int)fTVARatesList.size();
}

void CSxTVARates::GetTVARatesList(_STL::map<long, SSxTVARatesData>& listOfTVARates)
{
	listOfTVARates.clear();
	
	_STL::map<long, SSxTVARatesData>::iterator iter;

	for (iter = fTVARatesList.begin(); iter != fTVARatesList.end();iter++)
		listOfTVARates[iter->first] = iter->second;
}

/*static*/ double CSxTVARates::GetTVARate(long rateId)
{
	if (fTVARatesList.empty())
	{
		GetData();
	}
	
	double ret = 0.;
	
	_STL::map<long, SSxTVARatesData>::iterator iter = fTVARatesList.find(rateId);
	
	if (iter != fTVARatesList.end())
		ret = fTVARatesList[rateId].fRate;

	return ret;
}

void CSxTVARates::SetTVARate(long rateId,double rate)
{
	_STL::map<long, SSxTVARatesData>::iterator iter = fTVARatesList.find(rateId);
	if (iter != fTVARatesList.end())
		fTVARatesList[rateId].fRate = rate;
}

/*static*/ long CSxTVARates::GetTVARateIdFromBE(long BEId)
{
	long ret = 0;

	if (fBusinessEventTVARateMap.empty())
	{
		struct SSResult
		{
			long fBEId;
			long fTVARateId;
		};

		SSResult *resultBuffer = NULL;
		int		 nbResults = 0;

		CSRStructureDescriptor	desc(2, sizeof(SSResult));

		ADD(&desc, SSResult, fBEId, rdfInteger);
		ADD(&desc, SSResult, fTVARateId, rdfInteger);

		_STL::string sqlQuery = "select BE_ID,CFG_TVA_RATE_ID from CFG_BUSINESS_EVENT_RATE";			

		//DPH
		//CSRSqlQuery::QueryWithNResults(sqlQuery.c_str(), &desc, (void **) &resultBuffer, &nbResults);
		CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery.c_str(), &desc, (void **)&resultBuffer, &nbResults);


		for (int i = 0; i < nbResults ; i++)
		{
			fBusinessEventTVARateMap[resultBuffer[i].fBEId] = resultBuffer[i].fTVARateId;
		}

		free((char*)resultBuffer);
	}

	_STL::map<long, long>::iterator iter = fBusinessEventTVARateMap.find(BEId);

	if (iter != fBusinessEventTVARateMap.end())
		ret = iter->second;

	return ret;
}
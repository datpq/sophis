#ifndef __CSxTVARates__H__
#define __CSxTVARates__H__

#include "SphInc/SphMacros.h"
#include __STL_INCLUDE_PATH(map)
#include "..\CFG_ReposExport.h"
#include "Constants.h"

#pragma warning(push)
#pragma warning(disable:4251) // 'CSxTVARates::fTVARatesList' : class 'stlp_std::map<_Key,_Tp>' needs to have dll-interface to be used by clients of class 'CSxTVARates'



class CFG_REPOS CSxTVARates 
{
public:

	CSxTVARates();

	struct SSxTVARatesData
	{
		long	fRateId;
		char	fRateType[NAME_SIZE+1];
		char	fRateName[NAME_SIZE+1];
		double	fRate;
	};

	static void GetData();	
	static void SaveData(_STL::map<long, CSxTVARates::SSxTVARatesData>& listOfTVARates);

	static int GetTVARatesListSize();
	static void GetTVARatesList(_STL::map<long, SSxTVARatesData>& listOfTVARates);
	static double GetTVARate(long rateId);
	static void SetTVARate(long rateId,double rate);
	static long GetTVARateIdFromBE(long BEId);

private:
	static const char* __CLASS__;

	static _STL::map<long, SSxTVARatesData> fTVARatesList;
	static _STL::map<long, long> fBusinessEventTVARateMap;
};

#pragma warning(pop)

#endif // __CSxTVARates__H__
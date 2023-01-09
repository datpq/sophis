#pragma once
//#include "SphInc/SphIncludes.h"
#include <stddef.h>
#include <stdlib.h>
#include <string.h>

class CSxCorporateActionUtil
{
public:
	//static _STL::map<_STL::string,long> GetAccountFundMap();
	static bool CreateFreeAttribution(int sicovam, double coefficient, int businessEvent, int exDivDate, int date, int paymentDate, char* comment = NULL);
	static bool CreateMerger(int sicovam, int take_over, double coefficient, double convRatioNum, int convRatioDenom, int businessEvent, int exDivDate, int date, int paymentDate, bool withAveragePrice = false, char* comment = NULL);
	static bool CreateDemerger(int sicovam, int diffused_code, double coefficient, double convRatioNum, int convRatioDenom, int businessEvent, int exDivDate, int date, int paymentDate, char* comment = NULL);
	CSxCorporateActionUtil(void);
	~CSxCorporateActionUtil(void);
private:
	static const char * __CLASS__;
};

#ifndef __CSxInterest__H__
#define __CSxInterest__H__

#include "SphInc/instrument/SphLoanAndRepo.h"
#include "SphInc/collateral/SphLoanAndRepoDialog.h"


class CSxInterest 
{
public:	
	
	static double GetSLRoundedInterest(const sophis::instrument::CSRLoanAndRepo* loanAndRepo, double amount, double spreadHT, long startDate, long endDate, 
												double quantity, sophis::collateral::eDealDirection dealDirection, double &interestAmountHT);	
	
	static int GetNbDecimalsForRounding();

private:
	/*
	** Logger data
	*/
	static const char* __CLASS__;

	static bool fIsNbDecimalsForRoundingLoaded;
	static int fNbDecimalsForRounding;	
};

#endif // __CSxInterest__H__
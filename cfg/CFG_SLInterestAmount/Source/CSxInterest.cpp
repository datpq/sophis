#pragma warning(disable:4251)
/*
** Includes
*/

#include "Constants.h"
#include "sophismath.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphTools/SphLoggerUtil.h"
#include "CSxInterest.h"
#include "../../CFG_Repos/Source/CSxTVARates.h"

/*
** Namespace
*/
using namespace sophis::instrument;
using namespace sophis::collateral;
using namespace sophis::sql;

/*
** Static
*/
const char* CSxInterest::__CLASS__ = "CSxInterest";
bool CSxInterest::fIsNbDecimalsForRoundingLoaded = false;
int CSxInterest::fNbDecimalsForRounding = 0;

/*
** Methods
*/
double CSxInterest::GetSLRoundedInterest(const CSRLoanAndRepo* loanAndRepo, double amount, double spreadHT, long startDate, long endDate, double quantity, eDealDirection dealDirection, double &interestAmountHT)
{
	BEGIN_LOG("GetSLRoundedInterest");

	double interestAmount = 0.; 
	
	//Compute the interests HT		

	CSRLoanAndRepo* loanAndRepoClone = dynamic_cast<CSRLoanAndRepo*>(loanAndRepo->Clone());

	if (loanAndRepoClone)
	{				
		if (endDate && startDate && quantity)
			interestAmountHT = spreadHT*(endDate-startDate)/360.*fabs(amount)/quantity;		

		int nbDecimalsForRoundingInterests = GetNbDecimalsForRounding();
		
		if (nbDecimalsForRoundingInterests > 0)
			interestAmountHT = GetRoundedValue(nbDecimalsForRoundingInterests,interestAmountHT);
		MESS(Log::debug, "After rounding " << interestAmountHT);

		interestAmountHT *= quantity;		
		MESS(Log::debug, "Multiplied by quantity " << interestAmountHT);

		//Don't forget to add TVA for 'Mise en pension' case
		if (loanAndRepoClone->GetLoanAndRepoType() ==  larStockLoan && dealDirection == eLend || loanAndRepoClone->GetLoanAndRepoType() ==  larRepo && dealDirection == eBorrow)
		{
			double TVARate = CSxTVARates::GetTVARate(REPO_BORROWING_TVA_RATE_ID);
			interestAmount = interestAmountHT*(1+TVARate);
			MESS(Log::debug, "Adjusted by the TVA " << interestAmount);
		}
		else
		{
			interestAmount = interestAmountHT;
			MESS(Log::debug, "Non adjusted by the TVA " << interestAmount);
		}

		delete loanAndRepoClone;
	}
	
	END_LOG();
	return interestAmount;
}

//--------------------------------------------------------------------------------------------------------------
int CSxInterest::GetNbDecimalsForRounding()
{	
	if (!fIsNbDecimalsForRoundingLoaded)
	{
		struct SSxResult
		{	
			char	fNbDecimalBuffer[41];		
		};

		SSxResult *resultBuffer = NULL;
		int		 nbResults = 0;

		CSRStructureDescriptor	desc(1, sizeof(SSxResult));

		ADD(&desc, SSxResult, fNbDecimalBuffer, rdfString);		

		char query[QUERY_BUFFER_SIZE];
		sprintf_s(query,QUERY_BUFFER_SIZE,"select PREFVALEUR from RISKPREF where PREFNOM = 'StockLoanRoundedInterest'");

		//DPH
		//CSRSqlQuery::QueryWithNResults(query, &desc, (void **) &resultBuffer, &nbResults);
		CSRSqlQuery::QueryWithNResultsWithoutParam(query, &desc, (void **)&resultBuffer, &nbResults);

		if (nbResults > 0)
			fNbDecimalsForRounding = atoi(resultBuffer[0].fNbDecimalBuffer);

		free((char*)resultBuffer);

		fIsNbDecimalsForRoundingLoaded = true;
	}	

	return fNbDecimalsForRounding;
}


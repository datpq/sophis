
#pragma warning(disable:4251) // '...' : struct '...' needs to have dll-interface to be used by clients of class '...')
#include "SphTools/SphLoggerUtil.h"
#include "SphSDBCInc/internal/g_oci.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphLLInc/Sphtools/compatibility/globalsophis.h"
#include "SphInc/portfolio/SphPortfolioColumn.h"
#include "SphInc/portfolio/SphExtraction.h"
#include "SphInc/value/kernel/SphFundFeesPeriods.h"
#include "SphInc/value/kernel/endOfDay/SphAmEODData.h"
#include __STL_INCLUDE_PATH(string)
#include "CSxManagementFees.h"
#include "Constants.h"
#define EXCLUDE_KEY_WORD	"[Exclus]"


using namespace sophis;
using namespace sophis::value;
using namespace sophis::sql;
using namespace sophis::portfolio;
using namespace sophis::instrument;
using namespace _STL;

#define IS_EQUAL(A, B)		( (fabs((A) - (B)) < 1e-8) ? TRUE : FALSE )

const char* CSxManagementFees::__CLASS__ = "CSxManagementFees";

DEFINE_FUND_FEES_CLASS(CSxManagementFees) // performs Clone() implementation

//---------------------------------------------------------------------------------------------------------------------------------------------
CSxManagementFees::CSxManagementFees() : CSAMFundManagementFees()
{
	fModeChoice = acStandard;
	fNAVType = ntBeginning;
	fRatesPerLevelList.clear();
}

//---------------------------------------------------------------------------------------------------------------------------------------------
void CSxManagementFees::Initialise()
{
	CSAMFundManagementFees::Initialise();
	
	fModeChoice	= acStandard;
	fNAVType = ntBeginning;

	//Rate per level
	Load(0);
}

//---------------------------------------------------------------------------------------------------------------------------------------------
void CSxManagementFees::Load(int i)
{
	SSxRatePerLevel *resultBuffer = NULL;
	int			   nbResults = 0;

	CSRStructureDescriptor	desc(2, sizeof(SSxRatePerLevel));

	ADD(&desc, SSxRatePerLevel, fLevel, rdfFloat);	
	ADD(&desc, SSxRatePerLevel, fRate, rdfFloat);

	string sqlQuery = FROM_STREAM("select CFG_LEVEL,RATE from CFGMANAGEMENT_FEES_BY_LEVEL where ID = " << i << " order by CFG_LEVEL");		

	//DPH
	//CSRSqlQuery::QueryWithNResults(sqlQuery.c_str(), &desc, (void **) &resultBuffer, &nbResults);
	CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery.c_str(), &desc, (void **)&resultBuffer, &nbResults);

	fRatesPerLevelList.clear();	

	for (int i = 0; i < nbResults ; i++)
	{		
		fRatesPerLevelList.push_back(resultBuffer[i]);		
	}

	delete (char*)resultBuffer;
}

//---------------------------------------------------------------------------------------------------------------------------------------------
void CSxManagementFees::Initialise(const CSAMFundFees* fees)
{
	CSAMFundManagementFees::Initialise(fees);
	
	const CSxManagementFees* manFees = dynamic_cast<const CSxManagementFees*>(fees);
	if (manFees)
	{		
		fModeChoice	= manFees->fModeChoice;

		// rates per market list 		
		fRatesPerLevelList.clear();
		fRatesPerLevelList = manFees->fRatesPerLevelList;
	}
	else
	{		
		fModeChoice	= acStandard;
		fNAVType = ntBeginning;
		// Load rates per level list 
		Load(0);
	}
}

//---------------------------------------------------------------------------------------------------------------------------------------------
double CSxManagementFees::Compute(const sophis::value::CSAMFund* fund, long date, double nav, double nbshares, double totalnav) const
{	
	BEGIN_LOG("Compute");
	double result = 0.;
	double sumExcludedFees = 0.;
	double sumFees = 0.;

	switch (fNAVType)
	{
	case ntDayToDay:
	{
		result = ComputeDayToDayFees(fund, date, nav, nbshares, totalnav);

		int feesDataCount = fund->GetFundFeesCount();

		for (int i = 0 ; i < feesDataCount; i++)
		{
			const CSAMFundFees* fees =fund->GetNthFundFees(i);

			if (fees)
			{
				double accountedFees = GetAccountedFeesDuringEOD(); // FOR DEBUG ONLY
				const CSxManagementFees* CFGFees = dynamic_cast<const CSxManagementFees*>(fees);

				if (!CFGFees)
				{																			
					_STL::string comment = fees->GetComment();
					_STL::string keyWord = EXCLUDE_KEY_WORD;
					size_t found = comment.find(keyWord);				 

					if (found == string::npos)
					{			
						double feesAmount = fees->Compute(fund, date, nav, nbshares, totalnav);
						MESS(Log::debug, "Fees " << i << " included : " << feesAmount);
						result -= feesAmount;
					}
					else
					{
						double feesAmount = fees->Compute(fund, date, nav, nbshares, totalnav);
						MESS(Log::debug, "Fees " << i << " excluded : " << feesAmount);
						sumExcludedFees += feesAmount;
					}

					sumFees += fees->Compute(fund, date, nav, nbshares, totalnav);
				}	
				else
				{
					MESS(Log::debug, "Fees " << i << " not management");
				}
			}
			else
			{
				MESS(Log::debug, "Fees " << i << " unknown");
			}
		}
	}
	break;
	default:
		{
			double alreadyAccounted = CSAMFundManagementFees::GetAlreadyAccountedFees(fund, date);
			double sincPeriodeStartFees = ComputeSincePeriodStart(fund, date, nav, nbshares, totalnav);	

			int feesDataCount = fund->GetFundFeesCount();

			for (int i = 0 ; i < feesDataCount; i++)
			{
				const CSAMFundFees* fees =fund->GetNthFundFees(i);

				if (fees)
				{
					double accountedFees = GetAccountedFeesDuringEOD(); 
					const CSxManagementFees * CFGFees = dynamic_cast<const CSxManagementFees*>(fees);
					const CSAMFundManagementFees * tempFees = dynamic_cast<const CSAMFundManagementFees *>(fees);

					//CheckPeriod(fund, date, fees);

					if (!CFGFees)
					{																			
						_STL::string comment = fees->GetComment();
						_STL::string keyWord = EXCLUDE_KEY_WORD;
						size_t found = comment.find(keyWord);				 

						if (found == string::npos)
						{		
							double feesAmount = fees->ComputeSincePeriodStart(fund, date, nav, nbshares, totalnav);
							MESS(Log::debug, "Fees " << i << " included : " << feesAmount);
							sincPeriodeStartFees -= feesAmount;
						}
						else
						{
							double feesAmount = fees->Compute(fund, date, nav, nbshares, totalnav);
							MESS(Log::debug, "Fees " << i << " excluded : " << feesAmount);
							sumExcludedFees += feesAmount;
						}

						sumFees += fees->Compute(fund, date, nav, nbshares, totalnav);
					}
					else
					{
						double accountedFeesTemp = fees->GetAlreadyAccountedFees(fund, date); // FOR DEBUG PURPOSE
						double feesAmountTemp = fees->ComputeSincePeriodStart(fund, date, nav, nbshares, totalnav);
						MESS(Log::debug, "Fees " << i << " is a Toolkit Management Fees");
					}
				}
				else
				{
					MESS(Log::warning, "Fees " << i << " is null");
				}

			}

			if (sincPeriodeStartFees <= 0.)
				return 0.;
			
			result = sincPeriodeStartFees - alreadyAccounted;
		}
	}
	
	double sum = result + sumFees; 
	MESS(Log::debug, "Total Fees (" << sum << ") = CFGManagement Fees (" << result << ") + Others (" << sumFees << ")");

	double amount = 0.;
	if (fAmountChoice == acNAV)
		amount = totalnav;//  - GetAccountedFeesDuringEOD();
	else if (fAmountChoice == acPtfCol)
	{
		const CSRPortfolioColumn* col = CSRPortfolioColumn::GetCSRPortfolioColumn(fPtfColName);
		if(fund && col)
		{
			SSCellValue cellValue;
			SSCellStyle cellStyle;
			eHedgeFundType hedgeFundType = fund->GetHedgeFundType();
			if (hedgeFundType == hfFundClass)
			{
				MESS(Log::warning, "Possible wrong fee calculation because wrong trading portfolio");
			}

			//DPH
			//col->GetPortfolioCell(fund->GetTradingPortfolio(), fund->GetTradingPortfolio(), &CSRExtraction::gMain, &cellValue, &cellStyle , true);
			//DPH 733
			//col->GetPortfolioCell(fund->GetTradingPortfolio(), fund->GetTradingPortfolio(), CSRExtraction::PSRMain, &cellValue, &cellStyle, true);
			col->GetPortfolioCell(fund->GetTradingPortfolio(), fund->GetTradingPortfolio(), CSRExtraction::GetMainPortfolioExtraction(), &cellValue, &cellStyle, true);
			amount = cellValue.floatValue;			
		}
	}

	double limit = amount*GetEquivalentRate(.024, fAccountingPeriod.first, fAccountingPeriod.second, fTimeBasis, fCalculationType); // Limit = 2.4%	

	if (sum > limit)
	{
		result = limit - sumFees;
	}
	
	END_LOG();
	return result;
			
}

//---------------------------------------------------------------------------------------------------------------------------------------------
/*static*/ bool CSxManagementFees::CompareSSxRatePerLevelElements(SSxRatePerLevel a, SSxRatePerLevel b)
{
	return (a.fLevel < b.fLevel); 
}

//---------------------------------------------------------------------------------------------------------------------------------------------
double CSxManagementFees::ComputePerLevelFeesInPeriod(const CSAMFund * fund, long date, long startDate, long endDate, double baseAmount) const
{
	BEGIN_LOG("ComputePerLevelFeesInPeriod");

	if (baseAmount <= 0.)
	{
		MESS(Log::debug, "CFG Management amount of fee " << fCustomInfosId << " at date " << date << " is <= 0");
		END_LOG();
		return 0.;
	}	

	assert(baseAmount>=0.);

	size_t nbLevels = fRatesPerLevelList.size();

	if (nbLevels == 0)
	{
		MESS(Log::debug, "The rates per level list of fee " << fCustomInfosId << " is empty. You should set this list with at least one level");
		END_LOG();
		return 0.;
	}

	const SSPeriodDates& periodDates = GetPeriodDates();

	_STL::vector<double> listOfMaxValuesPerLevel;

	for (unsigned int i = 0; i < nbLevels-1; i++)
	{
		double level1 = fRatesPerLevelList[i+1].fLevel;
		double level0 = fRatesPerLevelList[i].fLevel;
		double rate0 = fRatesPerLevelList[i].fRate;
		double equivalentRate = GetEquivalentRate(rate0*0.01, startDate, endDate, fTimeBasis, fCalculationType);
		double calculated = (level1-level0)*equivalentRate;
		listOfMaxValuesPerLevel.push_back(calculated);
	}

	double result = 0.;

	for (unsigned int i = 0; i < nbLevels; i++)
	{
		if (baseAmount > fRatesPerLevelList[nbLevels-i-1].fLevel)
		{
			double rateLevel = fRatesPerLevelList[nbLevels-i-1].fLevel;
			double rateRate = fRatesPerLevelList[nbLevels-i-1].fRate;
			double equivalentRate = GetEquivalentRate(rateRate*0.01, startDate, endDate, fTimeBasis, fCalculationType);
			result = (baseAmount-rateLevel)*equivalentRate;
			for (unsigned int j = 0; j < nbLevels-i-1; j++)
			{
				result += listOfMaxValuesPerLevel[j];
			}
			break;
		}			
	}	

	MESS(Log::debug, "CFG Management fee " << fCustomInfosId << " at date " << date << " since period start " << 
		periodDates.fStartFirstAccountingPeriod << ": " << result);

	END_LOG();
	return result;
}

//---------------------------------------------------------------------------------------------------------------------------------------------
double	CSxManagementFees::ComputeDayToDayFees(	const sophis::value::CSAMFund * fund, 
												long date, 
												double nav, 
												double nbshares, 
												double totalnav) const
{
	BEGIN_LOG("ComputeDayToDayFees");

	double amount = 0.;
	if (fAmountChoice == acNAV)
		amount = totalnav;
	else if (fAmountChoice == acPtfCol)
	{
		const CSRPortfolioColumn* col = CSRPortfolioColumn::GetCSRPortfolioColumn(fPtfColName);
		if(fund && col)
		{
			SSCellValue cellValue;
			SSCellStyle cellStyle;
			long fundTradingFolio(0);
			eHedgeFundType hedgeFundType = fund->GetHedgeFundType();
			if (hedgeFundType!=hfFundClass)
			{
				fundTradingFolio = fund->GetTradingPortfolio();
			}
			else
			{
				// This is a fund class, get the trading folio of its multi-class fund
				long multiClassFundCode = 0;
				errorCode errorSQL = CSRSqlQuery::QueryReturning1Long(FROM_STREAM("SELECT Issuer FROM Titres WHERE Sicovam = " << fund->GetCode() << ""), &multiClassFundCode);
				if (errorSQL != 0)
				{
					MESS(Log::error, "Failed to get Multi Class fund for sicovam " << fund->GetCode() << " (oracle error " << errorSQL << ")");
					END_LOG();
					return 0.0;
				}

				if (multiClassFundCode)
				{
					const CSAMFund * multiClassFund = CSAMFund::GetFund(multiClassFundCode);
					if (multiClassFund)
						fundTradingFolio = multiClassFund->GetTradingPortfolio();
					else
					{
						MESS(Log::error, "Failed to find fund " << multiClassFundCode);
						END_LOG();
						return 0.0;
					}
				}
				else
				{
					MESS(Log::error, "Failed to get Fund Class for fund " << fund->GetCode() << ", type " << hedgeFundType);
					END_LOG();
					return 0.0;
				}
			}

			//DPH
			//col->GetPortfolioCell(fundTradingFolio, fundTradingFolio, &CSRExtraction::gMain, &cellValue, &cellStyle , true);
			//DPH 733
			//col->GetPortfolioCell(fundTradingFolio, fundTradingFolio, CSRExtraction::PSRMain, &cellValue, &cellStyle, true);
			col->GetPortfolioCell(fundTradingFolio, fundTradingFolio, CSRExtraction::GetMainPortfolioExtraction(), &cellValue, &cellStyle, true);
			amount = cellValue.floatValue;// + GetAccountedFeesDuringEOD(); // we have to add Accounted fees as we retrieve NAV from the portfolio column because fees are not taken into account
			MESS(Log::debug, "Portfolio column " << fPtfColName << " used: " << amount);
		}
	}

	double result = ComputeFeesInPeriod(fund, date, fAccountingPeriod.first, fAccountingPeriod.second, amount);

	END_LOG();
	return result;
}

//---------------------------------------------------------------------------------------------------------------------------------------------
double CSxManagementFees::ComputeFeesInPeriod(const CSAMFund * fund, long date, long startDate, long endDate, double baseAmount) const
{
	BEGIN_LOG("ComputeFeesInPeriod");

	double result = 0.;

	if (fModeChoice == acStandard)
	{
		result = CSAMFundManagementFees::ComputeFeesInPeriod(fund,date,startDate,endDate,baseAmount);
	}
	else
	{
		result = ComputePerLevelFeesInPeriod(fund,date,startDate,endDate,baseAmount);
	}

	END_LOG();
	return result;
}

////---------------------------------------------------------------------------------------------------------------------------------------------
//double CSxManagementFees::ComputeSincePeriodStart(const CSAMFund * fund, long date, double nav, double nbshares, double totalnav) const
//{
//	BEGIN_LOG("ComputeSincePeriodStart");
//
//	double amount = 0.;
//	if (fAmountChoice == acNAV)
//		amount = GetTotalNav(fund, date, totalnav);
//	else if (fAmountChoice == acPtfCol)
//	{
//		const CSRPortfolioColumn* col = CSRPortfolioColumn::GetCSRPortfolioColumn(fPtfColName);
//		if(fund && col)
//		{
//			SSCellValue cellValue;
//			SSCellStyle cellStyle;
//			long fundTradingFolio(0);
//			eHedgeFundType hedgeFundType = fund->GetHedgeFundType();
//			//if (hedgeFundType!=hfFundClass) 
//				fundTradingFolio = fund->GetTradingPortfolio();
//			//else
//			//{
//			//	// this is a fund class, get the trading folio of its multi-class fund
//			//	long multiClassFundCode = CSAMFundClassLinker::GetInstance()->GetMultiClassFund(fund->GetCode()) ;
//			//	if (multiClassFundCode)
//			//	{
//			//		const CSAMFund* multiClassFund = CSAMFund::GetFund(multiClassFundCode);
//			//		if (multiClassFund)
//			//			fundTradingFolio = multiClassFund->GetTradingPortfolio();
//			//	}
//			//}
//			col->GetPortfolioCell(fundTradingFolio, fundTradingFolio, &CSRExtraction::gMain, &cellValue, &cellStyle , true);
//			MESS(Log::debug, "Portfolio column " << fPtfColName << " used: " << amount);
//		}
//	}
//
//	const SSPeriodDates& periodDates = GetPeriodDates();
//	AccountingPeriod date3 = GetAccountingPeriod();
//	double result = ComputeFeesInPeriod(fund, date, periodDates.fStartFirstAccountingPeriod, fAccountingPeriod.second, amount);
//
//	END_LOG();
//	return result;
//}

//---------------------------------------------------------------------------------------------------------------------------------------------
//DPH
//short CSxManagementFees::ReadCustomInfos(long id)
sophis::sql::errorCode CSxManagementFees::ReadCustomInfos(long id)
{
	Load(id);
	
	struct tmpDB
	{
		int fAmountChoice;
		int fNAVType;
		char fPtfColName[40];
		double fRate;
		double fRateRangeMin;
		double fRateRangeMax;
		int fModeChoice;
	} tmp;

	sophis::sql::CSRStructureDescriptor fDesc (7, sizeof(tmpDB), false);
	ADD(&fDesc, tmpDB, fRate, rdfFloat);
	ADD(&fDesc, tmpDB, fRateRangeMin, rdfFloat);
	ADD(&fDesc, tmpDB, fRateRangeMax, rdfFloat);
	ADD(&fDesc, tmpDB, fNAVType, rdfInteger);
	ADD(&fDesc, tmpDB, fAmountChoice, rdfInteger);
	ADD(&fDesc, tmpDB, fPtfColName, rdfString);
	ADD(&fDesc, tmpDB, fModeChoice, rdfInteger);

	char req[SIZE_BUFFER];
	char tableName[256];
	GetTableName(tableName);
	sprintf_s(req, "select rate, rate_range_min, rate_range_max, navtype, amount_type, ptfcol_name, mode_type from %s where id=%ld", tableName, id);
	//DPH
	//errorCode err = CSRSqlQuery::QueryWith1Result(req, &fDesc, &tmp);
	errorCode err = CSRSqlQuery::QueryWith1ResultWithoutParam(req, &fDesc, &tmp);

	if (err==noErr)
	{
		fAmountChoice = (eAmountChoice)tmp.fAmountChoice;
		fNAVType = (eNAVType)tmp.fNAVType;
		strcpy_s(fPtfColName,39,tmp.fPtfColName);
		fRate = tmp.fRate;
		fRateRangeMin = tmp.fRateRangeMin;
		fRateRangeMax = tmp.fRateRangeMax;
		fModeChoice = (eModeChoice)tmp.fModeChoice;
	}

	return err;
}

//---------------------------------------------------------------------------------------------------------------------------------------------
//DPH
//short CSxManagementFees::WriteCustomInfos(long* id, const CSAMFund * fund) const
sophis::sql::errorCode CSxManagementFees::WriteCustomInfos(long* id, const CSAMFund * fund) const
{		
	if (id) *id = 0;
	char req[SIZE_BUFFER];
	char tableName[256];
	char seqName[256];
	GetTableName(tableName);
	GetSeqName(seqName);
	long nextVal = 0;
	//DPH
	//short err = O_GetNextVal(&nextVal, tableName, seqName);
	sophis::sql::errorCode err = O_GetNextVal(&nextVal, tableName, seqName);
	if (err) return err;
	sprintf_s(req, "insert into %s (id, rate, rate_range_min, rate_range_max, navtype, amount_type, ptfcol_name,mode_type) values (%ld, %f, %f, %f, %ld, %ld, '%s',%ld)",
		tableName, nextVal, fRate, fRateRangeMin, fRateRangeMax, fNAVType, fAmountChoice, fPtfColName, fModeChoice);
	//DPH
	//err = CSRSqlQuery::QueryWithoutResult(req);
	err = CSRSqlQuery::QueryWithoutResultAndParam(req);
	if (err) return err;
	
	// Remember rates per level		

	for (unsigned int i = 0; i < fRatesPerLevelList.size(); i++)
	{
		char sqlQueryBuffer[SIZE_BUFFER];
		sprintf_s(sqlQueryBuffer,SIZE_BUFFER,"insert into CFGMANAGEMENT_FEES_BY_LEVEL(ID,CFG_LEVEL,RATE) values (%ld,%lf,%lf)",nextVal,
										fRatesPerLevelList[i].fLevel,fRatesPerLevelList[i].fRate);

		//DPH
		//err = CSRSqlQuery::QueryWithoutResult(sqlQueryBuffer);
		err = CSRSqlQuery::QueryWithoutResultAndParam(sqlQueryBuffer);
		if (err) return err;
	}
			
	CSRSqlQuery::Commit();
	
	if (id) *id = nextVal;	
	return noErr;
}

//---------------------------------------------------------------------------------------------------------------------------------------------
//DPH
//short CSxManagementFees::UpdateCustomInfos(const CSAMFund* fund) const
sophis::sql::errorCode CSxManagementFees::UpdateCustomInfos(const CSAMFund* fund) const
{
	char req[SIZE_BUFFER];
	char tableName[256];

	GetTableName(tableName);

	sprintf_s(req, "update %s set rate=%f, rate_range_min=%f, rate_range_max=%f, navtype=%ld, amount_type=%ld, ptfcol_name='%s', mode_type = %ld where id=%ld",
		tableName, fRate, fRateRangeMin, fRateRangeMax, fNAVType, fAmountChoice, fPtfColName, fModeChoice, fCustomInfosId);

	//DPH
	//short err = CSRSqlQuery::QueryWithoutResult(req);
	sophis::sql::errorCode err = CSRSqlQuery::QueryWithoutResultAndParam(req);
	if (err) return err;

	//Update CFGMANAGEMENT_FEES_BY_LEVEL table	
	
	char sqlQueryDeleteBuffer[SIZE_BUFFER];
	sprintf_s(sqlQueryDeleteBuffer,SIZE_BUFFER,"delete from CFGMANAGEMENT_FEES_BY_LEVEL where ID = %ld", fCustomInfosId);
	//DPH
	//err = CSRSqlQuery::QueryWithoutResult(sqlQueryDeleteBuffer);
	err = CSRSqlQuery::QueryWithoutResultAndParam(sqlQueryDeleteBuffer);
	if (err) return err;

	for (unsigned int i = 0; i < fRatesPerLevelList.size(); i++)
	{
		char sqlQueryBuffer[SIZE_BUFFER];			
		sprintf_s(sqlQueryBuffer,SIZE_BUFFER,"insert into CFGMANAGEMENT_FEES_BY_LEVEL(ID,CFG_LEVEL,RATE) values (%ld,%lf,%lf)",fCustomInfosId,
													fRatesPerLevelList[i].fLevel,fRatesPerLevelList[i].fRate);
		
		//DPH
		//err = CSRSqlQuery::QueryWithoutResult(sqlQueryBuffer);
		err = CSRSqlQuery::QueryWithoutResultAndParam(sqlQueryBuffer);
		if (err) return err;
	}

	CSRSqlQuery::Commit();
		
	return noErr;
}

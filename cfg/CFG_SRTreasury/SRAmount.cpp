#pragma warning(disable:4251)
/*
** Includes
*/
// specific
#include "SRAmount.h"
#include "SphInc/portfolio/SphPosition.h" 
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/SphPreference.h"
#include "SphInc/market_data/SphMarketData.h" 
#include "SphInc/value/kernel/SphFund.h" 
#include "Sphinc/portfolio/SphPortfolio.h"
#include "SphSDBCInc/SphSQLQuery.h" 
#include <stdio.h>
#include "SphInc\value\kernel\SphAMPosition.h"

using _STL::map;

int SRAmount::refreshVersion(-1);
map<long,double> SRAmount::cache;

/* 
** Methods 
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(SRAmount, "Uncataloged")

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void SRAmount::GetPortfolioCell(	long				activePortfolioCode,
															long				portfolioCode,
															//DPH
															//const CSRExtraction *extraction,
															PSRExtraction		extraction,
															SSCellValue			*cellValue,
															SSCellStyle			*cellStyle,
															bool				onlyTheValue) const
{
	cellStyle->kind = NSREnums::dDouble;

	if(refreshVersion != GetRefreshVersion())
	{
		cache.clear();
		refreshVersion = GetRefreshVersion();
	}


	if(!onlyTheValue) 
	{
		cellStyle->alignment	= aRight;
		cellStyle->decimal		= 2;
		cellStyle->null			= nvZeroAndUndefined;
		cellStyle->style		= tsBold;
	} 

	

	double Value=0;
	
	const CSRPortfolio * SomePortfolio =CSRPortfolio::GetCSRPortfolio(portfolioCode, extraction);
	if(!SomePortfolio)
		return ;

	




	const sophis::value::CSAMFund * ElderFund = sophis::value::CSAMFund::GetFundFromFolio(portfolioCode, extraction); 
	if (!ElderFund) 
		return ;

	long currency=ElderFund->GetCurrency();
	const CSRCurrency *ptrCurrency = CSRCurrency::GetCSRCurrency (currency); 

	if (!ptrCurrency)
	return;
	 ptrCurrency->GetRGBColor(&(cellStyle->color));



 

	map<long,double>::iterator it = cache.find(ElderFund->GetCode());

	if (it!=cache.end())
		Value=it->second;
	if(it==cache.end())
	{
		Value=SRAmount::getAmountValue( portfolioCode, extraction);
		cache[ElderFund->GetCode()]=Value;
		
	}

	cellValue->floatValue=Value;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void SRAmount::GetUnderlyingCell(	long				activePortfolioCode,
															long				portfolioCode,
															//DPH
															//const CSRExtraction *extraction,
															PSRExtraction		extraction,
															long				underlyingCode,
															SSCellValue			*cellValue,
															SSCellStyle			*cellStyle,
															bool				onlyTheValue) const
{
	// TO DO
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void SRAmount::GetPositionCell(	const CSRPosition&	position,
														long				activePortfolioCode,
														long				portfolioCode,
														//DPH
														//const CSRExtraction *extraction,
														PSRExtraction		extraction,
														long				underlyingCode,
														long				instrumentCode,
														SSCellValue			*cellValue,
														SSCellStyle			*cellStyle,
														bool				onlyTheValue) const
{
	
cellStyle->kind = NSREnums::dDouble;

	if(refreshVersion != GetRefreshVersion())
	{
		cache.clear();
		refreshVersion = GetRefreshVersion();
	}


	if(!onlyTheValue) 
	{
		cellStyle->alignment	= aRight;
		cellStyle->decimal		= 2;
		cellStyle->null			= nvZeroAndUndefined;
		cellStyle->style		= tsBold;
	} 

	

	double Value=0;
	const CSRPosition* pPosition=&position;

	ePositionType PositionType;
	PositionType=dynamic_cast<const sophis::value::CSAMPosition*>(pPosition)->CSAMPosition::GetPositionType();
	if(PositionType==sophis::value::vptSR)  
	{
	
			const CSRPortfolio * SomePortfolio =CSRPortfolio::GetCSRPortfolio(portfolioCode, extraction);
			if(!SomePortfolio)
				return ;

			
			const sophis::value::CSAMFund * ElderFund = sophis::value::CSAMFund::GetFundFromFolio(portfolioCode, extraction); 
			if (!ElderFund) 
				return ;


			long currency=ElderFund->GetCurrency();
			const CSRCurrency *ptrCurrency = CSRCurrency::GetCSRCurrency (currency); 
			if (!ptrCurrency)
			return; 
			ptrCurrency->GetRGBColor(&(cellStyle->color));


			map<long,double>::iterator it = cache.find(ElderFund->GetCode());

			if (it!=cache.end())
				Value=it->second;
			if(it==cache.end())
			{
				Value=SRAmount::getAmountValue( portfolioCode, extraction);
				cache[ElderFund->GetCode()]=Value;
				
			}

			cellValue->floatValue=Value;






	}


	

	



}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short SRAmount::GetDefaultWidth() const
{
	// TO DO
	return 60;
}

//double SRAmount::getAmountValue(long portfolioCode, const CSRExtraction *extraction)const
double SRAmount::getAmountValue(long portfolioCode, PSRExtraction extraction)const
{


	
	const CSRPortfolio * SomePortfolio =CSRPortfolio::GetCSRPortfolio(portfolioCode, extraction);
	if(!SomePortfolio)
		return 0.0;
	const sophis::value::CSAMFund * ElderFund = sophis::value::CSAMFund::GetFundFromFolio(portfolioCode, extraction); 
	if (!ElderFund) 
		return 0.0;
	long FundCode=ElderFund->GetCode();
	
	

	char Query[512];
	sprintf_s(Query,512,"select nvl(sum(AMOUNT),0) from FUND_PURCHASE where FUND=%ld AND NEGO_DATE = NUM_TO_DATE(%ld)",FundCode,gApplicationContext->GetDate());
	double AmountValue=0; 
	sophis::sql::CSRSqlQuery::QueryReturning1Double(Query,&AmountValue);
	


return AmountValue;
}
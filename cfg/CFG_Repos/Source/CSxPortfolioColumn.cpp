#pragma warning(disable:4251)
/*
** Includes
*/
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc/portfolio/SphPosition.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/SphPreference.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/portfolio/SphTransactionVector.h"

// specific

#include "CSxStandardDealInput.h"
#include "CSxPortfolioColumn.h"

//DPH
#include "UpgradeExtension.h"

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxIncomeHTPortfolioColumn,  "Income HT")

//-------------------------------------------------------------------------------------------------------------

double CSxIncomeHTPortfolioColumn::GetTvaAmount(const CSRPosition* position) const
{
	double result = 0.0;
	try {
		if ( position->GetIncome() != 0)
		{
			CSRTransactionVector transactions;
			position->GetTransactions(transactions);
			long id = position->GetIdentifier();						
			double allTvaAmount = 0.0;
			//DPH
			//for(CSRTransactionVector::iterator trans = transactions.begin()  ; trans < transactions.end() ; trans++)
			for (auto transaction = transactions.begin(); transaction < transactions.end(); transaction++)
			{
				//DPH
				//CSRTransaction* transaction = dynamic_cast<CSRTransaction*> (trans);				
				//if (transaction)
				{
					transaction->LoadUserElement();
					//DPH
					//long refCon = transaction->GetTransactionCode();
					TransactionIdent refCon = transaction->GetTransactionCode();
					double tvaAmount = 0.0;
					transaction->LoadGeneralElement(CSxStandardDealInput::eGrossTva, &tvaAmount);
					allTvaAmount += tvaAmount;
				}			
			}
			double income = position->GetIncome() * 1000;
			result = income - allTvaAmount;				
		}
	} catch (InvalidInvocationOrder e) 
	{

	}	
	return result;
}

/*virtual*/	void CSxIncomeHTPortfolioColumn::GetPortfolioCell(	long				activePortfolioCode,
												long				portfolioCode,
												//DPH
												//const CSRExtraction *extraction,
												PSRExtraction		extraction,
												SSCellValue			*cellValue,
												SSCellStyle			*cellStyle,
												bool				onlyTheValue) const
{
	cellStyle->kind = NSREnums::dDouble;

	const CSRPortfolio	* portfolio = CSRPortfolio::GetCSRPortfolio(portfolioCode, extraction);
	if(!portfolio)
		return;

	const CSRPosition	* position = NULL;

	cellValue->floatValue = 0.0;
	if(!onlyTheValue)
	{
		cellStyle->alignment	= aRight;
		cellStyle->decimal		= 0;
		cellStyle->null			= nvZeroAndUndefined;
		cellStyle->style		= tsBold;
	}

	int positionNumber = portfolio->GetFlatViewPositionCount();
	for(int index = 0 ; index < positionNumber ; index++)
	{
		position = portfolio->GetNthFlatViewPosition(index);
		cellValue->floatValue += GetTvaAmount(position);
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxIncomeHTPortfolioColumn::GetUnderlyingCell(	long				activePortfolioCode,
												 long				portfolioCode,
												 //DPH
												 //const CSRExtraction *extraction,
												 PSRExtraction		extraction,
												 long				underlyingCode,
												 SSCellValue			*cellValue,
												 SSCellStyle			*cellStyle,
												 bool				onlyTheValue) const
{
	cellStyle->kind = NSREnums::dDouble;

	const CSRPortfolio	* portfolio = CSRPortfolio::GetCSRPortfolio(portfolioCode, extraction);
	if(!portfolio)
		return;

	const CSRPosition	* position		= NULL;
	const CSRInstrument	* instrument	= NULL;
	long  internalCode = 0;

	cellValue->floatValue = 0.0;
	if(!onlyTheValue)
	{

		cellStyle->alignment	= aRight;
		cellStyle->decimal		= 2;
		cellStyle->null			= nvZeroAndUndefined;
		cellStyle->style		= tsBold;
	}

	int positionNumber = portfolio->GetFlatViewPositionCount();
	for(int index = 0; index <positionNumber ; index ++)
	{
		position		= portfolio->GetNthFlatViewPosition(index );
		if (!position)
			continue;

		internalCode	= position->GetInstrumentCode();
		instrument		= CSRMarketData::GetCurrentMarketData()->GetCSRInstrument(internalCode);
		if (instrument)
		{
			//DPH
			//internalCode = instrument->GetUnderlying(0);
			//internalCode = UpgradeExtension::GetUnderlying(instrument, 0);
			const CSRInstrument * underlyingInstrument = instrument->GetUnderlyingInstrument();
			if (underlyingInstrument) {
				internalCode = underlyingInstrument->GetCode();
			}
		}

		if(internalCode == underlyingCode)
		{
			cellValue->floatValue += GetTvaAmount(position);
		}
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxIncomeHTPortfolioColumn::GetPositionCell(	const CSRPosition&	position,
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

	const CSRInstrument * instrument = position.GetCSRInstrument();
	if (!instrument)
		return;

	try {
		if ( position.GetIncome() != 0)
		{
			CSRTransactionVector transactions;
			position.GetTransactions(transactions);
			long id = position.GetIdentifier();						
			double allTvaAmount = 0.0;
			//DPH
			//for(CSRTransactionVector::iterator trans = transactions.begin()  ; trans < transactions.end() ; trans++)
			for (auto transaction = transactions.begin(); transaction < transactions.end(); transaction++)
			{
				//DPH
				//CSRTransaction* transaction = dynamic_cast<CSRTransaction*> (trans);				
				//if (transaction)
				{
					transaction->LoadUserElement();
					//DPH
					//long refCon = transaction->GetTransactionCode();
					TransactionIdent refCon = transaction->GetTransactionCode();
					double tvaAmount = 0.0;
					transaction->LoadGeneralElement(CSxStandardDealInput::eGrossTva, &tvaAmount);
					allTvaAmount += tvaAmount;
				}			
			}
			double income = position.GetIncome() * 1000;
			cellValue->floatValue = income - allTvaAmount;				
		}
	} catch (InvalidInvocationOrder e) 
	{

	}	

	if(!onlyTheValue)
	{
		cellStyle->alignment	= aRight;
		cellStyle->decimal		= 0;
		cellStyle->null			= nvZeroAndUndefined;
		cellStyle->style		= tsNormal;
		if (instrument)
			cellStyle->currency	= instrument->GetCurrency();
		else
			cellStyle->currency = 0;

		if (cellStyle->currency)
		{
			const CSRCurrency * curr = CSRCurrency::GetCSRCurrency(cellStyle->currency);
			if (curr)
				curr->GetRGBColor(&(cellStyle->color));
		}
	}
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CSxIncomeHTPortfolioColumn::GetDefaultWidth() const
{
	return 80;
}


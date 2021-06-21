///////////////////////////////////////////////////////////////////////
// ShortSellManager.cpp
///////////////////////////////////////////////////////////////////////
#pragma warning(disable:4251)
/*
** Includes
*/
#include "ShortSellManager.h"
#include "SphInc/portfolio/SphExtraction.h"
#include "SphInc/portfolio/SphPortfolioColumn.h"
#include "SphTools/SphOStrStream.h"
#include "SphTools\SphLoggerUtil.h"
#include "SphInC/SphEnums.h"
#include "SphLLInc/portfolio/SphFolioStructures.h"

/*
** Namespace
*/
using namespace sophis::tools;
using namespace sophis::portfolio;
using namespace NSREnums; 

/*
** Static
*/
const char * CSxShortSellManager::__CLASS__ = "CSxShortSellManager";

/*
** Method
*/
CSxShortSellManager::CSxShortSellManager()
{
	BEGIN_LOG("CSxShortSellManager");

	END_LOG();
}

//------------------------------------------------------------------------------------
CSxShortSellManager::~CSxShortSellManager()
{
	BEGIN_LOG("~CSxShortSellManager");

	END_LOG();
}

//-------------------------------------------------------------------------------------
CSxShortSellManager & CSxShortSellManager::getInstance()
{
	BEGIN_LOG("getIntance");
	static CSxShortSellManager * localInstance = NULL;

	if (!localInstance)
	{
		localInstance = new CSxShortSellManager();
	}

	END_LOG();
	return *localInstance;
}

//---------------------------------------------------------------------------------------
double CSxShortSellManager::GetQuantityAvailableOrLended(long sicovam, long paymentDate, long fundId, bool lended)
{
	BEGIN_LOG("GetQuantityAvailableOrLended");

	_STL::string query = "";
	double returnedValue = 0.0;
	SSCellStyle currentCellStyle;
	SSCellValue currentCellValue;

	if (lended)
	{
		// Search SL underlying and instrument
		query = FROM_STREAM(" sicovam in (select sicovam from titres where code_emet = " << sicovam << ") or sicovam = " << sicovam);
	}
	else
	{
		// Only search for instrument
		query = FROM_STREAM(" sicovam = " << sicovam);
	}

	//DPH
	//PSRExtraction localExtraction = new CSRExtraction(query.c_str());
	PSRExtraction localExtraction = std::make_shared<CSRExtraction>(query.c_str());
	localExtraction->Create(icNothing, paymentDate);

	// Get the "Settled" portfolio column
	const CSRPortfolioColumn * settledCol = CSRPortfolioColumn::GetCSRPortfolioColumn("Settled");
	if (!settledCol)
	{
		MESS(Log::error, "Failed to find column 'Settled'");
		END_LOG();
		return returnedValue;
	}

	// Get the root folio
	//DPH
	//const CSRPortfolio * baseFolio = CSRPortfolio::GetCSRPortfolio(fundId, localExtraction.get());
	const CSRPortfolio * baseFolio = CSRPortfolio::GetCSRPortfolio(fundId, localExtraction);
	if (!baseFolio)
	{
		MESS(Log::error, "Failed to get root portfolio");
		END_LOG();
		return returnedValue;
	}		

	for (int i = 0; i < baseFolio->GetFlatViewPositionCount(); i++)
	{
		const CSRPosition * currentPosition = baseFolio->GetNthFlatViewPosition(i);
		if (currentPosition)
		{
			if (currentPosition->GetInstrumentCode() == sicovam)
			{
				currentCellValue.floatValue = 0.0;
				currentCellStyle.kind = dNullTerminatedString;

				settledCol->GetPositionCell(*currentPosition,
					fundId, // Fund folio
					fundId, // Flat view
					//DPH
					//localExtraction.get(), 
					localExtraction,
					0, // Underlying code
					sicovam, 
					&currentCellValue,
					&currentCellStyle,
					true);

				if (currentCellStyle.kind == dDouble)
				{
					returnedValue += currentCellValue.floatValue;
				}
			}
		}
	}	

	MESS(Log::debug, "Return " << returnedValue << " for sicovam " << sicovam << ", date " << paymentDate << ", fund " << fundId << " and lended " << lended);

	END_LOG();
	return returnedValue;	
}


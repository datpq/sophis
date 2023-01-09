
/*
** Includes
*/
// specific
#include "CSxVariationMarginColumn.h"
#include "SphInc/static_data/SphCurrency.h"
#include "../../MediolanumConstants.h"

#include "SphInc/misc/ConfigurationFileWrapper.h"
#include <accounting/Processing/SphBOKernelEnvironment.h>
#include <SphTools/SphLoggerUtil.h>

using namespace sophis::backoffice_kernel;
using namespace sophis::portfolio;
/*
** Methods
*/
	
//CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxVariationMarginColumn, MEDIO_COLUMNGROP_TKT)
/*static*/ const char* CSxVariationMarginColumn::__CLASS__ = "CSxVariationMarginColumn";
long CSxVariationMarginColumn::fBusinessEventGroup = -1;

CSxVariationMarginColumn::CSxVariationMarginColumn(void) : CSxCachedColumn()
{
	BEGIN_LOG("CSxVariationMarginColumn");
	fGroup = MEDIO_COLUMNGROP_TKT;

	_STL::string businessEventGroup = "";

	ConfigurationFileWrapper::getEntryValue("VMReporting",
		"BusinessEventGroupName",
		businessEventGroup, "VARIATION MARGIN");

	backoffice_kernel::ISRBOKernelEnvironment::KBEGroups grps = backoffice_kernel::gBOKernelEnvironment->GetAllKernBusinessEventsGroups();
	for (backoffice_kernel::ISRBOKernelEnvironment::KBEGroups::const_iterator itr = grps.begin(); itr != grps.end(); ++itr)
	{
		if (businessEventGroup.compare(itr->second.name) == 0)
		{
			fBusinessEventGroup = itr->first;
			break;
		}
	}
	if (fBusinessEventGroup == -1)
		LOG(Log::warning, FROM_STREAM("Cannot find business group " << businessEventGroup << ". VM columns will not be computed."));

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxVariationMarginColumn::ComputePortfolioCell(long activePortfolioCode, long portfolioCode, PSRExtraction extraction, SSCellValue* cellValue, SSCellStyle* cellStyle, bool onlyTheValue) const
{
	ConsolidateUnder(true, activePortfolioCode, portfolioCode, extraction, cellValue, cellStyle, onlyTheValue);
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxVariationMarginColumn::ComputePositionCell(const CSRPosition& position, long activePortfolioCode, long portfolioCode, PSRExtraction extraction, long underlyingCode, long instrumentCode, SSCellValue* cellValue, SSCellStyle* cellStyle, bool onlyTheValue) const
{
	BEGIN_LOG("CSxVariationMarginColumn");
	if (fBusinessEventGroup == -1) 
	{
		END_LOG();
		return;
	}

	if (position.GetIdentifier() <= 0) //JIRA 574, orders position
	{
		END_LOG();
		return;
	}

	try
	{
		CSRTransactionVector transactions;
		position.GetTransactions(transactions);
		for each (const CSRTransaction& deal in transactions)
		{
			long validGroup = CSRPreference::GetStatusGroupOfDealsInPortfolio();
			long boStatus = deal.GetBackOfficeType();
			bool valid = gBOKernelEnvironment->IsStatusInGroup(boStatus, validGroup);
			if (valid && gBOKernelEnvironment->IsBusinessEventInGroup(deal.GetTransactionType(), fBusinessEventGroup))
			{
				cellValue->floatValue -= deal.GetNetAmount();
			}
		}
	}
	catch (...)
	{
		MESS(Log::warning, FROM_STREAM("Error while accessing to trades of position " << position.GetIdentifier()));
		END_LOG();
		return;
	}
}



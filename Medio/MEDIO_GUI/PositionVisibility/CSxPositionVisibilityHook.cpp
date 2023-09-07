#include "CSxPositionVisibilityHook.h"
#include "SphTools\SphLoggerUtil.h"
#include "SphInc\SphMacros.h"
#include "SphTools\SphOStrStream.h"
#include "SphLLInc\portfolio\SphFolioStructures.h"
#include "SphInc\market_data\SphMarketData.h"
#include "SphInc\portfolio\SphReportingCustomFilter.h"

using namespace sophis::market_data;


//-------------------------------------------------------------------------------------------------------------
/*static*/ const char* CSxPositionVisibilityHook::__CLASS__ = "CSxPositionVisibilityHook";
//-------------------------------------------------------------------------------------------------------------

 bool CSxPositionVisibilityHook::_seeExpiredFXUserRight=false;
 bool CSxPositionVisibilityHook::_seeHedgeUserRight=false;
 set<string> CSxPositionVisibilityHook::_HedgeSet;

bool CSxPositionVisibilityHook::GetPositionVisibilityHook(const TViewMvts * position,
	const TViewFolio * folio,
	const sophis::portfolio::PSRExtraction& extraction,
	long activePortfolioCode,
	ListeEtat viewType,
	etat_ligne filter,
	bool & visible) const
{
	BEGIN_LOG("GetPositionVisibilityHook");

	// If bad position pointer return false (no hook, let FI handle it)
	if (!position)
	{
		END_LOG();
		return false;
	}

	_STL::string descForDebug = FROM_STREAM("Position ident: " << position->ident << ", sicovam: " << position->sicovam);
	try
	{	
		bool hidePos = false;

		if (_seeExpiredFXUserRight == false)
		{
			const CSRInstrument * inst = CSRInstrument::GetInstance(position->sicovam);
			if (inst != nullptr)
			{
				if (inst->GetType() == iForexSpot)
				{
					long posMaturityDate = inst->GetExpiry();
					long today = CSRMarketData::GetCurrentMarketData()->GetDate();
					if (posMaturityDate < today)
					{
						hidePos = true;
						MESS(Log::debug, "Position " << descForDebug.c_str() << " is matured.");
					}
				}
			}
		}

		if (_seeHedgeUserRight == false)
		{
			std::string folName = folio->name;
			if (_HedgeSet.find(folName) != _HedgeSet.end())
			{
				const CSRInstrument * inst = CSRInstrument::GetInstance(position->sicovam);
				if (inst != nullptr)
				{
					char instType = inst->GetType();
					if (instType == iForexSpot || instType == iCommission || instType == iForexFuture || instType == iForexNonDeliverable)
					{
						hidePos = true;
					}
				}
			}
		}		
		return hidePos;
	}
	catch (ExceptionBase exc)
	{
		MESS(Log::error, FROM_STREAM("Exception while reckoning visibility for filter for Position : " <<
			descForDebug.c_str() << " ." << exc));
	}
	catch (...)
	{
		MESS(Log::error, FROM_STREAM("Unhandled exception while reckoning visibility for filter for Position : " <<
			descForDebug.c_str() << " ."));
	}

	END_LOG();
	return true;
}

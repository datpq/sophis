#include "CSxPositionVisibilityHook.h"
#include "SphTools\SphLoggerUtil.h"
#include "SphInc\SphMacros.h"
#include "SphTools\SphOStrStream.h"
#include "SphLLInc\portfolio\SphFolioStructures.h"
#include "SphInc\market_data\SphMarketData.h"


using namespace sophis::market_data;


//-------------------------------------------------------------------------------------------------------------
/*static*/ const char* CSxPositionVisibilityHook::__CLASS__ = "CSxPositionVisibilityHook";
//-------------------------------------------------------------------------------------------------------------


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
		CSRPosition pos((TViewMvts *)position);

		// If unknown instrument, return false (no hook, let FI handle it)
		const CSRInstrument * inst = CSRInstrument::GetInstance(position->sicovam);
		if (!inst)
		{
			MESS(Log::debug, "No instrument; " << descForDebug.c_str());
			END_LOG();
			return false;
		}

		bool hidePos = false;
		char instType = inst->GetType();
		if (instType == iForexSpot)
		{
			long posMaturityDate = inst->GetExpiry();
			long today = CSRMarketData::GetCurrentMarketData()->GetDate();
			if (posMaturityDate < today)
			{
				hidePos = true;
				MESS(Log::debug, "Position " << descForDebug.c_str() << " is matured.");
			}
		}

		if (true == hidePos)
		{
			//Position is matured and should be hidden, but user right may overwrite this.
			bool userHasAccess = false;
			CSRUserRights currentUser;
			currentUser.LoadDetails();
			if (currentUser.HasAccess("See Expired FX Frwd"))
			{
				userHasAccess = true;
				MESS(Log::debug, "Current user (" << currentUser.GetName() << ") has access to see expired FX Frwd positions");
			}

			if (true == userHasAccess)
			{
				MESS(Log::debug, "Position " << descForDebug.c_str() << " IS visible");
				visible = true;
				return true;
			}

			MESS(Log::debug, "Position " << descForDebug.c_str() << " is NOT visible");
			visible = false;
			return true;
		}

		return false;
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
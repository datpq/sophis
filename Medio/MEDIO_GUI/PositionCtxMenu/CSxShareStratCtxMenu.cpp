/*
** Includes
*/
#include "CSxShareStratCtxMenu.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/GUI/SphInstrumentUI.h"
#include "SphInc/value/strategy/SphStrategyAllocation.h"
#include "SphInc/value/strategy/SphFolioStrategy.h"
#include "SphTools/SphLoggerUtil.h"

#include __STL_INCLUDE_PATH(string)

using namespace sophis::strategy;

const char* CSxShareStratCtxMenu::__CLASS__ = "CSxShareStratCtxMenu";

	/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_POSITION_CTX_MENU(CSxShareStratCtxMenu)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxShareStratCtxMenu::IsFolioAuthorized(const CSRPortfolioVector &folioVector) const const
{
	BEGIN_LOG("IsFolioAuthorized");
	CSRPortfolioVector::const_iterator iter = folioVector.begin();
	if( folioVector.size() != 1 )
	{
		MESS(Log::debug, "Several folios selected, do not display NAV_Strat.");
		return false;
	}

	if( iter != folioVector.end() )
	{
		const CSRPortfolio* folio = *iter;
		long folioId = folio->GetCode();
		long stratId = CSAmStrategyAllocationMgr::GetInstance()->GetFolioStrategy(folioId);
		if( stratId > 0 )
		{
			return true;
			MESS(Log::debug, "Selected folio is a nav.");
		}
	}
	MESS(Log::debug, "Selected folio is not a nav.");
	END_LOG();
	return false;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxShareStratCtxMenu::FolioAction(const CSRPortfolioVector &folioVector, const char* ActionName) const
{
	BEGIN_LOG("FolioAction")
	CSRPortfolioVector::const_iterator iter = folioVector.begin();
	
	if( iter != folioVector.end() )
	{
		const CSRPortfolio* folio = *iter;
		long folioId = folio->GetCode();
		std::stringstream instRef;
		instRef << "NAV_";
		instRef << folioId; 
		long sico = CSRInstrument::GetCodeWithReference(instRef.str().c_str());
		if( sico <= 0 )
		{
			MESS(Log::warning,FROM_STREAM("Cannot find NAV Instrument with reference NAV_" << folioId));
			long stratId = CSAmStrategyAllocationMgr::GetInstance()->GetFolioStrategy(folioId);
			if( stratId > 0 )
			{
				sophis::strategy::CSAmFolioStrategy strategy;
				if( CSAmStrategiesMgr::GetInstance()->GetStrategy(stratId, strategy) )
				{				
					CSRFitDialog::Message(FROM_STREAM("Cannot find NAV Instrument with reference NAV_" << folioId << ". Open the linked strategy " 
						<< strategy.GetName() << " and click on OK to create it automatically."));
					return;
				}
			}
			CSRFitDialog::Message(FROM_STREAM("Cannot find NAV Instrument with reference NAV_" << folioId) );
			
		}
		else
		{
			CSRInstrumentUI::OpenDialog(sico);
		}
	}
	END_LOG();
}
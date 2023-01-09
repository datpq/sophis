/*
** Includes
*/
// specific
#include "CSxCreateShareStrategyAction.h"
#include "SphInc/instrument/SphEquity.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/GUI/SphInstrumentUI.h"
#include "SphInc/value/strategy/SphStrategyAllocation.h"
#include "SphInc/gui/SphDialog.h"
#include "SphTools/SphLoggerUtil.h"

using namespace sophis::strategy;

const char* CreateShareStrategyAction::__CLASS__ = "CreateShareStrategyAction";

void CreateShareStrategyAction::CreateShare(const CSAmFolioStrategy& strategy, long folioId)
{
	BEGIN_LOG("CreateShare");
//	Share description
//•	Reference – “NAV_<identFolio>” (<identFolio> = ident of the strategy folio)
//•	Name – “NAV - <StrategyName>”
//•	External ref “STRATEGY” value – identFolio
//•	MANUALLY DEFINED: External ref “MANAGER_CODE” value – Manager code used in the RBC file to link NAV values to strategy 
//[see NAV_share.xlsx]
	try
	{
		std::stringstream instRef;
		instRef << "NAV_";
		instRef << folioId; 
		long sico = CSRInstrument::GetCodeWithReference(instRef.str().c_str());
		if( sico <= 0 )
		{
			//The NAV share was not created. Create it.
			CSREquity* equity = CSREquity::CreateInstance("Standard", false);
			std::stringstream instRef;
			instRef << "NAV_";
			instRef << folioId; 

			equity->SetReference(instRef.str().c_str());
			std::stringstream instName;
			instName << "NAV - ";
			instName << strategy.GetName(); 
			equity->SetName(instName.str().c_str());

			SSComplexReference* ptrRedRef = new SSComplexReference();
			//strcpy_s(ptrRedRef->type, 100, "STRATEGY");
			//strcpy_s(ptrRedRef->value, 100, std::to_string((long long) folioId).c_str());
			//SSComplexReferenceP complexRefptr;
			//complexRefptr.refNum = 1;
			//complexRefptr.ref = ptrRedRef;
			//equity->SetClientReference(complexRefptr);
			ptrRedRef->type = "STRATEGY";
			ptrRedRef->value = std::to_string((long long)folioId).c_str();
			std::vector<SSComplexReference> complexRefVctr;
			complexRefVctr.push_back(*ptrRedRef);
			equity->SetClientReference(std::move(complexRefVctr));

			equity->SetCurrency(strategy.GetCurrency());
			MESS(Log::debug,FROM_STREAM("Start saving NAV Share instrument for folio id" << folioId ));
			equity->Save(NSREnums::eParameterModificationType::pmInsertion);
			//NAV Share was already created. Do nothing
			MESS(Log::debug,FROM_STREAM("NAV Share instrument for folio id" << folioId <<" created"));

			long createdId = equity->GetCode();
			delete equity; equity = NULL;
		
			CSRInstrumentUI::OpenDialog(createdId);
			CSRFitDialog::Message("Please set the MANAGER_CODE property for the NAV Share");
		}
		else
		{
			//NAV Share was already created. Do nothing
			MESS(Log::debug,FROM_STREAM("NAV Share instrument for folio id" << folioId << " already exists."));
		}
	}
	catch(const ExceptionBase& ex)
	{
		MESS(Log::error,FROM_STREAM("Cannot create NAV Share instrument for folio id" << folioId << ":" << ex.getError()));
	}
	END_LOG();
}


void CreateShareStrategyAction::VoteForModification(const CSAmFolioStrategy & original, const CSAmFolioStrategy & newVersion) 
{
	BEGIN_LOG("VoteForModification");
	MESS(Log::debug,"In vote for modifications");
	std::vector<long> folioCodes;
	CSAmStrategyAllocationMgr::GetInstance()->GetPortfoliosLinkedToStrategy(newVersion.GetId(), folioCodes);
	if(folioCodes.size() == 1)
	{
		//We will just check if it was created
		MESS(Log::info,FROM_STREAM("Check NAV Share instrument for folio id" << folioCodes[0] ));
		CreateShare(newVersion, folioCodes[0]);
	}
	else
	{
		MESS(Log::warning,FROM_STREAM("The strategy is linked to more than one folio. Cannot check NAV Share instrument. Strat id = " << newVersion.GetId() ));
	}
}

const char* CreateShareStrategyAllocationAction::__CLASS__ = "CreateShareStrategyAllocationAction";

void CreateShareStrategyAllocationAction::VoteForCreation(const SStrategyAllocation& strategyAllocation) 
{
	BEGIN_LOG("VoteForCreation")
	CSAmFolioStrategy strategy;
	if( CSAmStrategiesMgr::GetInstance()->GetStrategy(strategyAllocation.fStrategyId, strategy) )
	{
		MESS(Log::info,FROM_STREAM("Check NAV Share instrument for folio id" << strategyAllocation.fFolioId ));
		CreateShareStrategyAction::CreateShare(strategy, strategyAllocation.fFolioId);
	}
	else
	{
		MESS(Log::warning,FROM_STREAM("Cannot find strategy with id " << strategyAllocation.fFolioId ));
	}
	END_LOG();
}

void CreateShareStrategyAllocationAction::VoteForModification(const SStrategyAllocation& original, const SStrategyAllocation& newVersion)
{
	BEGIN_LOG("VoteForModification")
	CSAmFolioStrategy strategy;
	if( CSAmStrategiesMgr::GetInstance()->GetStrategy(newVersion.fStrategyId, strategy) )
	{
		MESS(Log::info,FROM_STREAM("Check NAV Share instrument for folio id" << newVersion.fFolioId ));
		CreateShareStrategyAction::CreateShare(strategy, newVersion.fFolioId);
	}
	else
	{
		MESS(Log::warning,FROM_STREAM("Cannot find strategy with id " << newVersion.fFolioId ));
	}
	END_LOG();
}

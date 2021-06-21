#pragma warning(disable:4251)
/*
** Includes
*/

// specific
#include "TKTFieldAction.h"
#include "..\CFG_Repos\Source\CSxStandardDealInput.h"
#include "SphTools/SphLoggerUtil.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::tools;

/*static*/ const char* TKTFieldAction::__CLASS__ = "TKTFieldActionFORStockLoan";
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(TKTFieldAction)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void TKTFieldAction::NotifyCreated(const CSRTransaction &transaction, tools::CSREventVector & message, long event_id)
				throw (sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("NotifyCreated");
	//DPH
	//long masterTradeID=0;
	TransactionIdent masterTradeID = 0;
	//transaction.GetInstrument()->GetType()

	if(transaction.GetCreationKind() != toAutomatic)
	{
		masterTradeID = transaction.GetMirroringReference();
	}

	if(masterTradeID > 0)
	{
		double Spread=0;
		double Amount=0;
		double IRAmount=0;
	
		CSRTransaction masterTrade(masterTradeID );
		masterTrade.LoadUserElement();

		Boolean Loaded = masterTrade.LoadGeneralElement(CSxStandardDealInput::eSpreadHT,&Spread);	
		if (Loaded)
		{
			MESS(Log::debug, "Spread loaded from the master deal " << Spread);
		}
		else
		{
			MESS(Log::debug, "Spread not  loaded from the master deal " << Spread);	
		}
	
		Loaded = masterTrade.LoadGeneralElement(CSxStandardDealInput::eRepoAmount	,&Amount);
		if (Loaded)
		{
			MESS(Log::debug, "Amount loaded from the master deal " << Amount);	
		}
		else
		{
			MESS(Log::debug, "AMount not  loaded from the master deal " << Amount);	
		}
	
		Loaded = masterTrade.LoadGeneralElement(CSxStandardDealInput::eInterestAmount,&IRAmount);
		if (Loaded)
		{
			MESS(Log::debug, "IRAmount loaded from the master deal " << IRAmount);
		}
		else
		{
			MESS(Log::debug, "IRAmount not  loaded from the master deal " <<IRAmount);	
		}
		
		// Set the mirror deal Tkt fields:
		transaction.LoadUserElement();
		Boolean Saved = transaction.SaveGeneralElement(CSxStandardDealInput::eSpreadHT,&Spread);
		if (Saved)
		{
			MESS(Log::debug, "Spread copied to the mirror deal " << Spread);	
		}
		else
		{
			MESS(Log::debug, "Spread not copied to the mirror deal " << Spread);
		}
		
		Saved = transaction.SaveGeneralElement(CSxStandardDealInput::eRepoAmount	,&Amount);
		if (Saved)
		{
			MESS(Log::debug, "Amount copied to the mirror deal " << Amount);	
		}
		else
		{
			MESS(Log::debug, "Amount not copied to the mirror deal " << Amount);
		}
	
		Saved = transaction.SaveGeneralElement(CSxStandardDealInput::eInterestAmount,&IRAmount);
		if (Saved)
		{
			MESS(Log::debug, "IRAmount copied to the mirror deal " << IRAmount);	
		}
		else
		{
			MESS(Log::debug, "IRAmount not copied to the mirror deal " << IRAmount);
		}
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void TKTFieldAction::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
throw (VoteException)
{
}


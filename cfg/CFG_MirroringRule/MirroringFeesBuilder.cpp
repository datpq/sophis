#pragma warning(disable:4251)

/*
** Includes
*/
#include "MirroringFeesBuilder.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "..\CFG_Repos\Source\CSxStandardDealInput.h"

/*
** Namespace
*/
using namespace sophis::portfolio;

/*
** Statics
*/
/*static*/ const char* MirroringFeesBuilder::__CLASS__ = "MirroringFeesBuilder";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void MirroringFeesBuilder::generate(const CSRTransaction* mainMvt, CSRTransaction* mirrorMvt) const
throw (CSRMirrorTransactionBuilderException) 
{
	BEGIN_LOG("generate");
	////Get the original Tkt fields:
	double tva=0;
	double grossTva=0;
	double CtyTva=0;
	double MarketTva=0;
	double Spread=0;
	double Amount=0;
	double IRAmount=0;

	mirrorMvt->RecomputeGrossAmount();

    mainMvt->LoadUserElement();
	Boolean Loaded = mainMvt->LoadGeneralElement(CSxStandardDealInput::eBrokerTva,	&tva);
    Loaded = mainMvt->LoadGeneralElement(CSxStandardDealInput::eGrossTva,			&grossTva);
	Loaded = mainMvt->LoadGeneralElement(CSxStandardDealInput::eCounterpartyTva,	&CtyTva	);
	Loaded = mainMvt->LoadGeneralElement(CSxStandardDealInput::eMarketTva,			&MarketTva);	
	Loaded = mainMvt->LoadGeneralElement(CSxStandardDealInput::eSpreadHT,			&Spread);	
	if (Loaded)
	{
		MESS(Log::debug, "Spread loaded from the master deal " << Spread);
	}
	else
	{
		MESS(Log::debug, "Spread not  loaded from the master deal " << Spread);
	}
	
	Loaded = mainMvt->LoadGeneralElement(CSxStandardDealInput::eRepoAmount, &Amount);
	if (Loaded)
	{
		MESS(Log::debug, "Amount loaded from the master deal " << Amount);
	}
	else
	{
		MESS(Log::debug, "AMount not  loaded from the master deal " << Amount);	
	}
	Loaded = mainMvt->LoadGeneralElement(CSxStandardDealInput::eInterestAmount, &IRAmount);
	if (Loaded)
	{
		MESS(Log::debug, "IRAmount loaded from the master deal " << IRAmount);
	}
	else
	{
		MESS(Log::debug, "IRAmount not  loaded from the master deal " <<IRAmount);
	}

	//// Set the mirror deal Tkt fields:
	mirrorMvt->LoadUserElement();
	
	Boolean Saved = mirrorMvt->SaveGeneralElement(CSxStandardDealInput::eBrokerTva,	&tva);
	Saved = mirrorMvt->SaveGeneralElement(CSxStandardDealInput::eGrossTva,			&grossTva);
	Saved = mirrorMvt->SaveGeneralElement(CSxStandardDealInput::eCounterpartyTva,	&CtyTva);
	Saved = mirrorMvt->SaveGeneralElement(CSxStandardDealInput::eMarketTva,			&MarketTva);
	Saved = mirrorMvt->SaveGeneralElement(CSxStandardDealInput::eSpreadHT,			&Spread);
	if (Saved)
	{
		MESS(Log::debug, "Spread copied to the mirror deal " << Spread);
	}
	else
	{
		MESS(Log::debug, "Spread not copied to the mirror deal " << Spread);
	}
	Saved = mirrorMvt->SaveGeneralElement(CSxStandardDealInput::eRepoAmount, &Amount);
	if (Saved)
	{
		MESS(Log::debug, "Amount copied to the mirror deal " << Amount);
	}
	else
	{
		MESS(Log::debug, "Amount not copied to the mirror deal " << Amount);		
	}
	Saved = mirrorMvt->SaveGeneralElement(CSxStandardDealInput::eInterestAmount, &IRAmount);
	if (Saved)
	{
		MESS(Log::debug, "IRAmount copied to the mirror deal " << IRAmount);
	}
	else
	{
		MESS(Log::debug, "IRAmount not copied to the mirror deal " << IRAmount);	
	}	
	
	END_LOG();
}
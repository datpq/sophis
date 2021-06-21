#pragma warning(disable:4251)
/*
** Includes
*/
#include "CFGObligationsMirroringRule.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "..\CFG_Repos\Source\CSxStandardDealInput.h"
#include "..\CFG_Repos\Source\CSxTVARates.h"

/*
** Namespace
*/
using namespace sophis::portfolio;

/*
** Statics
*/
/*static*/ const char* CFGObligationsMirroringRule::__CLASS__ = "CFGObligationsMirroringRule";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGObligationsMirroringRule::generate(const CSRTransaction* mainMvt, CSRTransaction* mirrorMvt) const
throw (CSRMirrorTransactionBuilderException) /*= 0*/
{
	BEGIN_LOG("generate");

	if (!mirrorMvt)
	{
		MESS(Log::warning, "No mirror transaction");
		END_LOG();
		return;
	}

	if (!mainMvt)
	{
		MESS(Log::warning, "No main transaction");
		END_LOG();
		return;
	}

	// Quantity
	MESS(Log::debug, "Update Quantity");
	mirrorMvt->SetQuantity(mirrorMvt->GetQuantity());

	// Set Frais de gestion et frais bancaires
	eTransactionType businessEvent = mirrorMvt->GetTransactionType();		
	long TVARateId = CSxTVARates::GetTVARateIdFromBE(businessEvent);	
	double TVARate = CSxTVARates::GetTVARate(TVARateId);
	MESS(Log::debug, "Rate Id " << TVARateId << ", Rate " << TVARate);
	SetTvaAmount(mirrorMvt, mirrorMvt->GetGrossAmount(), CSxStandardDealInput::eGrossTva, TVARate);

	// Set market fees TVA amount
	TVARate = CSxTVARates::GetTVARate(MARKET_FEES_TVA_RATE_ID);
	SetTvaAmount(mirrorMvt, mirrorMvt->GetMarketFees(), CSxStandardDealInput::eMarketTva, TVARate);

	//Set couterparty fees TVA Amount
	TVARate = CSxTVARates::GetTVARate(COUNTERPARTY_FEES_TVA_RATE_ID);
	SetTvaAmount(mirrorMvt, mirrorMvt->GetCounterpartyFees(), CSxStandardDealInput::eCounterpartyTva, TVARate);

	//Set broker fees TVA Amount
	TVARate = CSxTVARates::GetTVARate(BROKER_FEES_TVA_RATE_ID);
	SetTvaAmount(mirrorMvt, mirrorMvt->GetBrokerFees(), CSxStandardDealInput::eBrokerTva, TVARate);
	
	END_LOG();
}

//---------------------------------------------------------------------------------------------------------------------
void CFGObligationsMirroringRule::SetTvaAmount(CSRTransaction * transaction, const double fieldAmount, const int eTvaField, const double rate) const
{
	BEGIN_LOG("SetTvaAmount");

	double amountTva = (fieldAmount * rate) / (1 + rate);
	//double localRate = rate;
	Boolean isOk = transaction->SaveGeneralElement(eTvaField, &amountTva);
	if (!isOk)
	{
		MESS(Log::warning, "Failed to save TVA in field " << eTvaField);
	}
	else
	{
		MESS(Log::debug, "Use TVA " << eTvaField << " amount " << amountTva << " with rate " << rate);
	}

	END_LOG();
}
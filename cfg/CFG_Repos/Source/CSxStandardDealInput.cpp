#pragma warning(disable:4251)
/*
** Includes
*/
#include <stdio.h>
#include "SphInc/gui/SphEditElement.h"
#include "SphInc/instrument/SphLoanAndRepo.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/portfolio/SphTransactionVector.h"
#include "SphInc/backoffice_kernel/SphCorporateAction.h"
#include "SphTools/SphLoggerUtil.h"

// specific
#include "CSxTVARates.h"
#include "CSxStandardDealInput.h"



/*
** Namespace
*/
using namespace sophis::gui;
using namespace sophis::backoffice_kernel;
using namespace sophis::collateral;
using namespace sophis::value;

/*
** Methods
*/


/*
** Static
*/
const char * CSxStandardDealInput::__CLASS__ = "CSxStandardDealInput";

//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_DIALOG(CSxStandardDealInput)

//-------------------------------------------------------------------------------------------------------------
CSxStandardDealInput::CSxStandardDealInput() : CSAMTransactionDialog()
{
	fResourceId	= IDD_TRANSACTION_DIALOG - ID_DIALOG_SHIFT;

	// MANDATORY: begin
	// specify the number of toolkit fields in the ticked dialog of value
	// it is VERY IMPORTANT for the memory allocation 
	// to add CSAMTransactionDialog::cCOUNT to your own toolkit field number
	int nbCount2  					= CSAMTransactionDialog::cCOUNT + eNbFields;
	CSRElement ** fElementList2		= new CSRElement*[nbCount2];

	if (fElementList2)
	{
		for (int i = 0; i < CSAMTransactionDialog::cCOUNT; i++) 
		{
			fElementList2[i] = fElementList[i];
		}
		// MANDATORY: end

		// ADD HERE YOUR TOOLKIT FIELDS
		// number of elements
		int nb = CSAMTransactionDialog::cCOUNT;

		fElementList2[nb++] = new CSREditDouble(this, eBrokerTva, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "BROKER_TVA");
		fElementList2[nb++] = new CSREditDouble(this, eGrossTva, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "GROSS_TVA");
		fElementList2[nb++] = new CSREditDouble(this, eMarketTva, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "MARKET_TVA");
		fElementList2[nb++] = new CSREditDouble(this, eCounterpartyTva, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "COUNTERPARTY_TVA");
		fElementList2[nb++] = new CSREditDouble(this, eSpreadHT, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "SPREAD_HT");
		fElementList2[nb++] = new CSREditDouble(this, eRepoAmount, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "CFG_REPO_AMOUNT");
		fElementList2[nb++] = new CSRStaticText(this, eInterestAmountLabel, 100);
		fElementList2[nb++] = new CSRStaticDouble(this, eInterestAmount, 2,0.,1e20,0.0,"CFG_INTEREST_AMOUNT");
		fElementList2[nb++] = new CSRStaticDouble(this, eFinalAmount, 2,0.,1e20);
		fElementList2[nb++] = new CSRStaticText(this, eInterestAmountHTLabel, 100);
		fElementList2[nb++] = new CSRStaticDouble(this, eInterestAmountHT, 2,0.,1e20);
		fElementList2[nb++] = new CSRStaticText(this, eFinalAmountHTLabel, 100);
		fElementList2[nb++] = new CSRStaticDouble(this, eFinalAmountHT, 2,0.,1e20);


		// END ADD HERE YOUR TOOLKIT FIELDS
		// MANDATORY: begin
		// memory reallocation
		CSRAssignement<CSRElement **> * assignement = new CSRAssignement<CSRElement**>(fElementList, fElementList2);
		CSRAssignement<int> * fCount = new CSRAssignement<int>(fElementCount, nbCount2);
		// MANDATORY: end
	}
}

/*virtual*/ void	CSxStandardDealInput::OpenAfterInit(void)
{
	CSAMTransactionDialog::OpenAfterInit();

	double dummyVal = 0.;
	GetElementByRelativeId(eSpreadHT)->SetValue(&dummyVal);
	GetElementByRelativeId(eRepoAmount)->SetValue(&dummyVal);
	HideElement(eSpreadHT);
	HideElement(eRepoAmount);
	HideElement(eInterestAmountLabel);
	HideElement(eInterestAmount);
	HideElement(eFinalAmount);
	HideElement(eInterestAmountHTLabel);
	HideElement(eInterestAmountHT);
	HideElement(eFinalAmountHTLabel);
	HideElement(eFinalAmountHT);
	
	CSRTransaction* transaction = new_CSRTransaction();
	if (transaction)
	{
		//DPH
		//long trxCode = transaction->GetTransactionCode();
		TransactionIdent trxCode = transaction->GetTransactionCode();
		
		if (!trxCode || transaction->GetCreationKind() == toAutomatic)
		{
			ComputeTvaAmounts();
		}				

		delete transaction;
		transaction = NULL;
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/void CSxStandardDealInput::ElementValidation(int EAId_Modified)
{
	CSAMTransactionDialog::ElementValidation(EAId_Modified);

	ComputeTvaAmounts();
}

//-------------------------------------------------------------------------------------------------------------
void CSxStandardDealInput::SetTvaAmount(const double fieldAmount, const int eTvaField, const double rate)
{
	double amountTva = (fieldAmount * rate) / (1 + rate);
	GetElementByRelativeId(eTvaField)->SetValue(&amountTva);
	UpdateElement(eTvaField);
}

//-------------------------------------------------------------------------------------------------------------
void CSxStandardDealInput::ComputeTvaAmounts()
{
	BEGIN_LOG("ComputeTvaAmounts");
	CSRTransaction* transaction = new_CSRTransaction();
	if (transaction)
	{	
		//Set Frais de gestion et frais bancaires
		eTransactionType businessEvent = transaction->GetTransactionType();		
		long TVARateId = CSxTVARates::GetTVARateIdFromBE(businessEvent);		
		double TVARate = CSxTVARates::GetTVARate(TVARateId);
		SetTvaAmount(transaction->GetGrossAmount(), eGrossTva, TVARate);

		//Set market fees TVA amount
		TVARate = CSxTVARates::GetTVARate(MARKET_FEES_TVA_RATE_ID);
		SetTvaAmount(transaction->GetMarketFees(), eMarketTva, TVARate);

		//Set couterparty fees TVA Amount
		TVARate = CSxTVARates::GetTVARate(COUNTERPARTY_FEES_TVA_RATE_ID);
		SetTvaAmount(transaction->GetCounterpartyFees(), eCounterpartyTva, TVARate);

		//Set broker fees TVA Amount
		TVARate = CSxTVARates::GetTVARate(BROKER_FEES_TVA_RATE_ID);
		SetTvaAmount(transaction->GetBrokerFees(), eBrokerTva, TVARate);

		delete transaction;
	}
	END_LOG();
}

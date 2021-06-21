

#include <afxwin.h>
#include "SphTools/base/CommonOS.h"
#include <math.h>
#include <stdio.h>

/*
** Includes
*/

#include "../Resource/resource.h"
#include "SphInc/Value/kernel/SphAMOverloadedDialogs.h"
#include "SphInc/gui/SphEditElement.h"
#include "SphInc/instrument/SphLoanAndRepo.h"
#include "SphInc/collateral/SphStockOnLoanEnums.h"
#include "SphInc/gui/SphDialog.h"
#include "SphInc/instrument/SphBond.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/SphPreference.h"
#include "SphLLInc/sophismath.h"
#include "SphInc/gui/SphCustomMenu.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphLLInc/interface/transdlog.h"
#include "SphInc/market_data/SphMarketData.h"

// specific

#include "CSxTVARates.h"
#include "CSxInterest.h"
#include "CSxStandardDealInput.h"
#include "CSxLoanAndRepoDealInput.h"



/*
** Namespace
*/
using namespace sophis;
using namespace sophis::gui;
using namespace sophis::portfolio;
using namespace sophis::instrument;
using namespace sophis::collateral;
using namespace sophis::math;
using namespace sophis::market_data;

/*
** Static
*/
const char * CSxLoanAndRepoDealInput::__CLASS__ = "CSxLoanAndRepoDealInput";



/*
** Methods
*/

//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_DIALOG(CSxLoanAndRepoDealInput)

//-------------------------------------------------------------------------------------------------------------
CSxLoanAndRepoDealInput::CSxLoanAndRepoDealInput() : CSRLoanAndRepoDialog()
{
	fResourceId	= IDD_LOAN_AND_REPO_DIALOG - ID_DIALOG_SHIFT;

	NewElementList(CSxStandardDealInput::eNbFields);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++] = new CSREditDouble(this, CSxStandardDealInput::eBrokerTva, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "BROKER_TVA");
		fElementList[nb++] = new CSREditDouble(this, CSxStandardDealInput::eGrossTva, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "GROSS_TVA");
		fElementList[nb++] = new CSREditDouble(this, CSxStandardDealInput::eMarketTva, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "MARKET_TVA");
		fElementList[nb++] = new CSREditDouble(this, CSxStandardDealInput::eCounterpartyTva, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "COUNTERPARTY_TVA");
		fElementList[nb++] = new CSREditDouble(this, CSxStandardDealInput::eSpreadHT, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "SPREAD_HT");
		fElementList[nb++] = new CSREditDouble(this, CSxStandardDealInput::eRepoAmount, CSRPreference::GetNumberOfDecimalsForPrice(),0,9999999999,0, "CFG_REPO_AMOUNT");
		fElementList[nb++] = new CSRStaticText(this, CSxStandardDealInput::eInterestAmountLabel, 100);
		fElementList[nb++] = new CSRStaticDouble(this, CSxStandardDealInput::eInterestAmount, 2,0.,1e20,0.0,"CFG_INTEREST_AMOUNT");
		fElementList[nb++] = new CSRStaticDouble(this, CSxStandardDealInput::eFinalAmount, 2,0.,1e20);
		fElementList[nb++] = new CSRStaticText(this, CSxStandardDealInput::eInterestAmountHTLabel, 100);
		fElementList[nb++] = new CSRStaticDouble(this, CSxStandardDealInput::eInterestAmountHT, 2,0.,1e20);
		fElementList[nb++] = new CSRStaticText(this, CSxStandardDealInput::eFinalAmountHTLabel, 100);
		fElementList[nb++] = new CSRStaticDouble(this, CSxStandardDealInput::eFinalAmountHT, 2,0.,1e20);
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxLoanAndRepoDealInput::OpenAfterInit(void)
{		
	BEGIN_LOG("OpenAfterInit");	

	HideElement(CSxStandardDealInput::eBrokerTva);
	HideElement(CSxStandardDealInput::eGrossTva);
	HideElement(CSxStandardDealInput::eMarketTva);
	HideElement(CSxStandardDealInput::eCounterpartyTva);		

	char buf[100];
	strcpy_s(buf,100,"Interest Amount");
	GetElementByRelativeId(CSxStandardDealInput::eInterestAmountLabel)->SetValue(buf);
	strcpy_s(buf,100,"Interests Amount HT");
	GetElementByRelativeId(CSxStandardDealInput::eInterestAmountHTLabel)->SetValue(buf);
	strcpy_s(buf,100,"Final Amount HT");
	GetElementByRelativeId(CSxStandardDealInput::eFinalAmountHTLabel)->SetValue(buf);

	ShowRepoAmountFields();
	ShowRepoSpreadFields();	

	CSRTransaction* trans = new_CSRTransaction();
	if( trans)
	{
		//DPH
		//long refCon = trans->GetTransactionCode();
		TransactionIdent refCon = trans->GetTransactionCode();
		//DPH
		//long mirroredTrade = trans->GetMirroringReference();
		TransactionIdent mirroredTrade = trans->GetMirroringReference();
		if(refCon || mirroredTrade > 0) // If the trade is the mirrored trade, field are already initialized
		{

			double commission = trans->GetCommission();
			double spreadHT = 0.;
			double repoAmount = 0.;			
			//Le ticket 'classique' charge les users elements.
			trans->LoadUserElement();
			//On va chercher dans la pos la valeur associée dans le user info.
			trans->LoadGeneralElement(CSxStandardDealInput::eSpreadHT, &spreadHT);
			trans->LoadGeneralElement(CSxStandardDealInput::eRepoAmount, &repoAmount);			

			GetElementByRelativeId(CSxStandardDealInput::eSpreadHT)->SetValue(&spreadHT);			
			UpdateElement(CSxStandardDealInput::eSpreadHT);

			GetElementByRelativeId(CSxStandardDealInput::eRepoAmount)->SetValue(&repoAmount);			
			UpdateElement(CSxStandardDealInput::eRepoAmount);

			// Update the spread, as spread must follow the rule mise en pension/prise en pension even for mirror deals.
			UpdateSpread();

			UpdateInterestAmount();

			//commission = trans->GetCommission();
		}
		else
		{
			UpdateSpread();
		}

		delete trans;
	}

	DisableTradeDateFields();

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/void CSxLoanAndRepoDealInput::ElementValidation(int EAId_Modified)
{
	BEGIN_LOG("ElementValidation");
	CSRLoanAndRepoDialog::ElementValidation(EAId_Modified);	

	switch (EAId_Modified)
	{
	case CSxStandardDealInput::eRepoAmount:
	case ePECoursSJ:
	case ePECours:	
	case ePENominal:
	case ePEHedging:

		{
			double principalQuantity = 0.0;
			double repoAmount = 0.;

			GetElementByRelativeId(CSxStandardDealInput::eRepoAmount)->GetValue(&repoAmount);

			if (!repoAmount)
				break;			

			CSRLoanAndRepo * currentInst = new_CSRInstrument();
			if (!currentInst)
			{
				MESS(Log::warning, "Failed to create Dialog Instrument");				
				break;
			}

			// Get Principal underlying
			long underlyingCode = currentInst->GetUnderlyingCode();
			const CSRBond * principalUnderlyingInst = dynamic_cast<const CSRBond *>(CSRInstrument::GetInstance(underlyingCode));
			if (!principalUnderlyingInst)
			{
				MESS(Log::debug, "Failed to find collateral underlying " << underlyingCode);				
				delete currentInst;
				currentInst = NULL;
				break;
			}

			// Check currency as we are not using the forex
			if (currentInst->GetCurrencyCode() != principalUnderlyingInst->GetCurrencyCode())
			{
				MESS(Log::warning, "Principal currency is different from collateral currency (not supported)");
				delete currentInst;
				currentInst = NULL;				
				break;
			}			

			// Get collateral underlying nominal
			double nominal = 0.;
			if (principalUnderlyingInst->IsFloatingNotional())			
				nominal = GetOutstanding(principalUnderlyingInst);											
			else
				nominal = principalUnderlyingInst->GetNotional();

			if (nominal == 0.0)
				nominal = 1.0;

			// Get input for calculation
			CSRTransaction * principalTrx = new_CSRTransaction();
			if (!principalTrx)
			{
				MESS(Log::warning, "Failed to create Principal Transaction");
				break;
			}

			double spot = principalTrx->GetSpot();

			if (!spot)
				break;			

			double bondPrice = 0.;

			// Calculation
			eAskQuotationType quotationType = principalUnderlyingInst->GetQuotationType();
			switch(quotationType)
			{
			case aqInPrice: // In Amount
			case aqInPriceWithoutAccrued:
				bondPrice = spot;
				principalQuantity = repoAmount/bondPrice;
				break;
			case aqInPercentage: // In Percent
			case aqInPercentWithAccrued:
				bondPrice = spot*nominal/100.;
				principalQuantity = repoAmount/bondPrice;
				break;
			default:
				{
					MESS(Log::warning, "Unsupported Quotation Type " << quotationType);
					principalQuantity = 0.0;
				}
			}

			if (!principalQuantity)
				return;

			if ((GetDealDirection() == eBorrow && EAId_Modified == ePECoursSJ) ||  (GetDealDirection() == eBorrow && EAId_Modified == ePECours))
			{
				principalQuantity = principalTrx->GetQuantity();
				repoAmount = principalQuantity*bondPrice*currentInst->GetHedgingInProduct()/100.0;
			}
			else
			{												
				principalQuantity = GetUpRoundedValue(0, principalQuantity);
				repoAmount = principalQuantity*bondPrice*currentInst->GetHedgingInProduct()/100.0;

				principalTrx->SetQuantity(principalQuantity);
			}
											

			GetElementByRelativeId(CSxStandardDealInput::eRepoAmount)->SetValue(&repoAmount);

			delete currentInst;
			currentInst = NULL;
			delete principalTrx;
			principalTrx = NULL;

			UpdateElement(ePEQuantite);

			ValidParentDialog(ePEQuantite);

			UpdateInterestAmount();

		}
		break;

	case ePEReferenceSJ:
		{		
			UpdateRepoAmount();
			UpdateSpread();
		}
		break;

	case ePEQuantite:	
		/*case ePECoursSJ:
		case ePECours:	
		case ePENominal:
		case ePEHedging:*/
		{				
			UpdateRepoAmount();
			UpdateInterestAmount();
		}		
		break;

	case CSxStandardDealInput::eSpreadHT:	
	case ePETypeOperation:
	case ePETaux:

		{						
			UpdateSpread();									
		}
		break;

	case ePEUnderlyingRefRadio:	
	case ePEUnderlyingCcyRadio:	
		{
			ShowRepoAmountFields();
			ShowRepoSpreadFields();
			UpdateSpread();
		}
		break;

	case ePETypeCollateral:
		{
			ShowRepoAmountFields();
			ShowRepoSpreadFields();
			UpdateSpread();
		}
		break;

	case ePETypeBase:
	case ePEFin:
		UpdateInterestAmount();
		break;
	}	

	DisableTradeDateFields();

	END_LOG();
}

/*virtual*/ void CSxLoanAndRepoDealInput::OnCreateTransaction(portfolio::CSRTransaction& transaction, sophis::tools::CSREventVector &messages)
{
	transaction.LoadUserElement();

	// TTP 75171 - For mirroring trades the dialog toolkit fields are empty but they are correct in the transaction
	// So don't copy them to the transaction
	//DPH
	//long mirroringRef = transaction.GetMirroringReference();
	TransactionIdent mirroringRef = transaction.GetMirroringReference();
	if (mirroringRef <= 0) // Original trade
	{
		double spreadHT = 0.;
		GetElementByRelativeId(CSxStandardDealInput::eSpreadHT)->GetValue(&spreadHT);			
		transaction.SaveGeneralElement(CSxStandardDealInput::eSpreadHT,&spreadHT);

		double amount = 0.;
		GetElementByRelativeId(CSxStandardDealInput::eRepoAmount)->GetValue(&amount);						
		transaction.SaveGeneralElement(CSxStandardDealInput::eRepoAmount,&amount);

		double interestAmount = 0.;
		GetElementByRelativeId(CSxStandardDealInput::eInterestAmount)->GetValue(&interestAmount);						
		transaction.SaveGeneralElement(CSxStandardDealInput::eInterestAmount,&interestAmount);

		double brokerTVA = 0.;
		transaction.SaveGeneralElement(CSxStandardDealInput::eBrokerTva,&brokerTVA);
		transaction.SaveGeneralElement(CSxStandardDealInput::eGrossTva,&brokerTVA);
		transaction.SaveGeneralElement(CSxStandardDealInput::eMarketTva,&brokerTVA);
		transaction.SaveGeneralElement(CSxStandardDealInput::eCounterpartyTva,&brokerTVA);
	}
	else
	{
		double interestAmount = 0.0;
		double counterpartyTVA = 0.0;
		double marketTVA = 0.0;
		transaction.SaveGeneralElement(CSxStandardDealInput::eInterestAmount, &interestAmount);
		transaction.SaveGeneralElement(CSxStandardDealInput::eMarketTva, & marketTVA);
		transaction.SaveGeneralElement(CSxStandardDealInput::eCounterpartyTva, &counterpartyTVA);
	}


	/*********** Work around for TTP 75112 ***************/

	CSRLoanAndRepo* loanAndRepo = new_CSRInstrument();
	if (loanAndRepo)
	{
		eLoanAndRepoType LBType = loanAndRepo->GetLoanAndRepoType();

		//Mise/Prise en pension
		if (LBType ==  larStockLoan)
		{
			eDealDirection dealDirection = GetDealDirection();

			if (dealDirection == eLend)
				transaction.SetQuantity(-fabs(transaction.GetQuantity()));
			else
				transaction.SetQuantity(fabs(transaction.GetQuantity()));			
		}				

		delete loanAndRepo;
	}

	/********** End of work around ***********************/
}

/*virtual*/ void CSxLoanAndRepoDealInput::OnModifyTransaction(portfolio::CSRTransaction& transaction, sophis::tools::CSREventVector &messages)
{
	transaction.LoadUserElement();

	// TTP 75171 - For mirroring trades the dialog toolkit fields are empty but they are correct in the transaction
	// So don't copy them to the transaction
	//DPH
	//long mirroringRef = transaction.GetMirroringReference();
	TransactionIdent mirroringRef = transaction.GetMirroringReference();
	if (mirroringRef <= 0)
	{
		double spreadHT = 0.;
		GetElementByRelativeId(CSxStandardDealInput::eSpreadHT)->GetValue(&spreadHT);			
		transaction.SaveGeneralElement(CSxStandardDealInput::eSpreadHT,&spreadHT);

		double amount = 0.;
		GetElementByRelativeId(CSxStandardDealInput::eRepoAmount)->GetValue(&amount);						
		transaction.SaveGeneralElement(CSxStandardDealInput::eRepoAmount,&amount);

		double interestAmount = 0.;
		GetElementByRelativeId(CSxStandardDealInput::eInterestAmount)->GetValue(&interestAmount);						
		transaction.SaveGeneralElement(CSxStandardDealInput::eInterestAmount,&interestAmount);

		double brokerTVA = 0.;
		transaction.SaveGeneralElement(CSxStandardDealInput::eBrokerTva,&brokerTVA);
		transaction.SaveGeneralElement(CSxStandardDealInput::eGrossTva,&brokerTVA);
		transaction.SaveGeneralElement(CSxStandardDealInput::eMarketTva,&brokerTVA);
		transaction.SaveGeneralElement(CSxStandardDealInput::eCounterpartyTva,&brokerTVA);
	}

	/*********** Work around for TTP 75112 ***************/

	CSRLoanAndRepo* loanAndRepo = new_CSRInstrument();
	if (loanAndRepo)
	{
		eLoanAndRepoType LBType = loanAndRepo->GetLoanAndRepoType();

		//Mise/Prise en pension
		if (LBType ==  larStockLoan)
		{
			eDealDirection dealDirection = GetDealDirection();

			if (dealDirection == eLend)
				transaction.SetQuantity(-fabs(transaction.GetQuantity()));
			else
				transaction.SetQuantity(fabs(transaction.GetQuantity()));			
		}				

		delete loanAndRepo;
	}

	/********** End of work around ***********************/
}

void CSxLoanAndRepoDealInput::UpdateSpread()
{
	BEGIN_LOG("UpdateSpread");

	double spread = 0.0;
	double spreadHT = 0.0;

	GetElementByRelativeId(CSxStandardDealInput::eSpreadHT)->GetValue(&spreadHT);

	//DPH
	if (spreadHT > 10000000000 || spreadHT < -10000000000) {
		MESS(Log::warning, FROM_STREAM("spreadHT=" << spreadHT << " which is too big, will be reset to 0"));
		spreadHT = 0; //ORA-01426: numeric overflow
		GetElementByRelativeId(CSxStandardDealInput::eSpreadHT)->SetValue(&spreadHT);
	}

	spreadHT /= 100.0;	

	CSRLoanAndRepo* loanAndRepo = new_CSRInstrument();
	if (loanAndRepo)
	{
		eLoanAndRepoType LBType = loanAndRepo->GetLoanAndRepoType();

		//Mise en pension
		if (LBType ==  larStockLoan && GetDealDirection() == eLend || LBType ==  larRepo && GetDealDirection() == eBorrow)
		{
			double rate = CSxTVARates::GetTVARate(REPO_BORROWING_TVA_RATE_ID);
			spread = spreadHT * ( 1 + rate);
		}
		else
		{
			spread = spreadHT;
		}		

		if (loanAndRepo->GetCollateralCode())
			loanAndRepo->SetMarginOnCollateral(spread);	
		else
			loanAndRepo->SetRateOnCollateral(spread);

		UpdateElement(ePEValeurTaux);		

		delete loanAndRepo;
	}

	UpdateInterestAmount();

	END_LOG();
}

void CSxLoanAndRepoDealInput::UpdateInterestAmount()
{
	BEGIN_LOG("UpdateInterestAmount");			

	//Compute the interest amount and final amount and display these values

	CSRLoanAndRepo* loanAndRepo = new_CSRInstrument();
	if (loanAndRepo)
	{				
		double interestAmount = 0.;
		double interestAmountHT = 0.;
		double finalAmount = 0.;
		double finalAmountHT = 0.;

		CSRTransaction* trx = new_CSRTransaction();
		if (trx)
		{
			long tradeDate = trx->GetTransactionDate();

			//Compute the interests HT			
			double spreadHT = 0.;			
			GetElementByRelativeId(CSxStandardDealInput::eSpreadHT)->GetValue(&spreadHT);
			spreadHT /= 100.0;

			interestAmount = CSxInterest::GetSLRoundedInterest(loanAndRepo,spreadHT,trx->GetTransactionDate(),trx->GetQuantity(),GetDealDirection(),interestAmountHT);							
			finalAmountHT = trx->GetNetAmount() + interestAmountHT - loanAndRepo->GetCouponCollateral() * trx->GetQuantity() * loanAndRepo->GetQuotity(false);
			finalAmount = trx->GetNetAmount() + interestAmount - loanAndRepo->GetCouponCollateral() * trx->GetQuantity() * loanAndRepo->GetQuotity(false);			

			finalAmountHT = GetRoundedValue(2,finalAmountHT);
            finalAmount = GetRoundedValue(2,finalAmount);


			delete trx;
		}

		GetElementByRelativeId(CSxStandardDealInput::eInterestAmount)->SetValue(&interestAmount);
		GetElementByRelativeId(CSxStandardDealInput::eFinalAmount)->SetValue(&finalAmount);
		GetElementByRelativeId(CSxStandardDealInput::eInterestAmountHT)->SetValue(&interestAmountHT);
		GetElementByRelativeId(CSxStandardDealInput::eFinalAmountHT)->SetValue(&finalAmountHT);

		UpdateElement(CSxStandardDealInput::eInterestAmount);
		UpdateElement(CSxStandardDealInput::eFinalAmount);
		UpdateElement(CSxStandardDealInput::eInterestAmountHT);
		UpdateElement(CSxStandardDealInput::eFinalAmountHT);

		delete loanAndRepo;
	}

	END_LOG();
}

void CSxLoanAndRepoDealInput::ShowRepoAmountFields()
{
	CSRLoanAndRepo* loanAndRepo = new_CSRInstrument();

	if (loanAndRepo)
	{		
		if (loanAndRepo->GetLoanAndRepoType() ==  larStockLoan && loanAndRepo->GetCollateralType() == cCash)
		{
			ShowElement(CSxStandardDealInput::eRepoAmountLabel);
			ShowElement(CSxStandardDealInput::eRepoAmount);			
			ShowElement(CSxStandardDealInput::eInterestAmountLabel);
			ShowElement(CSxStandardDealInput::eInterestAmount);
			ShowElement(CSxStandardDealInput::eFinalAmount);
			ShowElement(CSxStandardDealInput::eInterestAmountHTLabel);
			ShowElement(CSxStandardDealInput::eInterestAmountHT);
			ShowElement(CSxStandardDealInput::eFinalAmountHTLabel);
			ShowElement(CSxStandardDealInput::eFinalAmountHT);

			HideElement(ePEValeurTheo);
			HideElement(ePECommAmountLabel);
			HideElement(ePEReescompte);
		}
		else
		{
			HideElement(CSxStandardDealInput::eRepoAmountLabel);
			HideElement(CSxStandardDealInput::eRepoAmount);
			HideElement(CSxStandardDealInput::eInterestAmountLabel);
			HideElement(CSxStandardDealInput::eInterestAmount);
			HideElement(CSxStandardDealInput::eFinalAmount);
			HideElement(CSxStandardDealInput::eInterestAmountHTLabel);
			HideElement(CSxStandardDealInput::eInterestAmountHT);
			HideElement(CSxStandardDealInput::eFinalAmountHTLabel);
			HideElement(CSxStandardDealInput::eFinalAmountHT);
			
			ShowElement(ePEValeurTheo);
			ShowElement(ePECommAmountLabel);
			ShowElement(ePEReescompte);
		}

		delete loanAndRepo;
	}	
}

void CSxLoanAndRepoDealInput::ShowRepoSpreadFields()
{
	CSRLoanAndRepo* loanAndRepo = new_CSRInstrument();

	if (loanAndRepo)
	{		
		if (loanAndRepo->GetLoanAndRepoType() ==  larStockLoan && loanAndRepo->GetCollateralType() == cCash)
		{
			ShowElement(CSxStandardDealInput::eSpreadHTLabel);
			ShowElement(CSxStandardDealInput::eSpreadHT);
		}
		else
		{
			HideElement(CSxStandardDealInput::eSpreadHTLabel);
			HideElement(CSxStandardDealInput::eSpreadHT);
		}

		delete loanAndRepo;
	}
}

void CSxLoanAndRepoDealInput::UpdateRepoAmount()
{
	BEGIN_LOG("UpdateRepoAmount");

	double repoAmount = 0.;		

	CSRLoanAndRepo * currentInst = new_CSRInstrument();
	if (!currentInst)
	{
		MESS(Log::warning, "Failed to create Dialog Instrument");			
		return;
	}

	// Get Principal underlying
	long underlyingCode = currentInst->GetUnderlyingCode();
	const CSRBond * principalUnderlyingInst = dynamic_cast<const CSRBond *>(CSRInstrument::GetInstance(underlyingCode));
	if (!principalUnderlyingInst)
	{
		MESS(Log::debug, "Failed to find collateral underlying " << underlyingCode);			
		delete currentInst;
		currentInst = NULL;
		return;
	}

	// Check currency as we are not using the forex
	if (currentInst->GetCurrencyCode() != principalUnderlyingInst->GetCurrencyCode())
	{
		MESS(Log::warning, "Principal currency is different from collateral currency (not supported)");
		delete currentInst;
		currentInst = NULL;			
		return;
	}		

	// Get collateral underlying nominal
	double nominal = 0.;
	if (principalUnderlyingInst->IsFloatingNotional())			
		nominal = GetOutstanding(principalUnderlyingInst);											
	else
		nominal = principalUnderlyingInst->GetNotional();
	
	if (nominal == 0.0)
		nominal = 1.0;

	// Get input for calculation
	CSRTransaction * principalTrx = new_CSRTransaction();
	if (!principalTrx)
	{
		MESS(Log::warning, "Failed to create Principal Transaction");
		return;
	}

	double spot = principalTrx->GetSpot();

	if (!spot)
		return;			

	double bondPrice = 0.;				

	// Calculation
	eAskQuotationType quotationType = principalUnderlyingInst->GetQuotationType();
	switch(quotationType)
	{
	case aqInPrice: // In Amount
	case aqInPriceWithoutAccrued:
		bondPrice = spot;			
		break;
	case aqInPercentage: // In Percent
	case aqInPercentWithAccrued:
		bondPrice = spot*nominal/100.;			
		break;
	default:
		{
			MESS(Log::warning, "Unsupported Quotation Type " << quotationType);
			bondPrice = 0.0;
		}
	}

	if (!bondPrice)
		return;

	double principalQuantity = fabs(principalTrx->GetQuantity());

	if (principalQuantity)
		repoAmount = principalQuantity*bondPrice*currentInst->GetHedgingInProduct()/100.0;
	else
	{
		GetElementByRelativeId(CSxStandardDealInput::eRepoAmount)->GetValue(&repoAmount);

		//DPH problem of repo (infinite loop), updateamount, validate, updateamount again
		//if (!repoAmount)
		if (!repoAmount || repoAmount < 0.00000000001)
			return;

		principalQuantity = repoAmount/bondPrice;

		principalQuantity = GetUpRoundedValue(0, principalQuantity);
		repoAmount = principalQuantity*bondPrice*currentInst->GetHedgingInProduct()/100.0;

		principalTrx->SetQuantity(principalQuantity);												

		UpdateElement(ePEQuantite);

		ValidParentDialog(ePEQuantite);
	}

	GetElementByRelativeId(CSxStandardDealInput::eRepoAmount)->SetValue(&repoAmount);

	delete currentInst;
	currentInst = NULL;
	delete principalTrx;
	principalTrx = NULL;

	END_LOG();
}

double CSxLoanAndRepoDealInput::GetOutstanding(const CSRBond* bond)
{
	double ret = bond->GetNotionalInProduct();

	const CSRMarketData* mktData = CSRMarketData::GetCurrentMarketData();
	_STL::vector<SSRedemption> redemptionArray;
	bond->GetRedemption(redemptionArray,bond->GetIssueDate(),bond->GetMaturity());
	for (size_t i = 0; i < redemptionArray.size(); i++)
	{
		SSRedemption oneRedemption = redemptionArray[i];
		if (mktData->GetDate() > oneRedemption.startDate && oneRedemption.flowType == ftRedemption)
		{
			ret -= oneRedemption.redemption;
		}
	}

	return ret;
}

void CSxLoanAndRepoDealInput::DisableTradeDateFields()
{
	DisableElement(43); //Trade date
	DisableElement(74); // settlement date
	EnableElement(25); //start date
	
	CSRTransaction* trans = new_CSRTransaction();
	if (trans)
	{
		long settlementDate = trans->GetSettlementDate();
		trans->SetTransactionDate(settlementDate);
		UpdateElement(43);
	}

}
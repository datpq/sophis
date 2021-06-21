#pragma warning(disable:4251)
/*
** Includes
*/
#include "SphTools/base/CommonOS.h"
#include "SphInc/gui/SphEditElement.h"
#include "SphInc/gui/SphCustomMenu.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/portfolio/SphPaymentMthdGUIOverloader.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/backoffice_kernel/SphCorporateAction.h"
#include "SphInc/collateral/SphLoanAndRepoDialog.h"

// specific
#include "../Resource/resource.h"
#include "CSxStandardDealInput.h"
#include "CSxLoanAndRepoDealInput.h"
#include "CSxTVARates.h"
#include "CSxTransactionAction.h"
#include "CSxInterest.h"


/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::tools;
using namespace sophis::backoffice_kernel;
using namespace sophis::collateral;
using namespace sophis::value;

/*
** Methods
*/
/*
** Static
*/
const char * CSxTransactionAction::__CLASS__ = "CSxTransactionAction";

//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(CSxTransactionAction)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxTransactionAction::VoteForCreation(CSRTransaction &transaction) throw (VoteException)
{	
	BEGIN_LOG("VoteForCreation");

	transaction.LoadUserElement();

	//DPH
	double equalisationAmount = 0.;
	transaction.LoadGeneralElement(CSAMTransactionDialog::cTDEqualisationAmount, &equalisationAmount);
	if (equalisationAmount > 10000000000 || equalisationAmount < -10000000000) {
		MESS(Log::warning, FROM_STREAM("equalisationAmount=" << equalisationAmount << " which is too big, will be reset to 0"));
		equalisationAmount = 0; //ORA-01426: numeric overflow
		transaction.SaveGeneralElement(CSAMTransactionDialog::cTDEqualisationAmount, &equalisationAmount);
	}

	eTransactionOriginType trxOriginType = transaction.GetCreationKind();

	//Compute TVA amount for fees for automatic and electronic tickets
	if (trxOriginType == eTransactionOriginType::toAutomatic || trxOriginType == eTransactionOriginType::toElectronic)
	{
		ComputeTvaAmounts(transaction);
	}

	//In case of Collateral/Repo Spread Modification, the spread entered by the user is with TVA. Hence we should set this value to the spread without TVA
	eTransactionType businessEvent = transaction.GetTransactionType();
	CSRCADefaultBusinessEvent caDefaultBusinessEvent;
	eTransactionType LBSpreadModificationBECode = caDefaultBusinessEvent.GetValueElseDefault(CSRCADefaultBusinessEvent::item_stock_loan_repricing_interest_return);

	if (businessEvent == LBSpreadModificationBECode)
	{		
		const CSRInstrument* instr = CSRInstrument::GetInstance(transaction.GetInstrumentCode());
		if (instr)
		{
			int i = 0;
			const CSRLoanAndRepo* loanAndRepo = dynamic_cast<const CSRLoanAndRepo*> (instr);
			if (loanAndRepo)
			{
				double spread = transaction.GetSpot();				
				if (loanAndRepo->IsRepoCash() && transaction.GetQuantity() > 0 || !loanAndRepo->IsRepoCash() && transaction.GetQuantity() < 0)
				{
					double rate = CSxTVARates::GetTVARate(REPO_BORROWING_TVA_RATE_ID);
					spread = spread * ( 1 + rate);				
				}			

				transaction.SetSpot(spread);
			}		
		}
	}
		
	ComputeSLInterestAmount(transaction);		

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxTransactionAction::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction, long event_id)
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForModification");

	//DPH
	double equalisationAmount = 0.;
	transaction.LoadGeneralElement(CSAMTransactionDialog::cTDEqualisationAmount, &equalisationAmount);
	if (equalisationAmount > 10000000000 || equalisationAmount < -10000000000) {
		MESS(Log::warning, FROM_STREAM("equalisationAmount=" << equalisationAmount << " which is too big, will be reset to 0"));
		equalisationAmount = 0; //ORA-01426: numeric overflow
		transaction.SaveGeneralElement(CSAMTransactionDialog::cTDEqualisationAmount, &equalisationAmount);
	}

	ComputeSLInterestAmount(transaction);

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
void CSxTransactionAction::ComputeSLInterestAmount(CSRTransaction &transaction)
{
	BEGIN_LOG("ComputeSLInterestAmount");
	
	//Compute SL interest amount for Stock loans if it has not been done
	
	CSRCADefaultBusinessEvent caDefaultBusinessEvent;
	eTransactionType LBInitiationBECode = caDefaultBusinessEvent.GetValueElseDefault(CSRCADefaultBusinessEvent::item_stock_loan_lnb_initiation);
	if (transaction.GetTransactionType() == LBInitiationBECode)
	{
		const CSRLoanAndRepo* LBInstr = dynamic_cast<const CSRLoanAndRepo*>(CSRInstrument::GetInstance(transaction.GetInstrumentCode()));					 

		if (LBInstr)
		{						
			if (LBInstr->GetLoanAndRepoType() == larStockLoan && transaction.GetQuantity() < 0)
			{														
				double interestAmount = 0.;
				transaction.LoadUserElement();
				transaction.LoadGeneralElement(CSxStandardDealInput::eInterestAmount,&interestAmount);

				if (!interestAmount)
				{
					long tradeDate = transaction.GetTransactionDate();
					double amount = transaction.GetGrossAmount();
					double spreadHT = 0.;
					transaction.LoadUserElement();
					transaction.LoadGeneralElement(CSxStandardDealInput::eSpreadHT,&spreadHT);
					eDealDirection dealDirection;
					double quantity = transaction.GetQuantity();

					if (quantity >= 0)
						dealDirection = eBorrow;
					else
						dealDirection = eLend;

					double interestAmountHT = 0.;

					double interestAmount = CSxInterest::GetSLRoundedInterest(LBInstr, amount, spreadHT/100., transaction.GetTransactionDate(), transaction.GetCommissionDate(),
						quantity, dealDirection, interestAmountHT);							
					char mess[256];
					sprintf_s(mess,256,"Save SL CFG interest amount : %lf",interestAmount);
					MESS(Log::debug, mess);
					transaction.SaveGeneralElement(CSxStandardDealInput::eInterestAmount,&interestAmount);
				}																																																	
			}
		}
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
double CSxTransactionAction::ComputeTvaAmount(const double fieldAmount, const double rate)
{
	return (fieldAmount * rate) / (1 + rate);	
}

//-------------------------------------------------------------------------------------------------------------
void CSxTransactionAction::ComputeTvaAmounts(CSRTransaction& transaction)
{
	BEGIN_LOG("ComputeTvaAmounts");

	eTransactionType businessEvent = transaction.GetTransactionType();		
	double rate = CSxTVARates::GetTVARate(CSxTVARates::GetTVARateIdFromBE(businessEvent));

	//Set TVA amount for gross amount
	double TVAAmount = ComputeTvaAmount(transaction.GetGrossAmount(), rate);
	transaction.SaveGeneralElement(CSxStandardDealInput::eGrossTva, &TVAAmount);

	//Set TVA amount for broker fees
	rate = CSxTVARates::GetTVARate(BROKER_FEES_TVA_RATE_ID);
	TVAAmount = ComputeTvaAmount(transaction.GetBrokerFees(), rate);
	transaction.SaveGeneralElement(CSxStandardDealInput::eBrokerTva, &TVAAmount);

	//Set TVA amount for market fees
	rate = CSxTVARates::GetTVARate(MARKET_FEES_TVA_RATE_ID);
	TVAAmount = ComputeTvaAmount(transaction.GetMarketFees(), rate);
	transaction.SaveGeneralElement(CSxStandardDealInput::eMarketTva, &TVAAmount);

	//Set TVA amount for counterparty fees
	rate = CSxTVARates::GetTVARate(COUNTERPARTY_FEES_TVA_RATE_ID);
	TVAAmount = ComputeTvaAmount(transaction.GetCounterpartyFees(), rate);
	transaction.SaveGeneralElement(CSxStandardDealInput::eCounterpartyTva, &TVAAmount);

	END_LOG();
}
/*
** Includes
*/

// specific
#pragma warning(disable:4251) // '...' : struct '...' needs to have dll-interface to be used by clients of class '...'
#include "CheckPosition.h"
#include "SphInc/fund/SphFundBase.h"
#include "SphInc/value/kernel/SphFundPortfolio.h"
#include "SphInc/value/kernel/sphfund.h"
#include "SphInc/fund/SphFundBase.h"
#include "SphInc/value/kernel/SphFundPortfolio.h"
#include "SphInc/value/kernel/sphfund.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/instrument/SphLoanAndRepo.h"
#include "SphInc/collateral/SphStockOnLoanEnums.h"
#include "SphInc/instrument/SphBond.h"
#include "SphInc/instrument/SphEquity.h"
#include "SphInc/value/kernel/SphCashInterface.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/SphUserRights.h"
#include <stdio.h>
#include "ShortSellManager.h"
#include "SphInc/SphRiskApi.h"
#include __STL_INCLUDE_PATH(iomanip)
#include "SphInc/backoffice_kernel/SphCorporateAction.h"
//#include <algorithm>

//DPH
#include "UpgradeExtension.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::instrument;
using namespace sophis::static_data;
using namespace sophis::market_data;
using namespace sophis::value;
using namespace sophis::backoffice_kernel;
using namespace sophis::tools;

/*
** Static
*/
bool CheckPosition::isForceMode = false;
bool CheckPosition::isEquityOrBond = false;
const char * CheckPosition::__CLASS__ = "CheckPosition";
const long MAD = CSRCurrency::StringToCurrency("MAD");
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(CheckPosition)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckPosition::VoteForCreation(CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForCreation");

	try
	{
		BondCheckPosition(NULL, &transaction, false);		
	}
	catch (const VoteException& ex)
	{
		HandleException(ex, "creation");
	}
	try
	{
		// Do no check the cash when you sell
		// because I will have more cash after.
		if ( transaction.GetQuantity() > 0)
		{
			//CheckCash(NULL, &transaction, false);
		}
	}
	catch (const VoteException& ex)
	{
		HandleException(ex, "creation");
	}
	
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckPosition::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForModification");

	try
	{
		BondCheckPosition(&original, &transaction);		
	}
	catch (const VoteException & ex)
	{
		HandleException(ex, "modification");
	}
	try
	{
		// Do no check the cash when you sell
		// because I will have more cash after.
		if ( transaction.GetQuantity() > 0)
		{
			//CheckCash(&original, &transaction);
		}
	}
	catch (const VoteException & ex)
	{
		HandleException(ex, "modification");
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckPosition::VoteForDeletion(const CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForDeletion");
	try
	{
		BondCheckPosition(NULL, &transaction, true);
	}
	catch (const VoteException & ex)
	{
		HandleException(ex, "deletion");
	}
	try
	{
		// Do no check the cash when you cancel a
		// buy operation because I will have more cash 
		// after.
		if ( transaction.GetQuantity() < 0)
		{
			//CheckCash(NULL, &transaction, true);
		}		
	}
	catch (const VoteException & ex)
	{
		HandleException(ex, "deletion");
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------------
int CheckPosition::HandleException(const VoteException & ex, const char* operation) throw (VoteException)
{
	BEGIN_LOG("HandleException");
	CSRUserRights * myUserRights = new CSRUserRights();
	CSRUserRights * myGroupRights = new CSRUserRights(myUserRights->GetParentID());
	myUserRights->LoadDetails();
	myGroupRights->LoadDetails();
	eRightStatusType CFGCheckDeal = myUserRights->GetUserDefRight("CFG Check Deal");
	eRightStatusType CFGCheckDealGroup = myGroupRights->GetUserDefRight("CFG Check Deal");
	if ((CFGCheckDeal == eRightStatusType::rsEnable) || (myUserRights->GetIdent() == 1) || ((CFGCheckDeal == eRightStatusType::rsSameAsParent) && (CFGCheckDealGroup == eRightStatusType::rsEnable)))
	{
		if (isForceMode == false)
		{
			_STL::string ConfirmText = "Error in Check Deal : " + ex.getError() + ". Do you want to force the " + operation + " of the transaction ?";
			int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
			if ((resultDialog == 1) || (resultDialog == 2))
			{	
				throw;
			}
			else if (isEquityOrBond)
			{ // No double callback for LoanAndRepo
				isForceMode = true;
			}
		} else 
		{
			isForceMode = false;
		}
	}
	else if (CSRApi::IsInBatchMode())
	{
		MESS(Log::warning, "Batch Mode - Deactivate MFC Message (" << (const char *)ex << ")");
	}
	else
	{
		throw;
	}
	return 0;
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------------
void CheckPosition::BondCheckPosition(const CSRTransaction * original, const CSRTransaction * transaction, bool isDeleted /*= false*/)
{
	BEGIN_LOG("BondCheckPosition");

	if (!transaction)
	{
		MESS(Log::warning, "Empty transaction");
		END_LOG();
		return;
	}

	const CSRInstrument * currentInstrument = CSRInstrument::GetInstance(transaction->GetInstrumentCode());
	if (!currentInstrument)
	{
		MESS(Log::warning, "Empty instrument");
		END_LOG();
		return;
	}

	// Only check price of purchase / sale and stock loan initiation
	CSRCADefaultBusinessEvent caDefaultBusinessEvent;
	eTransactionType SLInitiationEventCode = caDefaultBusinessEvent.GetValueElseDefault(CSRCADefaultBusinessEvent::item_stock_loan_lnb_initiation);
	eTransactionType transactionType = transaction->GetTransactionType();
	if (transactionType != tPurchaseSale && transactionType != SLInitiationEventCode)
	{
		MESS(Log::verbose, "Don't check price for this business event (" << transactionType << ")");
		return;
	}

	bool isBuying = false;
	long underlying = 0;
	const CSRLoanAndRepo * currentLoanAndRepo = dynamic_cast<const CSRLoanAndRepo *>(currentInstrument);
	const CSRBond * currentBond = dynamic_cast<const CSRBond *>(currentInstrument);
	const CSREquity * currentEquity = dynamic_cast<const CSREquity *>(currentInstrument);
	if (currentLoanAndRepo)
	{
		isEquityOrBond = false;
		// Sicovam of the underlying
		//DPH
		//underlying = currentInstrument->GetUnderlying(0);
		//underlying = UpgradeExtension::GetUnderlying(currentInstrument, 0);
		const CSRInstrument * underlyingInstrument = currentInstrument->GetUnderlyingInstrument();
		if (underlyingInstrument) {
			underlying = underlyingInstrument->GetCode();
		}

		if (transaction->GetQuantity() > 0)
		{
			isBuying = true;
		}
		else
		{
			isBuying = false;
		}
	}

	if (currentBond)
	{
		// Sicovam of the instrument
		isEquityOrBond = true;
		underlying = currentInstrument->GetCode();
		if (transaction->GetQuantity() > 0)
			isBuying = true;
	}

	if (currentEquity)
	{
		isEquityOrBond = true;
		// Sicovam of the instrument
		underlying = currentInstrument->GetCode();
		if (transaction->GetQuantity() > 0)
			isBuying = true;
	}

	if (underlying == 0)
	{
		MESS(Log::warning, "Failed to find underlying for instrument " << transaction->GetInstrumentCode());
		END_LOG();
		return;
	}

	if (isBuying && !isDeleted && !original)
	{
		MESS(Log::debug, "Buying => Nothing to check");
		END_LOG();
		return;
	}
	else if (!isBuying && isDeleted)
	{
		MESS(Log::debug, "Delete a Selling => Nothing to check");
		END_LOG();
		return;
	}

	long paymentDate = gApplicationContext->GetDate();
	//long paymentDate = transaction->GetSettlementDate();

	// Search fund
	long folioId = transaction->GetFolioCode();

	// Retrieve the fund owner
	const CSAMPortfolio * portfolio = CSAMPortfolio::GetCSRPortfolio(folioId);
	if(!portfolio)
	{
		MESS(Log::warning, "Failed to get fund folio " << folioId);
		END_LOG();
		return;
	}

	long fundSico = portfolio->GetHedgeFund();
	const CSAMFund * fund = CSAMFund::GetFund(fundSico);
	if (!fund)
	{
		MESS(Log::warning, "Failed to get fund " << fundSico);
		END_LOG();
		return;
	}

	long fundFolioCode = fund->GetTradingPortfolio();

	// Get the quantity in the portfolio
	double limitQuantity = CSxShortSellManager::getInstance().GetQuantityAvailableOrLended(underlying, paymentDate, fundFolioCode, true);

	double currentQuantity = transaction->GetQuantity();

	if (original) // Modification Case
	{
		// Add the current quantity to the limit
		if (!isDeleted)
		{
			limitQuantity = limitQuantity + (currentQuantity - original->GetQuantity());
		}		
	} else if (isDeleted) // Deletion Case
	{
		limitQuantity = limitQuantity - currentQuantity;
		currentQuantity = 0; // If delete it means we request 0			
	} else // Creation Case
	{
		limitQuantity = limitQuantity + currentQuantity;
	}
	
	if (limitQuantity < 0.0)
	{
		char textMsg[200] = "";
		sprintf_s(textMsg, "The position will be short");
		MESS(Log::verbose, textMsg);
		throw VoteException(textMsg);
	}	

	END_LOG();
}

//----------------------------------------------------------------------------------------------------
void CheckPosition::CheckCash(const CSRTransaction * original, const CSRTransaction * transaction, bool isDeleted /*= false*/)
{
	BEGIN_LOG("CheckCash");

	bool isBuying = (transaction->GetQuantity() > 0);	
	if (!isBuying && !isDeleted)
	{
		MESS(Log::debug, "Selling => Nothing to check");
		END_LOG();
		return;
	}

	long paymentDate = transaction->GetSettlementDate();
	
	// Search fund
	long folioId = transaction->GetFolioCode();

	// Retrieve the fund owner
	const CSAMPortfolio * portfolio = CSAMPortfolio::GetCSRPortfolio(folioId);
	if(!portfolio)
	{
		MESS(Log::warning, "Failed to get fund folio " << folioId);
		END_LOG();
		return;
	}

	long fundSico = portfolio->GetHedgeFund();
	const CSAMFund * fund = CSAMFund::GetFund(fundSico);
	if (!fund)
	{
		MESS(Log::warning, "Failed to get fund " << fundSico);
		END_LOG();
		return;
	}

	double originalAmount = 0.0;
	if (original)
	{
		originalAmount = original->GetNetAmount();
	}

	
	/*
	if (originalAmount > 0 && isDeleted)
	{
		MESS(Log::debug, "Delete a Buying => Nothing to check");
		END_LOG();
		return;
	}
	else if (originalAmount < 0 && !isDeleted)
	{
		MESS(Log::debug, "Selling => Nothing to check");
		END_LOG();
		return;
	}*/
	
	const CSRCurrency* pntCur = CSRCurrency::GetCSRCurrency(transaction->GetSettlementCurrency());	
	if (pntCur && (pntCur->fCurrency == MAD) )
	{
		long cashAccountDate = paymentDate;
		if (paymentDate > CSRMarketData::GetCurrentMarketData()->GetDate())
			cashAccountDate =  transaction->GetSettlementDate();//CSRMarketData::GetCurrentMarketData()->GetDate();
		if (original || isDeleted)
		{
			cashAccountDate = gApplicationContext->GetDate();
		}

		/*{
			CashProjection cash(fundSico, cashAccountDate, 0 , 0);
			cash.ComputeCash();
			CashAmounts amounts = cash.GetCashAmounts(MAD, 0, cashAccountDate, true);
			double balance = cash.GetBalance(MAD, 0, cashAccountDate, true);
			double sramount = cash.GetSRAmount();
			double poseamount = cash.GetPositionAmount();
			double fxfxdamount = cash.GetFXForwardAmount();
			double accinterest = cash.GetAccountInterest();
			double tsramount = cash.GetTransferAmount();
			double redempfee = cash.GetRedemptionFee();
			int i = 0;
		}*/

		CashProjection cash(fundSico, cashAccountDate, 0 , 0);
		cash.ComputeCash();
		CashAmounts amounts = cash.GetCashAmounts(MAD, 0, cashAccountDate, true);
		double balance = cash.GetBalance(MAD, 0, cashAccountDate, true);

		/*CashPerAccount currentCashPerAccount(fundSico, true, cashAccountDate);
		currentCashPerAccount.ComputeCash(edtSettlementDate, 0);
		CashAmounts cashAmount = currentCashPerAccount.GetCumulatedCashAmounts(MAD, 0, cashAccountDate, true);
		double totalCash = originalAmount;
		if (cashAmount.fSRAmount			!= NOTDEFINED)	totalCash += cashAmount.fSRAmount;
		if (cashAmount.fPositionAmount		!= NOTDEFINED)	totalCash += cashAmount.fPositionAmount;
		if (cashAmount.fFXForwardAmount		!= NOTDEFINED)	totalCash += cashAmount.fFXForwardAmount;
		if (cashAmount.fInterestAmount		!= NOTDEFINED)	totalCash += cashAmount.fInterestAmount;
		if (cashAmount.fTransferAmount		!= NOTDEFINED)	totalCash += cashAmount.fTransferAmount;
		if (cashAmount.fRedemptionFeeAmount != NOTDEFINED)	totalCash += cashAmount.fRedemptionFeeAmount;

		double transactionAmount = transaction->GetNetAmount();
		if (balance + originalAmount < transactionAmount)
		{
			throw VoteException(FROM_STREAM("Not enough cash (available " 
										<< _STL::fixed << _STL::setprecision(2) << totalCash 
										<< ", required " << _STL::fixed << _STL::setprecision(2) << transactionAmount << ")"));
		}	
		*/
		double transactionAmount = 0;
		if (isDeleted)
		{
			transactionAmount -= transaction->GetNetAmount();
		}else 
			transactionAmount = transaction->GetNetAmount();


		if (balance + originalAmount < transactionAmount)
		{
			double newbalance = balance - transactionAmount ;
			if (original)
			{
				newbalance = balance + originalAmount - transactionAmount;
			} 
			throw VoteException(FROM_STREAM("Not enough cash (available " 
										<< _STL::fixed << _STL::setprecision(2) << balance 
										<< ", new balance deficit " << _STL::fixed << _STL::setprecision(2) << newbalance << ")"));
		}	
	}
	
	END_LOG();
}
/*
** Includes
*/

// specific
#include "CheckDevise.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphLLInc/portfolio/CashBalanceInterface.h"
#include "SphInc/value/kernel/SphCashInterface.h"
#include "SphInc/value/kernel/SphFundPortfolio.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/SphUserRights.h"
#include "SphInc/instrument/SphEquity.h"
#include "SphInc/SphRiskApi.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::value;
using namespace sophis::tools;
using namespace sophis::market_data;

/*
** Static
*/
const char * CheckDevise::__CLASS__ = "CheckDevise";
bool CheckDevise::isForceMode = false;
bool CheckDevise::isEquity = false;
const long CheckDevise::MAD = CSRCurrency::StringToCurrency("MAD");
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(CheckDevise)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckDevise::VoteForCreation(CSRTransaction &transaction) throw (VoteException)
{
	BEGIN_LOG("VoteForCreation");
	try
	{
		FundCheckCurrency(NULL, &transaction, false);
	}
	catch (const VoteException& ex)
	{
		CSRUserRights * myUserRights = new CSRUserRights();
		CSRUserRights * myGroupRights = new CSRUserRights(myUserRights->GetParentID());
		myUserRights->LoadDetails();
		myGroupRights->LoadDetails();
		eRightStatusType CFGCheckCoupon = myUserRights->GetUserDefRight("CFG Check Currency");
		eRightStatusType CFGCheckCouponGroup = myGroupRights->GetUserDefRight("CFG Check Currency");
		if ((CFGCheckCoupon == eRightStatusType::rsEnable) || (myUserRights->GetIdent() == 1) || ((CFGCheckCoupon == eRightStatusType::rsSameAsParent) && (CFGCheckCouponGroup == eRightStatusType::rsEnable)))
		{
			if (isForceMode == false)
			{
				_STL::string ConfirmText = "Warning in Check Currency : " + ex.getError() + " Do you want to force the creation of the transaction ?";
				int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
				if ((resultDialog == 1) || (resultDialog == 2)) 
				{
					throw;					
				}
				else if (isEquity)
				{
					isForceMode = true;
				}
			} else 
			{
				isForceMode = false;
			}
		} else if (CSRApi::IsInBatchMode())
		{
			MESS(Log::warning, "Batch Mode - Deactivate MFC Message (" << (const char *)ex << ")");
		}
		else
		{
			throw;			
		}
	}
	END_LOG();
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckDevise::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForCreation");
	try
	{
		FundCheckCurrency(&original, &transaction, false);
	}
	catch (const VoteException& ex)
	{
		CSRUserRights * myUserRights = new CSRUserRights();
		CSRUserRights * myGroupRights = new CSRUserRights(myUserRights->GetParentID());
		myUserRights->LoadDetails();
		myGroupRights->LoadDetails();
		eRightStatusType CFGCheckCoupon = myUserRights->GetUserDefRight("CFG Check Currency");
		eRightStatusType CFGCheckCouponGroup = myGroupRights->GetUserDefRight("CFG Check Currency");
		if ((CFGCheckCoupon == eRightStatusType::rsEnable) || (myUserRights->GetIdent() == 1) || ((CFGCheckCoupon == eRightStatusType::rsSameAsParent) && (CFGCheckCouponGroup == eRightStatusType::rsEnable)))
		{
			if (isForceMode == false)
			{
				_STL::string ConfirmText = "Warning in Check Currency : " + ex.getError() + " Do you want to force the creation of the transaction ?";
				int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
				if ((resultDialog == 1) || (resultDialog == 2)) 
				{
					throw;					
				}
				else if (isEquity)
				{
					isForceMode = true;
				}
			} else 
			{
				isForceMode = false;
			}
		} else if (CSRApi::IsInBatchMode())
		{
			MESS(Log::warning, "Batch Mode - Deactivate MFC Message (" << (const char *)ex << ")");
		}
		else
		{
			throw;			
		}
	}
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckDevise::VoteForDeletion(const CSRTransaction &transaction)
throw (VoteException)
{

}


void CheckDevise::FundCheckCurrency(const sophis::portfolio::CSRTransaction * original, const sophis::portfolio::CSRTransaction * transaction, bool isDeleted)
{
	BEGIN_LOG("FundCheckCurrency");	
	isEquity = false;
	if (transaction->GetQuantity() < 0)
	{
		return;
	}
	const CSRInstrument * currentInstrument = CSRInstrument::GetInstance(transaction->GetInstrumentCode());
	if (!currentInstrument)
	{
		MESS(Log::warning, "Empty instrument");
		END_LOG();
		return;
	}
	if ( currentInstrument->GetType() == eInstrumentType::iBond || currentInstrument->GetType() == eInstrumentType::iEquity)
	{
		isEquity = true;
		long portfolioId = transaction->GetFolioCode();
		const CSRCurrency* pntCur = CSRCurrency::GetCSRCurrency(transaction->GetSettlementCurrency());
		if (!pntCur || (pntCur->fCurrency == MAD) )
		{
			return;
		}
		const CSAMPortfolio* pAMFolio = sophis::value::CSAMPortfolio::GetCSRPortfolio(portfolioId);
		if (!pAMFolio)
		{
			return;
		}
		const CSAMPortfolio* pRootFolio = pAMFolio->GetFundRootPortfolio();

		if (pRootFolio)
		{
			CheckBalance(pRootFolio, transaction);
		}			
	}		
	END_LOG();
}

void CheckDevise::CheckBalance(const CSAMPortfolio* pRootFolio, const CSRTransaction * transaction) throw (VoteException)
{
	BEGIN_LOG("CheckBalance");	
	bool hasChangeDate = false;
	long today = gApplicationContext->GetDate();
	long paymentDate = transaction->GetSettlementDate();
	if (paymentDate > today)
	{
		ChangeSophisCurrentDate(paymentDate);		
		hasChangeDate = true;
	}			
	
	CashPerAccount cash(pRootFolio->GetHedgeFund(), true);
	cash.ComputeCash(EffectiveDateType::edtSettlementDate, paymentDate);				
	double balance = cash.GetBalance(transaction->GetSettlementCurrency());

	if (hasChangeDate)
	{
		ChangeSophisCurrentDate(today);
	}
	if (balance <= 0)
	{
		char textMsg[200] = "";
		char currency[8] = "";
		CSRCurrency::CurrencyToString(transaction->GetSettlementCurrency(), currency);
		sprintf_s(textMsg, "Fund balance doesn't contain any cash for currency %s.", currency);
		MESS(Log::verbose, textMsg);
		throw VoteException(textMsg);				
	}
	else if (transaction->GetNetAmount() > balance)
	{
		char textMsg[200] = "";
		sprintf_s(textMsg, "Net amount exceed the fund balance of %#.2lf.", balance);
		MESS(Log::verbose, textMsg);
		throw VoteException(textMsg);				
	}
	
	END_LOG();
}

void CheckDevise::ChangeSophisCurrentDate(const long newDate)
{
	CSRMarketData::SSDates dates;
	dates.fPosition = newDate;
	dates.UseIt();
}
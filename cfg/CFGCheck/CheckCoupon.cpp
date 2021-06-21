#pragma warning(disable:4251)
/*
** Includes
*/

// specific
#include "CheckCoupon.h"
#include "SphInc/fund/SphFundBase.h"
#include "SphInc/value/kernel/sphfund.h"
#include "SphInc/fund/SphFundBase.h"
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
#include "SphInc/SphRiskApi.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/backoffice_kernel/SphKernelStatusGroup.h"
#include "SphInc/backoffice_kernel/SphCorporateAction.h"

//DPH
#include "UpgradeExtension.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::instrument;
using namespace sophis::tools;
using namespace sophis::backoffice_kernel;

/*
** Static
*/
const char * CheckCoupon::__CLASS__ = "CheckCoupon";
bool CheckCoupon::isForceMode = false;
bool CheckCoupon::isBond = false;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(CheckCoupon)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckCoupon::VoteForCreation(CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForCreation");

	try
	{
		BondCheckCoupon(NULL, &transaction, false);
	}
	catch (const VoteException& ex)
	{		
		CSRUserRights * myUserRights = new CSRUserRights();
		CSRUserRights * myGroupRights = new CSRUserRights(myUserRights->GetParentID());
		myUserRights->LoadDetails();
		myGroupRights->LoadDetails();
		eRightStatusType CFGCheckCoupon = myUserRights->GetUserDefRight("CFG Check Coupon");
		eRightStatusType CFGCheckCouponGroup = myGroupRights->GetUserDefRight("CFG Check Coupon");
		if ((CFGCheckCoupon == eRightStatusType::rsEnable) || (myUserRights->GetIdent() == 1) || ((CFGCheckCoupon == eRightStatusType::rsSameAsParent) && (CFGCheckCouponGroup == eRightStatusType::rsEnable)))
		{
			if (CheckCoupon::isForceMode == false)
			{
				_STL::string ConfirmText = "Error in Check Coupon : " + ex.getError() + " Do you want to force the creation of the transaction ?";
				int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
				if ((resultDialog == 1) || (resultDialog == 2)) 
				{
					throw;					
				}
				else if (CheckCoupon::isBond)
				{
					CheckCoupon::isForceMode = true;
				}
			} else 
			{
				CheckCoupon::isForceMode = false;
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
/*virtual*/ void CheckCoupon::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
throw (VoteException)
{
/*	BEGIN_LOG("VoteForModification");

	try
	{
		BondCheckCoupon(&original, &transaction, false);
	}
	catch (const VoteException& ex)
	{		
		CSRUserRights * myUserRights = new CSRUserRights();
		CSRUserRights * myGroupRights = new CSRUserRights(myUserRights->GetParentID());
		myUserRights->LoadDetails();
		myGroupRights->LoadDetails();
		eRightStatusType CFGCheckCoupon = myUserRights->GetUserDefRight("CFG Check Coupon");
		eRightStatusType CFGCheckCouponGroup = myGroupRights->GetUserDefRight("CFG Check Coupon");
		if ((CFGCheckCoupon == eRightStatusType::rsEnable) || (myUserRights->GetIdent() == 1) || ((CFGCheckCoupon == eRightStatusType::rsSameAsParent) && (CFGCheckCouponGroup == eRightStatusType::rsEnable)))
		{
			if (CheckCoupon::isForceMode == false)
			{
				_STL::string ConfirmText = "Error in Check Coupon : " + ex.getError() + " Do you want to force the creation of the transaction ?";
				int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
				if ((resultDialog == 1) || (resultDialog == 2)) 
				{
					throw;					
				}
				else if (CheckCoupon::isBond)
				{
					CheckCoupon::isForceMode = true;
				}
			} else 
			{
				CheckCoupon::isForceMode = false;
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

	END_LOG();*/
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckCoupon::VoteForDeletion(const CSRTransaction &transaction)
throw (VoteException)
{
/*	BEGIN_LOG("VoteForDeletion");

	try
	{
		BondCheckCoupon(NULL, &transaction, false);
	}
	catch (const VoteException& ex)
	{		
		CSRUserRights * myUserRights = new CSRUserRights();
		CSRUserRights * myGroupRights = new CSRUserRights(myUserRights->GetParentID());
		myUserRights->LoadDetails();
		myGroupRights->LoadDetails();
		eRightStatusType CFGCheckCoupon = myUserRights->GetUserDefRight("CFG Check Coupon");
		eRightStatusType CFGCheckCouponGroup = myGroupRights->GetUserDefRight("CFG Check Coupon");
		if ((CFGCheckCoupon == eRightStatusType::rsEnable) || (myUserRights->GetIdent() == 1) || ((CFGCheckCoupon == eRightStatusType::rsSameAsParent) && (CFGCheckCouponGroup == eRightStatusType::rsEnable)))
		{
			if (CheckCoupon::isForceMode == false)
			{
				_STL::string ConfirmText = "Error in Check Coupon : " + ex.getError() + " Do you want to force the creation of the transaction ?";
				int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
				if ((resultDialog == 1) || (resultDialog == 2)) 
				{
					throw;					
				}
				else if (CheckCoupon::isBond)
				{
					CheckCoupon::isForceMode = true;
				}
			} else 
			{
				CheckCoupon::isForceMode = false;
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

	END_LOG();*/
}

bool CheckCoupon::HasACoupon(long startDate, long endDate, const CSRBond * currentBond)
{
	long nextCouponDate = 0;
	bool result = false;

	SSExplication explication;
	explication.transactionDate = currentBond->GetStartDate();
	explication.settlementDate  = currentBond->GetSettlementDate(explication.transactionDate);
	explication.pariPassuDate   = currentBond->GetPariPassuDate(explication.transactionDate, explication.settlementDate);
	explication.endDate         = 0; // all flows
	explication.adjustedDates   = currentBond->GetMarketCalculationYTMOnAdjustedDates();
	explication.valueDates      = currentBond->GetMarketCalculationYTMOnSettlementDate();
	explication.dayCountBasis   = CSRDayCountBasis::GetCSRDayCountBasis( currentBond->GetMarketYTMDayCountBasisType());

	_STL::vector<SSRedemption*>  explicationArray; 
	currentBond->GetRedemptionExplication(*gApplicationContext, explicationArray, explication, 0);

	size_t nbRedemptions = explicationArray.size();
	//DPH 733
	//CSRCalendar* calendar = currentBond->NewCSRCalendar();
	size_t iRedemption = 0;
	for(iRedemption = 0; iRedemption < nbRedemptions; iRedemption++)
	{
		SSBondExplication bondRedemption;
		bondRedemption = *( (SSBondExplication*) explicationArray.at(iRedemption));
		nextCouponDate = currentBond->GetAdjustedDate(bondRedemption.maturityDate);
		if (startDate <= nextCouponDate && nextCouponDate <= endDate)
		{ 
			result = true;
			break;
		} else if (nextCouponDate > endDate)
		{
			break;
		}
	}
	return result;
}



//-------------------------------------------------------------------------------------------------------------
void CheckCoupon::BondCheckCoupon(const sophis::portfolio::CSRTransaction * original, const sophis::portfolio::CSRTransaction * transaction, bool isDeleted)
{
	BEGIN_LOG("BondCheckCoupon");
	
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
	long paymentDate = transaction->GetSettlementDate();

	// Recover the open days option.
	long nbBusinessDays = 0;
	try
	{
		ConfigurationFileWrapper::getEntryValue("CFG_CHECK", "CFG_Business_Days", nbBusinessDays, 3);		
	}
	catch(ConfigurationFileException & cfe)
	{
		MESS(Log::warning, "Failed to find property 'CFG_Business_Days' in section 'CHECK' (" << (const char *)cfe << ")");
		return;
	}	
	catch (sophisTools::base::ExceptionBase ex)
	{
		MESS(Log::warning, (const char *)ex);
		return;
	}

	// Loand And Repo case
	const CSRLoanAndRepo * currentLoanAndRepo = dynamic_cast<const CSRLoanAndRepo *>(currentInstrument);	
	if (currentLoanAndRepo && currentInstrument->GetType() == eInstrumentType::iStockLoan )
	{
		CheckCoupon::isBond = false;
		// Sicovam of the underlying
		//DPH
		//underlying = currentInstrument->GetUnderlying(0);
		//underlying = UpgradeExtension::GetUnderlying(currentInstrument, 0);
		const CSRInstrument * underlyingInstrument = currentInstrument->GetUnderlyingInstrument();
		underlying = underlyingInstrument->GetCode();
		if (underlying == 0)
		{
			MESS(Log::warning, "Failed to find underlying for instrument " << transaction->GetInstrumentCode());
			END_LOG();
			return;
		} else 
		{						
			//DPH
			//const CSRInstrument * underlyingInstrument = CSRInstrument::GetInstance(underlying);
			// Underlying must be a Bon
			const CSRBond * currentBond = dynamic_cast<const CSRBond *>(underlyingInstrument);
			if (currentBond)
			{
				long maturityDate = transaction->GetCommissionDate();				
				long nextCouponDate = 0;
				if (HasACoupon(paymentDate, maturityDate, currentBond))
				{
					char textMsg[200] = "";
					sprintf_s(textMsg, "A outstanding coupon will occur between the StockLoan value and maturity dates.");
					MESS(Log::verbose, textMsg);
					throw VoteException(textMsg);					
				}
				else {
					//DPH 733
					//CSRCalendar* calendar = currentBond->NewCSRCalendar();
					const CSRCalendar* calendar = currentBond->GetCSRCalendar(eictDefault);
					paymentDate = calendar->AddNumberOfDays(paymentDate, -nbBusinessDays);
					maturityDate = calendar->AddNumberOfDays(maturityDate, nbBusinessDays);	
					if (HasACoupon(paymentDate, maturityDate, currentBond))
					{
						char textMsg[200] = "";
						sprintf_s(textMsg, "A outstanding coupon will occur between the Repo value date -%d business day(s) and Repo maturity date +%d business day(s).", nbBusinessDays, nbBusinessDays);
						MESS(Log::verbose, textMsg);
						throw VoteException(textMsg);					
					}
				}
			}
		}
	}else 
	{
		CheckCoupon::isBond = true;
		// new deal input case
		const CSRBond * currentBond = dynamic_cast<const CSRBond *>(currentInstrument);
		if (currentBond && transaction->GetQuantity() < 0)
		{
			long startDate = paymentDate;
			long endDate = paymentDate;
			if (nbBusinessDays > 0)
			{		
				//DPH
				//CSRCalendar* calendar = currentBond->NewCSRCalendar();
				const CSRCalendar* calendar = currentBond->GetCSRCalendar(eictDefault);
				startDate = calendar->AddNumberOfDays(paymentDate, -nbBusinessDays);
				endDate = calendar->AddNumberOfDays(paymentDate, nbBusinessDays);
			}
			if (HasACoupon(startDate, endDate, currentBond))
			{
				char textMsg[200] = "";
				sprintf_s(textMsg, "A outstanding coupon will occur between the trade value date -%d business day(s) and value date +%d business day(s).", nbBusinessDays, nbBusinessDays);
				MESS(Log::verbose, textMsg);
				throw VoteException(textMsg);							
			}
		}
	}

	END_LOG();
}
#pragma warning(disable:4251)
/*
** Includes
*/

// specific
#include "CheckPrice.h"
#include "ShareDialog.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/backoffice_kernel/SphKernelStatusGroup.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/SphUserRights.h"
#include "SphInc/SphRiskApi.h"
#include "SphInc/instrument/SphEquity.h"
#include "SphInc/instrument/SphBond.h"
#include "SphInc/instrument/SphLoanAndRepo.h"
#include "SphInc/collateral/SphStockOnLoanEnums.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/backoffice_kernel/SphKernelStatusGroup.h"
#include "SphInc/backoffice_kernel/SphCorporateAction.h"
//DPH
//#include __STL_TR1_INCLUDE_PATH(memory)
#include <memory>
#include "UpgradeExtension.h"


/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;
using namespace sophis::tools;
using namespace sophis::instrument;

/*
** Static
*/
const char * CheckPrice::__CLASS__ = "CheckPrice";
bool CheckPrice::isForceMode = false;
bool CheckPrice::isShare = false;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(CheckPrice)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckPrice::VoteForCreation(CSRTransaction &transaction) throw (VoteException)
{
	BEGIN_LOG("VoteForCreation");
	try
	{
		BondCheckPrice(NULL, &transaction);	
	}
	catch (const VoteException& ex)
	{	
		if (CSRApi::IsInBatchMode())
		{
			MESS(Log::warning, "Batch Mode - Deactivate MFC Message (" << (const char *)ex << ")");
		}
		else if (isForceMode == false)
		{
			_STL::string ConfirmText = "Warning in Check Price : " + ex.getError() + "\nDo you want to force the creation of the transaction ?";
			int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
			if ((resultDialog == 1) || (resultDialog == 2)) 
			{
				throw;					
			}
			else if (isShare)
			{
				isForceMode = true;
			}
		} else 
		{
			isForceMode = false;
		}						
	}
	END_LOG();
}

double CheckPrice::GetCollateralizedPrice(const CSRBond * currentBond, const sophis::portfolio::CSRTransaction * transaction, const CSRLoanAndRepo * currentLoanAndRepo)
{
	double hedging = currentLoanAndRepo->GetHedgingInProduct();
	double haircut = currentLoanAndRepo->GetHairCut();
	double coursSJ = transaction->GetSpot();
	double accruedCoupon = transaction->GetAccruedCoupon();
	double quotityCP = currentBond->GetQuotity();
	double forexSpot = transaction->GetForexSpot();
	double fCollateralizedPrice = (coursSJ + accruedCoupon) * quotityCP * hedging * forexSpot / haircut;
	return fCollateralizedPrice;
}

double string_to_double( const _STL::string& s )
 {
	 _STL::istringstream i(s);
   double x;
   if (!(i >> x))
     return 0;
   return x;
 }


//-------------------------------------------------------------------------------------------------------------------------------------------------------
void CheckPrice::BondCheckPrice(const sophis::portfolio::CSRTransaction * original, const sophis::portfolio::CSRTransaction * transaction, bool isDeleted)
{
	BEGIN_LOG("BondCheckPrice");
	
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
	
	long underlying = 0;
	//long paymentDate = transaction->GetSettlementDate();

	double priceGap = 5;
	_STL::string strPriceGap;
	try
	{
		ConfigurationFileWrapper::getEntryValue("CFG_CHECK", "CFG_Price_Gap", strPriceGap, "5.0");
		priceGap = abs(string_to_double(strPriceGap));		
	}
	catch(ConfigurationFileException & cfe)
	{
		MESS(Log::warning, "Failed to find property 'CFG_Price_Gap' in section 'CFG_CHECK' (" << (const char *)cfe << ")");
		return;
	}	
	catch (sophisTools::base::ExceptionBase ex)
	{
		MESS(Log::warning, (const char *)ex);
		return;
	}


	// StockLoan And Repo case
	const CSRLoanAndRepo * currentLoanAndRepo = dynamic_cast<const CSRLoanAndRepo *>(currentInstrument);	
	if (currentLoanAndRepo && currentInstrument->GetType() == 'P' ) // StockLoan Case
	{			
		isShare = false;
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
				double dirtyPrice;
				double last = currentBond->GetLast(currentBond->GetCode());
				if (last > 0.0)
				{
					dirtyPrice = last;
				} else
				{
					const CSRMarketData* context = CSRMarketData::GetCurrentMarketData();
					//DPH
					//dirtyPrice = currentBond->GetTheoreticalValue(*context);
					dirtyPrice = UpgradeExtension::GetTheoreticalValue(currentBond, *context);
				}
				if (dirtyPrice <= 0)
				{
					char textMsg[200] = "";
					sprintf_s(textMsg, "DirtyPrice should not be negative or equals to zero.");
					MESS(Log::verbose, textMsg);
					throw VoteException(textMsg);
				}
				//double collateralSpot = GetCollateralizedPrice(currentBond, transaction, currentLoanAndRepo);
				double collateralSpot = transaction->GetSpot();
				eAskQuotationType eQuotationType = currentBond->GetQuotationType();
				if ( (eQuotationType == eAskQuotationType::aqInPercentage) || 
				     (eQuotationType == eAskQuotationType::aqInPercentWithAccrued) )
				{										
					collateralSpot *= currentBond->GetNotional();
					collateralSpot /= 100;				
					dirtyPrice *= currentBond->GetNotional();
					dirtyPrice /= 100;
				}
				double gap	= abs((dirtyPrice - collateralSpot) / dirtyPrice) * 100;
				if (gap > priceGap)
				{
					char textMsg[200] = "";
					sprintf_s(textMsg, "The price exceeds the specified limit of %.2f%%.", priceGap);
					MESS(Log::verbose, textMsg);
					throw VoteException(textMsg);							
				}
			}
		}
	} else 
	{
		isShare = true;
		// new deal input case
		const CSREquity * currentEquity = dynamic_cast<const CSREquity *>(currentInstrument);
		if (currentEquity)
		{
			double sharePriceGap = -1.0;
			bool result = currentInstrument->LoadGeneralElement(ShareDialog::ePriceGap, &sharePriceGap);
			if (result)
			{
				double dealPrice = transaction->GetSpot();
				double lastPrice = currentEquity->GetLast(currentEquity->GetCode());
				double dealGap = abs((lastPrice - dealPrice) / lastPrice) * 100;
				if (lastPrice <= 0)
				{
					char textMsg[200] = "";
					sprintf_s(textMsg, "No historical price found.");
					MESS(Log::verbose, textMsg);
					throw VoteException(textMsg);
				}
				else if (dealGap > sharePriceGap)
				{
					char textMsg[200] = "";
					sprintf_s(textMsg, "The price exceeds the specified limit of %.2f%%.", sharePriceGap);
					MESS(Log::verbose, textMsg);
					throw VoteException(textMsg);
				}
			}
		}
	}

	END_LOG();
}
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckPrice::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForModification");
	try
	{
		BondCheckPrice(&original, &transaction);	
	}
	catch (const VoteException& ex)
	{	
		if (CSRApi::IsInBatchMode())
		{
			MESS(Log::warning, "Batch Mode - Deactivate MFC Message (" << (const char *)ex << ")");
		}
		else if (isForceMode == false)
		{
			_STL::string ConfirmText = "Warning in Check Price : " + ex.getError() + "\nDo you want to force the creation of the transaction ?";
			int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
			if ((resultDialog == 1) || (resultDialog == 2)) 
			{
				throw;					
			}
			else if (isShare)
			{
				isForceMode = true;
			}
		} else 
		{
			isForceMode = false;
		}						
	}
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckPrice::VoteForDeletion(const CSRTransaction &transaction)
throw (VoteException)
{

}

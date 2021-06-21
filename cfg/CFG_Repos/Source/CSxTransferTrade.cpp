#pragma warning(disable:4251)
/*
** Includes
*/
#include "SphTools/base/CommonOS.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/portfolio/SphTransactionVector.h"
#include "SphInc/backoffice_kernel/SphCorporateAction.h"
#include "SphInc/collateral/SphLoanAndRepoDialog.h"
#include <windows.h>
#include "SphLLInc/sophismath.h"
#include "SphInc/backoffice_kernel/SphBusinessEvent.h"
// specific
#include "Constants.h"
#include "CSxTVARates.h"
#include "CSxInterest.h"
#include "CSxStandardDealInput.h"
#include "CSxTransferTrade.h"
#include "CFG_Accounting/CFG_Conditions.h"


/*
** Namespace
*/
using namespace sophis::instrument;
using namespace sophis::portfolio;
using namespace sophis::tools;
using namespace sophis::backoffice_kernel;
using namespace sophis::collateral;

/*
** Static
*/
const char * CSxTransferTrade::__CLASS__ = "CSxTransferTrade";

/*
** Methods
*/
/*virtual*/ long CSxTransferTrade::TransferForCreationTrade(CSRTransaction &transaction)
{	
	BEGIN_LOG("TransferForCreationTrade");

	//DPH
	//long  id=transaction.GetTransactionCode();
	TransactionIdent id = transaction.GetTransactionCode();
	//DPH
	//long  id2=0;
	TransactionIdent id2 = 0;
	CSRCADefaultBusinessEvent caDefaultBusinessEvent;
	eTransactionType LBCollateralRemunerationBECode = caDefaultBusinessEvent.GetValueElseDefault(CSRCADefaultBusinessEvent::item_stock_loan_collateral_remuneration);
	if (transaction.GetTransactionType() == LBCollateralRemunerationBECode)
	{		
		const CSRPosition* pos = CSRPosition::GetCSRPosition(transaction.GetPositionID());
		if (pos)
		{						
			eTransactionType LBInitiationBECode = caDefaultBusinessEvent.GetValueElseDefault(CSRCADefaultBusinessEvent::item_stock_loan_lnb_initiation);

			CSRTransactionVector trxVect;
			pos->GetTransactions(trxVect);
			for (size_t i = 0; i < trxVect.size(); i++)
			{
				if (trxVect[i].GetTransactionType() == LBInitiationBECode)
				{
					const CSRLoanAndRepo* LBInstr = dynamic_cast<const CSRLoanAndRepo*>(CSRInstrument::GetInstance(trxVect[i].GetInstrumentCode()));					 

					if (LBInstr)
					{						
						if (LBInstr->GetLoanAndRepoType() == larStockLoan && trxVect[i].GetQuantity() < 0)
						{														
							long tradeDate = trxVect[i].GetTransactionDate();											
							double interestAmount = 0.;	
							 id2=trxVect[i].GetTransactionCode();
							trxVect[i].LoadUserElement();
							trxVect[i].LoadGeneralElement(CSxStandardDealInput::eInterestAmount,&interestAmount);
							double currentQuantity = transaction.GetQuantity();
							double spot = fabs(interestAmount/currentQuantity);
							double currentSpot = transaction.GetSpot();
							MESS(Log::debug, "Current Spot " << currentSpot);
							MESS(Log::debug, "Update with spot " << spot << " (interest amount " << interestAmount << ", quantity " << currentQuantity);
							transaction.SetSpot(spot);															

							break;																					
						}
					}
				}
			}
		}		
	}

	bool isABond = false;
	bool isAmortissable = false;
	bool isFinal = false;

	//DPH
	//short partialRedemptionBusinessEvent = CSRBusinessEvent::GetIdByName("Partial Redemption");
	eTransactionType partialRedemptionBusinessEvent = CSRBusinessEvent::GetIdByName("Partial Redemption");
	if (!partialRedemptionBusinessEvent)
	{
		MESS(Log::error, "Failed to find business Event 'Partial Redemption' id");
		END_LOG();
		return eActionToTransfer::attModifyInAutomatic;
	}

	//DPH
	//short redemptionBusinessEvent = CSRBusinessEvent::GetIdByName("Redemption");
	eTransactionType redemptionBusinessEvent = CSRBusinessEvent::GetIdByName("Redemption");
	if (!redemptionBusinessEvent)
	{
		MESS(Log::error, "Failed to find business Event 'Redemption' id");
		END_LOG();
		return eActionToTransfer::attModifyInAutomatic;
	}

	//Is it an amortizable bond and is it final ?
	CFG_AccountingConditionsMgr::GetBondCriteria(transaction, isABond, isAmortissable, isFinal);

	if ((!isABond || !isAmortissable || isFinal) //if it is not a amortizable bond or if it's a final redemption
		|| (transaction.GetTransactionType() != redemptionBusinessEvent)) //Is Redemeption its business event ? (if not it's a coupon, then we do not need to change its business event)
	{
		END_LOG();
		return eActionToTransfer::attModifyInAutomatic;
	}

	double previousAmount = transaction.GetNetAmount();

	//Set the business event "Partial Redemption" to the bond
	transaction.SetTransactionType((eTransactionType)partialRedemptionBusinessEvent);

	//Set the net amount to a positive value if its sign has been changed (due to the business event change)
	if (double qty = transaction.GetNetAmount() != previousAmount)
		transaction.SetNetAmount(previousAmount);
	
	END_LOG();
	return attModifyInAutomatic;
}

CSRTransferInterface::eActionToTransfer CSxTransferTrade::TransferCorporateAction(const CorporateAction &corporateAction)
{
	return eActionToTransfer::attTransfer;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////// CSxExecute //////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
** Static
*/
/*static*/ const char * CSxExecute::__CLASS__ = "CSxExecute";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CSxExecute)

//-------------------------------------------------------------------------------------------------------
/*virtual*/ eProcessingType	CSxExecute::GetProcessingType() const
{
	return pPostForecast;
}

//-------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxExecute::Run()
{	
	BEGIN_LOG("Run");

	try
	{						
		_STL::auto_ptr<CSRTransferTrade> ptr(CSRTransferTrade::CreateInstance());
		
		if (ptr.get())
			ptr->CSRTransferTrade::LaunchAll();			
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		MESS(Log::error,FROM_STREAM("ERROR ("<<ex<<") while running \"CFG Transfert trades\" dialog"));		
	}
	catch(...)
	{		
		MESS(Log::error,FROM_STREAM("Unhandled exception occured while running \"CFG Transfert trades\" dialog"));
	}

	END_LOG();
}


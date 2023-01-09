/*
** Includes
*/

// specific
#include "CSxGrossConsiderationAction.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc/value/kernel/SphFund.h"
#include "GUI\CSxTransactionDlg.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::tools;
using namespace sophis::value;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(CSxGrossConsiderationAction)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxGrossConsiderationAction::VoteForCreation(CSRTransaction &transaction)
{
 
 const CSRInstrument * ins = transaction.GetInstrument();
  
 if(ins->GetType()=='O')
 {
	 double grossCons =0.0;
	 double grossAmount = transaction.GetGrossAmount();
	 double accrued = transaction.GetAccruedAmount();
	 
	 CSRTransaction::ePaymentCurrencyType ePCT = transaction.GetPaymentCurrencyType();
		if(ePCT == CSRTransaction::ePaymentCurrencyType::pcUnderlying)
		{

			CSRTransaction::eForexCertaintyType	CertaintyType =	transaction.GetForexCertaintyType();
			double TradeForex = transaction.GetForexSpot();
			if(CertaintyType == CSRTransaction::eForexCertaintyType::fcCertain)
				accrued = accrued*TradeForex;
			else
				accrued = accrued/TradeForex;
		}
	 grossCons = grossAmount - accrued;

	 transaction.SaveGeneralElement(CSxTransactionDlg::eGrossConsAmount,&grossCons);

 }


}


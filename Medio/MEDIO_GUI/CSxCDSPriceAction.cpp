/*
** Includes
*/

// specific
#include "CSxCDSPriceAction.h"
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
CONSTRUCTOR_TRANSACTION_ACTION(CSxCDSPriceAction)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxCDSPriceAction::VoteForCreation(CSRTransaction &transaction)
{
 
 const CSRInstrument * ins = transaction.GetInstrument();
  
	if(ins!=nullptr )
	{
		long allotmentID=ins->GetAllotment();
		if(std::find(CSxTransactionDlg::AllotmentsCDSlist.begin(), CSxTransactionDlg::AllotmentsCDSlist.end(), allotmentID) != CSxTransactionDlg::AllotmentsCDSlist.end())		
		{
			 eAskQuotationType priceType = transaction.GetAskQuotationType();
		
			if(priceType== eAskQuotationType::aqInPercentage)
			{
				   double tradeQuantity = transaction.GetQuantity();
				   double revertedQty=-tradeQuantity;

				   transaction.SetQuantityOnly(revertedQty);
			}
		}
	}
 
 

}


 void CSxCDSPriceAction::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction, long event_id)
				throw (sophis::tools::VoteException)
 {

	const CSRInstrument * ins = transaction.GetInstrument();
	 if(ins!=nullptr )
	{
		long allotmentID=ins->GetAllotment();
		if(std::find(CSxTransactionDlg::AllotmentsCDSlist.begin(), CSxTransactionDlg::AllotmentsCDSlist.end(), allotmentID) != CSxTransactionDlg::AllotmentsCDSlist.end())		
		{
			 eAskQuotationType priceType = transaction.GetAskQuotationType();
		
			if(priceType== eAskQuotationType::aqInPercentage)
			{
				   double tradeQuantity = transaction.GetQuantity();
				   double revertedQty=-tradeQuantity;

				   transaction.SetQuantityOnly(revertedQty);
			}
		}
	}
 }

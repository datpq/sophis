#pragma warning(disable:4251)

/*
** Includes
*/

// specific
#include "CheckListedBond.h"
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

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::instrument;
using namespace sophis::tools;

/*
** Static
*/
const char * CheckListedBond::__CLASS__ = "CheckListedBond";
bool CheckListedBond::isForceMode = false;
bool CheckListedBond::isBond = false;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(CheckListedBond)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckListedBond::VoteForCreation(CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForCreation");
	try
	{
		BondCheckListedBond(NULL, &transaction, true);
	}
	catch (const VoteException & ex)
	{
		HandleException(ex, "creation");
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckListedBond::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForModification");
	try
	{
		BondCheckListedBond(&original, &transaction, true);
	}
	catch (const VoteException & ex)
	{
		HandleException(ex, "modification");
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CheckListedBond::VoteForDeletion(const CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForDeletion");
	try
	{
		BondCheckListedBond(NULL, &transaction, true);
	}
	catch (const VoteException & ex)
	{
		HandleException(ex, "deletion");
	}
}

//-------------------------------------------------------------------------------------------------------------------
void CheckListedBond::BondCheckListedBond(const sophis::portfolio::CSRTransaction * original, const sophis::portfolio::CSRTransaction * transaction, bool isDeleted)
{
	BEGIN_LOG("BondCheckListedBond");	
	isBond = false;	
	const CSRInstrument * currentInstrument = CSRInstrument::GetInstance(transaction->GetInstrumentCode());
	if (!currentInstrument)
	{
		MESS(Log::warning, "Empty instrument");
		END_LOG();
		return;
	}
	if ( currentInstrument->GetType() == eInstrumentType::iBond )
	{
		isBond = true;
		long marketCode = currentInstrument->GetMarketCode();
		if ( marketCode != 0 )
		{
			char textMsg[200] = "";
			sprintf_s(textMsg, "You cannot buy or sell listed bonds");
			MESS(Log::verbose, textMsg);
			throw VoteException(textMsg);
		}
	}		
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------------
int CheckListedBond::HandleException(const VoteException & ex, const char* operation) throw (VoteException)
{
	BEGIN_LOG("HandleException");
	CSRUserRights * myUserRights = new CSRUserRights();
	CSRUserRights * myGroupRights = new CSRUserRights(myUserRights->GetParentID());
	myUserRights->LoadDetails();
	myGroupRights->LoadDetails();
	eRightStatusType CFGCheckDeal = myUserRights->GetUserDefRight("CFG Check Listed Bonds");
	eRightStatusType CFGCheckDealGroup = myGroupRights->GetUserDefRight("CFG Check Listed Bonds");
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
			else if (isBond)
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
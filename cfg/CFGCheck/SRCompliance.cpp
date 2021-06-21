#pragma warning(disable:4251)
/*
** Includes
*/

// specific
#include "SRCompliance.h"
#include __STL_INCLUDE_PATH(string)
#include "SphSDBCInc/SphSQLQuery.h"

#include "SphInc/backoffice_kernel/SphBusinessEvent.h"
#include "SphInc/static_data/SphMarket.h"
#include "SphInc/static_data/SphPlace.h"
#include "SphInc/gui/SphDialog.h"
#include "SphInc/instrument/SphHandleError.h"
#include "SphInc/value/kernel/SphFund.h"
#include "SphInc/value/kernel/SphFundMacros.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc\static_data\SphHistoricalData.h"

#include "SphTools/SphLoggerUtil.h"
#include "SphTools/SphDay.h"
#include "SphInc/SphUserRights.h"
#include "SphInc/SphRiskApi.h"

#include "SphLLInc/Sphtools/compatibility/globalsophis.h"


/*
** Namespace
*/
using namespace sophis::value;
using namespace sophis::tools;
using namespace sophis::market_data;
using namespace sophisTools::base;

/*
** Static
*/
const char * SRCompliance::__CLASS__ = "SRCompliance";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void SRCompliance::VoteForCreation(CSAMFundSR &subscription)
throw (VoteException)
{
	BEGIN_LOG("VoteForCreation");	
	double shares = subscription.GetNbShares();
	try 
	{
		if ( shares < 0 )
		{
			double currentShares = GetThirdPartiesShares(	subscription.GetFundCode(),
															subscription.GetThird1(),
															subscription.GetBusinessPartner(),
															gApplicationContext->GetDate());		
			MESS(Log::debug, "Currency shares " << currentShares << ", SR Quantity " << shares );
			if ( currentShares <= 0 || (shares + currentShares) <=0 )
			{
				char textMsg[200] = "";
				sprintf_s(textMsg, "Your redemption amount (for Investor %ld and Business Partner %ld) exceeds your subscriptions amount.",
							subscription.GetThird1(), subscription.GetBusinessPartner());
				MESS(Log::verbose, textMsg);
				throw VoteException(textMsg);
			}		
		}
	}
	catch (const VoteException & ex)
	{
		HandleExceptionCreation(ex);
	}

	CheckLastNav(subscription);

	MESS(Log::debug, "CFG Workaround for rounding problem");
	MESS(Log::debug, "GetSRType=" << subscription.GetSRType() << ", GetAmount=" << subscription.GetAmount() << ", GetNAV=" << subscription.GetNAV() << ", GetFeesInt=" << subscription.GetFeesInt());
	if (subscription.GetSRType() == srtNetAmount && subscription.GetAmount() < 0) {
		double dunits = subscription.GetAmount() * (100 + subscription.GetFeesInt()) / 100 / subscription.GetNAV();
		int units = (int)floor(dunits);
		//double grossAmount = units * subscription.GetNAV() * 100 / (100 - subscription.GetFeesInt());
		double grossAmount = units * subscription.GetNAV();
		double feeAmountInt = abs(grossAmount) * subscription.GetFeesInt() / 100;
		MESS(Log::debug, "dunits = " << dunits << ", units = " << units << ", grossAmount=" << grossAmount << ", feeAmountInt=" << feeAmountInt);
		subscription.SetNbUnits(units);
		subscription.SetNbShares(units);
		subscription.SetGrossAmount(grossAmount);
		subscription.SetFeesAmountInt(feeAmountInt);
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void SRCompliance::VoteForModification(const CSAMFundSR & original, CSAMFundSR &subscription)
throw (VoteException)
{
	BEGIN_LOG("VoteForModification");	
//	if (!IsEqual(original, subscription))
//	{		
		CheckAMFundSRModification(original, subscription);
//	}
	CheckLastNav(subscription);

	MESS(Log::debug, "CFG Workaround for rounding problem");
	MESS(Log::debug, "GetSRType=" << subscription.GetSRType() << ", GetAmount=" << subscription.GetAmount() << ", GetNAV=" << subscription.GetNAV() << ", GetFeesInt=" << subscription.GetFeesInt());
	if (subscription.GetSRType() == srtNetAmount && subscription.GetAmount() < 0) {
		double dunits = subscription.GetAmount() * (100 + subscription.GetFeesInt()) / 100 / subscription.GetNAV();
		int units = (int)floor(dunits);
		//double grossAmount = units * subscription.GetNAV() * 100 / (100 - subscription.GetFeesInt());
		double grossAmount = units * subscription.GetNAV();
		double feeAmountInt = abs(grossAmount) * subscription.GetFeesInt() / 100;
		MESS(Log::debug, "dunits = " << dunits << ", units = " << units << ", grossAmount=" << grossAmount << ", feeAmountInt=" << feeAmountInt);
		subscription.SetNbUnits(units);
		subscription.SetNbShares(units);
		subscription.SetGrossAmount(grossAmount);
		subscription.SetFeesAmountInt(feeAmountInt);
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void SRCompliance::VoteForDeletion(const CSAMFundSR &subscription)
throw (VoteException)
{
	BEGIN_LOG("VoteForModification");	
	CheckAMFundSRDeletion(subscription);
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
double SRCompliance::GetThirdPartiesAmount(const int fundId, const int investorId, const int businessPartnerId, const long paymentDate)
{
	BEGIN_LOG("GetThirdPartiesAmount");

	SSAmount *resultBuffer = NULL;
	int	nbResults = 0;

	CSRStructureDescriptor	desc(1, sizeof(SSAmount));

	ADD(&desc, SSAmount, fAmount, rdfFloat);

	_STL::string sqlQuery = FROM_STREAM( "SELECT Amount FROM FUND_PURCHASE WHERE Fund = " << fundId 
																	 << " AND Third1 = "  << investorId 
																	 << " AND Third2 = " << businessPartnerId 
																	 << " AND Pay_Date <= num_to_date(" << paymentDate << ")"
																	 << " AND Backoffice != 174 ");

	//DPH
	//CSRSqlQuery::QueryWithNResults(sqlQuery.c_str(), &desc, (void **) &resultBuffer, &nbResults);
	CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery.c_str(), &desc, (void **)&resultBuffer, &nbResults);

	double totalAmount = 0.0;
	for (int i = 0; i < nbResults ; i++)
	{
		totalAmount += resultBuffer[i].fAmount;
	}

	if (resultBuffer)
	{
		free((char *)resultBuffer);
		resultBuffer = NULL;
		nbResults = 0;
	}

	MESS(Log::debug, "Total amount " << totalAmount);

	END_LOG();
	return totalAmount;
}

//-------------------------------------------------------------------------------------------------------------
double SRCompliance::GetThirdPartiesShares(const int fundId, const int investorId, const int businessPartnerId, const long paymentDate)
{
	BEGIN_LOG("GetThirdPartiesShares");
	SSShares * resultBuffer = NULL;
	int	nbResults = 0;

	CSRStructureDescriptor	desc(1, sizeof(SSShares));

	ADD(&desc, SSShares, fShares, rdfInteger);

	_STL::string sqlQuery = FROM_STREAM( "SELECT NUMBER_SHARES FROM FUND_PURCHASE WHERE FUND = " << fundId 
																			<< " AND Third1 = "  << investorId 
																			<< " AND Third2 = " << businessPartnerId 
																			<< " AND Nego_Date <= num_to_date(" << paymentDate << ")"
																			<< " AND Backoffice != 174 ");

	//DPH
	//CSRSqlQuery::QueryWithNResults(sqlQuery.c_str(), &desc, (void **) &resultBuffer, &nbResults);
	CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery.c_str(), &desc, (void **)&resultBuffer, &nbResults);

	int totalShares = 0;
	for (int i = 0; i < nbResults ; i++)
	{
		totalShares += resultBuffer[i].fShares;
	}

	if (resultBuffer)
	{
		free((char *)resultBuffer);
		resultBuffer = NULL;
		nbResults = 0;
	}
	MESS(Log::debug, "Sum of share " << totalShares);

	END_LOG();
	return totalShares;
}


//-------------------------------------------------------------------------------------------------------------
bool SRCompliance::IsEqual(const CSAMFundSR &original, const CSAMFundSR &subscription)
{
	return ( original.GetAmount() == subscription.GetAmount()   &&
		     original.GetFundCode() == subscription.GetFundCode() &&
		     original.GetThird1() == subscription.GetThird1() &&
		     original.GetBusinessPartner() == subscription.GetBusinessPartner() &&
		     original.GetPaymentDate() == subscription.GetPaymentDate() );
}

//-------------------------------------------------------------------------------------------------------------
void SRCompliance::CheckAMFundSRModification(const CSAMFundSR & original, CSAMFundSR &subscription)
{
	BEGIN_LOG("CheckAMFundSRModification");	
	double balance = GetThirdPartiesShares(	subscription.GetFundCode(),
											subscription.GetThird1(),
											subscription.GetBusinessPartner(),
											gApplicationContext->GetDate() );
	double shares = subscription.GetNbShares();
	double diffShares = shares - original.GetNbShares();
	MESS(Log::debug, "In Position " << balance << ", New difference " << diffShares);
	try 
	{
		if ( (diffShares + balance) < 0 )
		{
			char textMsg[200] = "";
			sprintf_s(textMsg, "Your redemption amount (for Investor %ld and Business Partner %ld) exceeds your subscriptions amount.",
				subscription.GetThird1(), subscription.GetBusinessPartner());
			MESS(Log::verbose, textMsg);
			throw VoteException(textMsg);
		}
	} 
	catch (const VoteException & ex)
	{
		HandleExceptionModification(ex);
	}
	END_LOG();	
}

//-------------------------------------------------------------------------------------------------------------
void SRCompliance::CheckAMFundSRDeletion(const sophis::value::CSAMFundSR &subscription)
{
	BEGIN_LOG("CheckAMFundSRDeletion");	
	double balance = GetThirdPartiesShares(	subscription.GetFundCode(),
											subscription.GetThird1(),
											subscription.GetBusinessPartner(),
											gApplicationContext->GetDate() );
	double shares =  - 1 * subscription.GetNbShares();
	try {
		if ( (shares + balance) < 0 )
		{
			char textMsg[200] = "";
			sprintf_s(textMsg, "Your redemption amount (for Investor %ld and Business Partner %ld) exceeds your subscriptions amount.",
								subscription.GetThird1(), subscription.GetBusinessPartner());
			MESS(Log::verbose, textMsg);
			throw VoteException(textMsg);
		}
	} catch (const VoteException & ex)
	{
		HandleExceptionDeletion(ex);
	}
	CheckLastNav(subscription);
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
void SRCompliance::CheckLastNav(const sophis::value::CSAMFundSR &subscription)
throw (VoteException)
{
	BEGIN_LOG("CheckLastNav");
	double quantity = subscription.GetNbShares();
//	double absAmount = abs(subscription.GetAmount());
//	MESS(Log::debug, "SR Amount " << absAmount);
//	const CSAMFund * currentFund = dynamic_cast<const CSAMFund *>(CSRInstrument::GetInstance(subscription.GetFundCode()));
//	double nav = 0.0;
//	if (currentFund)
//	{
//		nav = currentFund->GetFixing('LAST', CSRHistoricalData::GetInstance(), currentFund->GetLastEODDate(gApplicationContext->GetDate()), true);
//	}
//	else
//	{
//		nav = CSAMFund::GetLast(subscription.GetFundCode());
//	}
//
//	MESS(Log::debug, "Fund NAV " << nav);
	try 
	{
		MESS(Log::debug, "Quantity " << quantity);
		if ( fabs(quantity) <  1.0 )
		{
			char textMsg[400] = "";
			sprintf_s(textMsg, "Net amount corresponds to a number of shares smaller than one.");
			MESS(Log::verbose, textMsg);
			throw VoteException(textMsg);
		}
	} 
	catch (const VoteException & ex)
	{
		HandleExceptionModification(ex);
	}
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
void SRCompliance::HandleException(const VoteException & ex, _STL::string confirmMsg)
{
	BEGIN_LOG("HandleException");	
	CSRUserRights * myUserRights = new CSRUserRights();
	CSRUserRights * myGroupRights = new CSRUserRights(myUserRights->GetParentID());
	myUserRights->LoadDetails();
	myGroupRights->LoadDetails();
	eRightStatusType CFGCheckDeal = myUserRights->GetUserDefRight("CFG Check Position");
	eRightStatusType CFGCheckDealGroup = myGroupRights->GetUserDefRight("CFG Check Position");
	if ((CFGCheckDeal == eRightStatusType::rsEnable) || (myUserRights->GetIdent() == 1) || ((CFGCheckDeal == eRightStatusType::rsSameAsParent) && (CFGCheckDealGroup == eRightStatusType::rsEnable)))
	{
		_STL::string ConfirmText = "Warning in SRCompliance : " + ex.getError() + confirmMsg;
		int resultDialog = CSRFitDialog::ConfirmDialog(ConfirmText.c_str());
		if ((resultDialog == 1) || (resultDialog == 2))
			throw;
	}
	else if (CSRApi::IsInBatchMode())
	{
		MESS(Log::warning, "Batch Mode - Deactivate MFC Message (" << (const char *)ex << ")");
	}
	else
	{
		throw;
	}
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
void SRCompliance::HandleExceptionCreation(const VoteException & ex)
{
	_STL::string confirmMsg(" Do you want to force the creation of the SR ?");
	HandleException(ex, confirmMsg);
}


//-------------------------------------------------------------------------------------------------------------
void SRCompliance::HandleExceptionModification(const VoteException & ex)
{	
	_STL::string confirmMsg(" Do you want to force the modification of the SR ?");
	HandleException(ex, confirmMsg);
}

//-------------------------------------------------------------------------------------------------------------
void SRCompliance::HandleExceptionDeletion(const VoteException & ex)
{	
	_STL::string confirmMsg(" Do you want to force the deletion of the SR ?");
	HandleException(ex, confirmMsg);
}
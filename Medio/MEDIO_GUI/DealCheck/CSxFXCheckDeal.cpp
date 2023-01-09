#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "CSxFXCheckDeal.h"
#include "SphInc/tools/SphValidation.h"
#include "SphInc/instrument/SphForexFuture.h"
#include "cache/CSRFutureAndPositionSettlementCachingInterface.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphSDBCInc/queries/SphQuery.h"
#include "SphLLInc\cdc_donnee.h"

using namespace sophis::backoffice_kernel;
using namespace sophis::portfolio;
using namespace sophisTools::logger;

/*static*/ const char* CSxFXCheckDeal::__CLASS__ = "CSxFXCheckDeal";

CSxFXCheckDeal::CSxFXCheckDeal()
{
	//m_message = NULL;
}

bool CSxFXCheckDeal::IsActivated()
{
	BEGIN_LOG("CSxFXCheckDeal")
	try
	{
		sophis::sql::CSRQuery oneIntQuery;
		oneIntQuery.SetName("Performing one int query");
		int outVal = -1;

		oneIntQuery << "SELECT "
			<< CSROut("CONFIG_VALUE", outVal)
			<< " from MEDIO_TKT_CONFIG where "
			<< " CONFIG_NAME = 'FX_BOOKING_ISO'";

		if (oneIntQuery.Fetch())
		{
			LOG(Log::debug, FROM_STREAM("Int value fetched: " << outVal));
			END_LOG();
			return outVal == 1;
		}
		else
		{
			LOG(Log::debug, "No value fectched.");
			END_LOG();
			return false;
		}

	}
	catch (...)
	{
		return false;
	}
	return false; 
}
/*virtual*/ void CSxFXCheckDeal::VoteForCreation(CSRTransaction &transaction, long event_id)
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForCreation");
	//if (m_message == NULL)
	//{
	//	m_message = new CSREventVector();
	//}
	//m_message->clear(); //In case it would not have been deleted...

	const CSRInstrument*  pInstrument = transaction.GetInstrument();
	if ( !pInstrument ) throw VoteException("Cannot get CSRIntrument from transaction");
	if (pInstrument->GetType() != 'X' && pInstrument->GetType() != 'E' && pInstrument->GetType() != 'K')
	{
		return;
	}

	double oldNetAmount = transaction.GetNetAmount();
	double oldSpot2 = transaction.GetSpot();
	double oldQuantity = transaction.GetQuantity();
	int refCurFw;
	int refCur;
	if (transaction.GetTransaction()->forexSwap)
	{
		refCurFw = transaction.GetTransaction()->forexSwap->fRefCcyFar;
		refCur = transaction.GetTransaction()->forexSwap->fRefCcy;
	}

	if (!oldQuantity)
		return;

	const CSRNonDeliverableForexForward* pFXn = dynamic_cast<const CSRNonDeliverableForexForward*>(pInstrument);
	//const CSRForexFuture* pFXf = dynamic_cast<const CSRForexFuture*>(pInstrument); Non identifiable out of the transaction Instrument
	const CSRForexSpot* pFX = dynamic_cast<const CSRForexSpot*>(pInstrument);

	if (pFXn)
	{
		// long ccy1 = pFXn->GetExpiryCurrency();
		// long ccy2 = pFXn->GetCurrencyCode();
		long ccy1 = pFXn->GetCurrencyCode();
		long ccy2 = pFXn->GetExpiryCurrency();

		const CSRForexSpot* originalFxSpot = CSRForexSpot::GetCSRForexSpot(ccy1,ccy2);
		if (!originalFxSpot) throw VoteException("Cannot get the CSRForexSpot from currencies");
		if (originalFxSpot->GetMarketWay()==1) return;

		const CSRForexSpot* newFxSpot = CSRForexSpot::GetCSRForexSpot(ccy2,ccy1);
		if (!newFxSpot) throw VoteException("Cannot get the CSRForexSpot from inversed currencies");

		//static long GetInstrumentId(long instrumentId,
		//	long settlement_date,
		//	long discount_family,
		//	sophis::tools::CSREventVector * messages,
		//	sophis::instrument::eForwardContractType contractType = sophis::instrument::efctStandard);
		long sico = sophis::static_data_service::CSRFutureAndPositionSettlementCachingInterface::GetInstrumentId(
			newFxSpot->GetCode(), pFXn->GetSettlementDate(pFXn->GetTransactionDate(transaction.GetSettlementDate())), NULL,NULL, sophis::instrument::eForwardContractType::efctNonDeliverableForward);
		
		transaction.SetInstrumentCode(sico);
	}
	else if (pFX && IsFXForward(pFX, transaction.GetTransactionDate(), transaction.GetSettlementDate()))
	{
		long ccy1 = pFX->GetForex1();
		long ccy2 = pFX->GetForex2();

		const CSRForexSpot* originalFxSpot = CSRForexSpot::GetCSRForexSpot(ccy1, ccy2);
		if (!originalFxSpot) throw VoteException("Cannot get the CSRForexSpot from currencies");
		if (originalFxSpot->GetMarketWay() == 1) return;

		const CSRForexSpot* newFxSpot = CSRForexSpot::GetCSRForexSpot(ccy2, ccy1);
		if (!newFxSpot) throw VoteException("Cannot get the CSRForexSpot from inversed currencies");

		long sico = sophis::static_data_service::CSRFutureAndPositionSettlementCachingInterface::GetInstrumentId(
			newFxSpot->GetCode(), pFX->GetSettlementDate(pFX->GetTransactionDate(transaction.GetSettlementDate())), NULL,NULL, sophis::instrument::eForwardContractType::efctStandard);
		//We also need to create a line in position_settlement and set it in the trade.
		//else we will have an error...
		int posId = sophis::static_data_service::CSRFutureAndPositionSettlementCachingInterface::GetPositionId(sico, transaction.GetPositionID(), NULL);
		transaction.SetPositionID(posId);
		transaction.SetInstrumentCode(sico);
	}
	else if (pFX)
	{
		long ccy1 = pFX->GetForex1();
		long ccy2 = pFX->GetForex2();

		const CSRForexSpot* originalFxSpot = CSRForexSpot::GetCSRForexSpot(ccy1,ccy2);
		if (!originalFxSpot) throw VoteException("Cannot get the CSRForexSpot from currencies");
		if (originalFxSpot->GetMarketWay()==1) return;

		const CSRForexSpot* newFxSpot = CSRForexSpot::GetCSRForexSpot(ccy2,ccy1);
		if (!newFxSpot) throw VoteException("Cannot get the CSRForexSpot from inversed currencies");

		long settlementDate = transaction.GetSettlementDate();

		transaction.SetInstrumentCode(newFxSpot->GetCode()); //should fix a bug when settlement date is below 2 days.

		transaction.SetSettlementDate(settlementDate);
	}
	else
	{
		return;
	}
	if (transaction.GetTransaction()->forexSwap)
	{
		refCurFw == 1 ? transaction.GetTransaction()->forexSwap->fRefCcyFar = 2 : transaction.GetTransaction()->forexSwap->fRefCcyFar = 1;
		refCur == 1 ? transaction.GetTransaction()->forexSwap->fRefCcy = 2 : transaction.GetTransaction()->forexSwap->fRefCcy = 1;
	}
	transaction.SetQuantity(-oldNetAmount);
	transaction.SetNetAmount(-oldQuantity);
	//transaction.SetSpot(-oldQuantity/oldGrossAmount);
	//transaction.SetComment(FROM_STREAM(oldSpot2));
	//transaction.SetAccruedAmount(oldGrossAmount);
	transaction.SetAskQuotationType(eAskQuotationType::aqInPrice);

	END_LOG();
}

void CSxFXCheckDeal::NotifyCreated(const CSRTransaction &transaction, tools::CSREventVector & message, long event_id)
{
	//For data integrity we have to give an EventVector to CSRFutureAndPositionSettlementCachingInterface (else it will commit)
	//and we need to give back the messages to this event vector
	//if (m_message != NULL)
	//{
	//	tools::CSREventVector::iterator ite = m_message->begin();
	//	while (ite != m_message->end())
	//	{
	//		message.push_back(*ite);
	//		*ite = NULL; // it must not be deleted in destructor of m_message
	//		ite++;
	//	}
	//	delete m_message;
	//	m_message = NULL;
	//}
}

/*static*/ bool CSxFXCheckDeal::IsFXForward( const CSRForexSpot* pFX, long transactionDate, long settlementDate )
{
	long daysNum   = pFX->GetNbDayValue();
	long cutOffDate  = transactionDate+daysNum;

	const CSRCurrency *curr1 = CSRCurrency::GetCSRCurrency(pFX->GetForex1());
	const CSRCurrency *curr2 = CSRCurrency::GetCSRCurrency(pFX->GetForex2());

	for (int start = 1; start <= daysNum; start++)
	{
		if ( curr1->IsABankHolidayDay(transactionDate + start) || curr2->IsABankHolidayDay(transactionDate + start) )
			cutOffDate++;
	}

	while ( curr1->IsABankHolidayDay(cutOffDate) || curr2->IsABankHolidayDay(cutOffDate) )
	{
		cutOffDate++;
	}
	return settlementDate > cutOffDate;
}

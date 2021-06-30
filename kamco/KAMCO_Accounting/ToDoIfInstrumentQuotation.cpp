
/*
** Includes
*/
#include "ToDoIfInstrumentQuotation.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphTools/SphLoggerUtil.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;
using namespace eff::kamco::accounting;

const char* ToDoIfTradeInstrumentIsQuoted::__CLASS__ = "ToDoIfTradeInstrumentIsQuoted";
const char* ToDoIfTradeInstrumentIsUnquoted::__CLASS__ = "ToDoIfTradeInstrumentIsUnquoted";
const char* ToDoIfPositionInstrumentIsQuoted::__CLASS__ = "ToDoIfPositionInstrumentIsQuoted";
const char* ToDoIfPositionInstrumentIsUnquoted::__CLASS__ = "ToDoIfPositionInstrumentIsUnquoted";


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool ToDoIfTradeInstrumentIsQuoted::to_do(const CSRTransaction& trade, double amount, long posting_currency,
	long accounting_currency, long amount_currency) const /*= 0 */
{
	BEGIN_LOG("ToDoIfTradeInstrumentIsQuoted");
	long marketCode = 0;

	try
	{
		long instrId = trade.GetInstrumentCode();
		const CSRInstrument* instrument = CSRInstrument::GetInstance(instrId);
		if (instrument != nullptr)
		{
			marketCode = instrument->GetMarketCode();
			MESS(Log::debug, FROM_STREAM("ToDoIfTradeInstrumentIsQuoted trade = " << trade.GetTransactionCode()
				<< ", instrument = " << instrId
				<< ", amount = " << amount
				<< ", posting_currency = " << posting_currency
				<< ", marketCode = " << marketCode));
		}
		else
		{
			MESS(Log::debug, FROM_STREAM(" Instrument not found."));
		}

	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to check if instrument is quoted: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}

	END_LOG();
	return marketCode != 0;
}

/*virtual*/ bool ToDoIfTradeInstrumentIsUnquoted::to_do(const CSRTransaction& trade, double amount, long posting_currency,
	long accounting_currency, long amount_currency) const /*= 0 */
{
	BEGIN_LOG("ToDoIfTradeInstrumentIsUnquoted");

	long marketCode = 0;
	try
	{
		long instrId = trade.GetInstrumentCode();
		const CSRInstrument* instrument = CSRInstrument::GetInstance(instrId);
		if (instrument != nullptr)
		{
			marketCode = instrument->GetMarketCode();

			MESS(Log::debug, FROM_STREAM("ToDoIfTradeInstrumentIsUnquoted trade = " << trade.GetTransactionCode()
				<< ", instrument = " << instrId
				<< ", amount = " << amount
				<< ", posting_currency = " << posting_currency
				<< ", marketCode = " << marketCode));
		}
		else
		{
			MESS(Log::debug, FROM_STREAM(" Instrument not found."));
		}
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to check if instrument is unquoted: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return marketCode == 0;
}

/*virtual*/ bool ToDoIfPositionInstrumentIsQuoted::to_do(const CSRPosition& position, double amount, long posting_currency,
	long accounting_currency, long amount_currency) const /*= 0 */
{
	BEGIN_LOG("ToDoIfPositionInstrumentIsQuoted");
	long marketCode = 0;
	try
	{

		long instrId = position.GetInstrumentCode();
		const CSRInstrument * instrument = CSRInstrument::GetInstance(instrId);
		if (instrument != nullptr)
		{
			marketCode = instrument->GetMarketCode();

			MESS(Log::debug, FROM_STREAM("ToDoIfPositionInstrumentIsQuoted position = " << position.GetIdentifier()
				<< ", instrument = " << instrId
				<< ", amount = " << amount
				<< ", posting_currency = " << posting_currency
				<< ", marketCode = " << marketCode));
		}
		else
		{
			MESS(Log::debug, FROM_STREAM(" Instrument not found."));
		}
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to check if position instrument is quoted: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
	return marketCode != 0;
}

/*virtual*/ bool ToDoIfPositionInstrumentIsUnquoted::to_do(const CSRPosition& position, double amount, long posting_currency,
	long accounting_currency, long amount_currency) const /*= 0 */
{
	BEGIN_LOG("ToDoIfPositionInstrumentIsUnquoted");
	long marketCode = 0;
	try
	{

		long instrId = position.GetInstrumentCode();
		const CSRInstrument * instrument = CSRInstrument::GetInstance(instrId);

		if (instrument != nullptr)
		{
			marketCode = instrument->GetMarketCode();
			MESS(Log::debug, FROM_STREAM("ToDoIfPositionInstrumentIsUnquoted position = " << position.GetIdentifier()
				<< ", instrument = " << position.GetInstrumentCode()
				<< ", amount = " << amount
				<< ", posting_currency = " << posting_currency
				<< ", marketCode = " << marketCode));
		}
		else
		{
			MESS(Log::debug, FROM_STREAM(" Instrument not found."));
		}

	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to check if position instrument is unquoted: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured");
	}

	END_LOG();
	return marketCode == 0;
}

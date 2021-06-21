#pragma warning(disable:4251)
/*
** Includes
*/
// specific
#include "CFG_Forex_Rule.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc/portfolio/SphPosition.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/instrument/SphForexSpot.h"
#include "SphInc\Value\kernel\SphFundPurchase.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/static_data/SphCalendar.h"
#include "SphInc/market_data/SphMarketData.h"


/*
** Namespace
*/
using namespace sophis::backoffice_kernel;
using namespace sophis::accounting;
using namespace sophis::portfolio;
using namespace sophis::value;
using namespace sophis::static_data;
using namespace sophis::market_data;

const char * CFG_Forex_Rule::__CLASS__ = "CFG_Forex_Rule";
const sophis::accounting::CSRForexRule* CFG_Forex_Rule::pRisqueForexRule = NULL;
const sophis::accounting::CSAmAccForexRule* CFG_Forex_Rule::pValueForexRule = NULL;

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_FOREX_RULE(CFG_Forex_Rule);

//----------------------------------------------------------------------------------
double CFG_Forex_Rule::get_amount(double	amount,
								  long	currency_amount,
								  long	new_currency,
								  const	portfolio::CSRTransaction& trade,
								  bool	&notYetFixed,
								  long	date) const
{
	BEGIN_LOG("get_amount1");

	double newAmount = 0;

	char amountCurrencyString[4] = "";
	CSRCurrency::CurrencyToString(currency_amount, amountCurrencyString);
	char newCurrencyString[4] = "";
	CSRCurrency::CurrencyToString(new_currency, newCurrencyString);

	const CSRInstrument * myInstru = CSRInstrument::GetInstance(trade.GetInstrumentCode());
	if (myInstru && myInstru->GetType() == 'E')
	{
		MESS(Log::debug, "Instrument " << trade.GetInstrumentCode() << " is a forex => Use 'Default' rule");

		// Use default rule
		pRisqueForexRule = GetRisqueDefaultForexRule();
		newAmount = pRisqueForexRule->get_amount(amount, currency_amount, new_currency, trade, notYetFixed,date);
		/*delete defaultRule;
		defaultRule = NULL;*/

		END_LOG();
		return newAmount;
	}

	newAmount = ComputeWithLast(amount, currency_amount, new_currency, trade.GetTransactionDate() - 1, notYetFixed);

	MESS(Log::debug, "New amount: " << newAmount);

	END_LOG();
	return newAmount;
}

//----------------------------------------------------------------------------------
double CFG_Forex_Rule::get_amount(	double	amount,
								  long	currency_amount,
								  long	new_currency,
								  const portfolio::CSRPosition& position	) const
{
	BEGIN_LOG("get_amount2");

	double newAmount = 0;

	char amountCurrencyString[4] = "";
	CSRCurrency::CurrencyToString(currency_amount, amountCurrencyString);
	char newCurrencyString[4] = "";
	CSRCurrency::CurrencyToString(new_currency, newCurrencyString);

	bool notYetFixed = true;

	const CSRInstrument * myInstru = CSRInstrument::GetInstance(position.GetInstrumentCode());
	if (myInstru && myInstru->GetType() == 'E')
	{
		MESS(Log::debug, "Instrument " << position.GetInstrumentCode() << " is a forex => Use 'Default' rule");

		// Use default rule
		pRisqueForexRule = GetRisqueDefaultForexRule();
		newAmount = pRisqueForexRule->get_amount(amount, currency_amount, new_currency, position);
		/*delete defaultRule;
		defaultRule = NULL;*/

		END_LOG();
		return newAmount;
	}
	newAmount = ComputeWithLast(amount, currency_amount, new_currency, gApplicationContext->GetDate() - 1, notYetFixed);

	MESS(Log::debug, "New amount: " << newAmount);

	END_LOG();
	return newAmount;
/*
		// Use default rule
		CSRForexRule * defaultRule = CSRForexRule::GetPrototype().CreateInstance("Default");
		if (!defaultRule)
		{
			MESS(Log::error, "Failed to get the default CSRForexRule 'Default'");
			throw GeneralException("Failed to get the default CSRForexRule 'Default'");
		}
		END_LOG();
		return defaultRule->get_amount(amount, currency_amount, new_currency, position);*/
	
}

//----------------------------------------------------------------------------------
double CFG_Forex_Rule::get_amount(	double	amount,
								  long	currency_amount,
								  long	new_currency,
								  const	CSAMFundSR & theSR,
								  bool	&notYetFixed,
								  long	date) const
{
	BEGIN_LOG("get_amount3");
	MESS(Log::debug, "SR case");

	// Use default rule
	pValueForexRule = GetValueDefaultForexRule();

	double returnedValue = pValueForexRule->get_amount(amount, currency_amount, new_currency, theSR, notYetFixed, date);

	//delete defaultRule;
	//defaultRule = NULL;

	END_LOG();
	return returnedValue;
}


//----------------------------------------------------------------------------------
double CFG_Forex_Rule::get_amount(	double	amount,
								  long	currency_amount,
								  long	new_currency,
								  const	sophis::value::SSAMAccountInterest & interest,
								  bool	&notYetFixed,
								  long	date) const
{
	BEGIN_LOG("get_amount4");
	MESS(Log::debug, "Account Interest case");
	// Use default rule
	pValueForexRule = GetValueDefaultForexRule();

	double returnedValue = pValueForexRule->get_amount(amount, currency_amount, new_currency, interest, notYetFixed, date);

	/*delete defaultRule;
	defaultRule = NULL;*/

	END_LOG();
	return returnedValue;
}

//----------------------------------------------------------------------------------
double CFG_Forex_Rule::get_amount(	double	amount,
								  long	currency_amount,
								  long	new_currency,
								  const	sophis::value::SSAMCashTransferInfos & cashTransfer,
								  bool	&notYetFixed,
								  long	date) const
{
	BEGIN_LOG("get_amount5");

	MESS(Log::debug, "Cash Transfert case");

	// Use default rule
	pValueForexRule = GetValueDefaultForexRule();

	double returnedValue = pValueForexRule->get_amount(amount, currency_amount, new_currency, cashTransfer, notYetFixed, date);

	/*delete defaultRule;
	defaultRule = NULL;*/

	END_LOG();
	return returnedValue;
}

//----------------------------------------------------------------------------------
const CSRForexRule * CFG_Forex_Rule::GetRisqueDefaultForexRule() const
{
	BEGIN_LOG("GetRisqueDefaultForexRule");
	if(!pRisqueForexRule)
	{
		pRisqueForexRule = CSRForexRule::GetPrototype().CreateInstance("Default");
		if (!pRisqueForexRule)
		{
			MESS(Log::error, "Failed to get the default CSRForexRule 'Default'");
			throw GeneralException("Failed to get the default CSRForexRule 'Default'");
		}
	}

	END_LOG();
	return pRisqueForexRule;
}

//----------------------------------------------------------------------------------
const CSAmAccForexRule * CFG_Forex_Rule::GetValueDefaultForexRule() const
{
	BEGIN_LOG("GetValueDefaultForexRule");
	
	if(!pRisqueForexRule)
	{
		pRisqueForexRule = CSRForexRule::GetPrototype().CreateInstance("Default");
		if (!pRisqueForexRule)
		{
			MESS(Log::error, "Failed to get the default CSRForexRule 'Default'");
			throw GeneralException("Failed to get the default CSRForexRule 'Default'");
		}
	}

	if (!pValueForexRule)
	{
		pValueForexRule = dynamic_cast<const CSAmAccForexRule *>(pRisqueForexRule);
		if (!pValueForexRule)
		{
			MESS(Log::error, "Failed to get the default CSAmAccForexRule 'Default'");
			//delete pValueForexRule;
			//defaultRule = NULL;
			throw GeneralException("Failed to get the default CSAmAccForexRule 'Default'");
		}
	}

	END_LOG();
	return pValueForexRule;
}

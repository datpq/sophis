
/*
** Includes
*/
// specific
#include "CSxUCITSCommitment.h"
#include "SphTools/SphCommon.h"
#include "SphInc\backoffice_kernel\SphAllotment.h"
#include "../MediolanumConstants.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc\finance\SphPricer.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/instrument/SphOption.h"
#include "SphInc/instrument/SphForexFuture.h"
#include "SphInc/value/kernel/SphFundPortfolio.h"
#include "SphInc/instrument/SphSwap.h"
#include "SphInc/instrument/SphIndexLeg.h"
#include "SphInc/finance/SphProductType.h"
// #include "../Tools/CSxSQLHelper.h"

// #include "../cc_data/CSAm_UCITS_Data.h"
// #include <boost/algorithm/string/split.hpp>
// #include <boost/algorithm/string/classification.hpp>

using namespace _STL;
using namespace sophis::portfolio;
using namespace sophis::instrument;
using namespace sophis::finance;
using namespace sophis::backoffice_kernel;
using namespace sophisTools::logger;
using namespace sophis::value;

char const __ALLOTMENT_CERTIFICATE__[] = ALLOTMENT_CERTIFICATE;
char const __ALLOTMENT_CDS__[] = ALLOTMENT_CDS;
char const __ALLOTMENT_CDX__[] = ALLOTMENT_CDX;
char const __ALLOTMENT_COCO__[] = ALLOTMENT_COCO;
char const __ALLOTMENT_CB__[] = ALLOTMENT_CB;
char const __ALLOTMENT_IR_DERIVATIVE__[] = ALLOTMENT_IR_DERIVATIVE;
char const __ALLOTMENT_NDF__[] = ALLOTMENT_NDF;
char const __ALLOTMENT_OTC_FX_OPTION_SINGLE__[] = ALLOTMENT_OTC_FX_OPTION_SINGLE;
char const __ALLOTMENT_TRS_EQUITY_SINGLE__[] = ALLOTMENT_TRS_EQUITY_SINGLE;
char const __ALLOTMENT_TRS_EQUITY_BASKET__[] = ALLOTMENT_TRS_EQUITY_BASKET;
char const __ALLOTMENT_FX_FORWARD__[] = ALLOTMENT_FX_FORWARD;
char const __ALLOTMENT_LISTED_OPTION__[] = ALLOTMENT_LISTED_OPTION;

#pragma region CERTIFICATE

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_CERTIFICATE__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_CERTIFICATE__>";
double CSxUCIT_Calculator<__ALLOTMENT_CERTIFICATE__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	if (!position || !instr)
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		return abs(notional * numberOfSecurities);
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}

#pragma endregion CERTIFICATE
#pragma region CDS
/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_CDS__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_CDS__>";
double CSxUCIT_Calculator<__ALLOTMENT_CDS__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	if (!position || !instr)
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		return abs(notional * numberOfSecurities);
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}
#pragma endregion CDS
#pragma region CDX

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_CDX__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_CDX__>";
double CSxUCIT_Calculator<__ALLOTMENT_CDX__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	if (!position || !instr)
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		return abs(notional * numberOfSecurities);
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}

#pragma endregion CDX
#pragma region COCO

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_COCO__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_COCO__>";
double CSxUCIT_Calculator<__ALLOTMENT_COCO__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	if (!position || !instr)
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		return abs(notional * numberOfSecurities);
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}

#pragma endregion COCO
#pragma region CB

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_CB__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_CB__>";
double CSxUCIT_Calculator<__ALLOTMENT_CB__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	if (!position || !instr)
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		return abs(notional * numberOfSecurities);
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}

#pragma endregion CB
#pragma region IR_DERIVATIVE

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_IR_DERIVATIVE__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_IR_DERIVATIVE__>";
double CSxUCIT_Calculator<__ALLOTMENT_IR_DERIVATIVE__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	if (!position || !instr)
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		return abs(notional * numberOfSecurities);
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}

#pragma endregion IR_DERIVATIVE
#pragma region NDF

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_NDF__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_NDF__>";
double CSxUCIT_Calculator<__ALLOTMENT_NDF__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	const CSRNonDeliverableForexForward* fwd = dynamic_cast<const CSRNonDeliverableForexForward*>(instr);
	if (!position || !(position->GetAveragePrice()) || !fwd)
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position->GetPortfolioCode(), position->GetExtraction());
		// const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position->GetPortfolioCode());
		const CSAMPortfolio* fundFolio = NULL;

		long folioCcy = '\3EUR';
		if (folio && (fundFolio = folio->GetFundRootPortfolio()))
			folioCcy = fundFolio->GetCurrency();

		long buyCcy = fwd->GetCurrency();
		long sellCcy = fwd->GetExpiryCurrency();
		int marketWay = 0;
		const CSRForexSpot* fxSpot = CSRForexSpot::GetCSRForexSpot(buyCcy, sellCcy);	// buyCcy / sellCcy
		if (fxSpot)
			marketWay = fxSpot->GetMarketWay();

		double buyNotional = position->GetInstrumentCount();
		double averagePrice = position->GetAveragePrice();
		double realised = position->GetRealised();
		double sellNotional = realised - (marketWay == 1 ? (buyNotional * averagePrice) : (buyNotional / averagePrice));

		if (sellCcy == folioCcy)
			return abs(buyNotional);		// already in instrument currency. No conversion necessary

		double fxSellToBuy = fxSpot->GetLast();

		if (buyCcy == folioCcy)
			return abs(sellNotional / fxSellToBuy);	// convert to instrument currency

		double fxSellToFolio = 1.0;
		double fxBuyToFolio = 1.0;

		const CSRForexSpot* pFxSellToFolio = CSRForexSpot::GetCSRForexSpot(folioCcy, sellCcy);
		const CSRForexSpot* pFxBuyToFolio = CSRForexSpot::GetCSRForexSpot(folioCcy, buyCcy);

		if (pFxSellToFolio && pFxBuyToFolio)
		{
			fxSellToFolio = pFxSellToFolio->GetLast();
			fxBuyToFolio = pFxBuyToFolio->GetLast();
		}

		double sellNotionalCcyFolio = sellNotional / fxSellToFolio;
		double buyNotionalCcyFolio = buyNotional / fxBuyToFolio;
		return abs((sellNotionalCcyFolio + buyNotionalCcyFolio) * fxBuyToFolio);	// convert to instrument ccy
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}

#pragma endregion NDF
#pragma region OTC_FX_OPTION_SINGLE

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_OTC_FX_OPTION_SINGLE__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_OTC_FX_OPTION_SINGLE__>";
double CSxUCIT_Calculator<__ALLOTMENT_OTC_FX_OPTION_SINGLE__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	const CSROption* option = NULL;
	const CSRInstrument* underlying = NULL;
	const CSRForexSpot* fxSpot = NULL;


	if (!position || !instr || !(option = dynamic_cast<const CSROption*>(instr)) || !(option->GetStrikeInProduct())
		|| !(underlying = option->GetUnderlyingInstrument()) || !(fxSpot = dynamic_cast<const CSRForexSpot*>(underlying)))
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position->GetPortfolioCode(), position->GetExtraction());
	// const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position->GetPortfolioCode());
	const CSAMPortfolio* fundFolio = NULL;

	if (!folio || !(fundFolio = folio->GetFundRootPortfolio()))
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		long code = option->GetCode();
		const CSRComputationResults* result = CSRInstrument::GetComputationResults(code);
		double delta = option->GetDelta(*result, &code);
		double contractSize = option->GetQuotity();
		delta *= contractSize;			// delta seems to be x100
		// double delta = option->GetNthPercentDelta(*result, code);
		long folioCcy = fundFolio->GetCurrency();
		long buyCcy = fxSpot->GetForex1();
		long sellCcy = fxSpot->GetForex2();
		int marketWay = fxSpot->GetMarketWay();
		double buyNotional = position->GetInstrumentCount();
		double strike = option->GetStrikeInProduct();
		double realised = position->GetRealised();
		double sellNotional = realised - (marketWay == 1 ? (buyNotional * strike) : (buyNotional / strike));
		double buyFxNotional = buyNotional * delta;
		double sellFxNotional = sellNotional * delta;

		if (sellCcy == folioCcy)
			return abs(buyFxNotional);			// already in instrument currency

		double fxSellToBuy = CSRForexSpot::GetCSRForexSpot(buyCcy, sellCcy)->GetLast();

		if (buyCcy == folioCcy)
			return abs(sellFxNotional / fxSellToBuy);	// convert to instrument currency

		double fxSellToFolio = 1.0;
		double fxBuyToFolio = 1.0;

		const CSRForexSpot* pFxSellToFolio = CSRForexSpot::GetCSRForexSpot(folioCcy, sellCcy);
		const CSRForexSpot* pFxBuyToFolio = CSRForexSpot::GetCSRForexSpot(folioCcy, buyCcy);

		if (pFxSellToFolio && pFxBuyToFolio)
		{
			fxSellToFolio = pFxSellToFolio->GetLast();
			fxBuyToFolio = pFxBuyToFolio->GetLast();
		}

		double sellFxNotionalCcyFolio = sellFxNotional / fxSellToFolio;
		double buyFxNotionalCcyFolio = buyFxNotional / fxBuyToFolio;

		return abs((sellFxNotionalCcyFolio + buyFxNotionalCcyFolio) * fxBuyToFolio);	// convert to instrument currency
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}

#pragma endregion OTC_FX_OPTION_SINGLE
#pragma region TRS_EQUITY_SINGLE

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_TRS_EQUITY_SINGLE__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_TRS_EQUITY_SINGLE__>";
double CSxUCIT_Calculator<__ALLOTMENT_TRS_EQUITY_SINGLE__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	const CSRSwap* swap = NULL;
	if (!position || !instr || !(swap = dynamic_cast<const CSRSwap*>(instr)))
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		const CSRIndexedLeg* leg = NULL;
		if ((leg = dynamic_cast<const CSRIndexedLeg*>(swap->GetLeg(0))) ||
			(leg = dynamic_cast<const CSRIndexedLeg*>(swap->GetLeg(1))))
		{
			double spot = leg->GetSpotPrice();
			return abs(numberOfSecurities*spot);
		}
		return 0.0;
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}
#pragma endregion TRS_EQUITY_SINGLE
#pragma region TRS_EQUITY_BASKET

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_TRS_EQUITY_BASKET__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_TRS_EQUITY_BASKET__>";
double CSxUCIT_Calculator<__ALLOTMENT_TRS_EQUITY_BASKET__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	const CSRSwap* swap = NULL;
	if (!position || !instr || !(swap = dynamic_cast<const CSRSwap*>(instr)))
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		return abs(notional*numberOfSecurities);
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}
#pragma endregion TRS_EQUITY_BASKET
#pragma region TRS_FIXED_INCOME_SINGLE

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_TRS_FIXED_INCOME_SINGLE__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_TRS_FIXED_INCOME_SINGLE__>";
double CSxUCIT_Calculator<__ALLOTMENT_TRS_FIXED_INCOME_SINGLE__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	const CSRSwap* swap = NULL;
	if (!position || !instr || !(swap = dynamic_cast<const CSRSwap*>(instr)))
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		double notional = instr->GetNotional();
		double numberOfSecurities = position->GetInstrumentCount();
		const CSRIndexedLeg* leg = NULL;
		if ((leg = dynamic_cast<const CSRIndexedLeg*>(swap->GetLeg(0))) ||
			(leg = dynamic_cast<const CSRIndexedLeg*>(swap->GetLeg(1))))
		{
			double spot = leg->GetSpotPrice();
			return abs(notional*numberOfSecurities*spot*0.01);
		}
		return 0.0;

		return abs(notional*numberOfSecurities);
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}
#pragma endregion TRS_FIXED_INCOME_SINGLE

#pragma region FX_FORWARD

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_FX_FORWARD__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_FX_FORWARD__>";
double CSxUCIT_Calculator<__ALLOTMENT_FX_FORWARD__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	const CSRForexFuture* fwd = dynamic_cast<const CSRForexFuture*>(instr);
	if (!position || !position->GetAveragePrice() || !fwd)
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position->GetPortfolioCode(), position->GetExtraction());
		// const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position->GetPortfolioCode());
		const CSAMPortfolio* fundFolio = NULL;

		long folioCcy = '\3EUR';
		if (folio && (fundFolio = folio->GetFundRootPortfolio()))
			folioCcy = fundFolio->GetCurrency();

		long buyCcy = fwd->GetCurrency();
		long sellCcy = fwd->GetExpiryCurrency();
		int marketWay = 0;
		const CSRForexSpot* fxSpot = CSRForexSpot::GetCSRForexSpot(buyCcy, sellCcy);
		if (fxSpot)
			marketWay = fxSpot->GetMarketWay();	// market way is eg USD/SGD - so assumes that "major" currency is received. 1 if MW, -1 if not

		double buyNotional = position->GetInstrumentCount();
		double averagePrice = position->GetAveragePrice();
		double realised = position->GetRealised();
		double sellNotional = realised - (marketWay == 1 ? buyNotional * averagePrice : buyNotional / averagePrice);

		if (sellCcy == folioCcy)
			return abs(buyNotional);

		double fxSellToBuy = fxSpot->GetLast();

		if (buyCcy == folioCcy) 
			return abs(sellNotional / fxSellToBuy);	// convert to instrument currency

		double fxSellToFolio = 1.0;
		double fxBuyToFolio = 1.0;

		const CSRForexSpot* pFxSellToFolio = CSRForexSpot::GetCSRForexSpot(folioCcy, sellCcy);
		const CSRForexSpot* pFxBuyToFolio = CSRForexSpot::GetCSRForexSpot(folioCcy, buyCcy);

		if (pFxSellToFolio && pFxBuyToFolio)
		{
			fxSellToFolio = pFxSellToFolio->GetLast();
			fxBuyToFolio = pFxBuyToFolio->GetLast();
		}

		double sellNotionalCcyFolio = sellNotional / fxSellToFolio;
		double buyNotionalCcyFolio = buyNotional / fxBuyToFolio;
		return abs((sellNotionalCcyFolio + buyNotionalCcyFolio) * fxBuyToFolio);	// convert to instrument ccy
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}
#pragma endregion FX_FORWARD

#pragma region LISTED_OPTION

/*static*/ const char* CSxUCIT_Calculator<__ALLOTMENT_LISTED_OPTION__>::__CLASS__ = "CSxUCIT_Calculator<__ALLOTMENT_LISTED_OPTION__>";
double CSxUCIT_Calculator<__ALLOTMENT_LISTED_OPTION__>::calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* position)
{
	BEGIN_LOG("calculate");

	const CSROption* option = NULL;
	const CSRInstrument* underlying = NULL;
	const CSRForexSpot* fxSpot = NULL;


	if (!position || !position->GetAveragePrice() || !instr || !(option = dynamic_cast<const CSROption*>(instr)) || !(option->GetStrikeInProduct())
		|| !(underlying = option->GetUnderlyingInstrument()) || !(fxSpot = dynamic_cast<const CSRForexSpot*>(underlying)))
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position->GetPortfolioCode(), position->GetExtraction());
	const CSAMPortfolio* fundFolio = NULL;

	if (!folio || !(fundFolio = folio->GetFundRootPortfolio()))
	{
		MESS(Log::warning, "Unable to calculate UCITS Commitment");
		return 0.0;
	}

	try
	{
		long code = option->GetCode();
		const CSRComputationResults* result = CSRInstrument::GetComputationResults(code);
		double delta = option->GetDelta(*result, &code);
		double contractSize = option->GetQuotity();
		delta *= contractSize;
		// double delta = option->GetNthPercentDelta(*result, code);
		long folioCcy = fundFolio->GetCurrency();
		long buyCcy = fxSpot->GetForex1();
		long sellCcy = fxSpot->GetForex2();
		int marketWay = fxSpot->GetMarketWay();
		double buyNotional = position->GetInstrumentCount();
		// double strike = fxSpot->GetLast();
		double realised = position->GetRealised();
		double strike = option->GetStrikeInProduct();
		double sellNotional = realised - (marketWay == 1 ? buyNotional * strike : buyNotional / strike);
		double buyFxNotional = buyNotional * delta;
		double sellFxNotional = sellNotional * delta;

		if (sellCcy == folioCcy)
			return abs(buyFxNotional);

		double fxSellToBuy = fxSpot->GetLast();

		if (buyCcy == folioCcy)
			return abs(sellFxNotional / fxSellToBuy);		// convert to instrument ccy

		double fxSellToFolio = 1.0;
		double fxBuyToFolio = 1.0;

		const CSRForexSpot* pFxSellToFolio = CSRForexSpot::GetCSRForexSpot(folioCcy, sellCcy);
		const CSRForexSpot* pFxBuyToFolio = CSRForexSpot::GetCSRForexSpot(folioCcy, buyCcy);

		if (pFxSellToFolio && pFxBuyToFolio)
		{
			fxSellToFolio = pFxSellToFolio->GetLast();
			fxBuyToFolio = pFxBuyToFolio->GetLast();
		}

		double sellFxNotionalCcyFolio = sellFxNotional / fxSellToFolio;
		double buyFxNotionalCcyFolio = buyFxNotional / fxBuyToFolio;

		long instCcy = option->GetCurrency();
		const CSRForexSpot* pFolioToInst = NULL;
		double folioToInst = 1.0;
		if (pFolioToInst = CSRForexSpot::GetCSRForexSpot(instCcy, folioCcy))
			folioToInst = pFolioToInst->GetLast();
		return abs((sellFxNotionalCcyFolio + buyFxNotionalCcyFolio) / folioToInst);	// convert to instrument ccy
	}
	catch (const sophisTools::base::ExceptionBase& ex)
	{
		MESS(Log::warning, "Error in UCITS column [" << ex << "]");
	}

	END_LOG();
}

#pragma endregion LISTED_OPTION


#pragma region filter
CSxUCIT_Filter::CSxUCIT_Filter(char intrType, const char* allotment, const char* productType) : 
	allotment_(LoadAllotmentIdFromDB(allotment)), productType_(productType), instrumentType_(intrType), UCITS_Filter(intrType, productType){}

bool CSxUCIT_Filter::match(const sophis::instrument::CSRInstrument *instr)
{
	static const char* __CLASS__ = "CSxUCIT_Filter";
	BEGIN_LOG("match");

	LOG(Log::debug, FROM_STREAM("Start matching with instr=" << instr));
	LOG(Log::debug, FROM_STREAM("Instrument type is " << instrumentType_));
	LOG(Log::debug, FROM_STREAM("Product type is " << productType_));
	// bool isProductType = productType_ == "" ? true : UCITS_Filter::match(instr);
	bool isProductType = false;
	if (productType_ == "")
		isProductType = true;
	else
	{
		LOG(Log::debug, FROM_STREAM("calling UCITS_Filter::match(instr)"));
		try
		{
			productType_ = UCITS_Filter::match(instr);
		}
		catch (const sophisTools::base::ExceptionBase& ex)
		{
			MESS(Log::warning, "Error in UCITS column [" << ex << "]");
		}
		LOG(Log::debug, FROM_STREAM("calling UCITS_Filter::match(instr) returned " << (isProductType ? "true" : "false")));
	}

	bool isAllotment = (instr && instr->GetAllotment() == allotment_);
	LOG(Log::debug, FROM_STREAM("match returns " << (isProductType && isAllotment ? "true" : "false")));

	END_LOG();
	return isProductType && isAllotment;
}

#pragma endregion filter
#pragma region static

/*static*/long LoadAllotmentIdFromDB(std::string allotmentName)
{
	long id = 0;
	CSRQuery getIdent;
	getIdent << " SELECT " << CSROut("ident", id)
		<< " from AFFECTATION where libelle like " << CSRIn(allotmentName);
	getIdent.Fetch();
	return id;
}

#pragma endregion static


/*

CSxUCIT_FX_FUTURE_Filter::CSxUCIT_FX_FUTURE_Filter(char intrType, const char* product) : UCITS_Filter(intrType, product)
{
	//std::string sql = "select Allotments from MEDIO_TKT_ALLOTMENT_UCITS where InstrumentType = 'CDX'";
	//std::string ref = ALLOTMENT_CCY_FUTURE;
	//LoadAllotmentList(ref, _Allotments);
	LoadAllotmentIdFromDB(ref)
}

void LoadAllotmentList(std::string ref, std::vector<long> &AllotmentVec)
{
	//std::string allotmentStr = CSxSQLHelper::QueryReturning1String(sql);
	char allotmentStr[101] = { '\0' };
	CSRQuery query;
	query.SetName("GetAllotmentList");
	query << "SELECT " << CSROut("Allotments", allotmentStr, 100)
		<< " from MEDIO_TKT_ALLOTMENT_UCITS where InstrumentType = " << CSRIn(ref);
	query.Fetch();

	std::string delimiter = ";";
	vector<string> SplitVec;
	boost::split(SplitVec, allotmentStr, boost::is_any_of(delimiter));
	for (vector<string>::iterator it = SplitVec.begin(); it != SplitVec.end(); ++it)
	{
		string likeName = FROM_STREAM("%" << *it << "%");
		long allotment = LoadAllotmentIdFromDB(likeName);
		AllotmentVec.push_back(allotment);
	}
}

bool CSxUCIT_FX_FUTURE_Filter::match(const sophis::instrument::CSRInstrument *instr)
{
	static const char* __CLASS__ = "CSxUCIT_FX_FUTURE_Filter";
	BEGIN_LOG("match");

	LOG(Log::debug,FROM_STREAM("Start matching..."));
	bool res = false;
	if(!instr) return res;
	LOG(Log::debug,FROM_STREAM("instr type: "<<instr->GetType()));

	bool found = (std::find(_Allotments.begin(), _Allotments.end(), instr->GetAllotment()) != _Allotments.end());
	if (found)
	{
		LOG(Log::debug, FROM_STREAM("instr #" << instr->GetCode() << " is matched. Allotment = " << instr->GetAllotment()));
		res = true;
	}
	else
	{
		LOG(Log::debug, FROM_STREAM("instr #" << instr->GetCode() << " is not matched. Allotment = " << instr->GetAllotment()));
	}
	END_LOG();
	return res;
}

void LoadAllotmentList(std::string ref, std::vector<long> &AllotmentVec)
{
	AllotmentVec.push_back(LoadAllotmentIdFromDB(ref));
}

*/

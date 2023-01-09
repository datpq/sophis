
/*
** Includes
*/
// specific
#include "SphInc/finance/SphNotionalFuture.h"
#include "SphInc/instrument/SphFuture.h"
#include "CSxGrossLeverageColumn.h"
#include "SphInc/value/kernel/SphFundPortfolio.h"
#include "SphInc/instrument/SphSwap.h"
#include "SphInc/instrument/SphIndexLeg.h"
#include "SphInc/instrument/SphForexFuture.h"
#include "../MediolanumConstants.h"
#include "SphTools/SphLoggerUtil.h"


using namespace sophis::portfolio;
using namespace sophis::finance;
using namespace std;
using namespace sophis::value;
using namespace sophisTools::logger;

map<long, string> CSxGrossLeverageColumn::AllotmentMap;

const char* CSxGrossLeverageColumn::__CLASS__ = "CSxGrossLeverageColumn";


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_PORTFOLIO_COLUMN_GROUP(CSxGrossLeverageColumn, MEDIO_COLUMNGROP_TKT)

//-------------------------------------------------------------------------------------------------------------



/*virtual*/	void			CSxGrossLeverageColumn::ComputePortfolioCell(long				activePortfolioCode,
	long				portfolioCode,
	PSRExtraction		extraction,
	SSCellValue			*cellValue,
	SSCellStyle			*cellStyle,
	bool				onlyTheValue) const
{

	ConsolidateUnder(true, activePortfolioCode, portfolioCode, extraction, cellValue, cellStyle, onlyTheValue);
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxGrossLeverageColumn::ComputePositionCell(const CSRPosition&	position,
	long				activePortfolioCode,
	long				portfolioCode,
	PSRExtraction		extraction,
	long				underlyingCode,
	long				instrumentCode,
	SSCellValue			*cellValue,
	SSCellStyle			*cellStyle,
	bool				onlyTheValue) const
{
	BEGIN_LOG("ComputePositionCell");

	const CSRInstrument* instr = gApplicationContext->GetCSRInstrument(instrumentCode);

	if (instr)
	{
		cellStyle->decimal = 0;
		double result = 0.0;
		SSCellValue value;
		SSCellStyle style;

		long allotId = instr->GetAllotment();
		enum Allotments
		{
			CCY_FUTURE = 1120,
			CDS = 28,
			CDX = 1480,
			CERTIFICATE = 1565,
			COCO = 1660,
			CONVERTIBLE_BOND = 1204,
			EQUITY_FUTURE = 1080,
			FX_FORWARD = 1160,
			INFLATION_SWAP = 1566,
			INT_RATE_FUTURE = 1203,
			IR_DERIVATIVE = 8,
			IRS = 1181,
			LISTED_OPTION = 16,
			NDF = 1400,
			OTC_FX_OPTION_SINGLE = 1185,
			TRS_EQUITY_BASKET = 1800,
			TRS_EQUITY_SINGLE = 1769,
			TRS_FIXED_INCOME_SINGLE = 1770,
			WARRANT = 1564
		};

		switch (allotId)
		{

		case CDS:
		case CDX:
		case CERTIFICATE:
		case COCO:
		case CONVERTIBLE_BOND:
		case INFLATION_SWAP:
		case IR_DERIVATIVE:
		case IRS:
		case TRS_EQUITY_BASKET:
		{
			try
			{
				double notional = instr->GetNotional();
				double numberOfSecurities = position.GetInstrumentCount();
				result = abs(notional * numberOfSecurities);
			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;

		case CCY_FUTURE:
		{
			long fundCcy = 0;
			long underCCY = 0;
			long futureCCY = 0;

			string columnSecurities = "Number of securities";
			const CSRPortfolioColumn* column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnSecurities.c_str());
			if (column)
			{
				column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
			}

			double securitiesValue = value.floatValue;

			string columnContract = "Contract size";
			column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnContract.c_str());
			if (column)
			{
				column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
			}
			double contractValue = value.floatValue;

			const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position.GetPortfolioCode(), position.GetExtraction());
			if (folio != nullptr)
			{
				const CSAMPortfolio* fundFolio = folio->GetFundRootPortfolio();
				if (fundFolio != nullptr)
				{
					fundCcy = fundFolio->GetCurrency();
				}
			}

			const CSRFuture* pFuture = dynamic_cast<const CSRFuture*>(instr);
			if (pFuture != nullptr)
			{
				futureCCY = pFuture->GetCurrencyCode();
				if (pFuture->GetUnderlyingNature() == eUnderlyingNatureType::unCurrency)
				{
					underCCY = pFuture->GetUnderlyingCode();
				}
			}

			if (underCCY != 0 && futureCCY != 0 && fundCcy != 0 && futureCCY == fundCcy == underCCY)
			{
				result = 0;
			}
			else if (underCCY != 0 && futureCCY != 0 && fundCcy != 0 && futureCCY != fundCcy && underCCY != fundCcy)
			{
				//Gross Leverage = ABS(Number of securities * Contract size * FOREX(underlying ccy, fund ccy) + Number of securities * Contract size * FOREX(underlying ccy, future ccy) * FOREX(future ccy, fund ccy))
				double fx1 = CSRMarketData::GetCurrentMarketData()->GetForex(underCCY, fundCcy);
				double fx2 = CSRMarketData::GetCurrentMarketData()->GetForex(underCCY, futureCCY);
				double fx3 = CSRMarketData::GetCurrentMarketData()->GetForex(futureCCY, fundCcy);
				result = abs(securitiesValue * contractValue * fx1 + securitiesValue * contractValue * fx2 * fx3);
			}
			else if (underCCY != 0 && fundCcy != 0 && underCCY != fundCcy)
			{
				double fx = CSRMarketData::GetCurrentMarketData()->GetForex(underCCY, fundCcy);
				result = abs(securitiesValue * contractValue * fx);
				//Gross Leverage = ABS(Number of securities * Contract size *FOREX(underlying ccy, fund ccy))
			}
			else
			{
				//Gross Leverage = ABS(Number of securities * Contract size)
				result = abs(securitiesValue * contractValue);
			}
			END_LOG();
		}break;
		case EQUITY_FUTURE:
		{
			try
			{
				//Gross Leverage = ABS(Number of securities * Contract size*Underlying Price)					
				string columnSecurities = "Number of securities";
				const CSRPortfolioColumn* column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnSecurities.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}

				double securitiesValue = value.floatValue;

				string columnContract = "Contract size";
				column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnContract.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}
				double contractValue = value.floatValue;

				string columnUnderlyingPrice = "Underlying Price";
				column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnUnderlyingPrice.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}
				double underPriceValue = value.floatValue;

				result = abs(securitiesValue * contractValue*underPriceValue);


			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;
		case WARRANT:
		{
			try
			{
				string columnUCITS = "UCITS Commitment";
				const CSRPortfolioColumn*column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnUCITS.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}
				double ucitsValue = value.floatValue;
				if (ucitsValue == 0)
				{
					result = 0;
					break;
				}

				string columnContract = "Nominal";
				column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnContract.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}
				double nominalValue = value.floatValue;


				string columnUnderlyingPrice = "Underlying Price";
				column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnUnderlyingPrice.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}
				double underPriceValue = value.floatValue;
				//Gross Leverage = ABS(Nominal*Underlying Price)
				result = abs(nominalValue*underPriceValue);
			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;
		case TRS_FIXED_INCOME_SINGLE:
		{
			try
			{
				const CSRSwap* swap = dynamic_cast<const CSRSwap*>(instr);
				if (swap != nullptr)
				{
					double notional = instr->GetNotional();
					double numberOfSecurities = position.GetInstrumentCount();
					const CSRIndexedLeg* leg = NULL;
					if ((leg = dynamic_cast<const CSRIndexedLeg*>(swap->GetLeg(0))) ||
						(leg = dynamic_cast<const CSRIndexedLeg*>(swap->GetLeg(1))))
					{
						double spot = leg->GetSpotPrice();
						result = abs(notional*numberOfSecurities*spot*0.01);
					}
				}
			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;

		case TRS_EQUITY_SINGLE:
		{
			try
			{
				const CSRSwap* swap = dynamic_cast<const CSRSwap*>(instr);
				if (swap != nullptr)
				{
					double numberOfSecurities = position.GetInstrumentCount();
					const CSRIndexedLeg* leg = NULL;
					if ((leg = dynamic_cast<const CSRIndexedLeg*>(swap->GetLeg(0))) ||
						(leg = dynamic_cast<const CSRIndexedLeg*>(swap->GetLeg(1))))
					{
						double spot = leg->GetSpotPrice();
						result = abs(numberOfSecurities*spot);
					}
				}

			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;
		case FX_FORWARD:
		case NDF:
		{
			if (!(position.GetAveragePrice()))
			{
				result = 0.0;
				END_LOG();
				break;
			}
			try
			{
				const CSRForexFuture* fwd = dynamic_cast<const CSRForexFuture*>(instr);
				if (fwd != nullptr)
				{
					const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position.GetPortfolioCode(), position.GetExtraction());
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

					double buyNotional = position.GetInstrumentCount();
					double averagePrice = position.GetAveragePrice();
					double realised = position.GetRealised();
					double sellNotional = realised - (marketWay == 1 ? buyNotional * averagePrice : buyNotional / averagePrice);

					if (sellCcy == folioCcy) {
						result = abs(buyNotional);
						END_LOG();
						break;
					}
					double fxSellToBuy = fxSpot->GetLast();

					if (buyCcy == folioCcy)
					{
						result = abs(sellNotional / fxSellToBuy);	// convert to instrument currency
						END_LOG();
						break;
					}

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
					result = abs((sellNotionalCcyFolio + buyNotionalCcyFolio) * fxBuyToFolio);	// convert to instrument ccy
				}
			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;
		case INT_RATE_FUTURE:
		{
			try
			{

				string columnContract = "Nominal";
				double priceCTD = 0;
				const CSRPortfolioColumn*column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnContract.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}
				double nominalValue = value.floatValue;
				const CSRFuture* pFuture = dynamic_cast<const CSRFuture*>(instr);
				if (pFuture != nullptr)
				{
					int underCode = pFuture->GetUnderlyingCode();
					const CSRInstrument* underInstr = gApplicationContext->GetCSRInstrument(underCode);

					if (underInstr != nullptr)
					{
						char type = underInstr->GetType();
						if (type == 'R')
						{
							result = abs(nominalValue);
						}
						else if (type == 'O')
						{
							string columnContract = "Price CTD";
							const CSRPortfolioColumn*column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnContract.c_str());
							if (column)
							{
								column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
								priceCTD = value.floatValue;
							}
							result = abs(nominalValue*priceCTD / 100);
						}
					}

				}
			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;
		case LISTED_OPTION:
		{
			string prodType= "Product Type";
			const CSRPortfolioColumn*column = CSRPortfolioColumn::GetCSRPortfolioColumn(prodType.c_str());
			if (column)
			{
				column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
			}
			string prodColValue = value.nullTerminatedString;
			if (prodColValue == "Equity-Option")
			{

				string columnUCITS = "UCITS Commitment";
				column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnUCITS.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}
				double ucitsValue = value.floatValue;

				string columnDelta = "Delta";
				column = CSRPortfolioColumn::GetCSRPortfolioColumn(columnDelta.c_str());
				if (column)
				{
					column->GetPositionCell(position, portfolioCode, portfolioCode, extraction, 0, position.GetInstrumentCode(), &value, &style, true);
				}
				double deltaValue = value.floatValue;

				if (deltaValue != 0)
				{
					result = abs(ucitsValue)/abs(deltaValue);
					END_LOG();
					break;
				}
			}

			const CSROption* option = NULL;
			const CSRInstrument* underlying = NULL;
			const CSRForexSpot* fxSpot = NULL;

			if (!position.GetAveragePrice() || !instr || !(option = dynamic_cast<const CSROption*>(instr)) || !(option->GetStrikeInProduct())
				|| !(underlying = option->GetUnderlyingInstrument()) || !(fxSpot = dynamic_cast<const CSRForexSpot*>(underlying)))
			{
				result = 0.0;
				END_LOG();
				break;
			}

			const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position.GetPortfolioCode(), position.GetExtraction());
			const CSAMPortfolio* fundFolio = NULL;

			if (!folio || !(fundFolio = folio->GetFundRootPortfolio()))
			{
				result = 0.0;
				END_LOG();
				break;
			}

			try
			{
				long code = option->GetCode();
				const CSRComputationResults* compResult = CSRInstrument::GetComputationResults(code);
				double delta = option->GetDelta(*compResult, &code);
				if (delta != 0) delta = 1;
				double contractSize = option->GetQuotity();
				delta *= contractSize;
				// double delta = option->GetNthPercentDelta(*result, code);
				long folioCcy = fundFolio->GetCurrency();
				long buyCcy = fxSpot->GetForex1();
				long sellCcy = fxSpot->GetForex2();
				int marketWay = fxSpot->GetMarketWay();
				double buyNotional = position.GetInstrumentCount();
				// double strike = fxSpot->GetLast();
				double realised = position.GetRealised();
				double strike = option->GetStrikeInProduct();
				double sellNotional = realised - (marketWay == 1 ? buyNotional * strike : buyNotional / strike);
				double buyFxNotional = buyNotional * delta;
				double sellFxNotional = sellNotional * delta;

				if (sellCcy == folioCcy)
				{
					result = abs(buyFxNotional);
					END_LOG();
					break;
				}

				double fxSellToBuy = fxSpot->GetLast();

				if (buyCcy == folioCcy)
				{
					result = abs(sellFxNotional / fxSellToBuy);		// convert to instrument ccy
					END_LOG();
					break;
				}
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
				result = abs((sellFxNotionalCcyFolio + buyFxNotionalCcyFolio) / folioToInst);	// convert to instrument ccy
			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;
		case OTC_FX_OPTION_SINGLE:
		{
			const CSROption* option = NULL;
			const CSRInstrument* underlying = NULL;
			const CSRForexSpot* fxSpot = NULL;

			if (!instr || !(option = dynamic_cast<const CSROption*>(instr)) || !(option->GetStrikeInProduct())
				|| !(underlying = option->GetUnderlyingInstrument()) || !(fxSpot = dynamic_cast<const CSRForexSpot*>(underlying)))
			{
				result = 0.0;
				END_LOG();
				break;
			}

			const CSAMPortfolio* folio = CSAMPortfolio::GetCSRPortfolio(position.GetPortfolioCode(), position.GetExtraction());
			const CSAMPortfolio* fundFolio = NULL;

			if (!folio || !(fundFolio = folio->GetFundRootPortfolio()))
			{
				result = 0.0;
				END_LOG();
				break;
			}
			try
			{
				long code = option->GetCode();
				const CSRComputationResults* compResult = CSRInstrument::GetComputationResults(code);
				double delta = option->GetDelta(*compResult, &code);
				if (delta != 0) delta = 1;
				double contractSize = option->GetQuotity();
				delta *= contractSize;			// delta seems to be x100
				// double delta = option->GetNthPercentDelta(*result, code);
				long folioCcy = fundFolio->GetCurrency();
				long buyCcy = fxSpot->GetForex1();
				long sellCcy = fxSpot->GetForex2();
				int marketWay = fxSpot->GetMarketWay();
				double buyNotional = position.GetInstrumentCount();
				double strike = option->GetStrikeInProduct();
				double realised = position.GetRealised();
				double sellNotional = realised - (marketWay == 1 ? (buyNotional * strike) : (buyNotional / strike));
				double buyFxNotional = buyNotional * delta;
				double sellFxNotional = sellNotional * delta;

				if (sellCcy == folioCcy)
				{
					result = abs(buyFxNotional);			// already in instrument currency
					break;
				}
				double fxSellToBuy = CSRForexSpot::GetCSRForexSpot(buyCcy, sellCcy)->GetLast();

				if (buyCcy == folioCcy)
				{
					result = abs(sellFxNotional / fxSellToBuy);	// convert to instrument currency
					break;
				}

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

				result = abs((sellFxNotionalCcyFolio + buyFxNotionalCcyFolio) * fxBuyToFolio);	// convert to instrument currency
			}
			catch (const sophisTools::base::ExceptionBase& ex)
			{
				MESS(Log::error, "Error in Gross Leverage column [" << ex << "]");
			}
			END_LOG();
		}break;


		default:
			break;
		}

		cellValue->floatValue = result;
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ short CSxGrossLeverageColumn::GetDefaultWidth() const
{
	// TO DO
	return 60;
}


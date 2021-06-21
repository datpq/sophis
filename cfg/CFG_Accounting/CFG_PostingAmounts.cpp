#pragma warning(disable:4251)
/*
** Includes
*/
#include "SphTools/base/CommonOS.h"
#include "CFG_PostingAmounts.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/portfolio/SphTransactionVector.h"
#include "SphInc/portfolio/SphPosition.h"
#include "SphInc/portfolio/SphPortfolioColumn.h"
#include "SphInc/portfolio/sphExtraction.h"
#include "SphInc/instrument/SphLoanAndRepo.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/collateral/SphSecuritiesReport.h"
#include "SphInc/collateral/SphSecuritiesReportFactory.h"
#include "SphInc/collateral/SphSecuritiesReportResult.h"
#include "SphInc/collateral/SphSecuritiesReportParam.h"
#include "SphInc\collateral\SphSecuritiesReportCriteriaKey.h"
#include "SphInc\value\kernel\SphFundPurchase.h"
#include "SphInc\value\kernel\SphFund.h"
#include "SphLLInc\sophismath.h"
#include "SphInc\market_data\SphMarketData.h"
#include "SphInc\market_data\SphHistoric.h"
#include <stdio.h>
#include "UpgradeExtension.h"

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::instrument;
using namespace sophis::backoffice_kernel;
using namespace sophis::accounting;
using namespace sophis::collateral;
using namespace sophis::value;

/*
** Static
*/
const char* SFAccruedTotalInterestColumn							= "SF Accrued Total Interest";
const char* AccruedAmountColumn										= "Accrued Amount";

const char* CFG_UnsettledBalanceAmount::__CLASS__					= "Unsettled Balance Amount";
const char* CFG_PostingAmountPNL_AccruedAmount::__CLASS__			= "CFG_PostingAmountPNL_AccruedAmount";
const char* CFG_SF_Accrued_Total_Interest::__CLASS__				= "CFG_SF_Accrued_Total_Interest";
const char* CFG_Repo_Underlying_Instrument::__CLASS__				= "CFG_Repo_Underlying_Instrument";
const char* CFG_RepoStock_Loan_Asset_Value::__CLASS__				= "CFG_RepoStock_Loan_Asset_Value";
const char* CSxInterestDebtInstrumentAmountForTrade::__CLASS__		= "CFG Interest Amount on DAT";
const char* CFG_Revaluation_Asset_in_Stock::__CLASS__				= "CFG_Revaluation_Asset_in_Stock";
const char* CFGSRNet::__CLASS__										= "CFGSRNet";
const char* CFGSRNominal::__CLASS__									= "CFGSRNominal";
const char* CFGSRRevenueRegulation::__CLASS__						= "CFGSRRevenueRegulation";
const char* CFGSRIncomeRegulation::__CLASS__						= "CFGSRIncomeRegulation";
const char* CFGSRExpenseRegulation::__CLASS__						= "CFGSRExpenseRegulation";
const char* CFGSRRANRegulation::__CLASS__							= "CFGSRRANRegulation";
const char* CFGSRRANRoundingRegulation::__CLASS__					= "CFGSRRANRoundingRegulation";
const char* CFGSRFees::__CLASS__									= "CFGSRFees";
const char* CFGSubscriptionBalance::__CLASS__						= "CFGSubscriptionBalance";
const char* CFGRedemptionBalance::__CLASS__							= "CFGRedemptionBalance";


/*
** Methods
*/
CONSTRUCTOR_POSTING_AMOUNT_FOR_PNL(CFG_UnsettledBalanceAmount)

double CFG_UnsettledBalanceAmount::get_posting_amount(const CSRPosition& position, long* currency, long* instrument) const
{
	BEGIN_LOG("get_posting_amount");
	double amountRet = 0;
	*currency = 0 ;
	long instrumentCode = position.GetInstrumentCode();
	const CSRInstrument* inst	= CSRInstrument::GetInstance(instrumentCode) ;
	MESS(Log::debug, "Sicovam : " << instrumentCode << " and instrument type : " << inst->GetType() );
	const CSRPortfolio* pFolio = position.GetPortfolioWithoutException();
	//DPH
	//const CSRExtraction* pExtraction = NULL;
	PSRExtraction pExtraction;

	long folioCode = -1;	
	if (pFolio)
	{
		pExtraction = pFolio->GetExtraction();
		folioCode = pFolio->GetCode();
	}

	const CSRPortfolioColumn* pPtfCol = CSRPortfolioColumn::GetCSRPortfolioColumn("Unsettled Balance");
	if (pPtfCol && inst)
	{			
		MESS(Log::debug, "Column 'Unsettled Balance' successfully retrieved.");

		SSCellValue cellValue;
		SSCellStyle cellStyle;

		pPtfCol->GetPositionCell(position, folioCode,
			folioCode,
			pExtraction,
			instrumentCode,
			instrumentCode,
			&cellValue,
			&cellStyle,
			true);

		amountRet = cellValue.floatValue;
	}

	*currency = inst->GetSettlementCurrency() ;
	*instrument	= instrumentCode;
	END_LOG();
	return amountRet ;

}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_POSTING_AMOUNT_FOR_PNL(CFG_PostingAmountPNL_AccruedAmount);

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ double CFG_PostingAmountPNL_AccruedAmount::get_posting_amount( const CSRPosition& position, long* currency, long *instrument ) const /*=0*/
{
	BEGIN_LOG("get_posting_amount");
	double amountRet(0);
	double nbSecurities = 0;
	*currency = 0 ;

	long folioCode = -1;
	long instrumentCode = position.GetInstrumentCode();
	const CSRInstrument * inst	= CSRInstrument::GetInstance(instrumentCode);

	MESS(Log::debug, "Sicovam : " << instrumentCode << " and instrument type : " << inst->GetType() );

	if ((inst->GetType() == 'L') || (inst->GetType() == 'P') || (inst->GetType() == 'T'))
	{
		const CSRPortfolio * pFolio = position.GetPortfolioWithoutException();
		//DPH
		//const CSRExtraction * pExtraction = NULL;
		PSRExtraction pExtraction;

		if (pFolio)
		{
			pExtraction = pFolio->GetExtraction();
			folioCode = pFolio->GetCode();
		}

		const CSRPortfolioColumn * pPtfCol1 = CSRPortfolioColumn::GetCSRPortfolioColumn("Number of securities");

		if (pPtfCol1 && inst)
		{				
			SSCellValue cellValue;
			SSCellStyle cellStyle;

			pPtfCol1->GetPositionCell(	position, 
										folioCode,
										folioCode,
										pExtraction,
										instrumentCode,
										instrumentCode,
										&cellValue,
										&cellStyle,
										true);

			nbSecurities = cellValue.floatValue;
		}
		else
		{
			MESS(Log::warning, "Failed to find portfolio column 'Number of securities'");
		}

		if (nbSecurities != 0)
		{
			const CSRPortfolioColumn * pPtfCol2 = CSRPortfolioColumn::GetCSRPortfolioColumn(AccruedAmountColumn);

			if (pPtfCol2 && inst)
			{				
				SSCellValue cellValue;
				SSCellStyle cellStyle;

				pPtfCol2->GetPositionCell(	position, 
											folioCode,
											folioCode,
											pExtraction,
											instrumentCode,
											instrumentCode,
											&cellValue,
											&cellStyle,
											true);

				amountRet = cellValue.floatValue;
			}
			else
			{
				MESS(Log::warning, "Failed to find portfolio column '" << AccruedAmountColumn << "'");
			}
		}
	}

	*currency = inst->GetSettlementCurrency() ;
	*instrument	= instrumentCode;

	END_LOG();
	return amountRet ;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_POSTING_AMOUNT_FOR_PNL(CFG_SF_Accrued_Total_Interest)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ double CFG_SF_Accrued_Total_Interest::get_posting_amount( const CSRPosition& position, long* currency, long *instrument ) const /*=0*/
{
	BEGIN_LOG("get_posting_amount");
	double amountRet(0);
	double NbSecurities = 0;
	*currency = 0 ;

	long folioCode = -1;
	long instrumentCode = position.GetInstrumentCode();
	const CSRInstrument* inst	= CSRInstrument::GetInstance(instrumentCode) ;

	MESS(Log::debug, "Sicovam : " << instrumentCode << " and instrument type : " << inst->GetType() );

	if ((inst->GetType() == 'L') || (inst->GetType() == 'P') || (inst->GetType() == 'T'))
	{
		const CSRPortfolio* pFolio = position.GetPortfolioWithoutException();
		//DPH
		//const CSRExtraction* pExtraction = NULL;
		PSRExtraction pExtraction;

		if (pFolio)
		{
			pExtraction = pFolio->GetExtraction();
			folioCode = pFolio->GetCode();
		}

		const CSRPortfolioColumn* pPtfCol1 = CSRPortfolioColumn::GetCSRPortfolioColumn("Number of securities");

		if (pPtfCol1 && inst)
		{				
			SSCellValue cellValue;
			SSCellStyle cellStyle;

			pPtfCol1->GetPositionCell(position, folioCode,
				folioCode,
				pExtraction,
				instrumentCode,
				instrumentCode,
				&cellValue,
				&cellStyle,
				true);

			NbSecurities = cellValue.floatValue;
		}

		if (NbSecurities != 0)
		{
			const CSRPortfolioColumn* pPtfCol2 = CSRPortfolioColumn::GetCSRPortfolioColumn(SFAccruedTotalInterestColumn);

			if (pPtfCol2 && inst)
			{				
				SSCellValue cellValue;
				SSCellStyle cellStyle;

				pPtfCol2->GetPositionCell(position, folioCode,
					folioCode,
					pExtraction,
					instrumentCode,
					instrumentCode,
					&cellValue,
					&cellStyle,
					true);

				amountRet = cellValue.floatValue;
			}
		}
	}

	*currency = inst->GetSettlementCurrency() ;
	*instrument	= instrumentCode;
	END_LOG();
	return amountRet ;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_POSTING_AMOUNT_FOR_PNL(CFG_RepoStock_Loan_Asset_Value)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ double CFG_RepoStock_Loan_Asset_Value::get_posting_amount( const CSRPosition& position, long* currency, long *instrument ) const /*=0*/
{
	BEGIN_LOG("get_posting_amount");
	double amountRet(0);
	double NbSecurities = 0;
	*currency = 0 ;

	long folioCode = -1;
	long instrumentCode = position.GetInstrumentCode();
	const CSRInstrument* inst	= CSRInstrument::GetInstance(instrumentCode) ;

	if (inst)
	{
		*currency = inst->GetSettlementCurrency() ;

		MESS(Log::debug, "Sicovam : " << instrumentCode << " and instrument type : " << inst->GetType() );

		if (inst->GetType() == 'P')
		{
			const CSRPortfolio* pFolio = position.GetPortfolioWithoutException();
			//DPH
			//const CSRExtraction* pExtraction = NULL;
			PSRExtraction pExtraction;

			if (pFolio)
			{
				pExtraction = pFolio->GetExtraction();
				folioCode = pFolio->GetCode();
			}

			const CSRPortfolioColumn* pPtfCol1 = CSRPortfolioColumn::GetCSRPortfolioColumn("Number of securities");

			if (pPtfCol1 && inst)
			{				
				SSCellValue cellValue;
				SSCellStyle cellStyle;

				pPtfCol1->GetPositionCell(position, folioCode,
					folioCode,
					pExtraction,
					instrumentCode,
					instrumentCode,
					&cellValue,
					&cellStyle,
					true);

				NbSecurities = cellValue.floatValue;
			}

			//DPH
			//long code_emet = inst->GetUnderlying(0);
			//long code_emet = UpgradeExtension::GetUnderlying(inst, 0);
			//const CSRInstrument * under = CSRInstrument::GetInstance(code_emet);
			const CSRInstrument * under = inst->GetUnderlyingInstrument();
			if (under)
			{
				//DPH
				long code_emet = under->GetCode();
				double underlying_last = 0;
				if (under->GetType() == 'O')
				{
					CSRMarketData::SSDates prices(true);

					//DPH prices.fUseHistoricalFairValue is true in the version 4.3.2, and false in 7.3.3.
					//We have to force the value to true
					MESS(Log::debug, "forcing fUseHistoricalFairValue = true");
					prices.fUseHistoricalFairValue = true;

					if (prices.fUseHistoricalFairValue == false)
					{
						//DPH
						//double floatingFactor = under->GetFloatingNotionalFactor();
						double floatingFactor = UpgradeExtension::GetFloatingNotionalFactor(under);

						MESS(Log::debug, "floatingFactor = " << floatingFactor);

                        double undernotional = under->GetNotional();
						
						MESS(Log::debug, "undernotional = " << undernotional);

						//DPH
						//double theovalue =  under->GetTheoreticalValue();
						double theovalue = UpgradeExtension::GetTheoreticalValue(under);
						MESS(Log::debug, "TheoreticalValue : " << theovalue );

						underlying_last = theovalue * undernotional * floatingFactor;
						MESS(Log::debug, "underlying_last without fUseHistoricalFairValue : " << underlying_last );
					}
					else
					{
						char query0 [512] = {'\0'};
						sprintf_s(query0, "select nvl(t,0) + nvl(coupon,0) from historique where sicovam = %ld and DATE_TO_NUM(JOUR) <= %ld ORDER BY JOUR DESC", code_emet, gApplicationContext->GetDate());
						MESS(Log::debug, "query to get theoritical with fUseHistoricalFairValue = " << query0);
						errorCode err0 = CSRSqlQuery::QueryReturning1Double(query0, &underlying_last);
						if (err0)
							return 0;

						//DPH
						//double floatingFactor = under->GetFloatingNotionalFactor();
						double floatingFactor = UpgradeExtension::GetFloatingNotionalFactor(under);

						MESS(Log::debug, "floatingFactor = " << floatingFactor);

                        double undernotional = under->GetNotional();
						
						MESS(Log::debug, "undernotional = " << undernotional);

						if (floatingFactor != 0)
							underlying_last = underlying_last/100 * undernotional * floatingFactor;
						else
							underlying_last = underlying_last/100 * undernotional;
						
						MESS(Log::debug, "underlying_last with query and fUseHistoricalFairValue = " << underlying_last);
					}
				}
				else
				{
					underlying_last = CSRInstrument::GetLast(code_emet);
				}
				amountRet = underlying_last * NbSecurities;
				*instrument	= code_emet;
				END_LOG();
				return amountRet ;

			}
		}
	}

	*instrument	= instrumentCode;
	END_LOG();
	return amountRet ;
}


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_POSTING_AMOUNT_FOR_PNL(CFG_Revaluation_Asset_in_Stock)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ double CFG_Revaluation_Asset_in_Stock::get_posting_amount( const CSRPosition& position, long* currency, long *instrument ) const /*=0*/
{
	BEGIN_LOG("get_posting_amount");
	double amountRet(0);

	try
	{
		double NbSecurities = 0;
		*currency = 0 ;

		long instrumentCode = position.GetInstrumentCode();
		*instrument	= instrumentCode;
		MESS(Log::debug, "Instrument to test = " << instrumentCode);
		double NbLendedSecurities = 0;
		double netPosition = 0;
		instrument::ePositionType myPositionType = position.GetPositionType();
		long myPositionIdent = position.GetIdentifier();
		const CSRInstrument* instPose	= CSRInstrument::GetInstance(instrumentCode) ;
		if (instPose == NULL)
			return 0;

		switch(myPositionType)
		{
		case ePositionType::pVirtual:
		case ePositionType::pLended:
		{
			*currency = instPose->GetSettlementCurrency() ;
			MESS(Log::debug, "Instrument with code " << instrumentCode << " is in a Virtual or Lended position");
			return 0;
		}
		}

		const CSRPortfolio* myFolio = position.GetPortfolio();
		//DPH
		//const CSRExtraction* myExtraction = NULL;
		PSRExtraction myExtraction;

		if (myFolio)
		{
			myExtraction = myFolio->GetExtraction();
			long folioCode = myFolio->GetCode();

			const CSRPortfolioColumn* pPtfCol1 = CSRPortfolioColumn::GetCSRPortfolioColumn("Number of securities");
			const CSRInstrument* instPose	= CSRInstrument::GetInstance(instrumentCode) ;

			if (pPtfCol1 && instPose)
			{				
				SSCellValue cellValue;
				SSCellStyle cellStyle;

				pPtfCol1->GetPositionCell(position, folioCode,
					folioCode,
					myExtraction,
					instrumentCode,
					instrumentCode,
					&cellValue,
					&cellStyle,
					true);

				netPosition = cellValue.floatValue;
				MESS(Log::debug, "netPosition of the Bond = " << netPosition);
			}


			int level = myFolio->GetLevel();
			long parentCode = myFolio->GetParentCode();

			const CSRPortfolio* myParentFolio = CSRPortfolio::GetCSRPortfolio(parentCode,myExtraction);
			if (myParentFolio)
			{
				long ParentID = myParentFolio->GetCode();
				long parentFinalCode = myParentFolio->GetParentCode();

				const CSRPortfolio* myParentFinalFolio = CSRPortfolio::GetCSRPortfolio(parentFinalCode,myExtraction);
				if (myParentFinalFolio)
				{
					MESS(Log::debug, "Code of the account entity folio = " << parentFinalCode);

					int numberofFolio = myParentFinalFolio->GetChildCount();
					for (int i = 0; i< numberofFolio; i++)
					{
						const CSRPortfolio * Child = myParentFinalFolio->GetNthChild(i);
						if (Child)
						{
							int Childlevel = Child->GetLevel();
							long ChildCode = Child->GetCode();
							if (Childlevel == 3)
							{
								MESS(Log::debug, "Code of folio that will be read = " << ChildCode);

								int tree = Child->GetTreeViewPositionCount();
								for (int j = 0; j < tree; j++)
								{
									const CSRPosition * myPose = Child->GetNthTreeViewPosition(j);
									if (myPose)
									{
										long myPoseIntrrumentCode = myPose->GetInstrumentCode();

										const CSRInstrument* inst	= CSRInstrument::GetInstance(myPoseIntrrumentCode) ;
										if (inst)
										{
											if (inst->GetType() == 'P')
											{
												MESS(Log::debug, "MP that will be checked = " << myPoseIntrrumentCode);

												const CSRPortfolioColumn* pPtfCol1 = CSRPortfolioColumn::GetCSRPortfolioColumn("Number of securities");
												if (pPtfCol1 && inst)
												{				
													SSCellValue cellValue;
													SSCellStyle cellStyle;

													pPtfCol1->GetPositionCell(*myPose, ChildCode,
														ChildCode,
														myExtraction,
														myPoseIntrrumentCode,
														myPoseIntrrumentCode,
														&cellValue,
														&cellStyle,
														true);

													double NbSecurities = cellValue.floatValue;
													MESS(Log::debug, "NbSecurities of this MP = " << NbSecurities);

													if (NbSecurities < 0)
													{
														//DPH
														//long code_emet = inst->GetUnderlying(0);
														//long code_emet = UpgradeExtension::GetUnderlying(inst, 0);
														long code_emet = 0;
														const CSRInstrument * underlyingInstrument = inst->GetUnderlyingInstrument();
														if (underlyingInstrument) {
															code_emet = underlyingInstrument->GetCode();
														}
														if (code_emet == instrumentCode)
														{
															MESS(Log::debug, "This MP has the same underlying as the instrument of the position");
															NbLendedSecurities += NbSecurities;
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		MESS(Log::debug, "NbLendedSecurities = " << NbLendedSecurities);

		const CSRInstrument * under = CSRInstrument::GetInstance(instrumentCode);
		if (under)
		{
			double underlying_last = 0;
			if (under->GetType() == 'O')
			{
				CSRMarketData::SSDates prices(true);

				//DPH problem Unrealized PnL, prices.fUseHistoricalFairValue is true in the version 4.3.2, and false in 7.3.3.
				//We have to force the value to true
				MESS(Log::debug, "forcing fUseHistoricalFairValue = true");
				prices.fUseHistoricalFairValue = true;

				if (prices.fUseHistoricalFairValue == false)
				{
					//DPH
					//double floatingFactor = under->GetFloatingNotionalFactor();
					double floatingFactor = UpgradeExtension::GetFloatingNotionalFactor(under);

					MESS(Log::debug, "floatingFactor = " << floatingFactor);

					double undernotional = under->GetNotional();

					MESS(Log::debug, "undernotional = " << undernotional);

					//DPH
					//double theovalue =  under->GetTheoreticalValue();
					double theovalue = UpgradeExtension::GetTheoreticalValue(under);
					MESS(Log::debug, "TheoreticalValue : " << theovalue);

					underlying_last = theovalue * undernotional * floatingFactor;
					MESS(Log::debug, "underlying_last without fUseHistoricalFairValue : " << underlying_last );
				}
				else
				{
					char query0 [512] = {'\0'};
					sprintf_s(query0, "select nvl(t,0) + nvl(coupon,0) from historique where sicovam = %ld and DATE_TO_NUM(JOUR) <= %ld ORDER BY JOUR DESC", instrumentCode, gApplicationContext->GetDate());
					MESS(Log::debug, "query to get theoritical with fUseHistoricalFairValue = " << query0);
					errorCode err0 = CSRSqlQuery::QueryReturning1Double(query0, &underlying_last);
					if (err0)
						return 0;

					//DPH
					//double floatingFactor = under->GetFloatingNotionalFactor();
					double floatingFactor = UpgradeExtension::GetFloatingNotionalFactor(under);

					MESS(Log::debug, "floatingFactor = " << floatingFactor);

                    double undernotional = under->GetNotional();
					
					MESS(Log::debug, "undernotional = " << undernotional);

					if (floatingFactor != 0)
						underlying_last = underlying_last/100 * undernotional * floatingFactor;
					else
						underlying_last = underlying_last/100 * undernotional;
					
					MESS(Log::debug, "underlying_last with query and fUseHistoricalFairValue = " << underlying_last);
				}
			}
			else
			{
				underlying_last = CSRInstrument::GetLast(instrumentCode);
			}
			amountRet = underlying_last * (netPosition + NbLendedSecurities);
			MESS(Log::debug, "amountRet = underlying_last * (netPosition - NbLendedSecurities) = " << amountRet);
			*currency = instPose->GetSettlementCurrency() ;
			*instrument	= instrumentCode;
			END_LOG();
			return amountRet ;

		}
	}




	//	MESS(Log::debug, "Sicovam : " << instrumentCode << " and instrument type : " << inst->GetType() );

	//	const CSRSecuritiesReportFactory * factory = CSRSecuritiesReportFactory::GetPrototype().GetData("Default");

	//	MESS(Log::debug, "Factory done" );

	//	CSRSecuritiesReportParam * Myparam = factory->new_SecuritiesReportParam();
	//	long entity = position.GetEntity();
	//	MESS(Log::debug, "entity with position method = " << entity );

	//	if (entity == 0)
	//	{
	//		CSRTransactionVector transaction_list;
	//		position.GetTransactions(transaction_list);
	//		if (transaction_list.size() > 0)
	//		{
	//			entity = transaction_list[0].GetEntity();
	//			MESS(Log::debug, "entity with trade method = " << entity );
	//		}
	//	}

	//	Myparam->SetEntity(entity);
	//	Myparam->SetInstrument(instrumentCode);

	//	MESS(Log::debug, "Parameter filled" );

	//	CSRSecuritiesReport * mySecRep = new CSRSecuritiesReport(*factory, Myparam, gApplicationContext->GetDate(), eSecuritiesExtractionType::seTradeDate, CSRSecuritiesReport::efLoadAllInstruments, true);

	//	MESS(Log::debug, "CSRSecuritiesReport done" );

	//	CSRSecuritiesReportCriteriaKey key;
	//	const CSRSecuritiesReportResultHier* myResult = mySecRep->BuildResult(key, instrumentCode);

	//	MESS(Log::debug, "CSRSecuritiesReport built" );

	//	double TotalQuantity = myResult->GetQuantityTotal();

	//	MESS(Log::debug, "TotalQuantity : " << TotalQuantity );

	//	delete Myparam;

	//	double InstLast = CSRInstrument::GetLast(instrumentCode);

	//	amountRet = InstLast * TotalQuantity;

	//	*currency = inst->GetSettlementCurrency() ;
	//	*instrument	= instrumentCode;
	//}
	//catch (ExceptionBase& e)
	//{
	//	MESS(Log::error, "error : " << (const char *)e );
	//}

	catch (...)
	{
		MESS(Log::error, "Unknown error when computing CFG_SF_Accrued_Total_Interest");
	}

	//END_LOG();
	return amountRet ;

}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_POSTING_AMOUNT_FOR_TRADE(CFG_Repo_Underlying_Instrument)

//-------------------------------------------------------------------------------------------------------------
double CFG_Repo_Underlying_Instrument::get_posting_amount(const CSRTransaction& trade, long* currency, long* third_party, long* instrument) const
{			
	BEGIN_LOG("get_posting_amount");
	double amountRT = 0L;

	//DPH
	//long refcon = trade.GetTransactionCode();
	TransactionIdent refcon = trade.GetTransactionCode();
	long instrumentCode = trade.GetInstrumentCode();
	const CSRInstrument * instr = CSRInstrument::GetInstance(instrumentCode);
	if (instr)
	{
		if (instr->GetType() == 'P')
		{
			//DPH
			//long code_emet = inst->GetUnderlying(0);
			//long code_emet = UpgradeExtension::GetUnderlying(instr, 0);
			//const CSRInstrument * under = CSRInstrument::GetInstance(code_emet);
			const CSRInstrument * under = instr->GetUnderlyingInstrument();
			if (under)
			{
				//DPH
				long code_emet = under->GetCode();
				*instrument = code_emet;
				*currency = under->GetSettlementCurrency();
			}
		}
		else
		{
			char query [256] = {'\0'};
			sprintf_s(query, "select code_emet from titres where sicovam=(select sicovam from histomvts where refcon=(select reference from histomvts where refcon=%ld))",refcon);
			long code_emet = 0; 
			errorCode err = CSRSqlQuery::QueryReturning1Long (query, &code_emet);
			if (err)
				return amountRT;

			*instrument = code_emet;

			const CSRInstrument* pInstr = CSRInstrument::GetInstance(trade.GetInstrumentCode());
			if (pInstr)
				*currency = pInstr->GetSettlementCurrency();
		}
	}


	END_LOG();
	return amountRT;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_POSTING_AMOUNT_FOR_TRADE(CSxInterestDebtInstrumentAmountForTrade)

double CSxInterestDebtInstrumentAmountForTrade::get_posting_amount(const CSRTransaction& trade, long* currency, long* third_party, long* instrument) const
{			
	BEGIN_LOG("get_posting_amount");
	double amountRT = 0L;
	*instrument = trade.GetInstrumentCode();

	const CSRInstrument* pInstr = CSRInstrument::GetInstance(*instrument);
	if (pInstr->GetType() == 'T')
	{
		MESS(Log::debug, "Debt instrument amount computed using net amount from trade (" << trade.GetNetAmount()  << ") and Notional (" << trade.GetNotional());
		amountRT = trade.GetNetAmount() - trade.GetNotional();
		*currency = pInstr->GetSettlementCurrency();
	}

	END_LOG();
	return amountRT;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSRNet)

double CFGSRNet::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	//amountRet = sr.GetAmount();
	amountRet = sr.GetGrossAmount() + sr.GetFeesAmountInt();
	MESS(Log::debug, "SR Net Amount = " << amountRet);

	thirdParty = sr.GetThird1();
	MESS(Log::debug, "SR Thirdparty = " << thirdParty);

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSRNominal)

double CFGSRNominal::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	double initialNavShare = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	amountRet = sr.GetNbUnits();
	MESS(Log::debug, "SR Nb Units = " << amountRet);

	thirdParty = sr.GetThird1();
	MESS(Log::debug, "SR Thirdparty = " << thirdParty);

	long fundcode = sr.GetFundCode();
	MESS(Log::debug, "SR Fund Code = " << fundcode);
	const CSRInstrument * myinstrument = CSRInstrument::GetInstance(fundcode);
	if (myinstrument)
	{
		const CSAMFund * myFund = dynamic_cast<const CSAMFund *>(myinstrument);
		if (NULL != myFund)
		{
			initialNavShare = myFund->GetIssuePrice();
			MESS(Log::debug, "Fund_initialNavShare = " << initialNavShare);
		}
	}
	
	amountRet = amountRet * initialNavShare;
	MESS(Log::debug, "Result = " << amountRet);

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSRRevenueRegulation)

double CFGSRRevenueRegulation::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	double nbShares = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	long NavDate = sr.GetNAVDate();
	long fundcode = sr.GetFundCode();
	long entityCode = 0;
	const CSRInstrument * myinstrument = CSRInstrument::GetInstance(fundcode);
	if (myinstrument)
	{
		const CSAMFund * myFund = dynamic_cast<const CSAMFund *>(myinstrument);
		if (NULL != myFund)
		{
			entityCode = myFund->GetEntity();

			//nbShares = myFund->GetSharesCount();
			char query0 [512] = {'\0'};
			sprintf_s(query0, "select sum(decode(SIGN,'+',quantity,'-',-1*quantity)) from account_posting where account_number in ('1123','1124') and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
			MESS(Log::debug, "query nbShare the day before the nav_date = " << query0);
			errorCode err0 = CSRSqlQuery::QueryReturning1Double(query0, &nbShares);
			if (err0)
				return 0;

			MESS(Log::debug, "nbShares = " << nbShares);

		}
	}

	char query1 [512] = {'\0'};
	sprintf_s(query1, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_number ='180' and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde180 = " << query1);
	double Solde180 = 0; 
	errorCode err1 = CSRSqlQuery::QueryReturning1Double(query1, &Solde180);
	if (err1)
		return 0;
	MESS(Log::debug, "solde180 = " << Solde180);


	char query2 [512] = {'\0'};
	sprintf_s(query2, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_number ='1702' and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde1702 = " << query2);
	double Solde1702 = 0; 
	errorCode err2 = CSRSqlQuery::QueryReturning1Double(query2, &Solde1702);
	if (err2)
		return 0;
	MESS(Log::debug, "solde1702 = " << Solde1702);

	thirdParty = sr.GetThird1();
	MESS(Log::debug, "SR Thirdparty = " << thirdParty);

	MESS(Log::debug, "nbUnits = " << sr.GetNbUnits());

	amountRet = nbShares ?  ( (Solde180 + Solde1702) * abs(sr.GetNbUnits())/nbShares ) : 0;

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSRIncomeRegulation)

double CFGSRIncomeRegulation::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	double nbShares = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	long NavDate = sr.GetNAVDate();
	long fundcode = sr.GetFundCode();
	long entityCode = 0;
	const CSRInstrument * myinstrument = CSRInstrument::GetInstance(fundcode);
	if (myinstrument)
	{
		const CSAMFund * myFund = dynamic_cast<const CSAMFund *>(myinstrument);
		if (NULL != myFund)
		{
			entityCode = myFund->GetEntity();
			
			//nbShares = myFund->GetSharesCount();
			char query0 [512] = {'\0'};
			sprintf_s(query0, "select sum(decode(SIGN,'+',quantity,'-',-1*quantity)) from account_posting where account_number in ('1123','1124') and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
			MESS(Log::debug, "query nbShare the day before the nav_date = " << query0);
			errorCode err0 = CSRSqlQuery::QueryReturning1Double(query0, &nbShares);
			if (err0)
				return 0;

			MESS(Log::debug, "nbShares = " << nbShares);
		}
	}

	char query1 [512] = {'\0'};
	sprintf_s(query1, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_name_id in (select id from account_name where category = 'Classe 7') and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde7 = " << query1);
	double Solde7 = 0; 
	errorCode err1 = CSRSqlQuery::QueryReturning1Double(query1, &Solde7);
	if (err1)
		return 0;
	MESS(Log::debug, "Solde7 = " << Solde7);

	char query2 [512] = {'\0'};
	sprintf_s(query2, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_number ='8811' and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde8811 = " << query2);
	double Solde8811 = 0; 
	errorCode err2 = CSRSqlQuery::QueryReturning1Double(query2, &Solde8811);
	if (err2)
		return 0;
	MESS(Log::debug, "Solde8811 = " << Solde8811);

	thirdParty = sr.GetThird1();
	MESS(Log::debug, "SR Thirdparty = " << thirdParty);

	MESS(Log::debug, "nbUnits = " << sr.GetNbUnits());

	amountRet = nbShares ?  (Solde7 + Solde8811) * abs(sr.GetNbUnits())/nbShares : 0;

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSRExpenseRegulation)

double CFGSRExpenseRegulation::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	double nbShares = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	long NavDate = sr.GetNAVDate();
	long fundcode = sr.GetFundCode();
	long entityCode = 0;
	const CSRInstrument * myinstrument = CSRInstrument::GetInstance(fundcode);
	if (myinstrument)
	{
		const CSAMFund * myFund = dynamic_cast<const CSAMFund *>(myinstrument);
		if (NULL != myFund)
		{
			entityCode = myFund->GetEntity();

			//nbShares = myFund->GetSharesCount();
			char query0 [512] = {'\0'};
			sprintf_s(query0, "select sum(decode(SIGN,'+',quantity,'-',-1*quantity)) from account_posting where account_number in ('1123','1124') and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
			MESS(Log::debug, "query nbShare the day before the nav_date = " << query0);
			errorCode err0 = CSRSqlQuery::QueryReturning1Double(query0, &nbShares);
			if (err0)
				return 0;

			MESS(Log::debug, "nbShares = " << nbShares);
		}
	}

	char query1 [512] = {'\0'};
	sprintf_s(query1, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_name_id in (select id from account_name where category = 'Classe 6') and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde6 = " << query1);
	double Solde6 = 0; 
	errorCode err1 = CSRSqlQuery::QueryReturning1Double(query1, &Solde6);
	if (err1)
		return 0;
	MESS(Log::debug, "Solde6 = " << Solde6);

	char query2 [512] = {'\0'};
	sprintf_s(query2, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_number ='8812' and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde8812 = " << query2);
	double Solde8812 = 0; 
	errorCode err2 = CSRSqlQuery::QueryReturning1Double(query2, &Solde8812);
	if (err2)
		return 0;
	MESS(Log::debug, "Solde8811 = " << Solde8812);

	thirdParty = sr.GetThird1();
	MESS(Log::debug, "SR Thirdparty = " << thirdParty);

	MESS(Log::debug, "nbUnits = " << sr.GetNbUnits());

	amountRet = nbShares ? (Solde6 + Solde8812) * abs(sr.GetNbUnits())/nbShares : 0;

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSRRANRegulation)

double CFGSRRANRegulation::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	double nbShares = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	long NavDate = sr.GetNAVDate();
	long fundcode = sr.GetFundCode();
	long entityCode = 0;
	const CSRInstrument * myinstrument = CSRInstrument::GetInstance(fundcode);
	if (myinstrument)
	{
		const CSAMFund * myFund = dynamic_cast<const CSAMFund *>(myinstrument);
		if (NULL != myFund)
		{
			entityCode = myFund->GetEntity();

			//nbShares = myFund->GetSharesCount();
			char query0 [512] = {'\0'};
			sprintf_s(query0, "select sum(decode(SIGN,'+',quantity,'-',-1*quantity)) from account_posting where account_number in ('1123','1124') and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
			MESS(Log::debug, "query nbShare the day before the nav_date = " << query0);
			errorCode err0 = CSRSqlQuery::QueryReturning1Double(query0, &nbShares);
			if (err0)
				return 0;

			MESS(Log::debug, "nbShares = " << nbShares);
		}
	}

	char query1 [512] = {'\0'};
	sprintf_s(query1, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_number = '160' and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde160 = " << query1);
	double Solde160 = 0; 
	errorCode err1 = CSRSqlQuery::QueryReturning1Double(query1, &Solde160);
	if (err1)
		return 0;
	MESS(Log::debug, "Solde160 = " << Solde160);

	char query2 [512] = {'\0'};
	sprintf_s(query2, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_number = '1701' and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde1701 = " << query2);
	double Solde1701 = 0; 
	errorCode err2 = CSRSqlQuery::QueryReturning1Double(query2, &Solde1701);
	if (err2)
		return 0;
	MESS(Log::debug, "Solde1701 = " << Solde1701);

	thirdParty = sr.GetThird1();
	MESS(Log::debug, "SR Thirdparty = " << thirdParty);

	MESS(Log::debug, "nbUnits = " << sr.GetNbUnits());

	amountRet = nbShares ? (Solde160 + Solde1701) * abs(sr.GetNbUnits())/nbShares : 0;

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSRRANRoundingRegulation)

double CFGSRRANRoundingRegulation::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	double nbShares = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	long NavDate = sr.GetNAVDate();
	long fundcode = sr.GetFundCode();
	long entityCode = 0;
	const CSRInstrument * myinstrument = CSRInstrument::GetInstance(fundcode);
	if (myinstrument)
	{
		const CSAMFund * myFund = dynamic_cast<const CSAMFund *>(myinstrument);
		if (NULL != myFund)
		{
			entityCode = myFund->GetEntity();

			//nbShares = myFund->GetSharesCount();
			char query0 [512] = {'\0'};
			sprintf_s(query0, "select sum(decode(SIGN,'+',quantity,'-',-1*quantity)) from account_posting where account_number in ('1123','1124') and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
			MESS(Log::debug, "query nbShare the day before the nav_date = " << query0);
			errorCode err0 = CSRSqlQuery::QueryReturning1Double(query0, &nbShares);
			if (err0)
				return 0;

			MESS(Log::debug, "nbShares = " << nbShares);
		}
	}

	char query1 [512] = {'\0'};
	sprintf_s(query1, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_number = '1601' and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde1601 = " << query1);
	double Solde1601 = 0; 
	errorCode err1 = CSRSqlQuery::QueryReturning1Double(query1, &Solde1601);
	if (err1)
		return 0;
	MESS(Log::debug, "Solde1601 = " << Solde1601);

	char query2 [512] = {'\0'};
	sprintf_s(query2, "Select sum(decode(CREDIT_DEBIT,'D',amount,'C',-1*amount)) from account_posting where account_number ='17011' and DATE_TO_NUM(posting_date)<=%ld and account_entity_id = (select id from account_entity where record_type=1 and name like (select name from tiers where ident=%ld))",NavDate, entityCode);
	MESS(Log::debug, "query solde17011 = " << query2);
	double Solde17011 = 0; 
	errorCode err2 = CSRSqlQuery::QueryReturning1Double(query2, &Solde17011);
	if (err2)
		return 0;
	MESS(Log::debug, "Solde1701 = " << Solde17011);

	thirdParty = sr.GetThird1();
	MESS(Log::debug, "SR Thirdparty = " << thirdParty);

	MESS(Log::debug, "nbUnits = " << sr.GetNbUnits());
	
	amountRet = nbShares ? (Solde1601 + Solde17011) * abs(sr.GetNbUnits())/nbShares : 0;

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSRFees)

double CFGSRFees::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	amountRet = sr.GetFeesAmountInt(); /*sr.GetFeesAmountExt() + */ 
	MESS(Log::debug, "SR Fees = " << amountRet);

	thirdParty = sr.GetThird1();
	MESS(Log::debug, "SR Thirdparty = " << thirdParty);

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGSubscriptionBalance)

double CFGSubscriptionBalance::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	double amountCFGSRNet = CSAmSrAmountType::GetInstance("CFG S/R Net")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRNominal = CSAmSrAmountType::GetInstance("CFG S/R Nominal")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRRevenueRegulation = CSAmSrAmountType::GetInstance("CFG S/R Revenue Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRIncomeRegulation = CSAmSrAmountType::GetInstance("CFG S/R Income Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRExpenseRegulation = CSAmSrAmountType::GetInstance("CFG S/R Expense Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRRANRegulation = CSAmSrAmountType::GetInstance("CFG S/R R.A.N. Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRRANRoundingRegulation = CSAmSrAmountType::GetInstance("CFG S/R R.A.N. Rounding Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRFees = CSAmSrAmountType::GetInstance("CFG S/R Fees")->GetSRAmount(sr, thirdParty, currency);

	//amountRet = abs(amountCFGSRNet) - abs(amountCFGSRNominal) + amountCFGSRRevenueRegulation + amountCFGSRIncomeRegulation - amountCFGSRExpenseRegulation + amountCFGSRRANRegulation + amountCFGSRRANRoundingRegulation - abs(amountCFGSRFees);
	amountRet = amountCFGSRNet - abs(amountCFGSRNominal) + amountCFGSRRevenueRegulation + amountCFGSRIncomeRegulation + amountCFGSRExpenseRegulation + amountCFGSRRANRegulation + amountCFGSRRANRoundingRegulation - abs(amountCFGSRFees);
	MESS(Log::debug, "CFGSubscriptionBalance Amount = " << amountRet);

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_SR_AMOUNT_TYPE(CFGRedemptionBalance)

double CFGRedemptionBalance::GetSRAmount(const sophis::value::CSAMFundSR& sr, long& thirdParty, long& currency) const
{
	BEGIN_LOG("GetSRAmount");
	double amountRet = 0;
	MESS(Log::debug, "SR Code = " << sr.GetCode());

	double amountCFGSRNet = CSAmSrAmountType::GetInstance("CFG S/R Net")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRNominal = CSAmSrAmountType::GetInstance("CFG S/R Nominal")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRRevenueRegulation = CSAmSrAmountType::GetInstance("CFG S/R Revenue Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRIncomeRegulation = CSAmSrAmountType::GetInstance("CFG S/R Income Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRExpenseRegulation = CSAmSrAmountType::GetInstance("CFG S/R Expense Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRRANRegulation = CSAmSrAmountType::GetInstance("CFG S/R R.A.N. Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRRANRoundingRegulation = CSAmSrAmountType::GetInstance("CFG S/R R.A.N. Rounding Regulation")->GetSRAmount(sr, thirdParty, currency);
	double amountCFGSRFees = CSAmSrAmountType::GetInstance("CFG S/R Fees")->GetSRAmount(sr, thirdParty, currency);

	//amountRet = -abs(amountCFGSRNet) + abs(amountCFGSRNominal) - amountCFGSRRevenueRegulation - amountCFGSRIncomeRegulation + amountCFGSRExpenseRegulation - amountCFGSRRANRegulation - amountCFGSRRANRoundingRegulation + abs(amountCFGSRFees);
	//amountRet = amountCFGSRNet + abs(amountCFGSRNominal) + amountCFGSRRevenueRegulation + amountCFGSRIncomeRegulation + amountCFGSRExpenseRegulation + amountCFGSRRANRegulation + amountCFGSRRANRoundingRegulation - abs(amountCFGSRFees);
	amountRet = amountCFGSRNet + abs(amountCFGSRNominal) - amountCFGSRRevenueRegulation - amountCFGSRIncomeRegulation - amountCFGSRExpenseRegulation - amountCFGSRRANRegulation - amountCFGSRRANRoundingRegulation - abs(amountCFGSRFees);
	MESS(Log::debug, "CFGRedemptionBalance Amount = " << amountRet);

	END_LOG();
	amountRet = GetRoundedValue(2,amountRet);
	return amountRet;

}

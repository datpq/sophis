#pragma warning(disable:4251)

#include <stdio.h>
#include "SphTools/SphLoggerUtil.h"
#include "SphLLInc/portfolio/SphFolioStructures.h"
#include "SphInc/portfolio/SphPortfolioColumn.h"
#include "SphInc/portfolio/SphPosition.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc/SphRiskApi.h"
#include "SphInc/gui/SphDialog.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/instrument/SphBond.h"
#include "SphInc/market_data/SphYieldCurve.h"
#include "SphInc/static_data/SphYieldCurveFamily.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/static_data/SphDayCountBasis.h"
#include "SphInc/static_data/SphYieldCalculation.h"

#include "../../CFG_FixedIncome/Resource/resource.h"
#include "CSxMarketDataOverloader.h"
#include "CSxBondPricer.h"
#include "UpgradeExtension.h"


using namespace sophis::tools;
using namespace  sophis::instrument;
using namespace sophis::market_data;
using namespace sophis::static_data;


/*static*/ const char* CSxBondPricer::__CLASS__ = "CSxBondPricer";
/*static*/ CSxBondPricer* CSxBondPricer::fInstance = NULL;


CSxBondPricer::CSxBondPricer()
{
}

/*static*/ CSxBondPricer* CSxBondPricer::GetInstance()
{
	if (!fInstance)
	{
		fInstance = new CSxBondPricer();
	}

	return fInstance;
}

double CSxBondPricer::GetSensitivity(long instrumentCode, const char * sensitivityUserColumnName)
{
	BEGIN_LOG("GetSensitivity");

	double ret = 0.;
	
	try
	{
		const CSRPortfolioColumn * sensitivityUserColumn = CSRPortfolioColumn::GetCSRPortfolioColumn(sensitivityUserColumnName);

		if (sensitivityUserColumn)
		{
			TViewMvts viewMvt;
			viewMvt.sicovam = instrumentCode;			
			viewMvt.type_ligne = ePositionType::pStandard;
			//DPH
			//TODO method .initialise() does not exist anymore
			//viewMvt.initialise();
			CSRPosition pos(&viewMvt);						
	
			const CSRPortfolio* rootFolio = CSRPortfolio::GetRootPortfolio();
			//DPH
			//const CSRExtraction * extraction = NULL;
			PSRExtraction extraction;
			long rootFolioCode = 0;

			if (rootFolio)
			{
				rootFolioCode = rootFolio->GetCode();
				extraction = rootFolio->GetExtraction();

				if (!rootFolio->IsLoaded())
					rootFolio->Load();

				rootFolio->Compute();								
			}

			SSCellValue cellValue;
			SSCellStyle cellStyle;

			sensitivityUserColumn->GetPositionCell(pos,
				rootFolioCode,
				rootFolioCode,
				extraction,
				instrumentCode,
				instrumentCode,				
				&cellValue,
				&cellStyle,
				true);

			ret = cellValue.floatValue;	

			char mess[SIZE_MESSAGE];
			sprintf_s(mess,SIZE_MESSAGE,"Sensitivity User Column (%s) for instrument (%ld) is : %lf", sensitivityUserColumnName,instrumentCode,ret);
			MESS(Log::debug, mess);								
		}				
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		char mess[SIZE_MESSAGE];
		sprintf_s(mess,SIZE_MESSAGE,"Error (%s)  while calling \"GetSensitivity\" method with parameters(%ld,%s)", 
																FROM_STREAM(ex),instrumentCode,sensitivityUserColumnName);
		MESS(Log::error, mess);

		if (CSRApi::IsInBatchMode() == false)
		{
			CSRFitDialog::Message(mess);
		}
	}
	catch(...)
	{		
		char mess[SIZE_MESSAGE];
		sprintf_s(mess,SIZE_MESSAGE,"Unhandled exception occured while calling \"GetSensitivity\" method with parameters(%ld,%s)",
																				instrumentCode,sensitivityUserColumnName);
		MESS(Log::error, mess);

		if (CSRApi::IsInBatchMode() == false)
		{
			CSRFitDialog::Message(mess);
		}
	}

	END_LOG();

	return ret;
}

void CSxBondPricer::ComputeTheoretical(	const char * instrRef, 
										long date, 
										const char * curveFamilyName, 
										int pricingModel, 
										int quotationType, 
										double spread,
										_STL::vector<_STL::string> & maturitiesList, 
										_STL::vector<double> & ratesList, 
										double & price, 
										double & ytm, 
										double & accrued,
										double & duration, 
										double & sensitivity)
{
	BEGIN_LOG("ComputeTheoretical");	

	price = 0;
	ytm = 0;
	accrued = 0;
	duration = 0;
	sensitivity = 0;	
	
	//Check instrument reference
	const CSRInstrument* instr = CSRInstrument::GetInstance(CSRInstrument::GetCodeWithReference(instrRef));
	if (!instr)
	{
		_STL::string mess = FROM_STREAM("Instrument reference not found (" << instrRef << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException; 
	}

	//Check that instrument is a bond
	const CSRBond* bond = dynamic_cast<const CSRBond*>(instr);
	if (!bond)
	{
		_STL::string mess = FROM_STREAM("Instrument reference parameter (" << instrRef << ") is not a bond");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	//Clone the bond
	CSRBond* bondClone = dynamic_cast<CSRBond*>(bond->Clone_API());

	if (!bondClone)
	{
		_STL::string mess = FROM_STREAM("Failed to clone the bond (" << instrRef << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}			
	
	//Check currency
	long ccyCode = bondClone->GetCurrencyCode();

	if (!ccyCode)
	{
		_STL::string mess = FROM_STREAM("The currency is not defined for bond (" << instrRef << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	//set yield curve family
	long ycCurveFamilyCode = 0;

	const CSRYieldCurveFamily * ycFamily = NULL;
	char localYieldCurveFamilyName[100] = "";
	if (strcmp(curveFamilyName, "") == 0)
	{
		MESS(Log::debug, "Force the default family to 'COURBE BDT BAM'");
		strcpy_s(localYieldCurveFamilyName, 100, "COURBE BDT BAM");
	}
	else
	{
		MESS(Log::debug, "Use the parameter family to '" << curveFamilyName << "'");
		strcpy_s(localYieldCurveFamilyName, 100, curveFamilyName);
	}

	bool forceAlwaysLongTerm = false;
	if (strcmp(localYieldCurveFamilyName, "COURBE ZERO COUPON") == 0)
	{
		MESS(Log::debug, "Zero Coupon Curve => Force Long Term");
		forceAlwaysLongTerm = true;
	}
	
	ycCurveFamilyCode = CSRYieldCurveFamily::GetYieldCurveFamilyCode(ccyCode, localYieldCurveFamilyName);
	ycFamily = CSRYieldCurveFamily::GetCSRYieldCurveFamily(ycCurveFamilyCode);
	if (!ycFamily)
	{
		_STL::string mess = FROM_STREAM("Curve family parameter (" << localYieldCurveFamilyName << ") is not valid for currency (" << ccyCode << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	bondClone->SetFamily(localYieldCurveFamilyName);
			
	// Set pricing model
	char modelName[41];
	strcpy_s(modelName, 41, "");

	switch(pricingModel)
	{
	case 0: // 0 is the default value. Take instrument model name		
		break;

	case 1:		
		strcpy_s(modelName, 41, "Titre MAD remboursable in fine");
		bondClone->SetModelName(modelName);
		break;

	case 2:		
		strcpy_s(modelName, 41, "Titre MAD amortissable");
		bondClone->SetModelName(modelName);
		break;

	case 3:		
		strcpy_s(modelName, 41, "MtoM + Greeks MtoM");
		bondClone->SetModelName(modelName);
		bondClone->SetPricingMethod(eptMtM_Greeks_MtM);
		break;

	case 4:		
		strcpy_s(modelName, 41, "MtoM + Greeks Theo");
		bondClone->SetModelName(modelName);
		bondClone->SetPricingMethod(eptMtM_Greeks_Theo);
		break;

	case 5:		
		strcpy_s(modelName, 41, "Standard");
		bondClone->SetModelName(modelName);		
		break;

	default:
		{
			_STL::string mess = FROM_STREAM("Model parameter (" << pricingModel << ") is not valid");
			RunTimeFailure runTimeFailureException(mess.c_str());			
			throw runTimeFailureException;
		}			
	}

	//When one changes the pricing model, one should clone the bond in order to get a CSRBond derived class object that matches the model
	CSRBond* bondClone2 = dynamic_cast<CSRBond*>(bondClone->Clone_API());

	if (!bondClone2)
	{
		_STL::string mess = FROM_STREAM("Failed to clone the bond with model name(" << modelName << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	//set quotation type	

	switch(quotationType)
	{
	case 0: // 0 is the default value. Take instrument quotation type		
		break;

	case 1:
		bondClone2->SetQuotationType(aqInPrice);
		break;

	case 2:
		bondClone2->SetQuotationType(aqInPercentage);		
		break;
	
	case 3:
		bondClone2->SetQuotationType(aqInPercentWithAccrued);		
		break;

	default:
		{
			_STL::string mess = FROM_STREAM("Quotation type parameter (" << quotationType << ") is not valid");
			RunTimeFailure runTimeFailureException(mess.c_str());			
			throw runTimeFailureException;
		}
	}

	//Set CFG risk spread (toolkit field)	
	bondClone2->SaveGeneralElement(IDC_RISK_SPREAD - ID_ITEM_SHIFT,&spread);	

	//Set prices date
	const CSRMarketData* currentMarketData = CSRMarketData::GetCurrentMarketData();
	if (!currentMarketData)
	{
		_STL::string mess = FROM_STREAM("Current market data is null");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	long currentDate = currentMarketData->GetDate();

	if (date != currentDate)
	{						
		SetPricesDate(date);
	}
	
	long settlementDate = bondClone2->GetSettlementDate(date);
	long ownershipDate = bondClone2->GetPariPassuDate(date,settlementDate);	
	long accruedCouponDate = bondClone2->GetAccruedCouponDate(date,settlementDate);

	// Search the basis and yield calculation from the yield curve long term rate
	const CSRYieldCurve * currentYieldCurve =  CSRYieldCurve::GetInstanceByYieldCurveFamily(ycCurveFamilyCode);
	if (!currentYieldCurve)
	{
		_STL::string mess = FROM_STREAM("Failed to find default yield curve for family " << ycCurveFamilyCode);
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}
	const SSYieldCurve * yieldCurveData = currentYieldCurve->GetSSYieldCurve();
	if (!yieldCurveData)
	{
		_STL::string mess = FROM_STREAM("Failed to find default yield curve data");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;		
	}
	const CSRInterestRate * longTermRate = dynamic_cast<const CSRInterestRate *>(CSRInstrument::GetInstance(yieldCurveData->fLongTermRate));
	if (!longTermRate)
	{
		_STL::string mess = FROM_STREAM("Failed to find long term rate " << yieldCurveData->fLongTermRate);
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;	
	}
	eDayCountBasisType dayCountBasisType = longTermRate->GetDayCountBasisType();
	eYieldCalculationType yieldCalculationType = longTermRate->GetYieldCalculationType();
	MESS(Log::debug, "Day Count Basis " << dayCountBasisType << ", Yield Calculation Type " << yieldCalculationType);
	const CSRDayCountBasis * p_basis = CSRDayCountBasis::GetCSRDayCountBasis(dayCountBasisType);
	const CSRYieldCalculation * p_mode = CSRYieldCalculation::GetCSRYieldCalculation(yieldCalculationType);

	bool isAdjustedDatesCalc = bondClone2->GetMarketCalculationYTMOnAdjustedDates();	
	bool isValueDatesCalc = bondClone2->GetMarketCalculationYTMOnSettlementDate();	

	if (bondClone2->GetPricingMethod()==eptMtM_Greeks_MtM)
	{
		//DPH
		//_STL::auto_ptr<sophis::instrument::CSRMarketCategoryHandle> marketCategoryHandle;
		std::unique_ptr<sophis::instrument::CSRMarketCategoryHandle> marketCategoryHandle;
		bondClone2->ActivateMarketCategory(marketCategoryHandle);
		//DPH 733
		//double mtmSpread = bondClone2->ComputeMtMSpread();
		double mtmSpread = CSRBond::ComputeMtMSpreadForPricing(bondClone2);
		bondClone2->ResetMtMSpread(mtmSpread);
	}
	
	//In case of amortizing notional, one must adjust the price with the notional factor
	double floatingNotionalFactor = bondClone2->GetFloatingNotionalFactor(ownershipDate,accruedCouponDate);	
	double factor = 1.;
	eAskQuotationType e_quotationType = bondClone2->GetAskQuotationType();
	if(e_quotationType != aqInPrice && e_quotationType != aqInPriceWithoutAccrued && bondClone2->IsFloatingNotional())
	{
		if (floatingNotionalFactor > 1e-10)
			factor = 1./floatingNotionalFactor;
		else
			factor = 0;
	}	

	//overload ZC yield curve
	long tempShortTermRate = yieldCurveData->fShortTermRate;
	long tempLongTermRate = yieldCurveData->fLongTermRate;
	if (forceAlwaysLongTerm)
	{
		tempShortTermRate = tempLongTermRate;
	}
	CSxMarketDataOverloader marketDataOverloader(*currentMarketData, maturitiesList, ratesList, tempShortTermRate, tempLongTermRate);	
	
	//Dirty price
	double dirtyPrice = 0;
	double sensitivityStd = 0;
	double convexity = 0;
	bondClone2->GetPriceDeltaGammaByZC(	marketDataOverloader,
										&dirtyPrice,
										&sensitivityStd,
										&convexity,
										date,
										settlementDate,
										ownershipDate,
										isAdjustedDatesCalc,
										isValueDatesCalc/*,
										p_basis,
										p_mode*/);

	MESS(Log::debug, "Date " << date << ", Settlement Date " << settlementDate << ", Ownership Date " << ownershipDate << ", Basis " << dayCountBasisType << ", Yield Calculation " << yieldCalculationType);
	dirtyPrice *= factor;
	
	//accrued coupon		
	accrued = bondClone2->GetAccruedCoupon(ownershipDate,accruedCouponDate)*factor;	
	MESS(Log::debug, "Price " << dirtyPrice << ", Accrued " << accrued << ", Factor " << factor);

	//price
	if (bondClone2->GetQuotationType() == aqInPrice || bondClone2->GetQuotationType() == aqInPercentWithAccrued)
	{
		price = dirtyPrice;
	}
	else //aqInPercentage
	{
		price = dirtyPrice - accrued; //take clean price
	}	

	//ytm		

	double dirtyPriceForYTM = dirtyPrice;

	if(e_quotationType != aqInPrice && e_quotationType != aqInPriceWithoutAccrued && bondClone2->IsFloatingNotional())
	{
		dirtyPriceForYTM *= floatingNotionalFactor;
	}

	ytm = bondClone2->GetYTMByDirtyPrice(date,settlementDate,ownershipDate,dirtyPriceForYTM,isAdjustedDatesCalc,isValueDatesCalc,p_basis,
											p_mode,marketDataOverloader);

	//duration	

	duration = bondClone2->GetDurationByZC(marketDataOverloader,date,settlementDate,ownershipDate,p_basis,isAdjustedDatesCalc,isValueDatesCalc,p_basis,p_mode);

	//Sensitivity
	sensitivity = GetSensitivity(bondClone2,dirtyPrice,marketDataOverloader);

	bondClone2->ResetMtMSpread();
	
	delete bondClone2;
	delete bondClone;

	//Restore Sophis date
	if (date != currentDate) 
	{			
		SetPricesDate(currentDate);				
	}
	
	END_LOG();	
}

double CSxBondPricer::GetPriceFromYTM(const char* instrRef, long date, int pricingModel, int quotationType, double ytm)
{
	BEGIN_LOG("GetPriceFromYTM");	

	double price = 0;		

	//Check instrument reference
	const CSRInstrument* instr = CSRInstrument::GetInstance(CSRInstrument::GetCodeWithReference(instrRef));
	if (!instr)
	{
		_STL::string mess = FROM_STREAM("Instrument reference not found (" << instrRef << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException; 
	}

	//Check that instrument is a bond
	const CSRBond* bond = dynamic_cast<const CSRBond*>(instr);
	if (!bond)
	{
		_STL::string mess = FROM_STREAM("Instrument reference parameter (" << instrRef << ") is not a bond");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	//Clone the bond
	CSRBond* bondClone = dynamic_cast<CSRBond*>(bond->Clone_API());

	if (!bondClone)
	{
		_STL::string mess = FROM_STREAM("Failed to clone the bond (" << instrRef << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}			

	//set pricing model

	char modelName[41];
	strcpy_s(modelName, 41, "");

	switch(pricingModel)
	{
	case 0: // 0 is the default value. Take instrument model name		
		break;

	case 1:		
		strcpy_s(modelName, 41, "Titre MAD remboursable in fine");
		bondClone->SetModelName(modelName);
		break;

	case 2:		
		strcpy_s(modelName, 41, "Titre MAD amortissable");
		bondClone->SetModelName(modelName);
		break;

	case 3:		
		strcpy_s(modelName, 41, "MtoM + Greeks MtoM");
		bondClone->SetModelName(modelName);
		bondClone->SetPricingMethod(eptMtM_Greeks_MtM);
		break;

	case 4:		
		strcpy_s(modelName, 41, "MtoM + Greeks Theo");
		bondClone->SetModelName(modelName);
		bondClone->SetPricingMethod(eptMtM_Greeks_Theo);
		break;

	case 5:		
		strcpy_s(modelName, 41, "Standard");
		bondClone->SetModelName(modelName);		
		break;

	default:
		{
			_STL::string mess = FROM_STREAM("Model parameter (" << pricingModel << ") is not valid");
			RunTimeFailure runTimeFailureException(mess.c_str());			
			throw runTimeFailureException;
		}			
	}

	//When one changes the pricing model, one should clone the bond in order to get a CSRBond derived class object that matches the model
	CSRBond* bondClone2 = dynamic_cast<CSRBond*>(bondClone->Clone_API());

	if (!bondClone2)
	{
		_STL::string mess = FROM_STREAM("Failed to clone the bond with model name(" << modelName << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	//set quotation type	

	switch(quotationType)
	{
	case 0: // 0 is the default value. Take instrument quotation type		
		break;

	case 1:
		bondClone2->SetQuotationType(aqInPrice);
		break;

	case 2:
		bondClone2->SetQuotationType(aqInPercentage);		
		break;

	case 3:
		bondClone2->SetQuotationType(aqInPercentWithAccrued);		
		break;

	default:
		{
			_STL::string mess = FROM_STREAM("Quotation type parameter (" << quotationType << ") is not valid");
			RunTimeFailure runTimeFailureException(mess.c_str());			
			throw runTimeFailureException;
		}
	}

	//Set ytm	
	//DPH
	//bondClone2->SetYTM(ytm);
	UpgradeExtension::SetYTM(bondClone2, ytm);

	//Set prices date
	const CSRMarketData* currentMarketData = CSRMarketData::GetCurrentMarketData();
	if (!currentMarketData)
	{
		_STL::string mess = FROM_STREAM("Current market data is null");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	long currentDate = currentMarketData->GetDate();

	if (date != currentDate)
	{						
		SetPricesDate(date);
	}

	long settlementDate = bondClone2->GetSettlementDate(date);
	long ownershipDate = bondClone2->GetPariPassuDate(date,settlementDate);
	long accruedCouponDate = bondClone2->GetAccruedCouponDate(date,settlementDate);
	const CSRDayCountBasis* dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis(bondClone2->GetMarketYTMDayCountBasisType());
	const CSRYieldCalculation	*yieldCalculation = CSRYieldCalculation::GetCSRYieldCalculation(bondClone2->GetMarketYTMYieldCalculationType());
	bool isAdjustedDatesCalc = bondClone2->GetMarketCalculationYTMOnAdjustedDates();
	bool isValueDatesCalc = bondClone2->GetMarketCalculationYTMOnSettlementDate();
	double dirtyPrice = 0;

	if (bondClone2->GetPricingMethod()==eptMtM_Greeks_MtM)
	{
		//DPH
		//_STL::auto_ptr<sophis::instrument::CSRMarketCategoryHandle> marketCategoryHandle;
		std::unique_ptr<sophis::instrument::CSRMarketCategoryHandle> marketCategoryHandle;
		bondClone2->ActivateMarketCategory(marketCategoryHandle);
		//DPH 733
		//double mtmSpread = bondClone2->ComputeMtMSpread();
		double mtmSpread = CSRBond::ComputeMtMSpreadForPricing(bondClone2);
		bondClone2->ResetMtMSpread(mtmSpread);
	}

	//In case of amortizing notional, one must adjust the price with the notional factor
	double floatingNotionalFactor = bondClone2->GetFloatingNotionalFactor(ownershipDate,accruedCouponDate);	
	double factor = 1.;
	eAskQuotationType e_quotationType = bondClone2->GetAskQuotationType();
	if(e_quotationType != aqInPrice && e_quotationType != aqInPriceWithoutAccrued && bondClone2->IsFloatingNotional())
	{
		if (floatingNotionalFactor > 1e-10)
			factor = 1./floatingNotionalFactor;
		else
			factor = 0;
	}	

	//Dirty price    			

	dirtyPrice = bondClone2->GetDirtyPriceByYTM(date,settlementDate,ownershipDate,ytm,isAdjustedDatesCalc,isValueDatesCalc,dayCountBasis,
														yieldCalculation,*gApplicationContext)*factor;

	//accrued coupon		
	double accruedCoupon = bondClone2->GetAccruedCoupon(ownershipDate,accruedCouponDate) * factor;

	//price
	if (bondClone2->GetQuotationType() == aqInPrice || bondClone2->GetQuotationType() == aqInPercentWithAccrued)
	{
		price = dirtyPrice;
	}
	else //aqInPercentage
	{
		price = dirtyPrice - accruedCoupon; //take clean price
	}

	bondClone2->ResetMtMSpread();

	delete bondClone2;
	delete bondClone;

	//Restore Sophis date
	if (date != currentDate) 
	{			
		SetPricesDate(currentDate);				
	}

	END_LOG();

	return price;
}

double CSxBondPricer::GetYTMFromPrice(const char* instrRef, long date, int pricingModel, int quotationType, double price)
{
	BEGIN_LOG("GetYTMFromPrice");	

	double ytm = 0;		

	//Check instrument reference
	const CSRInstrument* instr = CSRInstrument::GetInstance(CSRInstrument::GetCodeWithReference(instrRef));
	if (!instr)
	{
		_STL::string mess = FROM_STREAM("Instrument reference not found (" << instrRef << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException; 
	}

	//Check that instrument is a bond
	const CSRBond* bond = dynamic_cast<const CSRBond*>(instr);
	if (!bond)
	{
		_STL::string mess = FROM_STREAM("Instrument reference parameter (" << instrRef << ") is not a bond");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	//Clone the bond
	CSRBond* bondClone = dynamic_cast<CSRBond*>(bond->Clone_API());

	if (!bondClone)
	{
		_STL::string mess = FROM_STREAM("Failed to clone the bond (" << instrRef << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}			

	//set pricing model

	char modelName[41];
	strcpy_s(modelName, 41, "");

	switch(pricingModel)
	{
	case 0: // 0 is the default value. Take instrument model name		
		break;

	case 1:		
		strcpy_s(modelName, 41, "Titre MAD remboursable in fine");
		bondClone->SetModelName(modelName);
		break;

	case 2:		
		strcpy_s(modelName, 41, "Titre MAD amortissable");
		bondClone->SetModelName(modelName);
		break;

	case 3:		
		strcpy_s(modelName, 41, "MtoM + Greeks MtoM");
		bondClone->SetModelName(modelName);
		bondClone->SetPricingMethod(eptMtM_Greeks_MtM);
		break;

	case 4:		
		strcpy_s(modelName, 41, "MtoM + Greeks Theo");
		bondClone->SetModelName(modelName);
		bondClone->SetPricingMethod(eptMtM_Greeks_Theo);
		break;

	case 5:		
		strcpy_s(modelName, 41, "Standard");
		bondClone->SetModelName(modelName);		
		break;

	default:
		{
			_STL::string mess = FROM_STREAM("Model parameter (" << pricingModel << ") is not valid");
			RunTimeFailure runTimeFailureException(mess.c_str());			
			throw runTimeFailureException;
		}			
	}

	//When one changes the pricing model, one should clone the bond in order to get a CSRBond derived class object that matches the model
	CSRBond* bondClone2 = dynamic_cast<CSRBond*>(bondClone->Clone_API());

	if (!bondClone2)
	{
		_STL::string mess = FROM_STREAM("Failed to clone the bond with model name(" << modelName << ")");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	//set quotation type	

	switch(quotationType)
	{
	case 0: // 0 is the default value. Take instrument quotation type		
		break;

	case 1:
		bondClone2->SetQuotationType(aqInPrice);
		break;

	case 2:
		bondClone2->SetQuotationType(aqInPercentage);		
		break;

	case 3:
		bondClone2->SetQuotationType(aqInPercentWithAccrued);		
		break;

	default:
		{
			_STL::string mess = FROM_STREAM("Quotation type parameter (" << quotationType << ") is not valid");
			RunTimeFailure runTimeFailureException(mess.c_str());			
			throw runTimeFailureException;
		}
	}	

	//Set prices date
	const CSRMarketData* currentMarketData = CSRMarketData::GetCurrentMarketData();
	if (!currentMarketData)
	{
		_STL::string mess = FROM_STREAM("Current market data is null");
		RunTimeFailure runTimeFailureException(mess.c_str());			
		throw runTimeFailureException;
	}

	long currentDate = currentMarketData->GetDate();

	if (date != currentDate)
	{						
		SetPricesDate(date);
	}

	long settlementDate = bondClone2->GetSettlementDate(date);
	long ownershipDate = bondClone2->GetPariPassuDate(date,settlementDate);
	long accruedCouponDate = bondClone2->GetAccruedCouponDate(date,settlementDate);
	const CSRDayCountBasis* dayCountBasis = CSRDayCountBasis::GetCSRDayCountBasis(bondClone2->GetMarketYTMDayCountBasisType());
	const CSRYieldCalculation	*yieldCalculation = CSRYieldCalculation::GetCSRYieldCalculation(bondClone2->GetMarketYTMYieldCalculationType());
	bool isAdjustedDatesCalc = bondClone2->GetMarketCalculationYTMOnAdjustedDates();
	bool isValueDatesCalc = bondClone2->GetMarketCalculationYTMOnSettlementDate();	

	if (bondClone2->GetPricingMethod()==eptMtM_Greeks_MtM)
	{
		//DPH
		//_STL::auto_ptr<sophis::instrument::CSRMarketCategoryHandle> marketCategoryHandle;
		std::unique_ptr<sophis::instrument::CSRMarketCategoryHandle> marketCategoryHandle;
		bondClone2->ActivateMarketCategory(marketCategoryHandle);
		//DPH 733
		//double mtmSpread = bondClone2->ComputeMtMSpread();
		double mtmSpread = CSRBond::ComputeMtMSpreadForPricing(bondClone2);
		bondClone2->ResetMtMSpread(mtmSpread);
	}		

	//accrued coupon		
	double accruedCoupon = bondClone2->GetAccruedCoupon(ownershipDate,accruedCouponDate);

	//dirty price
	double dirtyPrice = price;
	
	//In case of amortizing notional, one must adjust the price with the notional factor
	double floatingNotionalFactor = bondClone2->GetFloatingNotionalFactor(ownershipDate,accruedCouponDate);	
	eAskQuotationType e_quotationType = bondClone2->GetAskQuotationType();

	if(e_quotationType != aqInPrice && e_quotationType != aqInPriceWithoutAccrued && bondClone2->IsFloatingNotional())
	{
		dirtyPrice *= floatingNotionalFactor;
	}
	
	if (e_quotationType == aqInPercentage || e_quotationType == aqInPriceWithoutAccrued)
	{
		dirtyPrice += accruedCoupon;
	}	

	//YTM    			

	ytm = bondClone2->GetYTMByDirtyPrice(date,settlementDate,ownershipDate,dirtyPrice,isAdjustedDatesCalc,isValueDatesCalc,dayCountBasis,
																										yieldCalculation,*gApplicationContext);	

	bondClone2->ResetMtMSpread();

	delete bondClone2;
	delete bondClone;

	//Restore Sophis date
	if (date != currentDate) 
	{			
		SetPricesDate(currentDate);				
	}

	END_LOG();

	return ytm;
}

//DPH
//double CSxBondPricer::GetSensitivity(const CSRInstrument* instr, double dirtyPrice, const CSRMarketData& context)
double CSxBondPricer::GetSensitivity(const ISRInstrument* instr, double dirtyPrice, const CSRMarketData& context)
{
	double ret = 0;	

	if (instr && dirtyPrice)
	{
		//DPH
		//if(instr->GetRhoCount() > 0)
		if (UpgradeExtension::GetRhoCount(instr) > 0)
		{
			//DPH
			//double rho = instr->GetRho(context,0);
			double rho = UpgradeExtension::GetRho(instr, context, 0);
			ret = -rho/dirtyPrice;				
		}
	}

	return ret;
}

void CSxBondPricer::SetPricesDate(long date)
{
	BEGIN_LOG("SetPricesDate");	

	MESS(Log::debug, "Set prices dates to " << date); 				

	CSRMarketData::SSDates ssdate;

	ssdate.fCalculation = date;
	ssdate.fSpot = date;
	ssdate.fForex = date;
	//ssdate.fInstrument = date;	
	ssdate.fRate = date;
	ssdate.fRepoCost = date;
	ssdate.fUseHistoricalFairValue = false;

	ssdate.UseIt();

	END_LOG();
}
#pragma warning(disable:4251)
#include "SphInc/SphMacros.h"
#include "SphInc/value/kernel/SphFund.h"
#include "SphInc/gui/SphDialog.h"
#include "SphInc/backoffice_kernel/SphThirdParty.h"
#include __STL_INCLUDE_PATH(vector)
#include __STL_INCLUDE_PATH(string)
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/backoffice_kernel/SphBusinessEvent.h"
#include "SphInc/value/kernel/SphFundPurchase.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphTools/SphDay.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/portfolio/SphPosition.h"
#include "SphInc/portfolio/SphTransactionVector.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/backoffice_kernel/SphKernelStatusGroup.h"


#include "CSxCustomMenu.h"
#include "Constants.h"
#include "../../CFG_ManagementFees/Source/CSxManagementFees.h"
#include "CSxRetrocessionFees.h"
#include "CSxTools.h"

using namespace sophis::value;
using namespace sophis::gui;
using namespace sophis::backoffice_kernel;
using namespace sophis::sql;
using namespace sophis::portfolio;
using namespace  sophis::misc;

/*static*/ const char* CSxRetrocessionFees::__CLASS__ = "CSxRetrocessionFees";


CSxRetrocessionFees::CSxRetrocessionFees()
{
	ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES","ManagementFeesSteGestionBEName", fManagementFeesSteGestionBEName, "Provision Frais SteGestion");
	ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES","ManagementFeesCDVMBEName", fManagementFeesCDVMBEName, "Provision Frais CDVM");
	ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES","ManagementFeesDDGBEName", fManagementFeesDDGBEName, "Provision Frais DDG");
	ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES","ManagementFeesMCLBEName", fManagementFeesMCLBEName, "Provision Frais MCL");
	ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES","RetrocessionPromoterFeesBEName", fRetrocessionPromoterFeesBEName, "Provision Retrocession Promo");
}

CSxRetrocessionFees::~CSxRetrocessionFees()
{
}

void CSxRetrocessionFees::GetData(_STL::vector<long> &fundList)
{
	fRetrocessionFeesList.clear();	

	SSxRetrocessionFeesData *resultBuffer = NULL;
	int		 nbResults = 0;

	CSRStructureDescriptor	desc(14, sizeof(SSxRetrocessionFeesData));

	ADD(&desc, SSxRetrocessionFeesData, fThirdId, rdfInteger);
	ADD(&desc, SSxRetrocessionFeesData, fThirdType, rdfInteger);
	ADD(&desc, SSxRetrocessionFeesData, fFundCode, rdfInteger);
	ADD(&desc, SSxRetrocessionFeesData, fComputationMethod, rdfInteger);
	ADD(&desc, SSxRetrocessionFeesData, fStartDate, rdfInteger);
	ADD(&desc, SSxRetrocessionFeesData, fEndDate, rdfInteger);
	ADD(&desc, SSxRetrocessionFeesData, fCommissionRate, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fLevel1, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fRetrocessionRate1, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fLevel2, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fRetrocessionRate2, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fLevel3, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fRetrocessionRate3, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fLevel4, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fRetrocessionRate4, rdfFloat);
	ADD(&desc, SSxRetrocessionFeesData, fTVARate, rdfFloat);
	

	_STL::string sqlQuery = "select BUSINESS_PARTNER_ID,BUSINESS_PARTNER_TYPE,FUND_ID,COMPUTATION_METHOD,date_to_num(START_DATE), " 
									" date_to_num(END_DATE),COMMISSION_RATE,LEVEL_1,RETROCESSION_RATE_1,LEVEL_2,RETROCESSION_RATE_2,LEVEL_3, " 
									" RETROCESSION_RATE_3, LEVEL_4, RETROCESSION_RATE_4, TVA_RATE from CFG_RETRO_FEES_PARAMETERS ";		

	if (fundList.size() > 0)
	{
		sqlQuery += FROM_STREAM("where FUND_ID in (" << fundList[0]);
		for (unsigned int i = 1; i < fundList.size(); i++)
		{
			sqlQuery += FROM_STREAM("," << fundList[i]);
		}

		sqlQuery += ")";
	}

	sqlQuery += " order by FUND_ID"; // Compute fund promoters first in order to be able to compute the PNB
	
	//DPH
	//CSRSqlQuery::QueryWithNResults(sqlQuery.c_str(), &desc, (void **) &resultBuffer, &nbResults);
	CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery.c_str(), &desc, (void **)&resultBuffer, &nbResults);


	for (int i = 0; i < nbResults ; i++)
	{
		fRetrocessionFeesList.push_back(resultBuffer[i]);
	}

	free((char*)resultBuffer);
}

bool CSxRetrocessionFees::SaveData()
{	
	BEGIN_LOG("SaveData");
	
	bool ret = true;
	
	ret = CheckData();

	if (ret)
	{
		try
		{
			//DPH
			//CSRSqlQuery::QueryWithoutResult("delete from CFG_RETRO_FEES_PARAMETERS");
			CSRSqlQuery::QueryWithoutResultAndParam("delete from CFG_RETRO_FEES_PARAMETERS");

			for (unsigned int i = 0; i < fRetrocessionFeesList.size(); i++)
			{
				char query[QUERY_BUFFER_SIZE];
				sprintf_s(query,QUERY_BUFFER_SIZE,"insert into CFG_RETRO_FEES_PARAMETERS(BUSINESS_PARTNER_ID,BUSINESS_PARTNER_TYPE,FUND_ID,COMPUTATION_METHOD,START_DATE,END_DATE,"
					"COMMISSION_RATE,LEVEL_1,RETROCESSION_RATE_1,LEVEL_2,RETROCESSION_RATE_2,LEVEL_3,RETROCESSION_RATE_3,LEVEL_4,RETROCESSION_RATE_4,TVA_RATE) "
					" values (%ld,%ld,%ld,%ld,num_to_date(%ld),num_to_date(%ld),%lf,%lf,%lf,%lf,%lf,%lf,%lf,%lf,%lf,%lf)",
					fRetrocessionFeesList[i].fThirdId,fRetrocessionFeesList[i].fThirdType,fRetrocessionFeesList[i].fFundCode,fRetrocessionFeesList[i].fComputationMethod,
					fRetrocessionFeesList[i].fStartDate,fRetrocessionFeesList[i].fEndDate,fRetrocessionFeesList[i].fCommissionRate,fRetrocessionFeesList[i].fLevel1,
					fRetrocessionFeesList[i].fRetrocessionRate1,fRetrocessionFeesList[i].fLevel2,fRetrocessionFeesList[i].fRetrocessionRate2,fRetrocessionFeesList[i].fLevel3,fRetrocessionFeesList[i].fRetrocessionRate3,
					fRetrocessionFeesList[i].fLevel4,fRetrocessionFeesList[i].fRetrocessionRate4,fRetrocessionFeesList[i].fTVARate);

				//DPH
				//CSRSqlQuery::QueryWithoutResult(query);		
				CSRSqlQuery::QueryWithoutResultAndParam(query);
			}

			CSRSqlQuery::Commit();
		}
		catch(sophisTools::base::ExceptionBase &ex)
		{
			char mess[1024];
			sprintf_s(mess, 1024,"ERROR (%s) while saving Retrocession fees configuration", FROM_STREAM(ex));
			CSRFitDialog::Message(mess);
			//DPH
			//MESS(Log::business_error, mess);
			MESS(Log::error, mess);

			CSRSqlQuery::RollBack();
		}
		catch(...)
		{		
			char mess[1024];
			sprintf_s(mess, 1024,"Unhandled exception occured while saving Retrocession fees configuration");
			CSRFitDialog::Message(mess);
			//DPH
			//MESS(Log::business_error, mess);
			MESS(Log::error, mess);

			CSRSqlQuery::RollBack();
		}				
	}		
	
	END_LOG();

	return ret;
}

bool CSxRetrocessionFees::CheckData()
{
	bool ret = true;

	_STL::map<RetrocessionFeesKey,_STL::vector<SSxRetrocessionFeesData>,CompareRetrocessionFees> retrocessionFeesMap;

	for (unsigned int i = 0; i < fRetrocessionFeesList.size(); i++)
	{
		//Check data

		// 1- Check that the third party is defined as a business partner in that fund
		const CSAMFund* fund = dynamic_cast<const CSAMFund*>(CSRInstrument::GetInstance(fRetrocessionFeesList[i].fFundCode));

		if (fund)
		{
			if (fund->IsInternalFund(fRetrocessionFeesList[i].fFundCode) && fund->IsBusinessPartnerValid(fRetrocessionFeesList[i].fThirdId))
			{
				if (fRetrocessionFeesList[i].fThirdType == CSxThirdTypeMenu::eFundPromoter)
				{
					//2- Check if the third party is a real fund promoter
					const CSRThirdParty* thirdParty = CSRThirdParty::GetCSRThirdParty(fRetrocessionFeesList[i].fThirdId);
					if (thirdParty)
					{
						//Get the 'PROMOTEUR DE FOND' property
						char buffer[256];
						thirdParty->GetProperty("PROMOTEUR DE FOND",buffer,256);
						if (_stricmp(buffer,"yes"))
						{
							char mess[256];
							sprintf_s(mess,256,"Line %d : The business partner is not a fund promoter",i+1);
							CSRFitDialog::Message(mess);
							ret = false;
							break;
						}
					}
				}
			}
			else
			{
				char mess[256];
				sprintf_s(mess,256,"Line %d : The business partner is not defined at the fund level",i+1);
				CSRFitDialog::Message(mess);
				ret = false;
				break;
			}
		}

		////3- Check dates are filled
		//if (!fRetrocessionFeesList[i].fStartDate)
		//{
		//	char mess[256];
		//	sprintf_s(mess,256,"Line %d : The start date should be filled",i+1);
		//	CSRFitDialog::Message(mess);
		//	ret = false;
		//	break;
		//}

		//if (!fRetrocessionFeesList[i].fEndDate)
		//{
		//	char mess[256];
		//	sprintf_s(mess,256,"Line %d : The end date should be filled",i+1);
		//	CSRFitDialog::Message(mess);
		//	ret = false;
		//	break;
		//}		

		//4- Check that the periods do not overlap
		if (fRetrocessionFeesList[i].fStartDate > fRetrocessionFeesList[i].fEndDate && fRetrocessionFeesList[i].fEndDate != 0)
		{
			char mess[256];
			sprintf_s(mess,256,"Line %d : The start date should be before the end date",i+1);
			CSRFitDialog::Message(mess);
			ret = false;
			break;
		}

		RetrocessionFeesKey key(fRetrocessionFeesList[i].fThirdId,fRetrocessionFeesList[i].fThirdType,fRetrocessionFeesList[i].fFundCode);
		_STL::map<RetrocessionFeesKey,_STL::vector<SSxRetrocessionFeesData>,CompareRetrocessionFees>::iterator iter = retrocessionFeesMap.find(key);
		if (iter == retrocessionFeesMap.end())
		{			
			_STL::vector<SSxRetrocessionFeesData> retrocessionFeesVector;
			retrocessionFeesVector.push_back(fRetrocessionFeesList[i]);
			retrocessionFeesMap[key] = retrocessionFeesVector;
		}
		else
		{
			for (unsigned int j = 0; j < retrocessionFeesMap[key].size(); j++)
			{
				if (fRetrocessionFeesList[i].fStartDate >= (retrocessionFeesMap[key])[j].fStartDate && fRetrocessionFeesList[i].fStartDate < (retrocessionFeesMap[key])[j].fEndDate)
				{
					char mess[256];
					sprintf_s(mess,256,"Line %d : The periods should not overlap",i+1);
					CSRFitDialog::Message(mess);
					ret = false;
					break;
				}
			}

			if (!ret)
				break;

			retrocessionFeesMap[key].push_back(fRetrocessionFeesList[i]);
		}
	}

	return ret;
}

void CSxRetrocessionFees::Compute(_STL::vector<long>& fundList, long startDate, long endDate)
{
	BEGIN_LOG("Compute");	
	
	//Load Retrocession fees from DB
	GetData(fundList);
	
	long currentDate = gApplicationContext->GetDate();

	for (unsigned int i = 0; i < fRetrocessionFeesList.size(); i++)
	{
		ComputeFeesForOneData(fRetrocessionFeesList[i],startDate,endDate);
	}	

	END_LOG();
}

void CSxRetrocessionFees::ComputeFeesForOneData(SSxRetrocessionFeesData oneFeesData, long startDate, long endDate)
{
	BEGIN_LOG("ComputeFeesForOneData");
	
	//Check if computation date is in validity range
	if ((startDate < oneFeesData.fStartDate && oneFeesData.fStartDate > 0) || (endDate > oneFeesData.fEndDate && oneFeesData.fEndDate > 0))
	{		
		MESS(Log::info, FROM_STREAM("Retrocession fees for Fund (" << oneFeesData.fFundCode << ") , Business partner (" << oneFeesData.fThirdId << ") , Business partner type ("
			    << CSxThirdTypeMenu::GetThirdTypeName(oneFeesData.fThirdType) << ") , Validity start date (" << CSxTools::NumToDateDDMMYYYY(oneFeesData.fStartDate) 
				<< ") , Validity end date (" << CSxTools::NumToDateDDMMYYYY(oneFeesData.fStartDate) << ") will not be computed because computation start date (" << CSxTools::NumToDateDDMMYYYY(startDate) 
				<< " and computation end date (" << CSxTools::NumToDateDDMMYYYY(endDate) << ") are not in the validity range"));
		return;
	}	
	
	const CSAMFund* fund = dynamic_cast<const CSAMFund*>(CSRInstrument::GetInstance(oneFeesData.fFundCode));
	if (fund)
	{	
		double result = 0.;
		double finalAverageAsset = 0.;
		long finalNbDays = 0;
		double finalRetrocessionRate = 0.;
		double finalCommissionRate = 0.;
		_STL::vector<SSxRetrocessionFeesDetailsData> listOfRetroFeesDetails;

		if (oneFeesData.fThirdType == CSxThirdTypeMenu::eFundPromoter)
		{		
			//Fund promoters retrocession			

			result = GetManagementFees(oneFeesData.fFundCode, startDate, endDate, fManagementFeesSteGestionBEName.c_str());			
			finalAverageAsset = result;
			result = GetCouponRate(result,oneFeesData,finalRetrocessionRate);
		}
		else
		{				
			switch(oneFeesData.fComputationMethod)
			{
			case CSxComputationMethodMenu::eProrata:
				{						
					MESS(Log::debug, FROM_STREAM("Computation method : Prorata, Fund id : " << oneFeesData.fFundCode << ", Start date : " << CSxTools::NumToDateDDMMYYYY(startDate) <<
						", End date :" << CSxTools::NumToDateDDMMYYYY(endDate) ));
					
					//Get the list of NAV dates between start date and computation date
					_STL::vector<long> listOfNAVDates = fund->GetNAVDatesBetween(startDate,endDate);	
					MESS(Log::debug,FROM_STREAM("Number of NAV Dates : " << listOfNAVDates.size()));
					
					char mess[1024];										

					for (unsigned int i = 0; i < listOfNAVDates.size(); i++)
					{						
						//Get nb shares for this business partner					
						double NbSharesForThisBusinessPartner = GetNbSharesBusinessPartner(oneFeesData.fFundCode,oneFeesData.fThirdId,listOfNAVDates[i]);						
						sprintf_s(mess, 1024, "Number of shares for business partner(%ld) is %lf", oneFeesData.fThirdId, NbSharesForThisBusinessPartner);
						MESS(Log::debug, mess);

						double nbShares = GetNbShares(oneFeesData.fFundCode,listOfNAVDates[i]);
						sprintf_s(mess, 1024, "Number of shares is %lf", nbShares);
						MESS(Log::debug, mess);
						
						double CFGManagementFees = GetManagementFees(oneFeesData.fFundCode,listOfNAVDates[i],listOfNAVDates[i], fManagementFeesSteGestionBEName.c_str());
						sprintf_s(mess, 1024, "CFG Management fees (business event \"Provision Frais SteGestion\") is %lf", CFGManagementFees);
						MESS(Log::debug, mess);

						double CDVMManagementFees = GetManagementFees(oneFeesData.fFundCode,listOfNAVDates[i],listOfNAVDates[i], fManagementFeesCDVMBEName.c_str());
						sprintf_s(mess, 1024, "CDVM Management fees (business event \"Provision Frais CDVM\") is %lf", CDVMManagementFees);
						MESS(Log::debug, mess);

						double DDGManagementFees = GetManagementFees(oneFeesData.fFundCode,listOfNAVDates[i],listOfNAVDates[i], fManagementFeesDDGBEName.c_str());
						sprintf_s(mess, 1024, "DDG Management fees (business event \"Provision Frais DDG\") is %lf", DDGManagementFees);
						MESS(Log::debug, mess);

						double MCLManagementFees = GetManagementFees(oneFeesData.fFundCode,listOfNAVDates[i],listOfNAVDates[i], fManagementFeesMCLBEName.c_str());
						sprintf_s(mess, 1024, "MCL Management fees (business event \"Provision Frais MCL\") is %lf", MCLManagementFees);
						MESS(Log::debug, mess);

						double fundPromoterRetrocessionFees = GetManagementFees(oneFeesData.fFundCode,listOfNAVDates[i],listOfNAVDates[i], fRetrocessionPromoterFeesBEName.c_str());
						sprintf_s(mess, 1024, "fund Promoter Retrocession fees (business event \"Provision Retrocession Promo\") is %lf", fundPromoterRetrocessionFees);
						MESS(Log::debug, mess);
						
						double PNB = 0.;
						PNB = CFGManagementFees - CDVMManagementFees - DDGManagementFees - MCLManagementFees - fundPromoterRetrocessionFees;
						sprintf_s(mess, 1024, "PNB is %lf", PNB);
						MESS(Log::debug, mess);
						
						if (nbShares)
							result += PNB * NbSharesForThisBusinessPartner/nbShares;
						
						SSxRetrocessionFeesDetailsData retroFeesDetailsData(oneFeesData.fFundCode,oneFeesData.fThirdId, oneFeesData.fThirdType,oneFeesData.fComputationMethod,startDate,endDate,
															listOfNAVDates[i],NbSharesForThisBusinessPartner,nbShares,0,0,CFGManagementFees, CDVMManagementFees, 
															DDGManagementFees, MCLManagementFees, fundPromoterRetrocessionFees, PNB);
						
						listOfRetroFeesDetails.push_back(retroFeesDetailsData);

						sprintf_s(mess, 1024, "result is %lf", result);
						MESS(Log::debug, mess);
					}
										
					finalAverageAsset = result;
					finalNbDays = endDate-startDate+1;
					result = GetCouponRate(result,oneFeesData,finalRetrocessionRate);

				}
				break;
			case CSxComputationMethodMenu::eAverageAM:
				{
					MESS(Log::debug, FROM_STREAM("Computation method : Moyenne pondérée AM, Fund id : " << oneFeesData.fFundCode << ", Start date : " << CSxTools::NumToDateDDMMYYYY(startDate) <<
						", End date :" << CSxTools::NumToDateDDMMYYYY(endDate) ));
					
					//Get the list of NAV dates between start date and computation date
					_STL::vector<long> listOfNAVDates = fund->GetNAVDatesBetween(startDate,endDate);
					MESS(Log::debug,FROM_STREAM("Number of NAV Dates : " << listOfNAVDates.size()));

					double averageAsset = 0.;
					long sumNbDays = 0;
					size_t nbNavDates = listOfNAVDates.size();
					char mess[1024];																				
					
					if (nbNavDates > 0)
					{
						for (unsigned int i = 0; i < nbNavDates; i++)
						{						
							long nbDays = 0;

							if (i > 0)
							{
								nbDays = listOfNAVDates[i]-listOfNAVDates[i-1];
							}
							else
							{
								nbDays = listOfNAVDates[i]-startDate+1;
							}

							//Get nb shares for this business partner													
							double nbSharesForThisBusinessPartner = GetNbSharesBusinessPartner(oneFeesData.fFundCode,oneFeesData.fThirdId,listOfNAVDates[i]);												
							sprintf_s(mess, 1024, "Number of shares for business partner(%ld) is %lf", oneFeesData.fThirdId, nbSharesForThisBusinessPartner);
							MESS(Log::debug, mess);

							double assetValue = GetAssetValue(oneFeesData.fFundCode,listOfNAVDates[i]);
							sprintf_s(mess, 1024, "Asset Value for fund %ld and date %s is %lf", oneFeesData.fFundCode, CSxTools::NumToDateDDMMYYYY(listOfNAVDates[i]), assetValue);
							MESS(Log::debug, mess);

							averageAsset += nbSharesForThisBusinessPartner*assetValue*nbDays;
							sumNbDays += nbDays;	

							SSxRetrocessionFeesDetailsData retroFeesDetailsData(oneFeesData.fFundCode,oneFeesData.fThirdId, oneFeesData.fThirdType,oneFeesData.fComputationMethod,startDate,endDate,
																listOfNAVDates[i],nbSharesForThisBusinessPartner,0,nbDays,assetValue,0,0,0,0,0,0);																

							listOfRetroFeesDetails.push_back(retroFeesDetailsData);
						}

						if (listOfNAVDates[nbNavDates-1] < endDate)
						{
							long nbDays = endDate-listOfNAVDates[nbNavDates-1];

							//Get nb shares for this business partner													
							double nbSharesForThisBusinessPartner = GetNbSharesBusinessPartner(oneFeesData.fFundCode,oneFeesData.fThirdId,endDate);												
							sprintf_s(mess, 1024, "Number of shares for business partner(%ld) is %lf", oneFeesData.fThirdId, nbSharesForThisBusinessPartner);
							MESS(Log::debug, mess);

							double assetValue = GetAssetValue(oneFeesData.fFundCode,endDate);
							sprintf_s(mess, 1024, "Asset Value for fund %ld and date %s is %lf", oneFeesData.fFundCode, CSxTools::NumToDateDDMMYYYY(endDate), assetValue);
							MESS(Log::debug, mess);

							averageAsset += nbSharesForThisBusinessPartner*assetValue*nbDays;
							sumNbDays += nbDays;

							SSxRetrocessionFeesDetailsData retroFeesDetailsData(oneFeesData.fFundCode,oneFeesData.fThirdId, oneFeesData.fThirdType,oneFeesData.fComputationMethod,startDate,endDate,
																				endDate,nbSharesForThisBusinessPartner,0,nbDays,assetValue,0,0,0,0,0,0);																

							listOfRetroFeesDetails.push_back(retroFeesDetailsData);
						}
					}
					else
					{
						if (startDate <= endDate)
						{
							long nbDays = endDate-startDate+1;

							//Get nb shares for this business partner													
							double nbSharesForThisBusinessPartner = GetNbSharesBusinessPartner(oneFeesData.fFundCode,oneFeesData.fThirdId,endDate);												
							sprintf_s(mess, 1024, "Number of shares for business partner(%ld) is %lf", oneFeesData.fThirdId, nbSharesForThisBusinessPartner);
							MESS(Log::debug, mess);

							double assetValue = GetAssetValue(oneFeesData.fFundCode,endDate);
							sprintf_s(mess, 1024, "Asset Value for fund %ld and date %s is %lf", oneFeesData.fFundCode, CSxTools::NumToDateDDMMYYYY(endDate), assetValue);
							MESS(Log::debug, mess);

							averageAsset += nbSharesForThisBusinessPartner*assetValue*nbDays;
							sumNbDays += nbDays;

							SSxRetrocessionFeesDetailsData retroFeesDetailsData(oneFeesData.fFundCode,oneFeesData.fThirdId, oneFeesData.fThirdType,oneFeesData.fComputationMethod,startDate,endDate,
																					endDate,nbSharesForThisBusinessPartner,0,nbDays,assetValue,0,0,0,0,0,0);																

							listOfRetroFeesDetails.push_back(retroFeesDetailsData);
						}
					}										
					
					sprintf_s(mess, 1024, "Average asset is %lf", averageAsset);
					MESS(Log::debug, mess);

					sprintf_s(mess, 1024, "Sum of number of days is %ld", sumNbDays);
					MESS(Log::debug, mess);
					
					if (sumNbDays)
						averageAsset /= (double)sumNbDays;

					finalAverageAsset = averageAsset;
					finalNbDays = sumNbDays;

					CSRDay dayObject(startDate);
					CSRDay firstDayOfYear(1,1,dayObject.fYear);
					CSRDay lastDayOfYear(31,12,dayObject.fYear);

					long nbDaysInYear = lastDayOfYear-firstDayOfYear+1;
					averageAsset *= (double)sumNbDays/(double)nbDaysInYear;
					
					/*result = GetCouponPerLevelRate(averageAsset);*/

					result = GetCouponRate(averageAsset,oneFeesData,finalRetrocessionRate);
				}										

				break;
			case CSxComputationMethodMenu::eAverageCGR:

				{
					MESS(Log::debug, FROM_STREAM("Computation method : Moyenne pondérée CGR, Fund id : " << oneFeesData.fFundCode << ", Start date : " << CSxTools::NumToDateDDMMYYYY(startDate) <<
						", End date :" << CSxTools::NumToDateDDMMYYYY(endDate) ));
					
					//Get the list of NAV dates between start date and computation date
					_STL::vector<long> listOfNAVDates = fund->GetNAVDatesBetween(startDate,endDate);
					MESS(Log::debug,FROM_STREAM("Number of NAV Dates : " << listOfNAVDates.size()));

					double averageAsset = 0.;
					long sumNbDays = 0;
					size_t nbNavDates = listOfNAVDates.size();
					char mess[1024];																				

					if (nbNavDates > 0)
					{
						for (unsigned int i = 0; i < nbNavDates; i++)
						{						
							long nbDays = 0;

							if (i > 0)
							{
								nbDays = listOfNAVDates[i]-listOfNAVDates[i-1];
							}
							else
							{
								nbDays = listOfNAVDates[i]-startDate+1;
							}

							//Get nb shares for this business partner													
							double nbSharesForThisBusinessPartner = GetNbSharesBusinessPartner(oneFeesData.fFundCode,oneFeesData.fThirdId,listOfNAVDates[i]);												
							sprintf_s(mess, 1024, "Number of shares for business partner(%ld) is %lf", oneFeesData.fThirdId, nbSharesForThisBusinessPartner);
							MESS(Log::debug, mess);

							double assetValue = GetAssetValue(oneFeesData.fFundCode,listOfNAVDates[i]);
							sprintf_s(mess, 1024, "Asset Value for fund %ld and date %s is %lf", oneFeesData.fFundCode, CSxTools::NumToDateDDMMYYYY(listOfNAVDates[i]), assetValue);
							MESS(Log::debug, mess);

							averageAsset += nbSharesForThisBusinessPartner*assetValue*nbDays;
							sumNbDays += nbDays;						
						}

						if (listOfNAVDates[nbNavDates-1] < endDate)
						{
							long nbDays = endDate-listOfNAVDates[nbNavDates-1];

							//Get nb shares for this business partner													
							double nbSharesForThisBusinessPartner = GetNbSharesBusinessPartner(oneFeesData.fFundCode,oneFeesData.fThirdId,endDate);												
							sprintf_s(mess, 1024, "Number of shares for business partner(%ld) is %lf", oneFeesData.fThirdId, nbSharesForThisBusinessPartner);
							MESS(Log::debug, mess);

							double assetValue = GetAssetValue(oneFeesData.fFundCode,endDate);
							sprintf_s(mess, 1024, "Asset Value for fund %ld and date %s is %lf", oneFeesData.fFundCode, CSxTools::NumToDateDDMMYYYY(endDate), assetValue);
							MESS(Log::debug, mess);

							averageAsset += nbSharesForThisBusinessPartner*assetValue*nbDays;
							sumNbDays += nbDays;
						}
					}
					else
					{
						if (startDate <= endDate)
						{
							long nbDays = endDate-startDate+1;

							//Get nb shares for this business partner													
							double nbSharesForThisBusinessPartner = GetNbSharesBusinessPartner(oneFeesData.fFundCode,oneFeesData.fThirdId,endDate);												
							sprintf_s(mess, 1024, "Number of shares for business partner(%ld) is %lf", oneFeesData.fThirdId, nbSharesForThisBusinessPartner);
							MESS(Log::debug, mess);

							double assetValue = GetAssetValue(oneFeesData.fFundCode,endDate);
							sprintf_s(mess, 1024, "Asset Value for fund %ld and date %s is %lf", oneFeesData.fFundCode, CSxTools::NumToDateDDMMYYYY(endDate), assetValue);
							MESS(Log::debug, mess);

							averageAsset += nbSharesForThisBusinessPartner*assetValue*nbDays;
							sumNbDays += nbDays;
						}
					}										

					sprintf_s(mess, 1024, "Average asset is %lf", averageAsset);
					MESS(Log::debug, mess);

					sprintf_s(mess, 1024, "Sum of number of days is %ld", sumNbDays);
					MESS(Log::debug, mess);

					if (sumNbDays)
						averageAsset /= (double)sumNbDays;

					finalAverageAsset = averageAsset;
					finalCommissionRate = oneFeesData.fCommissionRate;
					finalNbDays = sumNbDays;

					averageAsset *= oneFeesData.fCommissionRate * .01;

					sprintf_s(mess, 1024, "Commission rate is %lf", oneFeesData.fCommissionRate);
					MESS(Log::debug, mess);

					CSRDay dayObject(startDate);
					CSRDay firstDayOfYear(1,1,dayObject.fYear);
					CSRDay lastDayOfYear(31,12,dayObject.fYear);

					long nbDaysInYear = lastDayOfYear-firstDayOfYear+1;
					averageAsset *= (double)sumNbDays/(double)nbDaysInYear;

					/*result = GetCouponPerLevelRate(averageAsset,oneFeesData);*/
					result = GetCouponRate(averageAsset,oneFeesData,finalRetrocessionRate);					
				}

				break;
			default:
				break;

			}
		}

		SaveResults(oneFeesData,startDate,endDate,result,finalAverageAsset,finalNbDays,finalRetrocessionRate,finalCommissionRate, listOfRetroFeesDetails);
	}

	END_LOG();
}

double CSxRetrocessionFees::GetFundPromoterRetroRate(SSxRetrocessionFeesData& oneFeesData, long startDate, long endDate)
{
	double result = 0.;

	for (unsigned int i = 0; i < fRetrocessionFeesList.size(); i++)
	{
		if (fRetrocessionFeesList[i].fThirdId == oneFeesData.fThirdId && fRetrocessionFeesList[i].fFundCode == oneFeesData.fFundCode && fRetrocessionFeesList[i].fThirdType == CSxThirdTypeMenu::eFundPromoter)
		{
			if ((startDate < fRetrocessionFeesList[i].fStartDate && fRetrocessionFeesList[i].fStartDate > 0) || (endDate > fRetrocessionFeesList[i].fEndDate && fRetrocessionFeesList[i].fEndDate > 0))
			{
				continue;
			}
			
			result = fRetrocessionFeesList[i].fRetrocessionRate1;
		}
	}

	return result;
}

double CSxRetrocessionFees::GetManagementFees(long fundCode, long startDate, long endDate, const char* name)
{
	BEGIN_LOG("GetManagementFees");
	
	double result = 0.;

	const CSRBusinessEvent* businessEvent = CSRBusinessEvent::GetBusinessEventByName(name);

	int businessEventCode = 0;
	if (businessEvent)
		businessEventCode = businessEvent->GetIdent();

	const CSAMFund* fund = dynamic_cast<const CSAMFund*>(CSRInstrument::GetInstance(fundCode));

	if (fund)
	{				
		long feesFolioCode = fund->GetFeesPortfolio();

		const CSRPortfolio* feesFolio = CSRPortfolio::GetCSRPortfolio(feesFolioCode);
		if (feesFolio)
		{
			if (!feesFolio->IsLoaded())
				feesFolio->Load();

			for (int i = 0; i < feesFolio->GetFlatViewPositionCount(); i++)
			{
				const CSRPosition* position = feesFolio->GetNthFlatViewPosition(i);
				if (position)
				{
					CSRTransactionVector trxVector;
					position->GetTransactions(trxVector);
					for (unsigned long j = 0; j < trxVector.size(); j++)
					{
						if (trxVector[j].GetTransactionType() == businessEventCode && trxVector[j].GetTransactionDate() >= startDate 
										&& trxVector[j].GetTransactionDate() <= endDate)
						{
							result += trxVector[j].GetNetAmount();
						}
					}
				}
			}
		}					
	}
	
	END_LOG();
	return result;
}

double CSxRetrocessionFees::GetCouponRate(double amount, SSxRetrocessionFeesData oneFeesData, double &retrocessionRate)
{
	BEGIN_LOG("GetCouponRate");

	char mess[1024];
	sprintf_s(mess, 1024, "Call GetCouponRate with amount : %lf", amount);
	MESS(Log::debug, mess);

	double result = 0.;
	
	if (oneFeesData.fLevel1 == 0 || amount <= oneFeesData.fLevel1)
	{							
		retrocessionRate = oneFeesData.fRetrocessionRate1;
		result = amount*oneFeesData.fRetrocessionRate1 * 0.01;
	}
	else 
	{
		if (oneFeesData.fLevel2 == 0 || amount <= oneFeesData.fLevel2)
		{
			retrocessionRate = oneFeesData.fRetrocessionRate2;
			result = amount*oneFeesData.fRetrocessionRate2 * 0.01;
		}
		else
		{
			
			if (oneFeesData.fLevel3 == 0 || amount <= oneFeesData.fLevel3)
			{
				retrocessionRate = oneFeesData.fRetrocessionRate3;
				result = amount*oneFeesData.fRetrocessionRate3 * 0.01;
			}
			else
			{
				retrocessionRate = oneFeesData.fRetrocessionRate4;
				result = amount*oneFeesData.fRetrocessionRate4 * 0.01;
			}
		}
	}

	sprintf_s(mess, 1024, "Result GetCouponRate is : %lf", result);
	MESS(Log::debug, mess);
	
	END_LOG();

	return result;
}

double CSxRetrocessionFees::GetCouponPerLevelRate(double amount, SSxRetrocessionFeesData oneFeesData)
{		
	BEGIN_LOG("GetCouponPerLevelRate");

	struct SSxRatePerLevel
	{
		double fLevel;		
		double fRate;
	};

	_STL::vector<SSxRatePerLevel> fRatesPerLevelList;

	if (!oneFeesData.fRetrocessionRate1)
	{
		SSxRatePerLevel ratePerLevel;
		ratePerLevel.fLevel = oneFeesData.fLevel1;
		ratePerLevel.fRate = oneFeesData.fRetrocessionRate1;
		fRatesPerLevelList.push_back(ratePerLevel);
	}

	if (!oneFeesData.fRetrocessionRate2)
	{
		SSxRatePerLevel ratePerLevel;
		ratePerLevel.fLevel = oneFeesData.fLevel2;
		ratePerLevel.fRate = oneFeesData.fRetrocessionRate2;
		fRatesPerLevelList.push_back(ratePerLevel);
	}

	if (!oneFeesData.fRetrocessionRate3)
	{
		SSxRatePerLevel ratePerLevel;
		ratePerLevel.fLevel = oneFeesData.fLevel3;
		ratePerLevel.fRate = oneFeesData.fRetrocessionRate3;
		fRatesPerLevelList.push_back(ratePerLevel);
	}
	
	size_t nbLevels = fRatesPerLevelList.size();

	if (nbLevels == 0)
	{
		MESS(Log::warning, FROM_STREAM("Retrocession fees for Fund (" << oneFeesData.fFundCode << ") , Business partner (" << oneFeesData.fThirdId << ") , Business partner type ("
			<< CSxThirdTypeMenu::GetThirdTypeName(oneFeesData.fThirdType) << ") , Validity start date (" << CSxTools::NumToDateDDMMYYYY(oneFeesData.fStartDate) 
			<< ") , Validity end date (" << CSxTools::NumToDateDDMMYYYY(oneFeesData.fStartDate) << ") has rates per level list empty. You should set this list with at least one level. Set the rate to 0."));
				
		END_LOG();
		return 0.;
	}	

	_STL::vector<double> listOfMaxValuesPerLevel;

	for (unsigned int i = 0; i < nbLevels-1; i++)
	{
		listOfMaxValuesPerLevel.push_back((fRatesPerLevelList[i+1].fLevel-fRatesPerLevelList[i].fLevel)* fRatesPerLevelList[i].fRate*0.01);
	}

	double result = 0.;

	for (unsigned int i = 0; i < nbLevels; i++)
	{
		if (amount > fRatesPerLevelList[nbLevels-i-1].fLevel)
		{
			result = (amount-fRatesPerLevelList[nbLevels-i-1].fLevel) * fRatesPerLevelList[nbLevels-i-1].fRate*0.01;
			for (unsigned int j = 0; j < nbLevels-i-1; j++)
			{
				result += listOfMaxValuesPerLevel[j];
			}
			break;
		}			
	}

	END_LOG();

	return result;
}

double CSxRetrocessionFees::GetNbSharesBusinessPartner(long fundCode, long businessPartnerId, long date)
{
	BEGIN_LOG("GetNbSharesBusinessPartner");

	double ret = 0.;
	
	_STL::string BOStatusGroup = "";	
	ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES", "SRBOStatusGroup", BOStatusGroup, "ALL But Virtual Trade");
	MESS(Log::debug, "SRBOStatusGroup(" << BOStatusGroup << ")");

	_STL::string BOStatusCondition = GetBOStatusCondition(BOStatusGroup);
	MESS(Log::debug, FROM_STREAM("BOStatusCondition(" << BOStatusCondition << ")"));

	char query[QUERY_BUFFER_SIZE];
	sprintf_s(query,QUERY_BUFFER_SIZE,"select sum(NUMBER_SHARES) from FUND_PURCHASE where FUND = %ld and THIRD2 = %ld and date_to_num(VALUE_DATE) < %ld %s"
													,fundCode,businessPartnerId,date,BOStatusCondition.c_str());
		
	CSRSqlQuery::QueryReturning1Double(query,&ret);

	END_LOG();
	
	return ret;
}

double CSxRetrocessionFees::GetNbShares(long fundCode, long date)
{
	BEGIN_LOG("GetNbShares");

	double ret = 0.;

	_STL::string BOStatusGroup = "";	
	ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES", "SRBOStatusGroup", BOStatusGroup, "ALL But Virtual Trade");
	MESS(Log::debug, "SRBOStatusGroup(" << BOStatusGroup << ")");

	_STL::string BOStatusCondition = GetBOStatusCondition(BOStatusGroup);
	MESS(Log::debug, FROM_STREAM("BOStatusCondition(" << BOStatusCondition << ")"));

	char query[QUERY_BUFFER_SIZE];
	sprintf_s(query,QUERY_BUFFER_SIZE,"select sum(NUMBER_SHARES) from FUND_PURCHASE where FUND = %ld and date_to_num(VALUE_DATE) < %ld %s",fundCode,date,BOStatusCondition.c_str());

	CSRSqlQuery::QueryReturning1Double(query,&ret);

	END_LOG();

	return ret;
}

//double CSxRetrocessionFees::GetNAVPerShareForThisBusinessPartner(long fundCode, long businessPartnerId, long date)
//{
//	double ret = 0.;
//
//	char query[QUERY_BUFFER_SIZE];
//	sprintf_s(query,QUERY_BUFFER_SIZE,"select sum(grossamount+srfees+srfees2) from FUND_PURCHASE where FUND = %ld and THIRD2 = %ld and date_to_num(VALUE_DATE) <= %ld",fundCode,businessPartnerId,date);
//
//	CSRSqlQuery::QueryReturning1Double(query,&ret);
//
//	return ret;
//}

//double CSxRetrocessionFees::GetNAVPerShare(long fundCode, long date)
//{
//	double ret = 0.;
//
//	char query[QUERY_BUFFER_SIZE];
//	sprintf_s(query,QUERY_BUFFER_SIZE,"select sum(grossamount+srfees+srfees2) from FUND_PURCHASE where FUND = %ld and date_to_num(VALUE_DATE) <= %ld",fundCode,date);
//
//	CSRSqlQuery::QueryReturning1Double(query,&ret);
//
//	return ret;
//}

void CSxRetrocessionFees::SaveResults(SSxRetrocessionFeesData &oneFeesData, long startDate, long endDate, double result, double averageAsset, long nbDays, 
											double retrocessionRate, double commissionRate, _STL::vector<SSxRetrocessionFeesDetailsData> &listOfRetroFeesDetails)
{
	BEGIN_LOG("SaveResults");
	
	double amountTTC = result*(1 + oneFeesData.fTVARate*0.01);
	
	try
	{
		char queryBuffer[QUERY_BUFFER_SIZE];	

		//Insert date into CFG_RETRO_FEES_RESULTS
		sprintf_s(queryBuffer,QUERY_BUFFER_SIZE, "delete from CFG_RETRO_FEES_RESULTS where FUND_ID = %ld and BUSINESS_PARTNER_ID = %ld and BUSINESS_PARTNER_TYPE = %d and COMPUTATION_METHOD = %d and date_to_num(START_DATE) = %ld and date_to_num(END_DATE) = %ld",
			oneFeesData.fFundCode,oneFeesData.fThirdId,oneFeesData.fThirdType,oneFeesData.fComputationMethod,startDate,endDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResult(queryBuffer);
		CSRSqlQuery::QueryWithoutResultAndParam(queryBuffer);

		sprintf_s(queryBuffer,QUERY_BUFFER_SIZE,"insert into CFG_RETRO_FEES_RESULTS (FUND_ID,BUSINESS_PARTNER_ID,BUSINESS_PARTNER_TYPE,COMPUTATION_METHOD,START_DATE,"
			"END_DATE,AVERAGE_ASSET,NB_DAYS,RETRO_RATE,COMMISSION_RATE,AMOUNT_HT,AMOUNT_TTC) values (%ld,%ld,%d,%d,num_to_date(%ld),num_to_date(%ld),%lf,%ld,%lf,%lf,%lf,%lf)",
			oneFeesData.fFundCode, oneFeesData.fThirdId, oneFeesData.fThirdType, oneFeesData.fComputationMethod, startDate,endDate, averageAsset, nbDays,
			retrocessionRate, commissionRate, result, amountTTC);

		//DPH
		//CSRSqlQuery::QueryWithoutResult(queryBuffer);
		CSRSqlQuery::QueryWithoutResultAndParam(queryBuffer);

		//Insert calculation details into CFG_RETRO_FEES_DETAILS
		sprintf_s(queryBuffer,QUERY_BUFFER_SIZE, "delete from CFG_RETRO_FEES_DETAILS where FUND_ID = %ld and BUSINESS_PARTNER_ID = %ld and BUSINESS_PARTNER_TYPE = %d and COMPUTATION_METHOD = %d and date_to_num(START_DATE) = %ld and date_to_num(END_DATE) = %ld",
			oneFeesData.fFundCode,oneFeesData.fThirdId,oneFeesData.fThirdType,oneFeesData.fComputationMethod,startDate,endDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResult(queryBuffer);
		CSRSqlQuery::QueryWithoutResultAndParam(queryBuffer);

		for (unsigned int i = 0; i < listOfRetroFeesDetails.size(); i++)
		{
			SSxRetrocessionFeesDetailsData oneData = listOfRetroFeesDetails[i];
			sprintf_s(queryBuffer,QUERY_BUFFER_SIZE, "insert into CFG_RETRO_FEES_DETAILS(FUND_ID,BUSINESS_PARTNER_ID,BUSINESS_PARTNER_TYPE,COMPUTATION_METHOD,START_DATE,"
				"END_DATE,NAV_DATE,NB_SHARES_BUSINESS_PARTNER,NB_SHARES_TOTAL,NB_DAYS,NAV,FDG,CDVM,DDG,MCL,FUND_PROMOTER_RETROCESSION,PNB) "
				"values (%ld,%ld,%d,%d,num_to_date(%ld),num_to_date(%ld),num_to_date(%ld),%lf,%lf,%ld,%lf,%lf,%lf,%lf,%lf,%lf,%lf)", oneData.fFundId, oneData.fBusinessPartnerId,
				oneData.fBusinessPartnerType,oneData.fComputationMethod,oneData.fStartDate,oneData.fEndDate,oneData.fNavDate,oneData.fNumberOfSharesForBusinessPartner,
				oneData.fTotalNumberOfShares,oneData.fNbDays,oneData.fNav,oneData.fFDG,oneData.fCDVM,oneData.fDDG,oneData.fMCL,oneData.fFundPromoterRetrocession,oneData.fPNB);

			//DPH
			//CSRSqlQuery::QueryWithoutResult(queryBuffer);
			CSRSqlQuery::QueryWithoutResultAndParam(queryBuffer);
		}

		CSRSqlQuery::Commit();
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		char mess[1024];
		sprintf_s(mess, 1024,"ERROR (%s) while saving Retrocession fees results", FROM_STREAM(ex));
		CSRFitDialog::Message(mess);
		//DPH
		//MESS(Log::business_error, mess);
		MESS(Log::error, mess);

		CSRSqlQuery::RollBack();
	}
	catch(...)
	{		
		char mess[1024];
		sprintf_s(mess, 1024,"Unhandled exception occured while saving Retrocession fees results");
		CSRFitDialog::Message(mess);
		//DPH
		//MESS(Log::business_error, mess);
		MESS(Log::error, mess);

		CSRSqlQuery::RollBack();
	}

	END_LOG();
}

double CSxRetrocessionFees::GetAssetValue(long fundCode, long date)
{
	double ret = 0.;

	char query[QUERY_BUFFER_SIZE];
	sprintf_s(query,QUERY_BUFFER_SIZE,"select D from HISTORIQUE where sicovam = %ld and date_to_num(JOUR) = %ld",fundCode,date);

	CSRSqlQuery::QueryReturning1Double(query,&ret);

	return ret;
}

_STL::string CSxRetrocessionFees::GetBOStatusCondition(const _STL::string statusGroup)
{
	_STL::string boGroupFilter = "";
	if (_stricmp(statusGroup.c_str(), "ALL"))
	{
		boGroupFilter = " and BACKOFFICE in (";
		_STL::vector<long> groupList; 
		CSRKernelGroupOfStatuses::GetList(groupList);
		_STL::vector<long>::const_iterator iterGrp = groupList.begin();
		for(; iterGrp != groupList.end(); iterGrp++)
		{
			CSRKernelGroupOfStatuses group(*iterGrp);
			// Find the status group with the input name
			if (!_stricmp(group.GetName(), statusGroup.c_str()))
			{
				_STL::vector<long> listStatus;
				group.GetStatuses(listStatus);
				_STL::vector<long>::const_iterator iterSt = listStatus.begin();
				size_t si = 1;
				// Add BO status clause
				for(; iterSt != listStatus.end(); si++, iterSt++)
				{		
					boGroupFilter += FROM_STREAM(*iterSt);
					if (si < listStatus.size())
						boGroupFilter += ",";
				}
				break;
			}
		}
		if (iterGrp == groupList.end())
			throw GeneralException("Status Group not found.");
		boGroupFilter += ") ";
	}	

	return boGroupFilter;
}
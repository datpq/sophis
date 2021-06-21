
/*
** Includes
*/
#include "SphTools/base/CommonOS.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/SphUserRights.h"
#include "SphInc/SphUserRightsEnums.h"
#include "SphInc/SphPreference.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/backoffice_kernel/SphKernelStatusGroup.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)
#include __STL_INCLUDE_PATH(set)
#include "SphInc/market_data/SphMarketData.h"
#include "SphTools/base/StringUtil.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/value/kernel/SphFund.h"

#include "CSxRetrocessionFeesScenario.h"
#include "CSxRetrocessionFeesDlg.h"
#include "CSxRetrocessionFees.h"
#include "Constants.h"
#include "CSxTools.h"


using namespace sophis::value;


/*static*/ const char* CSxRetrocessionFeesConfigScenario::__CLASS__ = "CSxRetrocessionFeesConfigScenario";
/*static*/ const char* CSxRetrocessionFeesScenario::__CLASS__ = "CSxRetrocessionFeesScenario";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CSxRetrocessionFeesConfigScenario)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	CSxRetrocessionFeesConfigScenario::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void CSxRetrocessionFeesConfigScenario::Run()
{
	BEGIN_LOG("Run");

	try
	{						
		//User rights				
		eRightStatusType rightStatusType = GetEditRetrocessionFeesUserRight();

		switch(rightStatusType)
		{
		case rsReadWrite:
			CSxRetrocessionFeesDlg::Display(rsReadWrite);
			break;
		case rsReadOnly:			
			CSxRetrocessionFeesDlg::Display(rsReadOnly);
			break;
		case rsNoRight:
			CSRFitDialog::Message("You do not have rights to access \"Retrocession Fees Configuration\" scenario.");
			break;					
		default:
			CSRFitDialog::Message("User right not handled.\nYou will not have rights to modify data.");
			CSxRetrocessionFeesDlg::Display(rsReadOnly);							
			break;
		}
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		CSRFitDialog::Message(FROM_STREAM("ERROR ("<<ex<<") while running \"Retrocession Fees Configuration\" dialog"));		
	}
	catch(...)
	{		
		CSRFitDialog::Message(FROM_STREAM("Unhandled exception occured while running \"Retrocession Fees Configuration\" dialog"));
	}					
	
	END_LOG();
}

eRightStatusType CSxRetrocessionFeesConfigScenario::GetEditRetrocessionFeesUserRight()
{
	long userID		= 0;
	long groupID	= 0;

	CSRPreference::GetUserID(&userID,&groupID);
	if(userID==1)
	{
		return rsReadWrite;
	}
	else
	{
		CSRUserRights userRights(userID);
		userRights.LoadDetails();
		eRightStatusType	userRightStatus = userRights.GetUserDefRight(EDIT_RETROCESSION_FEES_USER_RIGHT);
		if(userRightStatus==rsSameAsParent)
		{
			CSRUserRights groupRights(groupID);
			groupRights.LoadDetails();
			return groupRights.GetUserDefRight(EDIT_RETROCESSION_FEES_USER_RIGHT);
		}
		else
			return userRightStatus;
	}
}

//////////////////////////// CSxRetrocessionFeesScenario //////////////////////////////////////////////////////
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CSxRetrocessionFeesScenario)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	CSxRetrocessionFeesScenario::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void CSxRetrocessionFeesScenario::Run()
{
	BEGIN_LOG("Run");

	try
	{						
		//User rights				
		if (GetComputeRetrocessionFeesUserRight())
		{
			CSxRetrocessionFees fees;

			//Get parameters from risk.ini file

			// Funds list						
			_STL::string fundListStr = "";
			ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES","FundList",fundListStr, "");
			_STL::vector<long> fundList;
			fundList.clear();
			GetFundList(fundListStr,fundList);
			
			// start date
			_STL::string startDateStr = "";
			long startDate = gApplicationContext->GetDate();

			ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES", "StartDate", startDateStr, ""); //Date should be at format DD/MM/YYYY
			if (startDateStr != "")
			{				
				startDate = CSxTools::DDMMYYYYDateToNum(startDateStr);
			}

			// end date
			_STL::string endDateStr = "";
			long endDate = startDate;

			ConfigurationFileWrapper::getEntryValue("CFG_RETROCESSION_FEES", "EndDate", endDateStr, ""); //End date should be at format DD/MM/YYYY
			if (endDateStr != "")
			{				
				endDate = CSxTools::DDMMYYYYDateToNum(endDateStr);
			}

			MESS(Log::debug, FROM_STREAM("Start \"Retrocession Fees\" scenario with the following parameters : FundList (" << fundListStr << ") , Start date ( " 
														<< CSxTools::NumToDateDDMMYYYY(startDate) << ") , End date (" << CSxTools::NumToDateDDMMYYYY(endDate) << ")"));
			fees.Compute(fundList,startDate,endDate);

			MESS(Log::debug, "\"Retrocession Fees\" scenario finished successfully");
		}
		else
			MESS(Log::warning,"This user is not allowed to run \"Retrocession Fees\" scenario");

	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		MESS(Log::error, FROM_STREAM("ERROR ("<<ex<<") while running \"Retrocession Fees\" scenario"));		
	}
	catch(...)
	{		
		MESS(Log::error, FROM_STREAM("Unhandled exception occured while running \"Retrocession Fees\" scenario"));
	}					

	END_LOG();
}

bool CSxRetrocessionFeesScenario::GetComputeRetrocessionFeesUserRight()
{	
	long userID		= 0;
	long groupID	= 0;

	CSRPreference::GetUserID(&userID,&groupID);
	if(userID==1)
	{
		return true;
	}
	else
	{
		CSRUserRights userRights(userID);
		userRights.LoadDetails();
		return userRights.HasAccess(COMPUTE_RETROCESSION_FEES_USER_RIGHT);		
	}
}

void CSxRetrocessionFeesScenario::GetFundList(_STL::string fundListStr, _STL::vector<long>& fundList)
{			
	BEGIN_LOG("GetFundList");

	size_t position = 0;
	size_t index = 0;
	_STL::set<long> fundSet;

	do 
	{
		index = fundListStr.find(',', position);
		_STL::string argument = fundListStr.substr(position, index - position);
		sophisTools::base::StringUtil::trim(argument);
		long sicovam = atol(argument.c_str());
		
		//Check if it is an internal fund		
		if (CSAMFund::IsInternalFund(sicovam))
		{
			_STL::set<long>::iterator iter = fundSet.find(sicovam);
			if (iter == fundSet.end())
			{
				fundSet.insert(sicovam);
				fundList.push_back(sicovam);
			}
		}
		else
		{
			MESS(Log::warning, FROM_STREAM("Instrument("<< sicovam << ") is not an internal fund. It will not be processed."));
		}

		position = index + 1;
	} while(index!= _STL::string::npos);

	END_LOG();

}

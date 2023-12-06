#include "CSxNACKTemplateCondition.h"

/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc\backoffice_kernel\SphThirdParty.h"
#include <SphInc/misc/ConfigurationFileWrapper.h>

// specific
#include "CSxIsBrokerDIMCondition.h"
#include <boost/algorithm/string/split.hpp>
#include <boost/algorithm/string/classification.hpp>

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;
using namespace std;


/*
** Statics
*/
/*static*/  const char* CSxNACKTemplateCondition::__CLASS__ = "CSxNACKTemplateCondition";


bool CSxNACKTemplateCondition::get_condition(const portfolio::CSRTransaction& trade
	, const backoffice_kernel::SSDocGenerationCriteria& criteria) const {


	BEGIN_LOG("get_condition");
	
	MESS(Log::debug, "Begin(GetTransactionCode=" << trade.GetTransactionCode() << ")");
	bool res = true;
	try
	{
		std::string boStatusList = "";
		ConfigurationFileWrapper::getEntryValue("CashAutomation", "CashMGRNACKStatusList", boStatusList, "1760;1721");
		std::set<eBackOfficeType> boStsVector;
		std::string delimiter = ";";
		vector<string> SplitVec;
		boost::split(SplitVec, boStatusList, boost::is_any_of(delimiter));
		for (vector<string>::iterator it = SplitVec.begin(); it != SplitVec.end(); ++it)
		{
			eBackOfficeType statusId = (eBackOfficeType)stoi(*it);
			boStsVector.insert(statusId);
		}

		eBackOfficeType boStatusId = trade.GetBackOfficeType();
		if (boStsVector.find(boStatusId) != boStsVector.end())
		{
			res = false;
		}

		MESS(Log::debug, "Status id " << boStatusId << " result: " << res);
	}
	catch (...)
	{
		MESS(Log::error, FROM_STREAM("Exception encoutered!"));
	}
	
	END_LOG();
	return res;
}

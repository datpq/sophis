/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include <boost/algorithm/string.hpp>    


// specific
#include "CSxIsHedgedCondition.h"
#include <SphTools/SphLoggerUtil.h>
#include <locale>

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
//WITHOUT_CONSTRUCTOR_WORKFLOW_DEF_CONDITION(CSxIsHedgedCondition);

/*static*/ const char* CSxIsHedgedCondition::__CLASS__ = "CSxIsDelegateCondition";

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsHedgedCondition::GetCondition(const CSRTransaction & tr, const SSKernelInstrumentSelector &sel)const
{
	BEGIN_LOG("GetCondition");
	bool res = false;
	const CSRPortfolio * folio = CSRPortfolio::GetCSRPortfolio(tr.GetFolioCode());

	if (nullptr != folio)
	{
		char fullName[255];
		folio->GetFullName(fullName);
		std::size_t found = boost::algorithm::to_lower_copy(std::string(fullName)).find("hedge");
		
		res = found != std::string::npos;
		LOG(Log::debug, FROM_STREAM("Is folio " << fullName << "a hedged folio? " << res));
	}

	END_LOG();
	return res;
}


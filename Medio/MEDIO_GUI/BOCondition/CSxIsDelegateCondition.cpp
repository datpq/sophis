/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "..\..\Tools\CSxSQLHelper.h"

// specific
#include "CSxIsDelegateCondition.h"
#include <SphTools/SphLoggerUtil.h>

/*
** Namespace
*/
using namespace sophis::portfolio;
using namespace sophis::backoffice_kernel;
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
//WITHOUT_CONSTRUCTOR_WORKFLOW_DEF_CONDITION(CSxIsDelegateCondition);

/*static*/ const char* CSxIsDelegateCondition::__CLASS__ = "CSxIsDelegateCondition";

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsDelegateCondition::GetCondition(const CSRTransaction & tr, const SSKernelInstrumentSelector &sel)const 
{
	BEGIN_LOG("GetCondition");
	bool res = false;
	const CSRPortfolio * folio = CSRPortfolio::GetCSRPortfolio(tr.GetFolioCode());

	if(nullptr != folio)
	{
		char fullName[255];
		folio->GetFullName(fullName);
		std::size_t found = std::string(fullName).find(CSxSQLHelper::GetTargetPortfolioName());//"MAML"); //TOCHANGE
		res = found == std::string::npos;
		LOG(Log::debug, FROM_STREAM("Is folio " << fullName <<" a Medio folio? " << res));
	}

	END_LOG();
	return res;
}


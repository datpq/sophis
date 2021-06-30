/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
//#include "SphReportingInc/SphReporting.h"

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/ToolkitReportingVersion.h"
//#include "BindingFunctions.h"
#include "ParameterType.h"

//}}SOPHIS_TOOLKIT_INCLUDE

using namespace eff::ToolkitReporting;
using namespace sophis::reporting;
using namespace sophis::misc;

UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(ToolkitReporting_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	//Binding function used in Reporting Module
	//IBindingFunction * fn;
	//fn = new BfLastDayOfMonth();
	//CSRBindingFunctionManager::GetInstance()->AddBindingFunction(fn);
	//fn = new BfFirstDayOfMonth();
	//CSRBindingFunctionManager::GetInstance()->AddBindingFunction(fn);
	//fn = new BfLastDayOfPreviousMonth();
	//CSRBindingFunctionManager::GetInstance()->AddBindingFunction(fn);
	//fn = new BfConcatenate();
	//CSRBindingFunctionManager::GetInstance()->AddBindingFunction(fn);

	ParameterTypeImpl::InitializeAllParameterTypes();

//}}SOPHIS_INITIALIZATION
}

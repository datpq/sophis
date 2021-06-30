#include "SphInc/SphMacros.h"
#include "SphTools/SphLoggerUtil.h"
#if (TOOLKIT_VERSION < 7100)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif

#include "ParameterType.h"
#include <sstream>
#include <iostream>

using namespace eff::ToolkitReporting;
using namespace sophis::reporting;

void ParameterTypeImpl::InitializeAllParameterTypes()
{
#if (TOOLKIT_VERSION < 7100)
	_STL::string parameterTypeList = "";
#else
	std::string parameterTypeList = "";
#endif

	// combobox parametertypes
	ConfigurationFileWrapper::getEntryValue(SECTION_NAME, "ParameterTypeComboboxList", parameterTypeList, "");
	std::string param;
	std::stringstream ss(parameterTypeList.c_str());
	//INITIALISE_PARAMETERS_TYPES(ParameterTypeImpl, "Currency");
	//CSRParameterType::prototype& prototypes = CSRParameterType::GetPrototype();
	while (std::getline(ss, param, ','))
	{
		std::string * paramName = new std::string(param); // new string created, stay in heap, never be free
		INITIALISE_PARAMETERS_TYPES(ParameterTypeImpl, paramName->c_str());
		//ParameterTypeImpl *clone = new ParameterTypeImpl;
		//prototypes.insert(paramName->c_str(), clone);
	}

	//other parametertypes
	ConfigurationFileWrapper::getEntryValue(SECTION_NAME, "ParameterTypes", parameterTypeList, "");
	ss.str("");
	ss.clear();
	ss.str(parameterTypeList.c_str());
	while (std::getline(ss, param, ','))
	{
		std::string * paramName = new std::string(param); // new string created, stay in heap, never be free
		INITIALISE_PARAMETERS_TYPES(ParameterTypeImpl, paramName->c_str());
	}
}
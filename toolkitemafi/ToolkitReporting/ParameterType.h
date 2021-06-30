#include "SphReportingInc/SphReporting.h"
#include "SphInc/SphMacros.h"
#if (TOOLKIT_VERSION < 7100)
#include <string>
#endif

#define SECTION_NAME "EMC"

namespace eff {
	namespace ToolkitReporting {
		class ParameterTypeImpl : public sophis::reporting::CSRParameterType
		{
		public:
			DECLARATION_PARAMETERS_TYPES(ParameterTypeImpl);

			ParameterTypeImpl() : sophis::reporting::CSRParameterType() {}
			#if (TOOLKIT_VERSION < 7100)
				bool Validate(_STL::string value, _STL::string settings) const { return true; }
			#else
				bool Validate(std::string value, std::string settings) const { return true; }
			#endif

			static void InitializeAllParameterTypes();
		protected:
		private:
			static const char* __CLASS__;
		};
	}
}


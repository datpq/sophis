#include "SphReportingInc/SphReporting.h"

namespace eff {
	namespace ToolkitReporting {
		class BindingFunctionImpl : public sophis::reporting::IBindingFunction{
		public:
			BindingFunctionImpl(std::string functionName, int numOfParams, ...);

			virtual void InitializeFunctionName() {}
			virtual void InitializeParametersNames() {}
		protected:
			std::string DATE_FORMAT;
		};

		class BfLastDayOfMonth : public BindingFunctionImpl {
		public:
			BfLastDayOfMonth() : BindingFunctionImpl("LastDayOfMonth", 1, "date") {}
			std::string Execute(std::vector<std::string>& paramValues);
		private:
			static const char* __CLASS__;
		};

		class BfFirstDayOfMonth : public BindingFunctionImpl {
		public:
			BfFirstDayOfMonth() : BindingFunctionImpl("FirstDayOfMonth", 1, "date") {}
			std::string Execute(std::vector<std::string>& paramValues);
		private:
			static const char* __CLASS__;
		};

		class BfLastDayOfPreviousMonth : public BindingFunctionImpl {
		public:
			BfLastDayOfPreviousMonth() : BindingFunctionImpl("LastDayOfPreviousMonth", 1, "date") {}
			std::string Execute(std::vector<std::string>& paramValues);
		private:
			static const char* __CLASS__;
		};

		class BfConcatenate : public BindingFunctionImpl {
		public:
			BfConcatenate() : BindingFunctionImpl("Concatenate", 2, "str1", "str2") {}
			std::string Execute(std::vector<std::string>& paramValues);
		private:
			static const char* __CLASS__;
		};
	}
}

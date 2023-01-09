#ifdef _WIN32
#pragma once
#endif

#ifndef _EXTERNALEXECUTIONGETTER_H_
#define _EXTERNALEXECUTIONGETTER_H_

#include "SphInc/SphMacros.h"
#include "SphTools/base/CommonOS.h"

#include "../cc_data/DBCommon.h"


#include "SphLLInc/execution/SphExecution.h"
#include "SphSDBCInc/datatypes/SphClob.h"


#if (defined( WIN32 ) || defined( _WIN64 ))
#	pragma warning(push)
#	pragma warning(disable:4275) // Can not export a class derivated from a non exported one
#	pragma warning(disable:4251) // Can not export a class agregating a non exported one
#endif


using namespace sophis::sql;
using namespace sophis::portfolio;

/*
 *	ExecutionsDBManager
 */
namespace sophis
{
	namespace execution
	{
		namespace database
		{

			struct FxSwapLegs
			{
				long nearLeg;
				long farLeg;
			};

			class SOPHIS_EXECUTION ExternalExecutionDBGetter : public sophisTools::base::UnsafeRefCount
			{
			public :
				
				static void Get(sophis::execution::CSRExecution& execution, 
								const ExternalExecution& externalExecution)
					throw(sophisTools::base::GeneralException);

				static void GetExecutions(_STL::vector<ExternalExecution>& execs, const _STL::string& whereClause)
					throw(sophisTools::base::DatabaseException, sophisTools::base::GeneralException);

				static void GetExecutionAllocationRules(_STL::vector<AllocationRule>& executionAllocationRules, const _STL::string& whereClause)
					throw(sophisTools::base::DatabaseException, sophisTools::base::GeneralException);

				static void FillExecsWithAllocRules(ExternalExecutionList& executions)
					throw(sophisTools::base::DatabaseException, sophisTools::base::GeneralException);

				static void getFxSwapList(const _STL::string& sRefconsToLookFor, _STL::list<FxSwapLegs>& foundFxSwapList)
					throw (sophisTools::base::DatabaseException, sophisTools::base::ExceptionBase);
				
				static void getFxSwapLegs(const _STL::string& sRefconsToLookFor, _STL::list<long>& foundFxSwapLegs)
					throw (sophisTools::base::DatabaseException, sophisTools::base::ExceptionBase);
				


			private:
				static const char* __CLASS__;
			};
		};
	};
};

#if (defined( WIN32 ) || defined( _WIN64 ))
#	pragma warning(pop)
#endif

#endif //_EXTERNALEXECUTIONGETTER_H_

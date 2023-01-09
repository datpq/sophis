#if (defined( WIN32 ) || defined( _WIN64 ))
#	pragma once
#endif

/**
* ----------------------------------------------------------------------------
* Sophis Technology 
* Copyright (c) 2009
* File : DBCommon.h
* Creation : September 1 2009 by Silviu Marele
* 
* ----------------------------------------------------------------------------
*/
#ifndef __DATABASECOMMON_H__
#define __DATABASECOMMON_H__

#include __STL_INCLUDE_PATH(set)

#include "SphInc/SphMacros.h"
#include "SphTools/base/CommonOS.h"


#include "SphSDBCInc/datatypes/SphDate.h"
#include "SphSDBCInc/datatypes/SphClob.h"
#include "SphLLInc/execution/SphExecution.h"


//------------------------------------------------------------------------------------------------------------------
#define CUSTOMFEES_MAX				20
#define EXTERNALEXECREF_MAXLEN     254
#define EXTERNALORDERREF_MAXLEN    128
#define ALLOCATORNAME_MAXLEN       128
#define AGGREGATORNAME_MAXLEN      128
#define FEE_NAME_MAXLEN				50
#define SOURCE_ID_MAXLEN		    40
#define EXTERNALPROPERTY_VALUE_MAXLEN   128
#define EXTERNALPROPERTY_SOURCE_MAXLEN  255
#define EXTERNALPROPERTY_DEFAULTVALUES_MAXLEN    4000


#define PLACE_DEFAULT			-1
#define FEES_DEFAULT			INT_MIN
#define SOPHISEXECID_DEFAULT	0
#define SOPHISORDERID_DEFAULT	0
#define QTY_DEFAULT				DBL_MIN
#define PRICE_DEFAULT			DBL_MIN
#define QTYTYPE_DEFAULT			INT_MIN
#define ISLAST_DEFAULT			INT_MIN
#define FORCECREATE_DEFAULT		INT_MIN
#define MODE_DEFAULT			INT_MIN
#define TICKETID_DEFAULT		0
#define PROCMODE_DEFAULT		INT_MIN
#define PROCERRCODE_DEFAULT		-1
#define FOLIO_DEFAULT			0
#define EXT_FEE_ID_DEFAULT		0

#define MAXBUFLEN				500
#define SIZEBLOCK_MAX			500
#define MATCHING_ENGINE_REASON_LEN 512

#define EXEC_BROKERFEES_ROOT	"BROKERFEES"
#define EXEC_MARKETFEES_ROOT	"MARKETFEES"
#define EXEC_CTPYFEES_ROOT		"CTPYFEES"

/*
** defines
*/

namespace sophis
{
	namespace execution
	{
		
		//----------------------------------- Exception -----------------------------------------
		/*
		 * Thrown during a block query; contains the unsuccessful items
		 */
		class SOPHIS_EXECUTION BlockPartialExecutionException : sophisTools::base::GeneralException
		{
		public :
			long fSizeBlock;
			long fSizeExecuted;
			_STL::list<long> fListErrors;


			BlockPartialExecutionException(long sBlock, long sExecuted, const char* msg, _STL::list<long> listErrors ) : sophisTools::base::GeneralException(msg)
			{
				fSizeBlock = sBlock;
				fSizeExecuted = sExecuted;
				fListErrors = listErrors;
			}

			BlockPartialExecutionException(long sBlock, long sExecuted, const char* msg ) : sophisTools::base::GeneralException(msg)
			{
				fSizeBlock = sBlock;
				fSizeExecuted = sExecuted;
			}
		};

		namespace database
		{
			//----------------------------------- Executions -----------------------------------------
			enum ExecStatus
			{
				New,
				Modified,
				Canceled,
				Uninitialized
			};

			class SOPHIS_EXECUTION ExternalExecution
			{
			public:
				void Init();
				ExternalExecution();
				ExternalExecution(const ExternalExecution& externalExecution);
				ExternalExecution  &operator =(const ExternalExecution &);

				_STL::string GetString() const;

			public:
				long					PK_SOPHISEXECID;
				char					EXTERNALEXECREF[EXTERNALEXECREF_MAXLEN+1];
				char        			EXTERNALORDERREF[EXTERNALORDERREF_MAXLEN+1];
				long    				SOPHISORDERID;
				double					LASTQUANTITY;
				double      			LASTPRICE;
				double      			CUMQUANTITY;
				double					AVGPRICE;
				long					QTYTYPE;
				double					CUSTOM_BROKERFEES[CUSTOMFEES_MAX];
				double					CUSTOM_MARKETFEES[CUSTOMFEES_MAX];
				double					CUSTOM_CTPYFEES[CUSTOMFEES_MAX];
				char        			ALLOCATORNAME[ALLOCATORNAME_MAXLEN+1];
				long        			PLACE;
				long        			ISLAST;
				long					PROCMODE;
				char					SOURCEID[SOURCE_ID_MAXLEN+1];
				sophis::sql::CSRClob	OTHER_FIELDS;
				sophis::sql::CSRDate    ENTEREDDATETIME;
				long					LASTTICKETID;
				long					STATUS;
				long					KIND;
				long					MATCHING_ENGINE_STATUS;
				char					MATCHING_ENGINE_REASON[MATCHING_ENGINE_REASON_LEN+1];
				long					MATCHING_ENGINE_ACTION;
				sophis::sql::CSRClob	INSTR_DESC;
				long					REPOEXECKIND;
				long        			FORCECREATE;
			};

			//----------------------------------- Allocation Rules -----------------------------------------
			class SOPHIS_EXECUTION AllocationRule
			{
			public:
				AllocationRule();
				AllocationRule(const AllocationRule& allocationRule);
				AllocationRule  &operator =(const AllocationRule &);
				
				_STL::string GetString() const;

			public:
				long					PK_ALLOCID;
				long					SOPHISEXECID;
				long					FOLIO;
				double      			QUANTITY;
				long					PRIMEBROKER;
			};
			//----------------------------------- Execution User Fields description ---------------------------------------
			class SOPHIS_EXECUTION DBExecProperties
			{
			public:
				static void init()
						throw(sophisTools::base::DatabaseException, sophisTools::base::GeneralException);

				// id is the exec user property Id or Name
				static const sophis::execution::ExecProperty* getExecutionProperty(const _STL::string& id);

			protected :
				static long loadPropertiesDefinition()
					throw(sophisTools::base::GeneralException);

				
			public:

				static _STL::map<_STL::string, sophis::execution::ExecProperty> fUserPropertiesPerId;
				static _STL::map<_STL::string, sophis::execution::ExecProperty> fUserPropertiesPerName;
				

			private:
				static const char* __CLASS__;
			};
			
			
			class SOPHIS_EXECUTION DBExecProperty
			{
				public:
				DBExecProperty();
				DBExecProperty(const DBExecProperty& DBExecProperty);
				DBExecProperty  &operator =(const DBExecProperty &);
				
				_STL::string getString() const;

				public:
					unsigned long PK_ID;
					char CATEGORY[EXTERNALPROPERTY_VALUE_MAXLEN];
					char NAME[EXTERNALPROPERTY_VALUE_MAXLEN];
					char DATATYPE[EXTERNALPROPERTY_VALUE_MAXLEN];
					unsigned long TYPE;
					unsigned long VISIBILITY;
					char POSSIBLEVALUES[EXTERNALPROPERTY_DEFAULTVALUES_MAXLEN];
					char DEFAULTVALUE[EXTERNALPROPERTY_VALUE_MAXLEN];
			};

			//----------------------------------- Execution User Fields values ---------------------------------------
			class SOPHIS_EXECUTION DBExecPropertyAssociation
			{

			public:
				DBExecPropertyAssociation();
				DBExecPropertyAssociation(const DBExecPropertyAssociation& DBExecPropertyAssociation);
				DBExecPropertyAssociation  &operator =(const DBExecPropertyAssociation &);
				
				_STL::string getString() const;

			public:
				unsigned long sophisExecId;
				unsigned long execPropertyId;
				char	 value[EXTERNALPROPERTY_VALUE_MAXLEN];
				char     sourceId[EXTERNALPROPERTY_SOURCE_MAXLEN];		
			};

			//----------------------------------- Execution 2 Trades ---------------------------------------
			class SOPHIS_EXECUTION ExternalExecutionToTrade
			{
			public:
				long		PK_ID;
				long    	SOPHISEXECID;
				sophis::portfolio::TransactionIdent TRADEID;
				double      ALLOCATEDQUANTITY;
				double		CUSTOM_BROKERFEES[CUSTOMFEES_MAX];
				double		CUSTOM_MARKETFEES[CUSTOMFEES_MAX];
				double		CUSTOM_CTPYFEES[CUSTOMFEES_MAX];
				char        AGGREGATORNAME[AGGREGATORNAME_MAXLEN+1];
				long		FOLIO;
				long		TRADEGROUPID;
				long		PLACEINGROUP;
				
			public:
				ExternalExecutionToTrade();
				ExternalExecutionToTrade(const ExternalExecutionToTrade& externalExecution);
				ExternalExecutionToTrade &operator =(const ExternalExecutionToTrade &);

				_STL::string GetString() const;
			};

			//----------------------------------------- Fees Detail Mapping -----------------------------------------
			class SOPHIS_EXECUTION FeesDetailMapping
			{
			public:
				FeesDetailMapping();
				FeesDetailMapping(const FeesDetailMapping& feesDetailMapping);
				FeesDetailMapping  &operator =(const FeesDetailMapping &);
				
				_STL::string GetString() const;

			public:
				char					SOURCE_ID[SOURCE_ID_MAXLEN+1];
				long					EXT_FEE_ID;
				char					FEE_NAME[FEE_NAME_MAXLEN+1];
			};

			//----------------------------------------- Fees -----------------------------------------------
			typedef _STL::map<long, _STL::string> External2InternalFeeNameDetailMap;
			typedef _STL::map<_STL::string, External2InternalFeeNameDetailMap> FeesDetailSourceMapping;
			class SOPHIS_EXECUTION DBFees
			{
			public:
				static void init()
						throw(sophisTools::base::DatabaseException, sophisTools::base::GeneralException);

				static void loadConfigAdditionnalFees(_STL::string feesType, _STL::string feesConfig, _STL::set<long>& feesIdx, _STL::set<_STL::string>& feesColumns,
													  const _STL::set<long>& dbFeexIdx)
					throw(sophisTools::base::ExceptionBase);

				static void getFeesColumnIndexesFromTable(const char * typeFees, _STL::set<long>& feesColumnsIndexes, _STL::set<_STL::string>& feesColumnsNames)
					throw (sophisTools::base::DatabaseException, sophisTools::base::ExceptionBase);

				static void loadFeesDetailsMapping();

			public:

				// additionnal fees
				static _STL::set<_STL::string>& GetBrokerFeesColumns();
				static FeesDetailSourceMapping& GetBrokerFeesDetailSourceMapping();
				static _STL::set<long>&         GetBrokerFeesIdx(); // the indexes of the columns (starting with 1)
				static _STL::set<_STL::string>& GetCtpyFeesColumns();
				static FeesDetailSourceMapping& GetCtpyFeesDetailSourceMapping();
				static _STL::set<long>&         GetCtpyFeesIdx();
				static _STL::set<_STL::string>& GetMarketFeesColumns();
				static FeesDetailSourceMapping& GetMarketFeesDetailSourceMapping();
				static _STL::set<long>&         GetMarketFeesIdx();

				static double DEFAULT_FEES[CUSTOMFEES_MAX];

			private:
				static const char* __CLASS__;
			};	
		
			//----------------------------------------- exec properties -----------------------------------------------
			
			
		}
	}
}

SOPHIS_EXECUTION _STL::ostream & operator<< (_STL::ostream & os,const sophis::execution::database::ExternalExecution& extExecution);
SOPHIS_EXECUTION _STL::ostream & operator<< (_STL::ostream & os,const sophis::execution::database::AllocationRule& allocationRule);
SOPHIS_EXECUTION _STL::ostream & operator<< (_STL::ostream & os,const sophis::execution::database::ExternalExecutionToTrade & extExecutionToTrade);
SOPHIS_EXECUTION _STL::ostream & operator<< (_STL::ostream & os,const sophis::execution::database::FeesDetailMapping & feesDetailMapping);

#endif //__DATABASECOMMON_H__

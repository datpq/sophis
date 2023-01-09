#if (defined( WIN32 ) || defined( _WIN64 ))
#	pragma once
#endif

/**
* ----------------------------------------------------------------------------
* Sophis Technology 
* Copyright (c) 2009
* File : SphExecution.h
* Creation : May 19 2009 by Silviu Marele
* 
* ----------------------------------------------------------------------------
*/
#ifndef __CSREXTERNALEXECUTION_H__
#define __CSREXTERNALEXECUTION_H__

/*
** System includes
*/
#include "SphInc/SphMacros.h"
#include "SphTools/base/CommonOS.h"
#include "SphTools/base/UnsafeRefCount.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphTools/SphDay.h"
//#include "SphTools/compatibility/globalsophisdrnt.h"
#include "SphTools/compatibility/globalsophis.h"
#include "../cc_data/SphSystem/config/PropertySet.h"

/*
** Application includes
*/


#include "SphInc/misc/SphEvents.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphLLInc/portfolio/SphTradeAllocatorParameters.h"
#include "event/core/SphEvent.h"
#include "SphLLInc/interface/transskel_cp.h"
#include "SphLLInc/cdc_donnee.h"


SPH_PROLOG

/*
** defines
*/

/*
** typedef and classes
*/


namespace sophis
{
	namespace portfolio
	{
		class SOPHIS_EXECUTION CSRTransactionRefCount : public sophis::portfolio::CSRTransaction, public sophisTools::base::UnsafeRefCount
		{
		public:
			CSRTransactionRefCount(misc::CSREvent *event):sophis::portfolio::CSRTransaction(event){}
			CSRTransactionRefCount(const sophis::portfolio::CSRTransaction& tr):sophis::portfolio::CSRTransaction(tr){}
			static CSRTransactionRefCount** newCSRTransactionArray(const char *sqlQuery, long* size);
			static CSRTransactionRefCount** newCSRTransactionArray(const char *sqlQuery, long* size, const char* table);

			/** Compute accrued amount. */
			void ComputeAccruedCoupon(bool overrideDate, bool overrideCoupon);

			/** Compute net amount and specified fees. */
			void RecalculeTicket(bool *calculFrais);
		};
		typedef sophisTools::base::RefCountHandle<CSRTransactionRefCount> CSRTransactionHandle;

		typedef _STL::list<sophis::portfolio::CSRTransactionHandle> TradeList;
	};

	namespace tools
	{
		class CSREventVector;
	};

	namespace xml
	{
		namespace dataModel
		{
			class XMLDocument;
			HANDLE_DECL(XMLDocument);
		};

		class CSREventVector;
	};

	namespace collateral
	{
		struct SStockLoanRepoBookingData;
	}

	namespace execution
	{
		enum QuantityType
		{
			NumberOfInstruments,
			Notional
		};
		enum ProcessingMode
		{
			LastFill,
			CumulativeQuantity
		};
		
		enum LegType
		{
			NearLeg,
			FarLeg
		};
		enum ExecutionKind
		{
			Manual,
			Other
		};
		enum RepoExecutionType
		{
			OTC,
			Listed,
			Pledge
		};

		enum SOPHIS_EXECUTION CSRExecutionPropertiesType
		{
			TRADE = 0,
			EXEC = 1,
			EXEC_AND_TRADE = 2
		};

		enum SOPHIS_EXECUTION TicketType
		{
			Create, 
			Modify, 
			Cancel, 
			Replace
		};

		//the key of the map is the index of the column in the table EXTERNAL_EXECUTIONS or in EXTERNAL_EXECUTIONS_TO_TRADES;
		typedef _STL::map<long, double> FeesDetails;

		//fees details in risk api style
		typedef _STL::map<_STL::string, CSRDetailedFee> DetailedFeesMap;

		class SOPHIS_EXECUTION AllocationRule
		{
		public:
			AllocationRule():id(0),folioId(0), quantity(0.0), primeBroker(0){}
		public:
			long    id;
			long	folioId;
			double	quantity;
			long	primeBroker; //the depositary
		};
		typedef _STL::map<long, AllocationRule> AllocationRuleMap; //the key is the folio

		struct SOPHIS_EXECUTION RepoExecution;

		class SOPHIS_EXECUTION ExecProperty
		{
			public:
				unsigned long pk_id;
				_STL::string category;
				_STL::string name;
				_STL::string dataType;
				unsigned long type;
				unsigned long visibility;
				_STL::string possibleValues;
				_STL::string defaultValue;
		};

		class SOPHIS_EXECUTION CSRExecution : public virtual sophisTools::base::UnsafeRefCount
		{
		public:
			CSRExecution();
			CSRExecution(const CSRExecution& src);
			void update(const CSRExecution& fieldsToUpdate)
				throw(sophisTools::base::ExceptionBase);
			virtual ~CSRExecution();

			/**
			 * Create a CSRTransaction WITHOUT REFCON from this execution.
			 * Uses the CSREvent constructor of CSRTransaction
			 */
			sophis::portfolio::CSRTransactionHandle createCSRTransaction()
				throw(sophisTools::base::ExceptionBase);

			/**
			 * Extends createCSRTransaction for FXSwaps by creating 2 trades instead of one
			 * For all the other trades it returns one trade only
			 */
			void createCSRTransactions(sophis::portfolio::TradeList& tradeList, bool reserveRefcons)
				throw(sophisTools::base::ExceptionBase);

			/**
			 * For Repos, it creates several trades and several instruments
			 */
			void createRepoCSRTransactions(sophis::tools::CSREventVector &ev, sophis::portfolio::TradeList& tradeList, long kernelEventId)
				throw(sophisTools::base::ExceptionBase);

			/**
			 *	External reference of the execution as given by the external system
			 */
			bool getExternalRef(_STL::string &value) const;
			void setExternalRef(const _STL::string &value);

			/**
			 *	Sophis identifier of the execution as given by Sophis
			 */
			bool getSophisExecId(long &value) const;
			void setSophisExecId(long value);

			bool getTargetSophisExecId(long &value) const;
			void setTargetSophisExecId(long value);

			bool getLastQty(double &value) const;
			void setLastQty(double value);

			bool getLastPrice(double &value) const;
			void setLastPrice(double value);

			bool getCumQty(double &value) const;
			void setCumQty(double value);

			bool getAvgPrice(double &value) const;
			void setAvgPrice(double value);

			bool getQuantityType(QuantityType &value) const;
			void setQuantityType(QuantityType value);

			bool getProcessingMode(ProcessingMode &value) const;
			void setProcessingMode(ProcessingMode value);

			bool getUsedQty(double& qty, _STL::string& errMsg, long placeInGroup = 0) const;
			void setUsedQty(double qty, long placeInGroup);

			bool getUsedPrice(double& price, _STL::string& errMsg, long placeInGroup = 0) const;
			void setUsedPrice(double price, long placeInGroup);

			/**
			 *	In an execution, 2 pairs (quanty, price) may be specified:  (LastQuantity, LastPrice) and (CumulativeQuantity, AveragePrice)
			 *	Depending on the processing mode only one pair will be really used.
			 */
			bool getUsedQtyAndPrice(double& qty, double& price, _STL::string& errMsg, long placeInGroup = 0) const;
			void setUsedQtyAndPrice(double qty, double price, long placeInGroup = 0);

			bool getPlace(long &value) const;
			void setPlace(long value);

			bool getAllocatorName(_STL::string &value) const;
			void setAllocatorName(const _STL::string &value);

			bool getIsLast(bool &value) const;
			void setIsLast(bool value);

			bool getForceCreate(bool &value) const;
			void setForceCreate(bool value);
			
			bool getLastTicketId(long &value) const;
			void setLastTicketId(long value);

			bool getStatus(long &value) const;
			void setStatus(long value);

			void loadExecUserFieldProperties(); // load the exec user property  description in fUserFieldsExecTypes for exec User fields
			const sophis::execution::ExecProperty* getExecUserFieldProperty(const _STL::string& id) const; // get the exec user property definition loaded from exec user property DB table
			
			/**
			 *	The Identifier of the market adapter that brought this execution to Sophis
			 */
			bool getSourceId(_STL::string &value) const;
			void setSourceId(const _STL::string &value);

			//--------------------------- BackOffice events ---------------------------------
			/**
			 *	An execution may carry 3 types of BO events: 
			 *		1. An external one (provided by the external system)
			 *		2. An allocation one (set by the allocator during the allocation phase)
			 *		3. An aggregation one (set by the aggregator during the aggregation phase)
			 */
			bool getExternalBOEventId(long &value) const;
			void setExternalBOEventId(long value);

			bool getLegId(long &value) const;
			void setLegId(long value);

			bool getMLegsExecId(_STL::string &value) const;
			void setMLegsExecId(const _STL::string &value);
			
			//--------------------------- Fees ---------------------------------
			/**
			 *	An execution may contain not just one value but several for each fee type. These are fees details (as required by MiFID)
			 *	Up to 20 details per fee type may be specified
			 */
			const FeesDetails& getBrokerFeesDetails() const {return fBrokerFeesDetails;}
			const FeesDetails& getMarketFeesDetails() const {return fMarketFeesDetails;}
			const FeesDetails& getCtpyFeesDetails() const {return fCtpyFeesDetails;}

			//compute the fees details in risk api style. can fill the map, the vector or both
			void getAPIFeesDetails(DetailedFeesMap* detailedFeesMap, DetailedFeesVector* detailedFeesVector) const;

			/**
			 *	Set fee details
			 */
			void setBrokerFeesDetail(long index, double value);
			void setMarketFeesDetail(long index, double value);
			void setCtpyFeesDetail(long index, double value);

			/**
			 *	Obtains the total broker fess amount by making the sum of all broker fees details
			 */
			bool getBrokerFees(double& fees) const;
			/**
			 *	Obtains the total market fess amount by making the sum of all market fees details
			 */
			bool getMarketFees(double& fees) const;
			/**
			 *	Obtains the total counterpart fess amount by making the sum of all counterpart fees details
			 */
			bool getCtpyFees(double& fees) const;

			/**
			 *	check is the property is an execution property (exec user type execution or trade/execution)
			 */
			bool isUserFieldExecProperty(const _STL::string& id) const;

			/**
			 *	 check is there is some exec properties to handle for this execution 
			 */
			bool isUserFieldOnlyExecProperty(const _STL::string& id) const;

			/**
			 *	check is there is exec properties to handle
			 */
			bool hasExecUserField() const;

			
			/**
			 *	Obtains the payment currency
			 */
			long getPaymentCurrency() const;

			/**
			 *	Choose the BO Event to be used during an execution creation (this event will be applied to the corresponding sophis trade):
			 *	1. if an external BO event was set, choose this one, stop; if not, continue with 2;
			 *  2. choose the "MO Creation Event" as defined in Risk in "BO Kernel->Parameters->Default Kernel"
			 */
			virtual long getCreationBOEventId() const;
			/**
			 *	Choose the BO Event to be used during an execution modification (this event will be applied to the corresponding sophis trade):
			 *	1. if an external BO event was set, choose this one, stop; if not, continue with 2;
			 *  2. choose the "MO Modification Event" as defined in Risk in "BO Kernel->Parameters->Default Kernel"
			 */
			virtual long getModificationBOEventId() const;
			/**
			 *	Choose the BO Event to be used during an execution cancellation (this event will be applied to the corresponding sophis trade):
			 *	1. if an external BO event was set, choose this one, stop; if not, continue with 2;
			 *  2. choose the "MO Deletion Event" as defined in Risk in "BO Kernel->Parameters->Default Kernel"
			 */
			virtual long getCancellationBOEventId() const;

			virtual bool isFxSwap() const;

			static void updateCSREvent(sophis::misc::CSREvent& target, const sophis::misc::CSREvent& source);

			/**
			 * Allocation rules
			 */
			AllocationRuleMap& getAllocationRules() {return fAllocationRuleMap;}

			bool getKind(ExecutionKind &value) const;
			void setKind(ExecutionKind value);

			bool getRepoExecutionType(RepoExecutionType &value) const;
			void setRepoExecutionType(RepoExecutionType value);

			long 
				getRepoInstrument(long & startDate, long & endDate, long & direction) const;

			bool isSelectedForMatching() const { return fSelectedForMatching; }
			void setSelectedForMatching(bool value) {fSelectedForMatching = value;}

			bool getMatchingEngineStatus(long& value) const;
			void setMatchingEngineStatus(long value);
			void deleteMatchingEngineStatus();

			bool getMatchingEngineReason(_STL::string& value) const;
			void setMatchingEngineReason(const _STL::string& value);

			bool getMatchingEngineAction(long& value) const;
			void setMatchingEngineAction(long value);

			bool getInstrDescription(_STL::string& value) const;
			void setInstrDescription(const _STL::string& value);


			/*
				update userfield in CRSTransaction from the execution 
			*/
			void updateUserFields(sophis::portfolio::CSRTransactionHandle transaction) const
				throw(sophisTools::base::ExceptionBase);

		public:

		private:
			void checkAndSetUserFields(sophis::portfolio::CSRTransactionRefCount & transaction) const
				throw(sophisTools::base::ExceptionBase);

			const sophis::tools::dataModel::DataSet& getInstrumentDataSet()
				throw(sophisTools::base::ExceptionBase);

	
		public:
			sophis::misc::CSREvent			fFields; //fields in common with a CSRTransaction
			sphSystem::config::PropertySet	fAdditionalExecFields; //execution specific fields not having a column in EXTERNAL_EXECUTIONS
			sphSystem::config::PropertySet	fUserFields; //user defined fields
			

			sophis::misc::CSREvent			fFarFXSwapLeg; //fields of the far leg, only used in case of a FXSwap


		protected:
			//execution specific fields
			long fSophisExecId;
			long fTargetSophisExecId; //In case of a modification or replace this contains the initial execution
			
			_STL::string fExecExternalRef;
			bool fExecExternalRefIsSet;

			double fLastQty;
			bool fLastQtyIsSet;
			double fLastPrice;
			bool fLastPriceIsSet;

			double fCumQty;
			bool fCumQtyIsSet;
			double fAvgPrice;
			bool fAvgPriceIsSet;

			QuantityType fQuantityType;
			bool fQuantityTypeIsSet;
			
			_STL::string fAllocatorName;
			bool fAllocatorNameIsSet;

			long fPlace;
			
			bool fIsLast;
			bool fIsLastIsSet;

			bool fForceCreate;
			bool fForceCreateIsSet;
		
			long fLastTicketId;

			long fStatus;

			ProcessingMode fProcessingMode;
			bool fProcessingModeIsSet;

			_STL::string fSourceId; //the identifier of the market adapter who sent the execution
			bool fSourceIdIsSet;

			long fExternalBOEventId;
			
			FeesDetails	fBrokerFeesDetails;
			FeesDetails	fMarketFeesDetails;
			FeesDetails	fCtpyFeesDetails;

			AllocationRuleMap fAllocationRuleMap;
			ExecutionKind fKind; //indicates if an execution was created by the Sophis Manual Adapter or not
			bool fKindIsSet;

			RepoExecutionType fRepoExecutionType;
			bool fRepoExecutionTypeIsSet;

			_STL::map<_STL::string, const sophis::execution::ExecProperty*>	fUserFieldsExecTypes; //linked to fUserFields, list of properties that are only execution properties : not to be handled as trade user field

			bool fSelectedForMatching;

			long fMatchingEngineStatus;
			bool fMatchingEngineStatusIsSet;

			_STL::string fMatchingEngineReason;
			bool fMatchingEngineReasonIsSet;

			long fMatchingEngineAction;
			bool fMatchingEngineActionIsSet;

			_STL::string fInstrDescription;
			bool fInstrDescriptionIsSet;
			sophis::xml::dataModel::XMLDocumentHandle fInstrXmlDoc;

			static const char *			__CLASS__;
		};
		HANDLE_DECL(CSRExecution);
		typedef _STL::list<CSRExecutionHandle> ExternalExecutionList;

		///////////////////////////////////////////////////////////////////////////////////////
		class SOPHIS_EXECUTION CSRExecutionAllocation : public virtual CSRExecution
		{
		public:
			CSRExecutionAllocation();
			CSRExecutionAllocation(const CSRExecutionAllocation& src);
			CSRExecutionAllocation(const CSRExecution& src);
			void update(const CSRExecutionAllocation& fieldsToUpdate)
				throw(sophisTools::base::ExceptionBase);
			void update(const CSRExecution& fieldsToUpdate)
				throw(sophisTools::base::ExceptionBase);
			virtual ~CSRExecutionAllocation();

			bool getAllocationBOEventId(long &value) const;
			void setAllocationBOEventId(long value);

			bool getAggregationBOEventId(long &value) const;
			void setAggregationBOEventId(long value);

			/**
			 *	During the Creation / Modification / Cancellation of an execution, only one BO Event can be used
			 *	The external BO event has the highest priority, followed by the allocation BO Event and then by the aggregation BO Event.
			 *	If none of the 3 are present in the execution, the default events set in Risk in 
			 *	"BO Kernel->Parameters->Default Kernel" will be used.
			 */

			/**
			 *	Choose the BO Event to be used during an execution creation (this event will be applied to the corresponding sophis trade):
			 *	1. if an external BO event was set, choose this one, stop; if not, continue with 2;
			 *	2. if an allocation BO Event was set, choose this one, stop; if not, continue with 3;
			 *	3. if an aggregation BO Event was set, choose this one, stop; if not, continue with 4;
			 *  4. choose the "MO Creation Event" as defined in Risk in "BO Kernel->Parameters->Default Kernel"
			 */
			virtual long getCreationBOEventId() const;
			/**
			 *	Choose the BO Event to be used during an execution modification (this event will be applied to the corresponding sophis trade):
			 *	1. if an external BO event was set, choose this one, stop; if not, continue with 2;
			 *	2. if an allocation BO Event was set, choose this one, stop; if not, continue with 3;
			 *	3. if an aggregation BO Event was set, choose this one, stop; if not, continue with 4;
			 *  4. choose the "MO Modification Event" as defined in Risk in "BO Kernel->Parameters->Default Kernel"
			 */
			virtual long getModificationBOEventId() const;
			/**
			 *	Choose the BO Event to be used during an execution cancellation (this event will be applied to the corresponding sophis trade):
			 *	1. if an external BO event was set, choose this one, stop; if not, continue with 2;
			 *	2. if an allocation BO Event was set, choose this one, stop; if not, continue with 3;
			 *	3. if an aggregation BO Event was set, choose this one, stop; if not, continue with 4;
			 *  4. choose the "MO Deletion Event" as defined in Risk in "BO Kernel->Parameters->Default Kernel"
			 */
			virtual long getCancellationBOEventId() const;

		private:
			long fAllocationBOEventId;
			long fAggregationBOEventId;
		};
		HANDLE_DECL(CSRExecutionAllocation);
		typedef _STL::list<CSRExecutionAllocationHandle> ExecutionAllocationList;



	

	};
};

SOPHIS_EXECUTION _STL::ostream & operator<< (_STL::ostream & os,const sophis::execution::ExecProperty& execProperty);


SPH_EPILOG
#endif //__CSREXTERNALEXECUTION_H__